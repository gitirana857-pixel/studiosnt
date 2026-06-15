using Android.App;
using Android.Widget;
using AndroidHUD;
using Com.Cashfree.PG.Api;
using Com.Cashfree.PG.Core.Api;
using Com.Cashfree.PG.Core.Api.Callback;
using Com.Cashfree.PG.Core.Api.Utils;
using Com.Cashfree.PG.UI.Api;
using WoWonder.Helpers.Utils;
using WoWonder.Payment.Utils;
using WoWonderClient.Classes.Payments;
using WoWonderClient.Requests;
using Exception = System.Exception;
using Object = Java.Lang.Object;

namespace WoWonder.Payment
{
    public class InitCashFreePayment : Object, ICFCheckoutResponseCallback
    {
        private readonly Activity ActivityContext;
        private string Price;
        private CashFreeObject CashFreeObject;

        public InitCashFreePayment(Activity context)
        {
            try
            {
                ActivityContext = context;

                CFPaymentGatewayService.Initialize(context); // Application Context.
                AnalyticsUtil.SendPaymentEventsToBackend(); // required for error reporting.

                CFPaymentGatewayService.Instance?.SetCheckoutCallback(this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void DisplayCashFreePayment(CashFreeObject cashFreeObject, string price)
        {
            ActivityContext.RunOnUiThread(() =>
            {
                try
                {
                    CashFreeObject = cashFreeObject;
                    Price = price;

                    CFSession.Environment cfEnvironment = ListUtils.SettingsSiteList?.CashfreeMode switch
                    {
                        "SandBox" => CFSession.Environment.Sandbox,
                        "Live" => CFSession.Environment.Production,
                        _ => CFSession.Environment.Sandbox
                    };

                    CFSession cfSession = new CFSession.CFSessionBuilder()
                        ?.SetEnvironment(cfEnvironment)
                        ?.SetPaymentSessionID(CashFreeObject.OrderLinkObject.PaymentSessionId)
                        ?.SetOrderId(CashFreeObject.OrderLinkObject.OrderId)
                        ?.Build();

                    //CFPaymentComponent cfPaymentComponent = new CFPaymentComponent.CFPaymentComponentBuilder()
                    //    .Add(CFPaymentComponent.CFPaymentModes.Card)
                    //    .Add(CFPaymentComponent.CFPaymentModes.Upi)
                    //    .Add(CFPaymentComponent.CFPaymentModes.Wallet)
                    //    .Build();

                    CFTheme cfTheme = new CFTheme.CFThemeBuilder()
                        ?.SetNavigationBarBackgroundColor(AppSettings.MainColor)
                        ?.SetNavigationBarTextColor("#ffffff")
                        ?.SetButtonBackgroundColor(AppSettings.MainColor)
                        ?.SetButtonTextColor("#ffffff")
                        ?.SetPrimaryTextColor("#000000")
                        ?.SetSecondaryTextColor("#000000")
                        ?.Build();

                    CFDropCheckoutPayment cfDropCheckoutPayment = new CFDropCheckoutPayment.CFDropCheckoutPaymentBuilder()
                        ?.SetSession(cfSession)
                        //By default all modes are enabled. If you want to restrict the payment modes uncomment the next line
                        //?.SetCFUIPaymentModes(cfPaymentComponent)
                        ?.SetCFNativeCheckoutUITheme(cfTheme)
                        ?.Build();

                    CFPaymentGatewayService gatewayService = CFPaymentGatewayService.Instance;
                    gatewayService?.DoPayment(ActivityContext, cfDropCheckoutPayment);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            });
        }

        public void OnPaymentFailure(CFErrorResponse cfErrorResponse, string orderId)
        {
            try
            {
                //Error  
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public async void OnPaymentVerify(string orderId)
        {
            try
            {
                //verifyPayment triggered  
                if (Methods.CheckConnectivity())
                {
                    var (apiStatus, respond) = await RequestsAsync.Payments.CashFreeGetStatusAsyncV2(ListUtils.SettingsSiteList?.CashfreeClientKey, ListUtils.SettingsSiteList?.CashfreeSecretKey ?? "", orderId, ListUtils.SettingsSiteList?.CashfreeMode);
                    if (apiStatus == 200)
                    {
                        if (respond is CashFreeOrderLinkObject result)
                            CashFree(result, "wallet");
                    }
                    else
                        Methods.DisplayReportResult(ActivityContext, respond);
                }
                else
                {
                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void CashFree(CashFreeOrderLinkObject statusObject, string request)
        {
            try
            {
                if (ActivityContext is PaymentBaseActivity activity)
                {
                    activity.TopWallet();
                }

                //if (Methods.CheckConnectivity())
                //{
                //    var keyValues = new Dictionary<string, string>
                //    {
                //        {"txStatus", statusObject.TxStatus},
                //        {"orderAmount", statusObject.OrderAmount},
                //        {"referenceId", statusObject.ReferenceId},
                //        {"paymentMode", statusObject.PaymentMode},
                //        {"txMsg", statusObject.TxMsg},
                //        {"txTime", statusObject.TxTime},

                //        //{"signature", CashFreeObject.OrderLinkObject.Signature},
                //        {"orderId", CashFreeObject.OrderLinkObject.OrderId},
                //        {"amount", Price}
                //    };

                //    var (apiStatus, respond) = await RequestsAsync.Payments.CashFreeAsync(request, keyValues);
                //    switch (apiStatus)
                //    {
                //        case 200:
                //            AndHUD.Shared.Dismiss();

                //            switch (request)
                //            {
                //                case "fund":
                //                    ToastUtils.ShowToast(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_Donated), ToastLength.Long);
                //                    FundingViewActivity.GetInstance()?.StartApiService();
                //                    break;
                //                case "upgrade":
                //                {
                //                    var dataUser = ListUtils.MyProfileList?.FirstOrDefault();

                //                    if (dataUser != null)
                //                    {
                //                        dataUser.IsPro = "1";

                //                        var sqlEntity = new SqLiteDatabase();
                //                        sqlEntity.Insert_Or_Update_To_MyProfileTable(dataUser);

                //                    }
                //                    ToastUtils.ShowToast(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_Upgraded), ToastLength.Long);
                //                }   break;
                //                case "wallet":
                //                    if (respond is PaymentSuccessfullyObject creditObject)
                //                    {
                //                        var tabbedWallet = TabbedWalletActivity.GetInstance();
                //                        if (tabbedWallet != null)
                //                        {
                //                            tabbedWallet.AddFundsFragment.TxtAmount.Text = string.Empty;
                //                            tabbedWallet.AddFundsFragment?.Get_Data_User();

                //                            tabbedWallet.SendMoneyFragment?.Get_Data_User();
                //                        }

                //                        var data = ListUtils.MyProfileList?.FirstOrDefault();
                //                        if (data != null)
                //                        {
                //                            data.Balance = creditObject.Balance;
                //                            data.Wallet = creditObject.Wallet;

                //                            var sqlEntity = new SqLiteDatabase();
                //                            sqlEntity.Insert_Or_Update_To_MyProfileTable(data);
                //                        }

                //                        Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_PaymentSuccessfully), ToastLength.Long)?.Show();
                //                    }
                //                    break;
                //            }

                //            break;
                //        default:
                //            Methods.DisplayAndHudErrorResult(ActivityContext, respond);
                //            break;
                //    }
                //}
                //else
                //{
                //    ToastUtils.ShowToast(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                //}
            }
            catch (Exception e)
            {
                AndHUD.Shared.Dismiss();
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}