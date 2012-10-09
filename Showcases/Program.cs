using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using Raven.Client.Document;

namespace Showcases
{
    class Program
    {
        const string remoteRavenFromGit = "http://217.116.214.21:8080";

        static void Main(string[] args)
        {
            ServicePointManager.UseNagleAlgorithm = false;
            ServicePointManager.Expect100Continue = false;

            // The original problem that is easily fixed by turning off nagle on the client
            ShowPlus200msPUTs();

            // Using a home brewed http get (see below) - this is fast but with occasional spikes.
            // Note: Compression is turned off here by default, becasue it seems to be off on the DatabaseCommands.Get too.
            //ShowOccasionalSlowGETs();

            // Using the DatabaseCommands.Get - this is way slower and it spikes more often.
            // Note: The spikes can be seen in the server log, the general slowness is on the client
            // Note: This seems to not use compression.
            //ShowOccasionalSlowGETsUsingRavenClient();

            // Note: That the spikes are so eventual that they might not always be present in a run,
            // but sometimes it each and every second request that spikes. Please run multiple times.
        }

        private static void ShowPlus200msPUTs()
        {
            Console.WriteLine("PUT small documents");
            var smalldoc = File.ReadAllText("smalldoc.json");
            for (int i = 0; i < 10; i++)
            {
                Put(remoteRavenFromGit, smalldoc);
            }
        }

        private static void ShowOccasionalSlowGETs()
        {
            Console.WriteLine("GET larger documents using own GET method");
            var doc = File.ReadAllText("largedoc.json");
            Put(remoteRavenFromGit, doc);
            for (int i = 0; i < 10; i++)
            {
                Get(remoteRavenFromGit, acceptCompression: false);
            }
        }

        private static void ShowOccasionalSlowGETsUsingRavenClient()
        {
            var store = new DocumentStore
                                    {
                                        Url = remoteRavenFromGit,
                                        MaxNumberOfCachedRequests = 0
                                    };
            store.Initialize();

            Console.WriteLine("GET larger documents using raven client's method");
            var doc = File.ReadAllText("largedoc.json");
            Put(remoteRavenFromGit, doc);
            for (int i = 0; i < 100; i++)
            {
                var stopwatch = Stopwatch.StartNew();
                store.DatabaseCommands.Get("thedoc");
                Console.WriteLine("GET: " + stopwatch.ElapsedMilliseconds);
            }

            store.Dispose();
        }

        static long Get(HttpWebRequest request, bool acceptCompression)
        {
            var stopwatch = Stopwatch.StartNew();

            request.Method = "GET";
            request.ContentType = "application/json; charset=utf-8";
            request.Headers.Add("Raven-Client-Version", "1.2.0.0");

            if (acceptCompression) request.Headers.Add("Accept-Encoding", "gzip");

            var response = request.GetResponse();
            using (var responseStream = response.GetResponseStream())
            using(var stream = (acceptCompression) ? new GZipStream(responseStream, CompressionMode.Decompress) : responseStream)
            using (var s = new StreamReader(stream))
                s.ReadToEnd();

            response.Close();

            Console.WriteLine("GET: " + stopwatch.ElapsedMilliseconds);
            return stopwatch.ElapsedMilliseconds;
        }

        static long Get(string host, bool acceptCompression, string key = "thedoc")
        {
            var request = (HttpWebRequest)WebRequest.Create(string.Format("{0}/docs/{1}", host, key));
            request.Credentials = CredentialCache.DefaultNetworkCredentials;
            return Get(request, acceptCompression);
        }

        static long Put(HttpWebRequest request, string json)
        {
            var stopwatch = Stopwatch.StartNew();

            request.Method = "PUT";
            request.SendChunked = true;
            request.Headers.Add("Content-Encoding", "gzip");

            using (var requestStream = new BufferedStream(request.GetRequestStream(), 16000))
            using (var dataStream = new GZipStream(requestStream, CompressionMode.Compress))
            using (var writer = new StreamWriter(dataStream, Encoding.UTF8))
            {
                writer.Write(json);
                writer.Flush();
            }

            request.GetResponse().Close();

            Console.WriteLine("PUT: " + stopwatch.ElapsedMilliseconds);
            return stopwatch.ElapsedMilliseconds;
        }

        static long Put(string host, string json, string key = "thedoc")
        {
            var request = (HttpWebRequest)WebRequest.Create(string.Format("{0}/docs/{1}", host, key));
            request.Credentials = CredentialCache.DefaultNetworkCredentials;
            return Put(request, json);
        }
    }
}
