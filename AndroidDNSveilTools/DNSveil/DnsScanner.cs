using Android.Graphics;
using AndroidDNSveilTools.DNSveil.Resource;
using MsmhToolsClass;
using System.Globalization;

namespace AndroidDNSveilTools;

public class DnsScanner
{
    private static readonly string NL = Environment.NewLine;
    private bool Cancel = false;
    public bool IsRunning { get; private set; } = false;
    public bool IsCancelling => IsRunning && Cancel;
    public event EventHandler<LogEventArgs>? OnLogRecieved;
    public event EventHandler<EventArgs>? OnStatusChanged;
    private List<Tuple<int, string>> WorkingServers = new();
    public event EventHandler<WorkingServerEventArgs>? OnWorkingServerRecieved;
    public event EventHandler<ResultEventArgs>? OnScanFinished;

    public DnsScanner() { }

    public class LogEventArgs(string message, Color color) : EventArgs
    {
        public string Message { get; private set; } = message;
        public Color Color { get; private set; } = color;
    }

    public class WorkingServerEventArgs(string dns, int delay) : EventArgs
    {
        public string DNS { get; private set; } = dns;
        public int Delay { get; private set; } = delay;
    }

    public class ResultEventArgs(List<Tuple<int, string>> workingServers) : EventArgs
    {
        public List<Tuple<int, string>> WorkingServers { get; private set; } = workingServers;
    }

    public async void Start()
    {
        try
        {
            if (IsRunning) return;
            IsRunning = true;
            Cancel = false;
            WorkingServers.Clear();
            string onStatusChanged = "Started...";
            OnStatusChanged?.Invoke(onStatusChanged, EventArgs.Empty);

            string dlURLsContent = Storage.ReadSetting(SettingsUniqueNames.Settings_DnsProviderUrls, Defaults.Settings_DnsProviderUrls);
            List<string> dlURLs = dlURLsContent.ReplaceLineEndings().Split(NL, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

            OnLogRecieved?.Invoke(this, new LogEventArgs("Downloading Servers...", Color.Gray));
            List<string> dnss = new();
            foreach (string dlURL in dlURLs)
            {
                if (dlURL.StartsWith("http", StringComparison.OrdinalIgnoreCase) ||
                    dlURL.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    OnLogRecieved?.Invoke(this, new LogEventArgs("Scraping...", Color.Gray));
                    OnLogRecieved?.Invoke(this, new LogEventArgs(dlURL, Color.Gray));
                    List<string> ss = await Tools.GetServersFromLinkAsync(dlURL);
                    OnLogRecieved?.Invoke(this, new LogEventArgs($"Contains {ss.Count} Servers", Color.Gray));
                    dnss.AddRange(ss);
                }
                
                if (Cancel) break;
            }

            if (dnss.Count == 0)
            {
                OnLogRecieved?.Invoke(this, new LogEventArgs("URLs Unavailable. Using Backup.", Color.IndianRed));
                dnss = Resource1.dohs.ReplaceLineEndings().Split(NL).ToList();
            }

            // DeDup
            OnLogRecieved?.Invoke(this, new LogEventArgs("Removing Duplicates...", Color.Gray));
            dnss = dnss.Distinct().ToList();

            OnLogRecieved?.Invoke(this, new LogEventArgs("Scanning Servers...", Color.Gray));
            string domain = Storage.ReadSetting(SettingsUniqueNames.Settings_BlockedDomain, Defaults.Settings_BlockedDomain);
            int timeoutMS = Storage.ReadSetting(SettingsUniqueNames.Settings_TimeoutMS, Defaults.Settings_TimeoutMS);
            bool allowInsecure = Storage.ReadSetting(SettingsUniqueNames.Settings_AllowInsecure, false);
            CheckDns check = new(allowInsecure, false);
            int countWorkingServers = 0;
            int splitSize = 5;
            var lists = dnss.SplitToLists(splitSize);
            int count = 0;

            for (int n = 0; n < lists.Count; n++)
            {
                if (countWorkingServers >= 10) break;

                List<string> list = lists[n];

                // Percentage
                count += list.Count;
                if (dnss.Count > 0)
                {
                    int percent = count * 100 / dnss.Count;
                    onStatusChanged = $"Progress {count}/{dnss.Count} ({percent}%), Working Servers: {countWorkingServers}";
                    OnStatusChanged?.Invoke(onStatusChanged.ToString(CultureInfo.InvariantCulture), EventArgs.Empty);
                }
                
                await Parallel.ForEachAsync(list, async (dns, cancellationToken) =>
                {
                    if (Cancel) return;
                    await checkOneAsync(dns);
                });

                // Percentage (100%)
                if (n == lists.Count - 1)
                {
                    onStatusChanged = $"Progress {dnss.Count}/{dnss.Count} (100%), Working Servers: {countWorkingServers}";
                    OnStatusChanged?.Invoke(onStatusChanged.ToString(CultureInfo.InvariantCulture), EventArgs.Empty);
                }

                if (Cancel)
                {
                    OnStatusChanged?.Invoke("Canceling Check Operation...", EventArgs.Empty);
                    break;
                }
            }

            async Task checkOneAsync(string dns)
            {
                CheckDns.CheckDnsResult r = await check.CheckDnsAsync(domain, dns, timeoutMS);
                if (r.IsDnsOnline)
                {
                    countWorkingServers++;
                    WorkingServers.Add(new Tuple<int, string>(r.DnsLatency, dns));
                    OnLogRecieved?.Invoke(this, new LogEventArgs($"{r.DnsLatency} ms, {dns}", Color.MediumSeaGreen));
                    OnWorkingServerRecieved?.Invoke(this, new WorkingServerEventArgs(dns, r.DnsLatency));
                }
                else
                {
                    OnLogRecieved?.Invoke(this, new LogEventArgs($"{r.DnsLatency} ms, {dns}", Color.IndianRed));
                }
            }

            onStatusChanged = Cancel ? "Scan Cancelled" : "Scan Finished";
            OnStatusChanged?.Invoke(onStatusChanged, EventArgs.Empty);

            // Sort By Latency
            WorkingServers = WorkingServers.OrderBy(x => x.Item1).ToList();

            IsRunning = false;
            OnScanFinished?.Invoke(this, new ResultEventArgs(WorkingServers));
        }
        catch (Exception ex)
        {
            IsRunning = false;
            AlertDialog.Builder alert = new(Application.Context);
            alert.SetTitle("Exception")?.SetMessage(ex.Message)?.Show();
        }
    }

    public void Stop()
    {
        Cancel = true;
    }
}