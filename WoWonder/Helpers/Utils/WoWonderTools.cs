using Android.App;
using Android.Content;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.Locations;
using Android.Media;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using Google.Android.Material.Dialog;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WoWonder.Activities.BlockedUsers;
using WoWonder.Activities.MyProfile;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Activities.Tabbes;
using WoWonder.Activities.UserProfile;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.SQLite;
using WoWonderClient;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Group;
using WoWonderClient.Classes.Jobs;
using WoWonderClient.Requests;
using Exception = Java.Lang.Exception;
using Path = System.IO.Path;

namespace WoWonder.Helpers.Utils
{
    public static class WoWonderTools
    {
        public static string GetNameFinal(UserDataObject dataUser)
        {
            try
            {
                if (dataUser == null)
                    return "";

                if (!string.IsNullOrEmpty(dataUser.Name) || !string.IsNullOrWhiteSpace(dataUser.Name))
                    return Methods.FunString.DecodeString(dataUser.Name);

                if (!string.IsNullOrEmpty(dataUser.Username) || !string.IsNullOrWhiteSpace(dataUser.Username))
                    return Methods.FunString.DecodeString(dataUser.Username);

                return "";
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return "";
            }
        }

        public static string GetAboutFinal(UserDataObject dataUser)
        {
            try
            {
                return dataUser switch
                {
                    null => Application.Context.Resources?.GetString(Resource.String.Lbl_DefaultAbout) + " " + AppSettings.ApplicationName,
                    _ => string.IsNullOrEmpty(dataUser.About) switch
                    {
                        false when !string.IsNullOrWhiteSpace(dataUser.About) => Methods.FunString.DecodeString(dataUser.About),
                        _ => Application.Context.Resources?.GetString(Resource.String.Lbl_DefaultAbout) + " " +
                             AppSettings.ApplicationName
                    }
                };
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return Application.Context.Resources?.GetString(Resource.String.Lbl_DefaultAbout) + " " + AppSettings.ApplicationName;
            }
        }

        public static string GetIdCurrency(string currency)
        {
            try
            {
                if (ListUtils.SettingsSiteList?.CurrencySymbolArray.CurrencyList != null)
                {
                    string currencyIcon = "0";
                    switch (currency?.ToUpper())
                    {
                        case "USD":
                        case "$":
                            if (ListUtils.SettingsSiteList?.CurrencyArray.CurrencyList?.Count is > 0)
                                currencyIcon = ListUtils.SettingsSiteList?.CurrencyArray.CurrencyList.FindIndex(a => a.Contains("USD") || a.Contains("$")).ToString();
                            else
                                currencyIcon = ListUtils.SettingsSiteList?.CurrencyArray.StringMap?.Count switch
                                {
                                    > 0 => ListUtils.SettingsSiteList?.CurrencyArray.StringMap?.FirstOrDefault(a => a.Value.Contains("USD") || a.Value.Contains("$")).Key,
                                    _ => currencyIcon
                                };

                            break;
                        case "Jpy":
                        case "¥":
                            if (ListUtils.SettingsSiteList?.CurrencyArray.CurrencyList?.Count is > 0)
                                currencyIcon = ListUtils.SettingsSiteList?.CurrencyArray.CurrencyList.FindIndex(a => a.Contains("Jpy") || a.Contains("¥")).ToString();
                            else
                                currencyIcon = ListUtils.SettingsSiteList?.CurrencyArray.StringMap?.Count switch
                                {
                                    > 0 => ListUtils.SettingsSiteList?.CurrencyArray.StringMap?.FirstOrDefault(a => a.Value.Contains("Jpy") || a.Value.Contains("¥")).Key,
                                    _ => currencyIcon
                                };

                            break;
                        case "EUR":
                        case "€":
                            if (ListUtils.SettingsSiteList?.CurrencyArray.CurrencyList?.Count is > 0)
                                currencyIcon = ListUtils.SettingsSiteList?.CurrencyArray.CurrencyList.FindIndex(a => a.Contains("EUR") || a.Contains("€")).ToString();
                            else
                                currencyIcon = ListUtils.SettingsSiteList?.CurrencyArray.StringMap?.Count switch
                                {
                                    > 0 => ListUtils.SettingsSiteList?.CurrencyArray.StringMap?.FirstOrDefault(a => a.Value.Contains("EUR") || a.Value.Contains("€")).Key,
                                    _ => currencyIcon
                                };

                            break;
                        case "TRY":
                        case "₺":
                            if (ListUtils.SettingsSiteList?.CurrencyArray.CurrencyList?.Count is > 0)
                                currencyIcon = ListUtils.SettingsSiteList?.CurrencyArray.CurrencyList.FindIndex(a => a.Contains("TRY") || a.Contains("₺")).ToString();
                            else
                                currencyIcon = ListUtils.SettingsSiteList?.CurrencyArray.StringMap?.Count switch
                                {
                                    > 0 => ListUtils.SettingsSiteList?.CurrencyArray.StringMap?.FirstOrDefault(a => a.Value.Contains("TRY") || a.Value.Contains("₺")).Key,
                                    _ => currencyIcon
                                };

                            break;
                        case "GBP":
                        case "£":
                            if (ListUtils.SettingsSiteList?.CurrencyArray.CurrencyList?.Count is > 0)
                                currencyIcon = ListUtils.SettingsSiteList?.CurrencyArray.CurrencyList.FindIndex(a => a.Contains("GBP") || a.Contains("£")).ToString();
                            else
                                currencyIcon = ListUtils.SettingsSiteList?.CurrencyArray.StringMap?.Count switch
                                {
                                    > 0 => ListUtils.SettingsSiteList?.CurrencyArray.StringMap?.FirstOrDefault(a => a.Value.Contains("GBP") || a.Value.Contains("£")).Key,
                                    _ => currencyIcon
                                };

                            break;
                        case "RUB":
                        case "₽":
                            if (ListUtils.SettingsSiteList?.CurrencyArray.CurrencyList?.Count is > 0)
                                currencyIcon = ListUtils.SettingsSiteList?.CurrencyArray.CurrencyList.FindIndex(a => a.Contains("RUB") || a.Contains("₽")).ToString();
                            else
                                currencyIcon = ListUtils.SettingsSiteList?.CurrencyArray.StringMap?.Count switch
                                {
                                    > 0 => ListUtils.SettingsSiteList?.CurrencyArray.StringMap?.FirstOrDefault(a => a.Value.Contains("RUB") || a.Value.Contains("₽")).Key,
                                    _ => currencyIcon
                                };

                            break;
                        case "PLN":
                        case "zł":
                            if (ListUtils.SettingsSiteList?.CurrencyArray.CurrencyList?.Count is > 0)
                                currencyIcon = ListUtils.SettingsSiteList?.CurrencyArray.CurrencyList.FindIndex(a => a.Contains("PLN") || a.Contains("zł")).ToString();
                            else
                                currencyIcon = ListUtils.SettingsSiteList?.CurrencyArray.StringMap?.Count switch
                                {
                                    > 0 => ListUtils.SettingsSiteList?.CurrencyArray.StringMap?.FirstOrDefault(a => a.Value.Contains("PLN") || a.Value.Contains("zł")).Key,
                                    _ => currencyIcon
                                };

                            break;
                        case "ILS":
                        case "₪":
                            if (ListUtils.SettingsSiteList?.CurrencyArray.CurrencyList?.Count is > 0)
                                currencyIcon = ListUtils.SettingsSiteList?.CurrencyArray.CurrencyList.FindIndex(a => a.Contains("ILS") || a.Contains("₪")).ToString();
                            else
                                currencyIcon = ListUtils.SettingsSiteList?.CurrencyArray.StringMap?.Count switch
                                {
                                    > 0 => ListUtils.SettingsSiteList?.CurrencyArray.StringMap?.FirstOrDefault(a => a.Value.Contains("ILS") || a.Value.Contains("₪")).Key,
                                    _ => currencyIcon
                                };

                            break;
                        case "BRL":
                        case "R$":
                            if (ListUtils.SettingsSiteList?.CurrencyArray.CurrencyList?.Count is > 0)
                                currencyIcon = ListUtils.SettingsSiteList?.CurrencyArray.CurrencyList.FindIndex(a => a.Contains("BRL") || a.Contains("R$")).ToString();
                            else
                                currencyIcon = ListUtils.SettingsSiteList?.CurrencyArray.StringMap?.Count switch
                                {
                                    > 0 => ListUtils.SettingsSiteList?.CurrencyArray.StringMap?.FirstOrDefault(a => a.Value.Contains("BRL") || a.Value.Contains("R$")).Key,
                                    _ => currencyIcon
                                };

                            break;
                        case "INR":
                        case "₹":
                            if (ListUtils.SettingsSiteList?.CurrencyArray.CurrencyList?.Count is > 0)
                                currencyIcon = ListUtils.SettingsSiteList?.CurrencyArray.CurrencyList.FindIndex(a => a.Contains("INR") || a.Contains("₹")).ToString();
                            else
                                currencyIcon = ListUtils.SettingsSiteList?.CurrencyArray.StringMap?.Count switch
                                {
                                    > 0 => ListUtils.SettingsSiteList?.CurrencyArray.StringMap?.FirstOrDefault(a => a.Value.Contains("INR") || a.Value.Contains("₹")).Key,
                                    _ => currencyIcon
                                };

                            break;
                        default:

                            if (ListUtils.SettingsSiteList?.CurrencyArray.CurrencyList?.Count is > 0)
                                currencyIcon = ListUtils.SettingsSiteList?.CurrencyArray.CurrencyList.FindIndex(a => a.Contains(currency?.ToUpper())).ToString();
                            else if (ListUtils.SettingsSiteList?.CurrencyArray.StringMap?.Count > 0)
                            {
                                currencyIcon = ListUtils.SettingsSiteList?.CurrencyArray.StringMap?.FirstOrDefault(a => a.Value.Contains(currency?.ToUpper())).Key;
                            }
                            else
                            {
                                currencyIcon = "0";
                            }
                            break;
                    }

                    return currencyIcon;
                }

                return "0";
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return "0";
            }
        }

