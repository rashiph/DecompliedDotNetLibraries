namespace System.Web.UI.Design
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    internal static class ControlSerializer
    {
        private const char FILTER_SEPARATOR_CHAR = ':';
        private static readonly object licenseManagerLock = new object();
        private static bool licenseManagerLockHeld = false;
        private const char OM_CHAR = '.';
        private const char PERSIST_CHAR = '-';

        private static bool CanSerializeAsInnerDefaultString(string filter, string name, Type type, ObjectPersistData persistData, PersistenceMode mode, DataBindingCollection dataBindings, ExpressionBindingCollection expressions)
        {
            if (((((type == typeof(string)) && (filter.Length == 0)) && ((mode == PersistenceMode.InnerDefaultProperty) || (mode == PersistenceMode.EncodedInnerDefaultProperty))) && ((dataBindings == null) || (dataBindings[name] == null))) && ((expressions == null) || (expressions[name] == null)))
            {
                if (persistData == null)
                {
                    return true;
                }
                ICollection propertyAllFilters = persistData.GetPropertyAllFilters(name);
                if (propertyAllFilters.Count == 0)
                {
                    return true;
                }
                if (propertyAllFilters.Count == 1)
                {
                    foreach (PropertyEntry entry in propertyAllFilters)
                    {
                        if ((entry.Filter.Length == 0) && (entry is ComplexPropertyEntry))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static string ConvertObjectModelToPersistName(string objectModelName)
        {
            return objectModelName.Replace('.', '-');
        }

        private static string ConvertPersistToObjectModelName(string persistName)
        {
            return persistName.Replace('-', '.');
        }

        public static Control DeserializeControl(string text, IDesignerHost host)
        {
            return DeserializeControlInternal(text, host, false);
        }

        internal static Control DeserializeControlInternal(string text, IDesignerHost host, bool applyTheme)
        {
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }
            if ((text == null) || (text.Length == 0))
            {
                throw new ArgumentNullException("text");
            }
            string directives = GetDirectives(host);
            if ((directives != null) && (directives.Length > 0))
            {
                text = directives + text;
            }
            DesignTimeParseData data = new DesignTimeParseData(host, text, GetCurrentFilter(host)) {
                ShouldApplyTheme = applyTheme,
                DataBindingHandler = GlobalDataBindingHandler.Handler
            };
            Control control = null;
            lock (typeof(LicenseManager))
            {
                LicenseContext currentContext = LicenseManager.CurrentContext;
                bool flag = false;
                try
                {
                    try
                    {
                        if (!licenseManagerLockHeld)
                        {
                            LicenseManager.CurrentContext = new WebFormsDesigntimeLicenseContext(host);
                            LicenseManager.LockContext(licenseManagerLock);
                            licenseManagerLockHeld = true;
                            flag = true;
                        }
                        control = DesignTimeTemplateParser.ParseControl(data);
                    }
                    catch (TargetInvocationException exception)
                    {
                        throw exception.InnerException;
                    }
                    return control;
                }
                finally
                {
                    if (flag)
                    {
                        LicenseManager.UnlockContext(licenseManagerLock);
                        LicenseManager.CurrentContext = currentContext;
                        licenseManagerLockHeld = false;
                    }
                }
            }
            return control;
        }

        public static Control[] DeserializeControls(string text, IDesignerHost host)
        {
            return DeserializeControlsInternal(text, host, null);
        }

        internal static Control[] DeserializeControlsInternal(string text, IDesignerHost host, List<Triplet> userControlRegisterEntries)
        {
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }
            if ((text == null) || (text.Length == 0))
            {
                throw new ArgumentNullException("text");
            }
            string directives = GetDirectives(host);
            if ((directives != null) && (directives.Length > 0))
            {
                text = directives + text;
            }
            DesignTimeParseData data = new DesignTimeParseData(host, text, GetCurrentFilter(host)) {
                DataBindingHandler = GlobalDataBindingHandler.Handler
            };
            Control[] controlArray = null;
            lock (typeof(LicenseManager))
            {
                LicenseContext currentContext = LicenseManager.CurrentContext;
                try
                {
                    LicenseManager.CurrentContext = new WebFormsDesigntimeLicenseContext(host);
                    LicenseManager.LockContext(licenseManagerLock);
                    controlArray = DesignTimeTemplateParser.ParseControls(data);
                }
                catch (TargetInvocationException exception)
                {
                    throw exception.InnerException;
                }
                finally
                {
                    LicenseManager.UnlockContext(licenseManagerLock);
                    LicenseManager.CurrentContext = currentContext;
                }
            }
            if ((userControlRegisterEntries != null) && (data.UserControlRegisterEntries != null))
            {
                foreach (Triplet triplet in data.UserControlRegisterEntries)
                {
                    userControlRegisterEntries.Add(triplet);
                }
            }
            return controlArray;
        }

        public static ITemplate DeserializeTemplate(string text, IDesignerHost host)
        {
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }
            if ((text == null) || (text.Length == 0))
            {
                return null;
            }
            string parseText = text;
            string directives = GetDirectives(host);
            if ((directives != null) && (directives.Length > 0))
            {
                parseText = directives + text;
            }
            DesignTimeParseData data = new DesignTimeParseData(host, parseText) {
                DataBindingHandler = GlobalDataBindingHandler.Handler
            };
            ITemplate template = null;
            lock (typeof(LicenseManager))
            {
                LicenseContext currentContext = LicenseManager.CurrentContext;
                try
                {
                    LicenseManager.CurrentContext = new WebFormsDesigntimeLicenseContext(host);
                    LicenseManager.LockContext(licenseManagerLock);
                    template = DesignTimeTemplateParser.ParseTemplate(data);
                }
                catch (TargetInvocationException exception)
                {
                    throw exception.InnerException;
                }
                finally
                {
                    LicenseManager.UnlockContext(licenseManagerLock);
                    LicenseManager.CurrentContext = currentContext;
                }
            }
            if ((template != null) && (template is TemplateBuilder))
            {
                ((TemplateBuilder) template).Text = text;
            }
            return template;
        }

        internal static ArrayList GetControlPersistedAttribute(Control control, PropertyDescriptor propDesc, IDesignerHost host)
        {
            ObjectPersistData persistData = null;
            IControlBuilderAccessor accessor = control;
            if (accessor.ControlBuilder != null)
            {
                persistData = accessor.ControlBuilder.GetObjectPersistData();
            }
            string prefix = string.Empty;
            object obj2 = control;
            ArrayList attributes = new ArrayList();
            if (propDesc.SerializationVisibility == DesignerSerializationVisibility.Content)
            {
                obj2 = propDesc.GetValue(control);
                prefix = propDesc.Name;
                SerializeAttributesRecursive(obj2, host, prefix, persistData, GetCurrentFilter(host), attributes, null, null, true);
                return attributes;
            }
            DataBindingCollection dataBindings = ((IDataBindingsAccessor) control).DataBindings;
            ExpressionBindingCollection expressions = ((IExpressionsAccessor) control).Expressions;
            SerializeAttribute(obj2, propDesc, dataBindings, expressions, host, prefix, persistData, GetCurrentFilter(host), attributes, true);
            return attributes;
        }

        internal static ArrayList GetControlPersistedAttributes(Control control, IDesignerHost host)
        {
            ObjectPersistData persistData = null;
            IControlBuilderAccessor accessor = control;
            if (accessor.ControlBuilder != null)
            {
                persistData = accessor.ControlBuilder.GetObjectPersistData();
            }
            return SerializeAttributes(control, host, string.Empty, persistData, GetCurrentFilter(host), true);
        }

        private static string GetCurrentFilter(IDesignerHost host)
        {
            return string.Empty;
        }

        private static string GetDirectives(IDesignerHost designerHost)
        {
            string registerDirectives = string.Empty;
            WebFormsReferenceManager referenceManager = null;
            if (designerHost.RootComponent != null)
            {
                WebFormsRootDesigner designer = designerHost.GetDesigner(designerHost.RootComponent) as WebFormsRootDesigner;
                if (designer != null)
                {
                    referenceManager = designer.ReferenceManager;
                }
            }
            if (referenceManager == null)
            {
                IWebFormReferenceManager service = (IWebFormReferenceManager) designerHost.GetService(typeof(IWebFormReferenceManager));
                if (service != null)
                {
                    registerDirectives = service.GetRegisterDirectives();
                }
                return registerDirectives;
            }
            StringBuilder builder = new StringBuilder();
            foreach (string str2 in referenceManager.GetRegisterDirectives())
            {
                builder.Append(str2);
            }
            return builder.ToString();
        }

        private static IDictionary GetExpandos(string filter, string name, ObjectPersistData persistData)
        {
            IDictionary filteredProperties = null;
            if (persistData != null)
            {
                BuilderPropertyEntry filteredProperty = persistData.GetFilteredProperty(filter, name) as BuilderPropertyEntry;
                if (filteredProperty != null)
                {
                    filteredProperties = filteredProperty.Builder.GetObjectPersistData().GetFilteredProperties(ControlBuilder.DesignerFilter);
                }
            }
            return filteredProperties;
        }

        private static string GetPersistValue(PropertyDescriptor propDesc, Type propType, object propValue, BindingType bindingType, bool topLevelInDesigner)
        {
            string s = string.Empty;
            if (bindingType == BindingType.Data)
            {
                return ("<%# " + propValue.ToString() + " %>");
            }
            if (bindingType == BindingType.Expression)
            {
                return ("<%$ " + propValue.ToString() + " %>");
            }
            if (propType.IsEnum)
            {
                return Enum.Format(propType, propValue, "G");
            }
            if (propType == typeof(string))
            {
                if (propValue != null)
                {
                    s = propValue.ToString();
                    if (!topLevelInDesigner)
                    {
                        s = HttpUtility.HtmlEncode(s);
                    }
                }
                return s;
            }
            TypeConverter converter = null;
            if (propDesc != null)
            {
                converter = propDesc.Converter;
            }
            else
            {
                converter = TypeDescriptor.GetConverter(propValue);
            }
            if (converter != null)
            {
                s = converter.ConvertToInvariantString(null, propValue);
            }
            else
            {
                s = propValue.ToString();
            }
            if (!topLevelInDesigner)
            {
                s = HttpUtility.HtmlEncode(s);
            }
            return s;
        }

        private static object GetPropertyDefaultValue(PropertyDescriptor propDesc, string name, ObjectPersistData defaultPropertyEntries, string filter, IDesignerHost host)
        {
            if ((filter.Length > 0) && (defaultPropertyEntries != null))
            {
                string str = ConvertPersistToObjectModelName(name);
                IFilterResolutionService serviceInstance = null;
                ServiceContainer serviceProvider = new ServiceContainer();
                if (host != null)
                {
                    serviceInstance = (IFilterResolutionService) host.GetService(typeof(IFilterResolutionService));
                    if (serviceInstance != null)
                    {
                        serviceProvider.AddService(typeof(IFilterResolutionService), serviceInstance);
                    }
                    IThemeResolutionService service = (IThemeResolutionService) host.GetService(typeof(IThemeResolutionService));
                    if (service != null)
                    {
                        serviceProvider.AddService(typeof(IThemeResolutionService), service);
                    }
                }
                PropertyEntry filteredProperty = null;
                filteredProperty = defaultPropertyEntries.GetFilteredProperty(string.Empty, str);
                if (filteredProperty is SimplePropertyEntry)
                {
                    return ((SimplePropertyEntry) filteredProperty).Value;
                }
                if (filteredProperty is BoundPropertyEntry)
                {
                    string str2 = ((BoundPropertyEntry) filteredProperty).Expression.Trim();
                    string str3 = ((BoundPropertyEntry) filteredProperty).ExpressionPrefix.Trim();
                    if (str3.Length > 0)
                    {
                        str2 = str3 + ":" + str2;
                    }
                    return str2;
                }
                if (filteredProperty is ComplexPropertyEntry)
                {
                    ControlBuilder builder = ((ComplexPropertyEntry) filteredProperty).Builder;
                    builder.SetServiceProvider(serviceProvider);
                    object obj2 = null;
                    try
                    {
                        obj2 = builder.BuildObject();
                    }
                    finally
                    {
                        builder.SetServiceProvider(null);
                    }
                    return obj2;
                }
            }
            DefaultValueAttribute attribute = (DefaultValueAttribute) propDesc.Attributes[typeof(DefaultValueAttribute)];
            if (attribute != null)
            {
                return attribute.Value;
            }
            return null;
        }

        private static bool GetShouldSerializeValue(object obj, string name, out bool useResult)
        {
            useResult = false;
            Type type = obj.GetType();
            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;
            PropertyInfo property = type.GetProperty(name, bindingAttr);
            bindingAttr |= BindingFlags.NonPublic;
            MethodInfo method = property.DeclaringType.GetMethod("ShouldSerialize" + name, bindingAttr);
            if (method != null)
            {
                useResult = true;
                return (bool) method.Invoke(obj, new object[0]);
            }
            return true;
        }

        private static string GetTagName(Type type, IDesignerHost host)
        {
            string fullName = string.Empty;
            string tagPrefix = string.Empty;
            WebFormsReferenceManager referenceManager = null;
            if (host.RootComponent != null)
            {
                WebFormsRootDesigner designer = host.GetDesigner(host.RootComponent) as WebFormsRootDesigner;
                if (designer != null)
                {
                    referenceManager = designer.ReferenceManager;
                }
            }
            if (referenceManager == null)
            {
                IWebFormReferenceManager service = (IWebFormReferenceManager) host.GetService(typeof(IWebFormReferenceManager));
                if (service != null)
                {
                    tagPrefix = service.GetTagPrefix(type);
                }
            }
            else
            {
                tagPrefix = referenceManager.GetTagPrefix(type);
            }
            if (string.IsNullOrEmpty(tagPrefix))
            {
                tagPrefix = referenceManager.RegisterTagPrefix(type);
            }
            if ((tagPrefix != null) && (tagPrefix.Length != 0))
            {
                fullName = tagPrefix + ":" + type.Name;
            }
            if (fullName.Length == 0)
            {
                fullName = type.FullName;
            }
            return fullName;
        }

        private static void SerializeAttribute(object obj, PropertyDescriptor propDesc, DataBindingCollection dataBindings, ExpressionBindingCollection expressions, IDesignerHost host, string prefix, ObjectPersistData persistData, string filter, ArrayList attributes, bool topLevelInDesigner)
        {
            DesignOnlyAttribute attribute = (DesignOnlyAttribute) propDesc.Attributes[typeof(DesignOnlyAttribute)];
            if ((attribute == null) || !attribute.IsDesignOnly)
            {
                string name = propDesc.Name;
                Type propertyType = propDesc.PropertyType;
                PersistenceMode mode = ((PersistenceModeAttribute) propDesc.Attributes[typeof(PersistenceModeAttribute)]).Mode;
                bool flag = (dataBindings != null) && (dataBindings[name] != null);
                bool flag2 = (expressions != null) && (expressions[name] != null);
                if (((flag || flag2) || (propDesc.SerializationVisibility != DesignerSerializationVisibility.Hidden)) && (((mode == PersistenceMode.Attribute) || ((flag && flag2) && (propertyType == typeof(string)))) || ((mode != PersistenceMode.InnerProperty) && (propertyType == typeof(string)))))
                {
                    string str2 = string.Empty;
                    if (prefix.Length > 0)
                    {
                        str2 = prefix + "-" + name;
                    }
                    else
                    {
                        str2 = name;
                    }
                    if (propDesc.SerializationVisibility == DesignerSerializationVisibility.Content)
                    {
                        SerializeAttributesRecursive(propDesc.GetValue(obj), host, str2, persistData, filter, attributes, dataBindings, expressions, topLevelInDesigner);
                    }
                    else
                    {
                        IAttributeAccessor accessor = obj as IAttributeAccessor;
                        if (!propDesc.IsReadOnly || ((accessor != null) && (accessor.GetAttribute(str2) != null)))
                        {
                            string str3 = ConvertPersistToObjectModelName(str2);
                            if (!FilterableAttribute.IsPropertyFilterable(propDesc))
                            {
                                filter = string.Empty;
                            }
                            if (CanSerializeAsInnerDefaultString(filter, str3, propertyType, persistData, mode, dataBindings, expressions))
                            {
                                if (topLevelInDesigner)
                                {
                                    attributes.Add(new Triplet(filter, str2, null));
                                }
                            }
                            else
                            {
                                bool flag3 = true;
                                object objB = null;
                                object objA = propDesc.GetValue(obj);
                                BindingType none = BindingType.None;
                                if (dataBindings != null)
                                {
                                    DataBinding binding = dataBindings[str3];
                                    if (binding != null)
                                    {
                                        objA = binding.Expression;
                                        none = BindingType.Data;
                                    }
                                }
                                if (none == BindingType.None)
                                {
                                    if (expressions != null)
                                    {
                                        ExpressionBinding binding2 = expressions[str3];
                                        if ((binding2 != null) && !binding2.Generated)
                                        {
                                            objA = binding2.ExpressionPrefix + ":" + binding2.Expression;
                                            none = BindingType.Expression;
                                        }
                                    }
                                    else if (persistData != null)
                                    {
                                        BoundPropertyEntry filteredProperty = persistData.GetFilteredProperty(filter, name) as BoundPropertyEntry;
                                        if ((filteredProperty != null) && !filteredProperty.Generated)
                                        {
                                            objB = GetPropertyDefaultValue(propDesc, str2, persistData, filter, host);
                                            if (object.Equals(objA, objB))
                                            {
                                                objA = filteredProperty.ExpressionPrefix + ":" + filteredProperty.Expression;
                                                none = BindingType.Expression;
                                            }
                                        }
                                    }
                                }
                                if (filter.Length == 0)
                                {
                                    bool useResult = false;
                                    bool flag5 = false;
                                    if (none == BindingType.None)
                                    {
                                        flag5 = GetShouldSerializeValue(obj, name, out useResult);
                                    }
                                    if (useResult)
                                    {
                                        flag3 = flag5;
                                    }
                                    else
                                    {
                                        objB = GetPropertyDefaultValue(propDesc, str2, persistData, filter, host);
                                        flag3 = !object.Equals(objA, objB);
                                    }
                                }
                                else
                                {
                                    objB = GetPropertyDefaultValue(propDesc, str2, persistData, filter, host);
                                    flag3 = !object.Equals(objA, objB);
                                }
                                if (flag3)
                                {
                                    string z = GetPersistValue(propDesc, propertyType, objA, none, topLevelInDesigner);
                                    if (((topLevelInDesigner && (objB != null)) && ((z == null) || (z.Length == 0))) && ShouldPersistBlankValue(objB, propertyType))
                                    {
                                        z = string.Empty;
                                    }
                                    if ((z != null) && (!propertyType.IsArray || (z.Length > 0)))
                                    {
                                        attributes.Add(new Triplet(filter, str2, z));
                                    }
                                    else if (topLevelInDesigner)
                                    {
                                        attributes.Add(new Triplet(filter, str2, null));
                                    }
                                }
                                else if (topLevelInDesigner)
                                {
                                    attributes.Add(new Triplet(filter, str2, null));
                                }
                                if (persistData != null)
                                {
                                    foreach (PropertyEntry entry2 in persistData.GetPropertyAllFilters(str3))
                                    {
                                        if (string.Compare(entry2.Filter, filter, StringComparison.OrdinalIgnoreCase) != 0)
                                        {
                                            string str5 = string.Empty;
                                            if (entry2 is SimplePropertyEntry)
                                            {
                                                SimplePropertyEntry entry3 = (SimplePropertyEntry) entry2;
                                                if (entry3.UseSetAttribute)
                                                {
                                                    str5 = entry3.Value.ToString();
                                                }
                                                else
                                                {
                                                    str5 = GetPersistValue(propDesc, entry2.Type, entry3.Value, BindingType.None, topLevelInDesigner);
                                                }
                                            }
                                            else if (entry2 is BoundPropertyEntry)
                                            {
                                                BoundPropertyEntry entry4 = (BoundPropertyEntry) entry2;
                                                if (entry4.Generated)
                                                {
                                                    continue;
                                                }
                                                string propValue = entry4.Expression.Trim();
                                                none = BindingType.Data;
                                                string expressionPrefix = entry4.ExpressionPrefix;
                                                if (expressionPrefix.Length > 0)
                                                {
                                                    propValue = expressionPrefix + ":" + propValue;
                                                    none = BindingType.Expression;
                                                }
                                                str5 = GetPersistValue(propDesc, entry2.Type, propValue, none, topLevelInDesigner);
                                            }
                                            else if (entry2 is ComplexPropertyEntry)
                                            {
                                                ComplexPropertyEntry entry5 = (ComplexPropertyEntry) entry2;
                                                str5 = (string) entry5.Builder.BuildObject();
                                            }
                                            attributes.Add(new Triplet(entry2.Filter, str2, str5));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void SerializeAttributes(object obj, IDesignerHost host, string prefix, ObjectPersistData persistData, TextWriter writer, string filter)
        {
            foreach (Triplet triplet in SerializeAttributes(obj, host, prefix, persistData, filter, false))
            {
                WriteAttribute(writer, triplet.First.ToString(), triplet.Second.ToString(), triplet.Third.ToString());
            }
        }

        private static ArrayList SerializeAttributes(object obj, IDesignerHost host, string prefix, ObjectPersistData persistData, string filter, bool topLevelInDesigner)
        {
            ArrayList list = new ArrayList();
            SerializeAttributesRecursive(obj, host, prefix, persistData, filter, list, null, null, topLevelInDesigner);
            if (persistData != null)
            {
                foreach (PropertyEntry entry in persistData.AllPropertyEntries)
                {
                    BoundPropertyEntry entry2 = entry as BoundPropertyEntry;
                    if ((entry2 != null) && !entry2.Generated)
                    {
                        string[] strArray = entry2.Name.Split(new char[] { '.' });
                        if (strArray.Length > 1)
                        {
                            object component = obj;
                            foreach (string str in strArray)
                            {
                                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(component)[str];
                                if (descriptor == null)
                                {
                                    break;
                                }
                                PersistenceModeAttribute attribute = descriptor.Attributes[typeof(PersistenceModeAttribute)] as PersistenceModeAttribute;
                                if (attribute != PersistenceModeAttribute.Attribute)
                                {
                                    string propValue = string.IsNullOrEmpty(entry2.ExpressionPrefix) ? entry2.Expression : (entry2.ExpressionPrefix + ":" + entry2.Expression);
                                    string z = GetPersistValue(TypeDescriptor.GetProperties(entry2.PropertyInfo.DeclaringType)[entry2.PropertyInfo.Name], entry2.Type, propValue, string.IsNullOrEmpty(entry2.ExpressionPrefix) ? BindingType.Data : BindingType.Expression, topLevelInDesigner);
                                    list.Add(new Triplet(entry2.Filter, ConvertObjectModelToPersistName(entry2.Name), z));
                                    break;
                                }
                                component = descriptor.GetValue(component);
                            }
                        }
                    }
                }
            }
            if (obj is Control)
            {
                System.Web.UI.AttributeCollection attributes = null;
                if (obj is WebControl)
                {
                    attributes = ((WebControl) obj).Attributes;
                }
                else if (obj is HtmlControl)
                {
                    attributes = ((HtmlControl) obj).Attributes;
                }
                else if (obj is UserControl)
                {
                    attributes = ((UserControl) obj).Attributes;
                }
                if (attributes != null)
                {
                    foreach (string str4 in attributes.Keys)
                    {
                        string str5 = attributes[str4];
                        bool flag = false;
                        if (str5 != null)
                        {
                            object obj3;
                            bool flag2 = false;
                            string propName = ConvertPersistToObjectModelName(str4);
                            PropertyDescriptor descriptor2 = ControlDesigner.GetComplexProperty(obj, propName, out obj3);
                            if ((descriptor2 != null) && !descriptor2.IsReadOnly)
                            {
                                flag2 = true;
                            }
                            if (!flag2)
                            {
                                if (filter.Length == 0)
                                {
                                    flag = true;
                                }
                                else
                                {
                                    PropertyEntry filteredProperty = null;
                                    if (persistData != null)
                                    {
                                        filteredProperty = persistData.GetFilteredProperty(string.Empty, str4);
                                    }
                                    if (filteredProperty is SimplePropertyEntry)
                                    {
                                        flag = !str5.Equals(((SimplePropertyEntry) filteredProperty).PersistedValue);
                                    }
                                    else if (filteredProperty is BoundPropertyEntry)
                                    {
                                        string expression = ((BoundPropertyEntry) filteredProperty).Expression;
                                        string expressionPrefix = ((BoundPropertyEntry) filteredProperty).ExpressionPrefix;
                                        if (expressionPrefix.Length > 0)
                                        {
                                            expression = expressionPrefix + ":" + expression;
                                        }
                                        flag = !str5.Equals(expression);
                                    }
                                    else if (filteredProperty == null)
                                    {
                                        flag = true;
                                    }
                                }
                            }
                            if (flag)
                            {
                                list.Add(new Triplet(filter, str4, str5));
                            }
                        }
                    }
                }
            }
            if (persistData != null)
            {
                if (!string.IsNullOrEmpty(persistData.ResourceKey))
                {
                    list.Add(new Triplet("meta", "resourceKey", persistData.ResourceKey));
                }
                if (!persistData.Localize)
                {
                    list.Add(new Triplet("meta", "localize", "false"));
                }
                foreach (PropertyEntry entry4 in persistData.AllPropertyEntries)
                {
                    if (string.Compare(entry4.Filter, filter, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        if (entry4 is SimplePropertyEntry)
                        {
                            SimplePropertyEntry entry5 = (SimplePropertyEntry) entry4;
                            if (entry5.UseSetAttribute)
                            {
                                list.Add(new Triplet(entry4.Filter, ConvertObjectModelToPersistName(entry4.Name), entry5.Value.ToString()));
                            }
                        }
                        else if (entry4 is BoundPropertyEntry)
                        {
                            BoundPropertyEntry entry6 = (BoundPropertyEntry) entry4;
                            if (entry6.UseSetAttribute)
                            {
                                string str9 = ((BoundPropertyEntry) entry4).Expression;
                                string str10 = ((BoundPropertyEntry) entry4).ExpressionPrefix;
                                if (str10.Length > 0)
                                {
                                    str9 = str10 + ":" + str9;
                                }
                                list.Add(new Triplet(entry4.Filter, ConvertObjectModelToPersistName(entry4.Name), str9));
                            }
                        }
                    }
                }
            }
            if (((obj is Control) && (persistData != null)) && (host.GetDesigner((Control) obj) == null))
            {
                foreach (EventEntry entry7 in persistData.EventEntries)
                {
                    list.Add(new Triplet(string.Empty, "On" + entry7.Name, entry7.HandlerMethodName));
                }
            }
            return list;
        }

        private static void SerializeAttributesRecursive(object obj, IDesignerHost host, string prefix, ObjectPersistData persistData, string filter, ArrayList attributes, DataBindingCollection dataBindings, ExpressionBindingCollection expressions, bool topLevelInDesigner)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(obj);
            if (obj is IDataBindingsAccessor)
            {
                dataBindings = ((IDataBindingsAccessor) obj).DataBindings;
            }
            if (obj is Control)
            {
                try
                {
                    ControlCollection controls = ((Control) obj).Controls;
                }
                catch (Exception exception)
                {
                    IComponentDesignerDebugService service = host.GetService(typeof(IComponentDesignerDebugService)) as IComponentDesignerDebugService;
                    if (service != null)
                    {
                        service.Fail(exception.Message);
                    }
                }
            }
            if (obj is IExpressionsAccessor)
            {
                expressions = ((IExpressionsAccessor) obj).Expressions;
            }
            for (int i = 0; i < properties.Count; i++)
            {
                try
                {
                    SerializeAttribute(obj, properties[i], dataBindings, expressions, host, prefix, persistData, filter, attributes, topLevelInDesigner);
                }
                catch (Exception exception2)
                {
                    if (host != null)
                    {
                        IComponentDesignerDebugService service2 = host.GetService(typeof(IComponentDesignerDebugService)) as IComponentDesignerDebugService;
                        if (service2 != null)
                        {
                            service2.Fail(exception2.Message);
                        }
                    }
                }
            }
        }

        private static void SerializeCollectionProperty(object obj, IDesignerHost host, PropertyDescriptor propDesc, ObjectPersistData persistData, PersistenceMode persistenceMode, TextWriter writer, string filter)
        {
            string name = propDesc.Name;
            bool flag = false;
            ICollection is2 = propDesc.GetValue(obj) as ICollection;
            int count = 0;
            if (is2 != null)
            {
                count = is2.Count;
            }
            int num2 = 0;
            ObjectPersistData objectPersistData = null;
            if (persistData != null)
            {
                ComplexPropertyEntry filteredProperty = persistData.GetFilteredProperty(string.Empty, name) as ComplexPropertyEntry;
                if (filteredProperty != null)
                {
                    objectPersistData = filteredProperty.Builder.GetObjectPersistData();
                    num2 = objectPersistData.CollectionItems.Count;
                }
            }
            if (filter.Length == 0)
            {
                flag = true;
            }
            else if (persistData != null)
            {
                if (persistData.GetFilteredProperty(filter, name) is ComplexPropertyEntry)
                {
                    flag = true;
                }
                else if (num2 != count)
                {
                    flag = true;
                }
                else if (objectPersistData != null)
                {
                    IEnumerator enumerator = is2.GetEnumerator();
                    IEnumerator enumerator2 = objectPersistData.CollectionItems.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        enumerator2.MoveNext();
                        ComplexPropertyEntry current = (ComplexPropertyEntry) enumerator2.Current;
                        if (enumerator.Current.GetType() != current.Builder.ControlType)
                        {
                            flag = true;
                            break;
                        }
                    }
                }
            }
            bool flag2 = false;
            ArrayList list = new ArrayList();
            if (count > 0)
            {
                StringWriter writer2 = new StringWriter(CultureInfo.InvariantCulture);
                IDictionary table = new Hashtable(ReferenceKeyComparer.Default);
                if (objectPersistData != null)
                {
                    foreach (ComplexPropertyEntry entry3 in objectPersistData.CollectionItems)
                    {
                        ObjectPersistData data2 = entry3.Builder.GetObjectPersistData();
                        if (data2 != null)
                        {
                            data2.AddToObjectControlBuilderTable(table);
                        }
                    }
                }
                if (!flag)
                {
                    flag2 = true;
                    foreach (object obj2 in is2)
                    {
                        string tagName = GetTagName(obj2.GetType(), host);
                        ObjectPersistData data3 = null;
                        ControlBuilder builder = (ControlBuilder) table[obj2];
                        if (builder != null)
                        {
                            data3 = builder.GetObjectPersistData();
                        }
                        writer2.Write('<');
                        writer2.Write(tagName);
                        SerializeAttributes(obj2, host, string.Empty, data3, writer2, filter);
                        writer2.Write('>');
                        SerializeInnerProperties(obj2, host, data3, writer2, filter);
                        writer2.Write("</");
                        writer2.Write(tagName);
                        writer2.WriteLine('>');
                    }
                    IDictionary z = GetExpandos(filter, name, objectPersistData);
                    list.Add(new Triplet(string.Empty, writer2, z));
                }
                else
                {
                    foreach (object obj3 in is2)
                    {
                        string str3 = GetTagName(obj3.GetType(), host);
                        if (obj3 is Control)
                        {
                            SerializeControl((Control) obj3, host, writer2, string.Empty);
                        }
                        else
                        {
                            writer2.Write('<');
                            writer2.Write(str3);
                            ObjectPersistData data4 = null;
                            ControlBuilder builder2 = (ControlBuilder) table[obj3];
                            if (builder2 != null)
                            {
                                data4 = builder2.GetObjectPersistData();
                            }
                            if ((filter.Length == 0) && (data4 != null))
                            {
                                SerializeAttributes(obj3, host, string.Empty, data4, writer2, string.Empty);
                                writer2.Write('>');
                                SerializeInnerProperties(obj3, host, data4, writer2, string.Empty);
                            }
                            else
                            {
                                SerializeAttributes(obj3, host, string.Empty, null, writer2, string.Empty);
                                writer2.Write('>');
                                SerializeInnerProperties(obj3, host, null, writer2, string.Empty);
                            }
                            writer2.Write("</");
                            writer2.Write(str3);
                            writer2.WriteLine('>');
                        }
                    }
                    IDictionary dictionary3 = GetExpandos(filter, name, persistData);
                    list.Add(new Triplet(filter, writer2, dictionary3));
                }
            }
            else if (num2 > 0)
            {
                IDictionary dictionary4 = GetExpandos(filter, name, persistData);
                list.Add(new Triplet(filter, new StringWriter(CultureInfo.InvariantCulture), dictionary4));
            }
            if (persistData != null)
            {
                foreach (ComplexPropertyEntry entry4 in persistData.GetPropertyAllFilters(name))
                {
                    StringWriter writer3 = new StringWriter(CultureInfo.InvariantCulture);
                    if ((string.Compare(entry4.Filter, filter, StringComparison.OrdinalIgnoreCase) != 0) && (!flag2 || (entry4.Filter.Length > 0)))
                    {
                        ObjectPersistData data5 = entry4.Builder.GetObjectPersistData();
                        data5.CollectionItems.GetEnumerator();
                        foreach (ComplexPropertyEntry entry5 in data5.CollectionItems)
                        {
                            object obj4 = entry5.Builder.BuildObject();
                            if (obj4 is Control)
                            {
                                SerializeControl((Control) obj4, host, writer3, string.Empty);
                            }
                            else
                            {
                                string str4 = GetTagName(obj4.GetType(), host);
                                ObjectPersistData data6 = entry5.Builder.GetObjectPersistData();
                                writer3.Write('<');
                                writer3.Write(str4);
                                SerializeAttributes(obj4, host, string.Empty, data6, writer3, string.Empty);
                                writer3.Write('>');
                                SerializeInnerProperties(obj4, host, data6, writer3, string.Empty);
                                writer3.Write("</");
                                writer3.Write(str4);
                                writer3.WriteLine('>');
                            }
                        }
                        IDictionary dictionary5 = GetExpandos(entry4.Filter, name, persistData);
                        list.Add(new Triplet(entry4.Filter, writer3, dictionary5));
                    }
                }
            }
            foreach (Triplet triplet in list)
            {
                string str5 = triplet.First.ToString();
                IDictionary third = (IDictionary) triplet.Third;
                if ((((list.Count == 1) && (str5.Length == 0)) && (persistenceMode != PersistenceMode.InnerProperty)) && ((third == null) || (third.Count == 0)))
                {
                    writer.Write(triplet.Second.ToString());
                }
                else
                {
                    string str6 = triplet.Second.ToString().Trim();
                    if (str6.Length > 0)
                    {
                        WriteInnerPropertyBeginTag(writer, str5, name, third, true);
                        writer.WriteLine(str6);
                        WriteInnerPropertyEndTag(writer, str5, name);
                    }
                }
            }
        }

        private static void SerializeComplexProperty(object obj, IDesignerHost host, PropertyDescriptor propDesc, ObjectPersistData persistData, TextWriter writer, string filter)
        {
            string name = propDesc.Name;
            object obj2 = propDesc.GetValue(obj);
            ObjectPersistData objectPersistData = null;
            if (persistData != null)
            {
                ComplexPropertyEntry filteredProperty = persistData.GetFilteredProperty(string.Empty, name) as ComplexPropertyEntry;
                if (filteredProperty != null)
                {
                    objectPersistData = filteredProperty.Builder.GetObjectPersistData();
                }
            }
            StringWriter writer2 = new StringWriter(CultureInfo.InvariantCulture);
            SerializeInnerProperties(obj2, host, objectPersistData, writer2, filter);
            string str2 = writer2.ToString();
            ArrayList list = SerializeAttributes(obj2, host, string.Empty, objectPersistData, filter, false);
            StringWriter writer3 = new StringWriter(CultureInfo.InvariantCulture);
            bool flag = true;
            foreach (Triplet triplet in list)
            {
                string str3 = triplet.First.ToString();
                if (str3 != ControlBuilder.DesignerFilter)
                {
                    flag = false;
                }
                WriteAttribute(writer3, str3, triplet.Second.ToString(), triplet.Third.ToString());
            }
            string str4 = string.Empty;
            if (!flag || (str2.Length > 0))
            {
                str4 = writer3.ToString();
            }
            if ((str4.Length + str2.Length) > 0)
            {
                writer.WriteLine();
                writer.Write('<');
                writer.Write(name);
                writer.Write(str4);
                writer.Write('>');
                writer.Write(str2);
                WriteInnerPropertyEndTag(writer, string.Empty, name);
            }
            if (persistData != null)
            {
                foreach (ComplexPropertyEntry entry2 in persistData.GetPropertyAllFilters(name))
                {
                    if (entry2.Filter.Length > 0)
                    {
                        object obj3 = entry2.Builder.BuildObject();
                        writer.WriteLine();
                        writer.Write('<');
                        writer.Write(entry2.Filter);
                        writer.Write(':');
                        writer.Write(name);
                        SerializeAttributes(obj3, host, string.Empty, null, writer, string.Empty);
                        writer.Write('>');
                        SerializeInnerProperties(obj3, host, null, writer, string.Empty);
                        WriteInnerPropertyEndTag(writer, entry2.Filter, name);
                    }
                }
            }
        }

        public static string SerializeControl(Control control)
        {
            StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
            SerializeControl(control, writer);
            return writer.ToString();
        }

        public static string SerializeControl(Control control, IDesignerHost host)
        {
            StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
            SerializeControl(control, host, writer);
            return writer.ToString();
        }

        public static void SerializeControl(Control control, TextWriter writer)
        {
            ISite site = control.Site;
            if (site == null)
            {
                IComponent page = control.Page;
                if (page != null)
                {
                    site = page.Site;
                }
            }
            IDesignerHost service = null;
            if (site != null)
            {
                service = (IDesignerHost) site.GetService(typeof(IDesignerHost));
            }
            SerializeControl(control, service, writer);
        }

        public static void SerializeControl(Control control, IDesignerHost host, TextWriter writer)
        {
            SerializeControl(control, host, writer, GetCurrentFilter(host));
        }

        private static void SerializeControl(Control control, IDesignerHost host, TextWriter writer, string filter)
        {
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            if (control is LiteralControl)
            {
                writer.Write(((LiteralControl) control).Text);
            }
            else if (control is DesignerDataBoundLiteralControl)
            {
                DataBinding binding = ((IDataBindingsAccessor) control).DataBindings["Text"];
                if (binding != null)
                {
                    writer.Write("<%# ");
                    writer.Write(binding.Expression);
                    writer.Write(" %>");
                }
            }
            else if (control is UserControl)
            {
                IUserControlDesignerAccessor accessor = (IUserControlDesignerAccessor) control;
                string tagName = accessor.TagName;
                if (tagName.Length > 0)
                {
                    writer.Write('<');
                    writer.Write(tagName);
                    writer.Write(" runat=\"server\"");
                    ObjectPersistData persistData = null;
                    IControlBuilderAccessor accessor2 = control;
                    if (accessor2.ControlBuilder != null)
                    {
                        persistData = accessor2.ControlBuilder.GetObjectPersistData();
                    }
                    SerializeAttributes(control, host, string.Empty, persistData, writer, filter);
                    writer.Write('>');
                    string innerText = accessor.InnerText;
                    if ((innerText != null) && (innerText.Length > 0))
                    {
                        writer.Write(accessor.InnerText);
                    }
                    writer.Write("</");
                    writer.Write(tagName);
                    writer.WriteLine('>');
                }
            }
            else
            {
                string str3;
                HtmlControl control2 = control as HtmlControl;
                if (control2 != null)
                {
                    str3 = control2.TagName;
                }
                else
                {
                    str3 = GetTagName(control.GetType(), host);
                }
                writer.Write('<');
                writer.Write(str3);
                writer.Write(" runat=\"server\"");
                ObjectPersistData objectPersistData = null;
                IControlBuilderAccessor accessor3 = control;
                if (accessor3.ControlBuilder != null)
                {
                    objectPersistData = accessor3.ControlBuilder.GetObjectPersistData();
                }
                SerializeAttributes(control, host, string.Empty, objectPersistData, writer, filter);
                writer.Write('>');
                SerializeInnerContents(control, host, objectPersistData, writer, filter);
                writer.Write("</");
                writer.Write(str3);
                writer.WriteLine('>');
            }
        }

        public static string SerializeInnerContents(Control control, IDesignerHost host)
        {
            StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
            ObjectPersistData persistData = null;
            IControlBuilderAccessor accessor = control;
            if (accessor.ControlBuilder != null)
            {
                persistData = accessor.ControlBuilder.GetObjectPersistData();
            }
            SerializeInnerContents(control, host, persistData, writer, GetCurrentFilter(host));
            return writer.ToString();
        }

        internal static void SerializeInnerContents(Control control, IDesignerHost host, ObjectPersistData persistData, TextWriter writer, string filter)
        {
            PersistChildrenAttribute attribute = (PersistChildrenAttribute) TypeDescriptor.GetAttributes(control)[typeof(PersistChildrenAttribute)];
            ParseChildrenAttribute attribute2 = (ParseChildrenAttribute) TypeDescriptor.GetAttributes(control)[typeof(ParseChildrenAttribute)];
            if (attribute.Persist || (!attribute2.ChildrenAsProperties && control.HasControls()))
            {
                for (int i = 0; i < control.Controls.Count; i++)
                {
                    SerializeControl(control.Controls[i], host, writer, string.Empty);
                }
            }
            else
            {
                SerializeInnerProperties(control, host, persistData, writer, filter);
            }
        }

        public static string SerializeInnerProperties(object obj, IDesignerHost host)
        {
            StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
            SerializeInnerProperties(obj, host, writer);
            return writer.ToString();
        }

        internal static void SerializeInnerProperties(object obj, IDesignerHost host, TextWriter writer)
        {
            ObjectPersistData persistData = null;
            IControlBuilderAccessor accessor = (IControlBuilderAccessor) obj;
            if (accessor.ControlBuilder != null)
            {
                persistData = accessor.ControlBuilder.GetObjectPersistData();
            }
            SerializeInnerProperties(obj, host, persistData, writer, GetCurrentFilter(host));
        }

        private static void SerializeInnerProperties(object obj, IDesignerHost host, ObjectPersistData persistData, TextWriter writer, string filter)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(obj);
            if (obj is Control)
            {
                try
                {
                    ControlCollection controls = ((Control) obj).Controls;
                }
                catch (Exception exception)
                {
                    IComponentDesignerDebugService service = host.GetService(typeof(IComponentDesignerDebugService)) as IComponentDesignerDebugService;
                    if (service != null)
                    {
                        service.Fail(exception.Message);
                    }
                }
            }
            for (int i = 0; i < properties.Count; i++)
            {
                try
                {
                    if (!FilterableAttribute.IsPropertyFilterable(properties[i]))
                    {
                    }
                    if (properties[i].SerializationVisibility != DesignerSerializationVisibility.Hidden)
                    {
                        PersistenceModeAttribute attribute = (PersistenceModeAttribute) properties[i].Attributes[typeof(PersistenceModeAttribute)];
                        if (attribute.Mode != PersistenceMode.Attribute)
                        {
                            DesignOnlyAttribute attribute2 = (DesignOnlyAttribute) properties[i].Attributes[typeof(DesignOnlyAttribute)];
                            if ((attribute2 == null) || !attribute2.IsDesignOnly)
                            {
                                string name = properties[i].Name;
                                if (properties[i].PropertyType == typeof(string))
                                {
                                    SerializeStringProperty(obj, host, properties[i], persistData, attribute.Mode, writer, filter);
                                }
                                else if (typeof(ICollection).IsAssignableFrom(properties[i].PropertyType))
                                {
                                    SerializeCollectionProperty(obj, host, properties[i], persistData, attribute.Mode, writer, filter);
                                }
                                else if (typeof(ITemplate).IsAssignableFrom(properties[i].PropertyType))
                                {
                                    SerializeTemplateProperty(obj, host, properties[i], persistData, writer, filter);
                                }
                                else
                                {
                                    SerializeComplexProperty(obj, host, properties[i], persistData, writer, filter);
                                }
                            }
                        }
                    }
                }
                catch (Exception exception2)
                {
                    if (host != null)
                    {
                        IComponentDesignerDebugService service2 = host.GetService(typeof(IComponentDesignerDebugService)) as IComponentDesignerDebugService;
                        if (service2 != null)
                        {
                            service2.Fail(exception2.Message);
                        }
                    }
                }
            }
        }

        private static void SerializeStringProperty(object obj, IDesignerHost host, PropertyDescriptor propDesc, ObjectPersistData persistData, PersistenceMode persistenceMode, TextWriter writer, string filter)
        {
            string name = propDesc.Name;
            DataBindingCollection dataBindings = null;
            if (obj is IDataBindingsAccessor)
            {
                dataBindings = ((IDataBindingsAccessor) obj).DataBindings;
            }
            ExpressionBindingCollection expressions = null;
            if (obj is IExpressionsAccessor)
            {
                expressions = ((IExpressionsAccessor) obj).Expressions;
            }
            if ((persistenceMode == PersistenceMode.InnerProperty) || CanSerializeAsInnerDefaultString(filter, name, propDesc.PropertyType, persistData, persistenceMode, dataBindings, expressions))
            {
                ArrayList list = new ArrayList();
                if (((dataBindings == null) || (dataBindings[name] == null)) || ((expressions == null) || (expressions[name] == null)))
                {
                    string y = string.Empty;
                    object objA = propDesc.GetValue(obj);
                    if (objA != null)
                    {
                        y = objA.ToString();
                    }
                    bool flag = true;
                    if (filter.Length == 0)
                    {
                        bool flag2;
                        bool flag3 = GetShouldSerializeValue(obj, name, out flag2);
                        if (flag2)
                        {
                            flag = flag3;
                        }
                        else
                        {
                            object objB = GetPropertyDefaultValue(propDesc, name, persistData, filter, host);
                            flag = !object.Equals(objA, objB);
                        }
                    }
                    else
                    {
                        object obj4 = GetPropertyDefaultValue(propDesc, name, persistData, filter, host);
                        flag = !object.Equals(objA, obj4);
                    }
                    if (flag)
                    {
                        IDictionary z = GetExpandos(filter, name, persistData);
                        list.Add(new Triplet(filter, y, z));
                    }
                }
                if (persistData != null)
                {
                    foreach (PropertyEntry entry in persistData.GetPropertyAllFilters(name))
                    {
                        if ((string.Compare(entry.Filter, filter, StringComparison.OrdinalIgnoreCase) != 0) && (entry is ComplexPropertyEntry))
                        {
                            ComplexPropertyEntry entry2 = (ComplexPropertyEntry) entry;
                            string str3 = entry2.Builder.BuildObject().ToString();
                            IDictionary dictionary2 = GetExpandos(entry.Filter, name, persistData);
                            list.Add(new Triplet(entry.Filter, str3, dictionary2));
                        }
                    }
                }
                foreach (Triplet triplet in list)
                {
                    bool flag4 = false;
                    IDictionary third = triplet.Third as IDictionary;
                    if (((list.Count == 1) && (triplet.First.ToString().Length == 0)) && ((third == null) || (third.Count == 0)))
                    {
                        if (persistenceMode == PersistenceMode.InnerDefaultProperty)
                        {
                            writer.Write(triplet.Second.ToString());
                            flag4 = true;
                        }
                        else if (persistenceMode == PersistenceMode.EncodedInnerDefaultProperty)
                        {
                            HttpUtility.HtmlEncode(triplet.Second.ToString(), writer);
                            flag4 = true;
                        }
                    }
                    if (!flag4)
                    {
                        string str4 = triplet.First.ToString();
                        WriteInnerPropertyBeginTag(writer, str4, name, third, true);
                        writer.Write(triplet.Second.ToString());
                        WriteInnerPropertyEndTag(writer, str4, name);
                    }
                }
            }
        }

        public static string SerializeTemplate(ITemplate template, IDesignerHost host)
        {
            StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
            SerializeTemplate(template, writer, host);
            return writer.ToString();
        }

        public static void SerializeTemplate(ITemplate template, TextWriter writer, IDesignerHost host)
        {
            if (template != null)
            {
                if (writer == null)
                {
                    throw new ArgumentNullException("writer");
                }
                if (template is TemplateBuilder)
                {
                    writer.Write(((TemplateBuilder) template).Text);
                }
                else
                {
                    Control container = new Control();
                    StringBuilder builder = new StringBuilder();
                    try
                    {
                        template.InstantiateIn(container);
                        foreach (Control control2 in container.Controls)
                        {
                            builder.Append(SerializeControl(control2, host));
                        }
                        writer.Write(builder.ToString());
                    }
                    catch (Exception)
                    {
                    }
                }
                writer.Flush();
            }
        }

        private static void SerializeTemplateProperty(object obj, IDesignerHost host, PropertyDescriptor propDesc, ObjectPersistData persistData, TextWriter writer, string filter)
        {
            string name = propDesc.Name;
            string b = string.Empty;
            ITemplate template = (ITemplate) propDesc.GetValue(obj);
            if (template != null)
            {
                b = SerializeTemplate(template, host);
                string a = string.Empty;
                if ((filter.Length > 0) && (persistData != null))
                {
                    TemplatePropertyEntry filteredProperty = persistData.GetFilteredProperty(string.Empty, name) as TemplatePropertyEntry;
                    if (filteredProperty != null)
                    {
                        a = SerializeTemplate(filteredProperty.Builder as ITemplate, host);
                    }
                }
                IDictionary expandos = GetExpandos(filter, name, persistData);
                if ((((template != null) && (expandos != null)) && (expandos.Count > 0)) || !string.Equals(a, b))
                {
                    WriteInnerPropertyBeginTag(writer, filter, name, expandos, false);
                    if ((b.Length > 0) && !b.StartsWith("\r\n", StringComparison.Ordinal))
                    {
                        writer.WriteLine();
                    }
                    writer.Write(b);
                    if ((b.Length > 0) && !b.EndsWith("\r\n", StringComparison.Ordinal))
                    {
                        writer.WriteLine();
                    }
                    WriteInnerPropertyEndTag(writer, filter, name);
                }
            }
            if (persistData != null)
            {
                foreach (TemplatePropertyEntry entry2 in persistData.GetPropertyAllFilters(name))
                {
                    if (string.Compare(entry2.Filter, filter, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        IDictionary dictionary2 = GetExpandos(entry2.Filter, name, persistData);
                        WriteInnerPropertyBeginTag(writer, entry2.Filter, name, dictionary2, false);
                        string str4 = SerializeTemplate((ITemplate) entry2.Builder, host);
                        if (str4 != null)
                        {
                            if (!str4.StartsWith("\r\n", StringComparison.Ordinal))
                            {
                                writer.WriteLine();
                            }
                            writer.Write(str4);
                            if (!str4.EndsWith("\r\n", StringComparison.Ordinal))
                            {
                                writer.WriteLine();
                            }
                            WriteInnerPropertyEndTag(writer, entry2.Filter, name);
                        }
                    }
                }
            }
        }

        private static bool ShouldPersistBlankValue(object defValue, Type type)
        {
            if (type == typeof(string))
            {
                return !defValue.Equals("");
            }
            if (type == typeof(Color))
            {
                Color color = (Color) defValue;
                return !color.IsEmpty;
            }
            if (type == typeof(FontUnit))
            {
                FontUnit unit = (FontUnit) defValue;
                return !unit.IsEmpty;
            }
            return ((type == typeof(Unit)) && !defValue.Equals(Unit.Empty));
        }

        private static void WriteAttribute(TextWriter writer, string filter, string name, string value)
        {
            writer.Write(" ");
            if ((filter != null) && (filter.Length > 0))
            {
                writer.Write(filter);
                writer.Write(':');
            }
            writer.Write(name);
            if (value.IndexOf('"') > -1)
            {
                writer.Write("='");
                writer.Write(value);
                writer.Write("'");
            }
            else
            {
                writer.Write("=\"");
                writer.Write(value);
                writer.Write("\"");
            }
        }

        private static void WriteInnerPropertyBeginTag(TextWriter writer, string filter, string name, IDictionary expandos, bool requiresNewLine)
        {
            writer.Write('<');
            if ((filter != null) && (filter.Length > 0))
            {
                writer.Write(filter);
                writer.Write(':');
            }
            writer.Write(name);
            if (expandos != null)
            {
                foreach (DictionaryEntry entry in expandos)
                {
                    SimplePropertyEntry entry2 = entry.Value as SimplePropertyEntry;
                    if (entry2 != null)
                    {
                        WriteAttribute(writer, ControlBuilder.DesignerFilter, entry.Key.ToString(), entry2.Value.ToString());
                    }
                }
            }
            if (requiresNewLine)
            {
                writer.WriteLine('>');
            }
            else
            {
                writer.Write('>');
            }
        }

        private static void WriteInnerPropertyEndTag(TextWriter writer, string filter, string name)
        {
            writer.Write("</");
            if ((filter != null) && (filter.Length > 0))
            {
                writer.Write(filter);
                writer.Write(':');
            }
            writer.Write(name);
            writer.WriteLine('>');
        }

        private enum BindingType
        {
            None,
            Data,
            Expression
        }

        private class ReferenceKeyComparer : IEqualityComparer
        {
            internal static readonly ControlSerializer.ReferenceKeyComparer Default = new ControlSerializer.ReferenceKeyComparer();

            bool IEqualityComparer.Equals(object x, object y)
            {
                return object.ReferenceEquals(x, y);
            }

            int IEqualityComparer.GetHashCode(object obj)
            {
                return obj.GetHashCode();
            }
        }

        private sealed class WebFormsDesigntimeLicenseContext : DesigntimeLicenseContext
        {
            private IServiceProvider provider;

            public WebFormsDesigntimeLicenseContext(IServiceProvider provider)
            {
                this.provider = provider;
            }

            public override object GetService(Type serviceClass)
            {
                if (this.provider != null)
                {
                    return this.provider.GetService(serviceClass);
                }
                return null;
            }
        }
    }
}

