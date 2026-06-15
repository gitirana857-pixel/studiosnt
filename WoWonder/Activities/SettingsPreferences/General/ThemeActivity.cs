using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Content.Res;
using System;
using WoWonder.Activities.Base;
using WoWonder.Activities.Tabbes;
using WoWonder.Helpers.MediaPlayerController;
using WoWonder.Helpers.Utils;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.SettingsPreferences.General
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class ThemeActivity : BaseActivity
    {
        private TextView RbLight, RbDark, RbBattery;
        private string CurrentThemeMode;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.ThemeLayout);

                CurrentThemeMode = MainSettings.SharedData?.GetString("Night_Mode_key", string.Empty);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                AddOrRemoveEvent(true);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnPause()
        {
            try
            {
                base.OnPause();
                AddOrRemoveEvent(false);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnDestroy()
        {
            try
            {
                DestroyBasic();
                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        private void InitComponent()
        {
            try
            {
                RbLight = FindViewById<TextView>(Resource.Id.rbLight);
                RbDark = FindViewById<TextView>(Resource.Id.rbDark);
                RbBattery = FindViewById<TextView>(Resource.Id.rbBatterySaver);

                RbBattery.Visibility = ViewStates.Gone;

                switch ((int)Build.VERSION.SdkInt)
                {
                    case >= 29:
                        RbBattery.Visibility = ViewStates.Visible;
                        break;
                }

                if (CurrentThemeMode == MainSettings.LightMode)
                {
                    RbLight.SetCompoundDrawablesRelativeWithIntrinsicBounds(0, 0, Resource.Drawable.ic_circle_check, 0);
                    RbDark.SetCompoundDrawablesRelativeWithIntrinsicBounds(0, 0, 0, 0);
                    RbBattery.SetCompoundDrawablesRelativeWithIntrinsicBounds(0, 0, 0, 0);
                }
                else if (CurrentThemeMode == MainSettings.DarkMode)
                {
                    RbDark.SetCompoundDrawablesRelativeWithIntrinsicBounds(0, 0, Resource.Drawable.ic_circle_check, 0);
                    RbLight.SetCompoundDrawablesRelativeWithIntrinsicBounds(0, 0, 0, 0);
                    RbBattery.SetCompoundDrawablesRelativeWithIntrinsicBounds(0, 0, 0, 0);
                }
                else if (CurrentThemeMode == MainSettings.DefaultMode)
                {
                    RbBattery.SetCompoundDrawablesRelativeWithIntrinsicBounds(0, 0, Resource.Drawable.ic_circle_check, 0);
                    RbLight.SetCompoundDrawablesRelativeWithIntrinsicBounds(0, 0, 0, 0);
                    RbDark.SetCompoundDrawablesRelativeWithIntrinsicBounds(0, 0, 0, 0);
                }
                else
                {
                    RbLight.SetCompoundDrawablesRelativeWithIntrinsicBounds(0, 0, Resource.Drawable.ic_circle_check, 0);
                    RbDark.SetCompoundDrawablesRelativeWithIntrinsicBounds(0, 0, 0, 0);
                    RbBattery.SetCompoundDrawablesRelativeWithIntrinsicBounds(0, 0, 0, 0);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitToolbar()
        {
            try
            {
                var toolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolBar != null)
                {
                    toolBar.Title = GetString(Resource.String.Lbl_Theme);
                    toolBar.SetTitleTextColor(WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                    SetSupportActionBar(toolBar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
                    var icon = AppCompatResources.GetDrawable(this, AppSettings.FlowDirectionRightToLeft ? Resource.Drawable.icon_back_arrow_right : Resource.Drawable.icon_back_arrow_left);
                    icon?.SetTint(WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                    SupportActionBar.SetHomeAsUpIndicator(icon);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                switch (addEvent)
                {
                    // true +=  // false -=
                    case true:
                        RbLight.Click += SetLightModeClick;
                        RbDark.Click += SetDarkModeClick;
                        RbBattery.Click += SetBateryModeClick;
                        break;
                    default:
                        RbLight.Click -= SetLightModeClick;
                        RbDark.Click -= SetDarkModeClick;
                        RbBattery.Click -= SetBateryModeClick;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetBateryModeClick(object sender, EventArgs e)
        {
            try
            {
                if (CurrentThemeMode != MainSettings.DefaultMode)
                {
                    Constant.IsChangingTheme = true;
                    MainSettings.SharedData?.Edit()?.PutString("Night_Mode_key", MainSettings.DefaultMode)?.Commit();

                    switch ((int)Build.VERSION.SdkInt)
                    {
                        case >= 29:
                            {
                                AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightFollowSystem;

                                var currentNightMode = Resources?.Configuration?.UiMode & UiMode.NightMask;
                                switch (currentNightMode)
                                {
                                    case UiMode.NightNo:
                                        // Night mode is not active, we're using the light theme
                                        MainSettings.ApplyTheme(MainSettings.LightMode);
                                        break;
                                    case UiMode.NightYes:
                                        // Night mode is active, we're using dark theme
                                        MainSettings.ApplyTheme(MainSettings.DarkMode);
                                        break;
                                }

                                break;
                            }
                        default:
                            {
                                AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightAutoBattery;

                                var currentNightMode = Resources?.Configuration?.UiMode & UiMode.NightMask;
                                switch (currentNightMode)
                                {
                                    case UiMode.NightNo:
                                        // Night mode is not active, we're using the light theme
                                        MainSettings.ApplyTheme(MainSettings.LightMode);
                                        break;
                                    case UiMode.NightYes:
                                        // Night mode is active, we're using dark theme
                                        MainSettings.ApplyTheme(MainSettings.DarkMode);
                                        break;
                                }

                                switch (Build.VERSION.SdkInt)
                                {
                                    case >= BuildVersionCodes.Lollipop:
                                        Window?.ClearFlags(WindowManagerFlags.TranslucentStatus);
                                        Window?.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                                        break;
                                }

                                Intent intent = new Intent(this, typeof(TabbedMainActivity));
                                intent.AddCategory(Intent.CategoryHome);
                                intent.SetAction(Intent.ActionMain);
                                intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
                                intent.AddFlags(ActivityFlags.NoAnimation);
                                FinishAffinity();
                                if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
                                {
                                    ActivityOptions options = ActivityOptions.MakeCustomAnimation(this, 0, 0);
                                    StartActivity(intent, options?.ToBundle());
                                }
                                else
                                {
                                    OverridePendingTransition(0, 0);
                                    StartActivity(intent);
                                }
                                break;
                            }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SetDarkModeClick(object sender, EventArgs e)
        {
            try
            {
                if (CurrentThemeMode != MainSettings.DarkMode)
                {
                    Constant.IsChangingTheme = true;
                    MainSettings.ApplyTheme(MainSettings.DarkMode);
                    MainSettings.SharedData?.Edit()?.PutString("Night_Mode_key", MainSettings.DarkMode)?.Commit();

                    switch (Build.VERSION.SdkInt)
                    {
                        case >= BuildVersionCodes.Lollipop:
                            Window?.ClearFlags(WindowManagerFlags.TranslucentStatus);
                            Window?.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                            break;
                    }

                    Intent intent = new Intent(this, typeof(TabbedMainActivity));
                    intent.AddCategory(Intent.CategoryHome);
                    intent.SetAction(Intent.ActionMain);
                    intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
                    intent.AddFlags(ActivityFlags.NoAnimation);
                    FinishAffinity();

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
                    {
                        ActivityOptions options = ActivityOptions.MakeCustomAnimation(this, 0, 0);
                        StartActivity(intent, options?.ToBundle());
                    }
                    else
                    {
                        OverridePendingTransition(0, 0);
                        StartActivity(intent);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void DestroyBasic()
        {
            try
            {
                RbLight = null!;
                RbDark = null!;
                RbBattery = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetLightModeClick(object sender, EventArgs e)
        {
            try
            {
                if (CurrentThemeMode != MainSettings.LightMode)
                {
                    Constant.IsChangingTheme = true;
                    //Set Light Mode   
                    MainSettings.ApplyTheme(MainSettings.LightMode);
                    MainSettings.SharedData?.Edit()?.PutString("Night_Mode_key", MainSettings.LightMode)?.Commit();

                    switch (Build.VERSION.SdkInt)
                    {
                        case >= BuildVersionCodes.Lollipop:
                            Window?.ClearFlags(WindowManagerFlags.TranslucentStatus);
                            Window?.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                            break;
                    }

                    Intent intent = new Intent(this, typeof(TabbedMainActivity));
                    intent.AddCategory(Intent.CategoryHome);
                    intent.SetAction(Intent.ActionMain);
                    intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
                    intent.AddFlags(ActivityFlags.NoAnimation);
                    FinishAffinity();
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
                    {
                        ActivityOptions options = ActivityOptions.MakeCustomAnimation(this, 0, 0);
                        StartActivity(intent, options?.ToBundle());
                    }
                    else
                    {
                        OverridePendingTransition(0, 0);
                        StartActivity(intent);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

    }
}