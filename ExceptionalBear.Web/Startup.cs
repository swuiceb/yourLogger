using ExceptionalBear.Web.Filters;
using yourLogs.Exceptions.Core.LogReaders;
using yourLogs.Exceptions.Core.LogWriters;
using yourLogs.Exceptions.Core.LogWriters.Providers;
using yourLogs.Exceptions.Core.Models;
using yourLogs.Exceptions.Db.Ef;
using yourLogs.Exceptions.Db.Ef.LogWriters;
using yourLogs.Exceptions.Db.Ef.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class Startup
{
    private IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddMvc(
            options =>
            {
                options.EnableEndpointRouting = false;
                options.Filters.Add(new ExceptionFilter());
            });
        
        InitializeLogger(services);
    }

    private void InitializeLogger(IServiceCollection services)
    {
        var dbOptions = new DbContextOptionsBuilder()
            .UseSqlServer(
                Configuration.GetConnectionString("ErrorDatabase"))
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging()
            .Options;

        var loggerBuilder = new LoggerBuilder()
            .WithConsoleLevel(LogType.Trace)
            .WithExceptionTextProvider(ExceptionTextProviders.Default);

        var logger = loggerBuilder.WithWriter(loggerBuilder.BuildInDb(
                new EfCoreRepository(LogContextProvider).VerifyAsync().Result))
            .Build(LogType.Warning);
        
        services.AddSingleton<ILogWriter>(logger.Writer);
        services.AddSingleton<ILogReader>(logger.Reader);

        ErrorDbContext LogContextProvider()
        {
            return new ErrorDbContext(dbOptions);
        }
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            //app.UseDeveloperExceptionPage();
        }

        app.UseExceptionHandler(
            new ExceptionHandlerOptions()
            {
                AllowStatusCode404Response = true,
                ExceptionHandlingPath = "/Error"
            });

        app.UseMvc();
        app.UseStatusCodePages();
        app.UseRouting();
        app.UseStaticFiles();

        /*app.UseEndpoints(endpoints =>
        {
            endpoints.MapGet("/", async context => { await context.Response.WriteAsync("Hello World!"); });
            endpoints.MapControllers();
        });*/
    }
}