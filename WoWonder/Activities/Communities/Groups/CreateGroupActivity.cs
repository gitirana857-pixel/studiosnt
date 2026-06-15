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
using WoWonderClient.Classes.Group;
using WoWonderClient.Requests;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Communities.Groups
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class CreateGroupActivity : BaseActivity
    {
        #region Variables Basic

        private AdManagerAdView AdManagerAdView;

        private TextView TvStep, TvStepTitle;
        private ProgressBar ViewStep;
        private EditText EtStep1, EtStep3;
        private RadioGroup RgStep5;
        private FlexboxLayout RgStep4;
        private AppCompatButton BtnNext;
        private int NStep = 1;
        private readonly int MaxStep = 5;
        private string GroupTitle, GroupUsername, GroupAbout, Category, Privacy, CategoryId;
        private List<string> ArrayAdapter;
        private AppCompatButton BtnPrev;

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
                SetContentView(Resource.Layout.CreateGroupLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                InitBackPressed("CreateGroupActivity");
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
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
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
                //
                NStep = 1;
                TvStep = FindViewById<TextView>(Resource.Id.tv_step);
                TvStepTitle = FindViewById<TextView>(Resource.Id.tv_step_title);
                ViewStep = FindViewById<ProgressBar>(Resource.Id.view_step);
                EtStep1 = FindViewById<EditText>(Resource.Id.et_step12);
                EtStep3 = FindViewById<EditText>(Resource.Id.et_step3);
                RgStep4 = FindViewById<FlexboxLayout>(Resource.Id.rg_step4);
                RgStep5 = FindViewById<RadioGroup>(Resource.Id.rg_step5);
                BtnNext = FindViewById<AppCompatButton>(Resource.Id.btn_next);

                Methods.SetColorEditText(EtStep1, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(EtStep3, WoWonderTools.IsTabDark() ? Color.White : Color.Black);

                // Create category buttons
                CreateCategoryButtons();

                //
                AdManagerAdView = FindViewById<AdManagerAdView>(Resource.Id.multiple_ad_sizes_view);
                AdsGoogle.InitAdManagerAdView(AdManagerAdView);

                //
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
                int count = CategoriesController.ListCategoriesGroup.Count;
                if (count == 0)
                {
                    Methods.DisplayReportResult(this, "Not have List Categories Group");
                    return;
                }

                foreach (Classes.Categories category in CategoriesController.ListCategoriesGroup)
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
                    RgStep4.AddView(button);
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
                        EtStep3.Visibility = ViewStates.Gone;
                        RgStep4.Visibility = ViewStates.Gone;
                        RgStep5.Visibility = ViewStates.Gone;
                        TvStepTitle.Text = GetString(Resource.String.Lbl_SetGroupTitle);

                        BtnNext.Text = GetString(Resource.String.Lbl_Next);
                        break;
                    case 2:
                        EtStep1.Hint = GetString(Resource.String.Lbl_GroupUsername);
                        EtStep1.Visibility = ViewStates.Visible;
                        EtStep3.Visibility = ViewStates.Gone;
                        RgStep4.Visibility = ViewStates.Gone;
                        RgStep5.Visibility = ViewStates.Gone;
                        TvStepTitle.Text = GetString(Resource.String.Lbl_SetGroupUserName);

                        BtnNext.Text = GetString(Resource.String.Lbl_Next);
                        break;
                    case 3:
                        EtStep1.Visibility = ViewStates.Gone;
                        EtStep3.Visibility = ViewStates.Visible;
                        RgStep4.Visibility = ViewStates.Gone;
                        RgStep5.Visibility = ViewStates.Gone;
                        HideKeyboard();
                        TvStepTitle.Text = GetString(Resource.String.Lbl_Describe_Group);

                        BtnNext.Text = GetString(Resource.String.Lbl_Next);
                        break;
                    case 4:
                        HideKeyboard();
                        ArrayAdapter = CategoriesController.ListCategoriesGroup.Select(item => item.CategoriesName).ToList();
                        EtStep1.Visibility = ViewStates.Gone;
                        EtStep3.Visibility = ViewStates.Gone;
                        RgStep4.Visibility = ViewStates.Visible;
                        RgStep5.Visibility = ViewStates.Gone;
                        TvStepTitle.Text = GetString(Resource.String.Lbl_SelectCategory);

                        BtnNext.Text = GetString(Resource.String.Lbl_Next);
                        break;
                    case 5:
                        EtStep1.Visibility = ViewStates.Gone;
                        EtStep3.Visibility = ViewStates.Gone;
                        RgStep4.Visibility = ViewStates.Gone;
                        RgStep5.Visibility = ViewStates.Visible;
                        TvStepTitle.Text = GetString(Resource.String.Lbl_SelectPrivacy);

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
                    toolBar.Title = GetText(Resource.String.Lbl_Create_New_Group);
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
                        GroupTitle = EtStep1.Text;
                        if (GroupTitle.Length > 0)
                        {
                            NStep += 1;
                            SetStepChild();
                            EtStep1.Text = "";
                        }
                        break;
                    case 2:
                        GroupUsername = EtStep1.Text;
                        if (GroupUsername.Length > 0)
                        {
                            NStep += 1;
                            SetStepChild();
                        }
                        break;
                    case 3:
                        GroupAbout = EtStep3.Text;
                        if (GroupAbout.Length > 0)
                        {
                            NStep += 1;
                            SetStepChild();
                        }
                        break;
                    case 4:
                        if (Category.Length > 0)
                        {
                            CategoryId = CategoriesController.ListCategoriesGroup.FirstOrDefault(categories => categories.CategoriesName == Category)?.CategoriesId;

                            NStep += 1;
                            SetStepChild();
                        }
                        break;
                    case 5:
                        if (RgStep5.CheckedRadioButtonId > 0)
                        {
                            var rb1 = FindViewById<RadioButton>(RgStep5.CheckedRadioButtonId);

                            if (rb1.Text.Equals("Public"))
                                Privacy = "1";
                            else
                                Privacy = "2";

                            // Create Group
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
                EtStep3 = null!;
                RgStep4 = null!;
                RgStep5 = null!;
                BtnNext = null!;

                GroupTitle = "";
                GroupUsername = "";
                GroupAbout = "";
                Category = "";
                Privacy = "";
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
                AndHUD.Shared.Show(this, GetString(Resource.String.Lbl_Loading) + "...");

                var (apiStatus, respond) = await RequestsAsync.Group.CreateGroupAsync(GroupUsername.Replace(" ", ""), GroupTitle, GroupAbout, CategoryId, Privacy);
                switch (apiStatus)
                {
                    case 200:
                        {
                            switch (respond)
                            {
                                case CreateGroupObject result:
                                    {
                                        AndHUD.Shared.Dismiss();
                                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CreatedSuccessfully), ToastLength.Short);

                                        Intent returnIntent = new Intent();
                                        if (result.GroupData != null)
                                            returnIntent?.PutExtra("groupItem", JsonConvert.SerializeObject(result.GroupData));
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
                Methods.DisplayReportResultTrack(exception);
                AndHUD.Shared.Dismiss();
            }
        }

        #endregion

    }
}