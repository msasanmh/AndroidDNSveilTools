using MsmhToolsClass;
using MsmhToolsClass.MsmhAgnosticServer;
using System.Diagnostics;
using System.Net;

namespace AndroidDNSveilTools;

public class CheckDns
{
    public class CheckDnsResult
    {
        public bool IsDnsOnline { get; internal set; } = false;
        public string DNS { get; internal set; } = string.Empty;

        /// <summary>
        /// Returns -1 If DNS Fail
        /// </summary>
        public int DnsLatency { get; internal set; } = -1;
        public DnsMessage DnsMessage { get; set; } = new DnsMessage();
        public bool IsGoogleSafeSearchEnabled { get; internal set; } = false;
        public bool IsBingSafeSearchEnabled { get; internal set; } = false;
        public bool IsYoutubeRestricted { get; internal set; } = false;
        public bool IsAdultFilter { get; internal set; } = false;
    }

    private string AdultDomainToCheck { get; set; } = "pornhub.com";
    private List<IPAddress> GoogleSafeSearchIpList { get; set; } = new();
    private List<IPAddress> BingSafeSearchIpList { get; set; } = new();
    private List<IPAddress> YoutubeRestrictIpList { get; set; } = new();
    private List<IPAddress> AdultIpList { get; set; } = new();
    private bool Insecure { get; set; } = false;
    private bool CheckForFilters { get; set; } = false;
    private readonly int TimeoutMS = 10000;

    /// <summary>
    /// Check DNS Servers
    /// </summary>
    public CheckDns(bool insecure, bool checkForFilters)
    {
        Insecure = insecure;
        CheckForFilters = checkForFilters;
    }

    /// <summary>
    /// Check DNS and get latency (ms)
    /// </summary>
    public async Task<CheckDnsResult> CheckDnsAsync(string domain, string dnsServer, int timeoutMS, DnsEnums.RRType rrType = DnsEnums.RRType.A)
    {
        DnsLookupResult dlr = await CheckDnsWorkAsync(domain, dnsServer, timeoutMS, IPAddress.None, 0, rrType).ConfigureAwait(false);
        bool isDnsOnline = dlr.IsDnsOnline;
        int dnsLatency = isDnsOnline ? dlr.Latency : -1;

        bool isGoogleSafeSearchEnabled = false, isBingSafeSearchEnabled = false;
        bool isYoutubeRestricted = false, isAdultFilter = false;
        if (CheckForFilters && isDnsOnline)
        {
            var (IsGoogleSafeSearch, IsBingSafeSearch, IsYoutubeRestricted, IsAdultFilter) = await CheckDnsFiltersAsync(dnsServer);
            isGoogleSafeSearchEnabled = IsGoogleSafeSearch;
            isBingSafeSearchEnabled = IsBingSafeSearch;
            isYoutubeRestricted = IsYoutubeRestricted;
            isAdultFilter = IsAdultFilter;
        }

        return new CheckDnsResult
        {
            IsDnsOnline = isDnsOnline,
            DNS = dnsServer,
            DnsLatency = dnsLatency,
            DnsMessage = dlr.DnsMessage,
            IsGoogleSafeSearchEnabled = isGoogleSafeSearchEnabled,
            IsBingSafeSearchEnabled = isBingSafeSearchEnabled,
            IsYoutubeRestricted = isYoutubeRestricted,
            IsAdultFilter = isAdultFilter
        };
    }

    /// <summary>
    /// Check DNS And Get Latency (ms)
    /// </summary>
    public async Task<CheckDnsResult> CheckDnsAsync(string domain, string dnsServer, int timeoutMS, IPAddress bootstrapIP, int bootstrapPort, DnsEnums.RRType rrType = DnsEnums.RRType.A)
    {
        DnsLookupResult dlr = await CheckDnsWorkAsync(domain, dnsServer, timeoutMS, bootstrapIP, bootstrapPort, rrType).ConfigureAwait(false);
        bool isDnsOnline = dlr.IsDnsOnline;
        int dnsLatency = isDnsOnline ? dlr.Latency : -1;
        
        bool isGoogleSafeSearchEnabled = false, isBingSafeSearchEnabled = false;
        bool isYoutubeRestricted = false, isAdultFilter = false;
        if (CheckForFilters && isDnsOnline)
        {
            var (IsGoogleSafeSearch, IsBingSafeSearch, IsYoutubeRestricted, IsAdultFilter) = await CheckDnsFiltersAsync(dnsServer);
            isGoogleSafeSearchEnabled = IsGoogleSafeSearch;
            isBingSafeSearchEnabled = IsBingSafeSearch;
            isYoutubeRestricted = IsYoutubeRestricted;
            isAdultFilter = IsAdultFilter;
        }

        return new CheckDnsResult
        {
            IsDnsOnline = isDnsOnline,
            DNS = dnsServer,
            DnsLatency = dnsLatency,
            DnsMessage = dlr.DnsMessage,
            IsGoogleSafeSearchEnabled = isGoogleSafeSearchEnabled,
            IsBingSafeSearchEnabled = isBingSafeSearchEnabled,
            IsYoutubeRestricted = isYoutubeRestricted,
            IsAdultFilter = isAdultFilter
        };
    }

