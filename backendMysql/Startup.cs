global using backendMysql.Models;
global using backendMysql.Data;
global using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using backendTest.Infrastructure.Extensions;
using backendTest.Infrastructure.Models;
using backendTest.Infrastructure.Utils;
using backendTest.Infrastructure.Middleware;
using backendTest.Infrastructure.Services;

namespace backendMysql
{ 
    
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();

            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    IConfigurationSection googleAuthSection = Configuration.GetSection("Authentication:Google");

                    options.ClientId = googleAuthSection["ClientId"];
                    options.ClientSecret = googleAuthSection["ClientSecret"];
                });
                //.AddFacebook(facebookOptions =>
               // {
                //    facebookOptions.AppId = Configuration["Authentication:Facebook:AppId"];
                //    facebookOptions.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
               // })
                //.AddMicrosoftAccount(microsoftOptions =>
                //{
                //    microsoftOptions.ClientId = Configuration["Authentication:Microsoft:ClientId"];
                //    microsoftOptions.ClientSecret = Configuration["Authentication:Microsoft:ClientSecret"];
                //});

            services.AddControllers();

            // configure strongly typed settings object
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

            services.AddSwaggerDocumentation(GetAppSettings());

            // Firebase Admin Secret Key you can find at link (Replace [YOUR_PROJECT_ID] by your project ID)
            // https://console.firebase.google.com/u/0/project/[YOUR_PROJECT_ID]/settings/serviceaccounts/adminsdk
            services.AddFirebaseAdminWithCredentialFromFile("firebase-adminsdk-secret-key.json");
            services.AddDbContext<DataContext>(option =>
            {
                var connectionString = "server=localhost;user=root;password=;database=asp";
                var version = new MySqlServerVersion(new Version(10, 4, 28));
                option.UseMySql(connectionString, version);
            });
            services.AddHttpClient();

            // configure DI for application services
            services.AddScoped<IFirebaseAdminUtils, FirebaseAdminUtils>();
            services.AddScoped<IFirebaseService, FirebaseService>();
        }

        private AppSettings GetAppSettings()
        {
            var appSettings = new AppSettings();

            Configuration.Bind(nameof(AppSettings), appSettings);

            return appSettings;
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseSwaggerDocumentation();
          
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            // global cors policy
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            // global error handler
            app.UseMiddleware<ErrorHandlerMiddleware>();

            // custom auth middleware
            app.UseMiddleware<AuthorizationMiddleware>();

            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
