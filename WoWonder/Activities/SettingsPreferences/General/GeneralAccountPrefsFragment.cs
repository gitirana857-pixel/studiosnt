using Android.Content;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.Preference;
using System;
using System.Linq;
using WoWonder.Activities.BlockedUsers;
using WoWonder.Activities.MyProfile;
using WoWonder.Activities.SettingsPreferences.Custom;
using WoWonder.Helpers.Utils;
using Exception = System.Exception;

namespace WoWonder.Activities.SettingsPreferences.General
{
    public class GeneralAccountPrefsFragment : PreferenceFragmentCompat, ISharedPreferencesOnSharedPreferenceChangeListener
    {
        #region  Variables Basic

        private GeneralCustomPreference EditProfilePref, EditAccountPref, EditSocialLinksPref, EditPasswordPref, BlockedUsersPref, DeleteAccountPref, AboutMePref, TwoFactorPref, ManageSessionsPref, VerificationPref;
        private GeneralCustomPreference StorageConnectedMobilePref, StorageConnectedWiFiPref;
        private string SAbout = "";
        private readonly AppCompatActivity ActivityContext;
        private GeneralCustomPreference NightMode, LangPref;

        #endregion

        #region General

        public GeneralAccountPrefsFragment(AppCompatActivity context)
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

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                // create ContextThemeWrapper from the original Activity Context with the custom theme
                Context contextThemeWrapper = WoWonderTools.IsTabDark() ? new ContextThemeWrapper(ActivityContext, Resource.Style.SettingsThemeDark) : new ContextThemeWrapper(ActivityContext, Resource.Style.SettingsTheme);

                // clone the inflater using the ContextThemeWrapper
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);

                View view = base.OnCreateView(localInflater, container, savedInstanceState);

