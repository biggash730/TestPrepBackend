using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Web;
using TestPrep.Models;

namespace TestPrep.AxHelpers
{
    public class StringHelpers
    {
        public static string GetSubstringBetween(string value, string a, string b)
        {
            var posA = value.IndexOf(a);
            var posB = value.LastIndexOf(b);
            if (posA == -1)
            {
                return "";
            }
            if (posB == -1)
            {
                return "";
            }
            int adjustedPosA = posA + a.Length;
            if (adjustedPosA >= posB)
            {
                return "";
            }
            return value.Substring(adjustedPosA, posB - adjustedPosA);
        }
        public static string GenerateRandomString(int length)
        {
            var stringBuilder = new StringBuilder(length);
            var chArray = "abcdefghijklmnopqrstuvwxyz0123456789_-".ToCharArray();
            var random = new Random((int)DateTime.Now.Ticks);
            for (var index = 0; index < length; ++index)
                stringBuilder.Append(chArray[random.Next(chArray.Length)]);
            return stringBuilder.ToString().ToLower();
        }
        public static string GenerateRandomNumber(int length)
        {
            var stringBuilder = new StringBuilder(length);
            var chArray = "0123456789".ToCharArray();
            var random = new Random((int)DateTime.Now.Ticks);
            for (var index = 0; index < length; ++index)
                stringBuilder.Append(chArray[random.Next(chArray.Length)]);
            return stringBuilder.ToString();
        }

        public static Settings ReadSettngs()
        {
            var filePath = HttpContext.Current.Server.MapPath("~/Content/config/config.json");
            using (var reader = new StreamReader(filePath))
            {
                var settings = reader.ReadToEnd();
                var values = JsonConvert.DeserializeObject<Settings>(settings);
                return values;
            }
        }

        public static
            DateTime DecodeTimeToken(string token)
        {
            byte[] data = Convert.FromBase64String(token);
            DateTime when = DateTime.FromBinary(BitConverter.ToInt64(data, 0));
            if (when < DateTime.UtcNow.AddHours(-24))
            {
                // too old
            }
            return when;
        }
    }
}