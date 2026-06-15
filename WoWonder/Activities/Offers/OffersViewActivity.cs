using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AndroidX.Core.Graphics.Drawable;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide.Util;
using Google.Android.Material.Dialog;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using WoWonder.Activities.Base;
using WoWonder.Activities.Comment.Adapters;
using WoWonder.Activities.NativePost.Extra;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Activities.Tabbes;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using WoWonder.Library.Anjo.SuperTextLibrary;
using WoWonderClient.Classes.Comments;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Offers;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Requests;
using Exception = System.Exception;
using Reaction = WoWonderClient.Classes.Posts.Reaction;
using String = Java.Lang.String;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Offers
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class OffersViewActivity : BaseActivity, IDialogListCallBack
    {
        #region Variables Basic

        private ImageView OfferCoverImage;
        private ImageButton IconBack;
        private ImageView OfferAvatar;
        private TextView Name, Subname, PageTitle, DiscountNumber, DateNumber;
        private ImageButton IconMore;
        private SuperTextView Description;
        private OffersDataObject DataInfoObject;
        private StReadMoreOption ReadMoreOption;

        private string PostId;
        private RecyclerView CommentsRecyclerView;
        private CommentAdapter MAdapter;

        private LinearLayout MainSectionButton, ShareLinearLayout, CommentLinearLayout, SecondReactionLinearLayout, ReactLinearLayout;
        private ReactButton LikeButton;
        private TextView SecondReactionButton;

        private PostClickListener ClickListener;
        private PostDataObject PostData;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.Overlap_Dark : Resource.Style.Overlap_Light);

                Methods.App.FullScreenApp(this);

                Overlap();

                // Create your application here
                SetContentView(Resource.Layout.OffersViewLayout);

                ClickListener = new PostClickListener(this, NativeFeedType.Global);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                BindOfferPost();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void Overlap()
        {
            /*View mContentView = Window?.DecorView;

            Window?.ClearFlags(WindowManagerFlags.TranslucentStatus);
            Window?.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
            mContentView.SystemUiVisibility = (StatusBarVisibility)(SystemUiFlags.LightStatusBar);
            Window?.SetStatusBarColor(Color.Transparent);*/
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                Window?.SetFlags(WindowManagerFlags.LayoutNoLimits, WindowManagerFlags.LayoutNoLimits);
            }

        }
        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                AddOrRemoveEvent(true);
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
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
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
                    Finish();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                IconBack = FindViewById<ImageButton>(Resource.Id.iv_back);

                OfferCoverImage = FindViewById<ImageView>(Resource.Id.offerCoverImage);
                OfferAvatar = FindViewById<ImageView>(Resource.Id.offerAvatar);

                Name = FindViewById<TextView>(Resource.Id.tv_name);
                Subname = FindViewById<TextView>(Resource.Id.tv_subname);
                PageTitle = FindViewById<TextView>(Resource.Id.tv_page_title);

                DateNumber = FindViewById<TextView>(Resource.Id.tv_enddate);
                DiscountNumber = FindViewById<TextView>(Resource.Id.tv_discount_number);

                Description = FindViewById<SuperTextView>(Resource.Id.description);

                IconMore = FindViewById<ImageButton>(Resource.Id.iconMore);

                ReadMoreOption = new StReadMoreOption.Builder()
                    .TextLength(400, StReadMoreOption.TypeCharacter)
                    .MoreLabel(GetText(Resource.String.Lbl_ReadMore))
                    .LessLabel(GetText(Resource.String.Lbl_ReadLess))
                    .MoreLabelColor(Color.ParseColor(AppSettings.MainColor))
                    .LessLabelColor(Color.ParseColor(AppSettings.MainColor))
                    .LabelUnderLine(true)
                    .Build();

                ShareLinearLayout = FindViewById<LinearLayout>(Resource.Id.ShareLinearLayout);
                ShareLinearLayout.Visibility = ViewStates.Gone;

                CommentLinearLayout = FindViewById<LinearLayout>(Resource.Id.CommentLinearLayout);
                SecondReactionLinearLayout = FindViewById<LinearLayout>(Resource.Id.SecondReactionLinearLayout);
                ReactLinearLayout = FindViewById<LinearLayout>(Resource.Id.ReactLinearLayout);
                LikeButton = FindViewById<ReactButton>(Resource.Id.ReactButton);

                SecondReactionButton = FindViewById<TextView>(Resource.Id.SecondReactionText);

                MainSectionButton = FindViewById<LinearLayout>(Resource.Id.linerSecondReaction);
                switch (AppSettings.PostButton)
                {
                    case PostButtonSystem.Reaction:
                    case PostButtonSystem.Like:
                        MainSectionButton.WeightSum = 2;

                        SecondReactionLinearLayout.Visibility = ViewStates.Gone;
                        break;
                    case PostButtonSystem.Wonder:
                        MainSectionButton.WeightSum = 3;

                        SecondReactionLinearLayout.Visibility = ViewStates.Visible;

                        SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(Resource.Drawable.icon_post_wonder_vector, 0, 0, 0);
                        SecondReactionButton.Text = Application.Context.GetText(Resource.String.Btn_Wonder);
                        break;
                    case PostButtonSystem.DisLike:
                        MainSectionButton.WeightSum = 3;

                        SecondReactionLinearLayout.Visibility = ViewStates.Visible;
                        SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(Resource.Drawable.icon_post_dislike_vector, 0, 0, 0);
                        SecondReactionButton.Text = Application.Context.GetText(Resource.String.Btn_Dislike);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitToolbar()
        {
            try
            {
                var toolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolBar != null)
                {
                    toolBar.Title = "";
                    toolBar.SetTitleTextColor(WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                    SetSupportActionBar(toolBar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
                    var icon = AppCompatResources.GetDrawable(this, AppSettings.FlowDirectionRightToLeft ? Resource.Drawable.icon_back_arrow_right : Resource.Drawable.icon_back_arrow_left);
                    icon?.SetTint(WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                    SupportActionBar.SetHomeAsUpIndicator(icon);

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
                CommentsRecyclerView = FindViewById<RecyclerView>(Resource.Id.recycler_view);
                CommentsRecyclerView.NestedScrollingEnabled = false;

                MAdapter = new CommentAdapter(this)
                {
                    CommentList = new ObservableCollection<CommentObjectExtra>()
                };

                CommentsRecyclerView.SetLayoutManager(new LinearLayoutManager(this));
                CommentsRecyclerView.HasFixedSize = true;
                CommentsRecyclerView.SetItemViewCacheSize(10);
                CommentsRecyclerView.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<CommentObjectExtra>(this, MAdapter, sizeProvider, 10);
                CommentsRecyclerView.AddOnScrollListener(preLoader);
                CommentsRecyclerView.SetAdapter(MAdapter);

                Task.Factory.StartNew(StartApiService);
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
                        IconMore.Click += TxtMoreOnClick;
                        IconBack.Click += IconBackOnClick;
                        Description.LongClick += DescriptionOnLongClick;

                        CommentLinearLayout.Click += BtnCommentOnClick;

                        switch (AppSettings.PostButton)
                        {
                            case PostButtonSystem.Wonder:
                            case PostButtonSystem.DisLike:
                                SecondReactionLinearLayout.Click += SecondReactionLinearLayoutOnClick;
                                break;
                        }

                        LikeButton.Click += (sender, args) => LikeButton.ClickLikeAndDisLike(new GlobalClickEventArgs
                        {
                            NewsFeedClass = PostData,
                        }, null, "OffersViewActivity");

                        switch (AppSettings.PostButton)
                        {
                            case PostButtonSystem.Reaction:
                                LikeButton.LongClick += (sender, args) => LikeButton.LongClickDialog(new GlobalClickEventArgs
                                {
                                    NewsFeedClass = PostData,
                                }, null, "OffersViewActivity");
                                break;
                        }

                        break;
                    default:
                        IconMore.Click -= TxtMoreOnClick;
                        IconBack.Click -= IconBackOnClick;
                        Description.LongClick -= DescriptionOnLongClick;

                        CommentLinearLayout.Click -= BtnCommentOnClick;

                        switch (AppSettings.PostButton)
                        {
                            case PostButtonSystem.Wonder:
                            case PostButtonSystem.DisLike:
                                SecondReactionLinearLayout.Click -= SecondReactionLinearLayoutOnClick;
                                break;
                        }

                        LikeButton.Click += null!;
                        switch (AppSettings.PostButton)
                        {
                            case PostButtonSystem.Reaction:
                                LikeButton.LongClick -= null!;
                                break;
                        }

                        break;
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
                IconBack = null!;
                OfferCoverImage = null!;
                OfferAvatar = null!;
                Name = null!;
                Subname = null!;
                PageTitle = null!;
                DateNumber = null!;
                DiscountNumber = null!;
                Description = null!;
                IconMore = null!;
                DataInfoObject = null!;
                ReadMoreOption = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        #endregion

        #region Events

        private void DescriptionOnLongClick(object sender, View.LongClickEventArgs e)
        {
            try
            {
                if (Methods.FunString.StringNullRemover(DataInfoObject.Description) != "Empty")
                {
                    Methods.CopyToClipboard(this, DataInfoObject.Description);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void IconBackOnClick(object sender, EventArgs e)
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

        private void TxtMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialAlertDialogBuilder(this);

                arrayAdapter.Add(GetText(Resource.String.Lbl_Edit));
                arrayAdapter.Add(GetText(Resource.String.Lbl_Delete));

                dialogList.SetTitle(GetText(Resource.String.Lbl_More));
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                dialogList.Show();
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
                string text = itemString;
                if (text == GetText(Resource.String.Lbl_Edit))
                {
                    //Open Edit offer
                    var intent = new Intent(this, typeof(EditOffersActivity));
                    intent.PutExtra("OfferId", DataInfoObject.Id);
                    intent.PutExtra("OfferItem", JsonConvert.SerializeObject(DataInfoObject));
                    StartActivityForResult(intent, 246);
                }
                else if (text == GetText(Resource.String.Lbl_Delete))
                {
                    var dialogBuilder = new MaterialAlertDialogBuilder(this);
                    dialogBuilder.SetTitle(Resource.String.Lbl_Warning);
                    dialogBuilder.SetMessage(GetText(Resource.String.Lbl_DeleteOffers));
                    dialogBuilder.SetPositiveButton(GetText(Resource.String.Lbl_Yes), (sender, args) =>
                    {
                        try
                        {
                            // Send Api delete  
                            if (Methods.CheckConnectivity())
                            {
                                var adapterGlobal = WRecyclerView.GetInstance()?.NativeFeedAdapter;
                                var diff = adapterGlobal?.ListDiffer;
                                var dataGlobal = diff?.Where(a => a.PostData?.OfferId == DataInfoObject?.Id).ToList();
                                if (dataGlobal != null)
                                {
                                    foreach (var postData in dataGlobal)
                                    {
                                        WRecyclerView.GetInstance()?.RemoveByRowIndex(postData);
                                    }
                                }

                                var recycler = TabbedMainActivity.GetInstance()?.NewsFeedTab?.MainRecyclerView;
                                var dataGlobal2 = recycler?.NativeFeedAdapter.ListDiffer?.Where(a => a.PostData?.OfferId == DataInfoObject?.Id).ToList();
                                if (dataGlobal2 != null)
                                {
                                    foreach (var postData in dataGlobal2)
                                    {
                                        recycler.RemoveByRowIndex(postData);
                                    }
                                }

                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_postSuccessfullyDeleted), ToastLength.Short);
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Offers.DeleteOfferAsync(DataInfoObject.Id) });
                            }
                            else
                            {
                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                            }
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    });
                    dialogBuilder.SetNegativeButton(GetText(Resource.String.Lbl_No), new MaterialDialogUtils());

                    dialogBuilder.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Add Comment
        private void BtnCommentOnClick(object sender, EventArgs e)
        {
            try
            {
                ClickListener.CommentPostClick(new GlobalClickEventArgs
                {
                    NewsFeedClass = PostData,
                });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Add Wonder / Disliked
        private void SecondReactionLinearLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    return;
                }

                switch (UserDetails.SoundControl)
                {
                    case true:
                        Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("select.mp3");
                        break;
                }

                switch (AppSettings.PostButton)
                {
                    case PostButtonSystem.Wonder when PostData.IsWondered != null && (bool)PostData.IsWondered:
                        {
                            var x = Convert.ToInt32(PostData.PostWonders);
                            switch (x)
                            {
                                case > 0:
                                    x--;
                                    break;
                                default:
                                    x = 0;
                                    break;
                            }

                            PostData.IsWondered = false;
                            PostData.PostWonders = Convert.ToString(x, CultureInfo.InvariantCulture);

                            var unwrappedDrawable = AppCompatResources.GetDrawable(this, Resource.Drawable.icon_post_wonder_vector);
                            var wrappedDrawable = DrawableCompat.Wrap(unwrappedDrawable);
                            switch (Build.VERSION.SdkInt)
                            {
                                case <= BuildVersionCodes.Lollipop:
                                    DrawableCompat.SetTint(wrappedDrawable, Color.ParseColor("#666666"));
                                    break;
                                default:
                                    wrappedDrawable = wrappedDrawable.Mutate();
                                    wrappedDrawable.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#666666"), PorterDuff.Mode.SrcAtop));
                                    break;
                            }

                            SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable, null, null, null);

                            SecondReactionButton.Text = GetString(Resource.String.Btn_Wonder);
                            SecondReactionButton.SetTextColor(Color.ParseColor("#666666"));
                            break;
                        }
                    case PostButtonSystem.Wonder:
                        {
                            var x = Convert.ToInt32(PostData.PostWonders);
                            x++;

                            PostData.PostWonders = Convert.ToString(x, CultureInfo.InvariantCulture);
                            PostData.IsWondered = true;

                            var unwrappedDrawable = AppCompatResources.GetDrawable(this, Resource.Drawable.icon_post_wonder_vector);
                            var wrappedDrawable = DrawableCompat.Wrap(unwrappedDrawable);
                            switch (Build.VERSION.SdkInt)
                            {
                                case <= BuildVersionCodes.Lollipop:
                                    DrawableCompat.SetTint(wrappedDrawable, Color.ParseColor("#f89823"));
                                    break;
                                default:
                                    wrappedDrawable = wrappedDrawable.Mutate();
                                    wrappedDrawable.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#f89823"), PorterDuff.Mode.SrcAtop));
                                    break;
                            }

                            SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable, null, null, null);

                            SecondReactionButton.Text = GetString(Resource.String.Lbl_wondered);
                            SecondReactionButton.SetTextColor(Color.ParseColor("#f89823"));

                            PostData.Reaction ??= new Reaction();
                            if (PostData.Reaction.IsReacted != null && PostData.Reaction.IsReacted.Value)
                            {
                                PostData.Reaction.IsReacted = false;
                            }

                            break;
                        }
                    case PostButtonSystem.DisLike when PostData.IsWondered != null && PostData.IsWondered.Value:
                        {
                            var unwrappedDrawable = AppCompatResources.GetDrawable(this, Resource.Drawable.icon_post_dislike_vector);
                            var wrappedDrawable = DrawableCompat.Wrap(unwrappedDrawable);
                            switch (Build.VERSION.SdkInt)
                            {
                                case <= BuildVersionCodes.Lollipop:
                                    DrawableCompat.SetTint(wrappedDrawable, Color.ParseColor("#666666"));
                                    break;
                                default:
                                    wrappedDrawable = wrappedDrawable.Mutate();
                                    wrappedDrawable.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#666666"), PorterDuff.Mode.SrcAtop));
                                    break;
                            }

                            SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable, null, null, null);

                            SecondReactionButton.Text = GetString(Resource.String.Btn_Dislike);
                            SecondReactionButton.SetTextColor(Color.ParseColor("#666666"));

                            var x = Convert.ToInt32(PostData.PostWonders);
                            switch (x)
                            {
                                case > 0:
                                    x--;
                                    break;
                                default:
                                    x = 0;
                                    break;
                            }

                            PostData.IsWondered = false;
                            PostData.PostWonders = Convert.ToString(x, CultureInfo.InvariantCulture);
                            break;
                        }
                    case PostButtonSystem.DisLike:
                        {
                            var x = Convert.ToInt32(PostData.PostWonders);
                            x++;

                            PostData.PostWonders = Convert.ToString(x, CultureInfo.InvariantCulture);
                            PostData.IsWondered = true;

                            Drawable unwrappedDrawable = AppCompatResources.GetDrawable(this, Resource.Drawable.icon_post_dislike_vector);
                            Drawable wrappedDrawable = DrawableCompat.Wrap(unwrappedDrawable);

                            switch (Build.VERSION.SdkInt)
                            {
                                case <= BuildVersionCodes.Lollipop:
                                    DrawableCompat.SetTint(wrappedDrawable, Color.ParseColor("#f89823"));
                                    break;
                                default:
                                    wrappedDrawable = wrappedDrawable.Mutate();
                                    wrappedDrawable.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#f89823"), PorterDuff.Mode.SrcAtop));
                                    break;
                            }

                            SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable, null, null, null);

                            SecondReactionButton.Text = GetString(Resource.String.Lbl_disliked);
                            SecondReactionButton.SetTextColor(Color.ParseColor("#f89823"));

                            PostData.Reaction ??= new Reaction();
                            if (PostData.Reaction.IsReacted != null && PostData.Reaction.IsReacted.Value)
                            {
                                PostData.Reaction.IsReacted = false;
                            }

                            break;
                        }
                }

                var adapterGlobal = WRecyclerView.GetInstance()?.NativeFeedAdapter;

                var dataGlobal = adapterGlobal?.ListDiffer?.Where(a => a.PostData?.Id == PostData.Id).ToList();
                switch (dataGlobal?.Count)
                {
                    case > 0:
                        {
                            foreach (var dataClass in from dataClass in dataGlobal let index = adapterGlobal.ListDiffer.IndexOf(dataClass) where index > -1 select dataClass)
                            {
                                dataClass.PostData = PostData;
                                adapterGlobal.NotifyItemChanged(adapterGlobal.ListDiffer.IndexOf(dataClass), "reaction");
                            }

                            break;
                        }
                }

                switch (AppSettings.PostButton)
                {
                    case PostButtonSystem.Wonder:
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Posts.PostActionsAsync(PostData.Id, "wonder") });
                        break;
                    case PostButtonSystem.DisLike:
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Posts.PostActionsAsync(PostData.Id, "dislike") });
                        break;
                }

            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region  Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                switch (requestCode)
                {
                    case 246 when resultCode == Result.Ok:
                        {
                            var offersItem = data.GetStringExtra("OffersItem") ?? "";
                            if (string.IsNullOrEmpty(offersItem)) return;
                            var dataObject = JsonConvert.DeserializeObject<OffersDataObject>(offersItem);
                            if (dataObject != null)
                            {
                                DataInfoObject.DiscountType = dataObject.DiscountType;
                                DataInfoObject.Currency = dataObject.Currency;
                                DataInfoObject.ExpireDate = dataObject.ExpireDate;
                                DataInfoObject.Time = dataObject.Time;
                                DataInfoObject.Description = dataObject.Description;
                                DataInfoObject.DiscountedItems = dataObject.DiscountedItems;
                                DataInfoObject.Description = dataObject.Description;
                                DataInfoObject.DiscountPercent = dataObject.DiscountPercent;
                                DataInfoObject.DiscountAmount = dataObject.DiscountAmount;
                                DataInfoObject.DiscountPercent = dataObject.DiscountPercent;
                                DataInfoObject.Buy = dataObject.Buy;
                                DataInfoObject.GetPrice = dataObject.GetPrice;
                                DataInfoObject.Spend = dataObject.Spend;
                                DataInfoObject.AmountOff = dataObject.AmountOff;

                                BindOfferPost();
                            }

                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private void BindOfferPost()
        {
            try
            {
                DataInfoObject = JsonConvert.DeserializeObject<OffersDataObject>(Intent?.GetStringExtra("OffersObject") ?? "");
                if (DataInfoObject != null)
                {
                    PostId = DataInfoObject.Id;

                    DataInfoObject.IsOwner = DataInfoObject.UserId == UserDetails.UserId;

                    GlideImageLoader.LoadImage(this, DataInfoObject.Page.Avatar, OfferAvatar, ImageStyle.RoundedCrop, ImagePlaceholders.DrawableUser);
                    GlideImageLoader.LoadImage(this, DataInfoObject.Image, OfferCoverImage, ImageStyle.FitCenter, ImagePlaceholders.Drawable);

                    IconMore.Visibility = DataInfoObject.IsOwner ? ViewStates.Visible : ViewStates.Gone;

                    if (DataInfoObject.Page != null)
                    {
                        Name.Text = Methods.FunString.DecodeString(DataInfoObject.Page.Name);
                        Subname.Text = "@" + Methods.FunString.DecodeString(DataInfoObject.Page.PageName);
                    }

                    //Set Description
                    var description = Methods.FunString.DecodeString(DataInfoObject.Description);
                    Description.Text = description;
                    ReadMoreOption.AddReadMoreTo(Description, new String(description));

                    PageTitle.Text = Methods.FunString.DecodeString(DataInfoObject.OfferText) + " " + Methods.FunString.DecodeString(DataInfoObject.DiscountedItems);
                    DiscountNumber.Text = Methods.FunString.DecodeString(DataInfoObject.OfferText);
                    DateNumber.Text = DataInfoObject.ExpireDate;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadPostDataAsync, () => LoadDataComment("0") });
        }

        private async Task LoadPostDataAsync()
        {
            if (Methods.CheckConnectivity())
            {
                var (apiStatus, respond) = await RequestsAsync.Posts.GetPostDataAsync(PostId, "post_data");
                if (apiStatus == 200)
                {
                    if (respond is GetPostDataObject result)
                    {
                        PostData = result.PostData;

                        RunOnUiThread(() =>
                        {
                            try
                            {
                                //if (LikeButton != null)
                                //    LikeButton.Text = PostData.PostLikes;

                                switch (AppSettings.PostButton)
                                {
                                    case PostButtonSystem.Reaction:
                                        {
                                            PostData.Reaction ??= new Reaction();

                                            if (PostData.Reaction.IsReacted != null && PostData.Reaction.IsReacted.Value)
                                            {
                                                switch (string.IsNullOrEmpty(PostData.Reaction.Type))
                                                {
                                                    case false:
                                                        {
                                                            var react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Id == PostData.Reaction.Type).Value?.Id ?? "";
                                                            switch (react)
                                                            {
                                                                case "1":
                                                                    LikeButton.SetReactionPack(ReactConstants.Like);
                                                                    break;
                                                                case "2":
                                                                    LikeButton.SetReactionPack(ReactConstants.Love);
                                                                    break;
                                                                case "3":
                                                                    LikeButton.SetReactionPack(ReactConstants.HaHa);
                                                                    break;
                                                                case "4":
                                                                    LikeButton.SetReactionPack(ReactConstants.Wow);
                                                                    break;
                                                                case "5":
                                                                    LikeButton.SetReactionPack(ReactConstants.Sad);
                                                                    break;
                                                                case "6":
                                                                    LikeButton.SetReactionPack(ReactConstants.Angry);
                                                                    break;
                                                                default:
                                                                    LikeButton.SetReactionPack(ReactConstants.Default);
                                                                    break;
                                                            }

                                                            break;
                                                        }
                                                }
                                            }
                                            else
                                                LikeButton.SetReactionPack(ReactConstants.Default);

                                            break;
                                        }
                                    default:
                                        {
                                            if (PostData.Reaction.IsReacted != null && !PostData.Reaction.IsReacted.Value)
                                                LikeButton.SetReactionPack(ReactConstants.Default);

                                            if (PostData.IsLiked != null && PostData.IsLiked.Value)
                                                LikeButton.SetReactionPack(ReactConstants.Like);

                                            if (SecondReactionButton != null)
                                            {
                                                switch (AppSettings.PostButton)
                                                {
                                                    case PostButtonSystem.Wonder when PostData.IsWondered != null && PostData.IsWondered.Value:
                                                        {
                                                            Drawable unwrappedDrawable = AppCompatResources.GetDrawable(this, Resource.Drawable.icon_post_wonder_vector);
                                                            Drawable wrappedDrawable = DrawableCompat.Wrap(unwrappedDrawable);
                                                            switch (Build.VERSION.SdkInt)
                                                            {
                                                                case <= BuildVersionCodes.Lollipop:
                                                                    DrawableCompat.SetTint(wrappedDrawable, Color.ParseColor("#f89823"));
                                                                    break;
                                                                default:
                                                                    wrappedDrawable = wrappedDrawable.Mutate();
                                                                    wrappedDrawable.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#f89823"), PorterDuff.Mode.SrcAtop));
                                                                    break;
                                                            }

                                                            SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable, null, null, null);

                                                            SecondReactionButton.Text = GetString(Resource.String.Lbl_wondered);
                                                            SecondReactionButton.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                                                            break;
                                                        }
                                                    case PostButtonSystem.Wonder:
                                                        {
                                                            Drawable unwrappedDrawable = AppCompatResources.GetDrawable(this, Resource.Drawable.icon_post_wonder_vector);
                                                            Drawable wrappedDrawable = DrawableCompat.Wrap(unwrappedDrawable);
                                                            switch (Build.VERSION.SdkInt)
                                                            {
                                                                case <= BuildVersionCodes.Lollipop:
                                                                    DrawableCompat.SetTint(wrappedDrawable, Color.ParseColor("#666666"));
                                                                    break;
                                                                default:
                                                                    wrappedDrawable = wrappedDrawable.Mutate();
                                                                    wrappedDrawable.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#666666"), PorterDuff.Mode.SrcAtop));
                                                                    break;
                                                            }
                                                            SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable, null, null, null);

                                                            SecondReactionButton.Text = GetString(Resource.String.Btn_Wonder);
                                                            SecondReactionButton.SetTextColor(Color.ParseColor("#444444"));
                                                            break;
                                                        }
                                                    case PostButtonSystem.DisLike when PostData.IsWondered != null && PostData.IsWondered.Value:
                                                        {
                                                            Drawable unwrappedDrawable = AppCompatResources.GetDrawable(this, Resource.Drawable.icon_post_dislike_vector);
                                                            Drawable wrappedDrawable = DrawableCompat.Wrap(unwrappedDrawable);

                                                            switch (Build.VERSION.SdkInt)
                                                            {
                                                                case <= BuildVersionCodes.Lollipop:
                                                                    DrawableCompat.SetTint(wrappedDrawable, Color.ParseColor("#f89823"));
                                                                    break;
                                                                default:
                                                                    wrappedDrawable = wrappedDrawable.Mutate();
                                                                    wrappedDrawable.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#f89823"), PorterDuff.Mode.SrcAtop));
                                                                    break;
                                                            }

                                                            SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable, null, null, null);

                                                            SecondReactionButton.Text = GetString(Resource.String.Lbl_disliked);
                                                            SecondReactionButton.SetTextColor(Color.ParseColor("#f89823"));
                                                            break;
                                                        }
                                                    case PostButtonSystem.DisLike:
                                                        {
                                                            Drawable unwrappedDrawable = AppCompatResources.GetDrawable(this, Resource.Drawable.icon_post_dislike_vector);
                                                            Drawable wrappedDrawable = DrawableCompat.Wrap(unwrappedDrawable);
                                                            switch (Build.VERSION.SdkInt)
                                                            {
                                                                case <= BuildVersionCodes.Lollipop:
                                                                    DrawableCompat.SetTint(wrappedDrawable, Color.ParseColor("#666666"));
                                                                    break;
                                                                default:
                                                                    wrappedDrawable = wrappedDrawable.Mutate();
                                                                    wrappedDrawable.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#666666"), PorterDuff.Mode.SrcAtop));
                                                                    break;
                                                            }

                                                            SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable, null, null, null);

                                                            SecondReactionButton.Text = GetString(Resource.String.Btn_Dislike);
                                                            SecondReactionButton.SetTextColor(Color.ParseColor("#444444"));
                                                            break;
                                                        }
                                                }
                                            }

                                            break;
                                        }
                                }
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                    }
                }
                else
                    Methods.DisplayReportResult(this, respond);
            }
            else
            {
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            }
        }

        #region LoadDataComment

        private async Task LoadDataComment(string offset)
        {
            if (Methods.CheckConnectivity())
            {
                var countList = MAdapter.CommentList.Count;
                var (apiStatus, respond) = await RequestsAsync.Comment.GetPostCommentsAsync(PostId, "10", offset);
                if (apiStatus != 200 || respond is not CommentObject result || result.CommentList == null)
                {
                    Methods.DisplayReportResult(this, respond);
                }
                else
                {
                    var respondList = result.CommentList?.Count;
                    switch (respondList)
                    {
                        case > 0:
                            {
                                foreach (var item in from item in result.CommentList let check = MAdapter.CommentList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    var db = ClassMapper.Mapper?.Map<CommentObjectExtra>(item);
                                    if (db != null) MAdapter.CommentList.Add(db);
                                }

                                switch (countList)
                                {
                                    case > 0:
                                        RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.CommentList.Count - countList); });
                                        break;
                                    default:
                                        RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                                        break;
                                }

                                break;
                            }
                    }
                }

                RunOnUiThread(ShowEmptyPage2);
            }
        }

        private void ShowEmptyPage2()
        {
            try
            {
                switch (MAdapter.CommentList.Count)
                {
                    case > 0:
                        {
                            CommentsRecyclerView.Visibility = ViewStates.Visible;

                            var emptyStateChecker = MAdapter.CommentList.FirstOrDefault(a => a.Text == MAdapter.EmptyState);
                            if (emptyStateChecker != null && MAdapter.CommentList.Count > 1)
                            {
                                MAdapter.CommentList.Remove(emptyStateChecker);
                                MAdapter.NotifyDataSetChanged();
                            }

                            break;
                        }
                    default:
                        {
                            CommentsRecyclerView.Visibility = ViewStates.Gone;

                            MAdapter.CommentList.Clear();
                            var d = new CommentObjectExtra { Text = MAdapter.EmptyState };
                            MAdapter.CommentList.Add(d);
                            MAdapter.NotifyDataSetChanged();
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

    }
}