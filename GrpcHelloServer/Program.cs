using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using FileCacheAgent.Common;
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
                    string pfxBlobPath = certLocationAndFileNamePrefix + "FCACert.pfx";

                    string pemEncodedCertWithPublicKeyOnly, pemEncodedPrivateKey;
                    X509Certificate2 fileCacheAgentCert;
                    X509CertToPemUtil.GetPemEncodedCertChainAndPrivateKey(
                        File.ReadAllText(pfxBlobPath),
                        out pemEncodedCertWithPublicKeyOnly,
                        out pemEncodedPrivateKey,
                        out fileCacheAgentCert);

                    Console.WriteLine(string.Format("FileCacheAgentCert: {0} Issuer: {1} Thumbprint: {2}",
                                                                                            fileCacheAgentCert.Subject,
                                                                                            fileCacheAgentCert.Issuer,
                                                                                            fileCacheAgentCert.Thumbprint));

                    string pemEncodedCertChainWithPublicKeyOnly = X509CertToPemUtil.GetPemEncodedCertChain(fileCacheAgentCert);

                    File.WriteAllText("CertChain.pem", pemEncodedCertChainWithPublicKeyOnly);

                    KeyCertificatePair keyCertificatePair = new KeyCertificatePair(pemEncodedCertChainWithPublicKeyOnly, pemEncodedPrivateKey);

                    serverCredentials = new SslServerCredentials(new[] { keyCertificatePair });
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