        public static (string, string) GetCurrency(string idCurrency)
        {
            try
            {
                switch (AppSettings.CurrencyStatic)
                {
                    case true:
                        return (AppSettings.CurrencyCodeStatic, AppSettings.CurrencyIconStatic);
                }

                string currency = AppSettings.CurrencyCodeStatic;
                bool success = int.TryParse(idCurrency, out var number);
                if (success)
                {
                    Console.WriteLine("Converted '{0}' to {1}.", idCurrency, number);
                    if (ListUtils.SettingsSiteList?.CurrencyArray.CurrencyList?.Count > 0 && ListUtils.SettingsSiteList?.CurrencyArray.CurrencyList?.Count > number)
                        currency = ListUtils.SettingsSiteList?.CurrencyArray.CurrencyList[number] ?? AppSettings.CurrencyCodeStatic;
                    else if (ListUtils.SettingsSiteList?.CurrencyArray.StringMap?.Count > 0)
                    {
                        currency = ListUtils.SettingsSiteList?.CurrencyArray.StringMap?.FirstOrDefault(a => a.Key == number.ToString()).Value ?? AppSettings.CurrencyCodeStatic;
                    }
                }
                else
                {
                    Console.WriteLine("Attempted conversion of '{0}' failed.", idCurrency ?? "<null>");
                    currency = idCurrency;
                }

                if (ListUtils.SettingsSiteList?.CurrencySymbolArray.CurrencyList != null)
                {
                    string currencyIcon = ListUtils.SettingsSiteList?.CurrencySymbolArray.CurrencyList.FirstOrDefault(a => a.Key == currency?.ToUpper()).Value ?? "";

                    return (currency, currencyIcon);
                }

                return (AppSettings.CurrencyCodeStatic, AppSettings.CurrencyIconStatic);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return (AppSettings.CurrencyCodeStatic, AppSettings.CurrencyIconStatic);
            }
        }

        public static List<string> GetCurrencySymbolList()
        {
            try
            {
                var arrayAdapter = new List<string>();

                if (AppSettings.CurrencyStatic)
                {
                    arrayAdapter.Add(AppSettings.CurrencyIconStatic);
                    return arrayAdapter;
                }

                if (ListUtils.SettingsSiteList?.CurrencySymbolArray.CurrencyList?.Count > 0)
                {
                    arrayAdapter.AddRange(from cur in ListUtils.SettingsSiteList?.CurrencySymbolArray.CurrencyList select cur.Value);
                }

                return arrayAdapter;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return new List<string>();
            }
        }

