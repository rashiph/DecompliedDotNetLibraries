namespace System.Xaml
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Xaml.Schema;

    public class XamlDirective : XamlMember
    {
        private AllowedMemberLocations _allowedLocation;
        private IList<string> _xamlNamespaces;

        public XamlDirective(string xamlNamespace, string name) : base(name, null)
        {
            this._xamlNamespaces = GetReadOnly(xamlNamespace);
            this._allowedLocation = AllowedMemberLocations.Any;
        }

        internal XamlDirective(IEnumerable<string> xamlNamespaces, string name, AllowedMemberLocations allowedLocation, MemberReflector reflector) : base(name, reflector)
        {
            this._xamlNamespaces = GetReadOnly(xamlNamespaces);
            this._allowedLocation = allowedLocation;
        }

        public XamlDirective(IEnumerable<string> xamlNamespaces, string name, XamlType xamlType, XamlValueConverter<TypeConverter> typeConverter, AllowedMemberLocations allowedLocation) : base(name, new MemberReflector(xamlType, typeConverter))
        {
            if (xamlType == null)
            {
                throw new ArgumentNullException("xamlType");
            }
            this._xamlNamespaces = GetReadOnly(xamlNamespaces);
            this._allowedLocation = allowedLocation;
        }

        public override int GetHashCode()
        {
            int num = (base.Name == null) ? 0 : base.Name.GetHashCode();
            foreach (string str in this._xamlNamespaces)
            {
                num ^= str.GetHashCode();
            }
            return num;
        }

        private static ReadOnlyCollection<string> GetReadOnly(IEnumerable<string> xamlNamespaces)
        {
            if (xamlNamespaces == null)
            {
                throw new ArgumentNullException("xamlNamespaces");
            }
            List<string> list = new List<string>(xamlNamespaces);
            using (List<string>.Enumerator enumerator = list.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current == null)
                    {
                        throw new ArgumentException(System.Xaml.SR.Get("CollectionCannotContainNulls", new object[] { "xamlNamespaces" }));
                    }
                }
            }
            return list.AsReadOnly();
        }

        private static ReadOnlyCollection<string> GetReadOnly(string xamlNamespace)
        {
            if (xamlNamespace == null)
            {
                throw new ArgumentNullException("xamlNamespace");
            }
            return new ReadOnlyCollection<string>(new string[] { xamlNamespace });
        }

        public override IList<string> GetXamlNamespaces()
        {
            return this._xamlNamespaces;
        }

        protected sealed override ICustomAttributeProvider LookupCustomAttributeProvider()
        {
            return null;
        }

        protected sealed override XamlValueConverter<XamlDeferringLoader> LookupDeferringLoader()
        {
            return null;
        }

        protected sealed override IList<XamlMember> LookupDependsOn()
        {
            return null;
        }

        protected sealed override XamlMemberInvoker LookupInvoker()
        {
            return XamlMemberInvoker.DirectiveInvoker;
        }

        protected sealed override bool LookupIsAmbient()
        {
            return false;
        }

        protected sealed override bool LookupIsEvent()
        {
            return false;
        }

        protected sealed override bool LookupIsReadOnly()
        {
            return false;
        }

        protected sealed override bool LookupIsReadPublic()
        {
            return true;
        }

        protected sealed override bool LookupIsUnknown()
        {
            return base.IsUnknown;
        }

        protected sealed override bool LookupIsWriteOnly()
        {
            return false;
        }

        protected sealed override bool LookupIsWritePublic()
        {
            return true;
        }

        protected sealed override XamlType LookupTargetType()
        {
            return null;
        }

        protected sealed override XamlType LookupType()
        {
            return base.Type;
        }

        protected sealed override XamlValueConverter<TypeConverter> LookupTypeConverter()
        {
            return base.TypeConverter;
        }

        protected sealed override MethodInfo LookupUnderlyingGetter()
        {
            return null;
        }

        protected sealed override MemberInfo LookupUnderlyingMember()
        {
            return null;
        }

        protected sealed override MethodInfo LookupUnderlyingSetter()
        {
            return null;
        }

        internal static bool NamespacesAreEqual(XamlDirective directive1, XamlDirective directive2)
        {
            IList<string> list = directive1._xamlNamespaces;
            IList<string> list2 = directive2._xamlNamespaces;
            if (list.Count != list2.Count)
            {
                return false;
            }
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != list2[i])
                {
                    return false;
                }
            }
            return true;
        }

        public override string ToString()
        {
            if (this._xamlNamespaces.Count > 0)
            {
                return ("{" + this._xamlNamespaces[0] + "}" + base.Name);
            }
            return base.Name;
        }

        public AllowedMemberLocations AllowedLocation
        {
            get
            {
                return this._allowedLocation;
            }
        }
    }
}

