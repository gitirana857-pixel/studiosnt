using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AndroidX.ViewPager2.Widget;
using Google.Android.Material.Tabs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WoWonder.Activities.Articles;
using WoWonder.Activities.Base;
using WoWonder.Activities.Market.Fragment;
using WoWonder.Adapters;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Product;
using WoWonderClient.Requests;
using SearchView = AndroidX.AppCompat.Widget.SearchView;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Market
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class TabbedMarketActivity : BaseActivity, TabLayoutMediator.ITabConfigurationStrategy
    {
        #region Variables Basic

        private MainTabAdapter Adapter;
        public ViewPager2 ViewPager;
        public MarketFragment MarketTab;
        public MyProductsFragment MyProductsTab;
        public PurchasedProductsFragment MyPurchasedTab;
        private TabLayout TabLayout;
        private LinearLayout BtnAddProduct, BtnCategories;
        private SearchView SearchBox;
        private ImageView FilterButton;
        private ImageView DiscoverButton;
        public ImageView CartIcon;
        private string KeySearch = "";
        private static TabbedMarketActivity Instance;

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
                SetContentView(Resource.Layout.MarketMainLayout);

                Instance = this;

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

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
                ListUtils.ListCachedDataMyProduct = MyProductsTab.MAdapter.MarketList.Count switch
                {
                    > 0 => MyProductsTab.MAdapter.MarketList,
                    _ => ListUtils.ListCachedDataMyProduct
                };

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

        private void SearchViewOnQueryTextSubmit(object sender, SearchView.QueryTextSubmitEventArgs e)
        {
            try
            {
                KeySearch = e.NewText;

                MarketTab.MAdapter.MarketList.Clear();
                MarketTab.MAdapter.NotifyDataSetChanged();

                MarketTab.SwipeRefreshLayout.Refreshing = true;
                MarketTab.SwipeRefreshLayout.Enabled = true;

                if (Methods.CheckConnectivity())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => GetMarketByKey(KeySearch) });
                else
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SearchViewOnQueryTextChange(object sender, SearchView.QueryTextChangeEventArgs e)
        {
            try
            {
                KeySearch = e.NewText;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                ViewPager = FindViewById<ViewPager2>(Resource.Id.viewpager);
                TabLayout = FindViewById<TabLayout>(Resource.Id.tabs);
                CartIcon = FindViewById<ImageView>(Resource.Id.CartIcon);

                BtnAddProduct = FindViewById<LinearLayout>(Resource.Id.btnAddProduct);
                BtnCategories = FindViewById<LinearLayout>(Resource.Id.btnCategories);

                ViewPager.OffscreenPageLimit = 2;
                SetUpViewPager(ViewPager);
                new TabLayoutMediator(TabLayout, ViewPager, this).Attach();

                TabLayout.SetTabTextColors(WoWonderTools.IsTabDark() ? Color.White : Color.Black, Color.ParseColor(AppSettings.MainColor));

                DiscoverButton = (ImageView)FindViewById(Resource.Id.discoverButton);
                DiscoverButton.Visibility = AppSettings.ShowNearbyShops switch
                {
                    false => ViewStates.Gone,
                    _ => DiscoverButton.Visibility
                };

                FilterButton = (ImageView)FindViewById(Resource.Id.filter_icon);
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
                    toolBar.Title = GetText(Resource.String.Lbl_Marketplace);
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

                SearchBox = FindViewById<SearchView>(Resource.Id.searchBox);
                SearchBox.SetQuery("", false);
                SearchBox.SetIconifiedByDefault(false);
                SearchBox.OnActionViewExpanded();
                SearchBox.Iconified = false;
                SearchBox.QueryTextChange += SearchViewOnQueryTextChange;
                SearchBox.QueryTextSubmit += SearchViewOnQueryTextSubmit;
                SearchBox.ClearFocus();

                //Change text colors
                var editText = (EditText)SearchBox.FindViewById(Resource.Id.search_src_text);
                editText.SetHintTextColor(Color.ParseColor(AppSettings.MainColor));
                editText.SetTextColor(Color.Black);

                //Remove Icon Search
                ImageView searchViewIcon = (ImageView)SearchBox.FindViewById(Resource.Id.search_mag_icon);
                ViewGroup linearLayoutSearchView = (ViewGroup)searchViewIcon.Parent;
                linearLayoutSearchView.RemoveView(searchViewIcon);

                SearchBox.Visibility = ViewStates.Visible;
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
                        BtnCategories.Click += BtnCategoriesOnClick;
                        BtnAddProduct.Click += CreateProductOnClick;
                        CartIcon.Click += CartIconOnClick;
                        FilterButton.Click += FilterButtonOnClick;
                        DiscoverButton.Click += DiscoverButtonOnClick;
                        break;
                    default:
                        BtnCategories.Click -= BtnCategoriesOnClick;
                        BtnAddProduct.Click -= CreateProductOnClick;
                        CartIcon.Click += CartIconOnClick;
                        FilterButton.Click -= FilterButtonOnClick;
                        DiscoverButton.Click -= DiscoverButtonOnClick;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static TabbedMarketActivity GetInstance()
        {
            try
            {
                return Instance;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }
        private void DestroyBasic()
        {
            try
            {
                ViewPager = null!;
                TabLayout = null!;
                MarketTab = null!;
                MyProductsTab = null!;
                DiscoverButton = null!;
                FilterButton = null!;
                KeySearch = null!;
                Instance = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void CartIconOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(CheckoutCartActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void FilterButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                FilterMarketDialogFragment mFragment = new FilterMarketDialogFragment();

                Bundle bundle = new Bundle();
                bundle.PutString("TypeFilter", "Market");

                mFragment.Arguments = bundle;

                mFragment.Show(SupportFragmentManager, mFragment.Tag);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnCategoriesOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = CategoriesController.ListCategoriesProducts.Select(item => item.CategoriesName).ToList();

                arrayAdapter.Insert(0, GetString(Resource.String.Lbl_Default));
                //arrayAdapter.Insert(1, GetString(Resource.String.Lbl_MyProducts));

                Intent intent = new Intent(this, typeof(FilterCategoriesActivity));
                intent.PutExtra("filter_category", JsonConvert.SerializeObject(arrayAdapter));
                StartActivityForResult(intent, 2);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void CreateProductOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivityForResult(new Intent(this, typeof(CreateProductActivity)), 200);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Nearby Shops
        private void DiscoverButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                SearchBox.SetFocusable(ViewFocusability.Focusable);
                SearchBox.RequestFocus();
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
                if (requestCode == 200 && resultCode == Result.Ok)
                {
                    if (MarketTab != null)
                    {
                        var result = data.GetStringExtra("product") ?? "";
                        var item = JsonConvert.DeserializeObject<ProductDataObject>(result);
                        if (item != null)
                        {
                            MarketTab.MAdapter.MarketList.Insert(0, new Classes.ProductClass
                            {
                                Id = Convert.ToInt64(item.Id),
                                Type = Classes.ItemType.Product,
                                Product = item
                            });
                            MarketTab.MAdapter.NotifyItemInserted(0);
                        }
                    }
                }
                else if (requestCode == 2 && resultCode == Result.Ok)
                {
                    FilterCategory(data.GetStringExtra("category_item"));
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Set Tab

        private void SetUpViewPager(ViewPager2 viewPager)
        {
            try
            {
                MyProductsTab = new MyProductsFragment();
                MarketTab = new MarketFragment();
                MyPurchasedTab = new PurchasedProductsFragment();

                Adapter = new MainTabAdapter(this);
                Adapter.AddFragment(MarketTab, GetText(Resource.String.Lbl_Market));
                Adapter.AddFragment(MyProductsTab, GetText(Resource.String.Lbl_MyProducts));
                Adapter.AddFragment(MyPurchasedTab, GetText(Resource.String.Lbl_Purchased));

                viewPager.CurrentItem = Adapter.ItemCount;
                viewPager.OffscreenPageLimit = Adapter.ItemCount;

                viewPager.Orientation = ViewPager2.OrientationHorizontal;
                viewPager.Adapter = Adapter;
                viewPager.Adapter.NotifyDataSetChanged();
                ViewPager.UserInputEnabled = false;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnConfigureTab(TabLayout.Tab tab, int position)
        {
            try
            {
                tab.SetText(Adapter.GetFragment(position));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion Set Tab

        #region Get Market Api 

        private async Task GetMarketByKey(string key = "", string categoriesId = "")
        {
            if (MarketTab.MainScrollEvent.IsLoading) return;

            if (Methods.CheckConnectivity())
            {
                MarketTab.MainScrollEvent.IsLoading = true;
                var countList = MarketTab.MAdapter.MarketList.Count;
                var (apiStatus, respond) = await RequestsAsync.Market.GetProductsAsync("", "10", "0", categoriesId, key, UserDetails.MarketDistanceCount);
                if (apiStatus == 200)
                {
                    if (respond is GetProductsObject result)
                    {
                        var respondList = result.Products.Count;
                        if (respondList > 0)
                        {
                            foreach (var item in from item in result.Products let check = MarketTab.MAdapter.MarketList.FirstOrDefault(a => a.Id == Convert.ToInt64(item.Id)) where check == null select item)
                            {
                                MarketTab.MAdapter.MarketList.Add(new Classes.ProductClass
                                {
                                    Id = Convert.ToInt64(item.Id),
                                    Type = Classes.ItemType.Product,
                                    Product = item
                                });
                            }

                            if (countList > 0)
                            {
                                RunOnUiThread(() =>
                                {
                                    MarketTab.MAdapter.NotifyItemRangeInserted(countList, MarketTab.MAdapter.MarketList.Count - countList);
                                });
                            }
                            else
                            {
                                RunOnUiThread(() => { MarketTab.MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (MarketTab.MAdapter.MarketList.Count > 10 &&
                                !MarketTab.MRecycler.CanScrollVertically(1)) ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_NoMoreProducts), ToastLength.Short);
                        }
                    }
                }
                else
                    Methods.DisplayReportResult(this, respond);

                RunOnUiThread(() => { MarketTab.ShowEmptyPage("GetMarket"); });
            }
            else
            {
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                MarketTab.MainScrollEvent.IsLoading = false;
            }
        }

        #endregion

        private void FilterCategory(string item)
        {
            try
            {
                KeySearch = "";

                if (MarketTab != null)
                {
                    MarketTab.MAdapter.MarketList.Clear();
                    MarketTab.MAdapter.NotifyDataSetChanged();

                    MarketTab.SwipeRefreshLayout.Refreshing = true;
                    MarketTab.SwipeRefreshLayout.Enabled = true;

                    if (item == GetString(Resource.String.Lbl_Default))
                    {
                        Task.Factory.StartNew(() => MarketTab.StartApiService());
                    }
                    else
                    {
                        string CategoryId = CategoriesController.ListCategoriesProducts.FirstOrDefault(categories => categories.CategoriesName == item)?.CategoriesId;

                        if (Methods.CheckConnectivity())
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => GetMarketByKey(KeySearch, CategoryId) });
                        else
                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
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