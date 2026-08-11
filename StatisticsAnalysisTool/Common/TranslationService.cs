using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Common;

/// <summary>
/// Translation service using Google Translate free gtx endpoint.
/// No API key required. Falls back gracefully if translation fails.
/// </summary>
public class TranslationService
{
    private readonly HttpClient _http;
    private readonly Dictionary<string, string> _cache = new();
    private readonly SemaphoreSlim _cacheLock = new(1, 1);
    private readonly SemaphoreSlim _rateLimiter = new(1, 1);
    private DateTime _lastRequestTime = DateTime.MinValue;
    private readonly TimeSpan _minRequestInterval = TimeSpan.FromMilliseconds(200);
    private string _targetLanguage = "en";
    private bool _enabled = true;

    public static TranslationService Instance { get; } = new();

    private TranslationService()
    {
        _http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };
        _http.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (X11; Linux x86_64) AlbionOnlineCompanion/1.0");
    }

    public string TargetLanguage
    {
        get => _targetLanguage;
        set => _targetLanguage = value;
    }

    public bool Enabled
    {
        get => _enabled;
        set => _enabled = value;
    }

    /// <summary>
    /// Translate text using Google Translate free gtx endpoint (auto source detection).
    /// </summary>
    public Task<TranslationResult> TranslateAsync(string text, CancellationToken ct = default)
        => TranslateCoreAsync(text, _targetLanguage, ct);

    /// <summary>
    /// Translate text to an explicit target language (used by the type-to-translate box).
    /// </summary>
    public Task<TranslationResult> TranslateAsync(string text, string targetLanguage, CancellationToken ct = default)
        => TranslateCoreAsync(text, targetLanguage, ct);

    private async Task<TranslationResult> TranslateCoreAsync(string text, string targetLanguage, CancellationToken ct)
    {
        if (!_enabled || string.IsNullOrWhiteSpace(text))
            return new TranslationResult { TranslatedText = text };

        // Check cache
        var cacheKey = $"{text}:{targetLanguage}";
        await _cacheLock.WaitAsync(ct);
        try
        {
            if (_cache.TryGetValue(cacheKey, out var cached))
                return new TranslationResult { TranslatedText = cached, FromCache = true };
        }
        finally
        {
            _cacheLock.Release();
        }

        try
        {
            // Rate limiting
            await _rateLimiter.WaitAsync(ct);
            try
            {
                var elapsed = DateTime.UtcNow - _lastRequestTime;
                if (elapsed < _minRequestInterval)
                {
                    await Task.Delay(_minRequestInterval - elapsed, ct);
                }
                _lastRequestTime = DateTime.UtcNow;
            }
            finally
            {
                _rateLimiter.Release();
            }

            // sl=auto: let Google detect the source language — our local heuristic
            // missed accent-free Spanish/Portuguese ("busco party soy healer").
            var encodedText = Uri.EscapeDataString(text);
            var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=auto&tl={targetLanguage}&dt=t&q={encodedText}";

            Log.Debug("Translating: {Text} (auto→{Target})", text, targetLanguage);

            var response = await _http.GetAsync(url, ct);

            Log.Debug("Translation response: {Status}", response.StatusCode);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(ct);
                var (translated, detectedLang) = ParseGoogleTranslateResponse(json);

                Log.Debug("Translation result: {Result} (detected: {Lang})", translated ?? "null", detectedLang ?? "?");

                if (!string.IsNullOrEmpty(translated))
                {
                    // Google's verdict: already in the target language
                    if (detectedLang == targetLanguage || translated == text)
                    {
                        return new TranslationResult
                        {
                            TranslatedText = text,
                            DetectedLanguage = detectedLang,
                            FromCache = false
                        };
                    }

                    // Cache the result
                    await _cacheLock.WaitAsync(ct);
                    try
                    {
                        _cache[cacheKey] = translated;
                    }
                    finally
                    {
                        _cacheLock.Release();
                    }

                    return new TranslationResult
                    {
                        TranslatedText = translated,
                        DetectedLanguage = detectedLang,
                        FromCache = false
                    };
                }
            }

            Log.Debug("Translation failed: {Status}", response.StatusCode);
            return new TranslationResult { TranslatedText = text, Error = true };
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Translation error");
            return new TranslationResult { TranslatedText = text, Error = true };
        }
    }

    /// <summary>
    /// Parse Google Translate gtx response.
    /// Format: [[["translated","original",null,null,1]],null,"detected_source_lang"]
    /// </summary>
    private static (string? Translated, string? DetectedLanguage) ParseGoogleTranslateResponse(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            string? translated = null;
            string? detected = null;

            if (root.GetArrayLength() > 0)
            {
                var sentences = root[0];
                if (sentences.ValueKind == JsonValueKind.Array && sentences.GetArrayLength() > 0)
                {
                    // Concatenate all sentence segments, not just the first
                    var sb = new System.Text.StringBuilder();
                    foreach (var segment in sentences.EnumerateArray())
                    {
                        if (segment.GetArrayLength() > 0)
                            sb.Append(segment[0].GetString());
                    }
                    translated = sb.Length > 0 ? sb.ToString() : null;
                }

                // Detected source language sits at root[2]
                if (root.GetArrayLength() > 2 && root[2].ValueKind == JsonValueKind.String)
                    detected = root[2].GetString();
            }

            return (translated, detected);
        }
        catch
        {
            // Try simple string extraction as fallback
            var match = System.Text.RegularExpressions.Regex.Match(json, @"\[\[\[""([^""]+)""");
            if (match.Success)
            {
                return (match.Groups[1].Value, null);
            }
        }

        return (null, null);
    }
}

public class TranslationResult
{
    public string TranslatedText { get; set; } = string.Empty;
    public string? DetectedLanguage { get; set; }
    public bool FromCache { get; set; }
    public bool Error { get; set; }
}
