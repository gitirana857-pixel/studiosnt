using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.Dialog;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using WoWonder.Activities.Fundings.Adapters;
using WoWonder.Activities.NativePost.Extra;
using WoWonder.Activities.Tabbes;
using WoWonder.Activities.Wallet;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.Share;
using WoWonder.Library.Anjo.Share.Abstractions;
using WoWonderClient;
using WoWonderClient.Classes.Funding;
using WoWonderClient.Classes.Message;
using WoWonderClient.Classes.Payments;
using WoWonderClient.Requests;
using BaseActivity = WoWonder.Activities.Base.BaseActivity;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Fundings
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class FundingViewActivity : BaseActivity, IDialogListCallBack
    {
        #region Variables Basic

        private ImageView ImageUser, ImageFunding, IconBack, Avatar;
        private TextView TxtMore, TxtUsername, TxtTime, TxtTitle, TxtDescription, TxtFundRaise, TxtFundAmount, TxtDonation, Username;
        private ProgressBar ProgressBar;
        private AppCompatButton BtnDonate, BtnShare, BtnContact;
        private LinearLayout RecentDonationsLayout;
        private RecyclerView MRecycler;
        private RecentDonationAdapter MAdapter;
        private LinearLayoutManager LayoutManager;

        private FundingDataObject DataObject;
        private string FundId, CodeName;
        private static FundingViewActivity Instance;

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
                SetContentView(Resource.Layout.FundingViewLayout);

                Instance = this;

                //Get Value And Set Toolbar 
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();
                LoadData();
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
                ImageUser = FindViewById<ImageView>(Resource.Id.imageAvatar);
                ImageFunding = FindViewById<ImageView>(Resource.Id.imageFunding);
                IconBack = FindViewById<ImageView>(Resource.Id.iv_back);

                TxtUsername = FindViewById<TextView>(Resource.Id.username);
                TxtTime = FindViewById<TextView>(Resource.Id.time);
                TxtTitle = FindViewById<TextView>(Resource.Id.title);
                TxtDescription = FindViewById<TextView>(Resource.Id.description);
                TxtFundRaise = FindViewById<TextView>(Resource.Id.raised);
                TxtFundAmount = FindViewById<TextView>(Resource.Id.TottalAmount);
                TxtDonation = FindViewById<TextView>(Resource.Id.timedonation);
                BtnDonate = FindViewById<AppCompatButton>(Resource.Id.DonateButton);
                BtnShare = FindViewById<AppCompatButton>(Resource.Id.share);
                BtnContact = FindViewById<AppCompatButton>(Resource.Id.cont);
                Avatar = FindViewById<ImageView>(Resource.Id.avatar);
                Username = FindViewById<TextView>(Resource.Id.name);

                RecentDonationsLayout = FindViewById<LinearLayout>(Resource.Id.layout_recent_donations);
                RecentDonationsLayout.Visibility = ViewStates.Gone;

                MRecycler = (RecyclerView)FindViewById(Resource.Id.recycler);

                TxtMore = FindViewById<TextView>(Resource.Id.toolbar_title);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, TxtMore, IonIconsFonts.More);
                if (TxtMore != null)
                {
                    TxtMore.SetTextSize(ComplexUnitType.Sp, 20f);
                    TxtMore.Visibility = ViewStates.Gone;
                }

                ProgressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);

                BtnContact.Visibility = AppSettings.MessengerIntegration switch
                {
                    false => ViewStates.Gone,
                    _ => BtnContact.Visibility
                };
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
                    toolBar.Title = " ";
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

        private void SetRecyclerViewAdapters()
        {
            try
            {
                MAdapter = new RecentDonationAdapter(this)
                {
                    UserList = new ObservableCollection<RecentDonation>(),
                };
                LayoutManager = new LinearLayoutManager(this);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(50);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                MRecycler.SetAdapter(MAdapter);
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
                        TxtMore.Click += TxtMoreOnClick;
                        BtnDonate.Click += BtnDonateOnClick;
                        IconBack.Click += IconBackOnClick;
                        BtnShare.Click += BtnShareOnClick;
                        BtnContact.Click += BtnContactOnClick;
                        TxtTime.Click += UserImageAvatarOnClick;
                        TxtUsername.Click += UserImageAvatarOnClick;
                        ImageUser.Click += UserImageAvatarOnClick;
                        break;
                    default:
                        TxtMore.Click -= TxtMoreOnClick;
                        BtnDonate.Click -= BtnDonateOnClick;
                        IconBack.Click -= IconBackOnClick;
                        BtnShare.Click -= BtnShareOnClick;
                        BtnContact.Click -= BtnContactOnClick;
                        TxtTime.Click -= UserImageAvatarOnClick;
                        TxtUsername.Click -= UserImageAvatarOnClick;
                        ImageUser.Click -= UserImageAvatarOnClick;
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
                ImageUser = null!;
                ImageFunding = null!;
                IconBack = null!;
                TxtUsername = null!;
                TxtTime = null!;
                TxtTitle = null!;
                TxtDescription = null!;
                TxtFundRaise = null!;
                TxtFundAmount = null!;
                TxtDonation = null!;
                BtnDonate = null!;
                BtnShare = null!;
                BtnContact = null!;
                ProgressBar = null!;
                TxtMore = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static FundingViewActivity GetInstance()
        {
            try
            {
                return Instance;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }
        #endregion

        #region Events

        private void UserImageAvatarOnClick(object sender, EventArgs e)
        {
            try
            {
                WoWonderTools.OpenProfile(this, DataObject.UserData.UserId, DataObject.UserData);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Contact User
        private void BtnContactOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!WoWonderTools.ChatIsAllowed(DataObject.UserData))
                    return;

                if (AppSettings.ShowDialogAskOpenMessenger)
                {
                    var dialog = new MaterialAlertDialogBuilder(this);

                    dialog.SetTitle(Resource.String.Lbl_Warning);
                    dialog.SetMessage(GetText(Resource.String.Lbl_ContentAskOPenAppMessenger));
                    dialog.SetPositiveButton(GetText(Resource.String.Lbl_Yes), (materialDialog, action) =>
                    {
                        try
                        {
                            Methods.App.OpenAppByPackageName(this, AppSettings.MessengerPackageName, "OpenChat", new ChatObject { UserId = DataObject.UserData.UserId, Name = DataObject.UserData.Name, Avatar = DataObject.UserData.Avatar });
                        }
                        catch (Exception exception)
                        {
                            Methods.DisplayReportResultTrack(exception);
                        }
                    });
                    dialog.SetNegativeButton(GetText(Resource.String.Lbl_No), new MaterialDialogUtils());

                    dialog.Show();
                }
                else
                {
                    Methods.App.OpenAppByPackageName(this, AppSettings.MessengerPackageName, "OpenChat", new ChatObject { UserId = DataObject.UserData.UserId, Name = DataObject.UserData.Name, Avatar = DataObject.UserData.Avatar });
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Share
        private void BtnShareOnClick(object sender, EventArgs e)
        {
            try
            {
                ShareEvent();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //BAck
        private void IconBackOnClick(object sender, EventArgs e)
        {
            Finish();
        }

        private void TxtMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialAlertDialogBuilder(this);

                arrayAdapter.Add(GetText(Resource.String.Lbl_Copy));

                bool owner = DataObject.UserId == UserDetails.UserId;
                if (owner)
                {
                    arrayAdapter.Add(GetText(Resource.String.Lbl_Edit));
                    arrayAdapter.Add(GetText(Resource.String.Lbl_Delete));
                }

                dialogList.SetTitle(GetText(Resource.String.Lbl_More));
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Menu >> Edit
        private void EditEvent()
        {
            try
            {
                Intent intent = new Intent(this, typeof(EditFundingActivity));
                intent.PutExtra("FundingObject", JsonConvert.SerializeObject(DataObject));
                StartActivityForResult(intent, 253);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> Copy Link
        private void CopyLinkEvent()
        {
            try
            {
                Methods.CopyToClipboard(this, InitializeWoWonder.WebsiteUrl + "/show_fund/" + DataObject.HashedId);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> Share
        private async void ShareEvent()
        {
            try
            {
                switch (CrossShare.IsSupported)
                {
                    //Share Plugin same as video
                    case false:
                        return;
                    default:
                        await CrossShare.Current.Share(new ShareMessage
                        {
                            Title = DataObject.Title,
                            Text = DataObject.Description,
                            Url = InitializeWoWonder.WebsiteUrl + "/show_fund/" + DataObject.HashedId
                        });
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //open Payment
        private void BtnDonateOnClick(object sender, EventArgs e)
        {
            try
            {
                var dialog = new MaterialAlertDialogBuilder(this);
                dialog.SetTitle(Resource.String.Lbl_Donate);

                EditText input = new EditText(this);
                input.SetHint(Resource.String.Lbl_DonateCode);
                input.InputType = InputTypes.ClassNumber;
                LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                input.LayoutParameters = lp;

                dialog.SetView(input);

                dialog.SetPositiveButton(GetText(Resource.String.Btn_Send), (materialDialog, s) =>
                {
                    try
                    {
                        var text = input.Text ?? "";
                        if (text.Length <= 0) return;
                        CodeName = text;

                        var dialogBuilder = new MaterialAlertDialogBuilder(this);
                        dialogBuilder.SetTitle(Resource.String.Lbl_PurchaseRequired);
                        dialogBuilder.SetPositiveButton(GetText(Resource.String.Lbl_Purchase), async (materialDialog, action) =>
                        {
                            try
                            {
                                if (WoWonderTools.CheckWallet(Convert.ToInt32(text)))
                                {
                                    if (Methods.CheckConnectivity())
                                    {
                                        var (apiStatus, respond) = await RequestsAsync.Payments.FundingPayAsync(DataObject.Id, CodeName);
                                        if (apiStatus == 200)
                                        {
                                            if (respond is PaymentSuccessfullyObject result)
                                            {
                                                Console.WriteLine(result.Message);
                                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Donated), ToastLength.Long);
                                            }
                                        }
                                        else Methods.DisplayReportResult(this, respond);
                                    }
                                    else
                                        Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                                }
                                else
                                {
                                    var dialogTheme = new MaterialAlertDialogBuilder(this);
                                    dialogTheme.SetTitle(GetText(Resource.String.Lbl_Wallet));
                                    dialogTheme.SetMessage(GetText(Resource.String.Lbl_Error_NoWallet));
                                    dialogTheme.SetPositiveButton(GetText(Resource.String.Lbl_AddWallet), (materialDialog, action) =>
                                    {
                                        try
                                        {
                                            StartActivity(new Intent(this, typeof(TabbedWalletActivity)));
                                        }
                                        catch (Exception exception)
                                        {
                                            Methods.DisplayReportResultTrack(exception);
                                        }
                                    });
                                    dialogTheme.SetNegativeButton(GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());

                                    dialogTheme.Show();
                                }
                            }
                            catch (Exception exception)
                            {
                                Methods.DisplayReportResultTrack(exception);
                            }
                        });
                        dialogBuilder.SetNegativeButton(GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());

                        dialogBuilder.Show();
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
                dialog.SetNegativeButton(GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());

                dialog.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Result

        //Result
        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);

                switch (requestCode)
                {
                    case 253 when resultCode == Result.Ok:
                        {
                            if (string.IsNullOrEmpty(data.GetStringExtra("itemData"))) return;
                            var item = JsonConvert.DeserializeObject<FundingDataObject>(data.GetStringExtra("itemData") ?? "");
                            if (item != null)
                            {
                                DataObject = item;

                                TxtUsername.Text = Methods.FunString.DecodeString(item.UserData.Name);

                                TxtTime.Text = GetString(Resource.String.Lbl_Last_seen) + " " + Methods.Time.TimeAgo(Convert.ToInt32(item.Time), false);

                                TxtTitle.Text = Methods.FunString.DecodeString(item.Title);
                                TxtDescription.Text = Methods.FunString.DecodeString(item.Description);

                                ProgressBar.Progress = Convert.ToInt32(item.Bar);

                                //$0 Raised of $1000000
                                TxtFundRaise.Text = "$" + item.Raised.ToString(CultureInfo.InvariantCulture) + " " + GetString(Resource.String.Lbl_RaisedOf) + " " + "$" + item.Amount;
                            }

                            break;
                        }
                    case 2654 when resultCode == Result.Ok:
                        await Task.Factory.StartNew(StartApiService);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region MaterialDialog

        public void OnSelection(IDialogInterface dialog, int position, string itemString)
        {
            try
            {
                string text = itemString;
                if (text == GetString(Resource.String.Lbl_Share))
                {
                    ShareEvent();
                }
                else if (text == GetString(Resource.String.Lbl_Edit))
                {
                    EditEvent();
                }
                else if (text == GetString(Resource.String.Lbl_Copy))
                {
                    CopyLinkEvent();
                }
                else if (text == GetString(Resource.String.Lbl_Delete))
                {
                    var dialogBuilder = new MaterialAlertDialogBuilder(this);
                    dialogBuilder.SetTitle(Resource.String.Lbl_Warning);
                    dialogBuilder.SetMessage(GetText(Resource.String.Lbl_DeleteFunding));
                    dialogBuilder.SetPositiveButton(GetText(Resource.String.Lbl_Yes), (materialDialog, action) =>
                    {
                        try
                        {
                            // Send Api delete  
                            if (Methods.CheckConnectivity())
                            {
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Funding.DeleteFundingAsync(DataObject.Id) });

                                var instance = FundingActivity.GetInstance();
                                var dataFunding = instance?.FundingTab?.MAdapter?.FundingList?.FirstOrDefault(a => a.Id == DataObject.Id);
                                if (dataFunding != null)
                                {
                                    instance?.FundingTab?.MAdapter?.FundingList.Remove(dataFunding);
                                    instance.FundingTab?.MAdapter?.NotifyItemRemoved(instance.FundingTab.MAdapter.FundingList.IndexOf(dataFunding));
                                }

                                var dataMyFunding = instance?.MyFundingTab?.MAdapter?.FundingList?.FirstOrDefault(a => a.Id == DataObject.Id);
                                if (dataMyFunding != null)
                                {
                                    instance?.MyFundingTab?.MAdapter?.FundingList.Remove(dataMyFunding);
                                    instance.MyFundingTab?.MAdapter?.NotifyItemRemoved(instance.MyFundingTab.MAdapter.FundingList.IndexOf(dataMyFunding));
                                }

                                var recycler = TabbedMainActivity.GetInstance()?.NewsFeedTab?.MainRecyclerView;
                                var dataGlobal2 = recycler?.NativeFeedAdapter.ListDiffer?.Where(a => a.PostData?.FundId == DataObject.Id).ToList();
                                if (dataGlobal2 != null)
                                {
                                    foreach (var postData in dataGlobal2)
                                    {
                                        recycler.RemoveByRowIndex(postData);
                                    }
                                }

                                var adapterGlobal = WRecyclerView.GetInstance()?.NativeFeedAdapter;
                                var diff = adapterGlobal?.ListDiffer;
                                var dataGlobal = diff?.Where(a => a.PostData?.FundId == DataObject.Id).ToList();
                                if (dataGlobal != null)
                                {
                                    foreach (var postData in dataGlobal)
                                    {
                                        WRecyclerView.GetInstance()?.RemoveByRowIndex(postData);
                                    }
                                }

                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_postSuccessfullyDeleted), ToastLength.Short);
                            }
                            else
                            {
                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                            }
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    });
                    dialogBuilder.SetNegativeButton(GetText(Resource.String.Lbl_No), new MaterialDialogUtils());

                    dialogBuilder.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Load Data Funding 

        private void GetDataFunding(FundingDataObject dataObject)
        {
            try
            {
                if (dataObject != null)
                {
                    GlideImageLoader.LoadImage(this, dataObject.UserData.Avatar, ImageUser, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);
                    GlideImageLoader.LoadImage(this, dataObject.UserData.Avatar, Avatar, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);
                    GlideImageLoader.LoadImage(this, dataObject.Image, ImageFunding, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                    Username.Text = WoWonderTools.GetNameFinal(dataObject.UserData);
                    TxtUsername.Text = WoWonderTools.GetNameFinal(dataObject.UserData);

                    bool success = int.TryParse(dataObject.Time, out var number);
                    switch (success)
                    {
                        case true:
                            Console.WriteLine("Converted '{0}' to {1}.", dataObject.Time, number);
                            TxtTime.Text = GetString(Resource.String.Lbl_Last_seen) + " " + Methods.Time.TimeAgo(number, false);
                            TxtDonation.Text = Methods.Time.TimeAgo(number, false);
                            break;
                        default:
                            Console.WriteLine("Attempted conversion of '{0}' failed.", dataObject.Time ?? "<null>");
                            TxtTime.Text = Methods.Time.ReplaceTime(dataObject.Time);
                            TxtDonation.Text = dataObject.Time;
                            break;
                    }

                    TxtTitle.Text = Methods.FunString.DecodeString(dataObject.Title);
                    TxtDescription.Text = Methods.FunString.DecodeString(dataObject.Description);

                    TxtMore.Visibility = ViewStates.Visible;

                    try
                    {
                        dataObject.Raised = dataObject.Raised.Replace(AppSettings.CurrencyFundingPriceStatic, "");
                        dataObject.Amount = dataObject.Amount.Replace(AppSettings.CurrencyFundingPriceStatic, "");

                        decimal d = decimal.Parse(dataObject.Raised, CultureInfo.InvariantCulture);
                        TxtFundRaise.Text = GetText(Resource.String.Lbl_Collected) + " " + AppSettings.CurrencyFundingPriceStatic + d.ToString("0.00");

                        decimal amount = decimal.Parse(dataObject.Amount, CultureInfo.InvariantCulture);
                        TxtFundAmount.Text = GetText(Resource.String.Lbl_Goal) + " " + AppSettings.CurrencyFundingPriceStatic + amount.ToString("0.00");
                    }
                    catch (Exception exception)
                    {
                        TxtFundRaise.Text = AppSettings.CurrencyFundingPriceStatic + dataObject.Raised;
                        TxtFundAmount.Text = AppSettings.CurrencyFundingPriceStatic + dataObject.Amount;
                        Methods.DisplayReportResultTrack(exception);
                    }

                    BtnContact.Visibility = dataObject.UserData.UserId == UserDetails.UserId ? ViewStates.Gone : ViewStates.Visible;

                    ProgressBar.Progress = Convert.ToInt32(dataObject.Bar?.ToString("0") ?? "0");

                    if (dataObject.IsDonate != null && dataObject.IsDonate.Value == 1)
                    {
                        BtnDonate.Visibility = ViewStates.Gone;
                    }

                    switch (dataObject.RecentDonations?.Count)
                    {
                        case > 0:
                            MAdapter.UserList = new ObservableCollection<RecentDonation>(dataObject.RecentDonations);
                            MAdapter.NotifyDataSetChanged();

                            RecentDonationsLayout.Visibility = ViewStates.Visible;
                            break;
                        default:
                            RecentDonationsLayout.Visibility = ViewStates.Gone;
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadData()
        {
            try
            {
                FundId = Intent?.GetStringExtra("FundId") ?? "";
                DataObject = JsonConvert.DeserializeObject<FundingDataObject>(Intent?.GetStringExtra("ItemObject") ?? "");
                if (DataObject != null)
                {
                    FundId = DataObject.HashedId;

                    GetDataFunding(DataObject);
                }

                Task.Factory.StartNew(StartApiService);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { GetFundingById });
        }

        private async Task GetFundingById()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var (apiStatus, respond) = await RequestsAsync.Funding.GetFundingByIdAsync(FundId);
                    if (apiStatus == 200)
                    {
                        if (respond is GetFundingByIdObject result)
                            RunOnUiThread(() => GetDataFunding(result.Data));
                    }
                    else
                        Methods.DisplayReportResult(this, respond);
                }
                else
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

    }
}