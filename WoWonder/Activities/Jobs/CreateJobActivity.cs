using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Content.Res;
using AndroidX.Core.Content;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Com.Google.Android.Gms.Ads.Admanager;
using Google.Android.Material.Dialog;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WoWonder.Activities.Base;
using WoWonder.Activities.Jobs.Adapters;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Jobs;
using WoWonderClient.Requests;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Jobs
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class CreateJobActivity : BaseActivity, View.IOnFocusChangeListener, IDialogListCallBack
    {
        #region Variables Basic

        private TextView TxtSave;
        private TextView IconTitle, IconLocation, IconSalary, IconCurrency, IconJobType, IconDescription, IconCategory;
        private EditText TxtTitle, TxtLocation, TxtMinimum, TxtMaximum, TxtCurrency, TxtSalaryDate, TxtJobType, TxtDescription, TxtCategory;
        public EditText TxtAddQuestion;
        private ImageView Image;
        private RelativeLayout SelectImageView;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private QuestionAdapter MAdapter;
        private string TypeDialog, CategoryId, JobTypeId, SalaryDateId, CurrencyId, ImagePath, PageId, Lat, Lng;

        private AdManagerAdView AdManagerAdView;

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
                SetContentView(Resource.Layout.CreateJobLayout);

                PageId = Intent?.GetStringExtra("PageId") ?? "";

                //Get Value And Set Toolbar
                InitComponent();
                SetRecyclerViewAdapters();
                InitToolbar();

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
                AdsGoogle.LifecycleAdManagerAdView(AdManagerAdView, "Resume");
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
                AdsGoogle.LifecycleAdManagerAdView(AdManagerAdView, "Pause");
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
                TxtSave = FindViewById<TextView>(Resource.Id.toolbar_title);

                SelectImageView = FindViewById<RelativeLayout>(Resource.Id.SelectImageView);
                Image = FindViewById<ImageView>(Resource.Id.image);

                IconTitle = FindViewById<TextView>(Resource.Id.IconTitle);
                TxtTitle = FindViewById<EditText>(Resource.Id.TitleEditText);

                IconLocation = FindViewById<TextView>(Resource.Id.IconLocation);
                TxtLocation = FindViewById<EditText>(Resource.Id.LocationEditText);

                IconSalary = FindViewById<TextView>(Resource.Id.IconSalary);
                TxtMinimum = FindViewById<EditText>(Resource.Id.MinimumEditText);
                TxtMaximum = FindViewById<EditText>(Resource.Id.MaximumEditText);

                IconCurrency = FindViewById<TextView>(Resource.Id.IconCurrency);
                TxtCurrency = FindViewById<EditText>(Resource.Id.CurrencyEditText);
                TxtSalaryDate = FindViewById<EditText>(Resource.Id.SalaryDateEditText);

                IconJobType = FindViewById<TextView>(Resource.Id.IconJobType);
                TxtJobType = FindViewById<EditText>(Resource.Id.JobTypeEditText);

                IconCategory = FindViewById<TextView>(Resource.Id.IconCategory);
                TxtCategory = FindViewById<EditText>(Resource.Id.CategoryEditText);

                IconDescription = FindViewById<TextView>(Resource.Id.IconDescription);
                TxtDescription = FindViewById<EditText>(Resource.Id.DescriptionEditText);

                TxtAddQuestion = FindViewById<EditText>(Resource.Id.AddQuestionEditText);
                TxtAddQuestion.Text = GetText(Resource.String.Lbl_AddQuestion) + "(0)";

                MRecycler = FindViewById<RecyclerView>(Resource.Id.Recyler);

                AdManagerAdView = FindViewById<AdManagerAdView>(Resource.Id.multiple_ad_sizes_view);
                AdsGoogle.InitAdManagerAdView(AdManagerAdView);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconTitle, FontAwesomeIcon.User);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconLocation, FontAwesomeIcon.MapMarkedAlt);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconSalary, FontAwesomeIcon.MoneyBillAlt);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconCurrency, FontAwesomeIcon.DollarSign);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconJobType, FontAwesomeIcon.Briefcase);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconDescription, FontAwesomeIcon.Paragraph);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeBrands, IconCategory, FontAwesomeIcon.Buromobelexperte);

                Methods.SetColorEditText(TxtTitle, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtLocation, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtMinimum, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtMaximum, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtCurrency, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtSalaryDate, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtJobType, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtDescription, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtAddQuestion, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtCategory, WoWonderTools.IsTabDark() ? Color.White : Color.Black);

                Methods.SetFocusable(TxtCurrency);
                Methods.SetFocusable(TxtSalaryDate);
                Methods.SetFocusable(TxtJobType);
                Methods.SetFocusable(TxtCategory);
                Methods.SetFocusable(TxtAddQuestion);
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
                    toolBar.Title = GetText(Resource.String.Lbl_CreateJob);
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
                LayoutManager = new LinearLayoutManager(this);
                MAdapter = new QuestionAdapter(this) { QuestionList = new ObservableCollection<QuestionJob>() };
                MRecycler.SetLayoutManager(LayoutManager);
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
                        TxtSave.Click += TxtSaveOnClick;
                        TxtLocation.OnFocusChangeListener = this;
                        TxtSalaryDate.Touch += TxtSalaryDateOnTouch;
                        TxtJobType.Touch += TxtJobTypeOnTouch;
                        TxtCategory.Touch += TxtCategoryOnTouch;
                        TxtCurrency.Touch += TxtCurrencyOnTouch;
                        SelectImageView.Click += TxtAddImgOnClick;
                        TxtAddQuestion.Touch += TxtAddQuestionOnTouch;
                        break;
                    default:
                        TxtSave.Click -= TxtSaveOnClick;
                        TxtLocation.OnFocusChangeListener = null!;
                        TxtSalaryDate.Touch -= TxtSalaryDateOnTouch;
                        TxtJobType.Touch -= TxtJobTypeOnTouch;
                        TxtCategory.Touch -= TxtCategoryOnTouch;
                        TxtCurrency.Touch -= TxtCurrencyOnTouch;
                        SelectImageView.Click -= TxtAddImgOnClick;
                        TxtAddQuestion.Touch -= TxtAddQuestionOnTouch;
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
                AdsGoogle.LifecycleAdManagerAdView(AdManagerAdView, "Destroy");

                TxtSave = null!;
                Image = null!;
                SelectImageView = null!;
                IconTitle = null!;
                TxtTitle = null!;
                IconLocation = null!;
                TxtLocation = null!;
                IconSalary = null!;
                TxtMinimum = null!;
                TxtMaximum = null!;
                IconCurrency = null!;
                TxtCurrency = null!;
                TxtSalaryDate = null!;
                IconJobType = null!;
                TxtJobType = null!;
                IconCategory = null!;
                TxtCategory = null!;
                IconDescription = null!;
                TxtDescription = null!;
                TxtAddQuestion = null!;
                MRecycler = null!;
                MAdapter = null!;
                LayoutManager = null!;
                TypeDialog = null!;
                CategoryId = null!;
                JobTypeId = null!;
                SalaryDateId = null!;
                CurrencyId = null!;
                ImagePath = null!;
                PageId = null!;
                Lat = null!;
                Lng = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        #endregion

        #region Events

        private void TxtAddQuestionOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Up) return;

                switch (MAdapter.ItemCount)
                {
                    case < 4:
                        {
                            TypeDialog = "AddQuestion";

                            var dialogList = new MaterialAlertDialogBuilder(this);

                            var arrayAdapter = WoWonderTools.GetAddQuestionList(this).Select(item => item.Value).ToList();

                            dialogList.SetTitle(GetText(Resource.String.Lbl_TypeQuestion));
                            dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                            dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                            dialogList.Show();
                            break;
                        }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtAddImgOnClick(object sender, EventArgs e)
        {
            try
            {
                PixImagePickerUtils.OpenDialogGallery(this);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtCategoryOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Up) return;

                switch (CategoriesController.ListCategoriesJob.Count)
                {
                    case > 0:
                        {
                            TypeDialog = "Categories";

                            var dialogList = new MaterialAlertDialogBuilder(this);

                            var arrayAdapter = CategoriesController.ListCategoriesJob.Select(item => item.CategoriesName).ToList();

                            dialogList.SetTitle(GetText(Resource.String.Lbl_SelectCategories));
                            dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                            dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                            dialogList.Show();
                            break;
                        }
                    default:
                        Methods.DisplayReportResult(this, "Not have List Categories Job");
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtJobTypeOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Up) return;

                var dialogList = new MaterialAlertDialogBuilder(this);

                TypeDialog = "JobType";
                var arrayAdapter = WoWonderTools.GetJobTypeList(this).Select(item => item.Value).ToList();

                dialogList.SetTitle(GetText(Resource.String.Lbl_JobType));
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtSalaryDateOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Up) return;

                var dialogList = new MaterialAlertDialogBuilder(this);

                TypeDialog = "SalaryDate";

                var arrayAdapter = WoWonderTools.GetSalaryDateList(this).Select(item => item.Value).ToList();

                dialogList.SetTitle(GetText(Resource.String.Lbl_SalaryDate));
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                dialogList.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtCurrencyOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Up) return;

                if (ListUtils.SettingsSiteList?.CurrencySymbolArray.CurrencyList != null)
                {
                    TypeDialog = "Currency";

                    var arrayAdapter = WoWonderTools.GetCurrencySymbolList();
                    switch (arrayAdapter?.Count)
                    {
                        case > 0:
                            {
                                var dialogList = new MaterialAlertDialogBuilder(this);

                                dialogList.SetTitle(GetText(Resource.String.Lbl_SelectCurrency));
                                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                                dialogList.Show();
                                break;
                            }
                    }
                }
                else
                {
                    Methods.DisplayReportResult(this, "Not have List Currency");
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtLocationOnClick()
        {
            try
            {
                switch ((int)Build.VERSION.SdkInt)
                {
                    // Check if we're running on Android 5.0 or higher
                    case < 23:
                        //Open intent Location when the request code of result is 502
                        new IntentController(this).OpenIntentLocation();
                        break;
                    default:
                        {
                            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) == Permission.Granted && ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) == Permission.Granted)
                            {
                                //Open intent Location when the request code of result is 502
                                new IntentController(this).OpenIntentLocation();
                            }
                            else
                            {
                                new PermissionsController(this).RequestPermission(105);
                            }

                            break;
                        }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void TxtSaveOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {

                    if (string.IsNullOrEmpty(TxtTitle.Text) || string.IsNullOrEmpty(TxtLocation.Text) || string.IsNullOrEmpty(TxtMinimum.Text)
                        || string.IsNullOrEmpty(TxtMaximum.Text) || string.IsNullOrEmpty(TxtSalaryDate.Text) || string.IsNullOrEmpty(TxtJobType.Text)
                        || string.IsNullOrEmpty(TxtDescription.Text) || string.IsNullOrEmpty(TxtCategory.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_enter_your_data), ToastLength.Short);
                        return;
                    }

                    if (string.IsNullOrEmpty(ImagePath))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_select_Image), ToastLength.Short);
                        return;
                    }

                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                    var dictionary = new Dictionary<string, string>
                    {
                        {"job_title", TxtTitle.Text},
                        {"description", TxtDescription.Text},
                        {"location", TxtLocation.Text},
                        {"job_type", JobTypeId},
                        {"category", CategoryId},
                        {"page_id", PageId},
                        {"lat", Lat},
                        {"lng", Lng},
                        {"minimum", TxtMinimum.Text},
                        {"maximum", TxtMaximum.Text},
                        {"salary_date", SalaryDateId},
                        {"currency", CurrencyId},
                        {"image_type", "upload"},
                    };

                    switch (MAdapter.QuestionList.Count)
                    {
                        case > 0:
                            {
                                for (int i = 0; i < MAdapter.QuestionList.Count; i++)
                                {
                                    switch (i)
                                    {
                                        case 0:
                                            {
                                                var question = MAdapter.QuestionList[i];
                                                switch (question)
                                                {
                                                    case null:
                                                        continue;
                                                }
                                                dictionary.Add("question_one", question.Question);
                                                dictionary.Add("question_one_type", question.QuestionType);
                                                dictionary.Add("question_one_answers", question.QuestionAnswer);
                                                break;
                                            }
                                        case 1:
                                            {
                                                var question = MAdapter.QuestionList[i];
                                                switch (question)
                                                {
                                                    case null:
                                                        continue;
                                                }
                                                dictionary.Add("question_two", question.Question);
                                                dictionary.Add("question_two_type", question.QuestionType);
                                                dictionary.Add("question_two_answers", question.QuestionAnswer);
                                                break;
                                            }
                                        case 2:
                                            {
                                                var question = MAdapter.QuestionList[i];
                                                switch (question)
                                                {
                                                    case null:
                                                        continue;
                                                }
                                                dictionary.Add("question_three", question.Question);
                                                dictionary.Add("question_three_type", question.QuestionType);
                                                dictionary.Add("question_three_answers", question.QuestionAnswer);
                                                break;
                                            }
                                    }
                                }

                                break;
                            }
                    }

                    var (apiStatus, respond) = await RequestsAsync.Jobs.CreateJobAsync(dictionary, ImagePath);
                    switch (apiStatus)
                    {
                        case 200:
                            {
                                switch (respond)
                                {
                                    case CreateJobObject result:
                                        {
                                            ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_jobSuccessfullyAdded), ToastLength.Short);

                                            AndHUD.Shared.Dismiss();

                                            Intent intent = new Intent();
                                            intent.PutExtra("JobsItem", JsonConvert.SerializeObject(result.Data));
                                            SetResult(Result.Ok, intent);
                                            Finish();
                                            break;
                                        }
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
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                AndHUD.Shared.Dismiss();
            }
        }

        #endregion

        #region Permissions && Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);

                if (requestCode == 502 && resultCode == Result.Ok)
                {
                    GetPlaceFromPicker(data);
                }
                else if (requestCode == PixImagePickerActivity.RequestCode && resultCode == Result.Ok)
                {
                    var listPath = JsonConvert.DeserializeObject<ResultIntentPixImage>(data.GetStringExtra("ResultPixImage") ?? "");
                    if (listPath?.List?.Count > 0)
                    {
                        var filepath = listPath.List.FirstOrDefault();
                        if (!string.IsNullOrEmpty(filepath))
                        {
                            //Do something with your Uri
                            ImagePath = filepath;

                            Glide.With(this).Load(filepath).Apply(new RequestOptions()).Into(Image);
                        }
                        else
                        {
                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Failed_to_load), ToastLength.Short);
                        }
                    }
                }
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
                    case 105 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        //Open intent Camera when the request code of result is 503
                        new IntentController(this).OpenIntentLocation();
                        break;
                    case 105:
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                        break;
                    case 108 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        PixImagePickerUtils.OpenDialogGallery(this);
                        break;
                    case 108:
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

        #region MaterialDialog
        public void OnSelection(IDialogInterface dialog, int position, string itemString)
        {
            try
            {
                switch (TypeDialog)
                {
                    case "Categories":
                        CategoryId = CategoriesController.ListCategoriesJob.FirstOrDefault(categories => categories.CategoriesName == itemString)?.CategoriesId;
                        TxtCategory.Text = itemString;
                        break;
                    case "JobType":
                        JobTypeId = WoWonderTools.GetJobTypeList(this)?.FirstOrDefault(a => a.Value == itemString).Key.ToString();
                        TxtJobType.Text = itemString;
                        break;
                    case "SalaryDate":
                        SalaryDateId = WoWonderTools.GetSalaryDateList(this)?.FirstOrDefault(a => a.Value == itemString).Key.ToString();
                        TxtSalaryDate.Text = itemString;
                        break;
                    case "Currency":
                        TxtCurrency.Text = itemString;
                        CurrencyId = WoWonderTools.GetIdCurrency(itemString);
                        break;
                    case "AddQuestion":
                        {
                            TxtAddQuestion.Text = GetText(Resource.String.Lbl_AddQuestion) + "(" + MAdapter.ItemCount + ")";

                            var addQuestionId = WoWonderTools.GetAddQuestionList(this)?.FirstOrDefault(a => a.Value == itemString).Key.ToString();
                            //SetQuestionOne(addQuestionId);
                            MAdapter.QuestionList.Add(new QuestionJob
                            {
                                Id = MAdapter.ItemCount,
                                QuestionType = addQuestionId
                            });
                            MAdapter.NotifyItemInserted(MAdapter.QuestionList.IndexOf(MAdapter.QuestionList.Last()));
                            break;
                        }
                    case "AddQuestionAdapter":
                        {
                            TxtAddQuestion.Text = GetText(Resource.String.Lbl_AddQuestion) + "(" + MAdapter.ItemCount + ")";

                            var addQuestionId = WoWonderTools.GetAddQuestionList(this)?.FirstOrDefault(a => a.Value == itemString).Key.ToString();

                            var data = MAdapter.QuestionList.FirstOrDefault(a => a.Id == ItemQuestionJob.Id && a.QuestionType == ItemQuestionJob.QuestionType);
                            if (data != null)
                            {
                                data.QuestionType = addQuestionId;
                                MAdapter.NotifyItemChanged(MAdapter.QuestionList.IndexOf(data));
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

        #region Question

        private QuestionJob ItemQuestionJob;

        public void OpenDialogSetQuestion(QuestionJob item)
        {
            try
            {
                ItemQuestionJob = item;

                TypeDialog = "AddQuestionAdapter";

                var dialogList = new MaterialAlertDialogBuilder(this);
                var arrayAdapter = WoWonderTools.GetAddQuestionList(this).Select(pair => pair.Value).ToList();
                dialogList.SetTitle(GetText(Resource.String.Lbl_TypeQuestion));
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                dialogList.Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private void GetPlaceFromPicker(Intent data)
        {
            try
            {
                var placeAddress = data.GetStringExtra("Address") ?? "";
                var placeLatLng = data.GetStringExtra("latLng") ?? "";
                TxtLocation.Text = string.IsNullOrEmpty(placeAddress) switch
                {
                    false => placeAddress,
                    _ => TxtLocation.Text
                };
                switch (string.IsNullOrEmpty(placeLatLng))
                {
                    case false:
                        {
                            var latLng = placeLatLng.Split(",");
                            Lat = latLng.First();
                            Lng = latLng.Last();
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnFocusChange(View v, bool hasFocus)
        {
            try
            {
                if (v?.Id == TxtLocation.Id && hasFocus)
                {
                    TxtLocationOnClick();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}