using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Test
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
#if DEBUG
      // useful for logging, especially JWT issues
      IdentityModelEventSource.ShowPII = true;
#endif
      services.AddOptions();
      //standard settings
      services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

      //load settings locally
      var settings = Configuration.GetSection("AppSettings").Get<AppSettings>();

      services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
      services.AddSingleton<IRandomNameService, RandomNameService>();
      services.AddSingleton<IPasswordService, PasswordService>();
      services.AddSingleton<ITokenService, TokenService>();

      services.AddAuthentication(options =>
      {
        // Identity made Cookie authentication the default.
        // However, we want JWT Bearer Auth to be the default.
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
      })
      //default
      .AddJwtBearer(options =>
      {
        #region Scheme 'callbacks' Options

        options.RequireHttpsMetadata = false;
        options.IncludeErrorDetails = true;
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
              LifetimeValidator = (before, expires, token, param) =>
              {
                return expires > DateTime.UtcNow;
              },
              ValidateAudience = false,
              ValidateIssuer = false,
              ValidateActor = false,
              ValidateLifetime = true,
              IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(settings.JWTSecretKeys["Site"]))
            };

        options.Events = new JwtBearerEvents
        {
          OnTokenValidated = context =>
          {
            return Task.CompletedTask;
          },
          OnAuthenticationFailed = context =>
          {
            return Task.CompletedTask;
          },
          OnChallenge = context =>
          {
            return Task.CompletedTask;
          }
          //OnMessageReceived = context =>
          //{
          //  var path = context.HttpContext.Request.Path;
          //  if (!path.StartsWithSegments("/api/signin"))
          //  {
          //    var creds = AuthenticationHeaderValue.Parse(context.HttpContext.Request.Headers[HeaderNames.Authorization]);

          //    JwtSecurityTokenHandler hand = new JwtSecurityTokenHandler();
          //    var x = hand.ReadJwtToken(creds.Parameter);
          //    context.Token = x.ToString();
          //    if (creds != null) context.Token = creds.Parameter;
          //  }
          //  return Task.CompletedTask;
          //}
        };

        #endregion
      });


      services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

      //// In production, the Angular files will be served from this directory
      //services.AddSpaStaticFiles(configuration =>
      //{
      //	configuration.RootPath = "ClientApp/dist";
      //});

      services.AddDbContextPool<MyDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("TestDB")));

      services.AddCors(options => options.AddPolicy("CorsPolicy",
          builder =>
          {
            builder
            .WithOrigins( "http://localhost:4200" )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
          })
      );

    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(
      IApplicationBuilder app,
      IHostingEnvironment env,
      IRandomNameService names,
      IPasswordService pwd
    )
		{
      if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Error");
      }

      app
			  .UseAuthentication()
        .UseCors("CorsPolicy")
        .UseDefaultFiles(new DefaultFilesOptions { DefaultFileNames = new System.Collections.Generic.List<string> { "index.html" } })
        .UseStaticFiles();

      //app.UseSpaStaticFiles();

			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller}/{action=Index}/{id?}");
			});

      // I don't use the SPA stuff from MSFT because I want granular control of the debugging, building, and checkin..

      //app.UseSpa(spa =>
      //{
      //	// To learn more about options for serving an Angular SPA from ASP.NET Core,
      //	// see https://go.microsoft.com/fwlink/?linkid=864501

      //	spa.Options.SourcePath = "ClientApp";

      //	if (env.IsDevelopment())
      //	{
      //		spa.UseAngularCliServer(npmScript: "start");
      //	}
      //});

      // if this was more expensive I'd run the follow blocks concurrently.
      names.Load();

      var optionsBuilder = new DbContextOptionsBuilder<MyDbContext>();
      var db = new MyDbContext(optionsBuilder.UseSqlServer(Configuration.GetConnectionString("TestDB")).Options);
      db.Database.EnsureCreated();
      db.Initialize(pwd).GetAwaiter().GetResult();
		}
	}
}
