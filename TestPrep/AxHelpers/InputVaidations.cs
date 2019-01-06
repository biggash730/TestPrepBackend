using System;
using System.Linq;
using hubtelapi_dotnet_v1.Hubtel;
using RestSharp.Extensions;

namespace TestPrep.AxHelpers
{
    public class InputVaidations
    {
        public static bool IsValidName(string name)
        {
            name = name.Trim();
            if (string.IsNullOrWhiteSpace(name)) return false;
            //if (StringExtensions.IsEmpty(name)) return false;
            if (name.IsAlphanumeric()) return false;
            return !name.IsNumeric();
        }

        public static void CheckRandomCharacters(string strng, string type)
        {
            var chars = SetupConfig.Setting.RandomCharacters.ToCharArray();
            var xchars = chars.Where(c => strng.Contains(c.ToString())).ToList();

            if (xchars.Any()) throw new Exception($"{type} cannot contain any of the following characters, {string.Join(",",xchars)}."); 
        }
    }
}