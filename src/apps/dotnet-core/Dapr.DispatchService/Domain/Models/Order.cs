using System;
using System.Collections.Generic;

namespace Dapr.DispatchService.Domain.Models
{
    public class Order
    {
        public string OrderNumber { get; set; }

        public DateTime OrderDate { get; set; }

        public IEnumerable<OrderLine> OrderLines { get; set; }
    }
}