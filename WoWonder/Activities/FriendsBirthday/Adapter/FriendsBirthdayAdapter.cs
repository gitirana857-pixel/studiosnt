using Android.App;
using Android.Content.Res;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request;
using DE.Hdodenhof.CircleImageViewLib;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace WoWonder.Activities.FriendsBirthday.Adapter
{
    public class FriendsBirthdayAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<FriendsBirthdayAdapterClickEventArgs> ItemClick;
        public event EventHandler<FriendsBirthdayAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<UserDataObject> UserList = new ObservableCollection<UserDataObject>();

        public FriendsBirthdayAdapter(Activity activity)
        {
            try
            {
                HasStableIds = true;
                ActivityContext = activity;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => UserList?.Count ?? 0;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_FriendsBirthdayView
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_HContactMoreView, parent, false);
                var vh = new FriendsBirthdayAdapterViewHolder(itemView, Click, LongClick);
                return vh;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                if (viewHolder is FriendsBirthdayAdapterViewHolder holder)
                {
                    var item = UserList[position];
                    if (item != null)
                    {
                        GlideImageLoader.LoadImage(ActivityContext, item.Avatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);

                        holder.Name.Text = WoWonderTools.GetNameFinal(item);

                        if (AppSettings.FlowDirectionRightToLeft)
                            holder.Name.SetCompoundDrawablesWithIntrinsicBounds(item.Verified == "1" ? Resource.Drawable.icon_checkmark_small_vector : 0, 0, 0, 0);
                        else
                            holder.Name.SetCompoundDrawablesWithIntrinsicBounds(0, 0, item.Verified == "1" ? Resource.Drawable.icon_checkmark_small_vector : 0, 0);

                        //"birthday": "2007-05-05" >> 14 years old
                        holder.About.Text = WoWonderTools.GetAgeUser(item.Birthday) + " " + ActivityContext.GetText(Resource.String.Lbl_YearsOld);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void OnViewRecycled(Object holder)
        {
            try
            {
                if (ActivityContext?.IsDestroyed != false)
                    return;

                switch (holder)
                {
                    case FriendsBirthdayAdapterViewHolder viewHolder:
                        Glide.With(ActivityContext?.BaseContext).Clear(viewHolder.Image);
                        break;
                }
                base.OnViewRecycled(holder);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        public UserDataObject GetItem(int position)
        {
            return UserList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return position;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        public override int GetItemViewType(int position)
        {
            try
            {
                return position;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        private void Click(FriendsBirthdayAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(FriendsBirthdayAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = UserList[p0];
                switch (item)
                {
                    case null:
                        return Collections.SingletonList(p0);
                }

                if (item.Avatar != "")
                {
                    d.Add(item.Avatar);
                    return d;
                }

                return d;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return Collections.SingletonList(p0);
            }
        }

        public RequestBuilder GetPreloadRequestBuilder(Object p0)
        {
            return Glide.With(ActivityContext?.BaseContext).Load(p0.ToString()).Apply(new RequestOptions().CircleCrop().SetDiskCacheStrategy(DiskCacheStrategy.All));
        }

    }

    public class FriendsBirthdayAdapterViewHolder : RecyclerView.ViewHolder
    {
        public FriendsBirthdayAdapterViewHolder(View itemView, Action<FriendsBirthdayAdapterClickEventArgs> clickListener,
            Action<FriendsBirthdayAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = MainView.FindViewById<ImageView>(Resource.Id.card_pro_pic);
                Name = MainView.FindViewById<TextView>(Resource.Id.card_name);
                About = MainView.FindViewById<TextView>(Resource.Id.card_dist);
                ImageLastSeen = (CircleImageView)MainView.FindViewById(Resource.Id.ImageLastseen);
                ButtonMore = MainView.FindViewById<ImageView>(Resource.Id.more);

                ButtonMore.SetImageResource(Resource.Drawable.icon_birthday_cake_vector);
                ButtonMore.ImageTintList = ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));

                //Event 
                itemView.Click += (sender, e) => clickListener(new FriendsBirthdayAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new FriendsBirthdayAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Variables Basic

        public View MainView { get; }

        public ImageView Image { get; private set; }
        public TextView Name { get; private set; }
        public TextView About { get; private set; }
        public ImageView ButtonMore { get; private set; }
        public CircleImageView ImageLastSeen { get; private set; }

        #endregion
    }

    public class FriendsBirthdayAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }

}