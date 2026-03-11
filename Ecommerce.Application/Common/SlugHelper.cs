using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ecommerce.Application.Common
{
    public static class SlugHelper
    {
        public static string GenerateSlug(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            var str = input.ToLowerInvariant().Trim();
            str = str.Replace(" ", "-");
            str = Regex.Replace(str, @"[^a-z0-9\-]", "");
            str = Regex.Replace(str, @"-{2,}", "-");
            str = str.Trim('-');

            return str;
        }
    }
}