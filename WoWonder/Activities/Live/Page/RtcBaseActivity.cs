using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;
using WoWonder.Activities.Live.Rtc;
using WoWonder.Helpers.Utils;
using Exception = System.Exception;

namespace WoWonder.Activities.Live.Page
{
    [Activity]
    public class RtcBaseActivity : AppCompatActivity
    {
        protected LiveKitRoomManager LiveKitManager => LiveKitRoomManager.Instance;
        private bool _isConnected;

        protected EngineConfig MConfig;

        protected EngineConfig Config()
        {
            return MConfig ?? (MConfig = new EngineConfig());
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                MConfig = new EngineConfig();
                Window?.SetSoftInputMode(SoftInput.AdjustResize);
                Methods.App.FullScreenApp(this, true);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected async System.Threading.Tasks.Task JoinLiveKitRoom(string roomName, string identity, bool asPublisher)
        {
            try
            {
                var success = await LiveKitManager.JoinRoomAsync(
                    AppSettings.LiveKitUrl,
                    LiveKitManager.CurrentToken ?? "",
                    roomName,
                    identity
                );
                _isConnected = success;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected void LeaveLiveKitRoom()
        {
            try
            {
                _ = LiveKitManager.DisconnectAsync();
                _isConnected = false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected void SetAudioEnabled(bool enabled)
        {
            LiveKitManager.SetAudioEnabled(enabled);
        }

        protected void SetVideoEnabled(bool enabled)
        {
            LiveKitManager.SetVideoEnabled(enabled);
        }

        protected void SwitchCamera()
        {
            LiveKitManager.SwitchCamera();
        }

        protected void ToggleBeautyEffect(bool enabled)
        {
            LiveKitManager.ToggleBeautyEffect(enabled);
        }

        protected override void OnDestroy()
        {
            try
            {
                LeaveLiveKitRoom();
                base.OnDestroy();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}
