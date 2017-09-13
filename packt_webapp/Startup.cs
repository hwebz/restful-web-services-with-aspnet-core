using System;
using System.IdentityModel.Tokens.Jwt;
using IdentityServer4.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NLog.Web;
using packt_webapp.Dtos;
using packt_webapp.Entities;
using packt_webapp.Middlewares;
using packt_webapp.Repositories;
using packt_webapp.Services;
using Swashbuckle.AspNetCore.Swagger;

namespace packt_webapp
{
    public class MyConfiguration
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
    }

    public class Startup
    {

        public IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            //var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            var builder = new ConfigurationBuilder().SetBasePath(env.ContentRootPath).AddJsonFile("appsettings.json").AddEnvironmentVariables();

            // nlog.config Properties -> Copy Always
            env.ConfigureNLog("nlog.config");

            Configuration = builder.Build();

            //Debug.WriteLine($" ---> From Config: {Configuration["firstname"]}");
            //Debug.WriteLine($" ---> From Config: {Configuration["withChild:option1"]}");
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<MyConfiguration>(Configuration);

            // Allow all CORS
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins", builder =>
                {
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });


            // Use configuration string from json file
            // services.AddDbContext<PacktDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // Docker DB setup instead of reading from JSON file
            var hostname = Environment.GetEnvironmentVariable("DATABASE_IP");
            var databasename = Environment.GetEnvironmentVariable("DATABASE_NAME");
            var databaseuser = Environment.GetEnvironmentVariable("DATABASE_USER");
            var databasepassword = Environment.GetEnvironmentVariable("DATABASE_PASSWORD");
            var connString = $"Server={hostname};Database={databasename};User ID={databaseuser};Password={databasepassword}";
            services.AddDbContext<PacktDbContext>(options => options.UseSqlServer(connString));

            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<ISeedDataService, SeedDataService>();

            // Open PMC
            // PM> Add-Migration FirstMigration
            // PM> Update-Database

            services.AddMvc(config =>
            {
                config.ReturnHttpNotAcceptable = true;
                config.OutputFormatters.Add(new XmlSerializerOutputFormatter()); // Allow xml output format
                config.InputFormatters.Add(new XmlSerializerInputFormatter());
            });

            // Swagger - API Documentation
            services.AddSwaggerGen(config =>
            {
                config.SwaggerDoc("v1", new Info {Title = "My first WebAPI", Version = "v1"});
            });

            // API Versioning
            //services.AddApiVersioning();
            services.AddApiVersioning(config =>
            {
                config.ReportApiVersions = true;
                config.AssumeDefaultVersionWhenUnspecified = true;
                config.DefaultApiVersion = new ApiVersion(1,0);
                config.ApiVersionReader = new HeaderApiVersionReader("api-version");
            });

            // IdentityServer - Authorization
            services.AddAuthorization(options =>
            {
                options.AddPolicy("resourcesAdmin", policyAdmin =>
                {
                    policyAdmin.RequireClaim("role", "resources.admin");
                });
                options.AddPolicy("resourcesUser", policyUser =>
                {
                    policyUser.RequireClaim("role", "resources.user");
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();
            //loggerFactory.AddConsole(Configuration.GetSection("Logging")); // Log to cmd console window in packt_webapp running mode
            //loggerFactory.AddDebug(LogLevel.Error); // Also log to debug windows in VS

            // add NLog to ASP.NET Core
            loggerFactory.AddNLog();

            // Add NLog.Web
            app.AddNLogWeb();


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async context =>
                    {
                        context.Response.StatusCode = 500;
                        context.Response.ContentType = "text/plain";
                        var errorFeature = context.Features.Get<IExceptionHandlerFeature>();
                        if (errorFeature != null)
                        {
                            var logger = loggerFactory.CreateLogger("Global exception logger");
                            logger.LogError(500, errorFeature.Error, errorFeature.Error.Message);
                        }

                        await context.Response.WriteAsync("There was an error");
                    });
                });
            }

            // IdentityServer
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            app.UseIdentityServerAuthentication(new IdentityServerAuthenticationOptions
            {
                Authority = "http://localhost:53448/",
                //AllowedScopes = {"resourcesScope"},
                RequireHttpsMetadata = false,
                ApiName = "resourcesScope"
            });

            if (env.IsEnvironment("MyEnvironment"))
            {
                app.UseCustomMiddleware();
            }

            
            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseSwagger();
            // localhost/swagger
            app.UseSwaggerUI(config =>
            {
                config.SwaggerEndpoint("/swagger/v1/swagger.json", "My first WebAPI");
            });

            AutoMapper.Mapper.Initialize(mapper =>
            {
                mapper.CreateMap<Customer, CustomerDto>().ReverseMap();
                mapper.CreateMap<Customer, CustomerCreateDto>().ReverseMap();
                mapper.CreateMap<Customer, CustomerUpdateDto>().ReverseMap();
            });

            app.AddSeedData();

            //app.UseMiddleware<CustomMiddleware>();
            //app.UseCustomMiddleware();

            app.UseMvc();
        }
    }
}
