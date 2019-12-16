using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MediatR;
using System.IO;
using Newtonsoft.Json;

namespace Dapr.ProductService
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
            services.AddMediatR(typeof(Startup).Assembly);
            services.AddControllers().AddDapr();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapPost("/validate", ValidateProduct);
            });

            async Task ValidateProduct(HttpContext context)
            {
                try
                {
                    using (StreamReader reader = new StreamReader(context.Request.Body))
                    {
                        var validateProductRequest = JsonConvert.DeserializeObject<ValidateProductRequest>(await reader.ReadToEndAsync());

                        System.Console.WriteLine($"Validating {validateProductRequest.ProductCode}");

                        if (ValidProducts.Contains(validateProductRequest.ProductCode))
                        {
                            System.Console.WriteLine("VALID");
                            await context.Response.WriteAsync(JsonConvert.SerializeObject(new ValidateProductResponse(){Result = true}));
                        }
                        else
                        {
                            System.Console.WriteLine("INVALID");
                            await context.Response.WriteAsync(JsonConvert.SerializeObject(new ValidateProductResponse(){Result = false}));
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex.Message);
                }

            }
        }

        public class ValidateProductResponse
        {
            public bool Result { get; set; }
        }

        public class ValidateProductRequest
        {
            public string ProductCode { get; set; }
        }

        public static List<string> ValidProducts
        {
            get
            {
                return new List<string>(5){
            "WIDGET",
            "BOLT",
            "SCREW",
            "HAMMER",
            "TOOL"
        };
            }
        }
    }
}
