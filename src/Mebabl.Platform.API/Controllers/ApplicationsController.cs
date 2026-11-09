
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Mebabl.Platform.Application.Features.Applications.CreateApplication;
using Mebabl.Platform.Application.Features.Applications.GetApplicationById;
using Mebabl.Platform.Application.Features.Applications.GetMyApplications;
using Mebabl.Platform.Application.Features.Applications.UpdateApplication;
using Mebabl.Platform.Application.Features.Applications.DeleteApplication;

using Mebabl.Platform.Application.Features.Applications.Credentials.CreateCredential;
using Mebabl.Platform.Application.Features.Applications.Credentials.GetCredentials;
using Mebabl.Platform.Application.Features.Applications.Credentials.DisableCredential;
using Mebabl.Platform.Application.Features.Applications.Credentials.EnableCredential;

using Mebabl.Platform.Application.Features.Applications.Users;

namespace Mebabl.Platform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "Developer")]
public sealed class ApplicationsController : BaseApiController
{
    private readonly ISender _sender;

    public ApplicationsController(ISender sender)
    {
        _sender = sender;
    }

    // إنشاء تطبيق
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(
        [FromBody] CreateApplicationCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            command,
            cancellationToken);

        return Ok(result);
    }

    // عرض تطبيقات المطور، مع إمكانية التصفية حسب المشروع
    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<ApplicationItemResponse>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyApplications(
        [FromQuery] Guid? projectId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetMyApplicationsQuery(projectId),
            cancellationToken);

        return Ok(result);
    }

    // عرض تطبيق حسب المعرّف
    [HttpGet("{id:guid}")]
    [ProducesResponseType(
        typeof(GetApplicationByIdResponse),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetApplicationByIdQuery(id),
            cancellationToken);

        return Ok(result);
    }

    // تعديل تطبيق
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateApplicationCommand command,
        CancellationToken cancellationToken)
    {
        command = command with { Id = id };

        await _sender.Send(
            command,
            cancellationToken);

        return NoContent();
    }

    // حذف تطبيق
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new DeleteApplicationCommand(id),
            cancellationToken);

        return NoContent();
    }

    // عرض مستخدمي التطبيق
    [HttpGet("{applicationId:guid}/users")]
    [ProducesResponseType(
        typeof(IReadOnlyList<ApplicationUserDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ApplicationUserDto>>> GetUsers(
        Guid applicationId,
        CancellationToken cancellationToken)
    {
        var users = await _sender.Send(
            new GetApplicationUsersQuery(applicationId),
            cancellationToken);

        return Ok(users);
    }

    // إنشاء بيانات اعتماد للتطبيق
    [HttpPost("{id:guid}/credentials")]
    [ProducesResponseType(
        typeof(CreateApplicationCredentialResponse),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateCredential(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CreateApplicationCredentialCommand(id),
            cancellationToken);

        return Ok(result);
    }

    // عرض بيانات اعتماد التطبيق
    [HttpGet("{id:guid}/credentials")]
    [ProducesResponseType(
        typeof(IReadOnlyList<ApplicationCredentialResponse>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCredentials(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetApplicationCredentialsQuery(id),
            cancellationToken);

        return Ok(result);
    }

    // تعطيل بيانات اعتماد
    [HttpPost(
        "{applicationId:guid}/credentials/{credentialId:guid}/disable")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DisableCredential(
        Guid applicationId,
        Guid credentialId,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new DisableApplicationCredentialCommand(
                applicationId,
                credentialId),
            cancellationToken);

        return NoContent();
    }

    // تفعيل بيانات اعتماد
    [HttpPost(
        "{applicationId:guid}/credentials/{credentialId:guid}/enable")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> EnableCredential(
        Guid applicationId,
        Guid credentialId,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new EnableApplicationCredentialCommand(
                applicationId,
                credentialId),
            cancellationToken);

        return NoContent();
    }
}
