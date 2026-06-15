using Androidx.Media3.Exoplayer;
using System.Collections.ObjectModel;
using WoWonderClient.Classes.Posts;

namespace WoWonder.Helpers.MediaPlayerController
{
    public static class Constant
    {
        public static bool IsLoggingOut { get; set; } = false;
        public static bool IsChangingTheme { get; set; } = false;
        public static bool IsOpenNotify { get; set; } = false;
        public static bool IsRepeat { get; set; } = false;
        public static bool IsPlayed { get; set; } = false;
        public static bool IsSuffle { get; set; } = false;
        public static bool IsOnline { get; set; } = true;

        public static IExoPlayer Player { get; set; }

        public static ObservableCollection<PostDataObject> ArrayListPlay = new ObservableCollection<PostDataObject>();
        public static int PlayPos { get; set; } = 0;
    }
}