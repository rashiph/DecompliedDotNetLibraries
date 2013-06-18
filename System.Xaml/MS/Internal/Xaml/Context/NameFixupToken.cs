namespace MS.Internal.Xaml.Context
{
    using MS.Internal.Xaml.Runtime;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;
    using System.Xaml;

    internal class NameFixupToken : IAddLineInfo
    {
        private List<string> _names = new List<string>();
        private List<INameScopeDictionary> _nameScopeDictionaryList = new List<INameScopeDictionary>();
        private XamlRuntime _runtime;
        private ObjectWriterContext _targetContext;

        public NameFixupToken()
        {
            this.Target = new FixupTarget();
            this.Target.TemporaryCollectionIndex = -1;
            this.Target.InstanceIsOnTheStack = true;
        }

        XamlException IAddLineInfo.WithLineInfo(XamlException ex)
        {
            if (this.LineNumber > 0)
            {
                ex.SetLineInfo(this.LineNumber, this.LinePosition);
            }
            return ex;
        }

        internal object ResolveName(string name)
        {
            object obj2 = null;
            bool flag;
            if (this.CanAssignDirectly)
            {
                foreach (INameScopeDictionary dictionary in this.NameScopeDictionaryList)
                {
                    obj2 = dictionary.FindName(name);
                    if (obj2 != null)
                    {
                        return obj2;
                    }
                }
                return obj2;
            }
            this.TargetContext.IsInitializedCallback = null;
            return this.TargetContext.ResolveName(name, out flag);
        }

        public bool CanAssignDirectly { get; set; }

        public MS.Internal.Xaml.Context.FixupType FixupType { get; set; }

        public int LineNumber { get; set; }

        public int LinePosition { get; set; }

        public List<INameScopeDictionary> NameScopeDictionaryList
        {
            get
            {
                return this._nameScopeDictionaryList;
            }
        }

        public List<string> NeededNames
        {
            get
            {
                return this._names;
            }
        }

        public object ReferencedObject { get; set; }

        public XamlRuntime Runtime
        {
            get
            {
                return this._runtime;
            }
            set
            {
                this._runtime = value;
            }
        }

        public XamlSavedContext SavedContext { get; set; }

        public FixupTarget Target { get; set; }

        public ObjectWriterContext TargetContext
        {
            get
            {
                if (this._targetContext == null)
                {
                    this._targetContext = new ObjectWriterContext(this.SavedContext, null, null, this.Runtime);
                }
                return this._targetContext;
            }
        }
    }
}

