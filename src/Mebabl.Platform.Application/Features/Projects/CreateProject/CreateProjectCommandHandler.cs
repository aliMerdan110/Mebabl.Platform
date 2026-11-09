using System.Security.Cryptography;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.Projects.DTOs;
using Mebabl.Platform.Domain.Entities.Projects;

namespace Mebabl.Platform.Application.Features.Projects.CreateProject;

public sealed class CreateProjectCommandHandler
    : IRequestHandler<CreateProjectCommand, ProjectDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentDeveloper _currentDeveloper;

    public CreateProjectCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentDeveloper currentDeveloper)
    {
        _dbContext = dbContext;
        _currentDeveloper = currentDeveloper;
    }

    public async Task<ProjectDto> Handle(
        CreateProjectCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentDeveloper.IsAuthenticated)
        {
            throw new UnauthorizedAccessException(
                "Authenticated developer is required.");
        }

        var developerId = _currentDeveloper.DeveloperId;
        var name = request.Name.Trim();
        var code = request.Code.Trim().ToLowerInvariant();

        var exists = await _dbContext.Projects
            .AnyAsync(
                x => x.DeveloperId == developerId && x.Code == code,
                cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException(
                "Project code already exists.");
        }

        var project = new PlatformProject
        {
            DeveloperId = developerId,
            Name = name,
            Code = code,
            Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim(),
            IsActive = true
        };

        var sdkConfiguration = new ProjectSdkConfiguration
        {
            Project = project,
            ProjectNumber = GenerateProjectNumber(),
            PublicKey = GeneratePublicKey(),
            ConfigurationVersion = "1",
            IsActive = true
        };

        _dbContext.Projects.Add(project);
        _dbContext.ProjectSdkConfigurations.Add(sdkConfiguration);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ProjectDto(
            project.Id,
            project.DeveloperId,
            project.Name,
            project.Code,
            project.Description,
            project.IsActive,
            project.CreatedAt,
            project.UpdatedAt,
            0);
    }

    private static string GenerateProjectNumber()
    {
        return RandomNumberGenerator
            .GetInt32(1_000_000_000, 2_000_000_000)
            .ToString();
    }

    private static string GeneratePublicKey()
    {
        Span<byte> bytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(bytes);

        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}