using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace PingChecker
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var addr = args[0];
            var interval = int.Parse(args[1]);

            Console.WriteLine($"Ping: {addr}, Interval: {interval}ms...");

            var requestCount = 0;
            var goodResponseCount = 0;
            var googResponseTotalDelay = 0L;
            var goodResponseAvgDelay = 0L;
            var badResponseCount = 0;

            await foreach (var (ok, delay) in Check(addr, interval))
            {
                requestCount += 1;

                if (ok)
                {
                    goodResponseCount += 1;
                    googResponseTotalDelay += delay;
                    goodResponseAvgDelay = googResponseTotalDelay / goodResponseCount;
                }
                else
                {
                    badResponseCount += 1;
                }

                Console.Write($"Bad response/Total response: {badResponseCount}/{requestCount}, Rate: {100 * (double)badResponseCount / requestCount:F2}%, Avg delay: {goodResponseAvgDelay}\r");
            }
        }

        public static async IAsyncEnumerable<(bool ok, long delay)> Check(string addr, int interval)
        {
            var sender = new Ping();
            var pingOptions = new PingOptions() { DontFragment = true };

            const string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            var buffer = Encoding.ASCII.GetBytes(data);

            const int timeout = 120;

            while (true)
            {
                var reply = await sender.SendPingAsync(addr, timeout, buffer, pingOptions);
                if (reply.Status != IPStatus.Success)
                {
                    yield return (false, long.MaxValue);
                }
                else
                {
                    yield return (true, reply.RoundtripTime);
                }

                await Task.Delay(interval);
            }
        }
    }
}
