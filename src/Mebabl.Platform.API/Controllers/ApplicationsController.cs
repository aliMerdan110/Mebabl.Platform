using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mebabl.Platform.Application.Features.Applications.CreateApplication;
using Mebabl.Platform.Application.Features.Applications.GetApplicationById;
using Mebabl.Platform.Application.Features.Applications.GetMyApplications;
using Mebabl.Platform.Application.Features.Applications.UpdateApplication;
using Mebabl.Platform.Application.Features.Applications.Credentials.CreateCredential;
using Mebabl.Platform.Application.Features.Applications.Credentials.GetCredentials;
using Mebabl.Platform.Application.Features.Applications.Credentials.DisableCredential;
using Mebabl.Platform.Application.Features.Applications.Credentials.EnableCredential;

using Mebabl.Platform.Application.Features.Applications.Users;



namespace Mebabl.Platform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ApplicationsController : BaseApiController
{

      private readonly ISender _sender;
    public ApplicationsController(ISender sender)
    {
        _sender = sender;
    }
    [HttpGet("{applicationId:guid}/users")]
    public async Task<ActionResult<
        IReadOnlyList<ApplicationUserDto>>> GetUsers(
        Guid applicationId,
        CancellationToken cancellationToken)
    {
        var users = await _sender.Send(
            new GetApplicationUsersQuery(applicationId),
            cancellationToken);
        return Ok(users);
    }


    // 
    [HttpPost]
    public async Task<IActionResult> Create(
        CreateApplicationCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            command,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ApplicationItemResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyApplications(
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            new GetMyApplicationsQuery(),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<GetApplicationByIdResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            new GetApplicationByIdQuery(id),
            cancellationToken);

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateApplicationCommand command,
        CancellationToken cancellationToken)
    {
        command = command with { Id = id };

        await Sender.Send(command, cancellationToken);

        return NoContent();
        
    }



    [HttpPost("{id:guid}/credentials")]
[ProducesResponseType<CreateApplicationCredentialResponse>(StatusCodes.Status200OK)]
public async Task<IActionResult> CreateCredential(
    Guid id,
    CancellationToken cancellationToken)
{
    var result = await Sender.Send(
        new CreateApplicationCredentialCommand(id),
        cancellationToken);

    return Ok(result);
}

[HttpGet("{id:guid}/credentials")]
[ProducesResponseType<IReadOnlyList<ApplicationCredentialResponse>>(StatusCodes.Status200OK)]
public async Task<IActionResult> GetCredentials(
    Guid id,
    CancellationToken cancellationToken)
{
    var result = await Sender.Send(
        new GetApplicationCredentialsQuery(id),
        cancellationToken);

    return Ok(result);
}


[HttpPost("{applicationId:guid}/credentials/{credentialId:guid}/disable")]
public async Task<IActionResult> DisableCredential(
    Guid applicationId,
    Guid credentialId,
    CancellationToken cancellationToken)
{
    await Sender.Send(
        new DisableApplicationCredentialCommand(
            applicationId,
            credentialId),
        cancellationToken);

    return NoContent();
}

[HttpPost("{applicationId:guid}/credentials/{credentialId:guid}/enable")]
public async Task<IActionResult> EnableCredential(
    Guid applicationId,
    Guid credentialId,
    CancellationToken cancellationToken)
{
    await Sender.Send(
        new EnableApplicationCredentialCommand(
            applicationId,
            credentialId),
        cancellationToken);

    return NoContent();
}





}