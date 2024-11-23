using Android.Views;
using Android.Widget;
using Fragment = AndroidX.Fragment.App.Fragment;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace AndroidDNSveilTools;

public class SettingsFragment : Fragment
{
    private View? view;

    private LinearLayout? linearLayout_Settings_BlockedDomain;
    private TextView? textView_Settings_BlockedDomain;

    private LinearLayout? linearLayout_Settings_TimeoutMS;
    private TextView? textView_Settings_TimeoutMS;

    private LinearLayout? linearLayout_Settings_AllowInsecure;
    private CheckBox? checkBox_Settings_AllowInsecure;

    private LinearLayout? linearLayout_Settings_DnsProviderUrls;
    private TextView? textView_Settings_DnsProviderUrls;

    private Button? button_Settings_ResetSettingsToDefaults;

    public override View? OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState)
    {
        Toolbar? toolbarMain = Activity?.FindViewById<Toolbar>(Resource.Id.toolbar1);
        if (toolbarMain != null) toolbarMain.Title = Resources.GetString(Resource.String.title_Settings);

        view = inflater.Inflate(Resource.Layout.fragment_settings, container, false);
        if (view == null) return view;
        if (savedInstanceState != null) return view;
        //return base.OnCreateView(inflater, container, savedInstanceState);

        linearLayout_Settings_BlockedDomain = view.FindViewById<LinearLayout>(Resource.Id.linearLayout_Settings_BlockedDomain);
        if (linearLayout_Settings_BlockedDomain == null) return view;
        textView_Settings_BlockedDomain = view.FindViewById<TextView>(Resource.Id.textView_Settings_BlockedDomain);
        if (textView_Settings_BlockedDomain == null) return view;

        linearLayout_Settings_TimeoutMS = view.FindViewById<LinearLayout>(Resource.Id.linearLayout_Settings_TimeoutMS);
        if (linearLayout_Settings_TimeoutMS == null) return view;
        textView_Settings_TimeoutMS = view.FindViewById<TextView>(Resource.Id.textView_Settings_TimeoutMS);
        if (textView_Settings_TimeoutMS == null) return view;

        linearLayout_Settings_AllowInsecure = view.FindViewById<LinearLayout>(Resource.Id.linearLayout_Settings_AllowInsecure);
        if (linearLayout_Settings_AllowInsecure == null) return view;
        checkBox_Settings_AllowInsecure = view.FindViewById<CheckBox>(Resource.Id.checkBox_Settings_AllowInsecure);
        if (checkBox_Settings_AllowInsecure == null) return view;

        linearLayout_Settings_DnsProviderUrls = view.FindViewById<LinearLayout>(Resource.Id.linearLayout_Settings_DnsProviderUrls);
        if (linearLayout_Settings_DnsProviderUrls == null) return view;
        textView_Settings_DnsProviderUrls = view.FindViewById<TextView>(Resource.Id.textView_Settings_DnsProviderUrls);
        if (textView_Settings_DnsProviderUrls == null) return view;

        button_Settings_ResetSettingsToDefaults = view.FindViewById<Button>(Resource.Id.button_Settings_ResetSettingsToDefaults);
        if (button_Settings_ResetSettingsToDefaults == null) return view;

        linearLayout_Settings_BlockedDomain.Clickable = true;
        linearLayout_Settings_BlockedDomain.Focusable = true;
        linearLayout_Settings_BlockedDomain.Click -= LinearLayout_Settings_BlockedDomain_Click;
        linearLayout_Settings_BlockedDomain.Click += LinearLayout_Settings_BlockedDomain_Click;
        textView_Settings_BlockedDomain.Text = Storage.ReadSetting(SettingsUniqueNames.Settings_BlockedDomain, Defaults.Settings_BlockedDomain);
        textView_Settings_BlockedDomain.TextChanged -= TextView_Settings_BlockedDomain_TextChanged;
        textView_Settings_BlockedDomain.TextChanged += TextView_Settings_BlockedDomain_TextChanged;

        linearLayout_Settings_TimeoutMS.Clickable = true;
        linearLayout_Settings_TimeoutMS.Focusable = true;
        linearLayout_Settings_TimeoutMS.Click -= LinearLayout_Settings_TimeoutMS_Click;
        linearLayout_Settings_TimeoutMS.Click += LinearLayout_Settings_TimeoutMS_Click;
        textView_Settings_TimeoutMS.Text = Storage.ReadSetting(SettingsUniqueNames.Settings_TimeoutMS, Defaults.Settings_TimeoutMS).ToString();
        textView_Settings_TimeoutMS.TextChanged -= TextView_Settings_TimeoutMS_TextChanged;
        textView_Settings_TimeoutMS.TextChanged += TextView_Settings_TimeoutMS_TextChanged;

        linearLayout_Settings_AllowInsecure.Clickable = true;
        linearLayout_Settings_AllowInsecure.Focusable = true;
        linearLayout_Settings_AllowInsecure.Click -= LinearLayout_Settings_AllowInsecure_Click;
        linearLayout_Settings_AllowInsecure.Click += LinearLayout_Settings_AllowInsecure_Click;
        checkBox_Settings_AllowInsecure.Checked = Storage.ReadSetting(SettingsUniqueNames.Settings_AllowInsecure, false);
        checkBox_Settings_AllowInsecure.CheckedChange -= CheckBox_Settings_AllowInsecure_CheckedChange;
        checkBox_Settings_AllowInsecure.CheckedChange += CheckBox_Settings_AllowInsecure_CheckedChange;

        linearLayout_Settings_DnsProviderUrls.Clickable = true;
        linearLayout_Settings_DnsProviderUrls.Focusable = true;
        linearLayout_Settings_DnsProviderUrls.Click -= LinearLayout_Settings_DnsProviderUrls_Click;
        linearLayout_Settings_DnsProviderUrls.Click += LinearLayout_Settings_DnsProviderUrls_Click;
        textView_Settings_DnsProviderUrls.Text = Storage.ReadSetting(SettingsUniqueNames.Settings_DnsProviderUrls, Defaults.Settings_DnsProviderUrls);
        textView_Settings_DnsProviderUrls.TextChanged -= TextView_Settings_DnsProviderUrls_TextChanged;
        textView_Settings_DnsProviderUrls.TextChanged += TextView_Settings_DnsProviderUrls_TextChanged;

        button_Settings_ResetSettingsToDefaults.Click -= Button_Settings_ResetSettingsToDefaults_Click;
        button_Settings_ResetSettingsToDefaults.Click += Button_Settings_ResetSettingsToDefaults_Click;

        return view;
    }

    private void LinearLayout_Settings_BlockedDomain_Click(object? sender, EventArgs e)
    {
        if (textView_Settings_BlockedDomain == null) return;

        string text = textView_Settings_BlockedDomain.Text ?? string.Empty;
        InputBox inputBox = new(Context, "Enter A Blocked Domain", "e.g. youtube.com", text, 1, 1);
        inputBox.OnResultRecieved += (sender, e) =>
        {
            if (e.IsOK)
            {
                textView_Settings_BlockedDomain.Text = e.Text;
            }
        };
    }

    private void TextView_Settings_BlockedDomain_TextChanged(object? sender, Android.Text.TextChangedEventArgs e)
    {
        if (sender is not TextView textView) return;
        Storage.SaveSetting(SettingsUniqueNames.Settings_BlockedDomain, textView.Text);
    }

    private void LinearLayout_Settings_TimeoutMS_Click(object? sender, EventArgs e)
    {
        if (textView_Settings_TimeoutMS == null) return;

        string text = textView_Settings_TimeoutMS.Text ?? string.Empty;
        InputBox inputBox = new(Context, "Enter Timeout (MS)", "Enter Number Between 100-10000", text, 1, 1);
        inputBox.OnResultRecieved += (sender, e) =>
        {
            if (e.IsOK)
            {
                bool isInt = int.TryParse(e.Text, out int value);
                if (isInt && value >= 100 && value <= 10000)
                {
                    textView_Settings_TimeoutMS.Text = e.Text;
                }
            }
        };
    }

    private void TextView_Settings_TimeoutMS_TextChanged(object? sender, Android.Text.TextChangedEventArgs e)
    {
        if (sender is not TextView textView) return;
        bool isInt = int.TryParse(textView.Text, out int value);
        if (isInt && value >= 100 && value <= 10000)
        {
            Storage.SaveSetting(SettingsUniqueNames.Settings_TimeoutMS, value);
        }
    }

    private void LinearLayout_Settings_AllowInsecure_Click(object? sender, EventArgs e)
    {
        if (checkBox_Settings_AllowInsecure == null) return;

        checkBox_Settings_AllowInsecure.Checked = !checkBox_Settings_AllowInsecure.Checked;
    }

    private void CheckBox_Settings_AllowInsecure_CheckedChange(object? sender, CompoundButton.CheckedChangeEventArgs e)
    {
        if (sender is not CheckBox checkBox) return;
        Storage.SaveSetting(SettingsUniqueNames.Settings_AllowInsecure, checkBox.Checked);
    }

    private void LinearLayout_Settings_DnsProviderUrls_Click(object? sender, EventArgs e)
    {
        if (textView_Settings_DnsProviderUrls == null) return;

        string text = textView_Settings_DnsProviderUrls.Text ?? string.Empty;
        InputBox inputBox = new(Context, "Enter URLs", "Each Line One URL", text, 5, 50);
        inputBox.OnResultRecieved += (sender, e) =>
        {
            if (e.IsOK)
            {
                textView_Settings_DnsProviderUrls.Text = e.Text;
            }
        };
    }

    private void TextView_Settings_DnsProviderUrls_TextChanged(object? sender, Android.Text.TextChangedEventArgs e)
    {
        if (sender is not TextView textView) return;
        Storage.SaveSetting(SettingsUniqueNames.Settings_DnsProviderUrls, textView.Text);
    }

    private void Button_Settings_ResetSettingsToDefaults_Click(object? sender, EventArgs e)
    {
        if (view == null) return;
        if (textView_Settings_BlockedDomain == null) return;
        if (textView_Settings_TimeoutMS == null) return;
        if (checkBox_Settings_AllowInsecure == null) return;
        if (textView_Settings_DnsProviderUrls == null) return;

        Activity?.RunOnUiThread(() =>
        {
            textView_Settings_BlockedDomain.Text = Defaults.Settings_BlockedDomain;
            textView_Settings_TimeoutMS.Text = Defaults.Settings_TimeoutMS.ToString();
            checkBox_Settings_AllowInsecure.Checked = false;
            textView_Settings_DnsProviderUrls.Text = Defaults.Settings_DnsProviderUrls;
        });
    }

}