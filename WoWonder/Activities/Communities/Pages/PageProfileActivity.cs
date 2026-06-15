using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using AndroidX.CardView.Widget;
using AndroidX.Core.Content;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Com.Google.Android.Gms.Ads;
using Google.Android.Material.AppBar;
using Google.Android.Material.Dialog;
using Google.Android.Material.FloatingActionButton;
using Java.Lang;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WoWonder.Activities.AddPost;
using WoWonder.Activities.Base;
using WoWonder.Activities.Communities.Adapters;
using WoWonder.Activities.Communities.Pages.Settings;
using WoWonder.Activities.Live.Utils;
using WoWonder.Activities.MyProfile;
using WoWonder.Activities.NativePost.Extra;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Activities.Upgrade;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.Share;
using WoWonder.Library.Anjo.Share.Abstractions;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Message;
using WoWonderClient.Classes.Page;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Classes.Product;
using WoWonderClient.Requests;
using static WoWonder.Activities.NativePost.Extra.WRecyclerView;
using Exception = System.Exception;
using File = Java.IO.File;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.Communities.Pages
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", WindowSoftInputMode = SoftInput.AdjustNothing | SoftInput.AdjustPan, ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class PageProfileActivity : BaseActivity, IDialogListCallBack, IDialogInputCallBack, AppBarLayout.IOnOffsetChangedListener
    {
        #region Variables Basic

        private AppBarLayout AppBarLayout;
        private CollapsingToolbarLayout CollapsingToolbar;

        private SwipeRefreshLayout SwipeRefreshLayout;
        public ImageView ProfileImage;
        private ImageView CoverImage, IconBack;
        private TextView TxtSearchForPost, TxtPageName, TxtPageUsername;
        private ImageButton BtnMore;
        private FloatingActionButton FloatingActionButtonView;
        private RelativeLayout EditAvatarImagePage, EditCoverImage;
        public WRecyclerView MainRecyclerView;
        public NativePostAdapter PostFeedAdapter;
        private string PageId = "", UserId = "";
        public static PageDataObject PageData;
        private FeedCombiner Combiner;
        private static PageProfileActivity Instance;
        private AdView MAdView;
        private CardView LikeCardView, ActionCardView, MessageCardView;
        private TextView LikeTxt, ActionTxt, MessageTxt;

        private string TypeDialog;
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
                SetContentView(Resource.Layout.PageProfileLayout);

                Instance = this;
                PageId = Intent?.GetStringExtra("PageId") ?? string.Empty;

                //Get Value And Set Toolbar
                InitComponent();
                SetRecyclerViewAdapters();


                GetDataPage();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

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
                MainRecyclerView?.ReleasePlayer();
                AdsGoogle.LifecycleAdView(MAdView, "Destroy");
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

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                AppBarLayout = FindViewById<AppBarLayout>(Resource.Id.appBarLayout);
                AppBarLayout.SetExpanded(true);
                AppBarLayout.AddOnOffsetChangedListener(this);

                CollapsingToolbar = (CollapsingToolbarLayout)FindViewById(Resource.Id.collapsingToolbar);
                CollapsingToolbar.Title = " ";

                TxtSearchForPost = FindViewById<TextView>(Resource.Id.tv_SearchForPost);
                TxtSearchForPost.Visibility = ViewStates.Invisible;

                MainRecyclerView = FindViewById<WRecyclerView>(Resource.Id.newsfeedRecyler);

                SwipeRefreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(WoWonderTools.IsTabDark() ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));

                ProfileImage = (ImageView)FindViewById(Resource.Id.image_profile);
                CoverImage = (ImageView)FindViewById(Resource.Id.iv1);
                IconBack = (ImageView)FindViewById(Resource.Id.image_back);
                EditAvatarImagePage = (RelativeLayout)FindViewById(Resource.Id.LinearEdit);
                EditCoverImage = (RelativeLayout)FindViewById(Resource.Id.cover_layout);
                TxtPageName = (TextView)FindViewById(Resource.Id.Page_name);
                TxtPageUsername = (TextView)FindViewById(Resource.Id.Page_Username);

                BtnMore = (ImageButton)FindViewById(Resource.Id.morebutton);

                FloatingActionButtonView = FindViewById<FloatingActionButton>(Resource.Id.floatingActionButtonView);
                FloatingActionButtonView.Visibility = ViewStates.Gone;

                MAdView = FindViewById<AdView>(Resource.Id.adView);
                AdsGoogle.InitAdView(MAdView, MainRecyclerView);

                // buttons with cardview
                LikeCardView = FindViewById<CardView>(Resource.Id.likeButton);
                ActionCardView = FindViewById<CardView>(Resource.Id.actionButton);
                MessageCardView = FindViewById<CardView>(Resource.Id.messagebutton);
                LikeCardView.Tag = "UserPage";

                LikeTxt = FindViewById<TextView>(Resource.Id.likeTxt);
                ActionTxt = FindViewById<TextView>(Resource.Id.actionTxt);
                MessageTxt = FindViewById<TextView>(Resource.Id.msgTxt);

                if (AppSettings.MessengerIntegration)
                    MessageCardView.Visibility = ViewStates.Gone;
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
                PostFeedAdapter = new NativePostAdapter(this, PageId, MainRecyclerView, NativeFeedType.Page);
                MainRecyclerView?.SetXAdapter(PostFeedAdapter, SwipeRefreshLayout);
                Combiner = new FeedCombiner(null, PostFeedAdapter?.ListDiffer, this, NativeFeedType.Page);

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
                        SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                        LikeCardView.Click += BtnLikeOnClick;
                        BtnMore.Click += BtnMoreOnClick;
                        EditCoverImage.Click += TxtEditPageInfoOnClick;
                        IconBack.Click += IconBackOnClick;
                        EditAvatarImagePage.Click += UserProfileImageOnClick;
                        FloatingActionButtonView.Click += AddPostOnClick;
                        MessageCardView.Click += MessageButtonOnClick;
                        ProfileImage.Click += UserProfileImageOnClick;
                        CoverImage.Click += CoverImageOnClick;
                        break;
                    default:
                        SwipeRefreshLayout.Refresh -= SwipeRefreshLayoutOnRefresh;
                        LikeCardView.Click -= BtnLikeOnClick;
                        BtnMore.Click -= BtnMoreOnClick;
                        EditCoverImage.Click -= TxtEditPageInfoOnClick;
                        IconBack.Click -= IconBackOnClick;
                        EditAvatarImagePage.Click -= UserProfileImageOnClick;
                        FloatingActionButtonView.Click -= AddPostOnClick;
                        MessageCardView.Click -= MessageButtonOnClick;
                        ProfileImage.Click -= UserProfileImageOnClick;
                        CoverImage.Click -= CoverImageOnClick;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static PageProfileActivity GetInstance()
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
                SwipeRefreshLayout = null!;
                ProfileImage = null!;
                CoverImage = null!;
                IconBack = null!;
                TxtPageName = null!;
                TxtPageUsername = null!;
                LikeCardView = null!;
                ActionCardView = null!;
                BtnMore = null!;
                MessageCardView = null!;
                LikeTxt = null!;
                ActionTxt = null!;
                MessageTxt = null!;
                FloatingActionButtonView = null!;
                EditCoverImage = null!;
                MainRecyclerView = null!;
                PostFeedAdapter = null!;
                PageId = null!;
                UserId = null!;
                PageData = null!;
                EditAvatarImagePage = null!;
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
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    return;
                }

                PostFeedAdapter?.ListDiffer?.Clear();
                PostFeedAdapter?.NotifyDataSetChanged();

                if (PageData != null)
                    LoadPassedData(PageData);

                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open image Cover
        private void CoverImageOnClick(object sender, EventArgs e)
        {
            try
            {
                var media = WoWonderTools.GetFile("", Methods.Path.FolderDiskImage, PageData.Cover.Split('/').Last(), PageData.Cover);
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
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open image Avatar
        private void UserProfileImageOnClick(object sender, EventArgs e)
        {
            try
            {
                OptionAvatarProfileDialogFragment dialogFragment = new OptionAvatarProfileDialogFragment();
                Bundle bundle = new Bundle();
                bundle.PutString("Page", "PageProfile");
                bundle.PutString("PageData", JsonConvert.SerializeObject(PageData));

                dialogFragment.Arguments = bundle;

                dialogFragment.Show(SupportFragmentManager, dialogFragment.Tag);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        // Rating Page 
        public void RatingLinerOnClick()
        {
            try
            {
                if (PageData.IsRated != null && PageData.IsRated.Value)
                {
                    // You have already rated this page!
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_YouHaveAlReadyRatedThisPage), ToastLength.Short);
                }
                else
                {
                    var dialog = new DialogRatingBarFragment(this, PageId, PageData);
                    dialog.Show(SupportFragmentManager, dialog.Tag);
                    dialog.OnUpComplete += DialogOnOnUpComplete;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void DialogOnOnUpComplete(object sender, DialogRatingBarFragment.RatingBarUpEventArgs e)
        {
            try
            {
                var th = new Thread(ActLikeARequest);
                th.Start();
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        private void ActLikeARequest()
        {
            var x = Resource.Animation.slide_right;
            Console.WriteLine(x);
        }

        //Event Add New post  
        private void AddPostOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(AddPostActivity));
                intent.PutExtra("Type", "SocialPage");
                intent.PutExtra("PostId", PageId);
                intent.PutExtra("itemObject", JsonConvert.SerializeObject(PageData));
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void IconBackOnClick(object sender, EventArgs e)
        {
            Finish();
        }

        private void TxtEditPageInfoOnClick(object sender, EventArgs e)
        {
            try
            {
                ImageType = "Cover";
                PixImagePickerUtils.OpenDialogGallery(this);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Show More : Copy Link , Share , Edit (If user isOwner_Pages)
        private void BtnMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialAlertDialogBuilder(this);

                arrayAdapter.Add(GetString(Resource.String.Lbl_CopeLink));
                arrayAdapter.Add(GetString(Resource.String.Lbl_Share));
                arrayAdapter.Add(GetString(Resource.String.Lbl_Reviews));
                if (PageData.IsPageOnwer != null && PageData.IsPageOnwer.Value)
                {
                    switch (PageData?.Boosted)
                    {
                        case "0":
                            arrayAdapter.Add(GetString(Resource.String.Lbl_BoostPage));
                            break;
                        case "1":
                            arrayAdapter.Add(GetString(Resource.String.Lbl_UnBoostPage));
                            break;
                    }
                    arrayAdapter.Add(GetString(Resource.String.Lbl_Settings));
                }

                if (PageData.IsReported != null && PageData.IsReported.Value)
                    arrayAdapter.Add(GetText(Resource.String.Lbl_CancelReport));
                else
                    arrayAdapter.Add(GetText(Resource.String.Lbl_ReportThisPage));

                dialogList.SetTitle(GetString(Resource.String.Lbl_More));
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Like => like , dislike 
        private async void BtnLikeOnClick(object sender, EventArgs e)
        {
            try
            {
                if (LikeCardView?.Tag?.ToString() == "MyPage")
                {
                    SettingsPage_OnClick();
                }
                else
                {
                    if (!Methods.CheckConnectivity())
                    {
                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                        return;
                    }

                    if (LikeCardView?.Tag?.ToString() == "false")
                    {
                        LikeCardView.BackgroundTintList = WoWonderTools.IsTabDark() ? ColorStateList.ValueOf(Color.ParseColor("#282828")) : ColorStateList.ValueOf(Color.ParseColor("#FFFEFE"));
                        LikeTxt.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                        LikeTxt.Text = GetText(Resource.String.Btn_Liked);
                        LikeCardView.Tag = "true";
                    }
                    else
                    {
                        LikeCardView.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));
                        LikeTxt.SetTextColor(Color.White);
                        LikeTxt.Text = GetText(Resource.String.Btn_Like);
                        LikeCardView.Tag = "false";
                    }

                    var (apiStatus, respond) = await RequestsAsync.Page.LikePageAsync(PageId);
                    if (apiStatus == 200)
                    {
                        if (respond is LikePageObject result)
                        {
                            var isLiked = result.LikeStatus == "unliked" ? "false" : "true";
                            if (isLiked == "false")
                            {
                                LikeCardView.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));
                                LikeTxt.SetTextColor(Color.White);
                                LikeTxt.Text = GetText(Resource.String.Btn_Like);
                                LikeCardView.Tag = "false";
                            }
                            else
                            {
                                LikeCardView.BackgroundTintList = WoWonderTools.IsTabDark() ? ColorStateList.ValueOf(Color.ParseColor("#282828")) : ColorStateList.ValueOf(Color.ParseColor("#FFFEFE"));
                                LikeTxt.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                                LikeTxt.Text = GetText(Resource.String.Btn_Liked);
                                LikeCardView.Tag = "true";
                            }
                        }
                    }
                    else
                        Methods.DisplayReportResult(this, respond);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Send Message to page  
        private void MessageButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                switch (AppSettings.MessengerIntegration)
                {
                    case true when AppSettings.ShowDialogAskOpenMessenger:
                        {
                            var dialog = new MaterialAlertDialogBuilder(this);

                            dialog.SetTitle(Resource.String.Lbl_Warning);
                            dialog.SetMessage(GetText(Resource.String.Lbl_ContentAskOPenAppMessenger));
                            dialog.SetPositiveButton(GetText(Resource.String.Lbl_Yes), (materialDialog, action) =>
                            {
                                try
                                {
                                    Methods.App.OpenAppByPackageName(this, AppSettings.MessengerPackageName, "OpenChatPage", new ChatObject { UserId = UserId, PageId = PageId, PageName = PageData.PageName, Avatar = PageData.Avatar });
                                }
                                catch (Exception exception)
                                {
                                    Methods.DisplayReportResultTrack(exception);
                                }
                            });
                            dialog.SetNegativeButton(GetText(Resource.String.Lbl_No), new MaterialDialogUtils());

                            dialog.Show();
                            break;
                        }
                    case true:
                        Methods.App.OpenAppByPackageName(this, AppSettings.MessengerPackageName, "OpenChatPage", new ChatObject { UserId = UserId, PageId = PageId, PageName = PageData.PageName, Avatar = PageData.Avatar });
                        break;
                    default:
                        {
                            if (!Methods.CheckConnectivity())
                            {
                                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                                return;
                            }

                            TypeDialog = "SendMessageToPage";
                            var dialog = new MaterialAlertDialogBuilder(this);

                            dialog.SetTitle(GetString(Resource.String.Lbl_SendMessageTo) + " " + Methods.FunString.DecodeString(PageData.Name));

                            EditText input = new EditText(this);
                            input.SetHint(Resource.String.Lbl_WriteMessage);
                            input.InputType = InputTypes.TextFlagImeMultiLine;
                            LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                            input.LayoutParameters = lp;

                            dialog.SetView(input);

                            dialog.SetPositiveButton(GetText(Resource.String.Btn_Send), new MaterialDialogUtils(input, this));
                            dialog.SetNegativeButton(GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());
                            dialog.Show();

                            break;
                        }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Invite friends to like this Page
        private void MembersLinerOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(InviteMembersPageActivity));
                intent.PutExtra("PageId", PageId);
                StartActivity(intent);
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
                //Edit post
                if (requestCode == 3950 && resultCode == Result.Ok)
                {
                    var postId = data.GetStringExtra("PostId") ?? "";
                    var postText = data.GetStringExtra("PostText") ?? "";
                    var diff = PostFeedAdapter?.ListDiffer;
                    var dataGlobal = diff.Where(a => a.PostData?.Id == postId).ToList();
                    if (dataGlobal.Count > 0)
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

                            var headerPostIndex =
                                diff.IndexOf(dataGlobal.FirstOrDefault(w =>
                                    w.TypeView == PostModelType.HeaderPost));
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
                    var item = JsonConvert.DeserializeObject<ProductDataObject>(data.GetStringExtra("itemData") ?? "");
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

                                    PostFeedAdapter?.NotifyItemChanged(
                                        PostFeedAdapter.ListDiffer.IndexOf(postData));
                                }
                            }
                    }
                }
                else if (requestCode == 2005 && resultCode == Result.Ok)
                {
                    var result = data.GetStringExtra("pageItem");
                    var item = JsonConvert.DeserializeObject<PageDataObject>(result ?? string.Empty);
                    if (item != null)
                        LoadPassedData(item);
                }
                else if (requestCode == 2019 && resultCode == Result.Ok)
                {
                    var instance = PagesActivity.GetInstance();
                    var manged = instance?.MAdapter?.SocialList?.FirstOrDefault(a => a.Page?.PageId == PageId && a.TypeView == SocialModelType.MangedPages);
                    if (manged?.Page != null)
                    {
                        instance.MAdapter.SocialList.Remove(manged);
                        instance.MAdapter.NotifyDataSetChanged();

                        ListUtils.MyPageList.Remove(manged.Page);
                    }
                    Finish();
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
                            if (ImageType == "Cover")
                            {
                                //Set image 
                                switch (AppSettings.CoverImageStyle)
                                {
                                    case CoverImageStyle.CenterCrop:
                                        Glide.With(this).Load(filepath).Apply(new RequestOptions().CenterCrop().Error(Resource.Drawable.Cover_image)).Into(CoverImage);
                                        break;
                                    case CoverImageStyle.FitCenter:
                                        Glide.With(this).Load(filepath).Apply(new RequestOptions().FitCenter().Error(Resource.Drawable.Cover_image)).Into(CoverImage);
                                        break;
                                    default:
                                        Glide.With(this).Load(filepath).Apply(new RequestOptions().Error(Resource.Drawable.Cover_image)).Into(CoverImage);
                                        break;
                                }

                                UpdateImagePage_Api(ImageType, filepath);
                            }
                            else if (ImageType == "Avatar")
                            {
                                //Set image
                                Glide.With(this).Load(filepath).Apply(new RequestOptions().CircleCrop()).Into(ProfileImage);

                                //GlideImageLoader.LoadImage(this, photoUri.ToString(), ProfileImage, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                                var dataPage = PagesActivity.GetInstance()?.MAdapter?.SocialList.FirstOrDefault(a =>
                                    a.Page?.PageId == PageId && a.TypeView == SocialModelType.MangedPages);
                                if (dataPage?.Page != null)
                                {
                                    dataPage.Page.Avatar = filepath;
                                    PagesActivity.GetInstance()?.MAdapter?.NotifyDataSetChanged();

                                    var dataPage2 = ListUtils.MyPageList.FirstOrDefault(a => a.PageId == PageId);
                                    if (dataPage2 != null)
                                    {
                                        dataPage2.Avatar = filepath;
                                    }
                                }

                                UpdateImagePage_Api(ImageType, filepath);
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

        #region Update Image Avatar && Cover

        // Function Update Image Page : Avatar && Cover
        private async void UpdateImagePage_Api(string type, string path)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
                else
                {
                    switch (type)
                    {
                        case "Avatar":
                            {
                                var (apiStatus, respond) = await RequestsAsync.Page.UpdatePageAvatarAsync(PageId, path).ConfigureAwait(false);
                                switch (apiStatus)
                                {
                                    case 200:
                                        {
                                            switch (respond)
                                            {
                                                case MessageObject result:
                                                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Image_changed_successfully), ToastLength.Short);

                                                    //GlideImageLoader.LoadImage(this, file.Path, ProfileImage, ImageStyle.RoundedCrop, ImagePlaceholders.Color);
                                                    break;
                                            }

                                            break;
                                        }
                                    default:
                                        Methods.DisplayReportResult(this, respond);
                                        break;
                                }

                                break;
                            }
                        case "Cover":
                            {
                                var (apiStatus, respond) = await RequestsAsync.Page.UpdatePageCoverAsync(PageId, path).ConfigureAwait(false);
                                switch (apiStatus)
                                {
                                    case 200:
                                        {
                                            if (respond is not MessageObject result)
                                                return;

                                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Image_changed_successfully), ToastLength.Short);

                                            //GlideImageLoader.LoadImage(this, file.Path, CoverImage, ImageStyle.CenterCrop, ImagePlaceholders.Color);
                                            break;
                                        }
                                    default:
                                        Methods.DisplayReportResult(this, respond);
                                        break;
                                }

                                break;
                            }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Get Data Page

        private void GetDataPage()
        {
            try
            {
                PageData = JsonConvert.DeserializeObject<PageDataObject>(Intent?.GetStringExtra("PageObject") ?? "");
                if (PageData != null)
                {
                    LoadPassedData(PageData);
                }

                PostFeedAdapter?.SetLoading();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }

            StartApiService();
        }

        private void LoadPassedData(PageDataObject pageData)
        {
            try
            {
                PageData = pageData;
                UserId = pageData.IsPageOnwer != null && pageData.IsPageOnwer.Value ? UserDetails.UserId : pageData.UserId;

                GlideImageLoader.LoadImage(this, pageData.Avatar, ProfileImage, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);

                switch (AppSettings.CoverImageStyle)
                {
                    case CoverImageStyle.CenterCrop:
                        Glide.With(this).Load(pageData.Cover.Replace(" ", "")).Apply(new RequestOptions().CenterCrop().Error(Resource.Drawable.Cover_image)).Into(CoverImage);
                        break;
                    case CoverImageStyle.FitCenter:
                        Glide.With(this).Load(pageData.Cover.Replace(" ", "")).Apply(new RequestOptions().FitCenter().Error(Resource.Drawable.Cover_image)).Into(CoverImage);
                        break;
                    default:
                        Glide.With(this).Load(pageData.Cover.Replace(" ", "")).Apply(new RequestOptions().Error(Resource.Drawable.Cover_image)).Into(CoverImage);
                        break;
                }

                if (pageData.IsPageOnwer != null && pageData.IsPageOnwer.Value)
                {
                    LikeCardView.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));
                    LikeTxt.Text = GetText(Resource.String.Lbl_Edit);
                    LikeTxt.SetTextColor(Color.White);
                    LikeCardView.Tag = "MyPage";
                    //BtnMore.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));
                    //BtnMore.ImageTintList = ColorStateList.ValueOf(Color.White); 
                }
                else
                {
                    if (WoWonderTools.IsLikedPage(pageData))
                    {
                        LikeCardView.BackgroundTintList = WoWonderTools.IsTabDark() ? ColorStateList.ValueOf(Color.ParseColor("#282828")) : ColorStateList.ValueOf(Color.ParseColor("#FFFEFE"));
                        LikeTxt.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                        LikeTxt.Text = GetText(Resource.String.Btn_Liked);
                        LikeCardView.Tag = "true";
                    }
                    else
                    {
                        LikeCardView.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));
                        LikeTxt.SetTextColor(Color.White);
                        LikeTxt.Text = GetText(Resource.String.Btn_Like);
                        LikeCardView.Tag = "false";
                    }
                }

                if (pageData.IsPageOnwer != null && pageData.IsPageOnwer.Value)
                {
                    EditAvatarImagePage.Visibility = ViewStates.Visible;
                    EditCoverImage.Visibility = ViewStates.Visible;
                    FloatingActionButtonView.Visibility = ViewStates.Visible;
                    MessageCardView.Visibility = ViewStates.Gone;
                }
                else
                {
                    EditAvatarImagePage.Visibility = ViewStates.Gone;
                    EditCoverImage.Visibility = ViewStates.Gone;
                    FloatingActionButtonView.Visibility = ViewStates.Gone;
                    MessageCardView.Visibility = ViewStates.Visible;
                }

                var modelsClass = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.InfoPageBox);
                switch (modelsClass)
                {
                    case null:
                        Combiner.InfoPageBox(new PageInfoModelClass { PageClass = pageData, PageId = pageData.PageId }, 0);
                        break;
                    default:
                        modelsClass.PageInfoModelClass = new PageInfoModelClass
                        {
                            PageClass = pageData,
                            PageId = pageData.PageId
                        };
                        PostFeedAdapter?.NotifyItemChanged(PostFeedAdapter.ListDiffer.IndexOf(modelsClass));
                        break;
                }

                if (!string.IsNullOrEmpty(pageData.About))
                {
                    var checkAboutBox = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.AboutBox);
                    switch (checkAboutBox)
                    {
                        case null:
                            Combiner.AboutBoxPostView(Methods.FunString.DecodeString(pageData.About), 0);
                            break;
                        default:
                            checkAboutBox.AboutModel.Description = Methods.FunString.DecodeString(pageData.About);
                            PostFeedAdapter?.NotifyItemChanged(PostFeedAdapter.ListDiffer.IndexOf(checkAboutBox));
                            break;
                    }
                }

                if (pageData.IsPageOnwer != null && pageData.IsPageOnwer.Value || pageData.UsersPost == "1")
                {
                    var checkSection = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.AddPostBox);
                    switch (checkSection)
                    {
                        case null:
                            {
                                Combiner.AddPostBoxPostView("Page", -1, new PostDataObject { PageId = pageData.PageId, Publisher = new PublisherPost { PageName = pageData.PageName, Avatar = pageData.Avatar } });

                                //switch (AppSettings.ShowSearchForPosts)
                                //{
                                //    case true:
                                //        Combiner.SearchForPostsView("Page", new PostDataObject { PageId = pageData.PageId, Publisher = new PublisherPost { PageName = pageData.PageName, Avatar = pageData.Avatar } });
                                //        break;
                                //}

                                PostFeedAdapter?.NotifyItemInserted(PostFeedAdapter.ListDiffer.Count - 1);
                                break;
                            }
                    }

                    FloatingActionButtonView.Visibility = ViewStates.Visible;
                }

                TxtPageUsername.Text = "@" + pageData.Username;
                TxtPageName.Text = Methods.FunString.DecodeString(pageData.Name);

                TxtPageName.SetTextColor(Color.ParseColor(WoWonderTools.IsTabDark() ? "#ffffff" : "#000000"));
                TxtPageUsername.SetTextColor(Color.ParseColor(WoWonderTools.IsTabDark() ? "#ffffff" : "#C3C7D0"));

                SetCallActionsButtons(pageData);
                SetAdminInfo(pageData);

                WoWonderTools.GetFile("", Methods.Path.FolderDiskImage, pageData.Cover.Split('/').Last(), pageData.Cover);
                WoWonderTools.GetFile("", Methods.Path.FolderDiskImage, pageData.Avatar.Split('/').Last(), pageData.Avatar);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetCallActionsButtons(PageDataObject pageData)
        {
            try
            {

                if (pageData.CallActionType != "0" && !string.IsNullOrEmpty(pageData.CallActionTypeUrl))
                {
                    var name = "Lbl_call_action_" + pageData.CallActionType;
                    var resourceId = Resources?.GetIdentifier(name, "string", ApplicationInfo?.PackageName) ?? 0;
                    ActionCardView.Visibility = ViewStates.Visible;
                    ActionTxt.Text = Resources?.GetString(resourceId);
                    switch (ActionCardView.HasOnClickListeners)
                    {
                        case false:
                            ActionCardView.Click += (sender, args) =>
                            {
                                try
                                {
                                    switch (string.IsNullOrEmpty(pageData.CallActionTypeUrl))
                                    {
                                        case false:
                                            new IntentController(this).OpenBrowserFromApp(pageData.CallActionTypeUrl);
                                            break;
                                        default:
                                            ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_call_action_sorry), ToastLength.Short);
                                            break;
                                    }
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            };
                            break;
                    }
                }
                else
                {
                    ReplaceView(ActionCardView, LikeCardView);
                }

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetAdminInfo(PageDataObject pageData)
        {
            try
            {
                if (pageData.AdminInfo?.AdminInfoClass != null && pageData.AdminInfo?.AdminInfoClass?.UserId == UserDetails.UserId)
                {
                    switch (pageData.AdminInfo?.AdminInfoClass.Avatar)
                    {
                        case "0":
                            EditCoverImage.Visibility = ViewStates.Gone;
                            EditAvatarImagePage.Visibility = ViewStates.Gone;
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void ReplaceView(View currentView, View newView)
        {
            var parent = GetParent(currentView);
            switch (parent)
            {
                case null:
                    return;
            }
            var index = parent.IndexOfChild(currentView);
            RemoveView(currentView);
            RemoveView(newView);
            parent.AddView(newView, index);
        }

        private ViewGroup GetParent(View view)
        {
            return (ViewGroup)view.Parent;
        }

        private void RemoveView(View view)
        {
            var parent = GetParent(view);
            parent?.RemoveView(view);
        }

        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { GetPageDataApi, () => MainRecyclerView.ApiPostAsync.FetchNewsFeedApiPosts() });
        }

        private async Task GetPageDataApi()
        {
            var (apiStatus, respond) = await RequestsAsync.Page.GetPageDataAsync(PageId);

            if (apiStatus != 200 || respond is not GetPageDataObject result || result.PageData == null)
                Methods.DisplayReportResult(this, respond);
            else
            {
                PageData = result.PageData;
                RunOnUiThread(() => LoadPassedData(PageData));
            }
        }

        #endregion

        #region MaterialDialog

        public void OnSelection(IDialogInterface dialog, int position, string itemString)
        {
            try
            {
                var text = itemString;
                if (text == GetString(Resource.String.Lbl_CopeLink))
                {
                    CopyLinkEvent();
                }
                else if (text == GetString(Resource.String.Lbl_Share))
                {
                    ShareEvent();
                }
                else if (text == GetString(Resource.String.Lbl_Settings))
                {
                    SettingsPage_OnClick();
                }
                else if (text == GetString(Resource.String.Lbl_BoostPage) || text == GetString(Resource.String.Lbl_UnBoostPage))
                {
                    BoostPageEvent();
                }
                else if (text == GetString(Resource.String.Lbl_Reviews))
                {
                    ReviewsEvent();
                }
                else if (text == GetText(Resource.String.Lbl_ReportThisPage))
                {
                    OnReport_Button_Click();
                }
                else if (text == GetText(Resource.String.Lbl_CancelReport))
                {
                    if (!Methods.CheckConnectivity())
                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    else
                    {
                        PageData.IsReported = false;

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Page.ReportPageAsync(PageId, "") });
                    }
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

                TypeDialog = "ReportThisPage";
                var dialog = new MaterialAlertDialogBuilder(this);
                dialog.SetTitle(GetString(Resource.String.Lbl_ReportThisPage));

                EditText input = new EditText(this);
                input.SetHint(Resource.String.text);
                input.InputType = InputTypes.TextFlagImeMultiLine;
                LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                input.LayoutParameters = lp;

                dialog.SetView(input);

                dialog.SetPositiveButton(GetText(Resource.String.Btn_Send), new MaterialDialogUtils(input, this));
                dialog.SetNegativeButton(GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());

                dialog.Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnInput(IDialogInterface dialog, string input)
        {
            //Send Message to page 
            try
            {
                if (input.Length > 0)
                {
                    var strName = input;

                    if (!Methods.CheckConnectivity())
                    {
                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                        return;
                    }

                    if (TypeDialog == "SendMessageToPage")
                    {
                        if (!string.IsNullOrEmpty(strName) || !string.IsNullOrWhiteSpace(strName))
                        {
                            var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                            var time = unixTimestamp.ToString();

                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.PageChat.SendMessageToPageChatAsync(PageId, UserId, time, strName) });
                            ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_MessageSentSuccessfully), ToastLength.Short);
                        }
                        else
                        {
                            ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_something_went_wrong), ToastLength.Short);
                        }
                    }
                    else if (TypeDialog == "ReportThisPage")
                    {
                        PageData.IsReported = true;

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Page.ReportPageAsync(PageId, input) });
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_HasBeenReported), ToastLength.Short);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> Copy Link
        private void CopyLinkEvent()
        {
            try
            {
                Methods.CopyToClipboard(this, PageData.Url);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> Share
        private async void ShareEvent()
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
                            Title = PageData.PageTitle,
                            Text = PageData.About,
                            Url = PageData.Url
                        });
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> Settings
        private void SettingsPage_OnClick()
        {
            try
            {
                var intent = new Intent(this, typeof(SettingsPageActivity));
                intent.PutExtra("PageData", JsonConvert.SerializeObject(PageData));
                intent.PutExtra("PagesId", PageId);
                StartActivityForResult(intent, 2019);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async void BoostPageEvent()
        {
            try
            {
                var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                if (dataUser?.IsPro != "1" && ListUtils.SettingsSiteList?.Pro == "1" && AppSettings.ShowGoPro)
                {
                    var intent = new Intent(this, typeof(GoProActivity));
                    StartActivity(intent);
                    return;
                }

                if (Methods.CheckConnectivity())
                {
                    PageData.Boosted = "1";
                    //Sent Api 
                    var (apiStatus, respond) = await RequestsAsync.Page.BoostPageAsync(PageId);
                    switch (apiStatus)
                    {
                        case 200:
                            {
                                switch (respond)
                                {
                                    case MessageObject actionsObject when actionsObject.Message == "boosted":
                                        PageData.Boosted = "1";
                                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_PageSuccessfullyBoosted), ToastLength.Short);
                                        break;
                                    case MessageObject actionsObject:
                                        PageData.Boosted = "0";
                                        break;
                                }

                                break;
                            }
                        default:
                            Methods.DisplayReportResult(this, respond);
                            break;
                    }
                }
                else
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void ReviewsEvent()
        {
            try
            {
                var intent = new Intent(this, typeof(ReviewsPageActivity));
                intent.PutExtra("PageId", PageId);
                StartActivity(intent);
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

                if (scale >= 0)
                {
                    IconBack.SetColorFilter(Color.White);
                    BtnMore.SetColorFilter(Color.White);
                    TxtSearchForPost.Visibility = ViewStates.Invisible;
                }
                else
                {
                    IconBack.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
                    BtnMore.SetColorFilter(Color.ParseColor(AppSettings.MainColor));

                    if (AppSettings.ShowSearchForPosts)
                    {
                        TxtSearchForPost.BackgroundTintList = ColorStateList.ValueOf(WoWonderTools.IsTabDark() ? Color.ParseColor("#262626") : Color.ParseColor("#ecedf1"));
                        TxtSearchForPost.Visibility = ViewStates.Visible;
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