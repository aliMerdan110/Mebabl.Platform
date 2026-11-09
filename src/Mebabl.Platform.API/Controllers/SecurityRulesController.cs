using System.Globalization;
using System.Text.Json;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Mebabl.Platform.Application.Features.Database.SecurityRules.CreateSecurityRule;
using Mebabl.Platform.Application.Features.Database.SecurityRules.DeleteSecurityRule;
using Mebabl.Platform.Application.Features.Database.SecurityRules.GetSecurityRules;
using Mebabl.Platform.Application.Features.Database.SecurityRules.UpdateSecurityRule;

namespace Mebabl.Platform.API.Controllers;

/// <summary>
/// Developer-managed, versioned collection security policies.
/// Project ownership is enforced by the application handlers; route and body
/// identifiers are never treated as proof of ownership.
/// </summary>
[ApiController]
[Authorize(Policy = "ProjectManagement")]
[Route("api/database/security-policies")]
[Route("api/database/security-rules")] // Backward-compatible route alias.
public sealed class SecurityRulesController : BaseApiController
{
    private const int PreconditionRequired = 428;

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateSecurityPolicyRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var id = await Sender.Send(
            new CreateSecurityRuleCommand(
                request.CollectionId,
                request.Definition),
            cancellationToken);

        Response.Headers["Cache-Control"] = "no-store";

        return Ok(new
        {
            id,
            collectionId = request.CollectionId,
            isActive = false
        });
    }

    [HttpGet("{collectionId:guid}")]
    public async Task<IActionResult> Get(
        Guid collectionId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            new GetSecurityRulesQuery(collectionId),
            cancellationToken);

        Response.Headers["Cache-Control"] = "no-store";
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateSecurityPolicyRequest request,
        [FromHeader(Name = "If-Match")] string? ifMatch,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!TryReadExpectedRevision(ifMatch, out var expectedRevision))
        {
            return InvalidIfMatch(ifMatch);
        }

        await Sender.Send(
            new UpdateSecurityRuleCommand(
                id,
                request.Definition,
                expectedRevision),
            cancellationToken);

        // The update handler increments the revision exactly once.
        Response.Headers["ETag"] = FormatETag(expectedRevision + 1);
        Response.Headers["Cache-Control"] = "no-store";

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        [FromHeader(Name = "If-Match")] string? ifMatch,
        CancellationToken cancellationToken)
    {
        if (!TryReadExpectedRevision(ifMatch, out var expectedRevision))
        {
            return InvalidIfMatch(ifMatch);
        }

        await Sender.Send(
            new DeleteSecurityRuleCommand(id, expectedRevision),
            cancellationToken);

        Response.Headers["Cache-Control"] = "no-store";
        return NoContent();
    }

    private IActionResult InvalidIfMatch(string? ifMatch)
    {
        if (string.IsNullOrWhiteSpace(ifMatch))
        {
            return StatusCode(
                PreconditionRequired,
                new
                {
                    success = false,
                    message = "The If-Match header with the current policy revision is required.",
                    statusCode = PreconditionRequired
                });
        }

        return BadRequest(new
        {
            success = false,
            message = "If-Match must contain one strong numeric entity tag, for example: \"3\".",
            statusCode = StatusCodes.Status400BadRequest
        });
    }

    private static bool TryReadExpectedRevision(
        string? ifMatch,
        out int revision)
    {
        revision = 0;

        if (string.IsNullOrWhiteSpace(ifMatch))
            return false;

        var value = ifMatch.Trim();

        // Weak validators, wildcard validators and multiple validators are
        // deliberately rejected: updates require one exact policy revision.
        if (value.StartsWith("W/", StringComparison.OrdinalIgnoreCase) ||
            value.Length < 3 ||
            value[0] != '"' ||
            value[^1] != '"')
        {
            return false;
        }

        return int.TryParse(
                   value.AsSpan(1, value.Length - 2),
                   NumberStyles.None,
                   CultureInfo.InvariantCulture,
                   out revision) &&
               revision > 0;
    }

    private static string FormatETag(int revision) => $"\"{revision}\"";

    public sealed record CreateSecurityPolicyRequest(
        Guid CollectionId,
        JsonElement Definition);

    public sealed record UpdateSecurityPolicyRequest(
        JsonElement Definition);
}
