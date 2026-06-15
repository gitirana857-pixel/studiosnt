using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using System;
using System.Collections.ObjectModel;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Address;

namespace WoWonder.Activities.Address.Adapters
{
    public class AddressAdapter : RecyclerView.Adapter
    {
        public event EventHandler<AddressAdapterClickEventArgs> EditItemClick;
        public event EventHandler<AddressAdapterClickEventArgs> DeleteItemClick;
        public event EventHandler<AddressAdapterClickEventArgs> ItemClick;
        public event EventHandler<AddressAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;

        public ObservableCollection<AddressDataObject> AddressList = new ObservableCollection<AddressDataObject>();
        private readonly string TypeSystem = "Edit";  //Edit , Select

        public AddressAdapter(Activity context, string typeSystem = "Edit")
        {
            try
            {
                //HasStableIds = true;
                ActivityContext = context;
                TypeSystem = typeSystem;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => AddressList?.Count ?? 0;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_AddressesView
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_AddressView, parent, false);
                var vh = new AddressAdapterViewHolder(itemView, EditClick, DeleteClick, Click, LongClick);
                return vh;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null;
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                if (viewHolder is AddressAdapterViewHolder holder)
                {
                    var item = AddressList[position];
                    if (item != null)
                    {
                        holder.Name.Text = item.Name;

                        holder.Phone.Text = item.Phone;
                        holder.Address.Text = item.Address;
                        holder.Country.Text = item.Country + " / " + item.City;

                        if (TypeSystem == "Edit")
                        {
                            holder.CheckBox.Visibility = ViewStates.Gone;

                            holder.Edit.Visibility = ViewStates.Visible;
                            holder.Delete.Visibility = ViewStates.Visible;
                        }
                        else if (TypeSystem == "Select")
                        {
                            holder.CheckBox.Visibility = ViewStates.Visible;

                            holder.Edit.Visibility = ViewStates.Gone;
                            holder.Delete.Visibility = ViewStates.Gone;

                            holder.CheckBox.Checked = item.Selected;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public AddressDataObject GetItem(int position)
        {
            return AddressList[position];
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

        private void EditClick(AddressAdapterClickEventArgs args)
        {
            EditItemClick?.Invoke(this, args);
        }

        private void DeleteClick(AddressAdapterClickEventArgs args)
        {
            DeleteItemClick?.Invoke(this, args);
        }

        private void Click(AddressAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(AddressAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }
    }

    public class AddressAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; }

        public TextView Name { get; private set; }
        public TextView Edit { get; private set; }
        public TextView Delete { get; private set; }
        public TextView Phone { get; private set; }
        public TextView Address { get; private set; }
        public TextView Country { get; private set; }
        public CheckBox CheckBox { get; private set; }

        #endregion

        public AddressAdapterViewHolder(View itemView, Action<AddressAdapterClickEventArgs> editClickListener, Action<AddressAdapterClickEventArgs> deleteClickListener, Action<AddressAdapterClickEventArgs> clickListener, Action<AddressAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Name = MainView.FindViewById<TextView>(Resource.Id.name);
                Edit = MainView.FindViewById<TextView>(Resource.Id.edit);
                Delete = MainView.FindViewById<TextView>(Resource.Id.delete);
                Phone = MainView.FindViewById<TextView>(Resource.Id.phone);
                Address = MainView.FindViewById<TextView>(Resource.Id.address);
                Country = MainView.FindViewById<TextView>(Resource.Id.country);
                CheckBox = MainView.FindViewById<CheckBox>(Resource.Id.check);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, Edit, FontAwesomeIcon.Edit);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, Delete, FontAwesomeIcon.TrashAlt);

                //Event  
                Edit.Click += (sender, e) => editClickListener(new AddressAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                Delete.Click += (sender, e) => deleteClickListener(new AddressAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.Click += (sender, e) => clickListener(new AddressAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new AddressAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

    }

    public class AddressAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}