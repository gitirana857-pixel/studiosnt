using Android.OS;
using Android.Runtime;
using AndroidX.Fragment.App;
using AndroidX.Lifecycle;
using AndroidX.ViewPager2.Adapter;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using Fragment = AndroidX.Fragment.App.Fragment;
using FragmentManager = AndroidX.Fragment.App.FragmentManager;

namespace WoWonder.Activities.ReelsVideo.Adapters
{
    public class ReelsVideoPagerAdapter : FragmentStateAdapter
    {
        private int CountVideo;
        private ObservableCollection<Classes.ReelsVideoClass> DataVideos;

        public ReelsVideoPagerAdapter(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public ReelsVideoPagerAdapter(Fragment fragment) : base(fragment)
        {
        }

        public ReelsVideoPagerAdapter(FragmentActivity fragmentActivity) : base(fragmentActivity)
        {
        }

        public ReelsVideoPagerAdapter(FragmentManager fragmentManager, Lifecycle lifecycle) : base(fragmentManager, lifecycle)
        {
        }

        public ReelsVideoPagerAdapter(FragmentActivity fragmentActivity, int size, ObservableCollection<Classes.ReelsVideoClass> dataVideos) : base(fragmentActivity)
        {
            try
            {
                CountVideo = size;
                DataVideos = dataVideos;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void UpdateReelsVideoPager(int size, ObservableCollection<Classes.ReelsVideoClass> dataVideos)
        {
            try
            {
                CountVideo = size;
                DataVideos = dataVideos;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => CountVideo;

        public override Fragment CreateFragment(int position)
        {
            try
            {
                Bundle bundle = new Bundle();
                bundle.PutInt("position", position);

                var dataItem = DataVideos[position];
                if (dataItem.Type == Classes.ItemType.ReelsVideo)
                {
                    if (dataItem.VideoData != null)
                    {
                        bundle.PutString("DataItem", JsonConvert.SerializeObject(dataItem.VideoData));

                        ViewReelsVideoFragment viewReelsVideoFragment = new ViewReelsVideoFragment { Arguments = bundle };
                        return viewReelsVideoFragment;
                    }
                }
                else
                {
                    AdsFragment adsFragment = new AdsFragment();
                    return adsFragment;
                }

                return null;
            }
            catch (Exception a)
            {
                Methods.DisplayReportResultTrack(a);
                return null!;
            }
        }

        public override bool ContainsItem(long itemId)
        {
            try
            {
                return base.ContainsItem(itemId);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;
            }
        }

        public override long GetItemId(int position)
        {
            try
            {
                return position;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;
            }
        }
    }
}