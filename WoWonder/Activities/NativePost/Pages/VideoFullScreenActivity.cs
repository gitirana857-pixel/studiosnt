using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Androidx.Media3.UI;
using System;
using WoWonder.Activities.Base;
using WoWonder.Activities.Videos;
using WoWonder.Helpers.Utils;
using WoWonder.MediaPlayers.Exo;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.NativePost.Pages
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", LaunchMode = LaunchMode.SingleInstance, ConfigurationChanges = ConfigChanges.Keyboard | ConfigChanges.Locale | ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenLayout | ConfigChanges.ScreenSize | ConfigChanges.SmallestScreenSize | ConfigChanges.UiMode)]
    public class VideoFullScreenActivity : BaseActivity
    {
        #region Variables Basic

        private ExoController ExoController;
        private PlayerView PlayerViewFullScreen;
        public static VideoFullScreenActivity Instance;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                //Set Full screen 
                Methods.App.FullScreenApp(this, true);

                var type = Intent?.GetStringExtra("type") ?? "";
                if (type == "RequestedOrientation")
                {
                    //ScreenOrientation.Portrait >>  Make to run your application only in portrait mode
                    //ScreenOrientation.Landscape >> Make to run your application only in LANDSCAPE mode 
                    RequestedOrientation = ScreenOrientation.Landscape;
                }

                SetContentView(Resource.Layout.VideoFullScreenLayout);

                Instance = this;

                //Get Value And Set Toolbar
                InitComponent();
                InitBackPressed("VideoFullScreenActivity");
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
                ExoController?.PlayVideo();
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
                ExoController?.StopVideo();
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
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        protected override void OnDestroy()
        {
            try
            {
                Instance = null;
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnDestroy();
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
                PlayerViewFullScreen = FindViewById<PlayerView>(Resource.Id.pv_fullscreen);

                //===================== Exo Player ======================== 

                var page = Intent?.GetStringExtra("page") ?? "";
                if (page == "VideoViewerActivity")
                {
                    var videoViewerPage = VideoViewerActivity.GetInstance();
                    ExoController = videoViewerPage.ExoController;

                    ExoController?.SetFullScreenPlayerView(PlayerViewFullScreen);
                    ExoController?.PlayFullScreen();
                    ExoController?.SetPlayerControl(true, true);
                }
                else if (page == "ViewFullVideoPostActivity")
                {
                    var videoViewerPage = ViewFullVideoPostActivity.GetInstance();
                    ExoController = videoViewerPage.ExoController;

                    ExoController?.SetFullScreenPlayerView(PlayerViewFullScreen);
                    ExoController?.PlayFullScreen();
                    ExoController?.SetPlayerControl(true, true);
                }
                else
                {
                    var videoUrl = Intent?.GetStringExtra("videoUrl") ?? "";
                    var videoPosition = Intent?.GetStringExtra("videoPosition") ?? "0";

                    ExoController = new ExoController(this);
                    ExoController.SetPlayer(PlayerViewFullScreen);
                    ExoController.SetPlayerControl(true, true);

                    ExoController?.SetFullScreenPlayerView(PlayerViewFullScreen);

                    // Uri
                    Uri uri = Uri.Parse(videoUrl);
                    ExoController?.FirstPlayVideo(uri, int.Parse(videoPosition));
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void BackPressed()
        {
            ExoController?.InitFullscreenDialog("Close");
            Finish();
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            try
            {
                if (newConfig.Orientation == Orientation.Landscape)
                {
                }
                else if (newConfig.Orientation == Orientation.Portrait)
                {
                    ExoController?.InitFullscreenDialog("Close");
                }
                base.OnConfigurationChanged(newConfig);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

    }
}