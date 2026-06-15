using Android.Graphics;
using Newtonsoft.Json;
using System.Collections.Generic;
using WoWonder.Helpers.Fonts;
using WoWonderClient.Classes.Call;
using WoWonderClient.Classes.Games;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Jobs;
using WoWonderClient.Classes.Message;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Classes.Product;
using WoWonderClient.Classes.User;

namespace WoWonder.Helpers.Model
{
    public class Classes
    {
        public class PostType
        {
            public long Id { get; set; }
            public string TypeText { get; set; }
            public int Image { get; set; }
            public string ImageColor { get; set; }
        }

        public class Categories
        {
            public string CategoriesId { get; set; }
            public string CategoriesName { get; set; }
            public string CategoriesColor { get; set; }
            public string CategoriesIcon { get; set; }
            public List<SubCategories> SubList { get; set; }
        }

        public class Family
        {
            public string FamilyId { get; set; }
            public string FamilyName { get; set; }
        }

        public class Gender
        {
            public string GenderId { get; set; }
            public string GenderName { get; set; }
            public string GenderColor { get; set; }
            public bool GenderSelect { get; set; }
        }

        public class MyInformation
        {
            public long Id { get; set; }
            public string Name { get; set; }
            public Color Color { get; set; }
            public Color BgColor { get; set; }
            public int Icon { get; set; }
            public string Type { get; set; }
        }

        public class ShortCuts
        {
            public string Type { get; set; }
            public string SocialId { get; set; }
            public string Name { get; set; }
            public PageDataObject PageClass { get; set; }
            public GroupDataObject GroupClass { get; set; }
        }

        public class ExchangeCurrencyObject
        {
            [JsonProperty("disclaimer", NullValueHandling = NullValueHandling.Ignore)]
            public string Disclaimer { get; set; }

            [JsonProperty("license", NullValueHandling = NullValueHandling.Ignore)]
            public string License { get; set; }

            [JsonProperty("timestamp", NullValueHandling = NullValueHandling.Ignore)]
            public long? Timestamp { get; set; }

            [JsonProperty("base", NullValueHandling = NullValueHandling.Ignore)]
            public string Base { get; set; }

            [JsonProperty("rates", NullValueHandling = NullValueHandling.Ignore)]
            public Dictionary<string, double> Rates { get; set; }
        }

