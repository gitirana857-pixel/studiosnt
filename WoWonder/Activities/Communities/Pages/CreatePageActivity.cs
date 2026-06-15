using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Content.Res;
using AndroidX.AppCompat.Widget;
using Com.Google.Android.Gms.Ads.Admanager;
using Google.Android.Flexbox;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using WoWonder.Activities.Base;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Page;
using WoWonderClient.Requests;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Communities.Pages
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class CreatePageActivity : BaseActivity
    {
        #region Variables Basic

        private AdManagerAdView AdManagerAdView;

        private TextView TvStep, TvStepTitle;
        private ProgressBar ViewStep;
        private EditText EtStep1, EtStep4;
        private FlexboxLayout RgStep3;
        private AppCompatButton BtnNext, BtnPrev;
        private int NStep = 1;
        private readonly int MaxStep = 4;
        private string PageTitle, PageUsername, PageAbout, Category, CategoryId;
        private List<string> ArrayAdapter;

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
                SetContentView(Resource.Layout.CreatePageLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                InitBackPressed("CreatePageActivity");
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
                //
                NStep = 1;
                TvStep = FindViewById<TextView>(Resource.Id.tv_step);
                TvStepTitle = FindViewById<TextView>(Resource.Id.tv_step_title);
                ViewStep = FindViewById<ProgressBar>(Resource.Id.view_step);

                EtStep1 = FindViewById<EditText>(Resource.Id.et_step12);
                EtStep4 = FindViewById<EditText>(Resource.Id.et_step4);
                RgStep3 = FindViewById<FlexboxLayout>(Resource.Id.rg_step3);
                BtnNext = FindViewById<AppCompatButton>(Resource.Id.btn_next);

                Methods.SetColorEditText(EtStep1, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(EtStep4, WoWonderTools.IsTabDark() ? Color.White : Color.Black);

                // Create category buttons
                CreateCategoryButtons();

                AdManagerAdView = FindViewById<AdManagerAdView>(Resource.Id.multiple_ad_sizes_view);
                AdsGoogle.InitAdManagerAdView(AdManagerAdView);

                SetStepChild();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void CreateCategoryButtons()
        {
            try
            {
                int count = CategoriesController.ListCategoriesPage.Count;
                if (count == 0)
                {
                    Methods.DisplayReportResult(this, "Not have List Categories Page");
                    return;
                }

                foreach (Classes.Categories category in CategoriesController.ListCategoriesPage)
                {
                    AppCompatButton button = new AppCompatButton(this);
                    int px = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 36, Resources.DisplayMetrics);

                    var ll = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, px);
                    ll.SetMargins(20, 18, 18, 20);
                    button.LayoutParameters = ll;

                    button.Text = category.CategoriesName;
                    button.SetBackgroundResource(Resource.Drawable.round_button_normal_outline);
                    button.SetTextColor(Color.ParseColor("#3E424B"));
                    button.TextSize = 15;
                    button.SetAllCaps(false);
                    button.SetPadding(25, 0, 25, 0);
                    button.Click += CategoryOnClick;
                    RgStep3.AddView(button);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void CategoryOnClick(object sender, EventArgs e)
        {
            if (BtnPrev != null)
            {
                BtnPrev.SetTextColor(Color.ParseColor("#3E424B"));
                BtnPrev.SetBackgroundResource(Resource.Drawable.round_button_normal_outline);
            }
            AppCompatButton BtnCurrent = sender as AppCompatButton;
            BtnCurrent.SetTextColor(Color.ParseColor("#ffffff"));
            BtnCurrent.SetBackgroundResource(Resource.Drawable.round_button_pressed);
            Category = BtnCurrent.Text;

            BtnPrev = BtnCurrent;
        }

        private void HideKeyboard()
        {
            try
            {
                var inputManager = (InputMethodManager)GetSystemService(InputMethodService);
                inputManager?.HideSoftInputFromWindow(CurrentFocus?.WindowToken, HideSoftInputFlags.None);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SetStepChild()
        {
            try
            {
                TvStep.Text = GetText(Resource.String.Lbl_Step) + " " + NStep + "/" + MaxStep;
                var progress = 100 / MaxStep * NStep;
                ViewStep.Progress = progress;

                switch (NStep)
                {
                    case 1:
                        EtStep1.Visibility = ViewStates.Visible;
                        EtStep4.Visibility = ViewStates.Gone;
                        RgStep3.Visibility = ViewStates.Gone;
                        TvStepTitle.Text = GetString(Resource.String.Lbl_SetPageTitle);

                        BtnNext.Text = GetString(Resource.String.Lbl_Next);
                        break;
                    case 2:
                        EtStep1.Hint = GetString(Resource.String.Lbl_PageUsername);
                        EtStep1.Visibility = ViewStates.Visible;
                        EtStep4.Visibility = ViewStates.Gone;
                        RgStep3.Visibility = ViewStates.Gone;
                        TvStepTitle.Text = GetString(Resource.String.Lbl_SetPageUserName);

                        BtnNext.Text = GetString(Resource.String.Lbl_Next);
                        break;
                    case 3:
                        HideKeyboard();
                        ArrayAdapter = CategoriesController.ListCategoriesPage.Select(item => item.CategoriesName).ToList();
                        EtStep1.Visibility = ViewStates.Gone;
                        EtStep4.Visibility = ViewStates.Gone;
                        RgStep3.Visibility = ViewStates.Visible;
                        TvStepTitle.Text = GetString(Resource.String.Lbl_SelectCategory);

                        BtnNext.Text = GetString(Resource.String.Lbl_Next);
                        break;
                    case 4:
                        EtStep1.Visibility = ViewStates.Gone;
                        EtStep4.Visibility = ViewStates.Visible;
                        RgStep3.Visibility = ViewStates.Gone;
                        AdManagerAdView.Visibility = ViewStates.Gone;
                        TvStepTitle.Text = GetString(Resource.String.Lbl_Describe_Page);

                        BtnNext.Text = GetString(Resource.String.Lbl_Create);
                        break;
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
                    toolBar.Title = GetText(Resource.String.Lbl_Create_New_Page);
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
                        BtnNext.Click += BtnNext_Click;
                        break;
                    default:
                        BtnNext.Click -= BtnNext_Click;
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
                SetStepChild();
                return;
            }
            Finish();
        }

        private void BtnNext_Click(object sender, EventArgs e)
        {
            try
            {
                switch (NStep)
                {
                    case 1:
                        PageTitle = EtStep1.Text;
                        if (PageTitle.Length > 0)
                        {
                            NStep += 1;
                            SetStepChild();
                            EtStep1.Text = "";
                        }
                        break;
                    case 2:
                        PageUsername = EtStep1.Text;
                        if (PageUsername.Length > 0)
                        {
                            NStep += 1;
                            SetStepChild();
                        }
                        break;
                    case 3:
                        if (Category.Length > 0)
                        {
                            CategoryId = CategoriesController.ListCategoriesPage.FirstOrDefault(categories => categories.CategoriesName == Category)?.CategoriesId;

                            NStep += 1;
                            SetStepChild();
                        }
                        break;

                    case 4:
                        PageAbout = EtStep4.Text;
                        if (PageAbout.Length > 0)
                        {
                            OnSave();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        private void DestroyBasic()
        {
            try
            {
                AdsGoogle.LifecycleAdManagerAdView(AdManagerAdView, "Destroy");

                TvStep = null!;
                TvStepTitle = null!;
                EtStep1 = null!;
                EtStep4 = null!;
                RgStep3 = null!;
                BtnNext = null!;

                PageTitle = "";
                PageUsername = "";
                PageAbout = "";
                Category = "";
                NStep = 1;

                AdManagerAdView = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private async void OnSave()
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    return;
                }
                //Show a progress
                AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading) + "...");

                var (apiStatus, respond) = await RequestsAsync.Page.CreatePageAsync(PageUsername.Replace(" ", ""), PageTitle, CategoryId, PageAbout);
                switch (apiStatus)
                {
                    case 200:
                        {
                            switch (respond)
                            {
                                case CreatePageObject result:
                                    {
                                        AndHUD.Shared.Dismiss();
                                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CreatedSuccessfully), ToastLength.Short);

                                        Intent returnIntent = new Intent();
                                        if (result.PageData != null)
                                            returnIntent?.PutExtra("pageItem", JsonConvert.SerializeObject(result.PageData));
                                        SetResult(Result.Ok, returnIntent);
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
            catch (Exception exception)
            {
                AndHUD.Shared.Dismiss();
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region MaterialDialog

        #endregion
    }
}