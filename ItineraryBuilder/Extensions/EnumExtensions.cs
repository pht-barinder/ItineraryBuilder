using System;
using System.Collections.Generic;

namespace ItineraryBuilder.Util
{
    public static class ExtensionMethods
    {

        public static string UtilGetEnumAsString(this Enum obj, bool isGetFull = true)
        {
            try
            {
                var result = obj.GetType().GetField(obj.ToString()).GetCustomAttributes(typeof(UtilEnumStringValueAttribute), false);
                if (result != null && result.Length > 0)
                {
                    if (isGetFull)
                        return (result[0] as UtilEnumStringValueAttribute).LargeDescription;
                    else
                        return (result[0] as UtilEnumStringValueAttribute).SmallDescription;
                }
                else
                    return obj.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string UtilGetEnumAsString(this Enum obj, string key)
        {
            try
            {
                var result = obj.GetType().GetField(obj.ToString()).GetCustomAttributes(typeof(UtilEnumStringValueAttribute), false);
                if (result != null && result.Length > 0)
                {
                    var item = (result[0] as UtilEnumStringValueAttribute);
                    if (item.Key.Equals(key))
                        return item.KeyValue;
                }
                return obj.ToString();
            }
            catch { return string.Empty; }
        }

        

        public static string UtilGetEnumShortString<T>(this Enum obj)
        {
            return (Enum.Parse(typeof(T), obj.ToString()) as Enum).UtilGetEnumAsString(false);
        }

        public static string UtilGetEnumLongString<T>(this Enum obj)
        {
            return (Enum.Parse(typeof(T), obj.ToString()) as Enum).UtilGetEnumAsString(true);
        }

        public static T ParseToEnum<T>(this object val)
        {
            return (T)Enum.Parse(typeof(T), val.ToString());
        }
    }
}
