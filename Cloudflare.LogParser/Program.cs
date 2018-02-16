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

            // Filters
            string filterHost = null;
            string filterRequest = null;

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
            Console.WriteLine($"Found {logs.Count} rows");

            logs = logs
                .Where(x => filterHost != null && x.ClientRequestHost.Contains(filterHost))
                .Where(x => filterRequest != null && x.ClientRequestURI.Contains(filterRequest))
                .ToList();

            Console.WriteLine($"Found {logs.Count} filtered rows");

            if (!logs.Any())
            {
                return;
            }

            var avgMilliseconds = logs.Average(x => x.Milliseconds);
            var minMilliseconds = logs.Min(x => x.Milliseconds);
            var maxMilliseconds = logs.OrderByDescending(x => x.Milliseconds).First();
            var aboveSeconds = logs.Count(x => x.Milliseconds > 5000);

            Console.WriteLine("Avg: " + avgMilliseconds);
            Console.WriteLine("Min: " + minMilliseconds);
            Console.WriteLine("Max: " + maxMilliseconds.Milliseconds + " RayID: " + maxMilliseconds.RayID + " " + maxMilliseconds.EndTime);
            Console.WriteLine("Long (> 5s): " + aboveSeconds);

            Console.ReadLine();
        }

    }
}
