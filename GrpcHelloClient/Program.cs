using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using Grpc.Core;
using GrpcMessage;

namespace GrpcHelloClient
{
    class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                ClientDriver1(args);
                // ClientDriver2(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception encountered: \r\n{0}", ex.ToString());
            }
        }

        private static void ClientDriver1(string[] args)
        {
            if (args.Length < 2)
            {
                System.Console.WriteLine("Usage: <EXE> <host:port> [<serverCertLocationAndFileNamePrefix>]");
                return;
            }

            string endpoint = args[0];  

            string serverCertLocationAndFileNamePrefix = null;
            if (args.Length > 1)
            {
                serverCertLocationAndFileNamePrefix = args[1];
            }

            Channel channel;
            Console.WriteLine("Endpoint = {0}", endpoint);
            GrpcMessage.GrpcHello.GrpcHelloClient client = CreateClient(endpoint, serverCertLocationAndFileNamePrefix, out channel);

            GetHelloResponse(client);
        }
        
        private static void GetHelloResponse(GrpcMessage.GrpcHello.GrpcHelloClient client)
        {
            HelloRequest helloRequest = new HelloRequest { Name = "Thai" };
            HelloResponse helloResponse = client.HelloService(helloRequest);

            Console.WriteLine("Response from Server: {0}", helloResponse.Response);

        }


        private static GrpcMessage.GrpcHello.GrpcHelloClient CreateClient(string endpoint, string serverCertLocationAndFileNamePrefix, out Channel channel)
        {
            if (serverCertLocationAndFileNamePrefix == null)
            {
                channel = new Channel(endpoint, ChannelCredentials.Insecure);
            }
            else
            {
                //string certificate = serverCertLocationAndFileNamePrefix + "GrpcCert.crt.pem";

                //// NOTE: This should match the target name(Subject Alternate Name) in the server cert. 
                //// If there is no target name, then this is the same as Subject Common Name in the cert
                //string certSubjectName = "MyGRPCService2.com";
                //var channelCredentials = new SslCredentials(File.ReadAllText(certificate), null, ValidateGrpcServerCertificate);

                string certSubjectName = @"FileCacheAgent.appservice-test.compute.ce.azure.net";

                SslCredentials sslCredentials = new SslCredentials(
                    File.ReadAllText(@"d:\cert\fca\AllowedKeys.pem"),
                    null,
                    verifyPeerContext => ValidateFileCacheAgentServerCert(verifyPeerContext));

                channel = new Channel(endpoint, sslCredentials, new[] { new ChannelOption(ChannelOptions.SslTargetNameOverride, certSubjectName) });
            }

            return new GrpcMessage.GrpcHello.GrpcHelloClient(channel);
        }

        private static bool ValidateFileCacheAgentServerCert(VerifyPeerContext verifyPeerContext)
        {
            // TODO
            return true;
        }

        private static bool ValidateGrpcServerCertificate(VerifyPeerContext context)
        {
            Console.WriteLine("Target Name = " + context.TargetName);

            Console.WriteLine("PeerPem  = " + context.PeerPem);

            string serverCertPem = context.PeerPem;
            StringReader reader = new StringReader(serverCertPem);

            List<string> linesInPem = new List<string>();
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                linesInPem.Add(line);
            }

            // Lame, Wrong in the general case.
            if (linesInPem.First().StartsWith("-----BEGIN CERTIFICATE-----"))
            {
                linesInPem.RemoveAt(0);
            }

            if (linesInPem.Last().StartsWith("-----END CERTIFICATE-----"))
            {
                linesInPem.RemoveAt(linesInPem.Count - 1);
            }

            string pemNoLabels = string.Join("", linesInPem);

            byte[] certBytes = Convert.FromBase64String(pemNoLabels);

            X509Certificate2 cert = new X509Certificate2(certBytes);

            Console.WriteLine("Subject Name = " + cert.SubjectName.Name);

            Console.WriteLine("Thumbprint  = " + cert.Thumbprint);

            return true;
        }
    }
}
