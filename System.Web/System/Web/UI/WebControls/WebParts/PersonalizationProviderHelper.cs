namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Web;
    using System.Web.Util;

    internal static class PersonalizationProviderHelper
    {
        internal static string[] CheckAndTrimNonEmptyStringEntries(string[] array, string paramName, bool throwIfArrayIsNull, bool checkCommas, int lengthToCheck)
        {
            if (array == null)
            {
                if (throwIfArrayIsNull)
                {
                    throw new ArgumentNullException(paramName);
                }
                return null;
            }
            if (array.Length == 0)
            {
                throw new ArgumentException(System.Web.SR.GetString("PersonalizationProviderHelper_Empty_Collection", new object[] { paramName }));
            }
            string[] destinationArray = null;
            for (int i = 0; i < array.Length; i++)
            {
                string str = array[i];
                string str2 = (str == null) ? null : str.Trim();
                if (string.IsNullOrEmpty(str2))
                {
                    throw new ArgumentException(System.Web.SR.GetString("PersonalizationProviderHelper_Null_Or_Empty_String_Entries", new object[] { paramName }));
                }
                if (checkCommas && (str2.IndexOf(',') != -1))
                {
                    throw new ArgumentException(System.Web.SR.GetString("PersonalizationProviderHelper_CannotHaveCommaInString", new object[] { paramName, str }));
                }
                if ((lengthToCheck > -1) && (str2.Length > lengthToCheck))
                {
                    throw new ArgumentException(System.Web.SR.GetString("PersonalizationProviderHelper_Trimmed_Entry_Value_Exceed_Maximum_Length", new object[] { str, paramName, lengthToCheck.ToString(CultureInfo.CurrentCulture) }));
                }
                if ((str.Length != str2.Length) && (destinationArray == null))
                {
                    destinationArray = new string[array.Length];
                    Array.Copy(array, destinationArray, i);
                }
                if (destinationArray != null)
                {
                    destinationArray[i] = str2;
                }
            }
            if (destinationArray == null)
            {
                return array;
            }
            return destinationArray;
        }

        internal static string CheckAndTrimStringWithoutCommas(string paramValue, string paramName)
        {
            string str = StringUtil.CheckAndTrimString(paramValue, paramName);
            if (str.IndexOf(',') != -1)
            {
                throw new ArgumentException(System.Web.SR.GetString("PersonalizationProviderHelper_CannotHaveCommaInString", new object[] { paramName, paramValue }));
            }
            return str;
        }

        internal static void CheckNegativeInteger(int paramValue, string paramName)
        {
            if (paramValue < 0)
            {
                throw new ArgumentException(System.Web.SR.GetString("PersonalizationProviderHelper_Negative_Integer"), paramName);
            }
        }

        internal static void CheckNegativeReturnedInteger(int returnedValue, string methodName)
        {
            if (returnedValue < 0)
            {
                throw new HttpException(System.Web.SR.GetString("PersonalizationAdmin_UnexpectedPersonalizationProviderReturnValue", new object[] { returnedValue.ToString(CultureInfo.CurrentCulture), methodName }));
            }
        }

        internal static void CheckNullEntries(ICollection array, string paramName)
        {
            if (array == null)
            {
                throw new ArgumentNullException(paramName);
            }
            if (array.Count == 0)
            {
                throw new ArgumentException(System.Web.SR.GetString("PersonalizationProviderHelper_Empty_Collection", new object[] { paramName }));
            }
            using (IEnumerator enumerator = array.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current == null)
                    {
                        throw new ArgumentException(System.Web.SR.GetString("PersonalizationProviderHelper_Null_Entries", new object[] { paramName }));
                    }
                }
            }
        }

        internal static void CheckOnlyOnePathWithUsers(string[] paths, string[] usernames)
        {
            if (((usernames != null) && (usernames.Length > 0)) && ((paths != null) && (paths.Length > 1)))
            {
                throw new ArgumentException(System.Web.SR.GetString("PersonalizationProviderHelper_More_Than_One_Path", new object[] { "paths", "usernames" }));
            }
        }

        internal static void CheckPageIndexAndSize(int pageIndex, int pageSize)
        {
            if (pageIndex < 0)
            {
                throw new ArgumentException(System.Web.SR.GetString("PersonalizationProviderHelper_Invalid_Less_Than_Parameter", new object[] { "pageIndex", "0" }));
            }
            if (pageSize < 1)
            {
                throw new ArgumentException(System.Web.SR.GetString("PersonalizationProviderHelper_Invalid_Less_Than_Parameter", new object[] { "pageSize", "1" }));
            }
            long num = ((pageIndex * pageSize) + pageSize) - 1L;
            if (num > 0x7fffffffL)
            {
                throw new ArgumentException(System.Web.SR.GetString("PageIndex_PageSize_bad"));
            }
        }

        internal static void CheckPersonalizationScope(PersonalizationScope scope)
        {
            if ((scope < PersonalizationScope.User) || (scope > PersonalizationScope.Shared))
            {
                throw new ArgumentOutOfRangeException("scope");
            }
        }

        internal static void CheckUsernamesInSharedScope(string[] usernames)
        {
            if (usernames != null)
            {
                throw new ArgumentException(System.Web.SR.GetString("PersonalizationProviderHelper_No_Usernames_Set_In_Shared_Scope", new object[] { "usernames", "scope", PersonalizationScope.Shared.ToString() }));
            }
        }
    }
}

