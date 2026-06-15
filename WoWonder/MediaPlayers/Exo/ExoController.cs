using Android.App;
using Android.Content;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Androidx.Media3.Common;
using Androidx.Media3.Common.Util;
using Androidx.Media3.Datasource;
using Androidx.Media3.Datasource.Cache;
using Androidx.Media3.Exoplayer;
using Androidx.Media3.Exoplayer.Dash;
using Androidx.Media3.Exoplayer.Hls;
using Androidx.Media3.Exoplayer.Rtsp;
using Androidx.Media3.Exoplayer.Smoothstreaming;
using Androidx.Media3.Exoplayer.Source;
using Androidx.Media3.Exoplayer.Trackselection;
using Androidx.Media3.Extractor.TS;
using Androidx.Media3.UI;
using Java.Net;
using Java.Util.Concurrent;
using Newtonsoft.Json;
using System;
using System.Linq;
using WoWonder.Activities.NativePost.Extra;
using WoWonder.Activities.NativePost.Pages;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Helpers.Animinations;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Equalizer;
using WoWonderClient;
using Uri = Android.Net.Uri;

namespace WoWonder.MediaPlayers.Exo
{
    public class ExoController
    {
        public VolumeState VolumeStateProvider = VolumeState.On;
        public enum VolumeState
        {
            On = 0,
            Off = 1
        }

        private readonly Activity ActivityContext;
        public readonly string Page;

        private IExoPlayer VideoPlayer, FullScreenVideoPlayer;
        private PlayerView PlayerView, FullScreenPlayerView;
        private PlayerControlView ControlView;

        private PreCachingExoPlayerVideo PreCachingExoPlayerVideo;

        private IDataSource.IFactory DataSourceFactory;
        private IDataSource.IFactory HttpDataSourceFactory;
        private PlayerEvents PlayerListener;

        private ImageView MVolumeIcon, MFullScreenIcon;
        private FrameLayout MFullScreenButton;

        private string FullScreenTag;
        private bool IsFullScreen;

        private ImageView ExpandControl;
        public ProgressBar BufferProgressControl;
        public LinearLayout PositionLinearLayout;
        public EqualizerView Equalizer;
        public TextView TextPositionControl;

        private bool RepeatAniminationOnclicks = true;

        private Uri VideoUrl;
        public AdapterHolders.PostVideoSectionViewHolder VideoHolder;

