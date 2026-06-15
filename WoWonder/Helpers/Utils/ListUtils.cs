using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using WoWonder.Helpers.Model;
using WoWonder.SQLite;
using WoWonderClient.Classes.Games;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Message;
using WoWonderClient.Classes.Movies;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Classes.Socket;

namespace WoWonder.Helpers.Utils
{
    public static class ListUtils
    {
        //############# DON'T MODIFY HERE #############
        //List Items Declaration 
        //*********************************************************

        public static GetSiteSettingsObject.ConfigObject SettingsSiteList;

        public static ObservableCollection<DataTables.LoginTb> DataUserLoginList = new ObservableCollection<DataTables.LoginTb>();

        public static ObservableCollection<UserDataObject> MyProfileList = new ObservableCollection<UserDataObject>();
        public static ObservableCollection<UserDataObject> MyFollowingList = new ObservableCollection<UserDataObject>();
        public static ObservableCollection<PageDataObject> MyPageList = new ObservableCollection<PageDataObject>();
        public static ObservableCollection<PageDataObject> InvitesPagesList = new ObservableCollection<PageDataObject>();
        public static ObservableCollection<GroupDataObject> MyGroupList = new ObservableCollection<GroupDataObject>();

        public static ObservableCollection<Classes.Family> FamilyList = new ObservableCollection<Classes.Family>();

        public static ObservableCollection<PostDataObject> ListCachedDataAlbum = new ObservableCollection<PostDataObject>();
        public static ObservableCollection<ArticleDataObject> ListCachedDataArticle = new ObservableCollection<ArticleDataObject>();
        public static ObservableCollection<Classes.ProductClass> ListCachedDataMyProduct = new ObservableCollection<Classes.ProductClass>();
        public static ObservableCollection<MoviesDataObject> ListCachedDataMovie = new ObservableCollection<MoviesDataObject>();
        public static ObservableCollection<UserDataObject> ListCachedDataNearby = new ObservableCollection<UserDataObject>();
        public static ObservableCollection<GiftObject.DataGiftObject> GiftsList = new ObservableCollection<GiftObject.DataGiftObject>();
        public static ObservableCollection<PostDataObject> ListCachedDataMyPhotos = new ObservableCollection<PostDataObject>();
        public static ObservableCollection<PostDataObject> ListCachedDataMyVideos = new ObservableCollection<PostDataObject>();
        public static ObservableCollection<Classes.GameClass> ListCachedDataGames = new ObservableCollection<Classes.GameClass>();
        public static ObservableCollection<GamesDataObject> ListCachedDataPopularGames = new ObservableCollection<GamesDataObject>();
        public static ObservableCollection<GamesDataObject> ListCachedDataMyGames = new ObservableCollection<GamesDataObject>();
        public static ObservableCollection<GroupDataObject> SuggestedGroupList = new ObservableCollection<GroupDataObject>();
        public static ObservableCollection<UserDataObject> SuggestedUserList = new ObservableCollection<UserDataObject>();
        public static ObservableCollection<PageDataObject> SuggestedPageList = new ObservableCollection<PageDataObject>();

        public static ObservableCollection<UserDataObject> FriendRequestsList = new ObservableCollection<UserDataObject>();
        public static ObservableCollection<TrendingHashtag> HashTagList = new ObservableCollection<TrendingHashtag>();
        public static ObservableCollection<Classes.ShortCuts> ShortCutsList = new ObservableCollection<Classes.ShortCuts>();

        //Chat
        public static ObservableCollection<ChatObject> UserList = new ObservableCollection<ChatObject>();
        public static ObservableCollection<Classes.SharedFile> ListSharedFiles = new ObservableCollection<Classes.SharedFile>();
        public static ObservableCollection<Classes.SharedFile> LastSharedFiles = new ObservableCollection<Classes.SharedFile>();
        public static ObservableCollection<GroupChatRequest> GroupRequestsList = new ObservableCollection<GroupChatRequest>();
        public static ObservableCollection<Classes.OptionLastChat> MuteList = new ObservableCollection<Classes.OptionLastChat>();
        public static ObservableCollection<Classes.OptionLastChat> PinList = new ObservableCollection<Classes.OptionLastChat>();
        public static ObservableCollection<Classes.LastChatArchive> ArchiveList = new ObservableCollection<Classes.LastChatArchive>();
        public static ObservableCollection<DataTables.StickersTb> StickersList = new ObservableCollection<DataTables.StickersTb>();
        public static ObservableCollection<PrivateMessageObject> MessageUnreadList = new ObservableCollection<PrivateMessageObject>();
        public static ObservableCollection<string> NotifyShowList = new ObservableCollection<string>();

        public static List<Classes.StorageTypeSelectClass> StorageTypeWiFiSelect = new List<Classes.StorageTypeSelectClass>();
        public static List<Classes.StorageTypeSelectClass> StorageTypeMobileSelect = new List<Classes.StorageTypeSelectClass>();

