namespace MS.Internal.Xaml.Context
{
    using MS.Internal.Xaml;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Xaml;

    internal class XamlParserContext : XamlContext
    {
        private Dictionary<string, string> _prescopeNamespaces;
        private XamlContextStack<XamlParserFrame> _stack;

        public XamlParserContext(XamlSchemaContext schemaContext, Assembly localAssembly) : base(schemaContext)
        {
            this._stack = new XamlContextStack<XamlParserFrame>(() => new XamlParserFrame());
            this._prescopeNamespaces = new Dictionary<string, string>();
            base._localAssembly = localAssembly;
        }

        public override void AddNamespacePrefix(string prefix, string xamlNS)
        {
            this._prescopeNamespaces.Add(prefix, xamlNS);
        }

        public bool CurrentMemberIsWriteVisible()
        {
            Type accessingType = null;
            if (this.AllowProtectedMembersOnRoot && (this._stack.Depth == 1))
            {
                accessingType = this.CurrentType.UnderlyingType;
            }
            return this.CurrentMember.IsWriteVisibleTo(this.LocalAssembly, accessingType);
        }

        public override string FindNamespaceByPrefix(string prefix)
        {
            if (this.XmlNamespaceResolver != null)
            {
                return this.XmlNamespaceResolver(prefix);
            }
            return this.FindNamespaceByPrefixInParseStack(prefix);
        }

        public string FindNamespaceByPrefixInParseStack(string prefix)
        {
            string str;
            if ((this._prescopeNamespaces != null) && this._prescopeNamespaces.TryGetValue(prefix, out str))
            {
                return str;
            }
            for (XamlParserFrame frame = this._stack.CurrentFrame; frame.Depth > 0; frame = (XamlParserFrame) frame.Previous)
            {
                if (frame.TryGetNamespaceByPrefix(prefix, out str))
                {
                    return str;
                }
            }
            return null;
        }

        public override IEnumerable<NamespaceDeclaration> GetNamespacePrefixes()
        {
            XamlParserFrame currentFrame = this._stack.CurrentFrame;
            Dictionary<string, string> iteratorVariable1 = new Dictionary<string, string>();
            while (currentFrame.Depth > 0)
            {
                if (currentFrame._namespaces != null)
                {
                    foreach (NamespaceDeclaration iteratorVariable2 in currentFrame.GetNamespacePrefixes())
                    {
                        if (iteratorVariable1.ContainsKey(iteratorVariable2.Prefix))
                        {
                            continue;
                        }
                        iteratorVariable1.Add(iteratorVariable2.Prefix, null);
                        yield return iteratorVariable2;
                    }
                }
                currentFrame = (XamlParserFrame) currentFrame.Previous;
            }
            if (this._prescopeNamespaces != null)
            {
                foreach (KeyValuePair<string, string> iteratorVariable3 in this._prescopeNamespaces)
                {
                    if (iteratorVariable1.ContainsKey(iteratorVariable3.Key))
                    {
                        continue;
                    }
                    iteratorVariable1.Add(iteratorVariable3.Key, null);
                    yield return new NamespaceDeclaration(iteratorVariable3.Value, iteratorVariable3.Key);
                }
            }
        }

        internal override bool IsVisible(XamlMember member, XamlType rootObjectType)
        {
            if (member == null)
            {
                return false;
            }
            Type accessingType = null;
            if (this.AllowProtectedMembersOnRoot && (rootObjectType != null))
            {
                accessingType = rootObjectType.UnderlyingType;
            }
            if (member.IsWriteVisibleTo(this.LocalAssembly, accessingType))
            {
                return true;
            }
            if (!member.IsReadOnly && ((member.Type == null) || !member.Type.IsUsableAsReadOnly))
            {
                return false;
            }
            return member.IsReadVisibleTo(this.LocalAssembly, accessingType);
        }

        public void PopScope()
        {
            this._stack.PopScope();
        }

        public void PushScope()
        {
            this._stack.PushScope();
            if (this._prescopeNamespaces.Count > 0)
            {
                this._stack.CurrentFrame.SetNamespaces(this._prescopeNamespaces);
                this._prescopeNamespaces = new Dictionary<string, string>();
            }
        }

        public bool AllowProtectedMembersOnRoot { get; set; }

        public int CurrentArgCount
        {
            get
            {
                return this._stack.CurrentFrame.CtorArgCount;
            }
            set
            {
                this._stack.CurrentFrame.CtorArgCount = value;
            }
        }

        public bool CurrentForcedToUseConstructor
        {
            get
            {
                return this._stack.CurrentFrame.ForcedToUseConstructor;
            }
            set
            {
                this._stack.CurrentFrame.ForcedToUseConstructor = value;
            }
        }

        public bool CurrentInCollectionFromMember
        {
            get
            {
                return this._stack.CurrentFrame.InCollectionFromMember;
            }
            set
            {
                this._stack.CurrentFrame.InCollectionFromMember = value;
            }
        }

        public bool CurrentInContainerDirective
        {
            get
            {
                return this._stack.CurrentFrame.InContainerDirective;
            }
            set
            {
                this._stack.CurrentFrame.InContainerDirective = value;
            }
        }

        public bool CurrentInImplicitArray
        {
            get
            {
                return this._stack.CurrentFrame.InImplicitArray;
            }
            set
            {
                this._stack.CurrentFrame.InImplicitArray = value;
            }
        }

        public bool CurrentInInitProperty
        {
            get
            {
                return (this._stack.CurrentFrame.Member == XamlLanguage.Initialization);
            }
        }

        public bool CurrentInItemsProperty
        {
            get
            {
                return (this._stack.CurrentFrame.Member == XamlLanguage.Items);
            }
        }

        public bool CurrentIsUnknownContent
        {
            get
            {
                return (this._stack.CurrentFrame.Member == XamlLanguage.UnknownContent);
            }
        }

        public XamlMember CurrentMember
        {
            get
            {
                return this._stack.CurrentFrame.Member;
            }
            set
            {
                this._stack.CurrentFrame.Member = value;
            }
        }

        public XamlType CurrentPreviousChildType
        {
            get
            {
                return this._stack.CurrentFrame.PreviousChildType;
            }
            set
            {
                this._stack.CurrentFrame.PreviousChildType = value;
            }
        }

        public XamlType CurrentType
        {
            get
            {
                return this._stack.CurrentFrame.XamlType;
            }
            set
            {
                this._stack.CurrentFrame.XamlType = value;
            }
        }

        public bool CurrentTypeIsRoot
        {
            get
            {
                return ((this._stack.CurrentFrame.XamlType != null) && (this._stack.Depth == 1));
            }
        }

        public string CurrentTypeNamespace
        {
            get
            {
                return this._stack.CurrentFrame.TypeNamespace;
            }
            set
            {
                this._stack.CurrentFrame.TypeNamespace = value;
            }
        }

        public Func<string, string> XmlNamespaceResolver { get; set; }

    }
}