        public ExoController(Activity context, string page = "")
        {
            try
            {
                ActivityContext = context;
                Page = page;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetPlayer(PlayerView playerView, bool useController = true)
        {
            try
            {
                PlayerView = playerView;

                PreCachingExoPlayerVideo = new PreCachingExoPlayerVideo(ActivityContext);
                DefaultTrackSelector trackSelector = new DefaultTrackSelector(ActivityContext);
                ControlView = PlayerView.FindViewById<PlayerControlView>(Resource.Id.exo_controller);

                VideoPlayer = new IExoPlayer.Builder(ActivityContext)?.SetTrackSelector(trackSelector)?.Build();
                PlayerListener = new PlayerEvents(this, ControlView);
                VideoPlayer?.AddListener(PlayerListener);

                PlayerView.UseController = useController;
                PlayerView.Player = VideoPlayer;

                if (Page == "Post")
                {
                    PlayerView.Player.RepeatMode = 2;
                    PlayerView.SetKeepContentOnPlayerReset(true);

                    PlayerView.ControllerShowTimeoutMs = 0;
                    PlayerView.ControllerHideOnTouch = false;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetPlayer(AdapterHolders.PostVideoSectionViewHolder holder)
        {
            try
            {
                VideoHolder = holder;
                SetPlayer(holder.ExoPlayer);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetPlayerControl(bool showFullScreen = true, bool isFullScreen = false)
        {
            try
            {
                if (ControlView != null)
                {
                    RelativeLayout defControlLayout = ControlView.FindViewById<RelativeLayout>(Resource.Id.defControlLayout);
                    RelativeLayout ExoRootView = ControlView.FindViewById<RelativeLayout>(Resource.Id.feedControlLayout);

                    if (Page == "Post")
                    {
                        ExoRootView.Visibility = ViewStates.Visible;
                        defControlLayout.Visibility = ViewStates.Gone;

                        MVolumeIcon = ControlView.FindViewById<ImageView>(Resource.Id.volume_icon);
                        ExpandControl = ControlView.FindViewById<ImageView>(Resource.Id.expand_icon);
                        BufferProgressControl = ControlView.FindViewById<ProgressBar>(Resource.Id.Progres_buffering);
                        PositionLinearLayout = ControlView.FindViewById<LinearLayout>(Resource.Id.positionLinear);
                        Equalizer = ControlView.FindViewById<EqualizerView>(Resource.Id.equalizer_view);
                        TextPositionControl = ControlView.FindViewById<TextView>(Resource.Id.txt_position);

                        if (!ExoRootView.HasOnClickListeners)
                        {
                            ExpandControl.Click += (sender, args) =>
                            {
                                try
                                {
                                    Intent intent = new Intent(ActivityContext, typeof(VideoFullScreenActivity));
                                    intent.PutExtra("type", "auto");
                                    intent.PutExtra("page", Page);
                                    intent.PutExtra("videoUrl", VideoUrl?.ToString());
                                    intent.PutExtra("videoPosition", PlayerView?.Player?.CurrentPosition.ToString());
                                    ActivityContext.StartActivity(intent);
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            };

                            ExoRootView.Click += (sender, args) =>
                            {
                                try
                                {
                                    if (ViewFullVideoPostActivity.PageIsOpen)
                                        return;

                                    var item = WRecyclerView.GetInstance().NativeFeedAdapter.ListDiffer[VideoHolder.BindingAdapterPosition]?.PostData;

                                    var intent = new Intent(ActivityContext, typeof(ViewFullVideoPostActivity));
                                    intent.PutExtra("PostId", item?.PostId);
                                    intent.PutExtra("PostObject", JsonConvert.SerializeObject(item));
                                    ActivityContext.StartActivity(intent);
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            };
                        }
                    }
                    else
                    {
                        ExoRootView.Visibility = ViewStates.Gone;
                        defControlLayout.Visibility = ViewStates.Visible;

                        //Check All Views 
                        MVolumeIcon = ControlView.FindViewById<ImageView>(Resource.Id.exo_volume_icon);
                        MFullScreenIcon = ControlView.FindViewById<ImageView>(Resource.Id.exo_fullscreen_icon);
                        MFullScreenButton = ControlView.FindViewById<FrameLayout>(Resource.Id.exo_fullscreen_button);

                        if (!showFullScreen)
                        {
                            MVolumeIcon.Visibility = ViewStates.Gone;
                            MFullScreenButton.Visibility = ViewStates.Gone;
                        }
                        else
                        {
                            MFullScreenButton.Visibility = ViewStates.Visible;

                            MFullScreenButton.Click += MFullScreenButtonOnClick;

                            if (isFullScreen)
                            {
                                FullScreenTag = "true";
                                MFullScreenIcon.SetImageResource(Resource.Drawable.ic_action_ic_fullscreen_skrink);
                            }
                            else
                            {
                                FullScreenTag = "false";
                                MFullScreenIcon.SetImageResource(Resource.Drawable.ic_action_ic_fullscreen_expand);
                            }
                        }
                    }

                    switch (VolumeStateProvider)
                    {
                        case VolumeState.Off:
                            MVolumeIcon.SetImageResource(Resource.Drawable.ic_volume_off_grey_24dp);
                            VideoPlayer.Volume = 0f;
                            VolumeStateProvider = VolumeState.Off;
                            break;
                        case VolumeState.On:
                            MVolumeIcon.SetImageResource(Resource.Drawable.ic_volume_up_grey_24dp);
                            VideoPlayer.Volume = 1f;
                            VolumeStateProvider = VolumeState.On;
                            break;
                        default:
                            MVolumeIcon.SetImageResource(Resource.Drawable.ic_volume_off_grey_24dp);
                            VideoPlayer.Volume = 0f;
                            VolumeStateProvider = VolumeState.Off;
                            break;
                    }

                    MVolumeIcon.Click += ToggleVolumeOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public IMediaSource GetMediaSourceFromUrl(Uri uri, string tag)
        {
            try
            {
                var extension = uri?.Path?.Split('.').LastOrDefault();
                var mime = MimeTypeMap.GetMimeType(extension);
                var mediaItem = new MediaItem.Builder()?.SetUri(uri)?.SetMediaId(tag)?.SetMimeType(mime)?.Build();

                IMediaSource src;
                if (!string.IsNullOrEmpty(uri.Path) && (uri.Path.Contains("file://") || uri.Path.Contains("content://") || uri.Path.Contains("storage") || uri.Path.Contains("/data/user/0/")))
                {
                    DataSourceFactory = new FileDataSource.Factory();
                    DefaultDataSource.Factory upstreamFactory = new DefaultDataSource.Factory(ActivityContext, DataSourceFactory);
                    src = new ProgressiveMediaSource.Factory(upstreamFactory).CreateMediaSource(mediaItem);
                }
                else
                {
                    DefaultDataSource.Factory upstreamFactory = new DefaultDataSource.Factory(ActivityContext, GetHttpDataSourceFactory());
                    DataSourceFactory = BuildReadOnlyCacheDataSource(upstreamFactory, PreCachingExoPlayerVideo.GetCache());

                    int contentType = Util.InferContentTypeForUriAndMimeType(uri, extension);
                    switch (contentType)
                    {
                        case C.ContentTypeSs:
                            src = new SsMediaSource.Factory(DataSourceFactory).CreateMediaSource(mediaItem);
                            break;
                        case C.ContentTypeDash:
                            src = new DashMediaSource.Factory(DataSourceFactory).CreateMediaSource(mediaItem);
                            break;
                        case C.ContentTypeRtsp:
                            src = new RtspMediaSource.Factory().CreateMediaSource(mediaItem);
                            break;
                        case C.ContentTypeHls:
                            DefaultHlsExtractorFactory defaultHlsExtractorFactory = new DefaultHlsExtractorFactory(DefaultTsPayloadReaderFactory.FlagAllowNonIdrKeyframes, true);
                            src = new HlsMediaSource.Factory(DataSourceFactory).SetExtractorFactory(defaultHlsExtractorFactory)?.CreateMediaSource(mediaItem);
                            break;
                        default:
                            src = new ProgressiveMediaSource.Factory(DataSourceFactory).CreateMediaSource(mediaItem);
                            break;
                    }
                }

                return src;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null;
            }
        }

        private IDataSource.IFactory GetHttpDataSourceFactory()
        {
            if (HttpDataSourceFactory == null)
            {
                CookieManager cookieManager = new CookieManager();
                cookieManager.SetCookiePolicy(ICookiePolicy.AcceptOriginalServer);
                CookieHandler.Default = cookieManager;
                HttpDataSourceFactory = new DefaultHttpDataSource.Factory();
            }

            return HttpDataSourceFactory;
        }

        private CacheDataSource.Factory BuildReadOnlyCacheDataSource(IDataSource.IFactory upstreamFactory, ICache cache)
        {
            return new CacheDataSource.Factory()?.SetCache(cache)?.SetUpstreamDataSourceFactory(upstreamFactory)?.SetCacheWriteDataSinkFactory(null)?.SetFlags(CacheDataSource.FlagIgnoreCacheOnError);
        }

        public void FirstPlayVideo(Uri uri)
        {
            try
            {
                VideoUrl = uri;
                var videoSource = GetMediaSourceFromUrl(uri, "normal");

                if (PlayerSettings.EnableOfflineMode && uri.ToString()!.Contains("http"))
                {
                    PreCachingExoPlayerVideo.CacheVideosFiles(uri);
                    videoSource = new ProgressiveMediaSource.Factory(PreCachingExoPlayerVideo.CacheDataSourceFactory).CreateMediaSource(MediaItem.FromUri(uri));
                }

                VideoPlayer.SetMediaSource(videoSource, true);
                VideoPlayer.Prepare();
                VideoPlayer.PlayWhenReady = true;
                VideoPlayer.SeekTo(0, 0);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void FirstPlayVideo(Uri uri, int videoDuration)
        {
            try
            {
                VideoUrl = uri;
                var videoSource = GetMediaSourceFromUrl(uri, "normal");

                if (PlayerSettings.EnableOfflineMode && uri.ToString()!.Contains("http"))
                {
                    PreCachingExoPlayerVideo.CacheVideosFiles(uri);
                    videoSource = new ProgressiveMediaSource.Factory(PreCachingExoPlayerVideo.CacheDataSourceFactory).CreateMediaSource(MediaItem.FromUri(uri));
                }

                VideoPlayer.SetMediaSource(videoSource);
                VideoPlayer.Prepare();
                VideoPlayer.PlayWhenReady = true;
                VideoPlayer.SeekTo(videoDuration);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void PlayVideo()
        {
            try
            {
                if (PlayerView?.Player != null && PlayerView.Player.PlaybackState == IPlayer.StateReady && !PlayerView.Player.PlayWhenReady)
                    PlayerView.Player.PlayWhenReady = true;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void StopVideo()
        {
            try
            {
                if (PlayerView?.Player != null && PlayerView.Player.PlayWhenReady)
                    PlayerView.Player.PlayWhenReady = false;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void ReleaseVideo()
        {
            try
            {
                StopVideo();
                PlayerView?.Player?.Stop();

                if (VideoPlayer != null)
                {
                    VideoPlayer.Release();
                    VideoPlayer = null!;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public PlayerView GetPlayerView()
        {
            return PlayerView;
        }

        public IExoPlayer GetExoPlayer()
        {
            return VideoPlayer;
        }

        public void ToggleExoPlayerKeepScreenOnFeature(bool keepScreenOn)
        {
            try
            {
                if (PlayerView != null)
                {
                    PlayerView.KeepScreenOn = keepScreenOn;
                }

                if (FullScreenPlayerView != null)
                {
                    FullScreenPlayerView.KeepScreenOn = keepScreenOn;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void Repeat()
        {
            try
            {
                if (VideoPlayer != null)
                {
                    VideoPlayer.SeekTo(0, 0);
                    VideoPlayer.PlayWhenReady = true;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region FullScreen

        private void MFullScreenButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (FullScreenTag == "false")
                {
                    InitFullscreenDialog("Open");
                }
                else if (FullScreenTag == "true")
                {
                    InitFullscreenDialog("Close");
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void InitFullscreenDialog(string action)
        {
            try
            {
                if (action == "Open")
                {
                    Intent intent = new Intent(ActivityContext, typeof(VideoFullScreenActivity));
                    intent.PutExtra("type", "auto");
                    intent.PutExtra("page", Page);
                    ActivityContext.StartActivityForResult(intent, 2000);
                    IsFullScreen = true;
                }
                else
                {
                    Intent intent = new Intent();
                    VideoFullScreenActivity.Instance?.SetResult(Result.Ok, intent);
                    VideoFullScreenActivity.Instance?.Finish();
                    IsFullScreen = false;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetFullScreenPlayerView(PlayerView playerView)
        {
            try
            {
                FullScreenPlayerView = playerView;

                ControlView = FullScreenPlayerView.FindViewById<PlayerControlView>(Resource.Id.exo_controller);

                FullScreenVideoPlayer = new IExoPlayer.Builder(ActivityContext)?.SetTrackSelector(new DefaultTrackSelector(ActivityContext))?.Build();
                var playerListener = new PlayerEvents(this, ControlView);
                FullScreenVideoPlayer?.AddListener(playerListener);

                FullScreenPlayerView.UseController = true;
                FullScreenPlayerView.Player = VideoPlayer;
                FullScreenPlayerView.Player.PlayWhenReady = true;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void PlayFullScreen()
        {
            try
            {
                if (FullScreenPlayerView != null)
                {
                    FullScreenPlayerView.Player = VideoPlayer;
                    if (FullScreenPlayerView.Player != null) FullScreenPlayerView.Player.PlayWhenReady = true;

                    FullScreenTag = "true";
                    MFullScreenIcon.SetImageDrawable(ActivityContext.GetDrawable(Resource.Drawable.ic_action_ic_fullscreen_skrink));
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void RestartPlayAfterShrinkScreen()
        {
            try
            {
                PlayerView.Player = null!;
                if (FullScreenPlayerView != null)
                {
                    PlayerView.Player = FullScreenPlayerView.Player;
                    PlayerView.Player.PlayWhenReady = true;
                    PlayerView.RequestFocus();
                    PlayerView.Visibility = ViewStates.Visible;

                    FullScreenTag = "false";
                    MFullScreenIcon.SetImageDrawable(ActivityContext.GetDrawable(Resource.Drawable.ic_action_ic_fullscreen_expand));

                    FullScreenPlayerView.Player = null;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Volume

        private void ToggleVolumeOnClick(object sender, EventArgs e)
        {
            try
            {
                switch (VolumeStateProvider)
                {
                    case VolumeState.Off:
                        Console.WriteLine("togglePlaybackState: enabling volume.");
                        SetVolumeControl(VolumeState.On);
                        break;
                    case VolumeState.On:
                        Console.WriteLine("togglePlaybackState: disabling volume.");
                        SetVolumeControl(VolumeState.Off);
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void SetVolumeControl(VolumeState state)
        {
            try
            {
                VolumeStateProvider = state;
                switch (state)
                {
                    case VolumeState.Off:
                        MVolumeIcon.SetImageResource(Resource.Drawable.ic_volume_off_grey_24dp);
                        VideoPlayer.Volume = 0f;
                        break;
                    case VolumeState.On:
                        MVolumeIcon.SetImageResource(Resource.Drawable.ic_volume_up_grey_24dp);
                        VideoPlayer.Volume = 1f;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Post

        public void OnPrePlay(AdapterHolders.PostVideoSectionViewHolder holder, ExoController exoGlobalController, Uri VideoUrl)
        {
            try
            {
                VideoHolder = holder;

                holder.ExoPlayer.Visibility = ViewStates.Gone;
                holder.VideoImage.Visibility = ViewStates.Visible;

                exoGlobalController.FirstPlayVideo(VideoUrl);

                if (ControlView != null)
                    ControlView.ProgressUpdate += ControlView_ProgressUpdate;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnPlayCanceled(AdapterHolders.PostVideoSectionViewHolder holder)
        {
            try
            {
                VideoHolder = holder;

                ReleaseVideo();

                if (ControlView != null)
                    ControlView.ProgressUpdate -= ControlView_ProgressUpdate;

                holder.PlayButton.Visibility = ViewStates.Visible;
                holder.VideoImage.Visibility = ViewStates.Visible;
                RepeatAniminationOnclicks = true;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnPlay()
        {
            try
            {
                if (!RepeatAniminationOnclicks || VideoHolder == null)
                    return;

                Animation fadeOut = new AlphaAnimation(1, 0);
                fadeOut.Interpolator = new AccelerateInterpolator();
                fadeOut.Duration = 400;
                fadeOut.SetAnimationListener(new ImageFadeOutAnimationListener(VideoHolder.VideoImage));
                Action myAction = () =>
                {
                    if (PlayerView.Player != null)
                    {
                        PlayerView.Visibility = ViewStates.Visible;
                        VideoHolder.PlayButton.Visibility = ViewStates.Invisible;
                        VideoHolder.VideoImage.StartAnimation(fadeOut);
                        RepeatAniminationOnclicks = false;
                    }
                };

                VideoHolder.MainView.PostDelayed(myAction, 500L);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ControlView_ProgressUpdate(object sender, PlayerControlView.ProgressUpdateEventArgs e)
        {
            try
            {
                var timeLeft = (e.P1 - e.P0);
                var duration = $"{TimeUnit.Milliseconds.ToMinutes(timeLeft) % TimeUnit.Hours.ToMinutes(1):00}:{(TimeUnit.Milliseconds.ToSeconds(timeLeft) % TimeUnit.Minutes.ToSeconds(1)):00}";
                TextPositionControl.Text = duration;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }



        #endregion
    }
}