        public static void OpenProfile(Activity activity, string userId, UserDataObject item, string namePage)
        {
            try
            {
                if (userId != UserDetails.UserId)
                {
                    var intent = new Intent(activity, typeof(UserProfileActivity));
                    if (item != null) intent.PutExtra("UserObject", JsonConvert.SerializeObject(item));
                    intent.PutExtra("UserId", userId);
                    intent.PutExtra("NamePage", namePage);
                    activity.StartActivity(intent);
                }
                else
                {
                    var intent = new Intent(activity, typeof(MyProfileActivity));
                    activity.StartActivity(intent);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static void OpenProfile(Activity activity, string userId, UserDataObject item)
        {
            try
            {
                if (userId != UserDetails.UserId)
                {
                    if (UserProfileActivity.SUserId != userId)
                    {
                        if (item != null && item.IsBlocked)
                        {
                            var dialog = new MaterialAlertDialogBuilder(activity);
                            dialog.SetTitle(activity.GetText(Resource.String.Lbl_Warning));
                            dialog.SetMessage(activity.GetText(Resource.String.Lbl_TextBlockedUser_BoxOpenProfile));
                            dialog.SetPositiveButton(activity.GetText(Resource.String.Btn_UnBlock), (materialDialog, action) =>
                            {
                                try
                                {
                                    var intent = new Intent(activity, typeof(BlockedUsersActivity));
                                    activity.StartActivity(intent);
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });
                            dialog.SetNegativeButton(activity.GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                            dialog.Show();
                            return;
                        }

                        var intent = new Intent(activity, typeof(UserProfileActivity));
                        if (item != null)
                            intent.PutExtra("UserObject", JsonConvert.SerializeObject(item));
                        intent.PutExtra("UserId", userId);
                        activity.StartActivity(intent);
                    }
                    else
                    {
                        activity.Finish();
                    }
                }
                else
                {
                    if (PostClickListener.OpenMyProfile) return;

                    var intent = new Intent(activity, typeof(MyProfileActivity));
                    activity.StartActivity(intent);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static bool GetStatusOnline(int lastSeen, string isShowOnline)
        {
            try
            {
                string time = Methods.Time.TimeAgo(lastSeen, false);
                bool status = isShowOnline == "on" && time == Methods.Time.LblJustNow;
                return status;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;
            }
        }

        public static bool GetStatusAds()
        {
            try
            {
                switch (AppSettings.ShowAds)
                {
                    case ShowAds.AllUsers:
                        return true;
                    case ShowAds.UnProfessional:
                        {
                            var isPro = ListUtils.MyProfileList?.FirstOrDefault()?.IsPro ?? "0";
                            if (isPro == "0")
                                return true;
                            return false;
                        }
                    default:
                        return false;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;
            }
        }

        public static bool CanAddPost()
        {
            try
            {
                switch (AppSettings.AddPostSystem)
                {
                    case AddPostSystem.AllUsers:
                        return true;
                    case AddPostSystem.OnlyAdmin:
                        {
                            var isAdmin = ListUtils.MyProfileList?.FirstOrDefault()?.Admin ?? "0";
                            if (isAdmin == "0")
                                return false;
                            return true;
                        }
                    default:
                        return true;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return true;
            }
        }

        public static JobInfoObject ListFilterJobs(JobInfoObject jobInfoObject)
        {
            try
            {
                jobInfoObject.Image = jobInfoObject.Image.Contains(InitializeWoWonder.WebsiteUrl) switch
                {
                    false => GetTheFinalLink(jobInfoObject.Image),
                    _ => jobInfoObject.Image
                };

                jobInfoObject.IsOwner = jobInfoObject.UserId == UserDetails.UserId;

                return jobInfoObject;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return jobInfoObject;
            }
        }

        public static Dictionary<string, string> GetSalaryDateList(Activity activity)
        {
            try
            {
                var arrayAdapter = new Dictionary<string, string>
                {
                    {"per_hour", activity.GetString(Resource.String.Lbl_per_hour)},
                    {"per_day", activity.GetString(Resource.String.Lbl_per_day)},
                    {"per_week", activity.GetString(Resource.String.Lbl_per_week)},
                    {"per_month", activity.GetString(Resource.String.Lbl_per_month)},
                    {"per_year", activity.GetString(Resource.String.Lbl_per_year)}
                };

                return arrayAdapter;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return new Dictionary<string, string>();
            }
        }

        public static Dictionary<string, string> GetJobTypeList(Activity activity)
        {
            try
            {
                var arrayAdapter = new Dictionary<string, string>
                {
                    {"full_time", activity.GetString(Resource.String.Lbl_full_time)},
                    {"part_time", activity.GetString(Resource.String.Lbl_part_time)},
                    {"internship", activity.GetString(Resource.String.Lbl_internship)},
                    {"volunteer", activity.GetString(Resource.String.Lbl_volunteer)},
                    {"contract", activity.GetString(Resource.String.Lbl_contract)}
                };

                return arrayAdapter;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return new Dictionary<string, string>();
            }
        }

        public static Dictionary<string, string> GetAddQuestionList(Activity activity)
        {
            try
            {
                var arrayAdapter = new Dictionary<string, string>
                {
                    {"free_text_question", activity.GetString(Resource.String.Lbl_FreeTextQuestion)},
                    {"yes_no_question", activity.GetString(Resource.String.Lbl_YesNoQuestion)},
                    {"multiple_choice_question", activity.GetString(Resource.String.Lbl_MultipleChoiceQuestion)},
                };

                return arrayAdapter;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return new Dictionary<string, string>();
            }
        }

        public static Dictionary<string, string> GetAddDiscountList(Activity activity)
        {
            try
            {
                var arrayAdapter = new Dictionary<string, string>
                {
                    {"discount_percent", activity.GetString(Resource.String.Lbl_DiscountPercent)},
                    {"discount_amount", activity.GetString(Resource.String.Lbl_DiscountAmount)},
                    {"buy_get_discount", activity.GetString(Resource.String.Lbl_BuyGetDiscount)},
                    {"spend_get_off", activity.GetString(Resource.String.Lbl_SpendGetOff)},
                    {"free_shipping", activity.GetString(Resource.String.Lbl_FreeShipping)},
                };

                return arrayAdapter;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return new Dictionary<string, string>();
            }
        }

        public static Dictionary<string, string> GetCountryList(Activity activity)
        {
            try
            {
                var arrayAdapter = new Dictionary<string, string>
                {
                    {"1",  activity.GetString(Resource.String.Lbl_country1)},
                    {"2",  activity.GetString(Resource.String.Lbl_country2)},
                    {"3",  activity.GetString(Resource.String.Lbl_country3)},
                    {"4",  activity.GetString(Resource.String.Lbl_country4)},
                    {"5",  activity.GetString(Resource.String.Lbl_country5)},
                    {"6",  activity.GetString(Resource.String.Lbl_country6)},
                    {"7",  activity.GetString(Resource.String.Lbl_country7)},
                    {"8",  activity.GetString(Resource.String.Lbl_country8)},
                    {"9",  activity.GetString(Resource.String.Lbl_country9)},
                    {"10", activity.GetString(Resource.String.Lbl_country10)},
                    {"11", activity.GetString(Resource.String.Lbl_country11)},
                    {"12", activity.GetString(Resource.String.Lbl_country12)},
                    {"13", activity.GetString(Resource.String.Lbl_country13)},
                    {"14", activity.GetString(Resource.String.Lbl_country14)},
                    {"15", activity.GetString(Resource.String.Lbl_country15)},
                    {"16", activity.GetString(Resource.String.Lbl_country16)},
                    {"17", activity.GetString(Resource.String.Lbl_country17)},
                    {"18", activity.GetString(Resource.String.Lbl_country18)},
                    {"19", activity.GetString(Resource.String.Lbl_country19)},
                    {"20", activity.GetString(Resource.String.Lbl_country20)},
                    {"21", activity.GetString(Resource.String.Lbl_country21)},
                    {"22", activity.GetString(Resource.String.Lbl_country22)},
                    {"23", activity.GetString(Resource.String.Lbl_country23)},
                    {"24", activity.GetString(Resource.String.Lbl_country24)},
                    {"25", activity.GetString(Resource.String.Lbl_country25)},
                    {"26", activity.GetString(Resource.String.Lbl_country26)},
                    {"27", activity.GetString(Resource.String.Lbl_country27)},
                    {"28", activity.GetString(Resource.String.Lbl_country28)},
                    {"29", activity.GetString(Resource.String.Lbl_country29)},
                    {"30", activity.GetString(Resource.String.Lbl_country30)},
                    {"31", activity.GetString(Resource.String.Lbl_country31)},
                    {"32", activity.GetString(Resource.String.Lbl_country32)},
                    {"34", activity.GetString(Resource.String.Lbl_country34)},
                    {"35", activity.GetString(Resource.String.Lbl_country35)},
                    {"36", activity.GetString(Resource.String.Lbl_country36)},
                    {"37", activity.GetString(Resource.String.Lbl_country37)},
                    {"38", activity.GetString(Resource.String.Lbl_country38)},
                    {"39", activity.GetString(Resource.String.Lbl_country39)},
                    {"40", activity.GetString(Resource.String.Lbl_country40)},
                    {"41", activity.GetString(Resource.String.Lbl_country41)},
                    {"42", activity.GetString(Resource.String.Lbl_country42)},
                    {"43", activity.GetString(Resource.String.Lbl_country43)},
                    {"44", activity.GetString(Resource.String.Lbl_country44)},
                    {"45", activity.GetString(Resource.String.Lbl_country45)},
                    {"46", activity.GetString(Resource.String.Lbl_country46)},
                    {"47", activity.GetString(Resource.String.Lbl_country47)},
                    {"48", activity.GetString(Resource.String.Lbl_country48)},
                    {"49", activity.GetString(Resource.String.Lbl_country49)},
                    {"50", activity.GetString(Resource.String.Lbl_country50)},
                    {"51", activity.GetString(Resource.String.Lbl_country51)},
                    {"52", activity.GetString(Resource.String.Lbl_country52)},
                    {"53", activity.GetString(Resource.String.Lbl_country53)},
                    {"54", activity.GetString(Resource.String.Lbl_country54)},
                    {"55", activity.GetString(Resource.String.Lbl_country55)},
                    {"56", activity.GetString(Resource.String.Lbl_country56)},
                    {"57", activity.GetString(Resource.String.Lbl_country57)},
                    {"58", activity.GetString(Resource.String.Lbl_country58)},
                    {"59", activity.GetString(Resource.String.Lbl_country59)},
                    {"60", activity.GetString(Resource.String.Lbl_country60)},
                    {"61", activity.GetString(Resource.String.Lbl_country61)},
                    {"62", activity.GetString(Resource.String.Lbl_country62)},
                    {"63", activity.GetString(Resource.String.Lbl_country63)},
                    {"64", activity.GetString(Resource.String.Lbl_country64)},
                    {"65", activity.GetString(Resource.String.Lbl_country65)},
                    {"66", activity.GetString(Resource.String.Lbl_country66)},
                    {"67", activity.GetString(Resource.String.Lbl_country67)},
                    {"68", activity.GetString(Resource.String.Lbl_country68)},
                    {"69", activity.GetString(Resource.String.Lbl_country69)},
                    {"70", activity.GetString(Resource.String.Lbl_country70)},
                    {"71", activity.GetString(Resource.String.Lbl_country71)},
                    {"72", activity.GetString(Resource.String.Lbl_country72)},
                    {"73", activity.GetString(Resource.String.Lbl_country73)},
                    {"74", activity.GetString(Resource.String.Lbl_country74)},
                    {"75", activity.GetString(Resource.String.Lbl_country75)},
                    {"76", activity.GetString(Resource.String.Lbl_country76)},
                    {"77", activity.GetString(Resource.String.Lbl_country77)},
                    {"78", activity.GetString(Resource.String.Lbl_country78)},
                    {"79", activity.GetString(Resource.String.Lbl_country79)},
                    {"80", activity.GetString(Resource.String.Lbl_country80)},
                    {"81", activity.GetString(Resource.String.Lbl_country81)},
                    {"82", activity.GetString(Resource.String.Lbl_country82)},
                    {"83", activity.GetString(Resource.String.Lbl_country83)},
                    {"84", activity.GetString(Resource.String.Lbl_country84)},
                    {"85", activity.GetString(Resource.String.Lbl_country85)},
                    {"86", activity.GetString(Resource.String.Lbl_country86)},
                    {"87", activity.GetString(Resource.String.Lbl_country87)},
                    {"88", activity.GetString(Resource.String.Lbl_country88)},
                    {"89", activity.GetString(Resource.String.Lbl_country89)},
                    {"90", activity.GetString(Resource.String.Lbl_country90)},
                    {"91", activity.GetString(Resource.String.Lbl_country91)},
                    {"92", activity.GetString(Resource.String.Lbl_country92)},
                    {"93", activity.GetString(Resource.String.Lbl_country93)},
                    {"94", activity.GetString(Resource.String.Lbl_country94)},
                    {"95", activity.GetString(Resource.String.Lbl_country95)},
                    {"96", activity.GetString(Resource.String.Lbl_country96)},
                    {"97", activity.GetString(Resource.String.Lbl_country97)},
                    {"98", activity.GetString(Resource.String.Lbl_country98)},
                    {"99", activity.GetString(Resource.String.Lbl_country99)},
                    {"100",activity.GetString(Resource.String.Lbl_country100)},
                    {"101",activity.GetString(Resource.String.Lbl_country101)},
                    {"102",activity.GetString(Resource.String.Lbl_country102)},
                    {"103",activity.GetString(Resource.String.Lbl_country103)},
                    {"104",activity.GetString(Resource.String.Lbl_country104)},
                    {"105",activity.GetString(Resource.String.Lbl_country105)},
                    {"106",activity.GetString(Resource.String.Lbl_country106)},
                    {"107",activity.GetString(Resource.String.Lbl_country107)},
                    {"108",activity.GetString(Resource.String.Lbl_country108)},
                    {"109",activity.GetString(Resource.String.Lbl_country109)},
                    {"110",activity.GetString(Resource.String.Lbl_country110)},
                    {"111",activity.GetString(Resource.String.Lbl_country111)},
                    {"112",activity.GetString(Resource.String.Lbl_country112)},
                    {"113",activity.GetString(Resource.String.Lbl_country113)},
                    {"114",activity.GetString(Resource.String.Lbl_country114)},
                    {"115",activity.GetString(Resource.String.Lbl_country115)},
                    {"116",activity.GetString(Resource.String.Lbl_country116)},
                    {"117",activity.GetString(Resource.String.Lbl_country117)},
                    {"118",activity.GetString(Resource.String.Lbl_country118)},
                    {"119",activity.GetString(Resource.String.Lbl_country119)},
                    {"120",activity.GetString(Resource.String.Lbl_country120)},
                    {"121",activity.GetString(Resource.String.Lbl_country121)},
                    {"122",activity.GetString(Resource.String.Lbl_country122)},
                    {"123",activity.GetString(Resource.String.Lbl_country123)},
                    {"124",activity.GetString(Resource.String.Lbl_country124)},
                    {"125",activity.GetString(Resource.String.Lbl_country125)},
                    {"126",activity.GetString(Resource.String.Lbl_country126)},
                    {"127",activity.GetString(Resource.String.Lbl_country127)},
                    {"128",activity.GetString(Resource.String.Lbl_country128)},
                    {"129",activity.GetString(Resource.String.Lbl_country129)},
                    {"130",activity.GetString(Resource.String.Lbl_country130)},
                    {"131",activity.GetString(Resource.String.Lbl_country131)},
                    {"132",activity.GetString(Resource.String.Lbl_country132)},
                    {"133",activity.GetString(Resource.String.Lbl_country133)},
                    {"134",activity.GetString(Resource.String.Lbl_country134)},
                    {"135",activity.GetString(Resource.String.Lbl_country135)},
                    {"136",activity.GetString(Resource.String.Lbl_country136)},
                    {"137",activity.GetString(Resource.String.Lbl_country137)},
                    {"138",activity.GetString(Resource.String.Lbl_country138)},
                    {"139",activity.GetString(Resource.String.Lbl_country139)},
                    {"140",activity.GetString(Resource.String.Lbl_country140)},
                    {"141",activity.GetString(Resource.String.Lbl_country141)},
                    {"142",activity.GetString(Resource.String.Lbl_country142)},
                    {"143",activity.GetString(Resource.String.Lbl_country143)},
                    {"144",activity.GetString(Resource.String.Lbl_country144)},
                    {"145",activity.GetString(Resource.String.Lbl_country145)},
                    {"146",activity.GetString(Resource.String.Lbl_country146)},
                    {"147",activity.GetString(Resource.String.Lbl_country147)},
                    {"148",activity.GetString(Resource.String.Lbl_country148)},
                    {"149",activity.GetString(Resource.String.Lbl_country149)},
                    {"150",activity.GetString(Resource.String.Lbl_country150)},
                    {"151",activity.GetString(Resource.String.Lbl_country151)},
                    {"152",activity.GetString(Resource.String.Lbl_country152)},
                    {"153",activity.GetString(Resource.String.Lbl_country153)},
                    {"154",activity.GetString(Resource.String.Lbl_country154)},
                    {"155",activity.GetString(Resource.String.Lbl_country155)},
                    {"156",activity.GetString(Resource.String.Lbl_country156)},
                    {"157",activity.GetString(Resource.String.Lbl_country157)},
                    {"158",activity.GetString(Resource.String.Lbl_country158)},
                    {"159",activity.GetString(Resource.String.Lbl_country159)},
                    {"160",activity.GetString(Resource.String.Lbl_country160)},
                    {"161",activity.GetString(Resource.String.Lbl_country161)},
                    {"162",activity.GetString(Resource.String.Lbl_country162)},
                    {"163",activity.GetString(Resource.String.Lbl_country163)},
                    {"164",activity.GetString(Resource.String.Lbl_country164)},
                    {"165",activity.GetString(Resource.String.Lbl_country165)},
                    {"166",activity.GetString(Resource.String.Lbl_country166)},
                    {"167",activity.GetString(Resource.String.Lbl_country167)},
                    {"168",activity.GetString(Resource.String.Lbl_country168)},
                    {"169",activity.GetString(Resource.String.Lbl_country169)},
                    {"170",activity.GetString(Resource.String.Lbl_country170)},
                    {"171",activity.GetString(Resource.String.Lbl_country171)},
                    {"172",activity.GetString(Resource.String.Lbl_country172)},
                    {"173",activity.GetString(Resource.String.Lbl_country173)},
                    {"174",activity.GetString(Resource.String.Lbl_country174)},
                    {"175",activity.GetString(Resource.String.Lbl_country175)},
                    {"176",activity.GetString(Resource.String.Lbl_country176)},
                    {"177",activity.GetString(Resource.String.Lbl_country177)},
                    {"178",activity.GetString(Resource.String.Lbl_country178)},
                    {"179",activity.GetString(Resource.String.Lbl_country179)},
                    {"180",activity.GetString(Resource.String.Lbl_country180)},
                    {"181",activity.GetString(Resource.String.Lbl_country181)},
                    {"182",activity.GetString(Resource.String.Lbl_country182)},
                    {"183",activity.GetString(Resource.String.Lbl_country183)},
                    {"184",activity.GetString(Resource.String.Lbl_country184)},
                    {"185",activity.GetString(Resource.String.Lbl_country185)},
                    {"186",activity.GetString(Resource.String.Lbl_country186)},
                    {"187",activity.GetString(Resource.String.Lbl_country187)},
                    {"188",activity.GetString(Resource.String.Lbl_country188)},
                    {"189",activity.GetString(Resource.String.Lbl_country189)},
                    {"190",activity.GetString(Resource.String.Lbl_country190)},
                    {"191",activity.GetString(Resource.String.Lbl_country191)},
                    {"192",activity.GetString(Resource.String.Lbl_country192)},
                    {"193",activity.GetString(Resource.String.Lbl_country193)},
                    {"194",activity.GetString(Resource.String.Lbl_country194)},
                    {"195",activity.GetString(Resource.String.Lbl_country195)},
                    {"196",activity.GetString(Resource.String.Lbl_country196)},
                    {"197",activity.GetString(Resource.String.Lbl_country197)},
                    {"198",activity.GetString(Resource.String.Lbl_country198)},
                    {"199",activity.GetString(Resource.String.Lbl_country199)},
                    {"200",activity.GetString(Resource.String.Lbl_country200)},
                    {"201",activity.GetString(Resource.String.Lbl_country201)},
                    {"202",activity.GetString(Resource.String.Lbl_country202)},
                    {"203",activity.GetString(Resource.String.Lbl_country203)},
                    {"204",activity.GetString(Resource.String.Lbl_country204)},
                    {"205",activity.GetString(Resource.String.Lbl_country205)},
                    {"206",activity.GetString(Resource.String.Lbl_country206)},
                    {"207",activity.GetString(Resource.String.Lbl_country207)},
                    {"208",activity.GetString(Resource.String.Lbl_country208)},
                    {"209",activity.GetString(Resource.String.Lbl_country209)},
                    {"210",activity.GetString(Resource.String.Lbl_country210)},
                    {"211",activity.GetString(Resource.String.Lbl_country211)},
                    {"212",activity.GetString(Resource.String.Lbl_country212)},
                    {"213",activity.GetString(Resource.String.Lbl_country213)},
                    {"214",activity.GetString(Resource.String.Lbl_country214)},
                    {"215",activity.GetString(Resource.String.Lbl_country215)},
                    {"216",activity.GetString(Resource.String.Lbl_country216)},
                    {"217",activity.GetString(Resource.String.Lbl_country217)},
                    {"218",activity.GetString(Resource.String.Lbl_country218)},
                    {"219",activity.GetString(Resource.String.Lbl_country219)},
                    {"220",activity.GetString(Resource.String.Lbl_country220)},
                    {"221",activity.GetString(Resource.String.Lbl_country221)},
                    {"222",activity.GetString(Resource.String.Lbl_country222)},
                    {"223",activity.GetString(Resource.String.Lbl_country223)},
                    {"224",activity.GetString(Resource.String.Lbl_country224)},
                    {"225",activity.GetString(Resource.String.Lbl_country225)},
                    {"226",activity.GetString(Resource.String.Lbl_country226)},
                    {"227",activity.GetString(Resource.String.Lbl_country227)},
                    {"228",activity.GetString(Resource.String.Lbl_country228)},
                    {"229",activity.GetString(Resource.String.Lbl_country229)},
                    {"230",activity.GetString(Resource.String.Lbl_country230)},
                    {"231",activity.GetString(Resource.String.Lbl_country231)},
                    {"232",activity.GetString(Resource.String.Lbl_country232)},
                    {"233",activity.GetString(Resource.String.Lbl_country233)},
                    {"238",activity.GetString(Resource.String.Lbl_country238)},
                    {"239",activity.GetString(Resource.String.Lbl_country239)},
                    {"240",activity.GetString(Resource.String.Lbl_country240)},
                    {"241",activity.GetString(Resource.String.Lbl_country241)},
                    {"242",activity.GetString(Resource.String.Lbl_country242)},
                };

                return arrayAdapter;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return new Dictionary<string, string>();
            }
        }

        public static bool CheckAllowedFileUpload()
        {
            try
            {
                var dataSettings = ListUtils.SettingsSiteList;
                if (dataSettings?.WhoUpload == "pro") //just pro user can chat 
                {
                    var dataUser = ListUtils.MyProfileList?.FirstOrDefault()?.IsPro;
                    if (dataUser == "0") // Not Pro remove call
                    {
                        return false;
                    }
                }
                else  //"all"
                {
                    return true;
                }

                return true;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return true;
            }
        }

        public static bool CheckAllowedFileSharingInServer(string type)
        {
            try
            {
                switch (type)
                {
                    case "File" when !string.IsNullOrEmpty(ListUtils.SettingsSiteList?.FileSharing) && ListUtils.SettingsSiteList?.FileSharing == "1":
                    // Allowed
                    case "Video" when !string.IsNullOrEmpty(ListUtils.SettingsSiteList?.VideoUpload) && ListUtils.SettingsSiteList?.VideoUpload == "1":
                    // Allowed
                    case "Audio" when !string.IsNullOrEmpty(ListUtils.SettingsSiteList?.AudioUpload) && ListUtils.SettingsSiteList?.AudioUpload == "1":
                    // Allowed
                    case "Image":
                        // Allowed
                        return true;
                    default:
                        return false;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;
            }
        }

        public static async Task<(bool, string)> CheckMimeTypesWithServer(string path)
        {
            try
            {
                var allowedExtenstionStatic = "jpg,png,jpeg,gif,mp4,m4v,webm,flv,mov,mpeg,mp3,wav";
                if (!string.IsNullOrEmpty(path))
                {
                    var fileName = path.Split('/').Last();
                    var fileNameWithExtension = fileName.Split('.').Last();

                    if (!string.IsNullOrEmpty(ListUtils.SettingsSiteList?.MimeTypes))
                    {
                        var allowedExtenstion = ListUtils.SettingsSiteList?.AllowedExtenstion; //jpg,png,jpeg,gif,mkv,docx,zip,rar,pdf,doc,mp3,mp4,flv,wav,txt,mov,avi,webm,wav,mpeg
                        var mimeTypes = ListUtils.SettingsSiteList?.MimeTypes; //video/mp4,video/mov,video/mpeg,video/flv,video/avi,video/webm,audio/wav,audio/mpeg,video/quicktime,audio/mp3,image/png,image/jpeg,image/gif,application/pdf,application/msword,application/zip,application/x-rar-compressed,text/pdf,application/x-pointplus,text/css

                        var getMimeType = MimeTypeMap.GetMimeType(fileNameWithExtension);

                        if (allowedExtenstion.Contains(fileNameWithExtension) && mimeTypes.Contains(getMimeType))
                        {
                            var type = Methods.AttachmentFiles.Check_FileExtension(path);

                            var check = CheckAllowedFileSharingInServer(type);
                            if (check)
                            {
                                if (!string.IsNullOrEmpty(ListUtils.SettingsSiteList?.VisionApiKey) && ListUtils.SettingsSiteList?.AdultImages == "1")
                                {
                                    byte[] dataDate = await File.ReadAllBytesAsync(path);

                                    var data = await GoogleVision.CheckVisionImage(dataDate);
                                    if (!string.IsNullOrEmpty(data?.Error?.Message) || data?.Responses[0]?.SafeSearchAnnotation?.Adult is "LIKELY" or "VERY_LIKELY")
                                    {
                                        //"This photo contains sensitive content which some people may find offensive or disturbing"
                                        return (false, "AdultImages");
                                    }
                                }

                                if (ListUtils.SettingsSiteList?.FfmpegSystem == "on" & type == "Video")
                                {
                                    var allowedffmpegExtenstion = ListUtils.SettingsSiteList?.AllowedffmpegExtenstion; //mov,mp4,m4a,3gp,3g2,mj2,asf,avi,flv,webm,m4v,mpeg,mpeg,mpeg,ogv,mkv,webm,mov
                                    if (allowedffmpegExtenstion.Contains(fileNameWithExtension))
                                    {
                                        return (true, "");
                                    }
                                }
                                else
                                {
                                    return (true, "");
                                }
                            }
                        }
                    }

                    //just this Allowed : >> jpg,png,jpeg,gif,mp4,m4v,webm,flv,mov,mpeg,mp3,wav
                    if (allowedExtenstionStatic.Contains(fileNameWithExtension))
                        return (true, "");
                }

                return (false, "");
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return (false, "");
            }
        }

        // Functions Save Images
        private static async void SaveFile(string id, string folder, string fileName, string url)
        {
            try
            {
                if (url.Contains("http"))
                {
                    string folderDestination = folder + id + "/";

                    string filePath = Path.Combine(folderDestination);
                    string mediaFile = filePath + "/" + fileName;

                    if (File.Exists(mediaFile)) return;

                    HttpClient client;
                    if (AppSettings.TurnSecurityProtocolType3072On)
                    {
                        HttpClientHandler clientHandler = new HttpClientHandler();
                        clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                        //clientHandler.SslProtocols = SslProtocols.Tls | SslProtocols.Ssl2 | SslProtocols.Ssl3 | SslProtocols.Tls11 | SslProtocols.Tls12 | SslProtocols.Tls13 | SslProtocols.Default;

                        // Pass the handler to httpClient(from you are calling api)
                        client = new HttpClient(clientHandler);
                    }
                    else
                    {
                        client = new HttpClient();
                    }

                    var s = await client.GetStreamAsync(new Uri(url));
                    if (s.CanRead)
                    {
                        if (File.Exists(mediaFile)) return;
                        await using FileStream fs = new FileStream(mediaFile, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
                        await s.CopyToAsync(fs);
                    }

                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        // Functions file from folder
        public static string GetFile(string id, string folder, string filename, string url)
        {
            try
            {
                string folderDestination = folder + id + "/";

                if (!Directory.Exists(folderDestination))
                {
                    if (Directory.Exists(Methods.Path.FolderDiskStory))
                        Directory.Delete(Methods.Path.FolderDiskStory, true);

                    Directory.CreateDirectory(folderDestination);
                }

                string imageFile = Methods.MultiMedia.GetMediaFrom_Gallery(folderDestination, filename);
                switch (imageFile)
                {
                    case "File Dont Exists":
                        //This code runs on a new thread, control is returned to the caller on the UI thread.
                        Task.Factory.StartNew(() => { SaveFile(id, folder, filename, url); });
                        return url;
                    default:
                        return imageFile;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return url;
            }
        }

        public static string GetDuration(string mediaFile)
        {
            if (string.IsNullOrEmpty(mediaFile))
                return "0";

            try
            {
                var type = Methods.AttachmentFiles.Check_FileExtension(mediaFile);
                if (type == "Audio" || type == "Video")
                {
                    string duration;
                    MediaMetadataRetriever retriever;
                    if (mediaFile.Contains("http"))
                    {
                        retriever = new MediaMetadataRetriever();
                        switch ((int)Build.VERSION.SdkInt)
                        {
                            case >= 14:
                                retriever.SetDataSource(mediaFile, new Dictionary<string, string>());
                                break;
                            default:
                                retriever.SetDataSource(mediaFile);
                                break;
                        }

                        duration = retriever.ExtractMetadata(MetadataKey.Duration); //time In Millisec 
                        retriever.Release();
                    }
                    else
                    {
                        var file = Android.Net.Uri.FromFile(new Java.IO.File(mediaFile));
                        retriever = new MediaMetadataRetriever();
                        //if ((int)Build.VERSION.SdkInt >= 14)
                        //    retriever.SetDataSource(file.Path, new Dictionary<string, string>());
                        //else
                        retriever.SetDataSource(file?.Path);

                        duration = retriever.ExtractMetadata(MetadataKey.Duration); //time In Millisec 
                        retriever.Release();
                    }
                    return duration;
                }

                return "0";
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return "0";
            }
        }

        private static readonly string[] RelationshipLocal = Application.Context.Resources?.GetStringArray(Resource.Array.RelationShipArray);
        public static string GetRelationship(int index)
        {
            try
            {
                switch (index)
                {
                    case > -1:
                        {
                            string name = RelationshipLocal[index];
                            return name;
                        }
                    default:
                        return RelationshipLocal?.First() ?? "";
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return "";
            }
        }

        public static bool IsValidPassword(string password)
        {
            try
            {
                bool flag;
                switch (password.Length)
                {
                    case >= 8 when password.Any(char.IsUpper) && password.Any(char.IsLower) && password.Any(char.IsNumber) && password.Any(char.IsSymbol):
                        Console.WriteLine("valid");
                        flag = true;
                        break;
                    default:
                        Console.WriteLine("invalid");
                        flag = false;
                        break;
                }

                return flag;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;
            }
        }

        public static bool IsLikedPage(PageDataObject pageData)
        {
            try
            {
                switch (string.IsNullOrEmpty(pageData?.IsLiked.String))
                {
                    case false:
                        switch (pageData.IsLiked.String.ToLower())
                        {
                            case "no":
                                return false;
                            case "yes":
                                return true;
                        }

                        break;
                }

                return pageData?.IsLiked.Bool != null && pageData.IsLiked.Bool.Value;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;
            }
        }

        public static string IsJoinedGroup(GroupDataObject groupData)
        {
            try
            {
                //(0 => not joined , 1 =>joined , 2 =>  requested) 
                if (groupData.IsGroupJoined != null)
                {
                    return groupData.IsGroupJoined.Value.ToString();
                }

                if (!string.IsNullOrEmpty(groupData?.IsJoined.String))
                {
                    if (groupData.IsJoined.String.ToLower() == "no")
                        return "0";
                    if (groupData.IsJoined.String.ToLower() == "yes")
                        return "1";
                    return groupData.IsJoined.String;
                }

                if (groupData?.IsJoined.Bool != null && groupData.IsJoined.Bool.Value)
                {
                    return "1";
                }

                return "0";
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return "0";
            }
        }

        /// <summary>
        /// ['amazone_s3'] == 1   >> $wo['config']['s3_site_url'] . '/' . $media;
        /// ['spaces'] == 1       >> 'https://' . $wo['config']['space_name'] . '.' . $wo['config']['space_region'] . '.digitaloceanspaces.com/' . $media;
        /// ['ftp_upload'] == 1   >> "http://".$wo['config']['ftp_endpoint'] . '/' . $media;
        /// ['cloud_upload'] == 1 >> "'https://storage.cloud.google.com/'. $wo['config']['cloud_bucket_name'] . '/' . $media;
        /// </summary>
        /// <param name="media"></param>
        /// <returns></returns>
        public static string GetTheFinalLink(string media)
        {
            try
            {
                if (media.Contains("http"))
                    return media;

                var path = media;
                var config = ListUtils.SettingsSiteList;
                switch (string.IsNullOrEmpty(config?.AmazoneS3))
                {
                    case false when config?.AmazoneS3 == "1":
                        path = config.S3SiteUrl + "/" + media;
                        return path;
                }

                switch (string.IsNullOrEmpty(config?.Spaces))
                {
                    case false when config?.Spaces == "1":
                        path = "https://" + config.SpaceName + "." + config.SpaceRegion + ".digitaloceanspaces.com/" + media;
                        return path;
                }

                switch (string.IsNullOrEmpty(config?.FtpUpload))
                {
                    case false when config?.FtpUpload == "1":
                        path = "http://" + config.FtpEndpoint + "/" + media;
                        return path;
                }

                switch (string.IsNullOrEmpty(config?.CloudUpload))
                {
                    case false when config?.CloudUpload == "1":
                        path = "https://storage.cloud.google.com/" + config.BucketName + "/" + media;
                        return path;
                }

                path = media.Contains(InitializeWoWonder.WebsiteUrl) switch
                {
                    false => InitializeWoWonder.WebsiteUrl + "/" + media,
                    _ => path
                };

                return path;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return media;
            }
        }

        public static void SetAddFriendReels(Activity activity, UserDataObject item, ImageView btnAddUser)
        {
            try
            {
                item.IsFollowing = item.IsFollowing switch
                {
                    null => "0",
                    _ => item.IsFollowing
                };

                var dbDatabase = new SqLiteDatabase();
                switch (item.IsFollowing)
                {
                    case "0": // Add Or request friends
                    case "no":
                    case "No":
                        {
                            if (item.ConfirmFollowers == "1" || AppSettings.ConnectivitySystem == 0)
                            {
                                item.IsFollowing = "2";
                                btnAddUser.Tag = "request";

                                dbDatabase.Insert_Or_Replace_OR_Delete_UsersContact(item, "Update");
                            }
                            else
                            {
                                item.IsFollowing = "1";
                                btnAddUser.Tag = "friends";

                                dbDatabase.Insert_Or_Replace_OR_Delete_UsersContact(item, "Insert");
                            }

                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.FollowUserAsync(item.UserId) });

                            btnAddUser.Visibility = ViewStates.Invisible;
                            ToastUtils.ShowToast(activity, activity.GetString(Resource.String.Lbl_YouAreNowFollowing) + " " + GetNameFinal(item), ToastLength.Short);

                        }
                        break;
                    case "1": // Remove friends
                    case "yes":
                    case "Yes":
                        {
                            var dialog = new MaterialAlertDialogBuilder(activity);
                            dialog.SetMessage(activity.GetText(Resource.String.Lbl_confirmationUnFriend));
                            dialog.SetPositiveButton(activity.GetText(Resource.String.Lbl_Confirm), (materialDialog, action) =>
                            {
                                try
                                {
                                    item.IsFollowing = "0";
                                    btnAddUser.Tag = "Add";

                                    dbDatabase.Insert_Or_Replace_OR_Delete_UsersContact(item, "Delete");

                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.FollowUserAsync(item.UserId) });
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });
                            dialog.SetNegativeButton(activity.GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                            dialog.Show();
                        }
                        break;
                    case "2": // Remove request friends 
                        {
                            var dialog = new MaterialAlertDialogBuilder(activity);
                            dialog.SetMessage(activity.GetText(Resource.String.Lbl_confirmationUnFriend));
                            dialog.SetPositiveButton(activity.GetText(Resource.String.Lbl_Confirm), (materialDialog, action) =>
                            {
                                try
                                {
                                    item.IsFollowing = "0";
                                    btnAddUser.Tag = "Add";

                                    dbDatabase = new SqLiteDatabase();
                                    dbDatabase.Insert_Or_Replace_OR_Delete_UsersContact(item, "Delete");

                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.FollowUserAsync(item.UserId) });
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });
                            dialog.SetNegativeButton(activity.GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                            dialog.Show();
                        }

                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static void SetAddFriend(Activity activity, UserDataObject item, AppCompatButton btnAddUser, bool reels = false)
        {
            try
            {
                item.IsFollowing = item.IsFollowing switch
                {
                    null => "0",
                    _ => item.IsFollowing
                };

                var dbDatabase = new SqLiteDatabase();
                string isFollowing;
                switch (item.IsFollowing)
                {
                    case "0": // Add Or request friends
                    case "no":
                    case "No":
                        {
                            if (item.ConfirmFollowers == "1" || AppSettings.ConnectivitySystem == 0)
                            {
                                item.IsFollowing = isFollowing = "2";
                                btnAddUser.Tag = "request";

                                dbDatabase.Insert_Or_Replace_OR_Delete_UsersContact(item, "Update");
                            }
                            else
                            {
                                item.IsFollowing = isFollowing = "1";
                                btnAddUser.Tag = "friends";

                                dbDatabase.Insert_Or_Replace_OR_Delete_UsersContact(item, "Insert");
                            }

                            SetAddFriendCondition(item, isFollowing, btnAddUser, reels);

                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.FollowUserAsync(item.UserId) });

                            if (reels)
                            {
                                btnAddUser.Visibility = ViewStates.Invisible;
                                ToastUtils.ShowToast(activity, activity.GetString(Resource.String.Lbl_YouAreNowFollowing) + " " + GetNameFinal(item), ToastLength.Short);
                            }
                        }
                        break;
                    case "1": // Remove friends
                    case "yes":
                    case "Yes":
                        {
                            var dialog = new MaterialAlertDialogBuilder(activity);
                            dialog.SetMessage(activity.GetText(Resource.String.Lbl_confirmationUnFriend));
                            dialog.SetPositiveButton(activity.GetText(Resource.String.Lbl_Confirm), (materialDialog, action) =>
                            {
                                try
                                {
                                    item.IsFollowing = isFollowing = "0";
                                    btnAddUser.Tag = "Add";

                                    dbDatabase.Insert_Or_Replace_OR_Delete_UsersContact(item, "Delete");

                                    SetAddFriendCondition(item, isFollowing, btnAddUser, reels);

                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.FollowUserAsync(item.UserId) });
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });
                            dialog.SetNegativeButton(activity.GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                            dialog.Show();
                        }
                        break;
                    case "2": // Remove request friends 
                        {
                            var dialog = new MaterialAlertDialogBuilder(activity);
                            dialog.SetMessage(activity.GetText(Resource.String.Lbl_confirmationUnFriend));
                            dialog.SetPositiveButton(activity.GetText(Resource.String.Lbl_Confirm), (materialDialog, action) =>
                            {
                                try
                                {
                                    item.IsFollowing = isFollowing = "0";
                                    btnAddUser.Tag = "Add";

                                    dbDatabase = new SqLiteDatabase();
                                    dbDatabase.Insert_Or_Replace_OR_Delete_UsersContact(item, "Delete");

                                    SetAddFriendCondition(item, isFollowing, btnAddUser, reels);

                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.FollowUserAsync(item.UserId) });
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });
                            dialog.SetNegativeButton(activity.GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());

                            dialog.Show();
                        }

                        break;
                }

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static void SetAddFriendCondition(UserDataObject userData, string isFollowing, AppCompatButton btnAddUser, bool reels = false)
        {
            try
            {
                if (userData.CanFollow == "0" && isFollowing == "0" && userData.UserId != UserDetails.UserId)
                {
                    btnAddUser.Visibility = ViewStates.Gone;
                }

                if (userData.FollowPrivacy == "0") // Everyone
                {
                    btnAddUser.Visibility = ViewStates.Visible;
                }
                else if (userData.FollowPrivacy == "1") // People i Follow
                {
                    if (userData.IsFollowingMe == "0")
                    {
                        btnAddUser.Visibility = isFollowing == "0" ? ViewStates.Gone : ViewStates.Visible;
                    }
                    else if (userData.IsFollowingMe == "1")
                    {
                        btnAddUser.Visibility = ViewStates.Visible;
                    }
                }
                else
                    btnAddUser.Visibility = ViewStates.Visible;

                if (!reels)
                {
                    switch (isFollowing)
                    {
                        //>> Not Friend
                        case "0":
                            btnAddUser.SetTextColor(Color.White);
                            btnAddUser.Text = Application.Context.GetText(AppSettings.ConnectivitySystem == 1 ? Resource.String.Lbl_Follow : Resource.String.Lbl_AddFriends);
                            btnAddUser.SetBackgroundResource(Resource.Drawable.round_button_pressed);
                            btnAddUser.Tag = "Add";
                            break;
                        //>> Friend
                        case "1":
                            btnAddUser.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                            btnAddUser.Text = Application.Context.GetText(AppSettings.ConnectivitySystem == 1 ? Resource.String.Lbl_Following : Resource.String.Lbl_Friends);
                            btnAddUser.SetBackgroundResource(Resource.Drawable.round_button_outline);
                            btnAddUser.Tag = "friends";
                            break;
                        //>> Request
                        case "2":
                            btnAddUser.SetTextColor(Color.ParseColor("#444444"));
                            btnAddUser.Text = Application.Context.GetText(Resource.String.Lbl_Requested);
                            btnAddUser.SetBackgroundResource(Resource.Drawable.round_button_outline);
                            btnAddUser.Tag = "request";
                            break;
                        default:
                            btnAddUser.SetTextColor(Color.White);
                            btnAddUser.Text = Application.Context.GetText(AppSettings.ConnectivitySystem == 1 ? Resource.String.Lbl_Follow : Resource.String.Lbl_AddFriends);
                            btnAddUser.SetBackgroundResource(Resource.Drawable.round_button_pressed);
                            btnAddUser.Tag = "Add";
                            break;
                    }
                }
                else
                {
                    switch (isFollowing)
                    {
                        //>> Not Friend
                        case "0":
                            btnAddUser.Visibility = ViewStates.Visible;
                            btnAddUser.Text = Application.Context.GetText(AppSettings.ConnectivitySystem == 1 ? Resource.String.Lbl_Follow : Resource.String.Lbl_AddFriends);
                            btnAddUser.Tag = "Add";
                            break;
                        //>> Friend
                        case "1":
                            btnAddUser.Visibility = ViewStates.Invisible;
                            btnAddUser.Tag = "friends";
                            break;
                        //>> Request
                        case "2":
                            btnAddUser.Visibility = ViewStates.Visible;
                            btnAddUser.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                            btnAddUser.Text = Application.Context.GetText(Resource.String.Lbl_Requested);
                            btnAddUser.Tag = "request";
                            break;
                        default:
                            btnAddUser.Visibility = ViewStates.Visible;
                            btnAddUser.Text = Application.Context.GetText(AppSettings.ConnectivitySystem == 1 ? Resource.String.Lbl_Follow : Resource.String.Lbl_AddFriends);
                            btnAddUser.Tag = "Add";
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static async void SetJoinGroup(Activity activity, string groupId, AppCompatButton button)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(activity, activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    return;
                }

                var (apiStatus, respond) = await RequestsAsync.Group.JoinGroupAsync(groupId);
                switch (apiStatus)
                {
                    case 200:
                        {
                            switch (respond)
                            {
                                case JoinGroupObject { JoinStatus: "requested" } result:
                                    button.SetTextColor(Color.ParseColor("#444444"));
                                    button.Text = Application.Context.GetText(Resource.String.Lbl_Requested);
                                    button.SetBackgroundResource(Resource.Drawable.round_button_outline);
                                    break;
                                case JoinGroupObject result:
                                    {
                                        var isJoined = result.JoinStatus == "left" ? "false" : "true";
                                        button.Text = activity.GetText(isJoined == "true" ? Resource.String.Btn_Joined : Resource.String.Btn_Join_Group);

                                        switch (isJoined)
                                        {
                                            case "true":
                                                button.SetBackgroundResource(Resource.Drawable.round_button_outline);
                                                button.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                                                break;
                                            default:
                                                button.SetBackgroundResource(Resource.Drawable.round_button_pressed);
                                                button.SetTextColor(Color.White);
                                                break;
                                        }

                                        break;
                                    }
                            }

                            break;
                        }
                    default:
                        Methods.DisplayReportResult(activity, respond);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static async void SetJoinGroup2(Activity activity, string groupId, AppCompatButton button)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(activity, activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    return;
                }

                var (apiStatus, respond) = await RequestsAsync.Group.JoinGroupAsync(groupId);
                switch (apiStatus)
                {
                    case 200:
                        {
                            switch (respond)
                            {
                                case JoinGroupObject { JoinStatus: "requested" } result:
                                    button.SetTextColor(Color.ParseColor("#444444"));
                                    button.Text = Application.Context.GetText(Resource.String.Lbl_Requested);
                                    button.SetBackgroundResource(Resource.Drawable.round_button_normal);
                                    break;
                                case JoinGroupObject result:
                                    {
                                        var isJoined = result.JoinStatus == "left" ? "false" : "true";
                                        button.Text = activity.GetText(isJoined == "true" ? Resource.String.Btn_Joined : Resource.String.Btn_Join_Group);

                                        switch (isJoined)
                                        {
                                            case "true":
                                                button.SetBackgroundResource(Resource.Drawable.round_button_normal);
                                                button.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                                                break;
                                            default:
                                                button.SetBackgroundResource(Resource.Drawable.round_button_pressed);
                                                button.SetTextColor(Color.White);
                                                break;
                                        }

                                        break;
                                    }
                            }

                            break;
                        }
                    default:
                        Methods.DisplayReportResult(activity, respond);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static void SetLikePage(Activity activity, string pageId, AppCompatButton button)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(activity, activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    return;
                }

                switch (button?.Tag?.ToString())
                {
                    case "false":
                        button.SetBackgroundResource(Resource.Drawable.round_button_outline);
                        button.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                        button.Text = activity.GetText(Resource.String.Btn_Unlike);
                        button.Tag = "true";
                        break;
                    default:
                        button.SetBackgroundResource(Resource.Drawable.round_button_pressed);
                        button.SetTextColor(Color.White);
                        button.Text = activity.GetText(Resource.String.Btn_Like);
                        button.Tag = "false";
                        break;
                }

                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Page.LikePageAsync(pageId) });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static void ChangeMenuIconColor(IMenu menu, Color color)
        {
            try
            {
                for (int i = 0; i < menu.Size(); i++)
                {
                    var drawable = menu.GetItem(i)?.Icon;
                    switch (drawable)
                    {
                        case null:
                            continue;
                        default:
                            drawable.Mutate();
                            drawable.SetColorFilter(new PorterDuffColorFilter(color, PorterDuff.Mode.SrcAtop));
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static string GetAgeUser(string Birthday)
        {
            try
            {
                if (!string.IsNullOrEmpty(Birthday) && Birthday != "0000-00-00" && Birthday != "00-00-0000" && Birthday != "0")
                {
                    //06-05-1994 
                    var units = Birthday.Split('-');

                    var year = Convert.ToInt32(units[0]);
                    if (units[1][0] == '0')
                        units[1] = units[1][1].ToString();

                    var month = Convert.ToInt32(units[1]);

                    if (units[2][0] == '0')
                        units[2] = units[2][1].ToString();

                    var day = Convert.ToInt32(units[2]);

                    DateTime now = DateTime.Now;
                    if (year.ToString().Length == 4)
                    {
                        DateTime birthday = new DateTime(year, month, day);
                        int age = now.Year - birthday.Year;
                        if (now < birthday.AddYears(age)) age--;

                        return age.ToString();
                    }
                    else
                    {
                        DateTime birthday = new DateTime(day, month, year);
                        int age = now.Year - birthday.Year;
                        if (now < birthday.AddYears(age)) age--;
                        return age.ToString();
                    }
                }
                return "";
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return "";
            }
        }

        public static async Task<LatLng> GetLocationFromAddress(Context mainContext, string strAddress)
        {
            if (string.IsNullOrEmpty(strAddress))
                return null!;

#pragma warning disable 618
            var locale = (int)Build.VERSION.SdkInt < 25 ? mainContext?.Resources?.Configuration?.Locale : mainContext?.Resources?.Configuration?.Locales.Get(0) ?? mainContext?.Resources?.Configuration?.Locale;
#pragma warning restore 618
            Geocoder coder = new Geocoder(mainContext, locale);

            try
            {
                var address = await coder?.GetFromLocationNameAsync(strAddress, 2);
                if (address?.Count <= 0) return null!;

                Address location = address?.FirstOrDefault();
                if (location != null)
                {
                    var lat = location.Latitude;
                    var lng = location.Longitude;

                    LatLng p1 = new LatLng(lat, lng);

                    return p1;
                }
                return null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        public static bool ChatIsAllowed(UserDataObject dataObject)
        {
            try
            {
                if (dataObject.MessagePrivacy == "2") //No_body
                {
                    ToastUtils.ShowToast(Application.Context, Application.Context.GetString(Resource.String.Lbl_ChatNotAllowed), ToastLength.Short);
                    return false;
                }

                if (dataObject.MessagePrivacy == "1") //People_i_Follow
                {
                    if (dataObject.IsFollowing == "1")
                    {
                        return true;
                    }

                    ToastUtils.ShowToast(Application.Context, Application.Context.GetString(Resource.String.Lbl_ChatNotAllowed), ToastLength.Short);
                    return false;
                }

                if (dataObject.MessagePrivacy == "0") //Everyone
                {
                    return true;
                }

                return true;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return true;

            }
        }

        public static bool StoryIsAvailable(string userId)
        {
            try
            {
                var tab = TabbedMainActivity.GetInstance()?.NewsFeedTab;
                var checkSection = tab?.PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.Story);
                if (checkSection != null)
                {
                    var check = checkSection.StoryList.FirstOrDefault(a => a.UserId == userId)?.Stories;
                    if (check?.Count > 0)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;

            }
        }

        public static bool IsTabDark()
        {
            try
            {
                return AppSettings.SetTabDarkTheme is TabTheme.Dark;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;

            }
        }

        public static string GetDefaultAvatar()
        {
            try
            {
                var local = ListUtils.MyProfileList?.FirstOrDefault();
                if (local != null)
                {
                    if (local.Gender == Application.Context.GetText(Resource.String.Radio_Male))
                    {
                        return "no_profile_image";
                    }

                    if (local.Gender == Application.Context.GetText(Resource.String.Radio_Female))
                    {
                        return "no_profile_female_image";
                    }
                }

                return "no_profile_image";
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return "no_profile_image";
            }
        }

        public static bool CheckWallet(long price)
        {
            try
            {
                var wallet = ListUtils.MyProfileList.FirstOrDefault()?.Wallet ?? "0";
                if (!string.IsNullOrEmpty(wallet) && wallet != "0")
                {
                    bool isParable = double.TryParse(wallet, out var number);
                    if (isParable)
                    {
                        if (number >= price)
                        {
                            return true;
                        }

                        return false;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;
            }
        }
    }
}