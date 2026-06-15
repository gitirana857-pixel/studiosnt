using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using AndroidX.Core.Content;
using IO.Agora.Rtc2;
using System;
using WoWonder.Activities.Live.Page;
using WoWonder.Activities.SettingsPreferences;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;

namespace WoWonder.Activities.Live.Utils
{
    public class LiveUtil
    {
        private readonly Activity Activity;
        public LiveUtil(Activity activity)
        {
            try
            {
                Activity = activity;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Live

        //Go Live
        public void GoLiveOnClick()
        {
            try
            {
                switch ((int)Build.VERSION.SdkInt)
                {
                    // Check if we're running on Android 5.0 or higher
                    case < 23:
                        OpenDialogLive();
                        break;
                    default:
                        {
                            if (PermissionsController.CheckPermissionStorage(Activity) &&
                                ContextCompat.CheckSelfPermission(Activity, Manifest.Permission.Camera) == Permission.Granted &&
                                ContextCompat.CheckSelfPermission(Activity, Manifest.Permission.RecordAudio) == Permission.Granted &&
                                ContextCompat.CheckSelfPermission(Activity, Manifest.Permission.ModifyAudioSettings) == Permission.Granted &&
                                ContextCompat.CheckSelfPermission(Activity, Manifest.Permission.ReadPhoneState) == Permission.Granted &&
                                ContextCompat.CheckSelfPermission(Activity, Manifest.Permission.BluetoothConnect) == Permission.Granted)
                            {
                                OpenDialogLive();
                            }
                            else
                            {
                                new PermissionsController(Activity).RequestPermission(111);
                            }

                            break;
                        }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OpenDialogLive()
        {
            try
            {
                var ugcPrivacyPerf = MainSettings.UgcPrivacy?.GetBoolean("UgcPrivacy_key", false) ?? false;
                if (ugcPrivacyPerf)
                {
                    OpenLive();
                }
                else
                {
                    new UgcPrivacyDialog(Activity).DisplayPrivacyDialog();
                }

                //var dialog = new MaterialAlertDialogBuilder(this);
                //dialog.SetTitle(GetText(Resource.String.Lbl_CreateLiveVideo));
                //dialog.Input(Resource.String.Lbl_AddLiveVideoContext, 0, false, (materialDialog, s) =>
                //{
                //    try
                //    {

                //    }
                //    catch (Exception e)
                //    {
                //        Methods.DisplayReportResultTrack(e);
                //    }
                //});
                //dialog.InputType(InputTypes.TextFlagImeMultiLine);
                //dialog.SetPositiveButton(GetText(Resource.String.Lbl_Go_Live), new MaterialDialogUtils());
                //dialog.SetNegativeButton(GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());
                //dialog.;
                //dialog.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OpenLive()
        {
            try
            {
                var streamName = "stream_" + UserDetails.UserId + "_" + Methods.Time.CurrentTimeMillis();
                if (string.IsNullOrEmpty(streamName) || string.IsNullOrWhiteSpace(streamName))
                {
                    ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_PleaseEnterLiveStreamName), ToastLength.Short);
                    return;
                }
                //Owner >> ClientRoleBroadcaster , Users >> ClientRoleAudience
                Intent intent = new Intent(Activity, typeof(LiveStreamingActivity));
                intent.PutExtra(LiveConstants.KeyClientRole, Constants.ClientRoleBroadcaster);
                intent.PutExtra("StreamName", streamName);
                Activity.StartActivity(intent);

                //var dialog = new MaterialAlertDialogBuilder(this);
                //dialog.SetTitle(GetText(Resource.String.Lbl_CreateLiveVideo));
                //dialog.Input(Resource.String.Lbl_AddLiveVideoContext, 0, false, (materialDialog, s) =>
                //{
                //    try
                //    {

                //    }
                //    catch (Exception e)
                //    {
                //        Methods.DisplayReportResultTrack(e);
                //    }
                //});
                //dialog.InputType(InputTypes.TextFlagImeMultiLine);
                //dialog.SetPositiveButton(GetText(Resource.String.Lbl_Go_Live), new MaterialDialogUtils());
                //dialog.SetNegativeButton(GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());
                //dialog.;
                //dialog.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion
    }
}