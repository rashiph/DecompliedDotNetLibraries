namespace System.Web.Configuration
{
    using System;
    using System.Collections;
    using System.Configuration;
    using System.IO;
    using System.Security;
    using System.Security.Permissions;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.UI;
    using System.Xml;

    public class HttpCapabilitiesSectionHandler : IConfigurationSectionHandler
    {
        private const int _defaultUserAgentCacheKeyLength = 0x40;
        private static Regex errRegex = new Regex(@"\G\S {0,8}");
        private static Regex lineRegex = new Regex("\\G(?<var>\\w+)\\s*=\\s*(?:\"(?<pat>[^\"\r\n\\\\]*(?:\\\\.[^\"\r\n\\\\]*)*)\"|(?!\")(?<pat>\\S+))\\s*");
        private static Regex wsRegex = new Regex(@"\G\s*");

        private static void AppendLines(ArrayList setlist, string text, XmlNode node)
        {
            int lineNumber = ConfigurationErrorsException.GetLineNumber(node);
            int startat = 0;
            while (true)
            {
                Match match;
                if ((match = wsRegex.Match(text, startat)).Success)
                {
                    lineNumber += Util.LineCount(text, startat, match.Index + match.Length);
                    startat = match.Index + match.Length;
                }
                if (startat == text.Length)
                {
                    return;
                }
                if (!(match = lineRegex.Match(text, startat)).Success)
                {
                    match = errRegex.Match(text, startat);
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Problem_reading_caps_config", new object[] { match.ToString() }), ConfigurationErrorsException.GetFilename(node), lineNumber);
                }
                setlist.Add(new CapabilitiesAssignment(match.Groups["var"].Value, new CapabilitiesPattern(match.Groups["pat"].Value)));
                lineNumber += Util.LineCount(text, startat, match.Index + match.Length);
                startat = match.Index + match.Length;
            }
        }

        public object Create(object parent, object configurationContext, XmlNode section)
        {
            if (!System.Web.Configuration.HandlerBase.IsServerConfiguration(configurationContext))
            {
                return null;
            }
            ParseState parseState = new ParseState {
                SectionName = section.Name,
                Evaluator = new HttpCapabilitiesDefaultProvider((HttpCapabilitiesDefaultProvider) parent)
            };
            int val = 0;
            if (parent != null)
            {
                val = ((HttpCapabilitiesDefaultProvider) parent).UserAgentCacheKeyLength;
            }
            System.Web.Configuration.HandlerBase.GetAndRemovePositiveIntegerAttribute(section, "userAgentCacheKeyLength", ref val);
            if (val == 0)
            {
                val = 0x40;
            }
            parseState.Evaluator.UserAgentCacheKeyLength = val;
            string browserCapabilitiesProviderType = null;
            if (parent != null)
            {
                browserCapabilitiesProviderType = ((HttpCapabilitiesDefaultProvider) parent).BrowserCapabilitiesProviderType;
            }
            System.Web.Configuration.HandlerBase.GetAndRemoveNonEmptyStringAttribute(section, "provider", ref browserCapabilitiesProviderType);
            parseState.Evaluator.BrowserCapabilitiesProviderType = browserCapabilitiesProviderType;
            System.Web.Configuration.HandlerBase.CheckForUnrecognizedAttributes(section);
            ArrayList rulelist = RuleListFromElement(parseState, section, true);
            if (rulelist.Count > 0)
            {
                parseState.RuleList.Add(new CapabilitiesSection(2, null, null, rulelist));
            }
            if (parseState.FileList.Count > 0)
            {
                parseState.IsExternalFile = true;
                ResolveFiles(parseState, configurationContext);
            }
            parseState.Evaluator.AddRuleList(parseState.RuleList);
            return parseState.Evaluator;
        }

        private static void ProcessFile(ArrayList fileList, XmlNode node)
        {
            string val = null;
            XmlNode y = System.Web.Configuration.HandlerBase.GetAndRemoveRequiredStringAttribute(node, "src", ref val);
            System.Web.Configuration.HandlerBase.CheckForUnrecognizedAttributes(node);
            System.Web.Configuration.HandlerBase.CheckForNonCommentChildNodes(node);
            fileList.Add(new Pair(val, y));
        }

        private static void ProcessResult(HttpCapabilitiesDefaultProvider capabilitiesEvaluator, XmlNode node)
        {
            bool val = true;
            System.Web.Configuration.HandlerBase.GetAndRemoveBooleanAttribute(node, "inherit", ref val);
            if (!val)
            {
                capabilitiesEvaluator.ClearParent();
            }
            Type type = null;
            XmlNode node2 = System.Web.Configuration.HandlerBase.GetAndRemoveTypeAttribute(node, "type", ref type);
            if ((node2 != null) && !type.Equals(capabilitiesEvaluator._resultType))
            {
                System.Web.Configuration.HandlerBase.CheckAssignableType(node2, capabilitiesEvaluator._resultType, type);
                capabilitiesEvaluator._resultType = type;
            }
            int num = 0;
            if (System.Web.Configuration.HandlerBase.GetAndRemovePositiveIntegerAttribute(node, "cacheTime", ref num) != null)
            {
                capabilitiesEvaluator.CacheTime = TimeSpan.FromSeconds((double) num);
            }
            System.Web.Configuration.HandlerBase.CheckForUnrecognizedAttributes(node);
            System.Web.Configuration.HandlerBase.CheckForNonCommentChildNodes(node);
        }

        private static void ResolveFiles(ParseState parseState, object configurationContext)
        {
            HttpConfigurationContext context = (HttpConfigurationContext) configurationContext;
            string directoryName = null;
            bool flag = false;
            try
            {
                if (context.VirtualPath == null)
                {
                    flag = true;
                    new FileIOPermission(PermissionState.None) { AllFiles = FileIOPermissionAccess.PathDiscovery }.Assert();
                }
                Pair pair = (Pair) parseState.FileList[0];
                XmlNode second = (XmlNode) pair.Second;
                directoryName = Path.GetDirectoryName(ConfigurationErrorsException.GetFilename(second));
            }
            finally
            {
                if (flag)
                {
                    CodeAccessPermission.RevertAssert();
                }
            }
            foreach (Pair pair2 in parseState.FileList)
            {
                XmlNode documentElement;
                string first = (string) pair2.First;
                string filename = Path.Combine(directoryName, first);
                try
                {
                    if (flag)
                    {
                        InternalSecurityPermissions.FileReadAccess(filename).Assert();
                    }
                    Exception exception = null;
                    try
                    {
                        HttpConfigurationSystem.AddFileDependency(filename);
                    }
                    catch (Exception exception2)
                    {
                        exception = exception2;
                    }
                    ConfigXmlDocument document = new ConfigXmlDocument();
                    try
                    {
                        document.Load(filename);
                        documentElement = document.DocumentElement;
                    }
                    catch (Exception exception3)
                    {
                        throw new ConfigurationErrorsException(System.Web.SR.GetString("Error_loading_XML_file", new object[] { filename, exception3.Message }), exception3, (XmlNode) pair2.Second);
                    }
                    if (exception != null)
                    {
                        throw exception;
                    }
                }
                finally
                {
                    if (flag)
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                }
                if (documentElement.Name != parseState.SectionName)
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Capability_file_root_element", new object[] { parseState.SectionName }), documentElement);
                }
                System.Web.Configuration.HandlerBase.CheckForUnrecognizedAttributes(documentElement);
                ArrayList rulelist = RuleListFromElement(parseState, documentElement, true);
                if (rulelist.Count > 0)
                {
                    parseState.RuleList.Add(new CapabilitiesSection(2, null, null, rulelist));
                }
            }
        }

        private static CapabilitiesRule RuleFromElement(ParseState parseState, XmlNode element)
        {
            int num;
            DelayedRegex regex;
            CapabilitiesPattern pattern;
            if (element.Name == "filter")
            {
                num = 2;
            }
            else if (element.Name == "case")
            {
                num = 3;
            }
            else
            {
                if (element.Name != "use")
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Unknown_tag_in_caps_config", new object[] { element.Name }), element);
                }
                System.Web.Configuration.HandlerBase.CheckForNonCommentChildNodes(element);
                string variable = System.Web.Configuration.HandlerBase.RemoveRequiredAttribute(element, "var");
                string asParam = System.Web.Configuration.HandlerBase.RemoveAttribute(element, "as");
                System.Web.Configuration.HandlerBase.CheckForUnrecognizedAttributes(element);
                if (asParam == null)
                {
                    asParam = string.Empty;
                }
                parseState.Evaluator.AddDependency(variable);
                return new CapabilitiesUse(variable, asParam);
            }
            string s = System.Web.Configuration.HandlerBase.RemoveAttribute(element, "match");
            string text = System.Web.Configuration.HandlerBase.RemoveAttribute(element, "with");
            System.Web.Configuration.HandlerBase.CheckForUnrecognizedAttributes(element);
            if (s == null)
            {
                if (text != null)
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Cannot_specify_test_without_match"), element);
                }
                regex = null;
                pattern = null;
            }
            else
            {
                try
                {
                    regex = new DelayedRegex(s);
                }
                catch (Exception exception)
                {
                    throw new ConfigurationErrorsException(exception.Message, exception, element);
                }
                if (text == null)
                {
                    pattern = CapabilitiesPattern.Default;
                }
                else
                {
                    pattern = new CapabilitiesPattern(text);
                }
            }
            return new CapabilitiesSection(num, regex, pattern, RuleListFromElement(parseState, element, false));
        }

        private static ArrayList RuleListFromElement(ParseState parseState, XmlNode node, bool top)
        {
            ArrayList setlist = new ArrayList();
            foreach (XmlNode node2 in node.ChildNodes)
            {
                switch (node2.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (node2.Name)
                        {
                            case "file":
                                goto Label_00B6;
                        }
                        goto Label_00DD;

                    case XmlNodeType.Text:
                    case XmlNodeType.CDATA:
                    {
                        top = false;
                        AppendLines(setlist, node2.Value, node);
                        continue;
                    }
                    case XmlNodeType.Comment:
                    case XmlNodeType.Whitespace:
                    {
                        continue;
                    }
                    default:
                        goto Label_00F0;
                }
                if (top)
                {
                    ProcessResult(parseState.Evaluator, node2);
                    goto Label_00EB;
                }
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Result_must_be_at_the_top_browser_section"), node2);
            Label_00B6:
                if (parseState.IsExternalFile)
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("File_element_only_valid_in_config"), node2);
                }
                ProcessFile(parseState.FileList, node2);
                goto Label_00EB;
            Label_00DD:
                setlist.Add(RuleFromElement(parseState, node2));
            Label_00EB:
                top = false;
                continue;
            Label_00F0:
                System.Web.Configuration.HandlerBase.ThrowUnrecognizedElement(node2);
            }
            return setlist;
        }

        private class ParseState
        {
            internal HttpCapabilitiesDefaultProvider Evaluator;
            internal ArrayList FileList = new ArrayList();
            internal bool IsExternalFile;
            internal ArrayList RuleList = new ArrayList();
            internal string SectionName;

            internal ParseState()
            {
            }
        }
    }
}

