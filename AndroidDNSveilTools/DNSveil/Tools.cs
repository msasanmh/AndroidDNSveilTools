using MsmhToolsClass.MsmhAgnosticServer;
using MsmhToolsClass;
using System.Text;
using System.Diagnostics;

namespace AndroidDNSveilTools;

public class Tools
{
    private static readonly string NL = Environment.NewLine;

    public static bool IsDnsProtocolSupported(string dns)
    {
        try
        {
            dns = dns.Trim();
            StringComparison sc = StringComparison.OrdinalIgnoreCase;
            if (dns.StartsWith("udp://", sc) || dns.StartsWith("tcp://", sc) || dns.StartsWith("http://", sc) || dns.StartsWith("https://", sc) ||
                dns.StartsWith("h3://", sc) || dns.StartsWith("tls://", sc) || dns.StartsWith("quic://", sc) || dns.StartsWith("sdns://", sc))
                return true;
            else
                return isPlainDnsWithUnusualPort(dns);

            static bool isPlainDnsWithUnusualPort(string dns) // Support for plain DNS with unusual port
            {
                if (dns.Contains(':'))
                {
                    NetworkTool.GetUrlDetails(dns, 53, out _, out string ipStr, out _, out _, out int port, out _, out _);
                    if (NetworkTool.IsIP(ipStr, out _)) return port >= 1 && port <= 65535;
                }
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("IsDnsProtocolSupported: " + ex.Message);
            return false;
        }
    }

    public static async Task<List<string>> GetServersFromContentAsync(string content)
    {
        List<string> dnss = new();

        try
        {
            List<string> links = new();
            string[] lines = content.ReplaceLineEndings(NL).Split(NL, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            for (int n = 0; n < lines.Length; n++)
            {
                string line = lines[n];
                if (string.IsNullOrEmpty(line)) continue;

                // Support IP:Port
                bool isIP = false;
                try
                {
                    if (!line.Contains("://") && line.Contains(':'))
                    {
                        string temp = line;
                        int index = temp.LastIndexOf(':');
                        if (index != -1) temp = temp.Remove(index);
                        if (NetworkTool.IsIP(temp, out _)) isIP = true;
                    }
                }
                catch (Exception) { }

                if (isIP) links.Add(line);
                else
                {
                    // Support For Anonymized DNSCrypt
                    bool isWithRelay = false;
                    List<string> urls = await TextTool.GetLinksAsync(line);
                    if (urls.Count == 1)
                    {
                        try
                        {
                            string url1 = urls[0];
                            DnsReader dr1 = new(url1);
                            if (dr1.Protocol == DnsEnums.DnsProtocol.DnsCrypt)
                            {
                                string tempWithPort = line.Replace(url1, string.Empty).Trim();
                                string temp = tempWithPort;
                                int index = temp.LastIndexOf(':');
                                if (index != -1) temp = temp.Remove(index);
                                if (NetworkTool.IsIP(temp, out _))
                                {
                                    isWithRelay = true;
                                    urls.Add(tempWithPort);
                                }
                            }
                        }
                        catch (Exception) { }
                    }
                    else if (urls.Count == 2)
                    {
                        try
                        {
                            string url1 = urls[0];
                            string url2 = urls[1];
                            DnsReader dr1 = new(url1);
                            DnsReader dr2 = new(url2);
                            if (dr1.Protocol == DnsEnums.DnsProtocol.DnsCrypt)
                            {
                                if (dr2.Protocol == DnsEnums.DnsProtocol.AnonymizedDNSCryptRelay ||
                                    dr2.Protocol == DnsEnums.DnsProtocol.UDP ||
                                    dr2.Protocol == DnsEnums.DnsProtocol.TCP)
                                    isWithRelay = true;
                            }
                        }
                        catch (Exception) { }
                    }
                    //Debug.WriteLine("-=-==-= " + urls.ToString(" "));
                    if (isWithRelay) links.Add(urls.ToString(" "));
                    else links.AddRange(urls);
                }
            }

            for (int n = 0; n < links.Count; n++)
            {
                string dns = links[n];
                if (dns.StartsWith("http://") || dns.StartsWith("https://"))
                {
                    if (dns.EndsWith(".html", StringComparison.OrdinalIgnoreCase)) continue;
                    if (dns.Contains("github.com", StringComparison.OrdinalIgnoreCase)) continue;
                    if (dns.StartsWith("https://www.google.com", StringComparison.OrdinalIgnoreCase)) continue;
                    if (dns.StartsWith("https://google.com", StringComparison.OrdinalIgnoreCase)) continue;
                    if (dns.EndsWith("//www.google.com", StringComparison.OrdinalIgnoreCase)) continue;
                    if (dns.EndsWith("//google.com", StringComparison.OrdinalIgnoreCase)) continue;
                    if (dns.Contains("support.google.com", StringComparison.OrdinalIgnoreCase)) continue;
                    if (dns.StartsWith("https://www.microsoft.com", StringComparison.OrdinalIgnoreCase)) continue;
                    if (dns.StartsWith("https://microsoft.com", StringComparison.OrdinalIgnoreCase)) continue;
                    if (dns.EndsWith("//www.microsoft.com", StringComparison.OrdinalIgnoreCase)) continue;
                    if (dns.EndsWith("//microsoft.com", StringComparison.OrdinalIgnoreCase)) continue;
                    if (dns.Contains("learn.microsoft.com", StringComparison.OrdinalIgnoreCase)) continue;
                }
                if (IsDnsProtocolSupported(dns)) dnss.Add(dns);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("GetServersFromContentAsync: " + ex.Message);
        }

        return dnss;
    }

    public static async Task<List<string>> GetServersFromLinkAsync(string url)
    {
        try
        {
            Uri uri = new(url, UriKind.Absolute);

            HttpRequest hr = new()
            {
                AllowAutoRedirect = true,
                AllowInsecure = false,
                TimeoutMS = 20000,
                URI = uri
            };

            string content = string.Empty;
            HttpRequestResponse hrr = await HttpRequest.SendAsync(hr);
            
            if (hrr.IsSuccess)
            {
                content = Encoding.UTF8.GetString(hrr.Data);
                content = await TextTool.RemoveHtmlTagsAsync(content, true);
            }
            else
            {
                // Try With System Proxy
                string systemProxyScheme = NetworkTool.GetSystemProxy();
                if (!string.IsNullOrWhiteSpace(systemProxyScheme))
                {
                    hr.ProxyScheme = systemProxyScheme;
                    hrr = await HttpRequest.SendAsync(hr);
                    if (hrr.IsSuccess)
                    {
                        content = Encoding.UTF8.GetString(hrr.Data);
                        content = await TextTool.RemoveHtmlTagsAsync(content, true);
                    }
                }
            }

            return await GetServersFromContentAsync(content);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("GetServersFromLinkAsync: " + ex.Message);
            return new List<string>();
        }
    }
}
