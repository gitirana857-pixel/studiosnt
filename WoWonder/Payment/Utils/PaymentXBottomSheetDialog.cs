using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Google.Android.Material.BottomSheet;
using System;
using WoWonder.Activities.Wallet;
using WoWonder.Helpers.Utils;
using WoWonder.PaymentGoogle;
using WoWonderClient;

namespace WoWonder.Payment.Utils
{
    public class PaymentXBottomSheetDialog : BottomSheetDialogFragment
    {
        #region Variables Basic

        private TabbedWalletActivity GlobalContext;
        private ImageView IconClose;
        private LinearLayout GooglePayLayout, PaypalLayout, CreditCardLayout, BankTransferLayout, RazorPayLayout, CashFreeLayout, PayStackLayout, PaySeraLayout;
        private LinearLayout SecurionPayLayout, AuthorizeNetLayout, IyziPayLayout, AamarPayLayout, FlutterWaveLayout;

        private string Price;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                GlobalContext = TabbedWalletActivity.GetInstance();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                var contextThemeWrapper = WoWonderTools.IsTabDark() ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme);

                // clone the inflater using the ContextThemeWrapper 
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);
                View view = localInflater?.Inflate(Resource.Layout.PaymentXBottomSheetLayout, container, false);

                Price = Arguments?.GetString("Price") ?? "";

                InitComponent(view);

                return view;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null;
            }
        }

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                IconClose = view.FindViewById<ImageView>(Resource.Id.iconClose);
                IconClose.Click += IconCloseOnClick;

                GooglePayLayout = view.FindViewById<LinearLayout>(Resource.Id.GooglePayLayout);
                GooglePayLayout.Click += GooglePayLayoutOnClick;

                PaypalLayout = view.FindViewById<LinearLayout>(Resource.Id.PaypalLayout);
                PaypalLayout.Click += PaypalLayoutOnClick;

                CreditCardLayout = view.FindViewById<LinearLayout>(Resource.Id.CreditCardLayout);
                CreditCardLayout.Click += CreditCardLayoutOnClick;

                BankTransferLayout = view.FindViewById<LinearLayout>(Resource.Id.BankTransferLayout);
                BankTransferLayout.Click += BankTransferLayoutOnClick;

                RazorPayLayout = view.FindViewById<LinearLayout>(Resource.Id.RazorPayLayout);
                RazorPayLayout.Click += RazorPayLayoutOnClick;

                CashFreeLayout = view.FindViewById<LinearLayout>(Resource.Id.CashFreeLayout);
                CashFreeLayout.Click += CashFreeLayoutOnClick;

                PayStackLayout = view.FindViewById<LinearLayout>(Resource.Id.PayStackLayout);
                PayStackLayout.Click += PayStackLayoutOnClick;

                PaySeraLayout = view.FindViewById<LinearLayout>(Resource.Id.PaySeraLayout);
                PaySeraLayout.Click += PaySeraLayoutOnClick;

                SecurionPayLayout = view.FindViewById<LinearLayout>(Resource.Id.SecurionPayLayout);
                SecurionPayLayout.Click += SecurionPayLayoutOnClick;

                AuthorizeNetLayout = view.FindViewById<LinearLayout>(Resource.Id.AuthorizeNetLayout);
                AuthorizeNetLayout.Click += AuthorizeNetLayoutOnClick;

                IyziPayLayout = view.FindViewById<LinearLayout>(Resource.Id.IyziPayLayout);
                IyziPayLayout.Click += IyziPayLayoutOnClick;

                AamarPayLayout = view.FindViewById<LinearLayout>(Resource.Id.AamarPayLayout);
                AamarPayLayout.Click += AamarPayLayoutOnClick;

                FlutterWaveLayout = view.FindViewById<LinearLayout>(Resource.Id.FlutterWaveLayout);
                FlutterWaveLayout.Click += FlutterWaveLayoutOnClick;

                if (AppSettings.ShowInAppBilling && InitializeWoWonder.IsExtended)
                    GooglePayLayout.Visibility = ViewStates.Visible;
                else
                    GooglePayLayout.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowPaypal)
                    PaypalLayout.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowCreditCard)
                    CreditCardLayout.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowBankTransfer)
                    BankTransferLayout.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowRazorPay)
                    RazorPayLayout.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowCashFree)
                    CashFreeLayout.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowSecurionPay)
                    SecurionPayLayout.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowAuthorizeNet)
                    AuthorizeNetLayout.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowIyziPay)
                    IyziPayLayout.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowPayStack)
                    PayStackLayout.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowPaySera)
                    PaySeraLayout.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowAamarPay)
                    AamarPayLayout.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowFlutterWave)
                    FlutterWaveLayout.Visibility = ViewStates.Gone;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event

        private void GooglePayLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                switch (Price)
                {
                    case "5": // Donation with Amount 5
                        GlobalContext.BillingSupport?.PurchaseNow(InAppBillingGoogle.Donation5);
                        break;
                    case "10":  // Donation with Amount 10
                        GlobalContext.BillingSupport?.PurchaseNow(InAppBillingGoogle.Donation10);
                        break;
                    case "15": // Donation with Amount 15
                        GlobalContext.BillingSupport?.PurchaseNow(InAppBillingGoogle.Donation15);
                        break;
                    case "20": // Donation with Amount 20
                        GlobalContext.BillingSupport?.PurchaseNow(InAppBillingGoogle.Donation20);
                        break;
                    case "25": // Donation with Amount 25
                        GlobalContext.BillingSupport?.PurchaseNow(InAppBillingGoogle.Donation25);
                        break;
                    case "30": // Donation with Amount 30
                        GlobalContext.BillingSupport?.PurchaseNow(InAppBillingGoogle.Donation30);
                        break;
                    case "35": // Donation with Amount 35
                        GlobalContext.BillingSupport?.PurchaseNow(InAppBillingGoogle.Donation35);
                        break;
                    case "40": // Donation with Amount 40
                        GlobalContext.BillingSupport?.PurchaseNow(InAppBillingGoogle.Donation40);
                        break;
                    case "45": // Donation with Amount 45
                        GlobalContext.BillingSupport?.PurchaseNow(InAppBillingGoogle.Donation45);
                        break;
                    case "50": // Donation with Amount 50
                        GlobalContext.BillingSupport?.PurchaseNow(InAppBillingGoogle.Donation50);
                        break;
                    case "55": // Donation with Amount 55
                        GlobalContext.BillingSupport?.PurchaseNow(InAppBillingGoogle.Donation55);
                        break;
                    case "60": // Donation with Amount 60
                        GlobalContext.BillingSupport?.PurchaseNow(InAppBillingGoogle.Donation60);
                        break;
                    case "65": // Donation with Amount 65
                        GlobalContext.BillingSupport?.PurchaseNow(InAppBillingGoogle.Donation65);
                        break;
                    case "70": // Donation with Amount 70
                        GlobalContext.BillingSupport?.PurchaseNow(InAppBillingGoogle.Donation70);
                        break;
                    case "75": // Donation with Amount 75
                        GlobalContext.BillingSupport?.PurchaseNow(InAppBillingGoogle.Donation75);
                        break;
                    case "80": // Donation with Amount 80
                    case "85": // Donation with Amount 85
                    case "90": // Donation with Amount 90
                    case "95": // Donation with Amount 95
                    case "100": // Donation with Amount 100
                    case "Funding": // Donation with Amount long 100
                        GlobalContext.BillingSupport?.PurchaseNow(InAppBillingGoogle.DonationDefault);
                        break;
                }
                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void PaypalLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                GlobalContext.InitPayPalPayment.BtnPaypalOnClick(Price);
                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void CreditCardLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                Intent intent = new Intent(GlobalContext, typeof(PaymentCardDetailsActivity));
                intent.PutExtra("Price", Price);
                GlobalContext.StartActivity(intent);

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BankTransferLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                Intent intent = new Intent(GlobalContext, typeof(PaymentLocalActivity));
                intent.PutExtra("Price", Price);
                GlobalContext.StartActivity(intent);

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void RazorPayLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                GlobalContext.InitRazorPay?.BtnRazorPayOnClick(Price);
                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void CashFreeLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                GlobalContext.OpenCashFreeDialog();
                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void PayStackLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                GlobalContext.OpenPayStackDialog();

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void PaySeraLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                Toast.MakeText(Activity, GetText(Resource.String.Lbl_Please_wait), ToastLength.Long)?.Show();
                await GlobalContext.PaySera();
                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void SecurionPayLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                Toast.MakeText(Activity, GetText(Resource.String.Lbl_Please_wait), ToastLength.Long)?.Show();
                await GlobalContext.OpenSecurionPay();

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void AuthorizeNetLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                Intent intent = new Intent(GlobalContext, typeof(AuthorizeNetPaymentActivity));
                intent.PutExtra("Price", Price);
                GlobalContext.StartActivity(intent);

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void IyziPayLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                GlobalContext.IyziPay();

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void AamarPayLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                GlobalContext.AamarPayPayment?.BtnAamarPayOnClick(Price);

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void FlutterWaveLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                Intent intent = new Intent(GlobalContext, typeof(FlutterWaveActivity));
                intent.PutExtra("Price", Price);
                GlobalContext.StartActivity(intent);

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void IconCloseOnClick(object sender, EventArgs e)
        {
            try
            {
                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

    }
}