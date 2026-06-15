using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Content;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Com.Google.Android.Gms.Ads;
using DE.Hdodenhof.CircleImageViewLib;
using Google.Android.Material.AppBar;
using Google.Android.Material.Dialog;
using Java.IO;
using Java.Lang;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WoWonder.Activities.Base;
using WoWonder.Activities.Contacts;
using WoWonder.Activities.Gift;
using WoWonder.Activities.Live.Utils;
using WoWonder.Activities.MyProfile;
using WoWonder.Activities.NativePost.Extra;
using WoWonder.Activities.NativePost.Pages;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Activities.SearchForPosts;
using WoWonder.Activities.SettingsPreferences.TellFriend;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.ShimmerUtils;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.Share;
using WoWonder.Library.Anjo.Share.Abstractions;
using WoWonder.SQLite;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Message;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Classes.Product;
using WoWonderClient.Classes.User;
using WoWonderClient.Requests;
using static WoWonder.Activities.NativePost.Extra.WRecyclerView;
using Console = System.Console;
using Exception = System.Exception;
using String = Java.Lang.String;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.UserProfile
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", WindowSoftInputMode = SoftInput.AdjustNothing | SoftInput.AdjustPan, ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class UserProfileActivity : BaseActivity, IDialogListCallBack, IDialogInputCallBack, AppBarLayout.IOnOffsetChangedListener
    {
        #region Variables Basic

        private AppBarLayout AppBarLayout;
        private CollapsingToolbarLayout CollapsingToolbar;

        private ImageView ImageCover, ImageBack;
        private ImageButton BtnMore;
        private TextView TxtSearchForPost;

        private CircleImageView ImageAvatar;
        private TextViewWithImages TxtName;
        private TextView TxtUsername;
        private AppCompatButton BtnFollow;
        private FrameLayout BtnMessage, BtnTopMore;

        private TextView CountFollowers, CountFollowings, CountLikes, CountPoints, TxtFollowers, TxtFollowing;
        private LinearLayout LlCountFollowers, LlCountFollowing, LlCountLike, LlPoint;

        private SwipeRefreshLayout SwipeRefreshLayout;
        private WRecyclerView MainRecyclerView;

        private AdView MAdView;
        private static UserProfileActivity Instance;

        private FeedCombiner Combiner;
        private UserDataObject UserData;

        public NativePostAdapter PostFeedAdapter;
        public static string SUserId;
        private string SUserName, GifLink, SPrivacyFriend;
        private bool IsPoked, IsFamily;

        private ViewStub ShimmerPageLayout;
        private View InflatedShimmer;
        private TemplateShimmerInflater ShimmerInflater;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.Overlap_Dark : Resource.Style.Overlap_Light);

                Methods.App.FullScreenApp(this);

                Overlap();

                // Create your application here
                SetContentView(Resource.Layout.UserProfileLayout);

                Instance = this;

                SUserName = Intent?.GetStringExtra("name") ?? string.Empty;
                SUserId = Intent?.GetStringExtra("UserId") ?? string.Empty;
                GifLink = Intent?.GetStringExtra("GifLink") ?? string.Empty;

                var userObject = Intent?.GetStringExtra("UserObject");
                UserData = string.IsNullOrEmpty(userObject) == false ? JsonConvert.DeserializeObject<UserDataObject>(userObject) : UserData;

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                InitShimmer();
                SetRecyclerViewAdapters();

                switch (string.IsNullOrEmpty(SUserName))
                {
                    case false:
                        GetDataUserByName();
                        break;
                    default:
                        {
                            if (UserData != null)
                                LoadPassedDate(UserData);

                            StartApiService();
                            break;
                        }
                }
                GetGiftsLists();

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
                MainRecyclerView?.StopVideo();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnStop()
        {
            try
            {
                base.OnStop();
                MainRecyclerView?.StopVideo();
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
                Console.WriteLine("WoLog: OnLowMemory  >> USerProfile = ");

                base.OnLowMemory();
                Glide.With(this).OnLowMemory();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                Console.WriteLine("WoLog: OnTrimMemory  >> UserProfile TrimMemory = " + level);
                //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);

                base.OnTrimMemory(level);
                Glide.With(this).OnTrimMemory(level);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        protected override void OnDestroy()
        {
            try
            {
                MainRecyclerView?.ReleasePlayer();
                PostClickListener.OpenMyProfile = false;
                AdsGoogle.LifecycleAdView(MAdView, "Destroy");
                DestroyBasic();
                base.OnDestroy();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Functions

        private void Overlap()
        {
            try
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                {
                    Window.ClearFlags(WindowManagerFlags.TranslucentStatus);
                    Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                    Window.SetStatusBarColor(Color.Transparent);
#pragma warning disable 618
                    Window.DecorView.SystemUiVisibility = (StatusBarVisibility)SystemUiFlags.LayoutFullscreen | (StatusBarVisibility)SystemUiFlags.LayoutStable;
#pragma warning restore 618
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitComponent()
        {
            try
            {
                AppBarLayout = FindViewById<AppBarLayout>(Resource.Id.appBarLayout);
                AppBarLayout.SetExpanded(true);
                AppBarLayout.AddOnOffsetChangedListener(this);

                CollapsingToolbar = (CollapsingToolbarLayout)FindViewById(Resource.Id.collapsingToolbar);
                CollapsingToolbar.Title = " ";

                ImageCover = FindViewById<ImageView>(Resource.Id.cover_image);
                ImageBack = FindViewById<ImageView>(Resource.Id.back);
                BtnMore = FindViewById<ImageButton>(Resource.Id.BtnMore);

                TxtSearchForPost = FindViewById<TextView>(Resource.Id.tv_SearchForPost);
                TxtSearchForPost.Visibility = ViewStates.Invisible;

                MainRecyclerView = FindViewById<WRecyclerView>(Resource.Id.newsfeedRecyler);
                MainRecyclerView.Visibility = ViewStates.Gone;

                SwipeRefreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(WoWonderTools.IsTabDark() ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));

                ImageAvatar = FindViewById<CircleImageView>(Resource.Id.profileimage_head);
                TxtName = FindViewById<TextViewWithImages>(Resource.Id.name_profile);
                TxtUsername = FindViewById<TextView>(Resource.Id.username_profile);
                BtnFollow = FindViewById<AppCompatButton>(Resource.Id.btnFollow);

                BtnMessage = FindViewById<FrameLayout>(Resource.Id.btnMessage);
                BtnTopMore = FindViewById<FrameLayout>(Resource.Id.BtnTopMore);

                CountFollowers = FindViewById<TextView>(Resource.Id.CountFollowers);
                CountFollowings = FindViewById<TextView>(Resource.Id.CountFollowing);
                CountLikes = FindViewById<TextView>(Resource.Id.CountLikes);
                CountPoints = FindViewById<TextView>(Resource.Id.CountPoints);

                TxtFollowers = FindViewById<TextView>(Resource.Id.txtFollowers);
                TxtFollowing = FindViewById<TextView>(Resource.Id.txtFollowing);

                LlCountFollowers = FindViewById<LinearLayout>(Resource.Id.CountFollowersLayout);
                LlCountFollowing = FindViewById<LinearLayout>(Resource.Id.CountFollowingLayout);
                LlCountLike = FindViewById<LinearLayout>(Resource.Id.CountLikesLayout);
                LlPoint = FindViewById<LinearLayout>(Resource.Id.CountPointsLayout);

                switch (AppSettings.ConnectivitySystem)
                {
                    // Following
                    case 1:
                        TxtFollowers.Text = GetText(Resource.String.Lbl_Followers);
                        TxtFollowing.Text = GetText(Resource.String.Lbl_Following);
                        break;
                    // Friend
                    default:
                        TxtFollowers.Text = GetText(Resource.String.Lbl_Friends);
                        TxtFollowing.Text = GetText(Resource.String.Lbl_Post);
                        break;
                }

                if (!AppSettings.ShowUserPoint)
                    LlPoint.Visibility = ViewStates.Gone;

                if (!AppSettings.MessengerIntegration)
                    BtnMessage.Visibility = ViewStates.Gone;

                MAdView = FindViewById<AdView>(Resource.Id.adView);
                AdsGoogle.InitAdView(MAdView, MainRecyclerView);
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
                    toolBar.Title = "";
                    SetSupportActionBar(toolBar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(false);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitShimmer()
        {
            try
            {
                ShimmerPageLayout = FindViewById<ViewStub>(Resource.Id.viewStubShimmer);
                InflatedShimmer ??= ShimmerPageLayout.Inflate();

                ShimmerInflater = new TemplateShimmerInflater();
                ShimmerInflater.InflateLayout(this, InflatedShimmer, ShimmerTemplateStyle.PostTemplate);
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
                PostFeedAdapter = new NativePostAdapter(this, SUserId, MainRecyclerView, NativeFeedType.User);
                MainRecyclerView.SetXAdapter(PostFeedAdapter, SwipeRefreshLayout);
                MainRecyclerView.SetXTemplateShimmer(ShimmerInflater);
                Combiner = new FeedCombiner(null, PostFeedAdapter?.ListDiffer, this, NativeFeedType.User);

                MainRecyclerView.MainScrollEvent = new RecyclerScrollListener(MainRecyclerView);
                MainRecyclerView.AddOnScrollListener(MainRecyclerView.MainScrollEvent);
                MainRecyclerView.MainScrollEvent.LoadMoreEvent += MainRecyclerView.MainScrollEvent_LoadMoreEvent;
                MainRecyclerView.MainScrollEvent.IsLoading = false;
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
                        ImageCover.Click += ImageCoverOnClick;
                        ImageBack.Click += ImageBackOnClick;
                        BtnMore.Click += BtnMoreOnClick;
                        BtnTopMore.Click += BtnMoreOnClick;
                        SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                        TxtSearchForPost.Click += TxtSearchForPostOnClick;

                        ImageAvatar.Click += ImageAvatarOnClick;
                        BtnFollow.Click += BtnFollowOnClick;
                        BtnMessage.Click += BtnMessageOnClick;
                        LlCountFollowers.Click += LlCountFollowersOnClick;
                        LlCountFollowing.Click += LlCountFollowingOnClick;
                        LlCountLike.Click += LlCountLikeOnClick;
                        LlPoint.Click += LlPointOnClick;
                        break;
                    default:
                        ImageCover.Click -= ImageCoverOnClick;
                        ImageBack.Click -= ImageBackOnClick;
                        BtnMore.Click -= BtnMoreOnClick;
                        BtnTopMore.Click -= BtnMoreOnClick;
                        SwipeRefreshLayout.Refresh -= SwipeRefreshLayoutOnRefresh;
                        TxtSearchForPost.Click -= TxtSearchForPostOnClick;

                        ImageAvatar.Click -= ImageAvatarOnClick;
                        BtnFollow.Click -= BtnFollowOnClick;
                        BtnMessage.Click -= BtnMessageOnClick;
                        LlCountFollowers.Click -= LlCountFollowersOnClick;
                        LlCountFollowing.Click -= LlCountFollowingOnClick;
                        LlCountLike.Click -= LlCountLikeOnClick;
                        LlPoint.Click -= LlPointOnClick;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static UserProfileActivity GetInstance()
        {
            try
            {
                return Instance;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null;
            }
        }

        private void DestroyBasic()
        {
            try
            {
                ImageCover = null!;
                ImageBack = null!;
                BtnMore = null!;
                Instance = null!;
                SwipeRefreshLayout = null!;
                MainRecyclerView = null!;
                MAdView = null!;
                Combiner = null!;
                UserData = null!;
                PostFeedAdapter = null!;
                SUserId = null!;
                SUserName = null!;
                GifLink = null!;
                SPrivacyFriend = null!;
                UserDetails.DataLivePost = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event

        private void LlCountFollowersOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(MyContactsActivity));
                intent.PutExtra("ContactsType", "Followers");
                intent.PutExtra("UserId", SUserId);

                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void LlCountFollowingOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(MyContactsActivity));
                intent.PutExtra("ContactsType", "Following");
                intent.PutExtra("UserId", SUserId);
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void LlCountLikeOnClick(object sender, EventArgs e)
        {

        }

        private void LlPointOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(MyPointsActivity));
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtSearchForPostOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(SearchForPostsActivity));
                intent.PutExtra("TypeSearch", "user");
                intent.PutExtra("IdSearch", SUserId);
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    return;
                }

                ShimmerInflater?.Show();

                PostFeedAdapter?.ListDiffer?.Clear();
                PostFeedAdapter?.NotifyDataSetChanged();

                if (UserData != null)
                    LoadPassedDate(UserData);

                MainRecyclerView.MainScrollEvent.IsLoading = false;
                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialAlertDialogBuilder(this);

                arrayAdapter.Add(GetText(Resource.String.Lbl_CopeLink));
                arrayAdapter.Add(GetText(Resource.String.Lbl_Share));
                arrayAdapter.Add(GetText(Resource.String.Lbl_Block));

                arrayAdapter.Add(!UserData.IsReported ? GetText(Resource.String.Lbl_ReportThisUser) : GetText(Resource.String.Lbl_CancelReport));

                switch (AppSettings.ShowPokes)
                {
                    case true:
                        arrayAdapter.Add(IsPoked ? GetText(Resource.String.Lbl_Poked) : GetText(Resource.String.Lbl_Poke));
                        break;
                }

                switch (IsFamily)
                {
                    case false when AppSettings.ShowAddToFamily:
                        arrayAdapter.Add(GetText(Resource.String.Lbl_AddToFamily));
                        break;
                }

                switch (AppSettings.ShowGift)
                {
                    case true when ListUtils.GiftsList.Count > 0:
                        arrayAdapter.Add(GetText(Resource.String.Lbl_SentGift));
                        break;
                }

                dialogList.SetTitle(Resource.String.Lbl_More);
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ImageAvatarOnClick(object sender, EventArgs e)
        {
            try
            {
                OptionAvatarProfileDialogFragment dialogFragment = new OptionAvatarProfileDialogFragment();
                Bundle bundle = new Bundle();
                bundle.PutString("Page", "UserProfile");
                bundle.PutString("UserData", JsonConvert.SerializeObject(UserData));

                dialogFragment.Arguments = bundle;

                dialogFragment.Show(SupportFragmentManager, dialogFragment.Tag);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnFollowOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
                else
                {
                    WoWonderTools.SetAddFriend(this, UserData, BtnFollow);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnMessageOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!WoWonderTools.ChatIsAllowed(UserData))
                    return;

                if (AppSettings.ShowDialogAskOpenMessenger)
                {
                    var dialog = new MaterialAlertDialogBuilder(this);

                    dialog.SetTitle(Resource.String.Lbl_Warning);
                    dialog.SetMessage(GetText(Resource.String.Lbl_ContentAskOPenAppMessenger));
                    dialog.SetPositiveButton(GetText(Resource.String.Lbl_Yes), (materialDialog, action) =>
                    {
                        try
                        {
                            Methods.App.OpenAppByPackageName(this, AppSettings.MessengerPackageName, "OpenChat", new ChatObject { UserId = UserData.UserId, Name = UserData.Name, Avatar = UserData.Avatar });
                        }
                        catch (Exception exception)
                        {
                            Methods.DisplayReportResultTrack(exception);
                        }
                    });
                    dialog.SetNegativeButton(GetText(Resource.String.Lbl_No), new MaterialDialogUtils());

                    dialog.Show();
                }
                else
                {
                    Methods.App.OpenAppByPackageName(this, AppSettings.MessengerPackageName, "OpenChat", new ChatObject { UserId = UserData.UserId, Name = UserData.Name, Avatar = UserData.Avatar });
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        private void ImageBackOnClick(object sender, EventArgs e)
        {
            try
            {
                Finish();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ImageCoverOnClick(object sender, EventArgs e)
        {
            try
            {
                if (UserData.Cover.Contains("d-cover"))
                    return;

                if (!string.IsNullOrEmpty(UserData.CoverPostId) && UserData.CoverPostId != "0")
                {
                    var intent = new Intent(this, typeof(ViewFullPostActivity));
                    intent.PutExtra("Id", UserData.CoverPostId);
                    //intent.PutExtra("DataItem", JsonConvert.SerializeObject(e.NewsFeedClass));
                    StartActivity(intent);
                }
                else
                {
                    var media = WoWonderTools.GetFile("", Methods.Path.FolderDiskImage, UserData.Cover.Split('/').Last(), UserData.Cover);
                    if (media.Contains("http"))
                    {
                        var intent = new Intent(Intent.ActionView, Uri.Parse(media));
                        StartActivity(intent);
                    }
                    else
                    {
                        var file2 = new File(media);
                        var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);

                        var intent = new Intent(Intent.ActionPick);
                        intent.SetAction(Intent.ActionView);
                        intent.AddFlags(ActivityFlags.GrantReadUriPermission);
                        intent.SetDataAndType(photoUri, "image/*");
                        StartActivity(intent);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Get Profile

        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { GetProfileApi });
        }

        private async Task GetProfileApi()
        {
            string fetch = "user_data,following";

            if (AppSettings.ShowUserPage)
                fetch += ",liked_pages";

            if (AppSettings.ShowUserGroup)
                fetch += ",joined_groups";

            if (AppSettings.ShowAddToFamily)
                fetch += ",family";

            var (apiStatus, respond) = await RequestsAsync.Global.GetUserDataAsync(SUserId, fetch);
            if (apiStatus != 200 || respond is not GetUserDataObject result || result.UserData == null)
            {
                Methods.DisplayReportResult(this, respond);
            }
            else
            {
                RunOnUiThread(() =>
                {
                    try
                    {
                        LoadPassedDate(result.UserData);

                        if (AppSettings.ShowAddToFamily)
                            if (result.Family?.Count > 0)
                            {
                                var data = result.Family.FirstOrDefault(o => o.UserData.UserId == UserDetails.UserId);
                                IsFamily = data != null!;
                            }
                            else
                            {
                                IsFamily = false;
                            }

                        if (AppSettings.ShowUserPage && result.LikedPages?.Count > 0)
                            LoadPagesLayout(result.LikedPages);

                        if (AppSettings.ShowUserGroup)
                        {
                            if (result.JoinedGroups?.Count > 0 && result.UserData.Details.DetailsClass != null)
                                LoadGroupsLayout(result.JoinedGroups, Methods.FunString.FormatPriceValue(Convert.ToInt32(result.UserData.Details.DetailsClass.GroupsCount)));
                            else if (result.JoinedGroups?.Count > 0)
                                LoadGroupsLayout(result.JoinedGroups, Methods.FunString.FormatPriceValue(result.JoinedGroups.Count));
                        }

                        if (SPrivacyFriend == "0" || result.UserData?.IsFollowing == "1" && SPrivacyFriend == "1" || SPrivacyFriend == "2")
                            if (result.Following?.Count > 0 && result.UserData.Details.DetailsClass != null)
                                LoadFriendsLayout(result.Following, Methods.FunString.FormatPriceValue(Convert.ToInt32(result.UserData.Details.DetailsClass.FollowingCount)));
                            else if (result.Following?.Count > 0)
                                LoadFriendsLayout(result.Following, Methods.FunString.FormatPriceValue(result.Following.Count));

                        string postPrivacy;
                        if (result.UserData.PostPrivacy.Contains("everyone"))
                        {
                            postPrivacy = "1";
                        }
                        else if (result.UserData.PostPrivacy.Contains("ifollow") && result.UserData?.IsFollowing == "1" && result.UserData?.IsFollowingMe == "1")
                        {
                            postPrivacy = "1";
                        }
                        else if (result.UserData.PostPrivacy.Contains("me")) //Lbl_People_Follow_Me
                        {
                            postPrivacy = "1";
                        }
                        else // Lbl_No_body
                        {
                            postPrivacy = "0";
                        }

                        //##Set the AddBox place on Main RecyclerView
                        //------------------------------------------------------------------------
                        var check7 = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.SocialLinks);
                        var check = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.PagesBox);
                        var check2 = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.GroupsBox);
                        var check3 = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.FollowersBox);
                        var check4 = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.ImagesBox);
                        var check5 = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.AboutBox);
                        var check6 = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.InfoUserBox);
                        var check8 = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.EmptyState);

                        if (postPrivacy == "1")
                        {
                            if (check7 != null)
                            {
                                Combiner.AddPostDivider(PostFeedAdapter.ListDiffer.IndexOf(check7) + 1);
                                Combiner.AddPostBoxPostView("user", PostFeedAdapter.ListDiffer.IndexOf(check7) + 2);
                            }
                            else if (check != null)
                            {
                                Combiner.AddPostDivider(PostFeedAdapter.ListDiffer.IndexOf(check) + 1);
                                Combiner.AddPostBoxPostView("user", PostFeedAdapter.ListDiffer.IndexOf(check) + 2);
                            }
                            else if (check2 != null)
                            {
                                Combiner.AddPostDivider(PostFeedAdapter.ListDiffer.IndexOf(check2) + 1);
                                Combiner.AddPostBoxPostView("user", PostFeedAdapter.ListDiffer.IndexOf(check2) + 2);
                            }
                            else if (check3 != null)
                            {
                                Combiner.AddPostDivider(PostFeedAdapter.ListDiffer.IndexOf(check3) + 1);
                                Combiner.AddPostBoxPostView("user", PostFeedAdapter.ListDiffer.IndexOf(check3) + 2);
                            }
                            else if (check4 != null)
                            {
                                Combiner.AddPostDivider(PostFeedAdapter.ListDiffer.IndexOf(check4) + 1);
                                Combiner.AddPostBoxPostView("user", PostFeedAdapter.ListDiffer.IndexOf(check4) + 2);
                            }
                            else if (check5 != null)
                            {
                                Combiner.AddPostDivider(PostFeedAdapter.ListDiffer.IndexOf(check5) + 1);
                                Combiner.AddPostBoxPostView("user", PostFeedAdapter.ListDiffer.IndexOf(check5) + 2);
                            }
                            else if (check6 != null)
                            {
                                Combiner.AddPostDivider(PostFeedAdapter.ListDiffer.IndexOf(check6) + 1);
                                Combiner.AddPostBoxPostView("user", PostFeedAdapter.ListDiffer.IndexOf(check6) + 2);
                            }
                            else if (check8 != null)
                            {
                                Combiner.AddPostDivider(PostFeedAdapter.ListDiffer.IndexOf(check8) + 1);
                                Combiner.AddPostBoxPostView("user", PostFeedAdapter.ListDiffer.IndexOf(check8) + 2);
                            }

                            //switch (AppSettings.ShowSearchForPosts)
                            //{
                            //    case true:
                            //        Combiner.SearchForPostsView("user");
                            //        break;
                            //}
                        }
                        else
                        {
                            if (check7 != null)
                            {
                                Combiner.AddPostDivider(PostFeedAdapter.ListDiffer.IndexOf(check7) + 1);
                            }
                            else if (check != null)
                            {
                                Combiner.AddPostDivider(PostFeedAdapter.ListDiffer.IndexOf(check) + 1);
                            }
                            else if (check2 != null)
                            {
                                Combiner.AddPostDivider(PostFeedAdapter.ListDiffer.IndexOf(check2) + 1);
                            }
                            else if (check3 != null)
                            {
                                Combiner.AddPostDivider(PostFeedAdapter.ListDiffer.IndexOf(check3) + 1);
                            }
                            else if (check4 != null)
                            {
                                Combiner.AddPostDivider(PostFeedAdapter.ListDiffer.IndexOf(check4) + 1);
                            }
                            else if (check5 != null)
                            {
                                Combiner.AddPostDivider(PostFeedAdapter.ListDiffer.IndexOf(check5) + 1);
                            }
                            else if (check6 != null)
                            {
                                Combiner.AddPostDivider(PostFeedAdapter.ListDiffer.IndexOf(check6) + 1);
                            }
                            else if (check8 != null)
                            {
                                Combiner.AddPostDivider(PostFeedAdapter.ListDiffer.IndexOf(check8) + 1);
                            }
                        }

                        PostFeedAdapter?.SetLoading();

                        var d = new Runnable(() =>
                        {
                            PostFeedAdapter?.NotifyDataSetChanged();
                        }); d.Run();
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });

                //------------------------------------------------------------------------ 
                await GetAlbumUserApi();

                MainRecyclerView.ApiPostAsync.ExcuteDataToMainThread();

                //PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => MainRecyclerView.ApiPostAsync.FetchNewsFeedApiPosts() });
            }
        }

        private async Task GetAlbumUserApi()
        {
            if (!AppSettings.ShowUserImage)
                return;

            var (apiStatus, respond) = await RequestsAsync.Album.GetPostByTypeAsync(SUserId, "photos", "10");
            if (apiStatus != 200 || respond is not PostObject result || result.Data == null)
            {
                Methods.DisplayReportResult(this, respond);
            }
            else
            {
                switch (result.Data?.Count)
                {
                    case > 0:
                        {
                            result.Data.RemoveAll(w => string.IsNullOrEmpty(w.PostFileFull));

                            var count = result.Data.Count;
                            switch (count)
                            {
                                case > 10:
                                    RunOnUiThread(() => { LoadImagesLayout(result.Data.Take(9).ToList(), Methods.FunString.FormatPriceValue(Convert.ToInt32(count.ToString()))); });
                                    break;
                                case > 5:
                                    RunOnUiThread(() => { LoadImagesLayout(result.Data.Take(6).ToList(), Methods.FunString.FormatPriceValue(Convert.ToInt32(count.ToString()))); });
                                    break;
                                case > 0:
                                    RunOnUiThread(() => { LoadImagesLayout(result.Data.ToList(), Methods.FunString.FormatPriceValue(Convert.ToInt32(count.ToString()))); });
                                    break;
                            }

                            break;
                        }
                }
            }
        }

        private void LoadPassedDate(UserDataObject result)
        {
            try
            {
                if (string.IsNullOrEmpty(result.IsFollowingMe))
                    result.IsFollowingMe = "";

                UserData = result;

                switch (AppSettings.CoverImageStyle)
                {
                    case CoverImageStyle.CenterCrop:
                        Glide.With(this).Load(result.Cover.Replace(" ", "")).Apply(new RequestOptions().CenterCrop().Error(Resource.Drawable.Cover_image)).Into(ImageCover);
                        break;
                    case CoverImageStyle.FitCenter:
                        Glide.With(this).Load(result.Cover.Replace(" ", "")).Apply(new RequestOptions().FitCenter().Error(Resource.Drawable.Cover_image)).Into(ImageCover);
                        break;
                    default:
                        Glide.With(this).Load(result.Cover.Replace(" ", "")).Apply(new RequestOptions().Error(Resource.Drawable.Cover_image)).Into(ImageCover);
                        break;
                }

                if (result.Avatar.Contains("d-avatar") || result.Avatar.Contains("f-avatar"))
                {
                    if (result.Avatar.Contains("f-avatar"))
                        GlideImageLoader.LoadImage(this, "no_profile_female_image_circle", ImageAvatar, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);
                    else
                        GlideImageLoader.LoadImage(this, "no_profile_image_circle", ImageAvatar, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);
                }
                else
                {
                    GlideImageLoader.LoadImage(this, result.Avatar, ImageAvatar, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);
                }

                if (!WoWonderTools.StoryIsAvailable(result.UserId))
                    ImageAvatar.BorderColor = WoWonderTools.IsTabDark() ? Color.ParseColor("#060606") : Color.White;
                else
                    ImageAvatar.BorderColor = Color.ParseColor(AppSettings.MainColor);

                TxtUsername.Text = "@" + result.Username + " " + Methods.Time.TimeAgo(Convert.ToInt32(result.LastseenUnixTime), false);

                var name = WoWonderTools.GetNameFinal(result);
                var textHighLighter = name;

                if (result.Verified == "1")
                    textHighLighter += " " + "[img src=icon_checkmark_small_vector/]";

                if (result.IsPro == "1")
                    textHighLighter += " " + "[img src=post_icon_flash/]";

                TextViewWithImages.Publisher = result;
                var decoratedContent = TextViewWithImages.GetTextWithImages(null, this, new String(textHighLighter.ToArray(), 0, textHighLighter.Length));

                TxtName.SetText(decoratedContent);

                WoWonderTools.SetAddFriendCondition(UserData, UserData.IsFollowing, BtnFollow);

                string followers = Methods.FunString.FormatPriceValue(Convert.ToInt32(result.Details.DetailsClass.FollowersCount));
                string following = Methods.FunString.FormatPriceValue(Convert.ToInt32(result.Details.DetailsClass.FollowingCount));
                string post = Methods.FunString.FormatPriceValue(Convert.ToInt32(result.Details.DetailsClass.PostCount));
                string likes = Methods.FunString.FormatPriceValue(Convert.ToInt32(result.Details.DetailsClass.LikesCount));
                string points = Methods.FunString.FormatPriceValue(Convert.ToInt32(result.Points));

                CountFollowers.Text = followers;
                CountFollowings.Text = following;
                CountLikes.Text = likes;
                CountPoints.Text = points;

                switch (AppSettings.ConnectivitySystem)
                {
                    // Following
                    case 1:
                        CountFollowers.Text = followers;
                        CountFollowings.Text = following;
                        break;
                    // Friend
                    default:
                        CountFollowers.Text = followers;
                        CountFollowings.Text = post;
                        break;
                }

                var checkAboutBox = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.AboutBox);
                switch (checkAboutBox)
                {
                    case null:
                        Combiner.AboutBoxPostView(WoWonderTools.GetAboutFinal(result), 0);
                        break;
                    default:
                        checkAboutBox.AboutModel.Description = WoWonderTools.GetAboutFinal(result);
                        break;
                }

                var checkInfoUserBox = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.InfoUserBox);
                switch (checkInfoUserBox)
                {
                    case null:
                        Combiner.InfoUserBoxPostView(result, 1);
                        break;
                    default:
                        checkInfoUserBox.InfoUserModel.UserData = result;
                        break;
                }

                if (AppSettings.ShowUserSocialLinks)
                {
                    var checkSocialBox = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.SocialLinks);
                    switch (checkSocialBox)
                    {
                        case null:
                            {
                                var socialLinksModel = new SocialLinksModelClass
                                {
                                    Facebook = result.Facebook,
                                    Instegram = result.Instagram,
                                    Twitter = result.Twitter,
                                    Google = result.Google,
                                    Vk = result.Vk,
                                    Youtube = result.Youtube,
                                };

                                Combiner.SocialLinksBoxPostView(socialLinksModel, -1);
                                //PostFeedAdapter?.NotifyItemInserted(0);
                                break;
                            }
                        default:
                            checkSocialBox.SocialLinksModel.Facebook = result.Facebook;
                            checkSocialBox.SocialLinksModel.Instegram = result.Instagram;
                            checkSocialBox.SocialLinksModel.Twitter = result.Twitter;
                            checkSocialBox.SocialLinksModel.Google = result.Google;
                            checkSocialBox.SocialLinksModel.Vk = result.Vk;
                            checkSocialBox.SocialLinksModel.Youtube = result.Youtube;
                            break;
                    }
                }

                //Set Privacy User
                //==================================
                SPrivacyFriend = result.FriendPrivacy;

                if (!result.Cover.Contains("d-cover"))
                    WoWonderTools.GetFile("", Methods.Path.FolderDiskImage, result.Cover.Split('/').Last(), result.Cover);

                if (!result.Avatar.Contains("d-avatar") || !result.Avatar.Contains("f-avatar"))
                    WoWonderTools.GetFile("", Methods.Path.FolderDiskImage, result.Avatar.Split('/').Last(), result.Avatar);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadFriendsLayout(List<UserDataObject> following, string friendsCounter)
        {
            try
            {
                if (following?.Count > 0)
                {
                    var followersClass = new FollowingModelClass
                    {
                        TitleHead = GetText(AppSettings.ConnectivitySystem == 1 ? Resource.String.Lbl_Following : Resource.String.Lbl_Friends),
                        FollowingList = new List<UserDataObject>(following.Take(6)),
                        Description = friendsCounter + " " + GetText(AppSettings.ConnectivitySystem == 1 ? Resource.String.Lbl_Following : Resource.String.Lbl_Friends),
                        More = GetText(Resource.String.Lbl_SeeAll) + " " + GetText(AppSettings.ConnectivitySystem == 1 ? Resource.String.Lbl_Following : Resource.String.Lbl_Friends)
                    };

                    var check = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.FollowersBox);
                    if (check != null)
                    {
                        check.FollowersModel = followersClass;
                        //PostFeedAdapter?.NotifyItemInserted(postFeedAdapter.ListDiffer.IndexOf(check) + 1);
                    }
                    else
                    {
                        Combiner.FollowersBoxPostView(followersClass, AppSettings.ShowUserSocialLinks ? 3 : 2);

                        //PostFeedAdapter?.NotifyItemInserted(1);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadImagesLayout(List<PostDataObject> images, string imagesCounter)
        {
            try
            {
                if (images?.Count > 0)
                {
                    var imagesClass = new ImagesModelClass
                    {
                        TitleHead = GetText(Resource.String.Lbl_Profile_Picture),
                        ImagesList = images,
                        More = imagesCounter
                    };

                    var check = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.ImagesBox);
                    if (check != null)
                    {
                        check.ImagesModel = imagesClass;
                        //PostFeedAdapter?.NotifyItemInserted(postFeedAdapter.ListDiffer.IndexOf(check) + 1);
                    }
                    else
                    {
                        var checkFollowersBox = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.FollowersBox);
                        if (checkFollowersBox != null)
                            Combiner?.ImagesBoxPostView(imagesClass, PostFeedAdapter.ListDiffer.IndexOf(checkFollowersBox));
                        else
                            Combiner?.ImagesBoxPostView(imagesClass, AppSettings.ShowUserSocialLinks ? 3 : 2);

                        //PostFeedAdapter?.NotifyItemInserted(1);
                    }

                    RunOnUiThread(() => { PostFeedAdapter?.NotifyDataSetChanged(); });
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadPagesLayout(List<PageDataObject> pages)
        {
            try
            {
                if (pages?.Count > 0)
                {
                    var checkNull = pages.Where(a => string.IsNullOrEmpty(a.PageId)).ToList();
                    if (checkNull.Count > 0)
                        foreach (var item in checkNull)
                            pages.Remove(item);

                    var pagesClass = new PagesModelClass
                    {
                        TitleHead = GetText(Resource.String.Lbl_PagesLiked),
                        PagesList = new List<PageDataObject>(pages)
                    };

                    var check = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.PagesBox);
                    if (check != null)
                    {
                        check.PagesModel = pagesClass;
                        //PostFeedAdapter?.NotifyItemInserted(postFeedAdapter.ListDiffer.IndexOf(check) + 1);
                    }
                    else
                    {
                        Combiner.PagesBoxPostView(pagesClass, AppSettings.ShowUserSocialLinks ? 3 : 2);
                        //PostFeedAdapter?.NotifyItemInserted(1);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadGroupsLayout(List<GroupDataObject> groups, string groupsCounter)
        {
            try
            {
                if (groups?.Count > 0)
                {
                    var groupsClass = new GroupsModelClass
                    {
                        TitleHead = GetText(Resource.String.Lbl_Groups),
                        GroupsList = new List<GroupDataObject>(groups.Take(12)),
                        More = groupsCounter,
                        UserProfileId = SUserId
                    };

                    var check = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.GroupsBox);
                    if (check != null)
                    {
                        check.GroupsModel = groupsClass;
                        //PostFeedAdapter?.NotifyItemInserted(postFeedAdapter.ListDiffer.IndexOf(check) + 1);
                    }
                    else
                    {
                        Combiner.GroupsBoxPostView(groupsClass, AppSettings.ShowUserSocialLinks ? 3 : 2);
                        //PostFeedAdapter?.NotifyItemInserted(1);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region MaterialDialog

        public void OnSelection(IDialogInterface dialog, int position, string itemString)
        {
            try
            {
                var text = itemString;
                if (text == GetText(Resource.String.Lbl_Block))
                {
                    BlockUserButtonClick();
                }
                else if (text == GetText(Resource.String.Lbl_CopeLink))
                {
                    OnCopeLinkToProfile_Button_Click();
                }
                else if (text == GetText(Resource.String.Lbl_Share))
                {
                    OnShare_Button_Click();
                }
                else if (text == GetText(Resource.String.Lbl_ReportThisUser))
                {
                    OnReport_Button_Click();
                }
                else if (text == GetText(Resource.String.Lbl_CancelReport))
                {
                    if (!Methods.CheckConnectivity())
                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    else
                    {
                        UserData.IsReported = false;

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.ReportUserAsync(SUserId, "") });
                    }
                }
                else if (text == GetText(Resource.String.Lbl_Poke))
                {
                    if (Methods.CheckConnectivity())
                    {
                        IsPoked = true;
                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_YouHavePoked), ToastLength.Short);

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.CreatePokeAsync(SUserId) });
                    }
                    else
                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
                else if (text == GetText(Resource.String.Lbl_Poked))
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_YouSentPoked), ToastLength.Short);
                }
                else if (text == GetText(Resource.String.Lbl_AddToFamily))
                {
                    switch (ListUtils.FamilyList.Count)
                    {
                        case > 0:
                            {
                                var dialogList = new MaterialAlertDialogBuilder(this);

                                var arrayAdapter = ListUtils.FamilyList.Select(item => item.FamilyName.Replace("_", " ")).ToList();

                                dialogList.SetTitle(GetText(Resource.String.Lbl_AddToFamily));
                                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                                dialogList.Show();
                                break;
                            }
                    }
                }
                else if (text == GetText(Resource.String.Lbl_SentGift))
                {
                    GiftButtonOnClick();
                }
                else
                {
                    var familyId = ListUtils.FamilyList[position]?.FamilyId;
                    switch (string.IsNullOrEmpty(familyId))
                    {
                        case false:
                            IsFamily = true;
                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Sent_successfully), ToastLength.Short);
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.AddToFamilyAsync(SUserId, familyId) });
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnInput(IDialogInterface dialog, string input)
        {
            try
            {
                switch (input.Length)
                {
                    case <= 0:
                        return;
                }

                UserData.IsReported = true;

                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.ReportUserAsync(SUserId, input) });
                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_HasBeenReported), ToastLength.Short);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Block User
        private async void BlockUserButtonClick()
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
                else
                {
                    var (apiStatus, respond) = await RequestsAsync.Global.BlockUserAsync(SUserId, true); //true >> "block"
                    switch (apiStatus)
                    {
                        case 200:
                            {
                                var dbDatabase = new SqLiteDatabase();
                                dbDatabase.Insert_Or_Replace_OR_Delete_UsersContact(UserData, "Delete");


                                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_Blocked_successfully), ToastLength.Short);

                                if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
                                {
                                    ActivityOptions options = ActivityOptions.MakeCustomAnimation(this, Resource.Animation.slide_out_left, Resource.Animation.slide_out_left);
                                    options?.ToBundle();
                                }
                                else
                                {
                                    OverridePendingTransition(Resource.Animation.slide_out_left, Resource.Animation.slide_out_left);
                                }

                                Finish();
                                break;
                            }
                        default:
                            Methods.DisplayReportResult(this, respond);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> Cope Link To Profile
        private void OnCopeLinkToProfile_Button_Click()
        {
            try
            {
                Methods.CopyToClipboard(this, UserData.Url);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> Share
        private async void OnShare_Button_Click()
        {
            try
            {
                switch (CrossShare.IsSupported)
                {
                    //Share Plugin same as video
                    case false:
                        return;
                    default:
                        await CrossShare.Current.Share(new ShareMessage
                        {
                            Title = UserData.Name,
                            Text = "",
                            Url = UserData.Url
                        });
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> Report
        private void OnReport_Button_Click()
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    return;
                }

                var dialog = new MaterialAlertDialogBuilder(this);
                dialog.SetTitle(GetString(Resource.String.Lbl_ReportThisUser));

                EditText input = new EditText(this);
                input.SetHint(Resource.String.text);
                input.InputType = InputTypes.TextFlagImeMultiLine;
                LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                input.LayoutParameters = lp;

                dialog.SetView(input);

                dialog.SetPositiveButton(GetText(Resource.String.Lbl_Update), new MaterialDialogUtils(input, this));
                dialog.SetNegativeButton(GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());

                dialog.Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Sent Gift
        private void GiftButtonOnClick()
        {
            try
            {
                Bundle bundle = new Bundle();
                bundle.PutString("UserId", SUserId);

                GiftDialogFragment mGiftFragment = new GiftDialogFragment
                {
                    Arguments = bundle
                };

                mGiftFragment.Show(SupportFragmentManager, mGiftFragment.Tag);
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

                switch (requestCode)
                {
                    //Edit post
                    case 3950 when resultCode == Result.Ok:
                        {
                            var postId = data.GetStringExtra("PostId") ?? "";
                            var postText = data.GetStringExtra("PostText") ?? "";
                            var diff = PostFeedAdapter?.ListDiffer;
                            List<AdapterModelsClass> dataGlobal = diff.Where(a => a.PostData?.Id == postId).ToList();
                            switch (dataGlobal.Count)
                            {
                                case > 0:
                                    {
                                        foreach (var postData in dataGlobal)
                                        {
                                            postData.PostData.Orginaltext = postText;
                                            var index = diff.IndexOf(postData);
                                            switch (index)
                                            {
                                                case > -1:
                                                    PostFeedAdapter?.NotifyItemChanged(index);
                                                    break;
                                            }
                                        }

                                        var checkTextSection = dataGlobal.FirstOrDefault(w => w.TypeView == PostModelType.TextSectionPostPart);
                                        switch (checkTextSection)
                                        {
                                            case null:
                                                {
                                                    var collection = dataGlobal.FirstOrDefault()?.PostData;
                                                    var item = new AdapterModelsClass
                                                    {
                                                        TypeView = PostModelType.TextSectionPostPart,
                                                        Id = Convert.ToInt32((int)PostModelType.TextSectionPostPart + collection?.Id),
                                                        PostData = collection,
                                                        IsDefaultFeedPost = true
                                                    };

                                                    var headerPostIndex = diff.IndexOf(dataGlobal.FirstOrDefault(w => w.TypeView == PostModelType.HeaderPost));
                                                    switch (headerPostIndex)
                                                    {
                                                        case > -1:
                                                            diff.Insert(headerPostIndex + 1, item);
                                                            PostFeedAdapter?.NotifyItemInserted(headerPostIndex + 1);
                                                            break;
                                                    }

                                                    break;
                                                }
                                        }

                                        break;
                                    }
                            }

                            break;
                        }
                    //Edit post product 
                    case 3500 when resultCode == Result.Ok:
                        {
                            if (string.IsNullOrEmpty(data.GetStringExtra("itemData"))) return;
                            var item = JsonConvert.DeserializeObject<ProductDataObject>(data.GetStringExtra("itemData") ?? "");
                            if (item != null)
                            {
                                var diff = PostFeedAdapter?.ListDiffer;
                                var dataGlobal = diff.Where(a => a.PostData?.Id == item.PostId).ToList();
                                switch (dataGlobal.Count)
                                {
                                    case > 0:
                                        {
                                            foreach (var postData in dataGlobal)
                                            {
                                                var index = diff.IndexOf(postData);
                                                switch (index)
                                                {
                                                    case > -1:
                                                        {
                                                            var productUnion = postData.PostData.Product?.ProductClass;
                                                            if (productUnion != null) productUnion.Id = item.Id;
                                                            productUnion = item;
                                                            Console.WriteLine(productUnion);

                                                            PostFeedAdapter?.NotifyItemChanged(PostFeedAdapter.ListDiffer.IndexOf(postData));
                                                            break;
                                                        }
                                                }
                                            }

                                            break;
                                        }
                                }
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

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                switch (requestCode)
                {
                    case 111 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        new LiveUtil(this).OpenDialogLive();
                        break;
                    case 111:
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private void GetGiftsLists()
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    return;
                }

                var sqlEntity = new SqLiteDatabase();
                var listGifts = sqlEntity.GetGiftsList();

                ListUtils.GiftsList = ListUtils.GiftsList.Count switch
                {
                    > 0 when listGifts?.Count > 0 => listGifts,
                    _ => ListUtils.GiftsList
                };

                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { ApiRequest.GetGifts });

                switch (string.IsNullOrEmpty(GifLink))
                {
                    case false:
                        OpenGifLink(GifLink);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void OpenGifLink(string imageGifLink)
        {
            try
            {
                var dialog = new MaterialAlertDialogBuilder(this);

                dialog.SetTitle(Resource.String.Lbl_HeSentYouGift);

                View view = LayoutInflater.Inflate(Resource.Layout.Post_Content_Image_Layout, null);
                dialog.SetView(view);
                dialog.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                var image = view.FindViewById<ImageView>(Resource.Id.image);
                GlideImageLoader.LoadImage(this, imageGifLink, image, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                dialog.Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async void GetDataUserByName()
        {
            try
            {
                var (apiStatus, respond) = await RequestsAsync.Global.GetUserDataByUsernameAsync(SUserName);
                if (apiStatus == 200)
                {
                    if (respond is GetUserDataByUsernameObject { UserData: { } } result1)
                    {
                        UserData = result1.UserData;

                        SUserId = UserData.UserId;

                        SetRecyclerViewAdapters();

                        LoadPassedDate(UserData);

                        StartApiService();
                    }
                    else if (respond is GetUserDataByUsernameObject result2)
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_NotHaveThisUser), ToastLength.Short);

                        if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
                        {
                            ActivityOptions options = ActivityOptions.MakeCustomAnimation(this, Resource.Animation.slide_out_left, Resource.Animation.slide_out_left);
                            options?.ToBundle();
                        }
                        else
                        {
                            OverridePendingTransition(Resource.Animation.slide_out_left, Resource.Animation.slide_out_left);
                        }
                        Finish();
                    }
                }
                else
                    Methods.DisplayReportResult(this, respond);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region appBarLayout

        public void OnOffsetChanged(AppBarLayout appBarLayout, int verticalOffset)
        {
            try
            {
                int minHeight = CollapsingToolbar.MinimumHeight * 2;
                float scale = (float)(minHeight + verticalOffset) / minHeight;

                //Console.WriteLine("MyProfileActivity >> VerticalOffset " + verticalOffset);
                SwipeRefreshLayout.Enabled = verticalOffset == 0;
                //Console.WriteLine("MyProfileActivity >> Enabled " + SwipeRefreshLayout.Enabled);

                if (scale >= 0)
                {
                    ImageBack.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
                    BtnMore.SetColorFilter(Color.White);
                    TxtSearchForPost.Visibility = ViewStates.Invisible;
                }
                else if (scale <= -2.80)
                {
                    if (TxtSearchForPost.Visibility == ViewStates.Visible)
                        return;

                    ImageBack.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
                    BtnMore.SetColorFilter(Color.ParseColor(AppSettings.MainColor));

                    if (AppSettings.ShowSearchForPosts)
                    {
                        TxtSearchForPost.BackgroundTintList = ColorStateList.ValueOf(WoWonderTools.IsTabDark() ? Color.ParseColor("#262626") : Color.ParseColor("#ecedf1"));
                        TxtSearchForPost.Visibility = ViewStates.Visible;
                    }
                }
                else if (scale >= -2.20)
                {
                    if (TxtSearchForPost.Visibility == ViewStates.Invisible)
                        return;

                    ImageBack.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
                    BtnMore.SetColorFilter(Color.White);
                    TxtSearchForPost.Visibility = ViewStates.Invisible;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        #endregion

        //private void LiveLayoutOnClick()
        //{
        //    try
        //    {
        //        if (UserDetails.DataLivePost?.LiveTime != null && UserDetails.DataLivePost?.LiveTime.Value > 0 && string.IsNullOrEmpty(UserDetails.DataLivePost?.AgoraResourceId) && string.IsNullOrEmpty(UserDetails.DataLivePost?.PostFile))
        //        {
        //            //Live
        //            //Owner >> ClientRoleBroadcaster , Users >> ClientRoleAudience
        //            Intent intent = new Intent(PostAdapter.ActivityContext, typeof(LiveStreamingActivity));
        //            intent.PutExtra(Constants.KeyClientRole, DT.Xamarin.Agora.Constants.ClientRoleAudience);
        //            intent.PutExtra("PostId", UserDetails.DataLivePost.PostId);
        //            intent.PutExtra("StreamName", UserDetails.DataLivePost.StreamName);
        //            intent.PutExtra("PostLiveStream", JsonConvert.SerializeObject(UserDetails.DataLivePost));
        //            PostAdapter.ActivityContext.StartActivity(intent);
        //        }
        //    }
        //    catch (Exception exception)
        //    {
        //        Methods.DisplayReportResultTrack(exception);
        //    }
        //}

        //private void StopNotifyLayoutOnClick(UserDataObject UserData)
        //{
        //    try
        //    {
        //        if (!Methods.CheckConnectivity())
        //        {
        //            ToastUtils.ShowToast(PostAdapter.ActivityContext, PostAdapter.ActivityContext.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
        //        }
        //        else
        //        {
        //            //check is stopped or no ic_notifications_on_vector , ic_notifications_off_vector
        //            if (UserData.IsNotifyStopped != null && UserData.IsNotifyStopped.Value == 1)
        //            {
        //                UserData.IsNotifyStopped = 0;
        //                ImageStopNotify.SetImageResource(Resource.Drawable.ic_notifications_on_vector);
        //                ToastUtils.ShowToast(PostAdapter.ActivityContext, PostAdapter.ActivityContext.GetString(Resource.String.Lbl_NotificationsAreTurnedOn), ToastLength.Short);
        //            }
        //            else
        //            {
        //                UserData.IsNotifyStopped = 1;
        //                ImageStopNotify.SetImageResource(Resource.Drawable.ic_notifications_off_vector);
        //                ToastUtils.ShowToast(PostAdapter.ActivityContext, PostAdapter.ActivityContext.GetString(Resource.String.Lbl_NotificationsHaveBeenStopped), ToastLength.Short);
        //            }

        //            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.StopNotifyAsync(UserData.UserId) });
        //        }
        //    }
        //    catch (Exception exception)
        //    {
        //        Methods.DisplayReportResultTrack(exception);
        //    }
        //}

    }
}