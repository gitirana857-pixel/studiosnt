using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Content.Res;
using AndroidX.AppCompat.Widget;
using Google.Android.Material.Dialog;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using WoWonder.Activities.Base;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Requests;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.EditPost
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class EditPostActivity : BaseActivity, IDialogListCallBack
    {
        #region Variables Basic

        private ImageView PostSectionImage;
        private TextView TxtAddPost, TxtUserName;
        private EditText TxtContentPost;
        private AppCompatButton PostPrivacyButton;
        private string IdPost = "", PostPrivacy = "", TypeDialog = "";
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
                SetContentView(Resource.Layout.EditPostLayout);

                PostData = JsonConvert.DeserializeObject<PostDataObject>(Intent?.GetStringExtra("PostItem") ?? "");

                var id = Intent?.GetStringExtra("PostId") ?? "Data not available";
                if (id != "Data not available" && !string.IsNullOrEmpty(id))
                    IdPost = id;

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
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
                TxtAddPost = FindViewById<TextView>(Resource.Id.toolbar_title);
                TxtContentPost = FindViewById<EditText>(Resource.Id.editTxtEmail);
                TxtUserName = FindViewById<TextView>(Resource.Id.card_name);
                PostSectionImage = FindViewById<ImageView>(Resource.Id.postsectionimage);
                PostPrivacyButton = FindViewById<AppCompatButton>(Resource.Id.cont);

                TxtContentPost.Text = Methods.FunString.DecodeString(PostData.Orginaltext);

                Methods.SetColorEditText(TxtContentPost, WoWonderTools.IsTabDark() ? Color.White : Color.Black);

                GetPrivacyPost();
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
                    toolBar.Title = GetText(Resource.String.Lbl_EditPost);
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

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                switch (addEvent)
                {
                    // true +=  // false -=
                    case true:
                        PostPrivacyButton.Click += PostPrivacyButton_Click;
                        TxtAddPost.Click += TxtAddPostOnClick;
                        break;
                    default:
                        PostPrivacyButton.Click -= PostPrivacyButton_Click;
                        TxtAddPost.Click -= TxtAddPostOnClick;
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
                PostSectionImage = null!;
                TxtUserName = null!;
                TxtAddPost = null!;
                TxtContentPost = null!;
                PostPrivacyButton = null!;
                IdPost = null!; PostPrivacy = null!; TypeDialog = null!;
                PostData = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        #endregion

        #region Events

        private async void TxtAddPostOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
                else
                {
                    if (string.IsNullOrEmpty(TxtContentPost.Text))
                    {
                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_YouCannot_PostanEmptyPost), ToastLength.Long);
                        return;
                    }

                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading) + "...");

                    var (apiStatus, respond) = await RequestsAsync.Posts.PostActionsAsync(IdPost, "edit", "", TxtContentPost.Text, PostPrivacy);
                    switch (apiStatus)
                    {
                        case 200:
                            {
                                switch (respond)
                                {
                                    case PostActionsObject result when result.Action.Contains("edited"):
                                        {
                                            ToastUtils.ShowToast(this, result.Action, ToastLength.Short);
                                            AndHUD.Shared.Dismiss();

                                            // put the String to pass back into an Intent and close this activity
                                            var resultIntent = new Intent();
                                            resultIntent?.PutExtra("PostId", IdPost);
                                            resultIntent?.PutExtra("PostText", TxtContentPost.Text);
                                            SetResult(Result.Ok, resultIntent);
                                            Finish();
                                            break;
                                        }
                                    case PostActionsObject result:
                                        //Show a Error image with a message
                                        AndHUD.Shared.ShowError(this, result.Action, MaskType.Clear, TimeSpan.FromSeconds(1));
                                        break;
                                }

                                break;
                            }
                        default:
                            Methods.DisplayAndHudErrorResult(this, respond);
                            break;
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                AndHUD.Shared.Dismiss();
            }
        }

        #endregion

        #region MaterialDialog

        public void OnSelection(IDialogInterface dialog, int position, string itemString)
        {
            try
            {
                switch (TypeDialog)
                {
                    case "PostPrivacy":
                        {
                            PostPrivacyButton.Text = itemString;

                            if (itemString == GetString(Resource.String.Lbl_Everyone))
                                PostPrivacy = "0";
                            else if (itemString == GetString(Resource.String.Lbl_People_i_Follow))
                                PostPrivacy = "2";
                            else if (itemString == GetString(Resource.String.Lbl_People_Follow_Me))
                                PostPrivacy = "1";
                            else if (itemString == GetString(Resource.String.Lbl_No_body))
                                PostPrivacy = "3";
                            else
                                PostPrivacy = "0";
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

        #region Privacy

        private void LoadDataUser()
        {
            try
            {
                var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                if (dataUser != null)
                {
                    GlideImageLoader.LoadImage(this, dataUser.Avatar, PostSectionImage, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);

                    TxtUserName.Text = WoWonderTools.GetNameFinal(dataUser);

                    if (PostData.PostPrivacy.Contains("0"))
                        PostPrivacyButton.Text = GetString(Resource.String.Lbl_Everyone);
                    else if (PostData.PostPrivacy.Contains("ifollow") || PostData.PostPrivacy == "2")
                        PostPrivacyButton.Text = GetString(Resource.String.Lbl_People_i_Follow);
                    else if (PostData.PostPrivacy.Contains("me") || PostData.PostPrivacy == "1")
                        PostPrivacyButton.Text = GetString(Resource.String.Lbl_People_Follow_Me);
                    else
                        PostPrivacyButton.Text = GetString(Resource.String.Lbl_No_body);

                    PostPrivacy = PostData.PostPrivacy;
                }
                else
                {
                    TxtUserName.Text = UserDetails.Username;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void GetPrivacyPost()
        {
            try
            {
                switch (string.IsNullOrEmpty(PostData.GroupId))
                {
                    case false when PostData.GroupId != "0":
                        {
                            var dataGroup = PostData.GroupRecipient;
                            if (dataGroup != null)
                            {
                                PostPrivacyButton.Visibility = ViewStates.Gone;
                                GlideImageLoader.LoadImage(this, dataGroup.Avatar, PostSectionImage, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);
                                TxtUserName.Text = dataGroup.GroupName;
                            }
                            else
                            {
                                LoadDataUser();
                            }

                            break;
                        }
                    default:
                        {
                            switch (string.IsNullOrEmpty(PostData.PageId))
                            {
                                case false when PostData.PageId != "0":
                                    PostPrivacyButton.Visibility = ViewStates.Gone;
                                    GlideImageLoader.LoadImage(this, PostData.Publisher.Avatar, PostSectionImage, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);
                                    TxtUserName.Text = PostData.Publisher.Name;
                                    break;
                                default:
                                    {
                                        switch (string.IsNullOrEmpty(PostData.EventId))
                                        {
                                            case false when PostData.EventId != "0":
                                                {
                                                    if (PostData.Event != null)
                                                    {
                                                        var dataEvent = PostData.Event.Value.EventClass;
                                                        if (dataEvent != null)
                                                        {
                                                            PostPrivacyButton.Visibility = ViewStates.Gone;
                                                            GlideImageLoader.LoadImage(this, dataEvent.Cover, PostSectionImage, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                                                            TxtUserName.Text = dataEvent.Name;
                                                        }
                                                        else
                                                        {
                                                            LoadDataUser();
                                                        }
                                                    }
                                                    else
                                                    {
                                                        LoadDataUser();
                                                    }

                                                    break;
                                                }
                                            default:
                                                LoadDataUser();
                                                break;
                                        }

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

        private void PostPrivacyButton_Click(object sender, EventArgs e)
        {
            try
            {
                TypeDialog = "PostPrivacy";

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialAlertDialogBuilder(this);

                arrayAdapter.Add(GetString(Resource.String.Lbl_Everyone));
                arrayAdapter.Add(GetString(Resource.String.Lbl_People_i_Follow));
                arrayAdapter.Add(GetText(Resource.String.Lbl_People_Follow_Me));
                arrayAdapter.Add(GetString(Resource.String.Lbl_No_body));

                dialogList.SetTitle(GetText(Resource.String.Lbl_PostPrivacy));
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

    }
}