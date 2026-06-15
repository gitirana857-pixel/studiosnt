using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using AndroidX.AppCompat.App;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;

namespace WoWonder.Activities
{
    [Activity(Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.KeyboardHidden)]
    public class ChatWebViewActivity : AppCompatActivity
    {
        private WebView _webView;
        private ImageView _backButton;
        private ProgressBar _progressBar;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetContentView(Resource.Layout.ChatWebViewLayout);

                _webView = FindViewById<WebView>(Resource.Id.ChatWebView);
                _backButton = FindViewById<ImageView>(Resource.Id.toolbar_back);
                _progressBar = FindViewById<ProgressBar>(Resource.Id.webview_progress);

                _backButton.Click += (sender, args) => Finish();

                SetupWebView();
            }
            catch (System.Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                Finish();
            }
        }

        private void SetupWebView()
        {
            if (_webView == null) return;

            string siteUrl = GetSiteUrl();
            string authToken = UserDetails.AccessToken;

            // Set the PHP session cookie so the WebView is already logged in
            if (!string.IsNullOrEmpty(authToken))
            {
                var cookieManager = CookieManager.Instance;
                cookieManager.SetAcceptCookie(true);
                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Lollipop)
                {
                    cookieManager.SetAcceptThirdPartyCookies(_webView, true);
                }

                // Set PHPSESSID cookie for the site domain
                string domain = Android.Net.Uri.Parse(siteUrl).Host;
                cookieManager.SetCookie(domain, "PHPSESSID=" + authToken + "; Domain=" + domain + "; Path=/");
                
                // Also try with www prefix if needed
                if (domain.StartsWith("www."))
                {
                    string bareDomain = domain.Substring(4);
                    cookieManager.SetCookie(bareDomain, "PHPSESSID=" + authToken + "; Domain=" + bareDomain + "; Path=/");
                }

                // Sync cookies
                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Lollipop)
                {
                    cookieManager.Flush();
                }
            }

            _webView.Settings.JavaScriptEnabled = true;
            _webView.Settings.DomStorageEnabled = true;
            _webView.Settings.AllowFileAccess = true;
            _webView.Settings.DefaultTextEncodingName = "utf-8";
            _webView.Settings.UseWideViewPort = true;
            _webView.Settings.LoadWithOverviewMode = true;
            _webView.Settings.BuiltInZoomControls = false;
            _webView.Settings.DisplayZoomControls = false;

            _webView.SetWebViewClient(new ChatWebViewClient(this));
            _webView.SetWebChromeClient(new ChatWebChromeClient(this));

            _webView.LoadUrl(siteUrl + "/messages");
        }

        private string GetSiteUrl()
        {
            string siteUrl = Resources.GetString(Resource.String.ApplicationUrlWeb);
            if (string.IsNullOrEmpty(siteUrl))
                siteUrl = "studiosnt.sntwork.com";

            if (!siteUrl.StartsWith("http"))
                siteUrl = "https://www." + siteUrl;

            return siteUrl.TrimEnd('/');
        }

        public override void OnBackPressed()
        {
            if (_webView.CanGoBack())
                _webView.GoBack();
            else
                Finish();
        }

        private class ChatWebViewClient : WebViewClient
        {
            private readonly ChatWebViewActivity _activity;

            public ChatWebViewClient(ChatWebViewActivity activity)
            {
                _activity = activity;
            }

            public override void OnPageStarted(WebView view, string url, Bitmap favicon)
            {
                base.OnPageStarted(view, url, favicon);
                _activity._progressBar?.SetVisibility(ViewStates.Visible);
            }

            public override void OnPageFinished(WebView view, string url)
            {
                base.OnPageFinished(view, url);
                _activity._progressBar?.SetVisibility(ViewStates.Gone);
            }

            public override bool ShouldOverrideUrlLoading(WebView view, IWebResourceRequest request)
            {
                var url = request.Url.ToString();
                if (url.Contains("studiosnt.sntwork.com"))
                    return false;
                
                try
                {
                    var intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(url));
                    view.Context.StartActivity(intent);
                }
                catch { }
                return true;
            }
        }

        private class ChatWebChromeClient : WebChromeClient
        {
            private readonly ChatWebViewActivity _activity;

            public ChatWebChromeClient(ChatWebViewActivity activity)
            {
                _activity = activity;
            }

            public override void OnProgressChanged(WebView view, int newProgress)
            {
                base.OnProgressChanged(view, newProgress);
                _activity._progressBar?.SetProgress(newProgress);
            }
        }
    }
}
