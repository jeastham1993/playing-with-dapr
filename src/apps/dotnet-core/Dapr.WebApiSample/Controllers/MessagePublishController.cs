using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dapr.WebApiSample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MessagePublishController : ControllerBase
    {
        private readonly ILogger<MessagePublishController> _logger;
        private readonly HttpClient _httpClient;

        
        public MessagePublishController(ILogger<MessagePublishController> logger)
        {
            _logger = logger;
            this._httpClient = new HttpClient();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            await this._httpClient.PostAsync($"http://localhost:{Environment.GetEnvironmentVariable("DAPR_HTTP_PORT")}/v1.0/publish/topic1", new StringContent("{ \"status\": \"completed\", \"id\": \"1\"}"));

            return new OkResult();
        }
    }
}
