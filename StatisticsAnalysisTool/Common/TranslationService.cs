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
/// Translation service using MyMemory API (free, no API key required).
/// Falls back gracefully if translation fails.
/// </summary>
public class TranslationService
{
    private readonly HttpClient _http;
    private readonly Dictionary<string, string> _cache = new();
    private readonly SemaphoreSlim _cacheLock = new(1, 1);
    private readonly SemaphoreSlim _rateLimiter = new(1, 1);
    private DateTime _lastRequestTime = DateTime.MinValue;
    private readonly TimeSpan _minRequestInterval = TimeSpan.FromMilliseconds(300);
    private string _targetLanguage = "en";
    private bool _enabled = true;

    public static TranslationService Instance { get; } = new();

    private TranslationService()
    {
        _http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };
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
    /// Translate text using MyMemory API (free, no key needed).
    /// </summary>
    public async Task<TranslationResult> TranslateAsync(string text, CancellationToken ct = default)
    {
        if (!_enabled || string.IsNullOrWhiteSpace(text))
            return new TranslationResult { TranslatedText = text };

        // Check cache
        var cacheKey = $"{text}:{_targetLanguage}";
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

            // Detect language (simple heuristic for now)
            var detectedLang = DetectLanguageSimple(text);

            // If already in target language, skip
            if (detectedLang == _targetLanguage)
            {
                return new TranslationResult
                {
                    TranslatedText = text,
                    DetectedLanguage = detectedLang,
                    FromCache = false
                };
            }

            // Translate via MyMemory
            var encodedText = Uri.EscapeDataString(text);
            var url = $"https://api.mymemory.translated.net/get?q={encodedText}&langpair={detectedLang}|{_targetLanguage}";

            var response = await _http.GetAsync(url, ct);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(ct);
                var result = JsonSerializer.Deserialize<MyMemoryResponse>(json);

                if (result?.ResponseData?.TranslatedText != null)
                {
                    var translated = result.ResponseData.TranslatedText;

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
    /// Simple language detection based on common patterns.
    /// </summary>
    private static string DetectLanguageSimple(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "en";

        // Spanish indicators
        if (text.Contains("¿") || text.Contains("¡") || 
            text.Contains("ñ") || text.Contains("á") || text.Contains("é") ||
            text.Contains("í") || text.Contains("ó") || text.Contains("ú"))
            return "es";

        // Portuguese indicators
        if (text.Contains("ã") || text.Contains("õ") || text.Contains("ç"))
            return "pt";

        // French indicators
        if (text.Contains("à") || text.Contains("â") || text.Contains("ê") ||
            text.Contains("ë") || text.Contains("î") || text.Contains("ô") ||
            text.Contains("ù") || text.Contains("û"))
            return "fr";

        // German indicators
        if (text.Contains("ä") || text.Contains("ö") || text.Contains("ü") ||
            text.Contains("ß"))
            return "de";

        // Russian indicators
        if (text.Any(c => c >= 0x0400 && c <= 0x04FF))
            return "ru";

        // Korean indicators
        if (text.Any(c => c >= 0xAC00 && c <= 0xD7AF))
            return "ko";

        // Chinese indicators
        if (text.Any(c => c >= 0x4E00 && c <= 0x9FFF))
            return "zh";

        // Japanese indicators
        if (text.Any(c => (c >= 0x3040 && c <= 0x309F) || (c >= 0x30A0 && c <= 0x30FF)))
            return "ja";

        // Arabic indicators
        if (text.Any(c => c >= 0x0600 && c <= 0x06FF))
            return "ar";

        // Default to English
        return "en";
    }
}

public class TranslationResult
{
    public string TranslatedText { get; set; } = string.Empty;
    public string? DetectedLanguage { get; set; }
    public bool FromCache { get; set; }
    public bool Error { get; set; }
}

internal class MyMemoryResponse
{
    [JsonPropertyName("responseData")]
    public MyMemoryResponseData? ResponseData { get; set; }

    [JsonPropertyName("responseStatus")]
    public int ResponseStatus { get; set; }
}

internal class MyMemoryResponseData
{
    [JsonPropertyName("translatedText")]
    public string? TranslatedText { get; set; }

    [JsonPropertyName("match")]
    public double Match { get; set; }
}
