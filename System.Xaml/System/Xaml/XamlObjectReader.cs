namespace System.Xaml
{
    using MS.Internal.Xaml.Runtime;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Windows.Markup;
    using System.Xaml.Schema;
    using System.Xml;
    using System.Xml.Serialization;

    public class XamlObjectReader : XamlReader
    {
        private object currentInstance;
        private XamlNode currentXamlNode;
        private Stack<MarkupInfo> nodes;
        private XamlSchemaContext schemaContext;
        private XamlObjectReaderSettings settings;

        public XamlObjectReader(object instance) : this(instance, (XamlObjectReaderSettings) null)
        {
        }

        public XamlObjectReader(object instance, XamlObjectReaderSettings settings) : this(instance, new XamlSchemaContext(), settings)
        {
        }

        public XamlObjectReader(object instance, XamlSchemaContext schemaContext) : this(instance, schemaContext, null)
        {
        }

        public XamlObjectReader(object instance, XamlSchemaContext schemaContext, XamlObjectReaderSettings settings)
        {
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }
            this.schemaContext = schemaContext;
            this.settings = settings ?? new XamlObjectReaderSettings();
            this.nodes = new Stack<MarkupInfo>();
            this.currentXamlNode = new XamlNode(XamlNode.InternalNodeType.StartOfStream);
            SerializerContext context = new SerializerContext(schemaContext, this.settings) {
                RootType = (instance == null) ? null : instance.GetType()
            };
            ObjectMarkupInfo item = ObjectMarkupInfo.ForObject(instance, context, null, true);
            while (context.PendingNameScopes.Count > 0)
            {
                context.PendingNameScopes.Dequeue().Resume(context);
            }
            Stack<HashSet<string>> namesInCurrentScope = new Stack<HashSet<string>>();
            namesInCurrentScope.Push(new HashSet<string>());
            item.EnsureNoDuplicateNames(namesInCurrentScope);
            item.FindNamespace(context);
            this.nodes.Push(item);
            foreach (XamlNode node in context.GetSortedNamespaceNodes())
            {
                NamespaceMarkupInfo info3 = new NamespaceMarkupInfo {
                    XamlNode = node
                };
                this.nodes.Push(info3);
            }
        }

        internal static string GetConstructorArgument(XamlMember member)
        {
            return XamlMemberExtensions.GetNearestMember(member, XamlMemberExtensions.GetNearestBaseMemberCriterion.HasConstructorArgument).ConstructorArgument;
        }

        internal static bool GetDefaultValue(XamlMember member, out object value)
        {
            XamlMember nearestMember = XamlMemberExtensions.GetNearestMember(member, XamlMemberExtensions.GetNearestBaseMemberCriterion.HasDefaultValue);
            if (nearestMember.HasDefaultValue)
            {
                value = nearestMember.DefaultValue;
                return true;
            }
            value = null;
            return false;
        }

        internal static DesignerSerializationVisibility GetSerializationVisibility(XamlMember member)
        {
            return XamlMemberExtensions.GetNearestMember(member, XamlMemberExtensions.GetNearestBaseMemberCriterion.HasSerializationVisibility).SerializationVisibility;
        }

        public override bool Read()
        {
            if (this.nodes.Count == 0)
            {
                if (this.currentXamlNode.NodeType != XamlNodeType.None)
                {
                    this.currentXamlNode = new XamlNode(XamlNode.InternalNodeType.EndOfStream);
                }
                return false;
            }
            MarkupInfo info = this.nodes.Pop();
            this.currentXamlNode = info.XamlNode;
            ObjectMarkupInfo info2 = info as ObjectMarkupInfo;
            this.currentInstance = (info2 != null) ? info2.Object : null;
            List<MarkupInfo> list = info.Decompose();
            if (list != null)
            {
                list.Reverse();
                foreach (MarkupInfo info3 in list)
                {
                    this.nodes.Push(info3);
                }
            }
            return true;
        }

        public virtual object Instance
        {
            get
            {
                if (this.currentXamlNode.NodeType == XamlNodeType.StartObject)
                {
                    return this.currentInstance;
                }
                return null;
            }
        }

        public override bool IsEof
        {
            get
            {
                return this.currentXamlNode.IsEof;
            }
        }

        public override XamlMember Member
        {
            get
            {
                return this.currentXamlNode.Member;
            }
        }

        public override System.Xaml.NamespaceDeclaration Namespace
        {
            get
            {
                return this.currentXamlNode.NamespaceDeclaration;
            }
        }

        public override XamlNodeType NodeType
        {
            get
            {
                return this.currentXamlNode.NodeType;
            }
        }

        public override XamlSchemaContext SchemaContext
        {
            get
            {
                return this.schemaContext;
            }
        }

        public override XamlType Type
        {
            get
            {
                return this.currentXamlNode.XamlType;
            }
        }

        public override object Value
        {
            get
            {
                return this.currentXamlNode.Value;
            }
        }

        private class EndMemberMarkupInfo : XamlObjectReader.MarkupInfo
        {
            private static XamlObjectReader.EndMemberMarkupInfo instance = new XamlObjectReader.EndMemberMarkupInfo();

            private EndMemberMarkupInfo()
            {
                base.XamlNode = new XamlNode(XamlNodeType.EndMember);
            }

            public static XamlObjectReader.EndMemberMarkupInfo Instance
            {
                get
                {
                    return instance;
                }
            }
        }

        private class EndObjectMarkupInfo : XamlObjectReader.MarkupInfo
        {
            private static XamlObjectReader.EndObjectMarkupInfo instance = new XamlObjectReader.EndObjectMarkupInfo();

            private EndObjectMarkupInfo()
            {
                base.XamlNode = new XamlNode(XamlNodeType.EndObject);
            }

            public static XamlObjectReader.EndObjectMarkupInfo Instance
            {
                get
                {
                    return instance;
                }
            }
        }

        private class HashSet<T>
        {
            private Dictionary<T, bool> dictionary;

            public HashSet()
            {
                this.dictionary = new Dictionary<T, bool>();
            }

            public HashSet(IEqualityComparer<T> comparer)
            {
                this.dictionary = new Dictionary<T, bool>(comparer);
            }

            public bool Add(T member)
            {
                if (this.Contains(member))
                {
                    return false;
                }
                this.dictionary.Add(member, true);
                return true;
            }

            public bool Contains(T member)
            {
                return this.dictionary.ContainsKey(member);
            }
        }

        private abstract class MarkupInfo
        {
            protected MarkupInfo()
            {
            }

            public virtual List<XamlObjectReader.MarkupInfo> Decompose()
            {
                return null;
            }

            public virtual void FindNamespace(XamlObjectReader.SerializerContext context)
            {
            }

            public System.Xaml.XamlNode XamlNode { get; set; }
        }

        private class MemberMarkupInfo : XamlObjectReader.MarkupInfo
        {
            private List<XamlObjectReader.MarkupInfo> children = new List<XamlObjectReader.MarkupInfo>();

            public static XamlObjectReader.XamlTemplateMarkupInfo ConvertToXamlReader(object propertyValue, XamlValueConverter<XamlDeferringLoader> deferringLoader, XamlObjectReader.SerializerContext context)
            {
                if (deferringLoader.ConverterInstance == null)
                {
                    throw new XamlObjectReaderException(System.Xaml.SR.Get("DeferringLoaderInstanceNull", new object[] { deferringLoader }));
                }
                context.Instance = propertyValue;
                XamlReader reader = context.Runtime.DeferredSave(context.TypeDescriptorContext, deferringLoader, propertyValue);
                context.Instance = null;
                using (reader)
                {
                    return new XamlObjectReader.XamlTemplateMarkupInfo(reader, context);
                }
            }

            public override List<XamlObjectReader.MarkupInfo> Decompose()
            {
                this.children.Add(XamlObjectReader.EndMemberMarkupInfo.Instance);
                return this.children;
            }

            public override void FindNamespace(XamlObjectReader.SerializerContext context)
            {
                XamlMember member = base.XamlNode.Member;
                if (this.MemberRequiresNamespaceHoisting(member))
                {
                    context.FindPrefix(member.PreferredXamlNamespace);
                }
                foreach (XamlObjectReader.MarkupInfo info in this.Children)
                {
                    info.FindNamespace(context);
                }
            }

            public static XamlObjectReader.MemberMarkupInfo ForAttachedProperty(object source, XamlMember attachedProperty, object value, XamlObjectReader.SerializerContext context)
            {
                if ((XamlObjectReader.GetSerializationVisibility(attachedProperty) != DesignerSerializationVisibility.Hidden) && ShouldWriteProperty(source, attachedProperty, context))
                {
                    if (context.IsPropertyWriteVisible(attachedProperty))
                    {
                        XamlObjectReader.MemberMarkupInfo info = new XamlObjectReader.MemberMarkupInfo {
                            XamlNode = new XamlNode(XamlNodeType.StartMember, attachedProperty)
                        };
                        info.Children.Add(GetPropertyValueInfo(value, attachedProperty, context));
                        return info;
                    }
                    if (attachedProperty.Type.IsDictionary)
                    {
                        return ForDictionary(value, attachedProperty, context, true);
                    }
                    if (attachedProperty.Type.IsCollection)
                    {
                        return ForSequence(value, attachedProperty, context, true);
                    }
                }
                return null;
            }

            private static XamlObjectReader.MemberMarkupInfo ForDictionary(object source, XamlMember property, XamlObjectReader.SerializerContext context, bool isAttachable)
            {
                XamlObjectReader.MemberMarkupInfo item = ForDictionaryItems(source, isAttachable ? null : property, property.Type, context);
                if ((item != null) && (item.Children.Count != 0))
                {
                    XamlObjectReader.MemberMarkupInfo info2 = new XamlObjectReader.MemberMarkupInfo {
                        XamlNode = new XamlNode(XamlNodeType.StartMember, property)
                    };
                    XamlObjectReader.ObjectMarkupInfo info3 = new XamlObjectReader.ObjectMarkupInfo {
                        XamlNode = new XamlNode(XamlNodeType.GetObject)
                    };
                    info3.Properties.Add(item);
                    info2.Children.Add(info3);
                    return info2;
                }
                return null;
            }

            public static XamlObjectReader.MemberMarkupInfo ForDictionaryItems(object sourceOrValue, XamlMember property, XamlType propertyType, XamlObjectReader.SerializerContext context)
            {
                object obj2;
                if (property != null)
                {
                    obj2 = context.Runtime.GetValue(sourceOrValue, property);
                    if (obj2 == null)
                    {
                        return null;
                    }
                }
                else
                {
                    obj2 = sourceOrValue;
                }
                XamlType keyType = propertyType.KeyType;
                XamlObjectReader.MemberMarkupInfo info = new XamlObjectReader.MemberMarkupInfo {
                    XamlNode = new XamlNode(XamlNodeType.StartMember, XamlLanguage.Items)
                };
                foreach (DictionaryEntry entry in context.Runtime.GetDictionaryItems(obj2, propertyType))
                {
                    XamlObjectReader.ObjectOrValueMarkupInfo info3;
                    XamlObjectReader.ObjectMarkupInfo item = XamlObjectReader.ObjectMarkupInfo.ForObject(entry.Value, context, null, false);
                    XamlType xamlType = null;
                    if (entry.Key != null)
                    {
                        xamlType = context.GetXamlType(entry.Key.GetType());
                    }
                    if ((entry.Key != null) && (xamlType != keyType))
                    {
                        TypeConverter converterInstance = XamlObjectReader.TypeConverterExtensions.GetConverterInstance<TypeConverter>(xamlType.TypeConverter);
                        info3 = XamlObjectReader.ObjectMarkupInfo.ForObject(entry.Key, context, converterInstance, false);
                    }
                    else
                    {
                        ValueSerializer propertyValueSerializer = XamlObjectReader.TypeConverterExtensions.GetConverterInstance<ValueSerializer>(keyType.ValueSerializer);
                        TypeConverter propertyConverter = XamlObjectReader.TypeConverterExtensions.GetConverterInstance<TypeConverter>(keyType.TypeConverter);
                        info3 = GetPropertyValueInfoInternal(entry.Key, propertyValueSerializer, propertyConverter, false, null, context);
                    }
                    if (!ShouldOmitKey(entry, context))
                    {
                        XamlObjectReader.MemberMarkupInfo info4 = new XamlObjectReader.MemberMarkupInfo {
                            XamlNode = new XamlNode(XamlNodeType.StartMember, XamlLanguage.Key)
                        };
                        info4.Children.Add(info3);
                        item.Properties.Insert(0, info4);
                    }
                    info.Children.Add(item);
                }
                return info;
            }

            public static XamlObjectReader.MemberMarkupInfo ForProperty(object source, XamlMember property, XamlObjectReader.SerializerContext context)
            {
                if (ShouldWriteProperty(source, property, context))
                {
                    if (context.IsPropertyWriteVisible(property))
                    {
                        return ForReadWriteProperty(source, property, context);
                    }
                    if (property.Type.IsXData)
                    {
                        return ForXmlSerializable(source, property, context);
                    }
                    if (property.Type.IsDictionary)
                    {
                        return ForDictionary(source, property, context, false);
                    }
                    if (property.Type.IsCollection)
                    {
                        return ForSequence(source, property, context, false);
                    }
                }
                return null;
            }

            private static XamlObjectReader.MemberMarkupInfo ForReadWriteProperty(object source, XamlMember xamlProperty, XamlObjectReader.SerializerContext context)
            {
                XamlObjectReader.MemberMarkupInfo info;
                object propertyValue = context.Runtime.GetValue(source, xamlProperty);
                XamlType declaringType = xamlProperty.DeclaringType;
                if ((xamlProperty == declaringType.GetAliasedProperty(XamlLanguage.Lang)) && (propertyValue is string))
                {
                    XamlObjectReader.MemberMarkupInfo info2 = new XamlObjectReader.MemberMarkupInfo {
                        XamlNode = new XamlNode(XamlNodeType.StartMember, XamlLanguage.Lang)
                    };
                    info2.Children.Add(GetPropertyValueInfo(propertyValue, xamlProperty, context));
                    info = info2;
                }
                else
                {
                    XamlObjectReader.MemberMarkupInfo info3 = new XamlObjectReader.MemberMarkupInfo {
                        XamlNode = new XamlNode(XamlNodeType.StartMember, xamlProperty)
                    };
                    info3.Children.Add(GetPropertyValueInfo(propertyValue, xamlProperty, context));
                    info = info3;
                }
                RemoveObjectNodesForCollectionOrDictionary(info);
                return info;
            }

            private static XamlObjectReader.MemberMarkupInfo ForSequence(object source, XamlMember property, XamlObjectReader.SerializerContext context, bool isAttachable)
            {
                XamlObjectReader.MemberMarkupInfo item = ForSequenceItems(source, isAttachable ? null : property, property.Type, context, false);
                if ((item != null) && (item.Children.Count != 0))
                {
                    XamlObjectReader.MemberMarkupInfo info2 = new XamlObjectReader.MemberMarkupInfo {
                        XamlNode = new XamlNode(XamlNodeType.StartMember, property)
                    };
                    XamlObjectReader.ObjectMarkupInfo info3 = new XamlObjectReader.ObjectMarkupInfo {
                        XamlNode = new XamlNode(XamlNodeType.GetObject)
                    };
                    info3.Properties.Add(item);
                    info2.Children.Add(info3);
                    return info2;
                }
                return null;
            }

            public static XamlObjectReader.MemberMarkupInfo ForSequenceItems(object sourceOrValue, XamlMember property, XamlType xamlType, XamlObjectReader.SerializerContext context, bool allowReadOnly)
            {
                object obj2;
                if (property != null)
                {
                    obj2 = context.Runtime.GetValue(sourceOrValue, property);
                    if (obj2 == null)
                    {
                        return null;
                    }
                }
                else
                {
                    obj2 = sourceOrValue;
                }
                if ((!allowReadOnly && (xamlType.IsReadOnlyMethod != null)) && ((bool) xamlType.IsReadOnlyMethod.Invoke(obj2, null)))
                {
                    return null;
                }
                XamlObjectReader.MemberMarkupInfo info = new XamlObjectReader.MemberMarkupInfo {
                    XamlNode = new XamlNode(XamlNodeType.StartMember, XamlLanguage.Items)
                };
                bool flag2 = false;
                IList<object> collectionItems = context.Runtime.GetCollectionItems(obj2, xamlType);
                for (int i = 0; i < collectionItems.Count; i++)
                {
                    object obj3 = collectionItems[i];
                    XamlObjectReader.ObjectMarkupInfo info2 = XamlObjectReader.ObjectMarkupInfo.ForObject(obj3, context, null, false);
                    XamlObjectReader.ObjectOrValueMarkupInfo info3 = null;
                    if (((xamlType.ContentWrappers != null) && (info2.Properties != null)) && (info2.Properties.Count == 1))
                    {
                        XamlObjectReader.MemberMarkupInfo info4 = (XamlObjectReader.MemberMarkupInfo) info2.Properties[0];
                        if (info4.XamlNode.Member == info2.XamlNode.XamlType.ContentProperty)
                        {
                            foreach (XamlType type in xamlType.ContentWrappers)
                            {
                                if ((type == info2.XamlNode.XamlType) && (info4.Children.Count == 1))
                                {
                                    XamlObjectReader.ObjectOrValueMarkupInfo info5 = (XamlObjectReader.ObjectOrValueMarkupInfo) info4.Children[0];
                                    if (info5 is XamlObjectReader.ValueMarkupInfo)
                                    {
                                        bool isFirstElementOfCollection = i == 0;
                                        bool isLastElementOfCollection = i == (collectionItems.Count - 1);
                                        if (flag2 || ShouldUnwrapDueToWhitespace((string) info5.XamlNode.Value, xamlType, isFirstElementOfCollection, isLastElementOfCollection))
                                        {
                                            continue;
                                        }
                                        info3 = info5;
                                        flag2 = true;
                                    }
                                    else
                                    {
                                        info3 = info5;
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    if ((info3 == null) || !(info3 is XamlObjectReader.ValueMarkupInfo))
                    {
                        flag2 = false;
                    }
                    info.Children.Add(info3 ?? info2);
                }
                return info;
            }

            private static XamlObjectReader.MemberMarkupInfo ForXmlSerializable(object source, XamlMember property, XamlObjectReader.SerializerContext context)
            {
                IXmlSerializable serializable = (IXmlSerializable) context.Runtime.GetValue(source, property);
                if (serializable != null)
                {
                    StringBuilder output = new StringBuilder();
                    XmlWriterSettings settings = new XmlWriterSettings {
                        ConformanceLevel = ConformanceLevel.Auto,
                        Indent = true,
                        OmitXmlDeclaration = true
                    };
                    using (XmlWriter writer = XmlWriter.Create(output, settings))
                    {
                        serializable.WriteXml(writer);
                    }
                    if (output.Length > 0)
                    {
                        XamlObjectReader.MemberMarkupInfo info = new XamlObjectReader.MemberMarkupInfo {
                            XamlNode = new XamlNode(XamlNodeType.StartMember, property)
                        };
                        XamlObjectReader.ObjectMarkupInfo item = new XamlObjectReader.ObjectMarkupInfo {
                            XamlNode = new XamlNode(XamlNodeType.StartObject, XamlLanguage.XData)
                        };
                        XamlObjectReader.MemberMarkupInfo info3 = new XamlObjectReader.MemberMarkupInfo {
                            XamlNode = new XamlNode(XamlNodeType.StartMember, XamlLanguage.XData.GetMember("Text"))
                        };
                        XamlObjectReader.ValueMarkupInfo info4 = new XamlObjectReader.ValueMarkupInfo {
                            XamlNode = new XamlNode(XamlNodeType.Value, output.ToString())
                        };
                        info3.Children.Add(info4);
                        item.Properties.Add(info3);
                        info.Children.Add(item);
                        return info;
                    }
                }
                return null;
            }

            private static XamlObjectReader.ObjectOrValueMarkupInfo GetPropertyValueInfo(object propertyValue, XamlMember xamlProperty, XamlObjectReader.SerializerContext context)
            {
                return GetPropertyValueInfoInternal(propertyValue, XamlObjectReader.TypeConverterExtensions.GetConverterInstance<ValueSerializer>(xamlProperty.ValueSerializer), XamlObjectReader.TypeConverterExtensions.GetConverterInstance<TypeConverter>(xamlProperty.TypeConverter), (xamlProperty != null) && (xamlProperty.DeferringLoader != null), xamlProperty, context);
            }

            private static XamlObjectReader.ObjectOrValueMarkupInfo GetPropertyValueInfoInternal(object propertyValue, ValueSerializer propertyValueSerializer, TypeConverter propertyConverter, bool isXamlTemplate, XamlMember xamlProperty, XamlObjectReader.SerializerContext context)
            {
                context.Instance = propertyValue;
                if (isXamlTemplate && (propertyValue != null))
                {
                    return ConvertToXamlReader(propertyValue, xamlProperty.DeferringLoader, context);
                }
                if (context.TryValueSerializeToString(propertyValueSerializer, propertyConverter, context, ref propertyValue))
                {
                    ThrowIfPropertiesAreAttached(context.Instance, xamlProperty, context);
                    context.Instance = null;
                    return new XamlObjectReader.ValueMarkupInfo { XamlNode = new XamlNode(XamlNodeType.Value, propertyValue) };
                }
                if ((propertyConverter != null) && context.TryConvertToMarkupExtension(propertyConverter, ref propertyValue))
                {
                    context.Instance = null;
                    return XamlObjectReader.ObjectMarkupInfo.ForObject(propertyValue, context, null, false);
                }
                if ((propertyConverter != null) && context.TryTypeConvertToString(propertyConverter, ref propertyValue))
                {
                    ThrowIfPropertiesAreAttached(context.Instance, xamlProperty, context);
                    context.Instance = null;
                    return new XamlObjectReader.ValueMarkupInfo { XamlNode = new XamlNode(XamlNodeType.Value, propertyValue ?? string.Empty) };
                }
                if (propertyValue is string)
                {
                    ThrowIfPropertiesAreAttached(propertyValue, xamlProperty, context);
                    context.Instance = null;
                    return new XamlObjectReader.ValueMarkupInfo { XamlNode = new XamlNode(XamlNodeType.Value, propertyValue) };
                }
                context.Instance = null;
                return XamlObjectReader.ObjectMarkupInfo.ForObject(propertyValue, context, propertyConverter, false);
            }

            private bool MemberRequiresNamespaceHoisting(XamlMember member)
            {
                if (!member.IsAttachable && (!member.IsDirective || XamlXmlWriter.IsImplicit(member)))
                {
                    return false;
                }
                return (member.PreferredXamlNamespace != "http://www.w3.org/XML/1998/namespace");
            }

            private static void RemoveObjectNodesForCollectionOrDictionary(XamlObjectReader.MemberMarkupInfo memberInfo)
            {
                XamlType type = memberInfo.XamlNode.Member.Type;
                if ((type.IsCollection || type.IsDictionary) && (memberInfo.Children.Count == 1))
                {
                    XamlObjectReader.ObjectMarkupInfo info = memberInfo.Children[0] as XamlObjectReader.ObjectMarkupInfo;
                    if (((info != null) && (info.Properties.Count == 1)) && ((type == info.XamlNode.XamlType) && (info.Properties[0].XamlNode.Member == XamlLanguage.Items)))
                    {
                        XamlObjectReader.MemberMarkupInfo info2 = info.Properties[0] as XamlObjectReader.MemberMarkupInfo;
                        if ((info2 != null) || (info2.Children.Count > 0))
                        {
                            XamlObjectReader.ObjectMarkupInfo info3 = info2.Children[0] as XamlObjectReader.ObjectMarkupInfo;
                            if (((info3 == null) || (info3.XamlNode.XamlType == null)) || !info3.XamlNode.XamlType.IsMarkupExtension)
                            {
                                info.XamlNode = new XamlNode(XamlNodeType.GetObject);
                            }
                        }
                    }
                }
            }

            public static bool ShouldOmitKey(DictionaryEntry entry, XamlObjectReader.SerializerContext context)
            {
                if (entry.Value != null)
                {
                    XamlMember aliasedProperty = context.GetXamlType(entry.Value.GetType()).GetAliasedProperty(XamlLanguage.Key);
                    if ((aliasedProperty != null) && XamlObjectReader.ObjectMarkupInfo.CanPropertyXamlRoundtrip(aliasedProperty, context))
                    {
                        object obj2 = context.Runtime.GetValue(entry.Value, aliasedProperty);
                        if (obj2 == null)
                        {
                            return (entry.Key == null);
                        }
                        if (obj2.Equals(entry.Key))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            private static bool ShouldUnwrapDueToWhitespace(string value, XamlType xamlType, bool isFirstElementOfCollection, bool isLastElementOfCollection)
            {
                if (!XamlXmlWriter.HasSignificantWhitespace(value))
                {
                    return false;
                }
                return (!xamlType.IsWhitespaceSignificantCollection || ((XamlXmlWriter.ContainsConsecutiveInnerSpaces(value) || XamlXmlWriter.ContainsWhitespaceThatIsNotSpace(value)) || ((XamlXmlWriter.ContainsTrailingSpace(value) && isLastElementOfCollection) || (XamlXmlWriter.ContainsLeadingSpace(value) && isFirstElementOfCollection))));
            }

            private static bool ShouldWriteProperty(object source, XamlMember property, XamlObjectReader.SerializerContext context)
            {
                object obj2;
                bool flag = !context.IsPropertyWriteVisible(property);
                if (!flag && XamlObjectReader.GetDefaultValue(property, out obj2))
                {
                    object objB = context.Runtime.GetValue(source, property);
                    return !object.Equals(obj2, objB);
                }
                ShouldSerializeResult result = context.Runtime.ShouldSerialize(property, source);
                if (result != ShouldSerializeResult.Default)
                {
                    if (result != ShouldSerializeResult.True)
                    {
                        return false;
                    }
                    return true;
                }
                return (!flag || (!context.Settings.RequireExplicitContentVisibility || (XamlObjectReader.GetSerializationVisibility(property) == DesignerSerializationVisibility.Content)));
            }

            private static void ThrowIfPropertiesAreAttached(object value, XamlMember property, XamlObjectReader.SerializerContext context)
            {
                KeyValuePair<AttachableMemberIdentifier, object>[] attachedProperties = context.Runtime.GetAttachedProperties(value);
                if (attachedProperties != null)
                {
                    if (property != null)
                    {
                        throw new InvalidOperationException(System.Xaml.SR.Get("AttachedPropertyOnTypeConvertedOrStringProperty", new object[] { property.Name, value.ToString(), attachedProperties[0].Key.ToString() }));
                    }
                    throw new InvalidOperationException(System.Xaml.SR.Get("AttachedPropertyOnDictionaryKey", new object[] { value.ToString(), attachedProperties[0].Key.ToString() }));
                }
            }

            public List<XamlObjectReader.MarkupInfo> Children
            {
                get
                {
                    return this.children;
                }
            }

            public bool IsAtomic
            {
                get
                {
                    return ((this.children.Count == 1) && (this.children[0] is XamlObjectReader.ValueMarkupInfo));
                }
            }

            public bool IsAttributable
            {
                get
                {
                    if (base.XamlNode.Member == XamlLanguage.PositionalParameters)
                    {
                        foreach (XamlObjectReader.MarkupInfo info in this.children)
                        {
                            XamlObjectReader.ObjectMarkupInfo info2 = info as XamlObjectReader.ObjectMarkupInfo;
                            if ((info2 != null) && !info2.IsAttributableMarkupExtension)
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                    if (this.Children.Count > 1)
                    {
                        return false;
                    }
                    if ((this.Children.Count == 0) || (this.Children[0] is XamlObjectReader.ValueMarkupInfo))
                    {
                        return true;
                    }
                    XamlObjectReader.ObjectMarkupInfo info3 = this.Children[0] as XamlObjectReader.ObjectMarkupInfo;
                    if (info3 == null)
                    {
                        throw new InvalidOperationException(System.Xaml.SR.Get("ExpectedObjectMarkupInfo"));
                    }
                    return info3.IsAttributableMarkupExtension;
                }
            }

            public bool IsAttributableMarkupExtension
            {
                get
                {
                    if (this.children.Count != 1)
                    {
                        return false;
                    }
                    XamlObjectReader.ObjectMarkupInfo info = this.children[0] as XamlObjectReader.ObjectMarkupInfo;
                    return ((info != null) && info.IsAttributableMarkupExtension);
                }
            }

            public bool IsContent { get; set; }

            public bool IsFactoryMethod { get; set; }
        }

        private class NameScopeMarkupInfo : XamlObjectReader.ObjectMarkupInfo
        {
            public override void EnsureNoDuplicateNames(Stack<XamlObjectReader.HashSet<string>> namesInCurrentScope)
            {
                namesInCurrentScope.Push(new XamlObjectReader.HashSet<string>());
                base.EnsureNoDuplicateNames(namesInCurrentScope);
                namesInCurrentScope.Pop();
            }

            public void Resume(XamlObjectReader.SerializerContext context)
            {
                context.ReferenceTable = new XamlObjectReader.ReferenceTable(this.ParentTable);
                base.AddRecordMembers(this.SourceObject, context);
            }

            public XamlObjectReader.ReferenceTable ParentTable { get; set; }

            public object SourceObject { get; set; }
        }

        private class NamespaceMarkupInfo : XamlObjectReader.MarkupInfo
        {
        }

        private class ObjectMarkupInfo : XamlObjectReader.ObjectOrValueMarkupInfo
        {
            private bool? isAttributableMarkupExtension = null;
            private List<XamlObjectReader.MarkupInfo> properties = new List<XamlObjectReader.MarkupInfo>();

            private void AddArgumentsMembers(ICollection arguments, XamlObjectReader.SerializerContext context)
            {
                if ((arguments != null) && (arguments.Count > 0))
                {
                    XamlObjectReader.MemberMarkupInfo info2 = new XamlObjectReader.MemberMarkupInfo {
                        XamlNode = new XamlNode(XamlNodeType.StartMember, XamlLanguage.Items)
                    };
                    XamlObjectReader.MemberMarkupInfo item = new XamlObjectReader.MemberMarkupInfo {
                        XamlNode = new XamlNode(XamlNodeType.StartMember, XamlLanguage.Arguments)
                    };
                    foreach (object obj2 in arguments)
                    {
                        item.Children.Add(ForObject(obj2, context, null, false));
                    }
                    this.Properties.Add(item);
                }
            }

            private static void AddAttachedProperties(object value, XamlObjectReader.ObjectMarkupInfo objectInfo, XamlObjectReader.SerializerContext context)
            {
                KeyValuePair<AttachableMemberIdentifier, object>[] attachedProperties = context.Runtime.GetAttachedProperties(value);
                if (attachedProperties != null)
                {
                    foreach (KeyValuePair<AttachableMemberIdentifier, object> pair in attachedProperties)
                    {
                        XamlType xamlType = context.GetXamlType(pair.Key.DeclaringType);
                        if (xamlType.IsVisibleTo(context.LocalAssembly))
                        {
                            XamlMember attachableMember = xamlType.GetAttachableMember(pair.Key.MemberName);
                            if (attachableMember == null)
                            {
                                throw new XamlObjectReaderException(System.Xaml.SR.Get("ObjectReaderAttachedPropertyNotFound", new object[] { xamlType, pair.Key.MemberName }));
                            }
                            if (CanPropertyXamlRoundtrip(attachableMember, context))
                            {
                                XamlObjectReader.MemberMarkupInfo item = XamlObjectReader.MemberMarkupInfo.ForAttachedProperty(value, attachableMember, pair.Value, context);
                                if (item != null)
                                {
                                    objectInfo.Properties.Add(item);
                                }
                            }
                        }
                    }
                }
            }

            private void AddFactoryMethodAndValidateArguments(Type valueType, MemberInfo memberInfo, ICollection arguments, XamlObjectReader.SerializerContext context, out ParameterInfo[] methodParams)
            {
                methodParams = null;
                if (memberInfo == null)
                {
                    methodParams = new ParameterInfo[0];
                }
                else if (memberInfo is ConstructorInfo)
                {
                    methodParams = ((ConstructorInfo) memberInfo).GetParameters();
                }
                else if (memberInfo is MethodInfo)
                {
                    methodParams = ((MethodInfo) memberInfo).GetParameters();
                    string name = memberInfo.Name;
                    Type declaringType = memberInfo.DeclaringType;
                    if (declaringType != valueType)
                    {
                        name = ConvertTypeAndMethodToString(declaringType, name, context);
                    }
                    XamlObjectReader.MemberMarkupInfo item = new XamlObjectReader.MemberMarkupInfo {
                        XamlNode = new XamlNode(XamlNodeType.StartMember, XamlLanguage.FactoryMethod),
                        IsFactoryMethod = true
                    };
                    XamlObjectReader.ValueMarkupInfo info4 = new XamlObjectReader.ValueMarkupInfo {
                        XamlNode = new XamlNode(XamlNodeType.Value, name)
                    };
                    item.Children.Add(info4);
                    this.Properties.Add(item);
                }
                else
                {
                    if (!valueType.IsValueType)
                    {
                        throw new XamlObjectReaderException(System.Xaml.SR.Get("ObjectReaderInstanceDescriptorInvalidMethod"));
                    }
                    if ((arguments != null) && (arguments.Count > 0))
                    {
                        throw new XamlObjectReaderException(System.Xaml.SR.Get("ObjectReaderInstanceDescriptorIncompatibleArguments"));
                    }
                    return;
                }
                if (arguments != null)
                {
                    if (arguments.Count != methodParams.Length)
                    {
                        throw new XamlObjectReaderException(System.Xaml.SR.Get("ObjectReaderInstanceDescriptorIncompatibleArguments"));
                    }
                    int num = 0;
                    foreach (object obj2 in arguments)
                    {
                        ParameterInfo info5 = methodParams[num++];
                        if (obj2 == null)
                        {
                            if (info5.ParameterType.IsValueType && (!info5.ParameterType.IsGenericType || (info5.ParameterType.GetGenericTypeDefinition() != typeof(Nullable<>))))
                            {
                                throw new XamlObjectReaderException(System.Xaml.SR.Get("ObjectReaderInstanceDescriptorIncompatibleArgumentTypes", new object[] { "null", info5.ParameterType }));
                            }
                        }
                        else if (!info5.ParameterType.IsAssignableFrom(obj2.GetType()))
                        {
                            throw new XamlObjectReaderException(System.Xaml.SR.Get("ObjectReaderInstanceDescriptorIncompatibleArgumentTypes", new object[] { obj2.GetType(), info5.ParameterType }));
                        }
                    }
                }
            }

            private void AddItemsProperty(object value, XamlObjectReader.SerializerContext context, XamlType xamlType)
            {
                XamlObjectReader.MemberMarkupInfo item = null;
                if (xamlType.IsDictionary)
                {
                    item = XamlObjectReader.MemberMarkupInfo.ForDictionaryItems(value, null, xamlType, context);
                }
                else if (xamlType.IsCollection)
                {
                    item = XamlObjectReader.MemberMarkupInfo.ForSequenceItems(value, null, xamlType, context, true);
                }
                if ((item != null) && (item.Children.Count != 0))
                {
                    this.properties.Add(item);
                }
            }

            public void AddNameProperty(XamlObjectReader.SerializerContext context)
            {
                XamlObjectReader.MemberMarkupInfo item = new XamlObjectReader.MemberMarkupInfo {
                    XamlNode = new XamlNode(XamlNodeType.StartMember, XamlLanguage.Name)
                };
                XamlObjectReader.ValueMarkupInfo info2 = new XamlObjectReader.ValueMarkupInfo {
                    XamlNode = new XamlNode(XamlNodeType.Value, this.Name)
                };
                item.Children.Add(info2);
                this.Properties.Add(item);
                if (base.XamlNode.NodeType == XamlNodeType.GetObject)
                {
                    XamlType data = context.LocalAssemblyAwareGetXamlType(this.Object.GetType());
                    base.XamlNode = new XamlNode(XamlNodeType.StartObject, data);
                }
            }

            private void AddRecordConstructionMembers(object value, XamlType valueXamlType, XamlObjectReader.SerializerContext context, TypeConverter converter, out bool isComplete, out ParameterInfo[] methodParams)
            {
                MemberInfo member = null;
                ICollection arguments = null;
                isComplete = false;
                if (valueXamlType.IsMarkupExtension)
                {
                    if (!this.TryGetInstanceDescriptorInfo(value, context, converter, out member, out arguments, out isComplete))
                    {
                        if (!this.TryGetDefaultConstructorInfo(valueXamlType, out member, out arguments, out isComplete))
                        {
                            this.GetConstructorInfo(value, valueXamlType, context, out member, out arguments, out isComplete);
                            if (!this.TryAddPositionalParameters(valueXamlType, member, arguments, context))
                            {
                                this.AddArgumentsMembers(arguments, context);
                            }
                        }
                    }
                    else if (!this.TryAddPositionalParameters(valueXamlType, member, arguments, context))
                    {
                        MemberInfo info2 = member;
                        ICollection is3 = arguments;
                        bool flag = isComplete;
                        if (!this.TryGetDefaultConstructorInfo(valueXamlType, out member, out arguments, out isComplete))
                        {
                            member = info2;
                            arguments = is3;
                            isComplete = flag;
                            this.AddArgumentsMembers(arguments, context);
                        }
                    }
                }
                else if (!this.TryGetDefaultConstructorInfo(valueXamlType, out member, out arguments, out isComplete))
                {
                    if (!this.TryGetInstanceDescriptorInfo(value, context, converter, out member, out arguments, out isComplete))
                    {
                        this.GetConstructorInfo(value, valueXamlType, context, out member, out arguments, out isComplete);
                    }
                    this.AddArgumentsMembers(arguments, context);
                }
                this.AddFactoryMethodAndValidateArguments(value.GetType(), member, arguments, context, out methodParams);
            }

            protected void AddRecordMembers(object value, XamlObjectReader.SerializerContext context)
            {
                this.AddRecordMembers(value, context, null);
            }

            protected void AddRecordMembers(object value, XamlObjectReader.SerializerContext context, TypeConverter converter)
            {
                bool flag;
                ParameterInfo[] infoArray;
                Type clrType = value.GetType();
                XamlType xamlType = context.GetXamlType(clrType);
                context.Instance = value;
                if ((converter == null) || !context.CanConvertTo(converter, typeof(InstanceDescriptor)))
                {
                    context.Instance = null;
                    converter = XamlObjectReader.TypeConverterExtensions.GetConverterInstance<TypeConverter>(xamlType.TypeConverter);
                }
                this.AddRecordConstructionMembers(value, xamlType, context, converter, out flag, out infoArray);
                if ((!flag || (xamlType.GetAliasedProperty(XamlLanguage.Name) != null)) || (context.Runtime.AttachedPropertyCount(value) > 0))
                {
                    this.AddRecordMembers(value, context, infoArray, xamlType);
                }
            }

            private void AddRecordMembers(object value, XamlObjectReader.SerializerContext context, ParameterInfo[] methodParameters, XamlType xamlType)
            {
                foreach (XamlMember member in GetXamlSerializableProperties(xamlType, context))
                {
                    if ((XamlObjectReader.GetSerializationVisibility(member) != DesignerSerializationVisibility.Hidden) && !PropertyUsedInMethodSignature(member, methodParameters))
                    {
                        XamlObjectReader.MemberMarkupInfo propertyInfo = XamlObjectReader.MemberMarkupInfo.ForProperty(value, member, context);
                        if (propertyInfo != null)
                        {
                            if (member == xamlType.GetAliasedProperty(XamlLanguage.Name))
                            {
                                if (IsNull(propertyInfo, context) || IsEmptyString(propertyInfo))
                                {
                                    continue;
                                }
                                this.Name = ValidateNamePropertyAndFindName(propertyInfo);
                            }
                            propertyInfo.IsContent = this.IsPropertyContent(propertyInfo, xamlType);
                            this.Properties.Add(propertyInfo);
                        }
                    }
                }
                this.AddItemsProperty(value, context, xamlType);
                AddAttachedProperties(value, this, context);
            }

            private static void AddReference(object value, XamlObjectReader.ObjectMarkupInfo objectInfo, XamlObjectReader.SerializerContext context)
            {
                context.ReferenceTable.Add(value, objectInfo);
            }

            public void AssignName(XamlObjectReader.SerializerContext context)
            {
                if (this.Name == null)
                {
                    this.Name = context.AllocateIdentifier();
                    this.AddNameProperty(context);
                }
            }

            public void AssignName(string name, XamlObjectReader.SerializerContext context)
            {
                if (this.Name == null)
                {
                    this.Name = name;
                    if (name.StartsWith("__ReferenceID", StringComparison.Ordinal))
                    {
                        this.AddNameProperty(context);
                    }
                }
            }

            internal static bool CanPropertyXamlRoundtrip(XamlMember property, XamlObjectReader.SerializerContext context)
            {
                if (property.IsEvent || !context.IsPropertyReadVisible(property))
                {
                    return false;
                }
                if (!context.IsPropertyWriteVisible(property))
                {
                    return property.Type.IsUsableAsReadOnly;
                }
                return true;
            }

            private static void CheckTypeCanRoundtrip(XamlObjectReader.ObjectMarkupInfo objInfo)
            {
                XamlType xamlType = objInfo.XamlNode.XamlType;
                if (!xamlType.IsConstructible)
                {
                    foreach (XamlObjectReader.MarkupInfo info in objInfo.Properties)
                    {
                        if (((XamlObjectReader.MemberMarkupInfo) info).IsFactoryMethod && !xamlType.UnderlyingType.IsNested)
                        {
                            return;
                        }
                    }
                    if (xamlType.UnderlyingType.IsNested)
                    {
                        throw new XamlObjectReaderException(System.Xaml.SR.Get("ObjectReaderTypeIsNested", new object[] { xamlType.Name }));
                    }
                    throw new XamlObjectReaderException(System.Xaml.SR.Get("ObjectReaderTypeCannotRoundtrip", new object[] { xamlType.Name }));
                }
            }

            [SecuritySafeCritical, PermissionSet(SecurityAction.Demand, Unrestricted=true)]
            private void ConvertToInstanceDescriptor(XamlObjectReader.SerializerContext context, object instance, TypeConverter converter, out MemberInfo member, out ICollection arguments, out bool isComplete)
            {
                InstanceDescriptor descriptor = context.ConvertTo<InstanceDescriptor>(converter, instance);
                context.Instance = null;
                member = descriptor.MemberInfo;
                arguments = descriptor.Arguments;
                isComplete = descriptor.IsComplete;
            }

            private static string ConvertTypeAndMethodToString(Type type, string methodName, XamlObjectReader.SerializerContext context)
            {
                return (context.ConvertXamlTypeToString(context.LocalAssemblyAwareGetXamlType(type)) + "." + methodName);
            }

            public override List<XamlObjectReader.MarkupInfo> Decompose()
            {
                this.SortProperties();
                this.Properties.Add(XamlObjectReader.EndObjectMarkupInfo.Instance);
                return this.properties;
            }

            public override void EnsureNoDuplicateNames(Stack<XamlObjectReader.HashSet<string>> namesInCurrentScope)
            {
                if (!string.IsNullOrEmpty(this.Name) && !namesInCurrentScope.Peek().Add(this.Name))
                {
                    throw new XamlObjectReaderException(System.Xaml.SR.Get("ObjectReaderXamlNamedElementAlreadyRegistered", new object[] { this.Name }));
                }
                foreach (XamlObjectReader.MarkupInfo info in this.Properties)
                {
                    XamlObjectReader.MemberMarkupInfo info2 = (XamlObjectReader.MemberMarkupInfo) info;
                    foreach (XamlObjectReader.MarkupInfo info3 in info2.Children)
                    {
                        ((XamlObjectReader.ObjectOrValueMarkupInfo) info3).EnsureNoDuplicateNames(namesInCurrentScope);
                    }
                }
            }

            private XamlObjectReader.HashSet<string> FindAllAttributableProperties(out int posOfFirstNonAttributableProperty)
            {
                XamlObjectReader.HashSet<string> set = new XamlObjectReader.HashSet<string>();
                int num = 0;
                while (num < this.Properties.Count)
                {
                    XamlObjectReader.MemberMarkupInfo info = (XamlObjectReader.MemberMarkupInfo) this.Properties[num];
                    if (!info.IsAtomic && !info.IsAttributableMarkupExtension)
                    {
                        break;
                    }
                    set.Add(info.XamlNode.Member.Name);
                    num++;
                }
                posOfFirstNonAttributableProperty = num;
                return set;
            }

            public override void FindNamespace(XamlObjectReader.SerializerContext context)
            {
                if (base.XamlNode.NodeType == XamlNodeType.StartObject)
                {
                    context.FindPrefix(base.XamlNode.XamlType.PreferredXamlNamespace);
                    XamlType xamlType = base.XamlNode.XamlType;
                    if (xamlType.IsGeneric)
                    {
                        context.FindPrefix(XamlLanguage.TypeArguments.PreferredXamlNamespace);
                        this.FindNamespaceForTypeArguments(xamlType.TypeArguments, context);
                    }
                }
                foreach (XamlObjectReader.MarkupInfo info in this.Properties)
                {
                    info.FindNamespace(context);
                }
            }

            private void FindNamespaceForTypeArguments(IList<XamlType> types, XamlObjectReader.SerializerContext context)
            {
                if ((types != null) && (types.Count != 0))
                {
                    foreach (XamlType type in types)
                    {
                        context.FindPrefix(type.PreferredXamlNamespace);
                        this.FindNamespaceForTypeArguments(type.TypeArguments, context);
                    }
                }
            }

            private static XamlObjectReader.ObjectMarkupInfo ForArray(Array value, XamlObjectReader.SerializerContext context)
            {
                if (value.Rank > 1)
                {
                    throw new XamlObjectReaderException(System.Xaml.SR.Get("ObjectReaderMultidimensionalArrayNotSupported"));
                }
                XamlType itemType = context.LocalAssemblyAwareGetXamlType(value.GetType()).ItemType;
                XamlObjectReader.MemberMarkupInfo item = new XamlObjectReader.MemberMarkupInfo {
                    XamlNode = new XamlNode(XamlNodeType.StartMember, XamlLanguage.Items)
                };
                foreach (object obj2 in value)
                {
                    item.Children.Add(ForObject(obj2, context, null, false));
                }
                XamlObjectReader.ObjectMarkupInfo info8 = new XamlObjectReader.ObjectMarkupInfo {
                    XamlNode = new XamlNode(XamlNodeType.StartObject, XamlLanguage.Array),
                    Object = value
                };
                XamlObjectReader.MemberMarkupInfo info9 = new XamlObjectReader.MemberMarkupInfo {
                    XamlNode = new XamlNode(XamlNodeType.StartMember, XamlLanguage.Array.GetMember("Type"))
                };
                XamlObjectReader.ValueMarkupInfo info10 = new XamlObjectReader.ValueMarkupInfo {
                    XamlNode = new XamlNode(XamlNodeType.Value, context.ConvertXamlTypeToString(itemType))
                };
                info9.Children.Add(info10);
                info8.Properties.Add(info9);
                XamlObjectReader.ObjectMarkupInfo objectInfo = info8;
                if (item.Children.Count != 0)
                {
                    XamlObjectReader.ObjectMarkupInfo info5 = new XamlObjectReader.ObjectMarkupInfo {
                        XamlNode = new XamlNode(XamlNodeType.GetObject)
                    };
                    info5.Properties.Add(item);
                    XamlObjectReader.ObjectMarkupInfo info3 = info5;
                    XamlObjectReader.MemberMarkupInfo info6 = new XamlObjectReader.MemberMarkupInfo {
                        XamlNode = new XamlNode(XamlNodeType.StartMember, XamlLanguage.Array.ContentProperty)
                    };
                    info6.Children.Add(info3);
                    XamlObjectReader.MemberMarkupInfo info4 = info6;
                    objectInfo.Properties.Add(info4);
                }
                AddAttachedProperties(value, objectInfo, context);
                return objectInfo;
            }

            private static XamlObjectReader.ObjectMarkupInfo ForNull()
            {
                return new XamlObjectReader.ObjectMarkupInfo { XamlNode = new XamlNode(XamlNodeType.StartObject, XamlLanguage.Null) };
            }

            public static XamlObjectReader.ObjectMarkupInfo ForObject(object value, XamlObjectReader.SerializerContext context, TypeConverter instanceConverter = null, bool isRoot = false)
            {
                XamlObjectReader.ObjectMarkupInfo info2;
                if (value == null)
                {
                    return ForNull();
                }
                XamlObjectReader.ObjectMarkupInfo target = context.ReferenceTable.Find(value);
                if (target != null)
                {
                    target.AssignName(context);
                    return new XamlObjectReader.ReferenceMarkupInfo(target);
                }
                context.IsRoot = isRoot;
                Array array = value as Array;
                if (array != null)
                {
                    return ForArray(array, context);
                }
                XamlType xamlType = context.GetXamlType(value.GetType());
                ValueSerializer valueSerializer = null;
                TypeConverter typeConverter = null;
                if ((xamlType.ContentProperty == null) || ((xamlType.ContentProperty.TypeConverter != BuiltInValueConverter.String) && (xamlType.ContentProperty.TypeConverter != BuiltInValueConverter.Object)))
                {
                    valueSerializer = XamlObjectReader.TypeConverterExtensions.GetConverterInstance<ValueSerializer>(xamlType.ValueSerializer);
                    typeConverter = XamlObjectReader.TypeConverterExtensions.GetConverterInstance<TypeConverter>(xamlType.TypeConverter);
                }
                context.Instance = value;
                if (xamlType.DeferringLoader != null)
                {
                    info2 = XamlObjectReader.MemberMarkupInfo.ConvertToXamlReader(value, xamlType.DeferringLoader, context);
                }
                else if (((typeConverter != null) && (valueSerializer != null)) && context.CanRoundtripUsingValueSerializer(valueSerializer, typeConverter, value))
                {
                    if (isRoot)
                    {
                        context.ReserveDefaultPrefixForRootObject(value);
                    }
                    string str = context.ConvertToString(valueSerializer, value);
                    context.Instance = null;
                    info2 = ForTypeConverted(str, value, context);
                }
                else if ((typeConverter != null) && context.TryConvertToMarkupExtension(typeConverter, ref value))
                {
                    context.Instance = null;
                    if (isRoot)
                    {
                        context.ReserveDefaultPrefixForRootObject(value);
                    }
                    info2 = ForObject(value, context, null, false);
                }
                else if (value is Type)
                {
                    context.Instance = null;
                    info2 = ForObject(new TypeExtension((Type) value), context, null, false);
                }
                else if ((typeConverter != null) && context.CanRoundTripString(typeConverter))
                {
                    if (isRoot)
                    {
                        context.ReserveDefaultPrefixForRootObject(value);
                    }
                    string str2 = context.ConvertTo<string>(typeConverter, value);
                    context.Instance = null;
                    info2 = ForTypeConverted(str2, value, context);
                }
                else if (value is string)
                {
                    context.Instance = null;
                    info2 = ForTypeConverted((string) value, value, context);
                }
                else
                {
                    if (isRoot)
                    {
                        context.ReserveDefaultPrefixForRootObject(value);
                    }
                    context.Instance = null;
                    info2 = ForObjectInternal(value, context, instanceConverter);
                }
                string name = context.ReferenceTable.FindInServiceProviderTable(value);
                if (name != null)
                {
                    info2.AssignName(name, context);
                }
                CheckTypeCanRoundtrip(info2);
                return info2;
            }

            private static XamlObjectReader.ObjectMarkupInfo ForObjectInternal(object value, XamlObjectReader.SerializerContext context, TypeConverter converter)
            {
                XamlType data = context.LocalAssemblyAwareGetXamlType(value.GetType());
                if (value is System.Windows.Markup.INameScope)
                {
                    XamlObjectReader.NameScopeMarkupInfo item = new XamlObjectReader.NameScopeMarkupInfo {
                        XamlNode = new XamlNode(XamlNodeType.StartObject, data),
                        Object = value,
                        SourceObject = value,
                        ParentTable = context.ReferenceTable
                    };
                    context.PendingNameScopes.Enqueue(item);
                    AddReference(value, item, context);
                    return item;
                }
                XamlObjectReader.ObjectMarkupInfo objectInfo = new XamlObjectReader.ObjectMarkupInfo {
                    XamlNode = new XamlNode(XamlNodeType.StartObject, data),
                    Object = value
                };
                AddReference(value, objectInfo, context);
                objectInfo.AddRecordMembers(value, context, converter);
                return objectInfo;
            }

            private static XamlObjectReader.ObjectMarkupInfo ForTypeConverted(string value, object originalValue, XamlObjectReader.SerializerContext context)
            {
                XamlType data = context.LocalAssemblyAwareGetXamlType(originalValue.GetType());
                XamlObjectReader.ObjectMarkupInfo objectInfo = new XamlObjectReader.ObjectMarkupInfo {
                    XamlNode = new XamlNode(XamlNodeType.StartObject, data),
                    Object = originalValue
                };
                value = value ?? string.Empty;
                XamlObjectReader.MemberMarkupInfo item = new XamlObjectReader.MemberMarkupInfo {
                    XamlNode = new XamlNode(XamlNodeType.StartMember, XamlLanguage.Initialization)
                };
                XamlObjectReader.ValueMarkupInfo info4 = new XamlObjectReader.ValueMarkupInfo {
                    XamlNode = new XamlNode(XamlNodeType.Value, value)
                };
                item.Children.Add(info4);
                objectInfo.Properties.Add(item);
                AddAttachedProperties(originalValue, objectInfo, context);
                return objectInfo;
            }

            private void GetConstructorInfo(object value, XamlType valueXamlType, XamlObjectReader.SerializerContext context, out MemberInfo member, out ICollection arguments, out bool isComplete)
            {
                member = null;
                arguments = null;
                isComplete = false;
                ICollection<XamlMember> allMembers = valueXamlType.GetAllMembers();
                ICollection<XamlMember> allExcludedReadOnlyMembers = valueXamlType.GetAllExcludedReadOnlyMembers();
                List<XamlMember> list = new List<XamlMember>();
                foreach (XamlMember member2 in allMembers)
                {
                    if (context.IsPropertyReadVisible(member2) && !string.IsNullOrEmpty(XamlObjectReader.GetConstructorArgument(member2)))
                    {
                        list.Add(member2);
                    }
                }
                foreach (XamlMember member3 in allExcludedReadOnlyMembers)
                {
                    if (context.IsPropertyReadVisible(member3) && !string.IsNullOrEmpty(XamlObjectReader.GetConstructorArgument(member3)))
                    {
                        list.Add(member3);
                    }
                }
                foreach (ConstructorInfo info in valueXamlType.GetConstructors())
                {
                    ParameterInfo[] parameters = info.GetParameters();
                    if (parameters.Length == list.Count)
                    {
                        IList list2 = new List<object>(parameters.Length);
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            ParameterInfo info2 = parameters[i];
                            XamlMember property = null;
                            foreach (XamlMember member5 in list)
                            {
                                if ((member5.Type.UnderlyingType == info2.ParameterType) && (XamlObjectReader.GetConstructorArgument(member5) == info2.Name))
                                {
                                    property = member5;
                                    break;
                                }
                            }
                            if (property == null)
                            {
                                break;
                            }
                            list2.Add(context.Runtime.GetValue(value, property));
                        }
                        if (list2.Count == list.Count)
                        {
                            member = info;
                            arguments = list2;
                            if (((list2.Count == allMembers.Count) && !valueXamlType.IsCollection) && !valueXamlType.IsDictionary)
                            {
                                isComplete = true;
                            }
                            break;
                        }
                    }
                }
                if ((member == null) && !valueXamlType.UnderlyingType.IsValueType)
                {
                    if (list.Count == 0)
                    {
                        throw new XamlObjectReaderException(System.Xaml.SR.Get("ObjectReaderNoDefaultConstructor", new object[] { value.GetType() }));
                    }
                    throw new XamlObjectReaderException(System.Xaml.SR.Get("ObjectReaderNoMatchingConstructor", new object[] { value.GetType() }));
                }
            }

            private ParameterInfo[] GetMethodParams(MemberInfo memberInfo)
            {
                ParameterInfo[] parameters = null;
                MethodBase base2 = memberInfo as MethodBase;
                if (base2 != null)
                {
                    parameters = base2.GetParameters();
                }
                return parameters;
            }

            private static List<XamlMember> GetXamlSerializableProperties(XamlType type, XamlObjectReader.SerializerContext context)
            {
                List<XamlMember> list = new List<XamlMember>();
                foreach (XamlMember member in type.GetAllMembers())
                {
                    if (CanPropertyXamlRoundtrip(member, context))
                    {
                        list.Add(member);
                    }
                }
                return list;
            }

            private void InsertPropertiesWithDO(List<XamlObjectReader.MarkupInfo> propertiesWithDO)
            {
                int num;
                XamlObjectReader.HashSet<string> namesOfAttributableProperties = this.FindAllAttributableProperties(out num);
                foreach (XamlObjectReader.MarkupInfo info in propertiesWithDO)
                {
                    XamlObjectReader.MemberMarkupInfo info2 = (XamlObjectReader.MemberMarkupInfo) info;
                    if (this.IsMemberOnlyDependentOnAttributableMembers(info2.XamlNode.Member, namesOfAttributableProperties) && (info2.IsAtomic || info2.IsAttributableMarkupExtension))
                    {
                        this.properties.Insert(num, info);
                        namesOfAttributableProperties.Add(info2.XamlNode.Member.Name);
                        num++;
                    }
                    else
                    {
                        this.Properties.Add(info);
                    }
                }
            }

            private static bool IsEmptyString(XamlObjectReader.MemberMarkupInfo propertyInfo)
            {
                if (propertyInfo.Children.Count == 1)
                {
                    XamlObjectReader.ValueMarkupInfo info = propertyInfo.Children[0] as XamlObjectReader.ValueMarkupInfo;
                    if (info != null)
                    {
                        return object.Equals(info.XamlNode.Value, string.Empty);
                    }
                }
                return false;
            }

            private bool IsMemberOnlyDependentOnAttributableMembers(XamlMember member, XamlObjectReader.HashSet<string> namesOfAttributableProperties)
            {
                foreach (XamlMember member2 in member.DependsOn)
                {
                    if (!namesOfAttributableProperties.Contains(member2.Name))
                    {
                        return false;
                    }
                }
                return true;
            }

            private static bool IsNull(XamlObjectReader.MemberMarkupInfo propertyInfo, XamlObjectReader.SerializerContext context)
            {
                if (propertyInfo.Children.Count == 1)
                {
                    XamlObjectReader.ObjectMarkupInfo info = propertyInfo.Children[0] as XamlObjectReader.ObjectMarkupInfo;
                    if (info != null)
                    {
                        return (info.XamlNode.XamlType == XamlLanguage.Null);
                    }
                }
                return false;
            }

            private bool IsPropertyContent(XamlObjectReader.MemberMarkupInfo propertyInfo, XamlType containingType)
            {
                XamlMember member = propertyInfo.XamlNode.Member;
                if (member != containingType.ContentProperty)
                {
                    return false;
                }
                if (propertyInfo.IsAtomic)
                {
                    return XamlLanguage.String.CanAssignTo(member.Type);
                }
                return true;
            }

            private static bool PropertyUsedInMethodSignature(XamlMember property, ParameterInfo[] methodParameters)
            {
                if ((methodParameters != null) && !string.IsNullOrEmpty(XamlObjectReader.GetConstructorArgument(property)))
                {
                    foreach (ParameterInfo info in methodParameters)
                    {
                        if ((info.Name == XamlObjectReader.GetConstructorArgument(property)) && (property.Type.UnderlyingType == info.ParameterType))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            private void ReorderPropertiesWithDO()
            {
                List<XamlObjectReader.MarkupInfo> list;
                this.SelectAndRemovePropertiesWithDO(out list);
                if (list != null)
                {
                    this.InsertPropertiesWithDO(list);
                }
            }

            private void SelectAndRemovePropertiesWithDO(out List<XamlObjectReader.MarkupInfo> removedProperties)
            {
                removedProperties = null;
                XamlObjectReader.PartiallyOrderedList<string, XamlObjectReader.MarkupInfo> collection = null;
                int index = 0;
                while (index < this.properties.Count)
                {
                    XamlObjectReader.MarkupInfo info = this.properties[index];
                    if (info.XamlNode.Member.DependsOn.Count > 0)
                    {
                        if (collection == null)
                        {
                            collection = new XamlObjectReader.PartiallyOrderedList<string, XamlObjectReader.MarkupInfo>();
                        }
                        string name = info.XamlNode.Member.Name;
                        collection.Add(name, info);
                        foreach (XamlMember member in info.XamlNode.Member.DependsOn)
                        {
                            collection.SetOrder(member.Name, name);
                        }
                        this.properties.RemoveAt(index);
                    }
                    else
                    {
                        index++;
                    }
                }
                if (collection != null)
                {
                    removedProperties = new List<XamlObjectReader.MarkupInfo>(collection);
                }
            }

            private void SortProperties()
            {
                if (this.IsAttributableMarkupExtension)
                {
                    this.Properties.Sort(PropertySorterForCurlySyntax.Instance);
                }
                else
                {
                    this.Properties.Sort(PropertySorterForXmlSyntax.Instance);
                }
                this.ReorderPropertiesWithDO();
            }

            private bool TryAddPositionalParameters(XamlType xamlType, MemberInfo member, ICollection arguments, XamlObjectReader.SerializerContext context)
            {
                if ((arguments == null) || (arguments.Count <= 0))
                {
                    return false;
                }
                ParameterInfo[] methodParams = this.GetMethodParams(member);
                XamlObjectReader.MemberMarkupInfo item = new XamlObjectReader.MemberMarkupInfo {
                    XamlNode = new XamlNode(XamlNodeType.StartMember, XamlLanguage.PositionalParameters)
                };
                int num = 0;
                foreach (object obj2 in arguments)
                {
                    XamlType type = context.GetXamlType(methodParams[num++].ParameterType);
                    ValueSerializer converterInstance = XamlObjectReader.TypeConverterExtensions.GetConverterInstance<ValueSerializer>(type.ValueSerializer);
                    TypeConverter typeConverter = XamlObjectReader.TypeConverterExtensions.GetConverterInstance<TypeConverter>(type.TypeConverter);
                    XamlObjectReader.ObjectMarkupInfo info2 = null;
                    object obj3 = obj2;
                    context.Instance = obj2;
                    if (((typeConverter != null) && (converterInstance != null)) && context.CanRoundtripUsingValueSerializer(converterInstance, typeConverter, obj2))
                    {
                        string data = context.ConvertToString(converterInstance, obj2);
                        context.Instance = null;
                        XamlObjectReader.ValueMarkupInfo info3 = new XamlObjectReader.ValueMarkupInfo {
                            XamlNode = new XamlNode(XamlNodeType.Value, data)
                        };
                        item.Children.Add(info3);
                    }
                    else if (((typeConverter != null) && context.TryConvertToMarkupExtension(typeConverter, ref obj3)) || (obj3 is MarkupExtension))
                    {
                        context.Instance = null;
                        info2 = ForObject(obj3, context, null, false);
                        if (!info2.IsAttributableMarkupExtension)
                        {
                            return false;
                        }
                        item.Children.Add(info2);
                    }
                    else if ((typeConverter != null) && context.CanRoundTripString(typeConverter))
                    {
                        string str2 = context.ConvertTo<string>(typeConverter, obj2);
                        context.Instance = null;
                        XamlObjectReader.ValueMarkupInfo info4 = new XamlObjectReader.ValueMarkupInfo {
                            XamlNode = new XamlNode(XamlNodeType.Value, str2)
                        };
                        item.Children.Add(info4);
                    }
                    else if (obj2 is string)
                    {
                        context.Instance = null;
                        XamlObjectReader.ValueMarkupInfo info5 = new XamlObjectReader.ValueMarkupInfo {
                            XamlNode = new XamlNode(XamlNodeType.Value, obj2)
                        };
                        item.Children.Add(info5);
                    }
                    else
                    {
                        context.Instance = null;
                        return false;
                    }
                }
                this.Properties.Add(item);
                return true;
            }

            private bool TryGetDefaultConstructorInfo(XamlType type, out MemberInfo member, out ICollection arguments, out bool isComplete)
            {
                arguments = null;
                isComplete = false;
                member = null;
                return (type.IsConstructible && !type.ConstructionRequiresArguments);
            }

            private bool TryGetInstanceDescriptorInfo(object value, XamlObjectReader.SerializerContext context, TypeConverter converter, out MemberInfo member, out ICollection arguments, out bool isComplete)
            {
                bool flag = false;
                member = null;
                arguments = null;
                isComplete = false;
                context.Instance = value;
                if ((converter != null) && context.CanConvertTo(converter, typeof(InstanceDescriptor)))
                {
                    this.ConvertToInstanceDescriptor(context, value, converter, out member, out arguments, out isComplete);
                    flag = true;
                }
                return flag;
            }

            private static string ValidateNamePropertyAndFindName(XamlObjectReader.MemberMarkupInfo propertyInfo)
            {
                if (propertyInfo.Children.Count == 1)
                {
                    XamlObjectReader.ValueMarkupInfo info = propertyInfo.Children[0] as XamlObjectReader.ValueMarkupInfo;
                    if (info != null)
                    {
                        string str = info.XamlNode.Value as string;
                        if (str != null)
                        {
                            return str;
                        }
                    }
                }
                XamlMember member = propertyInfo.XamlNode.Member;
                throw new XamlObjectReaderException(System.Xaml.SR.Get("ObjectReaderXamlNamePropertyMustBeString", new object[] { member.Name, member.DeclaringType }));
            }

            public virtual bool IsAttributableMarkupExtension
            {
                get
                {
                    if (this.isAttributableMarkupExtension.HasValue)
                    {
                        return this.isAttributableMarkupExtension.Value;
                    }
                    if (((base.XamlNode.NodeType == XamlNodeType.StartObject) && !base.XamlNode.XamlType.IsMarkupExtension) || (base.XamlNode.NodeType == XamlNodeType.GetObject))
                    {
                        this.isAttributableMarkupExtension = false;
                        return false;
                    }
                    foreach (XamlObjectReader.MarkupInfo info in this.Properties)
                    {
                        if (!((XamlObjectReader.MemberMarkupInfo) info).IsAttributable)
                        {
                            this.isAttributableMarkupExtension = false;
                            return false;
                        }
                    }
                    this.isAttributableMarkupExtension = true;
                    return true;
                }
            }

            public string Name { get; set; }

            public object Object { get; set; }

            public List<XamlObjectReader.MarkupInfo> Properties
            {
                get
                {
                    return this.properties;
                }
            }

            private class PropertySorterForCurlySyntax : IComparer<XamlObjectReader.MarkupInfo>
            {
                private const int Equal = 0;
                public static readonly XamlObjectReader.ObjectMarkupInfo.PropertySorterForCurlySyntax Instance = new XamlObjectReader.ObjectMarkupInfo.PropertySorterForCurlySyntax();
                private const int XFirst = -1;
                private const int YFirst = 1;

                public int Compare(XamlObjectReader.MarkupInfo x, XamlObjectReader.MarkupInfo y)
                {
                    XamlObjectReader.MemberMarkupInfo info = (XamlObjectReader.MemberMarkupInfo) x;
                    XamlObjectReader.MemberMarkupInfo info2 = (XamlObjectReader.MemberMarkupInfo) y;
                    XamlMember member = x.XamlNode.Member;
                    XamlMember member2 = y.XamlNode.Member;
                    bool flag = member == XamlLanguage.PositionalParameters;
                    bool flag2 = member2 == XamlLanguage.PositionalParameters;
                    if (flag && !flag2)
                    {
                        return -1;
                    }
                    if (flag2 && !flag)
                    {
                        return 1;
                    }
                    bool isFactoryMethod = info.IsFactoryMethod;
                    bool flag4 = info2.IsFactoryMethod;
                    if (isFactoryMethod && !flag4)
                    {
                        return -1;
                    }
                    if (flag4 && !isFactoryMethod)
                    {
                        return 1;
                    }
                    bool isAttributableMarkupExtension = info.IsAttributableMarkupExtension;
                    bool flag6 = info2.IsAttributableMarkupExtension;
                    if (isAttributableMarkupExtension && !flag6)
                    {
                        return -1;
                    }
                    if (flag6 && !isAttributableMarkupExtension)
                    {
                        return 1;
                    }
                    bool isAtomic = info.IsAtomic;
                    bool flag8 = info2.IsAtomic;
                    if (isAtomic && !flag8)
                    {
                        return -1;
                    }
                    if (flag8 && !isAtomic)
                    {
                        return 1;
                    }
                    bool isDirective = member.IsDirective;
                    bool flag10 = member2.IsDirective;
                    if (isDirective && !flag10)
                    {
                        return -1;
                    }
                    if (flag10 && !flag10)
                    {
                        return 1;
                    }
                    return string.CompareOrdinal(member.Name, member2.Name);
                }
            }

            private class PropertySorterForXmlSyntax : IComparer<XamlObjectReader.MarkupInfo>
            {
                private const int Equal = 0;
                public static readonly XamlObjectReader.ObjectMarkupInfo.PropertySorterForXmlSyntax Instance = new XamlObjectReader.ObjectMarkupInfo.PropertySorterForXmlSyntax();
                private const int XFirst = -1;
                private const int YFirst = 1;

                public int Compare(XamlObjectReader.MarkupInfo x, XamlObjectReader.MarkupInfo y)
                {
                    XamlObjectReader.MemberMarkupInfo info = (XamlObjectReader.MemberMarkupInfo) x;
                    XamlObjectReader.MemberMarkupInfo info2 = (XamlObjectReader.MemberMarkupInfo) y;
                    XamlMember member = x.XamlNode.Member;
                    XamlMember member2 = y.XamlNode.Member;
                    bool isFactoryMethod = info.IsFactoryMethod;
                    bool flag2 = info2.IsFactoryMethod;
                    if (isFactoryMethod && !flag2)
                    {
                        return -1;
                    }
                    if (flag2 && !isFactoryMethod)
                    {
                        return 1;
                    }
                    bool flag3 = info.IsContent || (member == XamlLanguage.Items);
                    bool flag4 = info2.IsContent || (member2 == XamlLanguage.Items);
                    if (flag3 && !flag4)
                    {
                        return 1;
                    }
                    if (flag4 && !flag3)
                    {
                        return -1;
                    }
                    bool isAttributableMarkupExtension = info.IsAttributableMarkupExtension;
                    bool flag6 = info2.IsAttributableMarkupExtension;
                    if (isAttributableMarkupExtension && !flag6)
                    {
                        return -1;
                    }
                    if (flag6 && !isAttributableMarkupExtension)
                    {
                        return 1;
                    }
                    bool isAtomic = info.IsAtomic;
                    bool flag8 = info2.IsAtomic;
                    bool flag9 = member == XamlLanguage.Initialization;
                    bool flag10 = member2 == XamlLanguage.Initialization;
                    bool flag11 = isAtomic && !flag9;
                    bool flag12 = flag8 && !flag10;
                    if (flag11 && !flag12)
                    {
                        return -1;
                    }
                    if (flag12 && !flag11)
                    {
                        return 1;
                    }
                    if (isAtomic && !flag8)
                    {
                        return -1;
                    }
                    if (flag8 && !isAtomic)
                    {
                        return 1;
                    }
                    if (flag9 && !flag10)
                    {
                        return -1;
                    }
                    if (flag10 && !flag9)
                    {
                        return 1;
                    }
                    bool flag13 = member == XamlLanguage.Arguments;
                    bool flag14 = member2 == XamlLanguage.Arguments;
                    if (flag13 && !flag14)
                    {
                        return -1;
                    }
                    if (flag14 && !flag13)
                    {
                        return 1;
                    }
                    bool isDirective = member.IsDirective;
                    bool flag16 = member2.IsDirective;
                    if (isDirective && !flag16)
                    {
                        return -1;
                    }
                    if (flag16 && !isDirective)
                    {
                        return 1;
                    }
                    return string.CompareOrdinal(member.Name, member2.Name);
                }
            }
        }

        private abstract class ObjectOrValueMarkupInfo : XamlObjectReader.MarkupInfo
        {
            protected ObjectOrValueMarkupInfo()
            {
            }

            public virtual void EnsureNoDuplicateNames(Stack<XamlObjectReader.HashSet<string>> namesInCurrentScope)
            {
            }
        }

        private class ObjectReferenceEqualityComparer : IEqualityComparer<object>
        {
            public bool Equals(object x, object y)
            {
                return object.ReferenceEquals(x, y);
            }

            public int GetHashCode(object obj)
            {
                if (obj == null)
                {
                    return 0;
                }
                return obj.GetHashCode();
            }
        }

        private class PartiallyOrderedList<TKey, TValue> : IEnumerable<TValue>, IEnumerable where TValue: class
        {
            private List<Entry<TKey, TValue>> _entries;
            private int _firstIndex;
            private int _lastIndex;

            public PartiallyOrderedList()
            {
                this._entries = new List<Entry<TKey, TValue>>();
                this._firstIndex = -1;
            }

            public void Add(TKey key, TValue value)
            {
                Entry<TKey, TValue> item = new Entry<TKey, TValue>(key, value);
                int index = this._entries.IndexOf(item);
                if (index >= 0)
                {
                    item.Predecessors = this._entries[index].Predecessors;
                    this._entries[index] = item;
                }
                else
                {
                    this._entries.Add(item);
                }
            }

            private void DepthFirstSearch(int index)
            {
                if (this._entries[index].Link == -1)
                {
                    this._entries[index].Link = -2;
                    if (this._entries[index].Predecessors != null)
                    {
                        foreach (int num in this._entries[index].Predecessors)
                        {
                            this.DepthFirstSearch(num);
                        }
                    }
                    if (this._lastIndex == -1)
                    {
                        this._firstIndex = index;
                    }
                    else
                    {
                        this._entries[this._lastIndex].Link = index;
                    }
                    this._lastIndex = index;
                }
            }

            private int GetEntryIndex(TKey key)
            {
                Entry<TKey, TValue> item = new Entry<TKey, TValue>(key, default(TValue));
                int index = this._entries.IndexOf(item);
                if (index < 0)
                {
                    index = this._entries.Count;
                    this._entries.Add(item);
                }
                return index;
            }

            public IEnumerator<TValue> GetEnumerator()
            {
                Entry<TKey, TValue> iteratorVariable1;
                if (this._firstIndex < 0)
                {
                    this.TopologicalSort();
                }
                for (int i = this._firstIndex; i >= 0; i = iteratorVariable1.Link)
                {
                    iteratorVariable1 = this._entries[i];
                    if (iteratorVariable1.Value != null)
                    {
                        yield return iteratorVariable1.Value;
                    }
                }
            }

            public void SetOrder(TKey predecessor, TKey key)
            {
                int entryIndex = this.GetEntryIndex(predecessor);
                Entry<TKey, TValue> local1 = this._entries[entryIndex];
                int num2 = this.GetEntryIndex(key);
                Entry<TKey, TValue> entry = this._entries[num2];
                if (entry.Predecessors == null)
                {
                    entry.Predecessors = new List<int>();
                }
                entry.Predecessors.Add(entryIndex);
                this._firstIndex = -1;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                foreach (TValue iteratorVariable0 in this)
                {
                    yield return iteratorVariable0;
                }
            }

            private void TopologicalSort()
            {
                this._firstIndex = -1;
                this._lastIndex = -1;
                for (int i = 0; i < this._entries.Count; i++)
                {
                    this._entries[i].Link = -1;
                }
                for (int j = 0; j < this._entries.Count; j++)
                {
                    this.DepthFirstSearch(j);
                }
            }

            [CompilerGenerated]
            private sealed class <GetEnumerator>d__36 : IEnumerator<TValue>, IEnumerator, IDisposable
            {
                private int <>1__state;
                private TValue <>2__current;
                public XamlObjectReader.PartiallyOrderedList<TKey, TValue> <>4__this;
                public XamlObjectReader.PartiallyOrderedList<TKey, TValue>.Entry <entry>5__38;
                public int <index>5__37;

                [DebuggerHidden]
                public <GetEnumerator>d__36(int <>1__state)
                {
                    this.<>1__state = <>1__state;
                }

                private bool MoveNext()
                {
                    switch (this.<>1__state)
                    {
                        case 0:
                            this.<>1__state = -1;
                            if (this.<>4__this._firstIndex < 0)
                            {
                                this.<>4__this.TopologicalSort();
                            }
                            this.<index>5__37 = this.<>4__this._firstIndex;
                            while (this.<index>5__37 >= 0)
                            {
                                this.<entry>5__38 = this.<>4__this._entries[this.<index>5__37];
                                if (this.<entry>5__38.Value == null)
                                {
                                    goto Label_009C;
                                }
                                this.<>2__current = this.<entry>5__38.Value;
                                this.<>1__state = 1;
                                return true;
                            Label_0095:
                                this.<>1__state = -1;
                            Label_009C:
                                this.<index>5__37 = this.<entry>5__38.Link;
                            }
                            break;

                        case 1:
                            goto Label_0095;
                    }
                    return false;
                }

                [DebuggerHidden]
                void IEnumerator.Reset()
                {
                    throw new NotSupportedException();
                }

                void IDisposable.Dispose()
                {
                }

                TValue IEnumerator<TValue>.Current
                {
                    [DebuggerHidden]
                    get
                    {
                        return this.<>2__current;
                    }
                }

                object IEnumerator.Current
                {
                    [DebuggerHidden]
                    get
                    {
                        return this.<>2__current;
                    }
                }
            }

            private class Entry
            {
                public const int INDFS = -2;
                public readonly TKey Key;
                public int Link;
                public List<int> Predecessors;
                public const int UNSEEN = -1;
                public readonly TValue Value;

                public Entry(TKey key, TValue value)
                {
                    this.Key = key;
                    this.Value = value;
                    this.Predecessors = null;
                    this.Link = 0;
                }

                public override bool Equals(object obj)
                {
                    XamlObjectReader.PartiallyOrderedList<TKey, TValue>.Entry entry = obj as XamlObjectReader.PartiallyOrderedList<TKey, TValue>.Entry;
                    return ((entry != null) && entry.Key.Equals(this.Key));
                }

                public override int GetHashCode()
                {
                    return this.Key.GetHashCode();
                }
            }

            [CompilerGenerated]
            private sealed class GetEnumerator>d__3a : IEnumerator<object>, IEnumerator, IDisposable
            {
                private int <>1__state;
                private object <>2__current;
                public XamlObjectReader.PartiallyOrderedList<TKey, TValue> <>4__this;
                public IEnumerator<TValue> <>7__wrap3c;
                public TValue <value>5__3b;

                [DebuggerHidden]
                public GetEnumerator>d__3a(int <>1__state)
                {
                    this.<>1__state = <>1__state;
                }

                private void <>m__Finally3d()
                {
                    this.<>1__state = -1;
                    if (this.<>7__wrap3c != null)
                    {
                        this.<>7__wrap3c.Dispose();
                    }
                }

                private bool MoveNext()
                {
                    bool flag;
                    try
                    {
                        switch (this.<>1__state)
                        {
                            case 0:
                                this.<>1__state = -1;
                                this.<>7__wrap3c = this.<>4__this.GetEnumerator();
                                this.<>1__state = 1;
                                goto Label_0070;

                            case 2:
                                this.<>1__state = 1;
                                goto Label_0070;

                            default:
                                goto Label_0083;
                        }
                    Label_003C:
                        this.<value>5__3b = this.<>7__wrap3c.Current;
                        this.<>2__current = this.<value>5__3b;
                        this.<>1__state = 2;
                        return true;
                    Label_0070:
                        if (this.<>7__wrap3c.MoveNext())
                        {
                            goto Label_003C;
                        }
                        this.<>m__Finally3d();
                    Label_0083:
                        flag = false;
                    }
                    fault
                    {
                        this.System.IDisposable.Dispose();
                    }
                    return flag;
                }

                [DebuggerHidden]
                void IEnumerator.Reset()
                {
                    throw new NotSupportedException();
                }

                void IDisposable.Dispose()
                {
                    switch (this.<>1__state)
                    {
                        case 1:
                        case 2:
                            try
                            {
                            }
                            finally
                            {
                                this.<>m__Finally3d();
                            }
                            return;
                    }
                }

                object IEnumerator<object>.Current
                {
                    [DebuggerHidden]
                    get
                    {
                        return this.<>2__current;
                    }
                }

                object IEnumerator.Current
                {
                    [DebuggerHidden]
                    get
                    {
                        return this.<>2__current;
                    }
                }
            }
        }

        private class ReferenceMarkupInfo : XamlObjectReader.ObjectMarkupInfo
        {
            private XamlObjectReader.MemberMarkupInfo nameProperty;

            public ReferenceMarkupInfo(XamlObjectReader.ObjectMarkupInfo target)
            {
                base.XamlNode = new XamlNode(XamlNodeType.StartObject, XamlLanguage.Reference);
                XamlObjectReader.MemberMarkupInfo info = new XamlObjectReader.MemberMarkupInfo {
                    XamlNode = new XamlNode(XamlNodeType.StartMember, XamlLanguage.PositionalParameters)
                };
                this.nameProperty = info;
                base.Properties.Add(this.nameProperty);
                this.Target = target;
                base.Object = target.Object;
            }

            public override List<XamlObjectReader.MarkupInfo> Decompose()
            {
                XamlObjectReader.ValueMarkupInfo item = new XamlObjectReader.ValueMarkupInfo {
                    XamlNode = new XamlNode(XamlNodeType.Value, this.Target.Name)
                };
                this.nameProperty.Children.Add(item);
                return base.Decompose();
            }

            public XamlObjectReader.ObjectMarkupInfo Target { get; set; }
        }

        private class ReferenceTable
        {
            private Dictionary<object, XamlObjectReader.ObjectMarkupInfo> objectGraphTable;
            private XamlObjectReader.ReferenceTable parent;
            private Dictionary<object, string> serviceProviderTable;

            public ReferenceTable(XamlObjectReader.ReferenceTable parent)
            {
                this.parent = parent;
                this.objectGraphTable = new Dictionary<object, XamlObjectReader.ObjectMarkupInfo>(new XamlObjectReader.ObjectReferenceEqualityComparer());
            }

            public void Add(object value, XamlObjectReader.ObjectMarkupInfo info)
            {
                this.objectGraphTable.Add(value, info);
            }

            public void AddToServiceProviderTable(object value, string name)
            {
                if (this.serviceProviderTable == null)
                {
                    this.serviceProviderTable = new Dictionary<object, string>(new XamlObjectReader.ObjectReferenceEqualityComparer());
                }
                this.serviceProviderTable.Add(value, name);
            }

            public XamlObjectReader.ObjectMarkupInfo Find(object value)
            {
                XamlObjectReader.ObjectMarkupInfo info;
                if (!this.objectGraphTable.TryGetValue(value, out info) && (this.parent != null))
                {
                    return this.parent.Find(value);
                }
                return info;
            }

            public string FindInServiceProviderTable(object value)
            {
                string str = null;
                if (this.serviceProviderTable != null)
                {
                    this.serviceProviderTable.TryGetValue(value, out str);
                }
                return str;
            }
        }

        private class SerializerContext
        {
            private int lastIdentifier;
            private Dictionary<string, string> namespaceToPrefixMap;
            private Queue<XamlObjectReader.NameScopeMarkupInfo> pendingNameScopes = new Queue<XamlObjectReader.NameScopeMarkupInfo>();
            private Dictionary<string, string> prefixToNamespaceMap;
            private ClrObjectRuntime runtime;
            private XamlSchemaContext schemaContext;
            private XamlObjectReaderSettings settings;
            private ITypeDescriptorContext typeDescriptorContext;
            private IValueSerializerContext valueSerializerContext;

            public SerializerContext(XamlSchemaContext schemaContext, XamlObjectReaderSettings settings)
            {
                XamlObjectReader.TypeDescriptorAndValueSerializerContext context = new XamlObjectReader.TypeDescriptorAndValueSerializerContext(this);
                this.typeDescriptorContext = context;
                this.valueSerializerContext = context;
                this.namespaceToPrefixMap = new Dictionary<string, string>();
                this.prefixToNamespaceMap = new Dictionary<string, string>();
                this.ReferenceTable = new System.Xaml.XamlObjectReader.ReferenceTable(null);
                this.schemaContext = schemaContext;
                this.runtime = new ClrObjectRuntime(null, false);
                this.settings = settings;
            }

            public string AllocateIdentifier()
            {
                return ("__ReferenceID" + this.lastIdentifier++);
            }

            public bool CanConvertTo(TypeConverter converter, Type type)
            {
                return this.Runtime.CanConvertTo(this.TypeDescriptorContext, converter, type);
            }

            public bool CanRoundTripString(TypeConverter converter)
            {
                if (converter is ReferenceConverter)
                {
                    return false;
                }
                return (this.Runtime.CanConvertFrom<string>(this.TypeDescriptorContext, converter) && this.Runtime.CanConvertTo(this.TypeDescriptorContext, converter, typeof(string)));
            }

            public bool CanRoundtripUsingValueSerializer(ValueSerializer valueSerializer, TypeConverter typeConverter, object value)
            {
                return ((((valueSerializer != null) && (typeConverter != null)) && this.Runtime.CanConvertToString(this.ValueSerializerContext, valueSerializer, value)) && this.Runtime.CanConvertFrom<string>(this.TypeDescriptorContext, typeConverter));
            }

            private static int CompareByValue(KeyValuePair<string, string> x, KeyValuePair<string, string> y)
            {
                return string.Compare(y.Value, x.Value, false, TypeConverterHelper.InvariantEnglishUS);
            }

            public T ConvertTo<T>(TypeConverter converter, object value)
            {
                return this.Runtime.ConvertToValue<T>(this.TypeDescriptorContext, converter, value);
            }

            public string ConvertToString(ValueSerializer valueSerializer, object value)
            {
                return this.Runtime.ConvertToString(this.valueSerializerContext, valueSerializer, value);
            }

            public string ConvertXamlTypeToString(XamlType type)
            {
                XamlTypeName name = new XamlTypeName(type);
                return name.ConvertToStringInternal(new Func<string, string>(this.FindPrefix));
            }

            public string FindPrefix(string ns)
            {
                string str = null;
                if (!this.namespaceToPrefixMap.TryGetValue(ns, out str))
                {
                    string preferredPrefix = this.SchemaContext.GetPreferredPrefix(ns);
                    if ((preferredPrefix != "x") && !this.namespaceToPrefixMap.ContainsValue(string.Empty))
                    {
                        str = string.Empty;
                    }
                    if (str == null)
                    {
                        str = preferredPrefix;
                        int num = 0;
                        while (this.namespaceToPrefixMap.ContainsValue(str))
                        {
                            num++;
                            str = preferredPrefix + num.ToString(TypeConverterHelper.InvariantEnglishUS);
                        }
                        if (str != string.Empty)
                        {
                            XmlConvert.VerifyNCName(str);
                        }
                    }
                    this.namespaceToPrefixMap.Add(ns, str);
                    this.prefixToNamespaceMap.Add(str, ns);
                }
                return str;
            }

            public string GetName(object objectToName)
            {
                string str = null;
                XamlMember aliasedProperty = this.GetXamlType(objectToName.GetType()).GetAliasedProperty(XamlLanguage.Name);
                if (aliasedProperty != null)
                {
                    str = this.Runtime.GetValue(objectToName, aliasedProperty) as string;
                }
                if (str != null)
                {
                    return str;
                }
                XamlObjectReader.ObjectMarkupInfo info = this.ReferenceTable.Find(objectToName);
                if (info != null)
                {
                    info.AssignName(this);
                    return info.Name;
                }
                string name = null;
                name = this.ReferenceTable.FindInServiceProviderTable(objectToName);
                if (name == null)
                {
                    name = this.AllocateIdentifier();
                    this.ReferenceTable.AddToServiceProviderTable(objectToName, name);
                }
                return name;
            }

            public List<XamlNode> GetSortedNamespaceNodes()
            {
                List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
                foreach (KeyValuePair<string, string> pair in this.namespaceToPrefixMap)
                {
                    list.Add(pair);
                }
                list.Sort(new Comparison<KeyValuePair<string, string>>(XamlObjectReader.SerializerContext.CompareByValue));
                return list.ConvertAll<XamlNode>(pair => new XamlNode(XamlNodeType.NamespaceDeclaration, new System.Xaml.NamespaceDeclaration(pair.Key, pair.Value)));
            }

            public XamlType GetXamlType(Type clrType)
            {
                XamlType xamlType = this.schemaContext.GetXamlType(clrType);
                if (xamlType == null)
                {
                    throw new XamlObjectReaderException(System.Xaml.SR.Get("ObjectReaderTypeNotAllowed", new object[] { this.schemaContext.GetType(), clrType }));
                }
                return xamlType;
            }

            public bool IsPropertyReadVisible(XamlMember property)
            {
                Type accessingType = null;
                if (this.Settings.AllowProtectedMembersOnRoot && this.IsRoot)
                {
                    accessingType = this.RootType;
                }
                return property.IsReadVisibleTo(this.LocalAssembly, accessingType);
            }

            public bool IsPropertyWriteVisible(XamlMember property)
            {
                Type accessingType = null;
                if (this.Settings.AllowProtectedMembersOnRoot && this.IsRoot)
                {
                    accessingType = this.RootType;
                }
                return property.IsWriteVisibleTo(this.LocalAssembly, accessingType);
            }

            public XamlType LocalAssemblyAwareGetXamlType(Type clrType)
            {
                XamlType xamlType = this.GetXamlType(clrType);
                if (!xamlType.IsVisibleTo(this.LocalAssembly) && !typeof(Type).IsAssignableFrom(clrType))
                {
                    throw new XamlObjectReaderException(System.Xaml.SR.Get("ObjectReader_TypeNotVisible", new object[] { clrType.FullName }));
                }
                return xamlType;
            }

            public void ReserveDefaultPrefixForRootObject(object obj)
            {
                string preferredXamlNamespace = this.GetXamlType(obj.GetType()).PreferredXamlNamespace;
                if (preferredXamlNamespace != "http://schemas.microsoft.com/winfx/2006/xaml")
                {
                    this.namespaceToPrefixMap.Add(preferredXamlNamespace, string.Empty);
                    this.prefixToNamespaceMap.Add(string.Empty, preferredXamlNamespace);
                }
            }

            public bool TryConvertToMarkupExtension(TypeConverter converter, ref object value)
            {
                if (value == null)
                {
                    return false;
                }
                if (!this.Runtime.CanConvertTo(this.TypeDescriptorContext, converter, typeof(MarkupExtension)))
                {
                    return false;
                }
                value = this.ConvertTo<MarkupExtension>(converter, value);
                return true;
            }

            public bool TryHoistNamespaceDeclaration(System.Xaml.NamespaceDeclaration namespaceDeclaration)
            {
                string str;
                if (this.prefixToNamespaceMap.TryGetValue(namespaceDeclaration.Prefix, out str))
                {
                    return (str == namespaceDeclaration.Namespace);
                }
                this.namespaceToPrefixMap.Add(namespaceDeclaration.Namespace, namespaceDeclaration.Prefix);
                this.prefixToNamespaceMap.Add(namespaceDeclaration.Prefix, namespaceDeclaration.Namespace);
                return true;
            }

            public bool TryTypeConvertToString(TypeConverter converter, ref object value)
            {
                if (value == null)
                {
                    return false;
                }
                if (!(value is string))
                {
                    if (!this.CanRoundTripString(converter))
                    {
                        return false;
                    }
                    value = this.ConvertTo<string>(converter, value);
                }
                return true;
            }

            public bool TryValueSerializeToString(ValueSerializer valueSerializer, TypeConverter propertyConverter, XamlObjectReader.SerializerContext context, ref object value)
            {
                if (value == null)
                {
                    return false;
                }
                if (!(value is string))
                {
                    TypeConverter converterInstance = XamlObjectReader.TypeConverterExtensions.GetConverterInstance<TypeConverter>(context.GetXamlType(value.GetType()).TypeConverter);
                    if (!this.CanRoundtripUsingValueSerializer(valueSerializer, propertyConverter, value) && !this.CanRoundtripUsingValueSerializer(valueSerializer, converterInstance, value))
                    {
                        return false;
                    }
                    value = this.Runtime.ConvertToString(this.ValueSerializerContext, valueSerializer, value);
                }
                return true;
            }

            public object Instance { get; set; }

            public bool IsRoot { get; set; }

            public Assembly LocalAssembly
            {
                get
                {
                    return this.Settings.LocalAssembly;
                }
            }

            public Queue<XamlObjectReader.NameScopeMarkupInfo> PendingNameScopes
            {
                get
                {
                    return this.pendingNameScopes;
                }
            }

            public System.Xaml.XamlObjectReader.ReferenceTable ReferenceTable { get; set; }

            public Type RootType { get; set; }

            public ClrObjectRuntime Runtime
            {
                get
                {
                    return this.runtime;
                }
            }

            public XamlSchemaContext SchemaContext
            {
                get
                {
                    return this.schemaContext;
                }
            }

            public XamlObjectReaderSettings Settings
            {
                get
                {
                    return this.settings;
                }
            }

            public ITypeDescriptorContext TypeDescriptorContext
            {
                get
                {
                    return this.typeDescriptorContext;
                }
            }

            public IValueSerializerContext ValueSerializerContext
            {
                get
                {
                    return this.valueSerializerContext;
                }
            }
        }

        internal static class TypeConverterExtensions
        {
            public static TConverter GetConverterInstance<TConverter>(XamlValueConverter<TConverter> converter) where TConverter: class
            {
                if (converter != null)
                {
                    return converter.ConverterInstance;
                }
                return default(TConverter);
            }
        }

        private class TypeDescriptorAndValueSerializerContext : IValueSerializerContext, ITypeDescriptorContext, IServiceProvider, INamespacePrefixLookup, IXamlSchemaContextProvider, IXamlNameProvider
        {
            private XamlObjectReader.SerializerContext context;

            public TypeDescriptorAndValueSerializerContext(XamlObjectReader.SerializerContext context)
            {
                this.context = context;
            }

            public string GetName(object value)
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                return this.context.GetName(value);
            }

            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(IValueSerializerContext))
                {
                    return this;
                }
                if (serviceType == typeof(ITypeDescriptorContext))
                {
                    return this;
                }
                if (serviceType == typeof(INamespacePrefixLookup))
                {
                    return this;
                }
                if (serviceType == typeof(IXamlSchemaContextProvider))
                {
                    return this;
                }
                if (serviceType == typeof(IXamlNameProvider))
                {
                    return this;
                }
                return null;
            }

            public ValueSerializer GetValueSerializerFor(System.ComponentModel.PropertyDescriptor propertyDescriptor)
            {
                return ValueSerializer.GetSerializerFor(propertyDescriptor);
            }

            public ValueSerializer GetValueSerializerFor(Type type)
            {
                return ValueSerializer.GetSerializerFor(type);
            }

            public string LookupPrefix(string ns)
            {
                return this.context.FindPrefix(ns);
            }

            public void OnComponentChanged()
            {
            }

            public bool OnComponentChanging()
            {
                return false;
            }

            public IContainer Container
            {
                get
                {
                    return null;
                }
            }

            public object Instance
            {
                get
                {
                    return this.context.Instance;
                }
            }

            public System.ComponentModel.PropertyDescriptor PropertyDescriptor
            {
                get
                {
                    return null;
                }
            }

            public XamlSchemaContext SchemaContext
            {
                get
                {
                    return this.context.SchemaContext;
                }
            }
        }

        private class ValueMarkupInfo : XamlObjectReader.ObjectOrValueMarkupInfo
        {
        }

        private static class XamlMemberExtensions
        {
            private static XamlMember GetExcludedReadOnlyMember(XamlType type, string name)
            {
                foreach (XamlMember member in type.GetAllExcludedReadOnlyMembers())
                {
                    if (member.Name == name)
                    {
                        return member;
                    }
                }
                return null;
            }

            internal static XamlMember GetNearestMember(XamlMember member, GetNearestBaseMemberCriterion criterion)
            {
                if ((!member.IsAttachable && !member.IsDirective) && !MeetsCriterion(member, criterion))
                {
                    XamlMember excludedReadOnlyMember;
                    MethodInfo info = member.Getter ?? member.Setter;
                    if ((info == null) || !info.IsVirtual)
                    {
                        return member;
                    }
                    Type declaringType = info.GetBaseDefinition().DeclaringType;
                    if (member.DeclaringType.UnderlyingType == declaringType)
                    {
                        return member;
                    }
                    for (XamlType type2 = member.DeclaringType.BaseType; (type2 != null) && (type2 != XamlLanguage.Object); type2 = excludedReadOnlyMember.DeclaringType.BaseType)
                    {
                        excludedReadOnlyMember = type2.GetMember(member.Name);
                        if (excludedReadOnlyMember == null)
                        {
                            excludedReadOnlyMember = GetExcludedReadOnlyMember(type2, member.Name);
                            if (excludedReadOnlyMember == null)
                            {
                                return member;
                            }
                        }
                        if (MeetsCriterion(excludedReadOnlyMember, criterion))
                        {
                            return excludedReadOnlyMember;
                        }
                        if (type2.UnderlyingType == declaringType)
                        {
                            return member;
                        }
                    }
                }
                return member;
            }

            private static bool MeetsCriterion(XamlMember member, GetNearestBaseMemberCriterion criterion)
            {
                switch (criterion)
                {
                    case GetNearestBaseMemberCriterion.HasSerializationVisibility:
                        return member.HasSerializationVisibility;

                    case GetNearestBaseMemberCriterion.HasDefaultValue:
                        return member.HasDefaultValue;

                    case GetNearestBaseMemberCriterion.HasConstructorArgument:
                        return (member.ConstructorArgument != null);
                }
                return false;
            }

            internal enum GetNearestBaseMemberCriterion
            {
                HasSerializationVisibility,
                HasDefaultValue,
                HasConstructorArgument
            }
        }

        private class XamlTemplateMarkupInfo : XamlObjectReader.ObjectMarkupInfo
        {
            private List<XamlObjectReader.MarkupInfo> nodes = new List<XamlObjectReader.MarkupInfo>();
            private int objectPosition;

            public XamlTemplateMarkupInfo(XamlReader reader, XamlObjectReader.SerializerContext context)
            {
                while (reader.Read() && (reader.NodeType != XamlNodeType.StartObject))
                {
                    if (reader.NodeType != XamlNodeType.NamespaceDeclaration)
                    {
                        throw new XamlObjectReaderException(System.Xaml.SR.Get("XamlFactoryInvalidXamlNode", new object[] { reader.NodeType }));
                    }
                    if (!context.TryHoistNamespaceDeclaration(reader.Namespace))
                    {
                        XamlObjectReader.ValueMarkupInfo info = new XamlObjectReader.ValueMarkupInfo {
                            XamlNode = new XamlNode(XamlNodeType.NamespaceDeclaration, reader.Namespace)
                        };
                        this.nodes.Add(info);
                    }
                }
                if (reader.NodeType != XamlNodeType.StartObject)
                {
                    throw new XamlObjectReaderException(System.Xaml.SR.Get("XamlFactoryInvalidXamlNode", new object[] { reader.NodeType }));
                }
                XamlObjectReader.ValueMarkupInfo item = new XamlObjectReader.ValueMarkupInfo {
                    XamlNode = new XamlNode(XamlNodeType.StartObject, reader.Type)
                };
                this.nodes.Add(item);
                this.objectPosition = this.nodes.Count;
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XamlNodeType.StartObject:
                        {
                            XamlObjectReader.ValueMarkupInfo info3 = new XamlObjectReader.ValueMarkupInfo {
                                XamlNode = new XamlNode(XamlNodeType.StartObject, reader.Type)
                            };
                            this.nodes.Add(info3);
                            continue;
                        }
                        case XamlNodeType.GetObject:
                        {
                            XamlObjectReader.ValueMarkupInfo info4 = new XamlObjectReader.ValueMarkupInfo {
                                XamlNode = new XamlNode(XamlNodeType.GetObject)
                            };
                            this.nodes.Add(info4);
                            continue;
                        }
                        case XamlNodeType.EndObject:
                        {
                            XamlObjectReader.ValueMarkupInfo info5 = new XamlObjectReader.ValueMarkupInfo {
                                XamlNode = new XamlNode(XamlNodeType.EndObject)
                            };
                            this.nodes.Add(info5);
                            continue;
                        }
                        case XamlNodeType.StartMember:
                        {
                            XamlObjectReader.ValueMarkupInfo info6 = new XamlObjectReader.ValueMarkupInfo {
                                XamlNode = new XamlNode(XamlNodeType.StartMember, reader.Member)
                            };
                            this.nodes.Add(info6);
                            continue;
                        }
                        case XamlNodeType.EndMember:
                        {
                            XamlObjectReader.ValueMarkupInfo info7 = new XamlObjectReader.ValueMarkupInfo {
                                XamlNode = new XamlNode(XamlNodeType.EndMember)
                            };
                            this.nodes.Add(info7);
                            continue;
                        }
                        case XamlNodeType.Value:
                        {
                            XamlObjectReader.ValueMarkupInfo info8 = new XamlObjectReader.ValueMarkupInfo {
                                XamlNode = new XamlNode(XamlNodeType.Value, reader.Value)
                            };
                            this.nodes.Add(info8);
                            continue;
                        }
                        case XamlNodeType.NamespaceDeclaration:
                        {
                            XamlObjectReader.ValueMarkupInfo info2 = new XamlObjectReader.ValueMarkupInfo {
                                XamlNode = new XamlNode(XamlNodeType.NamespaceDeclaration, reader.Namespace)
                            };
                            this.nodes.Add(info2);
                            continue;
                        }
                    }
                    throw new InvalidOperationException(System.Xaml.SR.Get("XamlFactoryInvalidXamlNode", new object[] { reader.NodeType }));
                }
                base.XamlNode = ((XamlObjectReader.ValueMarkupInfo) this.nodes[0]).XamlNode;
                this.nodes.RemoveAt(0);
            }

            public override List<XamlObjectReader.MarkupInfo> Decompose()
            {
                foreach (XamlObjectReader.MarkupInfo info in base.Properties)
                {
                    this.nodes.Insert(this.objectPosition, info);
                }
                return this.nodes;
            }

            public override void FindNamespace(XamlObjectReader.SerializerContext context)
            {
                foreach (XamlObjectReader.MarkupInfo info in base.Properties)
                {
                    info.FindNamespace(context);
                }
            }
        }
    }
}

