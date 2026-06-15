using Android.Content;
using Android.Util;
using System;

namespace WoWonder.Helpers.Utils
{
    public static class PixelUtil
    {
        public static int SpToPx(Context context, int dp)
        {
            return (int)TypedValue.ApplyDimension(ComplexUnitType.Px, dp, context.Resources.DisplayMetrics);
        }

        public static int PxToSp(Context context, int px)
        {
            return (int)TypedValue.ApplyDimension(ComplexUnitType.Sp, px, context.Resources.DisplayMetrics);
        }

        public static int DpToPx(Context context, int dp)
        {
            int px = (int)Math.Round(dp * GetPixelScaleFactor(context));
            return px;
        }

        public static int PxToDp(Context context, int px)
        {
            int dp = (int)Math.Round(px / GetPixelScaleFactor(context));
            return dp;
        }

        private static float GetPixelScaleFactor(Context context)
        {
            DisplayMetrics displayMetrics = context.Resources.DisplayMetrics;
            return displayMetrics.Xdpi / (int)DisplayMetricsDensity.Default;
        }

        public static int Dp(Context context, float value)
        {
            //wael test 
            var test1 = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, value, context.Resources.DisplayMetrics);

            var density = context.Resources.DisplayMetrics.ScaledDensity;
            var test2 = (int)Math.Ceiling(density * value);

            return test2;
        }
    }
}