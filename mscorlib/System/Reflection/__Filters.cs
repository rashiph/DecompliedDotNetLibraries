namespace System.Reflection
{
    using System;

    [Serializable]
    internal class __Filters
    {
        public virtual bool FilterTypeName(Type cls, object filterCriteria)
        {
            if ((filterCriteria == null) || !(filterCriteria is string))
            {
                throw new InvalidFilterCriteriaException(Environment.GetResourceString("RFLCT.FltCritString"));
            }
            string str = (string) filterCriteria;
            if ((str.Length > 0) && (str[str.Length - 1] == '*'))
            {
                str = str.Substring(0, str.Length - 1);
                return cls.Name.StartsWith(str, StringComparison.Ordinal);
            }
            return cls.Name.Equals(str);
        }

        public virtual bool FilterTypeNameIgnoreCase(Type cls, object filterCriteria)
        {
            if ((filterCriteria == null) || !(filterCriteria is string))
            {
                throw new InvalidFilterCriteriaException(Environment.GetResourceString("RFLCT.FltCritString"));
            }
            string strA = (string) filterCriteria;
            if ((strA.Length <= 0) || (strA[strA.Length - 1] != '*'))
            {
                return (string.Compare(strA, cls.Name, StringComparison.OrdinalIgnoreCase) == 0);
            }
            strA = strA.Substring(0, strA.Length - 1);
            string name = cls.Name;
            return ((name.Length >= strA.Length) && (string.Compare(name, 0, strA, 0, strA.Length, StringComparison.OrdinalIgnoreCase) == 0));
        }
    }
}

