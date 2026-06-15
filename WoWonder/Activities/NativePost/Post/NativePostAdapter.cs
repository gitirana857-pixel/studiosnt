using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Load.Resource.Bitmap;
using Bumptech.Glide.Request;
using Bumptech.Glide.Request.Target;
using Bumptech.Glide.Request.Transition;
using Bumptech.Glide.Signature;
using Bumptech.Glide.Util;
using Java.Lang;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WoWonder.Activities.Comment;
using WoWonder.Activities.Comment.Adapters;
using WoWonder.Activities.MyProfile;
using WoWonder.Activities.NativePost.Extra;
using WoWonder.Activities.NativePost.Pages;
using WoWonder.Activities.UserProfile;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using WoWonder.Library.Anjo.SuperTextLibrary;
using WoWonder.SQLite;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Posts;
using Console = System.Console;
using Exception = System.Exception;
using Math = System.Math;
using Object = Java.Lang.Object;

namespace WoWonder.Activities.NativePost.Post
{
    public class NativePostAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider, StTools.IXAutoLinkOnClickListener
    {
        public readonly AppCompatActivity ActivityContext;

        private RecyclerView MainRecyclerView { get; }
        public NativeFeedType NativePostType { get; set; }
        public string IdParameter { get; private set; }

        public readonly RequestBuilder FullGlideRequestBuilder;
        public readonly RequestBuilder CircleGlideRequestBuilder;

        public List<AdapterModelsClass> ListDiffer { get; set; }
        private PreCachingLayoutManager PreCachingLayout { get; set; }

        private RecyclerView.RecycledViewPool RecycledViewPool { get; set; }

        public readonly PostClickListener PostClickListener;
        public AdapterHolders.StoryViewHolder HolderStory { get; private set; }
        public StReadMoreOption ReadMoreOption { get; }
        //private IOnLoadMoreListener OnLoadMoreListener;
        //private bool Loading;
        public int PositionSound;
        private readonly AdapterBind AdapterBind;
        //private int HeaderCount=0;

        public ViewPreloadSizeProvider preloadSizeProvider;
        public CustomPreloadSizeProvider customPreloadSizeProvider;
        public AsyncViewHoldersContainers AsyncViewHoldersPreLoaders { get; set; }

        public List<AdapterHolders.PostDefaultSectionViewHolder> ListOfholders = new List<AdapterHolders.PostDefaultSectionViewHolder>();
        public List<AdapterHolders.PostTopSectionViewHolder> ListOfHeaderholders = new List<AdapterHolders.PostTopSectionViewHolder>();
        public List<AdapterHolders.PostBottomSectionViewHolder> ListOfBottomPostPartholders = new List<AdapterHolders.PostBottomSectionViewHolder>();
        public List<AdapterHolders.PostTextSectionViewHolder> ListOfTextSectionPostPartholders = new List<AdapterHolders.PostTextSectionViewHolder>();

        public RequestOptions GlideNormalOptions;
        public RequestOptions GlideCircleOptions;
        public RequestBuilder GlideThumbnailRequestBuilder;

        public int ScreenWidthPixels = 1024;
        public int ScreenHeightPixels = 768;

        public NativePostAdapter(AppCompatActivity context, string apiIdParameter, RecyclerView recyclerView, NativeFeedType nativePostType, bool highPreload = false)
        {
            try
            {

                ScreenWidthPixels = context.Resources.DisplayMetrics.WidthPixels / 2;
                ScreenHeightPixels = (context.Resources.DisplayMetrics.HeightPixels / 3);

                HasStableIds = true;
                ActivityContext = context;
                NativePostType = nativePostType;
                MainRecyclerView = recyclerView;
                IdParameter = apiIdParameter;
                PostClickListener = new PostClickListener(context, nativePostType);
                SetStateRestorationPolicy(StateRestorationPolicy.PreventWhenEmpty);

                RecycledViewPool = new RecyclerView.RecycledViewPool();

                ReadMoreOption = new StReadMoreOption.Builder()
                     .TextLength(200, StReadMoreOption.TypeCharacter)
                     .MoreLabel(context.GetText(Resource.String.Lbl_ReadMore))
                     .LessLabel(context.GetText(Resource.String.Lbl_ReadLess))
                     .MoreLabelColor(Color.ParseColor(AppSettings.MainColor))
                     .LessLabelColor(Color.ParseColor(AppSettings.MainColor))
                     .LabelUnderLine(true)
                     .Build();

                Glide.Get(context).SetMemoryCategory(MemoryCategory.Low);

                GlideNormalOptions = new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.None).Placeholder(new ColorDrawable(Color.ParseColor("#EFEFEF"))).Error(Resource.Drawable.ImagePlacholder).Format(DecodeFormat.PreferRgb565);
                FullGlideRequestBuilder = Glide.With(MainRecyclerView).AsBitmap().SetSizeMultiplier(0.96f).Apply(GlideNormalOptions).Timeout(3000);
                FullGlideRequestBuilder.DontTransform_T();
                FullGlideRequestBuilder.Downsample(DownsampleStrategy.CenterInside).CenterInside();
                FullGlideRequestBuilder.AddListener(new AdapterBind.GlideCustomRequestListener("Global")).CenterInside();

                GlideCircleOptions = new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.None).Placeholder(Resource.Drawable.no_profile_image_circle).Error(Resource.Drawable.no_profile_image_circle).Format(DecodeFormat.PreferRgb565).CircleCrop().Apply(RequestOptions.SignatureOf(new ObjectKey(DateTime.Now.Millisecond)));
                CircleGlideRequestBuilder = Glide.With(MainRecyclerView).AsBitmap().Downsample(DownsampleStrategy.AtMost).Apply(GlideCircleOptions).Timeout(3000).SetUseAnimationPool(false);
                CircleGlideRequestBuilder.DontTransform_T();
                CircleGlideRequestBuilder.Downsample(DownsampleStrategy.CenterInside);
                CircleGlideRequestBuilder.AddListener(new AdapterBind.GlideCustomRequestListener("Circle Global")).Override(50);

                GlideThumbnailRequestBuilder = Glide.With(MainRecyclerView).AsDrawable().Downsample(DownsampleStrategy.AtMost).Override(40).Apply(GlideNormalOptions).SetPriority(Priority.High).AddListener(new GlidePreLoaderRequestListener("Global Thumbnail"));
                GlideThumbnailRequestBuilder.DontTransform_T();
                GlideThumbnailRequestBuilder.Downsample(DownsampleStrategy.CenterInside).CenterInside();

                AdapterBind = new AdapterBind(this);

                ListUtils.NewPostList = new List<PostDataObject>();
                ListDiffer = new List<AdapterModelsClass>();
                PreCachingLayout = new PreCachingLayoutManager(ActivityContext)
                {
                    Orientation = LinearLayoutManager.Vertical
                };

                PreCachingLayout.SetPreloadItemCount(70);
                PreCachingLayout.ItemPrefetchEnabled = true;
                PreCachingLayout.AutoMeasureEnabled = false;
                PreCachingLayout.SetExtraLayoutSpace(3000);
                MainRecyclerView.SetLayoutManager(PreCachingLayout);
                MainRecyclerView.GetLayoutManager().ItemPrefetchEnabled = true;

                var sizeProvider = new FixedPreloadSizeProvider(ScreenWidthPixels, ScreenHeightPixels);
                //customPreloadSizeProvider = new CustomPreloadSizeProvider();
                //preloadSizeProvider = new ViewPreloadSizeProvider();
                var preLoader = new RecyclerViewPreloader<AdapterModelsClass>(Glide.With(ActivityContext?.BaseContext), this, sizeProvider, 5);
                MainRecyclerView.AddOnScrollListener(preLoader);

                AsyncViewHoldersPreLoaders = new AsyncViewHoldersContainers(MainRecyclerView, this);
                AsyncViewHoldersPreLoaders.SetCachedLimitHolders(16, highPreload);
                AsyncViewHoldersPreLoaders.StartGlobalCachHolders();

