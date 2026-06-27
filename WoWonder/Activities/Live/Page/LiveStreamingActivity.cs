using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.ConstraintLayout.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide.Util;
using Com.Aghajari.Emojiview.View;
using DE.Hdodenhof.CircleImageViewLib;
using Google.Android.Material.Dialog;
using Java.Lang;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using WoWonder.Activities.Base;
using WoWonder.Activities.Comment.Adapters;
using WoWonder.Activities.Live.Adapters;
using WoWonder.Activities.Live.Rtc;
using WoWonder.Activities.Live.Ui;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Activities.NativePost.Share;
using WoWonder.Activities.Tabbes;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using WoWonder.StickersView;
using WoWonderClient;
using WoWonderClient.Classes.Comments;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Requests;
using Exception = System.Exception;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.Live.Page
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class LiveStreamingActivity : RtcBaseActivity, IDialogListCallBack
    {
        #region Variables Basic

        private FrameLayout RootView, MVideoControlLyt, MLiveStreamEndedLyt;
        private ViewStub MHeaderViewStub, MFooterViewStub;
        private ConstraintLayout MGetReadyLyt, MLoadingViewer;
        private ImageView MAvatarBg, MCloseIn, MCloseOut, MCloseStreaming;
        private CircleImageView MAvatar;
        private TextView MTimeText, MViewersText;

        ////////////// Comments ///////////// 
        private ImageView MEmojisIconBtn, MMoreBtn, MShareBtn;
        private TextView MCameraBtn, MEffectBtn, MVideoEnabledBtn, MAudioEnabledBtn;
        private AXEmojiEditText TxtComment;
        private ImageView MSendBtn;
        private LinearLayoutManager LayoutManager;
        private RecyclerView MRecycler;
        private LiveMessageAdapter MAdapter;
        private Timer TimerComments;

        ////////////////////////////////
        private PostDataObject LiveStreamViewerObject;
        private PostDataObject PostObject;
        private string PostId, MStreamChannel;
        private bool IsOwner, IsStreamingStarted;
        private int Role;

        //////////////////////////////// 
        private VideoGridContainer MVideoLayout;
        private SurfaceView SurfaceView;
        private int MVideoWidth = 640;
        private int MVideoHeight = 360;

        ////////////////////////////////
        private ImageView BgAvatar, CloseEnded;
        private CircleImageView StreamRateLevel;
        private TextView Header, ShareStreamText, Comments, Viewers, Duration;
        private AppCompatButton GoLiveButton;
        private LinearLayout InfoLiveLayout;

        //////////////////////////////// 
        private bool IsStreamingTimeInitialed;
        private Handler CustomHandler;
        private MyRunnable UpdateTimerThread;
        private long StartTime;
        private long TimeInMilliseconds;
        private long TimeSwapBuff;
        private long UpdatedTime;

        ////////////////////////////////
        private string UidLive, ResourceId, SId, FileListLive;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                MStreamChannel = Intent?.GetStringExtra("StreamName") ?? "";
                Config().SetChannelName(MStreamChannel);

                base.OnCreate(savedInstanceState);

                // Create your application here
                SetContentView(Resource.Layout.LiveStreamingLayout);

                PostId = Intent?.GetStringExtra("PostId") ?? "";
                var audience = LiveConstants.ClientRoleAudience;
                Role = Intent?.GetIntExtra(LiveConstants.KeyClientRole, audience) ?? audience;
                IsOwner = Role == LiveConstants.ClientRoleBroadcaster;  //Owner >> ClientRoleBroadcaster , Users >> ClientRoleAudience

                switch (IsOwner)
                {
                    case false:
                        LiveStreamViewerObject = JsonConvert.DeserializeObject<PostDataObject>(Intent?.GetStringExtra("PostLiveStream") ?? ""); break;
                }

                //Get Value And Set Toolbar
                InitComponent();
                SetRecyclerViewAdapters();
                InitBackPressed();
                InitLiveKit();
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
                StartTimerComment();
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
                StopTimerComment();
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
                base.OnTrimMemory(level);
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
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
                base.OnLowMemory();
                GC.Collect(GC.MaxGeneration);
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
                DestroyBasic();
                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    BackPressed();
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region BackPressed && Close Live

        public void BackPressed()
        {
            try
            {
                switch (IsOwner)
                {
                    case true when IsStreamingStarted:
                        SetupFinishAsk(true);
                        break;
                    case true:
                        DeleteLiveStream();
                        Finish();
                        break;
                    default:
                        {
                            switch (IsStreamingStarted)
                            {
                                case true:
                                    SetupFinishAsk(false);
                                    break;
                                default:
                                    Finish();
                                    break;
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

        private void SetupFinishAsk(bool isStreamer)
        {
            try
            {
                var dialog = new MaterialAlertDialogBuilder(this);
                switch (isStreamer)
                {
                    case true:
                        dialog.SetTitle(Resource.String.Lbl_LiveStreamer_alert_title);
                        dialog.SetMessage(GetText(Resource.String.Lbl_LiveStreamer_alert_message));
                        break;
                    default:
                        dialog.SetTitle(Resource.String.Lbl_LiveViewer_alert_title);
                        dialog.SetMessage(GetText(Resource.String.Lbl_LiveViewer_alert_message));
                        break;
                }

                dialog.SetPositiveButton(GetText(Resource.String.Lbl_Yes), (materialDialog, action) =>
                {
                    try
                    {
                        FinishStreaming(isStreamer);
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
                dialog.SetNegativeButton(GetText(Resource.String.Lbl_No), new MaterialDialogUtils());

                dialog.Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async void FinishStreaming(bool isStreamer)
        {
            try
            {
                IsStreamingStarted = false;

                TabbedMainActivity.GetInstance()?.SetOffWakeLock();

                StopTimer();
                DestroyTimerComment();

                if (IsOwner)
                {
                    if (isStreamer)
                    {
                        if (ListUtils.SettingsSiteList?.LiveVideoSave is "0")
                            DeleteLiveStream();
                        else
                        {
                            // Keep live stream for replay
                        }
                    }
                    else
                        DeleteLiveStream();
                }

                LeaveLiveKitRoom();
                //add end page
                LiveStreamEnded();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LiveStreamEnded()
        {
            try
            {
                MLiveStreamEndedLyt.Visibility = ViewStates.Visible;

                MLoadingViewer.Visibility = ViewStates.Gone;
                MGetReadyLyt.Visibility = ViewStates.Gone;

                MVideoControlLyt.Visibility = ViewStates.Gone;
                MVideoLayout.Visibility = ViewStates.Gone;

                BgAvatar = FindViewById<ImageView>(Resource.Id.bg_avatar_end);
                StreamRateLevel = FindViewById<CircleImageView>(Resource.Id.streamRateLevel);
                CloseEnded = FindViewById<ImageView>(Resource.Id.close_ended);
                CloseEnded.Click += CloseEndedOnClick;

                Header = FindViewById<TextView>(Resource.Id.header);
                ShareStreamText = FindViewById<TextView>(Resource.Id.shareStreamText);

                GoLiveButton = FindViewById<AppCompatButton>(Resource.Id.goLiveButton);

                InfoLiveLayout = FindViewById<LinearLayout>(Resource.Id.infoLiveLayout);

                Comments = FindViewById<TextView>(Resource.Id.commentsValue);
                Viewers = FindViewById<TextView>(Resource.Id.viewersValue);
                Duration = FindViewById<TextView>(Resource.Id.timeValue);



                switch (IsOwner)
                {
                    case true:
                        {
                            if (PostObject != null)
                            {
                                GlideImageLoader.LoadImage(this, PostObject.Publisher.Avatar, BgAvatar, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                                StreamRateLevel.SetImageURI(Uri.Parse(PostObject.Publisher.Avatar));
                                //GlideImageLoader.LoadImage(this, PostObject.Avater, StreamRateLevel, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                            }

                            Header.Text = GetText(Resource.String.Lbl_YourLiveStreamHasEnded);
                            ShareStreamText.Text = GetText(Resource.String.Lbl_LiveStreamer_End_title);

                            InfoLiveLayout.Visibility = ViewStates.Visible;
                            GoLiveButton.Visibility = ViewStates.Gone;

                            Comments.Text = MAdapter.CommentList.Count.ToString();
                            Viewers.Text = MViewersText.Text?.Replace(GetText(Resource.String.Lbl_Views), "");
                            Duration.Text = MTimeText.Text;
                            break;
                        }
                    default:
                        {
                            if (LiveStreamViewerObject != null)
                            {
                                GlideImageLoader.LoadImage(this, LiveStreamViewerObject.Publisher.Avatar, BgAvatar, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                                StreamRateLevel.SetImageURI(Uri.Parse(LiveStreamViewerObject.Publisher.Avatar));
                                //GlideImageLoader.LoadImage(this, LiveStreamViewerObject.Avater, StreamRateLevel, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                            }

                            Header.Text = GetText(Resource.String.Lbl_LiveStreamHasEnded);
                            ShareStreamText.Text = GetText(Resource.String.Lbl_LiveViewer_End_title);

                            InfoLiveLayout.Visibility = ViewStates.Gone;
                            GoLiveButton.Visibility = ViewStates.Visible;
                            GoLiveButton.Click += GoLiveButtonOnClick;
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void CloseEndedOnClick(object sender, EventArgs e)
        {
            try
            {
                Finish();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void GoLiveButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                var streamName = "live" + Methods.Time.CurrentTimeMillis();
                if (string.IsNullOrEmpty(streamName) || string.IsNullOrWhiteSpace(streamName))
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_PleaseEnterLiveStreamName), ToastLength.Short);
                    return;
                }
                //Owner >> ClientRoleBroadcaster , Users >> ClientRoleAudience
                Intent intent = new Intent(this, typeof(LiveStreamingActivity));
                intent.PutExtra(LiveConstants.KeyClientRole, LiveConstants.ClientRoleBroadcaster);
                intent.PutExtra("StreamName", streamName);
                StartActivity(intent);

                Finish();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                RootView = FindViewById<FrameLayout>(Resource.Id.rootView);
                MHeaderViewStub = FindViewById<ViewStub>(Resource.Id.liveStreaming_headerStub);
                MFooterViewStub = FindViewById<ViewStub>(Resource.Id.liveStreaming_footer);

                MVideoControlLyt = FindViewById<FrameLayout>(Resource.Id.liveStreaming_videoAndControlsContainer);
                MGetReadyLyt = FindViewById<ConstraintLayout>(Resource.Id.streamerReady_root);
                MLoadingViewer = FindViewById<ConstraintLayout>(Resource.Id.loading_joining);
                MLiveStreamEndedLyt = FindViewById<FrameLayout>(Resource.Id.streamer_final_screen_root);

                MVideoLayout = FindViewById<VideoGridContainer>(Resource.Id.liveStreaming_videoContainer);
                //MVideoLayout.SetStatsManager(StatsManager()); -- Stats removed with Agora

                MAvatarBg = FindViewById<ImageView>(Resource.Id.streamLoadingProgress_backgroundAvatar);
                MAvatar = FindViewById<CircleImageView>(Resource.Id.streamLoadingProgress_foregroundAvatar);

                MEmojisIconBtn = FindViewById<ImageView>(Resource.Id.sendEmojisIconButton);
                MMoreBtn = FindViewById<ImageView>(Resource.Id.more_btn);
                MShareBtn = FindViewById<ImageView>(Resource.Id.share_btn);
                TxtComment = FindViewById<AXEmojiEditText>(Resource.Id.MessageWrapper);
                MSendBtn = FindViewById<ImageView>(Resource.Id.sendMessageButton);

                InitEmojisView();

                switch (IsOwner)
                {
                    case true:
                        MHeaderViewStub.LayoutResource = Resource.Layout.view_live_streaming_streamer_header;
                        MHeaderViewStub.Inflate();

                        MEmojisIconBtn.Visibility = ViewStates.Gone;
                        MMoreBtn.Visibility = ViewStates.Gone;

                        MFooterViewStub.LayoutResource = Resource.Layout.view_live_streaming_streamer_footer;
                        MFooterViewStub.Inflate();

                        InitViewerFooter();
                        break;
                    default:
                        MHeaderViewStub.LayoutResource = Resource.Layout.view_live_streaming_viewer_header;
                        MHeaderViewStub.Inflate();

                        MEmojisIconBtn.Visibility = ViewStates.Visible;
                        MMoreBtn.Visibility = ViewStates.Visible;
                        break;
                }

                MRecycler = FindViewById<RecyclerView>(Resource.Id.liveStreaming_messageList);

                MViewersText = FindViewById<TextView>(Resource.Id.livestreamingHeader_viewers);
                MCloseIn = FindViewById<ImageView>(Resource.Id.close_in);
                MCloseOut = FindViewById<ImageView>(Resource.Id.close_out);
                MCloseStreaming = FindViewById<ImageView>(Resource.Id.livestreamingHeader_close);

                MTimeText = FindViewById<TextView>(Resource.Id.livestreamingHeader_status);
                MViewersText.Text = "0 " + GetText(Resource.String.Lbl_Views);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitEmojisView()
        {
            Methods.SetColorEditText(TxtComment, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
            Task.Factory.StartNew(() =>
            {
                try
                {
                    if (WoWonderTools.IsTabDark())
                        EmojisViewTools.LoadDarkTheme();
                    else
                        EmojisViewTools.LoadTheme(AppSettings.MainColor);

                    EmojisViewTools.MStickerView = false;
                    EmojisViewTools.LoadView(this, TxtComment, "", MEmojisIconBtn);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            });
        }

        private void InitBackPressed()
        {
            try
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
                {
                    OnBackInvokedDispatcher.RegisterOnBackInvokedCallback(0, new BackCallAppBase2(this, "LiveStreamingActivity"));
                }
                else
                {
                    OnBackPressedDispatcher.AddCallback(new BackCallAppBase1(this, "LiveStreamingActivity", true));
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetRecyclerViewAdapters()
        {
            try
            {
                MAdapter = new LiveMessageAdapter(this)
                {
                    CommentList = new ObservableCollection<CommentObjectExtra>()
                };
                LayoutManager = new LinearLayoutManager(this);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(50);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<CommentObjectExtra>(this, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);
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
                        {
                            MAdapter.ItemLongClick += MAdapterOnItemLongClick;
                            if (MCloseIn != null) MCloseIn.Click += MCloseInOnClick;
                            if (MCloseOut != null) MCloseOut.Click += MCloseInOnClick;
                            if (MCloseStreaming != null) MCloseStreaming.Click += MCloseInOnClick;
                            if (MSendBtn != null) MSendBtn.Click += MSendBtnOnClick;
                            if (MMoreBtn != null) MMoreBtn.Click += MMoreBtnOnClick;
                            if (MShareBtn != null) MShareBtn.Click += MShareBtnOnClick;
                            if (MCameraBtn != null) MCameraBtn.Click += MCameraBtnOnClick;
                            if (MEffectBtn != null) MEffectBtn.Click += MEffectBtnOnClick;
                            if (MVideoEnabledBtn != null) MVideoEnabledBtn.Click += MVideoEnabledBtnOnClick;
                            if (MAudioEnabledBtn != null) MAudioEnabledBtn.Click += MAudioEnabledBtnOnClick;
                            break;
                        }
                    default:
                        {
                            MAdapter.ItemLongClick -= MAdapterOnItemLongClick;
                            if (MCloseIn != null) MCloseIn.Click -= MCloseInOnClick;
                            if (MCloseOut != null) MCloseOut.Click -= MCloseInOnClick;
                            if (MCloseStreaming != null) MCloseStreaming.Click -= MCloseInOnClick;
                            if (MSendBtn != null) MSendBtn.Click -= MSendBtnOnClick;
                            if (MMoreBtn != null) MMoreBtn.Click -= MMoreBtnOnClick;
                            if (MShareBtn != null) MShareBtn.Click -= MShareBtnOnClick;
                            if (MCameraBtn != null) MCameraBtn.Click -= MCameraBtnOnClick;
                            if (MEffectBtn != null) MEffectBtn.Click -= MEffectBtnOnClick;
                            if (MVideoEnabledBtn != null) MVideoEnabledBtn.Click -= MVideoEnabledBtnOnClick;
                            if (MAudioEnabledBtn != null) MAudioEnabledBtn.Click -= MAudioEnabledBtnOnClick;
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void DestroyBasic()
        {
            try
            {
                MVideoControlLyt = null!;
                MHeaderViewStub = null!;
                MFooterViewStub = null!;
                MGetReadyLyt = null!;
                MLoadingViewer = null!;
                MAvatarBg = null!;
                MCloseIn = null!;
                MCloseOut = null!;
                MCloseStreaming = null!;
                MAvatar = null!;
                MTimeText = null!;
                MEmojisIconBtn = null!;
                MMoreBtn = null!;
                MCameraBtn = null!;
                MEffectBtn = null!;
                MVideoEnabledBtn = null!;
                MAudioEnabledBtn = null!;
                TxtComment = null!;
                MSendBtn = null!;
                LayoutManager = null!;
                MRecycler = null!;
                MAdapter = null!;
                TimerComments = null!;
                LiveStreamViewerObject = null!;
                PostObject = null!;
                PostId = null!;
                MStreamChannel = null!;
                MVideoLayout = null!;
                SurfaceView = null!;
                MVideoDimension = null!;
                CustomHandler = null!;
                UpdateTimerThread = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitViewerFooter()
        {
            try
            {
                MCameraBtn = FindViewById<TextView>(Resource.Id.camera_switch_btn);

                MEffectBtn = FindViewById<TextView>(Resource.Id.effect_btn);
                MEffectBtn.Activated = true;

                MVideoEnabledBtn = FindViewById<TextView>(Resource.Id.video_enabled_btn);
                MVideoEnabledBtn.Activated = true;

                MAudioEnabledBtn = FindViewById<TextView>(Resource.Id.audio_enabled_btn);
                MAudioEnabledBtn.Activated = true;

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, MCameraBtn, FontAwesomeIcon.Camera);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, MEffectBtn, FontAwesomeIcon.Magic);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, MVideoEnabledBtn, FontAwesomeIcon.Video);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, MAudioEnabledBtn, FontAwesomeIcon.MicrophoneAlt);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void MAdapterOnItemLongClick(object sender, LiveMessageAdapterClickEventArgs e)
        {
            try
            {
                var item = MAdapter.CommentList.LastOrDefault();
                if (item?.Publisher != null)
                {
                    TxtComment.Text = "";
                    TxtComment.Text = "@" + item.Publisher.Username + " ";
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MAudioEnabledBtnOnClick(object sender, EventArgs e)
        {
            try
            {
                switch (sender)
                {
                    case View view:
                        {
                            // LiveKit: mute/unmute local audio
                            LiveKitManager.SetAudioEnabled(!view.Activated);
                            view.Activated = !view.Activated;

                            switch (view.Activated)
                            {
                                case true:
                                    FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, MAudioEnabledBtn, FontAwesomeIcon.MicrophoneAlt);
                                    break;
                                default:
                                    FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, MAudioEnabledBtn, FontAwesomeIcon.MicrophoneAltSlash);
                                    break;
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

        private void MVideoEnabledBtnOnClick(object sender, EventArgs e)
        {
            try
            {
                switch (sender)
                {
                    case View view:
                        {
                            switch (view.Activated)
                            {
                                case true:
                                    StopBroadcast();
                                    break;
                                default:
                                    StartBroadcast();
                                    break;
                            }
                            view.Activated = !view.Activated;

                            switch (view.Activated)
                            {
                                case true:
                                    FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, MVideoEnabledBtn, FontAwesomeIcon.Video);
                                    break;
                                default:
                                    FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, MVideoEnabledBtn, FontAwesomeIcon.VideoSlash);
                                    break;
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

        private void MEffectBtnOnClick(object sender, EventArgs e)
        {
            try
            {
                switch (sender)
                {
                    case View view:
                        {
                            view.Activated = !view.Activated;
                            LiveKitManager.ToggleBeautyEffect(view.Activated);

                            switch (view.Activated)
                            {
                                case true:
                                    MEffectBtn.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                                    break;
                                default:
                                    MEffectBtn.SetTextColor(Color.White);
                                    break;
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

        private void MCameraBtnOnClick(object sender, EventArgs e)
        {
            try
            {
                LiveKitManager.SwitchCamera();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MShareBtnOnClick(object sender, EventArgs e)
        {
            try
            {
                Bundle bundle = new Bundle();

                bundle.PutString("ItemData", IsOwner ? JsonConvert.SerializeObject(PostObject) : JsonConvert.SerializeObject(LiveStreamViewerObject));
                bundle.PutString("TypePost", JsonConvert.SerializeObject(PostModelType.AgoraLivePost));
                var searchFilter = new ShareBottomDialogFragment
                {
                    Arguments = bundle
                };
                searchFilter.Show(SupportFragmentManager, "ShareFilter");
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MMoreBtnOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialAlertDialogBuilder(this);

                arrayAdapter.Add(GetText(Resource.String.Lbl_ViewProfile));
                arrayAdapter.Add(GetText(Resource.String.Lbl_Copy));

                if (!IsOwner)
                    arrayAdapter.Add(GetText(Resource.String.Lbl_Report));

                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void MSendBtnOnClick(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(TxtComment.Text) && string.IsNullOrWhiteSpace(TxtComment.Text))
                    return;

                if (Methods.CheckConnectivity())
                {
                    var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                    //Comment Code 

                    var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    string time2 = unixTimestamp.ToString(CultureInfo.InvariantCulture);

                    CommentObjectExtra comment = new CommentObjectExtra
                    {
                        Id = unixTimestamp.ToString(),
                        PostId = PostId,
                        UserId = UserDetails.UserId,
                        Text = TxtComment.Text,
                        Time = time2,
                        CFile = "",
                        Record = "",
                        Publisher = dataUser,
                        Url = dataUser?.Url,
                        Fullurl = PostObject?.PostUrl,
                        Orginaltext = TxtComment.Text,
                        Owner = true,
                        CommentLikes = "0",
                        CommentWonders = "0",
                        IsCommentLiked = false,
                        Replies = "0",
                        RepliesCount = "0"
                    };

                    MAdapter.CommentList.Add(comment);

                    var index = MAdapter.CommentList.IndexOf(comment);
                    switch (index)
                    {
                        case > -1:
                            MAdapter.NotifyItemInserted(index);
                            break;
                    }

                    var text = TxtComment.Text;

                    //Hide keyboard
                    TxtComment.Text = "";

                    var (apiStatus, respond) = await RequestsAsync.Comment.CreatePostCommentsAsync(PostId, text, "", "", "");
                    switch (apiStatus)
                    {
                        case 200:
                            {
                                switch (respond)
                                {
                                    case CreateComments result:
                                        {
                                            var date = MAdapter.CommentList.FirstOrDefault(a => a.Id == comment.Id) ?? MAdapter.CommentList.FirstOrDefault(x => x.Id == result.Data.Id);
                                            if (date != null)
                                            {
                                                var db = ClassMapper.Mapper?.Map<CommentObjectExtra>(result.Data);

                                                date = db;
                                                date.Id = result.Data.Id;

                                                index = MAdapter.CommentList.IndexOf(MAdapter.CommentList.FirstOrDefault(a => a.Id == unixTimestamp.ToString()));
                                                switch (index)
                                                {
                                                    case > -1:
                                                        MAdapter.CommentList[index] = db;

                                                        MAdapter.NotifyItemChanged(index);
                                                        MRecycler.ScrollToPosition(index);
                                                        break;
                                                }

                                                var postFeedAdapter = TabbedMainActivity.GetInstance()?.NewsFeedTab?.PostFeedAdapter;
                                                var dataGlobal = postFeedAdapter?.ListDiffer?.Where(a => a.PostData?.Id == PostId).ToList();
                                                switch (dataGlobal?.Count)
                                                {
                                                    case > 0:
                                                        {
                                                            foreach (var dataClass in from dataClass in dataGlobal let indexCom = postFeedAdapter.ListDiffer.IndexOf(dataClass) where indexCom > -1 select dataClass)
                                                            {
                                                                dataClass.PostData.PostComments = MAdapter.CommentList.Count.ToString();

                                                                switch (dataClass.PostData.GetPostComments?.Count)
                                                                {
                                                                    case > 0:
                                                                        {
                                                                            var dataComment = dataClass.PostData.GetPostComments.FirstOrDefault(a => a.Id == date.Id);
                                                                            switch (dataComment)
                                                                            {
                                                                                case null:
                                                                                    dataClass.PostData.GetPostComments.Add(date);
                                                                                    break;
                                                                            }

                                                                            break;
                                                                        }
                                                                    default:
                                                                        dataClass.PostData.GetPostComments = new List<CommentDataObject> { date };
                                                                        break;
                                                                }

                                                                postFeedAdapter?.NotifyItemChanged(postFeedAdapter.ListDiffer.IndexOf(dataClass), "commentReplies");
                                                            }

                                                            break;
                                                        }
                                                }
                                            }

                                            break;
                                        }
                                }

                                break;
                            }
                    }
                    //else Methods.DisplayReportResult(this, respond);

                    //Hide keyboard
                    TxtComment.Text = "";
                }
                else
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MCloseInOnClick(object sender, EventArgs e)
        {
            try
            {
                BackPressed();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region LiveKit

        private void InitLiveKit()
        {
            try
            {
                switch (IsOwner)
                {
                    case true:
                        // Broadcaster role - will publish tracks
                        break;
                    default:
                        // Audience role - will subscribe
                        break;
                }

                MVideoWidth = 640;
                MVideoHeight = 360;
                InitValueLive();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitValueLive()
        {
            try
            {
                IsStreamingStarted = false;

                switch (IsOwner)
                {
                    case true:
                        MGetReadyLyt.Visibility = ViewStates.Visible;
                        MLoadingViewer.Visibility = ViewStates.Gone;
                        MVideoControlLyt.Visibility = ViewStates.Gone;
                        MLiveStreamEndedLyt.Visibility = ViewStates.Gone;

                        CreateLiveStream();
                        break;
                    default:
                        {
                            if (LiveStreamViewerObject != null)
                            {
                                GlideImageLoader.LoadImage(this, LiveStreamViewerObject.Publisher.Avatar, MAvatarBg, ImageStyle.CenterCrop, ImagePlaceholders.DrawableUser);
                                GlideImageLoader.LoadImage(this, LiveStreamViewerObject.Publisher.Avatar, MAvatar, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);
                            }

                            MLoadingViewer.Visibility = ViewStates.Visible;
                            MGetReadyLyt.Visibility = ViewStates.Gone;
                            MVideoControlLyt.Visibility = ViewStates.Gone;
                            MLiveStreamEndedLyt.Visibility = ViewStates.Gone;

                            JoinLiveKitRoom(Config().GetChannelName(), UserDetails.Username, false);
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitStreamerInfo()
        {
            try
            {
                ImageView streamerAvatar = FindViewById<ImageView>(Resource.Id.livestreamingHeader_streamerImage);
                TextView streamerName = FindViewById<TextView>(Resource.Id.livestreamingHeader_name);

                if (LiveStreamViewerObject != null)
                {
                    GlideImageLoader.LoadImage(this, LiveStreamViewerObject.Publisher.Avatar, streamerAvatar, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);
                    streamerName.Text = WoWonderTools.GetNameFinal(LiveStreamViewerObject.Publisher);

                    //if (LiveStreamViewerObject.LiveTime != null)
                    //    SetTimer(LiveStreamViewerObject.LiveTime.Value);
                    MTimeText.Text = GetText(Resource.String.Lbl_Live);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void StartBroadcast()
        {
            try
            {
                // LiveKit: publish local tracks
                LiveKitManager.SetVideoEnabled(true);
                RunOnUiThread(() =>
                {
                    MVideoLayout.Visibility = ViewStates.Visible;

                    MLoadingViewer.Visibility = ViewStates.Gone;
                    MGetReadyLyt.Visibility = ViewStates.Gone;
                    MLiveStreamEndedLyt.Visibility = ViewStates.Gone;

                    MVideoControlLyt.Visibility = ViewStates.Visible;
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void StopBroadcast()
        {
            try
            {
                // LiveKit: unpublish local tracks
                LiveKitManager.SetVideoEnabled(false);
                RunOnUiThread(() =>
                {
                    MVideoLayout.Visibility = ViewStates.Gone;

                    MLoadingViewer.Visibility = ViewStates.Gone;
                    MGetReadyLyt.Visibility = ViewStates.Gone;
                    MLiveStreamEndedLyt.Visibility = ViewStates.Gone;

                    MVideoControlLyt.Visibility = ViewStates.Visible;
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnLiveKitConnected()
        {
            try
            {
                RunOnUiThread(async () =>
                {
                    try
                    {
                        TabbedMainActivity.GetInstance()?.SetOnWakeLock();
                        IsStreamingStarted = true;

                        switch (IsOwner)
                        {
                            case true:
                                StartBroadcast();
                                break;
                            default:
                                InitStreamerInfo();
                                break;
                        }

                        if (IsOwner)
                        {
                            // TODO: Start LiveKit Egress recording if needed
                        }
                        else
                        {
                            await Task.Delay(TimeSpan.FromSeconds(3));
                        }
                        LoadMessages();
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

        public void OnLiveKitDisconnected()
        {
            try
            {
                RunOnUiThread(() =>
                {
                    MVideoLayout?.RemoveAllVideo();
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnParticipantJoined(string identity)
        {
            // Participant joined notification
            RunOnUiThread(() =>
            {
                MViewersText.Text = identity;
            });
        }

        public void OnParticipantLeft(string identity)
        {
            // Participant left notification
        }

        #endregion

        #region LiveKit Messages & Comments

        public void OnError(int err)
        {
            RunOnUiThread(() =>
            {
                try
                {
                    Console.WriteLine("Error code " + err);

                    var dialog = new MaterialAlertDialogBuilder(this);
                    dialog.SetTitle(GetText(Resource.String.Lbl_ErrorLive_Code) + " " + err);
                    dialog.SetMessage(GetText(Resource.String.Lbl_ErrorCall_Message));
                    dialog.SetPositiveButton(GetText(Resource.String.Lbl_Ok), (materialDialog, action) =>
                    {
                        try
                        {
                            Finish();
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    });
                    dialog.SetNeutralButton(GetText(Resource.String.Lbl_ContactUs), (materialDialog, action) =>
                    {
                        try
                        {
                            new IntentController(this).OpenBrowserFromApp(InitializeWoWonder.WebsiteUrl + "/contact-us");
                            Finish();
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    });
                    dialog.Show();
                }
                catch (Exception e)
                {
                    Finish();
                    Methods.DisplayReportResultTrack(e);
                }
            });
        }

        private void RenderRemoteUser(int uid)
        {
            try
            {
                // LiveKit: render remote participant track
                // SurfaceView will be set up when the LiveKit SDK is integrated
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void RemoveRemoteUser(int uid)
        {
            try
            {
                MVideoLayout.RemoveUserVideo(uid, false);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Api

        private void CreateLiveStream()
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { CreateLive });
        }

        private async Task CreateLive()
        {
            try
            {
                var streamName = Config().GetChannelName();
                var url = AppSettings.DomainUrl + "/xhr.php?f=live&s=create";

                var formData = new Dictionary<string, string>
                {
                    {"stream_name", streamName},
                    {"user_id", UserDetails.UserId}
                };

                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(30);
                httpClient.DefaultRequestHeaders.Add("x-requested-with", "XMLHttpRequest");

                var response = await httpClient.PostAsync(url, new FormUrlEncodedContent(formData));
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

                if (result != null && result.ContainsKey("livekit_token"))
                {
                    var token = result["livekit_token"]?.ToString();
                    var identity = result.ContainsKey("livekit_identity") ? result["livekit_identity"]?.ToString() : UserDetails.UserId;
                    var roomName = result.ContainsKey("livekit_room") ? result["livekit_room"]?.ToString() : streamName;

                    LiveKitRoomManager.Instance.CurrentToken = token;

                    RunOnUiThread(() =>
                    {
                        // Join the LiveKit room
                        _ = JoinLiveKitRoom(roomName, identity, true);
                        StartBroadcast();
                        OnLiveKitConnected();
                    });
                }
                else
                {
                    Methods.DisplayReportResult(this, json);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private void DeleteLiveStream()
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { async () => await RequestsAsync.Posts.DeleteLiveAsync(PostId) });
        }

        #region CreateLiveThumbnail


        //private void CreateLiveThumbnail()
        //{
        //    try
        //    {
        //        if (!Methods.CheckConnectivity())
        //            ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
        //        else
        //        {
        //            GetSurfaceBitmap(SurfaceView);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Methods.DisplayReportResultTrack(e);  
        //    }
        //}

        //private Bitmap SurfaceBitmap;
        //private void GetSurfaceBitmap(SurfaceView surfaceView)
        //{
        //    try
        //    {
        //        if (surfaceView == null)
        //            return;

        //        if (surfaceView.MeasuredHeight <= 0)
        //            surfaceView.Measure(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);

        //        SurfaceBitmap = Bitmap.CreateBitmap(surfaceView.Width, surfaceView.Height, Bitmap.Config.Argb8888);
        //        if (SurfaceBitmap != null)
        //        {
        //            HandlerThread handlerThread = new HandlerThread(PostId + "_thumbnail");
        //            handlerThread.Start();

        //            if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
        //            {
        //                PixelCopy.Request(surfaceView, SurfaceBitmap, this, new Handler(handlerThread.Looper));
        //            }
        //            else
        //            {
        //                Console.WriteLine("Saving an image of a SurfaceView is only supported for API 24 and above");
        //            }
        //        } 
        //    }
        //    catch (Exception e)
        //    {
        //        Methods.DisplayReportResultTrack(e);
        //    }
        //}

        //public void OnPixelCopyFinished(int copyResult)
        //{
        //    try
        //    {
        //        if (copyResult == (int)PixelCopyResult.Success)
        //        {
        //            var pathImage = Methods.MultiMedia.Export_Bitmap_As_Image(SurfaceBitmap, PostId + "_thumbnail", Methods.Path.FolderDcimImage);
        //            if (!string.IsNullOrEmpty(pathImage))
        //            {
        //                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Posts.CreateLiveThumbnail(PostId, pathImage) });
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Methods.DisplayReportResultTrack(e);
        //    }
        //}

        #endregion

        private void LoadMessages()
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { async () => await LoadDataComment() });
        }

        private async Task LoadDataComment(string resourceId = "", string sid = "", string fileList = "")
        {
            if (string.IsNullOrEmpty(PostId))
                return;

            if (Methods.CheckConnectivity())
            {
                var offset = MAdapter.CommentList.LastOrDefault()?.Id ?? "0";
                var (apiStatus, respond) = await RequestsAsync.Posts.CheckCommentsLiveAsync(PostId, IsOwner ? "live" : "story", "10", offset, resourceId, sid, fileList);
                if (apiStatus != 200 || respond is not CheckCommentsLiveObject result || result.Comments == null)
                {
                    if (respond is ErrorObject error)
                    {
                        if (error.Error.ErrorText == "post not found")
                        {
                            RunOnUiThread(() => { FinishStreaming(IsOwner); });
                        }
                    }
                    else
                        Methods.DisplayReportResult(this, respond);
                }
                else
                {
                    var respondList = result.Comments?.Count;
                    switch (respondList)
                    {
                        case > 0:
                            {
                                foreach (var item in result.Comments)
                                {
                                    CommentObjectExtra check = MAdapter.CommentList.FirstOrDefault(a => a.Id == item.Id);
                                    switch (check)
                                    {
                                        case null:
                                            {
                                                var db = ClassMapper.Mapper?.Map<CommentObjectExtra>(item);
                                                if (db != null) MAdapter.CommentList.Add(db);
                                                break;
                                            }
                                        default:
                                            check = ClassMapper.Mapper?.Map<CommentObjectExtra>(item);
                                            check.Replies = item.Replies;
                                            check.RepliesCount = item.RepliesCount;
                                            break;
                                    }
                                }

                                RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                                break;
                            }
                    }

                    RunOnUiThread(() =>
                    {
                        try
                        {
                            if (result.Count != null)
                                MViewersText.Text = Methods.FunString.FormatPriceValue(result.Count.Value) + " " + GetText(Resource.String.Lbl_Views);
                            else
                                MViewersText.Text = "0 " + GetText(Resource.String.Lbl_Views);

                            if (TimerComments != null && !string.IsNullOrEmpty(result.StillLive) && result.StillLive == "offline")
                                BackPressed();
                        }
                        catch (Exception exception)
                        {
                            Methods.DisplayReportResultTrack(exception);
                        }
                    });
                }

                RunOnUiThread(() =>
                {
                    try
                    {
                        MRecycler.Visibility = ViewStates.Visible;
                        var index = MAdapter.CommentList.IndexOf(MAdapter.CommentList.LastOrDefault());
                        switch (index)
                        {
                            case > -1:
                                MRecycler.ScrollToPosition(index);
                                break;
                        }

                        SetTimerComment();
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
            }
        }

        #region Timer Time LIve

        private void SetTimer(long elapsed)
        {
            try
            {
                switch (IsOwner)
                {
                    case true:
                        StartTime = elapsed;
                        CustomHandler ??= new Handler(Looper.MainLooper);
                        UpdateTimerThread = new MyRunnable(this);
                        CustomHandler.PostDelayed(UpdateTimerThread, 0);
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void StopTimer()
        {
            try
            {
                switch (IsOwner)
                {
                    case true:
                        TimeSwapBuff += TimeInMilliseconds;
                        CustomHandler.RemoveCallbacks(UpdateTimerThread);
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private class MyRunnable : Object, IRunnable
        {
            private readonly LiveStreamingActivity Activity;
            public MyRunnable(LiveStreamingActivity activity)
            {
                Activity = activity;
            }

            public void Run()
            {
                try
                {
                    Activity.TimeInMilliseconds = SystemClock.ElapsedRealtime() - Activity.StartTime;
                    Activity.UpdatedTime = Activity.TimeSwapBuff + Activity.TimeInMilliseconds;
                    int secs = (int)(Activity.UpdatedTime / 1000);
                    int min = secs / 60;
                    secs %= 60;

                    TimeSpan tsTemp = new TimeSpan(0, min, secs);
                    Activity.MTimeText.Text = tsTemp.ToString();

                    Activity.CustomHandler.PostDelayed(this, 0);
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            }
        }

        #endregion

        #region Timer Load Comment

        private void SetTimerComment()
        {
            try
            {
                switch (TimerComments)
                {
                    //Run timer
                    case null:
                        TimerComments = new Timer { Interval = 3000 };
                        TimerComments.Elapsed += TimerCommentsOnElapsed;
                        TimerComments.Enabled = true;
                        TimerComments.Start();
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void TimerCommentsOnElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                LoadMessages();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void StartTimerComment()
        {
            try
            {
                if (TimerComments != null)
                {
                    TimerComments.Enabled = true;
                    TimerComments.Start();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void StopTimerComment()
        {
            try
            {
                if (TimerComments != null)
                {
                    TimerComments.Enabled = false;
                    TimerComments.Stop();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void DestroyTimerComment()
        {
            try
            {
                if (TimerComments != null)
                {
                    TimerComments.Enabled = false;
                    TimerComments.Stop();
                    TimerComments.Dispose();
                    TimerComments = null!;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region MaterialDialog

        public void OnSelection(IDialogInterface dialog, int position, string itemString)
        {
            try
            {
                if (itemString == GetText(Resource.String.Lbl_ViewProfile))
                {
                    switch (IsOwner)
                    {
                        case true:
                            WoWonderTools.OpenProfile(this, PostObject.Publisher.UserId, PostObject.Publisher);
                            break;
                        default:
                            WoWonderTools.OpenProfile(this, LiveStreamViewerObject.Publisher.UserId, LiveStreamViewerObject.Publisher);
                            break;
                    }
                }
                else if (itemString == GetText(Resource.String.Lbl_Copy))
                {
                    Methods.CopyToClipboard(this, IsOwner ? PostObject.Url : LiveStreamViewerObject.Url);
                }
                else if (itemString == GetText(Resource.String.Lbl_Report))
                {
                    var arrayAdapter = new List<string>();
                    var dialogList = new MaterialAlertDialogBuilder(this);
                    dialogList.SetTitle(GetText(Resource.String.Lbl_ReportLive_Title));
                    //dialogList.SetMessage(GetText(Resource.String.Lbl_ReportLive_desc));

                    arrayAdapter.Add(GetText(Resource.String.Lbl_Nudity));
                    arrayAdapter.Add(GetText(Resource.String.Lbl_Violence));
                    arrayAdapter.Add(GetText(Resource.String.Lbl_Harassment));
                    arrayAdapter.Add(GetText(Resource.String.Lbl_Suicide));
                    arrayAdapter.Add(GetText(Resource.String.Lbl_FalseInformation));
                    arrayAdapter.Add(GetText(Resource.String.Lbl_Spam));
                    arrayAdapter.Add(GetText(Resource.String.Lbl_UnauthorizedSales));
                    arrayAdapter.Add(GetText(Resource.String.Lbl_HateSpeech));
                    arrayAdapter.Add(GetText(Resource.String.Lbl_Terrorism));
                    arrayAdapter.Add(GetText(Resource.String.Lbl_IntellectualProperty));
                    arrayAdapter.Add(GetText(Resource.String.Lbl_SomethingElse));
                    arrayAdapter.Add(GetText(Resource.String.Lbl_Other));

                    dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                    dialogList.SetPositiveButton(GetText(Resource.String.Lbl_Report), (materialDialog, action) =>
                    {
                        try
                        {
                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_YourReportPost), ToastLength.Short);
                            //Sent Api >>
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Posts.PostActionsAsync(IsOwner ? PostObject.Id : LiveStreamViewerObject.Id, "report") });
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    });
                    dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());

                    dialogList.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        private string GetRegion(string region)
        {
            try
            {
                return region switch
                {
                    "us-east-1" => "0",
                    "us-east-2" => "1",
                    "us-west-1" => "2",
                    "us-west-2" => "3",
                    "eu-west-1" => "4",
                    "eu-west-2" => "5",
                    "eu-west-3" => "6",
                    "eu-central-1" => "7",
                    "ap-southeast-1" => "8",
                    "ap-southeast-2" => "9",
                    "ap-northeast-1" => "10",
                    "ap-northeast-2" => "11",
                    "sa-east-1" => "12",
                    "ca-central-1" => "13",
                    "ap-south-1" => "14",
                    "cn-north-1" => "15",
                    "us-gov-west-1" => "17",
                    _ => ""
                };
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return "";
            }
        }
    }
}