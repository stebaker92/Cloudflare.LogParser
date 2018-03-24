using System;

namespace Cloudflare.LogParser
{
    internal class CloudflareLog
    {
        public string ClientIP { get; set; }
        public string ClientRequestHost { get; set; }
        public string ClientRequestMethod { get; set; }
        public string ClientRequestURI { get; set; }
        public long EdgeStartTimestamp { get; set; }
        public long EdgeEndTimestamp { get; set; }
        public int EdgeResponseBytes { get; set; }
        public short EdgeResponseStatus { get; set; }
        public string RayID { get; set; }


        public double Milliseconds
        {
            get
            {
                //return EdgeEndTimestamp - EdgeStartTimestamp;
                return (EndTime - StartTime).TotalMilliseconds;
            }
        }

        public DateTimeOffset StartTime
        {
            get
            {
                return UnixNanoToDateTime((EdgeStartTimestamp));
            }
        }
        public DateTimeOffset EndTime
        {
            get
            {
                return UnixNanoToDateTime((EdgeEndTimestamp));
            }
        }

        public bool IsSuccessful
        {
            get
            {
                return EdgeResponseStatus >= 200 && EdgeResponseStatus < 399;
            }
        }

        private long TimestampToUnix(string timestamp)
        {
            return Convert.ToInt64(timestamp.Substring(0, 13));
        }

        private DateTimeOffset UnixNanoToDateTime(long nanoseconds)
        {
            DateTime epochTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime result = epochTime.AddTicks(nanoseconds / 100);

            return result;
        }

    }
}