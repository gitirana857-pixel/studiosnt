using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Widget;
using AndroidX.Credentials;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request;
using Com.Facebook;
using Com.Facebook.Login;
using Java.Lang;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading.Tasks;
using WoWonder.Activities.AddPost;
using WoWonder.Activities.AddPost.Service;
using WoWonder.Activities.Default;
using WoWonder.Activities.SettingsPreferences;
using WoWonder.Helpers.MediaPlayerController;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.OneSignalNotif;
using WoWonder.Services;
using WoWonder.SQLite;
using WoWonderClient;
using WoWonderClient.Classes.Articles;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Group;
using WoWonderClient.Classes.Page;
using WoWonderClient.Classes.User;
using WoWonderClient.Requests;
using Exception = System.Exception;
using File = Java.IO.File;
using HttpMethod = System.Net.Http.HttpMethod;
using Thread = System.Threading.Thread;

namespace WoWonder.Helpers.Controller
{
    internal static class ApiRequest
    {
        internal static readonly string ApiGetSearchGif = "https://api.giphy.com/v1/gifs/search?api_key=b9427ca5441b4f599efa901f195c9f58&limit=45&rating=g&q=";
        internal static readonly string ApiGeTrendingGif = "https://api.giphy.com/v1/gifs/trending?api_key=b9427ca5441b4f599efa901f195c9f58&limit=45&rating=g";
        internal static readonly string ApiGetTimeZone = "http://ip-api.com/json/";
        internal static readonly string ApiGetWeatherApi = "https://api.weatherapi.com/v1/forecast.json?key=";
        internal static readonly string ApiGetExchangeCurrency = "https://openexchangerates.org/api/latest.json?app_id=";
        internal static readonly string ApiGetInfoCovid19 = "https://covid-193.p.rapidapi.com/statistics?country=";
        internal static readonly string ApiGetInfoTwitterEmbed = "https://publish.twitter.com/oembed?url=";

