// Copyright (c) 2025 Nikolaos Protopapas
// Licensed under the MIT License

using Microsoft.AspNetCore.Mvc;
using LocalizationManager.Core;
using LocalizationManager.Core.Validation;
using LocalizationManager.Shared.Enums;
using LocalizationManager.Models.Api;

namespace LocalizationManager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ValidationController : ControllerBase
{
    private readonly string _resourcePath;
    private readonly ResourceFileParser _parser;
    private readonly ResourceDiscovery _discovery;
    private readonly ResourceValidator _validator;

    public ValidationController(IConfiguration configuration)
    {
        _resourcePath = configuration["ResourcePath"] ?? Directory.GetCurrentDirectory();
        _parser = new ResourceFileParser();
        _discovery = new ResourceDiscovery();
        _validator = new ResourceValidator();
    }

    /// <summary>
    /// Validate all resource files
    /// </summary>
    [HttpPost("validate")]
    public ActionResult<ValidationResponse> Validate([FromBody] ValidateRequest? request)
    {
        try
        {
            var languages = _discovery.DiscoverLanguages(_resourcePath);
            var resourceFiles = languages.Select(l => _parser.Parse(l)).ToList();

            var placeholderTypes = request?.EnabledPlaceholderTypes ?? PlaceholderType.All;
            var result = _validator.Validate(resourceFiles, placeholderTypes);

            return Ok(new ValidationResponse
            {
                IsValid = result.IsValid,
                MissingKeys = result.MissingKeys,
                DuplicateKeys = result.DuplicateKeys,
                EmptyValues = result.EmptyValues,
                ExtraKeys = result.ExtraKeys,
                PlaceholderMismatches = result.PlaceholderMismatches.ToDictionary(
                    kv => kv.Key,
                    kv => kv.Value.Select(v => v.Key).ToList()
                ),
                Summary = new ValidationSummary
                {
                    TotalIssues = result.TotalIssues,
                    MissingCount = result.MissingKeys.Sum(kv => kv.Value.Count),
                    DuplicatesCount = result.DuplicateKeys.Sum(kv => kv.Value.Count),
                    EmptyCount = result.EmptyValues.Sum(kv => kv.Value.Count),
                    ExtraCount = result.ExtraKeys.Sum(kv => kv.Value.Count),
                    PlaceholderCount = result.PlaceholderMismatches.Sum(kv => kv.Value.Count)
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse { Error = ex.Message });
        }
    }

    /// <summary>
    /// Get validation issues summary
    /// </summary>
    [HttpGet("issues")]
    public ActionResult<ValidationSummary> GetIssues()
    {
        try
        {
            var languages = _discovery.DiscoverLanguages(_resourcePath);
            var resourceFiles = languages.Select(l => _parser.Parse(l)).ToList();

            var result = _validator.Validate(resourceFiles);

            return Ok(new ValidationSummary
            {
                HasIssues = !result.IsValid,
                MissingCount = result.MissingKeys.Sum(kv => kv.Value.Count),
                DuplicatesCount = result.DuplicateKeys.Sum(kv => kv.Value.Count),
                EmptyCount = result.EmptyValues.Sum(kv => kv.Value.Count),
                ExtraCount = result.ExtraKeys.Sum(kv => kv.Value.Count),
                PlaceholderCount = result.PlaceholderMismatches.Sum(kv => kv.Value.Count),
                TotalIssues = result.TotalIssues
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse { Error = ex.Message });
        }
    }
}
