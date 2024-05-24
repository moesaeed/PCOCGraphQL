using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace DF2023.Core.Helpers
{
    public static class StringHelper
    {
        public static string SetValidUrlName(string source)
        {
            source = Regex.Replace(source.ToLower(), @"[^\w\-\!\$\'\(\)\=\@\d_]+", "-");
            return source.Length > 210 ? source.ToString().Substring(0, 210) : source;
        }
        public static string UpperFirstLetter(string word)
        {
            return word[0].ToString().ToUpper() + word.Substring(1);
        }
        public static string LowerFirstLetter(string word)
        {
            return word[0].ToString().ToLower() + word.Substring(1);
        }
    }
}