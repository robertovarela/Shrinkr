using System.Reflection;
using HashidsNet;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.OpenApi.Models;
using RDS.Application.Services;
using RDS.Core.Interfaces;
using RDS.Infraestructure;
using RDS.Infraestructure.Repositories;

namespace RDS.API.Common;

public static class BuilderExtension
{
    public static void AddConfiguration(this WebApplicationBuilder builder)
    {
        //builder.Services.AddMudServices();
        builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies();

        ConfigurationApi.ConnectionString = builder
            .Configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        ConfigurationApi.BackendUrl = builder.Configuration.GetValue<string>("BackendUrl") ?? string.Empty;
        ConfigurationApi.FrontendUrl = builder.Configuration.GetValue<string>("FrontendUrl") ?? string.Empty;
    }

    public static void AddServiceContainer(this WebApplicationBuilder builder)
    {
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
            //.AddInteractiveWebAssemblyComponents()
            //.AddAuthenticationStateSerialization();

        builder.Services.AddCascadingAuthenticationState();

        // API Controllers com validação
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddMemoryCache();
    }

    public static void AddDataContexts(this WebApplicationBuilder builder)
    {
        // Registra a fábrica de DbContext para ser usada nos repositórios (evita problemas de concorrência).
        builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
            options.UseSqlServer(ConfigurationApi.ConnectionString,
                b => b.MigrationsAssembly("RDS.API")));

        // DbContext scoped para compatibilidade
        builder.Services.AddScoped<ApplicationDbContext>(provider =>
        {
            var factory = provider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
            return factory.CreateDbContext();
        });

    }

    public static void AddCrossOrigin(this WebApplicationBuilder builder)
    {
        var isDevelopment = builder.Environment.IsDevelopment();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(ConfigurationApi.CorsPolicyName, policy =>
            {
                if (isDevelopment)
                {
                    // Em desenvolvimento, permite qualquer origem para facilitar testes com Swagger
                    policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                }
                else
                {
                    // Em produção, restringe aos domínios configurados
                    policy.WithOrigins(
                            ConfigurationApi.BackendUrl,
                            ConfigurationApi.FrontendUrl
                        )
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                }
            });
        });
    }

    public static void AddDocumentation(this WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "ShortUrl API",
                Version = "v1",
                Description = @"API para encurtamento de URLs com alta performance e escalabilidade.
                **Requisitos do Sistema:**
                - Suporta 100 milhões de URLs/dia (1160 RPS escrita, 11.600 RPS leitura)
                - URLs armazenadas por 10 anos
                - Código curto: 7 caracteres (62^7 = ~3,5 trilhões de URLs possíveis)
                - Caracteres permitidos: 0-9, a-z, A-Z",
                Contact = new OpenApiContact
                {
                    Name = "Equipe ShortUrl",
                    Email = "suporte@mysoftwares.com.br"
                }
            });

            options.CustomSchemaIds(type => type.FullName);

            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        });
    }

    public static void AddRepositories(this WebApplicationBuilder builder)
    {
        // Registre a implementação concreta para que ela possa ser injetada no decorador.
        builder.Services.AddScoped<ShortUrlRepository>();

        // Registre o decorador em cache como IShortUrlRepository.
        builder.Services.AddScoped<IShortUrlRepository>(provider =>
        {
            var inner = provider.GetRequiredService<ShortUrlRepository>();
            var cache = provider.GetRequiredService<IMemoryCache>();
            var logger = provider.GetRequiredService<ILogger<CachedShortUrlRepository>>();
            return new CachedShortUrlRepository(inner, cache, logger);
        });
    }

    public static void AddServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpClient();

        // Serviços para URL Shortener
        // Repositório já registrado em AddRepositories
        builder.Services.AddScoped<IUrlShorteningService, UrlShorteningService>();

        builder.Services.AddSingleton<IHashids>(_ =>
            new Hashids(builder.Configuration.GetValue<string>("UrlShortener:Salt") ?? "default-salt",
                builder.Configuration.GetValue<int>("UrlShortener:MinLength")));
    }
}