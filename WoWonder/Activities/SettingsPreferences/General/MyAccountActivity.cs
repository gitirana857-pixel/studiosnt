using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Content.Res;
using AndroidX.AppCompat.Widget;
using Google.Android.Material.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using WoWonder.Activities.Base;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Utils;
using WoWonder.SQLite;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.SettingsPreferences.General
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MyAccountActivity : BaseActivity, View.IOnClickListener, IDialogListCallBack
    {
        #region Variables Basic

        private EditText TxtUsername, TxtEmail, TxtBirthday, TxtGender, TxtCountry;
        private AppCompatButton BtnSave;
        private string GenderStatus = "", CountryId, TypeDialog = "";

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.MyAccountLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                Get_Data_User();
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
                BtnSave = FindViewById<AppCompatButton>(Resource.Id.SaveButton);

                TxtUsername = FindViewById<EditText>(Resource.Id.NameEditText);

                TxtEmail = FindViewById<EditText>(Resource.Id.EmailEditText);

                TxtBirthday = FindViewById<EditText>(Resource.Id.BirthdayEditText);
                TxtBirthday.SetOnClickListener(this);

                TxtGender = (EditText)FindViewById(Resource.Id.GenderEditText);

                TxtCountry = FindViewById<EditText>(Resource.Id.CountryEditText);

                Methods.SetColorEditText(TxtCountry, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtUsername, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtEmail, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtBirthday, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtGender, WoWonderTools.IsTabDark() ? Color.White : Color.Black);

                Methods.SetFocusable(TxtGender);

                AdsGoogle.Ad_AdMobNative(this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitToolbar()
        {
            try
            {
                var toolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolBar != null)
                {
                    toolBar.Title = GetText(Resource.String.Lbl_My_Account);
                    toolBar.SetTitleTextColor(WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                    SetSupportActionBar(toolBar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
                    var icon = AppCompatResources.GetDrawable(this, AppSettings.FlowDirectionRightToLeft ? Resource.Drawable.icon_back_arrow_right : Resource.Drawable.icon_back_arrow_left);
                    icon?.SetTint(WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                    SupportActionBar.SetHomeAsUpIndicator(icon);

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
                switch (addEvent)
                {
                    // true +=  // false -=
                    case true:
                        TxtGender.Touch += TxtGenderOnTouch;
                        BtnSave.Click += SaveData_OnClick;
                        TxtCountry.Touch += TxtCountryOnTouch;
                        break;
                    default:
                        TxtGender.Touch -= TxtGenderOnTouch;
                        BtnSave.Click -= SaveData_OnClick;
                        TxtCountry.Touch -= TxtCountryOnTouch;
                        break;
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
                BtnSave = null!;
                TxtUsername = null!;
                TxtEmail = null!;
                TxtBirthday = null!;
                TxtGender = null!;
                TxtCountry = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        #endregion

        #region Events

        private void TxtCountryOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Up) return;

                TypeDialog = "Country";

                var countriesArray = WoWonderTools.GetCountryList(this);

                var dialogList = new MaterialAlertDialogBuilder(this);

                var arrayAdapter = countriesArray.Select(item => item.Value).ToList();

                dialogList.SetTitle(GetText(Resource.String.Lbl_Location));
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                dialogList.Show();
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

                TypeDialog = "Genders";

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

        //Save data 
        private async void SaveData_OnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                if (string.IsNullOrEmpty(TxtUsername.Text) || string.IsNullOrEmpty(TxtEmail.Text))
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_Please_enter_your_data), ToastLength.Short);
                }

                if (TxtUsername.Text.Contains(" "))
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_ErrorSpaceUsername), ToastLength.Short);
                }

                var check = Methods.FunString.IsEmailValid(TxtEmail.Text.Replace(" ", ""));
                if (!check)
                {
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_IsEmailValid), GetText(Resource.String.Lbl_Ok));
                    return;
                }

                if (Methods.CheckConnectivity())
                {
                    //Show a progress 
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                    var dataUser = ListUtils.MyProfileList?.FirstOrDefault();

                    var dictionary = new Dictionary<string, string>
                    {
                        {"username", TxtUsername.Text.Replace(" ","")},
                        {"email", TxtEmail.Text},
                        {"gender", GenderStatus},
                        {"country_id", CountryId},

                    };

                    string newFormat = "";
                    if (!string.IsNullOrEmpty(TxtBirthday.Text))
                    {
                        var date = TxtBirthday.Text.Split('-', '/');
                        if (date.Length > 0)
                            newFormat = date[0] + "-" + date[1] + "-" + date[2];

                        dictionary.Add("birthday", newFormat);
                    }

                    var (apiStatus, respond) = await RequestsAsync.Global.UpdateUserDataAsync(dictionary);
                    switch (apiStatus)
                    {
                        case 200:
                            {
                                switch (respond)
                                {
                                    case MessageObject result when result.Message.Contains("updated"):
                                        {
                                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_YourDetailsWasUpdated), ToastLength.Short);

                                            if (dataUser != null)
                                            {
                                                dataUser.Username = TxtUsername.Text.Replace(" ", "");

                                                dataUser.Birthday = string.IsNullOrEmpty(newFormat) switch
                                                {
                                                    false => newFormat,
                                                    _ => dataUser.Birthday
                                                };

                                                dataUser.Gender = GenderStatus;
                                                dataUser.GenderText = TxtGender.Text;
                                                dataUser.CountryId = CountryId;

                                                switch (ListUtils.SettingsSiteList?.EmailValidation)
                                                {
                                                    case "1" when dataUser.Email != TxtEmail.Text:
                                                        //wael send code Email Validation
                                                        break;
                                                    default:
                                                        dataUser.Email = TxtEmail.Text;
                                                        break;
                                                }

                                                var sqLiteDatabase = new SqLiteDatabase();
                                                sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);

                                            }

                                            AndHUD.Shared.Dismiss();
                                            break;
                                        }
                                    case MessageObject result:
                                        //Show a Error image with a message
                                        AndHUD.Shared.ShowError(this, result.Message, MaskType.Clear, TimeSpan.FromSeconds(1));
                                        break;
                                }

                                break;
                            }
                        default:
                            Methods.DisplayAndHudErrorResult(this, respond);
                            break;
                    }
                }
                else
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                //Show a Error image with a message
                AndHUD.Shared.ShowError(this, e.Message, MaskType.Clear, TimeSpan.FromSeconds(1));
                //AndHUD.Shared.Dismiss();
            }
        }

        #endregion

        private void Get_Data_User()
        {
            try
            {
                var local = ListUtils.MyProfileList?.FirstOrDefault();
                if (local != null)
                {
                    TxtUsername.Text = local.Username;
                    TxtEmail.Text = local.Email;

                    try
                    {
                        if (local.Birthday != "0000-00-00")
                        {
                            DateTime date = DateTime.Parse(local.Birthday);
                            string newFormat = date.Day + "/" + date.Month + "/" + date.Year;
                            TxtBirthday.Text = newFormat;
                        }
                    }
                    catch
                    {
                        TxtBirthday.Text = local.Birthday;
                    }


                    switch (string.IsNullOrEmpty(local.CountryId))
                    {
                        case false when local.CountryId != "0":
                            {
                                var countryName = WoWonderTools.GetCountryList(this).FirstOrDefault(a => a.Key == local.CountryId).Value;

                                TxtCountry.Text = countryName;
                                break;
                            }
                    }

                    switch (ListUtils.SettingsSiteList?.Genders?.Count)
                    {
                        case > 0:
                            {
                                var value = ListUtils.SettingsSiteList?.Genders?.FirstOrDefault(a => a.Key == local.Gender).Value;
                                if (value != null)
                                {
                                    TxtGender.Text = value;
                                    GenderStatus = local.Gender;
                                }
                                else
                                {
                                    TxtGender.Text = GetText(Resource.String.Radio_Male);
                                    GenderStatus = "male";
                                }

                                break;
                            }
                        default:
                            {
                                if (local.Gender == GetText(Resource.String.Radio_Male))
                                {
                                    TxtGender.Text = GetText(Resource.String.Radio_Male);
                                    GenderStatus = "male";
                                }
                                else if (local.Gender == GetText(Resource.String.Radio_Female))
                                {
                                    TxtGender.Text = GetText(Resource.String.Radio_Female);
                                    GenderStatus = "female";
                                }
                                else
                                {
                                    TxtGender.Text = GetText(Resource.String.Radio_Male);
                                    GenderStatus = "male";
                                }

                                break;
                            }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region MaterialDialog

        public void OnSelection(IDialogInterface dialog, int position, string itemString)
        {
            try
            {
                switch (TypeDialog)
                {
                    case "Genders" when ListUtils.SettingsSiteList?.Genders?.Count > 0:
                        {
                            var key = ListUtils.SettingsSiteList?.Genders?.FirstOrDefault(a => a.Value == itemString).Key;
                            if (key != null)
                            {
                                TxtGender.Text = itemString;
                                GenderStatus = key;
                            }
                            else
                            {
                                TxtGender.Text = itemString;
                                GenderStatus = "male";
                            }

                            break;
                        }
                    case "Genders" when itemString == GetText(Resource.String.Radio_Male):
                        TxtGender.Text = GetText(Resource.String.Radio_Male);
                        GenderStatus = "male";
                        break;
                    case "Genders" when itemString == GetText(Resource.String.Radio_Female):
                        TxtGender.Text = GetText(Resource.String.Radio_Female);
                        GenderStatus = "female";
                        break;
                    case "Genders":
                        TxtGender.Text = GetText(Resource.String.Radio_Male);
                        GenderStatus = "male";
                        break;
                    case "Country":
                        {
                            var countriesArray = WoWonderTools.GetCountryList(this);
                            var check = countriesArray.FirstOrDefault(a => a.Value == itemString).Key;
                            if (check != null)
                            {
                                CountryId = check;
                            }

                            TxtCountry.Text = itemString;
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

        public void OnClick(View v)
        {
            try
            {
                if (v.Id == TxtBirthday.Id)
                {
                    var frag = PopupDialogController.DatePickerFragment.NewInstance(delegate (DateTime time)
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
                    }, "Birthday");
                    frag.Show(SupportFragmentManager, PopupDialogController.DatePickerFragment.Tag);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}