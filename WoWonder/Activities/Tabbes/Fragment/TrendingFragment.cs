using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WoWonder.Activities.Articles;
using WoWonder.Activities.Hashtag;
using WoWonder.Activities.NativePost.Pages;
using WoWonder.Activities.Tabbes.Adapters;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.ShimmerUtils;
using WoWonder.Helpers.Utils;

namespace WoWonder.Activities.Tabbes.Fragment
{
    public class TrendingFragment : AndroidX.Fragment.App.Fragment
    {
        #region  Variables Basic

        private TabbedMainActivity GlobalContext;
        private RecyclerView MRecycler;
        public TrendingAdapter MAdapter;
        private LinearLayoutManager LayoutManager;
        private ViewStub ShimmerPageLayout;
        private View InflatedShimmer;
        private TemplateShimmerInflater ShimmerInflater;
        private bool MIsVisibleToUser;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            GlobalContext = TabbedMainActivity.GetInstance();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                View view = inflater.Inflate(Resource.Layout.TTrendingLayout, container, false);
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
                InitShimmer(view);
                SetRecyclerViewAdapters();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void SetMenuVisibility(bool menuVisible)
        {
            try
            {
                base.SetMenuVisibility(menuVisible);
                MIsVisibleToUser = menuVisible;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnResume()
        {
            try
            {
                base.OnResume();

                if (IsResumed && MIsVisibleToUser)
                {
                    Task.Factory.StartNew(StartApiService);
                }
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

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                MRecycler = (RecyclerView)view.FindViewById(Resource.Id.recyler);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitShimmer(View view)
        {
            try
            {
                ShimmerPageLayout = view.FindViewById<ViewStub>(Resource.Id.viewStubShimmer);
                InflatedShimmer ??= ShimmerPageLayout.Inflate();

                ShimmerInflater = new TemplateShimmerInflater();
                ShimmerInflater.InflateLayout(Activity, InflatedShimmer, ShimmerTemplateStyle.PostTemplate);
                ShimmerInflater.Show();
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
                LayoutManager = new LinearLayoutManager(Activity);
                MAdapter = new TrendingAdapter(Activity)
                {
                    TrendingList = new ObservableCollection<Classes.TrendingClass>()
                };
                MAdapter.ItemClick += MAdapterOnItemClick;
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(50);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                MRecycler.SetAdapter(MAdapter);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event

        private void MAdapterOnItemClick(object sender, TrendingAdapterClickEventArgs e)
        {
            try
            {
                var item = MAdapter.GetItem(e.Position);
                switch (item?.Type)
                {
                    case Classes.ItemType.HashTag:
                        {
                            string id = item.HashTags.Hash.Replace("#", "").Replace("_", " ");
                            string tag = item.HashTags?.Tag?.Replace("#", "");
                            var intent = new Intent(Activity, typeof(HashTagPostsActivity));
                            intent.PutExtra("Id", id);
                            intent.PutExtra("Tag", tag);
                            Activity.StartActivity(intent);
                            break;
                        }
                    case Classes.ItemType.Section when item.SectionType == Classes.ItemType.HashTag:
                        {
                            var intent = new Intent(Activity, typeof(HashTagActivity));
                            Activity.StartActivity(intent);
                            break;
                        }
                    case Classes.ItemType.LastBlogs:
                        {
                            var intent = new Intent(Activity, typeof(ArticlesViewActivity));
                            intent.PutExtra("Id", item.LastBlogs.Id);
                            intent.PutExtra("ArticleObject", JsonConvert.SerializeObject(item.LastBlogs));
                            Activity.StartActivity(intent);
                            break;
                        }
                    case Classes.ItemType.Section when item.SectionType == Classes.ItemType.LastBlogs:
                        {
                            var intent = new Intent(Activity, typeof(ArticlesActivity));
                            Activity.StartActivity(intent);
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

        #region Weather & Blogs 

        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { ApiRequest.GetMyGroups, ApiRequest.GetMyPages, ApiRequest.GetLastArticles, GetWeatherApi });
        }

        private async Task GetWeatherApi()
        {
            if (AppSettings.ShowWeather && Methods.CheckConnectivity())
            {
                GetWeatherObject respond = await ApiRequest.GetWeather();
                if (respond != null)
                {
                    var checkList = MAdapter.TrendingList.FirstOrDefault(q => q.Type == Classes.ItemType.Weather);
                    switch (checkList)
                    {
                        case null:
                            {
                                var weather = new Classes.TrendingClass
                                {
                                    Id = 600,
                                    Weather = respond,
                                    Type = Classes.ItemType.Weather
                                };

                                MAdapter.TrendingList.Add(weather);
                                MAdapter.TrendingList.Add(new Classes.TrendingClass
                                {
                                    Type = Classes.ItemType.Divider
                                });
                                break;
                            }
                        default:
                            checkList.Weather = respond;
                            break;
                    }
                }
            }

            await GetExchangeCurrencyApi();

            Activity?.RunOnUiThread(ShowEmptyPage);
        }

        private async Task GetExchangeCurrencyApi()
        {
            if (AppSettings.ShowExchangeCurrency && Methods.CheckConnectivity())
            {
                var (apiStatus, respond) = await ApiRequest.GetExchangeCurrencyAsync();
                if (apiStatus != 200 || respond is not Classes.ExchangeCurrencyObject result || result.Rates == null)
                {
                    switch (AppSettings.SetApisReportMode)
                    {
                        case true when apiStatus != 400 && respond is Classes.ExErrorObject error:
                            Methods.DialogPopup.InvokeAndShowDialog(Activity, "ReportMode", error?.Description, "Close");
                            break;
                    }
                }
                else
                {
                    var checkList = MAdapter.TrendingList.FirstOrDefault(q => q.Type == Classes.ItemType.ExchangeCurrency);
                    switch (checkList)
                    {
                        case null:
                            {
                                var exchangeCurrency = new Classes.TrendingClass
                                {
                                    Id = 2013,
                                    ExchangeCurrency = result,
                                    Type = Classes.ItemType.ExchangeCurrency
                                };

                                MAdapter.TrendingList.Add(exchangeCurrency);
                                MAdapter.TrendingList.Add(new Classes.TrendingClass
                                {
                                    Type = Classes.ItemType.Divider
                                });
                                break;
                            }
                        default:
                            checkList.ExchangeCurrency = result;
                            break;
                    }
                }
            }
        }

        private void ShowEmptyPage()
        {
            try
            {
                ShimmerInflater?.Hide();

                var respondListShortcuts = ListUtils.ShortCutsList?.Count;
                if (respondListShortcuts > 0 && AppSettings.ShowShortcuts)
                {
                    var checkList = GlobalContext.TrendingTab.MAdapter.TrendingList.FirstOrDefault(q => q.Type == Classes.ItemType.Shortcuts);
                    if (checkList == null)
                    {
                        var shortcuts = new Classes.TrendingClass
                        {
                            Id = 700,
                            ShortcutsList = new List<Classes.ShortCuts>(ListUtils.ShortCutsList),
                            Type = Classes.ItemType.Shortcuts
                        };

                        //shortcuts.ShortcutsList = shortcuts.ShortcutsList.OrderBy(cuts => cuts.SocialId).ToList();
                        GlobalContext.TrendingTab.MAdapter.TrendingList.Add(shortcuts);
                        GlobalContext.TrendingTab.MAdapter.TrendingList.Add(new Classes.TrendingClass
                        {
                            Type = Classes.ItemType.Divider
                        });
                    }
                }

                var respondLastBlogs = ListUtils.ListCachedDataArticle?.Count;
                if (respondLastBlogs > 0 && AppSettings.ShowArticles)
                {
                    var checkList = GlobalContext.TrendingTab.MAdapter.TrendingList.FirstOrDefault(q => q.Type == Classes.ItemType.LastBlogs);
                    if (checkList == null)
                    {
                        GlobalContext.TrendingTab.MAdapter.TrendingList.Add(new Classes.TrendingClass
                        {
                            Id = 1200,
                            Title = Activity.GetText(Resource.String.Lbl_LastBlogs),
                            SectionType = Classes.ItemType.LastBlogs,
                            Type = Classes.ItemType.Section,
                        });

                        var list = ListUtils.ListCachedDataArticle.Take(3)?.ToList();

                        foreach (var item in from item in list let check = GlobalContext.TrendingTab.MAdapter.TrendingList.FirstOrDefault(a => a.LastBlogs?.Id == item.Id && a.Type == Classes.ItemType.LastBlogs) where check == null select item)
                        {
                            GlobalContext.TrendingTab.MAdapter.TrendingList.Add(new Classes.TrendingClass
                            {
                                Id = long.Parse(item.Id),
                                LastBlogs = item,
                                Type = Classes.ItemType.LastBlogs
                            });
                        }

                        GlobalContext.TrendingTab.MAdapter.TrendingList.Add(new Classes.TrendingClass
                        {
                            Type = Classes.ItemType.Divider
                        });
                    }
                }

                if (MAdapter.TrendingList.Count > 0)
                {
                    var emptyStateChecker = MAdapter.TrendingList.FirstOrDefault(a => a.Type == Classes.ItemType.EmptyPage);
                    if (emptyStateChecker != null)
                    {
                        MAdapter.TrendingList.Remove(emptyStateChecker);
                    }

                    var adMob = MAdapter.TrendingList.FirstOrDefault(a => a.Type == Classes.ItemType.AdMob);
                    if (adMob == null && AppSettings.ShowAdMobBanner)
                    {
                        MAdapter.TrendingList.Add(new Classes.TrendingClass
                        {
                            Type = Classes.ItemType.AdMob
                        });
                    }

                    MAdapter.NotifyDataSetChanged();
                }
                else
                {
                    var emptyStateChecker = MAdapter.TrendingList.FirstOrDefault(q => q.Type == Classes.ItemType.EmptyPage);
                    if (emptyStateChecker == null)
                    {
                        MAdapter.TrendingList.Add(new Classes.TrendingClass
                        {
                            Id = 1000,
                            Type = Classes.ItemType.EmptyPage
                        });
                        MAdapter.NotifyDataSetChanged();
                    }
                }
            }
            catch (Exception e)
            {
                ShimmerInflater?.Hide();
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

    }
}