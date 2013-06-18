namespace System.Xaml.Schema
{
    using MS.Internal.Xaml.Parser;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Xaml;
    using System.Xaml.MS.Impl;

    internal class XamlNamespace
    {
        private ICollection<XamlType> _allPublicTypes;
        private List<AssemblyNamespacePair> _assemblyNamespaces;
        private ConcurrentDictionary<string, XamlType> _typeCache;
        public readonly XamlSchemaContext SchemaContext;

        public XamlNamespace(XamlSchemaContext schemaContext)
        {
            this.SchemaContext = schemaContext;
        }

        public XamlNamespace(XamlSchemaContext schemaContext, string clrNs, string assemblyName)
        {
            this.SchemaContext = schemaContext;
            this._assemblyNamespaces = this.GetClrNamespacePair(clrNs, assemblyName);
            if (this._assemblyNamespaces != null)
            {
                this.Initialize();
            }
            this.IsClrNamespace = true;
        }

        internal void AddAssemblyNamespacePair(AssemblyNamespacePair pair)
        {
            List<AssemblyNamespacePair> list;
            if (this._assemblyNamespaces == null)
            {
                list = new List<AssemblyNamespacePair>();
                this.Initialize();
            }
            else
            {
                list = new List<AssemblyNamespacePair>(this._assemblyNamespaces);
            }
            list.Add(pair);
            this._assemblyNamespaces = list;
        }

        private Type[] ConvertArrayOfXamlTypesToTypes(XamlType[] typeArgs)
        {
            Type[] typeArray = new Type[typeArgs.Length];
            for (int i = 0; i < typeArgs.Length; i++)
            {
                typeArray[i] = typeArgs[i].UnderlyingType;
            }
            return typeArray;
        }

        public ICollection<XamlType> GetAllXamlTypes()
        {
            if (this._allPublicTypes == null)
            {
                this._allPublicTypes = this.LookupAllTypes();
            }
            return this._allPublicTypes;
        }

        private List<AssemblyNamespacePair> GetClrNamespacePair(string clrNs, string assemblyName)
        {
            Assembly asm = this.SchemaContext.OnAssemblyResolve(assemblyName);
            if (asm == null)
            {
                return null;
            }
            return new List<AssemblyNamespacePair> { new AssemblyNamespacePair(asm, clrNs) };
        }

        private string GetTypeExtensionName(string typeName)
        {
            return (typeName + "Extension");
        }

        internal static Type GetTypeFromFullTypeName(string fullName)
        {
            return Type.GetType(fullName);
        }

        public XamlType GetXamlType(string typeName, params XamlType[] typeArgs)
        {
            if (!this.IsResolved)
            {
                return null;
            }
            string typeExtensionName = this.GetTypeExtensionName(typeName);
            if ((typeArgs == null) || (typeArgs.Length == 0))
            {
                return (this.TryGetXamlType(typeName) ?? this.TryGetXamlType(typeExtensionName));
            }
            Type[] typeArray = this.ConvertArrayOfXamlTypesToTypes(typeArgs);
            return (this.TryGetXamlType(typeName, typeArray) ?? this.TryGetXamlType(typeExtensionName, typeArray));
        }

        private void Initialize()
        {
            this._typeCache = XamlSchemaContext.CreateDictionary<string, XamlType>();
        }

        private ICollection<XamlType> LookupAllTypes()
        {
            List<XamlType> list = new List<XamlType>();
            if (this.IsResolved)
            {
                foreach (AssemblyNamespacePair pair in this._assemblyNamespaces)
                {
                    Assembly assembly = pair.Assembly;
                    if (assembly != null)
                    {
                        string clrNamespace = pair.ClrNamespace;
                        foreach (Type type in assembly.GetTypes())
                        {
                            if (KS.Eq(type.Namespace, clrNamespace))
                            {
                                XamlType xamlType = this.SchemaContext.GetXamlType(type);
                                list.Add(xamlType);
                            }
                        }
                    }
                }
            }
            return list.AsReadOnly();
        }

        private static Type MakeArrayType(Type elementType, string subscript)
        {
            Type type = elementType;
            int pos = 0;
            do
            {
                int rank = GenericTypeNameScanner.ParseSubscriptSegment(subscript, ref pos);
                if (rank == 0)
                {
                    return null;
                }
                type = (rank == 1) ? type.MakeArrayType() : type.MakeArrayType(rank);
            }
            while (pos < subscript.Length);
            return type;
        }

        private static string MangleGenericTypeName(string typeName, int paramNum)
        {
            if (paramNum != 0)
            {
                return (typeName + '`' + paramNum);
            }
            return null;
        }

        private Type SearchAssembliesForShortName(string shortName)
        {
            foreach (AssemblyNamespacePair pair in this._assemblyNamespaces)
            {
                Assembly assembly = pair.Assembly;
                if (assembly != null)
                {
                    string name = pair.ClrNamespace + "." + shortName;
                    Type type = assembly.GetType(name);
                    if (type != null)
                    {
                        return type;
                    }
                }
            }
            return null;
        }

        private Type TryGetType(string typeName)
        {
            Type clrNamespaceType = this.SearchAssembliesForShortName(typeName);
            if ((clrNamespaceType == null) && this.IsClrNamespace)
            {
                clrNamespaceType = XamlLanguage.LookupClrNamespaceType(this._assemblyNamespaces[0], typeName);
            }
            if (clrNamespaceType == null)
            {
                return null;
            }
            for (Type type2 = clrNamespaceType; type2.IsNested; type2 = type2.DeclaringType)
            {
                if (type2.IsNestedPrivate)
                {
                    return null;
                }
            }
            return clrNamespaceType;
        }

        private XamlType TryGetXamlType(string typeName)
        {
            XamlType xamlType;
            if (!this._typeCache.TryGetValue(typeName, out xamlType))
            {
                Type type2 = this.TryGetType(typeName);
                if (type2 == null)
                {
                    return null;
                }
                xamlType = this.SchemaContext.GetXamlType(type2);
                if (xamlType != null)
                {
                    xamlType = XamlSchemaContext.TryAdd<string, XamlType>(this._typeCache, typeName, xamlType);
                }
            }
            return xamlType;
        }

        private XamlType TryGetXamlType(string typeName, Type[] typeArgs)
        {
            string str;
            typeName = GenericTypeNameScanner.StripSubscript(typeName, out str);
            typeName = MangleGenericTypeName(typeName, typeArgs.Length);
            Type underlyingType = null;
            XamlType type2 = this.TryGetXamlType(typeName);
            if (type2 != null)
            {
                underlyingType = type2.UnderlyingType;
            }
            if (underlyingType == null)
            {
                return null;
            }
            Type elementType = underlyingType.MakeGenericType(typeArgs);
            if (!string.IsNullOrEmpty(str))
            {
                elementType = MakeArrayType(elementType, str);
                if (elementType == null)
                {
                    return null;
                }
            }
            return this.SchemaContext.GetXamlType(elementType);
        }

        public bool IsClrNamespace { get; private set; }

        public bool IsResolved
        {
            get
            {
                return (null != this._assemblyNamespaces);
            }
        }

        internal int RevisionNumber
        {
            get
            {
                if (this._assemblyNamespaces == null)
                {
                    return 0;
                }
                return this._assemblyNamespaces.Count;
            }
        }
    }
}