    private class DnsLookupResult
    {
        public int Latency { get; set; } = -1;
        public bool IsDnsOnline { get; set; } = false;
        public DnsMessage DnsMessage { get; set; } = new DnsMessage();
    }

    private async Task<DnsLookupResult> CheckDnsWorkAsync(string domain, string dnsServer, int timeoutMS, IPAddress bootstrapIP, int bootstrapPort, DnsEnums.RRType rrType = DnsEnums.RRType.A)
    {
        try
        {
            DnsLookupResult dlr = new();
            bool hasLocalIp = false;
            int aRecordCount = 0;
            int latency = -1;
            DnsMessage dmQ = DnsMessage.CreateQuery(DnsEnums.DnsProtocol.UDP, domain, rrType, DnsEnums.CLASS.IN);
            dlr.DnsMessage = dmQ;
            bool isWriteSuccess = DnsMessage.TryWrite(dmQ, out byte[] dmQBuffer);
            if (isWriteSuccess)
            {
                Stopwatch sw = Stopwatch.StartNew();
                byte[] dmABuffer = await DnsClient.QueryAsync(dmQBuffer, DnsEnums.DnsProtocol.UDP, dnsServer, Insecure, bootstrapIP, bootstrapPort, timeoutMS, CancellationToken.None).ConfigureAwait(false);
                sw.Stop();
                latency = Convert.ToInt32(sw.ElapsedMilliseconds);
                if (dmABuffer.Length >= 12) // 12 Header Length
                {
                    DnsMessage dmA = DnsMessage.Read(dmABuffer, DnsEnums.DnsProtocol.UDP);
                    dlr.DnsMessage = dmA;
                    if (dmA.IsSuccess)
                    {
                        //Debug.WriteLine("==========-------------> " + dmA.ToString());
                        if (dmA.Header.AnswersCount > 0 && dmA.Answers.AnswerRecords.Count > 0)
                        {
                            for (int n = 0; n < dmA.Answers.AnswerRecords.Count; n++)
                            {
                                IResourceRecord irr = dmA.Answers.AnswerRecords[n];
                                if (irr is not ARecord aRecord) continue;
                                if (NetworkTool.IsLocalIP(aRecord.IP.ToString())) hasLocalIp = true;
                                aRecordCount++;
                            }
                        }
                    }
                }
            }

            if (rrType == DnsEnums.RRType.A)
                dlr.IsDnsOnline = !hasLocalIp && aRecordCount > 0;
            else
                dlr.IsDnsOnline = latency != -1;
            dlr.Latency = latency;

            return dlr;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("CheckDnsWorkAsync: " + ex.Message);
            return new DnsLookupResult();
        }
    }

    //================================= Check Dns as SmartDns

