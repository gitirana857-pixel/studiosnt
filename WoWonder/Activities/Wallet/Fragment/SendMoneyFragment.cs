using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Widget;
using System;
using System.Linq;
using WoWonder.Activities.Contacts;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Utils;
using WoWonderClient.Requests;
using Exception = System.Exception;

namespace WoWonder.Activities.Wallet.Fragment
{
    public class SendMoneyFragment : AndroidX.Fragment.App.Fragment
    {
        #region  Variables Basic
        private ImageView Avatar;
        private TextView TxtProfileName, TxtUsername;

        public EditText TxtAmount, TxtEmail;
        private TextView TxtMyBalance;
        private AppCompatButton BtnContinue;
        public string UserId;
        private string Price;
        private TabbedWalletActivity GlobalContext;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            GlobalContext = (TabbedWalletActivity)Activity;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                View view = inflater.Inflate(Resource.Layout.SendMoneyLayout, container, false);
                return view;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            {
                base.OnViewCreated(view, savedInstanceState);

                InitComponent(view);
                AddOrRemoveEvent(true);
                Get_Data_User();
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

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                Avatar = view.FindViewById<ImageView>(Resource.Id.avatar);
                TxtProfileName = view.FindViewById<TextView>(Resource.Id.name);
                TxtUsername = view.FindViewById<TextView>(Resource.Id.tv_subname);

                TxtMyBalance = view.FindViewById<TextView>(Resource.Id.myBalance);

                TxtAmount = view.FindViewById<EditText>(Resource.Id.AmountEditText);
                TxtEmail = view.FindViewById<EditText>(Resource.Id.EmailEditText);
                BtnContinue = view.FindViewById<AppCompatButton>(Resource.Id.ContinueButton);

                Methods.SetColorEditText(TxtEmail, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtAmount, WoWonderTools.IsTabDark() ? Color.White : Color.Black);

                Methods.SetFocusable(TxtEmail);
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
                        BtnContinue.Click += BtnContinueOnClick;
                        TxtEmail.Touch += TxtEmailOnTouch;
                        break;
                    default:
                        BtnContinue.Click -= BtnContinueOnClick;
                        TxtEmail.Touch -= TxtEmailOnTouch;
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

        //select user >> Get User id
        private void TxtEmailOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Up) return;

                Intent intent = new Intent(Activity, typeof(SelectContactsActivity));
                Activity.StartActivityForResult(intent, 1202);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Send Amount
        private async void BtnContinueOnClick(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(TxtAmount.Text) || string.IsNullOrWhiteSpace(TxtAmount.Text) || Convert.ToInt32(TxtAmount.Text) == 0)
                {
                    ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_Please_enter_amount), ToastLength.Short);
                    return;
                }

                if (string.IsNullOrEmpty(TxtEmail.Text) || string.IsNullOrWhiteSpace(TxtEmail.Text))
                {
                    ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_Please_enter_name_email), ToastLength.Short);
                    return;
                }

                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                    return;
                }

                GlobalContext.TypeOpenPayment = "SendMoneyFragment";
                Price = TxtAmount.Text;

                //Show a progress
                AndHUD.Shared.Show(Activity, GetText(Resource.String.Lbl_Loading));

                var (apiStatus, respond) = await RequestsAsync.Global.SendMoneyWalletAsync(UserId, TxtAmount.Text);
                switch (apiStatus)
                {
                    case 200:
                        TxtAmount.Text = string.Empty;
                        TxtEmail.Text = string.Empty;

                        AndHUD.Shared.Dismiss();
                        ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_MoneySentSuccessfully), ToastLength.Short);
                        break;
                    default:
                        Methods.DisplayAndHudErrorResult(Activity, respond);
                        break;
                }
            }
            catch (Exception exception)
            {
                AndHUD.Shared.Dismiss();
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        public async void Get_Data_User()
        {
            try
            {
                if (ListUtils.MyProfileList.Count == 0)
                    await ApiRequest.Get_MyProfileData_Api(Activity);

                var userData = ListUtils.MyProfileList?.FirstOrDefault();
                if (userData != null)
                {
                    GlideImageLoader.LoadImage(GlobalContext, userData.Avatar, Avatar, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);
                    TxtProfileName.Text = WoWonderTools.GetNameFinal(userData);
                    TxtUsername.Text = "@" + userData.Username;

                    TxtMyBalance.Text = userData.Wallet;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}