using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using System;
using System.Linq;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Utils;
using WoWonder.Payment.Utils;
using Exception = System.Exception;

namespace WoWonder.Activities.Wallet.Fragment
{
    public class AddFundsFragment : AndroidX.Fragment.App.Fragment
    {
        #region  Variables Basic
        private ImageView Avatar;
        private TextView TxtProfileName, TxtUsername;

        private TextView TxtMyBalance;
        public EditText TxtAmount;
        private AppCompatButton BtnContinue;
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
                View view = inflater.Inflate(Resource.Layout.AddFundsLayout, container, false);
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
                BtnContinue = view.FindViewById<AppCompatButton>(Resource.Id.ContinueButton);

                Methods.SetColorEditText(TxtAmount, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
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
                        break;
                    default:
                        BtnContinue.Click -= BtnContinueOnClick;
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

        private void BtnContinueOnClick(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(TxtAmount.Text) || string.IsNullOrWhiteSpace(TxtAmount.Text) || Convert.ToInt32(TxtAmount.Text) == 0)
                {
                    ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_Please_enter_amount), ToastLength.Short);
                    return;
                }

                GlobalContext.TypeOpenPayment = "AddFundsFragment";
                GlobalContext.Price = TxtAmount.Text;

                Bundle bundle = new Bundle();
                bundle.PutString("Price", TxtAmount.Text);

                PaymentXBottomSheetDialog bottomSheetDialog = new PaymentXBottomSheetDialog
                {
                    Arguments = bundle
                };
                bottomSheetDialog.Show(ChildFragmentManager, bottomSheetDialog.Tag);
            }
            catch (Exception exception)
            {
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