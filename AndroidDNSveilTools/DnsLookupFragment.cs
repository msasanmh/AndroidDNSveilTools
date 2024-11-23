using Android.Graphics;
using Android.Views;
using AndroidX.Core.Content.Resources;
using MsmhToolsClass.MsmhAgnosticServer;
using Fragment = AndroidX.Fragment.App.Fragment;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace AndroidDNSveilTools;

public class DnsLookupFragment : Fragment
{
    private readonly string DnsAddress = string.Empty;
    public DnsLookupFragment() { DnsAddress = string.Empty; }
    public DnsLookupFragment(string dns)
    {
        DnsAddress = dns;
    }

    private View? view;
    private LinearLayout? linearLayout_DnsLookup_MainContainer;

    private LinearLayout? linearLayout_DnsLookup_DnsAddress;
    private TextView? textView_DnsLookup_DnsAddress;

    private LinearLayout? linearLayout_DnsLookup_Domain;
    private TextView? textView_DnsLookup_Domain;

    private Spinner? spinner_DnsLookup_RRTYPE;
    private CheckBox? checkBox_DnsLookup_AllowInsecure;

    private Button? button_DnsLookup_Lookup;

    private TextView? textView_DnsLookup_Info;

    public override View? OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState)
    {
        Toolbar? toolbarMain = Activity?.FindViewById<Toolbar>(Resource.Id.toolbar1);
        if (toolbarMain != null) toolbarMain.Title = Resources.GetString(Resource.String.title_DnsLookup);

        view = inflater.Inflate(Resource.Layout.fragment_DnsLookup, container, false);
        if (view == null) return view;
        if (Context == null) return view;
        if (savedInstanceState != null) return view;

        linearLayout_DnsLookup_MainContainer = view.FindViewById<LinearLayout>(Resource.Id.linearLayout_DnsLookup_MainContainer);
        if (linearLayout_DnsLookup_MainContainer == null) return view;

        linearLayout_DnsLookup_DnsAddress = view.FindViewById<LinearLayout>(Resource.Id.linearLayout_DnsLookup_DnsAddress);
        if (linearLayout_DnsLookup_DnsAddress == null) return view;
        textView_DnsLookup_DnsAddress = view.FindViewById<TextView>(Resource.Id.textView_DnsLookup_DnsAddress);
        if (textView_DnsLookup_DnsAddress == null) return view;

        linearLayout_DnsLookup_Domain = view.FindViewById<LinearLayout>(Resource.Id.linearLayout_DnsLookup_Domain);
        if (linearLayout_DnsLookup_Domain == null) return view;
        textView_DnsLookup_Domain = view.FindViewById<TextView>(Resource.Id.textView_DnsLookup_Domain);
        if (textView_DnsLookup_Domain == null) return view;

        spinner_DnsLookup_RRTYPE = view.FindViewById<Spinner>(Resource.Id.spinner_DnsLookup_RRTYPE);
        if (spinner_DnsLookup_RRTYPE == null) return view;
        checkBox_DnsLookup_AllowInsecure = view.FindViewById<CheckBox>(Resource.Id.checkBox_DnsLookup_AllowInsecure);
        if (checkBox_DnsLookup_AllowInsecure == null) return view;

        button_DnsLookup_Lookup = view.FindViewById<Button>(Resource.Id.button_DnsLookup_Lookup);
        if (button_DnsLookup_Lookup == null) return view;

        textView_DnsLookup_Info = view.FindViewById<TextView>(Resource.Id.textView_DnsLookup_Info);
        if (textView_DnsLookup_Info == null) return view;

        Color bgColor = new(ResourcesCompat.GetColor(Resources, Resource.Color.msmh_Dark, Context.Theme));
        linearLayout_DnsLookup_MainContainer.SetBackgroundColor(bgColor);

        linearLayout_DnsLookup_DnsAddress.Clickable = true;
        linearLayout_DnsLookup_DnsAddress.Focusable = true;
        linearLayout_DnsLookup_DnsAddress.Click -= LinearLayout_DnsLookup_DnsAddress_Click;
        linearLayout_DnsLookup_DnsAddress.Click += LinearLayout_DnsLookup_DnsAddress_Click;
        textView_DnsLookup_DnsAddress.Text = Storage.ReadSetting(SettingsUniqueNames.DnsLookup_DnsAddress, Defaults.DnsLookup_DnsAddress);
        textView_DnsLookup_DnsAddress.TextChanged -= TextView_DnsLookup_DnsAddress_TextChanged;
        textView_DnsLookup_DnsAddress.TextChanged += TextView_DnsLookup_DnsAddress_TextChanged;

        linearLayout_DnsLookup_Domain.Clickable = true;
        linearLayout_DnsLookup_Domain.Focusable = true;
        linearLayout_DnsLookup_Domain.Click -= LinearLayout_DnsLookup_Domain_Click;
        linearLayout_DnsLookup_Domain.Click += LinearLayout_DnsLookup_Domain_Click;
        textView_DnsLookup_Domain.Text = Storage.ReadSetting(SettingsUniqueNames.DnsLookup_Domain, Defaults.DnsLookup_Domain);
        textView_DnsLookup_Domain.TextChanged -= TextView_DnsLookup_Domain_TextChanged;
        textView_DnsLookup_Domain.TextChanged += TextView_DnsLookup_Domain_TextChanged;

        List<string> rrTypes = ["A", "AAAA", "CNAME", "MX", "NS", "SOA", "TEXT"];
        ArrayAdapter<string> adapter = new(Context, Android.Resource.Layout.SimpleSpinnerItem, rrTypes);
        adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
        spinner_DnsLookup_RRTYPE.Adapter = adapter;
        spinner_DnsLookup_RRTYPE.SetSelection(Storage.ReadSetting(SettingsUniqueNames.DnsLookup_RRTYPE, Defaults.DnsLookup_RRTYPE));
        spinner_DnsLookup_RRTYPE.ItemSelected -= Spinner_DnsLookup_RRTYPE_ItemSelected;
        spinner_DnsLookup_RRTYPE.ItemSelected += Spinner_DnsLookup_RRTYPE_ItemSelected;

        checkBox_DnsLookup_AllowInsecure.Checked = Storage.ReadSetting(SettingsUniqueNames.DnsLookup_AllowInsecure, false);
        checkBox_DnsLookup_AllowInsecure.CheckedChange -= CheckBox_DnsLookup_AllowInsecure_CheckedChange;
        checkBox_DnsLookup_AllowInsecure.CheckedChange += CheckBox_DnsLookup_AllowInsecure_CheckedChange;

        button_DnsLookup_Lookup.Text = "Lookup";
        button_DnsLookup_Lookup.Click -= Button_DnsLookup_Lookup_Click;
        button_DnsLookup_Lookup.Click += Button_DnsLookup_Lookup_Click;

        // Make Log Selectable Without Showing Keyboard
        textView_DnsLookup_Info.SetTextIsSelectable(true);
        textView_DnsLookup_Info.Focusable = true;
        textView_DnsLookup_Info.SetCursorVisible(false);
        textView_DnsLookup_Info.ShowSoftInputOnFocus = false;

        if (!string.IsNullOrWhiteSpace(DnsAddress))
        {
            //linearLayout_DnsLookup_MainContainer.SetBackgroundColor(Color.DodgerBlue);
            textView_DnsLookup_DnsAddress.Text = DnsAddress;
            textView_DnsLookup_Domain.Text = Storage.ReadSetting(SettingsUniqueNames.Settings_BlockedDomain, Defaults.Settings_BlockedDomain);
            spinner_DnsLookup_RRTYPE.SetSelection(0); // A Record
            checkBox_DnsLookup_AllowInsecure.Checked = Storage.ReadSetting(SettingsUniqueNames.Settings_AllowInsecure, false);
            Button_DnsLookup_Lookup_Click(null, EventArgs.Empty);
        }

        return view;
    }

    private void LinearLayout_DnsLookup_DnsAddress_Click(object? sender, EventArgs e)
    {
        if (textView_DnsLookup_DnsAddress == null) return;

        string text = textView_DnsLookup_DnsAddress.Text ?? string.Empty;
        InputBox inputBox = new(Context, "Enter DNS Address", string.Empty, text, 1, 1);
        inputBox.OnResultRecieved += (sender, e) =>
        {
            if (e.IsOK)
            {
                textView_DnsLookup_DnsAddress.Text = e.Text;
            }
        };
    }

    private void TextView_DnsLookup_DnsAddress_TextChanged(object? sender, Android.Text.TextChangedEventArgs e)
    {
        if (sender is not TextView textView) return;
        Storage.SaveSetting(SettingsUniqueNames.DnsLookup_DnsAddress, textView.Text);
    }

    private void LinearLayout_DnsLookup_Domain_Click(object? sender, EventArgs e)
    {
        if (textView_DnsLookup_Domain == null) return;

        string text = textView_DnsLookup_Domain.Text ?? string.Empty;
        InputBox inputBox = new(Context, "Enter A Domain", string.Empty, text, 1, 1);
        inputBox.OnResultRecieved += (sender, e) =>
        {
            if (e.IsOK)
            {
                textView_DnsLookup_Domain.Text = e.Text;
            }
        };
    }

    private void TextView_DnsLookup_Domain_TextChanged(object? sender, Android.Text.TextChangedEventArgs e)
    {
        if (sender is not TextView textView) return;
        Storage.SaveSetting(SettingsUniqueNames.DnsLookup_Domain, textView.Text);
    }

    private void Spinner_DnsLookup_RRTYPE_ItemSelected(object? sender, AdapterView.ItemSelectedEventArgs e)
    {
        if (sender is not Spinner) return;
        Storage.SaveSetting(SettingsUniqueNames.DnsLookup_RRTYPE, e.Position);
    }

    private void CheckBox_DnsLookup_AllowInsecure_CheckedChange(object? sender, CompoundButton.CheckedChangeEventArgs e)
    {
        if (sender is not CheckBox checkBox) return;
        Storage.SaveSetting(SettingsUniqueNames.DnsLookup_AllowInsecure, checkBox.Checked);
    }

    private async void Button_DnsLookup_Lookup_Click(object? sender, EventArgs e)
    {
        if (view == null) return;
        if (textView_DnsLookup_DnsAddress == null) return;
        if (textView_DnsLookup_DnsAddress.Text == null) return;
        if (textView_DnsLookup_Domain == null) return;
        if (textView_DnsLookup_Domain.Text == null) return;
        if (spinner_DnsLookup_RRTYPE == null) return;
        if (checkBox_DnsLookup_AllowInsecure == null) return;
        if (button_DnsLookup_Lookup ==  null) return;
        if (textView_DnsLookup_Info == null) return;

        Activity?.RunOnUiThread(() =>
        {
            button_DnsLookup_Lookup.Enabled = false;
            button_DnsLookup_Lookup.Text = "Looking Up...";
            textView_DnsLookup_Info.Text = string.Empty;
        });
        
        string dns = textView_DnsLookup_DnsAddress.Text;
        string domain = textView_DnsLookup_Domain.Text;
        string rrTypeStr = (string?)spinner_DnsLookup_RRTYPE.SelectedItem ?? nameof(DnsEnums.RRType.A);
        DnsEnums.RRType rrType = DnsEnums.ParseRRType(rrTypeStr);
        bool allowInsecure = checkBox_DnsLookup_AllowInsecure.Checked;

        DnsReader reader = new(dns);
        string nl = Environment.NewLine;
        string result = $"Lookup Result For {dns}{nl}";
        result += $"Protocol: {reader.ProtocolName}{nl}";

        if (reader.Protocol == DnsEnums.DnsProtocol.AnonymizedDNSCrypt ||
            reader.Protocol == DnsEnums.DnsProtocol.DnsCrypt ||
            reader.Protocol == DnsEnums.DnsProtocol.DoH ||
            reader.Protocol == DnsEnums.DnsProtocol.DoT ||
            reader.Protocol == DnsEnums.DnsProtocol.ObliviousDohTarget ||
            reader.Protocol == DnsEnums.DnsProtocol.System ||
            reader.Protocol == DnsEnums.DnsProtocol.TCP ||
            reader.Protocol == DnsEnums.DnsProtocol.UDP)
        {
            CheckDns checkDns = new(allowInsecure, false);
            CheckDns.CheckDnsResult cdr = await checkDns.CheckDnsAsync(domain, dns, 10000, rrType);

            result += $"Is Online: {cdr.IsDnsOnline}{nl}";
            result += $"Latency (ms): {cdr.DnsLatency}{nl}{nl}";
            result += cdr.DnsMessage.Header.ToString() + nl + nl;
            result += cdr.DnsMessage.Answers.ToString() + nl + nl;
            if (cdr.DnsMessage.Additionals.AdditionalRecords.Count > 0)
                result += nl + cdr.DnsMessage.Additionals.ToString() + nl;
            if (cdr.DnsMessage.Authorities.AuthorityRecords.Count > 0)
                result += nl + cdr.DnsMessage.Authorities.ToString() + nl;
        }
        else
        {
            result += $"{nl}NOT SUPPORTED: {reader.ProtocolName} Protocol.{nl}";
        }

        Activity?.RunOnUiThread(() =>
        {
            textView_DnsLookup_Info.Text = result;
            button_DnsLookup_Lookup.Text = "Lookup";
            button_DnsLookup_Lookup.Enabled = true;
        });
    }

    public class DisableOnTouchListener() : Java.Lang.Object, View.IOnTouchListener
    {
        public bool OnTouch(View? v, MotionEvent? e)
        {
            return true;
        }
    }

}