using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Content.Res;
using Com.Google.Android.Gms.Ads.Admanager;
using Google.Android.Material.Dialog;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using WoWonder.Activities.Base;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Communities.Pages.Settings
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class PageActionButtonsActivity : BaseActivity, IDialogListCallBack
    {
        #region Variables Basic

        private TextView TxtSave, IconCallToAction, IconCallToTargetUrl;
        private EditText TxtCallToAction, TxtCallToTargetUrl;
        private string PagesId = "", CallToActionId = "", DialogType;
        private PageDataObject PageData;
        private readonly string[] CallToAction = Application.Context.Resources?.GetStringArray(Resource.Array.call_action_type);
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
                SetContentView(Resource.Layout.PageActionButtonsLayout);

                var id = Intent?.GetStringExtra("PageId") ?? "Data not available";
                if (id != "Data not available" && !string.IsNullOrEmpty(id)) PagesId = id;

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                Get_Data_Page();
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
                TxtSave = FindViewById<TextView>(Resource.Id.toolbar_title);

                IconCallToAction = FindViewById<TextView>(Resource.Id.IconCallToAction);
                TxtCallToAction = FindViewById<EditText>(Resource.Id.CallToActionEditText);

                IconCallToTargetUrl = FindViewById<TextView>(Resource.Id.IconCallToTargetUrl);
                TxtCallToTargetUrl = FindViewById<EditText>(Resource.Id.CallToTargetUrlEditText);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconCallToAction, FontAwesomeIcon.BezierCurve);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconCallToTargetUrl, FontAwesomeIcon.Bullseye);

                Methods.SetColorEditText(TxtCallToAction, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtCallToTargetUrl, WoWonderTools.IsTabDark() ? Color.White : Color.Black);

                Methods.SetFocusable(TxtCallToAction);

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
                    toolBar.Title = GetText(Resource.String.Lbl_ActionButtons);
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
                        TxtSave.Click += TxtSaveOnClick;
                        TxtCallToAction.Touch += TxtCallToActionOnTouch;
                        break;
                    default:
                        TxtSave.Click -= TxtSaveOnClick;
                        TxtCallToAction.Touch -= TxtCallToActionOnTouch;
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

                TxtSave = null!;
                IconCallToAction = null!;
                IconCallToTargetUrl = null!;
                TxtCallToAction = null!;
                TxtCallToTargetUrl = null!;
                PagesId = "";
                CallToActionId = "";
                DialogType = null!;
                PageData = null!;
                AdManagerAdView = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        //CallToAction
        private void TxtCallToActionOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Up) return;

                var dialogList = new MaterialAlertDialogBuilder(this);

                DialogType = "CallToAction";
                var arrayAdapter = CallToAction.ToList();

                dialogList.SetTitle(GetText(Resource.String.Lbl_CallToAction));
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                dialogList.Show();
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
                }
                else
                {
                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading) + "...");

                    var dictionary = new Dictionary<string, string>
                    {
                        {"call_action_type", CallToActionId},
                        {"call_action_type_url", TxtCallToTargetUrl.Text},
                    };

                    var (apiStatus, respond) = await RequestsAsync.Page.UpdatePageDataAsync(PagesId, dictionary);
                    switch (apiStatus)
                    {
                        case 200:
                            {
                                switch (respond)
                                {
                                    case MessageObject result:
                                        {
                                            AndHUD.Shared.Dismiss();
                                            Console.WriteLine(result.Message);

                                            PageData.CallActionTypeText = TxtCallToAction.Text;
                                            PageData.CallActionTypeUrl = TxtCallToTargetUrl.Text;
                                            PageData.CallActionType = CallToActionId;

                                            PageProfileActivity.PageData = PageData;

                                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_YourPageWasUpdated), ToastLength.Short);

                                            Intent returnIntent = new Intent();
                                            returnIntent.PutExtra("pageItem", JsonConvert.SerializeObject(PageData));
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
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                AndHUD.Shared.Dismiss();
            }
        }

        #endregion

        #region MaterialDialog

        public void OnSelection(IDialogInterface dialog, int position, string itemString)
        {
            try
            {
                switch (DialogType)
                {
                    case "CallToAction":
                        TxtCallToAction.Text = itemString;
                        CallToActionId = (position + 1).ToString();
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        //Get Data Page and set Categories
        private void Get_Data_Page()
        {
            try
            {
                PageData = JsonConvert.DeserializeObject<PageDataObject>(Intent?.GetStringExtra("PageData") ?? "");
                if (PageData != null)
                {
                    TxtCallToAction.Text = PageData.CallActionTypeText;
                    TxtCallToTargetUrl.Text = PageData.CallActionTypeUrl;

                    CallToActionId = PageData.CallActionType;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

    }
}