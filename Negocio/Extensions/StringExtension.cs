using System;
using System.Collections.Generic;
using System.Text;

namespace Negocio.Extentions
{
    public static class StringExtension
    {
        public static decimal ToDecimalUSCulture(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return decimal.Zero;

            return Convert.ToDecimal(value, new System.Globalization.CultureInfo("en-US"));
        }
    }
}
