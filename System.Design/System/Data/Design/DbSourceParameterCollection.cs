namespace System.Data.Design
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Globalization;
    using System.Reflection;

    internal class DbSourceParameterCollection : DataSourceCollectionBase, IDataParameterCollection, IList, ICollection, IEnumerable, ICloneable
    {
        internal DbSourceParameterCollection(DataSourceComponent collectionHost) : base(collectionHost)
        {
        }

        public object Clone()
        {
            DbSourceParameterCollection parameters = new DbSourceParameterCollection(null);
            foreach (DesignParameter parameter in this)
            {
                DesignParameter parameter2 = (DesignParameter) parameter.Clone();
                ((IList) parameters).Add(parameter2);
            }
            return parameters;
        }

        public bool Contains(string value)
        {
            return (this.IndexOf(value) != -1);
        }

        public int IndexOf(string parameterName)
        {
            int count = base.InnerList.Count;
            for (int i = 0; i < count; i++)
            {
                if (StringUtil.EqualValue(parameterName, ((IDbDataParameter) base.InnerList[i]).ParameterName))
                {
                    return i;
                }
            }
            return -1;
        }

        private int RangeCheck(string parameterName)
        {
            int index = this.IndexOf(parameterName);
            if (index < 0)
            {
                throw new InternalException(string.Format(CultureInfo.CurrentCulture, "No parameter named '{0}' found", new object[] { parameterName }), 0x4e24);
            }
            return index;
        }

        public void RemoveAt(string parameterName)
        {
            int index = this.RangeCheck(parameterName);
            base.List.RemoveAt(index);
        }

        public DesignParameter this[int index]
        {
            get
            {
                return (DesignParameter) base.List[index];
            }
        }

        protected override Type ItemType
        {
            get
            {
                return typeof(DesignParameter);
            }
        }

        protected override INameService NameService
        {
            get
            {
                return SimpleNameService.DefaultInstance;
            }
        }

        object IDataParameterCollection.this[string parameterName]
        {
            get
            {
                int num = this.RangeCheck(parameterName);
                return base.List[num];
            }
            set
            {
                int num = this.RangeCheck(parameterName);
                base.List[num] = value;
            }
        }
    }
}

