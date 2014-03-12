using System;
using System.Text.RegularExpressions;

namespace Ecommerce.Prestashop
{
    public static class TypeTester
    {
        #region "Specific value types"
        public static bool IsBirthDate(string value)
        {
            return Regex.IsMatch(value, @"/^([0-9]{4})-((0?[1-9])|(1[0-2]))-((0?[1-9])|([1-2][0-9])|(3[01]))( [0-9]{2}:[0-9]{2}:[0-9]{2})?$/");
        }

        public static bool IsColor(string value)
        {
            return Regex.IsMatch(value, @"/^(#[0-9a-fA-F]{6}|[a-zA-Z0-9-]*)$/");
        }

        public static bool IsEmail(string value)
        {
            return Regex.IsMatch(value, @"/^[a-z0-9!#$%&\'*+\/=?^`{}|~_-]+[.a-z0-9!#$%&\'*+\/=?^`{}|~_-]*@[a-z0-9]+[._a-z0-9-]*\.[a-z0-9]+$/ui");
        }

        public static bool IsImageSize(string value)
        {
            return Regex.IsMatch(value, @"/^[0-9]{1,4}$/");
        }

        public static bool IsLanguageCode(string value)
        {
            return Regex.IsMatch(value, @"/^[a-zA-Z]{2}(-[a-zA-Z]{2})?$/");
        }

        public static bool IsLanguageIsoCode(string value)
        {
            return Regex.IsMatch(value, @"/^[a-zA-Z]{2,3}$/");
        }

        public static bool IsMd5(string value)
        {
            return Regex.IsMatch(value, @"/^[a-f0-9A-F]{32}$/");
        }

        public static bool IsNumericIsoCode(string value)
        {
            return Regex.IsMatch(value, @"/^[0-9]{2,3}$/");
        }

        public static bool IsPasswd(string value)
        {
            return Regex.IsMatch(value, @"/^[.a-zA-Z_0-9-!@#$%\^&*()]{5,32}$/");
        }

        public static bool IsPasswdAdmin(string value)
        {
            return Regex.IsMatch(value, @"/^[.a-zA-Z_0-9-!@#$%\^&*()]{8,32}$/");
        }

        public static bool IsPhpDateFormat(string value)
        {
            return Regex.IsMatch(value, @"/^[^<>]+$/");
        }

        public static bool IsReference(string value)
        {
            return Regex.IsMatch(value, @"/^[^<>;={}]*$/u");
        }

        public static bool IsUrl(string value)
        {
            return Regex.IsMatch(value, @"/^[~:#,%&_=\(\)\.\? \+\-@\/a-zA-Z0-9]+$/");
        }
        #endregion

        #region "Names"
        public static bool IsCatalogName(string value)
        {
            return IsGenericName(value);
        }

        public static bool IsCarrierName(string value)
        {
            return IsGenericName(value);
        }

        public static bool IsConfigName(string value)
        {
            return Regex.IsMatch(value, @"/^[a-zA-Z_0-9-]+$/");
        }

        public static bool IsGenericName(string value)
        {
            return Regex.IsMatch(value, @"/^[^<>;=#{}]*$/u");
        }

        public static bool IsImageTypeName(string value)
        {
            return Regex.IsMatch(value, @"/^[a-zA-Z0-9_ -]+$/");
        }

        public static bool IsName(string value)
        {
            return Regex.IsMatch(value, "/^[^0-9!<>,;?=+()@#\"°{}_$%:]*$/u");
        }

        public static bool IsTplName(string value)
        {
            return Regex.IsMatch(value, @"/^[a-zA-Z0-9_-]+$/");
        }
        #endregion

        #region "Location"
        public static bool IsAddress(string value)
        {
            return Regex.IsMatch(value, @"/^[^!<>?=+@{}_$%]*$/u");
        }

        public static bool IsCityName(string value)
        {
            return Regex.IsMatch(value, "/^[^!<>;?=+@#\"°{}_$%]*$/u");
        }

        public static bool IsCoordinate(string value)
        {
            return Regex.IsMatch(value, @"/^\-?[0-9]{1,8}\.[0-9]{1,8}$/s");
        }

        public static bool IsMessage(string value)
        {
            return Regex.IsMatch(value, @"/[<>{}]/i");
        }

        public static bool IsPhoneNumber(string value)
        {
            return Regex.IsMatch(value, @"/^[+0-9. ()-]*$/");
        }

        public static bool IsPostCode(string value)
        {
            return Regex.IsMatch(value, @"/^[a-zA-Z 0-9-]+$/");
        }

        public static bool IsStateIsoCode(string value)
        {
            return Regex.IsMatch(value, @"/^[a-zA-Z0-9]{2,3}((-)[a-zA-Z0-9]{1,3})?$/");
        }

        public static bool IsZipCodeFormat(string value) 
        {
            return Regex.IsMatch(value, @"/^[NLCnlc -]+$/");
        }
        #endregion

        #region "Products"
        public static bool IsAbsoluteUrl(string value)
        {
            return Regex.IsMatch(value, @"/^https?:\/\/[:#%&_=\(\)\.\? \+\-@\/a-zA-Z0-9]+$/");
        }

        public static bool IsDniLite(string value)
        {
            return Regex.IsMatch(value, @"/^[0-9A-Za-z-.]{1,16}$/U");
        }

        public static bool IsEan13(string value)
        {
            return Regex.IsMatch(value, @"/^[0-9]{0,13}$/");
        }

        public static bool IsLinkRewrite(string value)
        {
            return Regex.IsMatch(value, @"/^[_a-zA-Z0-9-]+$/");
        }

        public static bool IsUpc(string value)
        {
            return Regex.IsMatch(value, @"/^[0-9]{0,12}$/");
        }
        #endregion
    }
}
