using Android.Content;
using WoWonder.Activities.Live.Page;

namespace WoWonder.Activities.Live.Utils
{
    public class PrefManager
    {
        public static ISharedPreferences GetPreferences(Context context)
        {
            return context.GetSharedPreferences(LiveConstants.PrefName, FileCreationMode.Private);
        }
    }
}