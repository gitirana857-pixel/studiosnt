using Android.Views;
using Android.Widget;
using Androidx.Media3.Common;
using Androidx.Media3.Common.Text;
using Androidx.Media3.UI;
using System;
using WoWonder.Activities.ReelsVideo;
using WoWonder.Activities.Tabbes;
using WoWonder.Helpers.Utils;
using WoWonder.MediaPlayers.Exo;
using Object = Java.Lang.Object;

namespace WoWonder.MediaPlayers
{
    public class PlayerEvents : Object, IPlayer.IListener, PlayerView.IControllerVisibilityListener, PlayerControlView.IProgressUpdateListener
    {
        private readonly ExoController ExoController;
        private readonly ImageButton VideoPlayButton;

        public PlayerEvents(ExoController exoController, PlayerControlView controlView)
        {
            try
            {
                ExoController = exoController;

                if (controlView != null)
                {
                    if (ExoController.Page == "Post")
                    {

                    }
                    else
                    {
                        VideoPlayButton = controlView.FindViewById<ImageButton>(Resource.Id.exo_play_pause);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
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
            try
            {
                if (isPlaying)
                    ExoController.OnPlay();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnLoadingChanged(bool p0)
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

        public void OnPlaybackParametersChanged(PlaybackParameters p0)
        {

        }

        public void OnPlaybackStateChanged(int playbackState)
        {

        }

        public void OnPlaybackSuppressionReasonChanged(int playbackSuppressionReason)
        {

        }

        public void OnPlayerError(PlaybackException error)
        {

        }

        public void OnPlayerErrorChanged(PlaybackException error)
        {

        }

        public void OnPlayerStateChanged(bool playWhenReady, int playbackState)
        {
            try
            {
                if (playbackState == IPlayer.StateEnded || playbackState == IPlayer.StateIdle && !playWhenReady)
                {
                    TabbedMainActivity.GetInstance().SetOffWakeLock();
                    ExoController.ToggleExoPlayerKeepScreenOnFeature(false);

                    if (playbackState == IPlayer.StateEnded)
                    {
                        if (ExoController.Page == "ReelsVideo")
                        {
                            ViewReelsVideoFragment.GetInstance()?.ExoController?.Repeat();
                        }
                        // OnVideoEnded();
                    }
                }
                else
                {
                    // STATE_IDLE, STATE_ENDED
                    // This prevents the screen from getting dim/lock
                    TabbedMainActivity.GetInstance().SetOnWakeLock();
                    ExoController.ToggleExoPlayerKeepScreenOnFeature(true);
                }

                if (ExoController.Page == "Post")
                {
                    switch (playbackState)
                    {
                        case IPlayer.StateEnded:
                            {
                                switch (playWhenReady)
                                {
                                    case false:
                                        // VideoResumeButton.Visibility = ViewStates.Visible;
                                        break;
                                }

                                ExoController.BufferProgressControl.Visibility = ViewStates.Visible;
                                break;
                            }
                        case IPlayer.StateReady:
                            {
                                switch (playWhenReady)
                                {
                                    case false:
                                        //VideoResumeButton.Visibility = ViewStates.Gone;
                                        //VideoPlayButton.Visibility = ViewStates.Visible;
                                        break;
                                    default:
                                        ExoController.BufferProgressControl.Visibility = ViewStates.Gone;
                                        ExoController.Equalizer.Visibility = ViewStates.Visible;
                                        ExoController.Equalizer.AnimateBars();

                                        break;
                                }
                                break;
                            }
                        case IPlayer.StateBuffering:
                            ExoController.BufferProgressControl.Visibility = ViewStates.Visible;
                            ExoController.Equalizer.Visibility = ViewStates.Gone;
                            ExoController.Equalizer.StopBars();
                            //VideoPlayButton.Visibility = ViewStates.Invisible;
                            //LoadingProgressBar.Visibility = ViewStates.Visible;
                            //VideoResumeButton.Visibility = ViewStates.Invisible;
                            break;
                    }
                }
                else
                {
                    if (VideoPlayButton == null)
                        return;

                    switch (playbackState)
                    {
                        case IPlayer.StateEnded:
                            {
                                switch (playWhenReady)
                                {
                                    case false:
                                        VideoPlayButton.SetImageResource(Resource.Drawable.exo_icon_pause);
                                        break;
                                    default:
                                        VideoPlayButton.SetImageResource(Resource.Drawable.exo_icon_play);
                                        break;
                                }
                                VideoPlayButton.Visibility = ViewStates.Visible;
                                break;
                            }
                        case IPlayer.StateReady:
                            {
                                switch (playWhenReady)
                                {
                                    case false:
                                        VideoPlayButton.SetImageResource(Resource.Drawable.exo_icon_play);
                                        break;
                                    default:
                                        VideoPlayButton.SetImageResource(Resource.Drawable.exo_icon_pause);
                                        break;
                                }
                                VideoPlayButton.Visibility = ViewStates.Visible;
                                break;
                            }
                        case IPlayer.StateBuffering:
                            VideoPlayButton.Visibility = ViewStates.Invisible;
                            break;
                    }

                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnPlaylistMetadataChanged(MediaMetadata mediaMetadata)
        {

        }

        public void OnPositionDiscontinuity(int p0)
        {

        }

        public void OnRenderedFirstFrame()
        {

        }

        public void OnRepeatModeChanged(int p0)
        {

        }

        public void OnSeekBackIncrementChanged(long seekBackIncrementMs)
        {

        }

        public void OnSeekForwardIncrementChanged(long seekForwardIncrementMs)
        {

        }

        public void OnShuffleModeEnabledChanged(bool p0)
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

        public void OnProgressUpdate(long position, long bufferedPosition)
        {

        }

        public void OnVisibilityChanged(int visibility)
        {

        }
    }
}