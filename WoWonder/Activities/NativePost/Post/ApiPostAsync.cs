using Android.App;
using Android.OS;
using Android.Views;
using Java.Lang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WoWonder.Activities.NativePost.Extra;
using WoWonder.Activities.Tabbes;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Utils;
using WoWonder.MediaPlayers.Exo;
using WoWonderClient;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Classes.Story;
using WoWonderClient.Requests;
using static WoWonder.Helpers.Model.Classes;
using Exception = System.Exception;
using Math = System.Math;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.NativePost.Post
{
    public class ApiPostAsync
    {
        private readonly Activity ActivityContext;
        private readonly NativePostAdapter NativeFeedAdapter;
        private readonly WRecyclerView WRecyclerView;
        private static bool ShowFindMoreAlert;
        private static PostModelType LastAdsType = PostModelType.AdMob3;
        public static List<PostDataObject> PostCacheList { private set; get; }

        public ApiPostAsync(WRecyclerView recyclerView, NativePostAdapter adapter)
        {
            try
            {
                ActivityContext = adapter.ActivityContext;
                NativeFeedAdapter = adapter;
                WRecyclerView = recyclerView;
                PostCacheList = new List<PostDataObject>();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Api V2

        Random rand = new Random();
        private Task task;

        public void ExcuteDataToMainThread(string offset = "0", string typeRun = "Add", string hash = "")
        {
            try
            {
                var beforeList = NativeFeedAdapter.ListDiffer.Count;

                if (beforeList > 150)
                {
                    NativeFeedAdapter.ListDiffer.RemoveRange(10, Math.Min(30, beforeList));
                    NativeFeedAdapter.NotifyItemRangeRemoved(10, Math.Min(30, beforeList));

                    Console.WriteLine("API = Ended with offset " + offset + "With count of " + NativeFeedAdapter.ListDiffer.Count);
                }

                task = Task.Run(async () => await FetchFeedPostsApi(offset, typeRun, hash)).ContinueWith(task =>
                {
                    ////try
                    ////{
                    ////    // Executes in UI thread.
                    ////    var NewPostsList = task.Result;
                    ////    if (NewPostsList == null || NewPostsList.Count <= 0)
                    ////        return;

                    ////    NativeFeedAdapter.ListDiffer.AddRange(NewPostsList);

                    ////    var recyclerScrollFixer = new Runnable(() =>
                    ////    {
                    ////        if (beforeList == 0)
                    ////            NativeFeedAdapter.NotifyDataSetChanged();

                    ////        //WRecyclerView.SetItemAnimator(null);
                    ////        NativeFeedAdapter.NotifyItemRangeInserted(beforeList, NewPostsList.Count);
                    ////        Console.WriteLine("API = Ended with offset " + offset + "With count of " + NativeFeedAdapter.ListDiffer.Count);
                    ////    });

                    ////    WRecyclerView.Post(recyclerScrollFixer);
                    ////    WRecyclerView.MainScrollEvent.IsLoading = false;

                    ////    WRecyclerView.Visibility = ViewStates.Visible;
                    ////    WRecyclerView?.ShimmerInflater?.Hide();
                    ////}
                    ////catch (Exception e)
                    ////{
                    ////    Methods.DisplayReportResultTrack(e);
                    ////}
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        public async Task FetchFeedPostsApi(string offset = "0", string typeRun = "Add", string hash = "")
        {
            try
            {
                if ((task == null) && (task?.IsCompleted == false || task?.Status == TaskStatus.Running))
                    return;

                int apiStatus;
                dynamic respond;
                WRecyclerView.Hash = hash;

                if (WRecyclerView.MainScrollEvent.IsLoading)
                    return;

                var adId = NativeFeedAdapter.ListDiffer.LastOrDefault(a => a.TypeView == PostModelType.AdsPost && a.PostData.PostType == "ad")?.PostData?.Id ?? "";

                Console.WriteLine("API = Started FetchNewsFeedApi " + offset);
                Trace.BeginSection("API = Started FetchNewsFeedApi " + offset);
                WRecyclerView.MainScrollEvent.IsLoading = true;

                switch (NativeFeedAdapter.NativePostType)
                {
                    case NativeFeedType.Global:
                        (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, offset, "get_news_feed", NativeFeedAdapter.IdParameter, "", WRecyclerView.GetFilter(), adId, WRecyclerView.GetPostType());
                        break;
                    case NativeFeedType.User:
                        (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, offset, "get_user_posts", NativeFeedAdapter.IdParameter, "", "", adId);
                        break;
                    case NativeFeedType.Group:
                        (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, offset, "get_group_posts", NativeFeedAdapter.IdParameter, "", "", adId);
                        break;
                    case NativeFeedType.Page:
                        (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, offset, "get_page_posts", NativeFeedAdapter.IdParameter, "", "", adId);
                        break;
                    case NativeFeedType.Event:
                        (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, offset, "get_event_posts", NativeFeedAdapter.IdParameter, "", "", adId);
                        break;
                    case NativeFeedType.Saved:
                        (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, offset, "saved", "", "", "", adId);
                        break;
                    case NativeFeedType.HashTag:
                        (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, offset, "hashtag", "", hash, "", adId);
                        break;
                    case NativeFeedType.Video:
                        (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost("5", offset, "get_random_videos", "", "", "", adId);
                        break;
                    case NativeFeedType.Popular:
                        (apiStatus, respond) = await RequestsAsync.Posts.GetPopularPost(AppSettings.PostApiLimitOnScroll, offset);
                        break;
                    case NativeFeedType.Boosted:
                        (apiStatus, respond) = await RequestsAsync.Posts.GetBoostedPost();
                        break;
                    case NativeFeedType.Live:
                        (apiStatus, respond) = await RequestsAsync.Posts.GetLivePost();
                        break;
                    case NativeFeedType.Advertise:
                        (apiStatus, respond) = await RequestsAsync.Advertise.GetAdvertisePost(AppSettings.PostApiLimitOnScroll, offset);
                        break;
                    default:
                        (apiStatus, respond) = (400, null);
                        break;
                }

                Trace.EndSection();

                if (WRecyclerView.SwipeRefreshLayoutView is { Refreshing: true })
                    WRecyclerView.SwipeRefreshLayoutView.Refreshing = false;

                var countList2 = NativeFeedAdapter.ListDiffer.Count;

                Trace.BeginSection("LoadDataApi Start " + offset);

                Console.WriteLine("API = LoadDataApi Start " + offset);
                if (apiStatus != 200 || respond is not PostObject result || result.Data == null)
                {
                    WRecyclerView.MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(ActivityContext, respond);
                }
                else
                {
                    LoadDataApi(apiStatus, respond, offset);
                }

                Trace.EndSection();
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        public async Task<int> LoadDataApiAsync(int apiStatus, dynamic respond, string offset, string typeRun = "Add")
        {
            //offset = "10";


            if (respond is PostObject results)
            {
                await Task.Run(() =>
                {
                    Trace.BeginSection("LoadDataApiAsync For Each Simulation");
                    foreach (PostDataObject post in from post in results.Data let check = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a?.PostData?.PostId == post.PostId && a?.TypeView == PostFunctions.GetAdapterType(post)) where check == null select post)
                    {

                        // add = true;
                        var combiner = new FeedCombiner(null, NativeFeedAdapter.ListDiffer, ActivityContext, NativeFeedAdapter.NativePostType);

                        if (NativeFeedAdapter.NativePostType == NativeFeedType.Global)
                        {
                            if (results.Data.Count < 6 && NativeFeedAdapter.ListDiffer.Count < 6)
                                if (!ShowFindMoreAlert)
                                {
                                    ShowFindMoreAlert = true;

                                    combiner.AddFindMoreAlertPostView("Pages");
                                    combiner.AddFindMoreAlertPostView("Groups");
                                }

                            var check1 = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.SuggestedGroupsBox);
                            if (check1 == null && AppSettings.ShowSuggestedGroup && NativeFeedAdapter.ListDiffer.Count > 0 && NativeFeedAdapter.ListDiffer.Count % AppSettings.ShowSuggestedGroupCount == 0 && ListUtils.SuggestedGroupList.Count > 0)
                                combiner.AddSuggestedBoxPostView(PostModelType.SuggestedGroupsBox);

                            var check2 = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.SuggestedUsersBox);
                            if (check2 == null && AppSettings.ShowSuggestedUser && NativeFeedAdapter.ListDiffer.Count > 0 && NativeFeedAdapter.ListDiffer.Count % AppSettings.ShowSuggestedUserCount == 0 && ListUtils.SuggestedUserList.Count > 0)
                                combiner.AddSuggestedBoxPostView(PostModelType.SuggestedUsersBox);

                            var check3 = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.SuggestedPagesBox);
                            if (check3 == null && AppSettings.ShowSuggestedPage && NativeFeedAdapter.ListDiffer.Count > 0 && NativeFeedAdapter.ListDiffer.Count % AppSettings.ShowSuggestedPageCount == 0 && ListUtils.SuggestedPageList.Count > 0)
                                combiner.AddSuggestedBoxPostView(PostModelType.SuggestedPagesBox);
                        }
                        else if (NativeFeedAdapter.NativePostType == NativeFeedType.Advertise)
                        {
                            post.PostType = "ad";
                        }

                        if (NativeFeedAdapter.ListDiffer.Count % (AppSettings.ShowAdNativeCount * 10) == 0 && NativeFeedAdapter.ListDiffer.Count > 0 && AppSettings.ShowAdMobNativePost)
                            if (LastAdsType == PostModelType.AdMob1)
                            {
                                LastAdsType = PostModelType.AdMob2;
                                combiner.AddAdsPostView(PostModelType.AdMob1);
                            }
                            else if (LastAdsType == PostModelType.AdMob2)
                            {
                                LastAdsType = PostModelType.AdMob3;
                                combiner.AddAdsPostView(PostModelType.AdMob2);
                            }
                            else if (LastAdsType == PostModelType.AdMob3)
                            {
                                LastAdsType = PostModelType.AdMob1;
                                combiner.AddAdsPostView(PostModelType.AdMob3);
                            }

                        var combine = new FeedCombiner(RegexFilterText(post), NativeFeedAdapter.ListDiffer, ActivityContext, NativeFeedAdapter.NativePostType);
                        if (post.PostType == "ad" && AppSettings.ShowAdvertise)
                        {
                            combine.AddAdsPost();
                        }
                        else
                        {
                            bool isPromoted = post.IsPostBoosted == "1" || post.SharedInfo.SharedInfoClass != null && post.SharedInfo.SharedInfoClass?.IsPostBoosted == "1";
                            if (isPromoted)
                            {
                                if (NativeFeedAdapter.ListDiffer.Count == 0)
                                    combine.CombineDefaultPostSections();
                                else
                                {
                                    var p = NativeFeedAdapter.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.PromotePost);
                                    if (p != null)
                                        combine.CombineDefaultPostSections();
                                    else
                                        combine.CombineDefaultPostSections("Top");

                                }
                            }
                            else
                            {
                                combine.CombineDefaultPostSections();
                            }
                        }

                        if (NativeFeedAdapter.ListDiffer.Count % AppSettings.ShowAdNativeCount == 0 && NativeFeedAdapter.ListDiffer.Count > 0 && AppSettings.ShowFbNativeAds)
                            combiner.AddAdsPostView(PostModelType.FbAdNative);
                    }

                });
            }

            return NativeFeedAdapter.ItemCount;
        }

        #endregion

        #region Api

        public async Task FetchNewsFeedApiPosts(string offset = "0", string typeRun = "Add", string hash = "")
        {
            switch (WRecyclerView.MainScrollEvent.IsLoading)
            {
                case true:
                    return;
            }

            if (!Methods.CheckConnectivity())
                return;

            WRecyclerView.Hash = hash;
            int apiStatus;
            dynamic respond;

            WRecyclerView.MainScrollEvent.IsLoading = true;
            var adId = NativeFeedAdapter.ListDiffer.LastOrDefault(a => a.TypeView == PostModelType.AdsPost && a.PostData.PostType == "ad")?.PostData?.Id ?? "";
            switch (NativeFeedAdapter.NativePostType)
            {
                case NativeFeedType.Global:
                    (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, offset, "get_news_feed", NativeFeedAdapter.IdParameter, "", WRecyclerView.GetFilter(), adId, WRecyclerView.GetPostType());
                    break;
                case NativeFeedType.User:
                    (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, offset, "get_user_posts", NativeFeedAdapter.IdParameter, "", "", adId);
                    break;
                case NativeFeedType.Group:
                    (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, offset, "get_group_posts", NativeFeedAdapter.IdParameter, "", "", adId);
                    break;
                case NativeFeedType.Page:
                    (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, offset, "get_page_posts", NativeFeedAdapter.IdParameter, "", "", adId);
                    break;
                case NativeFeedType.Event:
                    (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, offset, "get_event_posts", NativeFeedAdapter.IdParameter, "", "", adId);
                    break;
                case NativeFeedType.Saved:
                    (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, offset, "saved", "", "", "", adId);
                    break;
                case NativeFeedType.HashTag:
                    (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, offset, "hashtag", "", hash, "", adId);
                    break;
                case NativeFeedType.Video:
                    (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost("5", offset, "get_random_videos", "", "", "", adId);
                    break;
                case NativeFeedType.Popular:
                    (apiStatus, respond) = await RequestsAsync.Posts.GetPopularPost(AppSettings.PostApiLimitOnScroll, offset);
                    break;
                case NativeFeedType.Boosted:
                    (apiStatus, respond) = await RequestsAsync.Posts.GetBoostedPost();
                    break;
                case NativeFeedType.Live:
                    (apiStatus, respond) = await RequestsAsync.Posts.GetLivePost();
                    break;
                case NativeFeedType.Advertise:
                    (apiStatus, respond) = await RequestsAsync.Advertise.GetAdvertisePost(AppSettings.PostApiLimitOnScroll, offset);
                    break;
                default:
                    return;
            }

            if (apiStatus != 200 || respond is not PostObject result || result.Data == null)
            {
                WRecyclerView.MainScrollEvent.IsLoading = false;
                Methods.DisplayReportResult(ActivityContext, respond);
            }
            else
            {
                if (typeRun == "FirstInsert")
                {
                    InsertTopDataApi(apiStatus, respond);
                }
                else
                {
                    LoadDataApi(apiStatus, respond, offset, typeRun);
                }
            }
        }

        public async Task FetchSearchForPosts(string offset, string id, string searchQuery, string type)
        {
            if (!Methods.CheckConnectivity())
                return;

            var (apiStatus, respond) = await RequestsAsync.Posts.SearchForPosts(AppSettings.PostApiLimitOnScroll, offset, id, searchQuery, type);
            if (apiStatus != 200 || respond is not PostObject result || result.Data == null)
            {
                WRecyclerView.MainScrollEvent.IsLoading = false;
                Methods.DisplayReportResult(ActivityContext, respond);
            }
            else LoadDataApi(apiStatus, respond, offset);
        }

        public async Task FetchMostLikedPosts(string offset, string lastTotal, string dt)
        {
            if (!Methods.CheckConnectivity())
                return;

            var (apiStatus, respond) = await RequestsAsync.Posts.MostLikedAsync(AppSettings.PostApiLimitOnScroll, offset, lastTotal, dt);
            if (apiStatus != 200 || respond is not PostObject result || result.Data == null)
            {
                WRecyclerView.MainScrollEvent.IsLoading = false;
                Methods.DisplayReportResult(ActivityContext, respond);
            }
            else LoadDataApi(apiStatus, respond, offset);
        }

        public void LoadDataApi(int apiStatus, dynamic respond, string offset, string typeRun = "Add")
        {
            try
            {
                if (respond is PostObject result)
                {
                    if (WRecyclerView.SwipeRefreshLayoutView is { Refreshing: true })
                        WRecyclerView.SwipeRefreshLayoutView.Refreshing = false;

                    var countList = NativeFeedAdapter.ItemCount;
                    if (result.Data.Count > 0)
                    {
                        result.Data.RemoveAll(a => a.Publisher == null && a.UserData == null);
                        GetAllPostLive(result.Data);

                        if (offset == "0" && countList > 10 && typeRun == "Insert" && NativeFeedAdapter.NativePostType == NativeFeedType.Global)
                        {
                            result.Data.Reverse();
                            bool add = false;

                            foreach (var post in from post in result.Data let check = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a.PostData?.PostId == post.PostId && a.TypeView == PostFunctions.GetAdapterType(post)) where check == null select post)
                            {
                                add = true;
                                ListUtils.NewPostList.Add(post);
                            }

                            ActivityContext?.RunOnUiThread(() =>
                            {
                                try
                                {
                                    if (add && WRecyclerView.PopupBubbleView != null && WRecyclerView.PopupBubbleView.Visibility != ViewStates.Visible && AppSettings.ShowNewPostOnNewsFeed)
                                        WRecyclerView.PopupBubbleView.Visibility = ViewStates.Visible;
                                    else
                                        WRecyclerView.PopupBubbleView.Visibility = WRecyclerView.PopupBubbleView.Visibility;
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });
                        }
                        else
                        {
                            bool add = false;

                            Trace.BeginSection("LoadDataApi Start");

                            foreach (var post in from post in result.Data let check = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a?.PostData?.PostId == post.PostId && a?.TypeView == PostFunctions.GetAdapterType(post)) where check == null select post)
                            {
                                add = true;
                                var combiner = new FeedCombiner(null, NativeFeedAdapter.ListDiffer, ActivityContext, NativeFeedAdapter.NativePostType);

                                if (NativeFeedAdapter.NativePostType == NativeFeedType.Global)
                                {
                                    if (result.Data.Count < 6 && NativeFeedAdapter.ListDiffer.Count < 6)
                                        if (!ShowFindMoreAlert)
                                        {
                                            ShowFindMoreAlert = true;

                                            combiner.AddFindMoreAlertPostView("Pages");
                                            combiner.AddFindMoreAlertPostView("Groups");
                                        }

                                    var check1 = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.SuggestedGroupsBox);
                                    if (check1 == null && AppSettings.ShowSuggestedGroup && NativeFeedAdapter.ListDiffer.Count > 0 && NativeFeedAdapter.ListDiffer.Count % AppSettings.ShowSuggestedGroupCount == 0 && ListUtils.SuggestedGroupList.Count > 0)
                                        combiner.AddSuggestedBoxPostView(PostModelType.SuggestedGroupsBox);

                                    var check2 = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.SuggestedUsersBox);
                                    if (check2 == null && AppSettings.ShowSuggestedUser && NativeFeedAdapter.ListDiffer.Count > 0 && NativeFeedAdapter.ListDiffer.Count % AppSettings.ShowSuggestedUserCount == 0 && ListUtils.SuggestedUserList.Count > 0)
                                        combiner.AddSuggestedBoxPostView(PostModelType.SuggestedUsersBox);

                                    var check3 = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.SuggestedPagesBox);
                                    if (check3 == null && AppSettings.ShowSuggestedPage && NativeFeedAdapter.ListDiffer.Count > 0 && NativeFeedAdapter.ListDiffer.Count % AppSettings.ShowSuggestedPageCount == 0 && ListUtils.SuggestedPageList.Count > 0)
                                        combiner.AddSuggestedBoxPostView(PostModelType.SuggestedPagesBox);
                                }
                                else if (NativeFeedAdapter.NativePostType == NativeFeedType.Advertise)
                                {
                                    post.PostType = "ad";
                                }

                                if (NativeFeedAdapter.ListDiffer.Count % AppSettings.ShowAdNativeCount == 0 && NativeFeedAdapter.ListDiffer.Count > 0 && AppSettings.ShowAdMobNativePost)
                                    if (LastAdsType == PostModelType.AdMob1)
                                    {
                                        LastAdsType = PostModelType.AdMob2;
                                        combiner.AddAdsPostView(PostModelType.AdMob1);
                                    }
                                    else if (LastAdsType == PostModelType.AdMob2)
                                    {
                                        LastAdsType = PostModelType.AdMob3;
                                        combiner.AddAdsPostView(PostModelType.AdMob2);
                                    }
                                    else if (LastAdsType == PostModelType.AdMob3)
                                    {
                                        LastAdsType = PostModelType.AdMob1;
                                        combiner.AddAdsPostView(PostModelType.AdMob3);
                                    }

                                var combine = new FeedCombiner(RegexFilterText(post), NativeFeedAdapter.ListDiffer, ActivityContext, NativeFeedAdapter.NativePostType);
                                if (post.PostType == "ad" && AppSettings.ShowAdvertise)
                                {
                                    combine.AddAdsPost();
                                }
                                else
                                {
                                    bool isPromoted = post.IsPostBoosted == "1" || post.SharedInfo.SharedInfoClass != null && post.SharedInfo.SharedInfoClass?.IsPostBoosted == "1";
                                    if (isPromoted)
                                    {
                                        if (NativeFeedAdapter.ListDiffer.Count == 0)
                                            combine.CombineDefaultPostSections();
                                        else
                                        {
                                            var p = NativeFeedAdapter.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.PromotePost);
                                            if (p != null)
                                                combine.CombineDefaultPostSections();
                                            else
                                                combine.CombineDefaultPostSections("Top");
                                        }
                                    }
                                    else
                                    {
                                        combine.CombineDefaultPostSections();
                                    }
                                }
                                Trace.BeginSection("LoadDataApi ForEach");
                                if (NativeFeedAdapter.ListDiffer.Count % AppSettings.ShowAdNativeCount == 0 && NativeFeedAdapter.ListDiffer.Count > 0 && AppSettings.ShowFbNativeAds)
                                    combiner.AddAdsPostView(PostModelType.FbAdNative);
                            }

                            Trace.BeginSection("LoadDataApi End");

                            if (add)
                            {
                                ActivityContext?.RunOnUiThread(() =>
                                {
                                    try
                                    {
                                        if (countList == 0)
                                        {
                                            NativeFeedAdapter.NotifyDataSetChanged();
                                        }
                                        else
                                        {

                                            //NativeFeedAdapter.NotifyItemRangeInserted(countList, NativeFeedAdapter.ListDiffer.Count - countList  );
                                            Trace.BeginSection("NotifyItemRangeInserted Before Simulation");
                                            WRecyclerView.SetItemAnimator(null);
                                            NativeFeedAdapter.NotifyItemRangeInserted(countList + 1, NativeFeedAdapter.ListDiffer.Count - countList);
                                            Trace.BeginSection("NotifyItemRangeInserted After Simulation");
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Methods.DisplayReportResultTrack(e);
                                    }
                                });

                                //WRecyclerView.Post(new Runnable(() =>
                                //{
                                //    if (countList == 0)
                                //    {
                                //        NativeFeedAdapter.NotifyDataSetChanged();
                                //    }
                                //    else
                                //    {

                                //        //NativeFeedAdapter.NotifyItemRangeInserted(countList, NativeFeedAdapter.ListDiffer.Count - countList  );
                                //        Trace.BeginSection("NotifyItemRangeInserted Before Simulation");
                                //        WRecyclerView.SetItemAnimator(null);
                                //        NativeFeedAdapter.NotifyItemRangeInserted(countList + 1, NativeFeedAdapter.ListDiffer.Count - countList);
                                //        Trace.BeginSection("NotifyItemRangeInserted After Simulation");
                                //    }
                                //}));
                            }
                            //else
                            //{
                            //    ToastUtils.ShowToast(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_NoMorePost), ToastLength.Short); 
                            //}
                        }
                    }

                    ActivityContext?.RunOnUiThread(() =>
                    {
                        try
                        {
                            WRecyclerView.Visibility = ViewStates.Visible;
                            WRecyclerView?.ShimmerInflater?.Hide();

                            //if (NativeFeedAdapter.NativePostType == NativeFeedType.Global)
                            //    WRecyclerView.DataPostJson = JsonConvert.SerializeObject(result);

                            ShowEmptyPage();
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    });
                }

                WRecyclerView.MainScrollEvent.IsLoading = false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                WRecyclerView.MainScrollEvent.IsLoading = false;
            }
        }

        public void LoadTopDataApi(List<PostDataObject> list)
        {
            try
            {
                NativeFeedAdapter.ListDiffer.Clear();
                NativeFeedAdapter.NotifyDataSetChanged();

                var combiner = new FeedCombiner(null, NativeFeedAdapter.ListDiffer, ActivityContext, NativeFeedAdapter.NativePostType);
                combiner.AddPostBoxPostView("feed", -1);

                switch (AppSettings.ShowStory)
                {
                    case true:
                        combiner.AddStoryPostView(new List<StoryDataObject>());
                        break;
                }

                switch (list.Count)
                {
                    case > 0:
                        {
                            bool add = false;
                            foreach (var post in from post in list let check = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a?.PostData?.PostId == post.PostId && a?.TypeView == PostFunctions.GetAdapterType(post)) where check == null select post)
                            {
                                add = true;
                                var combine = new FeedCombiner(RegexFilterText(post), NativeFeedAdapter.ListDiffer, ActivityContext, NativeFeedAdapter.NativePostType);
                                switch (post.PostType)
                                {
                                    case "ad" when AppSettings.ShowAdvertise:
                                        combine.AddAdsPost();
                                        break;
                                    default:
                                        combine.CombineDefaultPostSections();
                                        break;
                                }
                            }

                            switch (PostCacheList?.Count)
                            {
                                case > 0:
                                    LoadBottomDataApi(PostCacheList.Take(30).ToList());
                                    break;
                            }

                            switch (add)
                            {
                                case true:
                                    ActivityContext?.RunOnUiThread(() =>
                                    {
                                        try
                                        {
                                            NativeFeedAdapter.NotifyDataSetChanged();
                                            ListUtils.NewPostList.Clear();
                                        }
                                        catch (Exception e)
                                        {
                                            Methods.DisplayReportResultTrack(e);
                                        }
                                    });
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

        public void InsertTopDataApi(int apiStatus, dynamic respond)
        {
            try
            {
                if (apiStatus != 200 || respond is not PostObject result || result.Data == null)
                {
                    WRecyclerView.MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(ActivityContext, respond);
                }
                else
                {
                    result.Data.RemoveAll(a => a.Publisher == null && a.UserData == null);
                    GetAllPostLive(result.Data);
                    result.Data.Reverse();

                    switch (result.Data.Count)
                    {
                        case > 0:
                            {
                                bool add = false;
                                foreach (var post in from post in result.Data let check = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a?.PostData?.PostId == post.PostId && a?.TypeView == PostFunctions.GetAdapterType(post)) where check == null select post)
                                {
                                    add = true;
                                    var combine = new FeedCombiner(RegexFilterText(post), NativeFeedAdapter.ListDiffer, ActivityContext, NativeFeedAdapter.NativePostType);
                                    switch (post.PostType)
                                    {
                                        case "ad" when AppSettings.ShowAdvertise:
                                            combine.AddAdsPost("Top");
                                            break;
                                        default:
                                            combine.CombineDefaultPostSections("Top");
                                            break;
                                    }
                                }

                                switch (add)
                                {
                                    case true:
                                        ActivityContext?.RunOnUiThread(() =>
                                        {
                                            try
                                            {
                                                NativeFeedAdapter.NotifyDataSetChanged();
                                                ListUtils.NewPostList.Clear();
                                            }
                                            catch (Exception e)
                                            {
                                                Methods.DisplayReportResultTrack(e);
                                            }
                                        });
                                        break;
                                }

                                break;
                            }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void LoadMemoriesDataApi(int apiStatus, dynamic respond, List<AdapterModelsClass> diffList)
        {
            try
            {
                switch (WRecyclerView.MainScrollEvent.IsLoading)
                {
                    case true:
                        return;
                }

                WRecyclerView.MainScrollEvent.IsLoading = true;

                if (apiStatus != 200 || respond is not FetchMemoriesObject result || result.Data == null)
                {
                    WRecyclerView.MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(ActivityContext, respond);
                }
                else
                {
                    if (WRecyclerView.SwipeRefreshLayoutView != null && WRecyclerView.SwipeRefreshLayoutView.Refreshing)
                        WRecyclerView.SwipeRefreshLayoutView.Refreshing = false;

                    var countList = NativeFeedAdapter.ItemCount;
                    switch (result.Data.Posts.Count)
                    {
                        case > 0:
                            {
                                result.Data.Posts.RemoveAll(a => a.Publisher == null && a.UserData == null);
                                result.Data.Posts.Reverse();

                                foreach (var post in from post in result.Data.Posts let check = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a?.PostData?.PostId == post.PostId && a?.TypeView == PostFunctions.GetAdapterType(post)) where check == null select post)
                                {
                                    switch (post.Publisher)
                                    {
                                        case null when post.UserData == null:
                                            continue;
                                        default:
                                            {
                                                var combine = new FeedCombiner(RegexFilterText(post), NativeFeedAdapter.ListDiffer, ActivityContext, NativeFeedAdapter.NativePostType);
                                                combine.CombineDefaultPostSections();
                                                break;
                                            }
                                    }
                                }

                                ActivityContext?.RunOnUiThread(() =>
                                {
                                    try
                                    {
                                        WRecyclerView.Visibility = ViewStates.Visible;
                                        WRecyclerView?.ShimmerInflater?.Hide();

                                        var d = new Runnable(() => { NativeFeedAdapter.NotifyItemRangeInserted(countList, NativeFeedAdapter.ListDiffer.Count - countList); }); d.Run();
                                        GC.Collect();
                                    }
                                    catch (Exception e)
                                    {
                                        Methods.DisplayReportResultTrack(e);
                                    }
                                });
                                break;
                            }
                    }
                }

                WRecyclerView.MainScrollEvent.IsLoading = false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                WRecyclerView.MainScrollEvent.IsLoading = false;
            }
        }

        public async Task FetchLoadMoreNewsFeedApiPosts()
        {
            try
            {
                if (!Methods.CheckConnectivity())
                    return;

                if (NativeFeedAdapter.NativePostType != NativeFeedType.Global)
                    return;

                switch (PostCacheList?.Count)
                {
                    case > 40:
                        return;
                }

                var diff = NativeFeedAdapter.ListDiffer;
                var list = new List<AdapterModelsClass>(diff);
                switch (list.Count)
                {
                    case <= 20:
                        return;
                }

                var item = list.LastOrDefault();

                var lastItem = list.IndexOf(item);

                item = list[lastItem];

                string offset;

                switch (item.TypeView)
                {
                    case PostModelType.Divider:
                    case PostModelType.ViewProgress:
                    case PostModelType.AdMob1:
                    case PostModelType.AdMob2:
                    case PostModelType.AdMob3:
                    case PostModelType.FbAdNative:
                    case PostModelType.AdsPost:
                    case PostModelType.SuggestedPagesBox:
                    case PostModelType.SuggestedGroupsBox:
                    case PostModelType.SuggestedUsersBox:
                    case PostModelType.CommentSection:
                    case PostModelType.AddCommentSection:
                        item = list.LastOrDefault(a => a.TypeView != PostModelType.Divider && a.TypeView != PostModelType.ViewProgress && a.TypeView != PostModelType.AdMob1 && a.TypeView != PostModelType.AdMob2 && a.TypeView != PostModelType.AdMob3 && a.TypeView != PostModelType.FbAdNative && a.TypeView != PostModelType.AdsPost && a.TypeView != PostModelType.SuggestedPagesBox && a.TypeView != PostModelType.SuggestedGroupsBox && a.TypeView != PostModelType.SuggestedUsersBox && a.TypeView != PostModelType.CommentSection && a.TypeView != PostModelType.AddCommentSection);
                        offset = item?.PostData?.PostId ?? "0";
                        Console.WriteLine(offset);
                        break;
                    default:
                        offset = item.PostData?.PostId ?? "0";
                        break;
                }

                Console.WriteLine(offset);

                int apiStatus;
                dynamic respond;

                switch (NativeFeedAdapter.NativePostType)
                {
                    case NativeFeedType.Global:
                        var adId = NativeFeedAdapter.ListDiffer.LastOrDefault(a => a.TypeView == PostModelType.AdsPost && a.PostData.PostType == "ad")?.PostData?.Id ?? "";
                        (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, offset, "get_news_feed", NativeFeedAdapter.IdParameter, "", WRecyclerView.GetFilter(), adId, WRecyclerView.GetPostType());
                        break;
                    default:
                        return;
                }

                if (apiStatus != 200 || respond is not PostObject result || result.Data == null)
                {
                    Methods.DisplayReportResult(ActivityContext, respond);
                }
                else
                {
                    PostCacheList ??= new List<PostDataObject>();

                    var countList = PostCacheList?.Count ?? 0;
                    switch (result.Data?.Count)
                    {
                        case > 0:
                            {
                                result.Data.RemoveAll(a => a.Publisher == null && a.UserData == null);

                                switch (countList)
                                {
                                    case > 0:
                                        {
                                            foreach (var post in from post in result.Data let check = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a?.PostData?.PostId == post.PostId && a?.TypeView == PostFunctions.GetAdapterType(post)) where check == null select post)
                                            {
                                                PostCacheList.Add(post);
                                            }

                                            break;
                                        }
                                    default:
                                        PostCacheList = new List<PostDataObject>(result.Data);
                                        break;
                                }

                                break;
                            }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private bool LoadBottomDataApi(List<PostDataObject> list)
        {
            try
            {
                var countList = NativeFeedAdapter.ItemCount;
                switch (list?.Count)
                {
                    case > 0:
                        {
                            bool add = false;
                            foreach (var post in from post in list let check = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a?.PostData?.PostId == post.PostId && a?.TypeView == PostFunctions.GetAdapterType(post)) where check == null select post)
                            {
                                add = true;
                                var combiner = new FeedCombiner(null, NativeFeedAdapter.ListDiffer, ActivityContext, NativeFeedAdapter.NativePostType);

                                switch (NativeFeedAdapter.NativePostType)
                                {
                                    case NativeFeedType.Global:
                                        {
                                            var check1 = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.SuggestedGroupsBox);
                                            switch (check1)
                                            {
                                                case null when AppSettings.ShowSuggestedGroup && NativeFeedAdapter.ListDiffer.Count > 0 && NativeFeedAdapter.ListDiffer.Count % AppSettings.ShowSuggestedGroupCount == 0 && ListUtils.SuggestedGroupList.Count > 0:
                                                    combiner.AddSuggestedBoxPostView(PostModelType.SuggestedGroupsBox);
                                                    break;
                                            }

                                            var check2 = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.SuggestedUsersBox);
                                            switch (check2)
                                            {
                                                case null when AppSettings.ShowSuggestedUser && NativeFeedAdapter.ListDiffer.Count > 0 && NativeFeedAdapter.ListDiffer.Count % AppSettings.ShowSuggestedUserCount == 0 && ListUtils.SuggestedUserList.Count > 0:
                                                    combiner.AddSuggestedBoxPostView(PostModelType.SuggestedUsersBox);
                                                    break;
                                            }

                                            var check3 = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.SuggestedPagesBox);
                                            switch (check3)
                                            {
                                                case null when AppSettings.ShowSuggestedPage && NativeFeedAdapter.ListDiffer.Count > 0 && NativeFeedAdapter.ListDiffer.Count % AppSettings.ShowSuggestedPageCount == 0 && ListUtils.SuggestedPageList.Count > 0:
                                                    combiner.AddSuggestedBoxPostView(PostModelType.SuggestedPagesBox);
                                                    break;
                                            }

                                            break;
                                        }
                                }

                                switch (NativeFeedAdapter.ListDiffer.Count % (AppSettings.ShowAdNativeCount * 10))
                                {
                                    case 0 when NativeFeedAdapter.ListDiffer.Count > 0 && AppSettings.ShowAdMobNativePost:
                                        switch (LastAdsType)
                                        {
                                            case PostModelType.AdMob1:
                                                LastAdsType = PostModelType.AdMob2;
                                                combiner.AddAdsPostView(PostModelType.AdMob1);
                                                break;
                                            case PostModelType.AdMob2:
                                                LastAdsType = PostModelType.AdMob3;
                                                combiner.AddAdsPostView(PostModelType.AdMob2);
                                                break;
                                            case PostModelType.AdMob3:
                                                LastAdsType = PostModelType.AdMob1;
                                                combiner.AddAdsPostView(PostModelType.AdMob3);
                                                break;
                                        }

                                        break;
                                }

                                var combine = new FeedCombiner(RegexFilterText(post), NativeFeedAdapter.ListDiffer, ActivityContext, NativeFeedAdapter.NativePostType);
                                switch (post.PostType)
                                {
                                    case "ad" when AppSettings.ShowAdvertise:
                                        combine.AddAdsPost();
                                        break;
                                    default:
                                        {
                                            bool isPromoted = post.IsPostBoosted == "1" || post.SharedInfo.SharedInfoClass != null && post.SharedInfo.SharedInfoClass?.IsPostBoosted == "1";
                                            if (isPromoted)
                                            {
                                                if (NativeFeedAdapter.ListDiffer.Count == 0)
                                                    combine.CombineDefaultPostSections();
                                                else
                                                {
                                                    var p = NativeFeedAdapter.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.PromotePost);
                                                    if (p != null)
                                                        combine.CombineDefaultPostSections();
                                                    else
                                                        combine.CombineDefaultPostSections("Top");
                                                }
                                            }
                                            else
                                            {
                                                combine.CombineDefaultPostSections();
                                            }

                                            break;
                                        }
                                }

                                switch (NativeFeedAdapter.ListDiffer.Count % AppSettings.ShowAdNativeCount)
                                {
                                    case 0 when NativeFeedAdapter.ListDiffer.Count > 0 && AppSettings.ShowFbNativeAds:
                                        combiner.AddAdsPostView(PostModelType.FbAdNative);
                                        break;
                                }
                            }

                            switch (add)
                            {
                                case true:
                                    ActivityContext?.RunOnUiThread(() =>
                                    {
                                        try
                                        {
                                            var d = new Runnable(() => { NativeFeedAdapter.NotifyItemRangeInserted(countList, NativeFeedAdapter.ListDiffer.Count - countList); }); d.Run();
                                            GC.Collect();
                                        }
                                        catch (Exception e)
                                        {
                                            Methods.DisplayReportResultTrack(e);
                                        }
                                    });
                                    break;
                            }

                            PostCacheList.RemoveRange(0, list.Count - 1);
                            ActivityContext?.RunOnUiThread(ShowEmptyPage);

                            return add;
                        }
                    default:
                        return false;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;
            }
        }

        private void ShowEmptyPage()
        {
            try
            {
                NativeFeedAdapter.SetLoaded();
                var viewProgress = NativeFeedAdapter.ListDiffer.FirstOrDefault(anjo => anjo.TypeView == PostModelType.ViewProgress);
                if (viewProgress != null)
                    WRecyclerView.RemoveByRowIndex(viewProgress);

                var emptyStateCheck = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a.PostData != null && a.TypeView != PostModelType.AddPostBox /*&& a.TypeView != PostModelType.SearchForPosts*/);
                if (emptyStateCheck != null)
                {
                    var emptyStateChecker = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.EmptyState);
                    if (emptyStateChecker != null && NativeFeedAdapter.ListDiffer.Count > 1)
                        WRecyclerView.RemoveByRowIndex(emptyStateChecker);
                }
                else
                {
                    var emptyStateChecker = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.EmptyState);
                    if (emptyStateChecker == null)
                    {
                        var data = new AdapterModelsClass
                        {
                            TypeView = PostModelType.EmptyState,
                            Id = 744747447,
                        };
                        NativeFeedAdapter.ListDiffer.Add(data);
                        NativeFeedAdapter.NotifyDataSetChanged();
                    }
                }

                WRecyclerView.MainScrollEvent.IsLoading = false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private void GetAllPostLive(List<PostDataObject> list)
        {
            try
            {
                var listLivePost = list?.Where(a => a.LiveTime != null && a.LiveTime.Value > 0 && a.IsStillLive != null && a.IsStillLive.Value && string.IsNullOrEmpty(a.AgoraResourceId) && string.IsNullOrEmpty(a.PostFile))?.ToList();
                switch (NativeFeedAdapter.NativePostType)
                {
                    case NativeFeedType.Global:
                        var mainActivity = TabbedMainActivity.GetInstance();
                        var checkSection = mainActivity?.NewsFeedTab?.PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.Story);
                        if (checkSection != null)
                        {
                            if (listLivePost?.Count > 0)
                            {
                                foreach (var post in from post in listLivePost let check = checkSection.StoryList.FirstOrDefault(a => a?.DataLivePost?.PostId == post.PostId) where check == null select post)
                                {
                                    if (checkSection.StoryList.Count > 1)
                                    {
                                        checkSection.StoryList.Insert(1, new StoryDataObject
                                        {
                                            Avatar = post.Publisher.Avatar,
                                            Type = "Live",
                                            Username = ActivityContext.GetText(Resource.String.Lbl_Live),
                                            DataLivePost = post
                                        });
                                    }
                                    else
                                    {
                                        checkSection.StoryList.Add(new StoryDataObject
                                        {
                                            Avatar = post.Publisher.Avatar,
                                            Type = "Live",
                                            Username = WoWonderTools.GetNameFinal(post.Publisher),
                                            DataLivePost = post,
                                        });
                                    }
                                }

                                ActivityContext?.RunOnUiThread(() =>
                                {
                                    try
                                    {
                                        var d = new Runnable(() => { mainActivity?.NewsFeedTab?.PostFeedAdapter?.NotifyItemChanged(mainActivity.NewsFeedTab.PostFeedAdapter.ListDiffer.IndexOf(checkSection)); });
                                        d.Run();
                                    }
                                    catch (Exception e)
                                    {
                                        Methods.DisplayReportResultTrack(e);
                                    }
                                });
                            }
                        }
                        //wael
                        //case NativeFeedType.User when NativeFeedAdapter.IdParameter != UserDetails.UserId:
                        //    var userProfileActivity = UserProfileActivity.GetInstance();
                        //    if (userProfileActivity != null)
                        //    {
                        //        var data = userProfileActivity.PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.UserProfileInfoHeaderSection);
                        //        if (listLivePost?.Count > 0)
                        //        {
                        //            UserDetails.DataLivePost = listLivePost.FirstOrDefault();

                        //            if (data != null)
                        //                data.InfoUserModel.IsLive = true; 
                        //        }
                        //        else
                        //        {
                        //            UserDetails.DataLivePost = null;

                        //            if (data != null)
                        //                data.InfoUserModel.IsLive = false;
                        //        }

                        //        ActivityContext?.RunOnUiThread(() =>
                        //        {
                        //            try
                        //            {
                        //                var d = new Runnable(() => { userProfileActivity?.PostFeedAdapter?.NotifyItemChanged(userProfileActivity.PostFeedAdapter.ListDiffer.IndexOf(data)); });
                        //                d.Run();
                        //            }
                        //            catch (Exception e)
                        //            {
                        //                Methods.DisplayReportResultTrack(e);
                        //            }
                        //        });
                        //    }

                        break;
                    default:
                        return;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static PostDataObject RegexFilterText(PostDataObject item)
        {
            try
            {
                Dictionary<string, string> dataUser = new Dictionary<string, string>();

                if (string.IsNullOrEmpty(item.PostText))
                    return item;

                if (item.PostText.Contains("data-id="))
                {
                    try
                    {
                        //string pattern = @"(data-id=[""'](.*?)[""']|href=[""'](.*?)[""']|'>(.*?)a>)";

                        string pattern = @"(data-id=[""'](.*?)[""']|href=[""'](.*?)[""'])";
                        var aa = Regex.Matches(item.PostText, pattern);
                        switch (aa?.Count)
                        {
                            case > 0:
                                {
                                    for (int i = 0; i < aa.Count; i++)
                                    {
                                        string userid = "";
                                        if (aa.Count > i)
                                            userid = aa[i]?.Value?.Replace("data-id=", "").Replace('"', ' ').Replace(" ", "");

                                        string username = "";
                                        if (aa.Count > i + 1)
                                            username = aa[i + 1]?.Value?.Replace("href=", "").Replace('"', ' ').Replace(" ", "").Replace(InitializeWoWonder.WebsiteUrl, "").Replace("\n", "");

                                        if (string.IsNullOrEmpty(userid) || string.IsNullOrEmpty(username))
                                            continue;

                                        var data = dataUser.FirstOrDefault(a => a.Key?.ToString() == userid && a.Value?.ToString() == username);
                                        if (data.Key != null)
                                            continue;

                                        i++;

                                        switch (string.IsNullOrWhiteSpace(userid))
                                        {
                                            case false when !string.IsNullOrWhiteSpace(username) && !dataUser.ContainsKey(userid):
                                                dataUser.Add(userid, username);
                                                break;
                                        }
                                    }

                                    item.RegexFilterList = new Dictionary<string, string>(dataUser);
                                    return item;
                                }
                            default:
                                return item;
                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                }

                return item;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return item;
            }
        }

        public static async Task GetAllPostVideo()
        {
            try
            {
                if (!Methods.CheckConnectivity())
                    return;

                var (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost("10", "0", "get_news_feed", "", "", "0", "0", "video");
                if (apiStatus != 200 || respond is not PostObject result || result.Data == null)
                {
                    // Methods.DisplayReportResult(ActivityContext, respond);
                }
                else
                {
                    var respondList = result.Data?.Count;
                    if (respondList > 0)
                    {
                        foreach (var item in from item in result.Data let check = ListUtils.VideoReelsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                        {
                            var checkViewed = ListUtils.VideoReelsViewsList.FirstOrDefault(a => a.Id == item.Id);
                            if (checkViewed == null)
                            {
                                if (!AppSettings.ShowYouTubeReels && !string.IsNullOrEmpty(item.PostYoutube))
                                    continue;

                                if (!string.IsNullOrEmpty(item.PostFacebook) || !string.IsNullOrEmpty(item.PostVimeo) || !string.IsNullOrEmpty(item.PostDeepsound) || !string.IsNullOrEmpty(item.PostPlaytube))
                                    continue;

                                ListUtils.VideoReelsList.Add(new ReelsVideoClass
                                {
                                    Id = item.Id,
                                    Type = ItemType.ReelsVideo,
                                    VideoData = item
                                });

                                if (AdsGoogle.NativeAdsPool?.Count > 0 && ListUtils.VideoReelsList.Count % AppSettings.ShowAdNativeReelsCount == 0)
                                {
                                    ListUtils.VideoReelsList.Add(new ReelsVideoClass
                                    {
                                        Type = ItemType.AdMob,
                                    });
                                }
                            }
                        }

                        var list = ListUtils.VideoReelsList.Where(a => a.Type == ItemType.ReelsVideo).Take(7);
                        foreach (var videoObject in list)
                        {
                            new PreCachingExoPlayerVideo(Application.Context).CacheVideosFiles(Uri.Parse(videoObject.VideoData?.PostFileFull));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}