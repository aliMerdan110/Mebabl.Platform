using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mebabl.Platform.Application.Features.Live.Media.Srs;

namespace Mebabl.Platform.API.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/internal/srs")]
public sealed class SrsController : ControllerBase
{
    private readonly ISrsPublishAuthorizationService _authorization;
    private readonly IConfiguration _configuration;

    public SrsController(
        ISrsPublishAuthorizationService authorization,
        IConfiguration configuration)
    {
        _authorization = authorization;
        _configuration = configuration;
    }

    [HttpPost("on-publish")]
    public async Task<IActionResult> OnPublish(
        [FromBody] SrsPublishRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsTrustedSrsCallback())
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new { code = 1 });
        }

        var allowed = await _authorization.AuthorizePublishAsync(
            request,
            cancellationToken);

        if (!allowed)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new { code = 1 });
        }

        return Ok(new { code = 0 });
    }

    [HttpPost("on-unpublish")]
    public async Task<IActionResult> OnUnpublish(
        [FromBody] SrsPublishRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsTrustedSrsCallback())
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new { code = 1 });
        }

        await _authorization.HandleUnpublishAsync(
            request,
            cancellationToken);

        return Ok(new { code = 0 });
    }

    private bool IsTrustedSrsCallback()
    {
        var expected = _configuration["Srs:HookSecret"];
        var supplied = Request.Query["hookKey"].ToString();

        if (string.IsNullOrWhiteSpace(expected) ||
            string.IsNullOrWhiteSpace(supplied))
        {
            return false;
        }

        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        var suppliedBytes = Encoding.UTF8.GetBytes(supplied);

        return CryptographicOperations.FixedTimeEquals(
            expectedBytes,
            suppliedBytes);
    }
}