using Android.OS;
using Android.Views;
using AndroidX.Fragment.App;
using System;
using System.Linq;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Utils;

namespace WoWonder.Activities.ReelsVideo
{
    public class AdsFragment : Fragment
    {
        #region General

        //public override void OnCreate(Bundle savedInstanceState)
        //{
        //    base.OnCreate(savedInstanceState);
        //    HasOptionsMenu = true;
        //}

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                // Use this to return your custom view for this Fragment
                View view = inflater?.Inflate(Resource.Layout.PostType_AdMob4, container, false);

                //Get Value And Set Toolbar
                InitComponent(view);
                return view;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null;
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void OnResume()
        {
            try
            {
                base.OnResume();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnPause()
        {
            try
            {
                base.OnPause();

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnDestroy()
        {
            try
            {
                if (AdsGoogle.NativeAdsPool?.Count <= 3)
                {
                    AdsGoogle.AdMobNative ads = new AdsGoogle.AdMobNative();
                    ads.BindAdMobNative(Activity);
                }

                base.OnDestroy();
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
                var template = view.FindViewById<TemplateView>(Resource.Id.my_template);
                template.Visibility = ViewStates.Visible;

                var ad = AdsGoogle.NativeAdsPool?.FirstOrDefault();
                if (ad != null)
                {
                    NativeTemplateStyle styles = new NativeTemplateStyle.Builder().Build();
                    template.SetStyles(styles);
                    template.SetNativeAd(ad);

                    AdsGoogle.NativeAdsPool.Remove(ad);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

    }
}
