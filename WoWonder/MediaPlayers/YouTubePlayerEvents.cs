using Android.OS;
using Android.Widget;
using AndroidX.Lifecycle;
using Anjo.Android.YouTubePlayerX.Player;
using System;
using WoWonder.Activities.NativePost.Pages;
using WoWonder.Activities.ReelsVideo;
using WoWonder.Activities.Videos;
using WoWonder.Helpers.Utils;

namespace WoWonder.MediaPlayers
{
    public class YouTubePlayerEvents : IYouTubePlayerListener
    {
        private readonly IYouTubePlayer PlayerView;
        private readonly string VideoIdYoutube;
        public bool IsPlaying;
        public int CurrentSecond, Duration;
        public string Type;

        public YouTubePlayerEvents(IYouTubePlayer playerView, string videoId, string type = "Normal", int currentSecond = 0)
        {
            PlayerView = playerView;
            VideoIdYoutube = videoId;
            Type = type;
            CurrentSecond = currentSecond;
        }

        public void OnReady()
        {
            try
            {
                if (Type == "FullScreen")
                {
                    if (YouTubePlayerFullScreenActivity.Instance.Lifecycle.CurrentState == Lifecycle.State.Resumed)
                        PlayerView.LoadVideo(VideoIdYoutube, CurrentSecond);
                    else
                        PlayerView.CueVideo(VideoIdYoutube, CurrentSecond);
                }
                else if (Type == "YoutubePlayerActivity")
                {
                    if (YoutubePlayerActivity.Instance.Lifecycle.CurrentState == Lifecycle.State.Resumed)
                        PlayerView.LoadVideo(VideoIdYoutube, CurrentSecond);
                    else
                        PlayerView.CueVideo(VideoIdYoutube, CurrentSecond);
                }
                else if (Type == "VideoViewerActivity")
                {
                    if (VideoViewerActivity.GetInstance().Lifecycle.CurrentState == Lifecycle.State.Resumed)
                        PlayerView.LoadVideo(VideoIdYoutube, CurrentSecond);
                    else
                        PlayerView.CueVideo(VideoIdYoutube, CurrentSecond);
                }
                else if (Type == "ReelsVideo")
                {
                    if (ViewReelsVideoFragment.GetInstance().Lifecycle.CurrentState == Lifecycle.State.Resumed)
                        PlayerView.LoadVideo(VideoIdYoutube, CurrentSecond);
                    else
                        PlayerView.CueVideo(VideoIdYoutube, CurrentSecond);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnStateChange(int state)
        {
            try
            {
                if (state == PlayerConstants.PlayerState.Ended)
                {
                    //if (Type != "FullScreen")
                    //    OnVideoEnded();

                    IsPlaying = false;
                }
                else if (state == PlayerConstants.PlayerState.Paused)
                {
                    IsPlaying = false;
                }
                else if (state == PlayerConstants.PlayerState.Playing)
                {
                    IsPlaying = true;
                }

                //var mainActivity = TabbedMainActivity.GetInstance();
                //if (mainActivity?.VideoActionsController != null)
                //{
                //    mainActivity.VideoActionsController.LoadingProgressBar.Visibility = ViewStates.Invisible;
                //}

                //var globalPlayerActivity = GlobalPlayerActivity.GetInstance();
                //if (globalPlayerActivity?.VideoActionsController != null)
                //{
                //    globalPlayerActivity.VideoActionsController.LoadingProgressBar.Visibility = ViewStates.Invisible;
                //}

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnPlaybackQualityChange(string playbackQuality)
        {

        }

        public void OnPlaybackRateChange(string playbackRate)
        {

        }

        public void OnError(int error)
        {
            IsPlaying = false;
        }

        public void OnApiChange()
        {

        }

        public void OnCurrentSecond(int second)
        {
            CurrentSecond = second;
        }

        public void OnVideoDuration(int duration)
        {
            Duration = duration;
        }

        public void OnVideoLoadedFraction(int loadedFraction)
        {

        }

        public void OnVideoId(string videoId)
        {

        }

        public void OnVideoEnded()
        {
            try
            {
                //if (ListUtils.ArrayListPlay.Count > 0 && UserDetails.AutoNext)
                //{
                //    var data = ListUtils.ArrayListPlay.FirstOrDefault();
                //    if (data != null)
                //    {
                //        ListUtils.LessonList.Add(data);
                //        TabbedMainActivity.GetInstance()?.StartPlayVideo(data);
                //    }
                //}
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void BtnForwardOnClick(string page)
        {
            try
            {
                if (page == "YoutubePlayerActivity")
                {
                    var mainActivity = YoutubePlayerActivity.Instance;
                    if (mainActivity == null)
                        return;

                    if (ForwardPressed)
                    {
                        PressedHandler.RemoveCallbacks(() => { ForwardPressed = false; });
                        ForwardPressed = false;

                        //Add event
                        var fTime = 10; // 10 Sec
                        if (mainActivity.YoutubePlayer != null)
                        {
                            int eTime = Duration;
                            int sTime = CurrentSecond;
                            if (sTime + fTime <= eTime)
                            {
                                sTime += fTime;
                                mainActivity.YoutubePlayer.SeekTo(sTime);

                                if (!mainActivity.YouTubePlayerEvents.IsPlaying)
                                    mainActivity.YoutubePlayer.Play();
                            }
                            else
                            {
                                Toast.MakeText(mainActivity, mainActivity.GetText(Resource.String.Lbl_ErrorForward), ToastLength.Short)?.Show();
                            }
                        }
                    }
                    else
                    {
                        ForwardPressed = true;
                        PressedHandler.PostDelayed(() => { ForwardPressed = false; }, 2000L);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private bool BackwardPressed, ForwardPressed;
        private readonly Handler PressedHandler = new Handler(Looper.MainLooper);

        public void BtnBackwardOnClick(string page)
        {
            try
            {
                if (page == "YoutubePlayerActivity")
                {
                    var mainActivity = YoutubePlayerActivity.Instance;
                    if (mainActivity == null)
                        return;

                    if (BackwardPressed)
                    {
                        PressedHandler.RemoveCallbacks(() => { BackwardPressed = false; });
                        BackwardPressed = false;

                        //Add event
                        var bTime = 10; // 10 Sec
                        if (mainActivity.YoutubePlayer != null)
                        {
                            var sTime = CurrentSecond;

                            if (sTime - bTime > 0)
                            {
                                sTime -= bTime;
                                mainActivity.YoutubePlayer.SeekTo(sTime);

                                if (!mainActivity.YouTubePlayerEvents.IsPlaying)
                                    mainActivity.YoutubePlayer.Play();
                            }
                            else
                            {
                                Toast.MakeText(mainActivity, mainActivity.GetText(Resource.String.Lbl_ErrorBackward), ToastLength.Short)?.Show();
                            }
                        }
                    }
                    else
                    {
                        BackwardPressed = true;
                        PressedHandler.PostDelayed(() => { BackwardPressed = false; }, 2000L);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void BtnPreviousOnClick()
        {
            try
            {
                //if (ListUtils.LessonList.Count > 0)
                //{
                //    var data = ListUtils.LessonList.LastOrDefault();
                //    if (data != null)
                //    {
                //        TabbedMainActivity.GetInstance()?.StartPlayVideo(data);
                //        ListUtils.LessonList.Remove(data);
                //    }
                //}
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void BtnNextOnClick()
        {
            try
            {
                //if (ListUtils.ArrayListPlay.Count > 0 && UserDetails.AutoNext)
                //{
                //    var data = ListUtils.ArrayListPlay.FirstOrDefault();
                //    if (data != null)
                //    {
                //        ListUtils.LessonList.Add(data);
                //        TabbedMainActivity.GetInstance()?.StartPlayVideo(data);
                //    }
                //} 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

    }
}