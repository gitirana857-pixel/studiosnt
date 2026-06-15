using AndroidX.Core.Widget;
using System;
using Object = Java.Lang.Object;

namespace WoWonder.Helpers.Utils
{
    public class NestedScrollViewOnScroll : Object, NestedScrollView.IOnScrollChangeListener
    {
        public delegate void LoadMoreEventHandler(object sender, EventArgs e);

        public event LoadMoreEventHandler LoadMoreEvent;
        public bool IsLoading { get; set; }

        public void OnScrollChange(NestedScrollView v, int scrollX, int scrollY, int oldScrollX, int oldScrollY)
        {
            try
            {
                if (v.GetChildAt(v.ChildCount - 1) != null)
                {
                    if ((scrollY >= v.GetChildAt(v.ChildCount - 1).MeasuredHeight - v.MeasuredHeight && scrollY > oldScrollY))
                    {
                        //code to fetch more data for endless scrolling 
                        if (IsLoading)
                            return;

                        LoadMoreEvent?.Invoke(this, null);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}