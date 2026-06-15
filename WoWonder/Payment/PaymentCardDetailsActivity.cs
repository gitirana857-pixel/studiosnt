using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.Activity;
using AndroidX.AppCompat.Widget;
using Com.Google.Android.Gms.Ads.Admanager;
using Com.Stripe.Android;
using Com.Stripe.Android.Model;
using Com.Stripe.Android.Payments.Paymentlauncher;
using Com.Stripe.Android.View;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WoWonder.Activities.Wallet;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Payments;
using WoWonderClient.Requests;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Payment
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class PaymentCardDetailsActivity : ComponentActivity, IPaymentLauncher.IPaymentResultCallback
    {
        #region Variables Basic

        private TextView CardNumber, CardExpire, CardCvv, CardName;
        private EditText EtName;
        private AppCompatButton BtnApply;
        private CardMultilineWidget MultilineWidget;
        private AdManagerAdView AdManagerAdView;

        private IPaymentLauncher StripePaymentLauncher;
        private Stripe Stripe;
        private string Price, TokenId, ClientSecret/*, HashId*/, SessionId;

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
                InitWalletStripe();
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

                MultilineWidget = FindViewById<CardMultilineWidget>(Resource.Id.card_multiline_widget);

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
                var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    toolbar.Title = GetString(Resource.String.Lbl_CreditCard);
                    toolbar.SetTitleTextColor(WoWonderTools.IsTabDark() ? Color.White : Color.Black);
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

        //Stripe
        private void BtnApplyOnClick(object sender, EventArgs e)
        {
            try
            {
                //Show a progress
                PaymentMethodCreateParams createParams = MultilineWidget.PaymentMethodCreateParams;
                if (createParams != null && MultilineWidget.ValidateAllFields() && !string.IsNullOrEmpty(EtName.Text))
                {
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                    var token = Stripe.CreateCardTokenSynchronous(MultilineWidget.CardParams);
                    TokenId = token.Id;

                    ConfirmPaymentIntentParams confirmParams = ConfirmPaymentIntentParams.CreateWithPaymentMethodCreateParams(createParams, ClientSecret);
                    StripePaymentLauncher.Confirm(confirmParams);
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

        #region Stripe

        private void InitWalletStripe()
        {
            try
            {
                string stripePublishableKey = ListUtils.SettingsSiteList?.StripeId ?? "";
                if (!string.IsNullOrEmpty(stripePublishableKey))
                {
                    PaymentConfiguration.Init(this, stripePublishableKey);

                    PaymentConfiguration paymentConfiguration = PaymentConfiguration.GetInstance(this);

                    var paymentLauncher = IPaymentLauncher.Companion.GetInstance();
                    StripePaymentLauncher = paymentLauncher.Create(this, paymentConfiguration.PublishableKey, paymentConfiguration.StripeAccountId, this);
                    Stripe = new Stripe(this, stripePublishableKey);

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { StripeCreatePayment, CreateStripeHash });
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_ErrorConnectionSystemStripe), ToastLength.Long)?.Show();
                    Finish();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async void OnSuccess()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var tabbedWallet = TabbedWalletActivity.GetInstance();
                    if (tabbedWallet != null)
                    {
                        //var priceInt = Convert.ToInt32(Price) * 100;
                        var (apiStatus, respond) = await RequestsAsync.Payments.TopWalletStripeAsync(TokenId, Price);
                        switch (apiStatus)
                        {
                            case 200:
                                tabbedWallet.AddFundsFragment.TxtAmount.Text = string.Empty;
                                Toast.MakeText(this, GetText(Resource.String.Lbl_PaymentSuccessfully), ToastLength.Long)?.Show();

                                AndHUD.Shared.Dismiss();
                                Finish();
                                break;
                            default:
                                Methods.DisplayAndHudErrorResult(this, respond);
                                break;
                        }
                    }
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                    AndHUD.Shared.Dismiss();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                AndHUD.Shared.Dismiss();
            }
        }

        public void OnPaymentResult(PaymentResult paymentResult)
        {
            try
            {
                string message = "";
                if (paymentResult is PaymentResult.Completed completed)
                {
                    OnSuccess();
                }
                else if (paymentResult is PaymentResult.Canceled canceled)
                {
                    AndHUD.Shared.Dismiss();
                    message = "Canceled!";
                }
                else if (paymentResult is PaymentResult.Failed failed)
                {
                    // This string comes from the PaymentIntent's error message.
                    // See here: https://stripe.com/docs/api/payment_intents/object#payment_intent_object-last_payment_error-message
                    message = "Failed: " + failed.Throwable.Message;

                    AndHUD.Shared.Dismiss();
                }

                if (!string.IsNullOrEmpty(message))
                {
                    Toast.MakeText(this, message, ToastLength.Long)?.Show();
                }
                Console.WriteLine("PaymentResult: " + message);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task StripeCreatePayment()
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
                else
                {
                    var currency = ListUtils.SettingsSiteList?.StripeCurrency ?? "USD";
                    var stripeSecretKey = ListUtils.SettingsSiteList?.StripeSecret ?? "";
                    var priceInt = Convert.ToInt32(Price) * 100;
                    var (apiStatus, respond) = await RequestsAsync.Payments.StripeCreatePaymentIntentsAsync(stripeSecretKey, priceInt.ToString(), currency);
                    if (apiStatus == 200)
                    {
                        if (respond is StripeCreatePaymentObject result)
                        {
                            if (!string.IsNullOrEmpty(result?.ClientSecret))
                            {
                                ClientSecret = result.ClientSecret;
                            }
                        }
                    }
                    else
                    {
                        if (respond is StripeCreatePaymentObject result)
                        {
                            Toast.MakeText(this, result.Error?.Message, ToastLength.Long)?.Show();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task CreateStripeHash()
        {
            if (!Methods.CheckConnectivity())
            {
                Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
            else
            {
                var (apiStatus, respond) = await RequestsAsync.Payments.CreateStripeAsync(Price);
                if (apiStatus == 200)
                {
                    if (respond is CreateStripeObject result)
                    {
                        SessionId = result.SessionId;
                    }
                }
                else Methods.DisplayAndHudErrorResult(this, respond);
            }
        }

        #endregion

    }
}