        public static async Task<GetSiteSettingsObject.ConfigObject> GetSettings_Api(Activity context)
        {
            if (Methods.CheckConnectivity())
            {
                await SetLangUserAsync().ConfigureAwait(false);

                (var apiStatus, dynamic respond) = await Current.GetSettingsAsync();

                if (apiStatus != 200 || respond is not GetSiteSettingsObject.ConfigObject result || result == null)
                    return Methods.DisplayReportResult(context, respond);

                ListUtils.SettingsSiteList = result;

                UserDetails.VisionApiKey = result.VisionApiKey;

                if (AppSettings.OneSignalAppId != result.AndroidNPushId)
                {
                    AppSettings.OneSignalAppId = result.AndroidNPushId;
                    OneSignalNotification.Instance.RegisterNotificationDevice(context);
                }
                 
                SqLiteDatabase dbDatabase = new SqLiteDatabase();
                dbDatabase.InsertOrUpdateSettings(result);

                await Task.Factory.StartNew(() =>
                {
                    try
                    {
                        if (AppSettings.ShowColor && ListUtils.SettingsSiteList?.PostColors?.PostColorsList != null)
                        {
                            var fullGlideRequestBuilder = Glide.With(context?.BaseContext).AsDrawable().SetDiskCacheStrategy(DiskCacheStrategy.Automatic).SkipMemoryCache(true).Override(200);

                            foreach (var item in ListUtils.SettingsSiteList?.PostColors?.PostColorsList.Values)
                            {
                                if (!string.IsNullOrEmpty(item.Image))
                                {
                                    fullGlideRequestBuilder.Load(item.Image).Preload();
                                }
                                else
                                {
                                    var colorsList = new List<int>();

                                    if (!string.IsNullOrEmpty(item.Color1)) colorsList.Add(Color.ParseColor(item.Color1));

                                    if (!string.IsNullOrEmpty(item.Color2)) colorsList.Add(Color.ParseColor(item.Color2));

                                    GradientDrawable gd = new GradientDrawable(GradientDrawable.Orientation.TopBottom, colorsList.ToArray());
                                    gd.SetCornerRadius(0f);

                                    fullGlideRequestBuilder.Load(gd).Preload();
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                }).ConfigureAwait(false);

                CategoriesController.SetListCategories(result);

                return result;
            }

            ToastUtils.ShowToast(Application.Context, Application.Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            return null!;
        }

        public static async Task GetGifts()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var (apiStatus, respond) = await RequestsAsync.Global.FetchGiftAsync();
                    if (apiStatus == 200)
                    {
                        if (respond is GiftObject result)
                        {
                            if (result.Data.Count > 0)
                            {
                                ListUtils.GiftsList = new ObservableCollection<GiftObject.DataGiftObject>(result.Data);

                                SqLiteDatabase sqLiteDatabase = new SqLiteDatabase();
                                sqLiteDatabase.InsertAllGifts(ListUtils.GiftsList);

                                await Task.Factory.StartNew(() =>
                                {
                                    try
                                    {
                                        foreach (var item in result.Data)
                                        {
                                            Glide.With(Application.Context).Load(item.MediaFile).Apply(new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.All).CenterCrop()).Preload();
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Methods.DisplayReportResultTrack(e);
                                    }
                                });
                            }
                        }
                    }
                }
                else
                {
                    ToastUtils.ShowToast(Application.Context, Application.Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public static async Task<string> GetTimeZoneAsync()
        {
            try
            {
                if (AppSettings.AutoCodeTimeZone)
                {
                    HttpClientHandler clientHandler = new HttpClientHandler();
                    clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                    clientHandler.SslProtocols = SslProtocols.Tls | SslProtocols.Ssl2 | SslProtocols.Ssl3 | SslProtocols.Tls11 | SslProtocols.Tls12 | SslProtocols.Tls13 | SslProtocols.Default;

                    // Pass the handler to httpClient(from you are calling api)
                    var client = new HttpClient(clientHandler);
                    var response = await client.GetAsync(ApiGetTimeZone);
                    string json = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<TimeZoneObject>(json);
                    if (data != null)
                    {
                        UserDetails.Country = data.Country;
                        UserDetails.City = data.City;
                        UserDetails.Lat = data.Lat.ToString(CultureInfo.InvariantCulture);
                        UserDetails.Lng = data.Lon.ToString(CultureInfo.InvariantCulture);

                        return data.Timezone;
                    }
                }

                return AppSettings.CodeTimeZone;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return AppSettings.CodeTimeZone;
            }
        }

        private static async Task SetLangUserAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(Current.AccessToken) || !AppSettings.SetLangUser)
                    return;

                string lang = "english";
                if (UserDetails.LangName.Contains("en"))
                    lang = "english";
                else if (UserDetails.LangName.Contains("ar"))
                    lang = "arabic";
                else if (UserDetails.LangName.Contains("de"))
                    lang = "german";
                else if (UserDetails.LangName.Contains("el"))
                    lang = "greek";
                else if (UserDetails.LangName.Contains("es"))
                    lang = "spanish";
                else if (UserDetails.LangName.Contains("fr"))
                    lang = "french";
                else if (UserDetails.LangName.Contains("it"))
                    lang = "italian";
                else if (UserDetails.LangName.Contains("ja"))
                    lang = "japanese";
                else if (UserDetails.LangName.Contains("nl"))
                    lang = "dutch";
                else if (UserDetails.LangName.Contains("pt"))
                    lang = "portuguese";
                else if (UserDetails.LangName.Contains("ro"))
                    lang = "romanian";
                else if (UserDetails.LangName.Contains("ru"))
                    lang = "russian";
                else if (UserDetails.LangName.Contains("sq"))
                    lang = "albanian";
                else if (UserDetails.LangName.Contains("sr"))
                    lang = "serbian";
                else if (UserDetails.LangName.Contains("tr"))
                    lang = "turkish";
                //else
                //    lang = string.IsNullOrEmpty(UserDetails.LangName) ? AppSettings.Lang : "";

                await Task.Factory.StartNew(() =>
                {
                    if (lang != "")
                    {
                        Dictionary<string, string> dataPrivacy = new Dictionary<string, string>();

                        var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                        if (dataUser != null)
                        {
                            dataUser.Language = lang;

                            var sqLiteDatabase = new SqLiteDatabase();
                            sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);
                        }

                        dataPrivacy.Add("language", lang);

                        if (Methods.CheckConnectivity())
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dataPrivacy) });
                        else
                            ToastUtils.ShowToast(Application.Context, Application.Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    }
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static async Task<ObservableCollection<GifGiphyClass.Datum>> SearchGif(string searchKey, string offset)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(Application.Context, Application.Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    return null!;
                }

                var client = new HttpClient();
                var response = await client.GetAsync(ApiGetSearchGif + searchKey + "&offset=" + offset);
                string json = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<GifGiphyClass>(json);

                if (response.StatusCode == HttpStatusCode.OK)
                    return data.DataMeta.Status == 200 ? new ObservableCollection<GifGiphyClass.Datum>(data.Data) : null;
                return null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        public static async Task<ObservableCollection<GifGiphyClass.Datum>> TrendingGif(string offset)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(Application.Context, Application.Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    return null!;
                }

                var client = new HttpClient();
                var response = await client.GetAsync(ApiGeTrendingGif + "&offset=" + offset);
                string json = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<GifGiphyClass>(json);

                return response.StatusCode switch
                {
                    HttpStatusCode.OK => data.DataMeta.Status == 200
                        ? new ObservableCollection<GifGiphyClass.Datum>(data.Data)
                        : null,
                    _ => null!
                };
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        public static async Task<GetWeatherObject> GetWeather()
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(Application.Context, Application.Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    return null!;
                }

                if (string.IsNullOrEmpty(UserDetails.City))
                {
                    await GetTimeZoneAsync();
                }
                var client = new HttpClient();
                var response = await client.GetAsync(ApiGetWeatherApi + AppSettings.KeyWeatherApi + "&q=" + UserDetails.City + "&lang=" + UserDetails.LangName);
                string json = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<GetWeatherObject>(json);
                return response.StatusCode switch
                {
                    HttpStatusCode.OK => data,
                    _ => null!
                };
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        public static async Task<(int, dynamic)> GetExchangeCurrencyAsync()
        {
            try
            {
                var client = new HttpClient();
                var response = await client.GetAsync(ApiGetExchangeCurrency + AppSettings.KeyCurrencyApi + "&base=" + AppSettings.ExCurrency + "&symbols=" + AppSettings.ExCurrencies);
                string json = await response.Content.ReadAsStringAsync();

                var data = JsonConvert.DeserializeObject<Classes.ExchangeCurrencyObject>(json);
                if (data != null)
                {
                    return (200, data);
                }

                var error = JsonConvert.DeserializeObject<Classes.ExErrorObject>(json);
                return (400, error);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return (404, e.Message);
            }
        }

        public static async Task<(int, dynamic)> GetInfoCovid19Async(string country)
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(ApiGetInfoCovid19 + country),
                    Headers =
                    {
                        {"x-rapidapi-key", AppSettings.KeyCoronaVirus},
                        {"x-rapidapi-host", AppSettings.HostCoronaVirus},
                    }
                };
                var response = await client.SendAsync(request);
                string json = await response.Content.ReadAsStringAsync();

                var data = JsonConvert.DeserializeObject<Covid19Object>(json);
                if (data != null)
                {
                    return (200, data);
                }

                var error = JsonConvert.DeserializeObject<ErrorCovid19Object>(json);
                return (400, error);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return (404, e.Message);
            }
        }

        public static async Task<string> ApiGetInfoTwitterEmbedAsync(string url)
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(ApiGetInfoTwitterEmbed + url),
                };
                var response = await client.SendAsync(request);
                string json = await response.Content.ReadAsStringAsync();

