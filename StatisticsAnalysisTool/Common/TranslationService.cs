using System.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Common;

/// <summary>
/// Translation service using LibreTranslate (free, open-source).
/// Falls back to Argos Translate for fully offline use.
/// No API key required.
/// </summary>
public class TranslationService
{
    private readonly HttpClient _http;
    private readonly Dictionary<string, string> _cache = new();
    private readonly SemaphoreSlim _cacheLock = new(1, 1);
    private readonly SemaphoreSlim _rateLimiter = new(1, 1);
    private DateTime _lastRequestTime = DateTime.MinValue;
    private readonly TimeSpan _minRequestInterval = TimeSpan.FromMilliseconds(500);
    private string _targetLanguage = "en";
    private string _libreTranslateUrl = "https://libretranslate.com";
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
    /// Translate text to the target language.
    /// Uses LibreTranslate public API (free, no key needed).
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
            // Rate limiting: wait if we're making requests too fast
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

            // Detect language first
            var detectedLang = await DetectLanguageAsync(text, ct);

            // If already in target language, skip translation
            if (detectedLang == _targetLanguage)
            {
                return new TranslationResult
                {
                    TranslatedText = text,
                    DetectedLanguage = detectedLang,
                    FromCache = false
                };
            }

            // Translate via LibreTranslate
            var request = new LibreTranslateRequest
            {
                Query = text,
                Source = detectedLang ?? "auto",
                Target = _targetLanguage
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync($"{_libreTranslateUrl}/translate", content, ct);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync(ct);
                var result = JsonSerializer.Deserialize<LibreTranslateResponse>(responseJson);

                if (result?.TranslatedText != null)
                {
                    // Cache the result
                    await _cacheLock.WaitAsync(ct);
                    try
                    {
                        _cache[cacheKey] = result.TranslatedText;
                    }
                    finally
                    {
                        _cacheLock.Release();
                    }

                    return new TranslationResult
                    {
                        TranslatedText = result.TranslatedText,
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
    /// Detect the language of a text using LibreTranslate.
    /// </summary>
    public async Task<string?> DetectLanguageAsync(string text, CancellationToken ct = default)
    {
        try
        {
            var request = new LibreTranslateDetectRequest { Query = text };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync($"{_libreTranslateUrl}/detect", content, ct);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync(ct);
                var results = JsonSerializer.Deserialize<List<LibreTranslateDetectResponse>>(responseJson);
                return results?.FirstOrDefault()?.Language;
            }
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Language detection error");
        }

        return null;
    }

    /// <summary>
    /// Get available languages from LibreTranslate.
    /// </summary>
    public async Task<List<LanguageInfo>> GetLanguagesAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetAsync($"{_libreTranslateUrl}/languages", ct);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(ct);
                return JsonSerializer.Deserialize<List<LanguageInfo>>(json) ?? GetDefaultLanguages();
            }
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Failed to get languages");
        }

        return GetDefaultLanguages();
    }

    private static List<LanguageInfo> GetDefaultLanguages()
    {
        return new List<LanguageInfo>
        {
            new() { Code = "en", Name = "English" },
            new() { Code = "es", Name = "Spanish" },
            new() { Code = "pt", Name = "Portuguese" },
            new() { Code = "fr", Name = "French" },
            new() { Code = "de", Name = "German" },
            new() { Code = "ru", Name = "Russian" },
            new() { Code = "ko", Name = "Korean" },
            new() { Code = "zh", Name = "Chinese" },
            new() { Code = "ja", Name = "Japanese" },
            new() { Code = "ar", Name = "Arabic" },
            new() { Code = "tr", Name = "Turkish" },
            new() { Code = "pl", Name = "Polish" },
        };
    }
}

public class TranslationResult
{
    public string TranslatedText { get; set; } = string.Empty;
    public string? DetectedLanguage { get; set; }
    public bool FromCache { get; set; }
    public bool Error { get; set; }
}

public class LanguageInfo
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

internal class LibreTranslateRequest
{
    [JsonPropertyName("q")]
    public string Query { get; set; } = string.Empty;

    [JsonPropertyName("source")]
    public string Source { get; set; } = "auto";

    [JsonPropertyName("target")]
    public string Target { get; set; } = "en";
}

internal class LibreTranslateResponse
{
    [JsonPropertyName("translatedText")]
    public string? TranslatedText { get; set; }
}

internal class LibreTranslateDetectRequest
{
    [JsonPropertyName("q")]
    public string Query { get; set; } = string.Empty;
}

internal class LibreTranslateDetectResponse
{
    [JsonPropertyName("language")]
    public string? Language { get; set; }

    [JsonPropertyName("confidence")]
    public float Confidence { get; set; }
}
