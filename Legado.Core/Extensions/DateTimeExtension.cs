using System;
using System.Collections.Generic;
using System.Text;

namespace Legado.Core.Extensions;

public static class DateTimeExtension
{
    public static DateTime ToDateTimeUseMillseconds(this long unixTimeStamp)
    { 
        return TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).AddMilliseconds(unixTimeStamp);
    }

    public static DateTime ToDateTimeUseSeconds(this long unixTimeStamp)
    {
        return TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).AddSeconds(unixTimeStamp);
    }

    public static long ToTimeStamp(this DateTime dt)
    {
        DateTime dateTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
        return (long)(dt - dateTime).TotalMilliseconds;
    }

    public static long ToTimeStampSecond(this DateTime dt)
    {
        DateTime dateTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
        return (long)(dt - dateTime).TotalSeconds;
    }

    public static string ToDateTimeString(this DateTime dateTime, bool isRemoveSecond = false)
    {
        if (isRemoveSecond)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm");
        }

        return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
    }

    public static string ToDateTimeString(this DateTime? dateTime, bool isRemoveSecond = false)
    {
        if (!dateTime.HasValue)
        {
            return string.Empty;
        }

        return dateTime.Value.ToDateTimeString(isRemoveSecond);
    }

    public static string ToDateString(this DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-dd");
    }

    public static string ToDateString()
    {
        return DateTime.Now.ToString("yyyy-MM-dd");
    }

    public static string ToDateString(this DateTime? dateTime)
    {
        if (!dateTime.HasValue)
        {
            return string.Empty;
        }

        return dateTime.Value.ToDateString();
    }

    public static string ToTimeString(this DateTime dateTime)
    {
        return dateTime.ToString("HH:mm:ss");
    }

    public static string ToTimeString(this DateTime? dateTime)
    {
        if (!dateTime.HasValue)
        {
            return string.Empty;
        }

        return dateTime.Value.ToTimeString();
    }

    public static string ToMillisecondString(this DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
    }

    public static string ToMillisecondString(this DateTime? dateTime)
    {
        if (!dateTime.HasValue)
        {
            return string.Empty;
        }

        return dateTime.Value.ToMillisecondString();
    }

    public static string ToChineseDateString(this DateTime dateTime)
    {
        return $"{dateTime.Year}年{dateTime.Month}月{dateTime.Day}日";
    }
     

    public static string ToChineseDateTimeString(this DateTime dateTime, bool isRemoveSecond = false)
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendFormat("{0}年{1}月{2}日", dateTime.Year, dateTime.Month, dateTime.Day);
        stringBuilder.AppendFormat(" {0}时{1}分", dateTime.Hour, dateTime.Minute);
        if (!isRemoveSecond)
        {
            stringBuilder.AppendFormat("{0}秒", dateTime.Second);
        }

        return stringBuilder.ToString();
    }

    public static string ToChineseDateTimeString(this DateTime? dateTime, bool isRemoveSecond = false)
    {
        if (!dateTime.HasValue)
        {
            return string.Empty;
        }

        return dateTime.Value.ToChineseDateTimeString(isRemoveSecond);
    }
}