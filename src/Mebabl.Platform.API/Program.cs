using Mebabl.Platform.API.DependencyInjection;
using Mebabl.Platform.API.Extensions;
using Mebabl.Platform.Infrastructure.DependencyInjection;
using Mebabl.Platform.Application.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddPresentation(builder.Configuration);

builder.Services.AddApplication();

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseApplicationMiddlewares();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();