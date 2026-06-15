using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Util;
using Com.Facebook.Ads;
using Newtonsoft.Json;
using Q.Rorbin.Badgeview;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WoWonder.Activities.Market.Adapters;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using WoWonderClient.Classes.Product;
using WoWonderClient.Classes.User;
using WoWonderClient.Requests;

namespace WoWonder.Activities.Market.Fragment
{
    public class MarketFragment : AndroidX.Fragment.App.Fragment
    {
        #region Variables Basic

        public MarketAdapter MAdapter;
        private TabbedMarketActivity ContextMarket;
        public SwipeRefreshLayout SwipeRefreshLayout;
        public RecyclerView MRecycler;
        private GridLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        public RecyclerViewOnScrollListener MainScrollEvent;
        private AdView BannerAd;

        #endregion

        #region General

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                View view = inflater.Inflate(Resource.Layout.MainFragmentLayout, container, false);
                ContextMarket = (TabbedMarketActivity)Activity;
                return view;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            {
                base.OnViewCreated(view, savedInstanceState);

                InitComponent(view);
                SetRecyclerViewAdapters();
                Task.Factory.StartNew(() => StartApiService());
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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

        public override void OnDestroy()
        {
            try
            {
                BannerAd?.Destroy();
                base.OnDestroy();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                MRecycler = (RecyclerView)view.FindViewById(Resource.Id.recyler);

                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);

                SwipeRefreshLayout = (SwipeRefreshLayout)view.FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(WoWonderTools.IsTabDark() ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;

                LinearLayout adContainer = view.FindViewById<LinearLayout>(Resource.Id.bannerContainer);
                if (AppSettings.ShowFbBannerAds)
                    BannerAd = AdsFacebook.InitAdView(Activity, adContainer, MRecycler);
                else if (AppSettings.ShowAppLovinBannerAds)
                    AdsAppLovin.InitBannerAd(Activity, adContainer, MRecycler);
                else
                    AdsGoogle.InitBannerAdView(Activity, adContainer, MRecycler);
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
                MAdapter = new MarketAdapter(Activity) { MarketList = new ObservableCollection<Classes.ProductClass>() };
                MAdapter.ItemClick += MAdapterOnItemClick;
                LayoutManager = new GridLayoutManager(Activity, 2);
                MRecycler.AddItemDecoration(new GridSpacingItemDecoration(2, 10, true));
                MRecycler.SetLayoutManager(LayoutManager);

                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<Classes.ProductClass>(Activity, MAdapter, sizeProvider, 10 /*maxPreload*/);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;

                RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(LayoutManager);
                MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
                MRecycler.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                MainScrollEvent.IsLoading = false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event

        //Scroll
        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                //Code get last id where LoadMore >>
                var item = MAdapter.MarketList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.Product.Id) && !MainScrollEvent.IsLoading)
                {
                    if (Methods.CheckConnectivity())
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => GetMarket(item.Product.Id) });
                    else
                        ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MAdapterOnItemClick(object sender, MarketAdapterClickEventArgs e)
        {
            try
            {
                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    var intent = new Intent(Context, typeof(ProductViewActivity));
                    intent.PutExtra("Id", item.Product.PostId);
                    intent.PutExtra("ProductView", JsonConvert.SerializeObject(item.Product));
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
                MAdapter.MarketList.Clear();
                MAdapter.NotifyDataSetChanged();

                MainScrollEvent.IsLoading = false;

                if (Methods.CheckConnectivity())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => GetMarket() });
                else
                    ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Get Market Api 

        public void StartApiService(string offsetMarket = "0")
        {
            if (Methods.CheckConnectivity())
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => GetMarket(offsetMarket), LoadCarts });
            else
                ToastUtils.ShowToast(Context, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
        }

        public async Task GetMarket(string offset = "0")
        {
            switch (MainScrollEvent.IsLoading)
            {
                case true:
                    return;
            }

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;

                var countList = MAdapter.MarketList.Count;
                var (apiStatus, respond) = await RequestsAsync.Market.GetProductsAsync("", "10", offset, "", "", UserDetails.MarketDistanceCount);
                switch (apiStatus)
                {
                    case 200:
                        {
                            switch (respond)
                            {
                                case GetProductsObject result:
                                    {
                                        var respondList = result.Products.Count;
                                        if (respondList > 0)
                                        {
                                            //var checkList = MAdapter.MarketList.FirstOrDefault(q => q.Type == Classes.ItemType.Section);
                                            //if (checkList == null)
                                            //{
                                            //    MAdapter.MarketList.Add(new Classes.ProductClass
                                            //    {
                                            //        Id = 1200,
                                            //        Title = GetText(Resource.String.Lbl_PopularProducts),
                                            //        Type = Classes.ItemType.Section,
                                            //    });
                                            //} 
                                            foreach (var item in from item in result.Products let check = MAdapter.MarketList.FirstOrDefault(a => a.Id == Convert.ToInt64(item.Id)) where check == null select item)
                                            {
                                                MAdapter.MarketList.Add(new Classes.ProductClass
                                                {
                                                    Id = Convert.ToInt64(item.Id),
                                                    Type = Classes.ItemType.Product,
                                                    Product = item
                                                });
                                            }

                                            if (countList > 0)
                                            {
                                                Activity?.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.MarketList.Count - countList); });
                                            }
                                            else
                                            {
                                                Activity?.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                                            }
                                        }
                                        else
                                        {
                                            if (MAdapter.MarketList.Count > 10 && !MRecycler.CanScrollVertically(1))
                                                ToastUtils.ShowToast(Context, GetText(Resource.String.Lbl_NoMoreProducts), ToastLength.Short);
                                        }
                                        break;
                                    }
                            }

