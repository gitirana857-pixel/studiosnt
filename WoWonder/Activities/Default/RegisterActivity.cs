using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using Google.Android.Material.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.SQLite;
using WoWonderClient;
using WoWonderClient.Classes.Auth;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;
using Exception = System.Exception;

namespace WoWonder.Activities.Default
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class RegisterActivity : SocialLoginBaseActivity, IDialogListCallBack
    {
        #region Variables Basic

        private EditText TxtUsername, TxtFirstName, TxtLastName, TxtEmail, TxtGender, TxtBirthday, TxtPhoneNum, TxtPassword, TxtConfirmPassword;
        private LinearLayout PhoneLayout, BirthdayLayout;
        private CheckBox ChkTermsOfUse;
        private TextView TxtTermsOfService, LayoutLogin;
        private AppCompatButton BtnSignUp;
        private ProgressBar ProgressBar;
        private ImageView ImageShowPass, ImageShowConPass;
        private string GenderStatus = "male", Referral;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                // Create your application here
                SetContentView(Resource.Layout.RegisterLayout);

                Referral = Intent?.GetStringExtra("Referral") ?? "";

                //Get Value  
                InitComponent();
                InitSocialLogins();
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
                TxtUsername = FindViewById<EditText>(Resource.Id.UsernameEditText);

                TxtFirstName = FindViewById<EditText>(Resource.Id.FirstNameEditText);
                TxtLastName = FindViewById<EditText>(Resource.Id.LastNameEditText);

                TxtEmail = FindViewById<EditText>(Resource.Id.EmailEditText);
                TxtPassword = FindViewById<EditText>(Resource.Id.PasswordEditText);
                TxtConfirmPassword = FindViewById<EditText>(Resource.Id.ConfirmPasswordEditText);
                TxtGender = FindViewById<EditText>(Resource.Id.GenderEditText);

                BirthdayLayout = FindViewById<LinearLayout>(Resource.Id.BirthdayLayout);
                TxtBirthday = FindViewById<EditText>(Resource.Id.BirthdayEditText);

                PhoneLayout = FindViewById<LinearLayout>(Resource.Id.PhoneNumLayout);
                TxtPhoneNum = FindViewById<EditText>(Resource.Id.PhoneNumEditText);

                ImageShowPass = FindViewById<ImageView>(Resource.Id.imageShowPass);
                ImageShowPass.Tag = "hide";

                ImageShowConPass = FindViewById<ImageView>(Resource.Id.imageShowConPass);
                ImageShowConPass.Tag = "hide";

                ChkTermsOfUse = FindViewById<CheckBox>(Resource.Id.checkTermsOfService);
                TxtTermsOfService = FindViewById<TextView>(Resource.Id.terms_of_service);

                ProgressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
                BtnSignUp = FindViewById<AppCompatButton>(Resource.Id.btnRegister);

                LayoutLogin = FindViewById<TextView>(Resource.Id.layout_already_account);

                ToggleVisibility(false);
                Methods.SetFocusable(TxtGender);
                Methods.SetFocusable(TxtBirthday);

                if (!AppSettings.ShowBirthdayInRegister)
                {
                    BirthdayLayout.Visibility = ViewStates.Gone;
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
                    BtnSignUp.Click += BtnSignUpOnClick;
                    ImageShowPass.Click += ImageShowPassOnClick;
                    ImageShowConPass.Click += ImageShowConPassOnClick;
                    TxtBirthday.Touch += TxtBirthdayOnTouch;
                    TxtGender.Touch += TxtGenderOnTouch;
                    TxtTermsOfService.Click += TxtTermsOfServiceOnClick;
                    LayoutLogin.Click += LayoutLoginOnClick;

                }
                else
                {
                    BtnSignUp.Click -= BtnSignUpOnClick;
                    ImageShowPass.Click -= ImageShowPassOnClick;
                    ImageShowConPass.Click -= ImageShowConPassOnClick;
                    TxtBirthday.Touch -= TxtBirthdayOnTouch;
                    TxtGender.Touch -= TxtGenderOnTouch;
                    TxtTermsOfService.Click -= TxtTermsOfServiceOnClick;
                    LayoutLogin.Click -= LayoutLoginOnClick;

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
                TxtUsername = null!;
                TxtEmail = null!;
                TxtGender = null!;
                TxtPassword = null!;
                TxtConfirmPassword = null!;
                ChkTermsOfUse = null!;
                TxtTermsOfService = null!;
                BtnSignUp = null!;
                GenderStatus = "male";
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void LayoutLoginOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(LoginActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Show Con Password 
        private void ImageShowConPassOnClick(object sender, EventArgs e)
        {
            try
            {
                if (ImageShowConPass.Tag?.ToString() == "hide")
                {
                    ImageShowConPass.SetImageResource(Resource.Drawable.icon_eyes_vector);
                    ImageShowConPass.Tag = "show";
                    TxtConfirmPassword.InputType = InputTypes.TextVariationNormal | InputTypes.ClassText;
                    TxtConfirmPassword.SetSelection(TxtConfirmPassword.Text.Length);
                }
                else
                {
                    ImageShowConPass.SetImageResource(Resource.Drawable.ic_eye_hide);
                    ImageShowConPass.Tag = "hide";
                    TxtConfirmPassword.InputType = InputTypes.TextVariationPassword | InputTypes.ClassText;
                    TxtConfirmPassword.SetSelection(TxtConfirmPassword.Text.Length);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

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

        //start Create account 
        private async void BtnSignUpOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_CheckYourInternetConnection), GetText(Resource.String.Lbl_Ok));
                    return;
                }

                if (!ChkTermsOfUse.Checked)
                {
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_You_can_not_access_your_disapproval), GetText(Resource.String.Lbl_Ok));
                    return;
                }

                if (string.IsNullOrEmpty(TxtUsername.Text.Replace(" ", "")) || string.IsNullOrEmpty(TxtFirstName.Text.Replace(" ", "")) || string.IsNullOrEmpty(TxtLastName.Text.Replace(" ", "")) || string.IsNullOrEmpty(TxtEmail.Text.Replace(" ", "")) ||
                  string.IsNullOrEmpty(TxtGender.Text) || string.IsNullOrEmpty(TxtPassword.Text) || string.IsNullOrEmpty(TxtConfirmPassword.Text))
                {
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Please_enter_your_data), GetText(Resource.String.Lbl_Ok));
                    return;
                }

                if (AppSettings.ShowBirthdayInRegister && string.IsNullOrEmpty(TxtBirthday.Text))
                {
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Please_enter_your_data), GetText(Resource.String.Lbl_Ok));
                    return;
                }

                var smsOrEmail = ListUtils.SettingsSiteList?.SmsOrEmail;
                if (smsOrEmail == "sms" && string.IsNullOrEmpty(TxtPhoneNum.Text))
                {
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister_4), GetText(Resource.String.Lbl_Ok));
                    return;
                }

                var check = Methods.FunString.IsEmailValid(TxtEmail.Text.Replace(" ", ""));
                if (!check)
                {
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_IsEmailValid), GetText(Resource.String.Lbl_Ok));
                    return;
                }

                if (TxtPassword.Text != TxtConfirmPassword.Text)
                {
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Your_password_dont_match), GetText(Resource.String.Lbl_Ok));
                    return;
                }

                HideKeyboard();

                ToggleVisibility(true);

                var (apiStatus, respond) = await RequestsAsync.Auth.CreateAccountAsync(TxtUsername.Text.Replace(" ", ""), TxtFirstName.Text.Replace(" ", ""), TxtLastName.Text.Replace(" ", ""), TxtPassword.Text, TxtConfirmPassword.Text, TxtEmail.Text.Replace(" ", ""), GenderStatus, TxtPhoneNum.Text, Referral, UserDetails.DeviceId);
                if (apiStatus == 200 && respond is CreatAccountObject result)
                {
                    var dataPrivacy = new Dictionary<string, string> { { "first_name", TxtFirstName.Text.Replace(" ", "") }, { "last_name", TxtLastName.Text.Replace(" ", "") } };

                    if (AppSettings.ShowBirthdayInRegister)
                        dataPrivacy.Add("birthday", TxtBirthday.Text);

                    var autoUsername = ListUtils.SettingsSiteList?.AutoUsername;
                    if (autoUsername == "1")
                        dataPrivacy.Add("username", TxtUsername.Text.Replace(" ", ""));

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dataPrivacy) });
                    SetDataLogin(result, TxtUsername.Text.Replace(" ", ""), TxtPassword.Text, TxtEmail.Text.Replace(" ", ""));

                    ToggleVisibility(false);
                    FinishAffinity();
                }
                else if (apiStatus == 220)
                {
                    if (respond is AuthMessageObject message)
                    {
                        if (smsOrEmail == "sms")
                        {
                            UserDetails.Username = TxtUsername.Text;
                            UserDetails.FullName = TxtFirstName.Text + " " + TxtLastName.Text;
                            UserDetails.Password = TxtPassword.Text;
                            UserDetails.UserId = message.UserId;
                            UserDetails.Status = "Pending";
                            UserDetails.Email = TxtEmail.Text;

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

                            Intent newIntent = new Intent(this, typeof(VerificationCodeActivity));
                            newIntent?.PutExtra("TypeCode", "AccountSms");
                            StartActivity(newIntent);
                        }
                        else if (smsOrEmail == "mail")
                        {
                            var dialog = new MaterialAlertDialogBuilder(this);
                            dialog.SetTitle(GetText(Resource.String.Lbl_ActivationSent));
                            dialog.SetMessage(GetText(Resource.String.Lbl_ActivationDetails).Replace("@", TxtEmail.Text));
                            dialog.SetPositiveButton(GetText(Resource.String.Lbl_Ok), new MaterialDialogUtils());

                            dialog.Show();
                        }
                        else
                        {
                            ProgressBar.Visibility = ViewStates.Invisible;
                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), message.Message, GetText(Resource.String.Lbl_Ok));
                        }

                        ToggleVisibility(false);
                    }
                }
                else if (apiStatus == 400)
                {
                    if (respond is ErrorObject error)
                    {
                        ToggleVisibility(false);
                        var errorText = error.Error.ErrorText;
                        var errorId = error.Error.ErrorId;
                        switch (errorId)
                        {
                            case "3":
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister_3), GetText(Resource.String.Lbl_Ok));
                                break;
                            case "4":
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister_4), GetText(Resource.String.Lbl_Ok));
                                break;
                            case "5":
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_something_went_wrong), GetText(Resource.String.Lbl_Ok)); break;
                            case "6":
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister_6), GetText(Resource.String.Lbl_Ok));
                                break;
                            case "7":
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister_7), GetText(Resource.String.Lbl_Ok));
                                break;
                            case "8":
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister_8), GetText(Resource.String.Lbl_Ok));
                                break;
                            case "9":
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister_9), GetText(Resource.String.Lbl_Ok));
                                break;
                            case "10":
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister_10), GetText(Resource.String.Lbl_Ok));
                                break;
                            case "11":
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister_11), GetText(Resource.String.Lbl_Ok));
                                break;
                            default:
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                                break;
                        }
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
                ToggleVisibility(false);
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), exception.Message, GetText(Resource.String.Lbl_Ok));
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open Terms Of Service
        private void TxtTermsOfServiceOnClick(object sender, EventArgs e)
        {
            try
            {
                var url = InitializeWoWonder.WebsiteUrl + "/terms/terms";
                new IntentController(this).OpenBrowserFromApp(url);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtBirthdayOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Up) return;

                var frag = PopupDialogController.DatePickerFragment.NewInstance(delegate (DateTime time)
                {
                    try
                    {
                        if (AppSettings.IsUserYearsOld) // 18
                        {
                            if (!Methods.Time.HasAgeRequirement(time.Date)) // over 18 years
                            {
                                TxtBirthday.Text = time.Date.ToString("dd-MM-yyyy");
                            }
                            else
                            {
                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Error_IsUserYearsOld), ToastLength.Short);
                            }
                        }
                        else //All
                        {
                            TxtBirthday.Text = time.Date.ToString("dd-MM-yyyy");
                        }
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                }, "Birthday");
                frag.Show(SupportFragmentManager, PopupDialogController.DatePickerFragment.Tag);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtGenderOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Up) return;

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialAlertDialogBuilder(this);

                switch (ListUtils.SettingsSiteList?.Genders?.Count)
                {
                    case > 0:
                        arrayAdapter.AddRange(from item in ListUtils.SettingsSiteList?.Genders select item.Value);
                        break;
                    default:
                        arrayAdapter.Add(GetText(Resource.String.Radio_Male));
                        arrayAdapter.Add(GetText(Resource.String.Radio_Female));
                        break;
                }

                dialogList.SetTitle(GetText(Resource.String.Lbl_Gender));
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region MaterialDialog

        public void OnSelection(IDialogInterface dialog, int position, string itemString)
        {
            try
            {
                switch (ListUtils.SettingsSiteList?.Genders?.Count)
                {
                    case > 0:
                        {
                            TxtGender.Text = itemString;

                            var key = ListUtils.SettingsSiteList?.Genders?.FirstOrDefault(a => a.Value == itemString).Key;
                            if (key == "male")
                            {
                                GenderStatus = "male";
                                UserDetails.Avatar = "no_profile_image";
                            }
                            else if (key == "female")
                            {
                                GenderStatus = "female";
                                UserDetails.Avatar = "no_profile_female_image";
                            }
                            else
                            {
                                GenderStatus = key ?? "male";
                                UserDetails.Avatar = "no_profile_image";
                            }

                            break;
                        }
                    default:
                        {
                            if (itemString == GetText(Resource.String.Radio_Male))
                            {
                                TxtGender.Text = GetText(Resource.String.Radio_Male);
                                GenderStatus = "male";
                                UserDetails.Avatar = "no_profile_image";
                            }
                            else if (itemString == GetText(Resource.String.Radio_Female))
                            {
                                TxtGender.Text = GetText(Resource.String.Radio_Female);
                                GenderStatus = "female";
                                UserDetails.Avatar = "no_profile_female_image";
                            }
                            else
                            {
                                TxtGender.Text = GetText(Resource.String.Radio_Male);
                                GenderStatus = "male";
                                UserDetails.Avatar = "no_profile_image";
                            }

                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        public override void ToggleVisibility(bool isLoginProgress)
        {
            try
            {
                ProgressBar.Visibility = isLoginProgress ? ViewStates.Visible : ViewStates.Gone;
                BtnSignUp.Visibility = isLoginProgress ? ViewStates.Invisible : ViewStates.Visible;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

    }
}