namespace System.Web.Services.Protocols
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Web.Services;

    internal class MatchMember
    {
        private int capture;
        private int group;
        private MatchType matchType;
        private int maxRepeats;
        private MemberInfo memberInfo;
        private Regex regex;

        private static Exception BadCaptureIndexException(int index, string matchName, int highestIndex)
        {
            return new Exception(Res.GetString("WebTextMatchBadCaptureIndex", new object[] { index, matchName, highestIndex }));
        }

        private static Exception BadGroupIndexException(int index, string matchName, int highestIndex)
        {
            return new Exception(Res.GetString("WebTextMatchBadGroupIndex", new object[] { index, matchName, highestIndex }));
        }

        internal void Match(object target, string text)
        {
            if (this.memberInfo is FieldInfo)
            {
                ((FieldInfo) this.memberInfo).SetValue(target, (this.matchType == null) ? this.MatchString(text) : this.MatchClass(text));
            }
            else if (this.memberInfo is PropertyInfo)
            {
                ((PropertyInfo) this.memberInfo).SetValue(target, (this.matchType == null) ? this.MatchString(text) : this.MatchClass(text), new object[0]);
            }
        }

        private object MatchClass(string text)
        {
            System.Text.RegularExpressions.Match match = this.regex.Match(text);
            Type type = (this.memberInfo is FieldInfo) ? ((FieldInfo) this.memberInfo).FieldType : ((PropertyInfo) this.memberInfo).PropertyType;
            if (type.IsArray)
            {
                ArrayList list = new ArrayList();
                for (int i = 0; match.Success && (i < this.maxRepeats); i++)
                {
                    if (match.Groups.Count <= this.group)
                    {
                        throw BadGroupIndexException(this.group, this.memberInfo.Name, match.Groups.Count - 1);
                    }
                    Group group = match.Groups[this.group];
                    foreach (Capture capture in group.Captures)
                    {
                        list.Add(this.matchType.Match(text.Substring(capture.Index, capture.Length)));
                    }
                    match = match.NextMatch();
                }
                return list.ToArray(this.matchType.Type);
            }
            if (match.Success)
            {
                if (match.Groups.Count <= this.group)
                {
                    throw BadGroupIndexException(this.group, this.memberInfo.Name, match.Groups.Count - 1);
                }
                Group group2 = match.Groups[this.group];
                if (group2.Captures.Count > 0)
                {
                    if (group2.Captures.Count <= this.capture)
                    {
                        throw BadCaptureIndexException(this.capture, this.memberInfo.Name, group2.Captures.Count - 1);
                    }
                    Capture capture2 = group2.Captures[this.capture];
                    return this.matchType.Match(text.Substring(capture2.Index, capture2.Length));
                }
            }
            return null;
        }

        private object MatchString(string text)
        {
            System.Text.RegularExpressions.Match match = this.regex.Match(text);
            Type type = (this.memberInfo is FieldInfo) ? ((FieldInfo) this.memberInfo).FieldType : ((PropertyInfo) this.memberInfo).PropertyType;
            if (type.IsArray)
            {
                ArrayList list = new ArrayList();
                for (int i = 0; match.Success && (i < this.maxRepeats); i++)
                {
                    if (match.Groups.Count <= this.group)
                    {
                        throw BadGroupIndexException(this.group, this.memberInfo.Name, match.Groups.Count - 1);
                    }
                    Group group = match.Groups[this.group];
                    foreach (Capture capture in group.Captures)
                    {
                        list.Add(text.Substring(capture.Index, capture.Length));
                    }
                    match = match.NextMatch();
                }
                return list.ToArray(typeof(string));
            }
            if (match.Success)
            {
                if (match.Groups.Count <= this.group)
                {
                    throw BadGroupIndexException(this.group, this.memberInfo.Name, match.Groups.Count - 1);
                }
                Group group2 = match.Groups[this.group];
                if (group2.Captures.Count > 0)
                {
                    if (group2.Captures.Count <= this.capture)
                    {
                        throw BadCaptureIndexException(this.capture, this.memberInfo.Name, group2.Captures.Count - 1);
                    }
                    Capture capture2 = group2.Captures[this.capture];
                    return text.Substring(capture2.Index, capture2.Length);
                }
            }
            return null;
        }

        internal static MatchMember Reflect(MemberInfo memberInfo)
        {
            Type propertyType = null;
            if (memberInfo is PropertyInfo)
            {
                PropertyInfo info = (PropertyInfo) memberInfo;
                if (!info.CanRead)
                {
                    return null;
                }
                if (!info.CanWrite)
                {
                    return null;
                }
                MethodInfo getMethod = info.GetGetMethod();
                if (getMethod.IsStatic)
                {
                    return null;
                }
                if (getMethod.GetParameters().Length > 0)
                {
                    return null;
                }
                propertyType = info.PropertyType;
            }
            if (memberInfo is FieldInfo)
            {
                FieldInfo info3 = (FieldInfo) memberInfo;
                if (!info3.IsPublic)
                {
                    return null;
                }
                if (info3.IsStatic)
                {
                    return null;
                }
                if (info3.IsSpecialName)
                {
                    return null;
                }
                propertyType = info3.FieldType;
            }
            object[] customAttributes = memberInfo.GetCustomAttributes(typeof(MatchAttribute), false);
            if (customAttributes.Length == 0)
            {
                return null;
            }
            MatchAttribute attribute = (MatchAttribute) customAttributes[0];
            MatchMember member = new MatchMember {
                regex = new Regex(attribute.Pattern, RegexOptions.Singleline | (attribute.IgnoreCase ? (RegexOptions.CultureInvariant | RegexOptions.IgnoreCase) : RegexOptions.None)),
                group = attribute.Group,
                capture = attribute.Capture,
                maxRepeats = attribute.MaxRepeats,
                memberInfo = memberInfo
            };
            if (member.maxRepeats < 0)
            {
                member.maxRepeats = propertyType.IsArray ? 0x7fffffff : 1;
            }
            if (propertyType.IsArray)
            {
                propertyType = propertyType.GetElementType();
            }
            if (propertyType != typeof(string))
            {
                member.matchType = MatchType.Reflect(propertyType);
            }
            return member;
        }
    }
}

