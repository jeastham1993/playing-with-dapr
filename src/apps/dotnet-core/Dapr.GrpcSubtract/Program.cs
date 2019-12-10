using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Dapr.Client.Grpc;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using static Dapr.Client.Grpc.Dapr;

namespace Dapr.GrpcSubtract
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Get default port from environment, the environment is set when launched by Dapr runtime.
            var defaultPort = Environment.GetEnvironmentVariable("DAPR_GRPC_PORT") ?? "52918";

            // Set correct switch to make insecure gRPC service calls. This switch must be set before creating the GrpcChannel.
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            // Create Client
            var daprUri = $"http://127.0.0.1:{defaultPort}";
            var channel = GrpcChannel.ForAddress(daprUri);
            var client = new DaprClient(channel);
            
            var serviceEnvelope = new InvokeServiceEnvelope();
            var data = new Any();
            data.Value = ByteString.CopyFromUtf8(JsonSerializer.Serialize(Operands.Load(9, 1)));
            
            serviceEnvelope.Data = data;
            serviceEnvelope.Id = "adder";
            serviceEnvelope.Method = "add";

            var response = client.InvokeService(serviceEnvelope);

            System.Console.WriteLine(response.Data.Value.ToStringUtf8());
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
