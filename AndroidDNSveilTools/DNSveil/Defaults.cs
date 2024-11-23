namespace AndroidDNSveilTools;

public class Defaults
{
    public static readonly string DnsLookup_DnsAddress = "https://sky.rethinkdns.com/dns-query";
    public static readonly string DnsLookup_Domain = "youtube.com";
    public static readonly int DnsLookup_RRTYPE = 0; // A
    public static readonly string Settings_BlockedDomain = "youtube.com";
    public static readonly int Settings_TimeoutMS = 5000;
    public static readonly string Settings_DnsProviderUrls = $"https://github.com/curl/curl/wiki/DNS-over-HTTPS{Environment.NewLine}" +
        $"https://adguard-dns.io/kb/general/dns-providers/{Environment.NewLine}" +
        $"https://raw.githubusercontent.com/NiREvil/vless/refs/heads/main/DNS%20over%20HTTPS/any%20DNS-over-HTTPS%20server%20you%20want.md";
}