                MainRecyclerView.SetAdapter(this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        int d;
        public override void OnViewAttachedToWindow(Object holder)
        {
            if (holder != null)
            {
                // Console.WriteLine("WoLog: OnViewRecycled  ++ GetType " + holder.GetType());

                //switch (holder)
                //{
                //    case AdapterHolders.PostImageSectionViewHolder viewHolder:

                //        AdapterModelsClass item = ListDiffer[viewHolder.LayoutPosition];
                //        //FullGlideRequestBuilder.Load(item.).Into(viewHolder.Image);
                //        AdapterBind.ImagePostBind(viewHolder, item, "HightImage");
                //        Console.WriteLine("WoLog: OnViewAttachedToWindow  ++ PostImageSectionViewHolder " + holder.GetType());

                //        break;
                //}
            }

            base.OnViewAttachedToWindow(holder);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            //Console.WriteLine("WoLog: OnCreateViewHolder / Parent Type = " + parent.GetType() + "// parent.Id = " + parent.Id + "// parent.Tag = " + parent.Tag); 
            Trace.BeginSection("OnCreateViewHolder viewHolder =" + viewType);

            try
            {
                View itemView;
                switch (viewType)
                {
                    case (int)PostModelType.PromotePost:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Post_PromoteSection_Layout, parent, false);
                            var vh = new AdapterHolders.PromoteHolder(itemView);

                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = PromotePost " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.AddPostBox:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.ViewModel_AddPost, parent, false);
                            var vh = new AdapterHolders.AddPostViewHolder(itemView, this);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType =  AddPostBox " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.HeaderPost:
                        {
                            if (ListOfHeaderholders.Count > 0)
                            {
                                var vH = ListOfHeaderholders.First();
                                ListOfHeaderholders.Remove(vH);
                                Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >> = HeaderPost = FROM CASH LOADED " + viewType);
                                Trace.EndSection();
                                return vH;
                            }

                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Post_TopSection_Layout, parent, false);
                            var vh = new AdapterHolders.PostTopSectionViewHolder(itemView, this, PostClickListener);

                            Trace.EndSection();
                            return vh;
                        }
                    case (int)PostModelType.TextSectionPostPart:
                        {
                            Console.WriteLine("WoLog: OnCreateViewHolder  >> = TextSectionPostPart = " + (d++));

                            if (ListOfTextSectionPostPartholders.Count > 0)
                            {
                                var vH = ListOfTextSectionPostPartholders.First();
                                ListOfTextSectionPostPartholders.Remove(vH);
                                Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >> = TextSectionPostPart = FROM CASH LOADED " + ListOfTextSectionPostPartholders.Count);
                                Trace.EndSection();
                                return vH;
                            }

                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Post_TextSection_Layout, parent, false);
                            var vh = new AdapterHolders.PostTextSectionViewHolder(itemView, this, PostClickListener);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >> = TextSectionPostPart = WoDefault " + viewType);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = TextSectionPostPart " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.BottomPostPart:
                        {
                            var a = AsyncViewHoldersPreLoaders.ListOfBottomPostPartholders;
                            if (a.Count > 0)
                            {
                                var vH = a.First();
                                a.Remove(vH);
                                Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >> = BottomPostPart = FROM CASH LOADED " + AsyncViewHoldersPreLoaders.ListOfBottomPostPartholders);
                                Trace.EndSection();
                                return vH;
                            }

                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Post_ButtomSection_Layout, parent, false);
                            var vh = new AdapterHolders.PostBottomSectionViewHolder(itemView, this, PostClickListener);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >> = BottomPostPart = WoDefault " + viewType);
                            Trace.EndSection();
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType =  BottomPostPart " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.PrevBottomPostPart:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Post_PreButtomSection_Layout, parent, false);
                            var vh = new AdapterHolders.PostPrevBottomSectionViewHolder(itemView, this, PostClickListener);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = PrevBottomPostPart " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.AddCommentSection:
                        {
                            var a = AsyncViewHoldersPreLoaders.ListOfAddCommentholders;
                            if (a.Count > 0)
                            {
                                var vH = a.First();
                                a.Remove(vH);
                                Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >> = BottomPostPart = FROM CASH LOADED " + a.Count);
                                Trace.EndSection();
                                return vH;
                            }

                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Post_Content_AddComment_Section, parent, false);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType =  AddCommentSection " + viewType);
                            var vh = new AdapterHolders.PostAddCommentSectionViewHolder(itemView, this, PostClickListener);
                            return vh;
                        }
                    case (int)PostModelType.CommentSection:
                        {
                            var a = AsyncViewHoldersPreLoaders.ListOfCommentSectionholders;
                            if (a.Count > 0)
                            {
                                var vH = a.First(); a.Remove(vH);
                                return vH;
                            }

                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Post_Content_Comment_Section, parent, false);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType =  AddCommentSection " + viewType);
                            var vh = new CommentAdapterViewHolder(itemView, new CommentAdapter(ActivityContext), new CommentClickListener(ActivityContext, "Comment"), "Post");
                            return vh;
                        }
                    case (int)PostModelType.Divider:
                        {
                            var a = AsyncViewHoldersPreLoaders.ListDeviderholders;
                            if (a.Count > 0)
                            {
                                var vH = a.First(); a.Remove(vH);
                                Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >> = Divider = FROM CASH LOADED " + a.Count);
                                return vH;
                            }

                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Post_Devider, parent, false);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >> = Divider = Normal " + viewType);
                            var vh = new AdapterHolders.PostDividerSectionViewHolder(itemView);
                            return vh;
                        }
                    case (int)PostModelType.VideoPost:
                        {
                            var a = AsyncViewHoldersPreLoaders.ListOfVideoholders;
                            if (a.Count > 0)
                            {
                                var vH = a.First();
                                a.Remove(vH);
                                Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >> = VideoPost = FROM CASH LOADED " + AsyncViewHoldersPreLoaders.ListOfVideoholders.Count);
                                Trace.EndSection();

                                return vH;
                            }

                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Post_Content_video_layout, parent, false);
                            var vh = new AdapterHolders.PostVideoSectionViewHolder(itemView, this);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = VideoPost with dp=");
                            return vh;
                        }
                    case (int)PostModelType.MapPost:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Post_Content_Map_Layout, parent, false);
                            var vh = new AdapterHolders.PostMapSectionViewHolder(itemView, this, PostClickListener);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = ImagePost" + viewType);
                            return vh;
                        }
                    case (int)PostModelType.ImagePost:
                    case (int)PostModelType.StickerPost:
                        {
                            var a = AsyncViewHoldersPreLoaders.ListOfImageholders;
                            if (a.Count > 0)
                            {
                                var vH = a.First();
                                a.Remove(vH);
                                Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >> = ImagePost = FROM CASH LOADED " + a.Count);
                                Trace.EndSection();
                                return vH;
                            }

                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Post_Content_Image_Layout, parent, false);
                            var vh = new AdapterHolders.PostImageSectionViewHolder(itemView, this, PostClickListener, viewType);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = ImagePost with dp=");
                            return vh;
                        }
                    case (int)PostModelType.MultiImage2:
                        {
                            var a = AsyncViewHoldersPreLoaders.List2Imageholders;
                            if (a.Count > 0)
                            {
                                var vH = a.First();
                                a.Remove(vH);
                                Trace.EndSection();
                                Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >> = MultiImage2 = FROM CASH LOADED " + a.Count);
                                return vH;
                            }
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Post_Content_2Images_Layout, parent, false);
                            var vh = new AdapterHolders.PostMultiImageViewHolder(itemView, 2, this, PostClickListener);
                            Trace.EndSection();
                            return vh;
                        }
                    case (int)PostModelType.MultiImage3:
                        {
                            var a = AsyncViewHoldersPreLoaders.List3Imageholders;
                            if (a.Count > 0)
                            {
                                var vH = a.First();
                                a.Remove(vH);
                                Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >> = MultiImage3 = FROM CASH LOADED " + a.Count);
                                Trace.EndSection();
                                return vH;
                            }

                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Post_Content_3Images_Layout, parent, false);
                            var vh = new AdapterHolders.PostMultiImageViewHolder(itemView, 3, this, PostClickListener);
                            Trace.EndSection();
                            return vh;
                        }
                    case (int)PostModelType.MultiImage4:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Post_Content_4Images_Layout, parent, false);
                            var vh = new AdapterHolders.PostMultiImageViewHolder(itemView, 4, this, PostClickListener);
                            Trace.EndSection();
                            return vh;
                        }
                    case (int)PostModelType.MultiImage5:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Post_Content_5Images_Layout, parent, false);
                            var vh = new AdapterHolders.PostMultiImageViewHolder(itemView, 5, this, PostClickListener);
                            Trace.EndSection();
                            return vh;
                        }
                    case (int)PostModelType.MultiImage6:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Post_Content_6Images_Layout, parent, false);
                            var vh = new AdapterHolders.PostMultiImageViewHolder(itemView, 6, this, PostClickListener);
                            Trace.EndSection();
                            return vh;
                        }
                    case (int)PostModelType.MultiImage7:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Post_Content_7Images_Layout, parent, false);
                            var vh = new AdapterHolders.PostMultiImageViewHolder(itemView, 7, this, PostClickListener);
                            Trace.EndSection();
                            return vh;
                        }
                    case (int)PostModelType.MultiImage8:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Post_Content_8Images_Layout, parent, false);
                            var vh = new AdapterHolders.PostMultiImageViewHolder(itemView, 8, this, PostClickListener);
                            Trace.EndSection();
                            return vh;
                        }
                    case (int)PostModelType.MultiImage9:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Post_Content_9Images_Layout, parent, false);
                            var vh = new AdapterHolders.PostMultiImageViewHolder(itemView, 9, this, PostClickListener);
                            Trace.EndSection();
                            return vh;
                        }
                    case (int)PostModelType.MultiImage10:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Post_Content_10Images_Layout, parent, false);
                            var vh = new AdapterHolders.PostMultiImageViewHolder(itemView, 10, this, PostClickListener);
                            Trace.EndSection();
                            return vh;
                        }
                    case (int)PostModelType.MultiImages:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Post_Content_MultiImages_Layout, parent, false);
                            var vh = new AdapterHolders.PostMultiImagesViewHolder(itemView, this, PostClickListener);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = MultiImages" + viewType);
                            return vh;
                        }
                    case (int)PostModelType.ColorPost:
                        {
                            var a = AsyncViewHoldersPreLoaders.ListColorholders;
                            if (a.Count > 0)
                            {
                                var vH = a.First();
                                a.Remove(vH);
                                Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >> = ColorPost = FROM CASH LOADED " + a.Count);
                                return vH;
                            }

                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Post_Content_ColorBox_Layout, parent, false);
                            var vh = new AdapterHolders.PostColorBoxSectionViewHolder(itemView);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = ColorPost " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.LinkPost:
                        {
                            var a = AsyncViewHoldersPreLoaders.ListLinkholders;
                            if (a.Count > 0)
                            {
                                var vH = a.First();
                                a.Remove(vH);
                                Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >> = LinkPost = FROM CASH LOADED " + a.Count);
                                Trace.EndSection();
                                return vH;
                            }

                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Post_Content_Link_Layout, parent, false);
                            var vh = new AdapterHolders.LinkPostViewHolder(itemView, this, PostClickListener);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = LinkPost " + viewType);
                            Trace.EndSection();
                            return vh;
                        }
                    case (int)PostModelType.BlogPost:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Post_Content_Blog_Layout, parent, false);
                            var vh = new AdapterHolders.PostBlogSectionViewHolder(itemView, this, PostClickListener);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = BlogPost " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.EventPost:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Post_Content_Event_Section, parent, false);
                            var vh = new AdapterHolders.EventPostViewHolder(itemView, this, PostClickListener);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = EventPost " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.FilePost:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Post_Content_File_Layout, parent, false);
                            var vh = new AdapterHolders.FilePostViewHolder(itemView, this, PostClickListener);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = FilePost " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.PurpleFundPost:
                    case (int)PostModelType.FundingPost:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Post_Content_Funding_Layout, parent, false);
                            var vh = new AdapterHolders.FundingPostViewHolder(itemView, this, PostClickListener);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = FundingPost " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.ProductPost:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Post_Content_Product_Layout, parent, false);
                            var vh = new AdapterHolders.ProductPostViewHolder(itemView, this, PostClickListener);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = ProductPost " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.VoicePost:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Post_Content_Voice_Layout, parent, false);
                            var vh = new AdapterHolders.SoundPostViewHolder(itemView, this, PostClickListener);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = VoicePost " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.YoutubePost:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Post_Content_Youtube_Section, parent, false);
                            var vh = new AdapterHolders.YoutubePostViewHolder(itemView, this, PostClickListener);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = YoutubePost " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.OfferPost:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Post_Content_Offer_Section, parent, false);
                            var vh = new AdapterHolders.OfferPostViewHolder(itemView, this, PostClickListener);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = OfferPost " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.JobPostSection:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Post_Content_Job_Layout1, parent, false);
                            var vh = new AdapterHolders.JobPostViewHolder(itemView, this, PostClickListener);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = JobPostSection1 " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.PollPost:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Post_Content_Poll_Section, parent, false);
                            var vh = new AdapterHolders.PollsPostViewHolder(itemView, this);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = PollPost " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.SharedHeaderPost:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Post_TopSectionShare_Layout, parent, false);
                            var vh = new AdapterHolders.PostTopSharedSectionViewHolder(itemView, this, PostClickListener);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = SharedHeaderPost " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.AgoraLivePost:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Post_AgoraLive_Layout, parent, false);
                            var vh = new AdapterHolders.PostAgoraLiveViewHolder(itemView, this, PostClickListener);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = SharedHeaderPost " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.LivePost:
                    case (int)PostModelType.DeepSoundPost:
                    case (int)PostModelType.VimeoPost:
                    case (int)PostModelType.FacebookPost:
                    case (int)PostModelType.PlayTubePost:
                    case (int)PostModelType.TikTokPost:
                    case (int)PostModelType.TwitterPost:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Post_Content_WebView_Layout, parent, false);
                            var vh = new AdapterHolders.PostPlayTubeContentViewHolder(itemView);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = WebView " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.AlertBox:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.ViewModel_Alert, parent, false);
                            var vh = new AdapterHolders.AlertAdapterViewHolder(itemView, this, PostModelType.AlertBox);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType =  AlertBox " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.AlertBoxAnnouncement:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.ViewModel_Announcement, parent, false);
                            var vh = new AdapterHolders.AlertAdapterViewHolder(itemView, this, PostModelType.AlertBoxAnnouncement);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType =  AlertBoxAnnouncement " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.AlertJoinBox:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.ViewModel_AlertJoin, parent, false);
                            var vh = new AdapterHolders.AlertJoinAdapterViewHolder(itemView, this);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = AlertJoinBox " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.Section:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.ViewModel_Section, parent, false);
                            var vh = new AdapterHolders.SectionViewHolder(itemView);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = Section " + viewType);
                            return vh;
                        }
                    //case (int)PostModelType.SearchForPosts:
                    //    {
                    //        itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.ViewModel_SearchForPost, parent, false);
                    //        var vh = new AdapterHolders.SearchForPostsViewHolder(itemView, this);
                    //        //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = SearchForPosts" + viewType);
                    //        return vh;
                    //    }
                    case (int)PostModelType.SocialLinks:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.ViewModel_SociaLink, parent, false);
                            var vh = new AdapterHolders.SocialLinksViewHolder(itemView, this);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = SocialLinks " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.AboutBox:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.ViewModel_About, parent, false);
                            var vh = new AdapterHolders.AboutBoxViewHolder(itemView);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = AboutBox " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.InfoUserBox:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.ViewModel_InfoUser, parent, false);
                            var vh = new AdapterHolders.InfoUserBoxViewHolder(itemView, ActivityContext);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = InfoUserBox " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.InfoGroupBox:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.ViewModel_GroupPrivacy, parent, false);
                            var vh = new AdapterHolders.InfoGroupBoxViewHolder(itemView, this);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = InfoGroupBox " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.InfoPageBox:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.ViewModel_InfoPage, parent, false);
                            var vh = new AdapterHolders.InfoPageBoxViewHolder(itemView, this);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = InfoPageBox " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.Story:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.ViewModel_HRecyclerView, parent, false);
                            var vh = new AdapterHolders.StoryViewHolder(itemView, this, PostClickListener);
                            RecycledViewPool = new RecyclerView.RecycledViewPool();
                            vh.StoryRecyclerView.SetRecycledViewPool(RecycledViewPool);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType =  Story " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.FollowersBox:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.TemplateRecyclerViewLayout, parent, false);
                            var vh = new AdapterHolders.FollowersViewHolder(itemView, this, PostClickListener);
                            RecycledViewPool = new RecyclerView.RecycledViewPool();
                            vh.FollowersRecyclerView.SetRecycledViewPool(RecycledViewPool);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.GroupsBox:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.ViewModel_HRecyclerView, parent, false);
                            var vh = new AdapterHolders.GroupsViewHolder(itemView, this, PostClickListener);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = GroupsBox " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.SuggestedPagesBox:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.ViewModel_HRecyclerView, parent, false);
                            var vh = new AdapterHolders.SuggestedPagesViewHolder(itemView, this);
                            RecycledViewPool = new RecyclerView.RecycledViewPool();
                            vh.PagesRecyclerView.SetRecycledViewPool(RecycledViewPool);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.SuggestedGroupsBox:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.ViewModel_HRecyclerView, parent, false);
                            var vh = new AdapterHolders.SuggestedGroupsViewHolder(itemView, this);
                            RecycledViewPool = new RecyclerView.RecycledViewPool();
                            vh.GroupsRecyclerView.SetRecycledViewPool(RecycledViewPool);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.SuggestedUsersBox:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.ViewModel_HRecyclerView, parent, false);
                            var vh = new AdapterHolders.SuggestedUsersViewHolder(itemView, this);
                            RecycledViewPool = new RecyclerView.RecycledViewPool();
                            vh.UsersRecyclerView.SetRecycledViewPool(RecycledViewPool);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = SuggestedUsersBox " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.ImagesBox:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.ViewModel_HRecyclerView, parent, false);
                            var vh = new AdapterHolders.ImagesViewHolder(itemView, this, PostClickListener);
                            RecycledViewPool = new RecyclerView.RecycledViewPool();
                            vh.ImagesRecyclerView.SetRecycledViewPool(RecycledViewPool);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = ImagesBox " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.PagesBox:
                        {
                            //itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.ViewModel_Pages, parent, false);
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.ViewModel_HRecyclerView, parent, false);
                            var vh = new AdapterHolders.PagesViewHolder(itemView, this, PostClickListener);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType =  PagesBox" + viewType);
                            return vh;
                        }
                    case (int)PostModelType.AdsPost:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.PostType_Ads, parent, false);
                            var vh = new AdapterHolders.AdsPostViewHolder(itemView, this, PostClickListener);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = AdsPost " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.EmptyState:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_EmptyState, parent, false);
                            var vh = new AdapterHolders.EmptyStateAdapterViewHolder(itemView);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType =  EmptyState " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.AdMob1:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.PostType_AdMob, parent, false);
                            var vh = new AdapterHolders.AdsAdapterViewHolder(itemView, PostModelType.AdMob1, ActivityContext);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType =  AdMob " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.AdMob2:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.PostType_AdMob2, parent, false);
                            var vh = new AdapterHolders.AdsAdapterViewHolder(itemView, PostModelType.AdMob2, ActivityContext);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType =  AdMob " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.AdMob3:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.PostType_AdMob3, parent, false);
                            var vh = new AdapterHolders.AdsAdapterViewHolder(itemView, PostModelType.AdMob3, ActivityContext);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType =  AdMob " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.FbAdNative:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.PostType_FbNativeAd, parent, false);
                            var vh = new AdapterHolders.AdsAdapterViewHolder(itemView, PostModelType.FbAdNative, ActivityContext);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = FbAdNative " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.ViewProgress:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.ItemProgressView, parent, false);
                            var vh = new AdapterHolders.ProgressViewHolder(itemView);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = ViewProgress " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.NormalPost:
                        {
                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Post_Content_Null_Layout, parent, false);
                            var vh = new AdapterHolders.PostDefaultSectionViewHolder(itemView);
                            //Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = NormalPost " + viewType);
                            return vh;
                        }
                    default:
                        {
                            if (ListOfholders.Count > 0)
                            {
                                var vH = ListOfholders.First();
                                ListOfholders.Remove(vH);
                                Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostDefaultSectionViewHolder = FROM CASH LOADED " + viewType);
                                return vH;
                            }

                            itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Post_Content_Null_Layout, parent, false);
                            var vh = new AdapterHolders.PostDefaultSectionViewHolder(itemView);

                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = default " + viewType);
                            Trace.EndSection();
                            return vh;
                        }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position, IList<Object> payloads)
        {
            try
            {
                switch (payloads.Count)
                {
                    case > 0:
                        {
                            var item = ListDiffer[position];
                            switch (payloads[0].ToString())
                            {
                                case "StoryRefresh":
                                    {
                                        switch (viewHolder)
                                        {
                                            case AdapterHolders.StoryViewHolder holder:
                                                holder.RefreshData();
                                                break;
                                        }

                                        break;
                                    }
                                case "reaction":
                                    switch (viewHolder)
                                    {
                                        case AdapterHolders.PostPrevBottomSectionViewHolder holder:
                                            AdapterBind.PrevBottomPostPartBind(holder, item);
                                            break;
                                        case AdapterHolders.PostBottomSectionViewHolder holder2:
                                            AdapterBind.BottomPostPartBind(holder2, item);
                                            break;
                                    }

                                    break;
                                case "commentReplies":
                                    switch (viewHolder)
                                    {
                                        case AdapterHolders.PostPrevBottomSectionViewHolder holder:
                                            AdapterBind.PrevBottomPostPartBind(holder, item);
                                            break;
                                        case AdapterHolders.PostBottomSectionViewHolder holder2:
                                            AdapterBind.BottomPostPartBind(holder2, item);
                                            break;
                                        case CommentAdapterViewHolder holder:
                                            AdapterBind.CommentSectionBind(holder, item);
                                            break;
                                    }

                                    break;
                                case "BoostedPost":
                                    switch (viewHolder)
                                    {
                                        case AdapterHolders.PromoteHolder holder:
                                            AdapterBind.PromotePostBind(holder, item);
                                            break;
                                    }

                                    break;
                                case "WithoutBlobAudio":
                                    switch (viewHolder)
                                    {
                                        case AdapterHolders.SoundPostViewHolder holder:

                                            holder.PlayButton.Visibility = ViewStates.Visible;
                                            holder.PlayButton.SetImageResource(Resource.Drawable.icon_play_vector);
                                            holder.PlayButton.Tag = "Play";

                                            switch (Build.VERSION.SdkInt)
                                            {
                                                case >= BuildVersionCodes.N:
                                                    holder.SeekBar.SetProgress(0, true);
                                                    break;
                                                // For API < 24 
                                                default:
                                                    holder.SeekBar.Progress = 0;
                                                    break;
                                            }

                                            break;
                                    }

                                    break;
                                default:
                                    base.OnBindViewHolder(viewHolder, position, payloads);
                                    break;
                            }

                            break;
                        }
                    default:
                        base.OnBindViewHolder(viewHolder, position, payloads);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                base.OnBindViewHolder(viewHolder, position, payloads);
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                AdapterModelsClass item = ListDiffer[position];
                var itemViewType = viewHolder.ItemViewType;
                UserDataObject publisher = item.PostData?.Publisher ?? item.PostData?.UserData;

                Console.WriteLine("WoLog: OnBindViewHolder  >> = " + publisher?.Name + " ==== " + publisher?.Username + " " + item);

                switch (itemViewType)
                {
                    case (int)PostModelType.HeaderPost:
                        {
                            if (viewHolder is not AdapterHolders.PostTopSectionViewHolder holder)
                                return;

                            Console.WriteLine("WoLog: OnBindViewHolder  >> = HeaderPost = " + (d));

                            AdapterBind.HeaderPostBind(holder, item);

                            //Console.WriteLine("WoLog: NativePostAdapter / OnBindViewHolder  >>  PostModelType = HeaderPost " + position);
                            break;
                        }
                    case (int)PostModelType.PromotePost:
                        {
                            if (viewHolder is not AdapterHolders.PromoteHolder holder)
                                return;

                            AdapterBind.PromotePostBind(holder, item);

                            break;
                        }

                    case (int)PostModelType.SharedHeaderPost:
                        {
                            if (viewHolder is not AdapterHolders.PostTopSharedSectionViewHolder holder)
                                return;

                            AdapterBind.SharedHeaderPostBind(holder, item);

                            break;
                        }
                    case (int)PostModelType.PrevBottomPostPart:
                        {
                            if (viewHolder is not AdapterHolders.PostPrevBottomSectionViewHolder holder)
                                return;

                            AdapterBind.PrevBottomPostPartBind(holder, item);

                            //Console.WriteLine("WoLog: NativePostAdapter / OnBindViewHolder  >>  PostModelType = PrevBottomPostPart " + position);
                            break;
                        }
                    case (int)PostModelType.BottomPostPart:
                        {
                            if (viewHolder is not AdapterHolders.PostBottomSectionViewHolder holder)
                                return;

                            AdapterBind.BottomPostPartBind(holder, item);
                            Trace.EndSection();
                            //Console.WriteLine("WoLog: NativePostAdapter / OnBindViewHolder  >>  PostModelType =  BottomPostPart " + position);
                            break;
                        }
                    case (int)PostModelType.TextSectionPostPart:
                        {
                            if (viewHolder is not AdapterHolders.PostTextSectionViewHolder holder)
                                return;

                            AdapterBind.TextSectionPostPartBind(holder, item);
                            Trace.EndSection();
                            //Console.WriteLine("WoLog: NativePostAdapter / OnBindViewHolder  >>  PostModelType =  TextSectionPostPart " + position);
                            break;
                        }
                    case (int)PostModelType.CommentSection:
                        {
                            if (viewHolder is not CommentAdapterViewHolder holder)
                                return;

                            AdapterBind.CommentSectionBind(holder, item);

                            break;
                        }
                    case (int)PostModelType.AddCommentSection:
                        {
                            if (viewHolder is not AdapterHolders.PostAddCommentSectionViewHolder holder)
                                return;

                            var CircleGlideRequestBuilder = Glide.With(holder.ItemView).Load(UserDetails.Avatar).Apply(GlideCircleOptions).Timeout(3000).SetUseAnimationPool(false);
                            CircleGlideRequestBuilder.DontTransform_T();
                            CircleGlideRequestBuilder.Downsample(DownsampleStrategy.CenterInside);
                            CircleGlideRequestBuilder.AddListener(new AdapterBind.GlideCustomRequestListener("Circle Comment Global")).Override(70).Into(holder.ProfileImageView);

                            //GlideImageLoader.LoadImage(ActivityContext, UserDetails.Avatar, holder.ProfileImageView, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);

                            break;
                        }
                    case (int)PostModelType.StickerPost:
                    case (int)PostModelType.ImagePost:
                        {
                            if (viewHolder is not AdapterHolders.PostImageSectionViewHolder holder)
                                return;

                            AdapterBind.ImagePostBind(holder, item);

                            //Console.WriteLine("WoLog: NativePostAdapter / OnBindViewHolder  >>  PostModelType =  " + position);
                            break;
                        }
                    case (int)PostModelType.MapPost:
                        {
                            if (viewHolder is not AdapterHolders.PostMapSectionViewHolder holder)
                                return;

                            AdapterBind.MapPostBind(holder, item);

                            break;
                        }
                    case (int)PostModelType.MultiImage2:
                        {
                            if (viewHolder is not AdapterHolders.PostMultiImageViewHolder holder)
                                return;

                            AdapterBind.MultiImageBind(holder, 2, item);
                            break;
                        }
                    case (int)PostModelType.MultiImage3:
                        {
                            if (viewHolder is not AdapterHolders.PostMultiImageViewHolder holder)
                                return;

                            AdapterBind.MultiImageBind(holder, 3, item);
                            break;
                        }
                    case (int)PostModelType.MultiImage4:
                        {
                            if (viewHolder is not AdapterHolders.PostMultiImageViewHolder holder)
                                return;

                            AdapterBind.MultiImageBind(holder, 4, item);
                            break;
                        }
                    case (int)PostModelType.MultiImage5:
                        {
                            if (viewHolder is not AdapterHolders.PostMultiImageViewHolder holder)
                                return;

                            AdapterBind.MultiImageBind(holder, 5, item);
                            break;
                        }
                    case (int)PostModelType.MultiImage6:
                        {
                            if (viewHolder is not AdapterHolders.PostMultiImageViewHolder holder)
                                return;

                            AdapterBind.MultiImageBind(holder, 6, item);
                            break;
                        }
                    case (int)PostModelType.MultiImage7:
                        {
                            if (viewHolder is not AdapterHolders.PostMultiImageViewHolder holder)
                                return;

                            AdapterBind.MultiImageBind(holder, 7, item);
                            break;
                        }
                    case (int)PostModelType.MultiImage8:
                        {
                            if (viewHolder is not AdapterHolders.PostMultiImageViewHolder holder)
                                return;

                            AdapterBind.MultiImageBind(holder, 8, item);
                            break;
                        }
                    case (int)PostModelType.MultiImage9:
                        {
                            if (viewHolder is not AdapterHolders.PostMultiImageViewHolder holder)
                                return;

                            AdapterBind.MultiImageBind(holder, 9, item);
                            break;
                        }
                    case (int)PostModelType.MultiImage10:
                        {
                            if (viewHolder is not AdapterHolders.PostMultiImageViewHolder holder)
                                return;

                            AdapterBind.MultiImageBind(holder, 10, item);
                            break;
                        }
                    case (int)PostModelType.MultiImages:
                        {
                            if (viewHolder is not AdapterHolders.PostMultiImagesViewHolder holder)
                                return;

                            AdapterBind.MultiImagesBind(holder, item);
                            break;
                        }
                    case (int)PostModelType.VideoPost:
                        {
                            if (viewHolder is not AdapterHolders.PostVideoSectionViewHolder holder)
                                return;

                            AdapterBind.VideoPostBind(holder, item);
                            break;
                        }
                    case (int)PostModelType.BlogPost:
                        {
                            if (viewHolder is not AdapterHolders.PostBlogSectionViewHolder holder)
                                return;

                            AdapterBind.BlogPostBind(holder, item);

                            break;
                        }
                    case (int)PostModelType.ColorPost:
                        {
                            if (viewHolder is not AdapterHolders.PostColorBoxSectionViewHolder holder)
                                return;

                            AdapterBind.ColorPostBind(holder, item);

                            break;
                        }
                    case (int)PostModelType.EventPost:
                        {
                            if (viewHolder is not AdapterHolders.EventPostViewHolder holder)
                                return;

                            AdapterBind.EventPostBind(holder, item);
                            break;
                        }
                    case (int)PostModelType.LinkPost:
                        {
                            if (viewHolder is not AdapterHolders.LinkPostViewHolder holder)
                                return;

                            AdapterBind.LinkPostBind(holder, item);
                            break;
                        }
                    case (int)PostModelType.FilePost:
                        {
                            if (viewHolder is not AdapterHolders.FilePostViewHolder holder)
                                return;

                            holder.PostFileText.Text = item.PostData.PostFileName;
                            break;
                        }
                    case (int)PostModelType.FundingPost:
                        {
                            if (viewHolder is not AdapterHolders.FundingPostViewHolder holder)
                                return;

                            AdapterBind.FundingPostBind(holder, item);
                            break;
                        }
                    case (int)PostModelType.PurpleFundPost:
                        {
                            if (viewHolder is not AdapterHolders.FundingPostViewHolder holder)
                                return;

                            AdapterBind.PurpleFundPostBind(holder, item);
                            break;
                        }
                    case (int)PostModelType.ProductPost:
                        {
                            if (viewHolder is not AdapterHolders.ProductPostViewHolder holder)
                                return;

                            AdapterBind.ProductPostBind(holder, item);
                            break;
                        }
                    case (int)PostModelType.VoicePost:
                        {
                            if (viewHolder is not AdapterHolders.SoundPostViewHolder holder)
                                return;

                            AdapterBind.VoicePostBind(holder, item);

                            Console.WriteLine(holder);
                            break;
                        }
                    case (int)PostModelType.AgoraLivePost:
                        {
                            if (viewHolder is not AdapterHolders.PostAgoraLiveViewHolder holder)
                                return;

                            AdapterBind.AgoraLivePostBind(holder, item);

                            break;
                        }
                    case (int)PostModelType.YoutubePost:
                        {
                            if (viewHolder is not AdapterHolders.YoutubePostViewHolder holder)
                                return;

                            FullGlideRequestBuilder.Load("https://img.youtube.com/vi/" + item.PostData.PostYoutube + "/0.jpg").Into(holder.Image);
                            break;
                        }
                    case (int)PostModelType.PlayTubePost:
                        {
                            if (viewHolder is not AdapterHolders.PostPlayTubeContentViewHolder holder)
                                return;

                            AdapterBind.WebViewPostBind(holder, item, PostModelType.PlayTubePost);
                            break;
                        }
                    case (int)PostModelType.TikTokPost:
                        {
                            if (viewHolder is not AdapterHolders.PostPlayTubeContentViewHolder holder)
                                return;

                            AdapterBind.WebViewPostBind(holder, item, PostModelType.TikTokPost);
                            break;
                        }
                    case (int)PostModelType.TwitterPost:
                        {
                            if (viewHolder is not AdapterHolders.PostPlayTubeContentViewHolder holder)
                                return;

                            AdapterBind.WebViewPostBind(holder, item, PostModelType.TwitterPost);
                            break;
                        }
                    case (int)PostModelType.LivePost:
                        {
                            if (viewHolder is not AdapterHolders.PostPlayTubeContentViewHolder holder)
                                return;

                            AdapterBind.WebViewPostBind(holder, item, PostModelType.LivePost);

                            break;
                        }
                    case (int)PostModelType.DeepSoundPost:
                        {
                            if (viewHolder is not AdapterHolders.PostPlayTubeContentViewHolder holder)
                                return;

                            AdapterBind.WebViewPostBind(holder, item, PostModelType.DeepSoundPost);

                            break;
                        }
                    case (int)PostModelType.VimeoPost:
                        {
                            if (viewHolder is not AdapterHolders.PostPlayTubeContentViewHolder holder)
                                return;

                            AdapterBind.WebViewPostBind(holder, item, PostModelType.VimeoPost);

                            break;
                        }
                    case (int)PostModelType.FacebookPost:
                        {
                            if (viewHolder is not AdapterHolders.PostPlayTubeContentViewHolder holder)
                                return;

                            AdapterBind.WebViewPostBind(holder, item, PostModelType.FacebookPost);
                            break;
                        }
                    case (int)PostModelType.OfferPost:
                        {
                            if (viewHolder is not AdapterHolders.OfferPostViewHolder holder)
                                return;

                            AdapterBind.OfferPostBind(holder, item);

                            break;
                        }
                    case (int)PostModelType.JobPostSection:
                        {
                            if (viewHolder is not AdapterHolders.JobPostViewHolder holder)
                                return;

                            AdapterBind.JobPostSectionBind(holder, item);
                            break;
                        }
                    case (int)PostModelType.PollPost:
                        {
                            if (viewHolder is not AdapterHolders.PollsPostViewHolder holder)
                                return;

                            AdapterBind.PollPostBind(holder, item);

                            break;
                        }
                    case (int)PostModelType.AlertBox:
                        {
                            if (viewHolder is not AdapterHolders.AlertAdapterViewHolder holder)
                                return;

                            AdapterBind.AlertBoxBind(holder, item, PostModelType.AlertBox);
                            break;
                        }
                    case (int)PostModelType.AlertBoxAnnouncement:
                        {
                            if (viewHolder is not AdapterHolders.AlertAdapterViewHolder holder)
                                return;

                            AdapterBind.AlertBoxBind(holder, item, PostModelType.AlertBoxAnnouncement);

                            break;
                        }
                    case (int)PostModelType.AlertJoinBox:
                        {
                            if (viewHolder is not AdapterHolders.AlertJoinAdapterViewHolder holder)
                                return;

                            AdapterBind.AlertJoinBoxBind(holder, item);
                            break;
                        }
                    case (int)PostModelType.Section:
                        {
                            if (viewHolder is not AdapterHolders.SectionViewHolder holder)
                                return;

                            holder.AboutHead.Text = item.AboutModel.TitleHead;

                            break;
                        }
                    case (int)PostModelType.AddPostBox:
                        {
                            if (viewHolder is not AdapterHolders.AddPostViewHolder holder)
                                return;

                            GlideImageLoader.LoadImage(ActivityContext, UserDetails.Avatar, holder.ProfileImageView, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                            break;
                        }
                    //case (int)PostModelType.SearchForPosts:
                    //    {
                    //        if (viewHolder is not AdapterHolders.SearchForPostsViewHolder holder)
                    //            return;
                    //        Console.WriteLine(holder);
                    //        break;
                    //    }
                    case (int)PostModelType.SocialLinks:
                        {
                            if (viewHolder is not AdapterHolders.SocialLinksViewHolder holder)
                                return;
                            AdapterBind.SocialLinksBind(holder, item);

                            break;
                        }
                    case (int)PostModelType.AboutBox:
                        {
                            if (viewHolder is not AdapterHolders.AboutBoxViewHolder holder)
                                return;

                            AdapterBind.AboutBoxBind(holder, item);

                            break;
                        }
                    case (int)PostModelType.InfoUserBox:
                        {
                            if (viewHolder is not AdapterHolders.InfoUserBoxViewHolder holder)
                                return;

                            AdapterBind.InfoUserBoxBind(holder, item);

                            break;
                        }
                    case (int)PostModelType.InfoGroupBox:
                        {
                            if (viewHolder is not AdapterHolders.InfoGroupBoxViewHolder holder)
                                return;

                            AdapterBind.InfoGroupBoxBind(holder, item);

                            break;
                        }
                    case (int)PostModelType.InfoPageBox:
                        {
                            if (viewHolder is not AdapterHolders.InfoPageBoxViewHolder holder)
                                return;

                            AdapterBind.InfoPageBoxBind(holder, item);

                            break;
                        }
                    case (int)PostModelType.Story:
                        {
                            if (viewHolder is not AdapterHolders.StoryViewHolder holder)
                                return;

                            HolderStory = holder;
                            AdapterBind.StoryBind(holder, item);
                            break;
                        }
                    case (int)PostModelType.FollowersBox:
                        {
                            if (viewHolder is not AdapterHolders.FollowersViewHolder holder)
                                return;

                            AdapterBind.FollowersBoxBind(holder, item);
                            break;
                        }
                    case (int)PostModelType.GroupsBox:
                        {
                            if (viewHolder is not AdapterHolders.GroupsViewHolder holder)
                                return;

                            AdapterBind.GroupsBoxBind(holder, item);
                            break;
                        }
                    case (int)PostModelType.SuggestedPagesBox:
                        {
                            if (viewHolder is not AdapterHolders.SuggestedPagesViewHolder holder)
                                return;

                            AdapterBind.SuggestedPagesBoxBind(holder, item);
                            break;
                        }
                    case (int)PostModelType.SuggestedGroupsBox:
                        {
                            if (viewHolder is not AdapterHolders.SuggestedGroupsViewHolder holder)
                                return;

                            AdapterBind.SuggestedGroupsBoxBind(holder, item);
                            break;
                        }
                    case (int)PostModelType.SuggestedUsersBox:
                        {
                            if (viewHolder is not AdapterHolders.SuggestedUsersViewHolder holder)
                                return;

                            AdapterBind.SuggestedUsersBoxBind(holder, item);

                            break;
                        }
                    case (int)PostModelType.ImagesBox:
                        {
                            if (viewHolder is not AdapterHolders.ImagesViewHolder holder)
                                return;

                            AdapterBind.ImagesBoxBind(holder, item);
                            break;
                        }
                    case (int)PostModelType.PagesBox:
                        {
                            if (viewHolder is not AdapterHolders.PagesViewHolder holder)
                                return;

                            AdapterBind.PagesBoxBind(holder, item);

                            break;
                        }
                    case (int)PostModelType.AdsPost:
                        {
                            if (viewHolder is not AdapterHolders.AdsPostViewHolder holder)
                                return;

                            AdapterBind.AdsPostBind(holder, item);
                            break;
                        }
                    case (int)PostModelType.EmptyState:
                        {
                            if (viewHolder is not AdapterHolders.EmptyStateAdapterViewHolder holder)
                                return;

                            BindEmptyState(holder);

                            break;
                        }
                    case (int)PostModelType.AdMob1:
                    case (int)PostModelType.AdMob2:
                    case (int)PostModelType.AdMob3:
                    case (int)PostModelType.FbAdNative:
                    case (int)PostModelType.Divider:
                        break;
                    case (int)PostModelType.ViewProgress:
                        {
                            if (viewHolder is not AdapterHolders.ProgressViewHolder holder)
                                return;
                            Console.WriteLine(holder);
                            break;
                        }
                    default:
                        {
                            if (viewHolder is not AdapterHolders.PostDefaultSectionViewHolder holder)
                                return;
                            Console.WriteLine(holder);
                            break;
                        }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #region Progress On Scroll

        //public void SetOnLoadMoreListener(IOnLoadMoreListener onLoadMoreListener)
        //{
        //    OnLoadMoreListener = onLoadMoreListener;
        //}

        //public override void OnAttachedToRecyclerView(RecyclerView recyclerView)
        //{
        //    LastItemViewDetector(recyclerView);
        //    base.OnAttachedToRecyclerView(recyclerView);
        //}

        //private void LastItemViewDetector(RecyclerView recyclerView)
        //{
        //    try
        //    {
        //        if (recyclerView.GetLayoutManager() is LinearLayoutManager layoutManager)
        //        {
        //            recyclerView.AddOnScrollListener(new MyScroll(this, layoutManager));
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Methods.DisplayReportResultTrack(e);
        //    }
        //}

        //private class MyScroll : RecyclerView.OnScrollListener
        //{
        //    private readonly LinearLayoutManager LayoutManager;
        //    private readonly NativePostAdapter PostAdapter;
        //    public MyScroll(NativePostAdapter postAdapter, LinearLayoutManager layoutManager)
        //    {
        //        PostAdapter = postAdapter;
        //        LayoutManager = layoutManager;
        //    }
        //    public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
        //    {
        //        try
        //        {
        //            base.OnScrolled(recyclerView, dx, dy);

        //            if (!PostAdapter.Loading && PostAdapter.ItemCount > 10)
        //            {
        //                if (LayoutManager != null && LayoutManager.FindLastCompletelyVisibleItemPosition() == PostAdapter.ItemCount - 2)
        //                {
        //                    //bottom of list!
        //                    int currentPage = PostAdapter.ItemCount / 5;
        //                    PostAdapter.OnLoadMoreListener.OnLoadMore(currentPage);
        //                    PostAdapter.Loading = true;
        //                }
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            Methods.DisplayReportResultTrack(e);
        //        }
        //    }
        //}

        public void SetLoading()
        {
            try
            {
                switch (ItemCount)
                {
                    case > 0:
                        {
                            var list = ListDiffer.FirstOrDefault(anjo => anjo.TypeView == PostModelType.ViewProgress);
                            switch (list)
                            {
                                case null:
                                    {
                                        var data = new AdapterModelsClass
                                        {
                                            TypeView = PostModelType.ViewProgress,
                                            Progress = true,
                                        };
                                        ListDiffer.Add(data);
                                        NotifyItemInserted(ListDiffer.IndexOf(data));
                                        //Loading = true;
                                        break;
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
        }

        public void SetLoaded()
        {
            try
            {
                //Loading = false;
                //var list = ListDiffer.FirstOrDefault(anjo => anjo.TypeView == PostModelType.ViewProgress);
                //if (list != null)
                //{
                //    ListDiffer.Remove(list);
                //    NotifyItemRemoved(ListDiffer.IndexOf(list));
                //}
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        public override bool OnFailedToRecycleView(Object holder)
        {
            Console.WriteLine("WoLog: OnFailedToRecycleView  ++ GetType " + holder.GetType());
            return true;
        }

        private void BindEmptyState(AdapterHolders.EmptyStateAdapterViewHolder holder)
        {
            try
            {
                holder.EmptyText.Text = NativePostType switch
                {
                    NativeFeedType.HashTag => ActivityContext.GetText(Resource.String.Lbl_NoPost_TitleText_hashtag),
                    NativeFeedType.Saved => ActivityContext.GetText(Resource.String.Lbl_NoPost_TitleText_saved),
                    NativeFeedType.Group => ActivityContext.GetText(Resource.String.Lbl_NoPost_TitleText_Group),
                    _ => ActivityContext.GetText(Resource.String.Lbl_NoPost_TitleText)
                };
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public AdapterModelsClass GetItem(int position)
        {
            try
            {
                if (ListDiffer.Count > position)
                {
                    var item = ListDiffer[position];
                    return item;
                }
                return null;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null;
            }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override int ItemCount => ListDiffer?.Count ?? 0;

        public override int GetItemViewType(int position)
        {
            try
            {
                var item = ListDiffer[position];

                return item switch
                {
                    null => (int)PostModelType.NormalPost,
                    _ => item.TypeView switch
                    {
                        PostModelType.SharedHeaderPost => (int)PostModelType.SharedHeaderPost,
                        PostModelType.HeaderPost => (int)PostModelType.HeaderPost,
                        PostModelType.TextSectionPostPart => (int)PostModelType.TextSectionPostPart,
                        PostModelType.PrevBottomPostPart => (int)PostModelType.PrevBottomPostPart,
                        PostModelType.BottomPostPart => (int)PostModelType.BottomPostPart,
                        PostModelType.Divider => (int)PostModelType.Divider,
                        PostModelType.AddCommentSection => (int)PostModelType.AddCommentSection,
                        PostModelType.CommentSection => (int)PostModelType.CommentSection,
                        PostModelType.AdsPost => (int)PostModelType.AdsPost,
                        PostModelType.AlertBoxAnnouncement => (int)PostModelType.AlertBoxAnnouncement,
                        PostModelType.AlertBox => (int)PostModelType.AlertBox,
                        PostModelType.AddPostBox => (int)PostModelType.AddPostBox,
                        PostModelType.SocialLinks => (int)PostModelType.SocialLinks,
                        PostModelType.VideoPost => (int)PostModelType.VideoPost,
                        PostModelType.AboutBox => (int)PostModelType.AboutBox,
                        PostModelType.InfoUserBox => (int)PostModelType.InfoUserBox,
                        PostModelType.BlogPost => (int)PostModelType.BlogPost,
                        PostModelType.AgoraLivePost => (int)PostModelType.AgoraLivePost,
                        PostModelType.LivePost => (int)PostModelType.LivePost,
                        PostModelType.DeepSoundPost => (int)PostModelType.DeepSoundPost,
                        PostModelType.EmptyState => (int)PostModelType.EmptyState,
                        PostModelType.FilePost => (int)PostModelType.FilePost,
                        PostModelType.MapPost => (int)PostModelType.MapPost,
                        PostModelType.FollowersBox => (int)PostModelType.FollowersBox,
                        PostModelType.GroupsBox => (int)PostModelType.GroupsBox,
                        PostModelType.SuggestedPagesBox => (int)PostModelType.SuggestedPagesBox,
                        PostModelType.SuggestedGroupsBox => (int)PostModelType.SuggestedGroupsBox,
                        PostModelType.SuggestedUsersBox => (int)PostModelType.SuggestedUsersBox,
                        PostModelType.ImagePost => (int)PostModelType.ImagePost,
                        PostModelType.ImagesBox => (int)PostModelType.ImagesBox,
                        PostModelType.LinkPost => (int)PostModelType.LinkPost,
                        PostModelType.PagesBox => (int)PostModelType.PagesBox,
                        PostModelType.PlayTubePost => (int)PostModelType.PlayTubePost,
                        PostModelType.ProductPost => (int)PostModelType.ProductPost,
                        PostModelType.StickerPost => (int)PostModelType.StickerPost,
                        PostModelType.Story => (int)PostModelType.Story,
                        PostModelType.VoicePost => (int)PostModelType.VoicePost,
                        PostModelType.YoutubePost => (int)PostModelType.YoutubePost,
                        PostModelType.Section => (int)PostModelType.Section,
                        PostModelType.AlertJoinBox => (int)PostModelType.AlertJoinBox,
                        PostModelType.SharedPost => (int)PostModelType.SharedPost,
                        PostModelType.EventPost => (int)PostModelType.EventPost,
                        PostModelType.ColorPost => (int)PostModelType.ColorPost,
                        PostModelType.FacebookPost => (int)PostModelType.FacebookPost,
                        PostModelType.VimeoPost => (int)PostModelType.VimeoPost,
                        PostModelType.MultiImage2 => (int)PostModelType.MultiImage2,
                        PostModelType.MultiImage3 => (int)PostModelType.MultiImage3,
                        PostModelType.MultiImage4 => (int)PostModelType.MultiImage4,
                        PostModelType.MultiImage5 => (int)PostModelType.MultiImage5,
                        PostModelType.MultiImage6 => (int)PostModelType.MultiImage6,
                        PostModelType.MultiImage7 => (int)PostModelType.MultiImage7,
                        PostModelType.MultiImage9 => (int)PostModelType.MultiImage9,
                        PostModelType.MultiImage10 => (int)PostModelType.MultiImage10,
                        PostModelType.MultiImages => (int)PostModelType.MultiImages,
                        PostModelType.JobPostSection => (int)PostModelType.JobPostSection,
                        PostModelType.FundingPost => (int)PostModelType.FundingPost,
                        PostModelType.PurpleFundPost => (int)PostModelType.PurpleFundPost,
                        PostModelType.PollPost => (int)PostModelType.PollPost,
                        PostModelType.AdMob1 => (int)PostModelType.AdMob1,
                        PostModelType.AdMob2 => (int)PostModelType.AdMob2,
                        PostModelType.AdMob3 => (int)PostModelType.AdMob3,
                        PostModelType.FbAdNative => (int)PostModelType.FbAdNative,
                        PostModelType.OfferPost => (int)PostModelType.OfferPost,
                        PostModelType.ViewProgress => (int)PostModelType.ViewProgress,
                        PostModelType.PromotePost => (int)PostModelType.PromotePost,
                        PostModelType.NormalPost => (int)PostModelType.NormalPost,
                        PostModelType.InfoPageBox => (int)PostModelType.InfoPageBox,
                        PostModelType.InfoGroupBox => (int)PostModelType.InfoGroupBox,
                        PostModelType.TikTokPost => (int)PostModelType.TikTokPost,
                        PostModelType.TwitterPost => (int)PostModelType.TwitterPost,
                        _ => (int)PostModelType.NormalPost
                    }
                };
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return (int)PostModelType.NormalPost;
            }
        }

        public override void OnViewRecycled(Object holder)
        {
            try
            {
                if (ActivityContext?.IsDestroyed != false)
                    return;

                if (holder != null)
                {
                    switch (holder)
                    {
                        case AdapterHolders.PostImageSectionViewHolder viewHolder:
                            Glide.With(viewHolder.ItemView).Clear(viewHolder.Image);

                            Console.WriteLine("WoLog: OnViewRecycled  ++ PostImageSectionViewHolder " + holder.GetType());
                            viewHolder.Image.SetImageDrawable(null);
                            Glide.With(ActivityContext?.BaseContext).OnTrimMemory(TrimMemory.RunningModerate);
                            break;
                        case AdapterHolders.PostTopSectionViewHolder viewHolder2:
                            Console.WriteLine("WoLog: OnViewRecycled  ++ PostTopSectionViewHolder " + holder.GetType());
                            Glide.With(viewHolder2.ItemView).Clear(viewHolder2.UserAvatar);
                            //viewHolder2.UserAvatar.SetImageDrawable(null);
                            //viewHolder2.TimeText.Text = null;
                            //viewHolder2.Username.Text = null;
                            break;
                        case AdapterHolders.PostBottomSectionViewHolder viewHolder18:
                            Console.WriteLine("WoLog: OnViewRecycled  ++ PostBottomSectionViewHolder " + holder.GetType());
                            //viewHolder18.SecondReactionButton = null;
                            //viewHolder18.LikeButton = null;
                            break;
                        case AdapterHolders.EventPostViewHolder viewHolder3:
                            Glide.With(ActivityContext?.BaseContext).Clear(viewHolder3.Image);
                            break;
                        case AdapterHolders.ProductPostViewHolder viewHolder4:
                            Glide.With(ActivityContext?.BaseContext).Clear(viewHolder4.Image);
                            break;
                        case AdapterHolders.OfferPostViewHolder viewHolder5:
                            Glide.With(ActivityContext?.BaseContext).Clear(viewHolder5.ImageOffer);
                            break;
                        case AdapterHolders.PostBlogSectionViewHolder viewHolder6:
                            Glide.With(ActivityContext?.BaseContext).Clear(viewHolder6.ImageBlog);
                            break;
                        case AdapterHolders.YoutubePostViewHolder viewHolder7:
                            Glide.With(ActivityContext?.BaseContext).Clear(viewHolder7.Image);
                            break;
                        case AdapterHolders.PostVideoSectionViewHolder viewHolder8:
                            Glide.With(ActivityContext?.BaseContext).Clear(viewHolder8.VideoImage);
                            break;
                        case AdapterHolders.FundingPostViewHolder viewHolder9:
                            Glide.With(ActivityContext?.BaseContext).Clear(viewHolder9.Image);
                            break;
                        case AdapterHolders.LinkPostViewHolder viewHolder10:
                            Glide.With(ActivityContext?.BaseContext).Clear(viewHolder10.Image);
                            break;
                        case AdapterHolders.PostColorBoxSectionViewHolder viewHolder11:
                            Glide.With(ActivityContext?.BaseContext).Clear(viewHolder11.ColorBoxImage);
                            break;
                        case AdapterHolders.PostAddCommentSectionViewHolder viewHolder20:
                            Glide.With(ActivityContext?.BaseContext).Clear(viewHolder20.ProfileImageView);
                            break;
                        case CommentAdapterViewHolder viewHolder21:
                            {
                                if (viewHolder21.CommentImage != null)
                                    Glide.With(ActivityContext?.BaseContext).Clear(viewHolder21.CommentImage);
                                break;
                            }
                        case AdapterHolders.PostTopSharedSectionViewHolder viewHolder22:
                            Glide.With(ActivityContext?.BaseContext).Clear(viewHolder22.UserAvatar);
                            break;
                        case AdapterHolders.PostPrevBottomSectionViewHolder viewHolder23:
                            Glide.With(ActivityContext?.BaseContext).Clear(viewHolder23.ImageCountLike);
                            break;
                        case AdapterHolders.JobPostViewHolder viewHolder24:
                            Glide.With(ActivityContext?.BaseContext).Clear(viewHolder24.JobCoverImage);
                            break;
                        case AdapterHolders.PostMultiImagesViewHolder viewHolder12:
                            Glide.With(ActivityContext?.BaseContext).Clear(viewHolder12.Image);
                            Glide.With(ActivityContext?.BaseContext).Clear(viewHolder12.Image2);
                            Glide.With(ActivityContext?.BaseContext).Clear(viewHolder12.Image3);
                            viewHolder12.Image.SetImageDrawable(null);
                            viewHolder12.Image2.SetImageDrawable(null);
                            viewHolder12.Image3.SetImageDrawable(null);
                            break;
                        case AdapterHolders.PostMultiImageViewHolder viewHolder13:
                            if (viewHolder13.Image != null) Glide.With(ActivityContext?.BaseContext).Clear(viewHolder13.Image);
                            if (viewHolder13.Image2 != null) Glide.With(ActivityContext?.BaseContext).Clear(viewHolder13.Image2);
                            if (viewHolder13.Image3 != null) Glide.With(ActivityContext?.BaseContext).Clear(viewHolder13.Image3);
                            if (viewHolder13.Image4 != null) Glide.With(ActivityContext?.BaseContext).Clear(viewHolder13.Image4);
                            if (viewHolder13.Image5 != null) Glide.With(ActivityContext?.BaseContext).Clear(viewHolder13.Image5);
                            if (viewHolder13.Image6 != null) Glide.With(ActivityContext?.BaseContext).Clear(viewHolder13.Image6);
                            if (viewHolder13.Image7 != null) Glide.With(ActivityContext?.BaseContext).Clear(viewHolder13.Image7);
                            if (viewHolder13.Image8 != null) Glide.With(ActivityContext?.BaseContext).Clear(viewHolder13.Image8);
                            if (viewHolder13.Image9 != null) Glide.With(ActivityContext?.BaseContext).Clear(viewHolder13.Image9);
                            if (viewHolder13.Image10 != null) Glide.With(ActivityContext?.BaseContext).Clear(viewHolder13.Image10);
                            viewHolder13.Image?.SetImageDrawable(null);
                            viewHolder13.Image2?.SetImageDrawable(null);
                            viewHolder13.Image3?.SetImageDrawable(null);
                            viewHolder13.Image4?.SetImageDrawable(null);
                            viewHolder13.Image5?.SetImageDrawable(null);
                            viewHolder13.Image6?.SetImageDrawable(null);
                            viewHolder13.Image7?.SetImageDrawable(null);
                            viewHolder13.Image8?.SetImageDrawable(null);
                            viewHolder13.Image9?.SetImageDrawable(null);
                            viewHolder13.Image10?.SetImageDrawable(null);
                            break;
                        case AdapterHolders.PostMapSectionViewHolder viewHolder16:
                            Glide.With(ActivityContext?.BaseContext).Clear(viewHolder16.Image);
                            viewHolder16.Image.SetImageDrawable(null);
                            break;
                    }
                }
                base.OnViewRecycled(holder);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public string CleanImageLink(string url)
        {
            return url.Replace("?cache=0", "");
        }

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();

                var item = GetItem(p0);
                switch (item?.PostData)
                {
                    case null:
                        return d;
                }

                switch (item.PostData.PhotoMulti?.Count)
                {
                    case > 0:
                        d.AddRange(from photo in item.PostData.PhotoMulti where !string.IsNullOrEmpty(photo.Image) select photo.Image);
                        break;
                }

                switch (item.PostData.PhotoAlbum?.Count)
                {
                    case > 0:
                        d.AddRange(from photo in item.PostData.PhotoAlbum where !string.IsNullOrEmpty(photo.Image) select photo.Image);
                        break;
                }

                // Preload Color images 
                if (item.PostData.ColorId != "0")
                {
                    if (ListUtils.SettingsSiteList?.PostColors != null && ListUtils.SettingsSiteList?.PostColors.Value.PostColorsList != null)
                    {
                        var getColorObject = ListUtils.SettingsSiteList.PostColors.Value.PostColorsList.FirstOrDefault(a => a.Key == item.PostData.ColorId);
                        if (getColorObject.Value != null)
                        {
                            switch (string.IsNullOrEmpty(getColorObject.Value.Image))
                            {
                                case false:
                                    if (!d.Contains(getColorObject.Value.Image))
                                        d.Add(getColorObject.Value.Image);
                                    break;
                            }
                        }
                    }
                }

                // Preload Video images 
                if (!string.IsNullOrEmpty(item.PostData.PostFileFull) && PostFunctions.GetVideosExtensions(item.PostData.PostFileFull))
                {
                    switch (string.IsNullOrEmpty(item.PostData.PostFileThumb))
                    {
                        case false:
                            d.Add(item.PostData.PostFileThumb);
                            break;
                        default:
                            d.Add(item.PostData.PostFileFull);
                            break;
                    }
                }

                // Preload PostSticker images 
                if (item.PostData.PostSticker != null && !string.IsNullOrEmpty(item.PostData.PostSticker) && !d.Contains(item.PostData.PostSticker))
                    d.Add(item.PostData.PostSticker);

                switch (string.IsNullOrEmpty(item.PostData.PostLinkImage) && !d.Contains(item.PostData.PostLinkImage))
                {
                    case false:
                        d.Add(item.PostData.PostLinkImage); //+ "===" + p0);
                        break;
                }

                if (PostFunctions.GetImagesExtensions(item.PostData.PostFileFull) && !d.Contains(item.PostData.PostFileFull))
                    d.Add(item.PostData.PostFileFull);// + "===" + p0);

                switch (string.IsNullOrEmpty(item.PostData.PostFileThumb) && !d.Contains(item.PostData.PostFileThumb))
                {
                    case false:
                        d.Add(item.PostData.PostFileThumb);
                        break;
                    default:
                        {
                            if (PostFunctions.GetVideosExtensions(item.PostData.PostFileFull) && !d.Contains(item.PostData.PostFileFull) && !d.Contains(item.PostData.PostFileFull))
                                d.Add(item.PostData.PostFileFull);
                            break;
                        }
                }

                switch (string.IsNullOrEmpty(item.PostData.Publisher?.Avatar) && !d.Contains(item.PostData.Publisher?.Avatar))
                {
                    case false:
                        d.Add(item.PostData.Publisher.Avatar);
                        break;
                }

                switch (string.IsNullOrEmpty(item.PostData.PostYoutube) && !d.Contains("https://img.youtube.com/vi/" + item.PostData.PostYoutube + "/0.jpg"))
                {
                    case false:
                        d.Add("https://img.youtube.com/vi/" + item.PostData.PostYoutube + "/0.jpg");
                        break;
                }

                if (item.PostData.Product?.ProductClass?.Images != null)
                    d.AddRange(from productImage in item.PostData.Product.Value.ProductClass?.Images select productImage.Image);

                switch (string.IsNullOrEmpty(item.PostData.Blog?.BlogClass.Thumbnail) && !d.Contains(item.PostData.Blog?.BlogClass.Thumbnail))
                {
                    case false:
                        d.Add(item.PostData.Blog?.BlogClass.Thumbnail);
                        break;
                }

                switch (string.IsNullOrEmpty(item.PostData.Event?.EventClass?.Cover) && !d.Contains(item.PostData.Event?.EventClass?.Cover))
                {
                    case false:
                        d.Add(item.PostData.Event.Value.EventClass?.Cover);
                        break;
                }

                switch (string.IsNullOrEmpty(item.PostData?.PostMap))
                {
                    case false:
                        {
                            switch (item.PostData.PostMap.Contains("https://maps.googleapis.com/maps/api/staticmap?"))
                            {
                                case false:
                                    {
                                        string imageUrlMap = "https://maps.googleapis.com/maps/api/staticmap?";
                                        //imageUrlMap += "center=" + item.CurrentLatitude + "," + item.CurrentLongitude;
                                        imageUrlMap += "center=" + item.PostData.PostMap.Replace("/", "");
                                        imageUrlMap += "&zoom=10";
                                        imageUrlMap += "&scale=1";
                                        imageUrlMap += "&size=300x300";
                                        imageUrlMap += "&maptype=roadmap";
                                        imageUrlMap += "&key=" + ActivityContext.GetText(Resource.String.google_maps_key);
                                        imageUrlMap += "&format=png";
                                        imageUrlMap += "&visual_refresh=true";
                                        imageUrlMap += "&markers=size:small|color:0xff0000|label:1|" + item.PostData.PostMap.Replace("/", "");

                                        item.PostData.ImageUrlMap = imageUrlMap;
                                        break;
                                    }
                                default:
                                    item.PostData.ImageUrlMap = item.PostData.PostMap;
                                    break;
                            }

                            if (!d.Contains(item.PostData.ImageUrlMap))
                                d.Add(item.PostData.ImageUrlMap);

                            break;
                        }
                }

                return d;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                var d = new List<string>();
                return d;
            }
        }

        private readonly List<string> ImageCachedList = new List<string>();
        private readonly List<string> ImageCircleCachedList = new List<string>();
        private int Count;
        public RequestBuilder GetPreloadRequestBuilder(Object p0)
        {
            try
            {
                var url = p0.ToString();
                if (url.Contains("avatar.png") || url.Contains("avatar.jpg") && !ImageCachedList.Contains(url))
                {
                    ImageCachedList.Add(url);
                    // Console.WriteLine("WoLog: GetPreloadRequestBuilder Avatar  >> = " + url + " ==== if");
                    // return CircleGlideRequestBuilder.Clone().Override(50).Load(url) ;
                }
                else if (url.EndsWith("####"))
                {
                    var url2 = url.Replace("####", "");
                    url = url2;
                    return FullGlideRequestBuilder.Clone().Thumbnail(GlideThumbnailRequestBuilder.Clone().AddListener(new GlidePreLoaderRequestListener("Preload Half size Multiple")).Override(50).Load(url)).Apply(RequestOptions.SignatureOf(new ObjectKey(url + "Thumb"))).Override(ScreenWidthPixels / 4, ScreenHeightPixels / 4).AddListener(new GlidePreLoaderRequestListener("Preload Normal")).Apply(RequestOptions.SignatureOf(new ObjectKey(url))).Load(url);
                }
                else
                {
                    if (!ImageCachedList.Contains(url))
                    {
                        ImageCachedList.Add(url);
                        Console.WriteLine("WoLog: GetPreloadRequestBuilder  >> = " + url + " ==== else");

                        var FullGlideRequestBuilder2 = Glide.With(ActivityContext?.BaseContext).Load(url).Into(new GlideTarget());

                        //return Glide.With(ActivityContext?.BaseContext).Load(url).AddListener(new AdapterBind.GlideCustomRequestListener("Preload Normal")).SetSizeMultiplier(0.1f).Downsample(DownsampleStrategy.CenterInside).Override(200, 250);
                        return FullGlideRequestBuilder.Clone().Thumbnail(GlideThumbnailRequestBuilder.Clone().AddListener(new GlidePreLoaderRequestListener("Preload Thumbnail")).Override(50).Load(url)).Apply(RequestOptions.SignatureOf(new ObjectKey(url + "Thumb"))).Override(ScreenWidthPixels, ScreenHeightPixels).AddListener(new GlidePreLoaderRequestListener("Preload Normal")).Apply(RequestOptions.SignatureOf(new ObjectKey(url))).Load(url);




                        //var glideRequestOptions = new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.None).SkipMemoryCache(true).Placeholder(new ColorDrawable(Color.ParseColor("#EFEFEF"))).Error(Resource.Drawable.ImagePlacholder).Format(Bumptech.Glide.Load.DecodeFormat.PreferRgb565).Apply(RequestOptions.SignatureOf(new ObjectKey(DateTime.Now.Millisecond)));

                        //FullGlideRequestBuilder.DontTransform_T();
                        //FullGlideRequestBuilder.Downsample(DownsampleStrategy.CenterInside).Transition(DrawableTransitionOptions.WithCrossFade(250)).AddListener(new AdapterBind.GlideCustomRequestListener("Preload Normal")).Load(url);
                        ////GlideBuilder.ReferenceEquals(FullGlideRequestBuilder, FullGlideRequestBuilder);

                    }
                }

                var f = Count++;
                //Console.WriteLine("Preloaded ++ " + f + " ++++ " + p0);
                return null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }

        }

        #region Glide

        public static List<int> HightImagePreloader = new List<int>();

        public class GlideTarget : Object, ITarget
        {
            public ImageView HolderImageView;

            public IRequest Request { get; set; }

            public void GetSize(ISizeReadyCallback p0)
            {

            }

            public void OnDestroy()
            {

            }

            public void OnLoadCleared(Drawable p0)
            {

            }

            public void OnLoadFailed(Drawable p0)
            {

            }

            public void OnLoadStarted(Drawable p0)
            {

            }

            public void OnResourceReady(Object p0, ITransition p1)
            {
                try
                {
                    Bitmap bitmap = ((Bitmap)p0);

                    int height = bitmap.Height;
                    int width = bitmap.Width;


                    HolderImageView.SetImageBitmap(bitmap);



                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }

            }

            public void OnSizeReady(int p0, int p1)
            {
                Console.WriteLine("WoLog: Glide / OnSizeReady Width = " + p0 + " Height= " + p1);
            }

            public void OnStart()
            {

            }

            public void OnStop()
            {
            }

            public void RemoveCallback(ISizeReadyCallback p0)
            {

            }
        }

        public class GlidePreLoaderRequestListener : Object, IRequestListener
        {
            string typeofload = "Default";
            public GlidePreLoaderRequestListener(string type)
            {
                typeofload = type;
            }


            public bool OnLoadFailed(GlideException p0, Object p1, ITarget p2, bool p3)
            {

                foreach (var d in p0.Causes)
                {
                    Console.WriteLine("WoLog: Glide / GlideException Cause = " + d.Cause + " Object= " + p1);
                }

                p2.GetSize(new GlideIsize());



                return false;
            }


            public bool OnResourceReady(Object p0, Object p1, ITarget p2, DataSource p3, bool p4)
            {
                try
                { //Glide gif drawble
                    Bitmap bitmap;
                    var myObject = p0 as Bitmap;

                    if (myObject != null)
                        bitmap = ((Bitmap)p0);
                    else
                        bitmap = ((BitmapDrawable)p0).Bitmap;


                    var BytehasKB = Math.Round((double)bitmap.ByteCount / 1024, 0);



                    if (typeofload == "Preload Normal")
                        HightImagePreloader.Add((int)bitmap.GetBitmapInfo().Height);


                    Console.WriteLine("WoLog: Glide Preload TYPE: " + typeofload + " / OnResourceReady URL: " + p1 + " Width = " + bitmap.GetBitmapInfo().Width + " Height= " + bitmap.GetBitmapInfo().Height + " AllocationByteCount = " + bitmap.AllocationByteCount + " ByteCount = " + bitmap.ByteCount + " to KB = " + BytehasKB);

                    var Aspect = bitmap.GetBitmapInfo().Width / bitmap.GetBitmapInfo().Height;
                    if (bitmap.GetBitmapInfo().Height > bitmap.GetBitmapInfo().Width)
                    {

                        Console.WriteLine("WoLog: Glide ScaleType: Height: " + bitmap.GetBitmapInfo().Height + " x Width:" + bitmap.GetBitmapInfo().Width + " Aspect:" + Aspect);
                    }

                    p2.GetSize(new GlideIsize());




                    return false;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                    return false;
                }

            }
        }

        public class GlideIsize : Object, ISizeReadyCallback
        {
            public void OnSizeReady(int p0, int p1)
            {
                Console.WriteLine("WoLog: Glide / OnSizeReady Width = " + p0 + " Height= " + p1);
            }
        }


        #endregion

        public void AutoLinkTextClick(StTools.XAutoLinkMode p0, string p1, Dictionary<string, string> userData)
        {
            try
            {
                p1 = p1.Replace(" ", "").Replace("\n", "");
                var typeText = Methods.FunString.Check_Regex(p1);
                switch (typeText)
                {
                    case "Email":
                        Methods.App.SendEmail(ActivityContext, p1);
                        break;
                    case "Website":
                        {
                            string url = p1.Contains("http") switch
                            {
                                false => "http://" + p1,
                                _ => p1
                            };

                            var shortLinkPlaytube = AppSettings.PlayTubeShortUrlSite + "watch/";
                            if (url.Contains(shortLinkPlaytube))
                            {
                                var videoId = url.Split("/watch/").Last();

                                var playTubeUrl = ListUtils.SettingsSiteList?.PlaytubeUrl;
                                url = playTubeUrl + "/watch/" + videoId;
                            }
                            //var intent = new Intent(MainContext, typeof(LocalWebViewActivity));
                            //intent.PutExtra("URL", url);
                            //intent.PutExtra("Type", url);
                            //MainContext.StartActivity(intent);
                            new IntentController(ActivityContext).OpenBrowserFromApp(url);
                            break;
                        }
                    case "Hashtag":
                        {
                            var intent = new Intent(ActivityContext, typeof(HashTagPostsActivity));
                            intent.PutExtra("Id", p1);
                            intent.PutExtra("Tag", p1);
                            ActivityContext.StartActivity(intent);
                            break;
                        }
                    case "Mention":
                        {
                            var dataUSer = ListUtils.MyProfileList?.FirstOrDefault();
                            string name = p1.Replace("@", "");

                            var sqlEntity = new SqLiteDatabase();
                            var user = sqlEntity.Get_DataOneUser(name);


                            if (user != null)
                            {
                                WoWonderTools.OpenProfile(ActivityContext, user.UserId, user);
                            }
                            else
                            {
                                switch (userData?.Count)
                                {
                                    case > 0:
                                        {
                                            var data = userData.FirstOrDefault(a => a.Value == name);
                                            if (data.Key != null && data.Key == UserDetails.UserId)
                                            {
                                                switch (PostClickListener.OpenMyProfile)
                                                {
                                                    case true:
                                                        return;
                                                    default:
                                                        {
                                                            var intent = new Intent(ActivityContext, typeof(MyProfileActivity));
                                                            ActivityContext.StartActivity(intent);
                                                            break;
                                                        }
                                                }
                                            }
                                            else if (data.Key != null)
                                            {
                                                var intent = new Intent(ActivityContext, typeof(UserProfileActivity));
                                                //intent.PutExtra("UserObject", JsonConvert.SerializeObject(item));
                                                intent.PutExtra("UserId", data.Key);
                                                ActivityContext.StartActivity(intent);
                                            }
                                            else
                                            {
                                                if (name == dataUSer?.Name || name == dataUSer?.Username)
                                                {
                                                    switch (PostClickListener.OpenMyProfile)
                                                    {
                                                        case true:
                                                            return;
                                                        default:
                                                            {
                                                                var intent = new Intent(ActivityContext, typeof(MyProfileActivity));
                                                                ActivityContext.StartActivity(intent);
                                                                break;
                                                            }
                                                    }
                                                }
                                                else
                                                {
                                                    var intent = new Intent(ActivityContext, typeof(UserProfileActivity));
                                                    //intent.PutExtra("UserObject", JsonConvert.SerializeObject(item));
                                                    intent.PutExtra("name", name);
                                                    ActivityContext.StartActivity(intent);
                                                }
                                            }

                                            break;
                                        }
                                    default:
                                        {
                                            if (name == dataUSer?.Name || name == dataUSer?.Username)
                                            {
                                                switch (PostClickListener.OpenMyProfile)
                                                {
                                                    case true:
                                                        return;
                                                    default:
                                                        {
                                                            var intent = new Intent(ActivityContext, typeof(MyProfileActivity));
                                                            ActivityContext.StartActivity(intent);
                                                            break;
                                                        }
                                                }
                                            }
                                            else
                                            {
                                                var intent = new Intent(ActivityContext, typeof(UserProfileActivity));
                                                //intent.PutExtra("UserObject", JsonConvert.SerializeObject(item));
                                                intent.PutExtra("name", name);
                                                ActivityContext.StartActivity(intent);
                                            }

                                            break;
                                        }
                                }
                            }

                            break;
                        }
                    case "Number":
                        Methods.App.SaveContacts(ActivityContext, p1, "", "2");
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }

    public sealed class PreCachingLayoutManager : LinearLayoutManager
    {
        private readonly Context Context;
        private int ExtraLayoutSpace = -1;
        private readonly int DefaultExtraLayoutSpace = 600;
        private OrientationHelper MOrientationHelper;
        private int MAdditionalAdjacentPrefetchItemCount;

        //wael Error on android 8.1
        //private RecyclerView.State recyclerstate;
        //public override void OnLayoutChildren(RecyclerView.Recycler recycler, RecyclerView.State state)
        //{
        //    try
        //    {
        //        recyclerstate = state;
        //        base.OnLayoutChildren(recycler, state);
        //    }
        //    catch (IndexOutOfBoundsException e)
        //    {
        //        Methods.DisplayReportResultTrack(e);
        //    }
        //}

        public PreCachingLayoutManager(Activity context) : base(context)
        {
            try
            {
                Context = context;
                Init();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void Init()
        {
            try
            {
                MOrientationHelper = OrientationHelper.CreateOrientationHelper(this, Orientation);
                ItemPrefetchEnabled = true;
                InitialPrefetchItemCount = 20;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetExtraLayoutSpace(int space)
        {
            ExtraLayoutSpace = space;
        }

        protected override void CalculateExtraLayoutSpace(RecyclerView.State state, int[] extraLayoutSpace)
        {
            //Console.WriteLine("CalculateExtraLayoutSpace = Data Loaded extraLayoutSpace " + extraLayoutSpace + " RecyclerView State now" + state);
            ////Trace.BeginSection("CalculateExtraLayoutSpace Simulation");
            //if (state.HasTargetScrollPosition)
            //{
            //    var d = state.TargetScrollPosition;
            //}

            //extraLayoutSpace[0] = MOrientationHelper.TotalSpace * 1;
            //extraLayoutSpace[1] = MOrientationHelper.TotalSpace * 1;

            base.CalculateExtraLayoutSpace(state, extraLayoutSpace);
        }

        [Obsolete("deprecated")]
        protected override int GetExtraLayoutSpace(RecyclerView.State state)
        {
            if (state.HasTargetScrollPosition)
            {
                return MOrientationHelper.TotalSpace * 2;
            }

            return MOrientationHelper.TotalSpace * 2;

            //return ExtraLayoutSpace switch
            //{
            //    > 0 => ExtraLayoutSpace,
            //    _ => DefaultExtraLayoutSpace
            //};
        }

        public void SetPreloadItemCount(int preloadItemCount)
        {
            MAdditionalAdjacentPrefetchItemCount = preloadItemCount switch
            {
                < 1 => throw new IllegalArgumentException("adjacentPrefetchItemCount must not smaller than 1!"),
                _ => preloadItemCount - 1
            };
        }

        public List<int> checkerinit = new List<int>();

        public void PrefetcOnIdleMode()
        {
            //this.CollectAdjacentPrefetchPositions(-3,0, recyclerstate, ((ILayoutPrefetchRegistry)this).mul(1, 1));
        }

        public override void CollectAdjacentPrefetchPositions(int dx, int dy, RecyclerView.State state, ILayoutPrefetchRegistry layoutPrefetchRegistry)
        {
            try

            {
                //Console.WriteLine("CollectAdjacentPrefetchPositions / state.RemainingScrollVertical = " + state.RemainingScrollVertical);
                //return;


                var delta1 = Orientation == Horizontal ? dx : dy;
                var layoutDirection1 = delta1 > 0 ? 1 : -1;
                var child1 = GetChildClosest(layoutDirection1);
                var currentPosition1 = GetPosition(child1) + layoutDirection1;

                base.CollectAdjacentPrefetchPositions(dx, dy, state, layoutPrefetchRegistry);
                //return;
                //if (checkerinit.Contains(currentPosition1))
                //{
                //    return;
                //}
                //else
                //{
                //    checkerinit.Add(currentPosition1);
                //}

                //Console.WriteLine("WoWonderallen = state ItemCount " + state.ItemCount + " state TargetScrollPosition" + state.TargetScrollPosition);





                var delta = Orientation == Horizontal ? dx : dy;
                if (ChildCount == 0 || delta == 0)
                    return;

                var layoutDirection = delta > 0 ? 1 : -1;
                var child = GetChildClosest(layoutDirection);
                var currentPosition = GetPosition(child) + layoutDirection;

                int scrollingOffset;

                if (layoutDirection != 1)
                    return;

                scrollingOffset = MOrientationHelper.GetDecoratedEnd(child) - MOrientationHelper.EndAfterPadding;


                //Console.WriteLine("CollectAdjacentPrefetchPositions / currentPosition = " + currentPosition);
                //Console.WriteLine("CollectAdjacentPrefetchPositions / scrollingOffset = " + scrollingOffset);
                //Console.WriteLine("CollectAdjacentPrefetchPositions / MAdditionalAdjacentPrefetchItemCount = " + MAdditionalAdjacentPrefetchItemCount);

                //switch (currentPosition)
                //{
                //    case >= 0 when currentPosition < state.ItemCount:
                //        var d = Java.Lang.Math.Max(0, scrollingOffset);
                //        Console.WriteLine("CollectAdjacentPrefetchPositions AddPosition / I = " + currentPosition + " Max = " + d);
                //        layoutPrefetchRegistry.AddPosition(currentPosition, d);
                //        break;
                //}

                //Console.WriteLine("CollectAdjacentPrefetchPositions / ChildCount = " + ChildCount);
                //Console.WriteLine("CollectAdjacentPrefetchPositions / currentPosition = " + currentPosition);
                //Console.WriteLine("CollectAdjacentPrefetchPositions / scrollingOffset = " + scrollingOffset);
                //Console.WriteLine("CollectAdjacentPrefetchPositions / MAdditionalAdjacentPrefetchItemCount = " + MAdditionalAdjacentPrefetchItemCount);

                //if (checkerinit.Contains(currentPosition))
                //{
                //    return;
                //}
                //else
                //{
                //    checkerinit.Add(currentPosition);
                //}

                Trace.BeginSection("CollectAdjacentPrefetchPositions Simulation " + currentPosition1);

                if (MAdditionalAdjacentPrefetchItemCount == 50 - 1)
                    MAdditionalAdjacentPrefetchItemCount = 40;

                if (MAdditionalAdjacentPrefetchItemCount == 20)
                    MAdditionalAdjacentPrefetchItemCount = 40;

                if (MAdditionalAdjacentPrefetchItemCount == 40)
                    MAdditionalAdjacentPrefetchItemCount = 60;





                for (var i = currentPosition + 1; i < currentPosition + MAdditionalAdjacentPrefetchItemCount + 1; i++)
                {
                    //Console.WriteLine("CollectAdjacentPrefetchPositions / Foreach I = " + (currentPosition + 1) + " < " + (currentPosition + MAdditionalAdjacentPrefetchItemCount + 1));
                    //Console.WriteLine("CollectAdjacentPrefetchPositions / State.ItemCount = " + (state.ItemCount));

                    //if (checkerinit.Contains(currentPosition))
                    //{
                    //    return;
                    //}
                    //else
                    //{
                    //    checkerinit.Add(currentPosition);
                    //}

                    switch (i)
                    {
                        case >= 0 when i < state.ItemCount:
                            var d = Java.Lang.Math.Max(0, scrollingOffset);
                            //Console.WriteLine("CollectAdjacentPrefetchPositions AddPosition / I = " + i + " Max = " + d);
                            layoutPrefetchRegistry.AddPosition(i, d);
                            break;
                    }
                }



                Trace.EndSection();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private View GetChildClosest(int layoutDirection)
        {
            return GetChildAt(layoutDirection == -1 ? 0 : ChildCount - 1);
        }
    }
}