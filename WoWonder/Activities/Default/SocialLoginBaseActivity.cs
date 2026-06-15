using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Common.Util.Concurrent;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.Core.Content;
using AndroidX.Credentials;
using Com.Facebook;
using Com.Facebook.Login;
using Com.Facebook.Login.Widget;
using Java.Util.Concurrent;
using Org.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WoWonder.Activities.Base;
using WoWonder.Activities.MyProfile;
using WoWonder.Activities.Tabbes;
using WoWonder.Activities.Upgrade;
using WoWonder.Activities.WalkTroutPage;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.SocialLogins;
using WoWonder.Helpers.Utils;
using WoWonder.Library.OneSignalNotif;
using WoWonder.SQLite;
using WoWonderClient;
using WoWonderClient.Classes.Auth;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;
using Xamarin.GoogleAndroid.Libraries.Identity.GoogleId;
using AccessToken = Com.Facebook.AccessToken;
using Credential = AndroidX.Credentials.Credential;
using Exception = System.Exception;
using GetCredentialRequest = AndroidX.Credentials.GetCredentialRequest;
using GetCredentialResponse = AndroidX.Credentials.GetCredentialResponse;
using Object = Java.Lang.Object;
using Task = System.Threading.Tasks.Task;

namespace WoWonder.Activities.Default
{
    public abstract class SocialLoginBaseActivity : BaseActivity, IFacebookCallback, GraphRequest.IGraphJSONObjectCallback, ICredentialManagerCallback
    {
        #region Variables Basic

        private ICallbackManager MFbCallManager;
        private FbMyProfileTracker ProfileTracker;
        public LinearLayout FbLoginButton;
        public LinearLayout GoogleSignInButton;
        public static ICredentialManager CredentialManager;
        public static SocialLoginBaseActivity Instance;

        private string TimeZone = "";
        private bool IsActiveUser = true;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                InitializeWoWonder.Initialize(AppSettings.TripleDesAppServiceProvider, PackageName, AppSettings.TurnTrustFailureOnWebException, MyReportModeApp.CreateInstance());

                //Set Full screen 
                SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);
                Window?.SetSoftInputMode(SoftInput.AdjustResize);

                Methods.App.FullScreenApp(this);

                Instance = this;

