namespace Cloudflare.LogParser
{
    internal static class LogFields
    {
        static string ClientIP = "ClientIP";
        static string ClientRequestHost = "ClientRequestHost";
        static string ClientRequestMethod = "ClientRequestMethod";
        static string ClientRequestURI = "ClientRequestURI";
        static string EdgeEndTimestamp = "EdgeEndTimestamp";
        static string EdgeResponseBytes = "EdgeResponseBytes";
        static string EdgeResponseStatus = "EdgeResponseStatus";
        static string EdgeStartTimestamp = "EdgeStartTimestamp";
        static string RayID = "RayID";

        internal static string[] All = new[] {
            ClientIP,
            ClientRequestHost,
            ClientRequestMethod,
            ClientRequestURI,
            EdgeEndTimestamp,
            EdgeResponseBytes,
            EdgeResponseStatus,
            EdgeStartTimestamp,
            RayID
        };
    }
}
