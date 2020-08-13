using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Akka.DI.Core;
using Akka.Persistence.Sqlite;
using AkkanetFsmDemo.Extensions;
using AkkanetFsmDemo.Models.Actors;
using AkkanetFsmDemo.Models.Options;
using AkkanetFsmDemo.Models.Services.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MQTTnet.AspNetCore;
using MQTTnet.AspNetCore.Extensions;
using MQTTnet.Server;

namespace AkkanetFsmDemo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;   
        }
        public IConfiguration Configuration { get; set; }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IActorSystemAccessor, ActorSystemAccessor>();
            services.AddSingleton<ICommandSender, ActorSystemCommandSender>();
            services.AddSingleton<IMqttServerApplicationMessageInterceptor, ApplicationMessageInterceptor>();
            services.AddSingleton<IActorSystemDependencyResolver, ActorSystemDependencyResolver>();
            services.AddSingleton<INotificationSender, MqttDotNetNotificationSender>();
            services.AddActorsFromAssembly(System.Reflection.Assembly.GetExecutingAssembly());
            services.Configure<ActorSystemOptions>(Configuration.GetSection("ActorSystem"));
            services.Configure<MqttOptions>(Configuration.GetSection("MqttClient"));
            var serviceProvider = services.BuildServiceProvider();

            //https://github.com/chkr1011/MQTTnet/wiki/Server
            services
                .AddHostedMqttServer(mqttServer => {
                    mqttServer
                    .WithApplicationMessageInterceptor(serviceProvider.GetService<IMqttServerApplicationMessageInterceptor>())
                    .WithoutDefaultEndpoint(); //TODO: Use an encrypted endpoint instead?
                })
                .AddMqttConnectionHandler()
                .AddConnections();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapMqtt("/mqtt");
            });

            app.UseMqttServer(server =>
            {
                //TODO: Do something with the server
            });
        }
    }
}
