using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Content.Res;
using AndroidX.AppCompat.Widget;
using AT.Markushi.UI;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Newtonsoft.Json;
using System;
using System.Linq;
using WoWonder.Activities.Base;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Utils;
using WoWonderClient.Requests;
using Console = System.Console;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Payment
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class PaymentLocalActivity : BaseActivity
    {
        #region Variables Basic

        private TextView BankName, CardNumber, CardCode, CardCountry, CardName;
        private ImageView Image;
        private CircleButton ImageClose;
        private AppCompatButton BtnAddImage, BtnApply;
        private string Price, PathImage = "";


        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                Methods.App.FullScreenApp(this);
                SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                // Create your application here
                SetContentView(Resource.Layout.PaymentLocalLayout);

                Price = Intent?.GetStringExtra("Price") ?? "";

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

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
                BankName = (TextView)FindViewById(Resource.Id.bank_name);
                CardNumber = (TextView)FindViewById(Resource.Id.card_number);
                CardCode = (TextView)FindViewById(Resource.Id.card_code);
                CardCountry = (TextView)FindViewById(Resource.Id.card_country);
                CardName = (TextView)FindViewById(Resource.Id.card_name);
                Image = (ImageView)FindViewById(Resource.Id.Image);

                ImageClose = (CircleButton)FindViewById(Resource.Id.ImageCircle);
                BtnAddImage = (AppCompatButton)FindViewById(Resource.Id.btn_AddPhoto);
                BtnApply = (AppCompatButton)FindViewById(Resource.Id.ApplyButton);

                string bankDescription = ListUtils.SettingsSiteList?.BankDescription;
                bankDescription = bankDescription?.Replace("&lt;", "<").Replace("&gt;", ">");
                var splitText = bankDescription?.Split(new[] { "<p>", "</p>" }, StringSplitOptions.None);
                Console.WriteLine(splitText);

                switch (splitText?.Length)
                {
                    case > 0:
                        {
                            CardNumber.Text = splitText[1];
                            CardName.Text = splitText[3];
                            CardCode.Text = splitText[5];
                            CardCountry.Text = splitText[7];

                            var bankName = splitText[0].Split(new[] { ">", "</h4>" }, StringSplitOptions.None);
                            switch (bankName.Length)
                            {
                                case > 0:
                                    BankName.Text = bankName[12];
                                    BankName.Visibility = ViewStates.Visible;
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

        private void InitToolbar()
        {
            try
            {
                var toolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolBar != null)
                {
                    toolBar.Title = GetString(Resource.String.Lbl_BankTransfer);
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

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                switch (addEvent)
                {
                    // true +=  // false -=
                    case true:
                        ImageClose.Click += ImageCloseOnClick;
                        BtnAddImage.Click += BtnAddImageOnClick;
                        BtnApply.Click += BtnApplyOnClick;
                        break;
                    default:
                        ImageClose.Click -= ImageCloseOnClick;
                        BtnAddImage.Click -= BtnAddImageOnClick;
                        BtnApply.Click -= BtnApplyOnClick;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void BtnAddImageOnClick(object sender, EventArgs e)
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

        private async void BtnApplyOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    if (string.IsNullOrEmpty(PathImage) || string.IsNullOrWhiteSpace(PathImage))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_ErrorPleaseSelectImage), ToastLength.Long);
                        return;
                    }

                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                    string description = "Add to balance = " + Price;

                    var (apiStatus, respond) = await RequestsAsync.Payments.UploadBankRecipeAsync("wallet", Price, description, PathImage);
                    if (apiStatus == 200)
                    {
                        AndHUD.Shared.Dismiss();
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_YourWasReceiptSuccessfullyUploaded), ToastLength.Short);
                        Finish();
                    }
                    else
                    {
                        Methods.DisplayAndHudErrorResult(this, respond);
                    }
                }
                else
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                }
            }
            catch (Exception exception)
            {
                AndHUD.Shared.Dismiss();
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ImageCloseOnClick(object sender, EventArgs e)
        {
            try
            {
                PathImage = "";
                GlideImageLoader.LoadImage(this, "Grey_Offline", Image, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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
                            PathImage = filepath;

                            Glide.With(this).Load(filepath).Apply(new RequestOptions().CenterCrop()).Into(Image);
                        }
                        else
                        {
                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Failed_to_load), ToastLength.Short);
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
                    case 106 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        PixImagePickerUtils.OpenDialogGallery(this);
                        break;
                    case 106:
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

    }
}