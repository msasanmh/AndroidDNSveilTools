using Android.Views;
using System.Runtime.InteropServices;
using Fragment = AndroidX.Fragment.App.Fragment;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace AndroidDNSveilTools;

public class AboutFragment : Fragment
{
    private View? view;
    private LinearLayout? linearLayout_AboutProjectPage;
    private TextView? textView_AboutProjectURL;
    private TextView? textView_AboutAppVersion;
    private TextView? textView_AboutAppArchitecture;

    public override View? OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState)
    {
        Toolbar? toolbarMain = Activity?.FindViewById<Toolbar>(Resource.Id.toolbar1);
        if (toolbarMain != null) toolbarMain.Title = Resources.GetString(Resource.String.title_About);

        view = inflater.Inflate(Resource.Layout.fragment_about, container, false);
        if (view == null) return view;
        //if (savedInstanceState != null) return view;

        linearLayout_AboutProjectPage = view.FindViewById<LinearLayout>(Resource.Id.linearLayout_AboutProjectPage);
        if (linearLayout_AboutProjectPage == null) return view;
        textView_AboutProjectURL = view.FindViewById<TextView>(Resource.Id.textView_AboutProjectURL);
        if (textView_AboutProjectURL == null) return view;
        textView_AboutAppVersion = view.FindViewById<TextView>(Resource.Id.textView_AboutAppVersion);
        if (textView_AboutAppVersion == null) return view;
        textView_AboutAppArchitecture = view.FindViewById<TextView>(Resource.Id.textView_AboutAppArchitecture);
        if (textView_AboutAppArchitecture == null) return view;

        linearLayout_AboutProjectPage.Click -= LinearLayout_AboutProjectPage_Click;
        linearLayout_AboutProjectPage.Click += LinearLayout_AboutProjectPage_Click;
        
        string ver = Application.Context.PackageManager?.GetPackageInfo(Application.Context.PackageName ?? string.Empty, 0)?.VersionName ?? string.Empty;
        textView_AboutAppVersion.Text = $"Version: {ver}";

        textView_AboutAppArchitecture.Text = $"Architecture: {RuntimeInformation.ProcessArchitecture}";

        return view;
    }

    private void LinearLayout_AboutProjectPage_Click(object? sender, EventArgs e)
    {
        if (view == null) return;
        if (textView_AboutProjectURL == null) return;
        if (textView_AboutProjectURL.Text == null) return;

        Helper.OpenUrl(Activity, textView_AboutProjectURL.Text);
    }

}