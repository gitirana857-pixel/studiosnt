using Android.App;
using Android.Content;
using Com.Onesignal;
using Com.Onesignal.Debug;
using Com.Onesignal.InAppMessages;
using Com.Onesignal.Notifications;
using Com.Onesignal.User.Subscriptions;
using Java.Util.Functions;
using Newtonsoft.Json;
using System;
using WoWonder.Activities.Communities.Groups;
using WoWonder.Activities.Communities.Pages;
using WoWonder.Activities.Events;
using WoWonder.Activities.Memories;
using WoWonder.Activities.NativePost.Pages;
using WoWonder.Activities.Tabbes;
using WoWonder.Activities.UserProfile;
using WoWonder.Helpers.MediaPlayerController;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.OneSignalNotif.Models;
using Object = Java.Lang.Object;

namespace WoWonder.Library.OneSignalNotif
{
    public class OneSignalNotification : Object, IConsumer, INotificationLifecycleListener, INotificationClickListener, IPushSubscriptionObserver, IInAppMessageLifecycleListener
    {
        //Force your app to Register Notification directly without loading it from server (For Best Result)

        private OsObject.OsNotificationObject DataNotification;

        private static volatile OneSignalNotification InstanceRenamed;
        public static OneSignalNotification Instance
        {
            get
            {
                OneSignalNotification localInstance = InstanceRenamed;
                if (localInstance == null)
                {
                    lock (typeof(OneSignalNotification))
                    {
                        localInstance = InstanceRenamed;
                        if (localInstance == null)
                        {
                            InstanceRenamed = localInstance = new OneSignalNotification();
                        }
                    }
                }
                return localInstance;

            }
        }

