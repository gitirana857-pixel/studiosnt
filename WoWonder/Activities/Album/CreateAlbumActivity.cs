using Android;
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
using AndroidX.Core.Content;
using AndroidX.RecyclerView.Widget;
using Com.Google.Android.Gms.Ads.Admanager;
using Google.Android.Material.Dialog;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using WoWonder.Activities.AddPost.Adapters;
using WoWonder.Activities.Base;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Album;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Requests;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.Album
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class CreateAlbumActivity : BaseActivity
    {
        #region Variables Basic

        private AttachmentsAdapter MAdapter;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private EditText TxtNameAlbum;
        private TextView TxtAdd;
        private AdManagerAdView AdManagerAdView;
        private LinearLayout llStep1;
        private RelativeLayout rlStep2;
        private ImageView EmptyPhotos;
        private AppCompatButton BtnCreateAlbum;
        private int nStep = 1;

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
                SetContentView(Resource.Layout.CreateAlbumLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                SetStep();
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
                AdsGoogle.LifecycleAdManagerAdView(AdManagerAdView, "Resume");
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
                AdsGoogle.LifecycleAdManagerAdView(AdManagerAdView, "Pause");
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
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
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
                llStep1 = FindViewById<LinearLayout>(Resource.Id.ll_step1);
                rlStep2 = FindViewById<RelativeLayout>(Resource.Id.rl_step2);
                EmptyPhotos = FindViewById<ImageView>(Resource.Id.imageView1);
                BtnCreateAlbum = FindViewById<AppCompatButton>(Resource.Id.btn_next);

                MRecycler = (RecyclerView)FindViewById(Resource.Id.recycler);
                TxtNameAlbum = (EditText)FindViewById(Resource.Id.NameEditText);

                TxtAdd = FindViewById<TextView>(Resource.Id.toolbar_title);
                TxtAdd.Text = GetText(Resource.String.Lbl_Create);
                TxtAdd.Visibility = ViewStates.Gone;

                Methods.SetColorEditText(TxtNameAlbum, WoWonderTools.IsTabDark() ? Color.White : Color.Black);

                AdManagerAdView = FindViewById<AdManagerAdView>(Resource.Id.multiple_ad_sizes_view);
                AdsGoogle.InitAdManagerAdView(AdManagerAdView);
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
                    toolBar.Title = GetString(Resource.String.Lbl_CreateAlbum);
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
                MAdapter = new AttachmentsAdapter(this) { AttachmentList = new ObservableCollection<Attachments>() };
                //var LayoutManager = new GridLayoutManager(this, 2);
                LayoutManager = new LinearLayoutManager(this, LinearLayoutManager.Horizontal, false);
                MRecycler.SetLayoutManager(LayoutManager);
                //MRecycler.AddItemDecoration(new GridSpacingItemDecoration(2, 2, true));
                MRecycler.SetAdapter(MAdapter);

                MRecycler.Visibility = ViewStates.Visible;

                // Add first image Default 
                //var attach = new Attachments
                //{
                //    Id = MAdapter.AttachmentList.Count + 1,
                //    TypeAttachment = "Default",
                //    FileSimple = "addImage",
                //    FileUrl = "addImage"
                //};

                //MAdapter.Add(attach);
                //MAdapter.NotifyDataSetChanged();
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
                        MAdapter.DeleteItemClick += MAdapterOnDeleteItemClick;
                        MAdapter.ItemClick += MAdapterOnItemClick;
                        TxtAdd.Click += TxtAddOnClick;
                        BtnCreateAlbum.Click += BtnCreateAlbum_Click;
                        break;
                    default:
                        MAdapter.DeleteItemClick -= MAdapterOnDeleteItemClick;
                        MAdapter.ItemClick -= MAdapterOnItemClick;
                        TxtAdd.Click -= TxtAddOnClick;
                        BtnCreateAlbum.Click -= BtnCreateAlbum_Click;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetStep()
        {
            try
            {
                switch (nStep)
                {
                    case 1:
                        llStep1.Visibility = ViewStates.Visible;
                        rlStep2.Visibility = ViewStates.Gone;
                        BtnCreateAlbum.Text = GetText(Resource.String.Lbl_CreateAlbum);
                        TxtAdd.Visibility = ViewStates.Gone;
                        break;
                    case 2:
                        llStep1.Visibility = ViewStates.Gone;
                        rlStep2.Visibility = ViewStates.Visible;
                        BtnCreateAlbum.Text = GetText(Resource.String.Lbl_AddPhotos);
                        TxtAdd.Visibility = ViewStates.Visible;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void BtnCreateAlbum_Click(object sender, EventArgs e)
        {
            try
            {
                switch (nStep)
                {
                    case 1:
                        nStep += 1;
                        SetStep();
                        break;
                    case 2:
                        switch ((int)Build.VERSION.SdkInt)
                        {
                            // Check if we're running on Android 5.0 or higher 
                            case < 23:
                                new IntentController(this).OpenIntentImageGallery(GetText(Resource.String.image), true);
                                break;
                            default:
                                {
                                    if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.Camera) == Permission.Granted && PermissionsController.CheckPermissionStorage(this))
                                    {
                                        new IntentController(this).OpenIntentImageGallery(GetText(Resource.String.image), true);
                                    }
                                    else
                                    {
                                        new PermissionsController(this).RequestPermission(108);
                                    }

                                    break;
                                }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        private void DestroyBasic()
        {
            try
            {
                AdsGoogle.LifecycleAdManagerAdView(AdManagerAdView, "Destroy");
                MAdapter = null!;
                MRecycler = null!;
                TxtNameAlbum = null!;
                TxtAdd = null!;

                AdManagerAdView = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void MAdapterOnDeleteItemClick(object sender, AttachmentsAdapterClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                switch (position)
                {
                    case >= 0:
                        {
                            var item = MAdapter.GetItem(position);
                            if (item != null)
                            {
                                MAdapter.Remove(item);
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

        private async void TxtAddOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
                else
                {
                    if (string.IsNullOrEmpty(TxtNameAlbum.Text.Replace(" ", "")))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_enter_name), ToastLength.Short);
                        return;
                    }

                    if (MAdapter.AttachmentList.Count <= 1)
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_select_Image), ToastLength.Short);
                    }
                    else
                    {
                        //Show a progress
                        AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading) + "...");

                        var list = MAdapter.AttachmentList.Where(a => a.TypeAttachment != "Default").ToList();
                        var (apiStatus, respond) = await RequestsAsync.Album.CreateAlbumAsync(TxtNameAlbum.Text.Replace(" ", ""), new ObservableCollection<Attachments>(list));
                        if (apiStatus == 200)
                        {
                            if (respond is CreateAlbumObject result)
                            {
                                if (result.Data.PhotoAlbum.Count > 0)
                                {
                                    AndHUD.Shared.Dismiss();
                                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CreatedSuccessfully), ToastLength.Short);

                                    //AlbumItem >> PostDataObject  
                                    Intent returnIntent = new Intent();
                                    returnIntent?.PutExtra("AlbumItem", JsonConvert.SerializeObject(result.Data));
                                    SetResult(Result.Ok, returnIntent);
                                    Finish();
                                }
                            }
                        }
                        else
                            Methods.DisplayAndHudErrorResult(this, respond);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                AndHUD.Shared.Dismiss();
            }
        }

        private void MAdapterOnItemClick(object sender, AttachmentsAdapterClickEventArgs adapterClickEvents)
        {
            try
            {
                var position = adapterClickEvents.Position;
                switch (position)
                {
                    case >= 0:
                        {
                            var item = MAdapter.GetItem(position);
                            switch (item)
                            {
                                case null:
                                    return;
                            }
                            if (item.TypeAttachment != "Default") return;
                            switch ((int)Build.VERSION.SdkInt)
                            {
                                // Check if we're running on Android 5.0 or higher 
                                case < 23:
                                    new IntentController(this).OpenIntentImageGallery(GetText(Resource.String.image), true);
                                    break;
                                default:
                                    {
                                        if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.Camera) == Permission.Granted && PermissionsController.CheckPermissionStorage(this))
                                        {
                                            new IntentController(this).OpenIntentImageGallery(GetText(Resource.String.image), true);
                                        }
                                        else
                                        {
                                            new PermissionsController(this).RequestPermission(108);
                                        }

                                        break;
                                    }
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

        #endregion

        #region Permissions && Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                if (requestCode == 500 && resultCode == Result.Ok)
                {
                    if (data.ClipData != null)
                    {
                        var mClipData = data.ClipData;
                        for (var i = 0; i < mClipData.ItemCount; i++)
                        {
                            var item = mClipData.GetItemAt(i);
                            Uri uri = item.Uri;
                            var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, uri);
                            PickiTonCompleteListener(filepath);
                        }
                    }
                    else
                    {
                        Uri uri = data.Data;
                        var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, uri);
                        PickiTonCompleteListener(filepath);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
                switch (requestCode)
                {
                    case 108 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        new IntentController(this).OpenIntentImageGallery(GetText(Resource.String.image), true);
                        break;
                    case 108:
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private async void PickiTonCompleteListener(string path)
        {
            //Dismiss dialog and return the path
            try
            {
                //  Check if it was a Drive/local/unknown provider file and display a Toast
                //if (wasDriveFile)
                //{
                //    // "Drive file was selected"
                //}
                //else if (wasUnknownProvider)
                //{
                //    // "File was selected from unknown provider"
                //}
                //else
                //{
                //    // "Local file was selected"
                //}

                //  Chick if it was successful
                var (check, info) = await WoWonderTools.CheckMimeTypesWithServer(path);
                if (check is false)
                {
                    if (info == "AdultImages")
                    {
                        //this file not allowed 
                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_Error_AdultImages), ToastLength.Short);

                        var dialog = new MaterialAlertDialogBuilder(this);
                        dialog.SetMessage(GetText(Resource.String.Lbl_Error_AdultImages));
                        dialog.SetPositiveButton(GetText(Resource.String.Lbl_IgnoreAndSend), (materialDialog, action) =>
                        {
                            try
                            {
                                var attach = new Attachments
                                {
                                    Id = MAdapter.AttachmentList.Count + 1,
                                    TypeAttachment = "postPhotos[]",
                                    FileSimple = path,
                                    FileUrl = path
                                };

                                MAdapter.Add(attach);
                                // 
                                EmptyPhotos.Visibility = ViewStates.Gone;
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                        dialog.SetNegativeButton(GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());

                        dialog.Show();
                    }
                    else
                    {
                        //this file not supported on the server , please select another file 
                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_ErrorFileNotSupported), ToastLength.Short);
                    }
                }
                else
                {
                    var attach = new Attachments
                    {
                        Id = MAdapter.AttachmentList.Count + 1,
                        TypeAttachment = "postPhotos[]",
                        FileSimple = path,
                        FileUrl = path
                    };

                    MAdapter.Add(attach);
                    // 
                    EmptyPhotos.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}