using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

namespace SpotifyNet
{
    internal static class Extension
    {
        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }

        public static Uri AddParameter(this Uri uri, string key, object value)
        {
            var builder = new UriBuilder(uri);
            var query = HttpUtility.ParseQueryString(builder.Query);

            query.Add(key, value.ToString());
            builder.Query = query.ToString();
            uri = builder.Uri;

            return uri;
        }

        public static IEnumerable<string> GetDescriptions(this Enum enumeration)
        {
            foreach (var item in enumeration.GetFlags())
                yield return item.GetAttributeValue<DescriptionAttribute, string>(x => x.Description);
        }

        public static Expected GetAttributeValue<T, Expected>(this Enum enumeration, Func<T, Expected> expression) where T : Attribute
        {
            T attribute =
              enumeration
                .GetType()
                .GetMember(enumeration.ToString())
                .Where(member => member.MemberType == MemberTypes.Field)
                .FirstOrDefault()
                .GetCustomAttributes(typeof(T), false)
                .Cast<T>()
                .FirstOrDefault();

            if (attribute == null)
                return default;

            return expression(attribute);
        }

        public static IEnumerable<Enum> GetFlags(this Enum enumeration)
        {
            return Enum.GetValues(enumeration.GetType()).Cast<Enum>().Where(x => !Equals((uint)(object)x, 0u) && enumeration.HasFlag(x));
        }

        public static string Base64Decode(this string base64EncodedData, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;

            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return encoding.GetString(base64EncodedBytes);
        }
    }
}
