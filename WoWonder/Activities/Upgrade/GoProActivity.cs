using Android.App;
using Android.BillingClient.Api;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.Dialog;
using InAppBilling.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WoWonder.Activities.Suggested.User;
using WoWonder.Activities.Tabbes;
using WoWonder.Activities.Upgrade.Adapters;
using WoWonder.Activities.WalkTroutPage;
using WoWonder.Activities.Wallet;
using WoWonder.Helpers.Utils;
using WoWonder.PaymentGoogle;
using WoWonder.SQLite;
using WoWonderClient;
using WoWonderClient.Requests;
using BaseActivity = WoWonder.Activities.Base.BaseActivity;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Upgrade
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Keyboard | ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenLayout | ConfigChanges.ScreenSize | ConfigChanges.SmallestScreenSize | ConfigChanges.UiMode | ConfigChanges.Locale)]
    public class GoProActivity : BaseActivity, IBillingPaymentListener, IDialogListCallBack
    {
        #region Variables Basic

        private RecyclerView MainRecyclerView, MainPlansRecyclerView;
        private GridLayoutManager LayoutManagerView;
        private LinearLayoutManager PlansLayoutManagerView;
        private GoProFeaturesAdapter FeaturesAdapter;
        private UpgradeGoProAdapter PlansAdapter;
        private ImageView IconClose;
        private string Caller;
        private UpgradeGoProClass ItemUpgrade;

        private BillingSupport BillingSupport;

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
                SetContentView(Resource.Layout.GoProLayout);

                Caller = Intent?.GetStringExtra("class") ?? "";

                //Get Value And Set Toolbar 
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                if (AppSettings.ShowInAppBilling && InitializeWoWonder.IsExtended)
                    BillingSupport = new BillingSupport(this, AppSettings.TripleDesAppServiceProvider, InAppBillingGoogle.ListProductSku, this);

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

        #endregion

        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    FinishPage();
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
                MainRecyclerView = FindViewById<RecyclerView>(Resource.Id.recycler);
                MainPlansRecyclerView = FindViewById<RecyclerView>(Resource.Id.recycler2);
                IconClose = FindViewById<ImageView>(Resource.Id.iv1);
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
                    toolBar.Title = GetText(Resource.String.Lbl_Go_Pro);
                    toolBar.SetTitleTextColor(WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                    SetSupportActionBar(toolBar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(false);
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

        private void SetRecyclerViewAdapters()
        {
            try
            {
                FeaturesAdapter = new GoProFeaturesAdapter(this);
                LayoutManagerView = new GridLayoutManager(this, 3);
                MainRecyclerView.SetLayoutManager(LayoutManagerView);
                MainRecyclerView.HasFixedSize = true;
                MainRecyclerView.SetAdapter(FeaturesAdapter);

                PlansAdapter = new UpgradeGoProAdapter(this);
                PlansLayoutManagerView = new LinearLayoutManager(this, LinearLayoutManager.Horizontal, false);
                MainPlansRecyclerView.SetLayoutManager(PlansLayoutManagerView);
                MainPlansRecyclerView.HasFixedSize = true;
                MainPlansRecyclerView.SetAdapter(PlansAdapter);
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
                        PlansAdapter.UpgradeButtonItemClick += PlansAdapterOnItemClick;
                        IconClose.Click += IconCloseOnClick;
                        break;
                    default:
                        PlansAdapter.UpgradeButtonItemClick -= PlansAdapterOnItemClick;
                        IconClose.Click -= IconCloseOnClick;
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
                MainRecyclerView = null!;
                MainPlansRecyclerView = null!;
                LayoutManagerView = null!;
                PlansLayoutManagerView = null!;
                FeaturesAdapter = null!;
                PlansAdapter = null!;
                IconClose = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void PlansAdapterOnItemClick(object sender, UpgradeGoProAdapterClickEventArgs e)
        {
            try
            {
                ItemUpgrade = PlansAdapter.GetItem(e.Position);
                if (ItemUpgrade != null)
                {
                    if (AppSettings.ShowInAppBilling && InitializeWoWonder.IsExtended)
                    {
                        var arrayAdapter = new List<string>();
                        var dialogList = new MaterialAlertDialogBuilder(this);

                        arrayAdapter.Add(GetString(Resource.String.Lbl_Wallet));

                        arrayAdapter.Add(GetString(Resource.String.Lbl_GooglePay));

                        dialogList.SetTitle(GetText(Resource.String.Lbl_PurchaseRequired));
                        dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                        dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                        dialogList.Show();
                    }
                    else
                    {
                        WalletDialog();
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Close
        private void IconCloseOnClick(object sender, EventArgs e)
        {
            try
            {
                FinishPage();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Billing

        public async void OnPaymentSuccess(IList<Purchase> result)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                    return;
                }

                await SetPro();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        public void OnPaymentError(string error)
        {

        }

        public void GetPurchase(IList<Purchase> result)
        {

        }

        #endregion

        #region MaterialDialog

        public void OnSelection(IDialogInterface dialog, int position, string itemString)
        {
            try
            {
                if (itemString == GetString(Resource.String.Lbl_Wallet))
                {
                    WalletDialog();
                }
                else if (itemString == GetString(Resource.String.Lbl_GooglePay))
                {
                    string type = "";
                    switch (ItemUpgrade.Id)
                    {
                        case "1":
                            type = InAppBillingGoogle.MembershipStar;
                            break;
                        case "2":
                            type = InAppBillingGoogle.MembershipHot;
                            break;
                        case "3":
                            type = InAppBillingGoogle.MembershipUltima;
                            break;
                        case "4":
                            type = InAppBillingGoogle.MembershipVip;
                            break;
                    }

                    BillingSupport?.PurchaseNow(type);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        private void FinishPage()
        {
            try
            {
                switch (Caller)
                {
                    case "register" when AppSettings.ShowSuggestedUsersOnRegister:
                        {
                            Intent newIntent = new Intent(this, typeof(SuggestionsUsersActivity));
                            newIntent?.PutExtra("class", "register");
                            StartActivity(newIntent);
                            break;
                        }
                    case "register":
                        StartActivity(new Intent(this, typeof(TabbedMainActivity)));
                        break;
                    case "login" when AppSettings.ShowWalkTroutPage:
                        {
                            Intent newIntent = new Intent(this, typeof(WalkTroutActivity));
                            newIntent?.PutExtra("class", "login");
                            StartActivity(newIntent);
                            break;
                        }
                    case "login":
                        StartActivity(new Intent(this, typeof(TabbedMainActivity)));
                        break;
                }

                Finish();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void WalletDialog()
        {
            try
            {
                var dialog = new MaterialAlertDialogBuilder(this);
                dialog.SetTitle(Resource.String.Lbl_PurchaseRequired);
                dialog.SetMessage(GetText(Resource.String.Lbl_Go_Pro));
                dialog.SetPositiveButton(GetText(Resource.String.Lbl_Purchase), async (materialDialog, action) =>
                {
                    try
                    {
                        if (WoWonderTools.CheckWallet(Convert.ToInt32(ItemUpgrade.PlanPrice)))
                        {
                            if (Methods.CheckConnectivity())
                            {
                                var (apiStatus, respond) = await RequestsAsync.Payments.UpgradeAsync(ItemUpgrade.Id);
                                switch (apiStatus)
                                {
                                    case 200:
                                        var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                                        if (dataUser != null)
                                        {
                                            dataUser.IsPro = "1";

                                            var sqlEntity = new SqLiteDatabase();
                                            sqlEntity.Insert_Or_Update_To_MyProfileTable(dataUser);
                                        }
                                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Upgraded), ToastLength.Long);
                                        FinishPage();
                                        break;
                                    default:
                                        Methods.DisplayReportResult(this, respond);
                                        break;
                                }
                            }
                            else
                            {
                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                            }
                        }
                        else
                        {
                            var dialogBuilder = new MaterialAlertDialogBuilder(this);
                            dialogBuilder.SetTitle(GetText(Resource.String.Lbl_Wallet));
                            dialogBuilder.SetMessage(GetText(Resource.String.Lbl_Error_NoWallet));
                            dialogBuilder.SetPositiveButton(GetText(Resource.String.Lbl_AddWallet), (materialDialog, action) =>
                            {
                                try
                                {
                                    StartActivity(new Intent(this, typeof(TabbedWalletActivity)));
                                }
                                catch (Exception exception)
                                {
                                    Methods.DisplayReportResultTrack(exception);
                                }
                            });
                            dialogBuilder.SetNegativeButton(GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());

                            dialogBuilder.Show();
                        }
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
                dialog.SetNegativeButton(GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());

                dialog.Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task SetPro()
        {
            if (Methods.CheckConnectivity())
            {
                var (apiStatus, respond) = await RequestsAsync.Global.SetProAsync(ItemUpgrade.Id);
                if (apiStatus == 200)
                {
                    var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                    if (dataUser != null)
                    {
                        dataUser.IsPro = "1";

                        var sqlEntity = new SqLiteDatabase();
                        sqlEntity.Insert_Or_Update_To_MyProfileTable(dataUser);
                    }

                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Upgraded), ToastLength.Long);
                    Finish();
                }
                else
                {
                    Methods.DisplayReportResult(this, respond);
                }
            }
            else
            {
                Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
            }
        }

    }
}