    public async Task<bool> CheckAsSmartDnsAsync(string uncensoredDns, string domain, string dns)
    {

        bool smart = false;

        List<IPAddress> realDomainIPs = await GetARecordIPsAsync(domain, uncensoredDns).ConfigureAwait(false);
        List<IPAddress> domainIPs = await GetARecordIPsAsync(domain, dns).ConfigureAwait(false);

        // Method 1: Reverse Dns
        if (realDomainIPs.Count != 0 && domainIPs.Count != 0)
        {
            bool isMatch = false;
            int count = 0;
            for (int n = 0; n < realDomainIPs.Count; n++)
            {
                IPAddress realDomainIP = realDomainIPs[n];
                var ipToHost1 = await NetworkTool.IpToHostAsync(realDomainIP.ToString());
                string realHost = ipToHost1.BaseHost;
                if (string.IsNullOrEmpty(realHost)) continue;
                for (int n2 = 0; n2 < domainIPs.Count; n2++)
                {

                    IPAddress domainIP = domainIPs[n2];
                    var ipToHost2 = await NetworkTool.IpToHostAsync(domainIP.ToString());
                    string host = ipToHost2.BaseHost;
                    if (string.IsNullOrEmpty(host)) continue;
                    if (NetworkTool.IsLocalIP(domainIP.ToString())) continue;

                    Debug.WriteLine(realHost + " == " + host);
                    if (!realHost.Equals(host, StringComparison.OrdinalIgnoreCase))
                    {
                        count++;
                        isMatch = true;
                    }
                }
            }

            smart = isMatch && (count == realDomainIPs.Count * domainIPs.Count);
        }
        if (smart) return smart;

        // Method 2: Reading Headers
        if (realDomainIPs.Count != 0 && domainIPs.Count != 0)
        {
            for (int n = 0; n < realDomainIPs.Count; n++)
            {
                if (smart) break;
                IPAddress realDomainIP = realDomainIPs[n];
                string readHeader = await NetworkTool.GetHeadersAsync(domain, realDomainIP.ToString(), 5000, false).ConfigureAwait(false);
                if (string.IsNullOrEmpty(readHeader)) continue; // There is nothing to check, continue
                Debug.WriteLine(readHeader);
                if (!readHeader.ToLower().StartsWith("forbidden")) break; // It's not Forbidden, break
                for (int n2 = 0; n2 < domainIPs.Count; n2++)
                {
                    IPAddress domainIP = domainIPs[n2];
                    string header = await NetworkTool.GetHeadersAsync(domain, domainIP.ToString(), 5000, false).ConfigureAwait(false);
                    if (string.IsNullOrEmpty(header)) continue;
                    Debug.WriteLine(header);

                    if (!header.ToLower().StartsWith("forbidden"))
                    {
                        smart = true;
                        if (smart) break;
                    }
                }
            }
        }
        return smart;
    }

    //================================= Generate IPs

    public async Task<int> GenerateGoogleSafeSearchIpsAsync(string uncensoredDns)
    {
        if (GoogleSafeSearchIpList.Count == 0)
        {
            string websiteSS = "forcesafesearch.google.com";
            GoogleSafeSearchIpList = await GetARecordIPsAsync(websiteSS, uncensoredDns).ConfigureAwait(false);
            Debug.WriteLine("Google Safe Search IPs Generated, Count: " + GoogleSafeSearchIpList.Count);
        }
        return GoogleSafeSearchIpList.Count;
    }

    public async Task<int> GenerateBingSafeSearchIpsAsync(string uncensoredDns)
    {
        if (BingSafeSearchIpList.Count == 0)
        {
            string websiteSS = "strict.bing.com";
            BingSafeSearchIpList = await GetARecordIPsAsync(websiteSS, uncensoredDns).ConfigureAwait(false);
            Debug.WriteLine("Bing Safe Search IPs Generated, Count: " + BingSafeSearchIpList.Count);
        }
        return BingSafeSearchIpList.Count;
    }

    public async Task<int> GenerateYoutubeRestrictIpsAsync(string uncensoredDns)
    {
        if (YoutubeRestrictIpList.Count == 0)
        {
            string websiteR = "restrict.youtube.com";
            string websiteRM = "restrictmoderate.youtube.com";
            List<IPAddress> youtubeR = await GetARecordIPsAsync(websiteR, uncensoredDns).ConfigureAwait(false);
            List<IPAddress> youtubeRM = await GetARecordIPsAsync(websiteRM, uncensoredDns).ConfigureAwait(false);

            YoutubeRestrictIpList = youtubeR.Concat(youtubeRM).ToList();
            Debug.WriteLine("Youtube Restrict IPs Generated, Count: " + YoutubeRestrictIpList.Count);
        }
        return YoutubeRestrictIpList.Count;
    }

    public async Task<int> GenerateAdultDomainIpsAsync(string uncensoredDns)
    {
        if (AdultIpList.Count == 0)
        {
            string websiteAD = AdultDomainToCheck;
            AdultIpList = await GetARecordIPsAsync(websiteAD, uncensoredDns).ConfigureAwait(false);
            Debug.WriteLine("Adult IPs Generated, Count: " + AdultIpList.Count);
        }
        return AdultIpList.Count;
    }

    //================================= Check DNS Filters

