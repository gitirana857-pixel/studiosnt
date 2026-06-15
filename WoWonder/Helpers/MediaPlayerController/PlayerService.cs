using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Media;
using Android.Media.Session;
using Android.OS;
using Android.Telephony;
using Android.Views;
using Android.Widget;
using Androidx.Media3.Common;
using Androidx.Media3.Common.Text;
using Androidx.Media3.Datasource;
using Androidx.Media3.Exoplayer;
using Androidx.Media3.Exoplayer.Source;
using Androidx.Media3.Exoplayer.Trackselection;
using Androidx.Media3.Exoplayer.Upstream;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using AndroidX.Media.Session;
using Java.Lang;
using System;
using System.Threading.Tasks;
using WoWonder.Activities;
using WoWonder.Activities.Tabbes;
using WoWonder.Helpers.Utils;
using WoWonderClient;
using WoWonderClient.Classes.Posts;
using AudioAttributes = Androidx.Media3.Common.AudioAttributes;
using Exception = System.Exception;
using MediaMetadata = Androidx.Media3.Common.MediaMetadata;
using Object = Java.Lang.Object;
using Uri = Android.Net.Uri;

namespace WoWonder.Helpers.MediaPlayerController
{
    [Service(Exported = false, ForegroundServiceType = ForegroundService.TypeMediaPlayback)]
    public class PlayerService : Service
    {
        #region Variables Basic

        public static string ActionBackward;
        public static string ActionForward;
        public static string ActionPlaybackSpeed;
        public static string ActionSeekTo;
        public static string ActionPlay;
        public static string ActionPause;
        public static string ActionStop;
        public static string ActionSkip;
        public static string ActionRewind;
        public static string ActionToggle;
        private static NotificationCompat.Builder Notification;
        private RemoteViews BigViews, SmallViews;
        private readonly string NotificationChannelId = "sound_ch_1";
        private NotificationManager MNotificationManager;
        private CallBroadcastReceiver OnCallIncome;
        private static PlayerService Service;
        private TabbedMainActivity GlobalContext;
        private PostDataObject Item;

        private HeadPhoneBroadcastReceiver OnHeadPhoneDetect;
        private readonly DefaultBandwidthMeter BandwidthMeter = new DefaultBandwidthMeter.Builder(Application.Context).Build();
        private IMediaSource PlayerSource;
        private PlayerEvents PlayerListener;
        private AudioManager AudioManager;
        private ComponentName ComponentName;
        private MyAudioFocusChangeListener OnAudioFocusChangeListener;
        private AudioFocusRequestClass FocusRequest;
        private readonly int ResumeWindow = 0;
        private readonly long ResumePosition = 0;

        #endregion

        #region General

        public static PlayerService GetPlayerService()
        {
            return Service;
        }

        public override IBinder OnBind(Intent intent)
        {
            return null!;
        }

