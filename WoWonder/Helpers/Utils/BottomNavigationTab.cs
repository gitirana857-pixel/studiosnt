using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Q.Rorbin.Badgeview;
using WoWonder.Activities.ReelsVideo;
using WoWonder.Activities.Tabbes;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Model;
using Exception = System.Exception;

namespace WoWonder.Helpers.Utils
{
    public class BottomNavigationTab : Object, View.IOnClickListener
    {
        private readonly TabbedMainActivity MainActivity;

        private LinearLayout Tab, HomeLayout, NotificationLayout, TrendingLayout, ReelLayout, MoreLayout;
        private ImageView ImageHome, ImageNotification, ImageTrending, ImageReel, ImageMore;
        private View SelectHome, SelectNotification, SelectTrending, SelectReel, SelectMore;

        private readonly Color UnSelectColor = Color.ParseColor("#9A9898");
        private static int OpenNewsFeedTab = 1;

        public BottomNavigationTab(TabbedMainActivity activity)
        {
            try
            {
                MainActivity = activity;

                Initialize();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void Initialize()
        {
            try
            {
                Tab = MainActivity.FindViewById<LinearLayout>(Resource.Id.bottomnavigationtab);

                HomeLayout = MainActivity.FindViewById<LinearLayout>(Resource.Id.llHome);
                ImageHome = MainActivity.FindViewById<ImageView>(Resource.Id.ivHome);
                SelectHome = MainActivity.FindViewById<View>(Resource.Id.selectHome);

                NotificationLayout = MainActivity.FindViewById<LinearLayout>(Resource.Id.llNotification);
                ImageNotification = MainActivity.FindViewById<ImageView>(Resource.Id.ivNotification);
                SelectNotification = MainActivity.FindViewById<View>(Resource.Id.selectNotification);

                TrendingLayout = MainActivity.FindViewById<LinearLayout>(Resource.Id.llTrending);
                ImageTrending = MainActivity.FindViewById<ImageView>(Resource.Id.ivTrending);
                SelectTrending = MainActivity.FindViewById<View>(Resource.Id.selectTrending);

                ReelLayout = MainActivity.FindViewById<LinearLayout>(Resource.Id.llReel);
                ImageReel = MainActivity.FindViewById<ImageView>(Resource.Id.ivReel);
                SelectReel = MainActivity.FindViewById<View>(Resource.Id.selectReel);

                MoreLayout = MainActivity.FindViewById<LinearLayout>(Resource.Id.llMore);
                ImageMore = MainActivity.FindViewById<ImageView>(Resource.Id.ivMore);
                SelectMore = MainActivity.FindViewById<View>(Resource.Id.selectMore);

                HomeLayout?.SetOnClickListener(this);
                NotificationLayout?.SetOnClickListener(this);
                TrendingLayout?.SetOnClickListener(this);
                ReelLayout?.SetOnClickListener(this);
                MoreLayout?.SetOnClickListener(this);

                float weightSum = 5;

                if (AppSettings.ReelsPosition is ReelsPosition.ToolBar or ReelsPosition.None)
                {
                    ReelLayout.Visibility = ViewStates.Gone;
                    weightSum--;
                }

                if (!AppSettings.ShowTrendingPage)
                {
                    TrendingLayout.Visibility = ViewStates.Gone;
                    weightSum--;
                }

                Tab.WeightSum = weightSum;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SelectItem(int index)
        {
            try
            {
                ImageHome.SetColorFilter(UnSelectColor);
                ImageNotification.SetColorFilter(UnSelectColor);
                ImageReel.SetColorFilter(UnSelectColor);
                ImageTrending.SetColorFilter(UnSelectColor);
                ImageMore.SetColorFilter(UnSelectColor);

                SelectHome.Visibility = ViewStates.Gone;
                SelectNotification.Visibility = ViewStates.Gone;
                SelectReel.Visibility = ViewStates.Gone;
                SelectTrending.Visibility = ViewStates.Gone;
                SelectMore.Visibility = ViewStates.Gone;

                switch (index)
                {
                    //Home
                    case 0:
                        {
                            ImageHome.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
                            SelectHome.Visibility = ViewStates.Visible;

                            MainActivity.ViewPager.SetCurrentItem(0, false);

                            if (WoWonderTools.CanAddPost())
                                MainActivity.FloatingActionButton.Visibility = AppSettings.ShowAddPostOnNewsFeed ? ViewStates.Visible : ViewStates.Gone;
                            else
                                MainActivity.FloatingActionButton.Visibility = ViewStates.Gone;

                            AdsGoogle.Ad_Interstitial(MainActivity);
                            break;
                        }
                    //Notification
                    case 1:
                        ImageNotification.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
                        SelectNotification.Visibility = ViewStates.Visible;
                        MainActivity.ViewPager.SetCurrentItem(1, false);

                        MainActivity.FloatingActionButton.Visibility = ViewStates.Gone;

                        AdsGoogle.Ad_AppOpenManager(MainActivity);
                        break;
                    //Trending
                    case 2:
                        ImageTrending.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
                        SelectTrending.Visibility = ViewStates.Visible;
                        MainActivity.ViewPager.SetCurrentItem(2, false);

                        MainActivity.FloatingActionButton.Visibility = ViewStates.Gone;

                        AdsGoogle.Ad_RewardedVideo(MainActivity);

                        MainActivity.InAppReview();
                        break;
                    //More
                    case 3:
                        ImageMore.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
                        SelectMore.Visibility = ViewStates.Visible;
                        MainActivity.ViewPager.SetCurrentItem(3, false);

                        MainActivity.FloatingActionButton.Visibility = ViewStates.Gone;

                        AdsGoogle.Ad_RewardedInterstitial(MainActivity);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ShowBadge(int id, int count, bool showBadge)
        {
            try
            {
                if (id < 0) return;

                if (showBadge)
                {
                    if (id == 0)
                        ShowOrHideBadgeViewIcon(MainActivity, HomeLayout, count, true);
                    else if (id == 1)
                        ShowOrHideBadgeViewIcon(MainActivity, NotificationLayout, count, true);
                    else if (id == 2)
                        ShowOrHideBadgeViewIcon(MainActivity, TrendingLayout, count, true);
                    else if (id == 3) ShowOrHideBadgeViewIcon(MainActivity, MoreLayout, count, true);
                }
                else if (id == 0)
                {
                    ShowOrHideBadgeViewIcon(MainActivity, HomeLayout);
                }
                else if (id == 1)
                {
                    ShowOrHideBadgeViewIcon(MainActivity, NotificationLayout);
                }
                else if (id == 2)
                {
                    ShowOrHideBadgeViewIcon(MainActivity, TrendingLayout);
                }
                else if (id == 3)
                {
                    ShowOrHideBadgeViewIcon(MainActivity, MoreLayout);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        private QBadgeView BadgeNotification, BadgeTrending;
        private void ShowOrHideBadgeViewIcon(Activity mainActivity, LinearLayout linearLayoutImage, int count = 0, bool show = false)
        {
            try
            {
                mainActivity?.RunOnUiThread(() =>
                {
                    try
                    {
                        if (show)
                        {
                            if (linearLayoutImage != null)
                            {
                                if (linearLayoutImage.Id == NotificationLayout.Id)
                                {
                                    BadgeNotification = new QBadgeView(mainActivity);
                                    int gravity = (int)(GravityFlags.End | GravityFlags.Top);
                                    BadgeNotification.BindTarget(linearLayoutImage);
                                    BadgeNotification.SetBadgeNumber(count);
                                    BadgeNotification.SetBadgeGravity(gravity);
                                    BadgeNotification.SetBadgeBackgroundColor(Color.ParseColor(AppSettings.MainColor));
                                    BadgeNotification.SetGravityOffset(10, true);
                                }
                                else if (linearLayoutImage.Id == TrendingLayout.Id)
                                {
                                    BadgeTrending = new QBadgeView(mainActivity);
                                    int gravity = (int)(GravityFlags.End | GravityFlags.Top);
                                    BadgeTrending.BindTarget(linearLayoutImage);
                                    BadgeTrending.SetBadgeNumber(count);
                                    BadgeTrending.SetBadgeGravity(gravity);
                                    BadgeTrending.SetBadgeBackgroundColor(Color.ParseColor(AppSettings.MainColor));
                                    BadgeTrending.SetGravityOffset(10, true);
                                }
                            }
                        }
                        else
                        {
                            if (linearLayoutImage?.Id == NotificationLayout.Id)
                                BadgeNotification?.BindTarget(linearLayoutImage).Hide(true);
                            else if (linearLayoutImage?.Id == TrendingLayout.Id)
                                BadgeTrending?.BindTarget(linearLayoutImage).Hide(true);
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

        public void OnClick(View v)
        {
            try
            {
                if (v.Id == HomeLayout.Id && OpenNewsFeedTab == 2)
                {
                    OpenNewsFeedTab = 1;
                    MainActivity?.NewsFeedTab?.MainRecyclerView?.ScrollToPosition(0);
                }
                else if (v.Id == HomeLayout.Id)
                {
                    OpenNewsFeedTab++;
                    SelectItem(0);
                }
                else if (v.Id == NotificationLayout?.Id)
                {
                    SelectItem(1);

                    MainActivity?.NewsFeedTab?.MainRecyclerView?.StopVideo();
                    OpenNewsFeedTab = 1;


                    ShowBadge(1, 0, false);

                }
                else if (v.Id == ReelLayout?.Id)
                {
                    MainActivity?.NewsFeedTab?.MainRecyclerView?.StopVideo();
                    OpenNewsFeedTab = 1;

                    var intent = new Intent(MainActivity, typeof(ReelsVideoDetailsActivity));
                    intent.PutExtra("Type", "VideoReels");
                    intent.PutExtra("VideosCount", ListUtils.VideoReelsList.Count);
                    //intent.PutExtra("DataItem", JsonConvert.SerializeObject(ListUtils.VideoReelsList.ToList()));
                    MainActivity.StartActivity(intent);
                }
                else if (v.Id == TrendingLayout?.Id && AppSettings.ShowTrendingPage)
                {
                    SelectItem(2);
                    MainActivity?.NewsFeedTab?.MainRecyclerView?.StopVideo();
                    OpenNewsFeedTab = 1;


                    ShowBadge(2, 0, false);

                }
                else if (v.Id == MoreLayout?.Id)
                {
                    SelectItem(3);

                    MainActivity?.NewsFeedTab?.MainRecyclerView?.StopVideo();
                    OpenNewsFeedTab = 1;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}