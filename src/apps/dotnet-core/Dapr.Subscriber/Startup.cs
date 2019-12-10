using System.Text.Json;
using System.Threading.Tasks;
using Dapr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Dapr.Subscriber
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
                    var subscribedTopics = new List<string>() { "topic1" };
                    await context.Response.WriteAsync(JsonSerializer.Serialize(subscribedTopics));
                });

                endpoints.MapPost("/topic1", async context =>
                {
                    using (var streamReader = new StreamReader(context.Request.Body))
                    {
                        var json = await streamReader.ReadToEndAsync();

                        System.Console.WriteLine(json);
                    }
                });

                endpoints.MapPost("/add", InvokedMethod);
            });

            async Task InvokedMethod(HttpContext context)
            {
                var operands = await JsonSerializer.DeserializeAsync<Operands>(context.Request.Body);

                await context.Response.WriteAsync("{ \"result\": " + operands.Sum + "}");
            }
        }
    }

    public class Operands
    {
        private Operands(){}
        public decimal Operand1 { get; set; }

        public decimal Operand2 { get; set; }

        public decimal Sum
        {
            get
            {
                return Operand1 + Operand2;
            }
        }
    }
}