        public static ObservableCollection<Classes.ReelsVideoClass> VideoReelsList = new ObservableCollection<Classes.ReelsVideoClass>();
        public static ObservableCollection<PostDataObject> VideoReelsViewsList = new ObservableCollection<PostDataObject>();
        public static List<PostDataObject> NewPostList = new List<PostDataObject>();

        public static void ClearAllList()
        {
            try
            {
                DataUserLoginList.Clear();
                MyProfileList.Clear();
                MyFollowingList.Clear();
                MyPageList.Clear();
                InvitesPagesList.Clear();
                MyGroupList.Clear();
                FamilyList.Clear();
                ListCachedDataAlbum.Clear();
                ListCachedDataArticle.Clear();
                ListCachedDataMyProduct.Clear();
                ListCachedDataMovie.Clear();
                ListCachedDataNearby.Clear();
                GiftsList.Clear();
                ListCachedDataMyPhotos.Clear();
                ListCachedDataMyVideos.Clear();
                ListCachedDataGames.Clear();
                ListCachedDataPopularGames.Clear();
                ListCachedDataMyGames.Clear();
                SuggestedGroupList.Clear();
                SuggestedUserList.Clear();
                FriendRequestsList.Clear();
                HashTagList.Clear();
                ShortCutsList.Clear();
                SuggestedPageList.Clear();
                StickersList.Clear();
                NewPostList.Clear();

                StickersList.Clear();

                //Chat
                UserList.Clear();
                ListSharedFiles.Clear();
                LastSharedFiles.Clear();
                GroupRequestsList.Clear();
                MuteList.Clear();
                PinList.Clear();
                ArchiveList.Clear();
                MessageUnreadList.Clear();
                StorageTypeWiFiSelect.Clear();
                StorageTypeMobileSelect.Clear();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        public static void AddRange<T>(ObservableCollection<T> collection, IEnumerable<T> items)
        {
            try
            {
                items.ToList().ForEach(collection.Add);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static List<List<T>> SplitList<T>(List<T> locations, int nSize = 30)
        {
            var list = new List<List<T>>();

            for (int i = 0; i < locations.Count; i += nSize)
            {
                list.Add(locations.GetRange(i, Math.Min(nSize, locations.Count - i)));
            }

            return list;
        }

        public static IEnumerable<T> TakeLast<T>(IEnumerable<T> source, int n)
        {
            var enumerable = source as T[] ?? source.ToArray();

            return enumerable.Skip(Math.Max(0, enumerable.Count() - n));
        }

        public static void Copy<T>(T from, T to)
        {
            Type t = typeof(T);
            PropertyInfo[] props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo p in props)
            {
                try
                {
                    if (!p.CanRead || !p.CanWrite) continue;

                    object val = p.GetGetMethod().Invoke(from, null);
                    object defaultVal = p.PropertyType.IsValueType ? Activator.CreateInstance(p.PropertyType) : null;
                    if (null != defaultVal && !val.Equals(defaultVal))
                    {
                        p.GetSetMethod().Invoke(to, new[] { val });
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static int Remove<T>(this ObservableCollection<T> coll, Func<T, bool> condition)
        {
            var itemsToRemove = coll.Where(condition).ToList();

            foreach (var itemToRemove in itemsToRemove)
            {
                coll.Remove(itemToRemove);
            }

            return itemsToRemove.Count;
        }

        /// <summary>
        /// Extends ObservableCollection adding a RemoveAll method to remove elements based on a boolean condition function
        /// </summary>
        /// <typeparam name="T">The type contained by the collection</typeparam>
        /// <param name="observableCollection">The ObservableCollection</param>
        /// <param name="condition">A function that evaluates to true for elements that should be removed</param>
        /// <returns>The number of elements removed</returns>
        public static int RemoveAll<T>(this ObservableCollection<T> observableCollection, Func<T, bool> condition)
        {
            // Find all elements satisfying the condition, i.e. that will be removed
            var toRemove = observableCollection
                .Where(condition)
                .ToList();

            // Remove the elements from the original collection, using the Count method to iterate through the list, 
            // incrementing the count whenever there's a successful removal
            return toRemove.Count(observableCollection.Remove);
        }

        /// <summary>
        /// Extends ObservableCollection adding a RemoveAll method to remove elements based on a boolean condition function
        /// </summary>
        /// <typeparam name="T">The type contained by the collection</typeparam>
        /// <param name="observableCollection">The ObservableCollection</param>
        /// <param name="toRemove">Find all elements satisfying the condition, i.e. that will be removed</param>
        /// <returns>The number of elements removed</returns>
        public static int RemoveAll<T>(this ObservableCollection<T> observableCollection, List<T> toRemove)
        {
            // Remove the elements from the original collection, using the Count method to iterate through the list, 
            // incrementing the count whenever there's a successful removal
            return toRemove.Count(observableCollection.Remove);
        }


    }
}