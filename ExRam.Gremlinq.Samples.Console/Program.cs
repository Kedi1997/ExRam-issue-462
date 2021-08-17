// #define GremlinServer
#define CosmosDB
//#define AWSNeptune
//#define JanusGraph

using System;
using System.Threading.Tasks;
using ExRam.Gremlinq.Core;
using ExRam.Gremlinq.Providers.WebSocket;
using Microsoft.Extensions.Logging;

// Put this into static scope to access the default GremlinQuerySource as "g". 
using static ExRam.Gremlinq.Core.GremlinQuerySource;

namespace ExRam.Gremlinq.Samples
{
    public class Program
    {
        private static async Task Main()
        {
            var gremlinQuerySource = g
                .ConfigureEnvironment(env => env //We call ConfigureEnvironment twice so that the logger is set on the environment from now on.
                    .UseLogger(LoggerFactory
                        .Create(builder => builder
                            .AddFilter(__ => true)
                            .AddConsole())
                        .CreateLogger("Queries")))
                .ConfigureEnvironment(env => env
                    .UseModel(GraphModel
                        .FromBaseTypes<Vertex, Edge>(lookup => lookup
                            .IncludeAssembliesOfBaseTypes())
                        //For CosmosDB, we exclude the 'PartitionKey' property from being included in updates.
                        .ConfigureProperties(model => model
                            .ConfigureElement<Vertex>(conf => conf
                                .IgnoreOnUpdate(x => x.pk))))
                                
                    //Disable query logging for a noise free console output.
                    //Enable logging by setting the verbosity to anything but None.
                    .ConfigureOptions(options => options
                        .SetValue(WebSocketGremlinqOptions.QueryLogLogLevel, LogLevel.None))

#if GremlinServer
                    .UseGremlinServer(builder => builder
                        .AtLocalhost()));
#elif AWSNeptune
                    .UseNeptune(builder => builder
                        .AtLocalhost()));
#elif CosmosDB
                    .UseCosmosDb(builder => builder
                        .At(new Uri("wss://myhobbypaldb.gremlin.cosmos.azure.com:443/"), "sos", "schools")
                        .AuthenticateBy("G1zzR144TrvovZ2hkMW7JLHtM4B55qD3uUUntb7qzyOF1zh7FxxbhtsRmWOhVxyFCIu1kihsASBhVcgCVsoXog==")
                        .ConfigureWebSocket(_ => _
                            .ConfigureGremlinClient(client => client
                                .ObserveResultStatusAttributes((requestMessage, statusAttributes) =>
                                {
                                    //Uncomment to log request charges for CosmosDB.
                                    //if (statusAttributes.TryGetValue("x-ms-total-request-charge", out var requestCharge))
                                    //    env.Logger.LogInformation($"Query {requestMessage.RequestId} had a RU charge of {requestCharge}.");
                                })))));
#elif JanusGraph
                    .UseJanusGraph(builder => builder
                        .AtLocalhost()));
#endif

            // await new Logic(gremlinQuerySource, Console.Out)
            //     .Run();

            try{

                var temp = await  gremlinQuerySource.V<Person>("BF238CAA-B964-4B0C-BDE3-112C639054C1");
                
                
            }catch(Exception e){
                Console.WriteLine(e.StackTrace);
            }

            Console.Write("Press any key...");
            Console.Read();
        }
    }

    public class Vertex{
        public string? id { get; set; }
        public string? label { get; set; }
        public string pk { get; set; } 
        public long? legacyId { get; set; }
        public DateTime? deleteTime { get ; set; }
    }

    public class Person : Vertex
    {
        public string addedTime { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string emailAddress { get; set; }
        public string phoneNumber { get; set; }
        public string mobileNumber { get; set; }
        public string password { get; set; }
        public string countryCode { get; set; }
        public int? zoneId { get; set; }
        public string addressStreet1 { get; set; }
        public string addressStreet2 { get; set; }
        public string addressTown { get; set; }
        public string address1Postcode { get; set; }
        public string importUniqueId { get; set; }
        public string importRef { get; set; }
        public string userToken { get; set; }
        public string externalId { get; set; }
    }

     public class Edge
    {
        public object? Id { get; set; }
        public string? Label { get; set; }
    }
}
