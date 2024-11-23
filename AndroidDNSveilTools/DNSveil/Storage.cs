using Android.Content;
using System.Diagnostics;

namespace AndroidDNSveilTools;

public class Storage
{
    private static readonly string SettingsName = "DNSveilSettings";
    private static readonly string DnsA_Name = "DNSveilDnsA";

    public static bool SaveSetting(string key, string? value)
    {
        bool result = false;

        try
        {
            var prefs = Application.Context.GetSharedPreferences(SettingsName, FileCreationMode.Private);
            if (prefs == null) return result;
            var editor = prefs.Edit();
            if (editor == null) return result;
            editor.PutString(key, value);
            editor.Apply(); // Apply() = Async, Commit() = Sync
            result = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Storage SaveSetting String: " + ex.Message);
        }

        return result;
    }

    public static bool SaveSetting(string key, int value)
    {
        bool result = false;

        try
        {
            var prefs = Application.Context.GetSharedPreferences(SettingsName, FileCreationMode.Private);
            if (prefs == null) return result;
            var editor = prefs.Edit();
            if (editor == null) return result;
            editor.PutInt(key, value);
            editor.Apply(); // Apply() = Async, Commit() = Sync
            result = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Storage SaveSetting Int: " + ex.Message);
        }

        return result;
    }

    public static bool SaveSetting(string key, bool value)
    {
        bool result = false;

        try
        {
            var prefs = Application.Context.GetSharedPreferences(SettingsName, FileCreationMode.Private);
            if (prefs == null) return result;
            var editor = prefs.Edit();
            if (editor == null) return result;
            editor.PutBoolean(key, value);
            editor.Apply(); // Apply() = Async, Commit() = Sync
            result = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Storage SaveSetting Bool: " + ex.Message);
        }

        return result;
    }

    public static string ReadSetting(string key, string? defValue)
    {
        string value = defValue ?? string.Empty;

        try
        {
            var prefs = Application.Context.GetSharedPreferences(SettingsName, FileCreationMode.Private);
            if (prefs != null) value = prefs.GetString(key, value) ?? string.Empty;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Storage ReadSetting String: " + ex.Message);
        }

        return value;
    }

    public static int ReadSetting(string key, int defValue)
    {
        int value = defValue;

        try
        {
            var prefs = Application.Context.GetSharedPreferences(SettingsName, FileCreationMode.Private);
            if (prefs != null) value = prefs.GetInt(key, value);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Storage ReadSetting Int: " + ex.Message);
        }

        return value;
    }

    public static bool ReadSetting(string key, bool defValue)
    {
        bool value = defValue;

        try
        {
            var prefs = Application.Context.GetSharedPreferences(SettingsName, FileCreationMode.Private);
            if (prefs != null) value = prefs.GetBoolean(key, value);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Storage ReadSetting Bool: " + ex.Message);
        }

        return value;
    }

    public static bool SaveDnsA(string dns, string? delay)
    {
        bool result = false;

        try
        {
            var prefs = Application.Context.GetSharedPreferences(DnsA_Name, FileCreationMode.Private);
            if (prefs == null) return result;
            var editor = prefs.Edit();
            if (editor == null) return result;
            editor.PutString(dns, delay);
            editor.Apply(); // Apply() = Async, Commit() = Sync
            //Toast.MakeText(Application.Context, $"Saved: {dns}", ToastLength.Short)?.Show();
            result = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Storage SaveDnsA: " + ex.Message);
        }

        return result;
    }

    public static Dictionary<string, int> ReadDnsA()
    {
        Dictionary<string, int> savedDNSs = new();
        
        try
        {
            var prefs = Application.Context.GetSharedPreferences(DnsA_Name, FileCreationMode.Private);
            if (prefs != null)
            {
                if (prefs.All != null)
                {
                    foreach (var kvp in prefs.All)
                    {
                        string dns = kvp.Key;
                        string? delay = kvp.Value.ToString();
                        //Toast.MakeText(Application.Context, dns, ToastLength.Short)?.Show();
                        if (delay != null)
                        {
                            bool isDelayInt = int.TryParse(delay, out int delayInt);
                            if (isDelayInt) savedDNSs.Add(dns, delayInt);
                        }
                    }

                    savedDNSs = savedDNSs.OrderBy(x => x.Value).ToDictionary();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Storage ReadDnsA: " + ex.Message);
        }

        return savedDNSs;
    }

    public static async Task<bool> ClearDnsA_Async()
    {
        bool result = false;

        try
        {
            var prefs = Application.Context.GetSharedPreferences(DnsA_Name, FileCreationMode.Private);
            if (prefs == null) return result;
            var editor = prefs.Edit();
            if (editor == null) return result;
            editor.Clear();
            editor.Apply();

            await Task.Run(async () =>
            {
                while (true)
                {
                    if (ReadDnsA().Count == 0) break;
                    await Task.Delay(200);
                }
            });

            result = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Storage ClearDnsA: " + ex.Message);
        }

        return result;
    }

}