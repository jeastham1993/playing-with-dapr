using System.Threading.Tasks;
using Dapr.OrderService.Domain.Commands;
using Dapr.OrderService.Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dapr.OrderService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly ILogger<OrderController> _logger;
        private readonly IMediator _mediator;

        public OrderController(ILogger<OrderController> logger
            ,IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }
        
        [HttpPost]
        public async Task<Order> CreateOrderAsync([FromBody] CreateOrderCommand createOrderCommand)
        {
            return await this._mediator.Send(createOrderCommand);
        }
    }
}
