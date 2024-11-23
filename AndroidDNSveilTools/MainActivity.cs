using Android.Views;
using AndroidX.DrawerLayout.Widget;
using AndroidX.AppCompat.App;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Fragment = AndroidX.Fragment.App.Fragment;
using FragmentTransaction = AndroidX.Fragment.App.FragmentTransaction;
using System.Diagnostics;
using Android.Content.PM;

namespace AndroidDNSveilTools
{
    [Activity(Label = "@string/app_name", ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.Navigation, MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private Toolbar? toolbar;
        private DrawerLayout? drawerLayout;
        private ActionBarDrawerToggle? drawerToggle;
        private ListView? drawerList;
        private string[] drawerItems = [];
        private ArrayAdapter<string>? adapter;

        public static DnsScanner MainDnsScanner { get; set; } = new();

        private long lastBackPressTime = 0;
        private const int ExitTimeInterval = 2000;
        
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar1);
            if (toolbar == null)
            {
                Debug.WriteLine("toolbar Is NULL");
                return;
            }
            SetSupportActionBar(toolbar);

            drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawerLayout1);
            if (drawerLayout == null)
            {
                Debug.WriteLine("drawerLayout Is NULL");
                return;
            }

            drawerLayout.CloseDrawers();

            drawerToggle = new ActionBarDrawerToggle(this, drawerLayout, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close) { };
            if (drawerToggle == null)
            {
                Debug.WriteLine("drawerToggle Is NULL");
                return;
            }

            drawerToggle.DrawerIndicatorEnabled = true; // Show Or Hide Hamburger Icon
            drawerLayout?.AddDrawerListener(drawerToggle);

            // Add A Hamburger Icon
            if (SupportActionBar == null)
            {
                Debug.WriteLine("SupportActionBar Is NULL");
                return;
            }

            SupportActionBar.SetDisplayShowHomeEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(false); // Show Or Hide Back Button
            SupportActionBar.SetHomeButtonEnabled(true);

            // Drawer
            drawerList = FindViewById<ListView>(Resource.Id.left_drawer);
            if (drawerList == null)
            {
                Debug.WriteLine("drawerList Is NULL");
                return;
            }

            // Define Drawer Items
            drawerItems = ["Home", "DNS Lookup", "Settings", "About"];
            adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, drawerItems);
            if (adapter == null)
            {
                Debug.WriteLine("adapter Is NULL");
                return;
            }

            // Set Adapter To drawerList
            drawerList.Adapter = adapter;

            // Handle darwer Item Clicks
            drawerList.ItemClick += DrawerList_ItemClick;
            
            drawerToggle.SyncState(); // Ensure The Hamburger Icon Is Synced

            SupportFragmentManager.BackStackChanged -= SupportFragmentManager_BackStackChanged;
            SupportFragmentManager.BackStackChanged += SupportFragmentManager_BackStackChanged;

            if (savedInstanceState != null) return;
            DisplayFragment(new HomeFragment(), false); // Set Startup Page
        }

        private void DrawerList_ItemClick(object? sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                // Handle Navigation Here
                string selectedItem = drawerItems[e.Position];

                (Fragment? Fragment, string Title, bool AddToBackStack) fragmentInfo()
                {
                    return e.Position switch
                    {
                        0 => (new HomeFragment(), GetString(Resource.String.app_name), false),
                        1 => (new DnsLookupFragment(), GetString(Resource.String.title_DnsLookup), false),
                        2 => (new SettingsFragment(), GetString(Resource.String.title_Settings), true),
                        3 => (new AboutFragment(), GetString(Resource.String.title_About), true),
                        _ => (new HomeFragment(), GetString(Resource.String.app_name), false)
                    };
                }
                var fi = fragmentInfo();
                
                DisplayFragment(fi.Fragment, fi.AddToBackStack);

                drawerLayout?.CloseDrawers();
                drawerToggle?.SyncState();
            }
            catch (Exception ex)
            {
                string text = "mainActivity DrawerList_ItemClick: " + ex.Message;
                Debug.WriteLine(text);
            }
        }

        public override void OnPostCreate(Bundle? savedInstanceState, Android.OS.PersistableBundle? persistentState)
        {
            base.OnPostCreate(savedInstanceState, persistentState);
            drawerToggle?.SyncState(); // Ensure The Hamburger Icon Is Synced
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (drawerToggle != null && drawerToggle.OnOptionsItemSelected(item)) return true;
            return base.OnOptionsItemSelected(item);
        }

        public override void OnBackPressed()
        {
            bool hasBackStack = SupportFragmentManager.BackStackEntryCount > 0;
            if (hasBackStack)
            {
                // Get Back
                OnBackPressedDispatcher.OnBackPressed();
            }
            else
            {
                long currentTime = Java.Lang.JavaSystem.CurrentTimeMillis();
                if (currentTime - lastBackPressTime < ExitTimeInterval)
                {
                    bool forceStop = false; // Google Play Policy Violations If Set To True
                    if (forceStop)
                    {
                        Java.Lang.JavaSystem.Exit(0); // Exits The App
                        Android.OS.Process.KillProcess(Android.OS.Process.MyPid()); // Kills The Process
                    }
                    else
                    {
                        // Exit The App
                        OnBackPressedDispatcher.OnBackPressed();
                    }
                }
                else
                {
                    // Show Toast
                    Toast.MakeText(this, "Press Back Again To Exit.", ToastLength.Short)?.Show();
                    lastBackPressTime = currentTime;
                }
            }
        }

        private void DisplayFragment(Fragment? fragment, bool addToBackStack)
        {
            if (fragment == null) return;
            FragmentTransaction transaction = SupportFragmentManager.BeginTransaction();
            transaction.SetCustomAnimations(Resource.Animation.enter_from_left, Resource.Animation.exit_to_left, Resource.Animation.enter_from_right, Resource.Animation.exit_to_right);
            transaction.Replace(Resource.Id.main_content, fragment);

            if (addToBackStack) transaction.AddToBackStack(null);

            transaction.Commit();
        }

        private void SupportFragmentManager_BackStackChanged(object? sender, EventArgs e)
        {
            if (drawerLayout == null) return;
            if (drawerToggle == null) return;
            if (SupportActionBar == null) return;

            bool hasBackStack = SupportFragmentManager.BackStackEntryCount > 0;
            drawerToggle.DrawerIndicatorEnabled = !hasBackStack;
            SupportActionBar.SetDisplayHomeAsUpEnabled(hasBackStack);
            drawerToggle.SyncState();
            
            if (hasBackStack)
            {
                // Disable Swipe To Open The Drawer
                drawerLayout.SetDrawerLockMode(DrawerLayout.LockModeLockedClosed);

                // Set Back Arrow Click Listener
                drawerToggle.ToolbarNavigationClickListener = new NavigationClickListener(() =>
                {
                    // Handle Back Button Click
                    SupportFragmentManager.PopBackStack();
                });
            }
            else
            {
                // Enable Swipe To Open The Drawer
                drawerLayout.SetDrawerLockMode(DrawerLayout.LockModeUnlocked);

                // Reset To Default Hamburger Icon Behavior
                drawerToggle.ToolbarNavigationClickListener = null;
            }
        }

    }

    public class NavigationClickListener(Action action) : Java.Lang.Object, View.IOnClickListener
    {
        public void OnClick(View? v)
        {
            action?.Invoke();
        }
    }
}