using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Ecommerce.Prestashop
{
    /// <summary>
    /// Helper class with simple utilities
    /// </summary>
    public class Helpers
    {
        /// <summary>
        /// Returns a canonicalized, escaped string of key=value pairs from a Dictionary of parameters
        /// </summary>
        /// <param name="options">A Dictionary of options ('filter', 'display' etc)</param>
        /// <returns>A canonicalized escaped string of the parameters</returns>
        public static string Canonicalize(Dictionary<string, string> options)
        {
            var builder = new StringBuilder();

            foreach (var option in options)
            {
                if (builder.Length > 0)
                {
                    builder.Append("&");
                }

                builder.AppendFormat("{0}={1}", option.Key, HttpUtility.UrlEncode(option.Value));
            }

            return builder.ToString();
        }

        /// <summary>
        /// Returns a valid link rewrite
        /// </summary>
        /// <param name="text">A string that will be turned into a valid link rewrite</param>
        /// <returns>A valid link rewrite</returns>
        public static string BuildLinkRewrite(string text)
        {
            var funnyChars = new string[] 
            { 
                ",", ".", ":", ";", "!", "´", "%", "$", "£", "€", "@", "&", "?", "/", @"\", "\"", "\'", "#", "<", ">", "(", ")", "[", "]", "«", "»", "®", "™" 
            };

            string link = text.ToLower();

            foreach (var fc in funnyChars)
            {
                link = link.Replace(fc, "");
            }

            link = link.Replace(' ', '-');
            link = link.Replace("æ", "ae");
            link = link.Replace("ø", "oe");
            link = link.Replace("å", "aa");
            link = link.Replace("ü", "u");
            link = link.Replace("ö", "o");
            link = link.Replace('+', '_');

            return link.Trim();
        }
    }
}