                return view;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            try
            {
                // Load the preferences from an XML resource
                AddPreferencesFromResource(Resource.Xml.SettingsPrefs_GeneralAccount);

                MainSettings.SharedData = PreferenceManager.SharedPreferences;
                InitComponent();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnResume()
        {
            try
            {
                base.OnResume();
                PreferenceManager.SharedPreferences.RegisterOnSharedPreferenceChangeListener(this);
                AddOrRemoveEvent(true);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnPause()
        {
            try
            {
                base.OnPause();
                PreferenceScreen.SharedPreferences.UnregisterOnSharedPreferenceChangeListener(this);
                AddOrRemoveEvent(false);
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
                MainSettings.SharedData = PreferenceManager.SharedPreferences;
                PreferenceManager.SharedPreferences.RegisterOnSharedPreferenceChangeListener(this);

                EditProfilePref = (GeneralCustomPreference)FindPreference("editprofile_key");
                AboutMePref = (GeneralCustomPreference)FindPreference("about_me_key");
                EditAccountPref = (GeneralCustomPreference)FindPreference("editAccount_key");
                EditSocialLinksPref = (GeneralCustomPreference)FindPreference("editSocialLinks_key");
                EditPasswordPref = (GeneralCustomPreference)FindPreference("editpassword_key");
                BlockedUsersPref = (GeneralCustomPreference)FindPreference("blocked_key");
                DeleteAccountPref = (GeneralCustomPreference)FindPreference("deleteaccount_key");
                TwoFactorPref = (GeneralCustomPreference)FindPreference("Twofactor_key");
                ManageSessionsPref = (GeneralCustomPreference)FindPreference("ManageSessions_key");
                NightMode = (GeneralCustomPreference)FindPreference("Night_Mode_key");
                LangPref = (GeneralCustomPreference)FindPreference("Lang_key");
                VerificationPref = (GeneralCustomPreference)FindPreference("verification_key");

                StorageConnectedMobilePref = (GeneralCustomPreference)FindPreference("StorageConnectedMobile_key");
                StorageConnectedWiFiPref = (GeneralCustomPreference)FindPreference("StorageConnectedWiFi_key");

                //Update Preferences data on Load
                OnSharedPreferenceChanged(MainSettings.SharedData, "about_me_key");
                OnSharedPreferenceChanged(MainSettings.SharedData, "Night_Mode_key");
                OnSharedPreferenceChanged(MainSettings.SharedData, "Lang_key");

                NightMode.IconSpaceReserved = false;

                //Delete Preference
                var mCategoryAccount = (PreferenceCategory)FindPreference("SectionAccount_key");
                switch (AppSettings.ShowSettingsAccount)
                {
                    case false:
                        mCategoryAccount.RemovePreference(EditAccountPref);
                        break;
                }

                switch (AppSettings.ShowSettingsSocialLinks)
                {
                    case false:
                        mCategoryAccount.RemovePreference(EditSocialLinksPref);
                        break;
                }

                switch (AppSettings.ShowSettingsBlockedUsers)
                {
                    case false:
                        mCategoryAccount.RemovePreference(BlockedUsersPref);
                        break;
                }

                switch (AppSettings.ShowSettingsVerification)
                {
                    case false:
                        mCategoryAccount.RemovePreference(VerificationPref);
                        break;
                }

                var mCategorySecurity = (PreferenceCategory)FindPreference("SecurityAccount_key");
                switch (AppSettings.ShowSettingsPassword)
                {
                    case false:
                        mCategorySecurity.RemovePreference(EditPasswordPref);
                        break;
                }

                switch (AppSettings.ShowSettingsDeleteAccount)
                {
                    case false:
                        mCategorySecurity.RemovePreference(DeleteAccountPref);
                        break;
                }

                switch (AppSettings.ShowSettingsTwoFactor)
                {
                    case false:
                        mCategorySecurity.RemovePreference(TwoFactorPref);
                        break;
                }

                switch (AppSettings.ShowSettingsManageSessions)
                {
                    case false:
                        mCategorySecurity.RemovePreference(ManageSessionsPref);
                        break;
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
                        //LangPref.PreferenceChange += LangPref_OnPreferenceChange; 
                        EditProfilePref.PreferenceClick += EditProfilePref_OnPreferenceClick;
                        EditAccountPref.PreferenceClick += EditAccountPrefOnPreferenceClick;
                        EditSocialLinksPref.PreferenceClick += EditSocialLinksPref_OnPreferenceClick;
                        EditPasswordPref.PreferenceClick += EditPasswordPref_OnPreferenceClick;
                        BlockedUsersPref.PreferenceClick += BlockedUsersPref_OnPreferenceClick;
                        DeleteAccountPref.PreferenceClick += DeleteAccountPref_OnPreferenceClick;
                        TwoFactorPref.PreferenceClick += TwoFactorPrefOnPreferenceClick;
                        ManageSessionsPref.PreferenceClick += ManageSessionsPrefOnPreferenceClick;
                        VerificationPref.PreferenceClick += VerificationPrefOnPreferenceClick;
                        break;
                    default:
                        //LangPref.PreferenceChange -= LangPref_OnPreferenceChange; 
                        EditProfilePref.PreferenceClick -= EditProfilePref_OnPreferenceClick;
                        EditAccountPref.PreferenceClick -= EditAccountPrefOnPreferenceClick;
                        EditSocialLinksPref.PreferenceClick -= EditSocialLinksPref_OnPreferenceClick;
                        EditPasswordPref.PreferenceClick -= EditPasswordPref_OnPreferenceClick;
                        BlockedUsersPref.PreferenceClick -= BlockedUsersPref_OnPreferenceClick;
                        DeleteAccountPref.PreferenceClick -= DeleteAccountPref_OnPreferenceClick;
                        TwoFactorPref.PreferenceClick -= TwoFactorPrefOnPreferenceClick;
                        ManageSessionsPref.PreferenceClick -= ManageSessionsPrefOnPreferenceClick;
                        VerificationPref.PreferenceClick -= VerificationPrefOnPreferenceClick;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        //Edit Profile
        private void EditProfilePref_OnPreferenceClick(object sender, Preference.PreferenceClickEventArgs preferenceClickEventArgs)
        {
            try
            {
                var intent = new Intent(ActivityContext, typeof(EditMyProfileActivity));
                ActivityContext.StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Edit Account
        private void EditAccountPrefOnPreferenceClick(object sender, Preference.PreferenceClickEventArgs preferenceClickEventArgs)
        {
            try
            {
                var intent = new Intent(ActivityContext, typeof(MyAccountActivity));
                ActivityContext.StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Edit Social Links
        private void EditSocialLinksPref_OnPreferenceClick(object sender, Preference.PreferenceClickEventArgs preferenceClickEventArgs)
        {
            try
            {
                var intent = new Intent(ActivityContext, typeof(EditSocialLinksActivity));
                ActivityContext.StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Edit Password
        private void EditPasswordPref_OnPreferenceClick(object sender, Preference.PreferenceClickEventArgs preferenceClickEventArgs)
        {
            try
            {
                var intent = new Intent(ActivityContext, typeof(PasswordActivity));
                ActivityContext.StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Block users
        private void BlockedUsersPref_OnPreferenceClick(object sender, Preference.PreferenceClickEventArgs preferenceClickEventArgs)
        {
            try
            {
                var intent = new Intent(ActivityContext, typeof(BlockedUsersActivity));
                ActivityContext.StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Verification
        private void VerificationPrefOnPreferenceClick(object sender, Preference.PreferenceClickEventArgs preferenceClickEventArgs)
        {
            try
            {
                var intent = new Intent(ActivityContext, typeof(VerificationActivity));
                ActivityContext.StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Delete Account  
        private void DeleteAccountPref_OnPreferenceClick(object sender, Preference.PreferenceClickEventArgs preferenceClickEventArgs)
        {
            try
            {
                var intent = new Intent(ActivityContext, typeof(DeleteAccountActivity));
                ActivityContext.StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //TwoFactor
        private void TwoFactorPrefOnPreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            try
            {
                var intent = new Intent(ActivityContext, typeof(TwoFactorAuthActivity));
                ActivityContext.StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //ManageSessions
        private void ManageSessionsPrefOnPreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            try
            {
                var intent = new Intent(ActivityContext, typeof(ManageSessionsActivity));
                ActivityContext.StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        //Lang
        //private void LangPref_OnPreferenceChange(object sender, Preference.PreferenceChangeEventArgs eventArgs)
        //{
        //    try
        //    {
        //        if (eventArgs.Handled)
        //        {
        //            var etp = (ListPreference) sender;
        //            var value = eventArgs.NewValue;

        //            AppSettings.Lang = value.ToString();

        //            MainSettings.SetApplicationLang(Activity, AppSettings.Lang);

        //            ToastUtils.ShowToast(ActivityContext, GetText(Resource.String.Lbl_Application_Restart), ToastLength.Long);

        //            var intent = new Intent(Activity, typeof(SplashScreenActivity));
        //            intent.AddCategory(Intent?.CategoryHome);
        //            intent.SetAction(Intent?.ActionMain);
        //            intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
        //            Activity.StartActivity(intent);
        //            Activity.FinishAffinity();

        //            AppSettings.Lang = value.ToString();
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Methods.DisplayReportResultTrack(e);
        //    }
        //}

        #endregion

        //On Change 
        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            try
            {
                var dataUser = ListUtils.MyProfileList?.FirstOrDefault();

                switch (key)
                {
                    case "about_me_key":
                        {
                            // Set summary to be the user-description for the selected value
                            Preference etp = FindPreference("about_me_key");
                            if (dataUser != null)
                            {
                                SAbout = WoWonderTools.GetAboutFinal(dataUser);

                                MainSettings.SharedData?.Edit()?.PutString("about_me_key", SAbout)?.Commit();
                                etp.Summary = SAbout;
                            }

                            string getvalue = MainSettings.SharedData?.GetString("about_me_key", SAbout);
                            etp.Summary = getvalue;
                            break;
                        }
                    case "Night_Mode_key":
                        {
                            // Set summary to be the user-description for the selected value
                            Preference etp = FindPreference("Night_Mode_key");

                            string getValue = MainSettings.SharedData?.GetString("Night_Mode_key", string.Empty);
                            if (getValue == MainSettings.LightMode)
                            {
                                etp.Summary = ActivityContext.GetString(Resource.String.Lbl_Light);
                            }
                            else if (getValue == MainSettings.DarkMode)
                            {
                                etp.Summary = ActivityContext.GetString(Resource.String.Lbl_Dark);
                            }
                            else if (getValue == MainSettings.DefaultMode)
                            {
                                etp.Summary = ActivityContext.GetString(Resource.String.Lbl_SetByBattery);
                            }
                            else
                            {
                                etp.Summary = getValue;
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

        public override bool OnPreferenceTreeClick(Preference preference)
        {
            try
            {
                switch (preference.Key)
                {
                    case "about_me_key":
                        {
                            var intent = new Intent(ActivityContext, typeof(UpdateAboutActivity));
                            intent.PutExtra("about", preference.Summary);
                            ActivityContext.StartActivity(intent);
                            break;
                        }
                    case "Night_Mode_key":
                        {
                            var intent = new Intent(ActivityContext, typeof(ThemeActivity));
                            ActivityContext.StartActivity(intent);

                            break;
                        }
                    case "Lang_key":
                        {
                            BottomSheetsLanguage BottomSheetsLanguage = new BottomSheetsLanguage();
                            BottomSheetsLanguage.Show(ActivityContext.SupportFragmentManager, BottomSheetsLanguage.Tag);

                            break;
                        }
                }
                return base.OnPreferenceTreeClick(preference);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return base.OnPreferenceTreeClick(preference);
            }
        }

    }
}