using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Content;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Google.Android.Material.BottomSheet;
using Java.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WoWonder.Activities.Communities.Groups;
using WoWonder.Activities.Communities.Pages;
using WoWonder.Activities.NativePost.Pages;
using WoWonder.Activities.Story;
using WoWonder.Activities.Tabbes;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Story;
using WoWonderClient.Requests;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.MyProfile
{
    public class OptionAvatarProfileDialogFragment : BottomSheetDialogFragment
    {
        #region Variables Basic

        private RelativeLayout ViewStoryLayout, ResetAvatarLayout, SelectAvatarLayout, ViewAvatarLayout;
        private TextView ViewStoryIcon, ResetAvatarIcon, SelectAvatarIcon, ViewAvatarIcon;
        private TextView ViewStoryText, ResetAvatarText, SelectAvatarText, ViewAvatarText;

        private string Page;
        private UserDataObject UserData;
        private GroupDataObject GroupData;
        private PageDataObject PageData;

        #endregion

        #region General

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                Context contextThemeWrapper = WoWonderTools.IsTabDark() ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme);
                // clone the inflater using the ContextThemeWrapper
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);

                View view = localInflater?.Inflate(Resource.Layout.BottomSheetOptionAvatarProfileLayout, container, false);
                return view;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            {
                base.OnViewCreated(view, savedInstanceState);

                Page = Arguments?.GetString("Page") ?? "";
                if (Page is "UserProfile" or "MyProfile")
                {
                    UserData = JsonConvert.DeserializeObject<UserDataObject>(Arguments?.GetString("UserData") ?? "");
                }
                else if (Page is "GroupProfile")
                {
                    GroupData = JsonConvert.DeserializeObject<GroupDataObject>(Arguments?.GetString("GroupData") ?? "");
                }
                else if (Page is "PageProfile")
                {
                    PageData = JsonConvert.DeserializeObject<PageDataObject>(Arguments?.GetString("PageData") ?? "");
                }

                InitComponent(view);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                ViewStoryLayout = view.FindViewById<RelativeLayout>(Resource.Id.ViewStoryLayout);
                ViewStoryIcon = view.FindViewById<TextView>(Resource.Id.ViewStoryIcon);
                ViewStoryText = view.FindViewById<TextView>(Resource.Id.ViewStoryText);
                ViewStoryLayout.Click += ViewStoryLayoutOnClick;

                ResetAvatarLayout = view.FindViewById<RelativeLayout>(Resource.Id.ResetAvatarLayout);
                ResetAvatarIcon = view.FindViewById<TextView>(Resource.Id.ResetAvatarIcon);
                ResetAvatarText = view.FindViewById<TextView>(Resource.Id.ResetAvatarText);
                ResetAvatarLayout.Click += ResetAvatarLayoutOnClick;

                SelectAvatarLayout = view.FindViewById<RelativeLayout>(Resource.Id.SelectAvatarLayout);
                SelectAvatarIcon = view.FindViewById<TextView>(Resource.Id.SelectAvatarIcon);
                SelectAvatarText = view.FindViewById<TextView>(Resource.Id.SelectAvatarText);
                SelectAvatarLayout.Click += SelectAvatarLayoutOnClick;

                ViewAvatarLayout = view.FindViewById<RelativeLayout>(Resource.Id.ViewAvatarLayout);
                ViewAvatarIcon = view.FindViewById<TextView>(Resource.Id.ViewAvatarIcon);
                ViewAvatarText = view.FindViewById<TextView>(Resource.Id.ViewAvatarText);
                ViewAvatarLayout.Click += ViewAvatarLayoutOnClick;

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, ViewStoryIcon, IonIconsFonts.RadioButtonOn);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, ResetAvatarIcon, IonIconsFonts.Refresh);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, SelectAvatarIcon, IonIconsFonts.IosImages);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeSolid, ViewAvatarIcon, FontAwesomeIcon.Image);

                if (Page is "UserProfile" or "MyProfile")
                {
                    if (!WoWonderTools.StoryIsAvailable(UserData.UserId))
                        ViewStoryLayout.Visibility = ViewStates.Gone;

                    if (UserData.Avatar.Contains("d-avatar") || UserData.Avatar.Contains("f-avatar"))
                        ViewAvatarLayout.Visibility = ViewStates.Gone;

                    if (Page == "UserProfile")
                    {
                        ResetAvatarLayout.Visibility = ViewStates.Gone;
                        SelectAvatarLayout.Visibility = ViewStates.Gone;
                    }
                }
                else if (Page is "GroupProfile")
                {
                    if (GroupData.Avatar.Contains("d-group"))
                        ViewAvatarLayout.Visibility = ViewStates.Gone;

                    if (GroupData.IsOwner != null && GroupData.IsOwner.Value)
                    {
                        ResetAvatarLayout.Visibility = ViewStates.Visible;
                        SelectAvatarLayout.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        ResetAvatarLayout.Visibility = ViewStates.Gone;
                        SelectAvatarLayout.Visibility = ViewStates.Gone;
                    }

                    ViewStoryLayout.Visibility = ViewStates.Gone;
                }
                else if (Page is "PageProfile")
                {
                    if (PageData.Avatar.Contains("d-page"))
                        ViewAvatarLayout.Visibility = ViewStates.Gone;

                    if (PageData.IsPageOnwer != null && PageData.IsPageOnwer.Value)
                    {
                        ResetAvatarLayout.Visibility = ViewStates.Visible;
                        SelectAvatarLayout.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        ResetAvatarLayout.Visibility = ViewStates.Gone;
                        SelectAvatarLayout.Visibility = ViewStates.Gone;
                    }

                    ViewStoryLayout.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event

        private void ViewAvatarLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Page is "UserProfile" or "MyProfile")
                {
                    if (UserData.Avatar.Contains("d-avatar") || UserData.Avatar.Contains("f-avatar"))
                        return;

                    if (!string.IsNullOrEmpty(UserData.AvatarPostId) && UserData.AvatarPostId != "0")
                    {
                        var intent = new Intent(Activity, typeof(ViewFullPostActivity));
                        intent.PutExtra("Id", UserData.AvatarPostId);
                        //intent.PutExtra("DataItem", JsonConvert.SerializeObject(e.NewsFeedClass));
                        Activity.StartActivity(intent);
                    }
                    else
                    {
                        var media = WoWonderTools.GetFile("", Methods.Path.FolderDiskImage, UserData.Avatar.Split('/').Last(), UserData.Avatar);
                        if (media.Contains("http"))
                        {
                            var intent = new Intent(Intent.ActionView, Uri.Parse(media));
                            Activity.StartActivity(intent);
                        }
                        else
                        {
                            var file2 = new File(media);
                            var photoUri = FileProvider.GetUriForFile(Activity, Activity.PackageName + ".fileprovider", file2);

                            var intent = new Intent(Intent.ActionPick);
                            intent.SetAction(Intent.ActionView);
                            intent.AddFlags(ActivityFlags.GrantReadUriPermission);
                            intent.SetDataAndType(photoUri, "image/*");
                            Activity.StartActivity(intent);
                        }
                    }
                }
                else if (Page is "GroupProfile")
                {
                    if (GroupData.Avatar.Contains("d-group"))
                        return;

                    var media = WoWonderTools.GetFile("", Methods.Path.FolderDiskImage, GroupData.Avatar.Split('/').Last(), GroupData.Avatar);
                    if (media.Contains("http"))
                    {
                        Intent intent = new Intent(Intent.ActionView, Uri.Parse(media));
                        StartActivity(intent);
                    }
                    else
                    {
                        File file2 = new File(media);
                        var photoUri = FileProvider.GetUriForFile(Activity, Activity.PackageName + ".fileprovider", file2);

                        Intent intent = new Intent(Intent.ActionPick);
                        intent.SetAction(Intent.ActionView);
                        intent.AddFlags(ActivityFlags.GrantReadUriPermission);
                        intent.SetDataAndType(photoUri, "image/*");
                        StartActivity(intent);
                    }
                }
                else if (Page is "PageProfile")
                {
                    if (PageData.Avatar.Contains("d-page"))
                        return;

                    var media = WoWonderTools.GetFile("", Methods.Path.FolderDiskImage, PageData.Avatar.Split('/').Last(), PageData.Avatar);
                    if (media.Contains("http"))
                    {
                        var intent = new Intent(Intent.ActionView, Uri.Parse(media));
                        StartActivity(intent);
                    }
                    else
                    {
                        var file2 = new File(media);
                        var photoUri = FileProvider.GetUriForFile(Activity, Activity.PackageName + ".fileprovider", file2);

                        var intent = new Intent(Intent.ActionPick);
                        intent.SetAction(Intent.ActionView);
                        intent.AddFlags(ActivityFlags.GrantReadUriPermission);
                        intent.SetDataAndType(photoUri, "image/*");
                        StartActivity(intent);
                    }
                }

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ResetAvatarLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                    ToastUtils.ShowToast(Context, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                else
                {
                    if (Page is "MyProfile")
                    {
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.ResetAvatarAsync("user") });

                        UserDetails.Avatar = WoWonderTools.GetDefaultAvatar();

                        var myProfile = MyProfileActivity.GetInstance();
                        if (myProfile != null)
                        {
                            if (UserData.Gender == "male")
                                Glide.With(myProfile).Load(Resource.Drawable.no_profile_image_circle).Apply(new RequestOptions().CircleCrop()).Into(myProfile.ImageAvatar);
                            else if (UserData.Gender == "female")
                                Glide.With(myProfile).Load(Resource.Drawable.no_profile_female_image_circle).Apply(new RequestOptions().CircleCrop()).Into(myProfile.ImageAvatar);
                            else
                                Glide.With(myProfile).Load(Resource.Drawable.no_profile_image_circle).Apply(new RequestOptions().CircleCrop()).Into(myProfile.ImageAvatar);

                            myProfile.PostFeedAdapter?.NotifyDataSetChanged();
                        }

                        var instance = TabbedMainActivity.GetInstance();
                        if (instance != null)
                        {
                            GlideImageLoader.LoadImage(instance, UserDetails.Avatar, instance.MoreTab?.ProfileImage, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);

                            instance.NewsFeedTab.PostFeedAdapter.NotifyDataSetChanged();
                        }
                    }
                    else if (Page is "GroupProfile")
                    {
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.ResetAvatarAsync("group", GroupData.GroupId) });

                        var groupProfile = GroupProfileActivity.GetInstance();
                        if (groupProfile != null)
                        {
                            Glide.With(groupProfile).Load(Resource.Drawable.default_group).Apply(new RequestOptions().CircleCrop()).Into(groupProfile.UserProfileImage);
                        }
                    }
                    else if (Page is "PageProfile")
                    {
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.ResetAvatarAsync("page", PageData.PageId) });

                        var pageProfile = PageProfileActivity.GetInstance();
                        if (pageProfile != null)
                        {
                            Glide.With(pageProfile).Load(Resource.Drawable.default_page).Apply(new RequestOptions().CircleCrop()).Into(pageProfile.ProfileImage);
                        }
                    }

                    Dismiss();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SelectAvatarLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Page is "MyProfile")
                {
                    var myProfile = MyProfileActivity.GetInstance();
                    if (myProfile != null)
                    {
                        myProfile.ImageType = "Avatar";
                        PixImagePickerUtils.OpenDialogGallery(myProfile);
                    }
                }
                else if (Page is "GroupProfile")
                {
                    var groupProfile = GroupProfileActivity.GetInstance();
                    if (groupProfile != null)
                    {
                        groupProfile.ImageType = "Avatar";
                        PixImagePickerUtils.OpenDialogGallery(groupProfile);
                    }
                }
                else if (Page is "PageProfile")
                {
                    var pageProfile = PageProfileActivity.GetInstance();
                    if (pageProfile != null)
                    {
                        pageProfile.ImageType = "Avatar";
                        PixImagePickerUtils.OpenDialogGallery(pageProfile);
                    }
                }

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ViewStoryLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Page is "UserProfile" or "MyProfile")
                {
                    var tab = TabbedMainActivity.GetInstance()?.NewsFeedTab;
                    StoryDataObject dataMyStory = tab?.PostFeedAdapter?.HolderStory?.StoryAdapter?.StoryList?.FirstOrDefault(o => o.UserId == UserData.UserId);
                    if (dataMyStory != null)
                    {
                        List<StoryDataObject> storyList = new List<StoryDataObject>(tab.PostFeedAdapter?.HolderStory.StoryAdapter.StoryList);
                        storyList.RemoveAll(o => o.Type is "Your" or "Live");

                        Intent intent = new Intent(Activity, typeof(StoryDetailsActivity));
                        intent.PutExtra("UserId", UserData.UserId);
                        intent.PutExtra("IndexItem", 0);
                        intent.PutExtra("StoriesCount", storyList.Count);
                        intent.PutExtra("DataItem", JsonConvert.SerializeObject(new ObservableCollection<StoryDataObject>(storyList)));
                        Activity.StartActivity(intent);
                    }
                }

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

    }
}