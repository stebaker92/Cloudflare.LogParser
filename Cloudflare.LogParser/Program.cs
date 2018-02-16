using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cloudflare.LogParser
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Loading Cloudflare logs");

            // Configs
            string zoneId = null;
            string authKey = null;
            string authEmail = null;

            // Options
            string filterHost = null;
            string filterRequest = null;
            int longRequestSeconds = 5;

            // TODO add a config file

            var client = new RestClient($"https://api.cloudflare.com/client/v4/zones/{zoneId}/logs/received");

            DateTime start = new DateTime();
            DateTime end = new DateTime();

            // TODO - get this from args / console input
            start = DateTime.Today.AddTicks(new TimeSpan(10, 17, 00).Ticks);
            end = start.AddMinutes(5);
            //end = DateTime.Today.AddTicks(new TimeSpan(10, 37, 00).Ticks);

            var request = new RestRequest();

            request.AddHeader("X-Auth-Key", authKey);
            request.AddHeader("X-Auth-Email", authEmail);

            request.AddQueryParameter("start", start.ToString("O"));
            request.AddQueryParameter("end", end.ToString("O"));
            request.AddQueryParameter("fields", string.Join(',', LogFields.All));

            var response = client.Execute(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception(response.Content);
            }

            // Convert the response to a json object
            var logsJson = $"[{response.Content}]".Replace("}", "},").Replace("},]", "}]");

            var logs = JsonConvert.DeserializeObject<List<CloudflareLog>>(logsJson);
            Console.WriteLine($"Found {logs.Count} logs");

            logs = logs
                .Where(x => filterHost != null && x.ClientRequestHost.Contains(filterHost))
                .Where(x => filterRequest != null && x.ClientRequestURI.Contains(filterRequest))
                .ToList();

            Console.WriteLine($"Found {logs.Count} filtered logs");

            if (!logs.Any())
            {
                return;
            }

            var avgMilliseconds = logs.Average(x => x.Milliseconds);
            var minMilliseconds = logs.Min(x => x.Milliseconds);
            var maxMilliseconds = logs.OrderByDescending(x => x.Milliseconds).First();
            var aboveSecondsCount = logs.Count(x => x.Milliseconds > longRequestSeconds * 1000);

            Console.WriteLine("Avg: " + avgMilliseconds);
            Console.WriteLine();

            Console.WriteLine("Min: " + minMilliseconds);
            Console.WriteLine();

            Console.WriteLine("Max: " + maxMilliseconds.Milliseconds);
            Console.WriteLine("RayID: " + maxMilliseconds.RayID);
            Console.WriteLine("EndTime:" + maxMilliseconds.EndTime);
            Console.WriteLine();

            Console.WriteLine($"Long (>{longRequestSeconds}s): " + aboveSecondsCount);

            Console.ReadLine();
        }
    }
}
