namespace Dapr.DispatchService.Domain.Models

{
    public class DaprContentWrapper<T> 
    {
        public T Data { get; set; }
    }
}