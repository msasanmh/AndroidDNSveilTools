using Android.Content;
using Android.Graphics;
using Android.Views;
using static AndroidDNSveilTools.DnsScanner;
using Fragment = AndroidX.Fragment.App.Fragment;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using FragmentTransaction = AndroidX.Fragment.App.FragmentTransaction;

namespace AndroidDNSveilTools;

public class HomeFragment : Fragment
{
    private View? view;
    private Button? button_HomeScan;
    private TextView? textView_HomeStatus;
    private ScrollView? scrollView_HomeInfo;
    private LinearLayout? linearLayout_HomeInfo;

    public override View? OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState)
    {
        Toolbar? toolbarMain = Activity?.FindViewById<Toolbar>(Resource.Id.toolbar1);
        if (toolbarMain != null) toolbarMain.Title = Resources.GetString(Resource.String.app_name);

        view = inflater.Inflate(Resource.Layout.fragment_home, container, false);
        if (view == null) return view;
        //if (savedInstanceState != null) return view;
        //SharedVM? sharedVM = new ViewModelProvider(RequireActivity()).Get(Java.Lang.Class.FromType(typeof(SharedVM))) as SharedVM;

        button_HomeScan = view.FindViewById<Button>(Resource.Id.button_HomeScan);
        if (button_HomeScan == null) return view;
        textView_HomeStatus = view.FindViewById<TextView>(Resource.Id.textView_HomeStatus);
        if (textView_HomeStatus == null) return view;
        scrollView_HomeInfo = view.FindViewById<ScrollView>(Resource.Id.scrollView_HomeInfo);
        if (scrollView_HomeInfo == null) return view;
        linearLayout_HomeInfo = view.FindViewById<LinearLayout>(Resource.Id.linearLayout_HomeInfo);
        if (linearLayout_HomeInfo == null) return view;
        
        button_HomeScan.Enabled = true;
        button_HomeScan.Text = MainActivity.MainDnsScanner.IsCancelling ? "Stopping..." : MainActivity.MainDnsScanner.IsRunning ? "Stop" : "Scan";
        button_HomeScan.Click -= Button_HomeScan_Click;
        button_HomeScan.Click += Button_HomeScan_Click;

        MainActivity.MainDnsScanner.OnLogRecieved -= MainDnsScanner_OnLogRecieved;
        MainActivity.MainDnsScanner.OnLogRecieved += MainDnsScanner_OnLogRecieved;
        MainActivity.MainDnsScanner.OnStatusChanged -= MainDnsScanner_OnStatusChanged;
        MainActivity.MainDnsScanner.OnStatusChanged += MainDnsScanner_OnStatusChanged;
        MainActivity.MainDnsScanner.OnWorkingServerRecieved -= MainDnsScanner_OnWorkingServerRecieved;
        MainActivity.MainDnsScanner.OnWorkingServerRecieved += MainDnsScanner_OnWorkingServerRecieved;
        MainActivity.MainDnsScanner.OnScanFinished -= MainDnsScanner_OnScanFinished;
        MainActivity.MainDnsScanner.OnScanFinished += MainDnsScanner_OnScanFinished;

        if (!MainActivity.MainDnsScanner.IsRunning) LoadSavedDNSs();

        return view;
    }

    private void MainDnsScanner_OnLogRecieved(object? sender, LogEventArgs e)
    {
        if (view == null) return;
        if (scrollView_HomeInfo == null) return;
        if (linearLayout_HomeInfo == null) return;

        AddLog(scrollView_HomeInfo, linearLayout_HomeInfo, e.Message, e.Color, true);
    }

    private void MainDnsScanner_OnStatusChanged(object? sender, EventArgs e)
    {
        if (sender is not string msg) return;
        if (view == null) return;
        if (button_HomeScan == null) return;
        if (textView_HomeStatus == null) return;

        Activity?.RunOnUiThread(() =>
        {
            textView_HomeStatus.Text = msg;
            button_HomeScan.Text = MainActivity.MainDnsScanner.IsCancelling ? "Stopping..." : MainActivity.MainDnsScanner.IsRunning ? "Stop" : "Scan";
        });
    }

    private void MainDnsScanner_OnWorkingServerRecieved(object? sender, WorkingServerEventArgs e)
    {
        Storage.SaveDnsA(e.DNS, e.Delay.ToString());
    }

    private void MainDnsScanner_OnScanFinished(object? sender, ResultEventArgs e)
    {
        if (view == null) return;
        if (button_HomeScan == null) return;
        if (scrollView_HomeInfo == null) return;

        LoadSavedDNSs();

        Activity?.RunOnUiThread(() =>
        {
            button_HomeScan.Text = MainActivity.MainDnsScanner.IsCancelling ? "Stopping..." : MainActivity.MainDnsScanner.IsRunning ? "Stop" : "Scan";
            scrollView_HomeInfo.Post(() => scrollView_HomeInfo.FullScroll(FocusSearchDirection.Up));
        });
    }

    private void LoadSavedDNSs()
    {
        if (view == null) return;
        if (textView_HomeStatus == null) return;
        if (scrollView_HomeInfo == null) return;
        if (linearLayout_HomeInfo == null) return;

        linearLayout_HomeInfo.RemoveAllViews();
        Dictionary<string, int> savedDNSs = Storage.ReadDnsA();
        //Toast.MakeText(Application.Context, $"Saved DNSs: {savedDNSs.Count}", ToastLength.Short)?.Show();

        Activity?.RunOnUiThread(() => textView_HomeStatus.Text = $"Working Servers: {savedDNSs.Count}");

        if (savedDNSs.Count > 0)
        {
            AddLog(scrollView_HomeInfo, linearLayout_HomeInfo, "Sorted By Latency:", Color.Gray, false);
            foreach (var kvp in savedDNSs)
            {
                AddResult(linearLayout_HomeInfo, $"{kvp.Value} ms", kvp.Key);
            }
        }
    }

    private async void Button_HomeScan_Click(object? sender, EventArgs? e)
    {
        if (sender is not Button button) return;
        if (view == null) return;
        if (scrollView_HomeInfo == null) return;
        if (linearLayout_HomeInfo == null) return;
        linearLayout_HomeInfo.RemoveAllViews();
        button.Enabled = false;

        if (!MainActivity.MainDnsScanner.IsRunning)
        {
            // Start
            await Storage.ClearDnsA_Async(); // Clear Saved Servers
            button.Text = "Stop";
            button.Enabled = true;
            MainActivity.MainDnsScanner.Start();
        }
        else
        {
            // Stop
            button.Text = "Stopping...";
            MainActivity.MainDnsScanner.Stop();

            await Task.Run(async () =>
            {
                while (true)
                {
                    if (!MainActivity.MainDnsScanner.IsRunning) break;
                    await Task.Delay(100);
                }
            });

            button.Text = "Scan";
            button.Enabled = true;
        }
    }

    private void AddLog(ScrollView scrollView, LinearLayout linearLayout, string text, Color color, bool goToEnd)
    {
        Activity?.RunOnUiThread(() =>
        {
            LinearLayout layout = new(linearLayout.Context);
            layout.Orientation = Orientation.Horizontal;
            layout.SetGravity(GravityFlags.CenterVertical);
            layout.LayoutParameters = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent);
            layout.SetPadding(0, 0, 0, 0);

            TextView textViewKey = new(layout.Context);
            textViewKey.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            textViewKey.SetTypeface(null, Android.Graphics.TypefaceStyle.Normal);
            textViewKey.SetPadding(0, 0, 0, 0);
            textViewKey.Text = text;
            textViewKey.SetTextColor(color);
            layout.AddView(textViewKey);

            linearLayout.AddView(layout);

            if (goToEnd) scrollView.Post(() => scrollView.FullScroll(FocusSearchDirection.Down));
        });
    }

    private void AddResult(LinearLayout linearLayout, string delay,  string dns)
    {
        Activity?.RunOnUiThread(() =>
        {
            LinearLayout layout = new(linearLayout.Context);
            layout.Orientation = Orientation.Horizontal;
            layout.SetGravity(GravityFlags.CenterVertical);
            layout.LayoutParameters = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent);
            AddClickableBackground(layout);
            layout.SetPadding(0, 20, 0, 20);
            layout.Click += (sender, args) =>
            {
                PopupMenu pm = new(linearLayout.Context, layout, GravityFlags.CenterHorizontal | GravityFlags.CenterVertical);
                pm.Menu?.Add(1, 1, 1, "Copy DNS");
                pm.Menu?.Add(1, 2, 2, "Lookup");
                pm.MenuItemClick += (s, e) =>
                {
                    if (e.Item?.ItemId == 1)
                    {
                        ClipboardManager? clipboard = (ClipboardManager?)Context?.GetSystemService(Context.ClipboardService);
                        if (clipboard != null) clipboard.Text = dns;
                    }
                    else if (e.Item?.ItemId == 2)
                    {
                        DnsLookupFragment popupFragment = new(dns);
                        FragmentTransaction transaction = Activity.SupportFragmentManager.BeginTransaction();
                        transaction.SetCustomAnimations(Resource.Animation.enter_from_bottom, Resource.Animation.exit_to_bottom, Resource.Animation.enter_from_bottom, Resource.Animation.exit_to_bottom);
                        transaction.Add(Resource.Id.main_content, popupFragment, "PopupFragment");
                        transaction.AddToBackStack(null);
                        transaction.Commit();
                    }
                };
                pm.Show();
            };

            TextView textViewKey = new(layout.Context);
            textViewKey.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            textViewKey.SetTypeface(null, Android.Graphics.TypefaceStyle.Bold);
            textViewKey.SetPadding(0, 0, 30, 0);
            textViewKey.Text = delay;
            layout.AddView(textViewKey);

            TextView textViewValue = new(layout.Context);
            textViewValue.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            textViewValue.Text = dns;
            textViewValue.SetTextColor(Color.MediumSeaGreen);
            layout.AddView(textViewValue);

            linearLayout.AddView(layout);
        });
    }

    private void AddClickableBackground(View view)
    {
        view.Clickable = true;
        view.Focusable = true;
        view.Background = Resources.GetDrawable(Resource.Drawable.bg_Rounded_NoPad, Application.Context.Theme);
    }

}