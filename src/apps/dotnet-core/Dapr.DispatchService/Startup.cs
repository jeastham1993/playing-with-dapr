using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Dapr.DispatchService.Domain.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace Dapr.DispatchService
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
            services.AddDaprClient();

            services.AddSingleton(new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/dapr/subscribe", async context =>
                {
                    var subscribedTopics = new List<string>() { "neworder" };
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(subscribedTopics));
                });

                endpoints.MapPost("/neworder", async context =>
                {
                    System.Console.WriteLine("Handling new order");

                    using (var streamReader = new StreamReader(context.Request.Body))
                    {
                        var json = await streamReader.ReadToEndAsync();

                        System.Console.WriteLine($"Order content {json}");

                        if (string.IsNullOrEmpty(json) == false){
                            var order = JsonConvert.DeserializeObject<DaprContentWrapper<Order>>(json);

                            File.WriteAllText($"C:\\Demonstration\\{order.Data.OrderNumber}.json", JsonConvert.SerializeObject(order, Formatting.Indented));
                        }
                    }
                });
            });
        }
    }
}
