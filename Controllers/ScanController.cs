// Copyright (c) 2025 Nikolaos Protopapas
// Licensed under the MIT License

using Microsoft.AspNetCore.Mvc;
using LocalizationManager.Core;
using LocalizationManager.Core.Scanning;
using LocalizationManager.Models.Api;

namespace LocalizationManager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScanController : ControllerBase
{
    private readonly string _resourcePath;
    private readonly string _sourcePath;
    private readonly ResourceFileParser _parser;
    private readonly ResourceDiscovery _discovery;
    private readonly CodeScanner _scanner;

    public ScanController(IConfiguration configuration)
    {
        _resourcePath = configuration["ResourcePath"] ?? Directory.GetCurrentDirectory();
        _sourcePath = configuration["SourcePath"] ?? Directory.GetParent(_resourcePath)?.FullName ?? _resourcePath;
        _parser = new ResourceFileParser();
        _discovery = new ResourceDiscovery();
        _scanner = new CodeScanner();
    }

    /// <summary>
    /// Scan source code for localization key references
    /// </summary>
    [HttpPost("scan")]
    public ActionResult<ScanResponse> Scan([FromBody] ScanRequest? request)
    {
        try
        {
            var languages = _discovery.DiscoverLanguages(_resourcePath);
            var resourceFiles = languages.Select(l => _parser.Parse(l)).ToList();

            var defaultFile = resourceFiles.FirstOrDefault(f => f.Language.IsDefault);
            if (defaultFile == null)
            {
                return StatusCode(500, new ErrorResponse { Error = "No default language file found" });
            }

            // Get all keys from resource files
            var allResourceKeys = defaultFile.Entries.Select(e => e.Key).Distinct().ToHashSet();

            // Scan source code
            var excludePatterns = request?.ExcludePatterns ?? new List<string>
            {
                "**/bin/**",
                "**/obj/**",
                "**/node_modules/**",
                "**/.git/**"
            };

            var result = _scanner.Scan(_sourcePath, resourceFiles, false, excludePatterns, null, null);

            return Ok(new ScanResponse
            {
                ScannedFiles = result.FilesScanned,
                TotalReferences = result.TotalReferences,
                UniqueKeysFound = result.UniqueKeysFound,
                UnusedKeysCount = result.UnusedKeys.Count,
                MissingKeysCount = result.MissingKeys.Count,
                Unused = result.UnusedKeys,
                Missing = result.MissingKeys.Select(k => k.Key).ToList(),
                References = result.AllKeyUsages.Select(k => new KeyReferenceInfo
                {
                    Key = k.Key,
                    ReferenceCount = k.References.Count,
                    References = k.References.Select(r => new CodeReference
                    {
                        File = r.FilePath,
                        Line = r.Line,
                        Pattern = r.Pattern,
                        Confidence = r.Confidence.ToString()
                    }).ToList()
                }).ToList()
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorResponse { Error = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Get unused keys
    /// </summary>
    [HttpGet("unused")]
    public ActionResult<UnusedKeysResponse> GetUnused()
    {
        try
        {
            var languages = _discovery.DiscoverLanguages(_resourcePath);
            var resourceFiles = languages.Select(l => _parser.Parse(l)).ToList();

            var defaultFile = resourceFiles.FirstOrDefault(f => f.Language.IsDefault);
            if (defaultFile == null)
            {
                return StatusCode(500, new ErrorResponse { Error = "No default language file found" });
            }

            var excludePatterns = new List<string>
            {
                "**/bin/**", "**/obj/**", "**/node_modules/**", "**/.git/**"
            };

            var result = _scanner.Scan(_sourcePath, resourceFiles, false, excludePatterns, null, null);

            return Ok(new UnusedKeysResponse { UnusedKeys = result.UnusedKeys });
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorResponse { Error = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Get missing keys (in code but not in resources)
    /// </summary>
    [HttpGet("missing")]
    public ActionResult<MissingKeysResponse> GetMissing()
    {
        try
        {
            var languages = _discovery.DiscoverLanguages(_resourcePath);
            var resourceFiles = languages.Select(l => _parser.Parse(l)).ToList();

            var defaultFile = resourceFiles.FirstOrDefault(f => f.Language.IsDefault);
            if (defaultFile == null)
            {
                return StatusCode(500, new ErrorResponse { Error = "No default language file found" });
            }

            var excludePatterns = new List<string>
            {
                "**/bin/**", "**/obj/**", "**/node_modules/**", "**/.git/**"
            };

            var result = _scanner.Scan(_sourcePath, resourceFiles, false, excludePatterns, null, null);

            return Ok(new MissingKeysResponse { MissingKeys = result.MissingKeys.Select(k => k.Key).ToList() });
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorResponse { Error = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Get code references for a specific key
    /// </summary>
    [HttpGet("references/{keyName}")]
    public ActionResult<KeyReferencesResponse> GetReferences(string keyName)
    {
        try
        {
            var languages = _discovery.DiscoverLanguages(_resourcePath);
            var resourceFiles = languages.Select(l => _parser.Parse(l)).ToList();

            var excludePatterns = new List<string>
            {
                "**/bin/**", "**/obj/**", "**/node_modules/**", "**/.git/**"
            };

            var result = _scanner.Scan(_sourcePath, resourceFiles, false, excludePatterns, null, null);
            var keyUsage = result.AllKeyUsages.FirstOrDefault(k => k.Key == keyName);

            if (keyUsage == null)
            {
                return NotFound(new ErrorResponse { Error = $"No references found for key '{keyName}'" });
            }

            return Ok(new KeyReferencesResponse
            {
                Key = keyName,
                ReferenceCount = keyUsage.References.Count,
                References = keyUsage.References.Select(r => new CodeReference
                {
                    File = r.FilePath,
                    Line = r.Line,
                    Pattern = r.Pattern,
                    Confidence = r.Confidence.ToString()
                }).ToList()
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorResponse { Error = "An error occurred while processing your request" });
        }
    }
}
