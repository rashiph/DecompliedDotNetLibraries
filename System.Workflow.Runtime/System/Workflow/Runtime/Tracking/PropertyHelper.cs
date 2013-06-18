namespace System.Workflow.Runtime.Tracking
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Workflow.ComponentModel;
    using System.Workflow.Runtime;

    internal sealed class PropertyHelper
    {
        private PropertyHelper()
        {
        }

        internal static void GetAllMembers(Activity activity, IList<TrackingDataItem> items, TrackingAnnotationCollection annotations)
        {
            Type type = activity.GetType();
            foreach (FieldInfo info in type.GetFields(BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
            {
                if (!IsInternalVariable(info.Name))
                {
                    TrackingDataItem item = new TrackingDataItem {
                        FieldName = info.Name,
                        Data = GetRuntimeValue(info.GetValue(activity), activity)
                    };
                    foreach (string str in annotations)
                    {
                        item.Annotations.Add(str);
                    }
                    items.Add(item);
                }
            }
            foreach (PropertyInfo info2 in type.GetProperties(BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
            {
                if (!IsInternalVariable(info2.Name) && (info2.GetIndexParameters().Length <= 0))
                {
                    TrackingDataItem item2 = new TrackingDataItem {
                        FieldName = info2.Name,
                        Data = GetRuntimeValue(info2.GetValue(activity, null), activity)
                    };
                    foreach (string str2 in annotations)
                    {
                        item2.Annotations.Add(str2);
                    }
                    items.Add(item2);
                }
            }
        }

        internal static void GetEnumerationMember(IEnumerable collection, int index, out object obj)
        {
            obj = null;
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            IEnumerator enumerator = collection.GetEnumerator();
            int num = 0;
            while (enumerator.MoveNext())
            {
                if (num++ == index)
                {
                    obj = enumerator.Current;
                    return;
                }
            }
            throw new IndexOutOfRangeException();
        }

        internal static object GetProperty(string name, object obj)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            string[] strArray = name.Split(new char[] { '.' });
            object o = obj;
            for (int i = 0; i < strArray.Length; i++)
            {
                if ((strArray[i] == null) || (strArray[i].Length == 0))
                {
                    throw new InvalidOperationException(ExecutionStringManager.TrackingProfileInvalidMember);
                }
                object obj3 = null;
                GetPropertyOrField(strArray[i], o, out obj3);
                if (o is Activity)
                {
                    o = GetRuntimeValue(obj3, (Activity) o);
                }
                else
                {
                    o = obj3;
                }
            }
            return o;
        }

        internal static void GetProperty(string name, Activity activity, TrackingAnnotationCollection annotations, out TrackingDataItem item)
        {
            item = null;
            object property = GetProperty(name, activity);
            item = new TrackingDataItem();
            item.FieldName = name;
            item.Data = property;
            foreach (string str in annotations)
            {
                item.Annotations.Add(str);
            }
        }

        internal static void GetPropertyOrField(string name, object o, out object obj)
        {
            obj = null;
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (o == null)
            {
                throw new ArgumentNullException("o");
            }
            Type type = o.GetType();
            string str = null;
            string realName = null;
            bool flag = false;
            int index = -1;
            if (TryParseIndex(name, out str, out index))
            {
                flag = true;
            }
            else
            {
                str = name;
            }
            object obj2 = null;
            if ((str != null) && (str.Length > 0))
            {
                if (!NameIsDefined(str, o, out realName))
                {
                    throw new MissingMemberException(o.GetType().Name, str);
                }
                obj2 = type.InvokeMember(realName, BindingFlags.GetProperty | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, o, null, CultureInfo.InvariantCulture);
            }
            else
            {
                obj2 = o;
            }
            if (flag)
            {
                IEnumerable collection = obj2 as IEnumerable;
                if (collection != null)
                {
                    GetEnumerationMember(collection, index, out obj);
                }
            }
            else
            {
                obj = obj2;
            }
        }

        internal static object GetRuntimeValue(object o, Activity activity)
        {
            if (o == null)
            {
                return o;
            }
            object obj2 = o;
            if (o is ActivityBind)
            {
                if (activity == null)
                {
                    throw new ArgumentNullException("activity");
                }
                return ((ActivityBind) o).GetRuntimeValue(activity);
            }
            if (o is WorkflowParameterBinding)
            {
                obj2 = ((WorkflowParameterBinding) o).Value;
            }
            return obj2;
        }

        private static bool IsInternalVariable(string name)
        {
            string[] strArray = new string[] { "__winoe_ActivityLocks_", "__winoe_StaticActivityLocks_", "__winoe_MethodLocks_" };
            foreach (string str in strArray)
            {
                if (string.Compare(str, name, StringComparison.Ordinal) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool NameIsDefined(string name, object o, out string realName)
        {
            realName = null;
            Type type = o.GetType();
            MemberInfo[] member = type.GetMember(name, BindingFlags.GetProperty | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            if ((member == null) || (member.Length == 0))
            {
                member = type.GetMember(name, BindingFlags.GetProperty | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if ((member == null) || (member.Length == 0))
                {
                    return false;
                }
            }
            if (((member == null) || (member.Length == 0)) || ((member[0].Name == null) || (member[0].Name.Length == 0)))
            {
                return false;
            }
            realName = member[0].Name;
            return true;
        }

        private static bool TryParseIndex(string fullName, out string name, out int index)
        {
            name = null;
            index = -1;
            int num = -1;
            int length = -1;
            for (int i = fullName.Length - 1; i > 0; i--)
            {
                if ((']' == fullName[i]) && (-1 == num))
                {
                    num = i;
                }
                else if (('[' == fullName[i]) && (-1 == length))
                {
                    length = i;
                    break;
                }
            }
            if ((-1 == num) || (-1 == length))
            {
                return false;
            }
            string s = fullName.Substring(length + 1, (num - 1) - length);
            name = fullName.Substring(0, length);
            return int.TryParse(s, out index);
        }
    }
}

