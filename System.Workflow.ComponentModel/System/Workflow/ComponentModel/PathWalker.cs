namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    internal class PathWalker
    {
        public EventHandler<PathMemberInfoEventArgs> MemberFound;
        public EventHandler<PathErrorInfoEventArgs> PathErrorFound;

        private static MemberInfo[] PopulateMembers(Type type, string memberName)
        {
            List<MemberInfo> list = new List<MemberInfo>();
            list.AddRange(type.GetMember(memberName, MemberTypes.Property | MemberTypes.Method | MemberTypes.Field | MemberTypes.Event, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance));
            if (type.IsInterface)
            {
                foreach (Type type2 in type.GetInterfaces())
                {
                    list.AddRange(type2.GetMember(memberName, MemberTypes.Property | MemberTypes.Method | MemberTypes.Field | MemberTypes.Event, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance));
                }
            }
            return list.ToArray();
        }

        public bool TryWalkPropertyPath(Type rootType, string path)
        {
            if (rootType == null)
            {
                throw new ArgumentNullException("rootType");
            }
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }
            Type type = rootType;
            string currentPath = string.Empty;
            PathParser parser = new PathParser();
            List<SourceValueInfo> list = parser.Parse(path, true);
            string error = parser.Error;
            for (int i = 0; i < list.Count; i++)
            {
                MemberInfo[] infoArray;
                PropertyInfo info7;
                PathMemberInfoEventArgs args5;
                SourceValueInfo info = list[i];
                if (string.IsNullOrEmpty(info.name))
                {
                    if (this.PathErrorFound != null)
                    {
                        this.PathErrorFound(this, new PathErrorInfoEventArgs(info, currentPath));
                    }
                    return false;
                }
                string str3 = (info.type == SourceValueType.Property) ? info.name : ("[" + info.name + "]");
                string str4 = string.IsNullOrEmpty(currentPath) ? str3 : (currentPath + ((info.type == SourceValueType.Property) ? "." : string.Empty) + str3);
                Type propertyType = null;
                MemberInfo memberInfo = null;
                switch (info.type)
                {
                    case SourceValueType.Property:
                        infoArray = PopulateMembers(type, info.name);
                        if (((infoArray != null) && (infoArray.Length != 0)) && (infoArray[0] != null))
                        {
                            break;
                        }
                        if (this.PathErrorFound != null)
                        {
                            this.PathErrorFound(this, new PathErrorInfoEventArgs(info, currentPath));
                        }
                        return false;

                    case SourceValueType.Indexer:
                    {
                        if (string.IsNullOrEmpty(info.name))
                        {
                            goto Label_04AC;
                        }
                        string[] aryArgName = info.name.Split(new char[] { ',' });
                        object[] objArray2 = new object[aryArgName.Length];
                        info7 = BindHelpers.GetMatchedPropertyInfo(type, aryArgName, objArray2);
                        if (info7 == null)
                        {
                            goto Label_048E;
                        }
                        if (this.MemberFound == null)
                        {
                            goto Label_046B;
                        }
                        args5 = new PathMemberInfoEventArgs(str4, type, info7, PathMemberKind.Index, i == (list.Count - 1), objArray2);
                        this.MemberFound(this, args5);
                        if (args5.Action != PathWalkAction.Cancel)
                        {
                            goto Label_045F;
                        }
                        return false;
                    }
                    default:
                        goto Label_04CA;
                }
                memberInfo = infoArray[0];
                if ((memberInfo is EventInfo) || (memberInfo is MethodInfo))
                {
                    if (this.MemberFound != null)
                    {
                        PathMemberInfoEventArgs e = new PathMemberInfoEventArgs(str4, type, memberInfo, PathMemberKind.Event, i == (list.Count - 1));
                        this.MemberFound(this, e);
                        if (e.Action == PathWalkAction.Cancel)
                        {
                            return false;
                        }
                        if (e.Action == PathWalkAction.Stop)
                        {
                            return true;
                        }
                    }
                    return string.IsNullOrEmpty(error);
                }
                if (memberInfo is PropertyInfo)
                {
                    PropertyInfo originalPropertyInfo = memberInfo as PropertyInfo;
                    MethodInfo getMethod = originalPropertyInfo.GetGetMethod();
                    MethodInfo setMethod = originalPropertyInfo.GetSetMethod();
                    ActivityBindPropertyInfo info6 = new ActivityBindPropertyInfo(type, getMethod, setMethod, originalPropertyInfo.Name, originalPropertyInfo);
                    propertyType = info6.PropertyType;
                    if (info6.GetIndexParameters().Length <= 0)
                    {
                        if (this.MemberFound != null)
                        {
                            PathMemberInfoEventArgs args3 = new PathMemberInfoEventArgs(str4, type, info6, PathMemberKind.Property, i == (list.Count - 1));
                            this.MemberFound(this, args3);
                            if (args3.Action == PathWalkAction.Cancel)
                            {
                                return false;
                            }
                            if (args3.Action == PathWalkAction.Stop)
                            {
                                return true;
                            }
                        }
                    }
                    else
                    {
                        if (((i >= (list.Count - 1)) || (list[i + 1].type != SourceValueType.Indexer)) || string.IsNullOrEmpty(list[i + 1].name))
                        {
                            if (this.PathErrorFound != null)
                            {
                                this.PathErrorFound(this, new PathErrorInfoEventArgs(info, currentPath));
                            }
                            return false;
                        }
                        string[] argNames = list[i + 1].name.Split(new char[] { ',' });
                        object[] args = new object[argNames.Length];
                        if (!BindHelpers.MatchIndexerParameters(info6, argNames, args))
                        {
                            if (this.PathErrorFound != null)
                            {
                                this.PathErrorFound(this, new PathErrorInfoEventArgs(info, currentPath));
                            }
                            return false;
                        }
                        str4 = str4 + "[" + list[i + 1].name + "]";
                        if (this.MemberFound != null)
                        {
                            PathMemberInfoEventArgs args2 = new PathMemberInfoEventArgs(str4, type, info6, PathMemberKind.IndexedProperty, i == (list.Count - 2), args);
                            this.MemberFound(this, args2);
                            if (args2.Action == PathWalkAction.Cancel)
                            {
                                return false;
                            }
                            if (args2.Action == PathWalkAction.Stop)
                            {
                                return true;
                            }
                        }
                        i++;
                    }
                }
                else
                {
                    if (this.MemberFound != null)
                    {
                        PathMemberInfoEventArgs args4 = new PathMemberInfoEventArgs(str4, type, memberInfo, PathMemberKind.Field, i == (list.Count - 1));
                        this.MemberFound(this, args4);
                        if (args4.Action == PathWalkAction.Cancel)
                        {
                            return false;
                        }
                        if (args4.Action == PathWalkAction.Stop)
                        {
                            return true;
                        }
                    }
                    propertyType = (memberInfo as FieldInfo).FieldType;
                }
                goto Label_04CA;
            Label_045F:
                if (args5.Action == PathWalkAction.Stop)
                {
                    return true;
                }
            Label_046B:
                propertyType = info7.PropertyType;
                if (propertyType == null)
                {
                    propertyType = info7.GetGetMethod().ReturnType;
                }
                goto Label_04CA;
            Label_048E:
                if (this.PathErrorFound != null)
                {
                    this.PathErrorFound(this, new PathErrorInfoEventArgs(info, currentPath));
                }
                return false;
            Label_04AC:
                if (this.PathErrorFound != null)
                {
                    this.PathErrorFound(this, new PathErrorInfoEventArgs(info, currentPath));
                }
                return false;
            Label_04CA:
                type = propertyType;
                currentPath = str4;
            }
            return string.IsNullOrEmpty(error);
        }
    }
}

