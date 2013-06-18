namespace System.Web.Services.Discovery
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Web.Services.Protocols;

    internal class LinkGrep
    {
        private static readonly Regex commentRegex = new Regex(@"\G<!--(?>[^-]*-)+?->");
        private static readonly Regex doctypeDirectiveRegex = new Regex("\\G<!doctype\\b(([\\s\\w]+)|(\".*\"))*>", RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline | RegexOptions.IgnoreCase);
        private static readonly Regex endtagRegex = new Regex(@"\G</(?<prefix>[\w:-]+(?=:)|):?(?<tagname>[\w-]+)\s*>");
        private static readonly Regex tagRegex = new Regex("\\G<(?<prefix>[\\w:.-]+(?=:)|):?(?<tagname>[\\w.-]+)(?:\\s+(?<attrprefix>[\\w:.-]+(?=:)|):?(?<attrname>[\\w.-]+)\\s*=\\s*(?:\"(?<attrval>[^\"]*)\"|'(?<attrval>[^']*)'|(?<attrval>[a-zA-Z0-9\\-._:]+)))*\\s*(?<empty>/)?>");
        private static readonly Regex textRegex = new Regex(@"\G[^<]+");
        private static readonly Regex whitespaceRegex = new Regex(@"\G\s+(?=<|\Z)");

        private LinkGrep()
        {
        }

        private static string ReadEntireStream(TextReader input)
        {
            int num2;
            char[] buffer = new char[0x1000];
            int index = 0;
        Label_000D:
            num2 = input.Read(buffer, index, buffer.Length - index);
            if (num2 != 0)
            {
                index += num2;
                if (index == buffer.Length)
                {
                    char[] destinationArray = new char[buffer.Length * 2];
                    Array.Copy(buffer, 0, destinationArray, 0, buffer.Length);
                    buffer = destinationArray;
                }
                goto Label_000D;
            }
            return new string(buffer, 0, index);
        }

        internal static string SearchForLink(Stream stream)
        {
            string input = null;
            Match match;
            bool flag;
            input = ReadEntireStream(new StreamReader(stream));
            int startat = 0;
            if ((match = doctypeDirectiveRegex.Match(input, startat)).Success)
            {
                startat += match.Length;
            }
        Label_002E:
            flag = false;
            if ((match = whitespaceRegex.Match(input, startat)).Success)
            {
                flag = true;
            }
            else if ((match = textRegex.Match(input, startat)).Success)
            {
                flag = true;
            }
            startat += match.Length;
            if (startat == input.Length)
            {
                goto Label_01F0;
            }
            if ((match = tagRegex.Match(input, startat)).Success)
            {
                flag = true;
                string strA = match.Groups["tagname"].Value;
                if (string.Compare(strA, "link", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    CaptureCollection captures = match.Groups["attrname"].Captures;
                    CaptureCollection captures2 = match.Groups["attrval"].Captures;
                    int count = captures.Count;
                    bool flag2 = false;
                    bool flag3 = false;
                    string str3 = null;
                    for (int i = 0; i < count; i++)
                    {
                        string str4 = captures[i].ToString();
                        string contentType = captures2[i].ToString();
                        if ((string.Compare(str4, "type", StringComparison.OrdinalIgnoreCase) == 0) && ContentType.MatchesBase(contentType, "text/xml"))
                        {
                            flag2 = true;
                        }
                        else if ((string.Compare(str4, "rel", StringComparison.OrdinalIgnoreCase) == 0) && (string.Compare(contentType, "alternate", StringComparison.OrdinalIgnoreCase) == 0))
                        {
                            flag3 = true;
                        }
                        else if (string.Compare(str4, "href", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            str3 = contentType;
                        }
                        if ((flag2 && flag3) && (str3 != null))
                        {
                            return str3;
                        }
                    }
                    goto Label_01D8;
                }
                if (!(strA == "body"))
                {
                    goto Label_01D8;
                }
                goto Label_01F0;
            }
            if ((match = endtagRegex.Match(input, startat)).Success)
            {
                flag = true;
            }
            else if ((match = commentRegex.Match(input, startat)).Success)
            {
                flag = true;
            }
        Label_01D8:
            startat += match.Length;
            if ((startat != input.Length) && flag)
            {
                goto Label_002E;
            }
        Label_01F0:
            return null;
        }
    }
}

