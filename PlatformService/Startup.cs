using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using PlatformService.AsyncDataServices.Http;
using PlatformService.Data;
using PlatformService.SyncDataServices.Http;

namespace PlatformService
{
    public class Startup
    {

        private readonly IWebHostEnvironment _env;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            this._env = env;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            if (this._env.IsProduction())
            {
                Console.WriteLine("--> Using SQL Server");

                services.AddDbContext<AppDbContext>(
                    opts => opts.UseSqlServer(Configuration.GetConnectionString("PlatformSQLServer"))
                );
            }
            else
            {
                Console.WriteLine("--> Using In Memory Database");

                services.AddDbContext<AppDbContext>(
                    opts => opts.UseInMemoryDatabase("InMem")
                );
            }


            services.AddScoped<IPlatformRepository, PlatformRepository>();
            services.AddSingleton<IMessageBusClient, MessageBusClient>();

            services.AddHttpClient<ICommandDataClient, CommandDataClient>();

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "PlatformService", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PlatformService v1"));
            }

            // app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            SeedDb.PrepPopulation(app, env.IsProduction());
            Console.WriteLine($"--> Commands Service Endpoint: {Configuration["CommandsService"]}");
        }
    }
}
