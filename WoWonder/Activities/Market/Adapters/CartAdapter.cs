using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Product;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace WoWonder.Activities.Market.Adapters
{
    public class CartAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<CartAdapterClickEventArgs> OnSelectQtyItemClick;
        public event EventHandler<CartAdapterClickEventArgs> OnRemoveButtItemClick;
        public event EventHandler<CartAdapterClickEventArgs> ItemClick;
        public event EventHandler<CartAdapterClickEventArgs> ItemLongClick;
        private readonly Activity ActivityContext;
        public ObservableCollection<ProductDataObject> CartsList = new ObservableCollection<ProductDataObject>();

        public CartAdapter(Activity context)
        {
            try
            {
                ActivityContext = context;
                HasStableIds = true;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_CartView
                View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_CartView, parent, false);
                var vh = new CartAdapterViewHolder(itemView, SelectQtyButtonClick, RemoveButtonClick, Click, LongClick);
                return vh;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null;
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                if (viewHolder is CartAdapterViewHolder holder)
                {
                    var item = CartsList[position];
                    if (item != null)
                    {
                        var image = item.Images.FirstOrDefault()?.Image;
                        GlideImageLoader.LoadImage(ActivityContext, image, holder.Image, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);

                        holder.Name.Text = Methods.FunString.DecodeString(item.Name);
                        holder.Username.Text = WoWonderTools.GetNameFinal(item.UserData);

                        holder.CountQty.Text = ActivityContext.GetText(Resource.String.Lbl_Qty) + " : " + item.Units;
                        holder.Price.Text = "$ " + item.PriceFormat;

                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => CartsList?.Count ?? 0;

        public ProductDataObject GetItem(int position)
        {
            return CartsList[position];
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

        public override int GetItemViewType(int position)
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

        void SelectQtyButtonClick(CartAdapterClickEventArgs args) => OnSelectQtyItemClick?.Invoke(this, args);
        void RemoveButtonClick(CartAdapterClickEventArgs args) => OnRemoveButtItemClick?.Invoke(this, args);
        void Click(CartAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void LongClick(CartAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = CartsList[p0];

                if (item == null)
                    return Collections.SingletonList(p0);

                var image = item.Images.FirstOrDefault()?.Image;
                if (!string.IsNullOrEmpty(image))
                {
                    d.Add(image);
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
            return Glide.With(ActivityContext?.BaseContext).Load(p0.ToString())
                .Apply(new RequestOptions().CircleCrop());
        }
    }

    public class CartAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }

        public ImageView Image { get; private set; }
        public TextView Name { get; private set; }
        public TextView Username { get; private set; }
        public ImageButton IconDelete { get; private set; }
        public TextView CountQty { get; private set; }
        public ImageView SelectQty { get; private set; }
        public TextView Price { get; private set; }

        #endregion

        public CartAdapterViewHolder(View itemView, Action<CartAdapterClickEventArgs> selectQtyClickListener, Action<CartAdapterClickEventArgs> removeButtonClickListener, Action<CartAdapterClickEventArgs> clickListener, Action<CartAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                //Get values
                Image = MainView.FindViewById<ImageView>(Resource.Id.image);
                Name = MainView.FindViewById<TextView>(Resource.Id.name);
                Username = MainView.FindViewById<TextView>(Resource.Id.username);
                IconDelete = MainView.FindViewById<ImageButton>(Resource.Id.delete);
                CountQty = MainView.FindViewById<TextView>(Resource.Id.countQty);
                SelectQty = MainView.FindViewById<ImageView>(Resource.Id.selectQty);
                Price = MainView.FindViewById<TextView>(Resource.Id.price);

                //Create an Event
                IconDelete.Click += (sender, e) => removeButtonClickListener(new CartAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition, CountQty = CountQty });
                SelectQty.Click += (sender, e) => selectQtyClickListener(new CartAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition, CountQty = CountQty });

                itemView.Click += (sender, e) => clickListener(new CartAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition, CountQty = CountQty });
                itemView.LongClick += (sender, e) => longClickListener(new CartAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition, CountQty = CountQty });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class CartAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
        public TextView CountQty { get; set; }
    }
}