using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Services.Password;
using Mebabl.Platform.Domain.Entities.Identity;
using System.Security.Cryptography;
using Mebabl.Platform.Application.Common.Services.ApplicationInitialization;
using Mebabl.Platform.Application.Common.Providers;


namespace Mebabl.Platform.Application.Features.Applications.CreateApplication;

public sealed class CreateApplicationCommandHandler
    : IRequestHandler<CreateApplicationCommand, CreateApplicationResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentDeveloper _currentDeveloper;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IApplicationInitializer _initializer;

   public CreateApplicationCommandHandler(
    IApplicationDbContext dbContext,
    ICurrentDeveloper currentDeveloper,
    IPasswordHasher passwordHasher,
    IApplicationInitializer initializer)
{
    _dbContext = dbContext;
    _currentDeveloper = currentDeveloper;
    _passwordHasher = passwordHasher;
    _initializer = initializer;
}

    public async Task<CreateApplicationResponse> Handle(
        CreateApplicationCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentDeveloper.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var code = request.Code
            .Trim()
            .ToLowerInvariant();

        var codeExists = await _dbContext.Applications
            .AnyAsync(
                x => x.Code == code,
                cancellationToken);

        if (codeExists)
            throw new Exception("Application code already exists.");

        var apiKey = GenerateKey();
        var apiSecret = GenerateKey();

        var application = new PlatformApplication
        {
            DeveloperId = _currentDeveloper.DeveloperId,
            Name = request.Name,
            Code = code,
            Description = request.Description
        };

        application.AuthProviders.Add(
    new ApplicationAuthProvider
    {
        Provider = AuthenticationProviders.EmailPassword,
        IsEnabled = true
    });

application.AuthProviders.Add(
    new ApplicationAuthProvider
    {
        Provider = AuthenticationProviders.EmailLink,
        IsEnabled = false
    });

application.AuthProviders.Add(
    new ApplicationAuthProvider
    {
        Provider = AuthenticationProviders.Phone,
        IsEnabled = false
    });

application.AuthProviders.Add(
    new ApplicationAuthProvider
    {
        Provider = AuthenticationProviders.Anonymous,
        IsEnabled = false
    });

application.AuthProviders.Add(
    new ApplicationAuthProvider
    {
        Provider = AuthenticationProviders.Google,
        IsEnabled = false
    });

application.AuthProviders.Add(
    new ApplicationAuthProvider
    {
        Provider = AuthenticationProviders.Facebook,
        IsEnabled = false
    });

application.AuthProviders.Add(
    new ApplicationAuthProvider
    {
        Provider = AuthenticationProviders.Apple,
        IsEnabled = false
    });

application.AuthProviders.Add(
    new ApplicationAuthProvider
    {
        Provider = AuthenticationProviders.GitHub,
        IsEnabled = false
    });

application.AuthProviders.Add(
    new ApplicationAuthProvider
    {
        Provider = AuthenticationProviders.Microsoft,
        IsEnabled = false
    });

application.AuthProviders.Add(
    new ApplicationAuthProvider
    {
        Provider = AuthenticationProviders.Twitter,
        IsEnabled = false
    });

application.AuthProviders.Add(
    new ApplicationAuthProvider
    {
        Provider = AuthenticationProviders.Yahoo,
        IsEnabled = false
    });

        application.Credentials.Add(new ApplicationCredential
        {
            ApiKey = apiKey,
            ApiSecretHash = _passwordHasher.Hash(apiSecret)
        });

        _dbContext.Applications.Add(application);

        await _dbContext.SaveChangesAsync(cancellationToken);


await _initializer.InitializeAsync(
    application.Id,
    cancellationToken); 

        return new CreateApplicationResponse(
            application.Id,
            application.Name,
            application.Code,
            apiKey,
            apiSecret);
    }

    private static string GenerateKey()
    {
        return Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(32));
    }
}