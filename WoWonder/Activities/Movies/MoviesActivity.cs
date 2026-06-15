using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Util;
using Com.Google.Android.Gms.Ads;
using Google.Android.Material.Dialog;
using Google.Android.Material.FloatingActionButton;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WoWonder.Activities.Base;
using WoWonder.Activities.Movies.Adapters;
using WoWonder.Activities.Videos;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using WoWonderClient.Classes.Movies;
using WoWonderClient.Requests;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Movies
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MoviesActivity : BaseActivity, IDialogListCallBack
    {
        #region Variables Basic

        private MoviesAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private RecyclerViewOnScrollListener MainScrollEvent;
        private AdView MAdView;
        private FloatingActionButton BtnFilter;
        private string CategoryId;


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
                SetContentView(Resource.Layout.RecyclerDefaultLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                LoadMovies();
                AdsGoogle.Ad_RewardedVideo(this);
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
                if (MAdapter.MoviesList.Count > 0 && CategoryId == "")
                    ListUtils.ListCachedDataMovie = MAdapter.MoviesList;

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
                MRecycler = (RecyclerView)FindViewById(Resource.Id.recyler);
                EmptyStateLayout = FindViewById<ViewStub>(Resource.Id.viewStub);

                SwipeRefreshLayout = (SwipeRefreshLayout)FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(WoWonderTools.IsTabDark() ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));


                BtnFilter = FindViewById<FloatingActionButton>(Resource.Id.floatingActionButtonView);
                BtnFilter.Visibility = ViewStates.Visible;

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
                    toolBar.Title = GetText(Resource.String.Lbl_Movies);
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
                MAdapter = new MoviesAdapter(this)
                {
                    MoviesList = new ObservableCollection<MoviesDataObject>()
                };
                LayoutManager = new LinearLayoutManager(this);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<MoviesDataObject>(this, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);

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

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                if (addEvent)
                {
                    // true +=  // false -=
                    MAdapter.ItemClick += MAdapterOnItemClick;
                    SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                    BtnFilter.Click += BtnFilterOnClick;
                }
                else
                {
                    MAdapter.ItemClick -= MAdapterOnItemClick;
                    SwipeRefreshLayout.Refresh -= SwipeRefreshLayoutOnRefresh;
                    BtnFilter.Click -= BtnFilterOnClick;
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
                AdsGoogle.LifecycleAdView(MAdView, "Destroy");


                MAdapter = null!;
                SwipeRefreshLayout = null!;
                MRecycler = null!;
                EmptyStateLayout = null!;
                Inflated = null!;
                MainScrollEvent = null!;
                BtnFilter = null!;
                CategoryId = null!;
                MAdView = null!;

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        #endregion

        #region Events

        //Refresh
        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                MAdapter.MoviesList.Clear();
                MAdapter.NotifyDataSetChanged();

                MainScrollEvent.IsLoading = false;

                Task.Factory.StartNew(() => StartApiService());
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Scroll
        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                //Code get last id where LoadMore >>
                var item = MAdapter.MoviesList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.Id) && !MainScrollEvent.IsLoading && CategoryId != "MostWatched")
                    StartApiService(item.Id);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MAdapterOnItemClick(object sender, MoviesAdapterClickEventArgs e)
        {
            try
            {
                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    var intent = new Intent(this, typeof(VideoViewerActivity));
                    intent.PutExtra("Viewer_Video", JsonConvert.SerializeObject(item));
                    intent.PutExtra("VideoId", item.Id);
                    StartActivity(intent);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnFilterOnClick(object sender, EventArgs e)
        {
            try
            {
                if (CategoriesController.ListCategoriesMovies.Count > 0)
                {
                    var dialogList = new MaterialAlertDialogBuilder(this);

                    var arrayAdapter = CategoriesController.ListCategoriesMovies.Select(item => item.CategoriesName)
                        .ToList();

                    arrayAdapter.Insert(0, GetString(Resource.String.Lbl_Default));
                    arrayAdapter.Insert(1, GetString(Resource.String.Lbl_MostWatched));

                    dialogList.SetTitle(GetText(Resource.String.Lbl_SelectCategories));
                    //dialogList.SetMessage(GetText(Resource.String.Lbl_GetMoviesByCategories));
                    dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                    dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                    dialogList.Show();
                }
                else
                {
                    Methods.DisplayReportResult(this, "Not have List Categories Movies");
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load Movies 

        private void LoadMovies()
        {
            try
            {
                if (ListUtils.ListCachedDataMovie.Count > 0)
                {
                    MAdapter.MoviesList = ListUtils.ListCachedDataMovie;
                    MAdapter.NotifyDataSetChanged();

                    var item = MAdapter.MoviesList.LastOrDefault();
                    if (item != null && !string.IsNullOrEmpty(item.Id))
                        StartApiService(item.Id);
                }
                else
                {
                    Task.Factory.StartNew(() => StartApiService());
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void StartApiService(string offset = "0")
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
            {
                if (CategoryId == "MostWatched")
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { MostWatchedAsync });
                else
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadMoviesAsync(offset) });
            }
        }

        private async Task LoadMoviesAsync(string offset = "0")
        {
            if (MainScrollEvent.IsLoading) return;

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;

                var countList = MAdapter.MoviesList.Count;
                var (apiStatus, respond) = await RequestsAsync.Movies.GetMoviesAsync("10", offset, "", CategoryId);
                if (apiStatus == 200)
                {
                    if (respond is GetMoviesObject result)
                    {
                        var respondList = result.Movies.Count;
                        if (respondList > 0)
                        {
                            if (countList > 0)
                            {
                                foreach (var item in from item in result.Movies let check = MAdapter.MoviesList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    MAdapter.MoviesList.Add(item);
                                }

                                RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.MoviesList.Count - countList); });
                            }
                            else
                            {
                                MAdapter.MoviesList = new ObservableCollection<MoviesDataObject>(result.Movies);
                                RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (MAdapter.MoviesList.Count > 10 && !MRecycler.CanScrollVertically(1))
                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_NoMoreMovies), ToastLength.Short);
                        }
                    }
                }
                else
                    Methods.DisplayReportResult(this, respond);

                RunOnUiThread(ShowEmptyPage);
            }
            else
            {
                Inflated = EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null!;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                MainScrollEvent.IsLoading = false;
            }
        }

        private async Task MostWatchedAsync()
        {

            if (MainScrollEvent.IsLoading) return;

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;

                var countList = MAdapter.MoviesList.Count;
                var (apiStatus, respond) = await RequestsAsync.Movies.MostWatchedAsync();
                if (apiStatus == 200)
                {
                    if (respond is MostWatchedObject result)
                    {
                        var respondList = result.Data.Count;
                        if (respondList > 0)
                        {
                            if (countList > 0)
                            {
                                foreach (var item in from item in result.Data let check = MAdapter.MoviesList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    MAdapter.MoviesList.Add(item);
                                }

                                RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.MoviesList.Count - countList); });
                            }
                            else
                            {
                                MAdapter.MoviesList = new ObservableCollection<MoviesDataObject>(result.Data);
                                RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (MAdapter.MoviesList.Count > 10 && !MRecycler.CanScrollVertically(1))
                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_NoMoreMovies), ToastLength.Short);
                        }
                    }
                }
                else
                    Methods.DisplayReportResult(this, respond);

                RunOnUiThread(ShowEmptyPage);
            }
            else
            {
                Inflated = EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null!;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                MainScrollEvent.IsLoading = false;
            }

        }

        private void ShowEmptyPage()
        {
            try
            {
                MainScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;

                if (MAdapter.MoviesList.Count > 0)
                {
                    MRecycler.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    MRecycler.Visibility = ViewStates.Gone;

                    Inflated ??= EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoMovies);
                    if (!x.EmptyStateButton.HasOnClickListeners) x.EmptyStateButton.Click += null!;

                    EmptyStateLayout.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception e)
            {
                MainScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;
                Methods.DisplayReportResultTrack(e);
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

        #region MaterialDialog

        public void OnSelection(IDialogInterface dialog, int position, string itemString)
        {
            try
            {
                if (itemString == GetString(Resource.String.Lbl_Default))
                    CategoryId = "";
                else if (itemString == GetString(Resource.String.Lbl_MostWatched))
                    CategoryId = "MostWatched";
                else
                    CategoryId = CategoriesController.ListCategoriesMovies.FirstOrDefault(categories => categories.CategoriesName == itemString)?.CategoriesId;

                MAdapter.MoviesList.Clear();
                MAdapter.NotifyDataSetChanged();

                MainScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;

                Task.Factory.StartNew(() => StartApiService());
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

    }
}