using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Content.Res;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Content;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Com.Google.Android.Gms.Ads.Admanager;
using ImageViews.Rounded;
using Newtonsoft.Json;
using System;
using System.Linq;
using WoWonder.Activities.Base;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Event;
using WoWonderClient.Requests;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Events
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class CreateEventActivity : BaseActivity, View.IOnClickListener, View.IOnFocusChangeListener
    {
        #region Variables Basic

        private EditText TxtEventName, TxtStartDate, TxtStartTime, TxtEndDate, TxtEndTime, TxtLocation, TxtDescription;
        private string EventPathImage = "";
        private AdManagerAdView AdManagerAdView;
        private LinearLayout llStep1, llStep2, llStep3, rlStep4, llStep5, llStep6;
        private AppCompatButton BtnSave;
        private RelativeLayout SelectImageView;
        private RoundedImageView RivImageEvent;
        private ProgressBar ViewStep;
        private TextView TxtStep;
        private int NStep = 1;
        private readonly int MaxStep = 6;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.CreateEventLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                InitBackPressed("CreateEventActivity");

                SetStep();
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
                AdsGoogle.LifecycleAdManagerAdView(AdManagerAdView, "Resume");
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
                AdsGoogle.LifecycleAdManagerAdView(AdManagerAdView, "Pause");
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

        #endregion

        #region Menu

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

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                ViewStep = FindViewById<ProgressBar>(Resource.Id.view_step);

                TxtStep = FindViewById<TextView>(Resource.Id.tv_step);

                llStep1 = FindViewById<LinearLayout>(Resource.Id.ll_step1);
                llStep2 = FindViewById<LinearLayout>(Resource.Id.ll_step2);
                llStep3 = FindViewById<LinearLayout>(Resource.Id.ll_step3);
                llStep5 = FindViewById<LinearLayout>(Resource.Id.ll_step5);
                llStep6 = FindViewById<LinearLayout>(Resource.Id.ll_step6);
                rlStep4 = FindViewById<LinearLayout>(Resource.Id.rl_step4);

                BtnSave = FindViewById<AppCompatButton>(Resource.Id.btn_next);

                TxtEventName = FindViewById<EditText>(Resource.Id.eventname);
                TxtStartDate = FindViewById<EditText>(Resource.Id.StartDateTextview);
                TxtStartTime = FindViewById<EditText>(Resource.Id.StartTimeTextview);
                TxtEndDate = FindViewById<EditText>(Resource.Id.EndDateTextview);
                TxtEndTime = FindViewById<EditText>(Resource.Id.EndTimeTextview);
                TxtLocation = FindViewById<EditText>(Resource.Id.LocationTextview);
                TxtDescription = FindViewById<EditText>(Resource.Id.description);

                SelectImageView = FindViewById<RelativeLayout>(Resource.Id.SelectImageView);
                RivImageEvent = FindViewById<RoundedImageView>(Resource.Id.fundingCover);

                Methods.SetColorEditText(TxtEventName, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtStartTime, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtStartDate, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtEndDate, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtEndTime, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtLocation, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtDescription, WoWonderTools.IsTabDark() ? Color.White : Color.Black);

                Methods.SetFocusable(TxtStartTime);
                Methods.SetFocusable(TxtEndTime);
                Methods.SetFocusable(TxtStartDate);
                Methods.SetFocusable(TxtEndDate);

                TxtStartTime.SetOnClickListener(this);
                TxtEndTime.SetOnClickListener(this);
                TxtStartDate.SetOnClickListener(this);
                TxtEndDate.SetOnClickListener(this);
                TxtLocation.ClearFocus();

                AdManagerAdView = FindViewById<AdManagerAdView>(Resource.Id.multiple_ad_sizes_view);
                AdsGoogle.InitAdManagerAdView(AdManagerAdView);
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
                    toolBar.Title = GetText(Resource.String.Lbl_Create_Events);
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
                        BtnSave.Click += TxtAddOnClick;
                        TxtLocation.OnFocusChangeListener = this;
                        SelectImageView.Click += BtnImageOnClick;
                        break;
                    default:
                        BtnSave.Click -= TxtAddOnClick;
                        TxtLocation.OnFocusChangeListener = null!;
                        SelectImageView.Click -= BtnImageOnClick;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void DestroyBasic()
        {
            try
            {
                AdsGoogle.LifecycleAdManagerAdView(AdManagerAdView, "Destroy");

                TxtEventName = null!;
                TxtStartDate = null!;
                TxtStartTime = null!;
                TxtEndDate = null!;
                TxtEndTime = null!;
                TxtLocation = null!;
                TxtDescription = null!;
                RivImageEvent = null!;
                SelectImageView = null!;
                BtnSave = null!;
                EventPathImage = null!;

                AdManagerAdView = null!;

                llStep1 = null!;
                llStep2 = null!;
                llStep3 = null!;
                llStep5 = null!;
                llStep6 = null!;
                rlStep4 = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        #endregion

        #region Events

        private void BtnImageOnClick(object sender, EventArgs e)
        {
            try
            {
                PixImagePickerUtils.OpenDialogGallery(this);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtLocationOnFocusChange()
        {
            try
            {
                switch ((int)Build.VERSION.SdkInt)
                {
                    // Check if we're running on Android 5.0 or higher
                    case < 23:
                        //Open intent Location when the request code of result is 502
                        new IntentController(this).OpenIntentLocation();
                        break;
                    default:
                        {
                            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) == Permission.Granted && ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) == Permission.Granted)
                            {
                                //Open intent Location when the request code of result is 502
                                new IntentController(this).OpenIntentLocation();
                            }
                            else
                            {
                                new PermissionsController(this).RequestPermission(105);
                            }

                            break;
                        }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void CreateEventFromSave()
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
                else
                {
                    if (string.IsNullOrEmpty(TxtEventName.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_enter_name), ToastLength.Short);
                        return;
                    }

                    if (string.IsNullOrEmpty(TxtStartDate.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_select_start_date), ToastLength.Short);
                        return;
                    }

                    if (string.IsNullOrEmpty(TxtEndDate.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_select_end_date), ToastLength.Short);
                        return;
                    }

                    if (string.IsNullOrEmpty(TxtLocation.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_select_Location), ToastLength.Short);
                        return;
                    }

                    if (string.IsNullOrEmpty(TxtStartTime.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_select_start_time), ToastLength.Short);
                        return;
                    }

                    if (string.IsNullOrEmpty(TxtEndTime.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_select_end_time), ToastLength.Short);
                        return;
                    }

                    if (string.IsNullOrEmpty(TxtDescription.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_enter_Description), ToastLength.Short);
                        return;
                    }

                    if (string.IsNullOrEmpty(EventPathImage))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_select_Image), ToastLength.Short);
                    }
                    else
                    {
                        //Show a progress
                        AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading) + "...");

                        var (apiStatus, respond) = await RequestsAsync.Event.CreateEventAsync(TxtEventName.Text, TxtLocation.Text, TxtDescription.Text, TxtStartDate.Text.Replace("/", "-"), TxtEndDate.Text.Replace("/", ""), TxtStartTime.Text.Replace("AM", "").Replace("PM", "").Replace(" ", ""), TxtEndTime.Text.Replace(" ", "-").Replace("AM", "").Replace("PM", ""), EventPathImage);
                        switch (apiStatus)
                        {
                            case 200:
                                {
                                    switch (respond)
                                    {
                                        case CreateEvent result:
                                            {
                                                AndHUD.Shared.Dismiss();
                                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CreatedSuccessfully), ToastLength.Short);

                                                var instance = EventMainActivity.GetInstance();
                                                //Add new item to my Event list
                                                if (result.Data != null)
                                                {
                                                    if (instance?.MyEventTab?.MAdapter?.EventList != null)
                                                    {
                                                        instance?.MyEventTab.MAdapter?.EventList?.Insert(0, result.Data);
                                                        instance?.MyEventTab.MAdapter?.NotifyItemInserted(0);
                                                    }

                                                    if (instance?.EventTab?.MAdapter?.EventList != null)
                                                    {
                                                        instance?.EventTab.MAdapter?.EventList?.Insert(0, result.Data);
                                                        instance?.EventTab.MAdapter?.NotifyItemInserted(0);
                                                    }
                                                }
                                                else
                                                {
                                                    var user = ListUtils.MyProfileList?.FirstOrDefault();
                                                    EventDataObject data = new EventDataObject
                                                    {
                                                        Id = result.EventId.ToString(),
                                                        Description = TxtDescription.Text,
                                                        Cover = EventPathImage,
                                                        EndDate = TxtEndDate.Text,
                                                        EndTime = TxtEndTime.Text,
                                                        IsOwner = true,
                                                        Location = TxtLocation.Text,
                                                        Name = TxtEventName.Text,
                                                        StartDate = TxtStartDate.Text,
                                                        StartTime = TxtStartTime.Text,
                                                        UserData = user,
                                                    };

                                                    if (instance?.MyEventTab?.MAdapter?.EventList != null)
                                                    {
                                                        instance?.MyEventTab.MAdapter?.EventList?.Insert(0, data);
                                                        instance?.MyEventTab.MAdapter?.NotifyItemInserted(0);
                                                    }

                                                    if (instance?.EventTab?.MAdapter?.EventList != null)
                                                    {
                                                        instance?.EventTab.MAdapter?.EventList?.Insert(0, data);
                                                        instance?.EventTab.MAdapter?.NotifyItemInserted(0);
                                                    }
                                                }

                                                Finish();
                                                break;
                                            }
                                    }

                                    break;
                                }
                            default:
                                Methods.DisplayAndHudErrorResult(this, respond);
                                break;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                AndHUD.Shared.Dismiss();
            }
        }

        private void TxtAddOnClick(object sender, EventArgs e)
        {
            switch (NStep)
            {
                case 1: // Event name
                    if (string.IsNullOrEmpty(TxtEventName.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_enter_name), ToastLength.Short);
                        return;
                    }
                    BtnSave.Text = GetString(Resource.String.Lbl_Next);
                    break;
                case 2: // Event start date and time
                    if (string.IsNullOrEmpty(TxtStartDate.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_select_start_date), ToastLength.Short);
                        return;
                    }
                    if (string.IsNullOrEmpty(TxtStartTime.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_select_start_time), ToastLength.Short);
                        return;
                    }
                    BtnSave.Text = GetString(Resource.String.Lbl_Next);
                    break;
                case 3: // Event end date and time
                    if (string.IsNullOrEmpty(TxtEndDate.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_select_end_date), ToastLength.Short);
                        return;
                    }
                    if (string.IsNullOrEmpty(TxtEndTime.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_select_end_time), ToastLength.Short);
                        return;
                    }
                    break;
                case 4: // Event Photo
                    if (string.IsNullOrEmpty(EventPathImage))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_select_Image), ToastLength.Short);
                    }
                    BtnSave.Text = GetString(Resource.String.Lbl_Next);
                    break;
                case 5:
                    if (string.IsNullOrEmpty(TxtLocation.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_select_Location), ToastLength.Short);
                        return;
                    }
                    BtnSave.Text = GetString(Resource.String.Lbl_Next);
                    break;
                case 6:
                    if (string.IsNullOrEmpty(TxtDescription.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_enter_Description), ToastLength.Short);
                        return;
                    }
                    BtnSave.Text = GetString(Resource.String.Lbl_Create);
                    break;
                default:
                    CreateEventFromSave();
                    return;
            }
            NStep += 1;
            SetStep();
        }

        #endregion

        #region Permissions && Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                // Location
                if (requestCode == 502 && resultCode == Result.Ok)
                {
                    var placeAddress = data.GetStringExtra("Address") ?? "";
                    TxtLocation.Text = string.IsNullOrEmpty(placeAddress) switch
                    {
                        //var placeLatLng = data.GetStringExtra("latLng") ?? "";
                        false => placeAddress,
                        _ => TxtLocation.Text
                    };
                }
                else if (requestCode == PixImagePickerActivity.RequestCode && resultCode == Result.Ok)
                {
                    var listPath = JsonConvert.DeserializeObject<ResultIntentPixImage>(data.GetStringExtra("ResultPixImage") ?? "");
                    if (listPath?.List?.Count > 0)
                    {
                        var filepath = listPath.List.FirstOrDefault();
                        if (!string.IsNullOrEmpty(filepath))
                        {
                            //Do something with your Uri
                            EventPathImage = filepath;

                            Glide.With(this).Load(filepath).Apply(new RequestOptions()).Into(RivImageEvent);
                        }
                        else
                        {
                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Failed_to_load), ToastLength.Short);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                switch (requestCode)
                {
                    case 108 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        PixImagePickerUtils.OpenDialogGallery(this);
                        break;
                    case 108:
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                        break;
                    //Open intent Location when the request code of result is 502
                    case 105 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        new IntentController(this).OpenIntentLocation();
                        break;
                    case 105:
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        public void OnClick(View v)
        {
            try
            {
                if (v.Id == TxtStartTime.Id)
                {
                    var frag = PopupDialogController.TimePickerFragment.NewInstance(delegate (DateTime time)
                    {
                        TxtStartTime.Text = time.ToShortTimeString();
                    });

                    frag.Show(SupportFragmentManager, PopupDialogController.TimePickerFragment.Tag);
                }
                else if (v.Id == TxtEndTime.Id)
                {
                    var frag = PopupDialogController.TimePickerFragment.NewInstance(delegate (DateTime time)
                    {
                        TxtEndTime.Text = time.ToShortTimeString();
                    });

                    frag.Show(SupportFragmentManager, PopupDialogController.TimePickerFragment.Tag);
                }
                else if (v.Id == TxtStartDate.Id)
                {
                    var frag = PopupDialogController.DatePickerFragment.NewInstance(delegate (DateTime time)
                    {
                        TxtStartDate.Text = time.Date.ToString("yyyy-MM-dd");
                    }, "StartDate");

                    frag.Show(SupportFragmentManager, PopupDialogController.DatePickerFragment.Tag);
                }
                else if (v.Id == TxtEndDate.Id)
                {
                    var frag = PopupDialogController.DatePickerFragment.NewInstance(delegate (DateTime time)
                    {
                        TxtEndDate.Text = time.Date.ToString("yyyy-MM-dd");
                    }, "StartDate");

                    frag.Show(SupportFragmentManager, PopupDialogController.DatePickerFragment.Tag);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnFocusChange(View v, bool hasFocus)
        {
            if (v?.Id == TxtLocation.Id && hasFocus)
            {
                TxtLocationOnFocusChange();
            }
        }

        public void SetStep()
        {
            try
            {
                TxtStep.Text = GetText(Resource.String.Lbl_Step) + " " + NStep + "/" + MaxStep;
                var progress = 100 / MaxStep * NStep;
                ViewStep.Progress = progress;

                switch (NStep)
                {
                    case 1:
                        llStep1.Visibility = ViewStates.Visible;
                        llStep2.Visibility = ViewStates.Gone;
                        llStep3.Visibility = ViewStates.Gone;
                        rlStep4.Visibility = ViewStates.Gone;
                        llStep5.Visibility = ViewStates.Gone;
                        llStep6.Visibility = ViewStates.Gone;
                        break;
                    case 2:
                        llStep1.Visibility = ViewStates.Gone;
                        llStep2.Visibility = ViewStates.Visible;
                        llStep3.Visibility = ViewStates.Gone;
                        rlStep4.Visibility = ViewStates.Gone;
                        llStep5.Visibility = ViewStates.Gone;
                        llStep6.Visibility = ViewStates.Gone;
                        break;
                    case 3:
                        llStep1.Visibility = ViewStates.Gone;
                        llStep2.Visibility = ViewStates.Gone;
                        llStep3.Visibility = ViewStates.Visible;
                        rlStep4.Visibility = ViewStates.Gone;
                        llStep5.Visibility = ViewStates.Gone;
                        llStep6.Visibility = ViewStates.Gone;
                        break;
                    case 4:
                        llStep1.Visibility = ViewStates.Gone;
                        llStep2.Visibility = ViewStates.Gone;
                        llStep3.Visibility = ViewStates.Gone;
                        rlStep4.Visibility = ViewStates.Visible;
                        llStep5.Visibility = ViewStates.Gone;
                        llStep6.Visibility = ViewStates.Gone;
                        break;
                    case 5:
                        llStep1.Visibility = ViewStates.Gone;
                        llStep2.Visibility = ViewStates.Gone;
                        llStep3.Visibility = ViewStates.Gone;
                        rlStep4.Visibility = ViewStates.Gone;
                        llStep5.Visibility = ViewStates.Visible;
                        llStep6.Visibility = ViewStates.Gone;
                        break;
                    case 6:
                        llStep1.Visibility = ViewStates.Gone;
                        llStep2.Visibility = ViewStates.Gone;
                        llStep3.Visibility = ViewStates.Gone;
                        rlStep4.Visibility = ViewStates.Gone;
                        llStep5.Visibility = ViewStates.Gone;
                        llStep6.Visibility = ViewStates.Visible;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void BackPressed()
        {
            if (NStep > 1)
            {
                NStep -= 1;
                SetStep();
                return;
            }
            Finish();
        }

    }
}