                            break;
                        }
                    default:
                        Methods.DisplayReportResult(Activity, respond);
                        break;
                }

                Activity?.RunOnUiThread(() => { ShowEmptyPage("GetMarket"); });
            }
            else
            {
                Inflated = EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                switch (x.EmptyStateButton.HasOnClickListeners)
                {
                    case false:
                        x.EmptyStateButton.Click += null!;
                        x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                        break;
                }

                ToastUtils.ShowToast(Context, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                MainScrollEvent.IsLoading = false;
            }
        }

        private async Task LoadShopsAsync(string offset = "0")
        {
            if (Methods.CheckConnectivity())
            {
                //int countList = MAdapter.MarketList.Count;
                var (apiStatus, respond) = await RequestsAsync.Nearby.GetNearbyShopsAsync("10", offset, "", UserDetails.NearbyShopsDistanceCount);
                if (apiStatus != 200 || respond is not NearbyShopsObject result || result.Data == null)
                {
                    Methods.DisplayReportResult(Activity, respond);
                }
                else
                {
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    {
                        var checkList = MAdapter.MarketList.FirstOrDefault(q => q.Type == Classes.ItemType.NearbyShops);
                        if (checkList == null)
                        {
                            var nearbyShops = new Classes.ProductClass
                            {
                                Id = 205530,
                                ProductList = new List<ProductDataObject>(),
                                Type = Classes.ItemType.NearbyShops
                            };

                            foreach (var item in from item in result.Data let check = nearbyShops.ProductList.FirstOrDefault(a => a.Id == item.Product?.ProductClass.Id) where check == null select item)
                            {
                                nearbyShops.ProductList.Add(item.Product?.ProductClass);
                            }

                            MAdapter.MarketList.Insert(0, nearbyShops);
                            Activity?.RunOnUiThread(() => { MAdapter.NotifyItemInserted(0); });
                        }
                        else
                        {
                            foreach (var item in from item in result.Data let check = checkList.ProductList.FirstOrDefault(a => a.Id == item.Product?.ProductClass.Id) where check == null select item)
                            {
                                checkList.ProductList.Add(item.Product?.ProductClass);
                            }
                        }
                    }
                }
            }
            else
            {
                ToastUtils.ShowToast(Context, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            }
        }

        public void ShowEmptyPage(string type)
        {
            try
            {
                switch (type)
                {
                    case "GetMarket":
                        {
                            MainScrollEvent.IsLoading = false;
                            SwipeRefreshLayout.Refreshing = false;

                            if (MAdapter.MarketList.Count > 0)
                            {
                                MRecycler.Visibility = ViewStates.Visible;
                                EmptyStateLayout.Visibility = ViewStates.Gone;
                            }
                            else
                            {
                                MRecycler.Visibility = ViewStates.Gone;

                                Inflated = Inflated switch
                                {
                                    null => EmptyStateLayout.Inflate(),
                                    _ => Inflated
                                };

                                EmptyStateInflater x = new EmptyStateInflater();
                                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoProduct);
                                switch (x.EmptyStateButton.HasOnClickListeners)
                                {
                                    case false:
                                        x.EmptyStateButton.Click += null!;
                                        x.EmptyStateButton.Click += BtnCreateProductsOnClick;
                                        break;
                                }
                                EmptyStateLayout.Visibility = ViewStates.Visible;
                            }
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                MainScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Add New Product  >> CreateProductActivity
        private void BtnCreateProductsOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(Context, typeof(CreateProductActivity));
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //No Internet Connection 
        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                Task.Factory.StartNew(() => StartApiService());
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Carts & Badge

        private async Task LoadCarts()
        {
            try
            {
                var (apiStatus, respond) = await RequestsAsync.Market.GetCartsAsync().ConfigureAwait(false);
                if (apiStatus == 200)
                {
                    if (respond is GetCartsObject result)
                    {
                        Activity?.RunOnUiThread(() =>
                        {
                            try
                            {
                                var count = result.Data?.Count ?? 0;
                                if (count > 0)
                                {
                                    ShowOrHideBadgeViewIcon(count, true);
                                }
                                else
                                {
                                    ShowOrHideBadgeViewIcon();
                                }
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private static int CountCartsStatic;
        private QBadgeView BadgeCart;
        private void ShowOrHideBadgeViewIcon(int count = 0, bool show = false)
        {
            try
            {
                CountCartsStatic = count;
                if (show)
                {
                    BadgeCart ??= new QBadgeView(Activity);
                    int gravity = (int)(GravityFlags.End | GravityFlags.Top);
                    BadgeCart.BindTarget(ContextMarket.CartIcon);
                    BadgeCart.SetBadgeNumber(count);
                    BadgeCart.SetBadgeGravity(gravity);
                    BadgeCart.SetBadgeBackgroundColor(Color.ParseColor(AppSettings.MainColor));
                    BadgeCart.SetGravityOffset(10, true);
                }
                else
                {
                    BadgeCart?.BindTarget(ContextMarket.CartIcon).Hide(true);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void UpdateBadgeViewIcon(bool add)
        {
            try
            {
                if (add)
                {
                    ShowOrHideBadgeViewIcon(CountCartsStatic++, true);
                }
                else
                {
                    if (CountCartsStatic > 1)
                    {
                        ShowOrHideBadgeViewIcon(CountCartsStatic--, true);
                    }
                    else
                    {
                        ShowOrHideBadgeViewIcon();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

    }
}