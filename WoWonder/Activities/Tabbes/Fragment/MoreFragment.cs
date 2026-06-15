using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WoWonder.Activities.Address;
using WoWonder.Activities.Advertise;
using WoWonder.Activities.Album;
using WoWonder.Activities.Articles;
using WoWonder.Activities.Boosted;
using WoWonder.Activities.CommonThings;
using WoWonder.Activities.Communities.Groups;
using WoWonder.Activities.Communities.Pages;
using WoWonder.Activities.Contacts;
using WoWonder.Activities.Covid19;
using WoWonder.Activities.Events;
using WoWonder.Activities.Fundings;
using WoWonder.Activities.Games;
using WoWonder.Activities.Jobs;
using WoWonder.Activities.Live.Page;
using WoWonder.Activities.Market;
using WoWonder.Activities.Memories;
using WoWonder.Activities.Movies;
using WoWonder.Activities.MyPhoto;
using WoWonder.Activities.MyProfile;
using WoWonder.Activities.MyVideo;
using WoWonder.Activities.NativePost.Pages;
using WoWonder.Activities.NearBy;
using WoWonder.Activities.Offers;
using WoWonder.Activities.Pokes;
using WoWonder.Activities.PopularPosts;
using WoWonder.Activities.SettingsPreferences.General;
using WoWonder.Activities.SettingsPreferences.InvitationLinks;
using WoWonder.Activities.SettingsPreferences.MyInformation;
using WoWonder.Activities.SettingsPreferences.Notification;
using WoWonder.Activities.SettingsPreferences.Privacy;
using WoWonder.Activities.SettingsPreferences.Support;
using WoWonder.Activities.SettingsPreferences.TellFriend;
using WoWonder.Activities.Tabbes.Adapters;
using WoWonder.Activities.Upgrade;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using Exception = System.Exception;

namespace WoWonder.Activities.Tabbes.Fragment
{
    public class MoreFragment : AndroidX.Fragment.App.Fragment
    {
        #region  Variables Basic

        public MoreSectionAdapter MoreSectionAdapter1, MoreSectionAdapter2;
        private RecyclerView MRecycler1, MRecycler2;
        private LinearLayout GoProLayout;
        private RelativeLayout ProfileLayout;
        public ImageView ProfileImage;
        private TextView ProfileName;

        #endregion

