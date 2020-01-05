using System;
using System.IO;
using System.Text;
using System.Text.Json;
using MessagingApi;
using MessagingApi.SendGrid;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using AspNetCore.Identity.Mongo;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;
using System.Reflection;
using CadmusApi.Services;
using CadmusApi.Models;
using AspNetCore.Identity.Mongo.Model;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Cadmus.Core;
using Cadmus.Seed;

namespace CadmusApi
{
    /// <summary>
    /// Startup.
    /// </summary>
    public sealed class Startup
    {
        /// <summary>
        /// Gets the configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Gets the host environment.
        /// </summary>
        public IHostEnvironment HostEnvironment { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="environment">The environment.</param>
        public Startup(IConfiguration configuration, IHostEnvironment environment)
        {
            Configuration = configuration;
            HostEnvironment = environment;
        }

        private void ConfigureOptionsServices(IServiceCollection services)
        {
            // configuration sections
            // https://andrewlock.net/adding-validation-to-strongly-typed-configuration-objects-in-asp-net-core/
            services.Configure<MessagingOptions>(Configuration.GetSection("Messaging"));
            services.Configure<SendGridMailerOptions>(Configuration.GetSection("SendGrid"));

            // explicitly register the settings object by delegating to the IOptions object
            services.AddSingleton(resolver =>
                resolver.GetRequiredService<IOptions<MessagingOptions>>().Value);
            services.AddSingleton(resolver =>
                resolver.GetRequiredService<IOptions<SendGridMailerOptions>>().Value);
        }

        private void ConfigureCorsServices(IServiceCollection services)
        {
            services.AddCors(o => o.AddPolicy("CorsPolicy", builder =>
            {
                builder.AllowAnyMethod()
                    .AllowAnyHeader()
                    // https://github.com/aspnet/SignalR/issues/2110 for AllowCredentials
                    .AllowCredentials()
                    .WithOrigins("http://localhost:4200",
                                 "http://www.fusisoft.it/",
                                 "https://www.fusisoft.it/");
            }));
        }

        private void ConfigureAuthServices(IServiceCollection services)
        {
            // identity
            string connStringTemplate = Configuration.GetConnectionString("Default");

            services.AddIdentityMongoDbProvider<ApplicationUser, MongoRole>(
                options => {},
                mongoOptions =>
                {
                    mongoOptions.ConnectionString =
                        string.Format(connStringTemplate,
                        Configuration.GetSection("Databases")["Auth"]);
                });

            // authentication service
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                })
               .AddJwtBearer(options =>
               {
                   // NOTE: remember to set the values in configuration:
                   // Jwt:SecureKey, Jwt:Audience, Jwt:Issuer
                   IConfigurationSection jwtSection = Configuration.GetSection("Jwt");
                   string key = jwtSection["SecureKey"];
                   if (string.IsNullOrEmpty(key))
                       throw new ApplicationException("Required JWT SecureKey not found");

                   options.SaveToken = true;
                   options.RequireHttpsMetadata = false;
                   options.TokenValidationParameters = new TokenValidationParameters()
                   {
                       ValidateIssuer = true,
                       ValidateAudience = true,
                       ValidAudience = jwtSection["Audience"],
                       ValidIssuer = jwtSection["Issuer"],
                       IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
                   };
               });
#if DEBUG
            // use to show more information when troubleshooting JWT issues
            IdentityModelEventSource.ShowPII = true;
#endif
        }

        private void ConfigureSwaggerServices(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "API",
                    Description = "Cadmus Services"
                });
                c.DescribeAllParametersInCamelCase();

                // include XML comments
                // (remember to check the build XML comments in the prj props)
                string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                    c.IncludeXmlComments(xmlPath);

                // JWT
                // cf. https://ppolyzos.com/2017/10/30/add-jwt-bearer-authorization-to-swagger-and-asp-net-core/
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header with Bearer scheme",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });
            });
        }

        /// <summary>
        /// Configures the services.
        /// </summary>
        /// <param name="services">The services.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // configuration
            ConfigureOptionsServices(services);

            // CORS (before MVC)
            ConfigureCorsServices(services);

            // base services
            services.AddControllers();
            // camel-case JSON in response
            services.AddMvc()
                // https://docs.microsoft.com/en-us/aspnet/core/migration/22-to-30?view=aspnetcore-2.2&tabs=visual-studio#jsonnet-support
                // Newtonsoft is required by MongoDB
                .AddNewtonsoftJson()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy =
                        JsonNamingPolicy.CamelCase;
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            // authentication
            ConfigureAuthServices(services);

            // Add framework services
            // for IMemoryCache: https://docs.microsoft.com/en-us/aspnet/core/performance/caching/memory
            services.AddMemoryCache();

            // user repository service
            services.AddTransient<IUserRepository<ApplicationUser>,
                ApplicationUserRepository>();

            // message builder service
            // TODO: replace with a true mailer service once we have SMTP
            services.AddTransient<IMailerService, NullMailerService>();

            services.AddTransient<IMessageBuilderService,
                FileMessageBuilderService>();

            // add configuration
            services.AddSingleton(_ => Configuration);
            services.AddSingleton<IRepositoryProvider, StandardRepositoryProvider>();
            services.AddSingleton<IPartSeederFactoryProvider, StandardPartSeederFactoryProvider>();

            // swagger
            ConfigureSwaggerServices(services);

            // serilog
            // Install-Package Serilog.Exceptions Serilog.Sinks.MongoDB
            // https://github.com/RehanSaeed/Serilog.Exceptions
            string maxSize = Configuration["Serilog:MaxMbSize"];
            services.AddSingleton<ILogger>(_ => new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.WithExceptionDetails()
                .WriteTo.Console()
                /*.WriteTo.MSSqlServer(Configuration["Serilog:ConnectionString"],*/
                .WriteTo.MongoDBCapped(Configuration["Serilog:ConnectionString"],
                    cappedMaxSizeMb: !string.IsNullOrEmpty(maxSize) &&
                        int.TryParse(maxSize, out int n) && n > 0 ? n : 10)
                    .CreateLogger());
        }

        /// <summary>
        /// Configures the specified application.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="env">The environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            // CORS
            app.UseCors("CorsPolicy");
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // Swagger
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                // options.BooleanValues(new object[] { 0, 1 });
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "V1 Docs");
                // options.ShowJsonEditor();
            });
        }
    }
}
