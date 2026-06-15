using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Content.Res;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Util;
using Com.Google.Android.Gms.Ads;
using Google.Android.Material.Dialog;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WoWonder.Activities.Address;
using WoWonder.Activities.Address.Adapters;
using WoWonder.Activities.Base;
using WoWonder.Activities.Market.Adapters;
using WoWonder.Activities.Wallet;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using WoWonderClient.Classes.Address;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Product;
using WoWonderClient.Requests;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Market
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class CheckoutCartActivity : BaseActivity
    {
        #region Variables Basic


        private LinearLayout TotalLinear, AddNewAddressLinear;
        private TextView TotalNumber;
        private AddressAdapter AddressAdapter;
        private CartAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler, AddressRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private AdView MAdView;
        private TextView BuyButton;
        private string AddressId;
        private long CountTotal;

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
                SetContentView(Resource.Layout.CheckoutCartLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();
                StartApiService();
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
                AdsGoogle.LifecycleAdView(MAdView, "Resume");
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
                AdsGoogle.LifecycleAdView(MAdView, "Pause");
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
                AdsGoogle.LifecycleAdView(MAdView, "Destroy");
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
                MRecycler = (RecyclerView)FindViewById(Resource.Id.CartRecycler);
                AddressRecycler = (RecyclerView)FindViewById(Resource.Id.AddressRecycler);
                EmptyStateLayout = FindViewById<ViewStub>(Resource.Id.viewStub);

                SwipeRefreshLayout = (SwipeRefreshLayout)FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(WoWonderTools.IsTabDark() ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));

                TotalLinear = FindViewById<LinearLayout>(Resource.Id.TotalLinear);
                TotalNumber = FindViewById<TextView>(Resource.Id.TotalNumber);

                AddNewAddressLinear = FindViewById<LinearLayout>(Resource.Id.AddNewAddressLinear);
                BuyButton = FindViewById<TextView>(Resource.Id.toolbar_title);

                MAdView = FindViewById<AdView>(Resource.Id.adView);
                AdsGoogle.InitAdView(MAdView, MRecycler);
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
                    toolBar.Title = GetText(Resource.String.Lbl_Carts);
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

        private void SetRecyclerViewAdapters()
        {
            try
            {
                MAdapter = new CartAdapter(this) { CartsList = new ObservableCollection<ProductDataObject>() };
                LayoutManager = new LinearLayoutManager(this);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<ProductDataObject>(this, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);

                //=====

                AddressAdapter = new AddressAdapter(this, "Select") { AddressList = new ObservableCollection<AddressDataObject>() };
                LayoutManager = new LinearLayoutManager(this);
                AddressRecycler.SetLayoutManager(LayoutManager);
                AddressRecycler.HasFixedSize = true;
                AddressRecycler.SetItemViewCacheSize(10);
                AddressRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                AddressRecycler.SetAdapter(AddressAdapter);
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
                    SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                    AddNewAddressLinear.Click += AddNewAddressLinearOnClick;
                    BuyButton.Click += BuyButtonOnClick;
                    MAdapter.ItemClick += MAdapterItemClick;
                    MAdapter.OnRemoveButtItemClick += MAdapterOnOnRemoveButtItemClick;
                    MAdapter.OnSelectQtyItemClick += MAdapterOnOnSelectQtyItemClick;
                    AddressAdapter.ItemClick += AddressAdapterItemClick;
                }
                else
                {
                    SwipeRefreshLayout.Refresh -= SwipeRefreshLayoutOnRefresh;
                    AddNewAddressLinear.Click -= AddNewAddressLinearOnClick;
                    BuyButton.Click -= BuyButtonOnClick;
                    MAdapter.ItemClick -= MAdapterItemClick;
                    MAdapter.OnRemoveButtItemClick -= MAdapterOnOnRemoveButtItemClick;
                    MAdapter.OnSelectQtyItemClick -= MAdapterOnOnSelectQtyItemClick;
                    AddressAdapter.ItemClick -= AddressAdapterItemClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event

        private void AddNewAddressLinearOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(AddressActivity));
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //select address
        private void AddressAdapterItemClick(object sender, AddressAdapterClickEventArgs e)
        {
            try
            {
                var item = AddressAdapter.GetItem(e.Position);
                if (item != null)
                {
                    AddressId = item.Id;

                    var list = AddressAdapter.AddressList.Where(a => a.Selected).ToList();
                    foreach (var data in list)
                    {
                        data.Selected = false;
                    }

                    item.Selected = true;
                    AddressAdapter.NotifyDataSetChanged();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Checkout all cart
        private async void BuyButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(AddressId))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseSelectAddress), ToastLength.Long)?.Show();
                    return;
                }

                if (WoWonderTools.CheckWallet(CountTotal))
                {
                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading) + "...");

                    if (Methods.CheckConnectivity())
                    {
                        var (apiStatus, respond) = await RequestsAsync.Market.BuyProductAsync(AddressId);
                        if (apiStatus == 200)
                        {
                            if (respond is MessageObject result)
                            {
                                Console.WriteLine(result.Message);

                                AndHUD.Shared.Dismiss();
                                Toast.MakeText(this, GetText(Resource.String.Lbl_OrderPlacedSuccessfully), ToastLength.Long)?.Show();
                                Finish();
                            }
                        }
                        else if (apiStatus == 400)
                        {
                            if (respond is ErrorObject result)
                            {
                                if (result.Error.ErrorText == "max qty is 0")
                                    Toast.MakeText(this, GetText(Resource.String.Lbl_ErrorCheckoutProducts), ToastLength.Long)?.Show();
                                else
                                    Toast.MakeText(this, result.Error.ErrorText, ToastLength.Long)?.Show();

                                AndHUD.Shared.Dismiss();
                            }

                            Methods.DisplayReportResult(this, respond);
                        }
                    }
                    else
                        Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
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
        }

        //change qty
        private void MAdapterOnOnSelectQtyItemClick(object sender, CartAdapterClickEventArgs e)
        {
            try
            {
                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    if (Methods.CheckConnectivity())
                    {
                        var dialogList = new MaterialAlertDialogBuilder(this);

                        var arrayAdapter = new List<string>();
                        for (int i = 1; i <= 100; i++)
                        {
                            arrayAdapter.Add(i.ToString());
                        }

                        dialogList.SetTitle(GetText(Resource.String.Lbl_Qty));
                        dialogList.SetItems(arrayAdapter.ToArray(), (o, args) =>
                        {
                            try
                            {
                                var text = arrayAdapter[args.Which];

                                item.Units = args.Which + 1;
                                e.CountQty.Text = GetText(Resource.String.Lbl_Qty) + " : " + text;

                                MAdapter.NotifyItemChanged(e.Position);

                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Market.ChangeQtyProductAsync(item.Id, text) });

                                CheckCountTotalProduct();
                            }
                            catch (Exception exception)
                            {
                                Methods.DisplayReportResultTrack(exception);
                            }
                        });
                        dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());
                        dialogList.Show();
                    }
                    else
                    {
                        Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //remove Cart
        private void MAdapterOnOnRemoveButtItemClick(object sender, CartAdapterClickEventArgs e)
        {
            try
            {
                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    if (Methods.CheckConnectivity())
                    {
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Market.AddToCartAsync(item.Id, "remove_cart") });

                        MAdapter.CartsList.Remove(item);
                        MAdapter.NotifyDataSetChanged();

                        TabbedMarketActivity.GetInstance()?.MarketTab?.UpdateBadgeViewIcon(false);

                        ShowEmptyPage("Carts");
                    }
                    else
                    {
                        Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //open Profile Cart
        private void MAdapterItemClick(object sender, CartAdapterClickEventArgs e)
        {
            try
            {
                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    var intent = new Intent(this, typeof(ProductViewActivity));
                    intent.PutExtra("Id", item.PostId);
                    intent.PutExtra("ProductView", JsonConvert.SerializeObject(item));
                    StartActivity(intent);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Refresh
        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                MAdapter.CartsList.Clear();
                MAdapter.NotifyDataSetChanged();

                MRecycler.Visibility = ViewStates.Visible;
                EmptyStateLayout.Visibility = ViewStates.Gone;

                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load Carts

        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadCarts, () => LoadAddress() });
        }

        private async Task LoadCarts()
        {
            if (Methods.CheckConnectivity())
            {
                int countList = MAdapter.CartsList.Count;
                var (apiStatus, respond) = await RequestsAsync.Market.GetCartsAsync();
                if (apiStatus == 200)
                {
                    if (respond is GetCartsObject result)
                    {
                        CountTotal = result.Total;
                        var respondList = result.Data?.Count;
                        if (respondList > 0)
                        {
                            if (countList > 0)
                            {
                                foreach (var item in from item in result.Data let check = MAdapter.CartsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    MAdapter.CartsList.Add(item);
                                }

                                RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.CartsList.Count - countList); });
                            }
                            else
                            {
                                MAdapter.CartsList = new ObservableCollection<ProductDataObject>(result.Data);
                                RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (MAdapter.CartsList.Count > 10 && !MRecycler.CanScrollVertically(1))
                                Toast.MakeText(this, GetText(Resource.String.Lbl_NoMoreCarts), ToastLength.Short)?.Show();
                        }
                    }
                }
                else
                {
                    Methods.DisplayReportResult(this, respond);
                }

                RunOnUiThread(() => { ShowEmptyPage("Carts"); });
            }
            else
            {
                Inflated = EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
        }

        private async Task LoadAddress(string offset = "0")
        {
            if (Methods.CheckConnectivity())
            {
                int countList = AddressAdapter.AddressList.Count;
                var (apiStatus, respond) = await RequestsAsync.Address.GetAddressListAsync("15", offset);
                if (apiStatus == 200)
                {
                    if (respond is GetAddressListObject result)
                    {
                        var respondList = result.Data?.Count;
                        if (respondList > 0)
                        {
                            if (countList > 0)
                            {
                                foreach (var item in from item in result.Data let check = AddressAdapter.AddressList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    AddressAdapter.AddressList.Add(item);
                                }

                                RunOnUiThread(() => { AddressAdapter.NotifyItemRangeInserted(countList, AddressAdapter.AddressList.Count - countList); });
                            }
                            else
                            {
                                AddressAdapter.AddressList = new ObservableCollection<AddressDataObject>(result.Data);
                                RunOnUiThread(() => { AddressAdapter.NotifyDataSetChanged(); });
                            }
                        }
                    }
                }
                else
                {
                    Methods.DisplayReportResult(this, respond);
                }

                RunOnUiThread(() => ShowEmptyPage("Address"));
            }
            else
            {
                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
        }

        private void ShowEmptyPage(string type)
        {
            try
            {
                SwipeRefreshLayout.Refreshing = false;
                TotalNumber.Text = "$" + CountTotal;

                if (type == "Cart")
                {
                    if (MAdapter.CartsList.Count > 0)
                    {
                        MRecycler.Visibility = ViewStates.Visible;
                        EmptyStateLayout.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        MRecycler.Visibility = ViewStates.Gone;

                        Inflated ??= EmptyStateLayout.Inflate();

                        EmptyStateInflater x = new EmptyStateInflater();
                        x.InflateLayout(Inflated, EmptyStateInflater.Type.NoCarts);
                        if (x.EmptyStateButton.HasOnClickListeners)
                        {
                            x.EmptyStateButton.Click += null;
                        }
                        EmptyStateLayout.Visibility = ViewStates.Visible;
                    }
                }
                else if (type == "Address")
                {
                    if (AddressAdapter.AddressList.Count > 0)
                    {
                        AddressRecycler.Visibility = ViewStates.Visible;
                        EmptyStateLayout.Visibility = ViewStates.Gone;
                    }
                }

                CheckCountTotalProduct();
            }
            catch (Exception e)
            {
                SwipeRefreshLayout.Refreshing = false;
                Methods.DisplayReportResultTrack(e);
            }
        }

        //No Internet Connection 
        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        private void CheckCountTotalProduct()
        {
            try
            {
                CountTotal = 0;
                foreach (var data in MAdapter.CartsList)
                {
                    if (data.Units != null && data.Units.Value > 1)
                    {
                        if (data.Price != null)
                            CountTotal += (Convert.ToInt64(data.Price) * data.Units.Value);
                    }
                    else
                    {
                        if (data.Price != null)
                            CountTotal += Convert.ToInt64(data.Price);
                    }
                }
                TotalNumber.Text = "$" + CountTotal;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


    }
}