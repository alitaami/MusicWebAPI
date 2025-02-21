using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Common
{
   public static class DateConvertor
    {
        public static string ToShamsi(this DateTime value)
        {
            PersianCalendar p = new PersianCalendar();
            return p.GetYear(value) + "-" + p.GetMonth(value).ToString("00") + "-" + p.GetDayOfMonth(value).ToString("00");
             
        }
    }
}
