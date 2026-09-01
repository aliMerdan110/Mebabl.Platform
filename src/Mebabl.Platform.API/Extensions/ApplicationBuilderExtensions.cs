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

        // يجب أن يأتي قبل CORS حتى يستطيع CORS معرفة الـ endpoint المطلوب
        app.UseRouting();

        // يعالج طلبات OPTIONS قبل المصادقة
        app.UseCors("MebablConsole");

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        return app;
    }
}