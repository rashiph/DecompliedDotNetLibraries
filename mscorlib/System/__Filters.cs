namespace System
{
    using System.Reflection;

    [Serializable]
    internal class __Filters
    {
        internal virtual bool FilterAttribute(MemberInfo m, object filterCriteria)
        {
            MethodAttributes attributes2;
            if (filterCriteria == null)
            {
                throw new InvalidFilterCriteriaException(Environment.GetResourceString("RFLCT.FltCritInt"));
            }
            MemberTypes memberType = m.MemberType;
            if (memberType != MemberTypes.Constructor)
            {
                if (memberType == MemberTypes.Field)
                {
                    FieldAttributes attributes3 = FieldAttributes.PrivateScope;
                    try
                    {
                        int num2 = (int) filterCriteria;
                        attributes3 = (FieldAttributes) num2;
                    }
                    catch
                    {
                        throw new InvalidFilterCriteriaException(Environment.GetResourceString("RFLCT.FltCritInt"));
                    }
                    FieldAttributes attributes = ((FieldInfo) m).Attributes;
                    if (((attributes3 & FieldAttributes.FieldAccessMask) != FieldAttributes.PrivateScope) && ((attributes & FieldAttributes.FieldAccessMask) != (attributes3 & FieldAttributes.FieldAccessMask)))
                    {
                        return false;
                    }
                    if (((attributes3 & FieldAttributes.Static) != FieldAttributes.PrivateScope) && ((attributes & FieldAttributes.Static) == FieldAttributes.PrivateScope))
                    {
                        return false;
                    }
                    if (((attributes3 & FieldAttributes.InitOnly) != FieldAttributes.PrivateScope) && ((attributes & FieldAttributes.InitOnly) == FieldAttributes.PrivateScope))
                    {
                        return false;
                    }
                    if (((attributes3 & FieldAttributes.Literal) != FieldAttributes.PrivateScope) && ((attributes & FieldAttributes.Literal) == FieldAttributes.PrivateScope))
                    {
                        return false;
                    }
                    if (((attributes3 & FieldAttributes.NotSerialized) != FieldAttributes.PrivateScope) && ((attributes & FieldAttributes.NotSerialized) == FieldAttributes.PrivateScope))
                    {
                        return false;
                    }
                    if (((attributes3 & FieldAttributes.PinvokeImpl) != FieldAttributes.PrivateScope) && ((attributes & FieldAttributes.PinvokeImpl) == FieldAttributes.PrivateScope))
                    {
                        return false;
                    }
                    return true;
                }
                if (memberType != MemberTypes.Method)
                {
                    return false;
                }
            }
            MethodAttributes privateScope = MethodAttributes.PrivateScope;
            try
            {
                int num = (int) filterCriteria;
                privateScope = (MethodAttributes) num;
            }
            catch
            {
                throw new InvalidFilterCriteriaException(Environment.GetResourceString("RFLCT.FltCritInt"));
            }
            if (m.MemberType == MemberTypes.Method)
            {
                attributes2 = ((MethodInfo) m).Attributes;
            }
            else
            {
                attributes2 = ((ConstructorInfo) m).Attributes;
            }
            if (((privateScope & MethodAttributes.MemberAccessMask) != MethodAttributes.PrivateScope) && ((attributes2 & MethodAttributes.MemberAccessMask) != (privateScope & MethodAttributes.MemberAccessMask)))
            {
                return false;
            }
            if (((privateScope & MethodAttributes.Static) != MethodAttributes.PrivateScope) && ((attributes2 & MethodAttributes.Static) == MethodAttributes.PrivateScope))
            {
                return false;
            }
            if (((privateScope & MethodAttributes.Final) != MethodAttributes.PrivateScope) && ((attributes2 & MethodAttributes.Final) == MethodAttributes.PrivateScope))
            {
                return false;
            }
            if (((privateScope & MethodAttributes.Virtual) != MethodAttributes.PrivateScope) && ((attributes2 & MethodAttributes.Virtual) == MethodAttributes.PrivateScope))
            {
                return false;
            }
            if (((privateScope & MethodAttributes.Abstract) != MethodAttributes.PrivateScope) && ((attributes2 & MethodAttributes.Abstract) == MethodAttributes.PrivateScope))
            {
                return false;
            }
            if (((privateScope & MethodAttributes.SpecialName) != MethodAttributes.PrivateScope) && ((attributes2 & MethodAttributes.SpecialName) == MethodAttributes.PrivateScope))
            {
                return false;
            }
            return true;
        }

        internal virtual bool FilterIgnoreCase(MemberInfo m, object filterCriteria)
        {
            if ((filterCriteria == null) || !(filterCriteria is string))
            {
                throw new InvalidFilterCriteriaException(Environment.GetResourceString("RFLCT.FltCritString"));
            }
            string strB = (string) filterCriteria;
            strB = strB.Trim();
            string name = m.Name;
            if (m.MemberType == MemberTypes.NestedType)
            {
                name = name.Substring(name.LastIndexOf('+') + 1);
            }
            if ((strB.Length > 0) && (strB[strB.Length - 1] == '*'))
            {
                strB = strB.Substring(0, strB.Length - 1);
                return (string.Compare(name, 0, strB, 0, strB.Length, StringComparison.OrdinalIgnoreCase) == 0);
            }
            return (string.Compare(strB, name, StringComparison.OrdinalIgnoreCase) == 0);
        }

        internal virtual bool FilterName(MemberInfo m, object filterCriteria)
        {
            if ((filterCriteria == null) || !(filterCriteria is string))
            {
                throw new InvalidFilterCriteriaException(Environment.GetResourceString("RFLCT.FltCritString"));
            }
            string str = (string) filterCriteria;
            str = str.Trim();
            string name = m.Name;
            if (m.MemberType == MemberTypes.NestedType)
            {
                name = name.Substring(name.LastIndexOf('+') + 1);
            }
            if ((str.Length > 0) && (str[str.Length - 1] == '*'))
            {
                str = str.Substring(0, str.Length - 1);
                return name.StartsWith(str, StringComparison.Ordinal);
            }
            return name.Equals(str);
        }
    }
}

