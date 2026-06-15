using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Content.Res;
using AndroidX.RecyclerView.Widget;
using Com.Google.Android.Gms.Ads.Admanager;
using Google.Android.Material.Dialog;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WoWonder.Activities.Base;
using WoWonder.Activities.Offers.Adapters;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Offers;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Requests;
using Console = System.Console;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Offers
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class EditOffersActivity : BaseActivity, IDialogListCallBack, View.IOnClickListener
    {
        #region Variables Basic

        private TextView TxtSave;
        private LinearLayout LayoutImage, LayoutDiscountItems, LayoutCurrency, LayoutDate, LayoutTime;
        private TextView IconDiscountType, IconDiscountItems, IconCurrency, IconDescription, IconDate, IconTime;
        private EditText TxtDiscountType, TxtDiscountItems, TxtCurrency, TxtDate, TxtTime, TxtDescription;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private DiscountTypeAdapter MAdapter;
        private string TypeDialog, OfferId, AddDiscountId;
        private OfferObject OfferClass;
        private AdManagerAdView AdManagerAdView;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.CreateOffersLayout);

                OfferId = Intent?.GetStringExtra("OfferId") ?? "";

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                Get_DataOffer();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                AddOrRemoveEvent(true);
                AdsGoogle.LifecycleAdManagerAdView(AdManagerAdView, "Resume");
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnPause()
        {
            try
            {
                base.OnPause();
                AddOrRemoveEvent(false);
                AdsGoogle.LifecycleAdManagerAdView(AdManagerAdView, "Pause");
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        protected override void OnDestroy()
        {
            try
            {
                DestroyBasic();
                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
        #endregion

        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                TxtSave = FindViewById<TextView>(Resource.Id.toolbar_title);
                TxtSave.Text = GetText(Resource.String.Lbl_Save);

                LayoutImage = FindViewById<LinearLayout>(Resource.Id.LayoutImage);
                LayoutImage.Visibility = ViewStates.Gone;

                IconDiscountType = FindViewById<TextView>(Resource.Id.IconDiscountType);
                TxtDiscountType = FindViewById<EditText>(Resource.Id.DiscountTypeEditText);

                MRecycler = FindViewById<RecyclerView>(Resource.Id.Recyler);
                MRecycler.Visibility = ViewStates.Gone;

                LayoutDiscountItems = FindViewById<LinearLayout>(Resource.Id.LayoutDiscountItems);
                IconDiscountItems = FindViewById<TextView>(Resource.Id.IconDiscountItems);
                TxtDiscountItems = FindViewById<EditText>(Resource.Id.DiscountItemsEditText);
                LayoutDiscountItems.Visibility = ViewStates.Gone;

                LayoutCurrency = FindViewById<LinearLayout>(Resource.Id.LayoutCurrency);
                IconCurrency = FindViewById<TextView>(Resource.Id.IconCurrency);
                TxtCurrency = FindViewById<EditText>(Resource.Id.CurrencyEditText);
                LayoutCurrency.Visibility = ViewStates.Gone;

                IconDescription = FindViewById<TextView>(Resource.Id.IconDescription);
                TxtDescription = FindViewById<EditText>(Resource.Id.DescriptionEditText);

                LayoutDate = FindViewById<LinearLayout>(Resource.Id.LayoutDate);
                IconDate = FindViewById<TextView>(Resource.Id.IconDate);
                TxtDate = FindViewById<EditText>(Resource.Id.DateEditText);
                LayoutDate.Visibility = ViewStates.Gone;

                LayoutTime = FindViewById<LinearLayout>(Resource.Id.LayoutTime);
                IconTime = FindViewById<TextView>(Resource.Id.IconTime);
                TxtTime = FindViewById<EditText>(Resource.Id.TimeEditText);
                LayoutTime.Visibility = ViewStates.Gone;

                AdManagerAdView = FindViewById<AdManagerAdView>(Resource.Id.multiple_ad_sizes_view);
                AdsGoogle.InitAdManagerAdView(AdManagerAdView);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconDiscountType, FontAwesomeIcon.User);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconDiscountItems, FontAwesomeIcon.MapMarkedAlt);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconCurrency, FontAwesomeIcon.DollarSign);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconDescription, FontAwesomeIcon.Paragraph);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconDate, FontAwesomeIcon.CalendarAlt);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconTime, FontAwesomeIcon.Clock);

                Methods.SetColorEditText(TxtDiscountType, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                //Methods.SetColorEditText(TxtDiscountItems, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtDescription, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                //Methods.SetColorEditText(TxtDate, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                //Methods.SetColorEditText(TxtTime, WoWonderTools.IsTabDark() ? Color.White : Color.Black);

                Methods.SetFocusable(TxtDiscountType);
                Methods.SetFocusable(TxtDate);
                Methods.SetFocusable(TxtTime);

                //TxtDate.SetOnClickListener(this);
                //TxtTime.SetOnClickListener(this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitToolbar()
        {
            try
            {
                var toolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolBar != null)
                {
                    toolBar.Title = GetText(Resource.String.Lbl_EditOffers);
                    toolBar.SetTitleTextColor(WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                    SetSupportActionBar(toolBar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
                    var icon = AppCompatResources.GetDrawable(this, AppSettings.FlowDirectionRightToLeft ? Resource.Drawable.icon_back_arrow_right : Resource.Drawable.icon_back_arrow_left);
                    icon?.SetTint(WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                    SupportActionBar.SetHomeAsUpIndicator(icon);

                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetRecyclerViewAdapters()
        {
            try
            {
                LayoutManager = new LinearLayoutManager(this);
                MAdapter = new DiscountTypeAdapter(this) { DiscountList = new ObservableCollection<DiscountOffers>() };
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.SetAdapter(MAdapter);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                switch (addEvent)
                {
                    // true +=  // false -=
                    case true:
                        TxtSave.Click += TxtSaveOnClick;
                        TxtDiscountType.Touch += TxtDiscountTypeOnTouch;
                        break;
                    default:
                        TxtSave.Click -= TxtSaveOnClick;
                        TxtDiscountType.Touch -= TxtDiscountTypeOnTouch;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void DestroyBasic()
        {
            try
            {
                AdsGoogle.LifecycleAdManagerAdView(AdManagerAdView, "Destroy");

                MAdapter = null!;
                TxtSave = null!;
                IconDiscountType = null!;
                TxtDiscountType = null!;
                MRecycler = null!;
                IconDiscountItems = null!;
                TxtDiscountItems = null!;
                IconCurrency = null!;
                TxtCurrency = null!;
                IconDescription = null!;
                TxtDescription = null!;
                IconDate = null!;
                TxtDate = null!;
                IconTime = null!;
                TxtTime = null!;
                OfferClass = null!;
                TypeDialog = null!;
                OfferId = null!;
                AddDiscountId = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        #endregion

        #region Events

        private void TxtCurrencyOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Up) return;

                if (ListUtils.SettingsSiteList?.CurrencySymbolArray.CurrencyList != null)
                {
                    TypeDialog = "Currency";

                    var arrayAdapter = WoWonderTools.GetCurrencySymbolList();
                    switch (arrayAdapter?.Count)
                    {
                        case > 0:
                            {
                                var dialogList = new MaterialAlertDialogBuilder(this);

                                dialogList.SetTitle(GetText(Resource.String.Lbl_SelectCurrency));
                                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                                dialogList.Show();
                                break;
                            }
                    }
                }
                else
                {
                    Methods.DisplayReportResult(this, "Not have List Currency");
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtDiscountTypeOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Up) return;

                TypeDialog = "DiscountOffersAdapter";

                var dialogList = new MaterialAlertDialogBuilder(this);
                var arrayAdapter = WoWonderTools.GetAddDiscountList(this).Select(pair => pair.Value).ToList();
                dialogList.SetTitle(GetText(Resource.String.Lbl_DiscountType));
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void TxtSaveOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    if (string.IsNullOrEmpty(TxtDiscountType.Text) || string.IsNullOrEmpty(TxtDescription.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_enter_your_data), ToastLength.Short);
                        return;
                    }

                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                    OfferObject newInfoObject = new OfferObject
                    {
                        Id = OfferId,
                        DiscountType = AddDiscountId,
                        Description = TxtDescription.Text,
                    };

                    var dictionary = new Dictionary<string, string>
                    {
                        {"discount_type", AddDiscountId},
                        {"offer_id", OfferId},
                        {"description", TxtDescription.Text},
                    };

                    switch (MAdapter.DiscountList.Count)
                    {
                        case > 0:
                            {
                                foreach (var discount in MAdapter.DiscountList)
                                {
                                    switch (discount)
                                    {
                                        case null:
                                            continue;
                                        default:
                                            switch (discount.DiscountType)
                                            {
                                                case "discount_percent":
                                                    dictionary.Add("discount_percent", discount.DiscountFirst);

                                                    newInfoObject.DiscountPercent = discount.DiscountFirst;
                                                    break;
                                                case "discount_amount":
                                                    dictionary.Add("discount_amount", discount.DiscountFirst);

                                                    newInfoObject.DiscountAmount = discount.DiscountFirst;
                                                    break;
                                                case "buy_get_discount":
                                                    dictionary.Add("discount_percent", discount.DiscountFirst);
                                                    dictionary.Add("buy", discount.DiscountSec);
                                                    dictionary.Add("get", discount.DiscountThr);

                                                    newInfoObject.DiscountPercent = discount.DiscountFirst;
                                                    newInfoObject.Buy = discount.DiscountSec;
                                                    newInfoObject.GetPrice = discount.DiscountThr;
                                                    break;
                                                case "spend_get_off":
                                                    dictionary.Add("spend", discount.DiscountSec);
                                                    dictionary.Add("amount_off", discount.DiscountThr);

                                                    newInfoObject.Spend = discount.DiscountSec;
                                                    newInfoObject.AmountOff = discount.DiscountThr;
                                                    break;
                                                case "free_shipping": //Not have tag
                                                    break;
                                            }

                                            break;
                                    }
                                }

                                break;
                            }
                    }

                    var (apiStatus, respond) = await RequestsAsync.Offers.EditOfferAsync(dictionary);
                    switch (apiStatus)
                    {
                        case 200:
                            {
                                switch (respond)
                                {
                                    case MessageOfferObject result:
                                        {
                                            Console.WriteLine(result.MessageData);
                                            ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_OfferSuccessfullyAdded), ToastLength.Short);

                                            AndHUD.Shared.Dismiss();

                                            var data = OffersActivity.GetInstance()?.MAdapter?.OffersList?.FirstOrDefault(a => a.Id == newInfoObject.Id);
                                            if (data != null)
                                            {
                                                data.DiscountType = AddDiscountId;
                                                data.ExpireDate = TxtDate.Text;
                                                data.Time = TxtTime.Text;
                                                data.Description = TxtDescription.Text;
                                                data.DiscountedItems = TxtDiscountItems.Text;
                                                data.Description = TxtDescription.Text;
                                                data.DiscountPercent = newInfoObject.DiscountPercent;
                                                data.DiscountAmount = newInfoObject.DiscountAmount;
                                                data.DiscountPercent = newInfoObject.DiscountPercent;
                                                data.Buy = newInfoObject.Buy;
                                                data.GetPrice = newInfoObject.GetPrice;
                                                data.Spend = newInfoObject.Spend;
                                                data.AmountOff = newInfoObject.AmountOff;

                                                OffersActivity.GetInstance().MAdapter.NotifyItemChanged(OffersActivity.GetInstance().MAdapter.OffersList.IndexOf(data));
                                            }

                                            Intent intent = new Intent();
                                            intent.PutExtra("OffersItem", JsonConvert.SerializeObject(newInfoObject));
                                            SetResult(Result.Ok, intent);
                                            Finish();
                                            break;
                                        }
                                }

                                break;
                            }
                        default:
                            Methods.DisplayAndHudErrorResult(this, respond);
                            break;
                    }
                }
                else
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }

            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                AndHUD.Shared.Dismiss();
            }
        }

        #endregion

        #region MaterialDialog

        public void OnSelection(IDialogInterface dialog, int position, string itemString)
        {
            try
            {
                switch (TypeDialog)
                {
                    case "DiscountOffersAdapter":
                        {
                            AddDiscountId = WoWonderTools.GetAddDiscountList(this)?.FirstOrDefault(a => a.Value == itemString).Key.ToString();

                            TxtDiscountType.Text = itemString;

                            switch (AddDiscountId)
                            {
                                case "free_shipping":
                                    MRecycler.Visibility = ViewStates.Gone;
                                    MAdapter.DiscountList.Clear();
                                    MAdapter.NotifyDataSetChanged();
                                    break;
                                default:
                                    MRecycler.Visibility = ViewStates.Visible;
                                    MAdapter.DiscountList.Clear();

                                    MAdapter.DiscountList.Add(new DiscountOffers
                                    {
                                        DiscountType = AddDiscountId,
                                        DiscountFirst = "",
                                        DiscountSec = "",
                                        DiscountThr = "",
                                    });
                                    MAdapter.NotifyDataSetChanged();
                                    break;
                            }

                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        public void OnClick(View v)
        {
            try
            {
                if (v.Id == TxtTime.Id)
                {
                    var frag = PopupDialogController.TimePickerFragment.NewInstance(delegate (DateTime time)
                    {
                        TxtTime.Text = time.ToShortTimeString();
                    });

                    frag.Show(SupportFragmentManager, PopupDialogController.TimePickerFragment.Tag);
                }
                else if (v.Id == TxtDate.Id)
                {
                    var frag = PopupDialogController.DatePickerFragment.NewInstance(delegate (DateTime time)
                    {
                        TxtDate.Text = time.Date.ToString("yyyy-MM-dd");
                    });

                    frag.Show(SupportFragmentManager, PopupDialogController.DatePickerFragment.Tag);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void Get_DataOffer()
        {
            try
            {
                OfferClass = JsonConvert.DeserializeObject<OfferObject>(Intent?.GetStringExtra("OfferItem") ?? "");
                if (OfferClass != null)
                {
                    AddDiscountId = OfferClass.DiscountType;
                    switch (OfferClass.DiscountType)
                    {
                        case "discount_percent":
                            {
                                TxtDiscountType.Text = GetString(Resource.String.Lbl_DiscountPercent);
                                MRecycler.Visibility = ViewStates.Visible;
                                MAdapter.DiscountList.Clear();

                                MAdapter.DiscountList.Add(new DiscountOffers
                                {
                                    DiscountType = OfferClass.DiscountType,
                                    DiscountFirst = OfferClass.DiscountPercent,
                                    DiscountSec = "",
                                    DiscountThr = "",
                                });
                                MAdapter.NotifyDataSetChanged();
                            }
                            break;
                        case "discount_amount":
                            {
                                TxtDiscountType.Text = GetString(Resource.String.Lbl_DiscountAmount);

                                MRecycler.Visibility = ViewStates.Visible;
                                MAdapter.DiscountList.Clear();

                                MAdapter.DiscountList.Add(new DiscountOffers
                                {
                                    DiscountType = OfferClass.DiscountType,
                                    DiscountFirst = OfferClass.DiscountAmount,
                                    DiscountSec = "",
                                    DiscountThr = "",
                                });
                                MAdapter.NotifyDataSetChanged();
                            }
                            break;
                        case "buy_get_discount":
                            {
                                TxtDiscountType.Text = GetString(Resource.String.Lbl_BuyGetDiscount);

                                MRecycler.Visibility = ViewStates.Visible;
                                MAdapter.DiscountList.Clear();

                                MAdapter.DiscountList.Add(new DiscountOffers
                                {
                                    DiscountType = OfferClass.DiscountType,
                                    DiscountFirst = OfferClass.DiscountPercent,
                                    DiscountSec = OfferClass.Buy,
                                    DiscountThr = OfferClass.GetPrice,
                                });
                                MAdapter.NotifyDataSetChanged();
                            }
                            break;
                        case "spend_get_off":
                            {
                                TxtDiscountType.Text = GetString(Resource.String.Lbl_SpendGetOff);

                                MRecycler.Visibility = ViewStates.Visible;
                                MAdapter.DiscountList.Clear();

                                MAdapter.DiscountList.Add(new DiscountOffers
                                {
                                    DiscountType = OfferClass.DiscountType,
                                    DiscountFirst = "",
                                    DiscountSec = OfferClass.Spend,
                                    DiscountThr = OfferClass.AmountOff,
                                });
                                MAdapter.NotifyDataSetChanged();
                            }
                            break;
                        case "free_shipping": //Not have tag
                            {
                                TxtDiscountType.Text = GetString(Resource.String.Lbl_FreeShipping);

                                MRecycler.Visibility = ViewStates.Gone;
                                MAdapter.DiscountList.Clear();
                                MAdapter.NotifyDataSetChanged();
                            }
                            break;
                    }

                    TxtDiscountItems.Text = Methods.FunString.DecodeString(OfferClass.DiscountedItems);
                    TxtCurrency.Text = OfferClass.Currency;
                    TxtDate.Text = OfferClass.ExpireDate;
                    TxtTime.Text = OfferClass.ExpireTime;
                    TxtDescription.Text = Methods.FunString.DecodeString(OfferClass.Description);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

    }
}