        public class ExErrorObject
        {
            [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
            public bool? Error { get; set; }

            [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
            public long? Status { get; set; }

            [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
            public string Message { get; set; }

            [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
            public string Description { get; set; }
        }


        public class TrendingClass
        {
            public long Id { get; set; }
            public ItemType Type { get; set; }
            public string Title { get; set; }
            public ItemType SectionType { get; set; }

            public List<UserDataObject> UserList { get; set; }
            public List<PageDataObject> PageList { get; set; }
            public ActivityDataObject LastActivities { get; set; }
            public List<ShortCuts> ShortcutsList { get; set; }
            public ArticleDataObject LastBlogs { get; set; }
            public TrendingHashtag HashTags { get; set; }
            public GetWeatherObject Weather { get; set; }
            public ExchangeCurrencyObject ExchangeCurrency { get; set; }
        }

        public class NotificationsClass
        {
            public long Id { get; set; }
            public ItemType Type { get; set; }
            public string Title { get; set; }

            public UserDataObject User { get; set; }
            public ActivityDataObject LastActivities { get; set; }
            public NotificationObject Notification { get; set; }
        }

        public class ProductClass
        {
            public long Id { get; set; }
            public ItemType Type { get; set; }
            public string Title { get; set; }

            public List<ProductDataObject> ProductList { get; set; }
            public ProductDataObject Product { get; set; }
            public PurchasedProductDataObject Purchased { get; set; }
        }

        public class JobClass
        {
            public long Id { get; set; }
            public ItemType Type { get; set; }
            public string Title { get; set; }

            public List<JobInfoObject> JobList { get; set; }
            public JobInfoObject Job { get; set; }
        }

        public class GameClass
        {
            public long Id { get; set; }
            public ItemType Type { get; set; }
            public string Title { get; set; }

            public List<GamesDataObject> GameList { get; set; }
            public GamesDataObject Game { get; set; }
        }

        public class ReelsVideoClass
        {
            public string Id { get; set; }
            public ItemType Type { get; set; }
            public PostDataObject VideoData { get; set; }
        }

        public class CallUser
        {
            public string VideoCall { get; set; }

            public string UserId { get; set; }
            public string Avatar { get; set; }
            public string Name { get; set; }

            //Data
            public string Id { get; set; } // call_id
            public string AccessToken { get; set; }
            public string AccessToken2 { get; set; }
            public string FromId { get; set; }
            public string ToId { get; set; }
            public string Active { get; set; }
            public string Called { get; set; }
            public string Time { get; set; }
            public string Declined { get; set; }
            public string Url { get; set; }
            public string Status { get; set; }
            public string RoomName { get; set; }
            public TypeCall Type { get; set; }

            //Style       
            public string TypeIcon { get; set; }
            public string TypeColor { get; set; }
        }

        public class SharedFile
        {
            public string FileName { set; get; }
            public string FileType { set; get; }
            public string FileDate { set; get; }
            public string FilePath { set; get; }
            public string FileExtension { set; get; }
            public string ImageExtra { set; get; }

        }

        public class OptionLastChat
        {
            public string ChatId { set; get; }
            public string PageId { set; get; }
            public string GroupId { set; get; }
            public string UserId { set; get; }
            public string Name { set; get; }
            public string ChatType { set; get; }
        }

        public class LastChatArchive : OptionLastChat
        {
            public string IdLastMessage { set; get; }

            public ChatObject LastChat { get; set; }
        }

        public class LastChatsClass
        {
            public ItemType Type { set; get; }

            public List<UserDataObject> UserRequestList { get; set; }
            public List<GroupChatRequest> GroupRequestList { get; set; }
            public ChatObject LastChat { get; set; }
            public string CountArchive { get; set; }

        }

        public class StorageTypeSelectClass
        {
            public int Id { set; get; }
            public string Type { set; get; }
            public string Text { get; set; }
            public bool Value { get; set; }

        }

        public class ItemOptionObject
        {
            public string Id { get; set; }
            public string Text { get; set; }
            public int Icon { get; set; }
            public FontsIconFrameWork TypeIcon { get; set; }
            public string Color { get; set; }
        }

        public class AddNewChatObject
        {
            public string Id { get; set; }
            public string Text { get; set; }
            public ItemType Type { get; set; }
            public UserDataObject User { get; set; }
        }

        public class ModelsWalkTroutPager
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public int Image { get; set; }
        }

        public enum ItemType
        {
            ProUser = 20201,
            ProPage = 20202,
            HashTag = 20203,
            FriendRequest = 20204,
            LastActivities = 20205,
            Weather = 20206,
            Shortcuts = 20207,
            AdMob = 20208,
            Section = 20209,
            EmptyPage = 202010,
            Divider = 202011,
            LastBlogs = 202012,
            ExchangeCurrency = 202013,

            NearbyShops = 202015,
            Product = 202016,
            MyProduct = 202017,
            PurchasedProduct = 202317,

            NearbyJob = 202018,
            Job = 202019,
            JobRecent = 202020,

            FriendsBirthday = 202021,

            RecentGame = 202022,
            PopularGame = 202023,
            RecommendGame = 202024,
            MyGame = 202025,
            SearchGame = 202026,
            Notifications = 202027,
            ReelsVideo = 202028,


            LastChatNewV = 40202,
            LastChatPage = 40203,
            GroupRequest = 40205,
            Archive = 40206,

            AddGroup = 40208,
            AddCall = 40209,
            User = 40210,
            AddBroadcast = 40211,

        }

    }
}