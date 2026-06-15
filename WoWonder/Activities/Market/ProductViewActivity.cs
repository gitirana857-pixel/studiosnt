using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Graphics.Drawable;
using AndroidX.RecyclerView.Widget;
using AndroidX.ViewPager.Widget;
using Bumptech.Glide.Util;
using Google.Android.Material.Dialog;
using Me.Relex.CircleIndicatorLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using WoWonder.Activities.Base;
using WoWonder.Activities.Comment.Adapters;
using WoWonder.Activities.Market.Adapters;
using WoWonder.Activities.MyProfile;
using WoWonder.Activities.NativePost.Extra;
using WoWonder.Activities.NativePost.Pages;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Activities.Tabbes;
using WoWonder.Activities.UserProfile;
using WoWonder.Activities.UsersPages;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using WoWonder.Library.Anjo.SuperTextLibrary;
using WoWonder.SQLite;
using WoWonderClient.Classes.Comments;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Message;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Classes.Product;
using WoWonderClient.Requests;
using Exception = System.Exception;
using Reaction = WoWonderClient.Classes.Posts.Reaction;
using String = Java.Lang.String;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Market
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class ProductViewActivity : BaseActivity, IDialogListCallBack, StTools.IXAutoLinkOnClickListener
    {
        #region Variables Basic

        private TextView TxtProductName, TxtProductPrice, TxtTypeProduct, TxtStatusProduct, TxtLocationProduct, TxtReviewsProduct, TxtCategoryProduct;
        private SuperTextView TxtProductDescription;

        private TextView TxtUserName;
        private ImageView ImageMore, UserImageAvatar;
        private AppCompatButton BtnBuy, BtnFollow;

        private LinearLayout ContactSellerLayout, ShareLayout;

        private ProductDataObject ProductData;

        private ViewPager ViewPagerView;
        private CircleIndicator CircleIndicatorView;
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
                SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.ProductView_Layout);

                PostId = Intent?.GetStringExtra("Id") ?? string.Empty;

                ClickListener = new PostClickListener(this, NativeFeedType.Global);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                ProductData = JsonConvert.DeserializeObject<ProductDataObject>(Intent?.GetStringExtra("ProductView") ?? "");
                switch (ProductData)
                {
                    case null:
                        return;
                    default:
                        Get_Data_Product(ProductData);
                        break;
                }
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
                ImageMore = (ImageView)FindViewById(Resource.Id.more);

                ViewPagerView = FindViewById<ViewPager>(Resource.Id.pager);
                CircleIndicatorView = FindViewById<CircleIndicator>(Resource.Id.indicator);

                TxtProductName = FindViewById<TextView>(Resource.Id.tv_name);
                TxtProductPrice = (TextView)FindViewById(Resource.Id.tv_price);
                BtnBuy = (AppCompatButton)FindViewById(Resource.Id.btnBuy);
                ContactSellerLayout = (LinearLayout)FindViewById(Resource.Id.ll_ContactSeller);
                ShareLayout = (LinearLayout)FindViewById(Resource.Id.ll_share);

                UserImageAvatar = (ImageView)FindViewById(Resource.Id.user_pic);
                TxtUserName = (TextView)FindViewById(Resource.Id.user_name);
                BtnFollow = (AppCompatButton)FindViewById(Resource.Id.btnFollow);

                TxtTypeProduct = (TextView)FindViewById(Resource.Id.TypeTextView);
                TxtStatusProduct = (TextView)FindViewById(Resource.Id.StatusTextView);
                TxtLocationProduct = (TextView)FindViewById(Resource.Id.locationTextView);
                TxtReviewsProduct = (TextView)FindViewById(Resource.Id.ReviewsTextView);
                TxtCategoryProduct = (TextView)FindViewById(Resource.Id.CategoryTextView);

                TxtProductDescription = (SuperTextView)FindViewById(Resource.Id.tv_description);
                TxtProductDescription.SetAutoLinkOnClickListener(this, new Dictionary<string, string>());

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
                    toolBar.Title = " ";
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

        public void ViewPagerViewOnClick()
        {
            try
            {
                var intent = new Intent(this, typeof(MultiImagesPostViewerActivity));
                intent.PutExtra("indexImage", "0"); // Index Image Show
                intent.PutExtra("TypePost", "Product"); // Index Image Show
                intent.PutExtra("AlbumObject", JsonConvert.SerializeObject(PostData)); // PostDataObject

                if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
                {
                    ActivityOptions options = ActivityOptions.MakeCustomAnimation(this, Resource.Animation.abc_popup_enter, Resource.Animation.popup_exit);
                    StartActivity(intent, options?.ToBundle());
                }
                else
                {
                    OverridePendingTransition(Resource.Animation.abc_popup_enter, Resource.Animation.popup_exit);
                    StartActivity(intent);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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
                        {
                            ShareLayout.Click += ShareLayoutOnClick;
                            BtnFollow.Click += BtnFollowOnClick;
                            BtnBuy.Click += BtnBuyOnClick;
                            ContactSellerLayout.Click += BtnContactOnClick;
                            UserImageAvatar.Click += UserImageAvatarOnClick;
                            TxtUserName.Click += UserImageAvatarOnClick;
                            ImageMore.Click += ImageMoreOnClick;
                            TxtProductDescription.LongClick += TxtProductDescriptionOnLongClick;
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
                            }, null, "ProductViewActivity");

                            switch (AppSettings.PostButton)
                            {
                                case PostButtonSystem.Reaction:
                                    LikeButton.LongClick += (sender, args) => LikeButton.LongClickDialog(new GlobalClickEventArgs
                                    {
                                        NewsFeedClass = PostData,
                                    }, null, "ProductViewActivity");
                                    break;
                            }
                            break;
                        }
                    default:
                        {
                            ShareLayout.Click -= ShareLayoutOnClick;
                            BtnBuy.Click -= BtnBuyOnClick;
                            ContactSellerLayout.Click -= BtnContactOnClick;
                            UserImageAvatar.Click -= UserImageAvatarOnClick;
                            TxtUserName.Click -= UserImageAvatarOnClick;
                            ImageMore.Click -= ImageMoreOnClick;
                            TxtProductDescription.LongClick -= TxtProductDescriptionOnLongClick;
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
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void ShareLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                ClickListener.SharePostClick(new GlobalClickEventArgs { NewsFeedClass = PostData, }, PostModelType.ProductPost);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void DestroyBasic()
        {
            try
            {
                ViewPagerView = null!;
                CircleIndicatorView = null!;
                TxtProductName = null!;
                TxtProductPrice = null!;
                TxtProductDescription = null!;
                UserImageAvatar = null!;
                MainSectionButton = null!;
                SecondReactionLinearLayout = null!;
                LikeButton = null!;
                ClickListener = null!;
                PostData = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void BtnBuyOnClick(object sender, EventArgs e)
        {
            try
            {
                if (BtnBuy.Tag?.ToString() == "Edit")
                {
                    EditInfoProduct_OnClick();
                }
                else if (BtnBuy.Tag?.ToString() == "true")
                {
                    BtnBuy.Text = GetText(Resource.String.Lbl_AddToCart);
                    BtnBuy.Tag = "false";
                    ProductData.AddedToCart = "0";

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Market.AddToCartAsync(ProductData.Id, "remove_cart") });

                    TabbedMarketActivity.GetInstance()?.MarketTab?.UpdateBadgeViewIcon(false);
                }
                else if (BtnBuy.Tag?.ToString() == "false")
                {
                    BtnBuy.Text = GetText(Resource.String.Lbl_RemoveFromCart);
                    BtnBuy.Tag = "true";
                    ProductData.AddedToCart = "1";

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Market.AddToCartAsync(ProductData.Id, "add_cart") });

                    TabbedMarketActivity.GetInstance()?.MarketTab?.UpdateBadgeViewIcon(true);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnFollowOnClick(object sender, EventArgs e)
        {
            try
            {
                WoWonderTools.SetAddFriend(this, ProductData.Seller, BtnFollow);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtProductDescriptionOnLongClick(object sender, View.LongClickEventArgs e)
        {
            try
            {
                Methods.CopyToClipboard(this, ProductData.Description);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Contact seller 
        private void BtnContactOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                if (!WoWonderTools.ChatIsAllowed(ProductData.Seller))
                    return;

                if (AppSettings.MessengerIntegration)
                {
                    if (AppSettings.ShowDialogAskOpenMessenger)
                    {
                        var dialog = new MaterialAlertDialogBuilder(this);

                        dialog.SetTitle(Resource.String.Lbl_Warning);
                        dialog.SetMessage(GetText(Resource.String.Lbl_ContentAskOPenAppMessenger));
                        dialog.SetPositiveButton(GetText(Resource.String.Lbl_Yes), (materialDialog, action) =>
                        {
                            try
                            {
                                Methods.App.OpenAppByPackageName(this, AppSettings.MessengerPackageName, "SendMsgProduct", new ChatObject { UserId = ProductData.Seller.UserId, Avatar = ProductData.Seller.Avatar, Name = ProductData.Seller.Name, LastMessage = new LastMessageUnion { LastMessageClass = new MessageData { ProductId = ProductData.Id, Product = new ProductUnion { ProductClass = ProductData } } } });
                            }
                            catch (Exception exception)
                            {
                                Methods.DisplayReportResultTrack(exception);
                            }
                        });
                        dialog.SetNegativeButton(GetText(Resource.String.Lbl_No), new MaterialDialogUtils());

                        dialog.Show();
                    }
                    else
                    {
                        Methods.App.OpenAppByPackageName(this, AppSettings.MessengerPackageName, "SendMsgProduct", new ChatObject { UserId = ProductData.Seller.UserId, Avatar = ProductData.Seller.Avatar, Name = ProductData.Seller.Name, LastMessage = new LastMessageUnion { LastMessageClass = new MessageData { ProductId = ProductData.Id, Product = new ProductUnion { ProductClass = ProductData } } } });
                    }
                }
                else
                {
                    if (!Methods.CheckConnectivity())
                    {
                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                        return;
                    }

                    var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    var time = unixTimestamp.ToString();

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.SendMessageAsync(ProductData.Seller.UserId, time, "", "", "", "", "", "", ProductData.Id) });
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_MessageSentSuccessfully), ToastLength.Short);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        // Event Open User Profile
        private void UserImageAvatarOnClick(object sender, EventArgs e)
        {
            try
            {
                WoWonderTools.OpenProfile(this, ProductData.Seller.UserId, ProductData.Seller);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event More >> Show Menu (CopeLink , Share)
        private void ImageMoreOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialAlertDialogBuilder(this);

                if (ProductData.Seller.UserId == UserDetails.UserId)
                {
                    arrayAdapter.Add(GetString(Resource.String.Lbl_EditProduct));
                    arrayAdapter.Add(GetString(Resource.String.Lbl_Delete));
                }

                arrayAdapter.Add(GetString(Resource.String.Lbl_CopeLink));
                arrayAdapter.Add(GetString(Resource.String.Lbl_Share));

                dialogList.SetTitle(GetString(Resource.String.Lbl_More));
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                dialogList.Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> Share
        private void OnShare_Button_Click()
        {
            try
            {
                ClickListener.SharePostClick(new GlobalClickEventArgs { NewsFeedClass = PostData, }, PostModelType.ProductPost);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> Edit Info Product if user == is_owner  
        private void EditInfoProduct_OnClick()
        {
            try
            {
                Intent intent = new Intent(this, typeof(EditProductActivity));
                intent.PutExtra("ProductView", JsonConvert.SerializeObject(ProductData));
                StartActivityForResult(intent, 3500);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> Edit Info Product if user == is_owner  
        private void DeleteProduct_OnClick()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var dialog = new MaterialAlertDialogBuilder(this);
                    dialog.SetTitle(GetText(Resource.String.Lbl_DeletePost));
                    dialog.SetMessage(GetText(Resource.String.Lbl_AreYouSureDeletePost));
                    dialog.SetPositiveButton(GetText(Resource.String.Lbl_Yes), (sender, args) =>
                    {
                        try
                        {
                            if (!Methods.CheckConnectivity())
                            {
                                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                                return;
                            }

                            var adapterGlobal = WRecyclerView.GetInstance()?.NativeFeedAdapter;
                            var diff = adapterGlobal?.ListDiffer;
                            var dataGlobal = diff?.Where(a => a.PostData?.PostId == ProductData?.PostId).ToList();
                            if (dataGlobal != null)
                            {
                                foreach (var postData in dataGlobal)
                                {
                                    WRecyclerView.GetInstance()?.RemoveByRowIndex(postData);
                                }
                            }

                            var recycler = TabbedMainActivity.GetInstance()?.NewsFeedTab?.MainRecyclerView;
                            var dataGlobal2 = recycler?.NativeFeedAdapter.ListDiffer?.Where(a => a.PostData?.PostId == ProductData?.PostId).ToList();
                            if (dataGlobal2 != null)
                            {
                                foreach (var postData in dataGlobal2)
                                {
                                    recycler.RemoveByRowIndex(postData);
                                }
                            }

                            var instance = TabbedMarketActivity.GetInstance();

                            var checkMyProductsTab = instance?.MyProductsTab?.MAdapter?.MarketList?.FirstOrDefault(a => a.Product.Id == ProductData.Id && a.Type == Classes.ItemType.MyProduct);
                            if (checkMyProductsTab != null)
                            {
                                instance.MyProductsTab.MAdapter.MarketList?.Remove(checkMyProductsTab);
                                instance.MyProductsTab.MAdapter.NotifyDataSetChanged();
                            }

                            var checkMarketTab = instance?.MarketTab?.MAdapter?.MarketList?.FirstOrDefault(a => a.Product.Id == ProductData.Id && a.Type == Classes.ItemType.Product);
                            if (checkMarketTab != null)
                            {
                                instance.MarketTab.MAdapter.MarketList?.Remove(checkMarketTab);
                                instance.MarketTab.MAdapter.NotifyDataSetChanged();
                            }

                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_postSuccessfullyDeleted), ToastLength.Short);
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Posts.PostActionsAsync(ProductData.PostId, "delete") });

                            Finish();
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }

                    });
                    dialog.SetNegativeButton(GetText(Resource.String.Lbl_No), new MaterialDialogUtils());

                    dialog.Show();
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

        #region MaterialDialog

        public void OnSelection(IDialogInterface dialog, int position, string itemString)
        {
            try
            {
                string text = itemString;
                if (text == GetString(Resource.String.Lbl_CopeLink))
                {
                    Methods.CopyToClipboard(this, ProductData.Url);
                }
                else if (text == GetString(Resource.String.Lbl_Share))
                {
                    OnShare_Button_Click();
                }
                else if (text == GetString(Resource.String.Lbl_EditProduct))
                {
                    EditInfoProduct_OnClick();
                }
                else if (text == GetString(Resource.String.Lbl_Delete))
                {
                    DeleteProduct_OnClick();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);

                switch (requestCode)
                {
                    case 3500 when resultCode == Result.Ok:
                        {
                            if (string.IsNullOrEmpty(data.GetStringExtra("itemData"))) return;
                            var item = JsonConvert.DeserializeObject<ProductDataObject>(data.GetStringExtra("itemData") ?? "");
                            if (item != null)
                            {
                                Get_Data_Product(item);
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

        private void Get_Data_Product(ProductDataObject productData)
        {
            try
            {
                ProductData = productData;

                PostData = new PostDataObject
                {
                    PostId = productData.PostId,
                    Product = new ProductUnion
                    {
                        ProductClass = productData,
                    },
                    ProductId = productData.Id,
                    UserId = productData.UserId,
                    UserData = productData.Seller,
                    Url = productData.Url,
                    PostUrl = productData.Url,
                };

                List<string> listImageUser = new List<string>();
                switch (productData.Images?.Count)
                {
                    case > 0:
                        listImageUser.AddRange(productData.Images.Select(t => t.Image));
                        break;
                    default:
                        listImageUser.Add(productData.Images?[0]?.Image);
                        break;
                }

                switch (ViewPagerView.Adapter)
                {
                    case null:
                        ViewPagerView.Adapter = new MultiImagePagerAdapter(this, listImageUser);
                        ViewPagerView.CurrentItem = 0;
                        CircleIndicatorView.SetViewPager(ViewPagerView);
                        break;
                }
                ViewPagerView.Adapter.NotifyDataSetChanged();

                TxtProductName.Text = Methods.FunString.DecodeString(productData.Name);

                var (currency, currencyIcon) = WoWonderTools.GetCurrency(productData.Currency);
                TxtProductPrice.Text = currencyIcon + productData.Price;
                Console.WriteLine(currency);

                GlideImageLoader.LoadImage(this, productData.Seller.Avatar, UserImageAvatar, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);
                TxtUserName.Text = Methods.FunString.SubStringCutOf(WoWonderTools.GetNameFinal(productData.Seller), 14);

                WoWonderTools.SetAddFriendCondition(productData.Seller, productData.Seller.IsFollowing, BtnFollow);

                var readMoreOption = new StReadMoreOption.Builder()
                    .TextLength(200, StReadMoreOption.TypeCharacter)
                    .MoreLabel(GetText(Resource.String.Lbl_ReadMore))
                    .LessLabel(GetText(Resource.String.Lbl_ReadLess))
                    .MoreLabelColor(Color.ParseColor(AppSettings.MainColor))
                    .LessLabelColor(Color.ParseColor(AppSettings.MainColor))
                    .LabelUnderLine(true)
                    .Build();

                if (Methods.FunString.StringNullRemover(productData.Description) != "Empty")
                {
                    var description = Methods.FunString.DecodeString(productData.Description);
                    readMoreOption.AddReadMoreTo(TxtProductDescription, new String(description));
                }
                else
                {
                    TxtProductDescription.Text = GetText(Resource.String.Lbl_Empty);
                }

                TxtLocationProduct.Text = !string.IsNullOrEmpty(productData.Location) ? Methods.FunString.DecodeString(productData.Location) : GetText(Resource.String.Lbl_Unknown);

                if (productData.Seller.UserId == UserDetails.UserId)
                {
                    BtnBuy.Text = GetText(Resource.String.Lbl_Edit);
                    BtnBuy.Tag = "Edit";
                }
                else
                {
                    if (productData.AddedToCart == "1")
                    {
                        BtnBuy.Text = GetText(Resource.String.Lbl_RemoveFromCart);
                        BtnBuy.Tag = "true";
                    }
                    else
                    {
                        BtnBuy.Text = GetText(Resource.String.Lbl_AddToCart);
                        BtnBuy.Tag = "false";
                    }
                }

                //Type == "0" >>  New // Type != "0"  Used
                TxtTypeProduct.Text = productData.Type == "0" ? GetText(Resource.String.Radio_New) : GetText(Resource.String.Radio_Used);

                // Status InStock
                TxtStatusProduct.Text = productData.Status == "0" ? GetText(Resource.String.Lbl_In_Stock) : "-------";

                TxtReviewsProduct.Text = productData.ReviewsCount + " " + GetText(Resource.String.Lbl_Reviews);

                CategoriesController cat = new CategoriesController();
                TxtCategoryProduct.Text = cat.Get_Translate_Categories_Communities(productData.Category, "", "Products");
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

        public void AutoLinkTextClick(StTools.XAutoLinkMode p0, string p1, Dictionary<string, string> userData)
        {
            try
            {
                p1 = p1.Replace(" ", "").Replace("\n", "");
                var typeText = Methods.FunString.Check_Regex(p1);
                if (typeText == "Email")
                {
                    Methods.App.SendEmail(this, p1);
                }
                else if (typeText == "Website")
                {
                    string url = p1.Contains("http") switch
                    {
                        false => "http://" + p1,
                        _ => p1
                    };

                    //var intent = new Intent(this, typeof(LocalWebViewActivity));
                    //intent.PutExtra("URL", url.Replace(" ", ""));
                    //intent.PutExtra("Type", url.Replace(" ", ""));
                    //this.StartActivity(intent);
                    new IntentController(this).OpenBrowserFromApp(url);
                }
                else if (typeText == "Hashtag")
                {
                    var intent = new Intent(this, typeof(HashTagPostsActivity));
                    intent.PutExtra("Id", p1);
                    intent.PutExtra("Tag", p1);
                    StartActivity(intent);
                }
                else if (typeText == "Mention")
                {
                    var dataUSer = ListUtils.MyProfileList?.FirstOrDefault();
                    string name = p1.Replace("@", "").Replace(" ", "");

                    var sqlEntity = new SqLiteDatabase();
                    var user = sqlEntity.Get_DataOneUser(name);


                    if (user != null)
                    {
                        WoWonderTools.OpenProfile(this, user.UserId, user);
                    }
                    else if (userData?.Count > 0)
                    {
                        var data = userData.FirstOrDefault(a => a.Value == name);
                        if (data.Key != null && data.Key == UserDetails.UserId)
                        {
                            if (PostClickListener.OpenMyProfile)
                            {
                                return;
                            }

                            var intent = new Intent(this, typeof(MyProfileActivity));
                            StartActivity(intent);
                        }
                        else if (data.Key != null)
                        {
                            var intent = new Intent(this, typeof(UserProfileActivity));
                            //intent.PutExtra("UserObject", JsonConvert.SerializeObject(item));
                            intent.PutExtra("UserId", data.Key);
                            StartActivity(intent);
                        }
                    }
                    else
                    {
                        if (name == dataUSer?.Name || name == dataUSer?.Username)
                        {
                            if (PostClickListener.OpenMyProfile)
                            {
                                return;
                            }

                            var intent = new Intent(this, typeof(MyProfileActivity));
                            StartActivity(intent);
                        }
                        else
                        {
                            var intent = new Intent(this, typeof(UserProfileActivity));
                            //intent.PutExtra("UserObject", JsonConvert.SerializeObject(item));
                            intent.PutExtra("name", name);
                            StartActivity(intent);
                        }
                    }
                }
                else if (typeText == "Number")
                {
                    Methods.App.SaveContacts(this, p1, "", "2");
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}