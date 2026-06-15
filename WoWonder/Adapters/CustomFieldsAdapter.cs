using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.Dialog;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;

namespace WoWonder.Adapters
{
    public class CustomFieldsAdapter : RecyclerView.Adapter
    {
        public event EventHandler<CustomFieldsAdapterClickEventArgs> ItemClick;
        public event EventHandler<CustomFieldsAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;

        public ObservableCollection<CustomField> FieldList = new ObservableCollection<CustomField>();

        public CustomFieldsAdapter(Activity context)
        {
            try
            {
                ActivityContext = context;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => FieldList?.Count ?? 0;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_HPage_view
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.ViewSub_CustomFields_EditText, parent, false);
                var vh = new CustomFieldsAdapterViewHolder(itemView, Click, LongClick);
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
                    case CustomFieldsAdapterViewHolder holder:
                        {
                            var item = FieldList[position];
                            if (item != null)
                            {
                                holder.DescriptionOfField.Text = Methods.FunString.DecodeString(item.Description);

                                switch (item.Type)
                                {
                                    case "selectbox":
                                        {
                                            holder.EdtField1.Hint = Methods.FunString.DecodeString(item.Name);
                                            holder.EdtField1.Text = Methods.FunString.DecodeString(item.FieldAnswer);

                                            holder.EdtField1.Visibility = ViewStates.Visible;
                                            holder.EdtField2.Visibility = ViewStates.Gone;

                                            Methods.SetFocusable(holder.EdtField1);

                                            holder.EdtField1.Touch += (sender, args) =>
                                            {
                                                try
                                                {
                                                    if (args.Event.Action != MotionEventActions.Down) return;

                                                    var arrayAdapter = item.Options.Split(',').ToList();

                                                    var dialogList = new MaterialAlertDialogBuilder(ActivityContext);
                                                    dialogList.SetTitle(Methods.FunString.DecodeString(item.Name));
                                                    dialogList.SetItems(arrayAdapter.ToArray(), (o, eventArgs) =>
                                                    {
                                                        try
                                                        {
                                                            var text = arrayAdapter[eventArgs.Which] ?? "";

                                                            holder.EdtField1.Text = text;
                                                            item.FieldAnswer = text;
                                                        }
                                                        catch (Exception e)
                                                        {
                                                            Methods.DisplayReportResultTrack(e);
                                                        }
                                                    });
                                                    dialogList.SetNegativeButton(ActivityContext.GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                                                    dialogList.Show();
                                                }
                                                catch (Exception e)
                                                {
                                                    Methods.DisplayReportResultTrack(e);
                                                }
                                            };

                                            break;
                                        }
                                    case "textbox":
                                        {
                                            holder.EdtField1.Hint = Methods.FunString.DecodeString(item.Name);
                                            holder.EdtField1.Text = Methods.FunString.DecodeString(item.FieldAnswer);

                                            holder.EdtField1.Visibility = ViewStates.Visible;
                                            holder.EdtField2.Visibility = ViewStates.Gone;

                                            break;
                                        }
                                    case "textarea":
                                        {
                                            holder.EdtField2.Hint = Methods.FunString.DecodeString(item.Name);
                                            holder.EdtField2.Text = Methods.FunString.DecodeString(item.FieldAnswer);

                                            holder.EdtField1.Visibility = ViewStates.Gone;
                                            holder.EdtField2.Visibility = ViewStates.Visible;

                                            break;
                                        }
                                }

                                holder.EdtField1.TextChanged += (sender, args) =>
                                {
                                    try
                                    {
                                        item.FieldAnswer = args.Text.ToString();
                                    }
                                    catch (Exception e)
                                    {
                                        Methods.DisplayReportResultTrack(e);
                                    }
                                };

                                holder.EdtField2.TextChanged += (sender, args) =>
                                {
                                    try
                                    {
                                        item.FieldAnswer = args.Text.ToString();
                                    }
                                    catch (Exception e)
                                    {
                                        Methods.DisplayReportResultTrack(e);
                                    }
                                };
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

        public CustomField GetItem(int position)
        {
            return FieldList[position];
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

        private void Click(CustomFieldsAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(CustomFieldsAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }

    }

    public class CustomFieldsAdapterViewHolder : RecyclerView.ViewHolder
    {
        public CustomFieldsAdapterViewHolder(View itemView, Action<CustomFieldsAdapterClickEventArgs> clickListener, Action<CustomFieldsAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;
                DescriptionOfField = MainView.FindViewById<TextView>(Resource.Id.DescriptionOfFieldTextView);

                EdtField1 = MainView.FindViewById<EditText>(Resource.Id.FieldEditText1);
                EdtField2 = MainView.FindViewById<EditText>(Resource.Id.FieldEditText2);

                Methods.SetColorEditText(EdtField1, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(EdtField2, WoWonderTools.IsTabDark() ? Color.White : Color.Black);

                //Event  
                itemView.Click += (sender, e) => clickListener(new CustomFieldsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new CustomFieldsAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Variables Basic

        public View MainView { get; }

        public TextView DescriptionOfField { get; private set; }

        public EditText EdtField1 { get; private set; }
        public EditText EdtField2 { get; private set; }

        #endregion
    }

    public class CustomFieldsAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}