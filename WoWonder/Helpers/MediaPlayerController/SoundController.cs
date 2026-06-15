using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using AndroidX.Core.Content;
using System;
using System.Collections.ObjectModel;
using System.Timers;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Activities.Tabbes;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Posts;
using Exception = System.Exception;
using Object = Java.Lang.Object;

namespace WoWonder.Helpers.MediaPlayerController
{
    public class SoundController : Object, SeekBar.IOnSeekBarChangeListener
    {
        #region Variables Basic

        private LinearLayout ViewStubFullScreenPlayer;
        private ImageView ImageArtist, PlayButton, CloseButton;
        private TextView NameArtist;

        private Timer Timer;
        private readonly Activity ActivityContext;
        private readonly TabbedMainActivity GlobalContext;
        private AdapterHolders.SoundPostViewHolder SoundPostViewHolder;
        private NativeFeedType NativeFeedType;
        public string PostId;

        #endregion

        #region General

        public SoundController(Activity activity)
        {
            try
            {
                ActivityContext = activity;
                GlobalContext = TabbedMainActivity.GetInstance();

                PlayerService.ActionSeekTo = ActivityContext.PackageName + ".action.ACTION_SEEK_TO";
                PlayerService.ActionPlay = ActivityContext.PackageName + ".action.ACTION_PLAY";
                PlayerService.ActionPause = ActivityContext.PackageName + ".action.PAUSE";
                PlayerService.ActionStop = ActivityContext.PackageName + ".action.STOP";
                PlayerService.ActionSkip = ActivityContext.PackageName + ".action.SKIP";
                PlayerService.ActionRewind = ActivityContext.PackageName + ".action.REWIND";
                PlayerService.ActionToggle = ActivityContext.PackageName + ".action.ACTION_TOGGLE";
                PlayerService.ActionBackward = ActivityContext.PackageName + ".action.ACTION_BACKWARD";
                PlayerService.ActionForward = ActivityContext.PackageName + ".action.ACTION_FORWARD";
                PlayerService.ActionPlaybackSpeed = ActivityContext.PackageName + ".action.ACTION_PLAYBACK_SPEED";
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void InitializeUi()
        {
            try
            {
                ViewStubFullScreenPlayer = GlobalContext.FindViewById<LinearLayout>(Resource.Id.viewStubFullScreenPlayer);
                ImageArtist = GlobalContext.FindViewById<ImageView>(Resource.Id.image_artist);
                NameArtist = GlobalContext.FindViewById<TextView>(Resource.Id.name_artist);
                PlayButton = GlobalContext.FindViewById<ImageView>(Resource.Id.play_button);
                CloseButton = GlobalContext.FindViewById<ImageView>(Resource.Id.close_button);

                PlayButton.Click += PlayButtonOnClick;
                CloseButton.Click += CloseButtonOnClick;

                ViewStubFullScreenPlayer.Visibility = ViewStates.Gone;
                ViewStubFullScreenPlayer.Tag = "Close";
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void Destroy()
        {
            try
            {
                if (Timer != null)
                {
                    Timer?.Stop();
                    Timer?.Dispose();
                }

                if (NativeFeedType == NativeFeedType.Global && ViewStubFullScreenPlayer.Visibility == ViewStates.Visible)
                {
                    Animation animation = new TranslateAnimation(0, 0, 0, ViewStubFullScreenPlayer.Top + ViewStubFullScreenPlayer.Height);
                    animation.Duration = 300;
                    animation.AnimationEnd += (o, args) =>
                    {
                        try
                        {
                            ViewStubFullScreenPlayer.Visibility = ViewStates.Gone;
                        }
                        catch (Exception exception)
                        {
                            Methods.DisplayReportResultTrack(exception);
                        }
                    };
                    ViewStubFullScreenPlayer.StartAnimation(animation);
                }

                ViewStubFullScreenPlayer.Tag = "Close";

                if (SoundPostViewHolder != null)
                {
                    SoundPostViewHolder.PlayButton.SetImageResource(Resource.Drawable.icon_play_vector);
                    SoundPostViewHolder.PlayButton.Tag = "Stop";

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                    {
                        SoundPostViewHolder.SeekBar.SetProgress(0, true);
                    }
                    else
                    {
                        // For API < 24 
                        SoundPostViewHolder.SeekBar.Progress = 0;
                    }
                }

                PostId = "";
                Constant.ArrayListPlay = new ObservableCollection<PostDataObject>();

                if (Constant.Player == null)
                    return;

                if (Constant.Player.PlayWhenReady)
                {
                    Intent intent = new Intent(ActivityContext, typeof(PlayerService));
                    intent.SetAction(PlayerService.ActionStop);
                    ContextCompat.StartForegroundService(GlobalContext, intent);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event Click

        private void CloseButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                Intent intent = new Intent(ActivityContext, typeof(PlayerService));
                intent.SetAction(PlayerService.ActionStop);
                ContextCompat.StartForegroundService(GlobalContext, intent);

                if (SoundPostViewHolder != null)
                {
                    SoundPostViewHolder.PlayButton.SetImageResource(Resource.Drawable.icon_play_vector);
                    SoundPostViewHolder.PlayButton.Tag = "Play";

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                    {
                        SoundPostViewHolder.SeekBar.SetProgress(0, true);
                    }
                    else
                    {
                        try
                        {
                            // For API < 24 
                            SoundPostViewHolder.SeekBar.Progress = 0;
                        }
                        catch (Exception exception)
                        {
                            Methods.DisplayReportResultTrack(exception);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void PlayButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                StartOrPausePlayer();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Fun >> SeekBar

        // set Progress bar values
        public void SetProgress()
        {
            try
            {
                // Run timer
                Timer = new Timer
                {
                    Interval = 1000
                };
                Timer.Elapsed += TimerOnElapsed;

                if (SoundPostViewHolder != null)
                {
                    SoundPostViewHolder.SeekBar.Max = MusicUtils.MaxProgress;
                    SoundPostViewHolder.SeekBar.SetOnSeekBarChangeListener(this);

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                    {
                        SoundPostViewHolder.SeekBar.SetProgress(0, true);
                    }
                    else
                    {
                        try
                        {
                            // For API < 24 
                            SoundPostViewHolder.SeekBar.Progress = 0;
                        }
                        catch (Exception exception)
                        {
                            Methods.DisplayReportResultTrack(exception);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnProgressChanged(SeekBar seekBar, int progress, bool fromUser)
        {

        }

        public void OnStartTrackingTouch(SeekBar seekBar)
        {
            try
            {
                if (Timer != null)
                {
                    // remove message Handler from updating progress bar
                    Timer.Enabled = false;
                    Timer.Stop();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnStopTrackingTouch(SeekBar seekBar)
        {
            try
            {
                if (Timer != null)
                {
                    Timer.Enabled = false;
                    Timer.Stop();
                }

                long progress = seekBar.Progress;

                Intent intent = new Intent(ActivityContext, typeof(PlayerService));
                intent.SetAction(PlayerService.ActionSeekTo);
                intent.PutExtra("seekTo", progress);
                ContextCompat.StartForegroundService(GlobalContext, intent);

                if (Timer != null)
                {
                    // update timer progress again
                    Timer.Enabled = true;
                    Timer.Start();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Fun Player

        public void StartPlaySound(GlobalClickEventArgs args, bool reset = false)
        {
            try
            {
                if (reset && SoundPostViewHolder != null)
                {
                    SoundPostViewHolder.PlayButton.SetImageResource(Resource.Drawable.icon_play_vector);
                    SoundPostViewHolder.PlayButton.Tag = "Stop";

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                    {
                        SoundPostViewHolder.SeekBar.SetProgress(0, true);
                    }
                    else
                    {
                        // For API < 24 
                        SoundPostViewHolder.SeekBar.Progress = 0;
                    }
                }

                SoundPostViewHolder = args.HolderSound;
                NativeFeedType = args.NativeFeedType;

                if (args.NewsFeedClass != null)
                {
                    Constant.IsPlayed = false;
                    Constant.IsOnline = true;
                    Constant.ArrayListPlay = new ObservableCollection<PostDataObject> { args.NewsFeedClass };
                    PostId = args.NewsFeedClass.Id;

                    LoadSoundData(args.NewsFeedClass);

                    ReleaseSound();

                    StartOrPausePlayer();

                    //Play Song  
                    if (NativeFeedType == NativeFeedType.Global && ViewStubFullScreenPlayer != null && ViewStubFullScreenPlayer.Tag?.ToString() != "Open")
                    {
                        ViewStubFullScreenPlayer.Visibility = ViewStates.Visible;
                        var animation = new TranslateAnimation(0, 0, ViewStubFullScreenPlayer.Height, 0) { Duration = 300 };

                        ViewStubFullScreenPlayer.StartAnimation(animation);

                        ViewStubFullScreenPlayer.Tag = "Open";
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        //Forward 15 Sec
        public void BtnForwardOnClick()
        {
            try
            {
                if (Timer != null)
                {
                    Timer.Enabled = false;
                    Timer.Stop();
                }

                Intent intent = new Intent(ActivityContext, typeof(PlayerService));
                intent.SetAction(PlayerService.ActionForward);
                ContextCompat.StartForegroundService(GlobalContext, intent);

                if (Timer != null)
                {
                    // update timer progress again
                    Timer.Enabled = true;
                    Timer.Start();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Backward 15 Sec
        public void BtnBackwardOnClick()
        {
            try
            {
                if (Timer != null)
                {
                    Timer.Enabled = false;
                    Timer.Stop();
                }

                Intent intent = new Intent(ActivityContext, typeof(PlayerService));
                intent.SetAction(PlayerService.ActionBackward);
                ContextCompat.StartForegroundService(GlobalContext, intent);

                if (Timer != null)
                {
                    // update timer progress again
                    Timer.Enabled = true;
                    Timer.Start();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        public void ReleaseSound()
        {
            try
            {
                if (Constant.Player != null)
                {
                    if (Constant.Player.PlayWhenReady)
                    {
                        Constant.Player.Stop();
                        Constant.Player.Release();
                        Constant.Player = null!;
                    }
                }

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void StartOrPausePlayer()
        {
            try
            {
                if (Constant.ArrayListPlay.Count > 0)
                {
                    if (Constant.ArrayListPlay.Count == 1)
                        Constant.PlayPos = 0;

                    var item = Constant.ArrayListPlay[Constant.PlayPos];
                    if (item == null) return;

                    Intent intent = new Intent(ActivityContext, typeof(PlayerService));
                    if (Constant.IsPlayed)
                    {
                        if (Constant.Player.PlayWhenReady)
                        {
                            item.IsPlay = false;
                            intent.SetAction(PlayerService.ActionPause);
                            ContextCompat.StartForegroundService(GlobalContext, intent);
                        }
                        else
                        {
                            if (!Constant.IsOnline || Methods.CheckConnectivity())
                            {
                                item.IsPlay = true;
                                intent.SetAction(PlayerService.ActionPlay);
                                ContextCompat.StartForegroundService(GlobalContext, intent);
                            }
                            else
                            {
                                Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                            }
                        }
                    }
                    else
                    {
                        if (!Constant.IsOnline || Methods.CheckConnectivity())
                        {
                            item.IsPlay = true;
                            intent.SetAction(PlayerService.ActionPlay);
                            ContextCompat.StartForegroundService(GlobalContext, intent);
                        }
                        else
                        {
                            Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                        }
                    }
                }
                //Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_NoSongSelected), ToastLength.Long)?.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void LoadSoundData(PostDataObject data)
        {
            try
            {
                GlobalContext?.RunOnUiThread(() =>
                {
                    try
                    {
                        GlideImageLoader.LoadImage(ActivityContext, data.Publisher.Avatar, ImageArtist, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);
                        NameArtist.Text = WoWonderTools.GetNameFinal(data.Publisher);
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

        public void ChangePlayPauseIcon()
        {
            try
            {
                GlobalContext?.RunOnUiThread(() =>
                {
                    // check for already playing
                    if (Constant.Player != null && Constant.Player.PlayWhenReady)
                    {
                        // Changing button image to pause button
                        PlayButton.SetImageResource(Resource.Drawable.icon_pause_vector);
                        if (SoundPostViewHolder != null)
                        {
                            SoundPostViewHolder.PlayButton.SetImageResource(Resource.Drawable.icon_pause_vector);
                            SoundPostViewHolder.PlayButton.Tag = "Pause";
                        }

                        if (Timer != null)
                        {
                            Timer.Enabled = true;
                            Timer.Start();
                        }
                    }
                    else
                    {
                        // Changing button image to play button
                        PlayButton.SetImageResource(Resource.Drawable.icon_play_vector);
                        if (SoundPostViewHolder != null)
                        {
                            SoundPostViewHolder.PlayButton.SetImageResource(Resource.Drawable.icon_play_vector);
                            SoundPostViewHolder.PlayButton.Tag = "Play";
                        }

                        if (Timer != null)
                        {
                            Timer.Enabled = false;
                            Timer.Stop();
                        }
                    }
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SeekUpdate()
        {
            try
            {
                ActivityContext?.RunOnUiThread(() =>
                {
                    try
                    {
                        if (Constant.Player == null)
                            return;

                        var totalDuration = Constant.Player.Duration;
                        var currentDuration = Constant.Player.CurrentPosition;

                        // Displaying Total Duration time
                        if (SoundPostViewHolder != null)
                        {
                            SoundPostViewHolder.TotalDuration.Text = MusicUtils.MilliSecondsToTimer(totalDuration);
                            // Displaying time completed playing
                            SoundPostViewHolder.CurrentDuration.Text = MusicUtils.MilliSecondsToTimer(currentDuration);

                            // Updating progress bar
                            int progress = MusicUtils.GetProgressSeekBar(currentDuration, totalDuration);

                            if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                            {
                                SoundPostViewHolder.SeekBar.SetProgress(progress, true);
                            }
                            else
                            {
                                // For API < 24 
                                SoundPostViewHolder.SeekBar.Progress = progress;
                            }
                        }

                        if (currentDuration >= totalDuration)
                        {
                            if (Constant.IsRepeat)
                            {
                                Constant.Player.SeekTo(0);
                                Constant.Player.PlayWhenReady = true;
                            }
                        }
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

        #region Runnable

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                //Running this thread after 10 milliseconds
                SeekUpdate();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

    }
}