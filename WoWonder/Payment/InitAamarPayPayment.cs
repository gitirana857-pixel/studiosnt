using AamarPay;
using Android.App;
using Android.Widget;
using Newtonsoft.Json;
using Org.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WoWonder.Activities.Tabbes;
using WoWonder.Activities.Wallet;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Utils;
using WoWonder.SQLite;
using WoWonderClient.Classes.Payments;
using WoWonderClient.Requests;

namespace WoWonder.Payment
{
    public class InitAamarPayPayment : InitAamarPay.IOnInitListener, InitAamarPay.ITransactionInfoListener
    {
        private readonly Activity ActivityContext;
        private readonly TabbedMainActivity GlobalContext;
        private DialogBuilder DialogBuilder;
        private AlertDialog AlertDialog;
        private string TransactionId;
        private InitAamarPay AamarPay;
        private string Price;

        public InitAamarPayPayment(Activity activity)
        {
            try
            {
                ActivityContext = activity;
                GlobalContext = TabbedMainActivity.GetInstance();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //AamarPay
        public void BtnAamarPayOnClick(string price)
        {
            try
            {
                Price = price;
                var option = ListUtils.MyProfileList.FirstOrDefault();
                var currency = ListUtils.SettingsSiteList?.AamarpayPayment ?? "USD";

                DialogBuilder = new DialogBuilder(ActivityContext, AlertDialog);

                // Initiate payment
                AamarPay = new InitAamarPay(ActivityContext, ListUtils.SettingsSiteList?.AamarpayStoreId, ListUtils.SettingsSiteList?.AamarpaySignatureKey);

                switch (ListUtils.SettingsSiteList?.AamarpayMode)
                {
                    case "live":
                        // Set Live Mode
                        AamarPay.TestMode(false);
                        break;
                    case "sandbox":
                        // Set Test Mode
                        AamarPay.TestMode(true);
                        break;
                    default:
                        // Set Test Mode
                        AamarPay.TestMode(true);
                        break;
                }

                // Auto generate Trx
                AamarPay.AutoGenerateTransactionId(true);

                // Generate unique transaction id
                TransactionId = AamarPay.generate_trx_id();

                DialogBuilder.ShowLoading();
                AamarPay.SetTransactionParameter(price, currency, "Pay the card");
                AamarPay.SetCustomerDetails(option.Name, option.Email, option.PhoneNumber, "Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1", "Istanbul", "Turkey");
                AamarPay.InitPgw(this);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnInitFailure(bool? error, string message)
        {
            DialogBuilder.DismissDialog();
            DialogBuilder.ErrorPopUp(message);
        }

        public void OnPaymentSuccess(JSONObject jsonObject)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<SuccessAamarPayObject>(jsonObject.ToString());
                if (data != null)
                {
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => SuccessAamarPay(data.MerTxnid, data.OptA, data.PayStatus) });
                    DialogBuilder.DismissDialog();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnPaymentFailure(JSONObject jsonObject)
        {
            DialogBuilder.DismissDialog();
        }

        public void OnPaymentProcessingFailed(JSONObject jsonObject)
        {
            DialogBuilder.DismissDialog();
        }

        public void OnPaymentCancel(JSONObject jsonObject)
        {
            try
            {
                AamarPay.GetTransactionInfo(jsonObject.GetString("trx_id"), this);
            }
            catch (JSONException e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnSuccess(JSONObject jsonObject)
        {
            DialogBuilder.DismissDialog();
        }

        public void OnFailure(bool? error, string message)
        {
            DialogBuilder.DismissDialog();
            DialogBuilder.ErrorPopUp(message);
        }

        private async Task SuccessAamarPay(string merTxnid, string OptA, string payStatus)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    if (Methods.CheckConnectivity())
                    {
                        var (apiStatus, respond) = await RequestsAsync.Payments.AamarPaySuccessAsync(Price, merTxnid, OptA, payStatus);
                        if (apiStatus == 200)
                        {
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

                                Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_PaymentSuccessfully), ToastLength.Long)?.Show();
                            }
                        }
                        else
                            Methods.DisplayReportResult(ActivityContext, respond);
                    }
                    else
                    {
                        Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                    }
                }
                else
                {
                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

    }
}