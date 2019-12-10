using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dapr.WebApiSample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InvokeController : ControllerBase
    {
        private readonly ILogger<InvokeController> _logger;
        private readonly HttpClient _httpClient;

        
        public InvokeController(ILogger<InvokeController> logger)
        {
            _logger = logger;
            this._httpClient = new HttpClient();
        }

        [HttpGet("sayhello")]
        public async Task<IActionResult> SayHello()
        {
            var response = await this._httpClient.PostAsync($"http://localhost:{Environment.GetEnvironmentVariable("DAPR_HTTP_PORT")}/v1.0/invoke/grpc/method/sayhello"
                , new StringContent(""));

            var responseContent = await response.Content.ReadAsStringAsync();

            return new OkObjectResult(responseContent);
        }

        [HttpGet("add/{operand1}/{operand2}")]
        public async Task<IActionResult> Add(decimal operand1, decimal operand2)
        {
            var response = await this._httpClient.PostAsync($"http://localhost:{Environment.GetEnvironmentVariable("DAPR_HTTP_PORT")}/v1.0/invoke/adder/method/add"
                , new StringContent(JsonSerializer.Serialize(Operands.Load(operand1, operand2))));

            var responseContent = await response.Content.ReadAsStringAsync();

            return new OkObjectResult(responseContent);
        }
    }

    public class Operands
    {
        private Operands(){}
        public decimal Operand1 { get; set; }

        public decimal Operand2 { get; set; }

        public static Operands Load(decimal operand1, decimal operand2)
        {
            return new Operands(){
                Operand1 = operand1,
                Operand2 = operand2
            };
        }
    }
}