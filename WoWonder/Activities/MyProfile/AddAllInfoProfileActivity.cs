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
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Com.Google.Android.Gms.Ads.Admanager;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WoWonder.Activities.Base;
using WoWonder.Activities.Suggested.User;
using WoWonder.Adapters;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.SQLite;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;
using Console = System.Console;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.MyProfile
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class AddAllInfoProfileActivity : BaseActivity, View.IOnFocusChangeListener
    {
        #region Variables Basic

        private AppCompatButton BtnSave;
        private TextView TxtTitle, TxtStep;
        private LinearLayout Step1Layout, Step2Layout;
        private RelativeLayout SelectImageView;
        private ImageView YourImage;

        private EditText TxtFirstName, TxtLastName, TxtLocation, TxtWork, TxtSchool;
        private EditText TxtAbout, TxtMobile, TxtWebsite;

        private RecyclerView MRecycler;
        private GendersAdapter MAdapter;

        private AdManagerAdView AdManagerAdView;
        private string PathYourImage, IdRelationShip;

        private int CurrentStep = 1;
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
                SetContentView(Resource.Layout.AddAllInfoProfileLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();
                InitBackPressed("AddAllInfoProfileActivity");
                LoadUserProfile();

                SetStep(1);
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
            if (item.ItemId == Android.Resource.Id.Home)
            {
                BackPressed();
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
                TxtTitle = FindViewById<TextView>(Resource.Id.toolbar_title);
                TxtStep = FindViewById<TextView>(Resource.Id.toolbar_step);

                Step1Layout = FindViewById<LinearLayout>(Resource.Id.step1);
                Step2Layout = FindViewById<LinearLayout>(Resource.Id.step2);

                SelectImageView = FindViewById<RelativeLayout>(Resource.Id.SelectImageView);
                YourImage = FindViewById<ImageView>(Resource.Id.Image);

                BtnSave = FindViewById<AppCompatButton>(Resource.Id.SaveButton);

                TxtFirstName = FindViewById<EditText>(Resource.Id.FirstNameEditText);
                TxtLastName = FindViewById<EditText>(Resource.Id.LastNameEditText);
                TxtLocation = FindViewById<EditText>(Resource.Id.LocationEditText);
                TxtWork = FindViewById<EditText>(Resource.Id.WorkStatusEditText);
                TxtSchool = FindViewById<EditText>(Resource.Id.SchoolEditText);

                TxtAbout = FindViewById<EditText>(Resource.Id.AboutEditText);
                TxtMobile = FindViewById<EditText>(Resource.Id.PhoneEditText);
                TxtWebsite = FindViewById<EditText>(Resource.Id.WebsiteEditText);
                MRecycler = FindViewById<RecyclerView>(Resource.Id.RelationshipRecycler);

                Methods.SetColorEditText(TxtFirstName, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtLastName, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtLocation, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtMobile, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtWebsite, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtWork, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtSchool, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtAbout, WoWonderTools.IsTabDark() ? Color.White : Color.Black);

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
                    toolBar.Title = " ";
                    //toolBar.SetTitleTextColor(WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                    SetSupportActionBar(toolBar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(false);
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

        private void SetRecyclerViewAdapters()
        {
            try
            {
                MRecycler.HasFixedSize = true;
                MRecycler.SetLayoutManager(new LinearLayoutManager(this, LinearLayoutManager.Horizontal, false));
                MAdapter = new GendersAdapter(this)
                {
                    GenderList = new ObservableCollection<Classes.Gender>()
                };
                MRecycler.SetAdapter(MAdapter);
                MRecycler.NestedScrollingEnabled = false;
                MAdapter.NotifyDataSetChanged();
                MRecycler.Visibility = ViewStates.Visible;

                string[] relationshipArray = Application.Context.Resources?.GetStringArray(Resource.Array.RelationShipArray);
                for (int i = 0; i < relationshipArray?.Length; i++)
                {
                    MAdapter.GenderList.Add(new Classes.Gender
                    {
                        GenderId = i.ToString(),
                        GenderName = relationshipArray[i],
                        GenderColor = AppSettings.MainColor,
                        GenderSelect = false
                    });
                }

                MAdapter.NotifyDataSetChanged();
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
                if (addEvent)
                {
                    // true +=  // false -=
                    BtnSave.Click += TxtSaveOnClick;
                    TxtLocation.OnFocusChangeListener = this;
                    SelectImageView.Click += BtnAddImageOnClick;
                    MAdapter.ItemClick += MAdapterOnItemClick;
                }
                else
                {
                    BtnSave.Click -= TxtSaveOnClick;
                    TxtLocation.OnFocusChangeListener = null!;
                    SelectImageView.Click += BtnAddImageOnClick;
                    MAdapter.ItemClick -= MAdapterOnItemClick;
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
                BtnSave = null!;
                TxtFirstName = null!;
                TxtLastName = null!;
                TxtLocation = null!;
                TxtMobile = null!;
                TxtWebsite = null!;
                TxtWork = null!;
                TxtSchool = null!;
                AdManagerAdView = null!;
                IdRelationShip = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Back Pressed 

        public void BackPressed()
        {
            try
            {
                if (CurrentStep == 2)
                {
                    SetStep(1);
                }
                else
                {
                    Intent newIntent = new Intent(this, typeof(SuggestionsUsersActivity));
                    newIntent?.PutExtra("class", "register");
                    StartActivity(newIntent);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Events

        private void BtnAddImageOnClick(object sender, EventArgs e)
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

        private void MAdapterOnItemClick(object sender, GendersAdapterClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                if (position >= 0)
                {
                    var item = MAdapter.GetItem(position);
                    if (item != null)
                    {
                        var check = MAdapter.GenderList.Where(a => a.GenderSelect).ToList();
                        if (check.Count > 0)
                            foreach (var all in check)
                                all.GenderSelect = false;

                        item.GenderSelect = true;
                        MAdapter.NotifyDataSetChanged();

                        IdRelationShip = item.GenderId;
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void TxtSaveOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    return;
                }

                if (BtnSave.Tag?.ToString() == "Next")
                {
                    if (string.IsNullOrEmpty(PathYourImage))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_select_Image), ToastLength.Short);
                        return;
                    }

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => Update_Image_Api(PathYourImage) });
                    SetStep(2);

                    return;
                }

                if (!string.IsNullOrEmpty(TxtMobile.Text) && !Methods.FunString.IsPhoneNumber(TxtMobile.Text))
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_PhoneNumberIsWrong), ToastLength.Short);
                    return;
                }

                if (!string.IsNullOrEmpty(TxtWebsite.Text) && Methods.FunString.Check_Regex(TxtWebsite.Text) != "Website")
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_enter_Website), ToastLength.Short);
                    return;
                }

                //Show a progress
                AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                var dictionary = new Dictionary<string, string>
                {
                    {"first_name", TxtFirstName.Text},
                    {"last_name", TxtLastName.Text},
                    {"address", TxtLocation.Text},
                    {"phone_number", TxtMobile.Text},
                    {"website", TxtWebsite.Text},
                    {"working", TxtWork.Text},
                    {"school", TxtSchool.Text},
                    {"about", TxtAbout.Text},
                    {"relationship", IdRelationShip},
                };

                var (apiStatus, respond) = await RequestsAsync.Global.UpdateUserDataAsync(dictionary);
                if (apiStatus == 200)
                {
                    if (respond is MessageObject result)
                    {
                        var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                        if (dataUser != null)
                        {
                            dataUser.FirstName = TxtFirstName.Text;
                            dataUser.LastName = TxtLastName.Text;
                            dataUser.Address = TxtLocation.Text;
                            dataUser.PhoneNumber = TxtMobile.Text;
                            dataUser.Website = TxtWebsite.Text;
                            dataUser.Working = TxtWork.Text;
                            dataUser.About = TxtAbout.Text;
                            dataUser.School = TxtSchool.Text;
                            dataUser.RelationshipId = IdRelationShip;

                            dataUser.Avatar = PathYourImage;
                            UserDetails.Avatar = PathYourImage;
                            UserDetails.FullName = TxtFirstName.Text + " " + TxtLastName.Text;

                            var sqLiteDatabase = new SqLiteDatabase();
                            sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);
                        }

                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_YourDetailsWasUpdated), ToastLength.Short);
                        AndHUD.Shared.Dismiss();

                        Intent newIntent = new Intent(this, typeof(SuggestionsUsersActivity));
                        newIntent?.PutExtra("class", "register");
                        StartActivity(newIntent);

                        Finish();
                    }
                }
                else
                    Methods.DisplayAndHudErrorResult(this, respond);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtLocationOnClick()
        {
            try
            {
                if ((int)Build.VERSION.SdkInt < 23) // Check if we're running on Android 5.0 or higher
                {
                    new IntentController(this).OpenIntentLocation();
                }
                else
                {
                    if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) == Permission.Granted && ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) == Permission.Granted)
                    {
                        new IntentController(this).OpenIntentLocation();
                    }
                    else
                    {
                        new PermissionsController(this).RequestPermission(105);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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

                if (requestCode == 502 && resultCode == Result.Ok)
                {
                    GetPlaceFromPicker(data);
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
                            PathYourImage = filepath;

                            Glide.With(this).Load(filepath).Apply(new RequestOptions()).Into(YourImage);
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

                if (requestCode == 105 && (grantResults.Length > 0 && grantResults[0] == Permission.Granted))
                    //Open intent Camera when the request code of result is 502
                    new IntentController(this).OpenIntentLocation();
                else if (requestCode == 105)
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                //Image Picker
                else if (requestCode == 108 && (grantResults.Length > 0 && grantResults[0] == Permission.Granted))
                    //Open Image 
                    PixImagePickerUtils.OpenDialogGallery(this);
                else if (requestCode == 108) ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private void GetPlaceFromPicker(Intent data)
        {
            try
            {
                var placeAddress = data.GetStringExtra("Address") ?? "";
                if (!string.IsNullOrEmpty(placeAddress))
                    //var placeLatLng = data.GetStringExtra("latLng") ?? "";
                    TxtLocation.Text = placeAddress;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Update Image Avatar

        private async Task Update_Image_Api(string path)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
                else
                {
                    var (apiStatus, respond) = await RequestsAsync.Global.UpdateUserAvatarAsync(path);
                    if (apiStatus == 200)
                    {
                        if (respond is MessageObject result) Console.WriteLine(result.Message);
                    }
                    else
                        Methods.DisplayReportResult(this, respond);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        #endregion

        private void SetStep(int step)
        {
            try
            {
                CurrentStep = step;
                if (step == 1)
                {
                    TxtTitle.Text = GetText(Resource.String.Lbl_Iam);
                    TxtStep.Text = "1/2";

                    Step1Layout.Visibility = ViewStates.Visible;
                    Step2Layout.Visibility = ViewStates.Gone;

                    BtnSave.Text = GetText(Resource.String.Lbl_Next);
                    BtnSave.Tag = "Next";
                }
                else if (step == 2)
                {
                    TxtTitle.Text = GetText(Resource.String.Lbl_WhatAboutYou);
                    TxtStep.Text = "2/2";

                    Step1Layout.Visibility = ViewStates.Gone;
                    Step2Layout.Visibility = ViewStates.Visible;

                    BtnSave.Text = GetText(Resource.String.Lbl_Save);
                    BtnSave.Tag = "Save";
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
                TxtLocationOnClick();
            }
        }

        private void LoadUserProfile()
        {
            try
            {
                var local = ListUtils.MyProfileList?.FirstOrDefault();
                if (local != null)
                {
                    TxtFirstName.Text = Methods.FunString.DecodeString(local.FirstName);
                    TxtLastName.Text = Methods.FunString.DecodeString(local.LastName);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

    }
}