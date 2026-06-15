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
using Newtonsoft.Json;
using System;
using System.Linq;
using WoWonder.Activities.Base;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Event;
using WoWonderClient.Requests;
using static WoWonder.Helpers.Controller.PopupDialogController;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Events
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class EditEventActivity : BaseActivity, View.IOnClickListener, View.IOnFocusChangeListener
    {
        #region Variables Basic

        private TextView IconStartDate, IconEndDate, IconLocation, TxtAdd;
        private EditText TxtEventName, TxtStartDate, TxtStartTime, TxtEndDate, TxtEndTime, TxtLocation, TxtDescription;
        private ImageView ImageEvent;
        private AppCompatButton BtnImage;
        private EventDataObject EventData;
        private string EventPathImage = "", EventId = "";

        private AdManagerAdView AdManagerAdView;

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
                SetContentView(Resource.Layout.EditEventLayout);

                EventId = Intent?.GetStringExtra("EventId") ?? string.Empty;

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();


                Get_Data_Event();
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
                TxtEventName = FindViewById<EditText>(Resource.Id.eventname);
                IconStartDate = FindViewById<TextView>(Resource.Id.StartIcondate);
                TxtStartDate = FindViewById<EditText>(Resource.Id.StartDateTextview);
                TxtStartTime = FindViewById<EditText>(Resource.Id.StartTimeTextview);
                IconEndDate = FindViewById<TextView>(Resource.Id.EndIcondate);
                TxtEndDate = FindViewById<EditText>(Resource.Id.EndDateTextview);
                TxtEndTime = FindViewById<EditText>(Resource.Id.EndTimeTextview);
                IconLocation = FindViewById<TextView>(Resource.Id.IconLocation);
                TxtLocation = FindViewById<EditText>(Resource.Id.LocationTextview);
                TxtDescription = FindViewById<EditText>(Resource.Id.description);

                ImageEvent = FindViewById<ImageView>(Resource.Id.EventCover);
                BtnImage = FindViewById<AppCompatButton>(Resource.Id.btn_selectimage);

                TxtAdd = FindViewById<TextView>(Resource.Id.toolbar_title);
                TxtAdd.Text = GetText(Resource.String.Lbl_Save);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconStartDate, IonIconsFonts.Time);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconEndDate, IonIconsFonts.Time);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconLocation, IonIconsFonts.Pin);

                Methods.SetColorEditText(TxtEventName, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtStartDate, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtStartTime, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
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
                    toolBar.Title = GetText(Resource.String.Lbl_EditEvents);
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
                        TxtAdd.Click += TxtAddOnClick;
                        TxtLocation.OnFocusChangeListener = this;
                        BtnImage.Click += BtnImageOnClick;
                        break;
                    default:
                        TxtAdd.Click -= TxtAddOnClick;
                        TxtLocation.OnFocusChangeListener = null!;
                        BtnImage.Click -= BtnImageOnClick;
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

                IconStartDate = null!;
                IconEndDate = null!;
                IconLocation = null!;
                TxtAdd = null!;
                TxtEventName = null!;
                TxtStartDate = null!;
                TxtStartTime = null!;
                TxtEndDate = null!;
                TxtEndTime = null!;
                TxtLocation = null!;
                TxtDescription = null!;
                ImageEvent = null!;
                BtnImage = null!;
                EventData = null!;
                EventPathImage = null!;
                EventId = null!;

                AdManagerAdView = null!;
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

        private async void TxtAddOnClick(object sender, EventArgs e)
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

                        var (apiStatus, respond) = await RequestsAsync.Event.EditEventAsync(EventId, TxtEventName.Text, TxtLocation.Text, TxtDescription.Text, TxtStartDate.Text, TxtEndDate.Text, TxtStartTime.Text, TxtEndTime.Text, EventPathImage);
                        switch (apiStatus)
                        {
                            case 200:
                                {
                                    switch (respond)
                                    {
                                        case EditEventObject result:
                                            {
                                                AndHUD.Shared.Dismiss();
                                                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_EventSuccessfullyEdited), ToastLength.Short);

                                                Console.WriteLine(result.MessageData);
                                                //Add new item to my Event list
                                                var user = ListUtils.MyProfileList?.FirstOrDefault();

                                                if (EventMainActivity.GetInstance()?.MyEventTab?.MAdapter.EventList != null)
                                                {
                                                    var data = EventMainActivity.GetInstance()?.MyEventTab?.MAdapter?.EventList?.FirstOrDefault(a => a.Id == EventId);
                                                    if (data != null)
                                                    {
                                                        data.Id = EventId;
                                                        data.Description = TxtDescription.Text;
                                                        data.Cover = EventPathImage;
                                                        data.EndDate = TxtEndDate.Text;
                                                        data.EndTime = TxtEndTime.Text;
                                                        data.IsOwner = true;
                                                        data.Location = TxtLocation.Text;
                                                        data.Name = TxtEventName.Text;
                                                        data.StartDate = TxtStartDate.Text;
                                                        data.StartTime = TxtStartTime.Text;
                                                        data.UserData = user;

                                                        EventMainActivity.GetInstance()?.MyEventTab?.MAdapter?.NotifyItemChanged(EventMainActivity.GetInstance().MyEventTab.MAdapter.EventList.IndexOf(data));
                                                    }
                                                }

                                                if (EventMainActivity.GetInstance()?.EventTab?.MAdapter.EventList != null)
                                                {
                                                    var data = EventMainActivity.GetInstance()?.EventTab?.MAdapter.EventList?.FirstOrDefault(a => a.Id == EventId);
                                                    if (data != null)
                                                    {
                                                        data.Id = EventId;
                                                        data.Description = TxtDescription.Text;
                                                        data.Cover = EventPathImage;
                                                        data.EndDate = TxtEndDate.Text;
                                                        data.EndTime = TxtEndTime.Text;
                                                        data.IsOwner = true;
                                                        data.Location = TxtLocation.Text;
                                                        data.Name = TxtEventName.Text;
                                                        data.StartDate = TxtStartDate.Text;
                                                        data.StartTime = TxtStartTime.Text;
                                                        data.UserData = user;

                                                        EventMainActivity.GetInstance()?.EventTab?.MAdapter?.NotifyItemChanged(EventMainActivity.GetInstance().EventTab.MAdapter.EventList.IndexOf(data));
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
                    if (!string.IsNullOrEmpty(placeAddress))
                        //var placeLatLng = data.GetStringExtra("latLng") ?? "";
                        TxtLocation.Text = placeAddress;
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

                            Glide.With(this).Load(filepath).Apply(new RequestOptions()).Into(ImageEvent);
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
                    var frag = TimePickerFragment.NewInstance(delegate (DateTime time)
                    {
                        TxtStartTime.Text = time.ToShortTimeString();
                    });

                    frag.Show(SupportFragmentManager, TimePickerFragment.Tag);
                }
                else if (v.Id == TxtEndTime.Id)
                {
                    var frag = TimePickerFragment.NewInstance(delegate (DateTime time)
                    {
                        TxtEndTime.Text = time.ToShortTimeString();
                    });

                    frag.Show(SupportFragmentManager, TimePickerFragment.Tag);
                }
                else if (v.Id == TxtStartDate.Id)
                {
                    var frag = DatePickerFragment.NewInstance(delegate (DateTime time)
                    {
                        TxtStartDate.Text = time.Date.ToString("yyyy-MM-dd");
                    }, "StartDate");

                    frag.Show(SupportFragmentManager, DatePickerFragment.Tag);
                }
                else if (v.Id == TxtEndDate.Id)
                {
                    var frag = DatePickerFragment.NewInstance(delegate (DateTime time)
                    {
                        TxtEndDate.Text = time.Date.ToString("yyyy-MM-dd");
                    }, "StartDate");

                    frag.Show(SupportFragmentManager, DatePickerFragment.Tag);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void Get_Data_Event()
        {
            try
            {
                EventData = JsonConvert.DeserializeObject<EventDataObject>(Intent?.GetStringExtra("EventData") ?? "");
                if (EventData != null)
                {
                    TxtEventName.Text = Methods.FunString.DecodeString(EventData.Name);
                    TxtStartDate.Text = EventData.StartDate;
                    TxtStartTime.Text = EventData.StartTime;
                    TxtEndDate.Text = EventData.EndDate;
                    TxtEndTime.Text = EventData.EndTime;
                    TxtLocation.Text = EventData.Location;
                    TxtDescription.Text = Methods.FunString.DecodeString(EventData.Description);

                    EventPathImage = EventData.Cover;
                    GlideImageLoader.LoadImage(this, EventData.Cover, ImageEvent, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
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
    }
}