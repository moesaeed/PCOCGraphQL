using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace DF2023.Core.Extensions
{
    public static class StringExtensions
    {
        public static string SetFirstLetterLowercase(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            return char.ToLower(input[0]) + input.Substring(1);
        }

        public static bool IsValidEmail(this string email)
        {
            string emailRegex = @"^([\w\.\-]+)@([\w\-]+)\.([a-zA-Z]{2,6})$";
            return Regex.IsMatch(email, emailRegex);
        }
    }
}