        public void RegisterNotificationDevice(Context context)
        {
            try
            {
                if (AppSettings.ShowNotification)
                {
                    if (!string.IsNullOrEmpty(AppSettings.OneSignalAppId) || !string.IsNullOrWhiteSpace(AppSettings.OneSignalAppId))
                    {
                        //The following options are available with increasingly more information:
                        //NONE, FATAL, ERROR, WARN, INFO, DEBUG, VERBOSE
                        OneSignal.Debug.LogLevel = LogLevel.Verbose;
                        OneSignal.Debug.AlertLevel = LogLevel.None;

                        // OneSignal Initialization  
                        OneSignal.InitWithContext(context, AppSettings.OneSignalAppId);

                        // OneSignal Methods
                        OneSignal.Notifications.RequestPermission(true, Continue.With(this));
                        OneSignal.Notifications.AddForegroundLifecycleListener(this);
                        OneSignal.Notifications.AddClickListener(this);
                        //OneSignal.Notifications.AddPermissionObserver(this);

                        OneSignal.InAppMessages.AddLifecycleListener(this);

                        OneSignal.Login(UserDetails.UserId);

                        OneSignal.ConsentRequired = true;
                        OneSignal.ConsentGiven = true;

                        OneSignal.InAppMessages.Paused = true;
                        OneSignal.Location.Shared = true;

                        OneSignal.User.PushSubscription.AddObserver(this);
                        IdsAvailable();
                    }
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        public void UnRegisterNotificationDevice()
        {
            try
            {
                OneSignal.Notifications.ClearAllNotifications();

                OneSignal.Notifications.RemoveForegroundLifecycleListener(this);
                OneSignal.Notifications.RemoveClickListener(this);

                OneSignal.User.PushSubscription.RemoveObserver(this);

                OneSignal.Logout();

                //AppSettings.ShowNotification = false;
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        public void IdsAvailable()
        {
            try
            {
                var device = OneSignal.User.PushSubscription;

                if (device != null)
                {
                    //string email = device.EmailAddress;
                    //string emailId = device.EmailUserId;
                    string pushToken = device.Token;
                    string userId = device.Id;

                    //bool enabled = device.AreNotificationsEnabled();
                    bool subscribed = device.OptedIn;
                    //bool subscribedToOneSignal = device.IsEmailSubscribed;

                    if (subscribed && !string.IsNullOrEmpty(userId))
                        UserDetails.DeviceId = userId;
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        /// <summary>
        /// NotificationWillShowInForeground
        /// Adds a listener to run before whenever a notification lifecycle event occurs.
        /// </summary>
        /// <param name="result"></param>
        public void OnWillDisplay(INotificationWillDisplayEvent result)
        {
            try
            {
                var notification = result;

                string title = notification.Notification.Title;
                string message = notification.Notification.Body;
                var additionalData = notification.Notification.AdditionalData?.ToString();
                DataNotification = JsonConvert.DeserializeObject<OsObject.OsNotificationObject>(additionalData);
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        /// <summary>
        /// NotificationOpened
        /// Adds a listener that will run whenever a notification is clicked on by the user.
        /// </summary>
        /// <param name="result"></param>
        public void OnClick(INotificationClickEvent result)
        {
            try
            {
                var notification = result;

                string title = notification.Notification.Title;
                string message = notification.Notification.Body;
                var additionalData = notification.Notification.AdditionalData?.ToString();
                DataNotification = JsonConvert.DeserializeObject<OsObject.OsNotificationObject>(additionalData);

                EventClickNotification();
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        public void EventClickNotification()
        {
            try
            {
                Constant.IsOpenNotify = true;

                Intent intent = new Intent(Application.Context, typeof(TabbedMainActivity));

                switch (DataNotification.Type)
                {
                    case "following":
                    case "visited_profile":
                    case "accepted_request":
                        {
                            intent = new Intent(Application.Context, typeof(UserProfileActivity));
                            intent.PutExtra("UserId", DataNotification.UserId);
                            break;
                        }
                    case "invited_page":
                        {
                            intent = new Intent(Application.Context, typeof(InvitedPageActivity));
                            break;
                        }
                    case "liked_page":
                    case "accepted_invite":
                        {
                            intent = new Intent(Application.Context, typeof(PageProfileActivity));
                            intent.PutExtra("PageId", DataNotification.PageId);
                            break;
                        }
                    case "joined_group":
                    case "accepted_join_request":
                    case "added_you_to_group":
                        {
                            intent = new Intent(Application.Context, typeof(GroupProfileActivity));
                            intent.PutExtra("GroupId", DataNotification.GroupId);
                            break;
                        }
                    case "comment":
                    case "wondered_post":
                    case "wondered_comment":
                    case "reaction":
                    case "wondered_reply_comment":
                    case "comment_mention":
                    case "comment_reply_mention":
                    case "liked_post":
                    case "liked_comment":
                    case "liked_reply_comment":
                    case "post_mention":
                    case "share_post":
                    case "shared_your_post":
                    case "comment_reply":
                    case "also_replied":
                    case "profile_wall_post":
                        {
                            intent = new Intent(Application.Context, typeof(ViewFullPostActivity));
                            intent.PutExtra("Id", DataNotification.PostId);
                            break;
                        }
                    case "going_event":
                        {
                            intent = new Intent(Application.Context, typeof(EventViewActivity));
                            intent.PutExtra("EventId", DataNotification.EventId);
                            break;
                        }
                    case "viewed_story":
                        {
                            intent = new Intent(Application.Context, typeof(TabbedMainActivity));
                            break;
                        }
                    case "requested_to_join_group":
                        {
                            intent = new Intent(Application.Context, typeof(JoinRequestActivity));
                            intent.PutExtra("GroupId", DataNotification.GroupId);
                            break;
                        }
                    case "memory":
                        {
                            intent = new Intent(Application.Context, typeof(MemoriesActivity));
                            break;
                        }
                    case "gift":
                        {
                            intent = new Intent(Application.Context, typeof(TabbedMainActivity));
                            break;
                        }
                    case "admin_notification":
                        {
                            intent = new Intent(Application.Context, typeof(ViewFullPostActivity));
                            intent.PutExtra("Id", DataNotification.PostId);
                            break;
                        }
                    case "live_video":
                        {
                            intent = new Intent(Application.Context, typeof(ViewFullPostActivity));
                            intent.PutExtra("Id", DataNotification.PostId);
                            break;
                        }
                    case "remaining":
                        {
                            //nothing
                            break;
                        }
                    default:
                        intent = new Intent(Application.Context, typeof(TabbedMainActivity));
                        break;
                }

                intent.PutExtra("NotificationObject", JsonConvert.SerializeObject(DataNotification));

                intent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                intent.AddFlags(ActivityFlags.SingleTop);
                intent.SetAction(Intent.ActionView);

                Application.Context.StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        /// <summary>
        /// respond to permission state change
        /// </summary>
        /// <param name="state"></param>
        public void OnPushSubscriptionChange(PushSubscriptionChangedState state)
        {
            try
            {
                if (state.Current.OptedIn && !string.IsNullOrEmpty(state.Current.Token))
                    UserDetails.DeviceId = state.Current.Token;

                IdsAvailable();
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        public void OnDidDismiss(IInAppMessageDidDismissEvent e)
        {

        }

        public void OnDidDisplay(IInAppMessageDidDisplayEvent e)
        {

        }

        public void OnWillDismiss(IInAppMessageWillDismissEvent e)
        {

        }

        public void OnWillDisplay(IInAppMessageWillDisplayEvent e)
        {

        }

        public void Accept(Object t)
        {

        }
    }
}