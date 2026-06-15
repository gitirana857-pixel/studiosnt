using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Java.Lang;

namespace WoWonder.Helpers.Animinations
{
    public class ImageFadeOutAnimationListener : Object, Animation.IAnimationListener
    {
        private readonly ImageView VideoImage;
        public ImageFadeOutAnimationListener(ImageView videoImage)
        {
            VideoImage = videoImage;
        }
        public void OnAnimationEnd(Animation animation)
        {
            VideoImage.Visibility = ViewStates.Invisible;
        }

        public void OnAnimationRepeat(Animation animation)
        {

        }

        public void OnAnimationStart(Animation animation)
        {

        }
    }
}
