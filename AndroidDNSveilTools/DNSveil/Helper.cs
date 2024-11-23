using Android.Content;
using Android.Content.Res;
using System.Diagnostics;
using Activity = Android.App.Activity;

namespace AndroidDNSveilTools;

public static class Helper
{
    public static string GetViewNameById(this Resources resources, int id)
    {
        string name = string.Empty;

        try
        {
            string? fullName = resources.GetResourceName(id);
            if (!string.IsNullOrEmpty(fullName)) name = fullName[(fullName.LastIndexOf('/') + 1)..];
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Helper GetViewNameById: " + ex);
        }

        return name;
    }

    public static void OpenUrl(Activity? activity, string url)
    {
		try
		{
			var uri = Android.Net.Uri.Parse(url);
			Intent intent = new(Intent.ActionView, uri);
			activity?.StartActivity(intent);
		}
		catch (Exception ex)
		{
			Debug.WriteLine("Helper OpenUrl: " + ex);
		}
    }
    
}