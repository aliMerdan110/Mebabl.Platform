using Mebabl.Platform.API.DependencyInjection;
using Mebabl.Platform.API.Extensions;
using Mebabl.Platform.Application.DependencyInjection;
using Mebabl.Platform.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);


builder.Services
    .AddPresentation(builder.Configuration)
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UsePresentation();

app.Run();