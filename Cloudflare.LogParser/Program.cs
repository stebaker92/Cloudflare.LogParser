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

            DateTime start = new DateTime();
            DateTime end = new DateTime();

            // TODO - get this from args / console input
            start = DateTime.Today.AddTicks(new TimeSpan(10, 17, 00).Ticks);
            end = start.AddMinutes(5);
            //end = DateTime.Today.AddTicks(new TimeSpan(10, 37, 00).Ticks);

            Console.WriteLine("Start Time is: " + start.ToString());
            Console.WriteLine("End Time is: " + end.ToString());

            var client = new RestClient($"https://api.cloudflare.com/client/v4/zones/{zoneId}/logs/received");

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

            // Convert the response to a valid json object
            var logsJson = $"[{response.Content}]".Replace("}", "},").Replace("},]", "}]");

            var logs = JsonConvert.DeserializeObject<List<CloudflareLog>>(logsJson);
            Console.WriteLine($"Found {logs.Count} logs before filtering");

            logs = logs
                .Where(x => filterHost != null && x.ClientRequestHost.Contains(filterHost))
                .Where(x => filterRequest != null && x.ClientRequestURI.Contains(filterRequest))
                .ToList();

            Console.WriteLine($"Found {logs.Count} logs after filtering host '{filterHost}' and request {filterRequest}");

            if (!logs.Any())
            {
                return;
            }

            var avgMilliseconds = logs.Average(x => x.Milliseconds);
            var minMilliseconds = logs.Min(x => x.Milliseconds);
            var maxMilliseconds = logs.OrderByDescending(x => x.Milliseconds).First();
            var aboveSecondsCount = logs.Count(x => x.Milliseconds > longRequestSeconds * 1000);
            var successfulCount = logs.Count(x => x.IsSuccessful);

            Console.WriteLine("Avg: " + avgMilliseconds);
            Console.WriteLine();

            Console.WriteLine("Min: " + minMilliseconds);
            Console.WriteLine();

            Console.WriteLine("Max: " + maxMilliseconds.Milliseconds);
            Console.WriteLine("Slowest RayID: " + maxMilliseconds.RayID);
            Console.WriteLine("EndTime:" + maxMilliseconds.EndTime);
            Console.WriteLine();

            Console.WriteLine($"Long (>{longRequestSeconds}s): " + aboveSecondsCount);

            Console.WriteLine($"Successful: {successfulCount}");
            Console.WriteLine($"Failed: {logs.Count - successfulCount}");
            Console.WriteLine();

            Console.WriteLine("Status Breakdown:");
            logs.GroupBy(x => x.EdgeResponseStatus)
                .Select(x => new KeyValuePair<short, int>(x.Key, x.Count()))
                .OrderBy(x => x.Key)
                .ToList()
                .ForEach(g => Console.WriteLine($"{g.Key}: {g.Value}"));

            Console.ReadLine();
        }
    }
}