    private async Task<(bool IsGoogleSafeSearch, bool IsBingSafeSearch, bool IsYoutubeRestricted, bool IsAdultFilter)> CheckDnsFiltersAsync(string dnsServer)
    {
        bool isGoogleSafeSearchOut = false;
        bool isBingSafeSearchOut = false;
        bool isYoutubeRestrictedOut = false;
        bool isAdultFilterOut = false;

        Task task = Task.Run(async () =>
        {
            await GenerateGoogleSafeSearchIpsAsync(dnsServer).ConfigureAwait(false);
            await GenerateBingSafeSearchIpsAsync(dnsServer).ConfigureAwait(false);
            await GenerateYoutubeRestrictIpsAsync(dnsServer).ConfigureAwait(false);
            await GenerateAdultDomainIpsAsync(dnsServer).ConfigureAwait(false);

            // Check Google Force Safe Search
            if (GoogleSafeSearchIpList.Count != 0)
            {
                List<IPAddress> googleIpList = await GetARecordIPsAsync("google.com", dnsServer).ConfigureAwait(false);
                isGoogleSafeSearchOut = HasSameItem(googleIpList, GoogleSafeSearchIpList, true);
            }

            // Check Bing Force Safe Search
            if (BingSafeSearchIpList.Count != 0)
            {
                List<IPAddress> bingIpList = await GetARecordIPsAsync("bing.com", dnsServer).ConfigureAwait(false);
                isBingSafeSearchOut = HasSameItem(bingIpList, BingSafeSearchIpList, true);
            }

            // Check Youtube Restriction
            if (YoutubeRestrictIpList.Count != 0)
            {
                List<IPAddress> youtubeIpList = await GetARecordIPsAsync("youtube.com", dnsServer).ConfigureAwait(false);
                isYoutubeRestrictedOut = HasSameItem(youtubeIpList, YoutubeRestrictIpList, true);
            }
            
            // Check Adult Filter
            if (AdultIpList.Count != 0)
            {
                List<IPAddress> adultIpList = await GetARecordIPsAsync(AdultDomainToCheck, dnsServer).ConfigureAwait(false);
                isAdultFilterOut = HasLocalIP(adultIpList);
                if (!isAdultFilterOut)
                    isAdultFilterOut = !HasSameItem(adultIpList, AdultIpList, false);
            }
        });

        try { await task.WaitAsync(CancellationToken.None); } catch (Exception) { }

        return (isGoogleSafeSearchOut, isBingSafeSearchOut, isYoutubeRestrictedOut, isAdultFilterOut);
    }

    private async Task<List<IPAddress>> GetARecordIPsAsync(string domain, string dnsServer)
    {
        List<IPAddress> ips = new();

        try
        {
            DnsMessage dmQ = DnsMessage.CreateQuery(DnsEnums.DnsProtocol.UDP, domain, DnsEnums.RRType.A, DnsEnums.CLASS.IN);
            bool isWriteSuccess = DnsMessage.TryWrite(dmQ, out byte[] dmQBuffer);
            if (isWriteSuccess)
            {
                byte[] dmABuffer = await DnsClient.QueryAsync(dmQBuffer, DnsEnums.DnsProtocol.UDP, dnsServer, Insecure, IPAddress.None, 0, TimeoutMS, CancellationToken.None).ConfigureAwait(false);
                DnsMessage dmA = DnsMessage.Read(dmABuffer, DnsEnums.DnsProtocol.UDP);
                if (dmA.IsSuccess)
                {
                    if (dmA.Header.AnswersCount > 0 && dmA.Answers.AnswerRecords.Count > 0)
                    {
                        for (int n = 0; n < dmA.Answers.AnswerRecords.Count; n++)
                        {
                            IResourceRecord irr = dmA.Answers.AnswerRecords[n];
                            if (irr is not ARecord aRecord) continue;
                            ips.Add(aRecord.IP);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("GetARecordIPsAsync: " + ex.Message);
        }

        return ips;
    }

    private static bool HasSameItem(List<IPAddress> list, List<IPAddress> uncensoredList, bool checkForLocalIPs)
    {
        bool hasSameItem = false;
        for (int i = 0; i < uncensoredList.Count; i++)
        {
            if (hasSameItem) break;
            IPAddress uncensoredIp = uncensoredList[i];
            for (int j = 0; j < list.Count; j++)
            {
                IPAddress ip = list[j];
                if (ip.Equals(uncensoredIp))
                {
                    hasSameItem = true;
                    break;
                }
                if (checkForLocalIPs && NetworkTool.IsLocalIP(ip.ToString()))
                {
                    hasSameItem = true;
                    break;
                }
            }
        }
        return hasSameItem;
    }

    private static bool HasLocalIP(List<IPAddress> list)
    {
        for (int n = 0; n < list.Count; n++)
        {
            IPAddress ip = list[n];
            if (NetworkTool.IsLocalIP(ip.ToString())) return true;
        }
        return false;
    }

}