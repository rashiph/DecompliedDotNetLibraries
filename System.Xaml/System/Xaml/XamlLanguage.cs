namespace System.Xaml
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;
    using System.Xaml.MS.Impl;
    using System.Xaml.Schema;
    using System.Xml.Serialization;

    public static class XamlLanguage
    {
        internal const string PreferredPrefix = "x";
        private static Lazy<ReadOnlyCollection<XamlDirective>> s_allDirectives = new Lazy<ReadOnlyCollection<XamlDirective>>(new Func<ReadOnlyCollection<XamlDirective>>(XamlLanguage.GetAllDirectives));
        private static Lazy<ReadOnlyCollection<XamlType>> s_allTypes = new Lazy<ReadOnlyCollection<XamlType>>(new Func<ReadOnlyCollection<XamlType>>(XamlLanguage.GetAllTypes));
        private static Lazy<XamlDirective> s_arguments = new Lazy<XamlDirective>(() => GetXamlDirective("Arguments", s_listOfObject.Value, null, AllowedMemberLocations.Any), true);
        private static Lazy<XamlType> s_array = new Lazy<XamlType>(() => GetXamlType(typeof(ArrayExtension)));
        private static Lazy<XamlDirective> s_asyncRecords = new Lazy<XamlDirective>(() => GetXamlDirective("AsyncRecords", String, BuiltInValueConverter.Int32, AllowedMemberLocations.Attribute), true);
        private static Lazy<XamlDirective> s_base = new Lazy<XamlDirective>(() => GetXmlDirective("base"));
        private static Lazy<XamlType> s_boolean = new Lazy<XamlType>(() => GetXamlType(typeof(bool)));
        private static Lazy<XamlType> s_byte = new Lazy<XamlType>(() => GetXamlType(typeof(byte)), true);
        private static Lazy<XamlType> s_char = new Lazy<XamlType>(() => GetXamlType(typeof(char)), true);
        private static Lazy<XamlDirective> s_class = new Lazy<XamlDirective>(() => GetXamlDirective("Class"));
        private static Lazy<XamlDirective> s_classAttributes = new Lazy<XamlDirective>(() => GetXamlDirective("ClassAttributes", s_listOfAttributes.Value, null, AllowedMemberLocations.MemberElement), true);
        private static Lazy<XamlDirective> s_classModifier = new Lazy<XamlDirective>(() => GetXamlDirective("ClassModifier"));
        private static Lazy<XamlDirective> s_code = new Lazy<XamlDirective>(() => GetXamlDirective("Code"));
        private static Lazy<XamlDirective> s_connectionId = new Lazy<XamlDirective>(() => GetXamlDirective("ConnectionId", s_string.Value, BuiltInValueConverter.Int32, AllowedMemberLocations.Any), true);
        private static Lazy<XamlType> s_decimal = new Lazy<XamlType>(() => GetXamlType(typeof(decimal)), true);
        private static Lazy<XamlType> s_double = new Lazy<XamlType>(() => GetXamlType(typeof(double)));
        private static Lazy<XamlDirective> s_factoryMethod = new Lazy<XamlDirective>(() => GetXamlDirective("FactoryMethod", s_string.Value, BuiltInValueConverter.String, AllowedMemberLocations.Any), true);
        private static Lazy<XamlDirective> s_fieldModifier = new Lazy<XamlDirective>(() => GetXamlDirective("FieldModifier"));
        private static Lazy<XamlType> s_iNameScope = new Lazy<XamlType>(() => GetXamlType(typeof(System.Windows.Markup.INameScope)));
        private static Lazy<XamlDirective> s_initialization = new Lazy<XamlDirective>(() => GetXamlDirective("_Initialization", s_object.Value, null, AllowedMemberLocations.Any), true);
        private static Lazy<XamlType> s_int16 = new Lazy<XamlType>(() => GetXamlType(typeof(short)), true);
        private static Lazy<XamlType> s_int32 = new Lazy<XamlType>(() => GetXamlType(typeof(int)));
        private static Lazy<XamlType> s_int64 = new Lazy<XamlType>(() => GetXamlType(typeof(long)), true);
        private static Lazy<XamlDirective> s_items = new Lazy<XamlDirective>(() => GetXamlDirective("_Items", s_listOfObject.Value, null, AllowedMemberLocations.Any), true);
        private static Lazy<XamlType> s_iXmlSerializable = new Lazy<XamlType>(() => GetXamlType(typeof(System.Xml.Serialization.IXmlSerializable)), true);
        private static Lazy<XamlDirective> s_key = new Lazy<XamlDirective>(() => GetXamlDirective("Key", s_object.Value, BuiltInValueConverter.String, AllowedMemberLocations.Any), true);
        private static Lazy<XamlDirective> s_lang = new Lazy<XamlDirective>(() => GetXmlDirective("lang"));
        private static Lazy<XamlType> s_listOfAttributes = new Lazy<XamlType>(() => GetXamlType(typeof(List<Attribute>)));
        private static Lazy<XamlType> s_listOfMembers = new Lazy<XamlType>(() => GetXamlType(typeof(List<MemberDefinition>)));
        private static Lazy<XamlType> s_listOfObject = new Lazy<XamlType>(() => GetXamlType(typeof(List<object>)));
        private static Lazy<XamlType> s_markupExtension = new Lazy<XamlType>(() => GetXamlType(typeof(System.Windows.Markup.MarkupExtension)));
        private static Lazy<XamlType> s_member = new Lazy<XamlType>(() => GetXamlType(typeof(MemberDefinition)));
        private static Lazy<XamlDirective> s_members = new Lazy<XamlDirective>(() => GetXamlDirective("Members", s_listOfMembers.Value, null, AllowedMemberLocations.MemberElement), true);
        private static Lazy<XamlDirective> s_name = new Lazy<XamlDirective>(() => GetXamlDirective("Name"));
        private static Lazy<XamlType> s_null = new Lazy<XamlType>(() => GetXamlType(typeof(NullExtension)));
        private static Lazy<XamlType> s_object = new Lazy<XamlType>(() => GetXamlType(typeof(object)));
        private static Lazy<XamlType> s_positionalParameterDescriptor = new Lazy<XamlType>(() => GetXamlType(typeof(System.Xaml.MS.Impl.PositionalParameterDescriptor)), true);
        private static Lazy<XamlDirective> s_positionalParameters = new Lazy<XamlDirective>(() => GetXamlDirective("_PositionalParameters", s_listOfObject.Value, null, AllowedMemberLocations.Any), true);
        private static Lazy<XamlType> s_property = new Lazy<XamlType>(() => GetXamlType(typeof(PropertyDefinition)));
        private static Lazy<XamlType> s_reference = new Lazy<XamlType>(() => GetXamlType(typeof(System.Windows.Markup.Reference)));
        private static Lazy<XamlSchemaContext> s_schemaContext = new Lazy<XamlSchemaContext>(new Func<XamlSchemaContext>(XamlLanguage.GetSchemaContext));
        private static Lazy<XamlDirective> s_shared = new Lazy<XamlDirective>(() => GetXamlDirective("Shared"), true);
        private static Lazy<XamlType> s_single = new Lazy<XamlType>(() => GetXamlType(typeof(float)), true);
        private static Lazy<XamlDirective> s_space = new Lazy<XamlDirective>(() => GetXmlDirective("space"));
        private static Lazy<XamlType> s_static = new Lazy<XamlType>(() => GetXamlType(typeof(StaticExtension)));
        private static Lazy<XamlType> s_string = new Lazy<XamlType>(() => GetXamlType(typeof(string)));
        private static Lazy<XamlDirective> s_subclass = new Lazy<XamlDirective>(() => GetXamlDirective("Subclass"), true);
        private static Lazy<XamlDirective> s_synchronousMode = new Lazy<XamlDirective>(() => GetXamlDirective("SynchronousMode"));
        private static Lazy<XamlType> s_timespan = new Lazy<XamlType>(() => GetXamlType(typeof(System.TimeSpan)), true);
        private static Lazy<XamlType> s_type = new Lazy<XamlType>(() => GetXamlType(typeof(TypeExtension)));
        private static Lazy<XamlDirective> s_typeArguments = new Lazy<XamlDirective>(() => GetXamlDirective("TypeArguments"));
        private static Lazy<XamlDirective> s_uid = new Lazy<XamlDirective>(() => GetXamlDirective("Uid"));
        private static Lazy<XamlDirective> s_unknownContent = new Lazy<XamlDirective>(() => GetXamlDirective("_UnknownContent", AllowedMemberLocations.MemberElement, MemberReflector.UnknownReflector), true);
        private static Lazy<XamlType> s_uri = new Lazy<XamlType>(() => GetXamlType(typeof(System.Uri)), true);
        private static ReadOnlyCollection<string> s_xamlNamespaces = new ReadOnlyCollection<string>(new string[] { "http://schemas.microsoft.com/winfx/2006/xaml" });
        private static Lazy<XamlType> s_xDataHolder = new Lazy<XamlType>(() => GetXamlType(typeof(System.Windows.Markup.XData)));
        private static ReadOnlyCollection<string> s_xmlNamespaces = new ReadOnlyCollection<string>(new string[] { "http://www.w3.org/XML/1998/namespace" });
        internal const string SWMNamespace = "System.Windows.Markup";
        private const string x_Arguments = "Arguments";
        private const string x_AsyncRecords = "AsyncRecords";
        private const string x_Class = "Class";
        private const string x_ClassAttributes = "ClassAttributes";
        private const string x_ClassModifier = "ClassModifier";
        private const string x_Code = "Code";
        private const string x_ConnectionId = "ConnectionId";
        private const string x_FactoryMethod = "FactoryMethod";
        private const string x_FieldModifier = "FieldModifier";
        private const string x_Initialization = "_Initialization";
        private const string x_Items = "_Items";
        private const string x_Key = "Key";
        private const string x_Members = "Members";
        private const string x_Name = "Name";
        private const string x_PositionalParameters = "_PositionalParameters";
        private const string x_Shared = "Shared";
        private const string x_Subclass = "Subclass";
        private const string x_SynchronousMode = "SynchronousMode";
        private const string x_TypeArguments = "TypeArguments";
        private const string x_Uid = "Uid";
        private const string x_UnknownContent = "_UnknownContent";
        public const string Xaml2006Namespace = "http://schemas.microsoft.com/winfx/2006/xaml";
        private const string xml_Base = "base";
        private const string xml_Lang = "lang";
        private const string xml_Space = "space";
        public const string Xml1998Namespace = "http://www.w3.org/XML/1998/namespace";

        [CompilerGenerated]
        private static XamlType <.cctor>b__1()
        {
            return GetXamlType(typeof(ArrayExtension));
        }

        [CompilerGenerated]
        private static XamlType <.cctor>b__10()
        {
            return GetXamlType(typeof(List<Attribute>));
        }

        [CompilerGenerated]
        private static XamlType <.cctor>b__11()
        {
            return GetXamlType(typeof(System.Windows.Markup.MarkupExtension));
        }

        [CompilerGenerated]
        private static XamlType <.cctor>b__12()
        {
            return GetXamlType(typeof(System.Windows.Markup.INameScope));
        }

        [CompilerGenerated]
        private static XamlType <.cctor>b__13()
        {
            return GetXamlType(typeof(System.Xml.Serialization.IXmlSerializable));
        }

        [CompilerGenerated]
        private static XamlType <.cctor>b__14()
        {
            return GetXamlType(typeof(System.Xaml.MS.Impl.PositionalParameterDescriptor));
        }

        [CompilerGenerated]
        private static XamlType <.cctor>b__15()
        {
            return GetXamlType(typeof(char));
        }

        [CompilerGenerated]
        private static XamlType <.cctor>b__16()
        {
            return GetXamlType(typeof(float));
        }

        [CompilerGenerated]
        private static XamlType <.cctor>b__17()
        {
            return GetXamlType(typeof(byte));
        }

        [CompilerGenerated]
        private static XamlType <.cctor>b__18()
        {
            return GetXamlType(typeof(short));
        }

        [CompilerGenerated]
        private static XamlType <.cctor>b__19()
        {
            return GetXamlType(typeof(long));
        }

        [CompilerGenerated]
        private static XamlType <.cctor>b__1a()
        {
            return GetXamlType(typeof(decimal));
        }

        [CompilerGenerated]
        private static XamlType <.cctor>b__1b()
        {
            return GetXamlType(typeof(System.Uri));
        }

        [CompilerGenerated]
        private static XamlType <.cctor>b__1c()
        {
            return GetXamlType(typeof(System.TimeSpan));
        }

        [CompilerGenerated]
        private static XamlDirective <.cctor>b__1d()
        {
            return GetXamlDirective("AsyncRecords", String, BuiltInValueConverter.Int32, AllowedMemberLocations.Attribute);
        }

        [CompilerGenerated]
        private static XamlDirective <.cctor>b__1e()
        {
            return GetXamlDirective("Arguments", s_listOfObject.Value, null, AllowedMemberLocations.Any);
        }

        [CompilerGenerated]
        private static XamlDirective <.cctor>b__1f()
        {
            return GetXamlDirective("Class");
        }

        [CompilerGenerated]
        private static XamlType <.cctor>b__2()
        {
            return GetXamlType(typeof(NullExtension));
        }

        [CompilerGenerated]
        private static XamlDirective <.cctor>b__20()
        {
            return GetXamlDirective("ClassModifier");
        }

        [CompilerGenerated]
        private static XamlDirective <.cctor>b__21()
        {
            return GetXamlDirective("Code");
        }

        [CompilerGenerated]
        private static XamlDirective <.cctor>b__22()
        {
            return GetXamlDirective("ConnectionId", s_string.Value, BuiltInValueConverter.Int32, AllowedMemberLocations.Any);
        }

        [CompilerGenerated]
        private static XamlDirective <.cctor>b__23()
        {
            return GetXamlDirective("FactoryMethod", s_string.Value, BuiltInValueConverter.String, AllowedMemberLocations.Any);
        }

        [CompilerGenerated]
        private static XamlDirective <.cctor>b__24()
        {
            return GetXamlDirective("FieldModifier");
        }

        [CompilerGenerated]
        private static XamlDirective <.cctor>b__25()
        {
            return GetXamlDirective("_Items", s_listOfObject.Value, null, AllowedMemberLocations.Any);
        }

        [CompilerGenerated]
        private static XamlDirective <.cctor>b__26()
        {
            return GetXamlDirective("_Initialization", s_object.Value, null, AllowedMemberLocations.Any);
        }

        [CompilerGenerated]
        private static XamlDirective <.cctor>b__27()
        {
            return GetXamlDirective("Key", s_object.Value, BuiltInValueConverter.String, AllowedMemberLocations.Any);
        }

        [CompilerGenerated]
        private static XamlDirective <.cctor>b__28()
        {
            return GetXamlDirective("Members", s_listOfMembers.Value, null, AllowedMemberLocations.MemberElement);
        }

        [CompilerGenerated]
        private static XamlDirective <.cctor>b__29()
        {
            return GetXamlDirective("ClassAttributes", s_listOfAttributes.Value, null, AllowedMemberLocations.MemberElement);
        }

        [CompilerGenerated]
        private static XamlDirective <.cctor>b__2a()
        {
            return GetXamlDirective("Name");
        }

        [CompilerGenerated]
        private static XamlDirective <.cctor>b__2b()
        {
            return GetXamlDirective("_PositionalParameters", s_listOfObject.Value, null, AllowedMemberLocations.Any);
        }

        [CompilerGenerated]
        private static XamlDirective <.cctor>b__2c()
        {
            return GetXamlDirective("Shared");
        }

        [CompilerGenerated]
        private static XamlDirective <.cctor>b__2d()
        {
            return GetXamlDirective("Subclass");
        }

        [CompilerGenerated]
        private static XamlDirective <.cctor>b__2e()
        {
            return GetXamlDirective("SynchronousMode");
        }

        [CompilerGenerated]
        private static XamlDirective <.cctor>b__2f()
        {
            return GetXamlDirective("TypeArguments");
        }

        [CompilerGenerated]
        private static XamlType <.cctor>b__3()
        {
            return GetXamlType(typeof(System.Windows.Markup.Reference));
        }

        [CompilerGenerated]
        private static XamlDirective <.cctor>b__30()
        {
            return GetXamlDirective("Uid");
        }

        [CompilerGenerated]
        private static XamlDirective <.cctor>b__31()
        {
            return GetXamlDirective("_UnknownContent", AllowedMemberLocations.MemberElement, MemberReflector.UnknownReflector);
        }

        [CompilerGenerated]
        private static XamlDirective <.cctor>b__32()
        {
            return GetXmlDirective("base");
        }

        [CompilerGenerated]
        private static XamlDirective <.cctor>b__33()
        {
            return GetXmlDirective("lang");
        }

        [CompilerGenerated]
        private static XamlDirective <.cctor>b__34()
        {
            return GetXmlDirective("space");
        }

        [CompilerGenerated]
        private static XamlType <.cctor>b__4()
        {
            return GetXamlType(typeof(StaticExtension));
        }

        [CompilerGenerated]
        private static XamlType <.cctor>b__5()
        {
            return GetXamlType(typeof(TypeExtension));
        }

        [CompilerGenerated]
        private static XamlType <.cctor>b__6()
        {
            return GetXamlType(typeof(string));
        }

        [CompilerGenerated]
        private static XamlType <.cctor>b__7()
        {
            return GetXamlType(typeof(double));
        }

        [CompilerGenerated]
        private static XamlType <.cctor>b__8()
        {
            return GetXamlType(typeof(int));
        }

        [CompilerGenerated]
        private static XamlType <.cctor>b__9()
        {
            return GetXamlType(typeof(bool));
        }

        [CompilerGenerated]
        private static XamlType <.cctor>b__a()
        {
            return GetXamlType(typeof(MemberDefinition));
        }

        [CompilerGenerated]
        private static XamlType <.cctor>b__b()
        {
            return GetXamlType(typeof(PropertyDefinition));
        }

        [CompilerGenerated]
        private static XamlType <.cctor>b__c()
        {
            return GetXamlType(typeof(System.Windows.Markup.XData));
        }

        [CompilerGenerated]
        private static XamlType <.cctor>b__d()
        {
            return GetXamlType(typeof(object));
        }

        [CompilerGenerated]
        private static XamlType <.cctor>b__e()
        {
            return GetXamlType(typeof(List<object>));
        }

        [CompilerGenerated]
        private static XamlType <.cctor>b__f()
        {
            return GetXamlType(typeof(List<MemberDefinition>));
        }

        private static ReadOnlyCollection<XamlDirective> GetAllDirectives()
        {
            return new ReadOnlyCollection<XamlDirective>(new XamlDirective[] { 
                Arguments, AsyncRecords, Class, Code, ClassModifier, ConnectionId, FactoryMethod, FieldModifier, Key, Initialization, Items, Members, ClassAttributes, Name, PositionalParameters, Shared, 
                Subclass, SynchronousMode, TypeArguments, Uid, UnknownContent, Base, Lang, Space
             });
        }

        private static ReadOnlyCollection<XamlType> GetAllTypes()
        {
            return new ReadOnlyCollection<XamlType>(new XamlType[] { 
                Array, Member, Null, Property, Reference, Static, Type, String, Double, Int16, Int32, Int64, Boolean, XData, Object, Char, 
                Single, Byte, Decimal, Uri, TimeSpan
             });
        }

        private static XamlSchemaContext GetSchemaContext()
        {
            Assembly[] referenceAssemblies = new Assembly[] { typeof(XamlLanguage).Assembly, typeof(System.Windows.Markup.MarkupExtension).Assembly };
            XamlSchemaContextSettings settings = new XamlSchemaContextSettings {
                SupportMarkupExtensionsWithDuplicateArity = true
            };
            return new XamlSchemaContext(referenceAssemblies, settings);
        }

        private static XamlDirective GetXamlDirective(string name)
        {
            return GetXamlDirective(name, String, BuiltInValueConverter.String, AllowedMemberLocations.Attribute);
        }

        private static XamlDirective GetXamlDirective(string name, AllowedMemberLocations allowedLocation, MemberReflector reflector)
        {
            return new XamlDirective(s_xamlNamespaces, name, allowedLocation, reflector);
        }

        private static XamlDirective GetXamlDirective(string name, XamlType xamlType, XamlValueConverter<TypeConverter> typeConverter, AllowedMemberLocations allowedLocation)
        {
            return new XamlDirective(s_xamlNamespaces, name, xamlType, typeConverter, allowedLocation);
        }

        private static XamlType GetXamlType(System.Type type)
        {
            return s_schemaContext.Value.GetXamlType(type);
        }

        private static XamlDirective GetXmlDirective(string name)
        {
            return new XamlDirective(s_xmlNamespaces, name, String, BuiltInValueConverter.String, AllowedMemberLocations.Attribute);
        }

        internal static System.Type LookupClrNamespaceType(AssemblyNamespacePair nsPair, string typeName)
        {
            if ((nsPair.ClrNamespace == "System.Windows.Markup") && (nsPair.Assembly == typeof(XamlLanguage).Assembly))
            {
                switch (typeName)
                {
                    case "Member":
                        return typeof(MemberDefinition);

                    case "Property":
                        return typeof(PropertyDefinition);
                }
            }
            return null;
        }

        internal static XamlDirective LookupXamlDirective(string name)
        {
            switch (name)
            {
                case "AsyncRecords":
                    return AsyncRecords;

                case "Arguments":
                    return Arguments;

                case "Class":
                    return Class;

                case "ClassModifier":
                    return ClassModifier;

                case "Code":
                    return Code;

                case "ConnectionId":
                    return ConnectionId;

                case "FactoryMethod":
                    return FactoryMethod;

                case "FieldModifier":
                    return FieldModifier;

                case "_Initialization":
                    return Initialization;

                case "_Items":
                    return Items;

                case "Key":
                    return Key;

                case "Members":
                    return Members;

                case "ClassAttributes":
                    return ClassAttributes;

                case "Name":
                    return Name;

                case "_PositionalParameters":
                    return PositionalParameters;

                case "Shared":
                    return Shared;

                case "Subclass":
                    return Subclass;

                case "SynchronousMode":
                    return SynchronousMode;

                case "TypeArguments":
                    return TypeArguments;

                case "Uid":
                    return Uid;

                case "_UnknownContent":
                    return UnknownContent;
            }
            return null;
        }

        internal static XamlType LookupXamlType(string typeNamespace, string typeName)
        {
            if (XamlNamespaces.Contains(typeNamespace))
            {
                switch (typeName)
                {
                    case "Array":
                    case "ArrayExtension":
                        return Array;

                    case "Member":
                        return Member;

                    case "Null":
                    case "NullExtension":
                        return Null;

                    case "Property":
                        return Property;

                    case "Reference":
                    case "ReferenceExtension":
                        return Reference;

                    case "Static":
                    case "StaticExtension":
                        return Static;

                    case "Type":
                    case "TypeExtension":
                        return Type;

                    case "String":
                        return String;

                    case "Double":
                        return Double;

                    case "Int16":
                        return Int16;

                    case "Int32":
                        return Int32;

                    case "Int64":
                        return Int64;

                    case "Boolean":
                        return Boolean;

                    case "XData":
                        return XData;

                    case "Object":
                        return Object;

                    case "Char":
                        return Char;

                    case "Single":
                        return Single;

                    case "Byte":
                        return Byte;

                    case "Decimal":
                        return Decimal;

                    case "Uri":
                        return Uri;

                    case "TimeSpan":
                        return TimeSpan;
                }
            }
            return null;
        }

        internal static XamlDirective LookupXmlDirective(string name)
        {
            switch (name)
            {
                case "base":
                    return Base;

                case "lang":
                    return Lang;

                case "space":
                    return Space;
            }
            return null;
        }

        internal static string TypeAlias(System.Type type)
        {
            if (type.Equals(typeof(MemberDefinition)))
            {
                return "Member";
            }
            if (type.Equals(typeof(PropertyDefinition)))
            {
                return "Property";
            }
            return null;
        }

        public static ReadOnlyCollection<XamlDirective> AllDirectives
        {
            get
            {
                return s_allDirectives.Value;
            }
        }

        public static ReadOnlyCollection<XamlType> AllTypes
        {
            get
            {
                return s_allTypes.Value;
            }
        }

        public static XamlDirective Arguments
        {
            get
            {
                return s_arguments.Value;
            }
        }

        public static XamlType Array
        {
            get
            {
                return s_array.Value;
            }
        }

        public static XamlDirective AsyncRecords
        {
            get
            {
                return s_asyncRecords.Value;
            }
        }

        public static XamlDirective Base
        {
            get
            {
                return s_base.Value;
            }
        }

        public static XamlType Boolean
        {
            get
            {
                return s_boolean.Value;
            }
        }

        public static XamlType Byte
        {
            get
            {
                return s_byte.Value;
            }
        }

        public static XamlType Char
        {
            get
            {
                return s_char.Value;
            }
        }

        public static XamlDirective Class
        {
            get
            {
                return s_class.Value;
            }
        }

        public static XamlDirective ClassAttributes
        {
            get
            {
                return s_classAttributes.Value;
            }
        }

        public static XamlDirective ClassModifier
        {
            get
            {
                return s_classModifier.Value;
            }
        }

        public static XamlDirective Code
        {
            get
            {
                return s_code.Value;
            }
        }

        public static XamlDirective ConnectionId
        {
            get
            {
                return s_connectionId.Value;
            }
        }

        public static XamlType Decimal
        {
            get
            {
                return s_decimal.Value;
            }
        }

        public static XamlType Double
        {
            get
            {
                return s_double.Value;
            }
        }

        public static XamlDirective FactoryMethod
        {
            get
            {
                return s_factoryMethod.Value;
            }
        }

        public static XamlDirective FieldModifier
        {
            get
            {
                return s_fieldModifier.Value;
            }
        }

        internal static XamlType INameScope
        {
            get
            {
                return s_iNameScope.Value;
            }
        }

        public static XamlDirective Initialization
        {
            get
            {
                return s_initialization.Value;
            }
        }

        public static XamlType Int16
        {
            get
            {
                return s_int16.Value;
            }
        }

        public static XamlType Int32
        {
            get
            {
                return s_int32.Value;
            }
        }

        public static XamlType Int64
        {
            get
            {
                return s_int64.Value;
            }
        }

        public static XamlDirective Items
        {
            get
            {
                return s_items.Value;
            }
        }

        internal static XamlType IXmlSerializable
        {
            get
            {
                return s_iXmlSerializable.Value;
            }
        }

        public static XamlDirective Key
        {
            get
            {
                return s_key.Value;
            }
        }

        public static XamlDirective Lang
        {
            get
            {
                return s_lang.Value;
            }
        }

        internal static XamlType MarkupExtension
        {
            get
            {
                return s_markupExtension.Value;
            }
        }

        public static XamlType Member
        {
            get
            {
                return s_member.Value;
            }
        }

        public static XamlDirective Members
        {
            get
            {
                return s_members.Value;
            }
        }

        public static XamlDirective Name
        {
            get
            {
                return s_name.Value;
            }
        }

        public static XamlType Null
        {
            get
            {
                return s_null.Value;
            }
        }

        public static XamlType Object
        {
            get
            {
                return s_object.Value;
            }
        }

        internal static XamlType PositionalParameterDescriptor
        {
            get
            {
                return s_positionalParameterDescriptor.Value;
            }
        }

        public static XamlDirective PositionalParameters
        {
            get
            {
                return s_positionalParameters.Value;
            }
        }

        public static XamlType Property
        {
            get
            {
                return s_property.Value;
            }
        }

        public static XamlType Reference
        {
            get
            {
                return s_reference.Value;
            }
        }

        public static XamlDirective Shared
        {
            get
            {
                return s_shared.Value;
            }
        }

        public static XamlType Single
        {
            get
            {
                return s_single.Value;
            }
        }

        public static XamlDirective Space
        {
            get
            {
                return s_space.Value;
            }
        }

        public static XamlType Static
        {
            get
            {
                return s_static.Value;
            }
        }

        public static XamlType String
        {
            get
            {
                return s_string.Value;
            }
        }

        public static XamlDirective Subclass
        {
            get
            {
                return s_subclass.Value;
            }
        }

        public static XamlDirective SynchronousMode
        {
            get
            {
                return s_synchronousMode.Value;
            }
        }

        public static XamlType TimeSpan
        {
            get
            {
                return s_timespan.Value;
            }
        }

        public static XamlType Type
        {
            get
            {
                return s_type.Value;
            }
        }

        public static XamlDirective TypeArguments
        {
            get
            {
                return s_typeArguments.Value;
            }
        }

        public static XamlDirective Uid
        {
            get
            {
                return s_uid.Value;
            }
        }

        public static XamlDirective UnknownContent
        {
            get
            {
                return s_unknownContent.Value;
            }
        }

        public static XamlType Uri
        {
            get
            {
                return s_uri.Value;
            }
        }

        public static IList<string> XamlNamespaces
        {
            get
            {
                return s_xamlNamespaces;
            }
        }

        public static XamlType XData
        {
            get
            {
                return s_xDataHolder.Value;
            }
        }

        public static IList<string> XmlNamespaces
        {
            get
            {
                return s_xmlNamespaces;
            }
        }
    }
}

