using Android.App;
using Android.Views;
using AndroidX.ViewPager.Widget;
using Sephiroth.ImageZoom;
using System.Collections.ObjectModel;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Posts;
using Exception = System.Exception;
using Object = Java.Lang.Object;

namespace WoWonder.Activities.MyPhoto.Adapters
{
    public class TouchMyPhotoAdapter : PagerAdapter
    {

        private readonly Activity ActivityContext;
        private readonly ObservableCollection<PostDataObject> ImagesList;
        private readonly LayoutInflater Inflater;

        public TouchMyPhotoAdapter(Activity context, ObservableCollection<PostDataObject> imagesList)
        {
            try
            {
                ActivityContext = context;
                ImagesList = new ObservableCollection<PostDataObject>(imagesList);
                //Inflater = LayoutInflater.From(context);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override Object InstantiateItem(ViewGroup view, int position)
        {
            try
            {
                var layout = LayoutInflater.From(ActivityContext)?.Inflate(Resource.Layout.Style_MultiImageCoursalVeiw, view, false);
                if (layout != null)
                {
                    var item = ImagesList[position];
                    if (item != null)
                    {
                        ImageViewTouch image = layout.FindViewById<ImageViewTouch>(Resource.Id.image);

                        var imageUrl = !string.IsNullOrEmpty(item.PostSticker) ? item.PostSticker : item.PostFileFull;

                        GlideImageLoader.LoadImage(ActivityContext, imageUrl, image, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                    }

                    view.AddView(layout);
                    return layout;
                }
                return null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        public override void DestroyItem(ViewGroup container, int position, Object @object)
        {
            try
            {
                View view = (View)@object;
                container.RemoveView(view);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override bool IsViewFromObject(View view, Object @object)
        {
            return view.Equals(@object);
        }

        public override int Count => ImagesList?.Count ?? 0;
    }
}