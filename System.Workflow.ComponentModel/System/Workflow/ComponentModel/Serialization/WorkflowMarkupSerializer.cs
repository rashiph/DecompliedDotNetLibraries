namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Workflow;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;
    using System.Xml;

    [DefaultSerializationProvider(typeof(WorkflowMarkupSerializationProvider))]
    public class WorkflowMarkupSerializer
    {
        public static readonly DependencyProperty ClrNamespacesProperty = DependencyProperty.RegisterAttached("ClrNamespaces", typeof(List<string>), typeof(WorkflowMarkupSerializer), new PropertyMetadata(null, new Attribute[] { new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden) }));
        public static readonly DependencyProperty EventsProperty = DependencyProperty.RegisterAttached("Events", typeof(Hashtable), typeof(WorkflowMarkupSerializer), new PropertyMetadata(null, new Attribute[] { new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden) }));
        public static readonly DependencyProperty XClassProperty = DependencyProperty.RegisterAttached("XClass", typeof(string), typeof(WorkflowMarkupSerializer), new PropertyMetadata(DependencyPropertyOptions.Metadata, new Attribute[] { new BrowsableAttribute(false), new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden) }));
        public static readonly DependencyProperty XCodeProperty = DependencyProperty.RegisterAttached("XCode", typeof(CodeTypeMemberCollection), typeof(WorkflowMarkupSerializer), new PropertyMetadata(DependencyPropertyOptions.Metadata, new Attribute[] { new BrowsableAttribute(false), new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden) }));

        protected internal virtual void AddChild(WorkflowMarkupSerializationManager serializationManager, object parentObject, object childObj)
        {
            if (serializationManager == null)
            {
                throw new ArgumentNullException("serializationManager");
            }
            if (parentObject == null)
            {
                throw new ArgumentNullException("parentObject");
            }
            if (childObj == null)
            {
                throw new ArgumentNullException("childObj");
            }
            throw new Exception(SR.GetString("Error_SerializerNoChildNotion", new object[] { parentObject.GetType().FullName }));
        }

        private void AdvanceReader(XmlReader reader)
        {
            while (((reader.NodeType != XmlNodeType.EndElement) && (reader.NodeType != XmlNodeType.Element)) && ((reader.NodeType != XmlNodeType.Text) && reader.Read()))
            {
            }
        }

        protected internal virtual bool CanSerializeToString(WorkflowMarkupSerializationManager serializationManager, object value)
        {
            if (serializationManager == null)
            {
                throw new ArgumentNullException("serializationManager");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            Type c = value.GetType();
            if (((!c.IsPrimitive && !(c == typeof(string))) && (!c.IsEnum && !typeof(Delegate).IsAssignableFrom(c))) && ((!typeof(IConvertible).IsAssignableFrom(c) && !(c == typeof(TimeSpan))) && (!(c == typeof(Guid)) && !(c == typeof(DateTime)))))
            {
                return false;
            }
            return true;
        }

        protected internal virtual void ClearChildren(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            if (serializationManager == null)
            {
                throw new ArgumentNullException("serializationManager");
            }
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
        }

        protected virtual object CreateInstance(WorkflowMarkupSerializationManager serializationManager, Type type)
        {
            if (serializationManager == null)
            {
                throw new ArgumentNullException("serializationManager");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            return Activator.CreateInstance(type);
        }

        private object CreateInstance(WorkflowMarkupSerializationManager serializationManager, XmlQualifiedName xmlQualifiedName, XmlReader reader)
        {
            object obj2 = null;
            Type type = null;
            try
            {
                type = serializationManager.GetType(xmlQualifiedName);
            }
            catch (Exception exception)
            {
                serializationManager.ReportError(CreateSerializationError(SR.GetString("Error_SerializerTypeNotResolvedWithInnerError", new object[] { GetClrFullName(serializationManager, xmlQualifiedName), exception.Message }), exception, reader));
                return null;
            }
            if ((type == null) && !xmlQualifiedName.Name.EndsWith("Extension", StringComparison.Ordinal))
            {
                string name = xmlQualifiedName.Name + "Extension";
                try
                {
                    type = serializationManager.GetType(new XmlQualifiedName(name, xmlQualifiedName.Namespace));
                }
                catch (Exception exception2)
                {
                    serializationManager.ReportError(CreateSerializationError(SR.GetString("Error_SerializerTypeNotResolvedWithInnerError", new object[] { GetClrFullName(serializationManager, xmlQualifiedName), exception2.Message }), exception2, reader));
                    return null;
                }
            }
            if (type == null)
            {
                serializationManager.ReportError(CreateSerializationError(SR.GetString("Error_SerializerTypeNotResolved", new object[] { GetClrFullName(serializationManager, xmlQualifiedName) }), reader));
                return null;
            }
            if (((type.IsPrimitive || (type == typeof(string))) || ((type == typeof(decimal)) || (type == typeof(DateTime)))) || (((type == typeof(TimeSpan)) || type.IsEnum) || (type == typeof(Guid))))
            {
                try
                {
                    string s = reader.ReadString();
                    if (type == typeof(DateTime))
                    {
                        return DateTime.Parse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                    }
                    if ((!type.IsPrimitive && !(type == typeof(decimal))) && ((!(type == typeof(TimeSpan)) && !type.IsEnum) && !(type == typeof(Guid))))
                    {
                        return s;
                    }
                    TypeConverter converter = TypeDescriptor.GetConverter(type);
                    if ((converter != null) && converter.CanConvertFrom(typeof(string)))
                    {
                        return converter.ConvertFrom(null, CultureInfo.InvariantCulture, s);
                    }
                    if (typeof(IConvertible).IsAssignableFrom(type))
                    {
                        obj2 = Convert.ChangeType(s, type, CultureInfo.InvariantCulture);
                    }
                    return obj2;
                }
                catch (Exception exception3)
                {
                    serializationManager.ReportError(CreateSerializationError(SR.GetString("Error_SerializerCreateInstanceFailed", new object[] { exception3.Message }), reader));
                    return null;
                }
            }
            WorkflowMarkupSerializer serializer = serializationManager.GetSerializer(type, typeof(WorkflowMarkupSerializer)) as WorkflowMarkupSerializer;
            if (serializer == null)
            {
                serializationManager.ReportError(CreateSerializationError(SR.GetString("Error_SerializerNotAvailable", new object[] { type.FullName }), reader));
                return null;
            }
            try
            {
                obj2 = serializer.CreateInstance(serializationManager, type);
            }
            catch (Exception exception4)
            {
                serializationManager.ReportError(CreateSerializationError(SR.GetString("Error_SerializerCreateInstanceFailed", new object[] { type.FullName, exception4.Message }), reader));
                return null;
            }
            return obj2;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal static WorkflowMarkupSerializationException CreateSerializationError(Exception e, XmlReader reader)
        {
            return CreateSerializationError(null, e, reader);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal static WorkflowMarkupSerializationException CreateSerializationError(string message, XmlReader reader)
        {
            return CreateSerializationError(message, null, reader);
        }

        internal static WorkflowMarkupSerializationException CreateSerializationError(string message, Exception e, XmlReader reader)
        {
            string str = message;
            if (string.IsNullOrEmpty(str))
            {
                str = e.Message;
            }
            IXmlLineInfo info = reader as IXmlLineInfo;
            if (info != null)
            {
                return new WorkflowMarkupSerializationException(str, info.LineNumber, info.LinePosition);
            }
            return new WorkflowMarkupSerializationException(str, 0, 0);
        }

        public object Deserialize(XmlReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            DesignerSerializationManager serializationManager = new DesignerSerializationManager();
            using (serializationManager.CreateSession())
            {
                return this.Deserialize(serializationManager, reader);
            }
        }

        public object Deserialize(IDesignerSerializationManager serializationManager, XmlReader reader)
        {
            if (serializationManager == null)
            {
                throw new ArgumentNullException("serializationManager");
            }
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            WorkflowMarkupSerializationManager markupSerializationManager = serializationManager as WorkflowMarkupSerializationManager;
            if (markupSerializationManager == null)
            {
                markupSerializationManager = new WorkflowMarkupSerializationManager(serializationManager);
            }
            string fileName = markupSerializationManager.Context[typeof(string)] as string;
            if (fileName == null)
            {
                fileName = string.Empty;
            }
            markupSerializationManager.FoundDefTag += delegate (object sender, WorkflowMarkupElementEventArgs eventArgs) {
                if (eventArgs.XmlReader.LookupNamespace(eventArgs.XmlReader.Prefix) == "http://schemas.microsoft.com/winfx/2006/xaml")
                {
                    WorkflowMarkupSerializationHelpers.ProcessDefTag(markupSerializationManager, eventArgs.XmlReader, markupSerializationManager.Context.Current as Activity, false, fileName);
                }
            };
            object obj2 = this.DeserializeXoml(markupSerializationManager, reader);
            Activity activity = obj2 as Activity;
            if (activity != null)
            {
                List<string> list = activity.GetValue(ClrNamespacesProperty) as List<string>;
                if (list == null)
                {
                    list = new List<string>();
                    activity.SetValue(ClrNamespacesProperty, list);
                }
                foreach (WorkflowMarkupSerializerMapping mapping in markupSerializationManager.ClrNamespaceBasedMappings.Values)
                {
                    list.Add(mapping.ClrNamespace);
                }
                activity.SetValue(ActivityCodeDomSerializer.MarkupFileNameProperty, fileName);
                if (!string.IsNullOrEmpty(activity.Name) && !(activity.Name == activity.GetType().Name))
                {
                    return obj2;
                }
                if (string.IsNullOrEmpty(activity.GetValue(XClassProperty) as string))
                {
                    return obj2;
                }
                string str = activity.GetValue(XClassProperty) as string;
                if (str.Contains("."))
                {
                    activity.Name = str.Substring(str.LastIndexOf('.') + 1);
                    return obj2;
                }
                activity.Name = str;
            }
            return obj2;
        }

        private void DeserializeCompoundProperty(WorkflowMarkupSerializationManager serializationManager, XmlReader reader, object obj)
        {
            string localName = reader.LocalName;
            bool flag = false;
            DependencyProperty current = serializationManager.Context.Current as DependencyProperty;
            PropertyInfo info = serializationManager.Context.Current as PropertyInfo;
            if (current != null)
            {
                flag = ((byte) (current.DefaultMetadata.Options & DependencyPropertyOptions.ReadOnly)) == 2;
            }
            else if (info != null)
            {
                flag = !info.CanWrite;
            }
            else
            {
                return;
            }
            if (flag)
            {
                object binding = null;
                if ((current != null) && (obj is DependencyObject))
                {
                    if (((DependencyObject) obj).IsBindingSet(current))
                    {
                        binding = ((DependencyObject) obj).GetBinding(current);
                    }
                    else if (!current.IsEvent)
                    {
                        binding = ((DependencyObject) obj).GetValue(current);
                    }
                    else
                    {
                        binding = ((DependencyObject) obj).GetHandler(current);
                    }
                }
                else if (info != null)
                {
                    binding = info.CanRead ? info.GetValue(obj, null) : null;
                }
                if (binding != null)
                {
                    this.DeserializeContents(serializationManager, binding, reader);
                }
                else
                {
                    serializationManager.ReportError(CreateSerializationError(SR.GetString("Error_SerializerReadOnlyPropertyAndValueIsNull", new object[] { localName, obj.GetType().FullName }), reader));
                }
            }
            else if (!reader.IsEmptyElement)
            {
                if (reader.HasAttributes)
                {
                    while (reader.MoveToNextAttribute())
                    {
                        if (!string.Equals(reader.LocalName, "xmlns", StringComparison.Ordinal) && !string.Equals(reader.Prefix, "xmlns", StringComparison.Ordinal))
                        {
                            serializationManager.ReportError(CreateSerializationError(SR.GetString("Error_SerializerAttributesFoundInComplexProperty", new object[] { localName, obj.GetType().FullName }), reader));
                        }
                    }
                }
                do
                {
                    if (!reader.Read())
                    {
                        return;
                    }
                }
                while (((reader.NodeType != XmlNodeType.Text) && (reader.NodeType != XmlNodeType.Element)) && ((reader.NodeType != XmlNodeType.ProcessingInstruction) && (reader.NodeType != XmlNodeType.EndElement)));
                if (reader.NodeType == XmlNodeType.Text)
                {
                    this.DeserializeSimpleProperty(serializationManager, reader, obj, reader.Value);
                }
                else
                {
                    this.AdvanceReader(reader);
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        object extension = this.DeserializeObject(serializationManager, reader);
                        if (extension != null)
                        {
                            extension = GetValueFromMarkupExtension(serializationManager, extension);
                            if (((extension != null) && (extension.GetType() == typeof(string))) && ((string) extension).StartsWith("{}", StringComparison.Ordinal))
                            {
                                extension = ((string) extension).Substring(2);
                            }
                            if (current != null)
                            {
                                WorkflowMarkupSerializer serializer = serializationManager.GetSerializer(obj.GetType(), typeof(WorkflowMarkupSerializer)) as WorkflowMarkupSerializer;
                                if (serializer == null)
                                {
                                    serializationManager.ReportError(CreateSerializationError(SR.GetString("Error_SerializerNotAvailable", new object[] { obj.GetType().FullName }), reader));
                                }
                                else
                                {
                                    try
                                    {
                                        serializer.SetDependencyPropertyValue(serializationManager, obj, current, extension);
                                    }
                                    catch (Exception exception)
                                    {
                                        serializationManager.ReportError(CreateSerializationError(SR.GetString("Error_SerializerThrewException", new object[] { obj.GetType().FullName, exception.Message }), exception, reader));
                                    }
                                }
                            }
                            else if (info != null)
                            {
                                try
                                {
                                    info.SetValue(obj, extension, null);
                                }
                                catch
                                {
                                    serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString("Error_SerializerComplexPropertySetFailed", new object[] { localName, localName, obj.GetType().Name })));
                                }
                            }
                        }
                    }
                }
            }
        }

        private void DeserializeContents(WorkflowMarkupSerializationManager serializationManager, object obj, XmlReader reader)
        {
            if (serializationManager == null)
            {
                throw new ArgumentNullException("serializationManager");
            }
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            if (reader.NodeType == XmlNodeType.Element)
            {
                WorkflowMarkupSerializer parentObjectSerializer = serializationManager.GetSerializer(obj.GetType(), typeof(WorkflowMarkupSerializer)) as WorkflowMarkupSerializer;
                if (parentObjectSerializer == null)
                {
                    serializationManager.ReportError(CreateSerializationError(SR.GetString("Error_SerializerNotAvailable", new object[] { obj.GetType().FullName }), reader));
                }
                else
                {
                    try
                    {
                        parentObjectSerializer.OnBeforeDeserialize(serializationManager, obj);
                    }
                    catch (Exception exception)
                    {
                        serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString("Error_SerializerThrewException", new object[] { obj.GetType().FullName, exception.Message }), exception));
                        return;
                    }
                    bool isEmptyElement = reader.IsEmptyElement;
                    string namespaceURI = reader.NamespaceURI;
                    List<PropertyInfo> propInfos = new List<PropertyInfo>();
                    List<EventInfo> events = new List<EventInfo>();
                    if (((obj.GetType().IsPrimitive || (obj.GetType() == typeof(string))) || ((obj.GetType() == typeof(decimal)) || obj.GetType().IsEnum)) || (((obj.GetType() == typeof(DateTime)) || (obj.GetType() == typeof(TimeSpan))) || (obj.GetType() == typeof(Guid))))
                    {
                        propInfos.AddRange(serializationManager.GetExtendedProperties(obj));
                    }
                    else
                    {
                        try
                        {
                            propInfos.AddRange(parentObjectSerializer.GetProperties(serializationManager, obj));
                            propInfos.AddRange(serializationManager.GetExtendedProperties(obj));
                            events.AddRange(parentObjectSerializer.GetEvents(serializationManager, obj));
                        }
                        catch (Exception exception2)
                        {
                            serializationManager.ReportError(CreateSerializationError(SR.GetString("Error_SerializerThrewException", new object[] { obj.GetType(), exception2.Message }), exception2, reader));
                            return;
                        }
                    }
                    if (reader.HasAttributes)
                    {
                        while (reader.MoveToNextAttribute())
                        {
                            if (!reader.LocalName.Equals("xmlns", StringComparison.Ordinal) && !reader.Prefix.Equals("xmlns", StringComparison.Ordinal))
                            {
                                XmlQualifiedName xmlQualifiedName = new XmlQualifiedName(reader.LocalName, reader.LookupNamespace(reader.Prefix));
                                if ((xmlQualifiedName.Namespace.Equals("http://schemas.microsoft.com/winfx/2006/xaml", StringComparison.Ordinal) && !IsMarkupExtension(xmlQualifiedName)) && (!ExtendedPropertyInfo.IsExtendedProperty(serializationManager, propInfos, xmlQualifiedName) && !ExtendedPropertyInfo.IsExtendedProperty(serializationManager, xmlQualifiedName)))
                                {
                                    serializationManager.FireFoundDefTag(new WorkflowMarkupElementEventArgs(reader));
                                }
                                else
                                {
                                    string fullPropertyName = XmlConvert.DecodeName(reader.LocalName);
                                    string str2 = reader.Value;
                                    DependencyProperty context = this.ResolveDependencyProperty(serializationManager, reader, obj, fullPropertyName);
                                    if (context != null)
                                    {
                                        serializationManager.Context.Push(context);
                                        try
                                        {
                                            if (context.IsEvent)
                                            {
                                                this.DeserializeEvent(serializationManager, reader, obj, str2);
                                            }
                                            else
                                            {
                                                this.DeserializeSimpleProperty(serializationManager, reader, obj, str2);
                                            }
                                            continue;
                                        }
                                        finally
                                        {
                                            serializationManager.Context.Pop();
                                        }
                                    }
                                    PropertyInfo property = LookupProperty(propInfos, fullPropertyName);
                                    if (property != null)
                                    {
                                        serializationManager.Context.Push(property);
                                        try
                                        {
                                            this.DeserializeSimpleProperty(serializationManager, reader, obj, str2);
                                            continue;
                                        }
                                        finally
                                        {
                                            serializationManager.Context.Pop();
                                        }
                                    }
                                    EventInfo info2 = LookupEvent(events, fullPropertyName);
                                    if ((events != null) && (info2 != null))
                                    {
                                        serializationManager.Context.Push(info2);
                                        try
                                        {
                                            this.DeserializeEvent(serializationManager, reader, obj, str2);
                                            continue;
                                        }
                                        finally
                                        {
                                            serializationManager.Context.Pop();
                                        }
                                    }
                                    serializationManager.ReportError(CreateSerializationError(SR.GetString("Error_SerializerNoMemberFound", new object[] { fullPropertyName, obj.GetType().FullName }), reader));
                                }
                            }
                        }
                    }
                    try
                    {
                        parentObjectSerializer.OnBeforeDeserializeContents(serializationManager, obj);
                    }
                    catch (Exception exception3)
                    {
                        serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString("Error_SerializerThrewException", new object[] { obj.GetType().FullName, exception3.Message }), exception3));
                        return;
                    }
                    try
                    {
                        parentObjectSerializer.ClearChildren(serializationManager, obj);
                    }
                    catch (Exception exception4)
                    {
                        serializationManager.ReportError(CreateSerializationError(SR.GetString("Error_SerializerThrewException", new object[] { obj.GetType(), exception4.Message }), exception4, reader));
                        return;
                    }
                    using (ContentProperty property2 = new ContentProperty(serializationManager, parentObjectSerializer, obj))
                    {
                        List<ContentInfo> contents = new List<ContentInfo>();
                        if (isEmptyElement)
                        {
                            goto Label_07C1;
                        }
                        reader.MoveToElement();
                        int depth = reader.Depth;
                        XmlQualifiedName name2 = new XmlQualifiedName(reader.LocalName, reader.LookupNamespace(reader.Prefix));
                    Label_047F:
                        if ((name2 != null) && !ExtendedPropertyInfo.IsExtendedProperty(serializationManager, name2))
                        {
                            name2 = null;
                        }
                        else
                        {
                            if ((depth + 1) < reader.Depth)
                            {
                                bool flag2 = false;
                                while (reader.Read() && ((depth + 1) < reader.Depth))
                                {
                                    if (((reader.NodeType != XmlNodeType.Comment) && (reader.NodeType != XmlNodeType.Whitespace)) && !flag2)
                                    {
                                        serializationManager.ReportError(CreateSerializationError(SR.GetString("Error_InvalidDataFoundForType", new object[] { obj.GetType().FullName }), reader));
                                        flag2 = true;
                                    }
                                }
                            }
                            this.AdvanceReader(reader);
                            if (reader.NodeType == XmlNodeType.Element)
                            {
                                XmlQualifiedName name3 = new XmlQualifiedName(reader.LocalName, reader.LookupNamespace(reader.Prefix));
                                if ((reader.LocalName.IndexOf('.') > 0) || ExtendedPropertyInfo.IsExtendedProperty(serializationManager, name3))
                                {
                                    string propertyName = reader.LocalName.Substring(reader.LocalName.IndexOf('.') + 1);
                                    PropertyInfo info3 = LookupProperty(propInfos, propertyName);
                                    DependencyProperty property3 = this.ResolveDependencyProperty(serializationManager, reader, obj, reader.LocalName);
                                    if ((property3 == null) && (info3 == null))
                                    {
                                        serializationManager.ReportError(CreateSerializationError(SR.GetString("Error_InvalidElementFoundForType", new object[] { reader.LocalName, obj.GetType().FullName }), reader));
                                        goto Label_07AC;
                                    }
                                    if (property3 != null)
                                    {
                                        LookupProperty(propInfos, property3.Name);
                                        serializationManager.Context.Push(property3);
                                        try
                                        {
                                            this.DeserializeCompoundProperty(serializationManager, reader, obj);
                                            goto Label_07AC;
                                        }
                                        finally
                                        {
                                            serializationManager.Context.Pop();
                                        }
                                    }
                                    if (info3 == null)
                                    {
                                        goto Label_07AC;
                                    }
                                    serializationManager.Context.Push(info3);
                                    try
                                    {
                                        this.DeserializeCompoundProperty(serializationManager, reader, obj);
                                        goto Label_07AC;
                                    }
                                    finally
                                    {
                                        serializationManager.Context.Pop();
                                    }
                                }
                                int lineNumber = (reader is IXmlLineInfo) ? ((IXmlLineInfo) reader).LineNumber : 1;
                                int linePosition = (reader is IXmlLineInfo) ? ((IXmlLineInfo) reader).LinePosition : 1;
                                object extension = this.DeserializeObject(serializationManager, reader);
                                if (extension != null)
                                {
                                    extension = GetValueFromMarkupExtension(serializationManager, extension);
                                    if (((extension != null) && (extension.GetType() == typeof(string))) && ((string) extension).StartsWith("{}", StringComparison.Ordinal))
                                    {
                                        extension = ((string) extension).Substring(2);
                                    }
                                    contents.Add(new ContentInfo(extension, lineNumber, linePosition));
                                }
                            }
                            else
                            {
                                if ((reader.NodeType == XmlNodeType.Text) && (property2.Property != null))
                                {
                                    int num5 = (reader is IXmlLineInfo) ? ((IXmlLineInfo) reader).LineNumber : 1;
                                    int num6 = (reader is IXmlLineInfo) ? ((IXmlLineInfo) reader).LinePosition : 1;
                                    contents.Add(new ContentInfo(reader.ReadString(), num5, num6));
                                    if (depth < reader.Depth)
                                    {
                                        goto Label_07AC;
                                    }
                                    goto Label_07C1;
                                }
                                if (((reader.NodeType == XmlNodeType.Entity) || (reader.NodeType == XmlNodeType.Text)) || (reader.NodeType == XmlNodeType.CDATA))
                                {
                                    serializationManager.ReportError(CreateSerializationError(SR.GetString("Error_InvalidDataFound", new object[] { reader.Value.Trim(), obj.GetType().FullName }), reader));
                                }
                            }
                        }
                    Label_07AC:
                        if (reader.Read() && (depth < reader.Depth))
                        {
                            goto Label_047F;
                        }
                    Label_07C1:
                        property2.SetContents(contents);
                    }
                    try
                    {
                        parentObjectSerializer.OnAfterDeserialize(serializationManager, obj);
                    }
                    catch (Exception exception5)
                    {
                        serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString("Error_SerializerThrewException", new object[] { obj.GetType().FullName, exception5.Message }), exception5));
                    }
                }
            }
        }

        private void DeserializeEvent(WorkflowMarkupSerializationManager serializationManager, XmlReader reader, object obj, string value)
        {
            Type memberType = null;
            EventInfo current = serializationManager.Context.Current as EventInfo;
            DependencyProperty property = serializationManager.Context.Current as DependencyProperty;
            if (property != null)
            {
                memberType = property.PropertyType;
            }
            else if (current != null)
            {
                memberType = current.EventHandlerType;
            }
            else
            {
                return;
            }
            this.DeserializeSimpleMember(serializationManager, memberType, reader, obj, value);
        }

        internal object DeserializeFromCompactFormat(WorkflowMarkupSerializationManager serializationManager, XmlReader reader, string attrValue)
        {
            if (((attrValue.Length == 0) || !attrValue.StartsWith("{", StringComparison.Ordinal)) || !attrValue.EndsWith("}", StringComparison.Ordinal))
            {
                serializationManager.ReportError(CreateSerializationError(SR.GetString("IncorrectSyntax", new object[] { attrValue }), reader));
                return null;
            }
            int index = attrValue.IndexOf(" ", StringComparison.Ordinal);
            if (index == -1)
            {
                index = attrValue.IndexOf("}", StringComparison.Ordinal);
            }
            string name = attrValue.Substring(1, index - 1).Trim();
            string args = attrValue.Substring(index + 1, attrValue.Length - (index + 1));
            string prefix = string.Empty;
            int length = name.IndexOf(":", StringComparison.Ordinal);
            if (length >= 0)
            {
                prefix = name.Substring(0, length);
                name = name.Substring(length + 1);
            }
            Type type = serializationManager.GetType(new XmlQualifiedName(name, reader.LookupNamespace(prefix)));
            if ((type == null) && !name.EndsWith("Extension", StringComparison.Ordinal))
            {
                name = name + "Extension";
                type = serializationManager.GetType(new XmlQualifiedName(name, reader.LookupNamespace(prefix)));
            }
            if (type == null)
            {
                serializationManager.ReportError(CreateSerializationError(SR.GetString("Error_MarkupSerializerTypeNotResolved", new object[] { name }), reader));
                return null;
            }
            object obj2 = null;
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            ArrayList list = null;
            try
            {
                list = this.TokenizeAttributes(serializationManager, args, (reader is IXmlLineInfo) ? ((IXmlLineInfo) reader).LineNumber : 1, (reader is IXmlLineInfo) ? ((IXmlLineInfo) reader).LinePosition : 1);
            }
            catch (Exception exception)
            {
                serializationManager.ReportError(CreateSerializationError(SR.GetString("Error_MarkupExtensionDeserializeFailed", new object[] { attrValue, exception.Message }), reader));
                return null;
            }
            if (list == null)
            {
                obj2 = Activator.CreateInstance(type);
            }
            else
            {
                ArrayList list2 = new ArrayList();
                bool flag = true;
                for (int i = 0; i < list.Count; i++)
                {
                    char ch = (list[i] is char) ? ((char) list[i]) : '\0';
                    if (ch == '=')
                    {
                        if ((list2.Count > 0) && flag)
                        {
                            list2.RemoveAt(list2.Count - 1);
                        }
                        flag = false;
                        dictionary.Add(list[i - 1] as string, list[i + 1] as string);
                        i++;
                    }
                    if ((ch != ',') && (dictionary.Count == 0))
                    {
                        list2.Add(list[i] as string);
                    }
                }
                if (list2.Count <= 0)
                {
                    obj2 = Activator.CreateInstance(type);
                }
                else
                {
                    ConstructorInfo info = null;
                    ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                    ParameterInfo[] infoArray2 = null;
                    foreach (ConstructorInfo info2 in constructors)
                    {
                        ParameterInfo[] parameters = info2.GetParameters();
                        if (parameters.Length == list2.Count)
                        {
                            info = info2;
                            infoArray2 = parameters;
                            break;
                        }
                    }
                    if (info != null)
                    {
                        for (int j = 0; j < list2.Count; j++)
                        {
                            list2[j] = XmlConvert.DecodeName((string) list2[j]);
                            string str4 = (string) list2[j];
                            this.RemoveEscapes(ref str4);
                            list2[j] = this.InternalDeserializeFromString(serializationManager, infoArray2[j].ParameterType, str4);
                            list2[j] = GetValueFromMarkupExtension(serializationManager, list2[j]);
                        }
                        obj2 = Activator.CreateInstance(type, list2.ToArray());
                    }
                }
            }
            if (obj2 == null)
            {
                serializationManager.ReportError(CreateSerializationError(SR.GetString("Error_CantCreateInstanceOfBaseType", new object[] { type.FullName }), reader));
                return null;
            }
            if (dictionary.Count > 0)
            {
                WorkflowMarkupSerializer serializer = serializationManager.GetSerializer(obj2.GetType(), typeof(WorkflowMarkupSerializer)) as WorkflowMarkupSerializer;
                if (serializer == null)
                {
                    serializationManager.ReportError(CreateSerializationError(SR.GetString("Error_SerializerNotAvailable", new object[] { obj2.GetType().FullName }), reader));
                    return obj2;
                }
                List<PropertyInfo> properties = new List<PropertyInfo>();
                try
                {
                    properties.AddRange(serializer.GetProperties(serializationManager, obj2));
                    properties.AddRange(serializationManager.GetExtendedProperties(obj2));
                }
                catch (Exception exception2)
                {
                    serializationManager.ReportError(CreateSerializationError(SR.GetString("Error_SerializerThrewException", new object[] { obj2.GetType().FullName, exception2.Message }), exception2, reader));
                    return obj2;
                }
                foreach (string str5 in dictionary.Keys)
                {
                    string str6 = str5;
                    string str7 = dictionary[str5] as string;
                    this.RemoveEscapes(ref str6);
                    this.RemoveEscapes(ref str7);
                    PropertyInfo property = LookupProperty(properties, str6);
                    if (property != null)
                    {
                        serializationManager.Context.Push(property);
                        try
                        {
                            this.DeserializeSimpleProperty(serializationManager, reader, obj2, str7);
                        }
                        finally
                        {
                            serializationManager.Context.Pop();
                        }
                    }
                    else
                    {
                        serializationManager.ReportError(CreateSerializationError(SR.GetString("Error_SerializerPrimitivePropertyNoLogic", new object[] { str6, str6, obj2.GetType().FullName }), reader));
                    }
                }
            }
            return obj2;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected internal virtual object DeserializeFromString(WorkflowMarkupSerializationManager serializationManager, Type propertyType, string value)
        {
            return this.InternalDeserializeFromString(serializationManager, propertyType, value);
        }

        internal object DeserializeObject(WorkflowMarkupSerializationManager serializationManager, XmlReader reader)
        {
            if (serializationManager == null)
            {
                throw new ArgumentNullException("serializationManager");
            }
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            object context = null;
            try
            {
                serializationManager.WorkflowMarkupStack.Push(reader);
                this.AdvanceReader(reader);
                if (reader.NodeType != XmlNodeType.Element)
                {
                    serializationManager.ReportError(CreateSerializationError(SR.GetString("Error_InvalidDataFound"), reader));
                    return context;
                }
                XmlQualifiedName xmlQualifiedName = new XmlQualifiedName(XmlConvert.DecodeName(reader.LocalName), reader.LookupNamespace(reader.Prefix));
                if ((xmlQualifiedName.Namespace.Equals("http://schemas.microsoft.com/winfx/2006/xaml", StringComparison.Ordinal) && !IsMarkupExtension(xmlQualifiedName)) && !ExtendedPropertyInfo.IsExtendedProperty(serializationManager, xmlQualifiedName))
                {
                    int depth = reader.Depth;
                    serializationManager.FireFoundDefTag(new WorkflowMarkupElementEventArgs(reader));
                    if ((depth + 1) < reader.Depth)
                    {
                        do
                        {
                            if (!reader.Read())
                            {
                                return context;
                            }
                        }
                        while ((depth + 1) < reader.Depth);
                    }
                    return context;
                }
                context = this.CreateInstance(serializationManager, xmlQualifiedName, reader);
                reader.MoveToElement();
                if (context == null)
                {
                    return context;
                }
                serializationManager.Context.Push(context);
                try
                {
                    this.DeserializeContents(serializationManager, context, reader);
                }
                finally
                {
                    serializationManager.Context.Pop();
                }
            }
            finally
            {
                serializationManager.WorkflowMarkupStack.Pop();
            }
            return context;
        }

        private void DeserializeSimpleMember(WorkflowMarkupSerializationManager serializationManager, Type memberType, XmlReader reader, object obj, string value)
        {
            WorkflowMarkupSerializer serializer = serializationManager.GetSerializer(memberType, typeof(WorkflowMarkupSerializer)) as WorkflowMarkupSerializer;
            if (serializer == null)
            {
                serializationManager.ReportError(CreateSerializationError(SR.GetString("Error_SerializerNotAvailable", new object[] { memberType.FullName }), reader));
            }
            else
            {
                object extension = null;
                try
                {
                    extension = serializer.DeserializeFromString(serializationManager, memberType, value);
                    extension = GetValueFromMarkupExtension(serializationManager, extension);
                    DependencyProperty current = serializationManager.Context.Current as DependencyProperty;
                    if (current != null)
                    {
                        WorkflowMarkupSerializer serializer2 = serializationManager.GetSerializer(obj.GetType(), typeof(WorkflowMarkupSerializer)) as WorkflowMarkupSerializer;
                        if (serializer2 == null)
                        {
                            serializationManager.ReportError(CreateSerializationError(SR.GetString("Error_SerializerNotAvailable", new object[] { obj.GetType().FullName }), reader));
                        }
                        else
                        {
                            serializer2.SetDependencyPropertyValue(serializationManager, obj, current, extension);
                        }
                    }
                    else
                    {
                        EventInfo info = serializationManager.Context.Current as EventInfo;
                        if (info != null)
                        {
                            try
                            {
                                WorkflowMarkupSerializationHelpers.SetEventHandlerName(obj, info.Name, extension as string);
                            }
                            catch (Exception innerException)
                            {
                                while ((innerException is TargetInvocationException) && (innerException.InnerException != null))
                                {
                                    innerException = innerException.InnerException;
                                }
                                serializationManager.ReportError(CreateSerializationError(SR.GetString("Error_SerializerMemberSetFailed", new object[] { reader.LocalName, reader.Value, reader.LocalName, obj.GetType().FullName, innerException.Message }), innerException, reader));
                            }
                        }
                        else
                        {
                            PropertyInfo info2 = serializationManager.Context.Current as PropertyInfo;
                            if (info2 != null)
                            {
                                try
                                {
                                    if ((extension is string) && TypeProvider.IsAssignable(typeof(Type), info2.PropertyType))
                                    {
                                        string key = info2.ReflectedType.FullName + "." + info2.Name;
                                        Helpers.SetDesignTimeTypeName(obj, key, extension as string);
                                    }
                                    else if (info2.CanWrite)
                                    {
                                        info2.SetValue(obj, extension, null);
                                    }
                                    else if (typeof(ICollection<string>).IsAssignableFrom(extension.GetType()))
                                    {
                                        ICollection<string> is2 = info2.GetValue(obj, null) as ICollection<string>;
                                        ICollection<string> is3 = extension as ICollection<string>;
                                        if ((is2 != null) && (is3 != null))
                                        {
                                            foreach (string str2 in is3)
                                            {
                                                is2.Add(str2);
                                            }
                                        }
                                    }
                                }
                                catch (Exception exception2)
                                {
                                    while ((exception2 is TargetInvocationException) && (exception2.InnerException != null))
                                    {
                                        exception2 = exception2.InnerException;
                                    }
                                    serializationManager.ReportError(CreateSerializationError(SR.GetString("Error_SerializerMemberSetFailed", new object[] { reader.LocalName, reader.Value, reader.LocalName, obj.GetType().FullName, exception2.Message }), exception2, reader));
                                }
                            }
                        }
                    }
                }
                catch (Exception exception3)
                {
                    while ((exception3 is TargetInvocationException) && (exception3.InnerException != null))
                    {
                        exception3 = exception3.InnerException;
                    }
                    serializationManager.ReportError(CreateSerializationError(SR.GetString("Error_SerializerMemberSetFailed", new object[] { reader.LocalName, reader.Value, reader.LocalName, obj.GetType().FullName, exception3.Message }), exception3, reader));
                }
            }
        }

        private void DeserializeSimpleProperty(WorkflowMarkupSerializationManager serializationManager, XmlReader reader, object obj, string value)
        {
            Type c = null;
            bool flag = false;
            DependencyProperty current = serializationManager.Context.Current as DependencyProperty;
            PropertyInfo info = serializationManager.Context.Current as PropertyInfo;
            if (current != null)
            {
                c = current.PropertyType;
                flag = ((byte) (current.DefaultMetadata.Options & DependencyPropertyOptions.ReadOnly)) == 2;
            }
            else if (info != null)
            {
                c = info.PropertyType;
                flag = !info.CanWrite;
            }
            else
            {
                return;
            }
            if (flag && !typeof(ICollection<string>).IsAssignableFrom(c))
            {
                serializationManager.ReportError(CreateSerializationError(SR.GetString("Error_SerializerPrimitivePropertyReadOnly", new object[] { info.Name, info.Name, obj.GetType().FullName }), reader));
            }
            else
            {
                this.DeserializeSimpleMember(serializationManager, c, reader, obj, value);
            }
        }

        private object DeserializeXoml(WorkflowMarkupSerializationManager serializationManager, XmlReader xmlReader)
        {
            if (serializationManager == null)
            {
                throw new ArgumentNullException("serializationManager");
            }
            if (xmlReader == null)
            {
                throw new ArgumentNullException("xmlReader");
            }
            object obj2 = null;
            serializationManager.WorkflowMarkupStack.Push(xmlReader);
            try
            {
                while ((xmlReader.Read() && (xmlReader.NodeType != XmlNodeType.Element)) && (xmlReader.NodeType != XmlNodeType.ProcessingInstruction))
                {
                }
                if (xmlReader.EOF)
                {
                    return null;
                }
                obj2 = this.DeserializeObject(serializationManager, xmlReader);
                while (xmlReader.Read() && !xmlReader.EOF)
                {
                }
            }
            catch (XmlException exception)
            {
                throw new WorkflowMarkupSerializationException(exception.Message, exception, exception.LineNumber, exception.LinePosition);
            }
            catch (Exception exception2)
            {
                throw CreateSerializationError(exception2, xmlReader);
            }
            finally
            {
                serializationManager.WorkflowMarkupStack.Pop();
            }
            return obj2;
        }

        internal static string EnsureMarkupExtensionTypeName(Type type)
        {
            string name = type.Name;
            if (name.EndsWith("Extension", StringComparison.OrdinalIgnoreCase))
            {
                name = name.Substring(0, name.Length - "Extension".Length);
            }
            return name;
        }

        internal static string EnsureMarkupExtensionTypeName(XmlQualifiedName xmlQualifiedName)
        {
            string name = xmlQualifiedName.Name;
            if (xmlQualifiedName.Namespace.Equals("http://schemas.microsoft.com/winfx/2006/xaml", StringComparison.Ordinal) && name.Equals(typeof(Array).Name, StringComparison.Ordinal))
            {
                name = typeof(ArrayExtension).Name;
            }
            return name;
        }

        protected internal virtual IList GetChildren(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            if (serializationManager == null)
            {
                throw new ArgumentNullException("serializationManager");
            }
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            return null;
        }

        private static string GetClrFullName(WorkflowMarkupSerializationManager serializationManager, XmlQualifiedName xmlQualifiedName)
        {
            string key = xmlQualifiedName.Namespace;
            string name = xmlQualifiedName.Name;
            List<WorkflowMarkupSerializerMapping> list = null;
            if (!serializationManager.XmlNamespaceBasedMappings.TryGetValue(key, out list) || (list.Count == 0))
            {
                return (xmlQualifiedName.Namespace + "." + xmlQualifiedName.Name);
            }
            WorkflowMarkupSerializerMapping mapping = list[0];
            string assemblyName = mapping.AssemblyName;
            string clrNamespace = mapping.ClrNamespace;
            string str3 = xmlQualifiedName.Name;
            if (clrNamespace.Length > 0)
            {
                str3 = clrNamespace + "." + xmlQualifiedName.Name;
            }
            return str3;
        }

        private IDictionary<DependencyProperty, object> GetDependencyProperties(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            if (serializationManager == null)
            {
                throw new ArgumentNullException("serializationManager");
            }
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            List<PropertyInfo> properties = new List<PropertyInfo>();
            properties.AddRange(this.GetProperties(serializationManager, obj));
            List<EventInfo> events = new List<EventInfo>();
            events.AddRange(this.GetEvents(serializationManager, obj));
            Dictionary<DependencyProperty, object> dictionary = new Dictionary<DependencyProperty, object>();
            DependencyObject obj2 = obj as DependencyObject;
            if (obj2 != null)
            {
                foreach (DependencyProperty property in obj2.MetaDependencyProperties)
                {
                    Attribute[] attributes = property.DefaultMetadata.GetAttributes(typeof(DesignerSerializationVisibilityAttribute));
                    if ((attributes.Length <= 0) || (((DesignerSerializationVisibilityAttribute) attributes[0]).Visibility != DesignerSerializationVisibility.Hidden))
                    {
                        if (((byte) (property.DefaultMetadata.Options & DependencyPropertyOptions.ReadOnly)) == 2)
                        {
                            object[] objArray = property.DefaultMetadata.GetAttributes(typeof(DesignerSerializationVisibilityAttribute));
                            if (((objArray == null) || (objArray.Length == 0)) || (((DesignerSerializationVisibilityAttribute) objArray[0]).Visibility != DesignerSerializationVisibility.Content))
                            {
                                continue;
                            }
                        }
                        object obj3 = null;
                        if (!property.IsAttached && !property.DefaultMetadata.IsMetaProperty)
                        {
                            if (property.IsEvent)
                            {
                                obj3 = LookupEvent(events, property.Name);
                            }
                            else
                            {
                                obj3 = LookupProperty(properties, property.Name);
                            }
                            if (obj3 == null)
                            {
                                serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString("Error_MissingCLRProperty", new object[] { property.Name, obj.GetType().FullName })));
                                continue;
                            }
                        }
                        if (obj2.IsBindingSet(property))
                        {
                            dictionary.Add(property, obj2.GetBinding(property));
                        }
                        else if (!property.IsEvent)
                        {
                            object obj4 = null;
                            obj4 = obj2.GetValue(property);
                            if (!property.IsAttached && !property.DefaultMetadata.IsMetaProperty)
                            {
                                PropertyInfo info = obj3 as PropertyInfo;
                                if ((obj4 != null) && info.PropertyType.IsAssignableFrom(obj4.GetType()))
                                {
                                    obj4 = (obj3 as PropertyInfo).GetValue(obj2, null);
                                }
                            }
                            dictionary.Add(property, obj4);
                        }
                        else
                        {
                            dictionary.Add(property, obj2.GetHandler(property));
                        }
                    }
                }
                foreach (DependencyProperty property2 in obj2.DependencyPropertyValues.Keys)
                {
                    Attribute[] attributeArray2 = property2.DefaultMetadata.GetAttributes(typeof(DesignerSerializationVisibilityAttribute));
                    if (((attributeArray2.Length <= 0) || (((DesignerSerializationVisibilityAttribute) attributeArray2[0]).Visibility != DesignerSerializationVisibility.Hidden)) && ((!property2.DefaultMetadata.IsMetaProperty && property2.IsAttached) && VerifyAttachedPropertyConditions(property2)))
                    {
                        dictionary.Add(property2, obj2.GetValue(property2));
                    }
                }
            }
            return dictionary;
        }

        protected internal virtual EventInfo[] GetEvents(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            if (serializationManager == null)
            {
                throw new ArgumentNullException("serializationManager");
            }
            List<EventInfo> list = new List<EventInfo>();
            foreach (EventInfo info in obj.GetType().GetEvents(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance))
            {
                if (Helpers.GetSerializationVisibility(info) != DesignerSerializationVisibility.Hidden)
                {
                    list.Add(info);
                }
            }
            return list.ToArray();
        }

        internal virtual ExtendedPropertyInfo[] GetExtendedProperties(WorkflowMarkupSerializationManager manager, object extendee)
        {
            return new ExtendedPropertyInfo[0];
        }

        private static object GetMarkupExtensionFromValue(object value)
        {
            if (value == null)
            {
                return new NullExtension();
            }
            if (value is Type)
            {
                return new TypeExtension(value as Type);
            }
            if (value is Array)
            {
                return new ArrayExtension(value as Array);
            }
            return value;
        }

        protected internal virtual PropertyInfo[] GetProperties(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            if (serializationManager == null)
            {
                throw new ArgumentNullException("serializationManager");
            }
            List<PropertyInfo> list = new List<PropertyInfo>();
            object[] customAttributes = obj.GetType().GetCustomAttributes(typeof(RuntimeNamePropertyAttribute), true);
            string name = null;
            if (customAttributes.Length > 0)
            {
                name = (customAttributes[0] as RuntimeNamePropertyAttribute).Name;
            }
            foreach (PropertyInfo info in obj.GetType().GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance))
            {
                DesignerSerializationVisibility serializationVisibility = Helpers.GetSerializationVisibility(info);
                if ((serializationVisibility != DesignerSerializationVisibility.Hidden) && (((serializationVisibility == DesignerSerializationVisibility.Content) || (info.CanWrite && (info.GetSetMethod() != null))) || ((obj is CodeObject) && typeof(ICollection).IsAssignableFrom(info.PropertyType))))
                {
                    TypeProvider service = serializationManager.GetService(typeof(ITypeProvider)) as TypeProvider;
                    if ((service == null) || service.IsSupportedProperty(info, obj))
                    {
                        if ((name == null) || !name.Equals(info.Name))
                        {
                            list.Add(info);
                        }
                        else
                        {
                            list.Add(new ExtendedPropertyInfo(info, new GetValueHandler(this.OnGetRuntimeNameValue), new SetValueHandler(this.OnSetRuntimeNameValue), new GetQualifiedNameHandler(this.OnGetRuntimeQualifiedName)));
                        }
                    }
                }
            }
            return list.ToArray();
        }

        private static object GetValueFromMarkupExtension(WorkflowMarkupSerializationManager manager, object extension)
        {
            object obj2 = extension;
            MarkupExtension extension2 = extension as MarkupExtension;
            if (extension2 != null)
            {
                obj2 = extension2.ProvideValue(manager);
            }
            return obj2;
        }

        private object InternalDeserializeFromString(WorkflowMarkupSerializationManager serializationManager, Type propertyType, string value)
        {
            if (serializationManager == null)
            {
                throw new ArgumentNullException("serializationManager");
            }
            if (propertyType == null)
            {
                throw new ArgumentNullException("propertyType");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            object obj2 = null;
            XmlReader reader = serializationManager.WorkflowMarkupStack[typeof(XmlReader)] as XmlReader;
            if (reader == null)
            {
                return null;
            }
            if (this.IsValidCompactAttributeFormat(value))
            {
                return this.DeserializeFromCompactFormat(serializationManager, reader, value);
            }
            if (value.StartsWith("{}", StringComparison.Ordinal))
            {
                value = value.Substring(2);
            }
            if (propertyType.IsGenericType && (propertyType.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                Type type = propertyType.GetGenericArguments()[0];
                propertyType = type;
            }
            if (propertyType.IsPrimitive || (propertyType == typeof(string)))
            {
                return Convert.ChangeType(value, propertyType, CultureInfo.InvariantCulture);
            }
            if (propertyType.IsEnum)
            {
                return Enum.Parse(propertyType, value, true);
            }
            if (!typeof(Delegate).IsAssignableFrom(propertyType))
            {
                if (typeof(TimeSpan) == propertyType)
                {
                    return TimeSpan.Parse(value, CultureInfo.InvariantCulture);
                }
                if (typeof(DateTime) == propertyType)
                {
                    return DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                }
                if (typeof(Guid) == propertyType)
                {
                    return Utility.CreateGuid(value);
                }
                if (typeof(Type).IsAssignableFrom(propertyType))
                {
                    obj2 = serializationManager.GetType(value);
                    if (obj2 != null)
                    {
                        Type type2 = obj2 as Type;
                        if ((type2.IsPrimitive || type2.IsEnum) || (type2 == typeof(string)))
                        {
                            return type2;
                        }
                    }
                    ITypeProvider service = serializationManager.GetService(typeof(ITypeProvider)) as ITypeProvider;
                    if (service != null)
                    {
                        Type type3 = service.GetType(value);
                        if (type3 != null)
                        {
                            return type3;
                        }
                    }
                    return value;
                }
                if (typeof(IConvertible).IsAssignableFrom(propertyType))
                {
                    return Convert.ChangeType(value, propertyType, CultureInfo.InvariantCulture);
                }
                if (!propertyType.IsAssignableFrom(value.GetType()))
                {
                    throw CreateSerializationError(SR.GetString("Error_SerializerPrimitivePropertyNoLogic", new object[] { "", value.Trim(), "" }), reader);
                }
            }
            return value;
        }

        private static bool IsMarkupExtension(Type type)
        {
            if (!typeof(MarkupExtension).IsAssignableFrom(type) && !typeof(Type).IsAssignableFrom(type))
            {
                return typeof(Array).IsAssignableFrom(type);
            }
            return true;
        }

        private static bool IsMarkupExtension(XmlQualifiedName xmlQualifiedName)
        {
            bool flag = false;
            if (!xmlQualifiedName.Namespace.Equals("http://schemas.microsoft.com/winfx/2006/xaml", StringComparison.Ordinal) || ((!xmlQualifiedName.Name.Equals(typeof(Array).Name) && !string.Equals(xmlQualifiedName.Name, "Null", StringComparison.Ordinal)) && ((!string.Equals(xmlQualifiedName.Name, typeof(NullExtension).Name, StringComparison.Ordinal) && !string.Equals(xmlQualifiedName.Name, "Type", StringComparison.Ordinal)) && !string.Equals(xmlQualifiedName.Name, typeof(TypeExtension).Name, StringComparison.Ordinal))))
            {
                return flag;
            }
            return true;
        }

        internal bool IsValidCompactAttributeFormat(string attributeValue)
        {
            return ((((attributeValue.Length > 0) && attributeValue.StartsWith("{", StringComparison.Ordinal)) && !attributeValue.StartsWith("{}", StringComparison.Ordinal)) && attributeValue.EndsWith("}", StringComparison.Ordinal));
        }

        private static EventInfo LookupEvent(IList<EventInfo> events, string eventName)
        {
            if ((events != null) && !string.IsNullOrEmpty(eventName))
            {
                foreach (EventInfo info in events)
                {
                    if (info.Name == eventName)
                    {
                        return info;
                    }
                }
            }
            return null;
        }

        private static PropertyInfo LookupProperty(IList<PropertyInfo> properties, string propertyName)
        {
            if ((properties != null) && !string.IsNullOrEmpty(propertyName))
            {
                foreach (PropertyInfo info in properties)
                {
                    if (info.Name == propertyName)
                    {
                        return info;
                    }
                }
            }
            return null;
        }

        protected virtual void OnAfterDeserialize(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
        }

        protected virtual void OnAfterSerialize(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
        }

        protected virtual void OnBeforeDeserialize(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
        }

        internal virtual void OnBeforeDeserializeContents(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
        }

        protected virtual void OnBeforeSerialize(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
        }

        internal virtual void OnBeforeSerializeContents(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
        }

        private object OnGetRuntimeNameValue(ExtendedPropertyInfo extendedProperty, object extendee)
        {
            return extendedProperty.RealPropertyInfo.GetValue(extendee, null);
        }

        private XmlQualifiedName OnGetRuntimeQualifiedName(ExtendedPropertyInfo extendedProperty, WorkflowMarkupSerializationManager manager, out string prefix)
        {
            prefix = "x";
            return new XmlQualifiedName(extendedProperty.Name, "http://schemas.microsoft.com/winfx/2006/xaml");
        }

        private void OnSetRuntimeNameValue(ExtendedPropertyInfo extendedProperty, object extendee, object value)
        {
            if ((extendee != null) && (value != null))
            {
                extendedProperty.RealPropertyInfo.SetValue(extendee, value, null);
            }
        }

        private void RemoveEscapes(ref string value)
        {
            StringBuilder builder = null;
            bool flag = true;
            for (int i = 0; i < value.Length; i++)
            {
                if (flag && (value[i] == '\\'))
                {
                    if (builder == null)
                    {
                        builder = new StringBuilder(value.Length);
                        builder.Append(value.Substring(0, i));
                    }
                    flag = false;
                }
                else if (builder != null)
                {
                    builder.Append(value[i]);
                    flag = true;
                }
            }
            if (builder != null)
            {
                value = builder.ToString();
            }
        }

        private DependencyProperty ResolveDependencyProperty(WorkflowMarkupSerializationManager serializationManager, XmlReader reader, object attachedObj, string fullPropertyName)
        {
            Type type = null;
            string str = string.Empty;
            int index = fullPropertyName.IndexOf(".");
            if (index != -1)
            {
                string str2 = fullPropertyName.Substring(0, index);
                str = fullPropertyName.Substring(index + 1);
                if (!string.IsNullOrEmpty(str2) && !string.IsNullOrEmpty(str))
                {
                    type = serializationManager.GetType(new XmlQualifiedName(str2, reader.LookupNamespace(reader.Prefix)));
                }
            }
            else
            {
                type = attachedObj.GetType();
                str = fullPropertyName;
            }
            if (type == null)
            {
                return null;
            }
            DependencyProperty property = null;
            FieldInfo field = type.GetField(str + "Property", BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            if (field == null)
            {
                field = type.GetField(str + "Event", BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            }
            if (field != null)
            {
                property = field.GetValue(attachedObj) as DependencyProperty;
                if (property != null)
                {
                    object[] attributes = property.DefaultMetadata.GetAttributes(typeof(DesignerSerializationVisibilityAttribute));
                    if (attributes.Length > 0)
                    {
                        DesignerSerializationVisibilityAttribute attribute = attributes[0] as DesignerSerializationVisibilityAttribute;
                        if (attribute.Visibility == DesignerSerializationVisibility.Hidden)
                        {
                            property = null;
                        }
                    }
                }
            }
            return property;
        }

        public void Serialize(XmlWriter writer, object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            DesignerSerializationManager serializationManager = new DesignerSerializationManager();
            using (serializationManager.CreateSession())
            {
                this.Serialize(serializationManager, writer, obj);
            }
        }

        public void Serialize(IDesignerSerializationManager serializationManager, XmlWriter writer, object obj)
        {
            if (serializationManager == null)
            {
                throw new ArgumentNullException("serializationManager");
            }
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            WorkflowMarkupSerializationManager manager = serializationManager as WorkflowMarkupSerializationManager;
            if (manager == null)
            {
                manager = new WorkflowMarkupSerializationManager(serializationManager);
            }
            StringWriter output = new StringWriter(CultureInfo.InvariantCulture);
            XmlWriter context = Helpers.CreateXmlWriter(output);
            manager.WorkflowMarkupStack.Push(context);
            manager.WorkflowMarkupStack.Push(output);
            try
            {
                this.SerializeObject(manager, obj, context);
            }
            finally
            {
                context.Close();
                writer.WriteRaw(output.ToString());
                writer.Flush();
                manager.WorkflowMarkupStack.Pop();
                manager.WorkflowMarkupStack.Pop();
            }
        }

        internal void SerializeContents(WorkflowMarkupSerializationManager serializationManager, object obj, XmlWriter writer, bool dictionaryKey)
        {
            if (serializationManager == null)
            {
                throw new ArgumentNullException("serializationManager");
            }
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            WorkflowMarkupSerializer parentObjectSerializer = null;
            try
            {
                parentObjectSerializer = serializationManager.GetSerializer(obj.GetType(), typeof(WorkflowMarkupSerializer)) as WorkflowMarkupSerializer;
            }
            catch (Exception exception)
            {
                serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString("Error_SerializerThrewException", new object[] { obj.GetType().FullName, exception.Message }), exception));
                return;
            }
            if (parentObjectSerializer != null)
            {
                try
                {
                    parentObjectSerializer.OnBeforeSerialize(serializationManager, obj);
                }
                catch (Exception exception2)
                {
                    serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString("Error_SerializerThrewException", new object[] { obj.GetType().FullName, exception2.Message }), exception2));
                    return;
                }
                Hashtable hashtable = new Hashtable();
                ArrayList list = new ArrayList();
                IDictionary<DependencyProperty, object> dependencyProperties = null;
                List<PropertyInfo> list2 = new List<PropertyInfo>();
                List<EventInfo> list3 = new List<EventInfo>();
                Hashtable hashtable2 = null;
                if (((obj.GetType().IsPrimitive || (obj.GetType() == typeof(string))) || ((obj.GetType() == typeof(decimal)) || (obj.GetType() == typeof(DateTime)))) || (((obj.GetType() == typeof(TimeSpan)) || obj.GetType().IsEnum) || (obj.GetType() == typeof(Guid))))
                {
                    if ((((obj.GetType() == typeof(char)) || (obj.GetType() == typeof(byte))) || ((obj.GetType() == typeof(short)) || (obj.GetType() == typeof(decimal)))) || (((obj.GetType() == typeof(DateTime)) || (obj.GetType() == typeof(TimeSpan))) || (obj.GetType().IsEnum || (obj.GetType() == typeof(Guid)))))
                    {
                        if ((obj.GetType() != typeof(char)) || (((char) obj) != '\0'))
                        {
                            string str = string.Empty;
                            if (obj.GetType() == typeof(DateTime))
                            {
                                str = ((DateTime) obj).ToString("o", CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                TypeConverter converter = TypeDescriptor.GetConverter(obj.GetType());
                                if ((converter != null) && converter.CanConvertTo(typeof(string)))
                                {
                                    str = converter.ConvertTo(null, CultureInfo.InvariantCulture, obj, typeof(string)) as string;
                                }
                                else
                                {
                                    str = Convert.ToString(obj, CultureInfo.InvariantCulture);
                                }
                            }
                            writer.WriteValue(str);
                        }
                    }
                    else if (obj.GetType() == typeof(string))
                    {
                        string str2 = obj as string;
                        str2 = str2.Replace('\0', ' ');
                        if (!str2.StartsWith("{", StringComparison.Ordinal) || !str2.EndsWith("}", StringComparison.Ordinal))
                        {
                            writer.WriteValue(str2);
                        }
                        else
                        {
                            writer.WriteValue("{}" + str2);
                        }
                    }
                    else
                    {
                        writer.WriteValue(obj);
                    }
                    if (!dictionaryKey)
                    {
                        list2.AddRange(serializationManager.GetExtendedProperties(obj));
                    }
                }
                else
                {
                    try
                    {
                        if ((obj is DependencyObject) && ((DependencyObject) obj).UserData.Contains(UserDataKeys.DesignTimeTypeNames))
                        {
                            hashtable2 = ((DependencyObject) obj).UserData[UserDataKeys.DesignTimeTypeNames] as Hashtable;
                        }
                        dependencyProperties = parentObjectSerializer.GetDependencyProperties(serializationManager, obj);
                        list2.AddRange(parentObjectSerializer.GetProperties(serializationManager, obj));
                        if (!dictionaryKey)
                        {
                            list2.AddRange(serializationManager.GetExtendedProperties(obj));
                        }
                        list3.AddRange(parentObjectSerializer.GetEvents(serializationManager, obj));
                    }
                    catch (Exception exception3)
                    {
                        serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString("Error_SerializerThrewException", new object[] { obj.GetType().FullName, exception3.Message }), exception3));
                        return;
                    }
                }
                if (dependencyProperties != null)
                {
                    if (hashtable2 != null)
                    {
                        foreach (object obj2 in hashtable2.Keys)
                        {
                            DependencyProperty key = obj2 as DependencyProperty;
                            if ((key != null) && !dependencyProperties.ContainsKey(key))
                            {
                                dependencyProperties.Add(key, hashtable2[key]);
                            }
                        }
                    }
                    foreach (DependencyProperty property2 in dependencyProperties.Keys)
                    {
                        string name = string.Empty;
                        if (property2.IsAttached)
                        {
                            string prefix = string.Empty;
                            name = serializationManager.GetXmlQualifiedName(property2.OwnerType, out prefix).Name + "." + property2.Name;
                        }
                        else
                        {
                            name = property2.Name;
                        }
                        if (property2.IsAttached || !property2.DefaultMetadata.IsMetaProperty)
                        {
                            hashtable.Add(name, property2);
                        }
                    }
                }
                if (list2 != null)
                {
                    foreach (PropertyInfo info in list2)
                    {
                        if ((info != null) && !hashtable.ContainsKey(info.Name))
                        {
                            hashtable.Add(info.Name, info);
                        }
                    }
                }
                if (list3 != null)
                {
                    foreach (EventInfo info2 in list3)
                    {
                        if ((info2 != null) && !hashtable.ContainsKey(info2.Name))
                        {
                            hashtable.Add(info2.Name, info2);
                        }
                    }
                }
                using (ContentProperty property3 = new ContentProperty(serializationManager, parentObjectSerializer, obj))
                {
                    SafeXmlNodeWriter writer2;
                    foreach (object obj3 in hashtable.Values)
                    {
                        string propertyName = string.Empty;
                        object designTimeTypeName = null;
                        Type propertyType = null;
                        try
                        {
                            if (obj3 is PropertyInfo)
                            {
                                PropertyInfo info3 = obj3 as PropertyInfo;
                                ParameterInfo[] indexParameters = info3.GetIndexParameters();
                                if ((indexParameters != null) && (indexParameters.Length > 0))
                                {
                                    continue;
                                }
                                propertyName = info3.Name;
                                designTimeTypeName = null;
                                if (info3.CanRead)
                                {
                                    designTimeTypeName = info3.GetValue(obj, null);
                                    if ((designTimeTypeName == null) && TypeProvider.IsAssignable(typeof(Type), info3.PropertyType))
                                    {
                                        DependencyProperty property4 = DependencyProperty.FromName(info3.Name, info3.ReflectedType);
                                        if (property4 != null)
                                        {
                                            designTimeTypeName = Helpers.GetDesignTimeTypeName(obj, property4);
                                        }
                                        if (designTimeTypeName == null)
                                        {
                                            string str6 = info3.ReflectedType.FullName + "." + info3.Name;
                                            designTimeTypeName = Helpers.GetDesignTimeTypeName(obj, str6);
                                        }
                                        if (designTimeTypeName != null)
                                        {
                                            designTimeTypeName = new TypeExtension((string) designTimeTypeName);
                                        }
                                    }
                                }
                                propertyType = info3.PropertyType;
                            }
                            else if (obj3 is EventInfo)
                            {
                                EventInfo info4 = obj3 as EventInfo;
                                propertyName = info4.Name;
                                designTimeTypeName = WorkflowMarkupSerializationHelpers.GetEventHandlerName(obj, info4.Name);
                                if (((designTimeTypeName == null) || ((designTimeTypeName is string) && string.IsNullOrEmpty((string) designTimeTypeName))) && (obj is DependencyObject))
                                {
                                    DependencyProperty dependencyEvent = DependencyProperty.FromName(propertyName, obj.GetType());
                                    if (dependencyEvent != null)
                                    {
                                        Activity activity = serializationManager.Context[typeof(Activity)] as Activity;
                                        Delegate handler = ((DependencyObject) obj).GetHandler(dependencyEvent) as Delegate;
                                        if (((handler != null) && (activity != null)) && object.Equals(handler.Target.GetType(), Helpers.GetRootActivity(activity).GetType()))
                                        {
                                            designTimeTypeName = handler;
                                        }
                                    }
                                }
                                propertyType = info4.EventHandlerType;
                            }
                            else if (obj3 is DependencyProperty)
                            {
                                DependencyProperty property6 = obj3 as DependencyProperty;
                                propertyName = property6.Name;
                                designTimeTypeName = dependencyProperties[property6];
                                propertyType = property6.PropertyType;
                            }
                        }
                        catch (Exception innerException)
                        {
                            while ((innerException is TargetInvocationException) && (innerException.InnerException != null))
                            {
                                innerException = innerException.InnerException;
                            }
                            serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString("Error_SerializerPropertyGetFailed", new object[] { propertyName, obj.GetType().FullName, innerException.Message })));
                            continue;
                        }
                        if (!(obj3 is PropertyInfo) || (property3.Property != ((PropertyInfo) obj3)))
                        {
                            Type objectType = null;
                            if (designTimeTypeName != null)
                            {
                                designTimeTypeName = GetMarkupExtensionFromValue(designTimeTypeName);
                                objectType = designTimeTypeName.GetType();
                            }
                            else if (obj3 is PropertyInfo)
                            {
                                designTimeTypeName = new NullExtension();
                                objectType = designTimeTypeName.GetType();
                                Attribute[] attributeArray = Attribute.GetCustomAttributes(obj3 as PropertyInfo, typeof(DefaultValueAttribute), true);
                                if (attributeArray.Length > 0)
                                {
                                    DefaultValueAttribute attribute = attributeArray[0] as DefaultValueAttribute;
                                    if (attribute.Value == null)
                                    {
                                        designTimeTypeName = null;
                                    }
                                }
                            }
                            if (designTimeTypeName != null)
                            {
                                objectType = designTimeTypeName.GetType();
                            }
                            serializationManager.Context.Push(obj3);
                            WorkflowMarkupSerializer serializer = null;
                            try
                            {
                                serializer = serializationManager.GetSerializer(objectType, typeof(WorkflowMarkupSerializer)) as WorkflowMarkupSerializer;
                            }
                            catch (Exception exception5)
                            {
                                serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString("Error_SerializerThrewException", new object[] { obj.GetType().FullName, exception5.Message }), exception5));
                                serializationManager.Context.Pop();
                                continue;
                            }
                            if (serializer == null)
                            {
                                serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString("Error_SerializerNotAvailableForSerialize", new object[] { objectType.FullName })));
                                serializationManager.Context.Pop();
                            }
                            else
                            {
                                try
                                {
                                    if (serializer.ShouldSerializeValue(serializationManager, designTimeTypeName))
                                    {
                                        if (serializer.CanSerializeToString(serializationManager, designTimeTypeName) && (propertyType != typeof(object)))
                                        {
                                            using (writer2 = new SafeXmlNodeWriter(serializationManager, obj, obj3, XmlNodeType.Attribute))
                                            {
                                                if (serializer is MarkupExtensionSerializer)
                                                {
                                                    serializer.SerializeToString(serializationManager, designTimeTypeName);
                                                }
                                                else
                                                {
                                                    string str7 = serializer.SerializeToString(serializationManager, designTimeTypeName);
                                                    if (!string.IsNullOrEmpty(str7))
                                                    {
                                                        str7 = str7.Replace('\0', ' ');
                                                        if (((designTimeTypeName is MarkupExtension) || !str7.StartsWith("{", StringComparison.Ordinal)) || !str7.EndsWith("}", StringComparison.Ordinal))
                                                        {
                                                            writer.WriteString(str7);
                                                        }
                                                        else
                                                        {
                                                            writer.WriteString("{}" + str7);
                                                        }
                                                    }
                                                }
                                                continue;
                                            }
                                        }
                                        list.Add(obj3);
                                    }
                                }
                                catch (Exception exception6)
                                {
                                    serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString("Error_SerializerNoSerializeLogic", new object[] { propertyName, obj.GetType().FullName }), exception6));
                                }
                                finally
                                {
                                    serializationManager.Context.Pop();
                                }
                            }
                        }
                    }
                    try
                    {
                        parentObjectSerializer.OnBeforeSerializeContents(serializationManager, obj);
                    }
                    catch (Exception exception7)
                    {
                        serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString("Error_SerializerThrewException", new object[] { obj.GetType().FullName, exception7.Message }), exception7));
                        return;
                    }
                    foreach (object obj5 in list)
                    {
                        string str8 = string.Empty;
                        object markupExtensionFromValue = null;
                        Type type = null;
                        bool flag = false;
                        try
                        {
                            if (obj5 is PropertyInfo)
                            {
                                PropertyInfo info5 = obj5 as PropertyInfo;
                                str8 = info5.Name;
                                markupExtensionFromValue = info5.CanRead ? info5.GetValue(obj, null) : null;
                                type = obj.GetType();
                                flag = !info5.CanWrite;
                            }
                            else if (obj5 is DependencyProperty)
                            {
                                DependencyProperty property7 = obj5 as DependencyProperty;
                                str8 = property7.Name;
                                markupExtensionFromValue = dependencyProperties[property7];
                                type = property7.OwnerType;
                                flag = ((byte) (property7.DefaultMetadata.Options & DependencyPropertyOptions.ReadOnly)) == 2;
                            }
                        }
                        catch (Exception exception8)
                        {
                            while ((exception8 is TargetInvocationException) && (exception8.InnerException != null))
                            {
                                exception8 = exception8.InnerException;
                            }
                            serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString("Error_SerializerPropertyGetFailed", new object[] { str8, type.FullName, exception8.Message })));
                            continue;
                        }
                        if ((!(obj5 is PropertyInfo) || (((PropertyInfo) obj5) != property3.Property)) && (markupExtensionFromValue != null))
                        {
                            markupExtensionFromValue = GetMarkupExtensionFromValue(markupExtensionFromValue);
                            WorkflowMarkupSerializer serializer3 = serializationManager.GetSerializer(markupExtensionFromValue.GetType(), typeof(WorkflowMarkupSerializer)) as WorkflowMarkupSerializer;
                            if (serializer3 != null)
                            {
                                using (writer2 = new SafeXmlNodeWriter(serializationManager, obj, obj5, XmlNodeType.Element))
                                {
                                    if (flag)
                                    {
                                        serializer3.SerializeContents(serializationManager, markupExtensionFromValue, writer, false);
                                    }
                                    else
                                    {
                                        serializer3.SerializeObject(serializationManager, markupExtensionFromValue, writer);
                                    }
                                    continue;
                                }
                            }
                            serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString("Error_SerializerNotAvailableForSerialize", new object[] { markupExtensionFromValue.GetType().FullName })));
                        }
                    }
                    try
                    {
                        object contents = property3.GetContents();
                        if (contents != null)
                        {
                            contents = GetMarkupExtensionFromValue(contents);
                            WorkflowMarkupSerializer serializer4 = serializationManager.GetSerializer(contents.GetType(), typeof(WorkflowMarkupSerializer)) as WorkflowMarkupSerializer;
                            if (serializer4 == null)
                            {
                                serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString("Error_SerializerNotAvailableForSerialize", new object[] { contents.GetType() })));
                            }
                            else if (serializer4.CanSerializeToString(serializationManager, contents) && ((property3.Property == null) || (property3.Property.PropertyType != typeof(object))))
                            {
                                string str9 = serializer4.SerializeToString(serializationManager, contents);
                                if (!string.IsNullOrEmpty(str9))
                                {
                                    str9 = str9.Replace('\0', ' ');
                                    if (((contents is MarkupExtension) || !str9.StartsWith("{", StringComparison.Ordinal)) || !str9.EndsWith("}", StringComparison.Ordinal))
                                    {
                                        writer.WriteString(str9);
                                    }
                                    else
                                    {
                                        writer.WriteString("{}" + str9);
                                    }
                                }
                            }
                            else if (CollectionMarkupSerializer.IsValidCollectionType(contents.GetType()))
                            {
                                if (property3.Property == null)
                                {
                                    IEnumerable enumerable = contents as IEnumerable;
                                    foreach (object obj8 in enumerable)
                                    {
                                        if (obj8 == null)
                                        {
                                            this.SerializeObject(serializationManager, new NullExtension(), writer);
                                        }
                                        else
                                        {
                                            object obj9 = obj8;
                                            bool flag2 = obj9 is DictionaryEntry;
                                            try
                                            {
                                                if (flag2)
                                                {
                                                    serializationManager.WorkflowMarkupStack.Push(obj8);
                                                    DictionaryEntry entry = (DictionaryEntry) obj9;
                                                    obj9 = entry.Value;
                                                }
                                                obj9 = GetMarkupExtensionFromValue(obj9);
                                                WorkflowMarkupSerializer serializer5 = serializationManager.GetSerializer(obj9.GetType(), typeof(WorkflowMarkupSerializer)) as WorkflowMarkupSerializer;
                                                if (serializer5 != null)
                                                {
                                                    serializer5.SerializeObject(serializationManager, obj9, writer);
                                                }
                                                else
                                                {
                                                    serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString("Error_SerializerNotAvailableForSerialize", new object[] { obj9.GetType() })));
                                                }
                                            }
                                            finally
                                            {
                                                if (flag2)
                                                {
                                                    serializationManager.WorkflowMarkupStack.Pop();
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    serializer4.SerializeContents(serializationManager, contents, writer, false);
                                }
                            }
                            else
                            {
                                serializer4.SerializeObject(serializationManager, contents, writer);
                            }
                        }
                    }
                    catch (Exception exception9)
                    {
                        serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString("Error_SerializerThrewException", new object[] { obj.GetType().FullName, exception9.Message }), exception9));
                        return;
                    }
                }
                try
                {
                    parentObjectSerializer.OnAfterSerialize(serializationManager, obj);
                }
                catch (Exception exception10)
                {
                    serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString("Error_SerializerThrewException", new object[] { obj.GetType().FullName, exception10.Message }), exception10));
                    return;
                }
            }
            else
            {
                serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString("Error_SerializerNotAvailableForSerialize", new object[] { obj.GetType().FullName })));
            }
        }

        internal void SerializeObject(WorkflowMarkupSerializationManager serializationManager, object obj, XmlWriter writer)
        {
            if (serializationManager == null)
            {
                throw new ArgumentNullException("serializationManager");
            }
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            try
            {
                serializationManager.WorkflowMarkupStack.Push(writer);
                using (new SafeXmlNodeWriter(serializationManager, obj, null, XmlNodeType.Element))
                {
                    DictionaryEntry? nullable = null;
                    if (serializationManager.WorkflowMarkupStack[typeof(DictionaryEntry)] != null)
                    {
                        nullable = new DictionaryEntry?((DictionaryEntry) serializationManager.WorkflowMarkupStack[typeof(DictionaryEntry)]);
                    }
                    bool dictionaryKey = (nullable.HasValue && (((!nullable.Value.GetType().IsValueType && (nullable.Value.Key == nullable.Value.Value)) && (nullable.Value.Value == obj)) || ((nullable.Value.GetType().IsValueType && nullable.Value.Key.Equals(nullable.Value.Value)) && nullable.Value.Value.Equals(obj)))) && serializationManager.SerializationStack.Contains(obj);
                    if (dictionaryKey || !serializationManager.SerializationStack.Contains(obj))
                    {
                        serializationManager.Context.Push(obj);
                        serializationManager.SerializationStack.Push(obj);
                        try
                        {
                            this.SerializeContents(serializationManager, obj, writer, dictionaryKey);
                            return;
                        }
                        finally
                        {
                            serializationManager.Context.Pop();
                            serializationManager.SerializationStack.Pop();
                        }
                    }
                    throw new WorkflowMarkupSerializationException(SR.GetString("Error_SerializerStackOverflow", new object[] { obj.ToString(), obj.GetType().FullName }), 0, 0);
                }
            }
            finally
            {
                serializationManager.WorkflowMarkupStack.Pop();
            }
        }

        protected internal virtual string SerializeToString(WorkflowMarkupSerializationManager serializationManager, object value)
        {
            if (serializationManager == null)
            {
                throw new ArgumentNullException("serializationManager");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (typeof(Delegate).IsAssignableFrom(value.GetType()))
            {
                return ((Delegate) value).Method.Name;
            }
            if (typeof(DateTime).IsAssignableFrom(value.GetType()))
            {
                DateTime time = (DateTime) value;
                return time.ToString("o", CultureInfo.InvariantCulture);
            }
            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        private void SetDependencyPropertyValue(WorkflowMarkupSerializationManager serializationManager, object obj, DependencyProperty dependencyProperty, object value)
        {
            if (dependencyProperty == null)
            {
                throw new ArgumentNullException("dependencyProperty");
            }
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            DependencyObject owner = obj as DependencyObject;
            if (owner == null)
            {
                throw new ArgumentException(SR.GetString("Error_InvalidArgumentValue"), "obj");
            }
            if (dependencyProperty.IsEvent)
            {
                if (value is ActivityBind)
                {
                    owner.SetBinding(dependencyProperty, value as ActivityBind);
                }
                else if (dependencyProperty.IsAttached)
                {
                    MethodInfo method = dependencyProperty.OwnerType.GetMethod("Add" + dependencyProperty.Name + "Handler", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
                    if (method != null)
                    {
                        ParameterInfo[] parameters = method.GetParameters();
                        if (((parameters == null) || (parameters.Length != 2)) || ((parameters[0].ParameterType != typeof(object)) || (parameters[1].ParameterType != typeof(object))))
                        {
                            method = null;
                        }
                    }
                    if (method != null)
                    {
                        WorkflowMarkupSerializationHelpers.SetEventHandlerName(owner, dependencyProperty.Name, value as string);
                    }
                    else
                    {
                        serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString("Error_MissingAddHandler", new object[] { dependencyProperty.Name, dependencyProperty.OwnerType.FullName })));
                    }
                }
                else
                {
                    WorkflowMarkupSerializationHelpers.SetEventHandlerName(owner, dependencyProperty.Name, value as string);
                }
            }
            else if (value is ActivityBind)
            {
                owner.SetBinding(dependencyProperty, value as ActivityBind);
            }
            else if ((value is string) && TypeProvider.IsAssignable(typeof(Type), dependencyProperty.PropertyType))
            {
                Helpers.SetDesignTimeTypeName(obj, dependencyProperty, value as string);
            }
            else if (dependencyProperty.IsAttached)
            {
                MethodInfo info2 = dependencyProperty.OwnerType.GetMethod("Set" + dependencyProperty.Name, BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
                if (info2 != null)
                {
                    ParameterInfo[] infoArray2 = info2.GetParameters();
                    if (((infoArray2 == null) || (infoArray2.Length != 2)) || ((infoArray2[0].ParameterType != typeof(object)) || (infoArray2[1].ParameterType != typeof(object))))
                    {
                        info2 = null;
                    }
                }
                if (info2 != null)
                {
                    info2.Invoke(null, new object[] { owner, value });
                }
                else
                {
                    serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString("Error_MissingSetAccessor", new object[] { dependencyProperty.Name, dependencyProperty.OwnerType.FullName })));
                }
            }
            else
            {
                List<PropertyInfo> properties = new List<PropertyInfo>();
                properties.AddRange(this.GetProperties(serializationManager, obj));
                PropertyInfo property = LookupProperty(properties, dependencyProperty.Name);
                if ((property != null) && ((value == null) || property.PropertyType.IsAssignableFrom(value.GetType())))
                {
                    if (property.CanWrite)
                    {
                        property.SetValue(obj, value, null);
                    }
                    else if (typeof(ICollection<string>).IsAssignableFrom(value.GetType()))
                    {
                        ICollection<string> is2 = property.GetValue(obj, null) as ICollection<string>;
                        ICollection<string> is3 = value as ICollection<string>;
                        if ((is2 != null) && (is3 != null))
                        {
                            foreach (string str in is3)
                            {
                                is2.Add(str);
                            }
                        }
                    }
                }
                else
                {
                    owner.SetValue(dependencyProperty, value);
                }
            }
        }

        protected internal virtual bool ShouldSerializeValue(WorkflowMarkupSerializationManager serializationManager, object value)
        {
            if (serializationManager == null)
            {
                throw new ArgumentNullException("serializationManager");
            }
            if (value == null)
            {
                return false;
            }
            try
            {
                PropertyInfo current = serializationManager.Context.Current as PropertyInfo;
                if (current != null)
                {
                    Attribute[] attributeArray = Attribute.GetCustomAttributes(current, typeof(DefaultValueAttribute), true);
                    if (attributeArray.Length > 0)
                    {
                        DefaultValueAttribute attribute = attributeArray[0] as DefaultValueAttribute;
                        if (((attribute.Value is IConvertible) && (value is IConvertible)) && object.Equals(Convert.ChangeType(attribute.Value, current.PropertyType, CultureInfo.InvariantCulture), Convert.ChangeType(value, current.PropertyType, CultureInfo.InvariantCulture)))
                        {
                            return false;
                        }
                    }
                }
            }
            catch
            {
            }
            return true;
        }

        private ArrayList TokenizeAttributes(WorkflowMarkupSerializationManager serializationManager, string args, int lineNumber, int linePosition)
        {
            ArrayList list = null;
            int length = args.Length;
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            char ch = '\'';
            int num2 = 0;
            bool flag4 = false;
            StringBuilder builder = null;
            int num3 = 0;
            while (num3 < length)
            {
                if (!flag2 && (args[num3] == '\\'))
                {
                    flag2 = true;
                }
                else
                {
                    if (!flag3 && !char.IsWhiteSpace(args[num3]))
                    {
                        flag3 = true;
                    }
                    if ((flag || (num2 > 0)) || flag3)
                    {
                        if (builder == null)
                        {
                            builder = new StringBuilder(length);
                            list = new ArrayList(1);
                        }
                        if (flag2)
                        {
                            builder.Append('\\');
                            builder.Append(args[num3]);
                            flag2 = false;
                        }
                        else if (flag || (num2 > 0))
                        {
                            if (flag && (args[num3] == ch))
                            {
                                flag = false;
                                list.Add(builder.ToString());
                                builder.Length = 0;
                                flag3 = false;
                            }
                            else
                            {
                                if ((num2 > 0) && (args[num3] == '}'))
                                {
                                    num2--;
                                }
                                else if (args[num3] == '{')
                                {
                                    num2++;
                                }
                                builder.Append(args[num3]);
                            }
                        }
                        else if ((args[num3] == '"') || (args[num3] == '\''))
                        {
                            if ((flag4 && (num3 < (args.Length - 1))) && (args[num3 + 1] == ']'))
                            {
                                flag4 = false;
                                builder.Append(args[num3]);
                            }
                            else if ((num3 > 0) && (args[num3 - 1] == '['))
                            {
                                flag4 = true;
                                builder.Append(args[num3]);
                            }
                            else
                            {
                                if (builder.Length != 0)
                                {
                                    return null;
                                }
                                flag = true;
                                ch = args[num3];
                            }
                        }
                        else if ((args[num3] == ',') || (args[num3] == '='))
                        {
                            if ((builder != null) && (builder.Length > 0))
                            {
                                list.Add(builder.ToString().Trim());
                                builder.Length = 0;
                            }
                            else if ((list.Count == 0) || (list[list.Count - 1] is char))
                            {
                                return null;
                            }
                            list.Add(args[num3]);
                            flag3 = false;
                        }
                        else
                        {
                            if (args[num3] == '}')
                            {
                                if (builder != null)
                                {
                                    if (builder.Length <= 0)
                                    {
                                        if ((list.Count > 0) && (list[list.Count - 1] is char))
                                        {
                                            return null;
                                        }
                                    }
                                    else
                                    {
                                        list.Add(builder.ToString().Trim());
                                        builder.Length = 0;
                                    }
                                }
                                break;
                            }
                            if (args[num3] == '{')
                            {
                                num2++;
                            }
                            builder.Append(args[num3]);
                        }
                    }
                }
                num3++;
            }
            if ((builder != null) && (builder.Length > 0))
            {
                throw new Exception(SR.GetString("Error_MarkupExtensionMissingTerminatingCharacter"));
            }
            if (num3 < length)
            {
                num3++;
                while (num3 < length)
                {
                    if (!char.IsWhiteSpace(args[num3]))
                    {
                        throw new Exception(SR.GetString("Error_ExtraCharacterFoundAtEnd"));
                    }
                    num3++;
                }
            }
            return list;
        }

        private static bool VerifyAttachedPropertyConditions(DependencyProperty dependencyProperty)
        {
            if (dependencyProperty.IsEvent)
            {
                if (dependencyProperty.OwnerType.GetField(dependencyProperty.Name + "Event", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly) == null)
                {
                    return false;
                }
                MethodInfo method = dependencyProperty.OwnerType.GetMethod("Add" + dependencyProperty.Name + "Handler", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
                if (method == null)
                {
                    return false;
                }
                ParameterInfo[] parameters = method.GetParameters();
                if (((parameters != null) && (parameters.Length == 2)) && ((parameters[0].ParameterType == typeof(object)) && (parameters[1].ParameterType == typeof(object))))
                {
                    return true;
                }
            }
            else
            {
                if (dependencyProperty.OwnerType.GetField(dependencyProperty.Name + "Property", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly) == null)
                {
                    return false;
                }
                MethodInfo info2 = dependencyProperty.OwnerType.GetMethod("Set" + dependencyProperty.Name, BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
                if (info2 == null)
                {
                    return false;
                }
                ParameterInfo[] infoArray2 = info2.GetParameters();
                if (((infoArray2 != null) && (infoArray2.Length == 2)) && ((infoArray2[0].ParameterType == typeof(object)) && (infoArray2[1].ParameterType == typeof(object))))
                {
                    return true;
                }
            }
            return false;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ContentInfo
        {
            public int LineNumber;
            public int LinePosition;
            public object Content;
            public ContentInfo(object content, int lineNumber, int linePosition)
            {
                this.Content = content;
                this.LineNumber = lineNumber;
                this.LinePosition = linePosition;
            }
        }

        private class ContentProperty : IDisposable
        {
            private PropertyInfo contentProperty;
            private WorkflowMarkupSerializer contentPropertySerializer;
            private object parentObject;
            private WorkflowMarkupSerializer parentObjectSerializer;
            private WorkflowMarkupSerializationManager serializationManager;

            public ContentProperty(WorkflowMarkupSerializationManager serializationManager, WorkflowMarkupSerializer parentObjectSerializer, object parentObject)
            {
                this.serializationManager = serializationManager;
                this.parentObjectSerializer = parentObjectSerializer;
                this.parentObject = parentObject;
                this.contentProperty = this.GetContentProperty(this.serializationManager, this.parentObject);
                if (this.contentProperty != null)
                {
                    this.contentPropertySerializer = this.serializationManager.GetSerializer(this.contentProperty.PropertyType, typeof(WorkflowMarkupSerializer)) as WorkflowMarkupSerializer;
                    if (this.contentPropertySerializer != null)
                    {
                        try
                        {
                            XmlReader reader = this.serializationManager.WorkflowMarkupStack[typeof(XmlReader)] as XmlReader;
                            object obj2 = null;
                            if (reader == null)
                            {
                                obj2 = this.contentProperty.GetValue(this.parentObject, null);
                            }
                            else if (((!this.contentProperty.PropertyType.IsValueType && !this.contentProperty.PropertyType.IsPrimitive) && ((this.contentProperty.PropertyType != typeof(string)) && !WorkflowMarkupSerializer.IsMarkupExtension(this.contentProperty.PropertyType))) && this.contentProperty.CanWrite)
                            {
                                WorkflowMarkupSerializer serializer = serializationManager.GetSerializer(this.contentProperty.PropertyType, typeof(WorkflowMarkupSerializer)) as WorkflowMarkupSerializer;
                                if (serializer == null)
                                {
                                    serializationManager.ReportError(WorkflowMarkupSerializer.CreateSerializationError(SR.GetString("Error_SerializerNotAvailable", new object[] { this.contentProperty.PropertyType.FullName }), reader));
                                    return;
                                }
                                try
                                {
                                    obj2 = serializer.CreateInstance(serializationManager, this.contentProperty.PropertyType);
                                }
                                catch (Exception exception)
                                {
                                    serializationManager.ReportError(WorkflowMarkupSerializer.CreateSerializationError(SR.GetString("Error_SerializerCreateInstanceFailed", new object[] { this.contentProperty.PropertyType.FullName, exception.Message }), reader));
                                    return;
                                }
                                this.contentProperty.SetValue(this.parentObject, obj2, null);
                            }
                            if ((obj2 != null) && (reader != null))
                            {
                                this.contentPropertySerializer.OnBeforeDeserialize(this.serializationManager, obj2);
                                this.contentPropertySerializer.OnBeforeDeserializeContents(this.serializationManager, obj2);
                            }
                        }
                        catch (Exception exception2)
                        {
                            this.serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString("Error_SerializerThrewException", new object[] { this.parentObject.GetType(), exception2.Message }), exception2));
                        }
                    }
                    else
                    {
                        this.serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString("Error_SerializerNotAvailableForSerialize", new object[] { this.contentProperty.PropertyType.FullName })));
                    }
                }
            }

            private PropertyInfo GetContentProperty(WorkflowMarkupSerializationManager serializationManager, object parentObject)
            {
                PropertyInfo property = null;
                string name = string.Empty;
                object[] customAttributes = parentObject.GetType().GetCustomAttributes(typeof(ContentPropertyAttribute), true);
                if ((customAttributes != null) && (customAttributes.Length > 0))
                {
                    name = ((ContentPropertyAttribute) customAttributes[0]).Name;
                }
                if (!string.IsNullOrEmpty(name))
                {
                    property = parentObject.GetType().GetProperty(name, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);
                    if (property == null)
                    {
                        serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString("Error_ContentPropertyCouldNotBeFound", new object[] { name, parentObject.GetType().FullName })));
                    }
                }
                return property;
            }

            internal object GetContents()
            {
                if (this.contentProperty != null)
                {
                    return this.contentProperty.GetValue(this.parentObject, null);
                }
                return this.parentObjectSerializer.GetChildren(this.serializationManager, this.parentObject);
            }

            internal void SetContents(IList<WorkflowMarkupSerializer.ContentInfo> contents)
            {
                if (contents.Count != 0)
                {
                    if (this.contentProperty == null)
                    {
                        int num = 0;
                        try
                        {
                            foreach (WorkflowMarkupSerializer.ContentInfo info in contents)
                            {
                                this.parentObjectSerializer.AddChild(this.serializationManager, this.parentObject, info.Content);
                                num++;
                            }
                        }
                        catch (Exception exception)
                        {
                            this.serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString("Error_SerializerThrewException", new object[] { this.parentObject.GetType(), exception.Message }), exception, contents[num].LineNumber, contents[num].LinePosition));
                        }
                    }
                    else if (this.contentPropertySerializer != null)
                    {
                        object parentObject = this.contentProperty.GetValue(this.parentObject, null);
                        if (CollectionMarkupSerializer.IsValidCollectionType(this.contentProperty.PropertyType))
                        {
                            if (parentObject == null)
                            {
                                this.serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString("Error_ContentPropertyCanNotBeNull", new object[] { this.contentProperty.Name, this.parentObject.GetType().FullName })));
                            }
                            else
                            {
                                int num2 = 0;
                                try
                                {
                                    foreach (WorkflowMarkupSerializer.ContentInfo info2 in contents)
                                    {
                                        this.contentPropertySerializer.AddChild(this.serializationManager, parentObject, info2.Content);
                                        num2++;
                                    }
                                }
                                catch (Exception exception2)
                                {
                                    this.serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString("Error_SerializerThrewException", new object[] { this.parentObject.GetType(), exception2.Message }), exception2, contents[num2].LineNumber, contents[num2].LinePosition));
                                }
                            }
                        }
                        else if (!this.contentProperty.CanWrite)
                        {
                            this.serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString("Error_ContentPropertyNoSetter", new object[] { this.contentProperty.Name, this.parentObject.GetType() }), contents[0].LineNumber, contents[0].LinePosition));
                        }
                        else
                        {
                            if (contents.Count > 1)
                            {
                                this.serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString("Error_ContentPropertyNoMultipleContents", new object[] { this.contentProperty.Name, this.parentObject.GetType() }), contents[1].LineNumber, contents[1].LinePosition));
                            }
                            object content = contents[0].Content;
                            if (!this.contentProperty.PropertyType.IsAssignableFrom(content.GetType()) && typeof(string).IsAssignableFrom(content.GetType()))
                            {
                                try
                                {
                                    content = this.contentPropertySerializer.DeserializeFromString(this.serializationManager, this.contentProperty.PropertyType, content as string);
                                    content = WorkflowMarkupSerializer.GetValueFromMarkupExtension(this.serializationManager, content);
                                }
                                catch (Exception exception3)
                                {
                                    this.serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString("Error_SerializerThrewException", new object[] { this.parentObject.GetType(), exception3.Message }), exception3, contents[0].LineNumber, contents[0].LinePosition));
                                    return;
                                }
                            }
                            if (content == null)
                            {
                                this.serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString("Error_ContentCanNotBeConverted", new object[] { content as string, this.contentProperty.Name, this.parentObject.GetType().FullName, this.contentProperty.PropertyType.FullName }), contents[0].LineNumber, contents[0].LinePosition));
                            }
                            else if (!this.contentProperty.PropertyType.IsAssignableFrom(content.GetType()))
                            {
                                this.serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString("Error_ContentPropertyValueInvalid", new object[] { content.GetType(), this.contentProperty.Name, this.contentProperty.PropertyType.FullName }), contents[0].LineNumber, contents[0].LinePosition));
                            }
                            else
                            {
                                try
                                {
                                    if (this.contentProperty.PropertyType == typeof(string))
                                    {
                                        content = new WorkflowMarkupSerializer().DeserializeFromString(this.serializationManager, this.contentProperty.PropertyType, content as string);
                                        content = WorkflowMarkupSerializer.GetValueFromMarkupExtension(this.serializationManager, content);
                                    }
                                    this.contentProperty.SetValue(this.parentObject, content, null);
                                }
                                catch (Exception exception4)
                                {
                                    this.serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString("Error_SerializerThrewException", new object[] { this.parentObject.GetType(), exception4.Message }), exception4, contents[0].LineNumber, contents[0].LinePosition));
                                }
                            }
                        }
                    }
                }
            }

            void IDisposable.Dispose()
            {
                if (((this.serializationManager.WorkflowMarkupStack[typeof(XmlReader)] is XmlReader) && (this.contentProperty != null)) && (this.contentPropertySerializer != null))
                {
                    try
                    {
                        object obj2 = this.contentProperty.GetValue(this.parentObject, null);
                        if (obj2 != null)
                        {
                            this.contentPropertySerializer.OnAfterDeserialize(this.serializationManager, obj2);
                        }
                    }
                    catch (Exception exception)
                    {
                        this.serializationManager.ReportError(new WorkflowMarkupSerializationException(SR.GetString("Error_SerializerThrewException", new object[] { this.parentObject.GetType(), exception.Message }), exception));
                    }
                }
            }

            internal PropertyInfo Property
            {
                get
                {
                    return this.contentProperty;
                }
            }
        }

        private sealed class SafeXmlNodeWriter : IDisposable
        {
            private WorkflowMarkupSerializationManager serializationManager;
            private XmlNodeType xmlNodeType;

            public SafeXmlNodeWriter(WorkflowMarkupSerializationManager serializationManager, object owner, object property, XmlNodeType xmlNodeType)
            {
                this.serializationManager = serializationManager;
                this.xmlNodeType = xmlNodeType;
                XmlWriter writer = serializationManager.WorkflowMarkupStack[typeof(XmlWriter)] as XmlWriter;
                if (writer == null)
                {
                    throw new InvalidOperationException(SR.GetString("Error_InternalSerializerError"));
                }
                string prefix = string.Empty;
                string name = string.Empty;
                string ns = string.Empty;
                DependencyProperty property2 = property as DependencyProperty;
                if (property2 != null)
                {
                    if (!property2.IsAttached && (xmlNodeType == XmlNodeType.Attribute))
                    {
                        name = property2.Name;
                        ns = string.Empty;
                    }
                    else
                    {
                        XmlQualifiedName xmlQualifiedName = this.serializationManager.GetXmlQualifiedName(property2.OwnerType, out prefix);
                        name = xmlQualifiedName.Name + "." + property2.Name;
                        ns = xmlQualifiedName.Namespace;
                    }
                }
                else if (property is MemberInfo)
                {
                    ExtendedPropertyInfo info = property as ExtendedPropertyInfo;
                    if (info != null)
                    {
                        XmlQualifiedName name2 = info.GetXmlQualifiedName(this.serializationManager, out prefix);
                        name = name2.Name;
                        ns = name2.Namespace;
                    }
                    else if (this.xmlNodeType == XmlNodeType.Element)
                    {
                        XmlQualifiedName name3 = this.serializationManager.GetXmlQualifiedName(owner.GetType(), out prefix);
                        name = name3.Name + "." + ((MemberInfo) property).Name;
                        ns = name3.Namespace;
                    }
                    else
                    {
                        name = ((MemberInfo) property).Name;
                        ns = string.Empty;
                    }
                }
                else
                {
                    XmlQualifiedName name4 = this.serializationManager.GetXmlQualifiedName(owner.GetType(), out prefix);
                    name = name4.Name;
                    ns = name4.Namespace;
                }
                name = XmlConvert.EncodeName(name);
                if (this.xmlNodeType == XmlNodeType.Element)
                {
                    writer.WriteStartElement(prefix, name, ns);
                    this.serializationManager.WriterDepth++;
                }
                else if (this.xmlNodeType == XmlNodeType.Attribute)
                {
                    writer.WriteStartAttribute(prefix, name, ns);
                }
            }

            void IDisposable.Dispose()
            {
                XmlWriter writer = this.serializationManager.WorkflowMarkupStack[typeof(XmlWriter)] as XmlWriter;
                if ((writer != null) && (writer.WriteState != WriteState.Error))
                {
                    if (this.xmlNodeType == XmlNodeType.Element)
                    {
                        writer.WriteEndElement();
                        this.serializationManager.WriterDepth--;
                    }
                    else if (writer.WriteState == WriteState.Attribute)
                    {
                        writer.WriteEndAttribute();
                    }
                }
            }
        }
    }
}