                string html = JObject.Parse(json)["html"]?.ToString() ?? "";
                if (!string.IsNullOrEmpty(html))
                {
                    return html;
                }
                return "";
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return "";
            }
        }

        public static async Task Get_MyProfileData_Api(Activity context)
        {
            if (Methods.CheckConnectivity())
            {
                string fetch = "user_data,following";

                if (AppSettings.ShowCommunitiesPages)
                    fetch += ",liked_pages";

                if (AppSettings.ShowCommunitiesGroups)
                    fetch += ",joined_groups";

                if (AppSettings.ShowAddToFamily)
                    fetch += ",family";

                var (apiStatus, respond) = await RequestsAsync.Global.GetUserDataAsync(UserDetails.UserId, fetch);

                switch (apiStatus)
                {
                    case 200:
                        {
                            switch (respond)
                            {
                                case GetUserDataObject result:
                                    {
                                        UserDetails.Avatar = result.UserData.Avatar;
                                        UserDetails.Cover = result.UserData.Cover;
                                        UserDetails.Username = result.UserData.Username;
                                        UserDetails.FullName = result.UserData.Name;
                                        UserDetails.Email = result.UserData.Email;

                                        ListUtils.MyProfileList = new ObservableCollection<UserDataObject> { result.UserData };

                                        ListUtils.MyFollowingList = result.Following?.Count switch
                                        {
                                            > 0 => new ObservableCollection<UserDataObject>(result.Following),
                                            _ => ListUtils.MyFollowingList
                                        };

                                        context?.RunOnUiThread(() =>
                                        {
                                            try
                                            {
                                                Glide.With(Application.Context).Load(UserDetails.Avatar).Apply(new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.All).CircleCrop()).Preload();
                                            }
                                            catch (Exception e)
                                            {
                                                Methods.DisplayReportResultTrack(e);
                                            }
                                        });

                                        await Task.Factory.StartNew(() =>
                                        {
                                            SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                            dbDatabase.Insert_Or_Update_To_MyProfileTable(result.UserData);

                                            switch (result.Following?.Count)
                                            {
                                                case > 0:
                                                    dbDatabase.Insert_Or_Replace_MyContactTable(new ObservableCollection<UserDataObject>(result.Following));
                                                    break;
                                            }


                                        });
                                        break;
                                    }
                            }

                            break;
                        }
                    default:
                        Methods.DisplayReportResult(context, respond);
                        break;
                }
            }
        }

        public static async Task LoadSuggestedGroup()
        {
            if (Methods.CheckConnectivity() && AppSettings.ShowSuggestedGroup)
            {
                //var countList = ListUtils.SuggestedGroupList.Count;
                var (respondCode, respondString) = await RequestsAsync.Group.GetRecommendedGroupsAsync("25", "0").ConfigureAwait(false);
                if (respondCode == 200)
                    if (respondString is ListGroupsObject result)
                    {
                        var respondList = result.Data.Count;
                        if (respondList > 0)
                            ListUtils.SuggestedGroupList = new ObservableCollection<GroupDataObject>(result.Data);
                    }
                //else Methods.DisplayReportResult(activity, respondString);
            }
        }

        public static async Task LoadSuggestedPage()
        {
            if (Methods.CheckConnectivity() && AppSettings.ShowSuggestedPage)
            {
                //var countList = ListUtils.SuggestedPageList.Count;
                var (respondCode, respondString) = await RequestsAsync.Page.GetRecommendedPagesAsync("25", "0").ConfigureAwait(false);
                if (respondCode == 200)
                    if (respondString is ListPagesObject result)
                    {
                        var respondList = result.Data.Count;
                        if (respondList > 0)
                            ListUtils.SuggestedPageList = new ObservableCollection<PageDataObject>(result.Data);
                    }

                //else Methods.DisplayReportResult(activity, respondString);
            }
        }

        public static async Task LoadSuggestedUser()
        {
            if (Methods.CheckConnectivity() && AppSettings.ShowSuggestedUser)
            {
                //var countList = ListUtils.SuggestedUserList.Count;
                var (respondCode, respondString) = await RequestsAsync.Global.GetRecommendedUsersAsync("25", "0").ConfigureAwait(false);
                if (respondCode == 200)
                    if (respondString is ListUsersObject result)
                    {
                        var respondList = result.Data.Count;
                        if (respondList > 0)
                            ListUtils.SuggestedUserList = new ObservableCollection<UserDataObject>(result.Data);
                    }

                //else Methods.DisplayReportResult(activity, respondString);
            }
        }

        public static async Task GetMyGroups()
        {
            if (Methods.CheckConnectivity() && AppSettings.ShowCommunitiesGroups)
            {
                try
                {
                    var (apiStatus, respond) = await RequestsAsync.Group.GetMyGroupsAsync("0", "25").ConfigureAwait(false);
                    if (apiStatus != 200 || respond is not ListGroupsObject result || result.Data == null)
                    {
                        //Methods.DisplayReportResult(this, respond);
                    }
                    else
                    {
                        var respondList = result.Data.Count;
                        if (respondList > 0)
                        {
                            result.Data.Reverse();
                            ListUtils.MyGroupList = new ObservableCollection<GroupDataObject>(result.Data);

                            foreach (var groupClass in result.Data)
                            {
                                ListUtils.ShortCutsList?.Add(new Classes.ShortCuts
                                {
                                    SocialId = groupClass.GroupId,
                                    Type = "Group",
                                    Name = groupClass.GroupName,
                                    GroupClass = groupClass,
                                });
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }

                try
                {
                    var (apiStatus2, respond2) = await RequestsAsync.Group.GetJoinedGroupsAsync(UserDetails.UserId, "0", "25").ConfigureAwait(false);
                    if (apiStatus2 != 200 || respond2 is not ListGroupsObject result2 || result2.Data == null)
                    {
                        //Methods.DisplayReportResult(this, respond);
                    }
                    else
                    {
                        var respondList = result2.Data.Count;
                        if (respondList > 0)
                            foreach (var item in result2.Data)
                            {
                                if (ListUtils.MyGroupList.FirstOrDefault(a => a.GroupId == item.GroupId) == null) ListUtils.MyGroupList.Add(item);
                            }
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        public static async Task GetMyPages()
        {
            if (Methods.CheckConnectivity() && AppSettings.ShowCommunitiesPages)
            {
                var (apiStatus, respond) = await RequestsAsync.Page.GetMyPagesAsync("0", "25").ConfigureAwait(false);
                if (apiStatus != 200 || respond is not ListPagesObject result || result.Data == null)
                {
                    //Methods.DisplayReportResult(this, respond);
                }
                else
                {
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    {
                        result.Data.Reverse();

                        ListUtils.MyPageList = new ObservableCollection<PageDataObject>(result.Data);

                        foreach (var pageClass in result.Data)
                        {
                            ListUtils.ShortCutsList.Add(new Classes.ShortCuts
                            {
                                SocialId = pageClass.PageId,
                                Type = "Page",
                                Name = pageClass.PageName,
                                PageClass = pageClass,
                            });
                        }
                    }
                }
            }
        }

        public static async Task GetLastArticles()
        {
            if (Methods.CheckConnectivity() && AppSettings.ShowArticles)
            {
                var (apiStatus, respond) = await RequestsAsync.Article.GetArticlesAsync("5");
                if (apiStatus != 200 || respond is not GetUsersArticlesObject result || result.Articles == null)
                {
                    //Methods.DisplayReportResult(this, respond);
                }
                else
                {
                    var respondList = result.Articles.Count;
                    if (respondList > 0)
                        ListUtils.ListCachedDataArticle = new ObservableCollection<ArticleDataObject>(result.Articles);
                }
            }
        }

        /////////////////////////////////////////////////////////////////
        private static bool RunLogout;

        public static async void Delete(Activity context)
        {
            try
            {
                switch (RunLogout)
                {
                    case false:
                        Constant.IsLoggingOut = true;
                        RunLogout = true;

                        await RemoveData("Delete");

                        context?.RunOnUiThread(() =>
                        {
                            try
                            {
                                OneSignalNotification.Instance.UnRegisterNotificationDevice();

                                context?.DeleteDatabase(AppSettings.DatabaseName + "24_.db");
                                context?.DeleteDatabase(SqLiteDatabase.PathCombine);

                                Methods.Path.DeleteAll_FolderUser();

                                SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                dbDatabase.DropAll();

                                Runtime.GetRuntime()?.RunFinalization();
                                Runtime.GetRuntime()?.Gc();
                                TrimCache(context);

                                ListUtils.ClearAllList();
                                CategoriesController.ResetListCategories();

                                UserDetails.ClearAllValueUserDetails();

                                dbDatabase.CheckTablesStatus();

                                context.StopService(new Intent(context, typeof(PostService)));
                                AppApiService.GetInstance()?.StopJob(context);

                                MainSettings.SharedData?.Edit()?.Clear()?.Commit();
                                MainSettings.InAppReview?.Edit()?.Clear()?.Commit();
                                MainSettings.LastPosition?.Edit()?.Clear()?.Commit();

                                Intent intent = new Intent(context, typeof(LoginActivity));
                                intent.AddCategory(Intent.CategoryHome);
                                intent.SetAction(Intent.ActionMain);
                                intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
                                context.StartActivity(intent);
                                context.FinishAffinity();
                                context.Finish();
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });

                        RunLogout = false;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static async void Logout(Activity context)
        {
            try
            {
                switch (RunLogout)
                {
                    case false:
                        Constant.IsLoggingOut = true;
                        RunLogout = true;

                        await RemoveData("Logout");

                        context?.RunOnUiThread(() =>
                        {
                            try
                            {
                                OneSignalNotification.Instance.UnRegisterNotificationDevice();

                                context?.DeleteDatabase(AppSettings.DatabaseName + "24_.db");
                                context?.DeleteDatabase(SqLiteDatabase.PathCombine);

                                Methods.Path.DeleteAll_FolderUser();

                                SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                dbDatabase.DropAll();

                                Runtime.GetRuntime()?.RunFinalization();
                                Runtime.GetRuntime()?.Gc();
                                TrimCache(context);

                                ListUtils.ClearAllList();
                                CategoriesController.ResetListCategories();

                                UserDetails.ClearAllValueUserDetails();

                                dbDatabase.CheckTablesStatus();

                                context.StopService(new Intent(context, typeof(PostService)));
                                AppApiService.GetInstance()?.StopJob(context);

                                MainSettings.SharedData?.Edit()?.Clear()?.Commit();
                                MainSettings.InAppReview?.Edit()?.Clear()?.Commit();
                                MainSettings.LastPosition?.Edit()?.Clear()?.Commit();

                                Intent intent = new Intent(context, typeof(LoginActivity));
                                intent.AddCategory(Intent.CategoryHome);
                                intent.SetAction(Intent.ActionMain);
                                intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
                                context.StartActivity(intent);
                                context.FinishAffinity();
                                context.Finish();
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });

                        RunLogout = false;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static void TrimCache(Activity context)
        {
            try
            {
                File dir = context?.CacheDir;
                if (dir != null && dir.IsDirectory)
                {
                    DeleteDir(dir);
                }

                if (context?.IsDestroyed != false)
                    return;

                Glide.Get(context)?.ClearMemory();
                new Thread(() =>
                {
                    try
                    {
                        Glide.Get(context)?.ClearDiskCache();
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                }).Start();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private static bool DeleteDir(File dir)
        {
            try
            {
                if (dir == null || !dir.IsDirectory) return dir != null && dir.Delete();
                string[] children = dir.List();
                if (children.Select(child => DeleteDir(new File(dir, child))).Any(success => !success))
                {
                    return false;
                }

                // The directory is now empty so delete it
                return dir.Delete();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;
            }
        }

        private static void Reset()
        {
            try
            {
                MentionActivity.MAdapter = null!;
                Current.AccessToken = string.Empty;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private static async Task RemoveData(string type)
        {
            try
            {
                if (type == "Logout")
                {
                    if (Methods.CheckConnectivity())
                        await RequestsAsync.Auth.DeleteTokenAsync(UserDetails.AccessToken);
                }
                else if (type == "Delete")
                {
                    Methods.Path.DeleteAll_FolderUser();

                    if (Methods.CheckConnectivity())
                        await RequestsAsync.Auth.DeleteUserAsync(UserDetails.Password);
                }

                if (AppSettings.ShowGoogleLogin && SocialLoginBaseActivity.CredentialManager != null)
                {
                    SocialLoginBaseActivity.CredentialManager.ClearCredentialState(new ClearCredentialStateRequest(), null);
                    SocialLoginBaseActivity.CredentialManager = null;
                }

                if (AppSettings.ShowFacebookLogin)
                {
                    var accessToken = AccessToken.CurrentAccessToken;
                    var isLoggedIn = accessToken != null && !accessToken.IsExpired;
                    if (isLoggedIn && Profile.CurrentProfile != null)
                        LoginManager.Instance.LogOut();
                }

                Reset();

                UserDetails.ClearAllValueUserDetails();

                Methods.DeleteNoteOnSD();

                GC.Collect();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}