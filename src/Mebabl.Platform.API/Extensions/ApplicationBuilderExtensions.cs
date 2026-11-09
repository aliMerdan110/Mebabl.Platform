using Microsoft.AspNetCore.RateLimiting;
using Mebabl.Platform.API.Middlewares;

namespace Mebabl.Platform.API.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UsePresentation(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseMiddleware<ExceptionMiddleware>();

        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.UseRouting();

        app.UseCors();

        // يجب أن يأتي بعد UseRouting لتطبيق سياسات المسارات.
        app.UseRateLimiter();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        return app;
    }
}