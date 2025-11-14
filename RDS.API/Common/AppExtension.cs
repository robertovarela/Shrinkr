namespace RDS.API.Common;

public static class AppExtension
{
    public static void LoadConfiguration(this WebApplication app)
    {
        // Configurações específicas se necessário
    }

    public static void ConfigureEnvironment(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "ShortUrl API v1");
                options.DocumentTitle = "Documentação ShortUrl API";
                options.RoutePrefix = "swagger";
            });
        }
        else
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            app.UseHsts();
        }
    }

    public static void ConfigureComponents(this WebApplication app)
    {
        app.UseHttpsRedirection();
        app.MapStaticAssets();
        app.UseRouting();
        app.UseAntiforgery();
        app.MapControllers();
    }
}