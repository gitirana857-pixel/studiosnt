using Android.Views;
using AndroidX.RecyclerView.Widget;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using WoWonder.Activities.Comment;
using WoWonder.Activities.Comment.Adapters;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Helpers.Utils;
using static WoWonder.Activities.NativePost.Post.AdapterHolders;

namespace WoWonder.Activities.NativePost.Extra
{
    public class AsyncViewHoldersContainers
    {
        public RecyclerView XRecyclerView;
        public NativePostAdapter MainAdapter;
        public int LimitCachHolders;



        public ObservableCollection<PostBottomSectionViewHolder> ListOfBottomPostPartholders;

        public ObservableCollection<PostImageSectionViewHolder> ListOfImageholders;
        public ObservableCollection<PostVideoSectionViewHolder> ListOfVideoholders;

        public ObservableCollection<PostAddCommentSectionViewHolder> ListOfAddCommentholders;
        public ObservableCollection<CommentAdapterViewHolder> ListOfCommentSectionholders;

        public List<PostMultiImageViewHolder> List2Imageholders;
        public List<PostMultiImageViewHolder> List3Imageholders;
        public List<PostMultiImageViewHolder> List4Imageholders;
        public List<LinkPostViewHolder> ListLinkholders;
        public List<PostColorBoxSectionViewHolder> ListColorholders;
        public List<EventPostViewHolder> ListEventholders;
        public List<PostDividerSectionViewHolder> ListDeviderholders;


        public bool HighPreload;


