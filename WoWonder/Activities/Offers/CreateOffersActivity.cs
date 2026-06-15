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
using Bumptech.Glide;
using Bumptech.Glide.Request;
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
using WoWonderClient.Requests;
using Console = System.Console;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Offers
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class CreateOffersActivity : BaseActivity, IDialogListCallBack, View.IOnClickListener
    {
        #region Variables Basic

        private TextView TxtSave;
        private TextView IconDiscountType, IconDiscountItems, IconCurrency, IconDescription, IconDate, IconTime, TxtAddImg;
        private EditText TxtDiscountType, TxtDiscountItems, TxtCurrency, TxtDate, TxtTime, TxtDescription;
        private ImageView Image;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private DiscountTypeAdapter MAdapter;
        private string TypeDialog, CurrencyId, ImagePath, PageId, AddDiscountId;

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

                PageId = Intent?.GetStringExtra("PageId") ?? "";

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

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

                Image = FindViewById<ImageView>(Resource.Id.image);
                TxtAddImg = FindViewById<TextView>(Resource.Id.addImg);

                IconDiscountType = FindViewById<TextView>(Resource.Id.IconDiscountType);
                TxtDiscountType = FindViewById<EditText>(Resource.Id.DiscountTypeEditText);

                MRecycler = FindViewById<RecyclerView>(Resource.Id.Recyler);
                MRecycler.Visibility = ViewStates.Gone;

                IconDiscountItems = FindViewById<TextView>(Resource.Id.IconDiscountItems);
                TxtDiscountItems = FindViewById<EditText>(Resource.Id.DiscountItemsEditText);

                IconCurrency = FindViewById<TextView>(Resource.Id.IconCurrency);
                TxtCurrency = FindViewById<EditText>(Resource.Id.CurrencyEditText);

                IconDescription = FindViewById<TextView>(Resource.Id.IconDescription);
                TxtDescription = FindViewById<EditText>(Resource.Id.DescriptionEditText);

                IconDate = FindViewById<TextView>(Resource.Id.IconDate);
                TxtDate = FindViewById<EditText>(Resource.Id.DateEditText);

                IconTime = FindViewById<TextView>(Resource.Id.IconTime);
                TxtTime = FindViewById<EditText>(Resource.Id.TimeEditText);

                AdManagerAdView = FindViewById<AdManagerAdView>(Resource.Id.multiple_ad_sizes_view);
                AdsGoogle.InitAdManagerAdView(AdManagerAdView);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconDiscountType, FontAwesomeIcon.User);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconDiscountItems, FontAwesomeIcon.MapMarkedAlt);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, IconCurrency, FontAwesomeIcon.DollarSign);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconDescription, FontAwesomeIcon.Paragraph);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconDate, FontAwesomeIcon.CalendarAlt);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconTime, FontAwesomeIcon.Clock);

                Methods.SetColorEditText(TxtDiscountType, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtDiscountItems, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtCurrency, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtDescription, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtDate, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtTime, WoWonderTools.IsTabDark() ? Color.White : Color.Black);

                Methods.SetFocusable(TxtDiscountType);
                Methods.SetFocusable(TxtCurrency);
                Methods.SetFocusable(TxtDate);
                Methods.SetFocusable(TxtTime);

                TxtDate.SetOnClickListener(this);
                TxtTime.SetOnClickListener(this);
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
                    toolBar.Title = GetText(Resource.String.Lbl_CreateOffers);
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
                        TxtCurrency.Touch += TxtCurrencyOnTouch;
                        TxtAddImg.Click += TxtAddImgOnClick;
                        TxtDiscountType.Touch += TxtDiscountTypeOnTouch;
                        break;
                    default:
                        TxtSave.Click -= TxtSaveOnClick;
                        TxtCurrency.Touch -= TxtCurrencyOnTouch;
                        TxtAddImg.Click -= TxtAddImgOnClick;
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
                Image = null!;
                TxtAddImg = null!;
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
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        #endregion

        #region Events

        private void TxtAddImgOnClick(object sender, EventArgs e)
        {
            try
            {
                PixImagePickerUtils.OpenDialogGallery(this);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

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
                    if (string.IsNullOrEmpty(TxtDiscountType.Text) || string.IsNullOrEmpty(TxtDiscountItems.Text) || string.IsNullOrEmpty(TxtCurrency.Text)
                        || string.IsNullOrEmpty(TxtDescription.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_enter_your_data), ToastLength.Short);
                        return;
                    }

                    if (string.IsNullOrEmpty(ImagePath))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_select_Image), ToastLength.Short);
                        return;
                    }

                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                    var dictionary = new Dictionary<string, string>
                    {
                        {"discount_type", AddDiscountId},
                        {"currency", CurrencyId},
                        {"page_id", PageId},
                        {"expire_date", TxtDate.Text},
                        {"expire_time", TxtTime.Text.Replace("PM" , "").Replace("AM" , "").Replace(" " , "")},
                        {"description", TxtDescription.Text},
                        {"discounted_items", TxtDiscountItems.Text},
                    };

                    if (MAdapter.DiscountList.Count > 0)
                    {
                        foreach (var discount in MAdapter.DiscountList)
                        {
                            switch (discount.DiscountType)
                            {
                                case "discount_percent":
                                    dictionary.Add("discount_percent", discount.DiscountFirst);
                                    break;
                                case "discount_amount":
                                    dictionary.Add("discount_amount", discount.DiscountFirst);
                                    break;
                                case "buy_get_discount":
                                    dictionary.Add("discount_percent", discount.DiscountFirst);
                                    dictionary.Add("buy", discount.DiscountSec);
                                    dictionary.Add("get", discount.DiscountThr);
                                    break;
                                case "spend_get_off":
                                    dictionary.Add("spend", discount.DiscountSec);
                                    dictionary.Add("amount_off", discount.DiscountThr);
                                    break;
                                case "free_shipping": //Not have tag
                                    break;
                            }
                        }
                    }

                    var (apiStatus, respond) = await RequestsAsync.Offers.CreateOfferAsync(dictionary, ImagePath);
                    switch (apiStatus)
                    {
                        case 200:
                            {
                                switch (respond)
                                {
                                    case CreateOfferObject result:
                                        Console.WriteLine(result.Data);
                                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_OfferSuccessfullyAdded), ToastLength.Short);

                                        AndHUD.Shared.Dismiss();
                                        Finish();
                                        break;
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

        #region Permissions && Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);

                if (requestCode == PixImagePickerActivity.RequestCode && resultCode == Result.Ok)
                {
                    var listPath = JsonConvert.DeserializeObject<ResultIntentPixImage>(data.GetStringExtra("ResultPixImage") ?? "");
                    if (listPath?.List?.Count > 0)
                    {
                        var filepath = listPath.List.FirstOrDefault();
                        if (!string.IsNullOrEmpty(filepath))
                        {
                            //Do something with your Uri
                            ImagePath = filepath;
                            Glide.With(this).Load(filepath).Apply(new RequestOptions()).Into(Image);
                        }
                        else
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long)?.Show();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                switch (requestCode)
                {
                    case 108 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        PixImagePickerUtils.OpenDialogGallery(this);
                        break;
                    case 108:
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
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
                    case "Currency":
                        TxtCurrency.Text = itemString;

                        //var (currency, currencyIcon) = WoWonderTools.GetCurrency(itemId.ToString());
                        CurrencyId = position.ToString();
                        //Console.WriteLine(currencyIcon);
                        break;
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

    }
}