using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Content.Res;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Content;
using AndroidX.Core.Graphics.Drawable;
using Google.Android.Material.Dialog;
using Java.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using WoWonder.Activities.Articles;
using WoWonder.Activities.Comment;
using WoWonder.Activities.Communities.Groups;
using WoWonder.Activities.Communities.Pages;
using WoWonder.Activities.EditPost;
using WoWonder.Activities.Events;
using WoWonder.Activities.Fundings;
using WoWonder.Activities.Jobs;
using WoWonder.Activities.Market;
using WoWonder.Activities.MyProfile;
using WoWonder.Activities.NativePost.Extra;
using WoWonder.Activities.NativePost.Pages;
using WoWonder.Activities.NativePost.Share;
using WoWonder.Activities.Offers;
using WoWonder.Activities.PostData;
using WoWonder.Activities.Tabbes;
using WoWonder.Activities.Upgrade;
using WoWonder.Activities.UsersPages;
using WoWonder.Activities.Videos;
using WoWonder.Activities.Wallet;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Requests;
using Exception = System.Exception;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.NativePost.Post
{
    public class PostClickListener
    {
        private readonly AppCompatActivity MainContext;
        private readonly NativeFeedType NativeFeedType;

        public static bool OpenMyProfile;

        private MoreBottomDialogFragment MorePost;

        public PostClickListener(AppCompatActivity context, NativeFeedType nativeFeedType)
        {
            try
            {
                MainContext = context;
                NativeFeedType = nativeFeedType;
                OpenMyProfile = false;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void ProfilePostClick(ProfileClickEventArgs e, string type, string typeEvent)
        {
            try
            {
                var username = e.View.FindViewById<TextView>(Resource.Id.username);
                if (username != null && username.Text.Contains(MainContext.GetText(Resource.String.Lbl_SharedPost)) && typeEvent == "Username")
                {
                    var intent = new Intent(MainContext, typeof(ViewFullPostActivity));
                    intent.PutExtra("Id", e.NewsFeedClass.ParentId);
                    intent.PutExtra("DataItem", JsonConvert.SerializeObject(e.NewsFeedClass));
                    MainContext.StartActivity(intent);
                }
                else if (username != null && username.Text == MainContext.GetText(Resource.String.Lbl_Anonymous))
                {
                    ToastUtils.ShowToast(MainContext, MainContext.GetText(Resource.String.Lbl_SorryUserIsAnonymous), ToastLength.Short);
                }
                else if (e.NewsFeedClass.PageId != null && e.NewsFeedClass.PageId != "0" && NativeFeedType != NativeFeedType.Page)
                {
                    var intent = new Intent(MainContext, typeof(PageProfileActivity));
                    intent.PutExtra("PageObject", JsonConvert.SerializeObject(e.NewsFeedClass.Publisher));
                    intent.PutExtra("PageId", e.NewsFeedClass.PageId);
                    MainContext.StartActivity(intent);
                }
                else if (e.NewsFeedClass.GroupId != null && e.NewsFeedClass.GroupId != "0" && NativeFeedType != NativeFeedType.Group)
                {
                    var intent = new Intent(MainContext, typeof(GroupProfileActivity));
                    intent.PutExtra("GroupObject", JsonConvert.SerializeObject(e.NewsFeedClass.GroupRecipient));
                    intent.PutExtra("GroupId", e.NewsFeedClass.GroupId);
                    MainContext.StartActivity(intent);
                }
                else
                {
                    switch (type)
                    {
                        case "CommentClass":
                            WoWonderTools.OpenProfile(MainContext, e.CommentClass.UserId, e.CommentClass.Publisher);
                            break;
                        default:
                            WoWonderTools.OpenProfile(MainContext, e.NewsFeedClass.UserId, e.NewsFeedClass.Publisher);
                            break;
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void CommentPostClick(GlobalClickEventArgs e, string type = "Normal")
        {
            try
            {
                var intent = new Intent(MainContext, typeof(CommentActivity));
                intent.PutExtra("PostId", e.NewsFeedClass.Id);
                intent.PutExtra("Type", type);
                intent.PutExtra("PostObject", JsonConvert.SerializeObject(e.NewsFeedClass));
                MainContext.StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void SharePostClick(GlobalClickEventArgs e, PostModelType clickType)
        {
            try
            {
                Bundle bundle = new Bundle();

                bundle.PutString("ItemData", JsonConvert.SerializeObject(e.NewsFeedClass));
                bundle.PutString("TypePost", JsonConvert.SerializeObject(clickType));
                var activity = MainContext;
                var searchFilter = new ShareBottomDialogFragment
                {
                    Arguments = bundle
                };
                searchFilter.Show(activity.SupportFragmentManager, "ShareFilter");
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Menu >> Copy Link
        public void CopyLinkEvent(string text)
        {
            try
            {
                Methods.CopyToClipboard(MainContext, text);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> Delete post
        private void DeletePostEvent(PostDataObject item)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var dialog = new MaterialAlertDialogBuilder(MainContext);
                    dialog.SetTitle(MainContext.GetText(Resource.String.Lbl_DeletePost));
                    dialog.SetMessage(MainContext.GetText(Resource.String.Lbl_AreYouSureDeletePost));
                    dialog.SetPositiveButton(MainContext.GetText(Resource.String.Lbl_Yes), (materialDialog, action) =>
                    {
                        try
                        {
                            if (!Methods.CheckConnectivity())
                            {
                                ToastUtils.ShowToast(MainContext, MainContext.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                                return;
                            }
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Posts.PostActionsAsync(item.Id, "delete") });

                            var feedTab = TabbedMainActivity.GetInstance()?.NewsFeedTab;
                            if (item.SharedInfo.SharedInfoClass != null)
                            {
                                var data = feedTab?.PostFeedAdapter?.ListDiffer?.Where(a => a.PostData?.Id == item?.Id || a.PostData?.Id == item?.SharedInfo.SharedInfoClass?.Id).ToList();
                                switch (data?.Count)
                                {
                                    case > 0:
                                        {
                                            foreach (var post in data)
                                            {
                                                feedTab.MainRecyclerView?.RemoveByRowIndex(post);
                                            }

                                            break;
                                        }
                                }
                            }
                            else
                            {
                                var data = feedTab?.PostFeedAdapter?.ListDiffer?.Where(a => a.PostData?.Id == item?.Id).ToList();
                                switch (data?.Count)
                                {
                                    case > 0:
                                        {
                                            foreach (var post in data)
                                            {
                                                feedTab.MainRecyclerView?.RemoveByRowIndex(post);
                                            }

                                            break;
                                        }
                                }
                            }

                            feedTab?.MainRecyclerView?.StopVideo();

                            var profileActivity = MyProfileActivity.GetInstance();
                            if (item.SharedInfo.SharedInfoClass != null)
                            {
                                var data = profileActivity?.PostFeedAdapter?.ListDiffer?.Where(a => a.PostData?.Id == item?.Id || a.PostData?.Id == item?.SharedInfo.SharedInfoClass?.Id).ToList();
                                switch (data?.Count)
                                {
                                    case > 0:
                                        {
                                            foreach (var post in data)
                                            {
                                                profileActivity.MainRecyclerView?.RemoveByRowIndex(post);
                                            }

                                            break;
                                        }
                                }
                            }
                            else
                            {
                                var data = profileActivity?.PostFeedAdapter?.ListDiffer?.Where(a => a.PostData?.Id == item?.Id).ToList();
                                switch (data?.Count)
                                {
                                    case > 0:
                                        {
                                            foreach (var post in data)
                                            {
                                                profileActivity.MainRecyclerView?.RemoveByRowIndex(post);
                                            }

                                            break;
                                        }
                                }
                            }

                            var recyclerView = WRecyclerView.GetInstance();
                            if (item.SharedInfo.SharedInfoClass != null)
                            {
                                var data = recyclerView?.NativeFeedAdapter?.ListDiffer?.Where(a => a.PostData?.Id == item?.Id || a.PostData?.Id == item?.SharedInfo.SharedInfoClass?.Id).ToList();
                                switch (data?.Count)
                                {
                                    case > 0:
                                        {
                                            foreach (var post in data)
                                            {
                                                recyclerView?.RemoveByRowIndex(post);
                                            }

                                            break;
                                        }
                                }
                            }
                            else
                            {
                                var data = recyclerView?.NativeFeedAdapter?.ListDiffer?.Where(a => a.PostData?.Id == item?.Id).ToList();
                                switch (data?.Count)
                                {
                                    case > 0:
                                        {
                                            foreach (var post in data)
                                            {
                                                recyclerView?.RemoveByRowIndex(post);
                                            }

                                            break;
                                        }
                                }
                            }

                            recyclerView?.StopVideo();

                            ToastUtils.ShowToast(MainContext, MainContext.GetText(Resource.String.Lbl_postSuccessfullyDeleted), ToastLength.Short);
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    });
                    dialog.SetNegativeButton(MainContext.GetText(Resource.String.Lbl_No), new MaterialDialogUtils());

                    dialog.Show();
                }
                else
                {
                    ToastUtils.ShowToast(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //ReportPost
        private void ReportPostEvent(PostDataObject item)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    item.IsPostReported = true;
                    ToastUtils.ShowToast(MainContext, MainContext.GetText(Resource.String.Lbl_YourReportPost), ToastLength.Short);
                    //Sent Api >>
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Posts.PostActionsAsync(item.Id, "report") });
                }
                else
                {
                    ToastUtils.ShowToast(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //HidePost 
        private void HidePostEvent(PostDataObject item)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    if (!Methods.CheckConnectivity())
                    {
                        ToastUtils.ShowToast(MainContext, MainContext.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                        return;
                    }
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Posts.HidePostAsync(item.Id) });

                    var feedTab = TabbedMainActivity.GetInstance()?.NewsFeedTab;
                    if (item.SharedInfo.SharedInfoClass != null)
                    {
                        var data = feedTab?.PostFeedAdapter?.ListDiffer?.Where(a => a.PostData?.Id == item?.Id || a.PostData?.Id == item?.SharedInfo.SharedInfoClass?.Id).ToList();
                        switch (data?.Count)
                        {
                            case > 0:
                                {
                                    foreach (var post in data)
                                    {
                                        feedTab.MainRecyclerView?.RemoveByRowIndex(post);
                                    }

                                    break;
                                }
                        }
                    }
                    else
                    {
                        var data = feedTab?.PostFeedAdapter?.ListDiffer?.Where(a => a.PostData?.Id == item?.Id).ToList();
                        switch (data?.Count)
                        {
                            case > 0:
                                {
                                    foreach (var post in data)
                                    {
                                        feedTab.MainRecyclerView?.RemoveByRowIndex(post);
                                    }

                                    break;
                                }
                        }
                    }

                    feedTab?.MainRecyclerView?.StopVideo();

                    var profileActivity = MyProfileActivity.GetInstance();
                    if (item.SharedInfo.SharedInfoClass != null)
                    {
                        var data = profileActivity?.PostFeedAdapter?.ListDiffer?.Where(a => a.PostData?.Id == item?.Id || a.PostData?.Id == item?.SharedInfo.SharedInfoClass?.Id).ToList();
                        switch (data?.Count)
                        {
                            case > 0:
                                {
                                    foreach (var post in data)
                                    {
                                        profileActivity.MainRecyclerView?.RemoveByRowIndex(post);
                                    }

                                    break;
                                }
                        }
                    }
                    else
                    {
                        var data = profileActivity?.PostFeedAdapter?.ListDiffer?.Where(a => a.PostData?.Id == item?.Id).ToList();
                        switch (data?.Count)
                        {
                            case > 0:
                                {
                                    foreach (var post in data)
                                    {
                                        profileActivity.MainRecyclerView?.RemoveByRowIndex(post);
                                    }

                                    break;
                                }
                        }
                    }

                    var recyclerView = WRecyclerView.GetInstance();
                    if (item.SharedInfo.SharedInfoClass != null)
                    {
                        var data = recyclerView?.NativeFeedAdapter?.ListDiffer?.Where(a => a.PostData?.Id == item?.Id || a.PostData?.Id == item?.SharedInfo.SharedInfoClass?.Id).ToList();
                        switch (data?.Count)
                        {
                            case > 0:
                                {
                                    foreach (var post in data)
                                    {
                                        recyclerView?.RemoveByRowIndex(post);
                                    }

                                    break;
                                }
                        }
                    }
                    else
                    {
                        var data = recyclerView?.NativeFeedAdapter?.ListDiffer?.Where(a => a.PostData?.Id == item?.Id).ToList();
                        switch (data?.Count)
                        {
                            case > 0:
                                {
                                    foreach (var post in data)
                                    {
                                        recyclerView?.RemoveByRowIndex(post);
                                    }

                                    break;
                                }
                        }
                    }

                    recyclerView?.StopVideo();

                    ToastUtils.ShowToast(MainContext, MainContext.GetText(Resource.String.Lbl_PostSuccessfullyHided), ToastLength.Short);
                }
                else
                {
                    ToastUtils.ShowToast(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //SavePost 
        private async void SavePostEvent(PostDataObject item)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    //Sent Api >>
                    var (apiStatus, respond) = await RequestsAsync.Posts.PostActionsAsync(item.Id, "save");
                    switch (apiStatus)
                    {
                        case 200:
                            {
                                item.IsPostSaved = respond switch
                                {
                                    PostActionsObject actionsObject => actionsObject.Code.ToString() != "0",
                                    _ => item.IsPostSaved
                                };

                                var adapterGlobal = WRecyclerView.GetInstance()?.NativeFeedAdapter;
                                var dataGlobal = adapterGlobal?.ListDiffer?.Where(a => a.PostData?.Id == item.Id).ToList();
                                switch (dataGlobal?.Count)
                                {
                                    case > 0:
                                        {
                                            foreach (var dataClass in from dataClass in dataGlobal let index = adapterGlobal.ListDiffer.IndexOf(dataClass) where index > -1 select dataClass)
                                            {
                                                dataClass.PostData.IsPostSaved = item.IsPostSaved;

                                                adapterGlobal.NotifyItemChanged(adapterGlobal.ListDiffer.IndexOf(dataClass));
                                            }

                                            break;
                                        }
                                }

                                var adapter = TabbedMainActivity.GetInstance()?.NewsFeedTab?.PostFeedAdapter;
                                var data = adapter?.ListDiffer?.Where(a => a.PostData?.Id == item.Id).ToList();
                                switch (data?.Count)
                                {
                                    case > 0:
                                        {
                                            foreach (var dataClass in from dataClass in data let index = adapter.ListDiffer.IndexOf(dataClass) where index > -1 select dataClass)
                                            {
                                                dataClass.PostData.IsPostSaved = item.IsPostSaved;

                                                adapter.NotifyItemChanged(adapter.ListDiffer.IndexOf(dataClass));
                                            }

                                            break;
                                        }
                                }

                                ToastUtils.ShowToast(MainContext, item.IsPostSaved == true ? MainContext.GetText(Resource.String.Lbl_postSuccessfullySaved) : MainContext.GetText(Resource.String.Lbl_postSuccessfullyUnSaved), ToastLength.Short);

                                break;
                            }
                    }
                }
                else
                {
                    ToastUtils.ShowToast(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //BoostPost 
        private async void BoostPostEvent(PostDataObject item)
        {
            try
            {
                var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                if (dataUser?.IsPro != "1" && ListUtils.SettingsSiteList?.Pro == "1" && AppSettings.ShowGoPro)
                {
                    var intent = new Intent(MainContext, typeof(GoProActivity));
                    MainContext.StartActivity(intent);
                    return;
                }

                if (Convert.ToDouble(dataUser?.Wallet) == 0 && AppSettings.ShowWallet)
                {
                    var intent = new Intent(MainContext, typeof(TabbedWalletActivity));
                    MainContext.StartActivity(intent);
                    return;
                }

                if (Methods.CheckConnectivity())
                {
                    item.Boosted = "1";
                    //Sent Api >>
                    var (apiStatus, respond) = await RequestsAsync.Posts.PostActionsAsync(item.Id, "boost");
                    switch (apiStatus)
                    {
                        case 200:
                            {
                                switch (respond)
                                {
                                    case PostActionsObject actionsObject:
                                        MainContext?.RunOnUiThread(() =>
                                        {
                                            try
                                            {
                                                item.Boosted = actionsObject.Code.ToString();
                                                item.IsPostBoosted = actionsObject.Code.ToString();

                                                var adapterGlobal = WRecyclerView.GetInstance()?.NativeFeedAdapter;
                                                var dataGlobal = adapterGlobal?.ListDiffer?.Where(a => a.PostData?.Id == item.Id).ToList();
                                                switch (dataGlobal?.Count)
                                                {
                                                    case > 0:
                                                        {
                                                            foreach (var dataClass in from dataClass in dataGlobal let index = adapterGlobal.ListDiffer.IndexOf(dataClass) where index > -1 select dataClass)
                                                            {
                                                                dataClass.PostData.Boosted = actionsObject.Code.ToString();
                                                                dataClass.PostData.IsPostBoosted = actionsObject.Code.ToString();
                                                                adapterGlobal.NotifyItemChanged(adapterGlobal.ListDiffer.IndexOf(dataClass), "BoostedPost");
                                                            }

                                                            var checkTextSection = dataGlobal.FirstOrDefault(w => w.TypeView == PostModelType.PromotePost);
                                                            switch (checkTextSection)
                                                            {
                                                                case null when item.Boosted == "1":
                                                                    {
                                                                        var collection = dataGlobal.FirstOrDefault()?.PostData;
                                                                        var adapterModels = new AdapterModelsClass
                                                                        {
                                                                            TypeView = PostModelType.PromotePost,
                                                                            Id = Convert.ToInt32((int)PostModelType.PromotePost + collection?.Id),
                                                                            PostData = collection,
                                                                            IsDefaultFeedPost = true
                                                                        };

                                                                        var headerPostIndex = adapterGlobal.ListDiffer.IndexOf(dataGlobal.FirstOrDefault(w => w.TypeView == PostModelType.HeaderPost));
                                                                        switch (headerPostIndex)
                                                                        {
                                                                            case > -1:
                                                                                adapterGlobal.ListDiffer.Insert(headerPostIndex, adapterModels);
                                                                                adapterGlobal.NotifyItemInserted(headerPostIndex);
                                                                                break;
                                                                        }

                                                                        break;
                                                                    }
                                                                default:
                                                                    WRecyclerView.GetInstance().RemoveByRowIndex(checkTextSection);
                                                                    break;
                                                            }

                                                            break;
                                                        }
                                                }

                                                var adapter = TabbedMainActivity.GetInstance()?.NewsFeedTab?.PostFeedAdapter;
                                                var data = adapter?.ListDiffer?.Where(a => a.PostData?.Id == item.Id).ToList();
                                                switch (data?.Count)
                                                {
                                                    case > 0:
                                                        {
                                                            foreach (var dataClass in from dataClass in data let index = adapter.ListDiffer.IndexOf(dataClass) where index > -1 select dataClass)
                                                            {
                                                                dataClass.PostData.Boosted = actionsObject.Code.ToString();
                                                                dataClass.PostData.IsPostBoosted = actionsObject.Code.ToString();
                                                                adapter.NotifyItemChanged(adapter.ListDiffer.IndexOf(dataClass), "BoostedPost");
                                                            }

                                                            var checkTextSection = data.FirstOrDefault(w => w.TypeView == PostModelType.PromotePost);
                                                            switch (checkTextSection)
                                                            {
                                                                case null when item.Boosted == "1":
                                                                    {
                                                                        var collection = data.FirstOrDefault()?.PostData;
                                                                        var adapterModels = new AdapterModelsClass
                                                                        {
                                                                            TypeView = PostModelType.PromotePost,
                                                                            Id = Convert.ToInt32((int)PostModelType.PromotePost + collection?.Id),
                                                                            PostData = collection,
                                                                            IsDefaultFeedPost = true
                                                                        };

                                                                        var headerPostIndex = adapter.ListDiffer.IndexOf(data.FirstOrDefault(w => w.TypeView == PostModelType.HeaderPost));
                                                                        switch (headerPostIndex)
                                                                        {
                                                                            case > -1:
                                                                                adapter.ListDiffer.Insert(headerPostIndex, adapterModels);
                                                                                adapter.NotifyItemInserted(headerPostIndex);
                                                                                break;
                                                                        }

                                                                        break;
                                                                    }
                                                                default:
                                                                    WRecyclerView.GetInstance().RemoveByRowIndex(checkTextSection);
                                                                    break;
                                                            }

                                                            break;
                                                        }
                                                }

                                                ToastUtils.ShowToast(MainContext, MainContext.GetText(Resource.String.Lbl_postSuccessfullyBoosted), ToastLength.Short);
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
                else
                {
                    ToastUtils.ShowToast(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Status Comments Post 
        private async void StatusCommentsPostEvent(PostDataObject item)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    item.CommentsStatus = "1";
                    //Sent Api >>
                    var (apiStatus, respond) = await RequestsAsync.Posts.PostActionsAsync(item.Id, "disable_comments");
                    switch (apiStatus)
                    {
                        case 200:
                            {
                                switch (respond)
                                {
                                    case PostActionsObject actionsObject:
                                        MainContext?.RunOnUiThread(() =>
                                        {
                                            try
                                            {
                                                item.CommentsStatus = actionsObject.Code.ToString();

                                                var adapterGlobal = WRecyclerView.GetInstance()?.NativeFeedAdapter;
                                                var dataGlobal = adapterGlobal?.ListDiffer?.Where(a => a.PostData?.Id == item.Id).ToList();
                                                switch (dataGlobal?.Count)
                                                {
                                                    case > 0:
                                                        {
                                                            foreach (var dataClass in from dataClass in dataGlobal let index = adapterGlobal.ListDiffer.IndexOf(dataClass) where index > -1 select dataClass)
                                                            {
                                                                dataClass.PostData.CommentsStatus = actionsObject.Code.ToString();

                                                                adapterGlobal.NotifyItemChanged(adapterGlobal.ListDiffer.IndexOf(dataClass));
                                                            }

                                                            break;
                                                        }
                                                }

                                                var adapter = TabbedMainActivity.GetInstance()?.NewsFeedTab?.PostFeedAdapter;
                                                var data = adapter?.ListDiffer?.Where(a => a.PostData?.Id == item.Id).ToList();
                                                switch (data?.Count)
                                                {
                                                    case > 0:
                                                        {
                                                            foreach (var dataClass in from dataClass in data let index = adapter.ListDiffer.IndexOf(dataClass) where index > -1 select dataClass)
                                                            {
                                                                dataClass.PostData.CommentsStatus = actionsObject.Code.ToString();

                                                                adapter.NotifyItemChanged(adapter.ListDiffer.IndexOf(dataClass));
                                                            }

                                                            break;
                                                        }
                                                }

                                                switch (actionsObject.Code)
                                                {
                                                    case 0:
                                                        ToastUtils.ShowToast(MainContext, MainContext.GetText(Resource.String.Lbl_PostCommentsDisabled), ToastLength.Short);
                                                        break;
                                                    default:
                                                        ToastUtils.ShowToast(MainContext, MainContext.GetText(Resource.String.Lbl_PostCommentsEnabled), ToastLength.Short);
                                                        break;
                                                }
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
                else
                {
                    ToastUtils.ShowToast(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void MorePostIconClick(GlobalClickEventArgs item, string type = "Normal")
        {
            try
            {
                if (type == "YoutubePlayer")
                {
                    var moreIcon = item.View.FindViewById<ImageView>(Resource.Id.moreicon);
                    ShowPopup(moreIcon, item.NewsFeedClass);
                }
                else
                {
                    Bundle bundle = new Bundle();
                    bundle.PutString("ItemData", JsonConvert.SerializeObject(item.NewsFeedClass));

                    MorePost = new MoreBottomDialogFragment(this)
                    {
                        Arguments = bundle
                    };
                    var activity = MainContext;
                    MorePost.Show(activity.SupportFragmentManager, "MorePost");
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void ShowPopup(View v, PostDataObject dataPost)
        {
            try
            {
                LayoutInflater layoutInflater = (LayoutInflater)MainContext.GetSystemService(Context.LayoutInflaterService);
                View popupView = layoutInflater?.Inflate(Resource.Layout.post_more_bottom, null);

                int px = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 200, MainContext.Resources.DisplayMetrics);
                var popupWindow = new PopupWindow(popupView, ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);

                var llSavePost = popupView.FindViewById<LinearLayout>(Resource.Id.more_save_post);
                var llCopyText = popupView.FindViewById<LinearLayout>(Resource.Id.more_copy_text);
                var llCopyLink = popupView.FindViewById<LinearLayout>(Resource.Id.more_copy_link);
                var llHidePost = popupView.FindViewById<LinearLayout>(Resource.Id.more_hide_post);
                var llReportPost = popupView.FindViewById<LinearLayout>(Resource.Id.more_report_post);
                var llEditPost = popupView.FindViewById<LinearLayout>(Resource.Id.more_edit_post);
                var llBoostPost = popupView.FindViewById<LinearLayout>(Resource.Id.more_boost_post);
                var llDisableComments = popupView.FindViewById<LinearLayout>(Resource.Id.more_disable_comment);
                var llDeletePost = popupView.FindViewById<LinearLayout>(Resource.Id.more_delete_post);

                var tvSavePost = popupView.FindViewById<TextView>(Resource.Id.tv_save_post);
                var tvCopyText = popupView.FindViewById<TextView>(Resource.Id.tv_copy_text);
                var tvCopyLink = popupView.FindViewById<TextView>(Resource.Id.tv_copy_link);
                var tvHidePost = popupView.FindViewById<TextView>(Resource.Id.tv_hide_post);
                var tvReportPost = popupView.FindViewById<TextView>(Resource.Id.tv_report_post);
                var tvEditPost = popupView.FindViewById<TextView>(Resource.Id.tv_edit_post);
                var tvBoostPost = popupView.FindViewById<TextView>(Resource.Id.tv_boost_post);
                var tvDisableComments = popupView.FindViewById<TextView>(Resource.Id.tv_disable_comment);
                var tvDeletePost = popupView.FindViewById<TextView>(Resource.Id.tv_delete_post);

                var postType = PostFunctions.GetAdapterType(dataPost);

                llCopyText.Visibility = !string.IsNullOrEmpty(dataPost.Orginaltext) ? ViewStates.Visible : ViewStates.Gone;
                llReportPost.Visibility = !Convert.ToBoolean(dataPost.IsPostReported) ? ViewStates.Visible : ViewStates.Gone;
                llHidePost.Visibility = dataPost.Publisher.UserId != UserDetails.UserId ? ViewStates.Visible : ViewStates.Gone;

                tvSavePost.Text = dataPost.IsPostSaved != null && dataPost.IsPostSaved.Value ? MainContext.GetText(Resource.String.Lbl_UnSavePost) : MainContext.GetText(Resource.String.Lbl_SavePost);

                if ((dataPost.UserId != "0" || dataPost.PageId != "0" || dataPost.GroupId != "0") && dataPost.Publisher.UserId == UserDetails.UserId)
                {
                    switch (postType)
                    {
                        case PostModelType.ProductPost:
                            tvEditPost.Text = MainContext.GetString(Resource.String.Lbl_EditProduct);
                            break;
                        case PostModelType.OfferPost:
                            tvEditPost.Text = MainContext.GetString(Resource.String.Lbl_EditOffers);
                            break;
                        default:
                            tvEditPost.Text = MainContext.GetString(Resource.String.Lbl_EditPost);
                            break;
                    }

                    switch (AppSettings.ShowAdvertisingPost)
                    {
                        case true:
                            switch (dataPost?.Boosted)
                            {
                                case "0":
                                    tvBoostPost.Text = MainContext.GetString(Resource.String.Lbl_BoostPost);
                                    break;
                                case "1":
                                    tvBoostPost.Text = MainContext.GetString(Resource.String.Lbl_UnBoostPost);
                                    break;
                            }

                            break;
                    }

                    switch (dataPost?.CommentsStatus)
                    {
                        case "0":
                            tvDisableComments.Text = MainContext.GetString(Resource.String.Lbl_EnableComments);
                            break;
                        case "1":
                            tvDisableComments.Text = MainContext.GetString(Resource.String.Lbl_DisableComments);
                            break;
                    }
                }
                else
                {
                    llEditPost.Visibility = ViewStates.Gone;
                    llBoostPost.Visibility = ViewStates.Gone;
                    llDisableComments.Visibility = ViewStates.Gone;
                    llDeletePost.Visibility = ViewStates.Gone;
                }

                llSavePost.Click += (sender, args) =>
                {
                    OnItemClick(tvSavePost.Text, dataPost);
                    popupWindow?.Dismiss();
                };

                llCopyText.Click += (sender, args) =>
                {
                    OnItemClick(tvCopyText.Text, dataPost);
                    popupWindow?.Dismiss();
                };

                llCopyLink.Click += (sender, args) =>
                {
                    OnItemClick(tvCopyLink.Text, dataPost);
                    popupWindow?.Dismiss();
                };

                llHidePost.Click += (sender, args) =>
                {
                    OnItemClick(tvHidePost.Text, dataPost);
                    popupWindow?.Dismiss();
                };

                llReportPost.Click += (sender, args) =>
                {
                    OnItemClick(tvReportPost.Text, dataPost);
                    popupWindow?.Dismiss();
                };

                llEditPost.Click += (sender, args) =>
                {
                    OnItemClick(tvEditPost.Text, dataPost);
                    popupWindow?.Dismiss();
                };

                llBoostPost.Click += (sender, args) =>
                {
                    OnItemClick(tvBoostPost.Text, dataPost);
                    popupWindow?.Dismiss();
                };

                llDisableComments.Click += (sender, args) =>
                {
                    OnItemClick(tvDisableComments.Text, dataPost);
                    popupWindow?.Dismiss();
                };

                llDeletePost.Click += (sender, args) =>
                {
                    OnItemClick(tvDeletePost.Text, dataPost);
                    popupWindow?.Dismiss();
                };

                popupWindow.SetBackgroundDrawable(new ColorDrawable());
                popupWindow.Focusable = true;
                popupWindow.ClippingEnabled = true;
                popupWindow.OutsideTouchable = false;
                popupWindow.DismissEvent += delegate
                {
                    try
                    {
                        popupWindow?.Dismiss();
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                };

                popupWindow?.ShowAsDropDown(v);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        public void JobPostClick(GlobalClickEventArgs item)
        {
            try
            {
                var intent = new Intent(MainContext, typeof(JobsViewActivity));
                if (item.NewsFeedClass != null)
                    intent.PutExtra("JobsObject", JsonConvert.SerializeObject(item.NewsFeedClass.Job?.JobInfoClass));
                MainContext.StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void JobButtonPostClick(GlobalClickEventArgs item)
        {
            try
            {
                using var jobButton = item.View.FindViewById<AppCompatButton>(Resource.Id.JobButton);
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(MainContext, MainContext?.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    return;
                }

                switch (jobButton?.Tag?.ToString())
                {
                    // Open Apply Job Activity 
                    case "ShowApply":
                        {
                            if (item.NewsFeedClass.Job != null && item.NewsFeedClass.Job.Value.JobInfoClass.ApplyCount == "0")
                            {
                                ToastUtils.ShowToast(MainContext, MainContext.GetString(Resource.String.Lbl_ThereAreNoRequests), ToastLength.Short);
                                return;
                            }

                            var intent = new Intent(MainContext, typeof(ShowApplyJobActivity));
                            if (item.NewsFeedClass.Job != null)
                                intent.PutExtra("JobsObject", JsonConvert.SerializeObject(item.NewsFeedClass.Job.Value.JobInfoClass));
                            MainContext.StartActivity(intent);
                            break;
                        }
                    case "Apply":
                        {
                            var intent = new Intent(MainContext, typeof(ApplyJobActivity));
                            if (item.NewsFeedClass.Job != null)
                                intent.PutExtra("JobsObject", JsonConvert.SerializeObject(item.NewsFeedClass.Job.Value.JobInfoClass));
                            MainContext.StartActivity(intent);
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ImagePostClick(GlobalClickEventArgs item)
        {
            try
            {
                if (item.NewsFeedClass != null)
                {
                    var intent = new Intent(MainContext, typeof(MultiImagesPostViewerActivity));
                    intent.PutExtra("indexImage", item.Position.ToString()); // Index Image Show
                    intent.PutExtra("AlbumObject", JsonConvert.SerializeObject(item.NewsFeedClass)); // PostDataObject

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
                    {
                        ActivityOptions options = ActivityOptions.MakeCustomAnimation(MainContext, Resource.Animation.abc_popup_enter, Resource.Animation.popup_exit);
                        MainContext.StartActivity(intent, options?.ToBundle());
                    }
                    else
                    {
                        MainContext.OverridePendingTransition(Resource.Animation.abc_popup_enter, Resource.Animation.popup_exit);
                        MainContext.StartActivity(intent);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SecondReactionButtonClick(GlobalClickEventArgs item)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(MainContext, MainContext.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    return;
                }

                switch (UserDetails.SoundControl)
                {
                    case true:
                        Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("select.mp3");
                        break;
                }

                var secondReactionText = item.View.FindViewById<TextView>(Resource.Id.SecondReactionText);

                switch (AppSettings.PostButton)
                {
                    case PostButtonSystem.Wonder when item.NewsFeedClass.IsWondered != null && (bool)item.NewsFeedClass.IsWondered:
                        {
                            var x = Convert.ToInt32(item.NewsFeedClass.PostWonders);
                            switch (x)
                            {
                                case > 0:
                                    x--;
                                    break;
                                default:
                                    x = 0;
                                    break;
                            }

                            item.NewsFeedClass.IsWondered = false;
                            item.NewsFeedClass.PostWonders = Convert.ToString(x, CultureInfo.InvariantCulture);

                            var unwrappedDrawable = AppCompatResources.GetDrawable(MainContext, Resource.Drawable.icon_post_wonder_vector);
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

                            secondReactionText.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable, null, null, null);

                            secondReactionText.Text = MainContext.GetString(Resource.String.Btn_Wonder);
                            secondReactionText.SetTextColor(Color.ParseColor("#666666"));
                            break;
                        }
                    case PostButtonSystem.Wonder:
                        {
                            var x = Convert.ToInt32(item.NewsFeedClass.PostWonders);
                            x++;

                            item.NewsFeedClass.PostWonders = Convert.ToString(x, CultureInfo.InvariantCulture);
                            item.NewsFeedClass.IsWondered = true;

                            var unwrappedDrawable = AppCompatResources.GetDrawable(MainContext, Resource.Drawable.icon_post_wonder_vector);
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

                            secondReactionText.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable, null, null, null);

                            secondReactionText.Text = MainContext.GetString(Resource.String.Lbl_wondered);
                            secondReactionText.SetTextColor(Color.ParseColor("#f89823"));

                            item.NewsFeedClass.Reaction ??= new Reaction();
                            if (item.NewsFeedClass.Reaction.IsReacted != null && item.NewsFeedClass.Reaction.IsReacted.Value)
                            {
                                item.NewsFeedClass.Reaction.IsReacted = false;
                            }

                            break;
                        }
                    case PostButtonSystem.DisLike when item.NewsFeedClass.IsWondered != null && item.NewsFeedClass.IsWondered.Value:
                        {
                            var unwrappedDrawable = AppCompatResources.GetDrawable(MainContext, Resource.Drawable.icon_post_dislike_vector);
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

                            secondReactionText.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable, null, null, null);

                            secondReactionText.Text = MainContext.GetString(Resource.String.Btn_Dislike);
                            secondReactionText.SetTextColor(Color.ParseColor("#666666"));

                            var x = Convert.ToInt32(item.NewsFeedClass.PostWonders);
                            switch (x)
                            {
                                case > 0:
                                    x--;
                                    break;
                                default:
                                    x = 0;
                                    break;
                            }

                            item.NewsFeedClass.IsWondered = false;
                            item.NewsFeedClass.PostWonders = Convert.ToString(x, CultureInfo.InvariantCulture);
                            break;
                        }
                    case PostButtonSystem.DisLike:
                        {
                            var x = Convert.ToInt32(item.NewsFeedClass.PostWonders);
                            x++;

                            item.NewsFeedClass.PostWonders = Convert.ToString(x, CultureInfo.InvariantCulture);
                            item.NewsFeedClass.IsWondered = true;

                            Drawable unwrappedDrawable = AppCompatResources.GetDrawable(MainContext, Resource.Drawable.icon_post_dislike_vector);
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

                            secondReactionText.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable, null, null, null);

                            secondReactionText.Text = MainContext.GetString(Resource.String.Lbl_disliked);
                            secondReactionText.SetTextColor(Color.ParseColor("#f89823"));

                            item.NewsFeedClass.Reaction ??= new Reaction();
                            if (item.NewsFeedClass.Reaction.IsReacted != null && item.NewsFeedClass.Reaction.IsReacted.Value)
                            {
                                item.NewsFeedClass.Reaction.IsReacted = false;
                            }

                            break;
                        }
                }

                var adapterGlobal = WRecyclerView.GetInstance()?.NativeFeedAdapter;

                var dataGlobal = adapterGlobal?.ListDiffer?.Where(a => a.PostData?.Id == item.NewsFeedClass.Id).ToList();
                switch (dataGlobal?.Count)
                {
                    case > 0:
                        {
                            foreach (var dataClass in from dataClass in dataGlobal let index = adapterGlobal.ListDiffer.IndexOf(dataClass) where index > -1 select dataClass)
                            {
                                dataClass.PostData = item.NewsFeedClass;
                                adapterGlobal.NotifyItemChanged(adapterGlobal.ListDiffer.IndexOf(dataClass), "reaction");
                            }

                            break;
                        }
                }

                switch (AppSettings.PostButton)
                {
                    case PostButtonSystem.Wonder:
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Posts.PostActionsAsync(item.NewsFeedClass.Id, "wonder") });
                        break;
                    case PostButtonSystem.DisLike:
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Posts.PostActionsAsync(item.NewsFeedClass.Id, "dislike") });
                        break;
                }

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SingleImagePostClick(GlobalClickEventArgs item)
        {
            try
            {
                if (item.NewsFeedClass != null)
                {
                    var intent = new Intent(MainContext, typeof(ImagePostViewerActivity));
                    intent.PutExtra("itemIndex", "00"); //PhotoAlbumObject
                    intent.PutExtra("AlbumObject", JsonConvert.SerializeObject(item.NewsFeedClass)); // PostDataObject

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
                    {
                        ActivityOptions options = ActivityOptions.MakeCustomAnimation(MainContext, Resource.Animation.abc_popup_enter, Resource.Animation.popup_exit);
                        MainContext.StartActivity(intent, options?.ToBundle());
                    }
                    else
                    {
                        MainContext.OverridePendingTransition(Resource.Animation.abc_popup_enter, Resource.Animation.popup_exit);
                        MainContext.StartActivity(intent);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public async void MapPostClick(GlobalClickEventArgs item)
        {
            try
            {
                if (item.NewsFeedClass != null)
                {
                    if (item.NewsFeedClass.CurrentLatitude > 0 && item.NewsFeedClass.CurrentLongitude > 0)
                    {
                        // Create a Uri from an intent string. Use the result to create an Intent?. 
                        var uri = Uri.Parse("geo:" + item.NewsFeedClass.CurrentLatitude + "," + item.NewsFeedClass.CurrentLongitude);
                        var intent = new Intent(Intent.ActionView, uri);
                        intent.SetPackage("com.google.android.apps.maps");
                        intent.AddFlags(ActivityFlags.NewTask);
                        MainContext.StartActivity(intent);
                    }
                    else
                    {
                        var latLng = await WoWonderTools.GetLocationFromAddress(MainContext, item.NewsFeedClass.PostMap);
                        if (latLng != null)
                        {
                            item.NewsFeedClass.CurrentLatitude = latLng.Latitude;
                            item.NewsFeedClass.CurrentLongitude = latLng.Longitude;

                            // Create a Uri from an intent string. Use the result to create an Intent?. 
                            var uri = Uri.Parse("geo:" + item.NewsFeedClass.CurrentLatitude + "," + item.NewsFeedClass.CurrentLongitude);
                            var intent = new Intent(Intent.ActionView, uri);
                            intent.SetPackage("com.google.android.apps.maps");
                            intent.AddFlags(ActivityFlags.NewTask);
                            MainContext.StartActivity(intent);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OffersPostClick(GlobalClickEventArgs item)
        {
            try
            {
                if (item.NewsFeedClass != null)
                {
                    var intent = new Intent(MainContext, typeof(OffersViewActivity));
                    intent.PutExtra("OffersObject", JsonConvert.SerializeObject(item.NewsFeedClass.Offer?.OfferClass));
                    MainContext.StartActivity(intent);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OpenAllViewer(string type, string passedId, AdapterModelsClass item)
        {
            try
            {
                var intent = new Intent(MainContext, typeof(AllViewerActivity));
                intent.PutExtra("Type", type); //StoryModel , FollowersModel , GroupsModel , PagesModel , ImagesModel
                intent.PutExtra("PassedId", passedId);

                switch (type)
                {
                    case "StoryModel":
                        intent.PutExtra("itemObject", JsonConvert.SerializeObject(item));
                        break;
                    case "FollowersModel":
                        intent.PutExtra("itemObject", JsonConvert.SerializeObject(item.FollowersModel));
                        break;
                    case "GroupsModel":
                        intent.PutExtra("itemObject", JsonConvert.SerializeObject(item.GroupsModel));
                        break;
                    case "PagesModel":
                        intent.PutExtra("itemObject", JsonConvert.SerializeObject(item.PagesModel));
                        break;
                    case "ImagesModel":
                        intent.PutExtra("itemObject", JsonConvert.SerializeObject(item.ImagesModel));
                        break;
                }
                MainContext.StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void YoutubePostClick(GlobalClickEventArgs item)
        {
            MainApplication.GetInstance()?.NavigateTo(MainContext, typeof(YoutubePlayerActivity), item.NewsFeedClass);
        }

        public void LinkPostClick(GlobalClickEventArgs item, string type)
        {
            try
            {
                switch (type)
                {
                    case "LinkPost" when item.NewsFeedClass.PostLink.Contains(InitializeWoWonder.WebsiteUrl) && item.NewsFeedClass.PostLink.Contains("movies/watch/"):
                        {
                            var videoId = item.NewsFeedClass.PostLink.Split("movies/watch/").Last().Replace("/", "");
                            var intent = new Intent(MainContext, typeof(VideoViewerActivity));
                            //intent.PutExtra("Viewer_Video", JsonConvert.SerializeObject(item));
                            intent.PutExtra("VideoId", videoId);
                            MainContext.StartActivity(intent);
                            break;
                        }
                    case "LinkPost":
                        {
                            item.NewsFeedClass.PostLink = item.NewsFeedClass.PostLink.Contains("http") switch
                            {
                                false => "http://" + item.NewsFeedClass.PostLink,
                                _ => item.NewsFeedClass.PostLink
                            };

                            if (item.NewsFeedClass.PostLink.Contains("tiktok"))
                                new IntentController(MainContext).OpenBrowserFromApp(item.NewsFeedClass.PostTikTok);
                            else
                                new IntentController(MainContext).OpenBrowserFromApp(item.NewsFeedClass.PostLink);
                            break;
                        }
                    default:
                        {
                            item.NewsFeedClass.Url = item.NewsFeedClass.Url.Contains("http") switch
                            {
                                false => "http://" + item.NewsFeedClass.Url,
                                _ => item.NewsFeedClass.Url
                            };

                            new IntentController(MainContext).OpenBrowserFromApp(item.NewsFeedClass.Url);
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ProductPostClick(GlobalClickEventArgs item)
        {
            try
            {
                var intent = new Intent(MainContext, typeof(ProductViewActivity));
                intent.PutExtra("Id", item.NewsFeedClass?.PostId);
                if (item?.NewsFeedClass?.Product != null)
                {
                    intent.PutExtra("ProductView", JsonConvert.SerializeObject(item.NewsFeedClass?.Product.Value.ProductClass));
                }
                MainContext.StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OpenFundingPostClick(GlobalClickEventArgs item)
        {
            try
            {
                Intent intent = new Intent(MainContext, typeof(FundingViewActivity));
                var postType = PostFunctions.GetAdapterType(item.NewsFeedClass);
                switch (postType)
                {
                    case PostModelType.FundingPost:
                        {
                            if (item.NewsFeedClass?.FundData != null)
                            {
                                item.NewsFeedClass.FundData.Value.FundDataClass.UserData =
                                    item.NewsFeedClass?.FundData.Value.FundDataClass.UserData switch
                                    {
                                        null => item.NewsFeedClass.Publisher,
                                        _ => item.NewsFeedClass.FundData.Value.FundDataClass.UserData
                                    };

                                intent.PutExtra("FundId", item.NewsFeedClass?.FundData.Value.FundDataClass.HashedId);
                                intent.PutExtra("ItemObject", JsonConvert.SerializeObject(item.NewsFeedClass?.FundData.Value.FundDataClass));
                            }

                            break;
                        }
                    case PostModelType.PurpleFundPost:
                        {
                            if (item.NewsFeedClass?.Fund != null)
                            {
                                item.NewsFeedClass.Fund.Value.PurpleFund.Fund.UserData =
                                    item.NewsFeedClass?.Fund.Value.PurpleFund.Fund.UserData switch
                                    {
                                        null => item.NewsFeedClass.Publisher,
                                        _ => item.NewsFeedClass.Fund.Value.PurpleFund.Fund.UserData
                                    };

                                intent.PutExtra("FundId", item.NewsFeedClass?.Fund.Value.PurpleFund.Fund.HashedId);
                                intent.PutExtra("ItemObject", JsonConvert.SerializeObject(item.NewsFeedClass?.Fund.Value.PurpleFund.Fund));
                            }

                            break;
                        }
                }

                MainContext.StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OpenFilePostClick(GlobalClickEventArgs item)
        {
            try
            {
                var fileSplit = item.NewsFeedClass.PostFileFull.Split('/').Last();
                string getFile = Methods.MultiMedia.GetMediaFrom_Disk(Methods.Path.FolderDcimFile, fileSplit);
                if (getFile != "File Dont Exists")
                {
                    File file2 = new File(getFile);
                    var photoUri = FileProvider.GetUriForFile(MainContext, MainContext.PackageName + ".fileprovider", file2);

                    Intent openFile = new Intent(Intent.ActionView, photoUri);
                    openFile.SetFlags(ActivityFlags.NewTask);
                    openFile.SetFlags(ActivityFlags.GrantReadUriPermission);
                    MainContext.StartActivity(openFile);
                }
                else
                {
                    Intent intent = new Intent(Intent.ActionView, Uri.Parse(item.NewsFeedClass.PostFileFull));
                    MainContext.StartActivity(intent);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                ToastUtils.ShowToast(MainContext, MainContext.GetText(Resource.String.Lbl_FileNotDeviceMemory), ToastLength.Long);
            }
        }

        //Download
        public void FileDownloadPostClick(GlobalClickEventArgs item)
        {
            try
            {
                switch (string.IsNullOrEmpty(item.NewsFeedClass.PostFileFull))
                {
                    case false:
                        {
                            Methods.Path.Chack_MyFolder();

                            var fileSplit = item.NewsFeedClass.PostFileFull.Split('/').Last();
                            string getFile = Methods.MultiMedia.GetMediaFrom_Disk(Methods.Path.FolderDcimFile, fileSplit);
                            if (getFile != "File Dont Exists")
                            {
                                ToastUtils.ShowToast(MainContext, MainContext.GetText(Resource.String.Lbl_FileExists), ToastLength.Long);
                            }
                            else
                            {
                                Methods.MultiMedia.DownloadMediaTo_DiskAsync(Methods.Path.FolderDcimFile, item.NewsFeedClass.PostFileFull);
                                ToastUtils.ShowToast(MainContext, MainContext.GetText(Resource.String.Lbl_YourFileIsDownloaded), ToastLength.Long);
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

        public void EventItemPostClick(GlobalClickEventArgs item)
        {
            try
            {
                var intent = new Intent(MainContext, typeof(EventViewActivity));
                if (item.NewsFeedClass.Event != null)
                {
                    intent.PutExtra("EventId", item.NewsFeedClass.Event.Value.EventClass.Id);
                    intent.PutExtra("EventType", "events");
                    intent.PutExtra("EventView", JsonConvert.SerializeObject(item.NewsFeedClass.Event.Value.EventClass));
                }
                MainContext.StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ArticleItemPostClick(ArticleDataObject item)
        {
            try
            {
                var intent = new Intent(MainContext, typeof(ArticlesViewActivity));
                intent.PutExtra("Id", item.Id);
                intent.PutExtra("ArticleObject", JsonConvert.SerializeObject(item));
                MainContext.StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void DataItemPostClick(GlobalClickEventArgs item)
        {
            try
            {
                switch (AppSettings.PostButton)
                {
                    case PostButtonSystem.Reaction:
                        {
                            switch (item.NewsFeedClass.Reaction.Count)
                            {
                                case > 0:
                                    {
                                        var intent = new Intent(MainContext, typeof(ReactionPostTabbedActivity));
                                        intent.PutExtra("PostObject", JsonConvert.SerializeObject(item.NewsFeedClass));
                                        MainContext.StartActivity(intent);
                                        break;
                                    }
                            }

                            break;
                        }
                    default:
                        {
                            var intent = new Intent(MainContext, typeof(PostDataActivity));
                            intent.PutExtra("PostId", item.NewsFeedClass.Id);
                            intent.PutExtra("PostType", "post_likes");
                            MainContext.StartActivity(intent);
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> Edit Info Post if user == is_owner  
        private void EditInfoPost_OnClick(PostDataObject item)
        {
            try
            {
                Intent intent = new Intent(MainContext, typeof(EditPostActivity));
                intent.PutExtra("PostId", item.Id);
                intent.PutExtra("PostItem", JsonConvert.SerializeObject(item));
                MainContext.StartActivityForResult(intent, 3950);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> Edit Info Product if user == is_owner  
        private void EditInfoProduct_OnClick(PostDataObject item)
        {
            try
            {
                Intent intent = new Intent(MainContext, typeof(EditProductActivity));
                if (item.Product != null)
                    intent.PutExtra("ProductView", JsonConvert.SerializeObject(item.Product.Value.ProductClass));
                MainContext.StartActivityForResult(intent, 3500);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> Edit Info Offers if user == is_owner  
        private void EditInfoOffers_OnClick(PostDataObject item)
        {
            try
            {
                Intent intent = new Intent(MainContext, typeof(EditOffersActivity));
                intent.PutExtra("OfferId", item.OfferId);
                if (item.Offer != null)
                    intent.PutExtra("OfferItem", JsonConvert.SerializeObject(item.Offer.Value.OfferClass));
                MainContext.StartActivityForResult(intent, 4000); //wael
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OpenImageLightBox(PostDataObject item)
        {
            try
            {
                var intent = new Intent(MainContext, typeof(ImagePostViewerActivity));
                intent.PutExtra("itemIndex", "00"); //PhotoAlbumObject
                intent.PutExtra("AlbumObject", JsonConvert.SerializeObject(item)); // PostDataObject
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
                {
                    ActivityOptions options = ActivityOptions.MakeCustomAnimation(MainContext, Resource.Animation.abc_popup_enter, Resource.Animation.popup_exit);
                    MainContext.StartActivity(intent, options?.ToBundle());
                }
                else
                {
                    MainContext.OverridePendingTransition(Resource.Animation.abc_popup_enter, Resource.Animation.popup_exit);
                    MainContext.StartActivity(intent);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region More Dialog

        public void OnItemClick(string item, PostDataObject dataPost)
        {
            try
            {
                string text = item;
                if (text == MainContext.GetString(Resource.String.Lbl_CopeLink))
                {
                    CopyLinkEvent(dataPost.Url);
                }
                else if (text == MainContext.GetString(Resource.String.Lbl_CopeText))
                {
                    CopyLinkEvent(Methods.FunString.DecodeString(dataPost.Orginaltext));
                }
                else if (text == MainContext.GetString(Resource.String.Lbl_EditPost))
                {
                    EditInfoPost_OnClick(dataPost);
                }
                else if (text == MainContext.GetString(Resource.String.Lbl_EditProduct))
                {
                    EditInfoProduct_OnClick(dataPost);
                }
                else if (text == MainContext.GetString(Resource.String.Lbl_EditOffers))
                {
                    EditInfoOffers_OnClick(dataPost);
                }
                else if (text == MainContext.GetString(Resource.String.Lbl_HidePost))
                {
                    HidePostEvent(dataPost);
                }
                else if (text == MainContext.GetString(Resource.String.Lbl_ReportPost))
                {
                    ReportPostEvent(dataPost);
                }
                else if (text == MainContext.GetString(Resource.String.Lbl_DeletePost))
                {
                    DeletePostEvent(dataPost);
                }
                else if (text == MainContext.GetString(Resource.String.Lbl_BoostPost) || text == MainContext.GetString(Resource.String.Lbl_UnBoostPost))
                {
                    BoostPostEvent(dataPost);
                }
                else if (text == MainContext.GetString(Resource.String.Lbl_EnableComments) || text == MainContext.GetString(Resource.String.Lbl_DisableComments))
                {
                    StatusCommentsPostEvent(dataPost);
                }
                else if (text == MainContext.GetString(Resource.String.Lbl_SavePost) || text == MainContext.GetString(Resource.String.Lbl_UnSavePost))
                {
                    SavePostEvent(dataPost);
                }
                MorePost?.Dismiss();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion MaterialDialog

    }
}