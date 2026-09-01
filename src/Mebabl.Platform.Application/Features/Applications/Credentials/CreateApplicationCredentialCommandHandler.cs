using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Services.Password;
using Mebabl.Platform.Domain.Entities.Identity;
using System.Security.Cryptography;

namespace Mebabl.Platform.Application.Features.Applications.Credentials.CreateCredential;

public sealed class CreateApplicationCredentialCommandHandler
    : IRequestHandler<
        CreateApplicationCredentialCommand,
        CreateApplicationCredentialResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentDeveloper _currentDeveloper;
    private readonly IPasswordHasher _passwordHasher;

    public CreateApplicationCredentialCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentDeveloper currentDeveloper,
        IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _currentDeveloper = currentDeveloper;
        _passwordHasher = passwordHasher;
    }

    public async Task<CreateApplicationCredentialResponse> Handle(
        CreateApplicationCredentialCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentDeveloper.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var application = await _dbContext.Applications
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.ApplicationId &&
                    x.DeveloperId == _currentDeveloper.DeveloperId,
                cancellationToken);

        if (application is null)
            throw new Exception("Application not found.");

        var apiKey = GenerateKey();
        var apiSecret = GenerateKey();

        var credential = new ApplicationCredential
        {
            ApplicationId = application.Id,
            ApiKey = apiKey,
            ApiSecretHash = _passwordHasher.Hash(apiSecret),
            IsActive = true
        };

        _dbContext.ApplicationCredentials.Add(credential);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateApplicationCredentialResponse(
            credential.Id,
            apiKey,
            apiSecret);
    }

    private static string GenerateKey()
    {
        return Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(32));
    }
}