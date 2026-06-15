using Android.BillingClient.Api;
using System.Collections.Generic;

namespace WoWonder.PaymentGoogle
{
    public static class InAppBillingGoogle
    {
        public const string Donation5 = "donation_5";
        public const string Donation10 = "donation_10";
        public const string Donation15 = "donation_15";
        public const string Donation20 = "donation_20";
        public const string Donation25 = "donation_25";
        public const string Donation30 = "donation_30";
        public const string Donation35 = "donation_35";
        public const string Donation40 = "donation_40";
        public const string Donation45 = "donation_45";
        public const string Donation50 = "donation_50";
        public const string Donation55 = "donation_55";
        public const string Donation60 = "donation_60";
        public const string Donation65 = "donation_65";
        public const string Donation70 = "donation_70";
        public const string Donation75 = "donation_75";
        public const string DonationDefault = "donation_defulte";
        public const string MembershipStar = "upgrade_vmembership_star";
        public const string MembershipHot = "upgrade_membership_hot";
        public const string MembershipUltima = "upgrade_membership_ultima";
        public const string MembershipVip = "upgrade_membership_vip";

        public static readonly List<QueryProductDetailsParams.Product> ListProductSku = new List<QueryProductDetailsParams.Product> // ID Product
        {
            //All products should be of the same product type.
            QueryProductDetailsParams.Product.NewBuilder().SetProductId(Donation5).SetProductType(BillingClient.IProductType.Subs).Build(),
            QueryProductDetailsParams.Product.NewBuilder().SetProductId(Donation10).SetProductType(BillingClient.IProductType.Subs).Build(),
            QueryProductDetailsParams.Product.NewBuilder().SetProductId(Donation15).SetProductType(BillingClient.IProductType.Subs).Build(),
            QueryProductDetailsParams.Product.NewBuilder().SetProductId(Donation20).SetProductType(BillingClient.IProductType.Subs).Build(),
            QueryProductDetailsParams.Product.NewBuilder().SetProductId(Donation25).SetProductType(BillingClient.IProductType.Subs).Build(),
            QueryProductDetailsParams.Product.NewBuilder().SetProductId(Donation30).SetProductType(BillingClient.IProductType.Subs).Build(),
            QueryProductDetailsParams.Product.NewBuilder().SetProductId(Donation35).SetProductType(BillingClient.IProductType.Subs).Build(),
            QueryProductDetailsParams.Product.NewBuilder().SetProductId(Donation40).SetProductType(BillingClient.IProductType.Subs).Build(),
            QueryProductDetailsParams.Product.NewBuilder().SetProductId(Donation45).SetProductType(BillingClient.IProductType.Subs).Build(),
            QueryProductDetailsParams.Product.NewBuilder().SetProductId(Donation50).SetProductType(BillingClient.IProductType.Subs).Build(),
            QueryProductDetailsParams.Product.NewBuilder().SetProductId(Donation55).SetProductType(BillingClient.IProductType.Subs).Build(),
            QueryProductDetailsParams.Product.NewBuilder().SetProductId(Donation60).SetProductType(BillingClient.IProductType.Subs).Build(),
            QueryProductDetailsParams.Product.NewBuilder().SetProductId(Donation65).SetProductType(BillingClient.IProductType.Subs).Build(),
            QueryProductDetailsParams.Product.NewBuilder().SetProductId(Donation70).SetProductType(BillingClient.IProductType.Subs).Build(),
            QueryProductDetailsParams.Product.NewBuilder().SetProductId(Donation75).SetProductType(BillingClient.IProductType.Subs).Build(),
            QueryProductDetailsParams.Product.NewBuilder().SetProductId(DonationDefault).SetProductType(BillingClient.IProductType.Subs).Build(),
            QueryProductDetailsParams.Product.NewBuilder().SetProductId(MembershipStar).SetProductType(BillingClient.IProductType.Subs).Build(),
            QueryProductDetailsParams.Product.NewBuilder().SetProductId(MembershipHot).SetProductType(BillingClient.IProductType.Subs).Build(),
            QueryProductDetailsParams.Product.NewBuilder().SetProductId(MembershipUltima).SetProductType(BillingClient.IProductType.Subs).Build(),
            QueryProductDetailsParams.Product.NewBuilder().SetProductId(MembershipVip).SetProductType(BillingClient.IProductType.Subs).Build(),
        };
    }
}