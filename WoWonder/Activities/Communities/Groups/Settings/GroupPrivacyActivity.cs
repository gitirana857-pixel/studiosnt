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
using WoWonder.Activities.Base;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Communities.Groups.Settings
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class GroupPrivacyActivity : BaseActivity, IDialogListCallBack
    {
        #region Variables Basic

        private TextView TxtCreate, IconType;
        private EditText TxtJoinPrivacy;
        private RadioButton RadioPublic, RadioPrivate;
        private string GroupsId, JoinPrivacyId = "", TypeDialog = "", GroupPrivacy = "";
        private GroupDataObject GroupData;
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
                SetContentView(Resource.Layout.GroupPrivacyLayout);

                var id = Intent?.GetStringExtra("GroupId") ?? "Data not available";
                if (id != "Data not available" && !string.IsNullOrEmpty(id)) GroupsId = id;

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                Get_Data_Group();
                AdsGoogle.Ad_Interstitial(this);
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
                TxtCreate = FindViewById<TextView>(Resource.Id.toolbar_title);


                IconType = FindViewById<TextView>(Resource.Id.IconType);
                RadioPublic = FindViewById<RadioButton>(Resource.Id.radioPublic);
                RadioPrivate = FindViewById<RadioButton>(Resource.Id.radioPrivate);

                TxtJoinPrivacy = FindViewById<EditText>(Resource.Id.JoinPrivacyEditText);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconType, FontAwesomeIcon.ShieldAlt);
                Methods.SetColorEditText(TxtJoinPrivacy, WoWonderTools.IsTabDark() ? Color.White : Color.Black);

                Methods.SetFocusable(TxtJoinPrivacy);

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
                    toolBar.Title = GetText(Resource.String.Lbl_Privacy);
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
                        TxtCreate.Click += TxtCreateOnClick;
                        TxtJoinPrivacy.Touch += TxtJoinPrivacyOnTouch;
                        RadioPublic.CheckedChange += RbPublicOnCheckedChange;
                        RadioPrivate.CheckedChange += RbPrivateOnCheckedChange;
                        break;
                    default:
                        TxtCreate.Click -= TxtCreateOnClick;
                        TxtJoinPrivacy.Touch -= TxtJoinPrivacyOnTouch;
                        RadioPublic.CheckedChange -= RbPublicOnCheckedChange;
                        RadioPrivate.CheckedChange -= RbPrivateOnCheckedChange;
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

                TxtCreate = null!;
                IconType = null!;
                TxtJoinPrivacy = null!;
                RadioPublic = null!;
                RadioPrivate = null!;
                GroupsId = null!;
                JoinPrivacyId = null!;
                TypeDialog = null!;
                GroupPrivacy = "";
                GroupData = null!;
                AdManagerAdView = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void RbPrivateOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                var isChecked = RadioPrivate.Checked;
                switch (isChecked)
                {
                    case true:
                        RadioPublic.Checked = false;
                        GroupPrivacy = "2";
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void RbPublicOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                var isChecked = RadioPublic.Checked;
                switch (isChecked)
                {
                    case true:
                        RadioPrivate.Checked = false;
                        GroupPrivacy = "1";
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void TxtCreateOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    return;
                }

                if (string.IsNullOrEmpty(TxtJoinPrivacy.Text))
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_enter_your_data), ToastLength.Short);
                    return;
                }
                if (string.IsNullOrEmpty(GroupPrivacy))
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_select_privacy), ToastLength.Short);
                    return;
                }

                //Show a progress
                AndHUD.Shared.Show(this, GetString(Resource.String.Lbl_Loading) + "...");

                var dictionary = new Dictionary<string, string>
                {
                    {"privacy", GroupPrivacy},
                    {"join_privacy", JoinPrivacyId},
                };

                var (apiStatus, respond) = await RequestsAsync.Group.UpdateGroupDataAsync(GroupsId, dictionary);
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
                                        GroupData.Privacy = GroupPrivacy;
                                        GroupData.JoinPrivacy = JoinPrivacyId;

                                        GroupProfileActivity.GroupDataClass = GroupData;

                                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_YourGroupWasUpdated), ToastLength.Short);

                                        Intent returnIntent = new Intent();
                                        returnIntent?.PutExtra("groupItem", JsonConvert.SerializeObject(GroupData));
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

        private void TxtJoinPrivacyOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Up) return;

                TypeDialog = "JoinPrivacy";

                var dialogList = new MaterialAlertDialogBuilder(this);

                var arrayAdapter = new List<string> { GetString(Resource.String.Lbl_Yes), GetString(Resource.String.Lbl_No) };

                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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
                    case "JoinPrivacy" when itemString == GetString(Resource.String.Lbl_Yes):
                        JoinPrivacyId = "2";
                        TxtJoinPrivacy.Text = GetString(Resource.String.Lbl_Yes);
                        break;
                    case "JoinPrivacy":
                        {
                            if (itemString == GetString(Resource.String.Lbl_No))
                            {
                                JoinPrivacyId = "1";
                                TxtJoinPrivacy.Text = GetString(Resource.String.Lbl_No);
                            }

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

        //Get Data Group and set Categories
        private void Get_Data_Group()
        {
            try
            {
                GroupData = JsonConvert.DeserializeObject<GroupDataObject>(Intent?.GetStringExtra("GroupData") ?? "");
                if (GroupData != null)
                {
                    switch (GroupData.Privacy)
                    {
                        //Public
                        case "1":
                            RadioPrivate.Checked = false;
                            RadioPublic.Checked = true;
                            break;
                        //Private
                        default:
                            RadioPrivate.Checked = true;
                            RadioPublic.Checked = false;
                            break;
                    }

                    GroupPrivacy = GroupData.Privacy;

                    switch (GroupData.JoinPrivacy)
                    {
                        case "1":
                            JoinPrivacyId = "1";
                            TxtJoinPrivacy.Text = GetString(Resource.String.Lbl_No);
                            break;
                        case "2":
                            JoinPrivacyId = "2";
                            TxtJoinPrivacy.Text = GetString(Resource.String.Lbl_Yes);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}