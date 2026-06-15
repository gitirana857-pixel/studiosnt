using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using AT.Markushi.UI;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WoWonder.Activities.Base;
using WoWonder.Activities.MyProfile;
using WoWonder.Activities.Suggested.User;
using WoWonder.Activities.Tabbes;
using WoWonder.Activities.WalkTroutPage.Adapters;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.OneSignalNotif;
using WoWonder.SQLite;
using WoWonder.StickersView;
using WoWonderClient.Requests;
using Object = Java.Lang.Object;
using ViewPager = AndroidX.ViewPager.Widget.ViewPager;

namespace WoWonder.Activities.WalkTroutPage
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class WalkTroutActivity : BaseActivity
    {
        #region Variables Basic

        private static int MaxStep = 4;

        private ViewPager ViewPager;
        private WalkTroutPagerAdapter MAdapter;
        private CircleButton BtnNext;
        private TextView BtnSkip;
        private List<Classes.ModelsWalkTroutPager> ListPage = new List<Classes.ModelsWalkTroutPager>();

        private string Caller = "";
        private RequestBuilder FullGlideRequestBuilder;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);
                Methods.App.FullScreenApp(this, true);

                // Create your application here
                SetContentView(Resource.Layout.WalkTroutLayout);

                Caller = Intent?.GetStringExtra("class") ?? "";

                ListPage = new List<Classes.ModelsWalkTroutPager>
                {
                    new Classes.ModelsWalkTroutPager
                    {
                        Title = GetText(Resource.String.Lbl_Title_page1),
                        Description = GetText(Resource.String.Lbl_Description_page1),
                        Image = Resource.Drawable.icon_WalkTroutPage1,
                    },
                    new Classes.ModelsWalkTroutPager
                    {
                        Title = GetText(Resource.String.Lbl_Title_page2),
                        Description = GetText(Resource.String.Lbl_Description_page2),
                        Image = Resource.Drawable.icon_WalkTroutPage2
                    },
                    new Classes.ModelsWalkTroutPager
                    {
                        Title = GetText(Resource.String.Lbl_Title_page3),
                        Description = GetText(Resource.String.Lbl_Description_page3),
                        Image = Resource.Drawable.icon_WalkTroutPage4
                    },
                    new Classes.ModelsWalkTroutPager
                    {
                        Title = GetText(Resource.String.Lbl_Title_page4),
                        Description = GetText(Resource.String.Lbl_Description_page4),
                        Image = Resource.Drawable.icon_WalkTroutPage3
                    }
                };

                MaxStep = ListPage.Count;

                //Get Value And Set Toolbar
                InitComponent();
                Pressed(0);
                LoadData();
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

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                ViewPager = (ViewPager)FindViewById(Resource.Id.view_pager);
                BtnNext = (CircleButton)FindViewById(Resource.Id.btn_next);
                BtnSkip = (TextView)FindViewById(Resource.Id.btn_skip);

                BtnNext.Tag = "step";

                // adding bottom dots
                BottomProgressDots(0);

                MAdapter = new WalkTroutPagerAdapter(this, ListPage);
                ViewPager.Adapter = MAdapter;
                //ViewPager.SetPageTransformer(false, new FadePageTransformer());
                ViewPager.Adapter.NotifyDataSetChanged();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadData()
        {
            try
            {
                //OneSignal Notification  
                //====================================== 
                if (string.IsNullOrEmpty(UserDetails.DeviceId))
                    OneSignalNotification.Instance.RegisterNotificationDevice(this);

                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.Get_MyProfileData_Api(this) });

                FullGlideRequestBuilder = Glide.With(this).AsDrawable().SetDiskCacheStrategy(DiskCacheStrategy.Automatic).SkipMemoryCache(true).Override(200);

                List<string> stickerList = new List<string>();
                stickerList.AddRange(StickersModel.Locally.StickerList1);
                stickerList.AddRange(StickersModel.Locally.StickerList2);
                stickerList.AddRange(StickersModel.Locally.StickerList3);
                stickerList.AddRange(StickersModel.Locally.StickerList4);
                stickerList.AddRange(StickersModel.Locally.StickerList5);
                stickerList.AddRange(StickersModel.Locally.StickerList6);
                stickerList.AddRange(StickersModel.Locally.StickerList7);
                stickerList.AddRange(StickersModel.Locally.StickerList8);
                stickerList.AddRange(StickersModel.Locally.StickerList9);
                stickerList.AddRange(StickersModel.Locally.StickerList10);
                stickerList.AddRange(StickersModel.Locally.StickerList11);

                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        foreach (var item in stickerList)
                        {
                            FullGlideRequestBuilder.Load(item).Preload();
                        }
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

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    BtnSkip.Click += BtnSkipOnClick;
                    BtnNext.Click += BtnNextOnClick;
                    ViewPager.PageSelected += ViewPagerOnPageSelected;
                }
                else
                {
                    BtnSkip.Click -= BtnSkipOnClick;
                    BtnNext.Click -= BtnNextOnClick;
                    ViewPager.PageSelected -= ViewPagerOnPageSelected;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void BtnDoneOnClick()
        {
            try
            {
                int current = ViewPager.CurrentItem + 1;
                if (current < MaxStep)
                {
                    // move to next screen
                    ViewPager.CurrentItem = current;
                }
                else
                {
                    if (BtnNext.Tag?.ToString() == "step")
                    {
                        BtnNext.Tag = "done";
                    }
                    else
                    {
                        if (Caller.Contains("register"))
                        {
                            if (AppSettings.AddAllInfoPorfileAfterRegister)
                            {
                                Intent newIntent = new Intent(this, typeof(AddAllInfoProfileActivity));
                                StartActivity(newIntent);
                            }
                            else if (AppSettings.ShowSuggestedUsersOnRegister)
                            {
                                Intent newIntent = new Intent(this, typeof(SuggestionsUsersActivity));
                                newIntent.PutExtra("class", "register");
                                StartActivity(newIntent);
                            }
                            else
                            {
                                StartActivity(new Intent(this, typeof(TabbedMainActivity)));
                            }
                        }
                        else
                        {
                            StartActivity(new Intent(this, typeof(TabbedMainActivity)));
                        }

                        Finish();
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnNextOnClick(object sender, EventArgs e)
        {
            try
            {
                int current = ViewPager.CurrentItem + 1;
                if (current < MaxStep)
                {
                    // move to next screen
                    ViewPager.CurrentItem = current;
                }
                else
                {
                    BtnDoneOnClick();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnSkipOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Caller.Contains("register"))
                {
                    if (AppSettings.ShowSuggestedUsersOnRegister)
                    {
                        Intent newIntent = new Intent(this, typeof(SuggestionsUsersActivity));
                        newIntent.PutExtra("class", "register");
                        StartActivity(newIntent);
                    }
                    else
                    {
                        StartActivity(new Intent(this, typeof(TabbedMainActivity)));
                    }
                }
                else
                {
                    StartActivity(new Intent(this, typeof(TabbedMainActivity)));
                }
                Finish();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ViewPagerOnPageSelected(object sender, ViewPager.PageSelectedEventArgs e)
        {
            try
            {
                BottomProgressDots(e.Position);
                Pressed(e.Position);

                if (e.Position == ListPage.Count - 1)
                {
                    BtnDoneOnClick();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Permissions 

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
                switch (requestCode)
                {
                    case 105 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                if (Methods.CheckConnectivity())
                                {
                                    Dictionary<string, string> dictionaryProfile = new Dictionary<string, string>();

                                    var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                                    if (dataUser != null)
                                    {
                                        dictionaryProfile = new Dictionary<string, string>();

                                        dataUser.Lat = UserDetails.Lat;
                                        dataUser.Lat = UserDetails.Lat;

                                        var sqLiteDatabase = new SqLiteDatabase();
                                        sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);
                                    }

                                    dictionaryProfile.Add("lat", UserDetails.Lat);
                                    dictionaryProfile.Add("lng", UserDetails.Lng);

                                    if (Methods.CheckConnectivity())
                                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dictionaryProfile) });
                                }
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                        break;
                    case 105:
                        //ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                        break;
                    case 108 when grantResults.Length > 0 && grantResults[0] == Permission.Granted && PermissionsController.CheckPermissionStorage(this, "file"):
                        Methods.Path.Chack_MyFolder();
                        break;
                    case 108:
                        //ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                        break;

                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private void Pressed(int count)
        {
            try
            {
                if (count == 0) //Location
                {
                    if ((int)Build.VERSION.SdkInt > 23)
                        new PermissionsController(this).RequestPermission(105);
                }
                else if (count == 1) //Contacts
                {
                    if ((int)Build.VERSION.SdkInt > 23 && AppSettings.InvitationSystem)
                        new PermissionsController(this).RequestPermission(101);
                }
                else if (count == 2) // Record
                {
                    if ((int)Build.VERSION.SdkInt > 23)
                        new PermissionsController(this).RequestPermission(102);
                }
                else if (count == 3) // Storage & Camera
                {
                    if ((int)Build.VERSION.SdkInt > 23)
                        new PermissionsController(this).RequestPermission(108, "file");
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void BottomProgressDots(int currentIndex)
        {
            try
            {
                ProgressBar stepBar = (ProgressBar)FindViewById(Resource.Id.progressBar);
                if (currentIndex == 0)
                {
                    stepBar.Progress = 25;
                }
                else if (currentIndex == 1)
                {
                    stepBar.Progress = 50;
                }
                else if (currentIndex == 2)
                {
                    stepBar.Progress = 75;
                }
                else if (currentIndex == 3)
                {
                    stepBar.Progress = 100;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public class FadePageTransformer : Object, ViewPager.IPageTransformer
        {
            public void TransformPage(View page, float position)
            {
                try
                {
                    if (position <= -1.0F || position >= 1.0F)
                    {
                        page.TranslationX = page.Width * position;
                        page.Alpha = 0.0F;
                    }
                    else if (position == 0.0F)
                    {
                        page.TranslationX = page.Width * position;
                        page.Alpha = 1.0F;
                    }
                    else
                    {
                        // position is between -1.0F & 0.0F OR 0.0F & 1.0F
                        page.TranslationX = page.Width * -position;
                        page.Alpha = 1.0F - Math.Abs(position);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }
}