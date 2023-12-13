using System;
using System.Collections.Generic;
using System.Text;

namespace Negocio.Extentions
{
    public static class DateTimeExtension
    {
        public static string ToDisplay(this DateTime dateTime)
        {
            if (dateTime == null)
                return "";

            return dateTime.ToString("dd MMM yy ddd HH:mm");
        }
    }
}