        public override void OnCreate()
        {
            try
            {
                base.OnCreate();
                Service = this;

                GlobalContext = TabbedMainActivity.GetInstance();
                MNotificationManager = (NotificationManager)GetSystemService(NotificationService);

                OnCallIncome = new CallBroadcastReceiver();
                OnHeadPhoneDetect = new HeadPhoneBroadcastReceiver();

                RegisterReceiver(OnCallIncome, new IntentFilter("android.intent.action.PHONE_STATE"));
                RegisterReceiver(OnHeadPhoneDetect, new IntentFilter(AudioManager.ActionAudioBecomingNoisy));

                SetAudioFocus();
                InitializePlayer();

                IsFirstTime = true;
                CreateNoti();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetAudioFocus()
        {
            try
            {
                if (OnAudioFocusChangeListener == null)
                {
                    OnAudioFocusChangeListener = new MyAudioFocusChangeListener();
                    AudioManager = (AudioManager)Application.Context.GetSystemService(AudioService);

#pragma warning disable 618
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                    {
                        var playbackAttributes = new Android.Media.AudioAttributes.Builder()
                            ?.SetUsage(AudioUsageKind.Media)
                            ?.SetContentType(AudioContentType.Music)
                            ?.Build();

                        FocusRequest = new AudioFocusRequestClass.Builder(AudioFocus.Gain)
                            ?.SetAudioAttributes(playbackAttributes)
                            ?.SetAcceptsDelayedFocusGain(true)
                            ?.SetOnAudioFocusChangeListener(OnAudioFocusChangeListener)
                            ?.Build();

                        AudioManager?.RequestAudioFocus(FocusRequest);
                    }
                    else
                    {
                        AudioManager.RequestAudioFocus(OnAudioFocusChangeListener, Stream.Music, AudioFocus.Gain);
#pragma warning restore 618
                    }

                    ComponentName = new ComponentName(Application.Context.PackageName, new MediaButtonIntentReceiver().Class.Name);
#pragma warning disable 618
                    AudioManager.RegisterMediaButtonEventReceiver(ComponentName);
#pragma warning restore 618 
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitializePlayer()
        {
            try
            {
                if (Constant.Player == null)
                {
                    AdaptiveTrackSelection.Factory trackSelectionFactory = new AdaptiveTrackSelection.Factory();
                    var trackSelector = new DefaultTrackSelector(Application.Context, trackSelectionFactory);
                    Constant.Player = new IExoPlayer.Builder(Application.Context)?.SetTrackSelector(trackSelector)?.Build();
                    Constant.Player.ShuffleModeEnabled = Constant.IsSuffle;
                    PlayerListener = new PlayerEvents(this);
                    Constant.Player.AddListener(PlayerListener);
                    Constant.Player.SetAudioAttributes(new AudioAttributes.Builder()?.SetUsage(C.UsageMedia)?.SetContentType(2)?.Build(), true);
                    Constant.Player.SeekParameters = SeekParameters.Default;
                    Constant.Player.SetAudioAttributes(AudioAttributes.Default, true);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            try
            {
                base.OnStartCommand(intent, flags, startId);
                string action = intent.Action;
                if (action == ActionSeekTo)
                {
                    SeekTo(intent.Extras?.GetLong("seekTo") ?? 0);
                }
                else if (action == ActionPlay)
                {
                    Play();
                }
                else if (action == ActionPause)
                {
                    Pause();
                }
                else if (action == ActionStop)
                {
                    Stop(intent);
                }
                else if (action == ActionRewind)
                {
                    Previous();
                }
                else if (action == ActionSkip)
                {
                    Next();
                }
                else if (action == ActionBackward)
                {
                    Backward();
                }
                else if (action == ActionForward)
                {
                    Forward();
                }
                else if (action == ActionToggle)
                {
                    TogglePlay();
                }
                else if (action == ActionPlaybackSpeed)
                {
                    PlaybackSpeed(intent.Extras?.GetString("PlaybackSpeed") ?? "Normal");
                }

                return StartCommandResult.Sticky;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return StartCommandResult.NotSticky;
            }
        }

        private void HandleFirstPlay()
        {
            try
            {
                Constant.IsPlayed = false;

                SetPlayAudio();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SeekTo(long seek)
        {
            try
            {
                var totalDuration = Constant.Player.Duration;
                var currentPosition = MusicUtils.ProgressToTimer(seek, totalDuration);

                // forward or backward to certain seconds
                Constant.Player.SeekTo(currentPosition);

                GlobalContext?.SoundController?.SeekUpdate();

                if (Constant.IsPlayed && !Constant.Player.PlayWhenReady)
                    Constant.Player.PlayWhenReady = true;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void Play()
        {
            try
            {
                if (Constant.IsPlayed)
                {
                    Constant.Player.PlayWhenReady = true;
                    GlobalContext?.SoundController?.SetProgress();
                }
                else
                {
                    HandleFirstPlay();
                }

                ChangePlayPauseIcon();
                UpdateNotiPlay(Constant.Player.PlayWhenReady);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void Pause()
        {
            try
            {
                Constant.Player.PlayWhenReady = false;

                ChangePlayPauseIcon();
                UpdateNotiPlay(Constant.Player.PlayWhenReady);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void Stop(Intent intent)
        {
            try
            {

                ChangePlayPauseIcon();
                Constant.IsPlayed = false;
                GlobalContext?.SoundController?.ReleaseSound();
                GlobalContext?.SoundController?.Destroy();
                RemoveNoti();

                if (AudioManager != null)
                {
#pragma warning disable 618
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                    {
                        if (FocusRequest != null) AudioManager.AbandonAudioFocusRequest(FocusRequest);
                    }
                    else
                        AudioManager.AbandonAudioFocus(OnAudioFocusChangeListener);

                    AudioManager.UnregisterMediaButtonEventReceiver(ComponentName);
#pragma warning restore 618
                }

                try { UnregisterReceiver(OnCallIncome); } catch { }
                try { UnregisterReceiver(OnHeadPhoneDetect); } catch { }

                if (intent != null) StopService(intent);
                StopForeground(StopForegroundFlags.Remove);

                StopSelf();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void Previous()
        {
            try
            {
                SetBuffer(true);
                if (Constant.IsSuffle)
                {
                    Random rand = new Random();
                    if (Constant.ArrayListPlay.Count > 0)
                        Constant.PlayPos = rand.Next(Constant.ArrayListPlay.Count);
                }
                else
                {
                    if (Constant.PlayPos > 0)
                    {
                        Constant.PlayPos -= 1;
                    }
                    else
                    {
                        Constant.PlayPos = Constant.ArrayListPlay.Count - 1;
                    }
                }

                HandleFirstPlay();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void Next()
        {
            try
            {
                SetBuffer(true);
                if (Constant.IsSuffle)
                {
                    Random rand = new Random();
                    Constant.PlayPos = rand.Next(Constant.ArrayListPlay.Count - 1 + 1);
                }
                else
                {
                    if (Constant.PlayPos < Constant.ArrayListPlay.Count - 1)
                    {
                        Constant.PlayPos += 1;
                    }
                    else
                    {
                        Constant.PlayPos = 0;
                    }
                }

                HandleFirstPlay();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void Backward()
        {
            try
            {
                var bTime = 15000; // 15 Sec
                if (Constant.Player != null)
                {
                    var sTime = Constant.Player.CurrentPosition;

                    if ((sTime - bTime) > 0)
                    {
                        sTime -= bTime;
                        Constant.Player.SeekTo(sTime);

                        GlobalContext?.SoundController?.SeekUpdate();

                        if (Constant.IsPlayed && !Constant.Player.PlayWhenReady)
                            Constant.Player.PlayWhenReady = true;
                    }
                    else
                    {
                        Toast.MakeText(Application.Context, "Cannot jump backward 15 seconds", ToastLength.Short)?.Show();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void Forward()
        {
            try
            {
                var fTime = 15000; // 15 Sec
                if (Constant.Player != null)
                {
                    var eTime = Constant.Player.Duration;
                    var sTime = Constant.Player.CurrentPosition;
                    if ((sTime + fTime) <= eTime)
                    {
                        sTime += fTime;
                        Constant.Player.SeekTo(sTime);

                        GlobalContext?.SoundController?.SeekUpdate();

                        if (Constant.IsPlayed && !Constant.Player.PlayWhenReady)
                            Constant.Player.PlayWhenReady = true;
                    }
                    else
                    {
                        Toast.MakeText(Application.Context, "Cannot jump forward 15 seconds", ToastLength.Short)?.Show();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void TogglePlay()
        {
            try
            {
                if (Constant.Player != null && Constant.Player.PlayWhenReady)
                {
                    Pause();
                }
                else
                {
                    Play();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void PlaybackSpeed(string speed)
        {
            try
            {
                const float speedNormal = 1f;
                const float speedMedium = 1.5f;
                const float speedHigh = 2f;

                if (Constant.Player != null)
                {
                    PlaybackParameters param = new PlaybackParameters(speedNormal);
                    switch (speed)
                    {
                        case "Medium":
                            param = new PlaybackParameters(speedMedium);
                            break;
                        case "High":
                            param = new PlaybackParameters(speedHigh);
                            break;
                        case "Normal":
                            param = new PlaybackParameters(speedNormal);
                            break;
                    }

                    Constant.Player.PlaybackParameters = param;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region data Noti

        private void ShowNotification()
        {
            try
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
                {
                    StartForeground(101, Notification?.Build(), ForegroundService.TypeMediaPlayback);
                }
                else
                {
                    StartForeground(101, Notification?.Build());
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void RemoveNoti()
        {
            try
            {
                MNotificationManager.CancelAll();
                Notification = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void CreateNoti()
        {
            try
            {
                BigViews = new RemoteViews(PackageName, Resource.Layout.CustomNotificationLayout);
                SmallViews = new RemoteViews(PackageName, Resource.Layout.CustomNotificationSmallLayout);

                Intent notificationIntent = new Intent(this, typeof(SplashScreenActivity));
                notificationIntent.SetAction(Intent.ActionMain);
                notificationIntent.AddCategory(Intent.CategoryLauncher);
                notificationIntent.PutExtra("isnoti", true);

                PendingIntent pendingIntent = PendingIntent.GetActivity(this, 0, notificationIntent, Build.VERSION.SdkInt >= BuildVersionCodes.M ? PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable : PendingIntentFlags.UpdateCurrent);

                Intent playIntent = new Intent(this, typeof(PlayerService));
                playIntent.SetAction(ActionToggle);
                PendingIntent pplayIntent = PendingIntent.GetService(this, 0, playIntent, Build.VERSION.SdkInt >= BuildVersionCodes.M ? 0 | PendingIntentFlags.Immutable : 0);

                Intent closeIntent = new Intent(this, typeof(PlayerService));
                closeIntent.SetAction(ActionStop);
                PendingIntent pcloseIntent = PendingIntent.GetService(this, 0, closeIntent, Build.VERSION.SdkInt >= BuildVersionCodes.M ? 0 | PendingIntentFlags.Immutable : 0);

                Notification = new NotificationCompat.Builder(this, NotificationChannelId)
                    .SetLargeIcon(BitmapFactory.DecodeResource(Resources, Resource.Mipmap.icon))
                    .SetContentTitle(AppSettings.ApplicationName)
                    .SetPriority((int)NotificationPriority.Max)
                    .SetContentIntent(pendingIntent)
                    .SetSmallIcon(Resource.Drawable.icon_player_notification)
                    //.SetTicker(Constant.ArrayListPlay[Constant.PlayPos]?.Title)
                    .SetChannelId(NotificationChannelId)
                    .SetOngoing(true)
                    .SetAutoCancel(true)
                    .SetOnlyAlertOnce(true);

                NotificationChannel mChannel;
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    NotificationImportance importance = NotificationImportance.Low;
                    mChannel = new NotificationChannel(NotificationChannelId, AppSettings.ApplicationName, importance);
                    MNotificationManager.CreateNotificationChannel(mChannel);

                    var mMediaSession = new MediaSession(Application.Context, AppSettings.ApplicationName);
                    mMediaSession.SetFlags(MediaSessionFlags.HandlesMediaButtons | MediaSessionFlags.HandlesTransportControls);

                    Notification.SetStyle(new AndroidX.Media.App.NotificationCompat.MediaStyle()?
                            //.SetMediaSession(mMediaSession.SessionToken)  
                            .SetShowCancelButton(true)?
                            .SetShowActionsInCompactView(0)?
                           .SetCancelButtonIntent(MediaButtonReceiver.BuildMediaButtonPendingIntent(Application.Context, PlaybackState.ActionStop)))
                        .AddAction(new NotificationCompat.Action(Resource.Drawable.icon_pause_vector, "Pause", pplayIntent))
                        .AddAction(new NotificationCompat.Action(Resource.Drawable.icon_close_vector, "Close", pcloseIntent));
                }
                else
                {
                    var Item = Constant.ArrayListPlay[Constant.PlayPos];
                    string songName = WoWonderTools.GetNameFinal(Item.Publisher);
                    string genresName = "";

                    BigViews.SetOnClickPendingIntent(Resource.Id.imageView_noti_play, pplayIntent);
                    BigViews.SetOnClickPendingIntent(Resource.Id.imageView_noti_close, pcloseIntent);
                    SmallViews.SetOnClickPendingIntent(Resource.Id.status_bar_collapse, pcloseIntent);

                    BigViews.SetImageViewResource(Resource.Id.imageView_noti_play, Android.Resource.Drawable.IcMediaPause);
                    BigViews.SetTextViewText(Resource.Id.textView_noti_name, songName);
                    SmallViews.SetTextViewText(Resource.Id.status_bar_track_name, songName);
                    BigViews.SetTextViewText(Resource.Id.textView_noti_artist, genresName);
                    SmallViews.SetTextViewText(Resource.Id.status_bar_artist_name, genresName);
                    BigViews.SetImageViewResource(Resource.Id.imageView_noti, Resource.Mipmap.icon);
                    SmallViews.SetImageViewResource(Resource.Id.status_bar_album_art, Resource.Mipmap.icon);
                    Notification.SetCustomContentView(SmallViews).SetCustomBigContentView(BigViews);
                }

                ShowNotification();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void UpdateNoti()
        {
            try
            {
                Task.Run(() =>
                {
                    try
                    {
                        var Item = Constant.ArrayListPlay[Constant.PlayPos];
                        string songName = WoWonderTools.GetNameFinal(Item.Publisher);
                        string genresName = "";

                        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                        {
                            Notification.SetContentTitle(songName);
                            Notification.SetContentText(genresName);
                        }
                        else
                        {
                            BigViews.SetTextViewText(Resource.Id.textView_noti_name, songName);
                            BigViews.SetTextViewText(Resource.Id.textView_noti_artist, genresName);
                            SmallViews.SetTextViewText(Resource.Id.status_bar_artist_name, genresName);
                            SmallViews.SetTextViewText(Resource.Id.status_bar_track_name, songName);
                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
                UpdateNotiPlay(Constant.Player.PlayWhenReady);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void UpdateNotiPlay(bool isPlay)
        {
            try
            {
                Task.Run(async () =>
                {
                    try
                    {
                        var Item = Constant.ArrayListPlay[Constant.PlayPos];
                        string songName = WoWonderTools.GetNameFinal(Item.Publisher);
                        string genresName = "";

                        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                        {
                            Intent playIntent = new Intent(this, typeof(PlayerService));
                            playIntent.SetAction(ActionToggle);
                            PendingIntent pPreviousIntent = PendingIntent.GetService(this, 0, playIntent, Build.VERSION.SdkInt >= BuildVersionCodes.M ? 0 | PendingIntentFlags.Immutable : 0);

                            if (isPlay)
                            {
                                if (Notification?.MActions.Count > 0)
                                    Notification.MActions[0] = new NotificationCompat.Action(Resource.Drawable.icon_pause_vector, "Pause", pPreviousIntent);
                            }
                            else
                            {
                                if (Notification?.MActions.Count > 0)
                                    Notification.MActions[0] = new NotificationCompat.Action(Resource.Drawable.icon_play_vector, "Play", pPreviousIntent);
                            }

                            if (!string.IsNullOrEmpty(songName))
                                Notification?.SetContentTitle(songName);
                            if (!string.IsNullOrEmpty(genresName))
                                Notification?.SetContentText(genresName);
                        }
                        else
                        {
                            if (isPlay)
                            {
                                BigViews.SetImageViewResource(Resource.Id.imageView_noti_play, Android.Resource.Drawable.IcMediaPause);
                            }
                            else
                            {
                                BigViews.SetImageViewResource(Resource.Id.imageView_noti_play, Android.Resource.Drawable.IcMediaPause);
                            }

                            if (!string.IsNullOrEmpty(songName))
                                BigViews.SetTextViewText(Resource.Id.textView_noti_name, songName);
                            if (!string.IsNullOrEmpty(genresName))
                                BigViews.SetTextViewText(Resource.Id.textView_noti_artist, genresName);
                            if (!string.IsNullOrEmpty(genresName))
                                SmallViews.SetTextViewText(Resource.Id.status_bar_artist_name, genresName);
                            if (!string.IsNullOrEmpty(songName))
                                SmallViews.SetTextViewText(Resource.Id.status_bar_track_name, songName);
                        }

                        var url = Constant.ArrayListPlay[Constant.PlayPos]?.Publisher?.Avatar;
                        if (!string.IsNullOrEmpty(url))
                        {
                            var bit = await BitmapUtil.GetImageBitmapFromUrl(url);
                            if (bit != null)
                                Notification?.SetLargeIcon(bit);
                        }

                        MNotificationManager.Notify(101, Notification?.Build());
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Player && Listener

        private void PauseAndReset()
        {
            try
            {
                Pause();
                if (Constant.Player != null)
                {
                    Constant.Player.SeekTo(0);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void OnCompletion()
        {
            try
            {
                if (IsReset)
                    return;

                Pause();
                RemoveNoti();

                //if (Constant.IsRepeat)
                //{
                //    Constant.Player.SeekTo(ResumeWindow, 0);
                //    Constant.Player.PlayWhenReady = true;
                //}
                //else
                //{
                //    if (Constant.IsSuffle)
                //    {
                //        Random rand = new Random();
                //        Constant.PlayPos = rand.Next((Constant.ArrayListPlay.Count - 1) + 1);
                //        SetPlayAudio();
                //    }
                //    else
                //    {
                //        SetNext();
                //    }
                //}
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        private bool IsFirstTime;
        private void OnPrepared()
        {
            try
            {
                Constant.IsPlayed = true;
                Constant.Player.PlayWhenReady = true;

                bool haveResumePosition = ResumeWindow != C.IndexUnset;
                if (haveResumePosition)
                    Constant.Player.SeekTo(ResumeWindow, ResumePosition);

                IsReset = false;

                SetBuffer(false);

                if (!IsFirstTime)
                {
                    CreateNoti();
                }

                IsFirstTime = false;
                UpdateNoti();

                if (Item != null)
                {
                    //add to Recent Played 
                    //var (apiStatus, respond) = await RequestsAsync.Tracks.GetTrackInfoAsync(Item.AudioId).ConfigureAwait(false);
                    //if (apiStatus.Equals(200))
                    //{
                    //    if (respond is GetTrackInfoObject result)
                    //    {
                    //        var data = Constant.ArrayListPlay.FirstOrDefault(a => a.Id == Item.Id);
                    //        if (data != null)
                    //        {
                    //            data = result.Data;
                    //            Item = result.Data;
                    //        }
                    //        Console.WriteLine(data);
                    //    }
                    //}
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private bool IsReset;
        private void SetPlayAudio()
        {
            try
            {
                try
                {
                    if (Constant.Player != null && Constant.Player.PlayWhenReady)
                    {
                        Constant.Player.Stop();
                        Constant.Player.Release();
                    }

                    Constant.Player = null!;
                    InitializePlayer();

                    //GC Collector
                    GC.Collect();
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }

                IsReset = true;

                Item = Constant.ArrayListPlay[Constant.PlayPos];
                if (Item == null) return;

                string filePath;
                if (!string.IsNullOrEmpty(Item.PostFileFull))
                    filePath = Item.PostFileFull;
                else
                {
                    filePath = Item.PostRecord;

                    if (!string.IsNullOrEmpty(Item.PostRecord) && !Item.PostRecord.Contains(InitializeWoWonder.WebsiteUrl))
                        filePath = WoWonderTools.GetTheFinalLink(filePath);
                }

                Uri mediaUri = Uri.Parse(filePath);

                PlayerSource = null!;
                PlayerSource = GetMediaSourceFromUrl(mediaUri, "normal");
                Constant.Player?.SetMediaSource(PlayerSource);
                Constant.Player?.Prepare();
                Constant.Player.PlayWhenReady = true;
                Constant.Player?.AddListener(PlayerListener);

                OnPrepared();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private IMediaSource GetMediaSourceFromUrl(Uri uri, string tag)
        {
            try
            {
                var httpDataSourceFactory = new DefaultHttpDataSource.Factory().SetAllowCrossProtocolRedirects(true);
                IMediaSource src = new ProgressiveMediaSource.Factory(httpDataSourceFactory).CreateMediaSource(MediaItem.FromUri(uri));
                return src;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null;
            }
        }

        private void SetBuffer(bool isBuffer)
        {
            try
            {
                if (isBuffer) return;
                GlobalContext?.RunOnUiThread(() =>
                {
                    GlobalContext?.SoundController?.SetProgress();
                    ChangePlayPauseIcon();
                    GlobalContext?.SoundController?.SeekUpdate();
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetNext()
        {
            try
            {
                if (Constant.PlayPos < Constant.ArrayListPlay.Count - 1)
                    Constant.PlayPos += 1;
                else
                    Constant.PlayPos = 0;

                SetPlayAudio();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void ChangePlayPauseIcon()
        {
            try
            {
                GlobalContext?.SoundController?.ChangePlayPauseIcon();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        [BroadcastReceiver]
        private class CallBroadcastReceiver : BroadcastReceiver
        {
            public override void OnReceive(Context context, Intent intent)
            {
                try
                {
                    string a = intent.GetStringExtra(TelephonyManager.ExtraState);
                    if (Constant.Player.PlayWhenReady)
                    {
                        if (a.Equals(TelephonyManager.ExtraStateOffhook) || a.Equals(TelephonyManager.ExtraStateRinging))
                        {
                            Constant.Player.PlayWhenReady = false;
                        }
                        else if (a.Equals(TelephonyManager.ExtraStateIdle))
                        {
                            Constant.Player.PlayWhenReady = true;
                        }
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        [BroadcastReceiver]
        private class HeadPhoneBroadcastReceiver : BroadcastReceiver
        {
            public override void OnReceive(Context context, Intent intent)
            {
                try
                {
                    //if (Constant.Player.PlayWhenReady)
                    //{
                    //    GetPlayerService()?.TogglePlay();
                    //}
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        [BroadcastReceiver]
        private class MediaButtonIntentReceiver : BroadcastReceiver
        {
            public override void OnReceive(Context context, Intent intent)
            {
                try
                {
                    string intentAction = intent.Action;
                    if (!Intent.ActionMediaButton.Equals(intentAction))
                    {
                        return;
                    }

                    Object key;
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
                    {
                        key = intent.GetParcelableExtra(Intent.ExtraKeyEvent, Class.FromType(typeof(Object)));
                    }
                    else
                    {
#pragma warning disable CS0618
                        key = intent.GetParcelableExtra(Intent.ExtraKeyEvent);
#pragma warning restore CS0618
                    }

                    if (key == null)
                    {
                        return;
                    }

                    if (key is KeyEvent keyEvent)
                    {
                        var action = keyEvent.Action;
                        if (action == KeyEventActions.Down)
                        {
                            // do something
                            if (GetPlayerService() != null)
                            {
                                Intent intentPause = new Intent(context, typeof(PlayerService));
                                intentPause.SetAction(ActionToggle);
                                ContextCompat.StartForegroundService(context, intent);
                            }
                        }
                        InvokeAbortBroadcast();
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);

                }
            }
        }

        private class MyAudioFocusChangeListener : Object, AudioManager.IOnAudioFocusChangeListener
        {
            public void OnAudioFocusChange(AudioFocus focusChange)
            {
                try
                {
                    switch (focusChange)
                    {
                        case AudioFocus.Gain:
                        case AudioFocus.LossTransientCanDuck:
                            // Resume your media player here
                            break;
                        case AudioFocus.Loss:
                        case AudioFocus.LossTransient:
                            try
                            {
                                //if (Constant.Player is {PlayWhenReady: true})
                                //{
                                //    GetPlayerService()?.TogglePlay();
                                //}
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                            break;
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        private class PlayerEvents : Object, IPlayer.IListener
        {
            private static PlayerService PlayerController;

            public PlayerEvents(PlayerService controller)
            {
                PlayerController = controller;
            }

            public void OnLoadingChanged(bool isLoading)
            {

            }

            public void OnMaxSeekToPreviousPositionChanged(long maxSeekToPreviousPositionMs)
            {

            }

            public void OnMediaItemTransition(MediaItem mediaItem, int reason)
            {

            }

            public void OnMediaMetadataChanged(MediaMetadata mediaMetadata)
            {

            }

            public void OnMetadata(Metadata metadata)
            {

            }

            public void OnPlayWhenReadyChanged(bool playWhenReady, int reason)
            {

            }

            public void OnPlaybackParametersChanged(PlaybackParameters playbackParameters)
            {

            }

            public void OnPlaybackStateChanged(int playbackState)
            {

            }

            public void OnPlaybackSuppressionReasonChanged(int playbackSuppressionReason)
            {

            }

            public void OnPlayerErrorChanged(PlaybackException error)
            {

            }

            public void OnPlayerError(PlaybackException error)
            {
                try
                {
                    Constant.Player.PlayWhenReady = false;
                    PlayerController.SetBuffer(false);
                    PlayerController.ChangePlayPauseIcon();
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public void OnPlayerStateChanged(bool playWhenReady, int playbackState)
            {
                try
                {
                    if (playbackState == IPlayer.StateEnded)
                    {
                        Constant.IsPlayed = false;
                        PlayerController.OnCompletion();
                    }
                    else if (playbackState == IPlayer.StateReady && playWhenReady)
                    {

                    }

                    //if (playWhenReady)
                    //{
                    //    PlayerController.GlobalContext?.SetWakeLock();
                    //}
                    //else
                    //{
                    //    PlayerController.GlobalContext?.OffWakeLock();
                    //}
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public void OnPlaylistMetadataChanged(MediaMetadata mediaMetadata)
            {

            }

            public void OnPositionDiscontinuity(int reason)
            {

            }

            public void OnRenderedFirstFrame()
            {

            }

            public void OnRepeatModeChanged(int repeatMode)
            {

            }

            public void OnSeekBackIncrementChanged(long seekBackIncrementMs)
            {

            }

            public void OnSeekForwardIncrementChanged(long seekForwardIncrementMs)
            {

            }

            public void OnShuffleModeEnabledChanged(bool shuffleModeEnabled)
            {

            }

            public void OnSkipSilenceEnabledChanged(bool skipSilenceEnabled)
            {

            }

            public void OnSurfaceSizeChanged(int width, int height)
            {

            }

            public void OnTimelineChanged(Timeline timeline, int reason)
            {

            }

            public void OnTrackSelectionParametersChanged(TrackSelectionParameters parameters)
            {

            }

            public void OnTracksChanged(Tracks tracks)
            {

            }

            public void OnVideoSizeChanged(VideoSize videoSize)
            {

            }

            public void OnVolumeChanged(float volume)
            {

            }

            public void OnAudioAttributesChanged(AudioAttributes audioAttributes)
            {

            }

            public void OnAudioSessionIdChanged(int audioSessionId)
            {

            }

            public void OnAvailableCommandsChanged(IPlayer.Commands availableCommands)
            {

            }

            public void OnCues(CueGroup cueGroup)
            {

            }

            public void OnDeviceInfoChanged(DeviceInfo deviceInfo)
            {

            }

            public void OnDeviceVolumeChanged(int volume, bool muted)
            {

            }

            public void OnEvents(IPlayer player, IPlayer.Events events)
            {

            }

            public void OnIsLoadingChanged(bool isLoading)
            {

            }

            public void OnIsPlayingChanged(bool isPlaying)
            {

            }
        }
    }
}