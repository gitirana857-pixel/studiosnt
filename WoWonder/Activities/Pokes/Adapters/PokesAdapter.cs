using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using DE.Hdodenhof.CircleImageViewLib;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Pocks;
using Exception = System.Exception;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace WoWonder.Activities.Pokes.Adapters
{
    public class PokesAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {

        public event EventHandler<PokesAdapterClickEventArgs> PokeItemClick;
        public event EventHandler<PokesAdapterClickEventArgs> ItemClick;
        public event EventHandler<PokesAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<PokeObject.Datum> PokeList = new ObservableCollection<PokeObject.Datum>();

        public PokesAdapter(Activity activity)
        {
            try
            {
                ActivityContext = activity;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => PokeList?.Count ?? 0;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_HContact_view
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_HContactView, parent, false);
                var vh = new PokesAdapterViewHolder(itemView, PokeClick, Click, LongClick);
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
                switch (viewHolder)
                {
                    case PokesAdapterViewHolder holder:
                        {
                            var item = PokeList[position];
                            if (item.UserData?.UserDataClass != null)
                            {
                                GlideImageLoader.LoadImage(ActivityContext, item.UserData?.UserDataClass.Avatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser, true);
                                holder.Name.Text = Methods.FunString.SubStringCutOf(WoWonderTools.GetNameFinal(item.UserData?.UserDataClass), 20);

                                if (AppSettings.FlowDirectionRightToLeft)
                                    holder.Name.SetCompoundDrawablesWithIntrinsicBounds(item.UserData?.UserDataClass.Verified == "1" ? Resource.Drawable.icon_checkmark_small_vector : 0, 0, 0, 0);
                                else
                                    holder.Name.SetCompoundDrawablesWithIntrinsicBounds(0, 0, item.UserData?.UserDataClass.Verified == "1" ? Resource.Drawable.icon_checkmark_small_vector : 0, 0);

                                holder.About.Text = Methods.Time.TimeAgo(Convert.ToInt32(item.UserData?.UserDataClass.LastseenUnixTime), false);
                            }
                            break;
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
                    case PokesAdapterViewHolder viewHolder:
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

        public PokeObject.Datum GetItem(int position)
        {
            return PokeList[position];
        }


        private void PokeClick(PokesAdapterClickEventArgs args)
        {
            PokeItemClick?.Invoke(this, args);
        }

        private void Click(PokesAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(PokesAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = PokeList[p0];
                switch (item)
                {
                    case null:
                        return Collections.SingletonList(p0);
                }

                if (item.UserData?.UserDataClass != null && item.UserData.Value.UserDataClass.Avatar != "")
                {
                    d.Add(item.UserData.Value.UserDataClass.Avatar);
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
            return GlideImageLoader.GetPreLoadRequestBuilder(ActivityContext, p0.ToString(), ImageStyle.CircleCrop);
        }


    }

    public class PokesAdapterViewHolder : RecyclerView.ViewHolder
    {
        public PokesAdapterViewHolder(View itemView, Action<PokesAdapterClickEventArgs> pokeButtonClickListener, Action<PokesAdapterClickEventArgs> clickListener, Action<PokesAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = MainView.FindViewById<ImageView>(Resource.Id.card_pro_pic);
                Name = MainView.FindViewById<TextView>(Resource.Id.card_name);
                About = MainView.FindViewById<TextView>(Resource.Id.card_dist);
                Button = MainView.FindViewById<AppCompatButton>(Resource.Id.cont);
                ImageLastSeen = (CircleImageView)MainView.FindViewById(Resource.Id.ImageLastseen);

                Button.SetBackgroundResource(Resource.Drawable.round_button_pressed);
                Button.SetTextColor(Color.White);
                Button.Text = MainView.Context?.GetText(Resource.String.Lbl_PokeBack);

                ImageLastSeen.Visibility = ViewStates.Visible;
                ImageLastSeen.SetColorFilter(Color.ParseColor("#FFAE35"));

                //Event
                Button.Click += (sender, e) => pokeButtonClickListener(new PokesAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.Click += (sender, e) => clickListener(new PokesAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new PokesAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
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
        public AppCompatButton Button { get; private set; }
        public CircleImageView ImageLastSeen { get; private set; }

        #endregion
    }

    public class PokesAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}