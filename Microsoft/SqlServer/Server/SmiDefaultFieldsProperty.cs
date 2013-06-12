namespace Microsoft.SqlServer.Server
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;

    internal class SmiDefaultFieldsProperty : SmiMetaDataProperty
    {
        private IList<bool> _defaults;

        internal SmiDefaultFieldsProperty(IList<bool> defaultFields)
        {
            this._defaults = new List<bool>(defaultFields).AsReadOnly();
        }

        [Conditional("DEBUG")]
        internal void CheckCount(int countToMatch)
        {
        }

        internal override string TraceString()
        {
            string str = "DefaultFields(";
            bool flag = false;
            for (int i = 0; i < this._defaults.Count; i++)
            {
                if (flag)
                {
                    str = str + ",";
                }
                else
                {
                    flag = true;
                }
                if (this._defaults[i])
                {
                    str = str + i;
                }
            }
            return (str + ")");
        }

        internal bool this[int ordinal]
        {
            get
            {
                if (this._defaults.Count <= ordinal)
                {
                    return false;
                }
                return this._defaults[ordinal];
            }
        }
    }
}

