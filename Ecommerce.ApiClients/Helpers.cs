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
    }
}
