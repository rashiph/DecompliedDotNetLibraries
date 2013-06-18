namespace System
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.Text;

    [TypeForwardedFrom("System.ServiceModel.Web, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public class UriTemplate
    {
        private readonly Dictionary<string, string> additionalDefaults;
        private IDictionary<string, string> defaults;
        internal readonly int firstOptionalSegment;
        private readonly string fragment;
        private readonly bool ignoreTrailingSlash;
        private const string NullableDefault = "null";
        internal readonly string originalTemplate;
        internal readonly Dictionary<string, UriTemplateQueryValue> queries;
        internal readonly List<UriTemplatePathSegment> segments;
        private Dictionary<string, string> unescapedDefaults;
        private VariablesCollection variables;
        private readonly WildcardInfo wildcard;
        internal const string WildcardPath = "*";

        public UriTemplate(string template) : this(template, false)
        {
        }

        public UriTemplate(string template, bool ignoreTrailingSlash) : this(template, ignoreTrailingSlash, null)
        {
        }

        public UriTemplate(string template, IDictionary<string, string> additionalDefaults) : this(template, false, additionalDefaults)
        {
        }

        public UriTemplate(string template, bool ignoreTrailingSlash, IDictionary<string, string> additionalDefaults)
        {
            string str;
            string str2;
            if (template == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("template");
            }
            this.originalTemplate = template;
            this.ignoreTrailingSlash = ignoreTrailingSlash;
            this.segments = new List<UriTemplatePathSegment>();
            this.queries = new Dictionary<string, UriTemplateQueryValue>(StringComparer.OrdinalIgnoreCase);
            if (template.StartsWith("/", StringComparison.Ordinal))
            {
                template = template.Substring(1);
            }
            int index = template.IndexOf('#');
            if (index == -1)
            {
                this.fragment = "";
            }
            else
            {
                this.fragment = template.Substring(index + 1);
                template = template.Substring(0, index);
            }
            int length = template.IndexOf('?');
            if (length == -1)
            {
                str2 = string.Empty;
                str = template;
            }
            else
            {
                str2 = template.Substring(length + 1);
                str = template.Substring(0, length);
            }
            template = null;
            if (!string.IsNullOrEmpty(str))
            {
                int startIndex = 0;
                while (startIndex < str.Length)
                {
                    string str3;
                    UriTemplatePartType type;
                    int num4 = str.IndexOf('/', startIndex);
                    if (num4 != -1)
                    {
                        str3 = str.Substring(startIndex, (num4 + 1) - startIndex);
                        startIndex = num4 + 1;
                    }
                    else
                    {
                        str3 = str.Substring(startIndex);
                        startIndex = str.Length;
                    }
                    if ((startIndex == str.Length) && UriTemplateHelpers.IsWildcardSegment(str3, out type))
                    {
                        switch (type)
                        {
                            case UriTemplatePartType.Literal:
                            {
                                this.wildcard = new WildcardInfo(this);
                                continue;
                            }
                            case UriTemplatePartType.Compound:
                            {
                                continue;
                            }
                            case UriTemplatePartType.Variable:
                            {
                                this.wildcard = new WildcardInfo(this, str3);
                                continue;
                            }
                        }
                    }
                    else
                    {
                        this.segments.Add(UriTemplatePathSegment.CreateFromUriTemplate(str3, this));
                    }
                }
            }
            if (!string.IsNullOrEmpty(str2))
            {
                int num5 = 0;
                while (num5 < str2.Length)
                {
                    int num8;
                    string str4;
                    string str5;
                    int num6 = str2.IndexOf('&', num5);
                    int num7 = num5;
                    if (num6 != -1)
                    {
                        num8 = num6;
                        num5 = num6 + 1;
                        if (num5 >= str2.Length)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UTQueryCannotEndInAmpersand", new object[] { this.originalTemplate })));
                        }
                    }
                    else
                    {
                        num8 = str2.Length;
                        num5 = str2.Length;
                    }
                    int num9 = str2.IndexOf('=', num7, num8 - num7);
                    if (num9 >= 0)
                    {
                        str4 = str2.Substring(num7, num9 - num7);
                        str5 = str2.Substring(num9 + 1, (num8 - num9) - 1);
                    }
                    else
                    {
                        str4 = str2.Substring(num7, num8 - num7);
                        str5 = null;
                    }
                    if (string.IsNullOrEmpty(str4))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UTQueryCannotHaveEmptyName", new object[] { this.originalTemplate })));
                    }
                    if (UriTemplateHelpers.IdentifyPartType(str4) != UriTemplatePartType.Literal)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("template", System.ServiceModel.SR.GetString("UTQueryMustHaveLiteralNames", new object[] { this.originalTemplate }));
                    }
                    str4 = UrlUtility.UrlDecode(str4, Encoding.UTF8);
                    if (this.queries.ContainsKey(str4))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UTQueryNamesMustBeUnique", new object[] { this.originalTemplate })));
                    }
                    this.queries.Add(str4, UriTemplateQueryValue.CreateFromUriTemplate(str5, this));
                }
            }
            if (additionalDefaults != null)
            {
                if (this.variables == null)
                {
                    if (additionalDefaults.Count > 0)
                    {
                        this.additionalDefaults = new Dictionary<string, string>(additionalDefaults, StringComparer.OrdinalIgnoreCase);
                    }
                }
                else
                {
                    foreach (KeyValuePair<string, string> pair in additionalDefaults)
                    {
                        string key = pair.Key.ToUpperInvariant();
                        if ((this.variables.DefaultValues != null) && this.variables.DefaultValues.ContainsKey(key))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("additionalDefaults", System.ServiceModel.SR.GetString("UTAdditionalDefaultIsInvalid", new object[] { pair.Key, this.originalTemplate }));
                        }
                        if (this.variables.PathSegmentVariableNames.Contains(key))
                        {
                            this.variables.AddDefaultValue(key, pair.Value);
                        }
                        else
                        {
                            if (this.variables.QueryValueVariableNames.Contains(key))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UTDefaultValueToQueryVarFromAdditionalDefaults", new object[] { this.originalTemplate, key })));
                            }
                            if (string.Compare(pair.Value, "null", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UTNullableDefaultAtAdditionalDefaults", new object[] { this.originalTemplate, key })));
                            }
                            if (this.additionalDefaults == null)
                            {
                                this.additionalDefaults = new Dictionary<string, string>(additionalDefaults.Count, StringComparer.OrdinalIgnoreCase);
                            }
                            this.additionalDefaults.Add(pair.Key, pair.Value);
                        }
                    }
                }
            }
            if ((this.variables != null) && (this.variables.DefaultValues != null))
            {
                this.variables.ValidateDefaults(out this.firstOptionalSegment);
            }
            else
            {
                this.firstOptionalSegment = this.segments.Count;
            }
        }

        internal string AddPathVariable(UriTemplatePartType sourceNature, string varDeclaration)
        {
            bool flag;
            return this.AddPathVariable(sourceNature, varDeclaration, out flag);
        }

        internal string AddPathVariable(UriTemplatePartType sourceNature, string varDeclaration, out bool hasDefaultValue)
        {
            if (this.variables == null)
            {
                this.variables = new VariablesCollection(this);
            }
            return this.variables.AddPathVariable(sourceNature, varDeclaration, out hasDefaultValue);
        }

        internal string AddQueryVariable(string varDeclaration)
        {
            if (this.variables == null)
            {
                this.variables = new VariablesCollection(this);
            }
            return this.variables.AddQueryVariable(varDeclaration);
        }

        private Uri Bind(Uri baseAddress, BindInformation bindInfo, bool omitDefaults)
        {
            int lastNonDefaultPathParameter;
            UriBuilder builder = new UriBuilder(baseAddress);
            int valueIndex = 0;
            int num2 = (this.variables == null) ? -1 : (this.variables.PathSegmentVariableNames.Count - 1);
            if (num2 == -1)
            {
                lastNonDefaultPathParameter = -1;
            }
            else if (omitDefaults)
            {
                lastNonDefaultPathParameter = bindInfo.LastNonDefaultPathParameter;
            }
            else
            {
                lastNonDefaultPathParameter = bindInfo.LastNonNullablePathParameter;
            }
            string[] normalizedParameters = bindInfo.NormalizedParameters;
            IDictionary<string, string> additionalParameters = bindInfo.AdditionalParameters;
            StringBuilder path = new StringBuilder(builder.Path);
            if (path[path.Length - 1] != '/')
            {
                path.Append('/');
            }
            if (lastNonDefaultPathParameter < num2)
            {
                int num4 = 0;
                while (valueIndex <= lastNonDefaultPathParameter)
                {
                    this.segments[num4++].Bind(normalizedParameters, ref valueIndex, path);
                }
                while (this.segments[num4].Nature == UriTemplatePartType.Literal)
                {
                    this.segments[num4++].Bind(normalizedParameters, ref valueIndex, path);
                }
                valueIndex = num2 + 1;
            }
            else if (this.segments.Count > 0)
            {
                for (int i = 0; i < this.segments.Count; i++)
                {
                    this.segments[i].Bind(normalizedParameters, ref valueIndex, path);
                }
                if (this.wildcard != null)
                {
                    this.wildcard.Bind(normalizedParameters, ref valueIndex, path);
                }
            }
            if (this.ignoreTrailingSlash && (path[path.Length - 1] == '/'))
            {
                path.Remove(path.Length - 1, 1);
            }
            builder.Path = path.ToString();
            if ((this.queries.Count != 0) || (additionalParameters != null))
            {
                StringBuilder query = new StringBuilder("");
                foreach (string str in this.queries.Keys)
                {
                    this.queries[str].Bind(str, normalizedParameters, ref valueIndex, query);
                }
                if (additionalParameters != null)
                {
                    foreach (string str2 in additionalParameters.Keys)
                    {
                        if (this.queries.ContainsKey(str2.ToUpperInvariant()))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("parameters", System.ServiceModel.SR.GetString("UTBothLiteralAndNameValueCollectionKey", new object[] { str2 }));
                        }
                        string str3 = additionalParameters[str2];
                        string str4 = string.IsNullOrEmpty(str3) ? string.Empty : UrlUtility.UrlEncode(str3, Encoding.UTF8);
                        query.AppendFormat("&{0}={1}", UrlUtility.UrlEncode(str2, Encoding.UTF8), str4);
                    }
                }
                if (query.Length != 0)
                {
                    query.Remove(0, 1);
                }
                builder.Query = query.ToString();
            }
            if (this.fragment != null)
            {
                builder.Fragment = this.fragment;
            }
            return builder.Uri;
        }

        public Uri BindByName(Uri baseAddress, NameValueCollection parameters)
        {
            return this.BindByName(baseAddress, parameters, false);
        }

        public Uri BindByName(Uri baseAddress, IDictionary<string, string> parameters)
        {
            return this.BindByName(baseAddress, parameters, false);
        }

        public Uri BindByName(Uri baseAddress, IDictionary<string, string> parameters, bool omitDefaults)
        {
            BindInformation information;
            if (baseAddress == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("baseAddress");
            }
            if (!baseAddress.IsAbsoluteUri)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("baseAddress", System.ServiceModel.SR.GetString("UTBadBaseAddress"));
            }
            if (this.variables == null)
            {
                information = this.PrepareBindInformation(parameters, omitDefaults);
            }
            else
            {
                information = this.variables.PrepareBindInformation(parameters, omitDefaults);
            }
            return this.Bind(baseAddress, information, omitDefaults);
        }

        public Uri BindByName(Uri baseAddress, NameValueCollection parameters, bool omitDefaults)
        {
            BindInformation information;
            if (baseAddress == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("baseAddress");
            }
            if (!baseAddress.IsAbsoluteUri)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("baseAddress", System.ServiceModel.SR.GetString("UTBadBaseAddress"));
            }
            if (this.variables == null)
            {
                information = this.PrepareBindInformation(parameters, omitDefaults);
            }
            else
            {
                information = this.variables.PrepareBindInformation(parameters, omitDefaults);
            }
            return this.Bind(baseAddress, information, omitDefaults);
        }

        public Uri BindByPosition(Uri baseAddress, params string[] values)
        {
            BindInformation information;
            if (baseAddress == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("baseAddress");
            }
            if (!baseAddress.IsAbsoluteUri)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("baseAddress", System.ServiceModel.SR.GetString("UTBadBaseAddress"));
            }
            if (this.variables == null)
            {
                if (values.Length > 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.ServiceModel.SR.GetString("UTBindByPositionNoVariables", new object[] { this.originalTemplate, values.Length })));
                }
                information = new BindInformation(this.additionalDefaults);
            }
            else
            {
                information = this.variables.PrepareBindInformation(values);
            }
            return this.Bind(baseAddress, information, false);
        }

        private void BindTerminalDefaults(int numMatchedSegments, NameValueCollection boundParameters)
        {
            for (int i = numMatchedSegments; i < this.segments.Count; i++)
            {
                if (this.segments[i].Nature == UriTemplatePartType.Variable)
                {
                    UriTemplateVariablePathSegment segment = this.segments[i] as UriTemplateVariablePathSegment;
                    this.variables.LookupDefault(segment.VarName, boundParameters);
                }
            }
        }

        internal UriTemplateMatch CreateUriTemplateMatch(Uri baseUri, Uri uri, object data, int numMatchedSegments, Collection<string> relativePathSegments, NameValueCollection uriQuery)
        {
            UriTemplateMatch match = new UriTemplateMatch {
                RequestUri = uri,
                BaseUri = baseUri
            };
            if (uriQuery != null)
            {
                match.SetQueryParameters(uriQuery);
            }
            match.SetRelativePathSegments(relativePathSegments);
            match.Data = data;
            match.Template = this;
            for (int i = 0; i < numMatchedSegments; i++)
            {
                this.segments[i].Lookup(match.RelativePathSegments[i], match.BoundVariables);
            }
            if (this.wildcard != null)
            {
                this.wildcard.Lookup(numMatchedSegments, match.RelativePathSegments, match.BoundVariables);
            }
            else if (numMatchedSegments < this.segments.Count)
            {
                this.BindTerminalDefaults(numMatchedSegments, match.BoundVariables);
            }
            if (this.queries.Count > 0)
            {
                foreach (KeyValuePair<string, UriTemplateQueryValue> pair in this.queries)
                {
                    pair.Value.Lookup(match.QueryParameters[pair.Key], match.BoundVariables);
                }
            }
            if (this.additionalDefaults != null)
            {
                foreach (KeyValuePair<string, string> pair2 in this.additionalDefaults)
                {
                    match.BoundVariables.Add(pair2.Key, this.UnescapeDefaultValue(pair2.Value));
                }
            }
            match.SetWildcardPathSegmentsStart(numMatchedSegments);
            return match;
        }

        private bool IsCandidatePathMatch(int numSegmentsInBaseAddress, string[] candidateSegments, out int numMatchedSegments, out Collection<string> relativeSegments)
        {
            int num = candidateSegments.Length - numSegmentsInBaseAddress;
            relativeSegments = new Collection<string>();
            bool flag = true;
            int num2 = 0;
            while (flag && (num2 < num))
            {
                string str = candidateSegments[num2 + numSegmentsInBaseAddress];
                if (num2 < this.segments.Count)
                {
                    bool ignoreTrailingSlash = this.ignoreTrailingSlash && (num2 == (num - 1));
                    UriTemplateLiteralPathSegment segment = UriTemplateLiteralPathSegment.CreateFromWireData(str);
                    if (!this.segments[num2].IsMatch(segment, ignoreTrailingSlash))
                    {
                        flag = false;
                        break;
                    }
                    string item = Uri.UnescapeDataString(str);
                    if (segment.EndsWithSlash)
                    {
                        item = item.Substring(0, item.Length - 1);
                    }
                    relativeSegments.Add(item);
                }
                else
                {
                    if (!this.HasWildcard)
                    {
                        flag = false;
                    }
                    break;
                }
                num2++;
            }
            if (flag)
            {
                numMatchedSegments = num2;
                if (num2 < num)
                {
                    while (num2 < num)
                    {
                        string str3 = Uri.UnescapeDataString(candidateSegments[num2 + numSegmentsInBaseAddress]);
                        if (str3.EndsWith("/", StringComparison.Ordinal))
                        {
                            str3 = str3.Substring(0, str3.Length - 1);
                        }
                        relativeSegments.Add(str3);
                        num2++;
                    }
                    return flag;
                }
                if (numMatchedSegments < this.firstOptionalSegment)
                {
                    flag = false;
                }
                return flag;
            }
            numMatchedSegments = 0;
            return flag;
        }

        public bool IsEquivalentTo(UriTemplate other)
        {
            if (other == null)
            {
                return false;
            }
            if ((other.segments == null) || (other.queries == null))
            {
                return false;
            }
            if (!this.IsPathFullyEquivalent(other))
            {
                return false;
            }
            if (!this.IsQueryEquivalent(other))
            {
                return false;
            }
            return true;
        }

        private bool IsPathFullyEquivalent(UriTemplate other)
        {
            if (this.HasWildcard != other.HasWildcard)
            {
                return false;
            }
            if (this.segments.Count != other.segments.Count)
            {
                return false;
            }
            for (int i = 0; i < this.segments.Count; i++)
            {
                if (!this.segments[i].IsEquivalentTo(other.segments[i], ((i == (this.segments.Count - 1)) && !this.HasWildcard) && (this.ignoreTrailingSlash || other.ignoreTrailingSlash)))
                {
                    return false;
                }
            }
            return true;
        }

        internal bool IsPathPartiallyEquivalentAt(UriTemplate other, int segmentsCount)
        {
            for (int i = 0; i < segmentsCount; i++)
            {
                if (!this.segments[i].IsEquivalentTo(other.segments[i], (i == (segmentsCount - 1)) && (this.ignoreTrailingSlash || other.ignoreTrailingSlash)))
                {
                    return false;
                }
            }
            return true;
        }

        internal bool IsQueryEquivalent(UriTemplate other)
        {
            if (this.queries.Count != other.queries.Count)
            {
                return false;
            }
            foreach (string str in this.queries.Keys)
            {
                UriTemplateQueryValue value3;
                UriTemplateQueryValue value2 = this.queries[str];
                if (!other.queries.TryGetValue(str, out value3))
                {
                    return false;
                }
                if (!value2.IsEquivalentTo(value3))
                {
                    return false;
                }
            }
            return true;
        }

        public UriTemplateMatch Match(Uri baseAddress, Uri candidate)
        {
            int num2;
            Collection<string> collection;
            if (baseAddress == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("baseAddress");
            }
            if (!baseAddress.IsAbsoluteUri)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("baseAddress", System.ServiceModel.SR.GetString("UTBadBaseAddress"));
            }
            if (candidate == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("candidate");
            }
            if (!candidate.IsAbsoluteUri)
            {
                return null;
            }
            string uriPath = UriTemplateHelpers.GetUriPath(baseAddress);
            string str2 = UriTemplateHelpers.GetUriPath(candidate);
            if (str2.Length < uriPath.Length)
            {
                return null;
            }
            if (!str2.StartsWith(uriPath, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            int length = baseAddress.Segments.Length;
            string[] segments = candidate.Segments;
            if (!this.IsCandidatePathMatch(length, segments, out num2, out collection))
            {
                return null;
            }
            NameValueCollection query = null;
            if (!UriTemplateHelpers.CanMatchQueryTrivially(this))
            {
                query = UriTemplateHelpers.ParseQueryString(candidate.Query);
                if (!UriTemplateHelpers.CanMatchQueryInterestingly(this, query, false))
                {
                    return null;
                }
            }
            return this.CreateUriTemplateMatch(baseAddress, candidate, null, num2, collection, query);
        }

        private BindInformation PrepareBindInformation(NameValueCollection parameters, bool omitDefaults)
        {
            BindInformation information;
            if (parameters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameters");
            }
            IDictionary<string, string> extraParameters = new Dictionary<string, string>(UriTemplateHelpers.GetQueryKeyComparer());
            foreach (string str in parameters.AllKeys)
            {
                if (string.IsNullOrEmpty(str))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("parameters", System.ServiceModel.SR.GetString("UTBindByNameCalledWithEmptyKey"));
                }
                extraParameters.Add(str, parameters[str]);
            }
            this.ProcessDefaultsAndCreateBindInfo(omitDefaults, extraParameters, out information);
            return information;
        }

        private BindInformation PrepareBindInformation(IDictionary<string, string> parameters, bool omitDefaults)
        {
            BindInformation information;
            if (parameters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameters");
            }
            IDictionary<string, string> extraParameters = new Dictionary<string, string>(UriTemplateHelpers.GetQueryKeyComparer());
            foreach (KeyValuePair<string, string> pair in parameters)
            {
                if (string.IsNullOrEmpty(pair.Key))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("parameters", System.ServiceModel.SR.GetString("UTBindByNameCalledWithEmptyKey"));
                }
                extraParameters.Add(pair);
            }
            this.ProcessDefaultsAndCreateBindInfo(omitDefaults, extraParameters, out information);
            return information;
        }

        private void ProcessDefaultsAndCreateBindInfo(bool omitDefaults, IDictionary<string, string> extraParameters, out BindInformation bindInfo)
        {
            if (this.additionalDefaults != null)
            {
                if (omitDefaults)
                {
                    foreach (KeyValuePair<string, string> pair in this.additionalDefaults)
                    {
                        string str;
                        if (extraParameters.TryGetValue(pair.Key, out str) && (string.Compare(str, pair.Value, StringComparison.Ordinal) == 0))
                        {
                            extraParameters.Remove(pair.Key);
                        }
                    }
                }
                else
                {
                    foreach (KeyValuePair<string, string> pair2 in this.additionalDefaults)
                    {
                        if (!extraParameters.ContainsKey(pair2.Key))
                        {
                            extraParameters.Add(pair2.Key, pair2.Value);
                        }
                    }
                }
            }
            if (extraParameters.Count == 0)
            {
                extraParameters = null;
            }
            bindInfo = new BindInformation(extraParameters);
        }

        internal static Uri RewriteUri(Uri uri, string host)
        {
            if (!string.IsNullOrEmpty(host) && !string.Equals(uri.Host + (!uri.IsDefaultPort ? (":" + uri.Port.ToString(CultureInfo.InvariantCulture)) : string.Empty), host, StringComparison.OrdinalIgnoreCase))
            {
                Uri uri2 = new Uri(string.Format(CultureInfo.InvariantCulture, "{0}://{1}", new object[] { uri.Scheme, host }));
                UriBuilder builder = new UriBuilder(uri) {
                    Host = uri2.Host,
                    Port = uri2.Port
                };
                return builder.Uri;
            }
            return uri;
        }

        public override string ToString()
        {
            return this.originalTemplate;
        }

        private string UnescapeDefaultValue(string escapedValue)
        {
            string str;
            if (string.IsNullOrEmpty(escapedValue))
            {
                return escapedValue;
            }
            if (this.unescapedDefaults == null)
            {
                this.unescapedDefaults = new Dictionary<string, string>(StringComparer.Ordinal);
            }
            if (!this.unescapedDefaults.TryGetValue(escapedValue, out str))
            {
                str = Uri.UnescapeDataString(escapedValue);
                this.unescapedDefaults.Add(escapedValue, str);
            }
            return str;
        }

        public IDictionary<string, string> Defaults
        {
            get
            {
                if (this.defaults == null)
                {
                    this.defaults = new UriTemplateDefaults(this);
                }
                return this.defaults;
            }
        }

        internal bool HasNoVariables
        {
            get
            {
                return (this.variables == null);
            }
        }

        internal bool HasWildcard
        {
            get
            {
                return (this.wildcard != null);
            }
        }

        public bool IgnoreTrailingSlash
        {
            get
            {
                return this.ignoreTrailingSlash;
            }
        }

        public ReadOnlyCollection<string> PathSegmentVariableNames
        {
            get
            {
                if (this.variables == null)
                {
                    return VariablesCollection.EmptyCollection;
                }
                return this.variables.PathSegmentVariableNames;
            }
        }

        public ReadOnlyCollection<string> QueryValueVariableNames
        {
            get
            {
                if (this.variables == null)
                {
                    return VariablesCollection.EmptyCollection;
                }
                return this.variables.QueryValueVariableNames;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BindInformation
        {
            private IDictionary<string, string> additionalParameters;
            private int lastNonDefaultPathParameter;
            private int lastNonNullablePathParameter;
            private string[] normalizedParameters;
            public BindInformation(string[] normalizedParameters, int lastNonDefaultPathParameter, int lastNonNullablePathParameter, IDictionary<string, string> additionalParameters)
            {
                this.normalizedParameters = normalizedParameters;
                this.lastNonDefaultPathParameter = lastNonDefaultPathParameter;
                this.lastNonNullablePathParameter = lastNonNullablePathParameter;
                this.additionalParameters = additionalParameters;
            }

            public BindInformation(IDictionary<string, string> additionalParameters)
            {
                this.normalizedParameters = null;
                this.lastNonDefaultPathParameter = -1;
                this.lastNonNullablePathParameter = -1;
                this.additionalParameters = additionalParameters;
            }

            public IDictionary<string, string> AdditionalParameters
            {
                get
                {
                    return this.additionalParameters;
                }
            }
            public int LastNonDefaultPathParameter
            {
                get
                {
                    return this.lastNonDefaultPathParameter;
                }
            }
            public int LastNonNullablePathParameter
            {
                get
                {
                    return this.lastNonNullablePathParameter;
                }
            }
            public string[] NormalizedParameters
            {
                get
                {
                    return this.normalizedParameters;
                }
            }
        }

        private class UriTemplateDefaults : IDictionary<string, string>, ICollection<KeyValuePair<string, string>>, IEnumerable<KeyValuePair<string, string>>, IEnumerable
        {
            private Dictionary<string, string> defaults = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            private ReadOnlyCollection<string> keys;
            private ReadOnlyCollection<string> values;

            public UriTemplateDefaults(UriTemplate template)
            {
                if ((template.variables != null) && (template.variables.DefaultValues != null))
                {
                    foreach (KeyValuePair<string, string> pair in template.variables.DefaultValues)
                    {
                        this.defaults.Add(pair.Key, pair.Value);
                    }
                }
                if (template.additionalDefaults != null)
                {
                    foreach (KeyValuePair<string, string> pair2 in template.additionalDefaults)
                    {
                        string key = pair2.Key.ToUpperInvariant();
                        this.defaults.Add(key, pair2.Value);
                    }
                }
                this.keys = new ReadOnlyCollection<string>(new List<string>(this.defaults.Keys));
                this.values = new ReadOnlyCollection<string>(new List<string>(this.defaults.Values));
            }

            public void Add(KeyValuePair<string, string> item)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("UTDefaultValuesAreImmutable")));
            }

            public void Add(string key, string value)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("UTDefaultValuesAreImmutable")));
            }

            public void Clear()
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("UTDefaultValuesAreImmutable")));
            }

            public bool Contains(KeyValuePair<string, string> item)
            {
                return this.defaults.Contains(item);
            }

            public bool ContainsKey(string key)
            {
                return this.defaults.ContainsKey(key);
            }

            public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
            {
                this.defaults.CopyTo(array, arrayIndex);
            }

            public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
            {
                return this.defaults.GetEnumerator();
            }

            public bool Remove(KeyValuePair<string, string> item)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("UTDefaultValuesAreImmutable")));
            }

            public bool Remove(string key)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("UTDefaultValuesAreImmutable")));
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.defaults.GetEnumerator();
            }

            public bool TryGetValue(string key, out string value)
            {
                return this.defaults.TryGetValue(key, out value);
            }

            public int Count
            {
                get
                {
                    return this.defaults.Count;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            public string this[string key]
            {
                get
                {
                    return this.defaults[key];
                }
                set
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("UTDefaultValuesAreImmutable")));
                }
            }

            public ICollection<string> Keys
            {
                get
                {
                    return this.keys;
                }
            }

            public ICollection<string> Values
            {
                get
                {
                    return this.values;
                }
            }
        }

        private class VariablesCollection
        {
            private Dictionary<string, string> defaultValues;
            private static ReadOnlyCollection<string> emptyStringCollection;
            private int firstNullablePathVariable;
            private readonly UriTemplate owner;
            private List<string> pathSegmentVariableNames;
            private ReadOnlyCollection<string> pathSegmentVariableNamesSnapshot;
            private List<UriTemplatePartType> pathSegmentVariableNature;
            private List<string> queryValueVariableNames;
            private ReadOnlyCollection<string> queryValueVariableNamesSnapshot;

            public VariablesCollection(UriTemplate owner)
            {
                this.owner = owner;
                this.pathSegmentVariableNames = new List<string>();
                this.pathSegmentVariableNature = new List<UriTemplatePartType>();
                this.queryValueVariableNames = new List<string>();
                this.firstNullablePathVariable = -1;
            }

            private void AddAdditionalDefaults(ref IDictionary<string, string> extraParameters)
            {
                if (extraParameters == null)
                {
                    extraParameters = this.owner.additionalDefaults;
                }
                else
                {
                    foreach (KeyValuePair<string, string> pair in this.owner.additionalDefaults)
                    {
                        if (!extraParameters.ContainsKey(pair.Key))
                        {
                            extraParameters.Add(pair.Key, pair.Value);
                        }
                    }
                }
            }

            public void AddDefaultValue(string varName, string value)
            {
                int index = this.pathSegmentVariableNames.IndexOf(varName);
                if (((this.owner.wildcard != null) && this.owner.wildcard.HasVariable) && (index == (this.pathSegmentVariableNames.Count - 1)))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UTStarVariableWithDefaultsFromAdditionalDefaults", new object[] { this.owner.originalTemplate, varName })));
                }
                if (((UriTemplatePartType) this.pathSegmentVariableNature[index]) != UriTemplatePartType.Variable)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UTDefaultValueToCompoundSegmentVarFromAdditionalDefaults", new object[] { this.owner.originalTemplate, varName })));
                }
                if (string.IsNullOrEmpty(value) || (string.Compare(value, "null", StringComparison.OrdinalIgnoreCase) == 0))
                {
                    value = null;
                }
                if (this.defaultValues == null)
                {
                    this.defaultValues = new Dictionary<string, string>();
                }
                this.defaultValues.Add(varName, value);
            }

            public string AddPathVariable(UriTemplatePartType sourceNature, string varDeclaration, out bool hasDefaultValue)
            {
                string str;
                string str2;
                this.ParseVariableDeclaration(varDeclaration, out str, out str2);
                hasDefaultValue = str2 != null;
                if (str.IndexOf("*", StringComparison.Ordinal) != -1)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.ServiceModel.SR.GetString("UTInvalidWildcardInVariableOrLiteral", new object[] { this.owner.originalTemplate, "*" })));
                }
                string item = str.ToUpperInvariant();
                if (this.pathSegmentVariableNames.Contains(item) || this.queryValueVariableNames.Contains(item))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UTVarNamesMustBeUnique", new object[] { this.owner.originalTemplate, str })));
                }
                this.pathSegmentVariableNames.Add(item);
                this.pathSegmentVariableNature.Add(sourceNature);
                if (hasDefaultValue)
                {
                    if (str2 == string.Empty)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UTInvalidDefaultPathValue", new object[] { this.owner.originalTemplate, varDeclaration, str })));
                    }
                    if (string.Compare(str2, "null", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        str2 = null;
                    }
                    if (this.defaultValues == null)
                    {
                        this.defaultValues = new Dictionary<string, string>();
                    }
                    this.defaultValues.Add(item, str2);
                }
                return item;
            }

            public string AddQueryVariable(string varDeclaration)
            {
                string str;
                string str2;
                this.ParseVariableDeclaration(varDeclaration, out str, out str2);
                if (str.IndexOf("*", StringComparison.Ordinal) != -1)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.ServiceModel.SR.GetString("UTInvalidWildcardInVariableOrLiteral", new object[] { this.owner.originalTemplate, "*" })));
                }
                if (str2 != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UTDefaultValueToQueryVar", new object[] { this.owner.originalTemplate, varDeclaration, str })));
                }
                string item = str.ToUpperInvariant();
                if (this.pathSegmentVariableNames.Contains(item) || this.queryValueVariableNames.Contains(item))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UTVarNamesMustBeUnique", new object[] { this.owner.originalTemplate, str })));
                }
                this.queryValueVariableNames.Add(item);
                return item;
            }

            private void LoadDefaultsAndValidate(string[] normalizedParameters, out int lastNonDefaultPathParameter, out int lastNonNullablePathParameter)
            {
                for (int i = 0; i < this.pathSegmentVariableNames.Count; i++)
                {
                    if (string.IsNullOrEmpty(normalizedParameters[i]) && (this.defaultValues != null))
                    {
                        this.defaultValues.TryGetValue(this.pathSegmentVariableNames[i], out normalizedParameters[i]);
                    }
                }
                lastNonDefaultPathParameter = this.pathSegmentVariableNames.Count - 1;
                if ((this.defaultValues != null) && (this.owner.segments[this.owner.segments.Count - 1].Nature != UriTemplatePartType.Literal))
                {
                    bool flag = false;
                    while (!flag && (lastNonDefaultPathParameter >= 0))
                    {
                        string str;
                        if (this.defaultValues.TryGetValue(this.pathSegmentVariableNames[lastNonDefaultPathParameter], out str))
                        {
                            if (string.Compare(normalizedParameters[lastNonDefaultPathParameter], str, StringComparison.Ordinal) != 0)
                            {
                                flag = true;
                            }
                            else
                            {
                                lastNonDefaultPathParameter--;
                            }
                        }
                        else
                        {
                            flag = true;
                        }
                    }
                }
                if (this.firstNullablePathVariable > lastNonDefaultPathParameter)
                {
                    lastNonNullablePathParameter = this.firstNullablePathVariable - 1;
                }
                else
                {
                    lastNonNullablePathParameter = lastNonDefaultPathParameter;
                }
                for (int j = 0; j <= lastNonNullablePathParameter; j++)
                {
                    if (((!this.owner.HasWildcard || !this.owner.wildcard.HasVariable) || (j != (this.pathSegmentVariableNames.Count - 1))) && string.IsNullOrEmpty(normalizedParameters[j]))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("parameters", System.ServiceModel.SR.GetString("BindUriTemplateToNullOrEmptyPathParam", new object[] { this.pathSegmentVariableNames[j] }));
                    }
                }
            }

            public void LookupDefault(string varName, NameValueCollection boundParameters)
            {
                boundParameters.Add(varName, this.owner.UnescapeDefaultValue(this.defaultValues[varName]));
            }

            private void ParseVariableDeclaration(string varDeclaration, out string varName, out string defaultValue)
            {
                if ((varDeclaration.IndexOf('{') != -1) || (varDeclaration.IndexOf('}') != -1))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.ServiceModel.SR.GetString("UTInvalidVarDeclaration", new object[] { this.owner.originalTemplate, varDeclaration })));
                }
                int index = varDeclaration.IndexOf('=');
                switch (index)
                {
                    case -1:
                        varName = varDeclaration;
                        defaultValue = null;
                        return;

                    case 0:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.ServiceModel.SR.GetString("UTInvalidVarDeclaration", new object[] { this.owner.originalTemplate, varDeclaration })));
                }
                varName = varDeclaration.Substring(0, index);
                defaultValue = varDeclaration.Substring(index + 1);
                if (defaultValue.IndexOf('=') != -1)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.ServiceModel.SR.GetString("UTInvalidVarDeclaration", new object[] { this.owner.originalTemplate, varDeclaration })));
                }
            }

            public UriTemplate.BindInformation PrepareBindInformation(params string[] parameters)
            {
                string[] strArray;
                int num2;
                int num3;
                if (parameters == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("values");
                }
                if ((parameters.Length < this.pathSegmentVariableNames.Count) || (parameters.Length > (this.pathSegmentVariableNames.Count + this.queryValueVariableNames.Count)))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.ServiceModel.SR.GetString("UTBindByPositionWrongCount", new object[] { this.owner.originalTemplate, this.pathSegmentVariableNames.Count, this.queryValueVariableNames.Count, parameters.Length })));
                }
                if (parameters.Length == (this.pathSegmentVariableNames.Count + this.queryValueVariableNames.Count))
                {
                    strArray = parameters;
                }
                else
                {
                    strArray = new string[this.pathSegmentVariableNames.Count + this.queryValueVariableNames.Count];
                    parameters.CopyTo(strArray, 0);
                    for (int i = parameters.Length; i < strArray.Length; i++)
                    {
                        strArray[i] = null;
                    }
                }
                this.LoadDefaultsAndValidate(strArray, out num2, out num3);
                return new UriTemplate.BindInformation(strArray, num2, num3, this.owner.additionalDefaults);
            }

            public UriTemplate.BindInformation PrepareBindInformation(IDictionary<string, string> parameters, bool omitDefaults)
            {
                UriTemplate.BindInformation information;
                if (parameters == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameters");
                }
                string[] normalizedParameters = this.PrepareNormalizedParameters();
                IDictionary<string, string> extraParameters = null;
                foreach (string str in parameters.Keys)
                {
                    this.ProcessBindParameter(str, parameters[str], normalizedParameters, ref extraParameters);
                }
                this.ProcessDefaultsAndCreateBindInfo(omitDefaults, normalizedParameters, extraParameters, out information);
                return information;
            }

            public UriTemplate.BindInformation PrepareBindInformation(NameValueCollection parameters, bool omitDefaults)
            {
                UriTemplate.BindInformation information;
                if (parameters == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameters");
                }
                string[] normalizedParameters = this.PrepareNormalizedParameters();
                IDictionary<string, string> extraParameters = null;
                foreach (string str in parameters.AllKeys)
                {
                    this.ProcessBindParameter(str, parameters[str], normalizedParameters, ref extraParameters);
                }
                this.ProcessDefaultsAndCreateBindInfo(omitDefaults, normalizedParameters, extraParameters, out information);
                return information;
            }

            private string[] PrepareNormalizedParameters()
            {
                string[] strArray = new string[this.pathSegmentVariableNames.Count + this.queryValueVariableNames.Count];
                for (int i = 0; i < strArray.Length; i++)
                {
                    strArray[i] = null;
                }
                return strArray;
            }

            private void ProcessBindParameter(string name, string value, string[] normalizedParameters, ref IDictionary<string, string> extraParameters)
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("parameters", System.ServiceModel.SR.GetString("UTBindByNameCalledWithEmptyKey"));
                }
                string item = name.ToUpperInvariant();
                int index = this.pathSegmentVariableNames.IndexOf(item);
                if (index != -1)
                {
                    normalizedParameters[index] = string.IsNullOrEmpty(value) ? string.Empty : value;
                }
                else
                {
                    int num2 = this.queryValueVariableNames.IndexOf(item);
                    if (num2 != -1)
                    {
                        normalizedParameters[this.pathSegmentVariableNames.Count + num2] = string.IsNullOrEmpty(value) ? string.Empty : value;
                    }
                    else
                    {
                        if (extraParameters == null)
                        {
                            extraParameters = new Dictionary<string, string>(UriTemplateHelpers.GetQueryKeyComparer());
                        }
                        extraParameters.Add(name, value);
                    }
                }
            }

            private void ProcessDefaultsAndCreateBindInfo(bool omitDefaults, string[] normalizedParameters, IDictionary<string, string> extraParameters, out UriTemplate.BindInformation bindInfo)
            {
                int num;
                int num2;
                this.LoadDefaultsAndValidate(normalizedParameters, out num, out num2);
                if (this.owner.additionalDefaults != null)
                {
                    if (omitDefaults)
                    {
                        this.RemoveAdditionalDefaults(ref extraParameters);
                    }
                    else
                    {
                        this.AddAdditionalDefaults(ref extraParameters);
                    }
                }
                bindInfo = new UriTemplate.BindInformation(normalizedParameters, num, num2, extraParameters);
            }

            private void RemoveAdditionalDefaults(ref IDictionary<string, string> extraParameters)
            {
                if (extraParameters != null)
                {
                    foreach (KeyValuePair<string, string> pair in this.owner.additionalDefaults)
                    {
                        string str;
                        if (extraParameters.TryGetValue(pair.Key, out str) && (string.Compare(str, pair.Value, StringComparison.Ordinal) == 0))
                        {
                            extraParameters.Remove(pair.Key);
                        }
                    }
                    if (extraParameters.Count == 0)
                    {
                        extraParameters = null;
                    }
                }
            }

            public void ValidateDefaults(out int firstOptionalSegment)
            {
                for (int i = this.pathSegmentVariableNames.Count - 1; (i >= 0) && (this.firstNullablePathVariable == -1); i--)
                {
                    string str2;
                    string key = this.pathSegmentVariableNames[i];
                    if (!this.defaultValues.TryGetValue(key, out str2))
                    {
                        this.firstNullablePathVariable = i + 1;
                    }
                    else if (str2 != null)
                    {
                        this.firstNullablePathVariable = i + 1;
                    }
                }
                if (this.firstNullablePathVariable == -1)
                {
                    this.firstNullablePathVariable = 0;
                }
                if (this.firstNullablePathVariable > 1)
                {
                    for (int j = this.firstNullablePathVariable - 2; j >= 0; j--)
                    {
                        string str4;
                        string str3 = this.pathSegmentVariableNames[j];
                        if (this.defaultValues.TryGetValue(str3, out str4) && (str4 == null))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UTNullableDefaultMustBeFollowedWithNullables", new object[] { this.owner.originalTemplate, str3, this.pathSegmentVariableNames[j + 1] })));
                        }
                    }
                }
                if (this.firstNullablePathVariable < this.pathSegmentVariableNames.Count)
                {
                    if (this.owner.HasWildcard)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UTNullableDefaultMustNotBeFollowedWithWildcard", new object[] { this.owner.originalTemplate, this.pathSegmentVariableNames[this.firstNullablePathVariable] })));
                    }
                    for (int k = this.pathSegmentVariableNames.Count - 1; k >= this.firstNullablePathVariable; k--)
                    {
                        int num4 = this.owner.segments.Count - (this.pathSegmentVariableNames.Count - k);
                        if (this.owner.segments[num4].Nature != UriTemplatePartType.Variable)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UTNullableDefaultMustNotBeFollowedWithLiteral", new object[] { this.owner.originalTemplate, this.pathSegmentVariableNames[this.firstNullablePathVariable], this.owner.segments[num4].OriginalSegment })));
                        }
                    }
                }
                int num5 = this.pathSegmentVariableNames.Count - this.firstNullablePathVariable;
                firstOptionalSegment = this.owner.segments.Count - num5;
                if (!this.owner.HasWildcard)
                {
                    while (firstOptionalSegment > 0)
                    {
                        UriTemplatePathSegment segment = this.owner.segments[firstOptionalSegment - 1];
                        if (segment.Nature != UriTemplatePartType.Variable)
                        {
                            return;
                        }
                        UriTemplateVariablePathSegment segment2 = segment as UriTemplateVariablePathSegment;
                        if (!this.defaultValues.ContainsKey(segment2.VarName))
                        {
                            return;
                        }
                        firstOptionalSegment--;
                    }
                }
            }

            public Dictionary<string, string> DefaultValues
            {
                get
                {
                    return this.defaultValues;
                }
            }

            public static ReadOnlyCollection<string> EmptyCollection
            {
                get
                {
                    if (emptyStringCollection == null)
                    {
                        emptyStringCollection = new ReadOnlyCollection<string>(new List<string>());
                    }
                    return emptyStringCollection;
                }
            }

            public ReadOnlyCollection<string> PathSegmentVariableNames
            {
                get
                {
                    if (this.pathSegmentVariableNamesSnapshot == null)
                    {
                        this.pathSegmentVariableNamesSnapshot = new ReadOnlyCollection<string>(this.pathSegmentVariableNames);
                    }
                    return this.pathSegmentVariableNamesSnapshot;
                }
            }

            public ReadOnlyCollection<string> QueryValueVariableNames
            {
                get
                {
                    if (this.queryValueVariableNamesSnapshot == null)
                    {
                        this.queryValueVariableNamesSnapshot = new ReadOnlyCollection<string>(this.queryValueVariableNames);
                    }
                    return this.queryValueVariableNamesSnapshot;
                }
            }
        }

        private class WildcardInfo
        {
            private readonly UriTemplate owner;
            private readonly string varName;

            public WildcardInfo(UriTemplate owner)
            {
                this.varName = null;
                this.owner = owner;
            }

            public WildcardInfo(UriTemplate owner, string segment)
            {
                bool flag;
                this.varName = owner.AddPathVariable(UriTemplatePartType.Variable, segment.Substring(1 + "*".Length, (segment.Length - 2) - "*".Length), out flag);
                if (flag)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UTStarVariableWithDefaults", new object[] { owner.originalTemplate, segment, this.varName })));
                }
                this.owner = owner;
            }

            public void Bind(string[] values, ref int valueIndex, StringBuilder path)
            {
                if (this.HasVariable)
                {
                    if (string.IsNullOrEmpty(values[valueIndex]))
                    {
                        valueIndex++;
                    }
                    else
                    {
                        path.Append(values[valueIndex++]);
                    }
                }
            }

            public void Lookup(int numMatchedSegments, Collection<string> relativePathSegments, NameValueCollection boundParameters)
            {
                if (this.HasVariable)
                {
                    StringBuilder builder = new StringBuilder();
                    for (int i = numMatchedSegments; i < relativePathSegments.Count; i++)
                    {
                        if (i < (relativePathSegments.Count - 1))
                        {
                            builder.AppendFormat("{0}/", relativePathSegments[i]);
                        }
                        else
                        {
                            builder.Append(relativePathSegments[i]);
                        }
                    }
                    boundParameters.Add(this.varName, builder.ToString());
                }
            }

            internal bool HasVariable
            {
                get
                {
                    return !string.IsNullOrEmpty(this.varName);
                }
            }
        }
    }
}

