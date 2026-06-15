using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Content.Res;
using AndroidX.AppCompat.Widget;
using Com.Google.Android.Gms.Ads.Admanager;
using Com.Stripe.Android.View;
using System;
using System.Collections.Generic;
using System.Linq;
using WoWonder.Activities.Base;
using WoWonder.Activities.Wallet;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Utils;
using WoWonder.SQLite;
using WoWonderClient.Classes.Payments;
using WoWonderClient.Requests;
using Exception = System.Exception;
using Math = System.Math;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Payment
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class AuthorizeNetPaymentActivity : BaseActivity
    {
        #region Variables Basic

        private TextView CardNumber, CardExpire, CardCvv, CardName;
        private EditText EtName;
        private AppCompatButton BtnApply;
        private CardMultilineWidget MultilineWidget;
        private AdManagerAdView AdManagerAdView;

        private string Price;

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
                SetContentView(Resource.Layout.PaymentCardDetailsLayout);

                Price = Intent?.GetStringExtra("Price") ?? "";

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                InitWalletAuthorizeNet();
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
                AdsGoogle.LifecycleAdManagerAdView(AdManagerAdView, "Destroy");
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
                CardNumber = (TextView)FindViewById(Resource.Id.card_number);
                CardExpire = (TextView)FindViewById(Resource.Id.card_expire);
                CardCvv = (TextView)FindViewById(Resource.Id.card_cvv);
                CardName = (TextView)FindViewById(Resource.Id.card_name);

                MultilineWidget = (CardMultilineWidget)FindViewById(Resource.Id.card_multiline_widget);

                EtName = (EditText)FindViewById(Resource.Id.et_name);
                BtnApply = (AppCompatButton)FindViewById(Resource.Id.ApplyButton);

                AdManagerAdView = FindViewById<AdManagerAdView>(Resource.Id.multiple_ad_sizes_view);
                AdsGoogle.InitAdManagerAdView(AdManagerAdView);

                Methods.SetColorEditText(EtName, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
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
                    toolBar.Title = GetString(Resource.String.Lbl_AuthorizeNet);
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
                // true +=  // false -=
                if (addEvent)
                {
                    MultilineWidget.CvcEditText.AfterTextChanged += CvcEditTextOnAfterTextChanged;
                    EtName.TextChanged += EtNameOnTextChanged;
                    BtnApply.Click += BtnApplyOnClick;
                }
                else
                {
                    MultilineWidget.CvcEditText.AfterTextChanged -= CvcEditTextOnAfterTextChanged;
                    EtName.TextChanged -= EtNameOnTextChanged;
                    BtnApply.Click -= BtnApplyOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void CvcEditTextOnAfterTextChanged(object sender, StripeEditText.AfterTextChangedEventArgs e)
        {
            try
            {
                if (MultilineWidget.CardParams != null && MultilineWidget.ValidateAllFields())
                {
                    var cardNumber = MultilineWidget.CardNumberEditText.Text;
                    var cardExpire = MultilineWidget.ExpiryDateEditText.Text;
                    var cardCvv = MultilineWidget.CvcEditText.Text;

                    CardNumber.Text = cardNumber.Trim().Length == 0 ? "**** **** **** ****" : InsertPeriodically(cardNumber.Trim(), " ", 4);
                    CardExpire.Text = cardExpire.Trim().Length == 0 ? "MM/YY" : cardExpire;
                    CardCvv.Text = cardCvv.Trim().Length == 0 ? "***" : cardCvv.Trim();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        private void EtNameOnTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                CardName.Text = e?.Text?.ToString()?.Trim().Length == 0 ? GetString(Resource.String.Lbl_YourName) : e?.Text?.ToString()?.Trim();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //AuthorizeNet
        private async void BtnApplyOnClick(object sender, EventArgs e)
        {
            try
            {
                if (MultilineWidget.ValidateAllFields() && !string.IsNullOrEmpty(EtName.Text))
                {
                    if (!InitWalletAuthorizeNet())
                        return;

                    var cardNumber = MultilineWidget.CardNumberEditText.Text.Replace(" ", "");
                    var cardExpire = MultilineWidget.ExpiryDateEditText.Text;

                    var ExpMonth = cardExpire.Split("/").First();
                    var ExpYear = cardExpire.Split("/").Last();

                    var cardCvv = MultilineWidget.CvcEditText.Text;

                    //EncryptTransactionObject transactionObject = PrepareTransactionObject(card);

                    //Make a call to get Token API
                    //apiClient.GetTokenWithRequest(transactionObject, this); 

                    if (Methods.CheckConnectivity())
                    {
                        //Show a progress
                        AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                        var (apiStatus, respond) = await RequestsAsync.Payments.TopWalletAuthorizeNetAsync(cardNumber, ExpMonth, ExpYear, cardCvv, Price);
                        switch (apiStatus)
                        {
                            case 200:
                                if (respond is PaymentSuccessfullyObject creditObject)
                                {
                                    var tabbedWallet = TabbedWalletActivity.GetInstance();
                                    if (tabbedWallet != null)
                                    {
                                        tabbedWallet.AddFundsFragment.TxtAmount.Text = string.Empty;
                                        tabbedWallet.AddFundsFragment?.Get_Data_User();

                                        tabbedWallet.SendMoneyFragment?.Get_Data_User();
                                    }

                                    var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                                    if (dataUser != null)
                                    {
                                        dataUser.Balance = creditObject.Balance;
                                        dataUser.Wallet = creditObject.Wallet;

                                        var sqlEntity = new SqLiteDatabase();
                                        sqlEntity.Insert_Or_Update_To_MyProfileTable(dataUser);
                                    }

                                    Toast.MakeText(this, GetText(Resource.String.Lbl_PaymentSuccessfully), ToastLength.Long)?.Show();
                                }

                                AndHUD.Shared.Dismiss();
                                Finish();
                                break;
                            default:
                                Methods.DisplayAndHudErrorResult(this, respond);
                                break;
                        }
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                    }
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseVerifyDataCard), ToastLength.Long)?.Show();
                }
            }
            catch (Exception exception)
            {
                AndHUD.Shared.Dismiss();
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private string InsertPeriodically(string text, string insert, int period)
        {
            try
            {
                var parts = SplitInParts(text, period);
                string formatted = string.Join(insert, parts);
                return formatted;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return text;
            }
        }

        public static IEnumerable<string> SplitInParts(string s, int partLength)
        {
            if (s == null)
                throw new ArgumentNullException("s");
            if (partLength <= 0)
                throw new ArgumentException("Part length has to be positive.", "partLength");

            for (var i = 0; i < s.Length; i += partLength)
                yield return s.Substring(i, Math.Min(partLength, s.Length - i));
        }

        #endregion

        #region AuthorizeNet

        //private string API_LOGIN_ID, CLIENT_KEY;
        private bool InitWalletAuthorizeNet()
        {
            try
            {
                var authorizeLoginId = ListUtils.SettingsSiteList?.AuthorizeLoginId ?? "";
                if (!string.IsNullOrEmpty(authorizeLoginId))
                {
                    //API_LOGIN_ID = ListUtils.SettingsSiteList?.AuthorizeLoginId;
                    //CLIENT_KEY = ListUtils.SettingsSiteList?.AuthorizeTransactionKey;

                    //var mode = ListUtils.SettingsSiteList?.AuthorizeTestMode;
                    //if (mode == "SANDBOX") 
                    //{
                    //    apiClient = new AcceptSdkApiClient.Builder(this, AcceptSdkApiClient.Environment.Sandbox)
                    //        .ConnectionTimeout(4000) // optional connection time out in milliseconds
                    //        .Build();
                    //}
                    //else if (mode == "PRODUCTION")
                    //{
                    //    apiClient = new AcceptSdkApiClient.Builder(this, AcceptSdkApiClient.Environment.Production)
                    //        .ConnectionTimeout(4000) // optional connection time out in milliseconds
                    //        .Build();
                    //} 
                    return true;
                }

                //error
                Toast.MakeText(this, GetText(Resource.String.Lbl_ErrorConnectionPayment), ToastLength.Long)?.Show();
                return false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;
            }
        }


        //private EncryptTransactionObject PrepareTransactionObject(Card card)
        //{
        //    try
        //    {
        //        ClientKeyBasedMerchantAuthentication merchantAuthentication = ClientKeyBasedMerchantAuthentication.CreateMerchantAuthentication(API_LOGIN_ID, CLIENT_KEY);

        //        // create a transaction object by calling the predefined api for creation
        //        var ttt = TransactionObject.CreateTransactionObject(TransactionType.SdkTransactionEncryption) // type of transaction object
        //            .CardData(PrepareCardData(card)) // card data to get Token
        //            .MerchantAuthentication(merchantAuthentication).Build();

        //        return ttt;
        //    }
        //    catch (Exception e)
        //    {
        //        Methods.DisplayReportResultTrack(e);
        //        return null;
        //    }
        //}

        //private CardData PrepareCardData(Card card)
        //{
        //    try
        //    {
        //        var sss = new CardData.Builder("4111 1111 1111 1111", card.ExpMonth.ToString(), card.ExpYear.ToString()).CvvCode(card.CVC)
        //            .ZipCode("")
        //            .CardHolderName("")
        //            .Build();
        //        return sss;
        //    }
        //    catch (Exception e)
        //    {
        //        Methods.DisplayReportResultTrack(e);
        //        return null;
        //    }
        //}

        #endregion

        //public void OnErrorReceived(ErrorTransactionResponse errorResponse)
        //{
        //    try
        //    {
        //        AndHUD.Shared.Dismiss();

        //        //error

        //        Message error = errorResponse.FirstErrorMessage;

        //        Console.WriteLine("code" + error.MessageCode + "\n" + "message" + error.MessageText); 
        //    }
        //    catch (Exception e)
        //    {
        //        Methods.DisplayReportResultTrack(e);
        //    }
        //}

        //public void OnEncryptionFinished(EncryptTransactionResponse response)
        //{
        //    try
        //    { 
        //        AndHUD.Shared.Dismiss();

        //        //
        //        Console.WriteLine("DataDescriptor" + response.DataDescriptor + "\n" + "DataValue" + response.DataValue); 

        //    }
        //    catch (Exception e)
        //    {
        //        Methods.DisplayReportResultTrack(e);
        //    }
        //}
    }
}