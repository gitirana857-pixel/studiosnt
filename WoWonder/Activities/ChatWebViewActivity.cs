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
        private bool _autoLoginAttempted;

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

            // Start at the welcome page to auto-login
            string siteUrl = GetSiteUrl();
            _webView.LoadUrl(siteUrl + "/welcome");
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

        private void TryAutoLogin()
        {
            if (_autoLoginAttempted) return;
            _autoLoginAttempted = true;

            string email = UserDetails.Email;
            string password = UserDetails.Password;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                // No credentials available, just go to messages
                string siteUrl = GetSiteUrl();
                _webView.LoadUrl(siteUrl + "/messages");
                return;
            }

            // Escape special characters for JavaScript string
            email = email.Replace("\\", "\\\\").Replace("'", "\\'");
            password = password.Replace("\\", "\\\\").Replace("'", "\\'");

            string js = "javascript:(function(){" +
                "var u=document.getElementById('username');" +
                "var p=document.getElementById('password');" +
                "if(u&&p){" +
                    "u.value='" + email + "';" +
                    "p.value='" + password + "';" +
                    "var btn=document.querySelector('button[type=submit]')||document.querySelector('input[type=submit]')||document.getElementById('sign-in-button');" +
                    "if(btn)btn.click();" +
                    "else{" +
                        "var f=u.closest('form');" +
                        "if(f)f.submit();" +
                    "}" +
                "}" +
            "})()";

            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Kitkat)
            {
                _webView.EvaluateJavascript(js, null);
            }
            else
            {
                _webView.LoadUrl(js);
            }
        }

        public override void OnBackPressed()
        {
            if (_webView.CanGoBack())
            {
                _webView.GoBack();
            }
            else
            {
                Finish();
            }
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
                _activity._progressBar.Visibility = ViewStates.Visible;
            }

            public override void OnPageFinished(WebView view, string url)
            {
                base.OnPageFinished(view, url);
                _activity._progressBar.Visibility = ViewStates.Gone;

                // If we're on the welcome page and haven't tried auto-login yet
                if (url.Contains("/welcome") && !_activity._autoLoginAttempted)
                {
                    _activity.TryAutoLogin();
                }

                // If login succeeded (we're no longer on welcome page), navigate to messages
                if (!url.Contains("/welcome") && !url.Contains("/messages") && _activity._autoLoginAttempted)
                {
                    string siteUrl = _activity.GetSiteUrl();
                    view.LoadUrl(siteUrl + "/messages");
                }
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
                _activity._progressBar.Progress = newProgress;
            }
        }
    }
}