                if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
                {
                    if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.PostNotifications) == Permission.Granted)
                    {
                        if (string.IsNullOrEmpty(UserDetails.DeviceId))
                            OneSignalNotification.Instance.RegisterNotificationDevice(this);
                    }
                    else
                    {
                        RequestPermissions(new[]
                        {
                            Manifest.Permission.PostNotifications
                        }, 16248);
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(UserDetails.DeviceId))
                        OneSignalNotification.Instance.RegisterNotificationDevice(this);
                }

                if (Methods.CheckConnectivity())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.GetSettings_Api(this) });

                GetTimezone();
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


        #endregion

        #region Events

        private void BtnFacebookOnClick(object sender, EventArgs e)
        {
            try
            {
                HideKeyboard();
                LoginManager.Instance.LogInWithReadPermissions(this, new List<string>
                {
                    "email",
                    "public_profile"
                });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        private void ProfileTrackerOnMOnProfileChanged(object sender, ProfileChangedEventArgs e)
        {
            try
            {
                HideKeyboard();
                if (e.MProfile != null)
                {
                    //FbFirstName = e.MProfile.FirstName;
                    //FbLastName = e.MProfile.LastName;
                    //FbName = e.MProfile.Name;
                    //FbProfileId = e.MProfile.Id;

                    var request = GraphRequest.NewMeRequest(AccessToken.CurrentAccessToken, this);
                    var parameters = new Bundle();
                    parameters.PutString("fields", "id,name,age_range,email");
                    request.Parameters = parameters;
                    request.ExecuteAndWait();
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        #endregion

        #region Functions

        public void InitSocialLogins()
        {
            try
            {
                //#Facebook
                if (AppSettings.ShowFacebookLogin)
                {
                    //FacebookSdk.SdkInitialize(this);
                    LoginButton loginButton = new LoginButton(this);
                    ProfileTracker = new FbMyProfileTracker();
                    ProfileTracker.StartTracking();

                    FbLoginButton = FindViewById<LinearLayout>(Resource.Id.bntLoginFacebook);
                    FbLoginButton.Visibility = ViewStates.Visible;
                    FbLoginButton.Click += BtnFacebookOnClick;

                    ProfileTracker.MOnProfileChanged += ProfileTrackerOnMOnProfileChanged;
                    loginButton.SetPermissions("email", "public_profile");

                    MFbCallManager = ICallbackManager.Factory.Create();
                    LoginManager.Instance.RegisterCallback(MFbCallManager, this);

                    //FB accessToken
                    var accessToken = AccessToken.CurrentAccessToken;
                    var isLoggedIn = accessToken != null && !accessToken.IsExpired;
                    if (isLoggedIn && Profile.CurrentProfile != null)
                    {
                        LoginManager.Instance.LogOut();
                    }

                    string hash = Methods.App.GetKeyHashesConfigured(this);
                    Console.WriteLine(hash);
                }
                else
                {
                    FbLoginButton = FindViewById<LinearLayout>(Resource.Id.bntLoginFacebook);
                    FbLoginButton.Visibility = ViewStates.Gone;
                }

                //#Google
                if (AppSettings.ShowGoogleLogin)
                {
                    GoogleSignInButton = FindViewById<LinearLayout>(Resource.Id.bntLoginGoogle);
                    GoogleSignInButton.Click += GoogleSignInButtonOnClick;
                }
                else
                {
                    GoogleSignInButton = FindViewById<LinearLayout>(Resource.Id.bntLoginGoogle);
                    GoogleSignInButton.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetDataLogin(AuthObject auth, string email, string password, bool chkRemember = true)
        {
            try
            {
                Current.AccessToken = auth.AccessToken;

                UserDetails.Username = email;
                UserDetails.FullName = email;
                UserDetails.Password = password;
                UserDetails.AccessToken = auth.AccessToken;
                UserDetails.UserId = auth.UserId;
                UserDetails.Status = "Pending";
                UserDetails.Cookie = auth.AccessToken;
                UserDetails.Email = email;

                //Insert user data to database
                var user = new DataTables.LoginTb
                {
                    UserId = UserDetails.UserId,
                    AccessToken = UserDetails.AccessToken,
                    Cookie = UserDetails.Cookie,
                    Username = UserDetails.Email,
                    Password = UserDetails.Password,
                    Status = "Pending",
                    Lang = "",
                    Email = UserDetails.Email,
                };

                ListUtils.DataUserLoginList.Clear();
                ListUtils.DataUserLoginList.Add(user);

                if (chkRemember)
                {
                    var dbDatabase = new SqLiteDatabase();
                    dbDatabase.InsertOrUpdateLogin_Credentials(user);
                }

                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.Get_MyProfileData_Api(this) });

                if (auth.IsNew != null && auth.IsNew.Value)
                {
                    if (AppSettings.ShowWalkTroutPage)
                    {
                        Intent newIntent = new Intent(this, typeof(WalkTroutActivity));
                        newIntent?.PutExtra("class", "register");
                        StartActivity(newIntent);
                    }
                    else
                    {
                        if (ListUtils.SettingsSiteList?.MembershipSystem == "1")
                        {
                            var intent = new Intent(this, typeof(GoProActivity));
                            intent.PutExtra("class", "register");
                            StartActivity(intent);
                        }
                        else
                        {
                            if (AppSettings.AddAllInfoPorfileAfterRegister)
                            {
                                Intent newIntent = new Intent(this, typeof(AddAllInfoProfileActivity));
                                StartActivity(newIntent);
                            }
                            else
                            {
                                StartActivity(new Intent(this, typeof(TabbedMainActivity)));
                            }
                        }
                    }
                }
                else
                {
                    if (auth.Membership != null && auth.Membership.Value)
                    {
                        var intent = new Intent(this, typeof(GoProActivity));
                        intent.PutExtra("class", "login");
                        StartActivity(intent);
                    }
                    else
                    {
                        if (AppSettings.ShowWalkTroutPage)
                        {
                            Intent newIntent = new Intent(this, typeof(WalkTroutActivity));
                            newIntent?.PutExtra("class", "login");
                            StartActivity(newIntent);
                        }
                        else
                        {
                            StartActivity(new Intent(this, typeof(TabbedMainActivity)));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetDataLogin(CreatAccountObject auth, string Username, string email, string password)
        {
            try
            {
                Current.AccessToken = auth.AccessToken;

                UserDetails.Username = Username;
                UserDetails.FullName = Username;
                UserDetails.Password = password;
                UserDetails.AccessToken = auth.AccessToken;
                UserDetails.UserId = auth.UserId;
                UserDetails.Status = "Pending";
                UserDetails.Cookie = auth.AccessToken;
                UserDetails.Email = email;

                //Insert user data to database
                var user = new DataTables.LoginTb
                {
                    UserId = UserDetails.UserId,
                    AccessToken = UserDetails.AccessToken,
                    Cookie = UserDetails.Cookie,
                    Username = UserDetails.Username,
                    Password = UserDetails.Password,
                    Status = "Pending",
                    Lang = "",
                    Email = UserDetails.Email,
                };

                ListUtils.DataUserLoginList.Clear();
                ListUtils.DataUserLoginList.Add(user);

                var dbDatabase = new SqLiteDatabase();
                dbDatabase.InsertOrUpdateLogin_Credentials(user);

                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.Get_MyProfileData_Api(this) });

                if (AppSettings.ShowWalkTroutPage)
                {
                    Intent newIntent = new Intent(this, typeof(WalkTroutActivity));
                    newIntent?.PutExtra("class", "register");
                    StartActivity(newIntent);
                }
                else
                {
                    if (ListUtils.SettingsSiteList?.MembershipSystem == "1")
                    {
                        var intent = new Intent(this, typeof(GoProActivity));
                        intent.PutExtra("class", "register");
                        StartActivity(intent);
                    }
                    else
                    {
                        if (AppSettings.AddAllInfoPorfileAfterRegister)
                        {
                            Intent newIntent = new Intent(this, typeof(AddAllInfoProfileActivity));
                            StartActivity(newIntent);
                        }
                        else
                        {
                            StartActivity(new Intent(this, typeof(TabbedMainActivity)));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Abstract members
        public abstract void ToggleVisibility(bool isLoginProgress);

        #endregion

        #region Social Logins

        private string FbAccessToken;

        #region Facebook

        public void OnCancel()
        {
            try
            {
                ToggleVisibility(false);

                //SetResult(Result.Canceled);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnError(FacebookException error)
        {
            try
            {

                ToggleVisibility(false);

                // Handle exception
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), error.Message, GetText(Resource.String.Lbl_Ok));

                //SetResult(Result.Canceled);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        void IFacebookCallback.OnSuccess(Object result)
        {
            try
            {
                //var loginResult = result as LoginResult;
                //var id = AccessToken.CurrentAccessToken.UserId;

                ToggleVisibility(false);

                //SetResult(Result.Ok);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public async void OnCompleted(JSONObject json, GraphResponse response)
        {
            try
            {
                ToggleVisibility(true);

                var accessToken = AccessToken.CurrentAccessToken;
                if (accessToken != null)
                {
                    FbAccessToken = accessToken.Token;

                    //Login Api 
                    var (apiStatus, respond) = await RequestsAsync.Auth.SocialLoginAsync(FbAccessToken, "facebook", UserDetails.DeviceId);
                    if (apiStatus == 200)
                    {
                        if (respond is AuthObject auth)
                        {
                            //if (!string.IsNullOrEmpty(json?.ToString()))
                            //{
                            //    var data = json.ToString();
                            //    var result = JsonConvert.DeserializeObject<FacebookResult>(data);
                            //    //FbEmail = result.Email;
                            //}

                            SetDataLogin(auth, "", "");

                            ToggleVisibility(false);
                            Finish();
                        }
                    }
                    else if (apiStatus == 400)
                    {
                        if (respond is ErrorObject error)
                        {
                            ToggleVisibility(false);
                            var errorText = error.Error.ErrorText;
                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                        }
                    }
                    else
                    {
                        ToggleVisibility(false);
                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond.ToString(), GetText(Resource.String.Lbl_Ok));
                    }
                }
                else
                {
                    ToggleVisibility(false);
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Please_enter_your_data), GetText(Resource.String.Lbl_Ok));
                }
            }
            catch (Exception exception)
            {
                ToggleVisibility(false);
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), exception.Message, GetText(Resource.String.Lbl_Ok));
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        //======================================================

        #region Google

        //Event Click login using google
        private void GoogleSignInButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                GetGoogleIdOption googleIdOption = new GetGoogleIdOption.Builder()
                    .SetFilterByAuthorizedAccounts(false)
                    .SetServerClientId(AppSettings.ClientId)
                    //.SetAutoSelectEnabled(false) 
                    .Build();

                GetCredentialRequest request = new GetCredentialRequest.Builder()
                    .AddCredentialOption(googleIdOption)
                    .Build();

                CancellationSignal cancellationSignal = new CancellationSignal();
                CredentialManager ??= ICredentialManager.Create(this);
                IExecutor executor = ContextCompat.GetMainExecutor(this);

                CredentialManager.GetCredentialAsync(this, request, cancellationSignal, executor, this);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void SetContentGoogle(string gAccessToken)
        {
            try
            {
                //Successful log in hooray!!
                if (!string.IsNullOrEmpty(gAccessToken))
                {
                    ToggleVisibility(true);

                    var (apiStatus, respond) = await RequestsAsync.Auth.SocialLoginAsync(gAccessToken, "google", UserDetails.DeviceId);
                    if (apiStatus == 200)
                    {
                        if (respond is AuthObject auth)
                        {
                            SetDataLogin(auth, "", "");

                            ToggleVisibility(false);
                            Finish();
                        }
                    }
                    else if (apiStatus == 400)
                    {
                        if (respond is ErrorObject error)
                        {
                            ToggleVisibility(false);
                            var errorText = error.Error.ErrorText;
                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                        }
                    }
                    else
                    {
                        ToggleVisibility(false);
                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond.ToString(), GetText(Resource.String.Lbl_Ok));
                    }
                }
                else
                {
                    ToggleVisibility(false);
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Please_enter_your_data), GetText(Resource.String.Lbl_Ok));
                }
            }
            catch (Exception exception)
            {
                ToggleVisibility(false);
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), exception.Message, GetText(Resource.String.Lbl_Ok));
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnError(Object result)
        {
            try
            {
                ToastUtils.ShowToast(this, result?.ToString(), ToastLength.Short);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public async void OnResult(Object result)
        {
            try
            {
                if (result is GetCredentialResponse response)
                {
                    Credential credential = response.Credential;
                    if (credential is CustomCredential customCredential)
                    {
                        if (customCredential.Type == GoogleIdTokenCredential.TypeGoogleIdTokenCredential)
                        {
                            GoogleIdTokenCredential googleIdTokenCredential = GoogleIdTokenCredential.CreateFrom(credential.Data);

                            if (googleIdTokenCredential != null)
                            {
                                string email = googleIdTokenCredential.Id;
                                string firstName = googleIdTokenCredential.GivenName;
                                string lastName = googleIdTokenCredential.FamilyName;
                                string token = googleIdTokenCredential.IdToken;
                                SetContentGoogle(token);
                            }
                        }
                    }
                    else if (credential is PasswordCredential passwordCredential)
                    {
                        HideKeyboard();

                        ToggleVisibility(true);
                        await AuthApi(passwordCredential.Id, passwordCredential.Password);
                    }
                }
                else if (result is CreatePublicKeyCredentialResponse credentialResponse)
                {

                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #endregion

        #region Result & Permissions

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                // Logins Facebook
                MFbCallManager?.OnActivityResult(requestCode, (int)resultCode, data);
                base.OnActivityResult(requestCode, resultCode, data);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
                switch (requestCode)
                {
                    case 16248 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        if (string.IsNullOrEmpty(UserDetails.DeviceId))
                            OneSignalNotification.Instance.RegisterNotificationDevice(this);
                        break;
                    case 16248:
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Cross App Authentication

        public void BuildClients()
        {
            try
            {
                GetPasswordOption getPasswordOption = new GetPasswordOption();

                GetCredentialRequest getCredRequest = new GetCredentialRequest.Builder()
                    .AddCredentialOption(getPasswordOption)
                    .Build();

                CredentialManager ??= ICredentialManager.Create(this);

                CredentialManager.GetCredentialAsync(this, getCredRequest, new CancellationSignal(), new HandlerExecutor(Looper.MainLooper), this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        public void HideKeyboard()
        {
            try
            {
                var inputManager = (InputMethodManager)GetSystemService(InputMethodService);
                inputManager?.HideSoftInputFromWindow(CurrentFocus?.WindowToken, HideSoftInputFlags.None);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public async Task AuthApi(string email, string password, bool chkRemember = true)
        {
            try
            {
                var (apiStatus, respond) = await RequestsAsync.Auth.AuthAsync(email, password, TimeZone, UserDetails.DeviceId);
                if (apiStatus == 200)
                {
                    if (respond is AuthObject auth)
                    {
                        var emailValidation = ListUtils.SettingsSiteList?.EmailValidation ?? "0";
                        if (emailValidation == "1")
                            IsActiveUser = await CheckIsActiveUser(auth.UserId);

                        if (IsActiveUser)
                        {
                            if (AppSettings.EnableSmartLockForPasswords)
                                CredentialManager?.CreateCredentialAsync(this, new CreatePasswordRequest(email, password), new CancellationSignal(), ContextCompat.GetMainExecutor(this), this);

                            SetDataLogin(auth, email, password, chkRemember);

                            ToggleVisibility(false);
                            FinishAffinity();
                        }
                        else
                        {
                            ToggleVisibility(false);
                        }
                    }
                    else if (respond is AuthMessageObject messageObject)
                    {
                        ToggleVisibility(false);

                        UserDetails.Username = email;
                        UserDetails.FullName = email;
                        UserDetails.Password = password;
                        UserDetails.UserId = messageObject.UserId;
                        UserDetails.Status = "Pending";
                        UserDetails.Email = email;

                        //Insert user data to database
                        var user = new DataTables.LoginTb
                        {
                            UserId = UserDetails.UserId,
                            AccessToken = "",
                            Cookie = "",
                            Username = email,
                            Password = password,
                            Status = "Pending",
                            Lang = "",
                        };
                        ListUtils.DataUserLoginList.Add(user);

                        var dbDatabase = new SqLiteDatabase();
                        dbDatabase.InsertOrUpdateLogin_Credentials(user);

                        Intent newIntent = new Intent(this, typeof(VerificationCodeActivity));
                        newIntent?.PutExtra("TypeCode", "TwoFactor");
                        StartActivity(newIntent);
                    }
                }
                else if (apiStatus == 400)
                {
                    if (respond is ErrorObject error)
                    {
                        ToggleVisibility(false);

                        var errorText = error.Error.ErrorText;
                        var errorId = error.Error.ErrorId;
                        if (errorId == "3")
                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorLogin_3), GetText(Resource.String.Lbl_Ok));
                        else if (errorId == "4")
                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorLogin_4), GetText(Resource.String.Lbl_Ok));
                        else if (errorId == "5")
                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorLogin_5), GetText(Resource.String.Lbl_Ok));
                        else
                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                    }
                }
                else
                {
                    ToggleVisibility(false);
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond.ToString(), GetText(Resource.String.Lbl_Ok));
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void GetTimezone()
        {
            try
            {
                if (Methods.CheckConnectivity())
                    TimeZone = await ApiRequest.GetTimeZoneAsync().ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async Task<bool> CheckIsActiveUser(string userId)
        {
            try
            {
                var (apiStatus, respond) = await RequestsAsync.Auth.IsActiveUserAsync(userId);
                if (apiStatus == 200 && respond is MessageObject auth)
                {
                    Console.WriteLine(auth);
                    return true;
                }

                if (apiStatus == 400)
                {
                    if (respond is ErrorObject error)
                    {
                        var errorText = error.Error.ErrorText;
                        var errorId = error.Error.ErrorId;
                        if (errorId == "5")
                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ThisUserNotActive), GetText(Resource.String.Lbl_Ok));
                        else if (errorId == "4")
                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_UserNotFound), GetText(Resource.String.Lbl_Ok));
                        else
                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                    }
                }
                else if (apiStatus == 404)
                {
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond.ToString(), GetText(Resource.String.Lbl_Ok));
                }

                return false;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return false;
            }
        }
    }
}