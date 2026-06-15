using Android.Content;
using Android.Graphics.Drawables;
using Android.Views;
using Bumptech.Glide;
using Bumptech.Glide.Module;
using Bumptech.Glide.Request.Target;
using Bumptech.Glide.Request.Transition;
using Java.Lang;
using Java.Util;

namespace WoWonder.Helpers.CacheLoaders
{
    public class CustomGlideModule : AppGlideModule
    {
        public override void ApplyOptions(Context context, GlideBuilder builder)
        {
            builder.SetLogLevel(2);
            base.ApplyOptions(context, builder);
        }
    }

    public class CustomPreloadSizeProvider : Object, ListPreloader.IPreloadSizeProvider, ISizeReadyCallback
    {
        private int[] size;
        private SizeViewTarget viewTarget;

        public int[] GetPreloadSize(Object p0, int p1, int p2)
        {
            if (size == null)
            {
                return null;
            }

            return Arrays.CopyOf(size, size.Length);
        }

        public void OnSizeReady(int width, int height)
        {
            size = new[] { width, height };
            viewTarget = null;
        }

        public void ViewPreloadSizeProvider(View view)
        {
            viewTarget = new SizeViewTarget(view);
            viewTarget.GetSize(this);
        }

        public void SetView(View view)
        {
            if (size != null || viewTarget != null)
            {
                return;
            }
            viewTarget = new SizeViewTarget(view);
            viewTarget.GetSize(this);
        }


    }

    public class SizeViewTarget : CustomViewTarget
    {
        public SizeViewTarget(Object view) : base(view)
        {

        }

        public override void OnLoadFailed(Drawable p0)
        {

        }

        public override void OnResourceReady(Object resource, ITransition transition)
        {

        }

        protected override void OnResourceCleared(Drawable p0)
        {

        }
    }


}