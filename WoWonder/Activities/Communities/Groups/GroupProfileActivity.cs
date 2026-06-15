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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WoWonder.Activities.AddPost;
using WoWonder.Activities.Base;
using WoWonder.Activities.Communities.Adapters;
using WoWonder.Activities.Communities.Groups.Settings;
using WoWonder.Activities.Live.Utils;
using WoWonder.Activities.MyProfile;
using WoWonder.Activities.NativePost.Extra;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Activities.SearchForPosts;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.Share;
using WoWonder.Library.Anjo.Share.Abstractions;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Group;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Classes.Product;
using WoWonderClient.Requests;
using static WoWonder.Activities.NativePost.Extra.WRecyclerView;
using Exception = System.Exception;
using File = Java.IO.File;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.Communities.Groups
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", WindowSoftInputMode = SoftInput.AdjustNothing | SoftInput.AdjustPan, ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class GroupProfileActivity : BaseActivity, IDialogListCallBack, AppBarLayout.IOnOffsetChangedListener, IDialogInputCallBack
    {
        #region Variables Basic

        private AppBarLayout AppBarLayout;
        private CollapsingToolbarLayout CollapsingToolbar;

        private SwipeRefreshLayout SwipeRefreshLayout;
        private ImageButton BtnMore;
        public ImageView UserProfileImage;
        private ImageView CoverImage, IconBack;
        private TextView TxtSearchForPost, TxtGroupName, TxtGroupUsername;
        private FloatingActionButton FloatingActionButtonView;
        private RelativeLayout EditAvatarImageGroup, EditCoverImageLayout;
        public WRecyclerView MainRecyclerView;
        public NativePostAdapter PostFeedAdapter;
        private string GroupId;
        public string ImageType;
        public static GroupDataObject GroupDataClass;
        private ImageView JoinRequestImage1, JoinRequestImage2, JoinRequestImage3;
        private RelativeLayout LayoutJoinRequest;
        private FeedCombiner Combiner;
        private static GroupProfileActivity Instance;
        private AdView MAdView;
        private CardView JoinCardView;
        private TextView TxtJoin;

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
                SetContentView(Resource.Layout.GroupProfileLayout);

                Instance = this;

                GroupId = Intent?.GetStringExtra("GroupId") ?? string.Empty;

                //Get Value And Set Toolbar
                InitComponent();
                SetRecyclerViewAdapters();


                GetDataGroup();
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

                UserProfileImage = (ImageView)FindViewById(Resource.Id.image_profile);
                CoverImage = (ImageView)FindViewById(Resource.Id.iv1);

                IconBack = (ImageView)FindViewById(Resource.Id.image_back);
                EditAvatarImageGroup = (RelativeLayout)FindViewById(Resource.Id.LinearEdit);
                EditCoverImageLayout = (RelativeLayout)FindViewById(Resource.Id.cover_layout);
                TxtGroupName = (TextView)FindViewById(Resource.Id.Group_name);
                TxtGroupUsername = (TextView)FindViewById(Resource.Id.Group_Username);
                BtnMore = (ImageButton)FindViewById(Resource.Id.morebutton);

                FloatingActionButtonView = FindViewById<FloatingActionButton>(Resource.Id.floatingActionButtonView);
                FloatingActionButtonView.Visibility = ViewStates.Gone;

                JoinRequestImage1 = (ImageView)FindViewById(Resource.Id.image_page_1);
                JoinRequestImage2 = (ImageView)FindViewById(Resource.Id.image_page_2);
                JoinRequestImage3 = (ImageView)FindViewById(Resource.Id.image_page_3);

                LayoutJoinRequest = (RelativeLayout)FindViewById(Resource.Id.layout_join_Request);

                MAdView = FindViewById<AdView>(Resource.Id.adView);
                AdsGoogle.InitAdView(MAdView, MainRecyclerView);

                // Join Button with cardview
                JoinCardView = FindViewById<CardView>(Resource.Id.joinButton);
                TxtJoin = FindViewById<TextView>(Resource.Id.joinTxt);
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
                PostFeedAdapter = new NativePostAdapter(this, GroupId, MainRecyclerView, NativeFeedType.Group);
                MainRecyclerView.SetXAdapter(PostFeedAdapter, SwipeRefreshLayout);
                Combiner = new FeedCombiner(null, PostFeedAdapter?.ListDiffer, this, NativeFeedType.Group);

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
                if (addEvent)
                {
                    // true +=  // false -=
                    SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                    IconBack.Click += IconBackOnClick;
                    EditCoverImageLayout.Click += TxtEditGroupInfoOnClick;
                    EditAvatarImageGroup.Click += UserProfileImageOnClick;
                    JoinCardView.Click += BtnJoinOnClick;
                    BtnMore.Click += BtnMoreOnClick;
                    FloatingActionButtonView.Click += AddPostOnClick;
                    LayoutJoinRequest.Click += LayoutJoinRequestOnClick;
                    UserProfileImage.Click += UserProfileImageOnClick;
                    CoverImage.Click += CoverImageOnClick;
                    TxtSearchForPost.Click += TxtSearchForPostOnClick;
                }
                else
                {
                    SwipeRefreshLayout.Refresh -= SwipeRefreshLayoutOnRefresh;
                    IconBack.Click -= IconBackOnClick;
                    EditCoverImageLayout.Click -= TxtEditGroupInfoOnClick;
                    EditAvatarImageGroup.Click -= UserProfileImageOnClick;
                    JoinCardView.Click -= BtnJoinOnClick;
                    BtnMore.Click -= BtnMoreOnClick;
                    FloatingActionButtonView.Click -= AddPostOnClick;
                    LayoutJoinRequest.Click -= LayoutJoinRequestOnClick;
                    UserProfileImage.Click -= UserProfileImageOnClick;
                    CoverImage.Click -= CoverImageOnClick;
                    TxtSearchForPost.Click -= TxtSearchForPostOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static GroupProfileActivity GetInstance()
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
                JoinCardView = null!;
                TxtJoin = null!;
                BtnMore = null!;
                UserProfileImage = null!;
                CoverImage = null!;
                IconBack = null!;
                TxtGroupName = null!;
                TxtGroupUsername = null!;
                EditCoverImageLayout = null!;
                FloatingActionButtonView = null!;
                EditAvatarImageGroup = null!;
                MainRecyclerView = null!;
                PostFeedAdapter = null!;
                GroupId = null!;
                GroupDataClass = null!;
                JoinRequestImage1 = null!;
                JoinRequestImage2 = null!;
                JoinRequestImage3 = null!;
                LayoutJoinRequest = null!;
                Combiner = null!;
                MAdView = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void TxtSearchForPostOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(SearchForPostsActivity));
                intent.PutExtra("TypeSearch", "group");
                intent.PutExtra("IdSearch", GroupId);
                StartActivity(intent);
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
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    return;
                }

                PostFeedAdapter?.ListDiffer?.Clear();
                PostFeedAdapter?.NotifyDataSetChanged();

                GetDataGroup();
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
                var media = WoWonderTools.GetFile("", Methods.Path.FolderDiskImage, GroupDataClass.Cover.Split('/').Last(), GroupDataClass.Cover);
                if (media.Contains("http"))
                {
                    Intent intent = new Intent(Intent.ActionView, Uri.Parse(media));
                    StartActivity(intent);
                }
                else
                {
                    File file2 = new File(media);
                    var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);

                    Intent intent = new Intent(Intent.ActionPick);
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
                bundle.PutString("Page", "GroupProfile");
                bundle.PutString("GroupData", JsonConvert.SerializeObject(GroupDataClass));

                dialogFragment.Arguments = bundle;

                dialogFragment.Show(SupportFragmentManager, dialogFragment.Tag);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Show More : Copy Link , Share , Edit (If user isOwner_Groups)
        private void BtnMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialAlertDialogBuilder(this);

                arrayAdapter.Add(GetString(Resource.String.Lbl_CopeLink));
                arrayAdapter.Add(GetString(Resource.String.Lbl_Share));
                if (GroupDataClass.IsOwner != null && GroupDataClass.IsOwner.Value)
                {
                    arrayAdapter.Add(GetString(Resource.String.Lbl_Settings));
                }

                if (GroupDataClass.IsReported != null && GroupDataClass.IsReported.Value)
                    arrayAdapter.Add(GetText(Resource.String.Lbl_CancelReport));
                else
                    arrayAdapter.Add(GetText(Resource.String.Lbl_ReportThisGroup));

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

        //Event Add New post
        private void AddPostOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(AddPostActivity));
                intent.PutExtra("Type", "SocialGroup");
                intent.PutExtra("PostId", GroupId);
                intent.PutExtra("itemObject", JsonConvert.SerializeObject(GroupDataClass));
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Join_Group => Joined , Join Group
        private async void BtnJoinOnClick(object sender, EventArgs e)
        {
            try
            {
                if (JoinCardView?.Tag?.ToString() == "MyGroup")
                {
                    SettingGroup_OnClick();
                }
                else
                {
                    if (!Methods.CheckConnectivity())
                    {
                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    }
                    else
                    {
                        var (apiStatus, respond) = await RequestsAsync.Group.JoinGroupAsync(GroupId);
                        if (apiStatus == 200)
                        {
                            if (respond is JoinGroupObject result1)
                            {
                                //Set style Btn Joined Group 
                                if (result1.JoinStatus == "joined") //joined
                                {
                                    JoinCardView.BackgroundTintList = WoWonderTools.IsTabDark() ? ColorStateList.ValueOf(Color.ParseColor("#282828")) : ColorStateList.ValueOf(Color.ParseColor("#FFFEFE"));
                                    TxtJoin.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                                    TxtJoin.Text = GetText(Resource.String.Btn_Joined);
                                    TxtJoin.Tag = "1";
                                }
                                else if (result1.JoinStatus == "requested") //requested
                                {
                                    JoinCardView.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));
                                    TxtJoin.SetTextColor(Color.White);
                                    TxtJoin.Text = GetText(Resource.String.Lbl_Requested);
                                    JoinCardView.Tag = "2";
                                }
                                else //not joined
                                {
                                    JoinCardView.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));
                                    TxtJoin.SetTextColor(Color.White);
                                    TxtJoin.Text = GetText(Resource.String.Btn_Join_Group);
                                    JoinCardView.Tag = "0";
                                }
                            }
                        }
                        else
                            Methods.DisplayReportResult(this, respond);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Update Image Cover Group
        private void TxtEditGroupInfoOnClick(object sender, EventArgs e)
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

        //Back
        private void IconBackOnClick(object sender, EventArgs e)
        {
            Finish();
        }

        //Join Request
        private void LayoutJoinRequestOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(JoinRequestActivity));
                intent.PutExtra("GroupId", GroupId);
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
                    List<AdapterModelsClass> dataGlobal = diff.Where(a => a.PostData?.Id == postId).ToList();
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

                                    PostFeedAdapter?.NotifyItemChanged(PostFeedAdapter.ListDiffer.IndexOf(postData));
                                }
                            }
                    }
                }
                else if (requestCode == 2005 && resultCode == Result.Ok)
                {
                    string result = data.GetStringExtra("groupItem") ?? "";
                    var item = JsonConvert.DeserializeObject<GroupDataObject>(result);
                    if (item != null)
                        LoadPassedData(item);
                }
                else if (requestCode == 2019 && resultCode == Result.Ok)
                {
                    var instance = GroupsActivity.GetInstance();
                    var manged = instance?.MAdapter?.SocialList?.FirstOrDefault(a => a.Group?.GroupId == GroupId && a.TypeView == SocialModelType.MangedGroups);
                    if (manged?.Group != null)
                    {
                        instance.MAdapter.SocialList.Remove(manged);
                        instance.MAdapter.NotifyDataSetChanged();

                        ListUtils.MyGroupList.Remove(manged.Group);
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
                            string pathImg;
                            if (ImageType == "Cover")
                            {
                                pathImg = filepath;

                                //Set image
                                if (AppSettings.CoverImageStyle == CoverImageStyle.CenterCrop)
                                    Glide.With(this).Load(filepath).Apply(new RequestOptions().CenterCrop().Error(Resource.Drawable.Cover_image)).Into(CoverImage);
                                else if (AppSettings.CoverImageStyle == CoverImageStyle.FitCenter)
                                    Glide.With(this).Load(filepath).Apply(new RequestOptions().FitCenter().Error(Resource.Drawable.Cover_image)).Into(CoverImage);
                                else
                                    Glide.With(this).Load(filepath).Apply(new RequestOptions().Error(Resource.Drawable.Cover_image)).Into(CoverImage);

                                UpdateImageGroup_Api(ImageType, pathImg);
                            }
                            else if (ImageType == "Avatar")
                            {
                                pathImg = filepath;

                                //Set image

                                var dataGroup = GroupsActivity.GetInstance()?.MAdapter.SocialList?.FirstOrDefault(a => a.Group?.GroupId == GroupId && a.TypeView == SocialModelType.MangedGroups);
                                if (dataGroup?.Group != null)
                                {
                                    dataGroup.Group.Avatar = pathImg;
                                    GroupsActivity.GetInstance()?.MAdapter?.NotifyDataSetChanged();

                                    var dataGroup2 = ListUtils.MyGroupList.FirstOrDefault(a => a.GroupId == GroupId);
                                    if (dataGroup2 != null)
                                    {
                                        dataGroup2.Avatar = pathImg;
                                    }
                                }

                                UpdateImageGroup_Api(ImageType, pathImg);
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
                if (requestCode == 108 && grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    PixImagePickerUtils.OpenDialogGallery(this);
                else if (requestCode == 108)
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                else if (requestCode == 111 && grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    new LiveUtil(this).OpenDialogLive();
                else if (requestCode == 111) ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
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
                string text = itemString;
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
                    SettingGroup_OnClick();
                }
                else if (text == GetText(Resource.String.Lbl_ReportThisGroup))
                {
                    OnReport_Button_Click();
                }
                else if (text == GetText(Resource.String.Lbl_CancelReport))
                {
                    if (!Methods.CheckConnectivity())
                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    else
                    {
                        GroupDataClass.IsReported = false;

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Group.ReportGroupAsync(GroupId, "") });
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
                if (input.Length <= 0) return;

                GroupDataClass.IsReported = true;

                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Group.ReportGroupAsync(GroupId, input) });
                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_HasBeenReported), ToastLength.Short);
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
                Methods.CopyToClipboard(this, GroupDataClass.Url);
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
                if (!CrossShare.IsSupported)
                    //Share Plugin same as video
                    return;

                await CrossShare.Current.Share(new ShareMessage
                {
                    Title = GroupDataClass.GroupName,
                    Text = GroupDataClass.About,
                    Url = GroupDataClass.Url
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> Setting
        private void SettingGroup_OnClick()
        {
            try
            {
                var intent = new Intent(this, typeof(SettingsGroupActivity));
                intent.PutExtra("itemObject", JsonConvert.SerializeObject(GroupDataClass));
                intent.PutExtra("GroupId", GroupId);
                StartActivityForResult(intent, 2019);
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
                dialog.SetTitle(GetString(Resource.String.Lbl_ReportThisGroup));

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


        #endregion

        #region Get Data Group

        private void GetDataGroup()
        {
            try
            {
                GroupDataClass = JsonConvert.DeserializeObject<GroupDataObject>(Intent?.GetStringExtra("GroupObject") ?? "");
                if (GroupDataClass != null)
                {
                    LoadPassedData(GroupDataClass);
                }

                PostFeedAdapter?.SetLoading();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }

            StartApiService();
        }

        private void LoadPassedData(GroupDataObject result)
        {
            try
            {
                GlideImageLoader.LoadImage(this, result.Avatar, UserProfileImage, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);

                switch (AppSettings.CoverImageStyle)
                {
                    case CoverImageStyle.CenterCrop:
                        Glide.With(this).Load(result.Cover.Replace(" ", "")).Apply(new RequestOptions().CenterCrop().Error(Resource.Drawable.Cover_image)).Into(CoverImage);
                        break;
                    case CoverImageStyle.FitCenter:
                        Glide.With(this).Load(result.Cover.Replace(" ", "")).Apply(new RequestOptions().FitCenter().Error(Resource.Drawable.Cover_image)).Into(CoverImage);
                        break;
                    default:
                        Glide.With(this).Load(result.Cover.Replace(" ", "")).Apply(new RequestOptions().Error(Resource.Drawable.Cover_image)).Into(CoverImage);
                        break;
                }

                TxtGroupUsername.Text = "@" + Methods.FunString.DecodeString(result.Username);
                TxtGroupName.Text = Methods.FunString.DecodeString(result.Name);

                TxtGroupName.SetTextColor(Color.ParseColor(WoWonderTools.IsTabDark() ? "#ffffff" : "#000000"));
                TxtGroupUsername.SetTextColor(Color.ParseColor(WoWonderTools.IsTabDark() ? "#ffffff" : "#C3C7D0"));

                if (result.UserId == UserDetails.UserId)
                    result.IsOwner = true;

                if (result.IsOwner != null && result.IsOwner.Value)
                {
                    JoinCardView.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));
                    TxtJoin.Text = GetText(Resource.String.Lbl_Edit);
                    TxtJoin.SetTextColor(Color.White);
                    JoinCardView.Tag = "MyGroup";
                    //BtnMore.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));
                    //BtnMore.ImageTintList = ColorStateList.ValueOf(Color.White); 
                }
                else
                {
                    //Set style Btn Joined Group 
                    if (WoWonderTools.IsJoinedGroup(result) == "1") //joined
                    {
                        JoinCardView.BackgroundTintList = WoWonderTools.IsTabDark() ? ColorStateList.ValueOf(Color.ParseColor("#282828")) : ColorStateList.ValueOf(Color.ParseColor("#FFFEFE"));
                        TxtJoin.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                        TxtJoin.Text = GetText(Resource.String.Btn_Joined);
                        TxtJoin.Tag = "1";
                    }
                    else if (WoWonderTools.IsJoinedGroup(result) == "2") //requested
                    {
                        JoinCardView.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));
                        TxtJoin.SetTextColor(Color.White);
                        TxtJoin.Text = GetText(Resource.String.Lbl_Requested);
                        JoinCardView.Tag = "2";
                    }
                    else //not joined
                    {
                        JoinCardView.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));
                        TxtJoin.SetTextColor(Color.White);
                        TxtJoin.Text = GetText(Resource.String.Btn_Join_Group);
                        JoinCardView.Tag = "0";
                    }

                    //BtnMore.BackgroundTintList = WoWonderTools.IsJoinedGroup(result) ? ColorStateList.ValueOf(Color.ParseColor("#efefef")) : ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));
                    //BtnMore.ImageTintList = WoWonderTools.IsJoinedGroup(result) ? ColorStateList.ValueOf(Color.Black) : ColorStateList.ValueOf(Color.White);
                }

                var modelsClass = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.InfoGroupBox);
                if (modelsClass == null)
                {
                    Combiner.InfoGroupBox(new GroupPrivacyModelClass { GroupClass = result, GroupId = result.GroupId }, 0);
                }
                else
                {
                    modelsClass.PrivacyModelClass = new GroupPrivacyModelClass
                    {
                        GroupClass = result,
                        GroupId = result.GroupId
                    };
                    PostFeedAdapter?.NotifyItemChanged(PostFeedAdapter.ListDiffer.IndexOf(modelsClass));
                }

                if (!string.IsNullOrEmpty(result.About))
                {
                    var checkAboutBox = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.AboutBox);
                    if (checkAboutBox == null)
                    {
                        Combiner.AboutBoxPostView(Methods.FunString.DecodeString(result.About), 0);
                    }
                    else
                    {
                        checkAboutBox.AboutModel.Description = Methods.FunString.DecodeString(result.About);
                        PostFeedAdapter?.NotifyItemChanged(PostFeedAdapter.ListDiffer.IndexOf(checkAboutBox));
                    }
                }

                if (result.IsOwner != null && result.IsOwner.Value || WoWonderTools.IsJoinedGroup(result) == "1")
                {
                    var checkSection = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.AddPostBox);
                    if (checkSection == null)
                    {
                        Combiner.AddPostBoxPostView("Group", -1, new PostDataObject { GroupRecipient = result });

                        //switch (AppSettings.ShowSearchForPosts)
                        //{
                        //    case true:
                        //        Combiner.SearchForPostsView("Group", new PostDataObject { GroupRecipient = result });
                        //        break;
                        //}
                        PostFeedAdapter?.NotifyItemInserted(PostFeedAdapter.ListDiffer.Count - 1);
                    }

                    FloatingActionButtonView.Visibility = ViewStates.Visible;
                }

                if (result.IsOwner != null && result.IsOwner.Value)
                {
                    EditAvatarImageGroup.Visibility = ViewStates.Visible;
                    //TxtEditGroupInfo.Visibility = ViewStates.Visible;

                    EditCoverImageLayout.Visibility = ViewStates.Visible;
                }
                else
                {
                    EditAvatarImageGroup.Visibility = ViewStates.Gone;
                    //TxtEditGroupInfo.Visibility = ViewStates.Gone;

                    EditCoverImageLayout.Visibility = ViewStates.Gone;
                }

                if (WoWonderTools.IsJoinedGroup(result) == "1" || result.Privacy == "1" || result.IsOwner != null && result.IsOwner.Value)
                {
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => MainRecyclerView.ApiPostAsync.FetchNewsFeedApiPosts() });
                }
                else
                {
                    PostFeedAdapter?.SetLoaded();

                    var viewProgress = PostFeedAdapter?.ListDiffer?.FirstOrDefault(anjo => anjo.TypeView == PostModelType.ViewProgress);
                    if (viewProgress != null)
                        MainRecyclerView.RemoveByRowIndex(viewProgress);

                    var emptyStateCheck = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.PostData != null && a.TypeView != PostModelType.AddPostBox && a.TypeView != PostModelType.InfoGroupBox);
                    if (emptyStateCheck != null)
                    {
                        var emptyStateChecker = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.EmptyState);
                        if (emptyStateChecker != null && PostFeedAdapter?.ListDiffer?.Count > 1)
                            PostFeedAdapter?.ListDiffer?.Remove(emptyStateChecker);
                    }
                    else
                    {
                        var emptyStateChecker = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.EmptyState);
                        if (emptyStateChecker == null)
                            PostFeedAdapter?.ListDiffer?.Add(new AdapterModelsClass { TypeView = PostModelType.EmptyState, Id = 744747447 });
                    }
                    PostFeedAdapter?.NotifyDataSetChanged();
                }

                WoWonderTools.GetFile("", Methods.Path.FolderDiskImage, result.Cover.Split('/').Last(), result.Cover);
                WoWonderTools.GetFile("", Methods.Path.FolderDiskImage, result.Avatar.Split('/').Last(), result.Avatar);
                SwipeRefreshLayout.Refreshing = false;
            }
            catch (Exception e)
            {
                SwipeRefreshLayout.Refreshing = false;
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { GetGroupDataApi, GetJoin });
        }

        private async Task GetGroupDataApi()
        {
            var (apiStatus, respond) = await RequestsAsync.Group.GetGroupDataAsync(GroupId);

            if (apiStatus != 200 || respond is not GetGroupDataObject result || result.GroupData == null)
                Methods.DisplayReportResult(this, respond);
            else
            {
                GroupDataClass = result.GroupData;
                RunOnUiThread(() => { LoadPassedData(GroupDataClass); });
            }
        }

        #endregion

        #region Update Image Avatar && Cover

        // Function Update Image Group : Avatar && Cover
        private async void UpdateImageGroup_Api(string type, string path)
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
                        var (apiStatus, respond) = await RequestsAsync.Group.UpdateGroupAvatarAsync(GroupId, path).ConfigureAwait(false);
                        if (apiStatus == 200)
                        {
                            if (respond is MessageObject result)
                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Image_changed_successfully), ToastLength.Short);
                        }
                        //GlideImageLoader.LoadImage(this, file.Path, UserProfileImage, ImageStyle.RoundedCrop, ImagePlaceholders.Color);
                        else
                            Methods.DisplayReportResult(this, respond);
                    }
                    else if (type == "Cover")
                    {
                        var (apiStatus, respond) = await RequestsAsync.Group.UpdateGroupCoverAsync(GroupId, path).ConfigureAwait(false);
                        if (apiStatus == 200)
                        {
                            if (respond is not MessageObject result)
                                return;

                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Image_changed_successfully), ToastLength.Short);
                            //GlideImageLoader.LoadImage(this, file.Path, CoverImage, ImageStyle.CenterCrop, ImagePlaceholders.Color);
                        }
                        else
                        {
                            Methods.DisplayReportResult(this, respond);
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

        private async Task GetJoin()
        {
            if (GroupDataClass.UserId == UserDetails.UserId)
            {
                var (apiStatus, respond) = await RequestsAsync.Group.GetGroupJoinRequestsAsync(GroupId, "5");
                if (apiStatus == 200)
                {
                    if (respond is GetGroupJoinRequestsObject result)
                        RunOnUiThread(() =>
                        {
                            var respondList = result.Data.Count;
                            if (respondList > 0)
                            {
                                LayoutJoinRequest.Visibility = ViewStates.Visible;
                                try
                                {
                                    var list = result.Data.TakeLast(4).ToList();

                                    for (var i = 0; i < list.Count; i++)
                                    {
                                        var item = list[i];
                                        if (item == null)
                                            continue;

                                        if (i == 0)
                                        {
                                            GlideImageLoader.LoadImage(this, item?.UserData?.Avatar, JoinRequestImage1, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);
                                        }
                                        else if (i == 1)
                                        {
                                            GlideImageLoader.LoadImage(this, item?.UserData?.Avatar, JoinRequestImage2, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);
                                        }
                                        else if (i == 2)
                                        {
                                            GlideImageLoader.LoadImage(this, item?.UserData?.Avatar, JoinRequestImage3, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            }
                            else
                            {
                                LayoutJoinRequest.Visibility = ViewStates.Gone;
                            }
                        });
                }
                else
                    Methods.DisplayReportResult(this, respond);
            }
            else
            {
                RunOnUiThread(() => { LayoutJoinRequest.Visibility = ViewStates.Gone; });
            }
        }


    }
}