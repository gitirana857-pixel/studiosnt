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
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Com.Google.Android.Gms.Ads.Admanager;
using Google.Android.Material.Dialog;
using Java.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WoWonder.Activities.Base;
using WoWonder.Activities.Communities.Pages;
using WoWonder.Activities.MyProfile;
using WoWonder.Activities.NativePost.Extra;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Activities.Tabbes;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Advertise;
using WoWonderClient.Requests;
using static WoWonder.Helpers.Controller.PopupDialogController;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Advertise
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class CreateAdvertiseActivity : BaseActivity, IDialogListCallBack
    {
        #region Variables Basic

        private AppCompatButton SubmitButton;
        private ImageView ImageAd;
        private RelativeLayout SelectImageView;
        private EditText TxtName, TxtTitle, TxtDescription, TxtStartDate, TxtEndDate, TxtWebsite, TxtMyPages, TxtLocation, TxtAudience, TxtGender, TxtPlacement, TxtBudget, TxtBidding;
        private string PathImage, GenderStatus, TotalIdAudienceChecked, PlacementStatus, BiddingStatus, TypeDialog;
        private AdManagerAdView AdManagerAdView;
        private TabbedMainActivity GlobalContextTabbed;

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
                SetContentView(Resource.Layout.CreateAdvertiseLayout);

                GlobalContextTabbed = TabbedMainActivity.GetInstance();

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                Task.Factory.StartNew(StartApiService);
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
                SubmitButton = FindViewById<AppCompatButton>(Resource.Id.SubmitButton);

                TxtName = FindViewById<EditText>(Resource.Id.NameEditText);

                SelectImageView = FindViewById<RelativeLayout>(Resource.Id.SelectImageView);
                ImageAd = FindViewById<ImageView>(Resource.Id.image);

                TxtTitle = FindViewById<EditText>(Resource.Id.TitleEditText);
                TxtDescription = FindViewById<EditText>(Resource.Id.DescriptionEditText);
                TxtStartDate = FindViewById<EditText>(Resource.Id.StartDateEditText);
                TxtEndDate = FindViewById<EditText>(Resource.Id.EndDateEditText);
                TxtWebsite = FindViewById<EditText>(Resource.Id.websiteEditText);
                TxtMyPages = FindViewById<EditText>(Resource.Id.MyPagesEditText);
                TxtLocation = FindViewById<EditText>(Resource.Id.LocationEditText);
                TxtAudience = FindViewById<EditText>(Resource.Id.AudienceEditText);
                TxtGender = FindViewById<EditText>(Resource.Id.GenderEditText);
                TxtPlacement = FindViewById<EditText>(Resource.Id.PlacementEditText);
                TxtBudget = FindViewById<EditText>(Resource.Id.BudgetEditText);
                TxtBidding = FindViewById<EditText>(Resource.Id.BiddingEditText);

                AdManagerAdView = FindViewById<AdManagerAdView>(Resource.Id.multiple_ad_sizes_view);
                AdsGoogle.InitAdManagerAdView(AdManagerAdView);

                Methods.SetColorEditText(TxtName, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtTitle, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtDescription, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtStartDate, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtEndDate, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtWebsite, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtMyPages, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtLocation, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtAudience, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtGender, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtPlacement, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtBudget, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtBidding, WoWonderTools.IsTabDark() ? Color.White : Color.Black);

                Methods.SetFocusable(TxtStartDate);
                Methods.SetFocusable(TxtEndDate);
                Methods.SetFocusable(TxtMyPages);
                Methods.SetFocusable(TxtLocation);
                Methods.SetFocusable(TxtAudience);
                Methods.SetFocusable(TxtGender);
                Methods.SetFocusable(TxtPlacement);
                Methods.SetFocusable(TxtBidding);
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
                    toolBar.Title = GetString(Resource.String.Lbl_Create_Ad);
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
                        SubmitButton.Click += SubmitButtonOnClick;
                        SelectImageView.Click += BtnSelectImageOnClick;
                        TxtStartDate.Touch += TxtStartDateOnTouch;
                        TxtEndDate.Touch += TxtEndDateOnTouch;
                        TxtMyPages.Touch += TxtMyPagesOnTouch;
                        TxtLocation.Touch += TxtLocationOnTouch;
                        TxtAudience.Touch += TxtAudienceOnTouch;
                        TxtGender.Touch += TxtGenderOnTouch;
                        TxtPlacement.Touch += TxtPlacementOnTouch;
                        TxtBidding.Touch += TxtBiddingOnTouch;
                        break;
                    default:
                        SubmitButton.Click -= SubmitButtonOnClick;
                        SelectImageView.Click -= BtnSelectImageOnClick;
                        TxtStartDate.Touch -= TxtStartDateOnTouch;
                        TxtEndDate.Touch -= TxtEndDateOnTouch;
                        TxtMyPages.Touch -= TxtMyPagesOnTouch;
                        TxtLocation.Touch -= TxtLocationOnTouch;
                        TxtAudience.Touch -= TxtAudienceOnTouch;
                        TxtGender.Touch -= TxtGenderOnTouch;
                        TxtPlacement.Touch -= TxtPlacementOnTouch;
                        TxtBidding.Touch -= TxtBiddingOnTouch;
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

                SubmitButton = null!;
                TxtName = null!;
                ImageAd = null!;
                SelectImageView = null!;
                TxtTitle = null!;
                TxtDescription = null!;
                TxtStartDate = null!;
                TxtEndDate = null!;
                TxtWebsite = null!;
                TxtMyPages = null!;
                TxtLocation = null!;
                TxtAudience = null!;
                TxtGender = null!;
                TxtPlacement = null!;
                TxtBudget = null!;
                TxtBidding = null!;

                AdManagerAdView = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        //Add Image
        private void BtnSelectImageOnClick(object sender, EventArgs e)
        {
            try
            {
                PixImagePickerUtils.OpenDialogGallery(this); //requestCode >> 500 => Image Gallery 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtBiddingOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Up) return;

                TypeDialog = "Bidding";

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialAlertDialogBuilder(this);

                arrayAdapter.Add(GetText(Resource.String.Lbl_BiddingClick)); //clicks
                arrayAdapter.Add(GetText(Resource.String.Lbl_BiddingViews)); //views

                dialogList.SetTitle(GetText(Resource.String.Lbl_Bidding));
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtPlacementOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Up) return;

                TypeDialog = "Placement";

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialAlertDialogBuilder(this);

                arrayAdapter.Add(GetText(Resource.String.Lbl_PlacementPost)); //post
                arrayAdapter.Add(GetText(Resource.String.Lbl_PlacementSidebar)); //sidebar
                arrayAdapter.Add(GetText(Resource.String.Lbl_PlacementJobs)); //jobs
                arrayAdapter.Add(GetText(Resource.String.Lbl_PlacementForum)); //forum
                arrayAdapter.Add(GetText(Resource.String.Lbl_PlacementMovies)); //movies
                arrayAdapter.Add(GetText(Resource.String.Lbl_PlacementOffer)); //offer
                arrayAdapter.Add(GetText(Resource.String.Lbl_PlacementFunding)); //funding
                arrayAdapter.Add(GetText(Resource.String.Lbl_PlacementStory)); //story

                dialogList.SetTitle(GetText(Resource.String.Lbl_Placement));
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtAudienceOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Up) return;
                TypeDialog = "Audience";

                var countriesArray = WoWonderTools.GetCountryList(this);
                var listItems = countriesArray.Select(item => item.Value).ToList();

                var checkedItems = new bool[listItems.Count];
                var selectedItems = new List<string>(listItems);

                var dialogList = new MaterialAlertDialogBuilder(this);

                dialogList.SetTitle(Resource.String.Lbl_Audience);
                dialogList.SetCancelable(false);
                dialogList.SetMultiChoiceItems(listItems.ToArray(), checkedItems, (o, args) =>
                {
                    try
                    {
                        checkedItems[args.Which] = args.IsChecked;

                        var text = selectedItems[args.Which] ?? "";
                        Console.WriteLine(text);
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
                dialogList.SetPositiveButton(GetText(Resource.String.Lbl_Ok), (o, args) =>
                {
                    try
                    {
                        TotalIdAudienceChecked = "";
                        for (int i = 0; i < checkedItems.Length; i++)
                        {
                            if (checkedItems[i])
                            {
                                var text = selectedItems[i];
                                var check = countriesArray.FirstOrDefault(a => a.Value == text).Key;
                                if (check != null)
                                {
                                    TotalIdAudienceChecked += check + ",";
                                }
                            }
                        }

                        TxtAudience.Text = TypeDialog == "Audience" && !string.IsNullOrEmpty(TotalIdAudienceChecked) ? GetText(Resource.String.Lbl_Selected) : TxtAudience.Text;
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
                dialogList.SetNegativeButton(Resource.String.Lbl_Close, new MaterialDialogUtils());
                dialogList.SetNeutralButton(Resource.String.Lbl_SelectAll, (o, args) =>
                {
                    try
                    {
                        Arrays.Fill(checkedItems, true);

                        TotalIdAudienceChecked = "";
                        foreach (var item in countriesArray)
                        {
                            TotalIdAudienceChecked += item.Key + ",";
                        }

                        TxtAudience.Text = TypeDialog == "Audience" && !string.IsNullOrEmpty(TotalIdAudienceChecked) ? GetText(Resource.String.Lbl_Selected) : TxtAudience.Text;
                    }
                    catch (Exception ex)
                    {
                        Methods.DisplayReportResultTrack(ex);
                    }
                });

                dialogList.Show();

            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtMyPagesOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Up) return;

                TypeDialog = "MyPages";

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialAlertDialogBuilder(this);

                if (ListUtils.MyPageList?.Count > 0)
                    arrayAdapter.AddRange(ListUtils.MyPageList.Select(item => item.PageName));

                dialogList.SetTitle(GetString(Resource.String.Lbl_MyPages));
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNeutralButton(GetText(Resource.String.Lbl_Create_New_Page), (materialDialog, action) =>
                {
                    try
                    {
                        var intent = new Intent(this, typeof(CreatePageActivity));
                        StartActivity(intent);
                        Finish();
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());
                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtGenderOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Up) return;

                TypeDialog = "Genders";

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialAlertDialogBuilder(this);

                arrayAdapter.Add(GetText(Resource.String.Lbl_All));

                switch (ListUtils.SettingsSiteList?.Genders?.Count)
                {
                    case > 0:
                        arrayAdapter.AddRange(from item in ListUtils.SettingsSiteList?.Genders select item.Value);
                        break;
                    default:
                        arrayAdapter.Add(GetText(Resource.String.Radio_Male));
                        arrayAdapter.Add(GetText(Resource.String.Radio_Female));
                        break;
                }

                dialogList.SetTitle(GetText(Resource.String.Lbl_Gender));
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());
                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtLocationOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Up) return;

                TypeDialog = "Country";

                var countriesArray = WoWonderTools.GetCountryList(this);

                var dialogList = new MaterialAlertDialogBuilder(this);

                var arrayAdapter = countriesArray.Select(item => item.Value).ToList();

                dialogList.SetTitle(GetText(Resource.String.Lbl_Location));
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());
                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtEndDateOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Up) return;

                var frag = DatePickerFragment.NewInstance(delegate (DateTime time)
                {
                    TxtEndDate.Text = time.Date.ToString("yyyy-MM-dd");
                }, "StartDate");

                frag.Show(SupportFragmentManager, DatePickerFragment.Tag);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtStartDateOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Up) return;

                var frag = DatePickerFragment.NewInstance(delegate (DateTime time)
                {
                    TxtStartDate.Text = time.Date.ToString("yyyy-MM-dd");
                }, "StartDate");

                frag.Show(SupportFragmentManager, DatePickerFragment.Tag);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Save 
        private async void SubmitButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
                else
                {
                    if (string.IsNullOrEmpty(TxtName.Text) || string.IsNullOrWhiteSpace(TxtName.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_enter_name), ToastLength.Short);
                        return;
                    }

                    if (string.IsNullOrEmpty(TxtTitle.Text) || string.IsNullOrWhiteSpace(TxtTitle.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_enter_title), ToastLength.Short);
                        return;
                    }

                    if (string.IsNullOrEmpty(TxtDescription.Text) || string.IsNullOrWhiteSpace(TxtDescription.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_enter_Description), ToastLength.Short);
                        return;
                    }

                    if (string.IsNullOrEmpty(TxtStartDate.Text) || string.IsNullOrWhiteSpace(TxtStartDate.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_select_start_date), ToastLength.Short);
                        return;
                    }

                    if (string.IsNullOrEmpty(TxtEndDate.Text) || string.IsNullOrWhiteSpace(TxtEndDate.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_select_end_date), ToastLength.Short);
                        return;
                    }

                    if (string.IsNullOrEmpty(TxtMyPages.Text) || string.IsNullOrWhiteSpace(TxtMyPages.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_select_page), ToastLength.Short);
                        return;
                    }

                    if (string.IsNullOrEmpty(TxtWebsite.Text) || string.IsNullOrWhiteSpace(TxtWebsite.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_enter_Website), ToastLength.Short);
                        return;
                    }

                    if (string.IsNullOrEmpty(TxtLocation.Text) || string.IsNullOrWhiteSpace(TxtLocation.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_select_Location), ToastLength.Short);
                        return;
                    }

                    if (string.IsNullOrEmpty(TxtAudience.Text) || string.IsNullOrWhiteSpace(TxtAudience.Text) || string.IsNullOrEmpty(TotalIdAudienceChecked))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_select_Audience), ToastLength.Short);
                        return;
                    }

                    if (string.IsNullOrEmpty(TxtGender.Text) || string.IsNullOrWhiteSpace(TxtGender.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_select_Gender), ToastLength.Short);
                        return;
                    }

                    if (string.IsNullOrEmpty(TxtPlacement.Text) || string.IsNullOrWhiteSpace(TxtPlacement.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_select_Placement), ToastLength.Short);
                        return;
                    }

                    if (string.IsNullOrEmpty(TxtBudget.Text) || string.IsNullOrWhiteSpace(TxtBudget.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_select_Budget), ToastLength.Short);
                        return;
                    }

                    if (string.IsNullOrEmpty(TxtBidding.Text) || string.IsNullOrWhiteSpace(TxtBidding.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_select_Bidding), ToastLength.Short);
                        return;
                    }

                    if (string.IsNullOrEmpty(PathImage))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_select_Image), ToastLength.Short);
                        return;
                    }

                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                    TotalIdAudienceChecked = TotalIdAudienceChecked.Length switch
                    {
                        > 0 => TotalIdAudienceChecked.Remove(TotalIdAudienceChecked.Length - 1, 1),
                        _ => TotalIdAudienceChecked
                    };

                    var dictionary = new Dictionary<string, string>
                    {
                        {"name", TxtName.Text},
                        {"website",TxtWebsite.Text},
                        {"headline",TxtTitle.Text},
                        {"description", TxtDescription.Text},
                        {"bidding", BiddingStatus},
                        {"appears", PlacementStatus},
                        {"audience-list", TotalIdAudienceChecked},
                        {"gender", GenderStatus},
                        {"location", TxtLocation.Text},
                        {"start", TxtStartDate.Text},
                        {"end", TxtEndDate.Text},
                    };

                    var (apiStatus, respond) = await RequestsAsync.Advertise.CreateAdvertiseAsync(dictionary, PathImage);
                    switch (apiStatus)
                    {
                        case 200:
                            {
                                switch (respond)
                                {
                                    case CreateAdvertiseObject result:
                                        {
                                            AndHUD.Shared.Dismiss();
                                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CreatedSuccessfully), ToastLength.Short);

                                            //Add new item to list
                                            if (result.Data?.PostClass != null)
                                            {
                                                result.Data.Value.PostClass.PostType = "ad";

                                                var countList = GlobalContextTabbed.NewsFeedTab.PostFeedAdapter?.ItemCount ?? 0;

                                                var combine = new FeedCombiner(ApiPostAsync.RegexFilterText(result.Data.Value.PostClass), GlobalContextTabbed.NewsFeedTab.PostFeedAdapter?.ListDiffer, this, NativeFeedType.Global);
                                                combine.AddAdsPost();

                                                int countIndex = 1;
                                                var model1 = GlobalContextTabbed.NewsFeedTab.PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.Story);
                                                var model2 = GlobalContextTabbed.NewsFeedTab.PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.AddPostBox);
                                                var model4 = GlobalContextTabbed.NewsFeedTab.PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.AlertBox);

                                                if (model4 != null)
                                                    countIndex += GlobalContextTabbed.NewsFeedTab.PostFeedAdapter.ListDiffer.IndexOf(model4) + 1;
                                                else if (model2 != null)
                                                    countIndex += GlobalContextTabbed.NewsFeedTab.PostFeedAdapter.ListDiffer.IndexOf(model2) + 1;
                                                else if (model1 != null)
                                                    countIndex += GlobalContextTabbed.NewsFeedTab.PostFeedAdapter.ListDiffer.IndexOf(model1) + 1;
                                                else
                                                    countIndex = 0;

                                                var emptyStateChecker = GlobalContextTabbed.NewsFeedTab.PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.EmptyState);
                                                if (emptyStateChecker != null && GlobalContextTabbed.NewsFeedTab.PostFeedAdapter?.ListDiffer?.Count > 1)
                                                    GlobalContextTabbed.NewsFeedTab.MainRecyclerView.RemoveByRowIndex(emptyStateChecker);

                                                GlobalContextTabbed.NewsFeedTab.PostFeedAdapter?.NotifyItemRangeInserted(countIndex, GlobalContextTabbed.NewsFeedTab.PostFeedAdapter.ListDiffer.Count - countList);

                                                // My Profile
                                                MyProfileActivity myProfileActivity = MyProfileActivity.GetInstance();
                                                if (myProfileActivity != null)
                                                {
                                                    var countList1 = myProfileActivity.PostFeedAdapter?.ItemCount ?? 0;

                                                    var combine1 = new FeedCombiner(ApiPostAsync.RegexFilterText(result.Data.Value.PostClass), myProfileActivity.PostFeedAdapter?.ListDiffer, this, NativeFeedType.User);

                                                    combine1.AddAdsPost();

                                                    int countIndex1 = 1;
                                                    var model11 = myProfileActivity.PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.Story);
                                                    var model21 = myProfileActivity.PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.AddPostBox);
                                                    var model41 = myProfileActivity.PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.AlertBox);

                                                    if (model41 != null)
                                                        countIndex1 += myProfileActivity.PostFeedAdapter.ListDiffer.IndexOf(model41) + 1;
                                                    else if (model21 != null)
                                                        countIndex1 += myProfileActivity.PostFeedAdapter.ListDiffer.IndexOf(model21) + 1;
                                                    else if (model11 != null)
                                                        countIndex1 += myProfileActivity.PostFeedAdapter.ListDiffer.IndexOf(model11) + 1;
                                                    else
                                                        countIndex1 = 0;

                                                    var emptyStateChecker1 = myProfileActivity.PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.EmptyState);
                                                    if (emptyStateChecker1 != null && myProfileActivity.PostFeedAdapter?.ListDiffer?.Count > 1)
                                                        myProfileActivity.MainRecyclerView.RemoveByRowIndex(emptyStateChecker1);

                                                    myProfileActivity.PostFeedAdapter?.NotifyItemRangeInserted(countIndex1, myProfileActivity.PostFeedAdapter.ListDiffer.Count - countList1);
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
            catch (Exception exception)
            {
                AndHUD.Shared.Dismiss();
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Permissions

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                if (requestCode == PixImagePickerActivity.RequestCode && resultCode == Result.Ok)
                {
                    var listPath = JsonConvert.DeserializeObject<ResultIntentPixImage>(data.GetStringExtra("ResultPixImage") ?? "");
                    if (listPath?.List?.Count > 0)
                    {
                        var filepath = listPath.List.FirstOrDefault();
                        if (!string.IsNullOrEmpty(filepath))
                        {
                            //Do something with your Uri
                            PathImage = filepath;
                            Glide.With(this).Load(filepath).Apply(new RequestOptions()).Into(ImageAd);
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

                        var text = "";
                        for (int i = 0; i < permissions.Length; i++)
                        {
                            text += "\n permissions: " + permissions[i] + " grantResults : " + grantResults[i];
                        }

                        Methods.DialogPopup.InvokeAndShowDialog(this, "ReportMode", text, "Close");

                        PixImagePickerUtils.OpenDialogGallery(this);
                        break;
                    case 108:
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

        #region MaterialDialog

        public void OnSelection(IDialogInterface dialog, int position, string itemString)
        {
            try
            {
                switch (TypeDialog)
                {
                    case "Genders" when itemString == GetText(Resource.String.Lbl_All):
                        TxtGender.Text = GetText(Resource.String.Lbl_All);
                        GenderStatus = "all";
                        break;
                    case "Genders" when ListUtils.SettingsSiteList?.Genders?.Count > 0:
                        {
                            var key = ListUtils.SettingsSiteList?.Genders?.FirstOrDefault(a => a.Value == itemString).Key;
                            if (key != null)
                            {
                                TxtGender.Text = itemString;
                                GenderStatus = key;
                            }
                            else
                            {
                                TxtGender.Text = itemString;
                                GenderStatus = "male";
                            }

                            break;
                        }
                    case "Genders" when itemString == GetText(Resource.String.Radio_Male):
                        TxtGender.Text = GetText(Resource.String.Radio_Male);
                        GenderStatus = "male";
                        break;
                    case "Genders" when itemString == GetText(Resource.String.Radio_Female):
                        TxtGender.Text = GetText(Resource.String.Radio_Female);
                        GenderStatus = "female";
                        break;
                    case "Genders":
                        TxtGender.Text = GetText(Resource.String.Radio_Male);
                        GenderStatus = "male";
                        break;
                    case "Country":
                        TxtLocation.Text = itemString;
                        break;
                    case "MyPages":
                        {
                            if (ListUtils.MyPageList?.Count > 0)
                            {
                                var dataPage = ListUtils.MyPageList[position];
                                if (dataPage != null)
                                {
                                    TxtWebsite.Text = dataPage.Url;
                                }
                            }

                            TxtMyPages.Text = itemString;
                            break;
                        }
                    case "Placement":
                        {
                            if (itemString == GetText(Resource.String.Lbl_PlacementPost))
                            {
                                PlacementStatus = "post";
                            }
                            else if (itemString == GetText(Resource.String.Lbl_PlacementSidebar))
                            {
                                PlacementStatus = "sidebar";
                            }
                            else if (itemString == GetText(Resource.String.Lbl_PlacementJobs))
                            {
                                PlacementStatus = "jobs";
                            }
                            else if (itemString == GetText(Resource.String.Lbl_PlacementForum))
                            {
                                PlacementStatus = "forum";
                            }
                            else if (itemString == GetText(Resource.String.Lbl_PlacementMovies))
                            {
                                PlacementStatus = "movies";
                            }
                            else if (itemString == GetText(Resource.String.Lbl_PlacementOffer))
                            {
                                PlacementStatus = "offer";
                            }
                            else if (itemString == GetText(Resource.String.Lbl_PlacementFunding))
                            {
                                PlacementStatus = "funding";
                            }
                            else if (itemString == GetText(Resource.String.Lbl_PlacementStory))
                            {
                                PlacementStatus = "story";
                            }

                            TxtPlacement.Text = itemString;
                            break;
                        }
                    case "Bidding":
                        {
                            if (itemString == GetText(Resource.String.Lbl_BiddingClick))
                            {
                                BiddingStatus = "clicks";
                            }
                            else if (itemString == GetText(Resource.String.Lbl_BiddingViews))
                            {
                                BiddingStatus = "views";
                            }

                            TxtBidding.Text = itemString;
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { ApiRequest.GetMyPages });
        }

    }
}