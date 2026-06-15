using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Util;
using Com.Facebook.Ads;
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
using WoWonderClient.Requests;

namespace WoWonder.Activities.Market.Fragment
{
    public class PurchasedProductsFragment : AndroidX.Fragment.App.Fragment
    {
        #region Variables Basic

        private MarketAdapter MAdapter;
        private TabbedMarketActivity ContextMarket;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private RecyclerViewOnScrollListener MainScrollEvent;
        private AdView BannerAd;
        private bool MIsVisibleToUser;

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
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void SetMenuVisibility(bool menuVisible)
        {
            try
            {
                base.SetMenuVisibility(menuVisible);
                MIsVisibleToUser = menuVisible;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnResume()
        {
            try
            {
                base.OnResume();

                if (IsResumed && MIsVisibleToUser)
                {
                    Task.Factory.StartNew(() => StartApiService());
                }
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
                LayoutManager = new LinearLayoutManager(Activity);
                MRecycler.SetLayoutManager(LayoutManager);
                var animation = AnimationUtils.LoadAnimation(Activity, Resource.Animation.slideUpAnim);
                MRecycler.StartAnimation(animation);
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
                if (item != null && !string.IsNullOrEmpty(item.Product?.Id) && !MainScrollEvent.IsLoading)
                {
                    Task.Factory.StartNew(() => StartApiService(item.Product?.Id));
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
                    //var intent = new Intent(Context, typeof(ProductViewActivity));
                    //intent.PutExtra("Id", item.Product.PostId);
                    //intent.PutExtra("ProductView", JsonConvert.SerializeObject(item.Product));
                    //Context.StartActivity(intent);
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

                Task.Factory.StartNew(() => StartApiService());
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Get Market Api 

        private void StartApiService(string offset = "0")
        {
            if (Methods.CheckConnectivity())
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => GetPurchasedProducts(offset) });
            else
                ToastUtils.ShowToast(Context, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
        }

        private async Task GetPurchasedProducts(string offset = "0")
        {
            if (MainScrollEvent.IsLoading) return;

            MainScrollEvent.IsLoading = true;
            var countList = MAdapter.MarketList.Count;
            var (apiStatus, respond) = await RequestsAsync.Market.GetPurchasedProductsAsync("10", offset);
            if (apiStatus == 200)
            {
                if (respond is GetPurchasedProductsObject result)
                {
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    {
                        foreach (var item in from item in result.Data let check = MAdapter.MarketList.FirstOrDefault(a => a.Id == Convert.ToInt64(item.Id)) where check == null select item)
                        {
                            MAdapter.MarketList.Add(new Classes.ProductClass
                            {
                                Id = Convert.ToInt64(item.Id),
                                Type = Classes.ItemType.PurchasedProduct,
                                Purchased = item
                            });
                        }

                        if (countList > 0)
                        {
                            Activity?.RunOnUiThread(() =>
                            {
                                MAdapter.NotifyItemRangeInserted(countList, MAdapter.MarketList.Count - countList);
                            });
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
                }
            }
            else
                Methods.DisplayReportResult(Activity, respond);

            Activity?.RunOnUiThread(ShowEmptyPage);
        }

        private void ShowEmptyPage()
        {
            try
            {
                MainScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;

                switch (MAdapter.MarketList.Count)
                {
                    case > 0:
                        MRecycler.Visibility = ViewStates.Visible;
                        EmptyStateLayout.Visibility = ViewStates.Gone;
                        break;
                    default:
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

    }
}