using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Text;
using Android.Text.Method;
using Android.Util;
using Android.Views;
using Android.Widget;
using Androidx.Media3.UI;
using AndroidX.AppCompat.Content.Res;
using AndroidX.Core.Graphics.Drawable;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Load.Resource.Bitmap;
using Bumptech.Glide.Request;
using Bumptech.Glide.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WoWonder.Activities.Base;
using WoWonder.Activities.Comment.Adapters;
using WoWonder.Activities.MyProfile;
using WoWonder.Activities.NativePost.Extra;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Activities.UserProfile;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using WoWonder.Library.Anjo.SuperTextLibrary;
using WoWonder.MediaPlayers.Exo;
using WoWonder.SQLite;
using WoWonderClient.Classes.Comments;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Requests;
using static WoWonder.Activities.NativePost.Post.AdapterBind;
using Reaction = WoWonderClient.Classes.Posts.Reaction;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.NativePost.Pages
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class ViewFullVideoPostActivity : BaseActivity, StTools.IXAutoLinkOnClickListener, View.IOnClickListener, View.IOnLongClickListener
    {
        #region Variables Basic

        private LinearLayout MainView;

        private SwipeRefreshLayout SwipeRefreshLayout;

        public TextViewWithImages Username { get; set; }

        public TextView TimeText { get; set; }
        public ImageView PrivacyPostIcon { get; set; }
        public ImageView MoreIcon { get; private set; }
        public ImageView UserAvatar { get; set; }
        public SuperTextView Description { get; private set; }
        public TextView ShareCount { get; private set; }
        public TextView CommentCount { get; private set; }
        public TextView LikeCount { get; private set; }
        public TextView ViewCount { get; private set; }
        public LinearLayout CountLikeSection { get; private set; }
        public ImageView ImageCountLike { get; private set; }
        public LinearLayout MainSectionButton { get; private set; }
        public LinearLayout ShareLinearLayout { get; private set; }
        public LinearLayout CommentLinearLayout { get; private set; }
        public LinearLayout SecondReactionLinearLayout { get; set; }
        public LinearLayout ReactLinearLayout { get; set; }
        public ReactButton LikeButton { get; set; }
        public TextView SecondReactionButton { get; set; }


        private PostClickListener PostClickListener;

        private PlayerView StyledPlayerView;
        public PlayerView VideoSurfaceView { get; set; }
        public ExoController ExoController { get; set; }

        private string PostId;
        private PostDataObject PostObject;

        private CommentAdapter MAdapter;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private RecyclerViewOnScrollListener MainScrollEvent;
        public static bool PageIsOpen;
        public static ViewFullVideoPostActivity Instance;

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
                SetContentView(Resource.Layout.ViewFullVideoPostLayout);

                Instance = this;
                PageIsOpen = true;

                PostId = Intent?.GetStringExtra("PostId") ?? string.Empty;
                PostObject = JsonConvert.DeserializeObject<PostDataObject>(Intent?.GetStringExtra("PostObject") ?? "");

                //Get Value And Set Toolbar 
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();
                PostClickListener = new PostClickListener(this, NativeFeedType.Global);

                LoadPost();
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
                ExoController?.StopVideo();
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
                ExoController?.StopVideo();
                base.OnStop();
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
                Instance = null;
                PageIsOpen = false;
                ExoController?.ReleaseVideo();
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
                MainView = (LinearLayout)FindViewById(Resource.Id.main_content);

                SwipeRefreshLayout = (SwipeRefreshLayout)FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = false;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(WoWonderTools.IsTabDark() ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));

                Username = FindViewById<TextViewWithImages>(Resource.Id.username);
                TimeText = FindViewById<TextView>(Resource.Id.time_text);
                PrivacyPostIcon = FindViewById<ImageView>(Resource.Id.privacyPost);
                UserAvatar = FindViewById<ImageView>(Resource.Id.userAvatar);
                MoreIcon = FindViewById<ImageView>(Resource.Id.moreicon);

                Description = FindViewById<SuperTextView>(Resource.Id.description);
                Description?.SetTextInfo(Description);
                Description.SetTextSize(ComplexUnitType.Sp, 13f);

                ShareCount = FindViewById<TextView>(Resource.Id.Sharecount);
                CommentCount = FindViewById<TextView>(Resource.Id.Commentcount);
                CountLikeSection = FindViewById<LinearLayout>(Resource.Id.countLikeSection);
                LikeCount = FindViewById<TextView>(Resource.Id.Likecount);
                ImageCountLike = FindViewById<ImageView>(Resource.Id.ImagecountLike);

                ViewCount = FindViewById<TextView>(Resource.Id.viewcount);

                ShareCount.Visibility = AppSettings.ShowCountSharePost switch
                {
                    false => ViewStates.Gone,
                    _ => ShareCount.Visibility
                };

                MRecycler = (RecyclerView)FindViewById(Resource.Id.RecylerComment);

                StyledPlayerView = FindViewById<PlayerView>(Resource.Id.itemVideoPlayer);
                SetPlayer(StyledPlayerView);

                ShareLinearLayout = FindViewById<LinearLayout>(Resource.Id.ShareLinearLayout);
                CommentLinearLayout = FindViewById<LinearLayout>(Resource.Id.CommentLinearLayout);
                SecondReactionLinearLayout = FindViewById<LinearLayout>(Resource.Id.SecondReactionLinearLayout);
                ReactLinearLayout = FindViewById<LinearLayout>(Resource.Id.ReactLinearLayout);
                LikeButton = FindViewById<ReactButton>(Resource.Id.ReactButton);

                SecondReactionButton = FindViewById<TextView>(Resource.Id.SecondReactionText);

                ShareLinearLayout.Visibility = AppSettings.ShowShareButton switch
                {
                    false => ViewStates.Gone,
                    _ => ShareLinearLayout.Visibility
                };

                MainSectionButton = FindViewById<LinearLayout>(Resource.Id.linerSecondReaction);
                switch (AppSettings.PostButton)
                {
                    case PostButtonSystem.Reaction:
                    case PostButtonSystem.Like:
                        MainSectionButton.WeightSum = AppSettings.ShowShareButton ? 3 : 2;

                        SecondReactionLinearLayout.Visibility = ViewStates.Gone;
                        break;
                    case PostButtonSystem.Wonder:
                        MainSectionButton.WeightSum = AppSettings.ShowShareButton ? 4 : 3;

                        SecondReactionLinearLayout.Visibility = ViewStates.Visible;

                        SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(Resource.Drawable.icon_post_wonder_vector, 0, 0, 0);
                        SecondReactionButton.Text = Application.Context.GetText(Resource.String.Btn_Wonder);
                        break;
                    case PostButtonSystem.DisLike:
                        MainSectionButton.WeightSum = AppSettings.ShowShareButton ? 4 : 3;

                        SecondReactionLinearLayout.Visibility = ViewStates.Visible;
                        SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(Resource.Drawable.icon_post_dislike_vector, 0, 0, 0);
                        SecondReactionButton.Text = Application.Context.GetText(Resource.String.Btn_Dislike);
                        break;
                }
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
                    toolBar.Title = GetText(Resource.String.Lbl_Post);
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
                MAdapter = new CommentAdapter(this)
                {
                    CommentList = new ObservableCollection<CommentObjectExtra>()
                };
                LayoutManager = new LinearLayoutManager(this);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<CommentObjectExtra>(this, MAdapter, sizeProvider, 10);
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
                switch (addEvent)
                {
                    // true +=  // false -=
                    case true:
                        SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                        UserAvatar?.SetOnClickListener(this);
                        Username?.SetOnClickListener(this);
                        CommentLinearLayout?.SetOnClickListener(this);
                        CommentCount?.SetOnClickListener(this);
                        ShareLinearLayout?.SetOnClickListener(this);
                        LikeButton?.SetOnClickListener(this);
                        LikeButton?.SetOnLongClickListener(this);
                        MoreIcon?.SetOnClickListener(this);
                        LikeCount?.SetOnClickListener(this);
                        SecondReactionButton?.SetOnClickListener(this);
                        break;
                    default:
                        SwipeRefreshLayout.Refresh -= SwipeRefreshLayoutOnRefresh;
                        UserAvatar?.SetOnClickListener(null);
                        Username?.SetOnClickListener(null);
                        CommentLinearLayout?.SetOnClickListener(null);
                        CommentCount?.SetOnClickListener(null);
                        ShareLinearLayout?.SetOnClickListener(null);
                        LikeButton?.SetOnClickListener(null);
                        LikeButton?.SetOnLongClickListener(null);
                        MoreIcon?.SetOnClickListener(null);
                        LikeCount?.SetOnClickListener(null);
                        SecondReactionButton?.SetOnClickListener(null);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static ViewFullVideoPostActivity GetInstance()
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

        #endregion

        #region Event

        //Refresh
        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    return;
                }

                MainScrollEvent.IsLoading = false;

                Task.Factory.StartNew(StartApiService);
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
                var item = MAdapter.CommentList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.Id) && !MainScrollEvent.IsLoading)
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadDataComment(item.Id) });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnClick(View v)
        {
            try
            {
                var postType = PostFunctions.GetAdapterType(PostObject);

                if (v.Id == UserAvatar.Id)
                    PostClickListener.ProfilePostClick(new ProfileClickEventArgs { NewsFeedClass = PostObject, Position = 0, View = MainView }, "NewsFeedClass", "UserAvatar");
                else if (v.Id == MoreIcon.Id)
                    PostClickListener.MorePostIconClick(new GlobalClickEventArgs { NewsFeedClass = PostObject, Position = 0, View = MainView });
                else if (v.Id == LikeCount.Id)
                    PostClickListener.DataItemPostClick(new GlobalClickEventArgs { NewsFeedClass = PostObject, Position = 0, View = MainView });
                else if (v.Id == CommentCount.Id)
                    PostClickListener.CommentPostClick(new GlobalClickEventArgs { NewsFeedClass = PostObject, Position = 0, View = MainView });
                else if (v.Id == ShareCount.Id)
                    PostClickListener.SharePostClick(new GlobalClickEventArgs { NewsFeedClass = PostObject, Position = 0, View = MainView }, postType);
                else if (v.Id == ReactLinearLayout.Id)
                    LikeButton.ClickLikeAndDisLike(new GlobalClickEventArgs { NewsFeedClass = PostObject, Position = 0, View = MainView }, null);
                else if (v.Id == CommentLinearLayout.Id)
                    PostClickListener.CommentPostClick(new GlobalClickEventArgs { NewsFeedClass = PostObject, Position = 0, View = MainView });
                else if (v.Id == ShareLinearLayout.Id)
                    PostClickListener.SharePostClick(new GlobalClickEventArgs { NewsFeedClass = PostObject, Position = 0, View = MainView }, postType);
                else if (v.Id == SecondReactionButton.Id)
                    PostClickListener.SecondReactionButtonClick(new GlobalClickEventArgs { NewsFeedClass = PostObject, Position = 0, View = MainView });

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public bool OnLongClick(View v)
        {
            switch (AppSettings.PostButton)
            {
                //add event if System = ReactButton 
                case PostButtonSystem.Reaction:
                    {
                        if (LikeButton.Id == v.Id)
                            LikeButton.LongClickDialog(new GlobalClickEventArgs { NewsFeedClass = PostObject, View = MainView }, null);
                        break;
                    }
            }

            return true;
        }

        public void AutoLinkTextClick(StTools.XAutoLinkMode p0, string p1, Dictionary<string, string> userData)
        {
            try
            {
                p1 = p1.Replace(" ", "").Replace("\n", "");
                var typeText = Methods.FunString.Check_Regex(p1);
                switch (typeText)
                {
                    case "Email":
                        Methods.App.SendEmail(this, p1);
                        break;
                    case "Website":
                        {
                            string url = p1.Contains("http") switch
                            {
                                false => "http://" + p1,
                                _ => p1
                            };

                            //var intent = new Intent(this, typeof(LocalWebViewActivity));
                            //intent.PutExtra("URL", url.Replace(" ", ""));
                            //intent.PutExtra("Type", url.Replace(" ", ""));
                            //this.StartActivity(intent);
                            new IntentController(this).OpenBrowserFromApp(url);
                            break;
                        }
                    case "Hashtag":
                        {
                            var intent = new Intent(this, typeof(HashTagPostsActivity));
                            intent.PutExtra("Id", p1);
                            intent.PutExtra("Tag", p1);
                            StartActivity(intent);
                            break;
                        }
                    case "Mention":
                        {
                            var dataUSer = ListUtils.MyProfileList?.FirstOrDefault();
                            string name = p1.Replace("@", "").Replace(" ", "");

                            var sqlEntity = new SqLiteDatabase();
                            var user = sqlEntity.Get_DataOneUser(name);


                            if (user != null)
                            {
                                WoWonderTools.OpenProfile(this, user.UserId, user);
                            }
                            else switch (userData?.Count)
                                {
                                    case > 0:
                                        {
                                            var data = userData.FirstOrDefault(a => a.Value == name);
                                            if (data.Key != null && data.Key == UserDetails.UserId)
                                            {
                                                switch (PostClickListener.OpenMyProfile)
                                                {
                                                    case true:
                                                        return;
                                                    default:
                                                        {
                                                            var intent = new Intent(this, typeof(MyProfileActivity));
                                                            StartActivity(intent);
                                                            break;
                                                        }
                                                }
                                            }
                                            else if (data.Key != null)
                                            {
                                                var intent = new Intent(this, typeof(UserProfileActivity));
                                                //intent.PutExtra("UserObject", JsonConvert.SerializeObject(item));
                                                intent.PutExtra("UserId", data.Key);
                                                StartActivity(intent);
                                            }

                                            break;
                                        }
                                    default:
                                        {
                                            if (name == dataUSer?.Name || name == dataUSer?.Username)
                                            {
                                                switch (PostClickListener.OpenMyProfile)
                                                {
                                                    case true:
                                                        return;
                                                    default:
                                                        {
                                                            var intent = new Intent(this, typeof(MyProfileActivity));
                                                            StartActivity(intent);
                                                            break;
                                                        }
                                                }
                                            }
                                            else
                                            {
                                                var intent = new Intent(this, typeof(UserProfileActivity));
                                                //intent.PutExtra("UserObject", JsonConvert.SerializeObject(item));
                                                intent.PutExtra("name", name);
                                                StartActivity(intent);
                                            }

                                            break;
                                        }
                                }

                            break;
                        }
                    case "Number":
                        Methods.App.SaveContacts(this, p1, "", "2");
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Player

        private void SetPlayer(PlayerView videoSurfaceView)
        {
            try
            {
                VideoSurfaceView = videoSurfaceView;
                VideoSurfaceView.ResizeMode = AspectRatioFrameLayout.ResizeModeZoom;

                //Create the player using ExoPlayerFactory
                ExoController = new ExoController(this, "ViewFullVideoPostActivity");
                ExoController?.SetPlayer(VideoSurfaceView);
                ExoController?.SetPlayerControl();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void PlayVideo()
        {
            try
            {
                var uri = Uri.Parse(PostObject.PostFileFull);
                ExoController?.FirstPlayVideo(uri);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                if (requestCode == 2000 && resultCode == Result.Ok)
                {
                    ExoController.RestartPlayAfterShrinkScreen();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region LoadPostData

        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadDataComment() });
        }

        private void LoadPost()
        {
            try
            {
                if (PostObject != null)
                {
                    PlayVideo();

                    HeaderPostBind();
                    TextSectionPostPartBind();
                    PrevBottomPostPartBind();
                    BottomPostPartBind();

                    Task.Factory.StartNew(StartApiService);
                }
                else
                {
                    if (!Methods.CheckConnectivity())
                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    else
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadPostDataAsync });
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task LoadPostDataAsync()
        {
            var (apiStatus, respond) = await RequestsAsync.Posts.GetPostDataAsync(PostId, "post_data");
            if (apiStatus == 200)
            {
                if (respond is GetPostDataObject result)
                {
                    PostObject = result.PostData;
                    RunOnUiThread(LoadPost);
                }
            }
            else
                Methods.DisplayReportResult(this, respond);
        }

        #endregion

        #region Load Data Comment

        private async Task LoadDataComment(string offset = "0")
        {
            switch (MainScrollEvent.IsLoading)
            {
                case true:
                    return;
            }

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;
                var (apiStatus, respond) = await RequestsAsync.Comment.GetPostCommentsAsync(PostId, "10", offset);
                if (apiStatus != 200 || respond is not CommentObject result || result.CommentList == null)
                {
                    MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(this, respond);
                }
                else
                {
                    var respondList = result.CommentList?.Count;
                    switch (respondList)
                    {
                        case > 0:
                            {
                                foreach (var item in result.CommentList)
                                {
                                    CommentObjectExtra check = MAdapter.CommentList.FirstOrDefault(a => a.Id == item.Id);
                                    switch (check)
                                    {
                                        case null:
                                            {
                                                var db = ClassMapper.Mapper?.Map<CommentObjectExtra>(item);
                                                if (db != null) MAdapter.CommentList.Add(db);
                                                break;
                                            }
                                        default:
                                            check = ClassMapper.Mapper?.Map<CommentObjectExtra>(item);
                                            check.Replies = item.Replies;
                                            check.RepliesCount = item.RepliesCount;
                                            break;
                                    }
                                }

                                RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                                break;
                            }
                    }
                }
            }
        }

        #endregion

        #region Load Data Post

        private void HeaderPostBind()
        {
            try
            {
                UserDataObject publisher = PostObject.Publisher ?? PostObject.UserData;


                var glideRequestOptions2 = new RequestOptions().SkipMemoryCache(true).CenterCrop().CircleCrop().Format(DecodeFormat.PreferRgb565)
                    .SetPriority(Priority.High)
                    .SetUseAnimationPool(false).SetDiskCacheStrategy(DiskCacheStrategy.All)
                    .Error(Resource.Drawable.ImagePlacholder_circle)
                    .Placeholder(Resource.Drawable.ImagePlacholder_circle);

                if (publisher.UserId == UserDetails.UserId)
                {
                    var CircleGlideRequestBuilder = Glide.With(this).AsBitmap().Downsample(DownsampleStrategy.AtMost).Apply(glideRequestOptions2).Timeout(3000).SetUseAnimationPool(false);
                    CircleGlideRequestBuilder.DontTransform_T();
                    CircleGlideRequestBuilder.Downsample(DownsampleStrategy.CenterInside);
                    CircleGlideRequestBuilder.AddListener(new GlideCustomRequestListener("GlobalCircle")).Override(70).Load(PostObject.PostPrivacy == "4" ? "user_anonymous" : UserDetails.Avatar).CircleCrop().Into(UserAvatar);
                }
                else
                {
                    var CircleGlideRequestBuilder = Glide.With(this).AsBitmap().Downsample(DownsampleStrategy.AtMost).Apply(glideRequestOptions2).Timeout(3000).SetUseAnimationPool(false);
                    CircleGlideRequestBuilder.DontTransform_T();
                    CircleGlideRequestBuilder.Downsample(DownsampleStrategy.CenterInside);
                    CircleGlideRequestBuilder.AddListener(new GlideCustomRequestListener("GlobalCircle")).Override(70).Load(PostObject.PostPrivacy == "4" ? "user_anonymous" : publisher.Avatar).CircleCrop().Into(UserAvatar);
                }

                //GlideImageLoader.LoadImage(this, PostObject.PostPrivacy == "4" ? "user_anonymous" : publisher.Avatar, UserAvatar, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);

                switch (PostObject.PostPrivacy)
                {
                    //Anonymous Post
                    case "4":
                        Username.Text = this.GetText(Resource.String.Lbl_Anonymous);
                        break;
                    default:
                        var postDataDecoratedContent = new WoTextDecorator().SetupStrings(PostObject, this);
                        Username.SetText(postDataDecoratedContent, TextView.BufferType.Spannable);
                        break;
                }

                TimeText.Text = PostObject.Time;

                if (PrivacyPostIcon != null && !string.IsNullOrEmpty(PostObject.PostPrivacy) && (publisher.UserId == UserDetails.UserId || AppSettings.ShowPostPrivacyForAllUser))
                {
                    switch (PostObject.PostPrivacy)
                    {
                        //Everyone
                        case "0":
                            PrivacyPostIcon.SetImageResource(Resource.Drawable.icon_post_global_vector);
                            break;
                        default:
                            {
                                if (PostObject.PostPrivacy.Contains("ifollow") || PostObject.PostPrivacy == "2") //People_i_Follow
                                {
                                    PrivacyPostIcon.SetImageResource(Resource.Drawable.ic_friend);
                                }
                                else if (PostObject.PostPrivacy.Contains("me") || PostObject.PostPrivacy == "1") //People_Follow_Me
                                {
                                    PrivacyPostIcon.SetImageResource(Resource.Drawable.ic_users);
                                }
                                else switch (PostObject.PostPrivacy)
                                    {
                                        //Anonymous
                                        case "4":
                                            PrivacyPostIcon.SetImageResource(Resource.Drawable.ic_detective);
                                            break;
                                        //No_body 
                                        default:
                                            PrivacyPostIcon.SetImageResource(Resource.Drawable.ic_lock);
                                            break;
                                    }

                                break;
                            }
                    }

                    PrivacyPostIcon.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void TextSectionPostPartBind()
        {
            try
            {
                if (string.IsNullOrEmpty(PostObject.Orginaltext) || string.IsNullOrWhiteSpace(PostObject.Orginaltext))
                {
                    if (Description.Visibility != ViewStates.Gone)
                        Description.Visibility = ViewStates.Gone;
                }
                else
                {
                    if (Description.Visibility != ViewStates.Visible)
                        Description.Visibility = ViewStates.Visible;

                    if (PostObject.RegexFilterList != null & PostObject.RegexFilterList?.Count > 0)
                    {
                        Description.SetAutoLinkOnClickListener(this, PostObject.RegexFilterList);
                    }
                    else
                        Description.SetAutoLinkOnClickListener(this, new Dictionary<string, string>());

                    var spendable = new SpannableStringBuilder(PostObject.Orginaltext.Replace("@", ""));
                    if (PostObject.RegexFilterList != null & PostObject.RegexFilterList?.Count > 0)
                    {
                        foreach (var user in PostObject.RegexFilterList)
                        {
                            string fullName = PostObject.MentionsUsers.MentionsUsersList?.FirstOrDefault(a => a.Key == user.Value?.Replace("/", "")).Value;

                            string content = spendable.ToString();
                            if (string.IsNullOrEmpty(content) || string.IsNullOrWhiteSpace(content))
                                continue;

                            var indexFrom = content.IndexOf(fullName, StringComparison.Ordinal);
                            indexFrom = indexFrom switch
                            {
                                <= -1 => 0,
                                _ => indexFrom
                            };

                            var indexLast = indexFrom + fullName.Length;
                            indexLast = indexLast switch
                            {
                                <= -1 => 0,
                                _ => indexLast
                            };

                            Console.WriteLine(indexFrom);

                            if (indexFrom == 0 && indexLast == 0)
                                continue;

                            //wael
                            spendable.SetSpan(new PostTextMentionsClickSpanClass(user.Key, this), indexFrom, indexLast, SpanTypes.ExclusiveExclusive);
                        }
                    }
                    var readMoreOption = new StReadMoreOption.Builder()
                        .TextLength(200, StReadMoreOption.TypeCharacter)
                        .MoreLabel(GetText(Resource.String.Lbl_ReadMore))
                        .LessLabel(GetText(Resource.String.Lbl_ReadLess))
                        .MoreLabelColor(Color.ParseColor(AppSettings.MainColor))
                        .LessLabelColor(Color.ParseColor(AppSettings.MainColor))
                        .LabelUnderLine(true)
                        .Build();
                    readMoreOption.AddReadMoreTo(Description, spendable);

                    Description.LinksClickable = true;
                    Description.Clickable = true;
                    Description.MovementMethod = LinkMovementMethod.Instance;

                    if (AppSettings.TextSizeDescriptionPost == WRecyclerView.VolumeState.On)
                    {
                        if (!string.IsNullOrEmpty(Description.Text) && !string.IsNullOrWhiteSpace(Description.Text) && Description.Text?.Length <= 50)
                        {
                            if (PostObject.Orginaltext.Contains("http") || PostObject.Orginaltext.Contains(this.GetText(Resource.String.Lbl_ReadMore)) || PostObject.Orginaltext.Contains(this.GetText(Resource.String.Lbl_ReadLess)))
                                Description.SetTextSize(ComplexUnitType.Sp, 13f);
                            else
                                Description.SetTextSize(ComplexUnitType.Sp, 20f);
                        }
                        else
                        {
                            Description.SetTextSize(ComplexUnitType.Sp, 13f);
                        }
                    }
                    else
                    {
                        Description.SetTextSize(ComplexUnitType.Sp, 13f);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void PrevBottomPostPartBind()
        {
            try
            {
                if (CommentCount != null)
                    CommentCount.Text = PostObject.PostComments + " " + this.GetString(Resource.String.Lbl_Comments);

                if (AppSettings.ShowTextShareButton)
                {
                    if (ShareCount != null)
                        ShareCount.Text = PostObject.DatumPostShare + " " + this.GetString(Resource.String.Lbl_Shares);
                }
                else
                {
                    if (ShareCount != null)
                        ShareCount.Visibility = ViewStates.Gone;
                }

                ViewCount.Text = PostObject.PrevButtonViewText;

                if (LikeCount != null)
                {
                    switch (AppSettings.PostButton)
                    {
                        case PostButtonSystem.Reaction:
                            LikeCount.Text = PostObject.PostLikes + " " + this.GetString(Resource.String.Lbl_Reactions);
                            break;
                        default:
                            LikeCount.Text = PostObject.PostLikes + " " + this.GetString(Resource.String.Btn_Likes);
                            break;
                    }
                }

                switch (AppSettings.PostButton)
                {
                    case PostButtonSystem.Reaction:
                        {
                            PostObject.Reaction ??= new Reaction();

                            ImageCountLike.Visibility = PostObject.Reaction.Count > 0 ? ViewStates.Visible : ViewStates.Gone;
                            if (PostObject.Reaction.Count > 0)
                                ImageCountLike.SetImageResource(Resource.Drawable.emoji_like);
                            else
                                ImageCountLike.SetImageResource(Resource.Drawable.icon_post_like_vector);

                            if (PostObject.Reaction.IsReacted != null && PostObject.Reaction.IsReacted.Value)
                            {
                                switch (string.IsNullOrEmpty(PostObject.Reaction.Type))
                                {
                                    case false:
                                        {
                                            var react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Id == PostObject.Reaction.Type).Value?.Id ?? "";
                                            switch (react)
                                            {
                                                case "1":
                                                    ImageCountLike.SetImageResource(Resource.Drawable.emoji_like);
                                                    break;
                                                case "2":
                                                    ImageCountLike.SetImageResource(Resource.Drawable.emoji_love);
                                                    break;
                                                case "3":
                                                    ImageCountLike.SetImageResource(Resource.Drawable.emoji_haha);
                                                    break;
                                                case "4":
                                                    ImageCountLike.SetImageResource(Resource.Drawable.emoji_wow);
                                                    break;
                                                case "5":
                                                    ImageCountLike.SetImageResource(Resource.Drawable.emoji_sad);
                                                    break;
                                                case "6":
                                                    ImageCountLike.SetImageResource(Resource.Drawable.emoji_angry);
                                                    break;
                                                default:
                                                    switch (PostObject.Reaction.Count)
                                                    {
                                                        case > 0:
                                                            ImageCountLike.SetImageResource(Resource.Drawable.emoji_like);
                                                            break;
                                                    }
                                                    break;
                                            }

                                            break;
                                        }
                                }
                            }
                            else
                            {
                                switch (PostObject.Reaction.Count)
                                {
                                    case > 0:
                                        ImageCountLike.SetImageResource(Resource.Drawable.emoji_like);
                                        break;
                                }
                            }

                            break;
                        }
                    default:
                        //ImageCountLike.Visibility = ViewStates.Invisible;
                        ImageCountLike.SetImageResource(Resource.Drawable.icon_post_like_vector);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void BottomPostPartBind()
        {
            try
            {
                if (LikeButton != null)
                    LikeButton.Text = PostObject.PostLikes;

                switch (AppSettings.PostButton)
                {
                    case PostButtonSystem.Reaction:
                        {
                            PostObject.Reaction ??= new Reaction();

                            if (PostObject.Reaction.IsReacted != null && PostObject.Reaction.IsReacted.Value)
                            {
                                switch (string.IsNullOrEmpty(PostObject.Reaction.Type))
                                {
                                    case false:
                                        {
                                            var react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Id == PostObject.Reaction.Type).Value?.Id ?? "";
                                            switch (react)
                                            {
                                                case "1":
                                                    LikeButton.SetReactionPack(ReactConstants.Like);
                                                    break;
                                                case "2":
                                                    LikeButton.SetReactionPack(ReactConstants.Love);
                                                    break;
                                                case "3":
                                                    LikeButton.SetReactionPack(ReactConstants.HaHa);
                                                    break;
                                                case "4":
                                                    LikeButton.SetReactionPack(ReactConstants.Wow);
                                                    break;
                                                case "5":
                                                    LikeButton.SetReactionPack(ReactConstants.Sad);
                                                    break;
                                                case "6":
                                                    LikeButton.SetReactionPack(ReactConstants.Angry);
                                                    break;
                                                default:
                                                    LikeButton.SetReactionPack(ReactConstants.Default);
                                                    break;
                                            }

                                            break;
                                        }
                                }
                            }
                            else
                                LikeButton.SetReactionPack(ReactConstants.Default);

                            break;
                        }
                    default:
                        {
                            if (PostObject.Reaction.IsReacted != null && !PostObject.Reaction.IsReacted.Value)
                                LikeButton.SetReactionPack(ReactConstants.Default);

                            if (PostObject.IsLiked != null && PostObject.IsLiked.Value)
                                LikeButton.SetReactionPack(ReactConstants.Like);

                            if (SecondReactionButton != null)
                            {
                                switch (AppSettings.PostButton)
                                {
                                    case PostButtonSystem.Wonder when PostObject.IsWondered != null && PostObject.IsWondered.Value:
                                        {
                                            Drawable unwrappedDrawable = AppCompatResources.GetDrawable(this, Resource.Drawable.icon_post_wonder_vector);
                                            Drawable wrappedDrawable = DrawableCompat.Wrap(unwrappedDrawable);
                                            switch (Build.VERSION.SdkInt)
                                            {
                                                case <= BuildVersionCodes.Lollipop:
                                                    DrawableCompat.SetTint(wrappedDrawable, Color.ParseColor("#f89823"));
                                                    break;
                                                default:
                                                    wrappedDrawable = wrappedDrawable.Mutate();
                                                    wrappedDrawable.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#f89823"), PorterDuff.Mode.SrcAtop));
                                                    break;
                                            }

                                            SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable, null, null, null);

                                            SecondReactionButton.Text = this.GetString(Resource.String.Lbl_wondered);
                                            SecondReactionButton.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                                            break;
                                        }
                                    case PostButtonSystem.Wonder:
                                        {
                                            Drawable unwrappedDrawable = AppCompatResources.GetDrawable(this, Resource.Drawable.icon_post_wonder_vector);
                                            Drawable wrappedDrawable = DrawableCompat.Wrap(unwrappedDrawable);
                                            switch (Build.VERSION.SdkInt)
                                            {
                                                case <= BuildVersionCodes.Lollipop:
                                                    DrawableCompat.SetTint(wrappedDrawable, Color.ParseColor("#666666"));
                                                    break;
                                                default:
                                                    wrappedDrawable = wrappedDrawable.Mutate();
                                                    wrappedDrawable.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#666666"), PorterDuff.Mode.SrcAtop));
                                                    break;
                                            }
                                            SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable, null, null, null);

                                            SecondReactionButton.Text = this.GetString(Resource.String.Btn_Wonder);
                                            SecondReactionButton.SetTextColor(Color.ParseColor("#444444"));
                                            break;
                                        }
                                    case PostButtonSystem.DisLike when PostObject.IsWondered != null && PostObject.IsWondered.Value:
                                        {
                                            Drawable unwrappedDrawable = AppCompatResources.GetDrawable(this, Resource.Drawable.icon_post_dislike_vector);
                                            Drawable wrappedDrawable = DrawableCompat.Wrap(unwrappedDrawable);

                                            switch (Build.VERSION.SdkInt)
                                            {
                                                case <= BuildVersionCodes.Lollipop:
                                                    DrawableCompat.SetTint(wrappedDrawable, Color.ParseColor("#f89823"));
                                                    break;
                                                default:
                                                    wrappedDrawable = wrappedDrawable.Mutate();
                                                    wrappedDrawable.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#f89823"), PorterDuff.Mode.SrcAtop));
                                                    break;
                                            }

                                            SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable, null, null, null);

                                            SecondReactionButton.Text = this.GetString(Resource.String.Lbl_disliked);
                                            SecondReactionButton.SetTextColor(Color.ParseColor("#f89823"));
                                            break;
                                        }
                                    case PostButtonSystem.DisLike:
                                        {
                                            Drawable unwrappedDrawable = AppCompatResources.GetDrawable(this, Resource.Drawable.icon_post_dislike_vector);
                                            Drawable wrappedDrawable = DrawableCompat.Wrap(unwrappedDrawable);
                                            switch (Build.VERSION.SdkInt)
                                            {
                                                case <= BuildVersionCodes.Lollipop:
                                                    DrawableCompat.SetTint(wrappedDrawable, Color.ParseColor("#666666"));
                                                    break;
                                                default:
                                                    wrappedDrawable = wrappedDrawable.Mutate();
                                                    wrappedDrawable.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#666666"), PorterDuff.Mode.SrcAtop));
                                                    break;
                                            }

                                            SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable, null, null, null);

                                            SecondReactionButton.Text = this.GetString(Resource.String.Btn_Dislike);
                                            SecondReactionButton.SetTextColor(Color.ParseColor("#444444"));
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

        #endregion

    }
}