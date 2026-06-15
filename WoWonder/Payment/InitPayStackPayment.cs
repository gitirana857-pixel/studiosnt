using Android.App;
using Android.Graphics;
using Android.Util;
using Android.Webkit;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WoWonder.Activities.Wallet;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Utils;
using WoWonder.SQLite;
using WoWonderClient.Classes.Payments;
using WoWonderClient.Requests;
using Exception = System.Exception;

namespace WoWonder.Payment
{
    public class InitPayStackPayment
    {
        private readonly Activity ActivityContext;
        private Dialog PayStackWindow;
        private WebView HybridView;
        private string Url, Price;
        public InitPayStackPayment(Activity context)
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

        public void DisplayPayStackPayment(string url, string price)
        {
            try
            {
                Url = url;
                Price = price;

                PayStackWindow = new Dialog(ActivityContext, WoWonderTools.IsTabDark() ? Resource.Style.MyDialogThemeDark : Resource.Style.MyDialogTheme);
                PayStackWindow.SetContentView(Resource.Layout.PaymentWebViewLayout);

                var title = (TextView)PayStackWindow.FindViewById(Resource.Id.toolbar_title);
                if (title != null)
                    title.Text = ActivityContext.GetText(Resource.String.Lbl_PayWith) + " " + ActivityContext.GetText(Resource.String.Lbl_PayStack);

                var closeButton = (TextView)PayStackWindow.FindViewById(Resource.Id.toolbar_close);
                if (closeButton != null)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, closeButton, IonIconsFonts.Close);

                    closeButton.SetTextSize(ComplexUnitType.Sp, 20f);
                    closeButton.Click += CloseButtonOnClick;
                }

                HybridView = PayStackWindow.FindViewById<WebView>(Resource.Id.LocalWebView);

                //Set WebView
                if (HybridView != null)
                {
                    HybridView.SetWebViewClient(new MyWebViewClient(this));
                    if (HybridView.Settings != null)
                    {
                        HybridView.Settings.LoadsImagesAutomatically = true;
                        HybridView.Settings.JavaScriptEnabled = true;
                        HybridView.Settings.JavaScriptCanOpenWindowsAutomatically = true;
                        HybridView.Settings.SetLayoutAlgorithm(WebSettings.LayoutAlgorithm.TextAutosizing);
                        HybridView.Settings.DomStorageEnabled = true;
                        HybridView.Settings.AllowFileAccess = true;
                        HybridView.Settings.DefaultTextEncodingName = "utf-8";

                        HybridView.Settings.UseWideViewPort = true;
                        HybridView.Settings.LoadWithOverviewMode = true;

                        HybridView.Settings.SetSupportZoom(false);
                        HybridView.Settings.BuiltInZoomControls = false;
                        HybridView.Settings.DisplayZoomControls = false;
                    }

                    //Load url to be rendered on WebView
                    HybridView.LoadUrl(Url);
                }

                PayStackWindow.Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void CloseButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                PayStackWindow.Hide();
                PayStackWindow.Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void StopPayStack()
        {
            try
            {
                if (PayStackWindow != null)
                {
                    PayStackWindow.Hide();
                    PayStackWindow.Dismiss();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task PayStack(string reference, string request)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var keyValues = new Dictionary<string, string>
                    {
                        {"reference", reference},
                    };

                    var priceInt = Convert.ToInt32(Price) * 100;

                    keyValues.Add("amount", priceInt.ToString());

                    var (apiStatus, respond) = await RequestsAsync.Payments.PayStackAsync(request, keyValues);
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

                                Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_PaymentSuccessfully), ToastLength.Long)?.Show();
                            }
                            ActivityContext.Finish();
                            break;
                        default:
                            Methods.DisplayReportResult(ActivityContext, respond);
                            break;
                    }
                }
                else
                {
                    ToastUtils.ShowToast(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private class MyWebViewClient : WebViewClient
        {
            private readonly InitPayStackPayment MActivity;
            public MyWebViewClient(InitPayStackPayment mActivity)
            {
                MActivity = mActivity;
            }

            public override bool ShouldOverrideUrlLoading(WebView view, IWebResourceRequest request)
            {
                try
                {
                    if (string.IsNullOrEmpty(request?.Url?.ToString()))
                        return false;

                    if (request.Url.ToString() == MActivity.Url)
                    {
                        view.LoadUrl(request.Url.ToString());
                    }
                    else if (request.Url.ToString().Contains("reference"))
                    {
                        //https://demo.wowonder.com/requests.php?f=paystack&s=fund&amount=12&fund_id=403&trxref=5f3f88e2a43c7&reference=5f3f88e2a43c7

                        var reference = request.Url.ToString()?.Split("&reference=")?.Last();
                        if (string.IsNullOrEmpty(reference))
                            return false;

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => MActivity.PayStack(reference, "wallet") });
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
                return false;
            }

            public override void OnPageStarted(WebView view, string url, Bitmap favicon)
            {
                try
                {
                    base.OnPageStarted(view, url, favicon);

                    if (view.Settings != null)
                    {
                        view.Settings.JavaScriptEnabled = true;
                        view.Settings.DomStorageEnabled = true;
                        view.Settings.AllowFileAccess = true;
                        view.Settings.JavaScriptCanOpenWindowsAutomatically = true;
                        view.Settings.UseWideViewPort = true;
                        view.Settings.LoadWithOverviewMode = true;
                        view.Settings.SetSupportZoom(false);
                        view.Settings.BuiltInZoomControls = false;
                        view.Settings.DisplayZoomControls = false;
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }
    }
}