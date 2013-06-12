namespace System.Xml
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml.XPath;

    internal static class ValidateNames
    {
        private static XmlCharType xmlCharType = XmlCharType.Instance;

        private static string CreateName(string prefix, string localName)
        {
            if (prefix.Length == 0)
            {
                return localName;
            }
            return (prefix + ":" + localName);
        }

        internal static Exception GetInvalidNameException(string s, int offsetStartChar, int offsetBadChar)
        {
            if (offsetStartChar >= s.Length)
            {
                return new XmlException("Xml_EmptyName", string.Empty);
            }
            if (xmlCharType.IsNCNameSingleChar(s[offsetBadChar]) && !xmlCharType.IsStartNCNameSingleChar(s[offsetBadChar]))
            {
                return new XmlException("Xml_BadStartNameChar", XmlException.BuildCharExceptionArgs(s, offsetBadChar));
            }
            return new XmlException("Xml_BadNameChar", XmlException.BuildCharExceptionArgs(s, offsetBadChar));
        }

        internal static bool IsNameNoNamespaces(string s)
        {
            int num = ParseNameNoNamespaces(s, 0);
            return ((num > 0) && (num == s.Length));
        }

        internal static bool IsNmtokenNoNamespaces(string s)
        {
            int num = ParseNmtokenNoNamespaces(s, 0);
            return ((num > 0) && (num == s.Length));
        }

        internal static bool IsReservedNamespace(string s)
        {
            if (!s.Equals("http://www.w3.org/XML/1998/namespace"))
            {
                return s.Equals("http://www.w3.org/2000/xmlns/");
            }
            return true;
        }

        internal static unsafe int ParseNameNoNamespaces(string s, int offset)
        {
            int num = offset;
            if (num < s.Length)
            {
                if (((xmlCharType.charProperties[s[num]] & 4) == 0) && (s[num] != ':'))
                {
                    return 0;
                }
                num++;
                while (num < s.Length)
                {
                    if (((xmlCharType.charProperties[s[num]] & 8) == 0) && (s[num] != ':'))
                    {
                        break;
                    }
                    num++;
                }
            }
            return (num - offset);
        }

        internal static void ParseNameTestThrow(string s, out string prefix, out string localName)
        {
            int num;
            if ((s.Length != 0) && (s[0] == '*'))
            {
                prefix = (string) (localName = null);
                num = 1;
            }
            else
            {
                num = ParseNCName(s, 0);
                if (num != 0)
                {
                    localName = s.Substring(0, num);
                    if ((num < s.Length) && (s[num] == ':'))
                    {
                        prefix = localName;
                        int offset = num + 1;
                        if ((offset < s.Length) && (s[offset] == '*'))
                        {
                            localName = null;
                            num += 2;
                        }
                        else
                        {
                            int length = ParseNCName(s, offset);
                            if (length != 0)
                            {
                                localName = s.Substring(offset, length);
                                num += length + 1;
                            }
                        }
                    }
                    else
                    {
                        prefix = string.Empty;
                    }
                }
                else
                {
                    string str2;
                    localName = (string) (str2 = null);
                    prefix = str2;
                }
            }
            if ((num == 0) || (num != s.Length))
            {
                ThrowInvalidName(s, 0, num);
            }
        }

        internal static int ParseNCName(string s)
        {
            return ParseNCName(s, 0);
        }

        internal static unsafe int ParseNCName(string s, int offset)
        {
            int num = offset;
            if (num < s.Length)
            {
                if ((xmlCharType.charProperties[s[num]] & 4) != 0)
                {
                    num++;
                    while (num < s.Length)
                    {
                        if ((xmlCharType.charProperties[s[num]] & 8) == 0)
                        {
                            break;
                        }
                        num++;
                    }
                }
                else
                {
                    return 0;
                }
            }
            return (num - offset);
        }

        private static bool ParseNCNameInternal(string s, bool throwOnError)
        {
            int offsetBadChar = ParseNCName(s, 0);
            if ((offsetBadChar != 0) && (offsetBadChar == s.Length))
            {
                return true;
            }
            if (throwOnError)
            {
                ThrowInvalidName(s, 0, offsetBadChar);
            }
            return false;
        }

        internal static string ParseNCNameThrow(string s)
        {
            ParseNCNameInternal(s, true);
            return s;
        }

        internal static unsafe int ParseNmtoken(string s, int offset)
        {
            int num = offset;
            while (num < s.Length)
            {
                if ((xmlCharType.charProperties[s[num]] & 8) == 0)
                {
                    break;
                }
                num++;
            }
            return (num - offset);
        }

        internal static unsafe int ParseNmtokenNoNamespaces(string s, int offset)
        {
            int num = offset;
            while (num < s.Length)
            {
                if (((xmlCharType.charProperties[s[num]] & 8) == 0) && (s[num] != ':'))
                {
                    break;
                }
                num++;
            }
            return (num - offset);
        }

        internal static int ParseQName(string s, int offset, out int colonOffset)
        {
            colonOffset = 0;
            int num = ParseNCName(s, offset);
            if (num != 0)
            {
                offset += num;
                if ((offset < s.Length) && (s[offset] == ':'))
                {
                    int num2 = ParseNCName(s, offset + 1);
                    if (num2 != 0)
                    {
                        colonOffset = offset;
                        num += num2 + 1;
                    }
                }
            }
            return num;
        }

        internal static void ParseQNameThrow(string s, out string prefix, out string localName)
        {
            int num;
            int offsetBadChar = ParseQName(s, 0, out num);
            if ((offsetBadChar == 0) || (offsetBadChar != s.Length))
            {
                ThrowInvalidName(s, 0, offsetBadChar);
            }
            if (num != 0)
            {
                prefix = s.Substring(0, num);
                localName = s.Substring(num + 1);
            }
            else
            {
                prefix = "";
                localName = s;
            }
        }

        internal static void SplitQName(string name, out string prefix, out string lname)
        {
            int index = name.IndexOf(':');
            if (-1 == index)
            {
                prefix = string.Empty;
                lname = name;
            }
            else
            {
                if ((index == 0) || ((name.Length - 1) == index))
                {
                    throw new ArgumentException(Res.GetString("Xml_BadNameChar", XmlException.BuildCharExceptionArgs(':', '\0')), "name");
                }
                prefix = name.Substring(0, index);
                index++;
                lname = name.Substring(index, name.Length - index);
            }
        }

        internal static bool StartsWithXml(string s)
        {
            if (s.Length < 3)
            {
                return false;
            }
            if ((s[0] != 'x') && (s[0] != 'X'))
            {
                return false;
            }
            if ((s[1] != 'm') && (s[1] != 'M'))
            {
                return false;
            }
            if ((s[2] != 'l') && (s[2] != 'L'))
            {
                return false;
            }
            return true;
        }

        internal static void ThrowInvalidName(string s, int offsetStartChar, int offsetBadChar)
        {
            if (offsetStartChar >= s.Length)
            {
                throw new XmlException("Xml_EmptyName", string.Empty);
            }
            if (xmlCharType.IsNCNameSingleChar(s[offsetBadChar]) && !XmlCharType.Instance.IsStartNCNameSingleChar(s[offsetBadChar]))
            {
                throw new XmlException("Xml_BadStartNameChar", XmlException.BuildCharExceptionArgs(s, offsetBadChar));
            }
            throw new XmlException("Xml_BadNameChar", XmlException.BuildCharExceptionArgs(s, offsetBadChar));
        }

        internal static bool ValidateName(string prefix, string localName, string ns, XPathNodeType nodeKind, Flags flags)
        {
            return ValidateNameInternal(prefix, localName, ns, nodeKind, flags, false);
        }

        private static bool ValidateNameInternal(string prefix, string localName, string ns, XPathNodeType nodeKind, Flags flags, bool throwOnError)
        {
            if ((flags & Flags.NCNames) != ((Flags) 0))
            {
                if ((prefix.Length != 0) && !ParseNCNameInternal(prefix, throwOnError))
                {
                    return false;
                }
                if ((localName.Length != 0) && !ParseNCNameInternal(localName, throwOnError))
                {
                    return false;
                }
            }
            if ((flags & Flags.CheckLocalName) != ((Flags) 0))
            {
                switch (nodeKind)
                {
                    case XPathNodeType.Element:
                        break;

                    case XPathNodeType.Attribute:
                        if ((ns.Length == 0) && localName.Equals("xmlns"))
                        {
                            if (throwOnError)
                            {
                                throw new XmlException("XmlBadName", new string[] { nodeKind.ToString(), localName });
                            }
                            return false;
                        }
                        break;

                    case XPathNodeType.ProcessingInstruction:
                        if ((localName.Length != 0) && ((localName.Length != 3) || !StartsWithXml(localName)))
                        {
                            goto Label_0102;
                        }
                        if (throwOnError)
                        {
                            throw new XmlException("Xml_InvalidPIName", localName);
                        }
                        return false;

                    default:
                        if (localName.Length != 0)
                        {
                            if (throwOnError)
                            {
                                throw new XmlException("XmlNoNameAllowed", nodeKind.ToString());
                            }
                            return false;
                        }
                        goto Label_0102;
                }
                if (localName.Length == 0)
                {
                    if (throwOnError)
                    {
                        throw new XmlException("Xdom_Empty_LocalName", string.Empty);
                    }
                    return false;
                }
            }
        Label_0102:
            if ((flags & Flags.CheckPrefixMapping) != ((Flags) 0))
            {
                switch (nodeKind)
                {
                    case XPathNodeType.Element:
                    case XPathNodeType.Attribute:
                    case XPathNodeType.Namespace:
                        if (ns.Length == 0)
                        {
                            if (prefix.Length != 0)
                            {
                                if (throwOnError)
                                {
                                    throw new XmlException("Xml_PrefixForEmptyNs", string.Empty);
                                }
                                return false;
                            }
                            goto Label_025E;
                        }
                        if ((prefix.Length == 0) && (nodeKind == XPathNodeType.Attribute))
                        {
                            if (throwOnError)
                            {
                                throw new XmlException("XmlBadName", new string[] { nodeKind.ToString(), localName });
                            }
                            return false;
                        }
                        if (prefix.Equals("xml"))
                        {
                            if (!ns.Equals("http://www.w3.org/XML/1998/namespace"))
                            {
                                if (throwOnError)
                                {
                                    throw new XmlException("Xml_XmlPrefix", string.Empty);
                                }
                                return false;
                            }
                            goto Label_025E;
                        }
                        if (prefix.Equals("xmlns"))
                        {
                            if (throwOnError)
                            {
                                throw new XmlException("Xml_XmlnsPrefix", string.Empty);
                            }
                            return false;
                        }
                        if (!IsReservedNamespace(ns))
                        {
                            goto Label_025E;
                        }
                        if (throwOnError)
                        {
                            throw new XmlException("Xml_NamespaceDeclXmlXmlns", string.Empty);
                        }
                        return false;

                    case XPathNodeType.ProcessingInstruction:
                        if ((prefix.Length == 0) && (ns.Length == 0))
                        {
                            goto Label_025E;
                        }
                        if (throwOnError)
                        {
                            throw new XmlException("Xml_InvalidPIName", CreateName(prefix, localName));
                        }
                        return false;
                }
                if ((prefix.Length != 0) || (ns.Length != 0))
                {
                    if (throwOnError)
                    {
                        throw new XmlException("XmlNoNameAllowed", nodeKind.ToString());
                    }
                    return false;
                }
            }
        Label_025E:
            return true;
        }

        internal static void ValidateNameThrow(string prefix, string localName, string ns, XPathNodeType nodeKind, Flags flags)
        {
            ValidateNameInternal(prefix, localName, ns, nodeKind, flags, true);
        }

        internal enum Flags
        {
            All = 7,
            AllExceptNCNames = 6,
            AllExceptPrefixMapping = 3,
            CheckLocalName = 2,
            CheckPrefixMapping = 4,
            NCNames = 1
        }
    }
}

