using Android.App;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;

namespace WoWonder.Helpers.Controller
{
    public class CategoriesController
    {
        public static ObservableCollection<Classes.Categories> ListCategoriesPage = new ObservableCollection<Classes.Categories>();
        public static ObservableCollection<Classes.Categories> ListCategoriesGroup = new ObservableCollection<Classes.Categories>();
        public static ObservableCollection<Classes.Categories> ListCategoriesBlog = new ObservableCollection<Classes.Categories>();
        public static ObservableCollection<Classes.Categories> ListCategoriesProducts = new ObservableCollection<Classes.Categories>();
        public static ObservableCollection<Classes.Categories> ListCategoriesJob = new ObservableCollection<Classes.Categories>();
        public static ObservableCollection<Classes.Categories> ListCategoriesMovies = new ObservableCollection<Classes.Categories>();

        public string Get_Translate_Categories_Communities(string idCategory, string textCategory, string type)
        {
            try
            {
                string categoryName = textCategory;

                switch (type)
                {
                    case "Page":
                        {
                            categoryName = ListCategoriesPage?.Count switch
                            {
                                > 0 => ListCategoriesPage.FirstOrDefault(a => a.CategoriesId == idCategory)
                                    ?.CategoriesName ?? textCategory,
                                _ => categoryName
                            };

                            break;
                        }
                    case "Group":
                        {
                            categoryName = ListCategoriesGroup?.Count switch
                            {
                                > 0 => ListCategoriesGroup.FirstOrDefault(a => a.CategoriesId == idCategory)
                                    ?.CategoriesName ?? textCategory,
                                _ => categoryName
                            };

                            break;
                        }
                    case "Blog":
                        {
                            categoryName = ListCategoriesBlog?.Count switch
                            {
                                > 0 => ListCategoriesBlog.FirstOrDefault(a => a.CategoriesId == idCategory)
                                    ?.CategoriesName ?? textCategory,
                                _ => categoryName
                            };

                            break;
                        }
                    case "Products":
                        {
                            categoryName = ListCategoriesProducts?.Count switch
                            {
                                > 0 => ListCategoriesProducts.FirstOrDefault(a => a.CategoriesId == idCategory)
                                    ?.CategoriesName ?? textCategory,
                                _ => categoryName
                            };

                            break;
                        }
                    case "Job":
                        {
                            categoryName = ListCategoriesJob?.Count switch
                            {
                                > 0 => ListCategoriesJob.FirstOrDefault(a => a.CategoriesId == idCategory)
                                    ?.CategoriesName ?? textCategory,
                                _ => categoryName
                            };

                            break;
                        }
                    case "Movies":
                        {
                            categoryName = ListCategoriesMovies?.Count switch
                            {
                                > 0 => ListCategoriesMovies.FirstOrDefault(a => a.CategoriesId == idCategory)
                                    ?.CategoriesName ?? textCategory,
                                _ => categoryName
                            };

                            break;
                        }
                    default:
                        categoryName = Application.Context.GetText(Resource.String.Lbl_Unknown);
                        break;
                }

                if (string.IsNullOrEmpty(categoryName))
                    return Application.Context.GetText(Resource.String.Lbl_Unknown);

                return categoryName;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

                if (string.IsNullOrEmpty(textCategory))
                    return Application.Context.GetText(Resource.String.Lbl_Unknown);

                return textCategory;
            }
        }

        public static void SetListCategories(GetSiteSettingsObject.ConfigObject result)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    //Page Categories
                    var listPage = result.PageCategories?.Select(cat => new Classes.Categories
                    {
                        CategoriesId = cat.Key,
                        CategoriesName = Methods.FunString.DecodeString(cat.Value),
                        CategoriesColor = "#ffffff",
                        SubList = new List<SubCategories>()
                    }).ToList();

                    ListCategoriesPage.Clear();
                    if (listPage?.Count > 0)
                        ListCategoriesPage = new ObservableCollection<Classes.Categories>(listPage);

                    if (result.PageSubCategories?.SubCategoriesList?.Count > 0)
                        //Sub Categories Page
                        foreach (var sub in result.PageSubCategories?.SubCategoriesList)
                        {
                            var subCategories = result.PageSubCategories?.SubCategoriesList?.FirstOrDefault(a => a.Key == sub.Key).Value;
                            if (subCategories?.Count > 0)
                            {
                                var cat = ListCategoriesPage.FirstOrDefault(a => a.CategoriesId == sub.Key);
                                if (cat != null)
                                {
                                    foreach (var pairs in subCategories)
                                    {
                                        cat.SubList.Add(pairs);
                                    }
                                }
                            }
                        }

                    //Group Categories
                    var listGroup = result.GroupCategories?.Select(cat => new Classes.Categories
                    {
                        CategoriesId = cat.Key,
                        CategoriesName = Methods.FunString.DecodeString(cat.Value),
                        CategoriesColor = "#ffffff",
                        SubList = new List<SubCategories>()
                    }).ToList();

                    ListCategoriesGroup.Clear();
                    if (listGroup?.Count > 0)
                        ListCategoriesGroup = new ObservableCollection<Classes.Categories>(listGroup);

