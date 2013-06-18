namespace System.Data.Design
{
    using System;
    using System.Collections;
    using System.Design;

    internal class SourceCollection : DataSourceCollectionBase, ICloneable
    {
        internal SourceCollection(DataSourceComponent collectionHost) : base(collectionHost)
        {
        }

        public int Add(Source s)
        {
            return base.List.Add(s);
        }

        public object Clone()
        {
            SourceCollection sources = new SourceCollection(null);
            foreach (Source source in this)
            {
                sources.Add((Source) source.Clone());
            }
            return sources;
        }

        public bool Contains(Source s)
        {
            return base.List.Contains(s);
        }

        private bool DbSourceNameExist(DbSource dbSource, bool isFillName, string nameToBeChecked)
        {
            if (isFillName && StringUtil.EqualValue(nameToBeChecked, dbSource.GetMethodName, true))
            {
                return true;
            }
            if (!isFillName && StringUtil.EqualValue(nameToBeChecked, dbSource.FillMethodName, true))
            {
                return true;
            }
            foreach (DbSource source in this)
            {
                if ((source != dbSource) && source.NameExist(nameToBeChecked))
                {
                    return true;
                }
            }
            DbSource mainSource = this.MainSource;
            return (((dbSource != mainSource) && (mainSource != null)) && mainSource.NameExist(nameToBeChecked));
        }

        protected internal override IDataSourceNamedObject FindObject(string name)
        {
            DbSource mainSource = this.MainSource;
            if ((mainSource != null) && mainSource.NameExist(name))
            {
                return mainSource;
            }
            IEnumerator enumerator = base.InnerList.GetEnumerator();
            while (enumerator.MoveNext())
            {
                DbSource current = enumerator.Current as DbSource;
                if (current != null)
                {
                    if (current.NameExist(name))
                    {
                        return current;
                    }
                }
                else
                {
                    IDataSourceNamedObject obj2 = (IDataSourceNamedObject) enumerator.Current;
                    if (StringUtil.EqualValue(obj2.Name, name, false))
                    {
                        return obj2;
                    }
                }
            }
            return null;
        }

        public int IndexOf(Source s)
        {
            return base.List.IndexOf(s);
        }

        public void Remove(Source s)
        {
            base.List.Remove(s);
        }

        protected internal override void ValidateName(IDataSourceNamedObject obj)
        {
            DbSource source = obj as DbSource;
            if (source != null)
            {
                if ((source.GenerateMethods & GenerateMethodTypes.Get) == GenerateMethodTypes.Get)
                {
                    this.NameService.ValidateName(source.GetMethodName);
                }
                if ((source.GenerateMethods & GenerateMethodTypes.Fill) == GenerateMethodTypes.Fill)
                {
                    this.NameService.ValidateName(source.FillMethodName);
                }
            }
            else
            {
                base.ValidateName(obj);
            }
        }

        private void ValidateNameWithMainSource(object dbSourceToCheck, string nameToCheck)
        {
            DbSource mainSource = this.MainSource;
            if (((dbSourceToCheck != mainSource) && (mainSource != null)) && mainSource.NameExist(nameToCheck))
            {
                throw new NameValidationException(System.Design.SR.GetString("CM_NameExist", new object[] { nameToCheck }));
            }
        }

        internal void ValidateUniqueDbSourceName(DbSource dbSource, string proposedName, bool isFillName)
        {
            if (this.DbSourceNameExist(dbSource, isFillName, proposedName))
            {
                throw new NameValidationException(System.Design.SR.GetString("CM_NameExist", new object[] { proposedName }));
            }
            this.NameService.ValidateName(proposedName);
        }

        protected internal override void ValidateUniqueName(IDataSourceNamedObject obj, string proposedName)
        {
            this.ValidateNameWithMainSource(obj, proposedName);
            base.ValidateUniqueName(obj, proposedName);
        }

        protected override Type ItemType
        {
            get
            {
                return typeof(Source);
            }
        }

        private DbSource MainSource
        {
            get
            {
                DesignTable collectionHost = this.CollectionHost as DesignTable;
                return (collectionHost.MainSource as DbSource);
            }
        }

        protected override INameService NameService
        {
            get
            {
                return SourceNameService.DefaultInstance;
            }
        }
    }
}

