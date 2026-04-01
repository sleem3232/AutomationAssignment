using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AutomationAssignment.Utils
{
    public static class TextUtils
    {
        public static string Normalize(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            text = text.ToLowerInvariant();
            text = Regex.Replace(text, @"[^\w\s]", " ");
            text = Regex.Replace(text, @"\s+", " ").Trim();

            return text;
        }

        public static int CountUnique(string text)
        {
            return Normalize(text)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Distinct()
                .Count();
        }
    }
}
