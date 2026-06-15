using Android.Content;
using Androidx.Media3.Database;
using Androidx.Media3.Datasource;
using Androidx.Media3.Datasource.Cache;
using System;
using System.Threading.Tasks;
using WoWonder.Helpers.Utils;
using Object = Java.Lang.Object;
using Uri = Android.Net.Uri;

namespace WoWonder.MediaPlayers.Exo
{
    public class PreCachingExoPlayerVideo : Object, CacheWriter.IProgressListener, CacheDataSource.IEventListener
    {
        public static SimpleCache Cache;
        private readonly long ExoPlayerCacheSize = 90 * 1024 * 1024;
        public readonly CacheDataSource.Factory CacheDataSourceFactory;
        private readonly IDataSource XacheDataSource;

        public PreCachingExoPlayerVideo(Context context)
        {
            try
            {
                Cache ??= new SimpleCache(context.CacheDir, new LeastRecentlyUsedCacheEvictor(ExoPlayerCacheSize), new StandaloneDatabaseProvider(context));

                CacheDataSourceFactory = new CacheDataSource.Factory();
                CacheDataSourceFactory.SetCache(Cache);
                CacheDataSourceFactory.SetCacheKeyFactory(ICacheKeyFactory.Default);
                CacheDataSourceFactory.SetUpstreamDataSourceFactory(new DefaultHttpDataSource.Factory().SetUserAgent(AppSettings.ApplicationName));
                CacheDataSourceFactory.SetFlags(CacheDataSource.FlagIgnoreCacheOnError);
                CacheDataSourceFactory.SetEventListener(this);

                XacheDataSource = CacheDataSourceFactory.CreateDataSource();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void CacheVideosFiles(Uri videoUrl)
        {
            try
            {
                if (!PlayerSettings.EnableOfflineMode)
                    return;

                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        if (videoUrl.Path != null && videoUrl.Path.Contains(".mp4") && videoUrl.Path.Contains("http"))
                        {
                            var cacheDataSource = new CacheDataSource(Cache, XacheDataSource);

                            CacheWriter cacheWriter = new CacheWriter(cacheDataSource, new DataSpec(videoUrl), null, this);
                            cacheWriter.Cache();
                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public SimpleCache GetCache()
        {
            return Cache;
        }

        public void Destroy()
        {
            try
            {
                // Cache = null;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnProgress(long requestLength, long bytesCached, long newBytesCached)
        {
            var downloadPercentage = (bytesCached * 100.0 / requestLength);
            Console.WriteLine("downloadPercentage " + downloadPercentage);
            try
            {
                var BytehasKB = Math.Round((double)bytesCached / 1024, 0);
                var BytenewBytesCachedKB = Math.Round((double)newBytesCached / 1024, 0);

                Console.WriteLine("OnProgress downloadPercentage:" + downloadPercentage + " requestLength = " + requestLength + " BytesCached = " + BytehasKB + " newBytesCached = " + BytenewBytesCachedKB);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnCachedBytesRead(long bytesCached, long requestLength)
        {
            try
            {
                Console.WriteLine("OnCachedBytesRead:" + bytesCached);
                var downloadPercentage = (bytesCached * 100 / requestLength);

                var BytehasKB = Math.Round((double)bytesCached / 1024, 0);

                Console.WriteLine("OnCachedBytesRead downloadPercentage:" + downloadPercentage + " requestLength= " + requestLength + " BytesCached = " + BytehasKB);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnCacheIgnored(int p0)
        {
            Console.WriteLine("OnCacheIgnored:" + p0);
        }
    }
}