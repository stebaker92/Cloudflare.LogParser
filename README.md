# Cloudflare.LogParser
A basic dotnet console app to parse & query cloudflare logs

Cloudflares API doesn't currently support much / any filtering of data, so this app allows you to filter by multiple options (such as `host` & `route`).

A breakdown of requests are also displayed with information such as average request time, a count of successful and failed requests and a status code breakdown.

## Usage

Add your details to the top of `Startup.cs` (e.g. ZoneId, AuthKey, AuthEmail)
