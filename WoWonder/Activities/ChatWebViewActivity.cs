using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using AndroidX.AppCompat.App;
using WoWonder.Helpers.Utils;

namespace WoWonder.Activities
{
    [Activity(Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.KeyboardHidden)]
    public class ChatWebViewActivity : AppCompatActivity
    {
        private WebView _webView;
        private ImageView _backButton;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetContentView(Resource.Layout.ChatWebViewLayout);

                _webView = FindViewById<WebView>(Resource.Id.ChatWebView);
                _backButton = FindViewById<ImageView>(Resource.Id.toolbar_back);

                _backButton.Click += (sender, args) => Finish();

                SetupWebView();
            }
            catch (System.Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetupWebView()
        {
            if (_webView == null) return;

            _webView.Settings.JavaScriptEnabled = true;
            _webView.Settings.DomStorageEnabled = true;
            _webView.Settings.AllowFileAccess = true;
            _webView.Settings.DefaultTextEncodingName = "utf-8";
            _webView.Settings.UseWideViewPort = true;
            _webView.Settings.LoadWithOverviewMode = true;
            _webView.Settings.BuiltInZoomControls = false;
            _webView.Settings.DisplayZoomControls = false;

            _webView.SetWebViewClient(new ChatWebViewClient());

            string siteUrl = Resources.GetString(Resource.String.ApplicationUrlWeb);
            if (string.IsNullOrEmpty(siteUrl))
                siteUrl = "studiosnt.sntwork.com";

            if (!siteUrl.StartsWith("http"))
                siteUrl = "https://www." + siteUrl;

            siteUrl = siteUrl.TrimEnd('/');
            _webView.LoadUrl(siteUrl + "/messages");
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
            public override void OnPageStarted(WebView view, string url, Bitmap favicon)
            {
                base.OnPageStarted(view, url, favicon);
            }

            public override void OnPageFinished(WebView view, string url)
            {
                base.OnPageFinished(view, url);
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
    }
}
