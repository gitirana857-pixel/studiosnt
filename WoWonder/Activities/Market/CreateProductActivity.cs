using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Content.Res;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Content;
using AndroidX.RecyclerView.Widget;
using Com.Google.Android.Gms.Ads.Admanager;
using Google.Android.Flexbox;
using Google.Android.Material.Dialog;
using ImageViews.Rounded;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WoWonder.Activities.AddPost.Adapters;
using WoWonder.Activities.Base;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Classes.Product;
using WoWonderClient.Requests;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.Market
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class CreateProductActivity : BaseActivity, IDialogListCallBack
    {
        #region Variables Basic

        private TextView TvStep, TvStepTitle;
        private EditText EtStep1, EtStep2, EtStep5, EtStep9;
        private FlexboxLayout RgStep4;
        private TextView TvStep3, TvStep6, TvStep8;
        private RelativeLayout RlStep7;
        private RecyclerView MRecycler;
        private AdManagerAdView AdManagerAdView;
        private AttachmentsAdapter MAdapter;
        private ProgressBar ViewStep;
        private AppCompatButton BtnNext, BtnPrev;
        private ImageView IvSelectPhoto;
        private RoundedImageView ProductCover;

        private int NStep = 1;
        private readonly int MaxStep = 9;
        private List<string> ArrayAdapter;
        private string ProductName, ProductPrice, ProductCurrency, ProductCategory, ProductDescription, ProductTotalItemUnits;
        private string ProductType, Category;
        private string TypeDialog, PlaceText;

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
                SetContentView(Resource.Layout.CreateProductLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();
                InitBackPressed("CreateProductActivity");
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
                NStep = 1;
                TvStep = FindViewById<TextView>(Resource.Id.tv_step);
                TvStepTitle = FindViewById<TextView>(Resource.Id.tv_step_title);
                MRecycler = (RecyclerView)FindViewById(Resource.Id.imageRecyler);
                EtStep1 = FindViewById<EditText>(Resource.Id.et_step1);
                EtStep2 = FindViewById<EditText>(Resource.Id.et_step2);
                EtStep5 = FindViewById<EditText>(Resource.Id.et_step5);
                TvStep3 = FindViewById<TextView>(Resource.Id.tv_step3);
                TvStep6 = FindViewById<TextView>(Resource.Id.tv_step6);
                RgStep4 = FindViewById<FlexboxLayout>(Resource.Id.rg_step4);
                RlStep7 = FindViewById<RelativeLayout>(Resource.Id.rl_step7);
                TvStep8 = FindViewById<TextView>(Resource.Id.tv_step8);
                EtStep9 = FindViewById<EditText>(Resource.Id.tv_step9);
                ViewStep = FindViewById<ProgressBar>(Resource.Id.view_step);

                BtnNext = FindViewById<AppCompatButton>(Resource.Id.btn_next);

                ProductCover = FindViewById<RoundedImageView>(Resource.Id.productCover);

                IvSelectPhoto = FindViewById<ImageView>(Resource.Id.btn_selectimage);

                // Create category buttons
                CreateCategoryButtons();

                AdManagerAdView = FindViewById<AdManagerAdView>(Resource.Id.multiple_ad_sizes_view);
                AdsGoogle.InitAdManagerAdView(AdManagerAdView);

                SetStepChild();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void CreateCategoryButtons()
        {
            try
            {
                int count = CategoriesController.ListCategoriesProducts.Count;
                if (count == 0)
                {
                    Methods.DisplayReportResult(this, "Not have List Categories Product");
                    return;
                }

                foreach (Classes.Categories category in CategoriesController.ListCategoriesProducts)
                {
                    AppCompatButton button = new AppCompatButton(this);
                    int px = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 36, Resources.DisplayMetrics);

                    var ll = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, px);
                    ll.SetMargins(20, 18, 18, 20);
                    button.LayoutParameters = ll;

                    button.Text = category.CategoriesName;
                    button.SetBackgroundResource(Resource.Drawable.round_button_normal_outline);
                    button.SetTextColor(Color.ParseColor("#3E424B"));
                    button.TextSize = 15;
                    button.SetAllCaps(false);
                    button.SetPadding(25, 0, 25, 0);
                    button.Click += CategoryOnClick;
                    RgStep4.AddView(button);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void CategoryOnClick(object sender, EventArgs e)
        {
            if (BtnPrev != null)
            {
                BtnPrev.SetTextColor(Color.ParseColor("#3E424B"));
                BtnPrev.SetBackgroundResource(Resource.Drawable.round_button_normal_outline);
            }
            AppCompatButton BtnCurrent = sender as AppCompatButton;
            BtnCurrent.SetTextColor(Color.ParseColor("#ffffff"));
            BtnCurrent.SetBackgroundResource(Resource.Drawable.round_button_pressed);
            Category = BtnCurrent.Text;

            BtnPrev = BtnCurrent;
        }

        private void InitToolbar()
        {
            try
            {
                var toolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolBar != null)
                {
                    toolBar.Title = GetText(Resource.String.Lbl_CreateNewProduct);
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
                MAdapter = new AttachmentsAdapter(this) { AttachmentList = new ObservableCollection<Attachments>() };
                MRecycler.SetLayoutManager(new LinearLayoutManager(this, LinearLayoutManager.Horizontal, false));
                MRecycler.SetAdapter(MAdapter);

                MRecycler.Visibility = ViewStates.Visible;

                // Add first image Default 
                /*var attach = new Attachments
                {
                    Id = MAdapter.AttachmentList.Count + 1,
                    TypeAttachment = "Default",
                    FileSimple = "addImage",
                    FileUrl = "addImage"
                };

                MAdapter.Add(attach);
                MAdapter.NotifyDataSetChanged();*/
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
                if (addEvent)
                {
                    // true +=  // false -=
                    MAdapter.DeleteItemClick += MAdapterOnDeleteItemClick;
                    BtnNext.Click += BtnNext_Click;
                    TvStep3.Touch += TxtCurrencyOnTouch;
                    TvStep6.Touch += TxtConditionOnTouch;
                    TvStep8.Click += TxtLocationOnClick;
                    RlStep7.Click += SelectPhotoOnClick;
                }
                else
                {
                    MAdapter.DeleteItemClick -= MAdapterOnDeleteItemClick;
                    BtnNext.Click -= BtnNext_Click;
                    TvStep3.Touch -= TxtCurrencyOnTouch;
                    TvStep6.Touch -= TxtConditionOnTouch;
                    TvStep8.Click -= TxtLocationOnClick;
                    RlStep7.Click -= SelectPhotoOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void BackPressed()
        {
            if (NStep > 1)
            {
                NStep -= 1;
                SetStepChild();
                return;
            }
            Finish();
        }

        private void DestroyBasic()
        {
            try
            {
                AdsGoogle.LifecycleAdManagerAdView(AdManagerAdView, "Destroy");

                EtStep1 = null!;
                EtStep2 = null!;
                EtStep5 = null!;
                TvStep3 = null!;
                TvStep6 = null!;
                RgStep4 = null!;
                RlStep7 = null!;
                TvStep = null!;
                TvStepTitle = null!;
                MAdapter = null!;
                MRecycler = null!;
                AdManagerAdView = null!;
                ArrayAdapter = null!;
                IvSelectPhoto = null!;
                TvStep8 = null!;
                EtStep9 = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void HideKeyboard()
        {
            try
            {
                var inputManager = (InputMethodManager)GetSystemService(InputMethodService);
                inputManager?.HideSoftInputFromWindow(CurrentFocus?.WindowToken, HideSoftInputFlags.None);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SetStepChild()
        {
            try
            {
                TvStep.Text = GetText(Resource.String.Lbl_Step) + " " + NStep + "/" + MaxStep;
                var progress = 100 / MaxStep * NStep;
                ViewStep.Progress = progress;

                if (NStep == 1)
                {
                    EtStep1.Visibility = ViewStates.Visible;
                    EtStep2.Visibility = ViewStates.Gone;
                    TvStep3.Visibility = ViewStates.Gone;
                    RgStep4.Visibility = ViewStates.Gone;
                    EtStep5.Visibility = ViewStates.Gone;
                    TvStep6.Visibility = ViewStates.Gone;
                    RlStep7.Visibility = ViewStates.Gone;
                    MRecycler.Visibility = ViewStates.Gone;
                    TvStep8.Visibility = ViewStates.Gone;
                    EtStep9.Visibility = ViewStates.Gone;
                    TvStepTitle.Text = GetString(Resource.String.Lbl_Product_name);

                    BtnNext.Text = GetString(Resource.String.Lbl_Next);
                }
                else if (NStep == 2)
                {
                    EtStep1.Visibility = ViewStates.Gone;
                    EtStep2.Visibility = ViewStates.Visible;
                    TvStep3.Visibility = ViewStates.Gone;
                    RgStep4.Visibility = ViewStates.Gone;
                    EtStep5.Visibility = ViewStates.Gone;
                    TvStep6.Visibility = ViewStates.Gone;
                    RlStep7.Visibility = ViewStates.Gone;
                    MRecycler.Visibility = ViewStates.Gone;
                    TvStep8.Visibility = ViewStates.Gone;
                    EtStep9.Visibility = ViewStates.Gone;
                    TvStepTitle.Text = GetString(Resource.String.Lbl_Product_price);

                    BtnNext.Text = GetString(Resource.String.Lbl_Next);
                }
                else if (NStep == 3)
                {
                    EtStep1.Visibility = ViewStates.Gone;
                    EtStep2.Visibility = ViewStates.Gone;
                    TvStep3.Visibility = ViewStates.Visible;
                    RgStep4.Visibility = ViewStates.Gone;
                    EtStep5.Visibility = ViewStates.Gone;
                    TvStep6.Visibility = ViewStates.Gone;
                    RlStep7.Visibility = ViewStates.Gone;
                    MRecycler.Visibility = ViewStates.Gone;
                    TvStep8.Visibility = ViewStates.Gone;
                    EtStep9.Visibility = ViewStates.Gone;
                    HideKeyboard();
                    TvStepTitle.Text = GetString(Resource.String.Lbl_Currency);

                    BtnNext.Text = GetString(Resource.String.Lbl_Next);
                }
                else if (NStep == 4)
                {
                    ArrayAdapter = CategoriesController.ListCategoriesProducts.Select(item => item.CategoriesName)
                        .ToList();
                    EtStep1.Visibility = ViewStates.Gone;
                    EtStep2.Visibility = ViewStates.Gone;
                    TvStep3.Visibility = ViewStates.Gone;
                    RgStep4.Visibility = ViewStates.Visible;
                    EtStep5.Visibility = ViewStates.Gone;
                    TvStep6.Visibility = ViewStates.Gone;
                    RlStep7.Visibility = ViewStates.Gone;
                    MRecycler.Visibility = ViewStates.Gone;
                    TvStep8.Visibility = ViewStates.Gone;
                    EtStep9.Visibility = ViewStates.Gone;
                    TvStepTitle.Text = GetString(Resource.String.Lbl_SelectCategory);

                    BtnNext.Text = GetString(Resource.String.Lbl_Next);
                }
                else if (NStep == 5)
                {
                    EtStep1.Visibility = ViewStates.Gone;
                    EtStep2.Visibility = ViewStates.Gone;
                    TvStep3.Visibility = ViewStates.Gone;
                    RgStep4.Visibility = ViewStates.Gone;
                    EtStep5.Visibility = ViewStates.Visible;
                    TvStep6.Visibility = ViewStates.Gone;
                    RlStep7.Visibility = ViewStates.Gone;
                    MRecycler.Visibility = ViewStates.Gone;
                    TvStep8.Visibility = ViewStates.Gone;
                    EtStep9.Visibility = ViewStates.Gone;
                    TvStepTitle.Text = GetString(Resource.String.Lbl_Product_description);

                    BtnNext.Text = GetString(Resource.String.Lbl_Next);
                }
                else if (NStep == 6)
                {
                    EtStep1.Visibility = ViewStates.Gone;
                    EtStep2.Visibility = ViewStates.Gone;
                    TvStep3.Visibility = ViewStates.Gone;
                    RgStep4.Visibility = ViewStates.Gone;
                    EtStep5.Visibility = ViewStates.Gone;
                    TvStep6.Visibility = ViewStates.Visible;
                    RlStep7.Visibility = ViewStates.Gone;
                    MRecycler.Visibility = ViewStates.Gone;
                    TvStep8.Visibility = ViewStates.Gone;
                    EtStep9.Visibility = ViewStates.Gone;
                    TvStepTitle.Text = GetString(Resource.String.Lbl_Product_Condition);

                    BtnNext.Text = GetString(Resource.String.Lbl_Next);
                }
                else if (NStep == 7)
                {
                    EtStep1.Visibility = ViewStates.Gone;
                    EtStep2.Visibility = ViewStates.Gone;
                    TvStep3.Visibility = ViewStates.Gone;
                    RgStep4.Visibility = ViewStates.Gone;
                    EtStep5.Visibility = ViewStates.Gone;
                    TvStep6.Visibility = ViewStates.Gone;
                    RlStep7.Visibility = ViewStates.Visible;
                    MRecycler.Visibility = ViewStates.Visible;
                    TvStep8.Visibility = ViewStates.Gone;
                    EtStep9.Visibility = ViewStates.Gone;
                    TvStepTitle.Text = GetString(Resource.String.Lbl_Product_Image);

                    BtnNext.Text = GetString(Resource.String.Lbl_Next);
                }
                else if (NStep == 8)
                {
                    EtStep1.Visibility = ViewStates.Gone;
                    EtStep2.Visibility = ViewStates.Gone;
                    TvStep3.Visibility = ViewStates.Gone;
                    RgStep4.Visibility = ViewStates.Gone;
                    EtStep5.Visibility = ViewStates.Gone;
                    TvStep6.Visibility = ViewStates.Gone;
                    RlStep7.Visibility = ViewStates.Gone;
                    MRecycler.Visibility = ViewStates.Gone;
                    TvStep8.Visibility = ViewStates.Visible;
                    EtStep9.Visibility = ViewStates.Gone;
                    TvStepTitle.Text = GetString(Resource.String.Lbl_Product_Location);

                    BtnNext.Text = GetString(Resource.String.Lbl_Next);
                }
                else if (NStep == 9)
                {
                    EtStep1.Visibility = ViewStates.Gone;
                    EtStep2.Visibility = ViewStates.Gone;
                    TvStep3.Visibility = ViewStates.Gone;
                    RgStep4.Visibility = ViewStates.Gone;
                    EtStep5.Visibility = ViewStates.Gone;
                    TvStep6.Visibility = ViewStates.Gone;
                    RlStep7.Visibility = ViewStates.Gone;
                    MRecycler.Visibility = ViewStates.Gone;
                    TvStep8.Visibility = ViewStates.Gone;
                    EtStep9.Visibility = ViewStates.Visible;
                    TvStepTitle.Text = GetString(Resource.String.Lbl_Product_TotalItemUnits);

                    BtnNext.Text = GetString(Resource.String.Lbl_Create);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        #endregion

        #region Events

        private void MAdapterOnDeleteItemClick(object sender, AttachmentsAdapterClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                if (position >= 0)
                {
                    var item = MAdapter.GetItem(position);
                    if (item != null)
                    {
                        MAdapter.Remove(item);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtConditionOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Up) return;

                var arrayAdapter = new List<string>
                {
                    GetText(Resource.String.Radio_New),
                    GetText(Resource.String.Radio_Used)
                };

                TypeDialog = "Condition";

                var dialogList = new MaterialAlertDialogBuilder(this);

                dialogList.SetTitle(GetText(Resource.String.Lbl_SelectCurrency));
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                dialogList.Show();
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

                var arrayAdapter = WoWonderTools.GetCurrencySymbolList();
                if (arrayAdapter?.Count > 0)
                {
                    TypeDialog = "Currency";

                    var dialogList = new MaterialAlertDialogBuilder(this);
                    dialogList.SetTitle(GetText(Resource.String.Lbl_SelectCurrency));
                    dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                    dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                    dialogList.Show();
                }
                else
                {
                    Methods.DisplayReportResult(this, "Not have List Currency Products");
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtCategoryOnClick(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Up) return;

                if (CategoriesController.ListCategoriesProducts.Count > 0)
                {
                    TypeDialog = "Categories";

                    var dialogList = new MaterialAlertDialogBuilder(this);

                    var arrayAdapter = CategoriesController.ListCategoriesProducts.Select(item => item.CategoriesName).ToList();

                    dialogList.SetTitle(GetText(Resource.String.Lbl_SelectCategories));
                    dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                    dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                    dialogList.Show();
                }
                else
                {
                    Methods.DisplayReportResult(this, "Not have List Categories Products");
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtLocationOnFocusChange()
        {
            try
            {
                if ((int)Build.VERSION.SdkInt <
                    // Check if we're running on Android 5.0 or higher
                    23)
                {
                    //Open intent Location when the request code of result is 502
                    new IntentController(this).OpenIntentLocation();
                }
                else
                {
                    if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) == Permission.Granted &&
                        ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) == Permission.Granted)
                    {
                        //Open intent Location when the request code of result is 502
                        new IntentController(this).OpenIntentLocation();
                    }
                    else
                    {
                        new PermissionsController(this).RequestPermission(105);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void OnSave()
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
                else
                {
                    var list = MAdapter.AttachmentList.Where(a => a.TypeAttachment != "Default").ToList();
                    if (list.Count == 0)
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_select_Image), ToastLength.Short);
                    }
                    else
                    {
                        //Show a progress
                        AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading) + "...");

                        var (apiStatus, respond) = await RequestsAsync.Market.CreateProductAsync(ProductName, ProductDescription, PlaceText, ProductPrice, ProductCurrency, ProductCategory, ProductType, ProductTotalItemUnits, list);
                        if (apiStatus == 200)
                        {
                            if (respond is CreateProductObject result)
                            {
                                AndHUD.Shared.Dismiss();
                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CreatedSuccessfully), ToastLength.Short);

                                var listImage = list.Select(productPathImage => new Images
                                {
                                    Id = "",
                                    ProductId = result.ProductId.ToString(),
                                    Image = productPathImage.FileSimple,
                                    ImageOrg = productPathImage.FileSimple
                                }).ToList();

                                //Add new item to my  Product list
                                var user = ListUtils.MyProfileList?.FirstOrDefault();
                                ProductDataObject data = new ProductDataObject
                                {
                                    Name = ProductName,
                                    UserId = UserDetails.UserId,
                                    Id = result.ProductId.ToString(),
                                    Location = PlaceText,
                                    Description = ProductDescription,
                                    Category = ProductCategory,
                                    Images = new List<Images>(listImage),
                                    Price = ProductPrice,
                                    Type = ProductType,
                                    Seller = user,
                                    Currency = ProductCurrency,
                                    PostId = result.ProductPostId.ToString(),
                                };

                                if (TabbedMarketActivity.GetInstance()?.MyProductsTab.MAdapter.MarketList != null)
                                {
                                    TabbedMarketActivity.GetInstance()?.MyProductsTab.MAdapter?.MarketList?.Insert(0, new Classes.ProductClass
                                    {
                                        Id = Convert.ToInt64(data.Id),
                                        Type = Classes.ItemType.MyProduct,
                                        Product = data
                                    });
                                    TabbedMarketActivity.GetInstance()?.MyProductsTab.MAdapter?.NotifyDataSetChanged();
                                }

                                Intent returnIntent = new Intent();
                                returnIntent?.PutExtra("product", JsonConvert.SerializeObject(data));
                                SetResult(Result.Ok, returnIntent);

                                Finish();
                            }
                        }
                        else
                            Methods.DisplayAndHudErrorResult(this, respond);
                    }
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

                if (requestCode == 502 && resultCode == Result.Ok)
                {
                    GetPlaceFromPicker(data);
                }
                else if (requestCode == 500 && resultCode == Result.Ok)
                {
                    if (data.ClipData != null)
                    {
                        var mClipData = data.ClipData;
                        for (var i = 0; i < mClipData.ItemCount; i++)
                        {
                            var item = mClipData.GetItemAt(i);
                            Uri uri = item.Uri;
                            var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, uri);
                            PickiTonCompleteListener(filepath);
                        }
                    }
                    else
                    {
                        Uri uri = data.Data;
                        var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, uri);
                        PickiTonCompleteListener(filepath);
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
                if (requestCode == 108 && grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    new IntentController(this).OpenIntentImageGallery(GetText(Resource.String.image), true);
                else if (requestCode == 108)
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                else if (requestCode == 105 && grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    //Open intent Location when the request code of result is 502
                    new IntentController(this).OpenIntentLocation();
                else if (requestCode == 105)
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
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
                if (TypeDialog == "Condition")
                {
                    TvStep6.Text = itemString;
                    if (TvStep6.Text.Equals(GetText(Resource.String.Radio_New)))
                        ProductType = "0";
                    else
                        ProductType = "1";
                }
                else if (TypeDialog == "Currency")
                {
                    TvStep3.Text = itemString;
                    ProductCurrency = WoWonderTools.GetIdCurrency(itemString);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private void GetPlaceFromPicker(Intent data)
        {
            try
            {
                var placeAddress = data.GetStringExtra("Address") ?? "";
                if (!string.IsNullOrEmpty(placeAddress))
                //var placeLatLng = data.GetStringExtra("latLng") ?? "";
                {
                    PlaceText = string.IsNullOrEmpty(PlaceText) switch
                    {
                        false => string.Empty,
                        _ => PlaceText
                    };

                    PlaceText = placeAddress;
                    TvStep8.Text = PlaceText;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void TxtLocationOnClick(object sender, EventArgs e)
        {
            TxtLocationOnFocusChange();
        }

        private void BtnNext_Click(object sender, EventArgs e)
        {
            try
            {
                if (NStep == 1)
                {
                    ProductName = EtStep1.Text;
                    if (ProductName.Length > 0)
                    {
                        NStep += 1;
                        SetStepChild();
                        EtStep1.Text = "";
                    }
                }
                else if (NStep == 2)
                {
                    ProductPrice = EtStep2.Text;
                    if (ProductPrice.Length > 0)
                    {
                        NStep += 1;
                        SetStepChild();
                        EtStep2.Text = "";
                    }
                }
                else if (NStep == 3)
                {
                    if (TvStep3.Text.Length > 0 && !TvStep3.Text.Equals(GetString(Resource.String.Lbl_Select_Product_Currency)))
                    {
                        NStep += 1;
                        SetStepChild();
                        TvStep3.Text = GetString(Resource.String.Lbl_Select_Product_Currency);
                    }
                }
                else if (NStep == 4)
                {
                    if (Category.Length > 0)
                    {
                        var cat = CategoriesController.ListCategoriesProducts.FirstOrDefault(categories => categories.CategoriesName == Category);
                        if (cat != null)
                        {
                            ProductCategory = cat.CategoriesId;

                            NStep += 1;
                            SetStepChild();
                        }
                    }
                }
                else if (NStep == 5)
                {
                    ProductDescription = EtStep5.Text;
                    if (ProductDescription.Length > 0)
                    {
                        NStep += 1;
                        SetStepChild();
                        EtStep5.Text = "";
                    }
                }
                else if (NStep == 6)
                {
                    if (TvStep6.Text.Length > 0 && !TvStep6.Text.Equals(GetString(Resource.String.Lbl_Select_New_Used)))
                    {
                        NStep += 1;
                        SetStepChild();
                        TvStep6.Text = GetString(Resource.String.Lbl_Select_New_Used);
                    }
                }
                else if (NStep == 7)
                {
                    var list = MAdapter.AttachmentList.Where(a => a.TypeAttachment != "Default").ToList();
                    if (list.Count == 0)
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_select_Image),
                            ToastLength.Short);
                    }
                    else
                    {
                        NStep += 1;
                        SetStepChild();
                    }
                }
                else if (NStep == 8)
                {
                    if (TvStep8.Text.Length > 0 && !TvStep8.Text.Equals(GetString(Resource.String.Lbl_Select_Location)))
                    {
                        NStep += 1;
                        SetStepChild();
                    }
                }
                else if (NStep == 9)
                {
                    ProductTotalItemUnits = EtStep9.Text;
                    if (ProductTotalItemUnits.Length > 0)
                    {
                        OnSave();
                    }
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        private void SelectPhotoOnClick(object sender, EventArgs e)
        {
            try
            {
                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    Methods.Path.Chack_MyFolder();

                    //requestCode >> 500 => Image Gallery
                    new IntentController(this).OpenIntentImageGallery(GetText(Resource.String.image), true);
                }
                else
                {
                    if (PermissionsController.CheckPermissionStorage(this) && ContextCompat.CheckSelfPermission(this, Manifest.Permission.Camera) == Permission.Granted)
                    {
                        Methods.Path.Chack_MyFolder();

                        //requestCode >> 500 => Image Gallery
                        new IntentController(this).OpenIntentImageGallery(GetText(Resource.String.image), true);
                    }
                    else
                    {
                        new PermissionsController(this).RequestPermission(108);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void PickiTonCompleteListener(string path)
        {
            //Dismiss dialog and return the path
            try
            {
                //  Check if it was a Drive/local/unknown provider file and display a Toast
                //if (wasDriveFile)
                //{
                //    // "Drive file was selected"
                //}
                //else if (wasUnknownProvider)
                //{
                //    // "File was selected from unknown provider"
                //}
                //else
                //{
                //    // "Local file was selected"
                //}

                //  Chick if it was successful
                var (check, info) = await WoWonderTools.CheckMimeTypesWithServer(path);
                if (check is false)
                {
                    if (info == "AdultImages")
                    {
                        //this file not allowed 
                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_Error_AdultImages), ToastLength.Short);

                        var dialog = new MaterialAlertDialogBuilder(this);
                        dialog.SetMessage(GetText(Resource.String.Lbl_Error_AdultImages));
                        dialog.SetPositiveButton(GetText(Resource.String.Lbl_IgnoreAndSend), (materialDialog, action) =>
                        {
                            try
                            {
                                var attach = new Attachments
                                {
                                    Id = MAdapter.AttachmentList.Count + 1,
                                    TypeAttachment = "images[]",
                                    FileSimple = path,
                                    FileUrl = path
                                };

                                MAdapter.Add(attach);
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                        dialog.SetNegativeButton(GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());

                        dialog.Show();
                    }
                    else
                    {
                        //this file not supported on the server , please select another file 
                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_ErrorFileNotSupported), ToastLength.Short);
                    }
                }
                else
                {
                    var attach = new Attachments
                    {
                        Id = MAdapter.AttachmentList.Count + 1,
                        TypeAttachment = "images[]",
                        FileSimple = path,
                        FileUrl = path
                    };

                    MAdapter.Add(attach);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


    }
}