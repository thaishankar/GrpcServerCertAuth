using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;

using GrpcMessage;

namespace GrpcHelloServer
{
    class HelloServiceImpl : GrpcMessage.GrpcHello.GrpcHelloBase
    {
        public override Task<HelloResponse> HelloService(HelloRequest request, ServerCallContext context)
        {
            Console.WriteLine("Hello Request recieved from : {0}", request.Name);

            string response = "Hellooooo... " + request.Name;

            HelloResponse helloResponse = new HelloResponse { Response = response };

            return Task.FromResult(helloResponse);
        }
    }

    class Program
    {
        const int Port = 50051;

        public static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("USAGE: <exe> [<certLocationAndFileNamePrefix>]");
                int port = Port;
                string certLocationAndFileNamePrefix = "";
                string key = "";
                string cert = "";

                ServerCredentials serverCredentials = ServerCredentials.Insecure;

                if (args.Length >= 1)
                {
                    certLocationAndFileNamePrefix = args[0];

                    key = certLocationAndFileNamePrefix + "GrpcCert2.key.pem";
                    cert = certLocationAndFileNamePrefix + "GrpcCert2.crt.pem";

                    var keyCertPair = new KeyCertificatePair(
                        File.ReadAllText(cert),
                        File.ReadAllText(key));

                    serverCredentials = new SslServerCredentials(new[] { keyCertPair });
                }

                Console.WriteLine("Key = {0} Cert: {1}", key, cert);
                string ipAddress = "0.0.0.0";
                Server server = new Server
                {
                    Services = { GrpcMessage.GrpcHello.BindService(new HelloServiceImpl()) },
                    Ports = { new ServerPort(ipAddress, port, serverCredentials) }
                };
                server.Start();

                Console.WriteLine("Server listening on {0}:{1}", ipAddress, port);
                Console.WriteLine("Press any key to stop the server...");
                Console.ReadKey();

                server.ShutdownAsync().Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught: {0}", ex.ToString());
            }
        }
    }
}