        public AsyncViewHoldersContainers(RecyclerView recyclerview, NativePostAdapter adapter)
        {
            try
            {
                MainAdapter = adapter;
                XRecyclerView = recyclerview;


                ListOfBottomPostPartholders = new ObservableCollection<PostBottomSectionViewHolder>();
                ListOfCommentSectionholders = new ObservableCollection<CommentAdapterViewHolder>();
                ListOfAddCommentholders = new ObservableCollection<PostAddCommentSectionViewHolder>();
                ListOfVideoholders = new ObservableCollection<PostVideoSectionViewHolder>();
                List2Imageholders = new List<PostMultiImageViewHolder>();
                List3Imageholders = new List<PostMultiImageViewHolder>();
                List4Imageholders = new List<PostMultiImageViewHolder>();
                ListColorholders = new List<PostColorBoxSectionViewHolder>();
                ListLinkholders = new List<LinkPostViewHolder>();
                ListEventholders = new List<EventPostViewHolder>();
                ListDeviderholders = new List<PostDividerSectionViewHolder>();


                ListOfBottomPostPartholders.CollectionChanged += OnListChanged;

                ListOfImageholders = new ObservableCollection<PostImageSectionViewHolder>();
                ListOfImageholders.CollectionChanged += OnListImageViewHolderChanged;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetCachedLimitHolders(int limitCount, bool highPreload)
        {
            try
            {
                LimitCachHolders = limitCount;
                HighPreload = highPreload;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }



        public Task[] BacgroundTasksQueue = new Task[10];
        public Task[] BacgroundImageTasksQueue = new Task[14];




        public void Clear()
        {
            try
            {
                ListOfBottomPostPartholders.Clear();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void OnListChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            //StartGlobalCachHolders();
        }

        private void OnListImageViewHolderChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            try
            {
                int numberOfLoad = 4;

                if (HighPreload)
                    numberOfLoad = 8;


                if (BacgroundImageTasksQueue[8]?.Status != TaskStatus.Running && ListDeviderholders.Count < 6)
                {
                    BacgroundImageTasksQueue[8] = Task.Run(() =>
                    {

                        for (int i = 0; i <= 12; i++)
                        {
                            var itemView = LayoutInflater.From(XRecyclerView.Context)?.Inflate(Resource.Layout.Post_Devider, XRecyclerView, false);
                            ListDeviderholders.Add(new PostDividerSectionViewHolder(itemView));
                        }
                    });
                }



                Console.WriteLine("WoLog: OnListImageViewHolderChanged Started Count  " + ListOfImageholders.Count);

                if (BacgroundImageTasksQueue[1]?.Status != TaskStatus.Running && ListOfImageholders.Count < 6)
                {
                    BacgroundImageTasksQueue[1] = Task.Run(() =>
                    {

                        for (int i = 0; i <= numberOfLoad + 4; i++)
                        {
                            var itemView = LayoutInflater.From(XRecyclerView.Context)?.Inflate(Resource.Layout.Post_Content_Image_Layout, XRecyclerView, false);
                            var vh = new PostImageSectionViewHolder(itemView, MainAdapter, MainAdapter.PostClickListener, (int)PostModelType.ImagePost);

                            ListOfImageholders.Add(vh);
                        }
                    });
                }



                if (BacgroundImageTasksQueue[2]?.Status != TaskStatus.Running && ListOfVideoholders.Count < 4)
                {
                    BacgroundImageTasksQueue[2] = Task.Run(() =>
                    {

                        for (int i = 0; i <= numberOfLoad; i++)
                        {
                            var itemView = LayoutInflater.From(XRecyclerView.Context)?.Inflate(Resource.Layout.Post_Content_video_layout, XRecyclerView, false);
                            ListOfVideoholders.Add(new PostVideoSectionViewHolder(itemView, MainAdapter));
                        }
                    });
                }


                if (BacgroundImageTasksQueue[3]?.Status != TaskStatus.Running && List2Imageholders.Count < 3)
                {
                    BacgroundImageTasksQueue[3] = Task.Run(() =>
                    {

                        for (int i = 0; i <= numberOfLoad; i++)
                        {
                            var itemView = LayoutInflater.From(XRecyclerView.Context)?.Inflate(Resource.Layout.Post_Content_2Images_Layout, XRecyclerView, false);
                            List2Imageholders.Add(new PostMultiImageViewHolder(itemView, 2, MainAdapter, MainAdapter.PostClickListener));
                        }
                    });
                }

                if (BacgroundImageTasksQueue[4]?.Status != TaskStatus.Running && List3Imageholders.Count < 3)
                {
                    BacgroundImageTasksQueue[4] = Task.Run(() =>
                    {

                        for (int i = 0; i <= numberOfLoad; i++)
                        {
                            var itemView = LayoutInflater.From(XRecyclerView.Context)?.Inflate(Resource.Layout.Post_Content_3Images_Layout, XRecyclerView, false);
                            List3Imageholders.Add(new PostMultiImageViewHolder(itemView, 3, MainAdapter, MainAdapter.PostClickListener));
                        }
                    });
                }

                if (BacgroundImageTasksQueue[5]?.Status != TaskStatus.Running && List4Imageholders.Count < 3)
                {
                    BacgroundImageTasksQueue[5] = Task.Run(() =>
                    {

                        for (int i = 0; i <= numberOfLoad; i++)
                        {
                            var itemView = LayoutInflater.From(XRecyclerView.Context)?.Inflate(Resource.Layout.Post_Content_4Images_Layout, XRecyclerView, false);
                            List4Imageholders.Add(new PostMultiImageViewHolder(itemView, 4, MainAdapter, MainAdapter.PostClickListener));
                        }
                    });
                }

                if (BacgroundImageTasksQueue[6]?.Status != TaskStatus.Running && ListLinkholders.Count < 3)
                {
                    BacgroundImageTasksQueue[6] = Task.Run(() =>
                    {

                        for (int i = 0; i <= numberOfLoad; i++)
                        {
                            var itemView = LayoutInflater.From(XRecyclerView.Context)?.Inflate(Resource.Layout.Post_Content_Link_Layout, XRecyclerView, false);
                            ListLinkholders.Add(new LinkPostViewHolder(itemView, MainAdapter, MainAdapter.PostClickListener));
                        }
                    });
                }

                if (BacgroundImageTasksQueue[7]?.Status != TaskStatus.Running && ListLinkholders.Count < 3)
                {
                    BacgroundImageTasksQueue[7] = Task.Run(() =>
                    {

                        for (int i = 0; i <= numberOfLoad; i++)
                        {
                            var itemView = LayoutInflater.From(XRecyclerView.Context)?.Inflate(Resource.Layout.Post_Content_ColorBox_Layout, XRecyclerView, false);
                            ListColorholders.Add(new PostColorBoxSectionViewHolder(itemView));
                        }
                    });
                }


                if (BacgroundImageTasksQueue[9]?.Status != TaskStatus.Running && MainAdapter.ListOfTextSectionPostPartholders.Count < 5)
                {
                    BacgroundImageTasksQueue[9] = Task.Run(() =>
                    {

                        for (int i = 0; i <= numberOfLoad + 3; i++)
                        {
                            var itemView = LayoutInflater.From(XRecyclerView.Context)?.Inflate(Resource.Layout.Post_TextSection_Layout, XRecyclerView, false);
                            var vh = new PostTextSectionViewHolder(itemView, MainAdapter, MainAdapter.PostClickListener);
                            MainAdapter.ListOfTextSectionPostPartholders.Add(vh);
                        }
                    });
                }

                if (BacgroundImageTasksQueue[10]?.Status != TaskStatus.Running && MainAdapter.ListOfHeaderholders.Count < 5)
                {
                    BacgroundImageTasksQueue[10] = Task.Run(() =>
                    {

                        for (int i = 0; i <= numberOfLoad + 3; i++)
                        {
                            var itemView = LayoutInflater.From(XRecyclerView.Context)?.Inflate(Resource.Layout.Post_TopSection_Layout, XRecyclerView, false);
                            var vh = new PostTopSectionViewHolder(itemView, MainAdapter, MainAdapter.PostClickListener);

                            MainAdapter.ListOfHeaderholders.Add(vh);
                        }

                        for (int i = 0; i <= numberOfLoad + 3; i++)
                        {
                            var itemView = LayoutInflater.From(XRecyclerView.Context)?.Inflate(Resource.Layout.Post_ButtomSection_Layout, XRecyclerView, false);
                            var vh = new PostBottomSectionViewHolder(itemView, MainAdapter, MainAdapter.PostClickListener);
                            ListOfBottomPostPartholders.Add(vh);
                        }


                    });
                }







                Console.WriteLine("WoLog: OnListImageViewHolderChanged Ended Count  " + ListOfImageholders.Count);
            }
            catch (Exception ex)
            {

                Console.WriteLine("WoLog: OnListImageViewHolderChanged Craching === " + ex);
            }

        }


        //public Task.Factory BackgroundTasksQueue;

        public void StartGlobalCachHolders()
        {


            try
            {
                if (BacgroundTasksQueue[1]?.Status == TaskStatus.Running || BacgroundTasksQueue[4]?.Status == TaskStatus.Running)
                    return;

                if (ListOfBottomPostPartholders.Count > 7)
                    return;

                Console.WriteLine("WoLog: StartGlobalCachHolders Count  " + ListOfBottomPostPartholders.Count);
                BacgroundImageTasksQueue[1] = Task.Run(() =>
                {

                    for (int i = 0; i <= LimitCachHolders; i++)
                    {
                        var itemView = LayoutInflater.From(XRecyclerView.Context)?.Inflate(Resource.Layout.Post_TopSection_Layout, XRecyclerView, false);
                        var vh = new PostTopSectionViewHolder(itemView, MainAdapter, MainAdapter.PostClickListener);

                        MainAdapter.ListOfHeaderholders.Add(vh);
                    }

                    for (int i = 0; i <= LimitCachHolders; i++)
                    {
                        var itemView = LayoutInflater.From(XRecyclerView.Context)?.Inflate(Resource.Layout.Post_ButtomSection_Layout, XRecyclerView, false);
                        var vh = new PostBottomSectionViewHolder(itemView, MainAdapter, MainAdapter.PostClickListener);
                        ListOfBottomPostPartholders.Add(vh);

                    }


                });


                BacgroundImageTasksQueue[2] = Task.Run(() =>
                {

                    for (int i = 0; i <= LimitCachHolders; i++)
                    {
                        var itemView = LayoutInflater.From(XRecyclerView.Context)?.Inflate(Resource.Layout.Post_Devider, XRecyclerView, false);
                        ListDeviderholders.Add(new PostDividerSectionViewHolder(itemView));
                    }

                    for (int i = 0; i <= LimitCachHolders; i++)
                    {
                        var itemView = LayoutInflater.From(XRecyclerView.Context)?.Inflate(Resource.Layout.Post_TextSection_Layout, XRecyclerView, false);
                        var vh = new PostTextSectionViewHolder(itemView, MainAdapter, MainAdapter.PostClickListener);
                        MainAdapter.ListOfTextSectionPostPartholders.Add(vh);
                    }

                });


                BacgroundImageTasksQueue[3] = Task.Run(() =>
                {

                    for (int i = 0; i <= LimitCachHolders; i++)
                    {
                        var itemView = LayoutInflater.From(XRecyclerView.Context)?.Inflate(Resource.Layout.Post_Content_Image_Layout, XRecyclerView, false);
                        var vh = new PostImageSectionViewHolder(itemView, MainAdapter, MainAdapter.PostClickListener, (int)PostModelType.ImagePost);

                        ListOfImageholders.Add(vh);
                    }



                    for (int i = 0; i <= 6; i++)
                    {
                        var itemView = LayoutInflater.From(XRecyclerView.Context)?.Inflate(Resource.Layout.Post_Content_video_layout, XRecyclerView, false);
                        var vh = new PostVideoSectionViewHolder(itemView, MainAdapter);
                        ListOfVideoholders.Add(vh);
                    }


                    // To be removed Later Elin Doughouz
                    for (int i = 0; i <= 30; i++)
                    {
                        var itemView = LayoutInflater.From(XRecyclerView.Context)?.Inflate(Resource.Layout.Post_Content_Null_Layout, XRecyclerView, false);
                        var vh = new PostDefaultSectionViewHolder(itemView);

                        MainAdapter.ListOfholders.Add(vh);
                    }
                });


                BacgroundTasksQueue[4] = Task.Run(() =>
                {

                    for (int i = 0; i <= 5; i++)
                    {
                        var itemView = LayoutInflater.From(XRecyclerView.Context)?.Inflate(Resource.Layout.Post_Content_Comment_Section, XRecyclerView, false);
                        var vh = new CommentAdapterViewHolder(itemView, new CommentAdapter(MainAdapter.ActivityContext), new CommentClickListener(MainAdapter.ActivityContext, "Comment"), "Post");

                        ListOfCommentSectionholders.Add(vh);
                    }

                    for (int i = 0; i <= 5; i++)
                    {
                        var itemView = LayoutInflater.From(XRecyclerView.Context)?.Inflate(Resource.Layout.Post_Content_AddComment_Section, XRecyclerView, false);
                        var vh = new PostAddCommentSectionViewHolder(itemView, MainAdapter, MainAdapter.PostClickListener);

                        ListOfAddCommentholders.Add(vh);
                    }
                });


                Console.WriteLine("WoLog: StartGlobalCachHolders Ended Count  " + ListOfBottomPostPartholders.Count);

                LimitCachHolders = 10;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}