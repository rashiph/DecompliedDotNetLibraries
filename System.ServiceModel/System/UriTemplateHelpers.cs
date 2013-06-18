namespace System
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    internal static class UriTemplateHelpers
    {
        private static UriTemplateQueryComparer queryComparer = new UriTemplateQueryComparer();
        private static UriTemplateQueryKeyComparer queryKeyComperar = new UriTemplateQueryKeyComparer();

        private static bool AllTemplatesAreEquivalent(IList<UriTemplate> array, int a, int b)
        {
            for (int i = a; i < (b - 1); i++)
            {
                if (!array[i].IsEquivalentTo(array[i + 1]))
                {
                    return false;
                }
            }
            return true;
        }

        [Conditional("DEBUG")]
        public static void AssertCanonical(string s)
        {
        }

        public static bool CanMatchQueryInterestingly(UriTemplate ut, NameValueCollection query, bool mustBeEspeciallyInteresting)
        {
            if (ut.queries.Count == 0)
            {
                return false;
            }
            string[] allKeys = query.AllKeys;
            foreach (KeyValuePair<string, UriTemplateQueryValue> pair in ut.queries)
            {
                string key = pair.Key;
                if (pair.Value.Nature == UriTemplatePartType.Literal)
                {
                    bool flag = false;
                    for (int i = 0; i < allKeys.Length; i++)
                    {
                        if (StringComparer.OrdinalIgnoreCase.Equals(allKeys[i], key))
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (flag)
                    {
                        if (pair.Value == UriTemplateQueryValue.Empty)
                        {
                            if (!string.IsNullOrEmpty(query[key]))
                            {
                                return false;
                            }
                            continue;
                        }
                        if (!(((UriTemplateLiteralQueryValue) pair.Value).AsRawUnescapedString() != query[key]))
                        {
                            continue;
                        }
                    }
                    return false;
                }
                if (mustBeEspeciallyInteresting && (Array.IndexOf<string>(allKeys, key) == -1))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool CanMatchQueryTrivially(UriTemplate ut)
        {
            return (ut.queries.Count == 0);
        }

        public static void DisambiguateSamePath(UriTemplate[] array, int a, int b, bool allowDuplicateEquivalentUriTemplates)
        {
            Array.Sort<UriTemplate>(array, a, b - a, queryComparer);
            if ((b - a) != 1)
            {
                if (allowDuplicateEquivalentUriTemplates)
                {
                    while ((a < b) && (array[a].queries.Count == 0))
                    {
                        a++;
                    }
                    if ((b - a) <= 1)
                    {
                        return;
                    }
                    goto Label_009C;
                }
                if (array[a].queries.Count == 0)
                {
                    a++;
                }
                if (array[a].queries.Count == 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UTTDuplicate", new object[] { array[a].ToString(), array[a - 1].ToString() })));
                }
                if ((b - a) != 1)
                {
                    goto Label_009C;
                }
            }
            return;
        Label_009C:
            EnsureQueriesAreDistinct(array, a, b, allowDuplicateEquivalentUriTemplates);
        }

        private static void EnsureQueriesAreDistinct(UriTemplate[] array, int a, int b, bool allowDuplicateEquivalentUriTemplates)
        {
            Dictionary<string, byte> dictionary = new Dictionary<string, byte>(StringComparer.OrdinalIgnoreCase);
            for (int i = a; i < b; i++)
            {
                foreach (KeyValuePair<string, UriTemplateQueryValue> pair in array[i].queries)
                {
                    if ((pair.Value.Nature == UriTemplatePartType.Literal) && !dictionary.ContainsKey(pair.Key))
                    {
                        dictionary.Add(pair.Key, 0);
                    }
                }
            }
            Dictionary<string, byte> queryVarNames = new Dictionary<string, byte>(dictionary);
            for (int j = a; j < b; j++)
            {
                foreach (string str in dictionary.Keys)
                {
                    if (!array[j].queries.ContainsKey(str) || (array[j].queries[str].Nature != UriTemplatePartType.Literal))
                    {
                        queryVarNames.Remove(str);
                    }
                }
            }
            dictionary = null;
            if ((queryVarNames.Count == 0) && (!allowDuplicateEquivalentUriTemplates || !AllTemplatesAreEquivalent(array, a, b)))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UTTOtherAmbiguousQueries", new object[] { array[a].ToString() })));
            }
            string[][] strArray = new string[b - a][];
            for (int k = 0; k < (b - a); k++)
            {
                strArray[k] = GetQueryLiterals(array[k + a], queryVarNames);
            }
            for (int m = 0; m < (b - a); m++)
            {
                for (int n = m + 1; n < (b - a); n++)
                {
                    if (Same(strArray[m], strArray[n]))
                    {
                        if (!array[m + a].IsEquivalentTo(array[n + a]))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UTTAmbiguousQueries", new object[] { array[a + m].ToString(), array[n + a].ToString() })));
                        }
                        if (!allowDuplicateEquivalentUriTemplates)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UTTDuplicate", new object[] { array[a + m].ToString(), array[n + a].ToString() })));
                        }
                    }
                }
            }
        }

        public static IEqualityComparer<string> GetQueryKeyComparer()
        {
            return queryKeyComperar;
        }

        private static string[] GetQueryLiterals(UriTemplate up, Dictionary<string, byte> queryVarNames)
        {
            string[] strArray = new string[queryVarNames.Count];
            int index = 0;
            foreach (string str in queryVarNames.Keys)
            {
                UriTemplateQueryValue value2 = up.queries[str];
                if (value2 == UriTemplateQueryValue.Empty)
                {
                    strArray[index] = null;
                }
                else
                {
                    strArray[index] = ((UriTemplateLiteralQueryValue) value2).AsRawUnescapedString();
                }
                index++;
            }
            return strArray;
        }

        public static string GetUriPath(Uri uri)
        {
            return uri.GetComponents(UriComponents.KeepDelimiter | UriComponents.Path, UriFormat.Unescaped);
        }

        public static bool HasQueryLiteralRequirements(UriTemplate ut)
        {
            foreach (UriTemplateQueryValue value2 in ut.queries.Values)
            {
                if (value2.Nature == UriTemplatePartType.Literal)
                {
                    return true;
                }
            }
            return false;
        }

        public static UriTemplatePartType IdentifyPartType(string part)
        {
            int index = part.IndexOf("{", StringComparison.Ordinal);
            int num2 = part.IndexOf("}", StringComparison.Ordinal);
            if (index == -1)
            {
                if (num2 != -1)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.ServiceModel.SR.GetString("UTInvalidFormatSegmentOrQueryPart", new object[] { part })));
                }
                return UriTemplatePartType.Literal;
            }
            if (num2 < (index + 2))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.ServiceModel.SR.GetString("UTInvalidFormatSegmentOrQueryPart", new object[] { part })));
            }
            if ((index <= 0) && ((num2 >= (part.Length - 2)) && ((num2 != (part.Length - 2)) || part.EndsWith("/", StringComparison.Ordinal))))
            {
                return UriTemplatePartType.Variable;
            }
            return UriTemplatePartType.Compound;
        }

        public static bool IsWildcardPath(string path)
        {
            UriTemplatePartType type;
            if (path.IndexOf('/') != -1)
            {
                return false;
            }
            return IsWildcardSegment(path, out type);
        }

        public static bool IsWildcardSegment(string segment, out UriTemplatePartType type)
        {
            type = IdentifyPartType(segment);
            switch (type)
            {
                case UriTemplatePartType.Literal:
                    return (string.Compare(segment, "*", StringComparison.Ordinal) == 0);

                case UriTemplatePartType.Compound:
                    return false;

                case UriTemplatePartType.Variable:
                    if ((segment.IndexOf("*", StringComparison.Ordinal) != 1) || segment.EndsWith("/", StringComparison.Ordinal))
                    {
                        return false;
                    }
                    return (segment.Length > ("*".Length + 2));
            }
            return false;
        }

        public static NameValueCollection ParseQueryString(string query)
        {
            NameValueCollection values = UrlUtility.ParseQueryString(query);
            string str = values[null];
            if (!string.IsNullOrEmpty(str))
            {
                values.Remove(null);
                string[] strArray = str.Split(new char[] { ',' });
                for (int i = 0; i < strArray.Length; i++)
                {
                    values.Add(strArray[i], null);
                }
            }
            return values;
        }

        private static bool Same(string[] a, string[] b)
        {
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }
            return true;
        }

        private class UriTemplateQueryComparer : IComparer<UriTemplate>
        {
            public int Compare(UriTemplate x, UriTemplate y)
            {
                return Comparer<int>.Default.Compare(x.queries.Count, y.queries.Count);
            }
        }

        private class UriTemplateQueryKeyComparer : IEqualityComparer<string>
        {
            public bool Equals(string x, string y)
            {
                return (string.Compare(x, y, StringComparison.OrdinalIgnoreCase) == 0);
            }

            public int GetHashCode(string obj)
            {
                if (obj == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("obj");
                }
                return obj.ToUpperInvariant().GetHashCode();
            }
        }
    }
}

