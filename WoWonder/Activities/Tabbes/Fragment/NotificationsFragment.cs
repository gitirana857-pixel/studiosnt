using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Util;
using Google.Android.Material.Tabs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WoWonder.Activities.Communities.Groups;
using WoWonder.Activities.Communities.Pages;
using WoWonder.Activities.Events;
using WoWonder.Activities.FriendRequest;
using WoWonder.Activities.Memories;
using WoWonder.Activities.NativePost.Pages;
using WoWonder.Activities.Story;
using WoWonder.Activities.Tabbes.Adapters;
using WoWonder.Activities.UserProfile;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.ShimmerUtils;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using WoWonder.Library.Anjo.SuperTextLibrary;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Page;
using WoWonderClient.Classes.Story;
using WoWonderClient.Classes.User;
using WoWonderClient.Requests;

namespace WoWonder.Activities.Tabbes.Fragment
{
    public class NotificationsFragment : AndroidX.Fragment.App.Fragment
    {
        #region Variables Basic

        private NotificationsAdapter MAdapter;
        private TabbedMainActivity GlobalContext;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout, AnnouncementViewStub, FriendRequestViewStub, CommunitiesRequestViewStub;
        private View Inflated, AnnouncementInflated, FriendRequestInflated, CommunitiesRequestInflated;
        private NestedScrollView ScrollView;

        private ViewStub ShimmerPageLayout;
        private View InflatedShimmer;
        private TemplateShimmerInflater ShimmerInflater;

        private TabLayout Tabs;
        private static string NameType = "All", TextAnnouncement;

        private NestedScrollViewOnScroll MainScrollEvent;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            GlobalContext = TabbedMainActivity.GetInstance();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                View view = inflater.Inflate(Resource.Layout.TNotificationsLayout, container, false);
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
                InitShimmer(view);
                SetRecyclerViewAdapters();
                Task.Factory.StartNew(() => StartApiService(false));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void OnResume()
        {
            try
            {
                base.OnResume();
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

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                ScrollView = (NestedScrollView)view.FindViewById(Resource.Id.ScrollView);
                SwipeRefreshLayout = (SwipeRefreshLayout)view.FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(WoWonderTools.IsTabDark() ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;

                AnnouncementViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubAnnouncement);
                FriendRequestViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubFriendRequest);
                FriendRequestViewStub.LayoutResource = Resource.Layout.ViewModel_Request;
                CommunitiesRequestViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubCommunitiesRequest);

                Tabs = view.FindViewById<TabLayout>(Resource.Id.tab_home);
                Tabs.TabSelected += TabsOnTabSelected;

                var tabLastActivities = Tabs.GetTabAt(1).View;
                var tabFriendsBirthday = Tabs.GetTabAt(2).View;

                if (!AppSettings.ShowLastActivities)
                    tabLastActivities.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowFriendsBirthday)
                    tabFriendsBirthday.Visibility = ViewStates.Gone;

