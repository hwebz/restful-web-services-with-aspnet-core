using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
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

            // Use configuration string from json file
            services.AddDbContext<PacktDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

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