                    if (result.GroupSubCategories?.SubCategoriesList?.Count > 0)
                        //Sub Categories Group
                        foreach (var sub in result.GroupSubCategories?.SubCategoriesList)
                        {
                            var subCategories = result.GroupSubCategories?.SubCategoriesList?.FirstOrDefault(a => a.Key == sub.Key).Value;
                            if (subCategories?.Count > 0)
                            {
                                var cat = ListCategoriesGroup.FirstOrDefault(a => a.CategoriesId == sub.Key);
                                if (cat != null)
                                {
                                    foreach (var pairs in subCategories)
                                    {
                                        cat.SubList.Add(pairs);
                                    }
                                }
                            }
                        }

                    if (ListCategoriesGroup.Count == 0 && ListCategoriesPage.Count > 0)
                        ListCategoriesGroup = new ObservableCollection<Classes.Categories>(ListCategoriesPage);

                    //Blog Categories
                    var listBlog = result.BlogCategories?.Select(cat => new Classes.Categories
                    {
                        CategoriesId = cat.Key,
                        CategoriesName = Methods.FunString.DecodeString(cat.Value),
                        CategoriesColor = "#ffffff",
                        SubList = new List<SubCategories>()
                    }).ToList();

                    ListCategoriesBlog.Clear();
                    if (listBlog?.Count > 0)
                        ListCategoriesBlog = new ObservableCollection<Classes.Categories>(listBlog);

                    //Products Categories
                    var listProducts = result.ProductsCategories?.Select(cat => new Classes.Categories
                    {
                        CategoriesId = cat.Key,
                        CategoriesName = Methods.FunString.DecodeString(cat.Value),
                        CategoriesColor = "#ffffff",
                        SubList = new List<SubCategories>()
                    }).ToList();

                    ListCategoriesProducts.Clear();
                    if (listProducts?.Count > 0)
                        ListCategoriesProducts = new ObservableCollection<Classes.Categories>(listProducts);

                    if (result.ProductsSubCategories?.SubCategoriesList?.Count > 0)
                        //Sub Categories Products
                        foreach (var sub in result.ProductsSubCategories?.SubCategoriesList)
                        {
                            var subCategories = result.ProductsSubCategories?.SubCategoriesList?.FirstOrDefault(a => a.Key == sub.Key).Value;
                            if (subCategories?.Count > 0)
                            {
                                var cat = ListCategoriesProducts.FirstOrDefault(a => a.CategoriesId == sub.Key);
                                if (cat != null)
                                {
                                    foreach (var pairs in subCategories)
                                    {
                                        cat.SubList.Add(pairs);
                                    }
                                }
                            }
                        }

                    //Job Categories
                    var listJob = result.JobCategories?.Select(cat => new Classes.Categories
                    {
                        CategoriesId = cat.Key,
                        CategoriesName = Methods.FunString.DecodeString(cat.Value),
                        CategoriesColor = "#ffffff",
                        SubList = new List<SubCategories>()
                    }).ToList();

                    ListCategoriesJob.Clear();
                    if (listJob?.Count > 0)
                        ListCategoriesJob = new ObservableCollection<Classes.Categories>(listJob);

                    //Family
                    var listFamily = result.Family?.Select(cat => new Classes.Family
                    {
                        FamilyId = cat.Key,
                        FamilyName = Methods.FunString.DecodeString(cat.Value),
                    }).ToList();

                    ListUtils.FamilyList.Clear();
                    if (listFamily?.Count > 0)
                        ListUtils.FamilyList = new ObservableCollection<Classes.Family>(listFamily);

                    //Movie Category
                    var listMovie = result.MovieCategory?.Select(cat => new Classes.Categories
                    {
                        CategoriesId = cat.Key,
                        CategoriesName = Methods.FunString.DecodeString(cat.Value),
                        CategoriesColor = "#ffffff",
                        SubList = new List<SubCategories>()
                    }).ToList();

                    ListCategoriesMovies.Clear();
                    if (listMovie?.Count > 0)
                        ListCategoriesMovies = new ObservableCollection<Classes.Categories>(listMovie);

                    if (AppSettings.SetApisReportMode)
                    {
                        if (ListCategoriesPage.Count == 0)
                            Console.WriteLine("ReportMode >> Message: List Categories Page Not Found, Please check api get_site_settings");

                        if (ListCategoriesGroup.Count == 0)
                            Console.WriteLine("ReportMode >> Message: List Categories Group Not Found, Please check api get_site_settings");

                        if (ListCategoriesProducts.Count == 0)
                            Console.WriteLine("ReportMode >> Message: List Categories Products Not Found, Please check api get_site_settings");

                        if (ListCategoriesMovies.Count == 0)
                            Console.WriteLine("ReportMode >> Message: List Categories Movies Not Found, Please check api get_site_settings");

                        if (ListUtils.FamilyList.Count == 0)
                            Console.WriteLine("ReportMode >> Message: Family List Not Found, Please check api get_site_settings");

                        if (AppSettings.SetApisReportMode && AppSettings.ShowColor)
                            if (ListUtils.SettingsSiteList?.PostColors != null && ListUtils.SettingsSiteList?.PostColors.Value.PostColorsList != null && ListUtils.SettingsSiteList?.PostColors.Value.PostColorsList.Count == 0)
                            {
                                Console.WriteLine("PostColors Not Found, Please check api get_site_settings ");
                            }
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            });
        }

        public static void ResetListCategories()
        {
            try
            {
                ListCategoriesBlog.Clear();
                ListCategoriesGroup.Clear();
                ListCategoriesJob.Clear();
                ListCategoriesMovies.Clear();
                ListCategoriesPage.Clear();
                ListCategoriesProducts.Clear();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}