                MRecycler = (RecyclerView)view.FindViewById(Resource.Id.recyler);
                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);

                NestedScrollViewOnScroll xamarinRecyclerViewOnScrollListener = new NestedScrollViewOnScroll();
                MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
                ScrollView.SetOnScrollChangeListener(xamarinRecyclerViewOnScrollListener);
                MainScrollEvent.IsLoading = false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitShimmer(View view)
        {
            try
            {
                ShimmerPageLayout = view.FindViewById<ViewStub>(Resource.Id.viewStubShimmer);
                InflatedShimmer ??= ShimmerPageLayout.Inflate();

                ShimmerInflater = new TemplateShimmerInflater();
                ShimmerInflater.InflateLayout(Activity, InflatedShimmer, ShimmerTemplateStyle.NotificationTemplate);
                ShimmerInflater.Show();
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
                MAdapter = new NotificationsAdapter(Activity) { NotificationsList = new ObservableCollection<Classes.NotificationsClass>() };
                MAdapter.ItemClick += MAdapterOnItemClick;
                LayoutManager = new LinearLayoutManager(Activity);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                MRecycler.NestedScrollingEnabled = false;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<Classes.NotificationsClass>(Activity, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);

                MRecycler.NestedScrollingEnabled = true;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event

        //Open user profile
        private void MAdapterOnItemClick(object sender, NotificationsAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position > -1)
                {
                    var item = MAdapter.GetItem(e.Position);
                    if (item != null)
                    {
                        if (NameType == "All")
                        {
                            EventClickNotification(Activity, item.Notification);
                        }
                        else if (NameType == "LastActivities")
                        {
                            if (item.LastActivities.ActivityType == "following" || item.LastActivities.ActivityType == "friend")
                            {
                                WoWonderTools.OpenProfile(Activity, item.LastActivities.UserId, item.LastActivities.Activator);
                            }
                            else
                            {
                                var intent = new Intent(Activity, typeof(ViewFullPostActivity));
                                intent.PutExtra("Id", item.LastActivities.PostId);
                                //intent.PutExtra("DataItem", JsonConvert.SerializeObject(item.PostData));
                                Activity.StartActivity(intent);
                            }
                        }
                        else if (NameType == "FriendsBirthday")
                        {
                            WoWonderTools.OpenProfile(Activity, item.User.UserId, item.User);
                        }
                    }
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
                ShimmerInflater?.Show();

                MAdapter.NotificationsList.Clear();
                MAdapter.NotifyDataSetChanged();

                if (MainScrollEvent != null) MainScrollEvent.IsLoading = false;

                MRecycler.Visibility = ViewStates.Visible;
                EmptyStateLayout.Visibility = ViewStates.Gone;

                if (NameType == "All")
                {
                    if (!Methods.CheckConnectivity())
                        ToastUtils.ShowToast(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    else
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadGeneralData(true) });
                }
                else if (NameType == "LastActivities")
                {
                    if (!Methods.CheckConnectivity())
                        ToastUtils.ShowToast(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    else
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadActivitiesAsync() });
                }
                else if (NameType == "FriendsBirthday")
                {
                    if (!Methods.CheckConnectivity())
                        ToastUtils.ShowToast(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    else
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { GetFriendsBirthdayApi });
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                //Code get last id where LoadMore >>
                var item = MAdapter.NotificationsList.LastOrDefault();

                if (NameType == "All")
                {
                    if (item != null && !string.IsNullOrEmpty(item.Notification.NotifierId) && !MainScrollEvent.IsLoading)
                    {
                        if (!Methods.CheckConnectivity())
                            ToastUtils.ShowToast(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                        else
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadGeneralData(false, item.Notification.NotifierId) });
                    }
                }
                else if (NameType == "LastActivities")
                {
                    if (item != null && !string.IsNullOrEmpty(item.LastActivities?.Id) && !MainScrollEvent.IsLoading)
                    {
                        if (!Methods.CheckConnectivity())
                            ToastUtils.ShowToast(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                        else
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadActivitiesAsync(item.LastActivities.Id) });
                    }
                }
                else if (NameType == "FriendsBirthday")
                {
                    //Nothing 
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void TabsOnTabSelected(object sender, TabLayout.TabSelectedEventArgs e)
        {
            try
            {
                ShimmerInflater.Show();

                switch (e.Tab.Position)
                {
                    //All Notifications
                    case 0:
                        {
                            NameType = "All";

                            MAdapter.NotificationsList.Clear();
                            MAdapter.NotifyDataSetChanged();

                            await LoadGeneralData(false);
                            break;
                        }
                    //Last Activities
                    case 1:
                        {
                            NameType = "LastActivities";

                            MAdapter.NotificationsList.Clear();
                            MAdapter.NotifyDataSetChanged();

                            await LoadActivitiesAsync();
                            break;
                        }
                    //Friends Birthday
                    case 2:
                        {
                            NameType = "FriendsBirthday";

                            MAdapter.NotificationsList.Clear();
                            MAdapter.NotifyDataSetChanged();

                            await GetFriendsBirthdayApi();
                            break;
                        }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load Notification 

        private void StartApiService(bool seenNotifications)
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadGeneralData(seenNotifications), LoadReviewsAsync });
        }

        //Get General Data Using Api >> notifications , pro_users , promoted_pages , trending_hashTag
        public async Task<(string, string, string)> LoadGeneralData(bool seenNotifications, string offset = "0")
        {
            try
            {
                if (MainScrollEvent != null && MainScrollEvent.IsLoading) return ("", "", "");

                if (Methods.CheckConnectivity())
                {
                    if (MainScrollEvent != null)
                        MainScrollEvent.IsLoading = true;

                    string fetch = "notifications,friend_requests";

                    if (AppSettings.ShowAnnouncement)
                        fetch += ",announcement";

                    if (AppSettings.ShowProUsersMembers)
                        fetch += ",pro_users";

                    if (AppSettings.ShowPromotedPages)
                        fetch += ",promoted_pages";

                    if (AppSettings.ShowTrendingHashTags)
                        fetch += ",trending_hashtag";

                    if (AppSettings.MessengerIntegration)
                        fetch += ",count_new_messages";

                    var (apiStatus, respond) = await RequestsAsync.Global.GetGeneralDataAsync(seenNotifications, UserDetails.OnlineUsers, UserDetails.DeviceId, offset, fetch);
                    if (apiStatus == 200)
                    {
                        if (respond is GetGeneralDataObject result)
                        {
                            // Notifications
                            var countList = MAdapter.NotificationsList.Count;
                            var respondList = result.Notifications.Count;
                            if (respondList > 0)
                            {
                                bool addItem = false;
                                foreach (var item in from item in result.Notifications let check = MAdapter.NotificationsList.FirstOrDefault(a => a.Notification?.Id == item.Id) where check == null select item)
                                {
                                    addItem = true;
                                    MAdapter.NotificationsList.Add(new Classes.NotificationsClass
                                    {
                                        Type = Classes.ItemType.Notifications,
                                        Notification = item
                                    });
                                }

                                var list = MAdapter.NotificationsList.Where(a => a.Type != Classes.ItemType.Notifications).ToList();
                                if (list.Count > 0)
                                    foreach (var item in list)
                                        MAdapter.NotificationsList.Remove(item);

                                if (addItem)
                                {
                                    if (countList > 0)
                                    {
                                        Activity?.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.NotificationsList.Count - countList); });
                                    }
                                    else
                                    {
                                        Activity?.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                                    }
                                }
                            }
                            else
                            {
                                if (MAdapter.NotificationsList.Count > 10 && !MRecycler.CanScrollVertically(1))
                                    ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_NoMoreNotifications), ToastLength.Short);
                            }

                            Activity?.RunOnUiThread(() =>
                            {
                                try
                                {
                                    // FriendRequests
                                    var respondListFriendRequests = result?.FriendRequests?.Count;
                                    if (respondListFriendRequests > 0)
                                    {
                                        ListUtils.FriendRequestsList = new ObservableCollection<UserDataObject>(result.FriendRequests);

                                        if (FriendRequestInflated == null)
                                        {
                                            FriendRequestInflated = FriendRequestViewStub.Inflate();

                                            var friendRequestImage = (ImageView)FriendRequestInflated.FindViewById(Resource.Id.imageFirstUser);
                                            var txTFriendRequest = (TextView)FriendRequestInflated.FindViewById(Resource.Id.tv_Friends_connection);
                                            var txtAllFriendRequest = (TextView)FriendRequestInflated.FindViewById(Resource.Id.tv_Friends);
                                            var friendRequestCount = (TextView)FriendRequestInflated.FindViewById(Resource.Id.count_view);

                                            FriendRequestInflated.Click += LayoutFriendRequestOnClick;

                                            txTFriendRequest.Text = Activity.GetText(AppSettings.ConnectivitySystem == 1 ? Resource.String.Lbl_FollowRequest : Resource.String.Lbl_FriendRequest);

                                            var user = ListUtils.FriendRequestsList.FirstOrDefault();
                                            if (user != null)
                                            {
                                                GlideImageLoader.LoadImage(Activity, user.Avatar, friendRequestImage, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);

                                                if (respondListFriendRequests > 2)
                                                    txtAllFriendRequest.Text = Methods.FunString.SubStringCutOf(WoWonderTools.GetNameFinal(user), 18) + " " + GetText(Resource.String.Lbl_And) + " " + ListUtils.FriendRequestsList.Count + " " + GetText(Resource.String.Lbl_OtherPeople);
                                                else
                                                    txtAllFriendRequest.Text = Methods.FunString.SubStringCutOf(WoWonderTools.GetNameFinal(user), 18) + " " + GetText(Resource.String.Lbl_And) + " " + GetText(Resource.String.Lbl_OtherPeople);
                                            }

                                            friendRequestCount.Text = ListUtils.FriendRequestsList.Count.ToString();
                                            friendRequestCount.Visibility = ViewStates.Visible;
                                        }
                                    }
                                    else
                                    {
                                        if (FriendRequestViewStub != null)
                                            FriendRequestViewStub.Visibility = ViewStates.Gone;
                                    }

                                    if (AppSettings.ShowTrendingPage && GlobalContext.TrendingTab != null)
                                    {
                                        // TrendingHashtag
                                        var respondListHashTag = result?.TrendingHashtag?.Count;
                                        if (respondListHashTag > 0 && AppSettings.ShowTrendingHashTags)
                                        {
                                            ListUtils.HashTagList = new ObservableCollection<TrendingHashtag>(result.TrendingHashtag);

                                            var checkList = GlobalContext.TrendingTab.MAdapter.TrendingList.FirstOrDefault(q => q.Type == Classes.ItemType.HashTag);
                                            if (checkList == null)
                                            {
                                                GlobalContext.TrendingTab.MAdapter.TrendingList.Add(new Classes.TrendingClass
                                                {
                                                    Id = 900,
                                                    Title = Activity.GetText(Resource.String.Lbl_TrendingHashTags),
                                                    SectionType = Classes.ItemType.HashTag,
                                                    Type = Classes.ItemType.Section,
                                                });

                                                var list = result.TrendingHashtag.Take(5).ToList();

                                                foreach (var item in from item in list let check = GlobalContext.TrendingTab.MAdapter.TrendingList.FirstOrDefault(a => a.HashTags?.Id == item.Id && a.Type == Classes.ItemType.HashTag) where check == null select item)
                                                {
                                                    GlobalContext.TrendingTab.MAdapter.TrendingList.Add(new Classes.TrendingClass
                                                    {
                                                        Id = long.Parse(item.Id),
                                                        HashTags = item,
                                                        Type = Classes.ItemType.HashTag
                                                    });
                                                }

                                                GlobalContext.TrendingTab.MAdapter.TrendingList.Add(new Classes.TrendingClass
                                                {
                                                    Type = Classes.ItemType.Divider
                                                });
                                            }
                                        }

                                        // PromotedPages
                                        var respondListPromotedPages = result.PromotedPages?.Count;
                                        if (respondListPromotedPages > 0 && AppSettings.ShowPromotedPages)
                                        {
                                            var checkList = GlobalContext.TrendingTab.MAdapter.TrendingList.FirstOrDefault(q => q.Type == Classes.ItemType.ProPage);
                                            if (checkList == null)
                                            {
                                                var proPage = new Classes.TrendingClass
                                                {
                                                    Id = 200,
                                                    PageList = new List<PageDataObject>(),
                                                    Type = Classes.ItemType.ProPage
                                                };

                                                foreach (var item in from item in result.PromotedPages let check = proPage.PageList.FirstOrDefault(a => a.PageId == item.PageId) where check == null select item)
                                                {
                                                    proPage.PageList.Add(item);
                                                }

                                                GlobalContext.TrendingTab.MAdapter.TrendingList.Insert(0, proPage);
                                                GlobalContext.TrendingTab.MAdapter.TrendingList.Insert(1, new Classes.TrendingClass
                                                {
                                                    Type = Classes.ItemType.Divider
                                                });
                                            }
                                            else
                                            {
                                                foreach (var item in from item in result.PromotedPages let check = checkList.PageList.FirstOrDefault(a => a.PageId == item.PageId) where check == null select item)
                                                {
                                                    checkList.PageList.Add(item);
                                                }
                                            }
                                        }

                                        // ProUsers
                                        var respondListProUsers = result?.ProUsers?.Count;
                                        if (respondListProUsers > 0 && AppSettings.ShowProUsersMembers)
                                        {
                                            var checkList = GlobalContext.TrendingTab.MAdapter.TrendingList.FirstOrDefault(q => q.Type == Classes.ItemType.ProUser);
                                            if (checkList == null)
                                            {
                                                var proUser = new Classes.TrendingClass
                                                {
                                                    Id = 100,
                                                    UserList = new List<UserDataObject>(),
                                                    Type = Classes.ItemType.ProUser
                                                };

                                                foreach (var item in from item in result.ProUsers let check = proUser.UserList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select item)
                                                {
                                                    proUser.UserList.Add(item);
                                                }

                                                GlobalContext.TrendingTab.MAdapter.TrendingList.Insert(0, proUser);
                                                GlobalContext.TrendingTab.MAdapter.TrendingList.Insert(1, new Classes.TrendingClass
                                                {
                                                    Type = Classes.ItemType.Divider
                                                });
                                            }
                                            else
                                            {
                                                foreach (var item in from item in result.ProUsers let check = checkList.UserList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select item)
                                                {
                                                    checkList.UserList.Add(item);
                                                }
                                            }
                                        }

                                        GlobalContext.TrendingTab.MAdapter.NotifyDataSetChanged();
                                    }

                                    if (!string.IsNullOrEmpty(result.Announcement?.AnnouncementClass?.TextDecode))
                                    {
                                        if (AnnouncementInflated == null)
                                        {
                                            AnnouncementInflated = AnnouncementViewStub.Inflate();

                                            var AnnouncementImage = (AppCompatButton)AnnouncementInflated.FindViewById(Resource.Id.announcementButton);
                                            AnnouncementImage.Click += AnnouncementImageOnClick;
                                        }

                                        TextAnnouncement = result.Announcement?.AnnouncementClass?.TextDecode;
                                    }

                                    if (MainScrollEvent != null) MainScrollEvent.IsLoading = false;
                                    ShowEmptyPage();
                                }
                                catch (Exception e)
                                {
                                    ShowEmptyPage();
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });
                            return (result.NewNotificationsCount, result.NewFriendRequestsCount, result.CountNewMessages);
                        }
                    }
                    else
                    {
                        if (MainScrollEvent != null) MainScrollEvent.IsLoading = false;
                        Methods.DisplayReportResult(Activity, respond);
                    }
                }
                else
                {
                    ToastUtils.ShowToast(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    if (MainScrollEvent != null) MainScrollEvent.IsLoading = false;
                }

                Activity?.RunOnUiThread(ShowEmptyPage);
                return ("", "", "");
            }
            catch (Exception e)
            {
                Activity?.RunOnUiThread(ShowEmptyPage);
                Methods.DisplayReportResultTrack(e);
                return ("", "", "");
            }
        }

        private async Task LoadActivitiesAsync(string offset = "0")
        {
            if (MainScrollEvent != null && MainScrollEvent.IsLoading) return;

            if (Methods.CheckConnectivity())
            {
                if (AppSettings.ShowLastActivities)
                {
                    if (MainScrollEvent != null)
                        MainScrollEvent.IsLoading = true;

                    var (apiStatus, respond) = await RequestsAsync.Global.GetActivitiesAsync("6", offset);
                    if (apiStatus == 200)
                    {
                        if (respond is LastActivitiesObject result)
                        {
                            // LastActivities
                            var countList = MAdapter.NotificationsList.Count;
                            var respondList = result.Activities.Count;
                            if (respondList > 0)
                            {
                                foreach (var item in from item in result.Activities let check = MAdapter.NotificationsList.FirstOrDefault(a => a.LastActivities?.Id == item.Id) where check == null select item)
                                {
                                    MAdapter.NotificationsList.Add(new Classes.NotificationsClass
                                    {
                                        Type = Classes.ItemType.LastActivities,
                                        LastActivities = item
                                    });
                                }

                                var list = MAdapter.NotificationsList.Where(a => a.Type != Classes.ItemType.LastActivities).ToList();
                                if (list.Count > 0)
                                    foreach (var item in list)
                                        MAdapter.NotificationsList.Remove(item);

                                if (countList > 0)
                                {
                                    Activity?.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.NotificationsList.Count - countList); });
                                }
                                else
                                {
                                    Activity?.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                                }
                            }
                            else
                            {
                                if (MAdapter.NotificationsList.Count > 10 && !MRecycler.CanScrollVertically(1))
                                    ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_NoMoreActivities), ToastLength.Short);
                            }
                        }
                    }
                    else
                    {
                        if (MainScrollEvent != null) MainScrollEvent.IsLoading = false;
                        Methods.DisplayReportResult(Activity, respond);
                    }
                }

                Activity?.RunOnUiThread(ShowEmptyPage);
            }
        }

        private async Task GetFriendsBirthdayApi()
        {
            if (Methods.CheckConnectivity())
            {
                if (AppSettings.ShowFriendsBirthday)
                {
                    var (apiStatus, respond) = await RequestsAsync.Global.GetFriendsBirthdayAsync();
                    if (apiStatus == 200)
                    {
                        if (respond is ListUsersObject result)
                        {
                            MAdapter.NotificationsList.Clear();

                            // FriendsBirthday 
                            var countList = MAdapter.NotificationsList.Count;
                            var respondList = result.Data.Count;
                            if (respondList > 0)
                            {
                                foreach (var item in from item in result.Data let check = MAdapter.NotificationsList.FirstOrDefault(a => a.User?.UserId == item.UserId) where check == null select item)
                                {
                                    MAdapter.NotificationsList.Add(new Classes.NotificationsClass
                                    {
                                        Type = Classes.ItemType.FriendsBirthday,
                                        User = item
                                    });
                                }

                                var list = MAdapter.NotificationsList.Where(a => a.Type != Classes.ItemType.FriendsBirthday).ToList();
                                if (list.Count > 0)
                                    foreach (var item in list)
                                        MAdapter.NotificationsList.Remove(item);

                                if (countList > 0)
                                {
                                    Activity?.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.NotificationsList.Count - countList); });
                                }
                                else
                                {
                                    Activity?.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                                }
                            }
                            else
                            {
                                if (MAdapter.NotificationsList.Count > 10 && !MRecycler.CanScrollVertically(1))
                                    ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_No_more_users), ToastLength.Short);
                            }
                        }
                    }
                    else
                    {
                        Methods.DisplayReportResult(Activity, respond);
                    }

                    Activity?.RunOnUiThread(ShowEmptyPage);
                }
            }
        }

        private void LayoutFriendRequestOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(Activity, typeof(FriendRequestActivity));
                Activity.StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ShowEmptyPage()
        {
            try
            {
                ShimmerInflater?.Hide();

                MainScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;

                switch (MAdapter.NotificationsList.Count)
                {
                    case > 0:
                        MRecycler.Visibility = ViewStates.Visible;
                        EmptyStateLayout.Visibility = ViewStates.Gone;
                        break;
                    default:
                        {
                            MRecycler.Visibility = ViewStates.Gone;

                            Inflated ??= EmptyStateLayout.Inflate();

                            EmptyStateInflater x = new EmptyStateInflater();
                            x.InflateLayout(Inflated, EmptyStateInflater.Type.NoNotifications);
                            switch (x.EmptyStateButton.HasOnClickListeners)
                            {
                                case false:
                                    x.EmptyStateButton.Click += null!;
                                    break;
                            }
                            EmptyStateLayout.Visibility = ViewStates.Visible;
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                ShimmerInflater?.Hide();

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
                if (!Methods.CheckConnectivity())
                    ToastUtils.ShowToast(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                else
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadGeneralData(true) });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async Task LoadReviewsAsync()
        {
            if (Methods.CheckConnectivity())
            {
                var (apiStatus, respond) = await RequestsAsync.Page.GetInvitesAsync("10");
                if (apiStatus == 200)
                {
                    if (respond is GetInvitesObject result)
                    {
                        Activity?.RunOnUiThread(() =>
                        {
                            try
                            {
                                var respondList = result.Data.Count;
                                if (respondList > 0)
                                {
                                    if (CommunitiesRequestInflated == null)
                                    {
                                        CommunitiesRequestInflated = CommunitiesRequestViewStub.Inflate();

                                        var CommunitiesImage = (ImageView)CommunitiesRequestInflated?.FindViewById(Resource.Id.imageFirstUser);
                                        var txTCommunities = (TextView)CommunitiesRequestInflated?.FindViewById(Resource.Id.tv_Friends_connection);
                                        var txtAllCommunities = (TextView)CommunitiesRequestInflated?.FindViewById(Resource.Id.tv_Friends);
                                        var CommunitiesCount = (TextView)CommunitiesRequestInflated?.FindViewById(Resource.Id.count_view);

                                        txtAllCommunities.Visibility = ViewStates.Gone;

                                        CommunitiesRequestInflated.Click += CommunitiesRequestInflatedOnClick;

                                        txTCommunities.Text = Activity.GetText(Resource.String.Lbl_InvitationToPage);

                                        var user = result.Data.FirstOrDefault();
                                        if (user != null)
                                            GlideImageLoader.LoadImage(Activity, user.Avatar, CommunitiesImage, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);

                                        CommunitiesCount.Text = result.Data.Count.ToString();
                                        CommunitiesCount.Visibility = ViewStates.Visible;
                                    }
                                }
                                else
                                {
                                    if (CommunitiesRequestViewStub != null)
                                        CommunitiesRequestViewStub.Visibility = ViewStates.Gone;
                                }
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                    }
                }
                else
                    Methods.DisplayReportResult(Activity, respond);
            }
        }

        private void CommunitiesRequestInflatedOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(Activity, typeof(InvitedPageActivity));
                Activity.StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        public void EventClickNotification(Activity activity, NotificationObject item)
        {
            try
            {
                Intent intent = null;
                switch (item.Type)
                {
                    case "following":
                    case "visited_profile":
                    case "accepted_request":
                        WoWonderTools.OpenProfile(activity, item.Notifier.UserId, string.IsNullOrEmpty(item.Notifier?.Username) ? null : item.Notifier);
                        break;
                    case "invited_page":
                        {
                            intent = new Intent(activity, typeof(InvitedPageActivity));
                            break;
                        }
                    case "liked_page":
                    case "accepted_invite":
                        {
                            intent = new Intent(activity, typeof(PageProfileActivity));
                            //intent.PutExtra("PageObject", JsonConvert.SerializeObject(item));
                            intent.PutExtra("PageId", item.PageId);
                            break;
                        }
                    case "joined_group":
                    case "accepted_join_request":
                    case "added_you_to_group":
                        {
                            intent = new Intent(activity, typeof(GroupProfileActivity));
                            //intent.PutExtra("GroupObject", JsonConvert.SerializeObject(item));
                            intent.PutExtra("GroupId", item.GroupId);
                            break;
                        }
                    case "comment":
                    case "wondered_post":
                    case "wondered_comment":
                    case "reaction":
                    case "wondered_reply_comment":
                    case "comment_mention":
                    case "comment_reply_mention":
                    case "liked_post":
                    case "liked_comment":
                    case "liked_reply_comment":
                    case "post_mention":
                    case "share_post":
                    case "shared_your_post":
                    case "comment_reply":
                    case "also_replied":
                    case "profile_wall_post":
                        {
                            intent = new Intent(activity, typeof(ViewFullPostActivity));
                            intent.PutExtra("Id", item.PostId);
                            // intent.PutExtra("DataItem", JsonConvert.SerializeObject(item));
                            break;
                        }
                    case "going_event":
                        {
                            intent = new Intent(activity, typeof(EventViewActivity));
                            intent.PutExtra("EventId", item.EventId);
                            if (item.Event != null)
                                intent.PutExtra("EventView", JsonConvert.SerializeObject(item.Event));
                            break;
                        }
                    case "viewed_story":
                        {
                            //"url": "https:\/\/demo.wowonder.com\/timeline&u=Matan&story=true&story_id=1946",
                            //var id = item.Url.Split("/").Last().Split("&story_id=").Last();

                            StoryDataObject dataMyStory = GlobalContext?.NewsFeedTab?.PostFeedAdapter?.HolderStory?.StoryAdapter?.StoryList?.FirstOrDefault(o => o.UserId == UserDetails.UserId);
                            if (dataMyStory != null)
                            {
                                ObservableCollection<StoryDataObject> storyList = new ObservableCollection<StoryDataObject> { dataMyStory };

                                intent = new Intent(activity, typeof(StoryDetailsActivity));
                                intent.PutExtra("UserId", dataMyStory.UserId);
                                intent.PutExtra("IndexItem", 0);
                                intent.PutExtra("StoriesCount", storyList.Count);
                                intent.PutExtra("DataItem", JsonConvert.SerializeObject(storyList));
                            }

                            break;
                        }
                    case "requested_to_join_group":
                        {
                            intent = new Intent(activity, typeof(JoinRequestActivity));
                            intent.PutExtra("GroupId", item.GroupId);
                            break;
                        }
                    case "memory":
                        {
                            intent = new Intent(activity, typeof(MemoriesActivity));
                            break;
                        }
                    case "gift":
                        {
                            var ajaxUrl = item.AjaxUrl.Split(new[] { "&", "gift_img=" }, StringSplitOptions.None);
                            var urlImage = WoWonderTools.GetTheFinalLink(ajaxUrl?[3]?.Replace("%2F", "/"));

                            intent = new Intent(activity, typeof(UserProfileActivity));

                            if (!string.IsNullOrEmpty(item.Notifier.Username))
                                intent.PutExtra("UserObject", JsonConvert.SerializeObject(item.Notifier));

                            intent.PutExtra("UserId", item.Notifier.UserId);
                            intent.PutExtra("GifLink", urlImage);
                            break;
                        }
                    case "admin_notification":
                        {
                            var postId = item.Url.Split("/").Last();

                            intent = new Intent(activity, typeof(ViewFullPostActivity));
                            intent.PutExtra("Id", postId);
                            // intent.PutExtra("DataItem", JsonConvert.SerializeObject(item));
                            break;
                        }
                    case "live_video":
                        {
                            //wael after update api .. change to live and check Is Still Live
                            WoWonderTools.OpenProfile(activity, item.Notifier.UserId, string.IsNullOrEmpty(item.Notifier?.Username) ? null : item.Notifier);
                            return;
                        }
                    case "remaining":
                        {
                            //nothing
                            break;
                        }
                    default:
                        {
                            WoWonderTools.OpenProfile(activity, item.Notifier.UserId, string.IsNullOrEmpty(item.Notifier?.Username) ? null : item.Notifier);
                            return;
                        }
                }

                if (intent != null)
                    activity.StartActivity(intent);

            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #region Announcement

        private void AnnouncementImageOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(TextAnnouncement))
                    OpenDialogAnnouncement(TextAnnouncement);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void OpenDialogAnnouncement(string textAnnouncement)
        {
            try
            {
                Dialog mAlertDialog = new Dialog(Context);
                mAlertDialog.RequestWindowFeature((int)WindowFeatures.NoTitle); // before
                mAlertDialog.SetContentView(Resource.Layout.DialogAnnouncement);
                mAlertDialog.SetCancelable(false);
                mAlertDialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));

                var subTitle = mAlertDialog?.FindViewById<SuperTextView>(Resource.Id.text);
                TextSanitizer headlineSanitizer = new TextSanitizer(subTitle, GlobalContext);
                headlineSanitizer.Load(Methods.FunString.DecodeString(textAnnouncement));

                ImageView closeButton = mAlertDialog.FindViewById<ImageView>(Resource.Id.CloseButton);

                closeButton.Click += (sender, args) =>
                {
                    try
                    {
                        mAlertDialog.Hide();
                        mAlertDialog.Dismiss();
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                };

                mAlertDialog.Show();
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        #endregion
    }
}