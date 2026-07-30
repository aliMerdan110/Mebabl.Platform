using Mebabl.Platform.API.Middlewares;

namespace Mebabl.Platform.API.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UsePresentation(this WebApplication app)
    {
       if (app.Environment.IsDevelopment())
{
    // app.MapOpenApi();

    app.UseSwagger();

    app.UseSwaggerUI();
}

        app.UseMiddleware<ExceptionMiddleware>();

        app.UseHttpsRedirection();

        app.UseAuthentication();

        app.UseAuthorization();

        app.MapControllers();

        return app;
    }
}