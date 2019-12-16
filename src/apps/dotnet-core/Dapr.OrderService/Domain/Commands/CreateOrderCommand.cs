using System.Threading;
using System.Threading.Tasks;
using Dapr.OrderService.Domain.Models;
using MediatR;
using Dapr;
using System.Text.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System;
using System.Text;
using System.IO;

namespace Dapr.OrderService.Domain.Commands
{
    public class CreateOrderCommand : IRequest<Order>
    {
        public Order Order { get; set; }
    }

    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Order>
    {
        private readonly ILogger<CreateOrderCommandHandler> _logger;
        private readonly HttpClient _httpClient;
        public CreateOrderCommandHandler(ILogger<CreateOrderCommandHandler> logger
            , IHttpClientFactory clientFactory)
        {
            this._logger = logger;
            this._httpClient = clientFactory.CreateClient();
        }
        public async Task<Order> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var validationResponses = new List<bool>(request.Order.OrderLines.Count());

            foreach (var orderLine in request.Order.OrderLines)
            {
                this._logger.LogInformation($"Validating line {orderLine.ProductCode}");

                var invokeResponse = await this._httpClient.PostAsync($"http://localhost:{Environment.GetEnvironmentVariable("DAPR_HTTP_PORT")}/v1.0/invoke/product-service/method/validate", new StringContent(JsonSerializer.Serialize(new ValidateProductRequest() { ProductCode = orderLine.ProductCode }), Encoding.UTF8, "application/json"));

                using (var responseStream = await invokeResponse.Content.ReadAsStreamAsync())
                {
                    var validationResponse = await JsonSerializer.DeserializeAsync<ValidateProductResponse>(responseStream);

                    this._logger.LogInformation($"Valid: {validationResponse.Result}");

                    validationResponses.Add(validationResponse.Result);
                }
            }


            if (validationResponses.Count(p => p == true) == request.Order.OrderLines.Count())
            {
                this._logger.LogInformation("Comitting order");

                var storedObject = new object[] { new { key = request.Order.OrderNumber, request.Order, } };

                await this._httpClient.PostAsync($"http://localhost:{Environment.GetEnvironmentVariable("DAPR_HTTP_PORT")}/v1.0/state", new StringContent(JsonSerializer.Serialize(storedObject)));

                await this._httpClient.PostAsync($"http://localhost:{Environment.GetEnvironmentVariable("DAPR_HTTP_PORT")}/v1.0/publish/neworder", new StringContent(JsonSerializer.Serialize(request.Order)));

                return request.Order;
            }
            else
            {
                this._logger.LogWarning("Order is invalid");

                return null;
            }
        }

        public class ValidateProductRequest
        {
            public string ProductCode { get; set; }
        }

        public class ValidateProductResponse
        {
            public bool Result { get; set; }
        }
    }
}