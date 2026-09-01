using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mebabl.Platform.Application.Features.Database.SecurityRules.CreateSecurityRule;
using Mebabl.Platform.Application.Features.Database.SecurityRules.DeleteSecurityRule;
using Mebabl.Platform.Application.Features.Database.SecurityRules.GetSecurityRules;
using Mebabl.Platform.Application.Features.Database.SecurityRules.UpdateSecurityRule;

namespace Mebabl.Platform.API.Controllers;

[ApiController]
[Route("api/database/security-rules")]
[Authorize]
public sealed class SecurityRulesController : BaseApiController
{

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateSecurityRuleCommand command,
        CancellationToken cancellationToken)
    {
        var id = await Sender.Send(
            command,
            cancellationToken);

        return Ok(new
        {
            id
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

        return Ok(result);
    }



    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateSecurityRuleCommand command,
        CancellationToken cancellationToken)
    {
        command = command with
        {
            Id = id
        };


        await Sender.Send(
            command,
            cancellationToken);

        return NoContent();
    }



    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await Sender.Send(
            new DeleteSecurityRuleCommand(id),
            cancellationToken);

        return NoContent();
    }
}