using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Locations;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.Content;
using AndroidX.ViewPager2.Widget;
using Bumptech.Glide;
using Com.Google.Android.Play.Core.Install.Model;
using Google.Android.Material.Dialog;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.Tabs;
using Newtonsoft.Json;
using Plugin.Geolocator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using WoWonder.Activities.AddPost;
using WoWonder.Activities.Base;
using WoWonder.Activities.Live.Page;
using WoWonder.Activities.Live.Utils;
using WoWonder.Activities.MostLikedPosts;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Activities.PopularPosts;
using WoWonder.Activities.ReelsVideo;
using WoWonder.Activities.Search;
using WoWonder.Activities.SettingsPreferences;
using WoWonder.Activities.Story;
using WoWonder.Activities.Story.Adapters;
using WoWonder.Activities.Tabbes.Fragment;
using WoWonder.Activities.Videos;
using WoWonder.Adapters;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.MediaPlayerController;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.OneSignalNotif;
using WoWonder.Library.OneSignalNotif.Models;
using WoWonder.Services;
using WoWonder.SQLite;
using WoWonderClient;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Product;
using WoWonderClient.Classes.Story;
using WoWonderClient.Requests;
using Console = System.Console;
using Exception = System.Exception;
using File = Java.IO.File;
using Task = System.Threading.Tasks.Task;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.Tabbes
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", WindowSoftInputMode = SoftInput.AdjustNothing | SoftInput.AdjustPan, ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class TabbedMainActivity : BaseActivity
    {
        #region Variables Basic

        private static TabbedMainActivity Instance;
        public ViewPager2 ViewPager;
        private MainTabAdapter TabAdapter;
        public NewsFeedNative NewsFeedTab;
        public NotificationsFragment NotificationsTab;
        public TrendingFragment TrendingTab;
        public MoreFragment MoreTab;
        private LinearLayout DefaultNavigationTabBar;
        private BottomNavigationTab BottomNavigationTab;

        public FloatingActionButton FloatingActionButton;
        private static string CountNotificationsStatic = "0", CountMessagesStatic = "0";
        private static bool RecentlyBackPressed;
        private readonly Handler ExitHandler = new Handler(Looper.MainLooper);

        private PowerManager.WakeLock Wl;

        private ImageView RlSearch, RlMessage, RlReels, RlAdd;

        public TabLayout Tabs;

        public SoundController SoundController;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                Delegate.SetLocalNightMode(WoWonderTools.IsTabDark() ? AppCompatDelegate.ModeNightYes : AppCompatDelegate.ModeNightNo);
                SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                //AddFlagsWakeLock();

                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.TabbedMainLayout);

                Instance = this;

                Task.Run(() => { MainApplication.GetInstance().SecondRunExcite(this); });

                GetGeneralAppData();

                //Get Value And Set Toolbar
                InitComponent();
                AddFragmentsTabs();
                InitBackPressed("TabbedMainActivity");

                SoundController = new SoundController(this);
                SoundController.InitializeUi();
                SetService();


                if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
                {
                    if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.PostNotifications) == Permission.Granted)
                    {
                        if (string.IsNullOrEmpty(UserDetails.DeviceId))
                            OneSignalNotification.Instance.RegisterNotificationDevice(this);
                    }
                    else
                    {
                        RequestPermissions(new[]
                        {
                            Manifest.Permission.PostNotifications
                        }, 16248);
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(UserDetails.DeviceId))
                        OneSignalNotification.Instance.RegisterNotificationDevice(this);
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
                NewsFeedTab?.MainRecyclerView?.StopVideo();
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
                NewsFeedTab?.MainRecyclerView?.StopVideo();
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
                Glide.Get(this).TrimMemory((int)level);
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
                GC.Collect();
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
                NewsFeedTab?.MainRecyclerView?.ReleasePlayer();

                if ((!Constant.IsLoggingOut || !Constant.IsChangingTheme || !Constant.IsOpenNotify) && Constant.IsPlayed)
                {
                    Intent intent = new Intent(this, typeof(PlayerService));
                    intent.SetAction(PlayerService.ActionStop);

                    ContextCompat.StartForegroundService(this, intent);

                    StopService(intent);
                }

                Constant.IsLoggingOut = false;
                Constant.IsChangingTheme = false;

                OffWakeLock();
                base.OnDestroy();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            try
            {
                base.OnConfigurationChanged(newConfig);

                var currentNightMode = newConfig.UiMode & UiMode.NightMask;
                switch (currentNightMode)
                {
                    case UiMode.NightNo:
                        // Night mode is not active, we're using the light theme
                        MainSettings.ApplyTheme(MainSettings.LightMode);
                        break;
                    case UiMode.NightYes:
                        // Night mode is active, we're using dark theme
                        MainSettings.ApplyTheme(MainSettings.DarkMode);
                        break;
                }

                Delegate.SetLocalNightMode(WoWonderTools.IsTabDark() ? AppCompatDelegate.ModeNightYes : AppCompatDelegate.ModeNightNo);
                SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                DefaultNavigationTabBar = FindViewById<LinearLayout>(Resource.Id.bottomnavigationtab);

                BottomNavigationTab = new BottomNavigationTab(this);

                ViewPager = FindViewById<ViewPager2>(Resource.Id.vp_horizontal_ntb);
                TabAdapter = new MainTabAdapter(this);

                RlSearch = FindViewById<ImageView>(Resource.Id.rlSearch);
                RlMessage = FindViewById<ImageView>(Resource.Id.rl_message);
                RlReels = FindViewById<ImageView>(Resource.Id.rlReels);
                RlAdd = FindViewById<ImageView>(Resource.Id.rlAdd);

                FloatingActionButton = FindViewById<FloatingActionButton>(Resource.Id.floatingActionButtonView);
                if (WoWonderTools.CanAddPost())
                    FloatingActionButton.Visibility = AppSettings.ShowAddPostOnNewsFeed ? ViewStates.Visible : ViewStates.Gone;
                else
                    FloatingActionButton.Visibility = ViewStates.Gone;

                RlAdd.Visibility = AppSettings.ShowBottomAddOnTab ? ViewStates.Visible : ViewStates.Gone;
                RlReels.Visibility = ViewStates.Gone;

                RlMessage.Visibility = AppSettings.MessengerIntegration ? ViewStates.Visible : ViewStates.Gone;
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
                        FloatingActionButton.Click += Btn_AddPost_OnClick;
                        RlSearch.Click += RlSearchOnClick;
                        RlMessage.Click += RlMessageOnClick;
                        RlAdd.Click += BtnAddOnClick;
                        RlReels.Click += RlReelsOnClick;
                        break;
                    default:
                        FloatingActionButton.Click -= Btn_AddPost_OnClick;
                        RlSearch.Click -= RlSearchOnClick;
                        RlMessage.Click -= RlMessageOnClick;
                        RlAdd.Click -= BtnAddOnClick;
                        RlReels.Click -= RlReelsOnClick;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static TabbedMainActivity GetInstance()
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

        #endregion

        #region Events

        //Add 
        private void BtnAddOnClick(object sender, EventArgs e)
        {
            try
            {
                OptionAddDialogFragment dialogFragment = new OptionAddDialogFragment();
                dialogFragment.Show(SupportFragmentManager, dialogFragment.Tag);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void RlReelsOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(ReelsVideoDetailsActivity));
                intent.PutExtra("Type", "VideoReels");
                intent.PutExtra("VideosCount", ListUtils.VideoReelsList.Count);
                //intent.PutExtra("DataItem", JsonConvert.SerializeObject(ListUtils.VideoReelsList.ToList()));
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void RlSearchOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(SearchTabbedActivity));
                intent.PutExtra("Key", "");
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void RlMessageOnClick(object sender, EventArgs e)
        {
            try
            {
                // Chat removido
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Open page add post
        private void Btn_AddPost_OnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                var intent = new Intent(this, typeof(AddPostActivity));
                intent.PutExtra("Type", "Normal");
                intent.PutExtra("PostId", UserDetails.UserId);
                //intent.PutExtra("itemObject", JsonConvert.SerializeObject(PageData));
                StartActivity(intent);
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

                switch (requestCode)
                {
                    // Add image story
                    case PixImagePickerActivity.RequestCode when resultCode == Result.Ok:
                        {
                            var listPath = JsonConvert.DeserializeObject<ResultIntentPixImage>(data.GetStringExtra("ResultPixImage") ?? "");
                            if (listPath?.List?.Count > 0)
                            {
                                var filepath = listPath.List.FirstOrDefault();
                                if (!string.IsNullOrEmpty(filepath))
                                    PickiTonCompleteListener(filepath);
                            }
                            break;
                        }
                    case 500 when resultCode == Result.Ok:
                        {
                            Uri uri = data.Data;
                            var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, uri);
                            PickiTonCompleteListener(filepath);
                            break;
                        }
                    // Add video story
                    case 501 when resultCode == Result.Ok:
                        {
                            var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                            if (filepath != null)
                            {
                                var type = Methods.AttachmentFiles.Check_FileExtension(filepath);
                                switch (type)
                                {
                                    case "Video":
                                        {
                                            var fileName = filepath.Split('/').Last();
                                            var fileNameWithoutExtension = fileName.Split('.').First();
                                            var pathWithoutFilename = Methods.Path.FolderDcimImage;
                                            var fullPathFile = new File(Methods.Path.FolderDcimImage, fileNameWithoutExtension + ".png");

                                            var videoPlaceHolderImage = Methods.MultiMedia.GetMediaFrom_Gallery(pathWithoutFilename, fileNameWithoutExtension + ".png");
                                            switch (videoPlaceHolderImage)
                                            {
                                                case "File Dont Exists":
                                                    {
                                                        var bitmapImage = Methods.MultiMedia.Retrieve_VideoFrame_AsBitmap(this, data.Data.ToString());
                                                        Methods.MultiMedia.Export_Bitmap_As_Image(bitmapImage, fileNameWithoutExtension, pathWithoutFilename);
                                                        break;
                                                    }
                                            }

                                            Intent intent = new Intent(this, typeof(VideoEditorActivity));
                                            intent.PutExtra("Uri", filepath);
                                            intent.PutExtra("Thumbnail", fullPathFile.Path);
                                            intent.PutExtra("Type", "Story");
                                            StartActivity(intent);
                                            break;
                                        }
                                }
                            }
                            else
                            {
                                Uri uri = data.Data;
                                var filepath2 = Methods.AttachmentFiles.GetActualPathFromFile(this, uri);
                                PickiTonCompleteListener(filepath2);
                            }

                            break;
                        }
                    // Add video camera story
                    case 513 when resultCode == Result.Ok:
                        {
                            if (IntentController.CurrentVideoPath != null)
                            {
                                var fileName = IntentController.CurrentVideoPath.Split('/').Last();
                                var fileNameWithoutExtension = fileName.Split('.').First();
                                var pathWithoutFilename = Methods.Path.FolderDcimImage;
                                var fullPathFile = new File(Methods.Path.FolderDcimImage, fileNameWithoutExtension + ".png");

                                var videoPlaceHolderImage = Methods.MultiMedia.GetMediaFrom_Gallery(pathWithoutFilename, fileNameWithoutExtension + ".png");
                                switch (videoPlaceHolderImage)
                                {
                                    case "File Dont Exists":
                                        {
                                            var bitmapImage = Methods.MultiMedia.Retrieve_VideoFrame_AsBitmap(this, IntentController.CurrentVideoPath);
                                            Methods.MultiMedia.Export_Bitmap_As_Image(bitmapImage, fileNameWithoutExtension, pathWithoutFilename);
                                            break;
                                        }
                                }

                                Intent intent = new Intent(this, typeof(VideoEditorActivity));
                                intent.PutExtra("Uri", IntentController.CurrentVideoPath);
                                intent.PutExtra("Thumbnail", fullPathFile.Path);
                                intent.PutExtra("Type", "Story");
                                StartActivity(intent);
                            }
                            else
                            {
                                var filepath2 = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                                PickiTonCompleteListener(filepath2);
                            }

                            break;
                        }
                    //Edit post
                    case 3950 when resultCode == Result.Ok:
                        {
                            var postId = data.GetStringExtra("PostId") ?? "";
                            var postText = data.GetStringExtra("PostText") ?? "";
                            var diff = NewsFeedTab.PostFeedAdapter?.ListDiffer;
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
                                                    NewsFeedTab.PostFeedAdapter?.NotifyItemChanged(index);
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
                                                            NewsFeedTab.PostFeedAdapter?.NotifyItemInserted(headerPostIndex + 1);
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
                            var item = JsonConvert.DeserializeObject<ProductDataObject>(data.GetStringExtra("itemData"));
                            if (item != null)
                            {
                                var diff = NewsFeedTab.PostFeedAdapter?.ListDiffer;
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

                                                            NewsFeedTab.PostFeedAdapter?.NotifyItemChanged(index);
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
                    case 4711:
                        switch (resultCode) // The switch block will be triggered only with flexible update since it returns the install result codes
                        {
                            case Result.Ok:
                                switch (UpdateManagerApp.AppUpdateTypeSupported)
                                {
                                    // In app update success
                                    case AppUpdateType.Immediate:
                                        ToastUtils.ShowToast(this, "App updated", ToastLength.Short);
                                        break;
                                }
                                break;
                            case Result.Canceled:
                                ToastUtils.ShowToast(this, "In app update cancelled", ToastLength.Short);
                                break;
                            case (Result)ActivityResult.ResultInAppUpdateFailed:
                                ToastUtils.ShowToast(this, "In app update failed", ToastLength.Short);
                                break;
                        }
                        break;
                    // => NiceArtEditor add story text
                    case 2200 when resultCode == Result.Ok:
                        RunOnUiThread(() =>
                        {
                            try
                            {
                                var imagePath = data.GetStringExtra("ImagePath") ?? "Data not available";
                                if (imagePath != "Data not available" && !string.IsNullOrEmpty(imagePath))
                                {
                                    //Do something with your Uri
                                    Intent intent = new Intent(this, typeof(AddStoryActivity));
                                    intent.PutExtra("Uri", imagePath);
                                    intent.PutExtra("Type", "image");
                                    StartActivity(intent);
                                }
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                        break;
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
                        PixImagePickerUtils.OpenDialogGallery(this, true);
                        break;
                    case 108:
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                        break;
                    case 105 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        CheckAndGetLocation();
                        break;
                    case 105:
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                        break;
                    case 111 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        new LiveUtil(this).OpenDialogLive();
                        break;
                    case 111:
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                        break;
                    case 16248 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        if (string.IsNullOrEmpty(UserDetails.DeviceId))
                            OneSignalNotification.Instance.RegisterNotificationDevice(this);
                        break;
                    case 16248:
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

        #region Tab New Feed

        private void TabsOnTabReselected(object sender, TabLayout.TabReselectedEventArgs e)
        {
            try
            {
                switch (e.Tab.Position)
                {
                    case 0:
                        ShowPopup(e.Tab.View);
                        break;
                    case 1:
                        {
                            var intent = new Intent(this, typeof(PopularPostsActivity));
                            StartActivity(intent);
                            break;
                        }
                    case 2:
                        {
                            var intent = new Intent(this, typeof(MostLikedPostsActivity));
                            StartActivity(intent);
                            break;
                        }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TabsOnTabSelected(object sender, TabLayout.TabSelectedEventArgs e)
        {
            try
            {
                switch (e.Tab.Position)
                {
                    case 0:
                        ShowPopup(e.Tab.View);
                        break;
                    case 1:
                        {
                            var intent = new Intent(this, typeof(PopularPostsActivity));
                            StartActivity(intent);
                            break;
                        }
                    case 2:
                        {
                            var intent = new Intent(this, typeof(MostLikedPostsActivity));
                            StartActivity(intent);
                            break;
                        }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private static int Type;
        private static string Filter = "0";
        private static string NameType = "News";
        private ImageView MenuCheckAll, MenuCheckPeopleIFollow, MenuCheckText, MenuCheckImage, MenuCheckVideo, MenuCheckFile, MenuCheckMusic, MenuCheckMap;
        private void ShowPopup(View v)
        {
            try
            {
                LayoutInflater layoutInflater = (LayoutInflater)GetSystemService(LayoutInflaterService);
                View popupView = layoutInflater.Inflate(Resource.Layout.PopupFilterPostLayout, null);

                int px = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 180, Resources.DisplayMetrics);
                var popupWindow = new PopupWindow(popupView, px, ViewGroup.LayoutParams.WrapContent);

                var menuAll = popupView.FindViewById<LinearLayout>(Resource.Id.menu_all);
                var menuPeopleIFollow = popupView.FindViewById<LinearLayout>(Resource.Id.menu_People_i_Follow);
                var menuText = popupView.FindViewById<LinearLayout>(Resource.Id.menu_text);
                var menuImage = popupView.FindViewById<LinearLayout>(Resource.Id.menu_image);
                var menuVideo = popupView.FindViewById<LinearLayout>(Resource.Id.menu_video);
                var menuFile = popupView.FindViewById<LinearLayout>(Resource.Id.menu_file);
                var menuMusic = popupView.FindViewById<LinearLayout>(Resource.Id.menu_music);
                var menuMap = popupView.FindViewById<LinearLayout>(Resource.Id.menu_map);

                MenuCheckAll = popupView.FindViewById<ImageView>(Resource.Id.menu_check_all);
                MenuCheckPeopleIFollow = popupView.FindViewById<ImageView>(Resource.Id.menu_check_People_i_Follow);
                MenuCheckText = popupView.FindViewById<ImageView>(Resource.Id.menu_check_text);
                MenuCheckImage = popupView.FindViewById<ImageView>(Resource.Id.menu_check_image);
                MenuCheckVideo = popupView.FindViewById<ImageView>(Resource.Id.menu_check_video);
                MenuCheckFile = popupView.FindViewById<ImageView>(Resource.Id.menu_check_file);
                MenuCheckMusic = popupView.FindViewById<ImageView>(Resource.Id.menu_check_music);
                MenuCheckMap = popupView.FindViewById<ImageView>(Resource.Id.menu_check_map);

                if (Filter == "0")
                {
                    MenuCheckAll.Visibility = ViewStates.Visible;
                    MenuCheckPeopleIFollow.Visibility = ViewStates.Invisible;

                    NameType = GetText(Resource.String.Lbl_News);
                }
                else if (Filter == "1")
                {
                    MenuCheckAll.Visibility = ViewStates.Invisible;
                    MenuCheckPeopleIFollow.Visibility = ViewStates.Visible;

                    NameType = GetText(Resource.String.Lbl_People_i_Follow);
                }
                else
                {
                    MenuCheckAll.Visibility = ViewStates.Visible;
                    MenuCheckPeopleIFollow.Visibility = ViewStates.Invisible;

                    NameType = GetText(Resource.String.Lbl_News);
                }

                CheckType(Type);

                //All Post for All Users 
                menuAll.Click += (sender, args) =>
                {
                    try
                    {
                        Type = 0;
                        Filter = "0";
                        NameType = GetText(Resource.String.Lbl_News);

                        MenuCheckAll.Visibility = ViewStates.Visible;
                        MenuCheckPeopleIFollow.Visibility = ViewStates.Invisible;

                        NewsFeedTab.MainRecyclerView.SetPostAndFilterType(Type, Filter);
                        CheckType(Type);
                        popupWindow.Dismiss();
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                };

                //All Post for People I Follow 
                menuPeopleIFollow.Click += (sender, args) =>
                {
                    try
                    {
                        Type = 0;
                        Filter = "1";
                        NameType = GetText(Resource.String.Lbl_People_i_Follow);

                        MenuCheckAll.Visibility = ViewStates.Invisible;
                        MenuCheckPeopleIFollow.Visibility = ViewStates.Visible;

                        NewsFeedTab.MainRecyclerView.SetPostAndFilterType(Type, Filter);
                        CheckType(Type);
                        popupWindow.Dismiss();
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                };

                menuText.Click += (sender, args) =>
                {
                    try
                    {
                        Type = 1;

                        CheckType(Type);

                        NewsFeedTab.MainRecyclerView.SetPostAndFilterType(Type, Filter);
                        popupWindow.Dismiss();
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                };

                menuImage.Click += (sender, args) =>
                {
                    try
                    {
                        Type = 2;

                        CheckType(Type);

                        NewsFeedTab.MainRecyclerView.SetPostAndFilterType(Type, Filter);
                        popupWindow.Dismiss();
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                };

                menuVideo.Click += (sender, args) =>
                {
                    try
                    {
                        Type = 3;

                        CheckType(Type);

                        NewsFeedTab.MainRecyclerView.SetPostAndFilterType(Type, Filter);
                        popupWindow.Dismiss();
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                };

                menuFile.Click += (sender, args) =>
                {
                    try
                    {
                        Type = 5;

                        MenuCheckFile.Visibility = ViewStates.Visible;

                        MenuCheckText.Visibility = ViewStates.Invisible;
                        MenuCheckImage.Visibility = ViewStates.Invisible;
                        MenuCheckVideo.Visibility = ViewStates.Invisible;
                        MenuCheckMusic.Visibility = ViewStates.Invisible;
                        MenuCheckMap.Visibility = ViewStates.Invisible;

                        NewsFeedTab.MainRecyclerView.SetPostAndFilterType(Type, Filter);
                        CheckType(Type);
                        popupWindow.Dismiss();
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                };

                menuMusic.Click += (sender, args) =>
                {
                    try
                    {
                        Type = 4;

                        CheckType(Type);

                        NewsFeedTab.MainRecyclerView.SetPostAndFilterType(Type, Filter);
                        popupWindow.Dismiss();
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                };

                menuMap.Click += (sender, args) =>
                {
                    try
                    {
                        Type = 6;

                        CheckType(Type);

                        NewsFeedTab.MainRecyclerView.SetPostAndFilterType(Type, Filter);
                        popupWindow.Dismiss();
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                };

                popupWindow.SetBackgroundDrawable(new ColorDrawable());
                popupWindow.Focusable = true;
                popupWindow.ClippingEnabled = true;
                popupWindow.OutsideTouchable = false;
                popupWindow.DismissEvent += delegate
                {
                    try
                    {
                        popupWindow.Dismiss();
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                };

                popupWindow.ShowAsDropDown(v);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void CheckType(int type)
        {
            try
            {
                switch (type)
                {
                    case 1:
                        MenuCheckText.Visibility = ViewStates.Visible;

                        MenuCheckImage.Visibility = ViewStates.Invisible;
                        MenuCheckVideo.Visibility = ViewStates.Invisible;
                        MenuCheckFile.Visibility = ViewStates.Invisible;
                        MenuCheckMusic.Visibility = ViewStates.Invisible;
                        MenuCheckMap.Visibility = ViewStates.Invisible;

                        NameType = GetText(Resource.String.text);
                        break;
                    case 2:
                        MenuCheckImage.Visibility = ViewStates.Visible;

                        MenuCheckText.Visibility = ViewStates.Invisible;
                        MenuCheckVideo.Visibility = ViewStates.Invisible;
                        MenuCheckFile.Visibility = ViewStates.Invisible;
                        MenuCheckMusic.Visibility = ViewStates.Invisible;
                        MenuCheckMap.Visibility = ViewStates.Invisible;

                        NameType = GetText(Resource.String.image);

                        break;
                    case 3:
                        MenuCheckVideo.Visibility = ViewStates.Visible;

                        MenuCheckText.Visibility = ViewStates.Invisible;
                        MenuCheckImage.Visibility = ViewStates.Invisible;
                        MenuCheckFile.Visibility = ViewStates.Invisible;
                        MenuCheckMusic.Visibility = ViewStates.Invisible;
                        MenuCheckMap.Visibility = ViewStates.Invisible;

                        NameType = GetText(Resource.String.video);

                        break;
                    case 4:
                        MenuCheckMusic.Visibility = ViewStates.Visible;

                        MenuCheckText.Visibility = ViewStates.Invisible;
                        MenuCheckImage.Visibility = ViewStates.Invisible;
                        MenuCheckVideo.Visibility = ViewStates.Invisible;
                        MenuCheckFile.Visibility = ViewStates.Invisible;
                        MenuCheckMap.Visibility = ViewStates.Invisible;

                        NameType = GetText(Resource.String.Lbl_Music);

                        break;
                    case 5:
                        MenuCheckFile.Visibility = ViewStates.Visible;

                        MenuCheckText.Visibility = ViewStates.Invisible;
                        MenuCheckImage.Visibility = ViewStates.Invisible;
                        MenuCheckVideo.Visibility = ViewStates.Invisible;
                        MenuCheckMusic.Visibility = ViewStates.Invisible;
                        MenuCheckMap.Visibility = ViewStates.Invisible;

                        NameType = GetText(Resource.String.Lbl_File);

                        break;
                    case 6:
                        MenuCheckMap.Visibility = ViewStates.Visible;

                        MenuCheckText.Visibility = ViewStates.Invisible;
                        MenuCheckImage.Visibility = ViewStates.Invisible;
                        MenuCheckVideo.Visibility = ViewStates.Invisible;
                        MenuCheckFile.Visibility = ViewStates.Invisible;
                        MenuCheckMusic.Visibility = ViewStates.Invisible;

                        NameType = GetText(Resource.String.Lbl_Map);

                        break;
                }

                Tabs.GetTabAt(0).SetText(NameType);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Set Tab

        private void AddFragmentsTabs()
        {
            try
            {
                TabAdapter.ClaerFragment();

                NewsFeedTab = new NewsFeedNative();
                NotificationsTab = new NotificationsFragment();
                TrendingTab = new TrendingFragment();
                MoreTab = new MoreFragment();

                if (TabAdapter != null && TabAdapter.ItemCount <= 0)
                {
                    TabAdapter.AddFragment(NewsFeedTab, GetText(Resource.String.Lbl_News_Feed));
                    TabAdapter.AddFragment(NotificationsTab, GetText(Resource.String.Lbl_Notifications));

                    switch (AppSettings.ShowTrendingPage)
                    {
                        case true:
                            TabAdapter.AddFragment(TrendingTab, GetText(Resource.String.Lbl_Trending));
                            break;
                    }

                    TabAdapter.AddFragment(MoreTab, GetText(Resource.String.Lbl_More));

                    ViewPager.UserInputEnabled = false;
                    ViewPager.CurrentItem = TabAdapter.ItemCount;
                    ViewPager.OffscreenPageLimit = TabAdapter.ItemCount;

                    ViewPager.Orientation = ViewPager2.OrientationHorizontal;
                    ViewPager.RegisterOnPageChangeCallback(new MyOnPageChangeCallback(this));
                    ViewPager.Adapter = TabAdapter;
                    ViewPager.Adapter.NotifyDataSetChanged();
                }

                BottomNavigationTab.SelectItem(0);

                //newsFeed Tab
                Tabs = FindViewById<TabLayout>(Resource.Id.tab_home);
                Tabs.Visibility = ViewStates.Visible;
                Tabs.TabSelected += TabsOnTabSelected;
                Tabs.TabReselected += TabsOnTabReselected;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private class MyOnPageChangeCallback : ViewPager2.OnPageChangeCallback
        {
            private readonly TabbedMainActivity Activity;

            public MyOnPageChangeCallback(TabbedMainActivity activity)
            {
                try
                {
                    Activity = activity;
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            }

            public override void OnPageSelected(int position)
            {
                try
                {
                    base.OnPageSelected(position);
                    switch (position)
                    {
                        case < 0:
                            return;
                        // News_Feed_Tab
                        case 0:
                            {
                                Activity.BottomNavigationTab.SelectItem(0);

                                Activity.RlMessage.Visibility = AppSettings.MessengerIntegration ? ViewStates.Visible : ViewStates.Gone;
                                Activity.RlReels.Visibility = ViewStates.Gone;

                                if (Activity.Tabs != null)
                                    Activity.Tabs.Visibility = ViewStates.Visible;

                                break;
                            }
                        // Notifications_Tab
                        case 1:
                            {
                                Activity.BottomNavigationTab.SelectItem(1);

                                Activity.RlMessage.Visibility = ViewStates.Gone;
                                if (Activity.Tabs != null) Activity.Tabs.Visibility = ViewStates.Gone;

                                if (AppSettings.ReelsPosition == ReelsPosition.ToolBar)
                                    Activity.RlReels.Visibility = ViewStates.Visible;

                                Activity.NewsFeedTab?.MainRecyclerView?.StopVideo();
                                break;
                            }
                        // Trending_Tab
                        case 2 when AppSettings.ShowTrendingPage:
                            {
                                Activity.BottomNavigationTab.SelectItem(2);

                                Activity.RlMessage.Visibility = ViewStates.Gone;
                                if (Activity.Tabs != null) Activity.Tabs.Visibility = ViewStates.Gone;

                                if (AppSettings.ReelsPosition == ReelsPosition.ToolBar)
                                    Activity.RlReels.Visibility = ViewStates.Visible;

                                Activity.NewsFeedTab?.MainRecyclerView?.StopVideo();
                                break;
                            }
                        // More_Tab
                        case 3:
                            {
                                Activity.BottomNavigationTab.SelectItem(3);

                                Activity.RlMessage.Visibility = ViewStates.Gone;
                                if (Activity.Tabs != null) Activity.Tabs.Visibility = ViewStates.Gone;

                                if (AppSettings.ReelsPosition == ReelsPosition.ToolBar)
                                    Activity.RlReels.Visibility = ViewStates.Visible;

                                Activity.NewsFeedTab?.MainRecyclerView?.StopVideo();
                                break;
                            }
                    }
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            }
        }

        #endregion

        #region MaterialDialog

        private void ShowDialogAddStory()
        {
            try
            {
                PixImagePickerUtils.OpenDialogGallery(this, true);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Stories

        public void StoryAdapterOnItemClick(object sender, StoryAdapterClickEventArgs e)
        {
            try
            {
                var diff = NewsFeedTab?.PostFeedAdapter?.ListDiffer;
                var checkSection = diff?.FirstOrDefault(a => a.TypeView == PostModelType.Story);
                if (checkSection != null)
                {
                    //Open View Story Or Create New Story
                    var item = NewsFeedTab?.PostFeedAdapter?.HolderStory?.StoryAdapter?.GetItem(e.Position);
                    if (item != null)
                    {
                        //var circleIndicator = e.View.FindViewById<CircleImageView>(Resource.Id.profile_indicator); 
                        //circleIndicator.BorderColor = Color.ParseColor(Settings.StoryReadColor);

                        switch (item.Type)
                        {
                            case "Your":
                                ShowDialogAddStory();
                                break;
                            case "Live":
                                {
                                    if (item.DataLivePost?.LiveTime != null && item.DataLivePost?.LiveTime.Value > 0 && string.IsNullOrEmpty(item.DataLivePost?.AgoraResourceId) && string.IsNullOrEmpty(item.DataLivePost?.PostFile))
                                    {
                                        //Live
                                        //Owner >> ClientRoleBroadcaster , Users >> ClientRoleAudience
                                        Intent intent = new Intent(this, typeof(LiveStreamingActivity));
                                        intent.PutExtra(LiveConstants.KeyClientRole, LiveConstants.ClientRoleAudience);
                                        intent.PutExtra("PostId", item.DataLivePost.PostId);
                                        intent.PutExtra("StreamName", item.DataLivePost.StreamName);
                                        intent.PutExtra("PostLiveStream", JsonConvert.SerializeObject(item.DataLivePost));
                                        StartActivity(intent);
                                    }
                                    break;
                                }
                            default:
                                {
                                    if (NewsFeedTab?.PostFeedAdapter?.HolderStory?.StoryAdapter?.StoryList?.Count > 0)
                                    {
                                        List<StoryDataObject> storyList = new List<StoryDataObject>(NewsFeedTab.PostFeedAdapter?.HolderStory.StoryAdapter.StoryList);
                                        storyList.RemoveAll(o => o.Type is "Your" or "Live");

                                        var indexItem = storyList.IndexOf(item);

                                        Intent intent = new Intent(this, typeof(StoryDetailsActivity));
                                        intent.PutExtra("UserId", item.UserId);
                                        intent.PutExtra("IndexItem", indexItem);
                                        intent.PutExtra("StoriesCount", storyList.Count);
                                        intent.PutExtra("DataItem", JsonConvert.SerializeObject(new ObservableCollection<StoryDataObject>(storyList)));
                                        StartActivity(intent);

                                        //item.ProfileIndicator = AppSettings.StoryReadColor;
                                        //NewsFeedTab?.PostFeedAdapter?.HolderStory?.StoryAdapter?.NotifyItemChanged(e.Position);
                                    }
                                    break;
                                }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Back Pressed 

        public void BackPressed()
        {
            try
            {
                switch (RecentlyBackPressed)
                {
                    case true:
                        ExitHandler.RemoveCallbacks(() => { RecentlyBackPressed = false; });
                        RecentlyBackPressed = false;
                        MoveTaskToBack(true);
                        //Finish();
                        break;
                    default:
                        RecentlyBackPressed = true;
                        ToastUtils.ShowToast(this, GetString(Resource.String.press_again_exit), ToastLength.Long);
                        ExitHandler.PostDelayed(() => { RecentlyBackPressed = false; }, 2000L);
                        break;
                }
            }
            catch (Exception exception)
            {
                Finish();
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region WakeLock System

        private void AddFlagsWakeLock()
        {
            try
            {
                switch ((int)Build.VERSION.SdkInt)
                {
                    case < 23:
                        Window?.AddFlags(WindowManagerFlags.KeepScreenOn);
                        break;
                    default:
                        {
                            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.WakeLock) == Permission.Granted)
                            {
                                Window?.AddFlags(WindowManagerFlags.KeepScreenOn);
                            }
                            else
                            {
                                //request Code 110
                                new PermissionsController(this).RequestPermission(110);
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

        public void SetOnWakeLock()
        {
            try
            {
                PowerManager pm = (PowerManager)GetSystemService(PowerService);
                Wl = pm.NewWakeLock(WakeLockFlags.ScreenDim, "My Tag");
                Wl.Acquire();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetOffWakeLock()
        {
            try
            {
                PowerManager pm = (PowerManager)GetSystemService(PowerService);
                Wl = pm.NewWakeLock(WakeLockFlags.ScreenBright, "My Tag");
                Wl.Acquire();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void OffWakeLock()
        {
            try
            {
                // ..screen will stay on during this section..
                Wl?.Release();
                Wl = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Service

        private void SetService(bool run = true)
        {
            try
            {
                if (run)
                {
                    // reschedule the job 
                    AppApiService.GetInstance()?.StartService(this);
                }
                else
                {
                    // Cancel all jobs
                    AppApiService.GetInstance()?.StopJob(this);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region General App Data

        private void GetGeneralAppData()
        {
            try
            {
                var sqlEntity = new SqLiteDatabase();

                if (string.IsNullOrEmpty(Current.AccessToken) || string.IsNullOrEmpty(UserDetails.UserId))
                    sqlEntity.Get_data_Login_Credentials();

                var data = ListUtils.DataUserLoginList.FirstOrDefault();
                if (data != null && data.Status != "Active")
                {
                    data.Status = "Active";
                    UserDetails.Status = "Active";
                    sqlEntity.InsertOrUpdateLogin_Credentials(data);
                }

                var settingsData = sqlEntity.GetSettings();
                if (settingsData != null)
                    ListUtils.SettingsSiteList = settingsData;

                var dataUser = sqlEntity.Get_MyProfile();

                ListUtils.StickersList = sqlEntity.Get_From_StickersTb();

                if (Methods.CheckConnectivity())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.Get_MyProfileData_Api(this), () => ApiRequest.GetSettings_Api(this), ApiRequest.GetTimeZoneAsync });

                switch (dataUser?.ShareMyLocation)
                {
                    // Check if we're running on Android 5.0 or higher
                    case "1" when (int)Build.VERSION.SdkInt >= 23:
                        {
                            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) == Permission.Granted &&
                                ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) == Permission.Granted)
                            {
                                CheckAndGetLocation();
                            }
                            else
                            {
                                // 100 >> Storage , 103 >> Camera , 105 >> Location
                                new PermissionsController(this).RequestPermission(105);
                            }
                            break;
                        }
                    case "1":
                        CheckAndGetLocation();
                        break;
                }

                InAppUpdate();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async void CheckAndGetLocation()
        {
            try
            {
                var locationManager = (LocationManager)GetSystemService(LocationService);
                if (!locationManager!.IsProviderEnabled(LocationManager.GpsProvider))
                {

                }
                else
                {
                    var locator = CrossGeolocator.Current;
                    locator.DesiredAccuracy = 50;
                    var position = await locator.GetPositionAsync(TimeSpan.FromMilliseconds(10000));
                    Console.WriteLine("Position Status: {0}", position.Timestamp);
                    Console.WriteLine("Position Latitude: {0}", position.Latitude);
                    Console.WriteLine("Position Longitude: {0}", position.Longitude);

                    UserDetails.Lat = position.Latitude.ToString(CultureInfo.InvariantCulture);
                    UserDetails.Lng = position.Longitude.ToString(CultureInfo.InvariantCulture);

                    await Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            if (Methods.CheckConnectivity())
                            {
                                Dictionary<string, string> dictionaryProfile = new Dictionary<string, string>();

                                var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                                if (dataUser != null)
                                {
                                    dictionaryProfile = new Dictionary<string, string>();

                                    dataUser.Lat = UserDetails.Lat;
                                    dataUser.Lat = UserDetails.Lat;

                                    var sqLiteDatabase = new SqLiteDatabase();
                                    sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);
                                }

                                dictionaryProfile.Add("lat", UserDetails.Lat);
                                dictionaryProfile.Add("lng", UserDetails.Lng);

                                if (Methods.CheckConnectivity())
                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dictionaryProfile) });
                            }
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    }).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void GetOneSignalNotification()
        {
            try
            {
                var notificationObject = JsonConvert.DeserializeObject<OsObject.OsNotificationObject>(Intent?.GetStringExtra("NotificationObject") ?? "");
                if (notificationObject != null)
                {
                    //PageId, GroupId,EventId
                    NotificationsTab.EventClickNotification(this, new NotificationObject
                    {
                        NotifierId = notificationObject.UserId,
                        Notifier = new UserDataObject
                        {
                            UserId = notificationObject.UserId,
                        },
                        PostId = notificationObject.PostId,
                        PageId = notificationObject.PageId,
                        GroupId = notificationObject.GroupId,
                        EventId = notificationObject.EventId,
                        Type = notificationObject.Type,
                    });
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private static bool RunNotifications = false;
        public async Task Get_Notifications()
        {
            try
            {
                if (NotificationsTab != null)
                {
                    if (RunNotifications)
                        return;

                    RunNotifications = true;
                    var (countNotifications, countFriend, countMessages) = await NotificationsTab.LoadGeneralData(false);

                    RunOnUiThread(() =>
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(countNotifications) == false && (countNotifications != "0" && countNotifications != CountNotificationsStatic))
                            {
                                BottomNavigationTab.ShowBadge(1, Convert.ToInt32(countNotifications), true);

                                CountNotificationsStatic = countNotifications;
                            }

                            if (AppSettings.MessengerIntegration)
                            {
                                if (!string.IsNullOrEmpty(countMessages) && countMessages != "0" && countMessages != CountMessagesStatic)
                                {
                                    var listMore = MoreTab.MoreSectionAdapter1.SectionList;
                                    if (listMore?.Count > 0)
                                    {
                                        var dataTab = listMore.FirstOrDefault(a => a.Id == 2);
                                        if (dataTab != null)
                                        {
                                            CountMessagesStatic = countMessages;
                                            dataTab.BadgeCount = Convert.ToInt32(countMessages);
                                            dataTab.Badgevisibilty = true;

                                            MoreTab.MoreSectionAdapter1.NotifyItemChanged(listMore.IndexOf(dataTab), "WithoutBlobBadge");
                                        }
                                    }
                                }
                                else if (countMessages == "0")
                                {
                                    var listMore = MoreTab.MoreSectionAdapter1?.SectionList;
                                    if (listMore?.Count > 0)
                                    {
                                        var dataTab = listMore.FirstOrDefault(a => a.Id == 2);
                                        if (dataTab != null)
                                        {
                                            CountMessagesStatic = "0";
                                            dataTab.BadgeCount = 0;
                                            dataTab.Badgevisibilty = false;
                                            dataTab.IconColor = Color.ParseColor("#03a9f4");

                                            MoreTab.MoreSectionAdapter1.NotifyItemChanged(listMore.IndexOf(dataTab), "WithoutBlobBadge");
                                        }
                                    }
                                }
                            }

                            RunNotifications = false;
                        }
                        catch (Exception e)
                        {
                            RunNotifications = false;
                            Methods.DisplayReportResultTrack(e);
                        }
                    });
                }
            }
            catch (Exception e)
            {
                RunNotifications = false;
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region In App 

        private void InAppUpdate()
        {
            RunOnUiThread(() =>
            {
                try
                {
                    if (AppSettings.ShowSettingsUpdateManagerApp)
                        UpdateManagerApp.CheckUpdateApp(this, 4711, Intent);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            });
        }

        private static int CountRateApp;
        public void InAppReview()
        {
            try
            {
                bool inAppReview = MainSettings.InAppReview?.GetBoolean(MainSettings.PrefKeyInAppReview, false) ?? false;
                switch (inAppReview)
                {
                    case false when AppSettings.ShowSettingsRateApp:
                        {
                            if (CountRateApp == AppSettings.ShowRateAppCount)
                            {
                                var dialog = new MaterialAlertDialogBuilder(this);
                                dialog.SetTitle(GetText(Resource.String.Lbl_RateOurApp));
                                dialog.SetMessage(GetText(Resource.String.Lbl_RateOurAppContent));
                                dialog.SetPositiveButton(GetText(Resource.String.Lbl_Rate), (materialDialog, action) =>
                                {
                                    try
                                    {
                                        StoreReviewApp store = new StoreReviewApp();
                                        store.OpenStoreReviewPage(this, PackageName);
                                    }
                                    catch (Exception e)
                                    {
                                        Methods.DisplayReportResultTrack(e);
                                    }
                                });
                                dialog.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                                dialog.Show();

                                MainSettings.InAppReview?.Edit()?.PutBoolean(MainSettings.PrefKeyInAppReview, true)?.Commit();
                            }
                            else
                            {
                                CountRateApp++;
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

        #region PickiT >> Gert path file

        public async void PickiTonCompleteListener(string path)
        {
            //Dismiss dialog and return the path
            try
            {
                //  Check if it was a Drive/local/unknown provider file and display a Toast
                //if (wasDriveFile) => "Drive file was selected" 
                //else if (wasUnknownProvider)  => "File was selected from unknown provider" 
                //else => "Local file was selected"

                //  Chick if it was successful
                var (check, info) = await WoWonderTools.CheckMimeTypesWithServer(path);
                if (!check)
                {
                    if (info == "AdultImages")
                    {
                        //this file not allowed 
                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_Error_AdultImages), ToastLength.Short);

                        var dialog = new MaterialAlertDialogBuilder(this);
                        dialog.SetMessage(GetText(Resource.String.Lbl_Error_AdultImages));
                        dialog.SetPositiveButton(GetText(Resource.String.Lbl_IgnoreAndSend), (materialDialog, action) =>
                        {
                            try
                            {
                                var type = Methods.AttachmentFiles.Check_FileExtension(path);
                                switch (type)
                                {
                                    case "Image":
                                        {
                                            Intent intent = new Intent(this, typeof(AddStoryActivity));
                                            intent.PutExtra("Uri", path);
                                            intent.PutExtra("Type", "image");
                                            StartActivity(intent);
                                            break;
                                        }
                                    case "Video":
                                        {
                                            var fileName = path.Split('/').Last();
                                            var fileNameWithoutExtension = fileName.Split('.').First();
                                            var pathWithoutFilename = Methods.Path.FolderDcimImage;
                                            var fullPathFile = new File(Methods.Path.FolderDcimImage, fileNameWithoutExtension + ".png");

                                            var videoPlaceHolderImage = Methods.MultiMedia.GetMediaFrom_Gallery(pathWithoutFilename, fileNameWithoutExtension + ".png");
                                            switch (videoPlaceHolderImage)
                                            {
                                                case "File Dont Exists":
                                                    {
                                                        var bitmapImage = Methods.MultiMedia.Retrieve_VideoFrame_AsBitmap(this, path);
                                                        Methods.MultiMedia.Export_Bitmap_As_Image(bitmapImage, fileNameWithoutExtension, pathWithoutFilename);
                                                        break;
                                                    }
                                            }

                                            Intent intent = new Intent(this, typeof(VideoEditorActivity));
                                            intent.PutExtra("Uri", path);
                                            intent.PutExtra("Thumbnail", fullPathFile.Path);
                                            intent.PutExtra("Type", "Story");
                                            StartActivity(intent);

                                            break;
                                        }
                                }
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                        dialog.SetNegativeButton(GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());

                        dialog.Show();
                    }
                    else
                    {
                        //this file not supported on the server , please select another file 
                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_ErrorFileNotSupported), ToastLength.Short);
                    }
                }
                else
                {
                    var type = Methods.AttachmentFiles.Check_FileExtension(path);
                    switch (type)
                    {
                        case "Image":
                            {
                                Intent intent = new Intent(this, typeof(AddStoryActivity));
                                intent.PutExtra("Uri", path);
                                intent.PutExtra("Type", "image");
                                StartActivity(intent);
                                break;
                            }
                        case "Video":
                            {
                                var fileName = path.Split('/').Last();
                                var fileNameWithoutExtension = fileName.Split('.').First();
                                var pathWithoutFilename = Methods.Path.FolderDcimImage;
                                var fullPathFile = new File(Methods.Path.FolderDcimImage, fileNameWithoutExtension + ".png");

                                var videoPlaceHolderImage = Methods.MultiMedia.GetMediaFrom_Gallery(pathWithoutFilename, fileNameWithoutExtension + ".png");
                                switch (videoPlaceHolderImage)
                                {
                                    case "File Dont Exists":
                                        {
                                            var bitmapImage = Methods.MultiMedia.Retrieve_VideoFrame_AsBitmap(this, path);
                                            Methods.MultiMedia.Export_Bitmap_As_Image(bitmapImage, fileNameWithoutExtension, pathWithoutFilename);
                                            break;
                                        }
                                }

                                Intent intent = new Intent(this, typeof(VideoEditorActivity));
                                intent.PutExtra("Uri", path);
                                intent.PutExtra("Thumbnail", fullPathFile.Path);
                                intent.PutExtra("Type", "Story");
                                StartActivity(intent);

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

    }
}