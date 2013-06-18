namespace System.ServiceModel.Configuration
{
    using System;
    using System.Collections;
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;
    using System.Text;
    using System.Xml;

    public class XPathMessageFilterElementComparer : IComparer
    {
        internal static string ParseXPathString(XPathMessageFilter filter)
        {
            return ParseXPathString(filter, false);
        }

        internal static string ParseXPathString(XPathMessageFilter filter, bool throwOnFailure)
        {
            XPathLexer lexer = new XPathLexer(filter.XPath);
            return ParseXPathString(lexer, filter.Namespaces, throwOnFailure);
        }

        private static string ParseXPathString(XPathLexer lexer, XmlNamespaceManager namespaceManager, bool throwOnFailure)
        {
            int firstTokenChar = lexer.FirstTokenChar;
            if (lexer.MoveNext())
            {
                XPathToken token = lexer.Token;
                StringBuilder builder = new StringBuilder(ParseXPathString(lexer, namespaceManager, throwOnFailure));
                if (XPathTokenID.NameTest == token.TokenID)
                {
                    string prefix = token.Prefix;
                    if (!string.IsNullOrEmpty(prefix))
                    {
                        string str3 = namespaceManager.LookupNamespace(prefix);
                        if (!string.IsNullOrEmpty(str3))
                        {
                            builder = builder.Replace(prefix, str3, firstTokenChar, prefix.Length);
                        }
                        else if (throwOnFailure)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new IndexOutOfRangeException(System.ServiceModel.SR.GetString("ConfigXPathNamespacePrefixNotFound", new object[] { prefix })));
                        }
                    }
                }
                return builder.ToString();
            }
            return lexer.ConsumedSubstring();
        }

        int IComparer.Compare(object x, object y)
        {
            string strA = this.TranslateObjectToElementKey(x);
            string strB = this.TranslateObjectToElementKey(y);
            return string.Compare(strA, strB, StringComparison.Ordinal);
        }

        private string TranslateObjectToElementKey(object obj)
        {
            string str = null;
            if (obj.GetType().IsAssignableFrom(typeof(XPathMessageFilter)))
            {
                str = ParseXPathString((XPathMessageFilter) obj);
            }
            else if (obj.GetType().IsAssignableFrom(typeof(XPathMessageFilterElement)))
            {
                str = ParseXPathString(((XPathMessageFilterElement) obj).Filter);
            }
            else if (obj.GetType().IsAssignableFrom(typeof(string)))
            {
                str = (string) obj;
            }
            if (string.IsNullOrEmpty(str))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ConfigCannotParseXPathFilter", new object[] { obj.GetType().AssemblyQualifiedName })));
            }
            return str;
        }
    }
}

