using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WoWonder.Activities.Base;
using WoWonder.Activities.Contacts;
using WoWonder.Activities.Live.Utils;
using WoWonder.Activities.NativePost.Extra;
using WoWonder.Activities.NativePost.Pages;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Activities.QrCode;
using WoWonder.Activities.SearchForPosts;
using WoWonder.Activities.SettingsPreferences.General;
using WoWonder.Activities.SettingsPreferences.Privacy;
using WoWonder.Activities.SettingsPreferences.TellFriend;
using WoWonder.Activities.Tabbes;
using WoWonder.Activities.Upgrade;
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
using WoWonderClient.Classes.Product;
using WoWonderClient.Classes.User;
using WoWonderClient.Requests;
using static WoWonder.Activities.NativePost.Extra.WRecyclerView;
using Console = System.Console;
using Exception = System.Exception;
using String = Java.Lang.String;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.MyProfile
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", WindowSoftInputMode = SoftInput.AdjustNothing | SoftInput.AdjustPan, ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MyProfileActivity : BaseActivity, IDialogListCallBack, AppBarLayout.IOnOffsetChangedListener
    {
        #region Variables Basic

        private AppBarLayout AppBarLayout;
        private CollapsingToolbarLayout CollapsingToolbar;

        private ImageView ImageCover, ImageBack;
        private ImageButton BtnMore;
        private TextView TxtSearchForPost;

        public CircleImageView ImageAvatar;
        private TextViewWithImages TxtName;
        private TextView TxtUsername;
        private AppCompatButton BtnEdit;
        private FrameLayout BtnQrCode, BtnTopMore;
        private TextView CountFollowers, CountFollowings, CountLikes, CountPoints, TxtFollowers, TxtFollowing;
        private LinearLayout LlCountFollowers, LlCountFollowing, LlCountLike, LlPoint;

        private SwipeRefreshLayout SwipeRefreshLayout;
        public WRecyclerView MainRecyclerView;

        private AdView MAdView;

        private FeedCombiner Combiner;
        private static MyProfileActivity Instance;
        private UserDataObject UserData;

        public NativePostAdapter PostFeedAdapter;

        private ViewStub ShimmerPageLayout;
        private View InflatedShimmer;
        private TemplateShimmerInflater ShimmerInflater;

        public string ImageType;

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
                SetContentView(Resource.Layout.MyProfile_Layout);

                Instance = this;

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                InitShimmer();
                SetRecyclerViewAdapters();


                GetMyInfoData();
                PostClickListener.OpenMyProfile = true;

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


                SwipeRefreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(WoWonderTools.IsTabDark() ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));

                ImageAvatar = FindViewById<CircleImageView>(Resource.Id.profileimage_head);
                TxtName = FindViewById<TextViewWithImages>(Resource.Id.name_profile);
                TxtUsername = FindViewById<TextView>(Resource.Id.username_profile);
                BtnEdit = FindViewById<AppCompatButton>(Resource.Id.btnEdit);

                BtnQrCode = FindViewById<FrameLayout>(Resource.Id.ll_qrCode);
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
                PostFeedAdapter = new NativePostAdapter(this, UserDetails.UserId, MainRecyclerView, NativeFeedType.User);
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
                        BtnEdit.Click += BtnEditOnClick;
                        BtnQrCode.Click += BtnQrCodeOnClick;
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
                        BtnEdit.Click -= BtnEditOnClick;
                        BtnQrCode.Click -= BtnQrCodeOnClick;
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

        public static MyProfileActivity GetInstance()
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
                Instance = null!;
                //ImageAvatar = null!;
                //ImageBack = null!;
                //BtnMore = null!;
                //TxtName = null!;
                //TxtUsername = null!;
                //BtnEdit = null!; 
                SwipeRefreshLayout = null!;
                MainRecyclerView = null!;
                MAdView = null!;
                Combiner = null!;
                UserData = null!;
                PostFeedAdapter = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event

        private void TxtSearchForPostOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(SearchForPostsActivity));
                intent.PutExtra("TypeSearch", "user");
                intent.PutExtra("IdSearch", UserDetails.UserId);
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        private void LlCountFollowersOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(MyContactsActivity));
                intent.PutExtra("ContactsType", "Followers");
                intent.PutExtra("UserId", UserDetails.UserId);

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
                intent.PutExtra("UserId", UserDetails.UserId);
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
                arrayAdapter.Add(GetText(Resource.String.Lbl_EditAvatar));
                arrayAdapter.Add(GetText(Resource.String.Lbl_EditCover));

                arrayAdapter.Add(GetText(Resource.String.Lbl_CopeLink));
                arrayAdapter.Add(GetText(Resource.String.Lbl_Share));
                arrayAdapter.Add(GetText(Resource.String.Lbl_Activities));
                arrayAdapter.Add(GetText(Resource.String.Lbl_ViewPrivacy));
                arrayAdapter.Add(GetText(Resource.String.Lbl_SettingsAccount));

                switch (ListUtils.SettingsSiteList?.Pro)
                {
                    case "1" when AppSettings.ShowGoPro && UserData.ProType != "4":
                        arrayAdapter.Add(GetText(Resource.String.Lbl_upgrade_now));
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


        private void ImageAvatarOnClick(object sender, EventArgs e)
        {
            try
            {
                OptionAvatarProfileDialogFragment dialogFragment = new OptionAvatarProfileDialogFragment();
                Bundle bundle = new Bundle();
                bundle.PutString("Page", "MyProfile");
                bundle.PutString("UserData", JsonConvert.SerializeObject(UserData));

                dialogFragment.Arguments = bundle;

                dialogFragment.Show(SupportFragmentManager, dialogFragment.Tag);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnQrCodeOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(QrCodeActivity));
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnEditOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(EditMyProfileActivity));
                StartActivityForResult(intent, 5124);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Get Profile

        private void GetMyInfoData()
        {
            try
            {
                var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                if (dataUser != null)
                {
                    LoadPassedDate(dataUser);

                    switch (ListUtils.MyFollowingList.Count)
                    {
                        case > 0 when dataUser.Details.DetailsClass != null:
                            LoadFriendsLayout(new List<UserDataObject>(ListUtils.MyFollowingList), Methods.FunString.FormatPriceValue(Convert.ToInt32(dataUser.Details.DetailsClass.FollowingCount)));
                            break;
                    }

                    PostFeedAdapter?.NotifyDataSetChanged();
                }

                PostFeedAdapter?.SetLoading();
                StartApiService();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { GetProfileApi });
        }

        private async Task GetProfileApi()
        {
            var (apiStatus, respond) = await RequestsAsync.Global.GetUserDataAsync(UserDetails.UserId, "user_data,following");

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

                        switch (result.Following.Count)
                        {
                            //if (SPrivacyFriend == "0" || result.UserProfileObject?.IsFollowing == "1" && SPrivacyFriend == "1" || SPrivacyFriend == "2")
                            case > 0 when result.UserData.Details.DetailsClass != null:
                                RunOnUiThread(() => { LoadFriendsLayout(result.Following, Methods.FunString.FormatPriceValue(Convert.ToInt32(result.UserData.Details.DetailsClass.FollowingCount))); });
                                break;
                        }

                        //##Set the AddBox place on Main RecyclerView
                        //------------------------------------------------------------------------
                        var check = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.FollowersBox);
                        if (check != null)
                        {
                            Combiner.AddPostDivider(PostFeedAdapter.ListDiffer.IndexOf(check) + 1);
                            Combiner.AddPostBoxPostView("user", PostFeedAdapter.ListDiffer.IndexOf(check) + 2);
                            //switch (AppSettings.ShowSearchForPosts)
                            //{
                            //    case true:
                            //        Combiner.SearchForPostsView("user");
                            //        break;
                            //}
                        }

                        //------------------------------------------------------------------------ 

                        PostFeedAdapter?.NotifyDataSetChanged();
                        var sqlEntity = new SqLiteDatabase();
                        sqlEntity.Insert_Or_Update_To_MyProfileTable(result.UserData);

                        switch (result.Following?.Count)
                        {
                            case > 0:
                                sqlEntity.Insert_Or_Replace_MyContactTable(new ObservableCollection<UserDataObject>(result.Following));
                                break;
                        }

                        ListUtils.MyFollowingList = result.Followers?.Count switch
                        {
                            > 0 => new ObservableCollection<UserDataObject>(result.Following),
                            _ => ListUtils.MyFollowingList
                        };

                        MainRecyclerView.ApiPostAsync.ExcuteDataToMainThread();

                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });


                //PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => MainRecyclerView.ApiPostAsync.FetchNewsFeedApiPosts() });
            }
        }

        private void LoadPassedDate(UserDataObject result)
        {
            try
            {
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

                TxtUsername.Text = "@" + result.Username;

                var name = WoWonderTools.GetNameFinal(result);
                var textHighLighter = name;

                if (result.Verified == "1")
                    textHighLighter += " " + "[img src=icon_checkmark_small_vector/]";

                if (result.IsPro == "1")
                    textHighLighter += " " + "[img src=post_icon_flash/]";

                TextViewWithImages.Publisher = result;
                var decoratedContent = TextViewWithImages.GetTextWithImages(null, this, new String(textHighLighter.ToArray(), 0, textHighLighter.Length));

                TxtName.SetText(decoratedContent);

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
                if (checkAboutBox == null)
                    Combiner.AboutBoxPostView(WoWonderTools.GetAboutFinal(result), 0);
                else
                    checkAboutBox.AboutModel.Description = WoWonderTools.GetAboutFinal(result);

                var checkInfoUserBox = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.InfoUserBox);
                if (checkInfoUserBox == null)
                    Combiner.InfoUserBoxPostView(result, 1);
                else
                    checkInfoUserBox.InfoUserModel.UserData = result;

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
                    BtnMore.Visibility = ViewStates.Visible;

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
                    }
                    else
                    {
                        Combiner.FollowersBoxPostView(followersClass, 2);
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
                if (text == GetText(Resource.String.Lbl_EditAvatar))
                {
                    ImageType = "Avatar";
                    PixImagePickerUtils.OpenDialogGallery(this);
                }
                else if (text == GetText(Resource.String.Lbl_EditCover))
                {
                    ImageType = "Cover";
                    PixImagePickerUtils.OpenDialogGallery(this);
                }
                else if (text == GetText(Resource.String.Lbl_CopeLink))
                {
                    OnCopeLinkToProfile_Button_Click();
                }
                else if (text == GetText(Resource.String.Lbl_Share))
                {
                    OnShare_Button_Click();
                }
                else if (text == GetText(Resource.String.Lbl_ViewPrivacy))
                {
                    OnViewPrivacy_Button_Click();
                }
                else if (text == GetText(Resource.String.Lbl_Activities))
                {
                    OnMyActivities_Button_Click();
                }
                else if (text == GetText(Resource.String.Lbl_SettingsAccount))
                {
                    OnSettingsAccount_Button_Click();
                }
                else if (text == GetText(Resource.String.Lbl_upgrade_now))
                {
                    UpgradeNow_Click();
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

        //Event Menu >> View Privacy Shortcuts
        private void OnViewPrivacy_Button_Click()
        {
            try
            {
                var intent = new Intent(this, typeof(PrivacyActivity));
                StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> View Privacy Shortcuts
        private void OnMyActivities_Button_Click()
        {
            try
            {
                var intent = new Intent(this, typeof(MyActivitiesActivity));
                StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> General Account 
        private void OnSettingsAccount_Button_Click()
        {
            try
            {
                var intent = new Intent(this, typeof(GeneralAccountActivity));
                StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void UpgradeNow_Click()
        {
            try
            {
                var intent = new Intent(this, typeof(GoProActivity));
                StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Update Image Avatar && Cover

        private async Task Update_Image_Api(string type, string path)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
                else
                {
                    if (type == "Avatar")
                    {
                        var (apiStatus, respond) = await RequestsAsync.Global.UpdateUserAvatarAsync(path);
                        if (apiStatus == 200)
                        {
                            if (respond is MessageObject result)
                            {
                                RunOnUiThread(() =>
                                {
                                    try
                                    {
                                        Console.WriteLine(result.Message);
                                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Image_changed_successfully), ToastLength.Short);

                                        var file2 = new File(path);
                                        var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);
                                        Glide.With(this).Load(photoUri).Apply(new RequestOptions().CircleCrop()).Into(ImageAvatar);
                                        //Set image  
                                        //GlideImageLoader.LoadImage(this, path, UserProfileImage, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                                        var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                                        if (dataUser != null)
                                        {
                                            dataUser.Avatar = path;

                                            var sqLiteDatabase = new SqLiteDatabase();
                                            sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);
                                        }

                                        PostFeedAdapter.NotifyDataSetChanged();

                                        var instance = TabbedMainActivity.GetInstance();
                                        if (instance != null)
                                        {
                                            GlideImageLoader.LoadImage(instance, UserDetails.Avatar, instance.MoreTab?.ProfileImage, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);

                                            if (instance.NewsFeedTab.PostFeedAdapter != null)
                                            {
                                                instance.NewsFeedTab.PostFeedAdapter.NotifyDataSetChanged();
                                                instance.NewsFeedTab.PostFeedAdapter.HolderStory?.StoryAdapter?.NotifyDataSetChanged();
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e);
                                    }
                                });
                            }
                        }
                        else
                            Methods.DisplayReportResult(this, respond);
                    }
                    else if (type == "Cover")
                    {
                        var (apiStatus, respond) = await RequestsAsync.Global.UpdateUserCoverAsync(path);
                        if (apiStatus == 200)
                        {
                            if (respond is MessageObject result)
                            {
                                RunOnUiThread(() =>
                                {
                                    try
                                    {
                                        Console.WriteLine(result.Message);
                                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Image_changed_successfully), ToastLength.Short);

                                        //Set image 
                                        var file2 = new File(path);
                                        var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);
                                        Glide.With(this).Load(photoUri).Apply(new RequestOptions()).Into(ImageCover);
                                        //GlideImageLoader.LoadImage(this, path, CoverImage, ImageStyle.FitCenter, ImagePlaceholders.Drawable);

                                        var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                                        if (dataUser != null)
                                        {
                                            dataUser.Cover = path;

                                            var sqLiteDatabase = new SqLiteDatabase();
                                            sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e);
                                    }
                                });
                            }
                        }
                        else
                            Methods.DisplayReportResult(this, respond);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
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
                //Edit post
                if (requestCode == 3950 && resultCode == Result.Ok)
                {
                    var postId = data.GetStringExtra("PostId") ?? "";
                    var postText = data.GetStringExtra("PostText") ?? "";
                    var diff = PostFeedAdapter?.ListDiffer;
                    var dataGlobal = diff?.Where(a => a.PostData?.Id == postId).ToList();
                    if (dataGlobal?.Count > 0)
                    {
                        foreach (var postData in dataGlobal)
                        {
                            postData.PostData.Orginaltext = postText;
                            var index = diff.IndexOf(postData);
                            if (index > -1) PostFeedAdapter?.NotifyItemChanged(index);
                        }

                        var checkTextSection = dataGlobal.FirstOrDefault(w => w.TypeView == PostModelType.TextSectionPostPart);
                        if (checkTextSection == null)
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
                            if (headerPostIndex > -1)
                            {
                                diff.Insert(headerPostIndex + 1, item);
                                PostFeedAdapter?.NotifyItemInserted(headerPostIndex + 1);
                            }
                        }
                    }
                }
                //Edit post product 
                else if (requestCode == 3500 && resultCode == Result.Ok)
                {
                    if (string.IsNullOrEmpty(data.GetStringExtra("itemData"))) return;
                    var item = JsonConvert.DeserializeObject<ProductDataObject>(data?.GetStringExtra("itemData") ?? "");
                    if (item != null)
                    {
                        var diff = PostFeedAdapter?.ListDiffer;
                        var dataGlobal = diff.Where(a => a.PostData?.Id == item.PostId).ToList();
                        if (dataGlobal.Count > 0)
                            foreach (var postData in dataGlobal)
                            {
                                var index = diff.IndexOf(postData);
                                if (index > -1)
                                {
                                    var productUnion = postData.PostData.Product?.ProductClass;
                                    if (productUnion != null) productUnion.Id = item.Id;
                                    productUnion = item;
                                    Console.WriteLine(productUnion);

                                    PostFeedAdapter?.NotifyItemChanged(PostFeedAdapter.ListDiffer.IndexOf(postData));
                                }
                            }
                    }
                }
                //Edit profile 
                else if (requestCode == 5124 && resultCode == Result.Ok)
                {
                    var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                    if (dataUser != null)
                    {
                        LoadPassedDate(dataUser);
                        PostFeedAdapter?.NotifyDataSetChanged();
                    }
                }
                else if (requestCode == PixImagePickerActivity.RequestCode && resultCode == Result.Ok)
                {
                    var listPath = JsonConvert.DeserializeObject<ResultIntentPixImage>(data.GetStringExtra("ResultPixImage") ?? "");
                    if (listPath?.List?.Count > 0)
                    {
                        var filepath = listPath.List.FirstOrDefault();
                        if (!string.IsNullOrEmpty(filepath))
                        {
                            //Do something with your Uri
                            switch (ImageType)
                            {
                                case "Cover":
                                    UserDetails.Cover = filepath;
                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => Update_Image_Api(ImageType, filepath) });
                                    break;
                                case "Avatar":
                                    UserDetails.Avatar = filepath;
                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => Update_Image_Api(ImageType, filepath) });
                                    break;
                            }
                        }
                        else
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long)?.Show();
                        }
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
                    case 108 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        PixImagePickerUtils.OpenDialogGallery(this);
                        break;
                    case 108:
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                        break;
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

    }
}