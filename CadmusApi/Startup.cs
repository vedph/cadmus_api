using System.IO;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CadmusApi.Models;
using CadmusApi.Services;
using Microsoft.Extensions.PlatformAbstractions;
using Swashbuckle.AspNetCore.Swagger;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;
using Serilog;

namespace CadmusApi
{
    /// <summary>
    /// Startup.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Gets the configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Configures the services.
        /// </summary>
        /// <param name="services">The services.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // setup options with DI
            // https://docs.asp.net/en/latest/fundamentals/configuration.html
            services.AddOptions();

            services.AddCors();

            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver =
                        new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
                });

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                // Configure the context to use Microsoft SQL Server.
                options.UseSqlServer(Configuration["Data:DefaultConnection:ConnectionString"]);

                // Register the entity sets needed by OpenIddict.
                // Note: use the generic overload if you need
                // to replace the default OpenIddict entities.
                options.UseOpenIddict();
            });

            // Register the Identity services.
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Configure Identity to use the same JWT claims as OpenIddict instead
            // of the legacy WS-Federation claims it uses by default (ClaimTypes),
            // which saves you from doing the mapping in your authorization controller.
            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = OpenIdConnectConstants.Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;
            });

            // AddCore etc: https://github.com/openiddict/openiddict-core/issues/593
            services.AddOpenIddict()
                .AddCore(options =>
                {
                    options.UseDefaultModels();

                    // register the Entity Framework stores.
                    options.AddEntityFrameworkCoreStores<ApplicationDbContext>();
                })
                .AddServer(options =>
                {
                    // register the ASP.NET Core MVC binder used by OpenIddict.
                    // Note: if you don't call this method, you won't be able to
                    // bind OpenIdConnectRequest or OpenIdConnectResponse parameters
                    options.AddMvcBinders();

                    // enable the token endpoints
                    options.EnableTokenEndpoint("/connect/token");
                    options.EnableLogoutEndpoint("/connect/logout");
                    // http://openid.net/specs/openid-connect-core-1_0.html#UserInfo
                    options.EnableUserinfoEndpoint("/connect/userinfo");

                    // enable the password flow
                    options.AllowPasswordFlow();
                    options.AllowRefreshTokenFlow();

                    // during development, you can disable the HTTPS requirement.
                    options.DisableHttpsRequirement();
                })
                .AddValidation();

            //// Register the OpenIddict services.
            //// Note: use the generic overload if you need
            //// to replace the default OpenIddict entities.
            //services.AddOpenIddict(options =>
            //{
            //    // Register the Entity Framework stores.
            //    options.AddEntityFrameworkCoreStores<ApplicationDbContext>();

            //    // Register the ASP.NET Core MVC binder used by OpenIddict.
            //    // Note: if you don't call this method, you won't be able to
            //    // bind OpenIdConnectRequest or OpenIdConnectResponse parameters.
            //    options.AddMvcBinders();

            //    // Enable the token endpoint (required to use the password flow).
            //    options.EnableTokenEndpoint("/connect/token");

            //    // Allow client applications to use the grant_type=password flow.
            //    options.AllowPasswordFlow();
            //    options.AllowRefreshTokenFlow();

            //    // During development, you can disable the HTTPS requirement.
            //    options.DisableHttpsRequirement();
            //});

            //services.AddAuthentication()
            //    .AddOAuthValidation();

            // Add framework services
            // for IMemoryCache: https://docs.microsoft.com/en-us/aspnet/core/performance/caching/memory
            services.AddMemoryCache();

            // add my services
            // services.AddTransient<ISomeService, SomeServiceImpl>();

            // add configuration
            services.AddSingleton(_ => Configuration);
            services.AddSingleton<RepositoryService, RepositoryService>();

            // database seeder
            services.AddTransient<IDatabaseInitializer, DatabaseInitializer>();

            // swagger
            // https://github.com/domaindrivendev/Swashbuckle.AspNetCore
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info
                {
                    Title = "Cadmus API",
                    Version = "v1",
                    Description = "Cadmus API"
                });
                options.DescribeAllParametersInCamelCase();

#if DEBUG
                // TODO find how this can work in Docker and add COPY to Dockerfile
                string basePath = PlatformServices.Default.Application.ApplicationBasePath;
                string xmlPath = Path.Combine(basePath, "CadmusApi.xml");
                options.IncludeXmlComments(xmlPath);
#endif
            });

            // serilog
            services.AddSingleton<ILogger>(_ =>
            {
                return new LoggerConfiguration()
                    .WriteTo.MSSqlServer(Configuration["Serilog:ConnectionString"],
                    Configuration["Serilog:TableName"], autoCreateSqlTable: true)
                    .CreateLogger();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// <summary>
        /// Configures the specified application.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="env">The environment.</param>
        /// <param name="databaseInitializer">The database initializer.</param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env,
            IDatabaseInitializer databaseInitializer)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            // CORS
            // https://docs.asp.net/en/latest/security/cors.html
            app.UseCors(builder =>
                builder.WithOrigins("http://localhost:4200")
                    .AllowAnyHeader()
                    .AllowAnyMethod());

            app.UseAuthentication();
            app.UseMvcWithDefaultRoute();
            //app.UseWelcomePage();

            // seed the database
            databaseInitializer.Seed().GetAwaiter().GetResult();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Cadmus API V1");
            });
        }
    }
}
