using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using System;
using WoWonder.Helpers.Utils;

namespace WoWonder.Activities.Default
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class LoginActivity : SocialLoginBaseActivity
    {
        #region Variables Basic

        private EditText TxtEmail, TxtPassword;
        private TextView TxtForgotPassword;
        private AppCompatButton BtnLogin;
        private ImageView ImageShowPass;
        private ProgressBar ProgressBar;
        private TextView LayoutCreateAccount;

        private CheckBox ChkRemember;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                // Create your application here
                SetContentView(Resource.Layout.LoginLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitSocialLogins();

                if (AppSettings.EnableSmartLockForPasswords)
                    BuildClients();
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
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        protected override void OnDestroy()
        {
            try
            {
                DestroyBasic();
                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                TxtEmail = FindViewById<EditText>(Resource.Id.EmailEditText);
                TxtPassword = FindViewById<EditText>(Resource.Id.PasswordEditText);

                ImageShowPass = FindViewById<ImageView>(Resource.Id.imageShowPass);
                ImageShowPass.Tag = "hide";

                ChkRemember = FindViewById<CheckBox>(Resource.Id.checkRememberMe);
                ChkRemember.Checked = true;

                TxtForgotPassword = FindViewById<TextView>(Resource.Id.textForgotPassword);

                ProgressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
                BtnLogin = FindViewById<AppCompatButton>(Resource.Id.btnLogin);

                LayoutCreateAccount = FindViewById<TextView>(Resource.Id.layout_create_account);
                LayoutCreateAccount.Visibility = AppSettings.EnableRegisterSystem == false ? ViewStates.Gone : ViewStates.Visible;
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
                    BtnLogin.Click += BtnLoginOnClick;
                    TxtForgotPassword.Click += TxtForgotPasswordOnClick;
                    ImageShowPass.Click += ImageShowPassOnClick;
                    LayoutCreateAccount.Click += LayoutCreateAccountOnClick;
                }
                else
                {
                    BtnLogin.Click -= BtnLoginOnClick;
                    TxtForgotPassword.Click -= TxtForgotPasswordOnClick;
                    ImageShowPass.Click -= ImageShowPassOnClick;
                    LayoutCreateAccount.Click -= LayoutCreateAccountOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void DestroyBasic()
        {
            try
            {
                TxtEmail = null!;
                TxtPassword = null!;
                TxtForgotPassword = null!;
                BtnLogin = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        //Show Password 
        private void ImageShowPassOnClick(object sender, EventArgs e)
        {
            try
            {
                if (ImageShowPass.Tag?.ToString() == "hide")
                {
                    ImageShowPass.SetImageResource(Resource.Drawable.icon_eyes_vector);
                    ImageShowPass.Tag = "show";
                    TxtPassword.InputType = InputTypes.TextVariationNormal | InputTypes.ClassText;
                    TxtPassword.SetSelection(TxtPassword.Text.Length);
                }
                else
                {
                    ImageShowPass.SetImageResource(Resource.Drawable.ic_eye_hide);
                    ImageShowPass.Tag = "hide";
                    TxtPassword.InputType = InputTypes.TextVariationPassword | InputTypes.ClassText;
                    TxtPassword.SetSelection(TxtPassword.Text.Length);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Forgot Password
        private void TxtForgotPasswordOnClick(object sender, EventArgs e)
        {
            try
            {
                HideKeyboard();
                StartActivity(new Intent(this, typeof(ForgetPasswordActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //start login 
        private async void BtnLoginOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_CheckYourInternetConnection), GetText(Resource.String.Lbl_Ok));
                    return;
                }

                if (string.IsNullOrEmpty(TxtEmail.Text.Replace(" ", "")) || string.IsNullOrEmpty(TxtPassword.Text))
                {
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Please_enter_your_data), GetText(Resource.String.Lbl_Ok));
                    return;
                }

                HideKeyboard();

                ToggleVisibility(true);
                await AuthApi(TxtEmail.Text.Replace(" ", ""), TxtPassword.Text, ChkRemember.Checked);
            }
            catch (Exception exception)
            {
                ToggleVisibility(false);
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), exception.Message, GetText(Resource.String.Lbl_Ok));
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //CreateAccount
        private void LayoutCreateAccountOnClick(object sender, EventArgs e)
        {
            try
            {
                HideKeyboard();
                StartActivity(new Intent(this, typeof(RegisterActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        public override void ToggleVisibility(bool isLoginProgress)
        {
            try
            {
                ProgressBar.Visibility = isLoginProgress ? ViewStates.Visible : ViewStates.Gone;
                BtnLogin.Visibility = isLoginProgress ? ViewStates.Invisible : ViewStates.Visible;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}