        #region General

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                View view = inflater.Inflate(Resource.Layout.TMoreLayout, container, false);
                return view;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            {
                base.OnViewCreated(view, savedInstanceState);

                InitComponent(view);
                SetRecyclerViewAdapters();
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
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void OnResume()
        {
            try
            {
                base.OnResume();
                LoadData();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                ProfileLayout = (RelativeLayout)view.FindViewById(Resource.Id.profileLayout);
                ProfileImage = (ImageView)view.FindViewById(Resource.Id.image);
                ProfileName = (TextView)view.FindViewById(Resource.Id.tv_name);
                ProfileLayout.Click += ProfileLayoutOnClick;

                GoProLayout = (LinearLayout)view.FindViewById(Resource.Id.GoProLayout);
                GoProLayout.Click += GoProLayoutOnClick;

                MRecycler1 = (RecyclerView)view.FindViewById(Resource.Id.recyler1);
                MRecycler2 = (RecyclerView)view.FindViewById(Resource.Id.recyler2);
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
                MRecycler1.NestedScrollingEnabled = true;

                if (AppSettings.MoreTheme == MoreTheme.Grid)
                {
                    MoreSectionAdapter1 = new MoreSectionAdapter(Activity, StyleRowMore.Grid);

                    var layoutManager = new GridLayoutManager(Activity, 4);
                    var countListFirstRow = MoreSectionAdapter1.SectionList.Where(q => q.StyleRow == StyleRowMore.Grid).ToList().Count;

                    layoutManager.SetSpanSizeLookup(new MySpanSizeLookup2(countListFirstRow, 1, 4));//20, 1, 4
                    MRecycler1.SetLayoutManager(layoutManager);
                }
                else if (AppSettings.MoreTheme == MoreTheme.Card)
                {
                    MoreSectionAdapter1 = new MoreSectionAdapter(Activity, StyleRowMore.Card);

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
                    {
                        var layoutManager = new SpannedGridLayoutManager(new MySpannedGridLayoutManager(), 3, 1.20f);
                        MRecycler1.AddItemDecoration(new GridSpacingItemDecoration(3, 15, true));
                        MRecycler1.SetLayoutManager(layoutManager);
                    }
                    else
                    {
                        var layoutManager = new SpannedGridLayoutManager(new MySpannedGridLayoutManager(), 3, 1.12f);
                        MRecycler1.AddItemDecoration(new GridSpacingItemDecoration(3, 15, true));
                        MRecycler1.SetLayoutManager(layoutManager);
                    }
                }

                MoreSectionAdapter1.ItemClick += MoreSectionAdapter1OnItemClick;
                MRecycler1.SetAdapter(MoreSectionAdapter1);
                MRecycler1.HasFixedSize = true;
                MRecycler1.SetItemViewCacheSize(50);
                MRecycler1.GetLayoutManager().ItemPrefetchEnabled = true;

                //=================================


                MRecycler2.NestedScrollingEnabled = true;

                MoreSectionAdapter2 = new MoreSectionAdapter(Activity, StyleRowMore.Row);
                MoreSectionAdapter2.ItemClick += MoreSectionAdapter2OnItemClick;

                MRecycler2.SetLayoutManager(new LinearLayoutManager(Activity));
                MRecycler2.SetAdapter(MoreSectionAdapter2);
                MRecycler2.HasFixedSize = true;
                MRecycler2.SetItemViewCacheSize(50);
                MRecycler2.GetLayoutManager().ItemPrefetchEnabled = true;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private class MySpannedGridLayoutManager : SpannedGridLayoutManager.IGridSpanLookup
        {
            private bool Big1 = true;
            private bool Big2 = true;

            public SpannedGridLayoutManager.SpanInfo GetSpanInfo(int position)
            {
                try
                {
                    // Conditions for 2x2 items 
                    if (position == 0)
                    {
                        return new SpannedGridLayoutManager.SpanInfo(3, 1);
                    }

                    if (position % 2 == 0)
                    {
                        if (Big1)
                        {
                            Big1 = false;
                            return new SpannedGridLayoutManager.SpanInfo(2, 1);
                        }

                        Big1 = true;
                        return new SpannedGridLayoutManager.SpanInfo(1, 1);
                    }

                    if (Big2)
                    {
                        Big2 = false;
                        return new SpannedGridLayoutManager.SpanInfo(1, 1);
                    }

                    Big2 = true;
                    return new SpannedGridLayoutManager.SpanInfo(2, 1);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                    return new SpannedGridLayoutManager.SpanInfo(2, 1);
                }
            }
        }

        #endregion

        #region Event

        private void GoProLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(Context, typeof(GoProActivity));
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ProfileLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(Context, typeof(MyProfileActivity));
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MoreSectionAdapter1OnItemClick(object sender, MoreSectionAdapterClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                var item = MoreSectionAdapter1?.GetItem(position);
                if (item != null)
                {
                    switch (item.Id)
                    {
                        // My Profile
                        case 1:
                            {
                                var intent = new Intent(Context, typeof(MyProfileActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Messages
                        case 2:
                            {
                                Methods.App.OpenAppByPackageName(Context, AppSettings.MessengerPackageName, "OpenChatApp");
                                break;
                            }
                        // Contacts
                        case 3:
                            {
                                var intent = new Intent(Context, typeof(MyContactsActivity));
                                intent.PutExtra("ContactsType", "Following");
                                intent.PutExtra("UserId", UserDetails.UserId);
                                StartActivity(intent);
                                break;
                            }
                        // Pokes
                        case 4:
                            {
                                var intent = new Intent(Context, typeof(PokesActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Album
                        case 5:
                            {
                                var intent = new Intent(Context, typeof(MyAlbumActivity));
                                StartActivity(intent);
                                break;
                            }
                        // MyImages
                        case 6:
                            {
                                var intent = new Intent(Context, typeof(MyPhotosActivity));
                                StartActivity(intent);
                                break;
                            }
                        // MyVideos
                        case 7:
                            {
                                var intent = new Intent(Context, typeof(MyVideoActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Saved Posts
                        case 8:
                            {
                                var intent = new Intent(Context, typeof(SavedPostsActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Groups
                        case 9:
                            {
                                var intent = new Intent(Context, typeof(GroupsActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Pages
                        case 10:
                            {
                                var intent = new Intent(Context, typeof(PagesActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Blogs
                        case 11:
                            StartActivity(new Intent(Context, typeof(ArticlesActivity)));
                            break;
                        // Market
                        case 12:
                            StartActivity(new Intent(Context, typeof(TabbedMarketActivity)));
                            break;
                        // Boosted Posts & Pages
                        case 13:
                            {
                                var intent = new Intent(Context, typeof(BoostedActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Popular Posts
                        case 14:
                            {
                                var intent = new Intent(Context, typeof(PopularPostsActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Events
                        case 15:
                            {
                                var intent = new Intent(Context, typeof(EventMainActivity));
                                StartActivity(intent);
                                break;
                            }
                        // NearBy Find Friends
                        case 16:
                            {
                                var intent = new Intent(Context, typeof(PeopleNearByActivity));
                                StartActivity(intent);
                                break;
                            }
                        //Offers
                        case 17:
                            {
                                var intent = new Intent(Context, typeof(OffersActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Movies
                        case 18:
                            {
                                var intent = new Intent(Context, typeof(MoviesActivity));
                                StartActivity(intent);
                                break;
                            }
                        // jobs
                        case 19:
                            {
                                var intent = new Intent(Context, typeof(JobsActivity));
                                StartActivity(intent);
                                break;
                            }
                        // common things
                        case 20:
                            {
                                var intent = new Intent(Context, typeof(CommonThingsActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Memories
                        case 21:
                            {
                                var intent = new Intent(Context, typeof(MemoriesActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Funding
                        case 22:
                            {
                                var intent = new Intent(Context, typeof(FundingActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Games
                        case 23:
                            {
                                var intent = new Intent(Context, typeof(GamesActivity));
                                StartActivity(intent);
                                break;
                            }
                        // CoronaVirus
                        case 24:
                            {
                                var intent = new Intent(Context, typeof(Covid19Activity));
                                StartActivity(intent);
                                break;
                            }
                        // Live
                        case 25:
                            {
                                var intent = new Intent(Context, typeof(LiveActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Advertising
                        case 26:
                            {
                                var intent = new Intent(Context, typeof(MyAdvertiseActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Go pro
                        case 27:
                            {
                                var intent = new Intent(Context, typeof(GoProActivity));
                                StartActivity(intent);
                                break;
                            }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MoreSectionAdapter2OnItemClick(object sender, MoreSectionAdapterClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                var item = MoreSectionAdapter2?.GetItem(position);
                if (item != null)
                {
                    switch (item.Id)
                    {
                        // General Account
                        case 100:
                            {
                                var intent = new Intent(Context, typeof(GeneralAccountActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Privacy
                        case 101:
                            {
                                var intent = new Intent(Context, typeof(PrivacyActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Notification
                        case 102:
                            {
                                var intent = new Intent(Context, typeof(MessegeNotificationActivity));
                                StartActivity(intent);
                                break;
                            }
                        // InvitationLinks
                        case 103:
                            {
                                var intent = new Intent(Context, typeof(InvitationLinksActivity));
                                StartActivity(intent);
                                break;
                            }
                        // MyInformation
                        case 104:
                            {
                                var intent = new Intent(Context, typeof(MyInformationActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Address
                        case 105:
                            {
                                var intent = new Intent(Context, typeof(AddressActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Tell Friends
                        case 106:
                            {
                                var intent = new Intent(Context, typeof(TellFriendActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Help & Support
                        case 107:
                            {
                                var intent = new Intent(Context, typeof(SupportActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Logout
                        case 108:
                            {
                                var dialog = new MaterialAlertDialogBuilder(Context);//.ba(WoWonderTools.IsTabDark() ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                                dialog.SetTitle(Resource.String.Lbl_Warning);
                                dialog.SetMessage(Context.GetText(Resource.String.Lbl_Are_you_logout));
                                dialog.SetPositiveButton(Context.GetText(Resource.String.Lbl_Ok), (o, args) =>
                                {
                                    try
                                    {
                                        ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_You_will_be_logged), ToastLength.Long);
                                        ApiRequest.Logout(Activity);
                                    }
                                    catch (Exception exception)
                                    {
                                        Methods.DisplayReportResultTrack(exception);
                                    }
                                });
                                dialog.SetNegativeButton(Context.GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());
                                dialog.Show();
                                break;
                            }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        private void LoadData()
        {
            try
            {
                if (Methods.CheckConnectivity())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.Get_MyProfileData_Api(Activity) });

                var myProfile = ListUtils.MyProfileList?.FirstOrDefault();
                GlideImageLoader.LoadImage(Activity, myProfile != null ? myProfile.Avatar : UserDetails.Avatar, ProfileImage, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);
                ProfileName.Text = WoWonderTools.GetNameFinal(myProfile);

                GoProLayout.Visibility = AppSettings.ShowGoPro ? ViewStates.Visible : ViewStates.Gone;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}