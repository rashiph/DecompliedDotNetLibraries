namespace System.Web.UI
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Globalization;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;
    using System.Web.Util;

    public class ControlBuilder
    {
        private PropertyDescriptor _bindingContainerDescriptor;
        private ArrayList _boundPropertyEntries;
        private ArrayList _complexPropertyEntries;
        private Type _controlType;
        private ArrayList _eventEntries;
        private ControlBuilderParseTimeData _parseTimeData;
        private IServiceProvider _serviceProvider;
        private ArrayList _simplePropertyEntries;
        private string _skinID;
        private ArrayList _subBuilders;
        private string _tagName;
        private ArrayList _templatePropertyEntries;
        private const int applyTheme = 0x8000;
        private static readonly Regex bindExpressionRegex = new BindExpressionRegex();
        private static readonly Regex bindParametersRegex = new BindParametersRegex();
        private const int controlTypeIsControl = 0x2000;
        private static readonly Regex databindRegex = new DataBindRegex();
        public static readonly string DesignerFilter = "__designer";
        private const int doneInitObjectOptimizations = 8;
        private const int entriesSorted = 0x4000;
        private static readonly Regex evalExpressionRegex = new EvalExpressionRegex();
        internal static readonly Regex expressionBuilderRegex = new ExpressionBuilderRegex();
        private SimpleBitVector32 flags;
        private static readonly Regex formatStringRegex = new FormatStringRegex();
        private const int hasFieldToControlBinding = 0x1000;
        private const int hasFilteredBoundProps = 0x200;
        private const int hasFilteredComplexProps = 0x80;
        private const int hasFilteredSimpleProps = 0x40;
        private const int hasFilteredTemplateProps = 0x100;
        private const int hasTwoWayBoundProps = 0x400;
        private const int isICollection = 0x10;
        private const int isIParserAccessor = 0x20;
        private const int needsTagAttribute = 4;
        private const int needsTagAttributeComputed = 2;
        private const int parseComplete = 1;
        private static Hashtable s_controlBuilderFactoryCache;
        private static FactoryGenerator s_controlBuilderFactoryGenerator;
        private static IWebObjectFactory s_defaultControlBuilderFactory = new DefaultControlBuilderFactory();
        private static ParseChildrenAttribute s_markerParseChildrenAttribute = new ParseChildrenAttribute();
        private static Hashtable s_parseChildrenAttributeCache = new Hashtable();
        private const int triedFieldToControlBinding = 0x800;

        private void AddBoundProperty(BoundPropertyEntry entry)
        {
            this.AddEntry(this.BoundPropertyEntriesInternal, entry);
            if (entry.TwoWayBound)
            {
                this.flags[0x400] = true;
            }
        }

        private void AddBoundProperty(string filter, string name, string expressionPrefix, string expression, ExpressionBuilder expressionBuilder, object parsedExpressionData, string fieldName, string formatString, bool twoWayBound)
        {
            this.AddBoundProperty(filter, name, expressionPrefix, expression, expressionBuilder, parsedExpressionData, false, fieldName, formatString, twoWayBound);
        }

        private void AddBoundProperty(string filter, string name, string expressionPrefix, string expression, ExpressionBuilder expressionBuilder, object parsedExpressionData, bool generated, string fieldName, string formatString, bool twoWayBound)
        {
            string iD = this.ParseTimeData.ID;
            IDesignerHost designerHost = this.DesignerHost;
            if (string.IsNullOrEmpty(expressionPrefix))
            {
                if (string.IsNullOrEmpty(iD))
                {
                    if (this.CompilationMode == System.Web.UI.CompilationMode.Never)
                    {
                        throw new HttpException(System.Web.SR.GetString("NoCompileBinding_requires_ID", new object[] { this._controlType.Name, fieldName }));
                    }
                    if (twoWayBound)
                    {
                        throw new HttpException(System.Web.SR.GetString("TwoWayBinding_requires_ID", new object[] { this._controlType.Name, fieldName }));
                    }
                }
                if (!this.flags[0x2000] && (TargetFrameworkUtil.GetEvent(this.ControlType, "DataBinding") == null))
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("ControlBuilder_DatabindingRequiresEvent", new object[] { this._controlType.FullName }));
                }
            }
            else if (expressionBuilder == null)
            {
                expressionBuilder = ExpressionBuilder.GetExpressionBuilder(expressionPrefix, this.VirtualPath, designerHost);
            }
            BoundPropertyEntry entry = new BoundPropertyEntry {
                Filter = filter,
                Expression = expression,
                ExpressionBuilder = expressionBuilder,
                ExpressionPrefix = expressionPrefix,
                Generated = generated,
                FieldName = fieldName,
                FormatString = formatString,
                ControlType = this._controlType,
                ControlID = iD,
                TwoWayBound = twoWayBound,
                ParsedExpressionData = parsedExpressionData
            };
            this.FillUpBoundPropertyEntry(entry, name);
            foreach (BoundPropertyEntry entry2 in this.BoundPropertyEntriesInternal)
            {
                if (string.Equals(entry2.Name, entry.Name, StringComparison.OrdinalIgnoreCase) && string.Equals(entry2.Filter, entry.Filter, StringComparison.OrdinalIgnoreCase))
                {
                    string str2 = entry.Name;
                    if (!string.IsNullOrEmpty(entry.Filter))
                    {
                        str2 = entry.Filter + ":" + str2;
                    }
                    throw new InvalidOperationException(System.Web.SR.GetString("ControlBuilder_CannotHaveMultipleBoundEntries", new object[] { str2, this.ControlType }));
                }
            }
            this.AddBoundProperty(entry);
        }

        private void AddCollectionItem(ControlBuilder builder)
        {
            ComplexPropertyEntry entry = new ComplexPropertyEntry(true) {
                Builder = builder,
                Filter = string.Empty
            };
            this.AddEntry(this.ComplexPropertyEntriesInternal, entry);
        }

        private void AddComplexProperty(string filter, string name, ControlBuilder builder)
        {
            string nameForCodeGen = string.Empty;
            MemberInfo info = PropertyMapper.GetMemberInfo(this._controlType, name, out nameForCodeGen);
            ComplexPropertyEntry entry = new ComplexPropertyEntry {
                Builder = builder,
                Filter = filter,
                Name = nameForCodeGen
            };
            Type propertyType = null;
            if (info == null)
            {
                throw new HttpException(System.Web.SR.GetString("Type_doesnt_have_property", new object[] { this._controlType.FullName, name }));
            }
            if (info is PropertyInfo)
            {
                PropertyInfo propInfo = (PropertyInfo) info;
                entry.PropertyInfo = propInfo;
                if (propInfo.GetSetMethod() == null)
                {
                    entry.ReadOnly = true;
                }
                this.ValidatePersistable(propInfo, false, false, false, filter);
                propertyType = propInfo.PropertyType;
            }
            else
            {
                propertyType = ((FieldInfo) info).FieldType;
            }
            entry.Type = propertyType;
            this.AddEntry(this.ComplexPropertyEntriesInternal, entry);
        }

        private void AddEntry(ArrayList entries, PropertyEntry entry)
        {
            if ((string.Equals(entry.Name, "ID", StringComparison.OrdinalIgnoreCase) && this.flags[0x2000]) && !(entry is SimplePropertyEntry))
            {
                throw new HttpException(System.Web.SR.GetString("ControlBuilder_IDMustUseAttribute"));
            }
            entry.Index = entries.Count;
            entries.Add(entry);
        }

        private void AddProperty(string filter, string name, string value, bool mainDirectiveMode)
        {
            if (!this.IgnoreControlProperty)
            {
                string nameForCodeGen = string.Empty;
                MemberInfo propertyInfo = null;
                if (this._controlType != null)
                {
                    if (string.Equals(name, "SkinID", StringComparison.OrdinalIgnoreCase) && this.flags[0x2000])
                    {
                        if (!string.IsNullOrEmpty(filter))
                        {
                            throw new InvalidOperationException(System.Web.SR.GetString("Illegal_Device", new object[] { "SkinID" }));
                        }
                        this.SkinID = value;
                        return;
                    }
                    propertyInfo = PropertyMapper.GetMemberInfo(this._controlType, name, out nameForCodeGen);
                }
                if (propertyInfo != null)
                {
                    SimplePropertyEntry entry = new SimplePropertyEntry {
                        Filter = filter,
                        Name = nameForCodeGen,
                        PersistedValue = value
                    };
                    Type objType = null;
                    if (propertyInfo is PropertyInfo)
                    {
                        PropertyInfo propInfo = (PropertyInfo) propertyInfo;
                        entry.PropertyInfo = propInfo;
                        if (propInfo.GetSetMethod() == null)
                        {
                            if (!this.SupportsAttributes)
                            {
                                throw new HttpException(System.Web.SR.GetString("Property_readonly", new object[] { name }));
                            }
                            entry.UseSetAttribute = true;
                            entry.Name = name;
                        }
                        this.ValidatePersistable(propInfo, entry.UseSetAttribute, mainDirectiveMode, true, filter);
                        objType = propInfo.PropertyType;
                    }
                    else
                    {
                        objType = ((FieldInfo) propertyInfo).FieldType;
                    }
                    entry.Type = objType;
                    if (entry.UseSetAttribute)
                    {
                        entry.Value = value;
                    }
                    else
                    {
                        object obj2 = PropertyConverter.ObjectFromString(objType, propertyInfo, value);
                        DesignTimePageThemeParser parser = this.Parser as DesignTimePageThemeParser;
                        if ((parser != null) && (propertyInfo.GetCustomAttributes(typeof(UrlPropertyAttribute), true).Length > 0))
                        {
                            string virtualPath = obj2.ToString();
                            if (UrlPath.IsRelativeUrl(virtualPath) && !UrlPath.IsAppRelativePath(virtualPath))
                            {
                                obj2 = parser.ThemePhysicalPath + virtualPath;
                            }
                        }
                        entry.Value = obj2;
                        if (objType.IsEnum)
                        {
                            if (obj2 == null)
                            {
                                throw new HttpException(System.Web.SR.GetString("Invalid_enum_value", new object[] { value, name, entry.Type.FullName }));
                            }
                            entry.PersistedValue = Enum.Format(objType, obj2, "G");
                        }
                        else if ((objType == typeof(bool)) && (obj2 == null))
                        {
                            entry.Value = true;
                        }
                    }
                    this.AddEntry(this.SimplePropertyEntriesInternal, entry);
                }
                else
                {
                    bool flag = false;
                    if (StringUtil.StringStartsWithIgnoreCase(name, "on"))
                    {
                        string str3 = name.Substring(2);
                        EventDescriptor descriptor = this.EventDescriptors.Find(str3, true);
                        if (descriptor != null)
                        {
                            if (this.InPageTheme)
                            {
                                throw new HttpException(System.Web.SR.GetString("Property_theme_disabled", new object[] { str3, this.ControlType.FullName }));
                            }
                            if (value != null)
                            {
                                value = value.Trim();
                            }
                            if (string.IsNullOrEmpty(value))
                            {
                                throw new HttpException(System.Web.SR.GetString("Event_handler_cant_be_empty", new object[] { name }));
                            }
                            if (filter.Length > 0)
                            {
                                throw new HttpException(System.Web.SR.GetString("Events_cant_be_filtered", new object[] { filter, name }));
                            }
                            flag = true;
                            if (!this.Parser.PageParserFilterProcessedEventHookupAttribute(this.ID, descriptor.Name, value))
                            {
                                this.Parser.OnFoundEventHandler(name);
                                EventEntry entry2 = new EventEntry {
                                    Name = descriptor.Name,
                                    HandlerType = descriptor.EventType,
                                    HandlerMethodName = value
                                };
                                this.EventEntriesInternal.Add(entry2);
                            }
                        }
                    }
                    if (!flag)
                    {
                        if (!this.SupportsAttributes && (filter != DesignerFilter))
                        {
                            if (this._controlType != null)
                            {
                                throw new HttpException(System.Web.SR.GetString("Type_doesnt_have_property", new object[] { this._controlType.FullName, name }));
                            }
                            throw new HttpException(System.Web.SR.GetString("Property_doesnt_have_property", new object[] { this.TagName, name }));
                        }
                        SimplePropertyEntry entry3 = new SimplePropertyEntry {
                            Filter = filter,
                            Name = name,
                            PersistedValue = value,
                            UseSetAttribute = true,
                            Value = value
                        };
                        this.AddEntry(this.SimplePropertyEntriesInternal, entry3);
                    }
                }
            }
        }

        internal void AddSubBuilder(object o)
        {
            this.SubBuilders.Add(o);
        }

        private void AddTemplateProperty(string filter, string name, TemplateBuilder builder)
        {
            string nameForCodeGen = string.Empty;
            MemberInfo info = PropertyMapper.GetMemberInfo(this._controlType, name, out nameForCodeGen);
            bool bindableTemplate = builder is BindableTemplateBuilder;
            TemplatePropertyEntry entry = new TemplatePropertyEntry(bindableTemplate) {
                Builder = builder,
                Filter = filter,
                Name = nameForCodeGen
            };
            Type fieldType = null;
            if (info == null)
            {
                throw new HttpException(System.Web.SR.GetString("Type_doesnt_have_property", new object[] { this._controlType.FullName, name }));
            }
            if (info is PropertyInfo)
            {
                PropertyInfo propInfo = (PropertyInfo) info;
                entry.PropertyInfo = propInfo;
                this.ValidatePersistable(propInfo, false, false, false, filter);
                TemplateContainerAttribute attribute = (TemplateContainerAttribute) Attribute.GetCustomAttribute(propInfo, typeof(TemplateContainerAttribute), false);
                if (attribute != null)
                {
                    if (!typeof(INamingContainer).IsAssignableFrom(attribute.ContainerType))
                    {
                        throw new HttpException(System.Web.SR.GetString("Invalid_template_container", new object[] { name, attribute.ContainerType.FullName }));
                    }
                    builder.SetControlType(attribute.ContainerType);
                }
                entry.Type = propInfo.PropertyType;
            }
            else
            {
                fieldType = ((FieldInfo) info).FieldType;
            }
            entry.Type = fieldType;
            this.AddEntry(this.TemplatePropertyEntriesInternal, entry);
        }

        public virtual bool AllowWhitespaceLiterals()
        {
            return true;
        }

        public virtual void AppendLiteralString(string s)
        {
            if (s != null)
            {
                if (this.FIsNonParserAccessor || this.FChildrenAsProperties)
                {
                    if (this.DefaultPropertyBuilder != null)
                    {
                        this.DefaultPropertyBuilder.AppendLiteralString(s);
                    }
                    else
                    {
                        s = s.Trim();
                        if (this.FChildrenAsProperties && s.StartsWith("<", StringComparison.OrdinalIgnoreCase))
                        {
                            throw new HttpException(System.Web.SR.GetString("Literal_content_not_match_property", new object[] { this._controlType.FullName, s }));
                        }
                        if (s.Length != 0)
                        {
                            throw new HttpException(System.Web.SR.GetString("Literal_content_not_allowed", new object[] { this._controlType.FullName, s }));
                        }
                    }
                }
                else if (this.AllowWhitespaceLiterals() || !System.Web.UI.Util.IsWhiteSpaceString(s))
                {
                    if (this.HtmlDecodeLiterals())
                    {
                        s = HttpUtility.HtmlDecode(s);
                    }
                    DataBoundLiteralControlBuilder lastBuilder = this.GetLastBuilder() as DataBoundLiteralControlBuilder;
                    if (lastBuilder != null)
                    {
                        lastBuilder.AddLiteralString(s);
                    }
                    else
                    {
                        this.AddSubBuilder(s);
                    }
                }
            }
        }

        public virtual void AppendSubBuilder(ControlBuilder subBuilder)
        {
            subBuilder.OnAppendToParentBuilder(this);
            if (this.FChildrenAsProperties)
            {
                if (subBuilder is CodeBlockBuilder)
                {
                    throw new HttpException(System.Web.SR.GetString("Code_not_supported_on_not_controls"));
                }
                if (this.DefaultPropertyBuilder != null)
                {
                    this.DefaultPropertyBuilder.AppendSubBuilder(subBuilder);
                }
                else
                {
                    string tagName = subBuilder.TagName;
                    if (subBuilder is TemplateBuilder)
                    {
                        TemplateBuilder builder = (TemplateBuilder) subBuilder;
                        this.AddTemplateProperty(builder.Filter, tagName, builder);
                    }
                    else if (subBuilder is CollectionBuilder)
                    {
                        if ((subBuilder.SubBuilders != null) && (subBuilder.SubBuilders.Count > 0))
                        {
                            IEnumerator enumerator = subBuilder.SubBuilders.GetEnumerator();
                            while (enumerator.MoveNext())
                            {
                                ControlBuilder current = (ControlBuilder) enumerator.Current;
                                subBuilder.AddCollectionItem(current);
                            }
                            subBuilder.SubBuilders.Clear();
                            this.AddComplexProperty(subBuilder.Filter, tagName, subBuilder);
                        }
                    }
                    else if (subBuilder is StringPropertyBuilder)
                    {
                        if (!string.IsNullOrEmpty(((StringPropertyBuilder) subBuilder).Text.Trim()))
                        {
                            this.AddComplexProperty(subBuilder.Filter, tagName, subBuilder);
                        }
                    }
                    else
                    {
                        this.AddComplexProperty(subBuilder.Filter, tagName, subBuilder);
                    }
                }
            }
            else
            {
                CodeBlockBuilder codeBlockBuilder = subBuilder as CodeBlockBuilder;
                if (codeBlockBuilder != null)
                {
                    if ((this.ControlType != null) && !this.flags[0x2000])
                    {
                        throw new HttpException(System.Web.SR.GetString("Code_not_supported_on_not_controls"));
                    }
                    if (codeBlockBuilder.BlockType == CodeBlockType.DataBinding)
                    {
                        if (bindExpressionRegex.Match(codeBlockBuilder.Content, 0).Success)
                        {
                            ControlBuilder parentBuilder = this;
                            while ((parentBuilder != null) && !(parentBuilder is TemplateBuilder))
                            {
                                parentBuilder = parentBuilder.ParentBuilder;
                            }
                            if (((parentBuilder != null) && (parentBuilder.ParentBuilder != null)) && (parentBuilder is TemplateBuilder))
                            {
                                throw new HttpException(System.Web.SR.GetString("DataBoundLiterals_cant_bind"));
                            }
                        }
                        if (this.InDesigner)
                        {
                            IDictionary attribs = new ParsedAttributeCollection();
                            attribs.Add("Text", "<%#" + codeBlockBuilder.Content + "%>");
                            subBuilder = CreateBuilderFromType(this.Parser, this, typeof(DesignerDataBoundLiteralControl), null, null, attribs, codeBlockBuilder.Line, codeBlockBuilder.PageVirtualPath);
                        }
                        else
                        {
                            object lastBuilder = this.GetLastBuilder();
                            DataBoundLiteralControlBuilder builder5 = lastBuilder as DataBoundLiteralControlBuilder;
                            bool flag = false;
                            if (builder5 == null)
                            {
                                builder5 = new DataBoundLiteralControlBuilder();
                                builder5.Init(this.Parser, this, typeof(DataBoundLiteralControl), null, null, null);
                                builder5.Line = codeBlockBuilder.Line;
                                builder5.VirtualPath = codeBlockBuilder.VirtualPath;
                                flag = true;
                                string s = lastBuilder as string;
                                if (s != null)
                                {
                                    this.SubBuilders.RemoveAt(this.SubBuilders.Count - 1);
                                    builder5.AddLiteralString(s);
                                }
                            }
                            builder5.AddDataBindingExpression(codeBlockBuilder);
                            if (!flag)
                            {
                                return;
                            }
                            subBuilder = builder5;
                        }
                    }
                    else
                    {
                        this.ParseTimeData.HasAspCode = true;
                    }
                }
                if (this.FIsNonParserAccessor)
                {
                    throw new HttpException(System.Web.SR.GetString("Children_not_supported_on_not_controls"));
                }
                this.AddSubBuilder(subBuilder);
            }
        }

        private void AttachTypeDescriptionProvider(object obj)
        {
            if ((this.InDesigner && (obj != null)) && (this._serviceProvider != null))
            {
                TypeDescriptionProviderService service = this._serviceProvider.GetService(typeof(TypeDescriptionProviderService)) as TypeDescriptionProviderService;
                if (service != null)
                {
                    TypeDescriptor.AddProvider(service.GetProvider(obj), obj);
                }
            }
        }

        private void BindFieldToControl(Control control)
        {
            if (!this.flags[0x800] || this.flags[0x1000])
            {
                this.flags[0x800] = true;
                System.Web.UI.TemplateControl templateControl = this.TemplateControl;
                if (templateControl != null)
                {
                    Type type = this.TemplateControl.GetType();
                    if (!this.flags[0x1000])
                    {
                        if (this.InDesigner)
                        {
                            return;
                        }
                        if (control.ID == null)
                        {
                            return;
                        }
                        if (type.Assembly == typeof(HttpRuntime).Assembly)
                        {
                            return;
                        }
                    }
                    FieldInfo info = TargetFrameworkUtil.GetField(templateControl.GetType(), control.ID, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (((info != null) && !info.IsPrivate) && info.FieldType.IsAssignableFrom(control.GetType()))
                    {
                        info.SetValue(templateControl, control);
                        this.flags[0x1000] = true;
                    }
                }
            }
        }

        internal virtual void BuildChildren(object parentObj)
        {
            if (this._subBuilders != null)
            {
                IEnumerator enumerator = this._subBuilders.GetEnumerator();
                for (int i = 0; enumerator.MoveNext(); i++)
                {
                    object obj2;
                    object current = enumerator.Current;
                    if (current is string)
                    {
                        obj2 = new LiteralControl((string) current);
                    }
                    else if (current is CodeBlockBuilder)
                    {
                        string str;
                        if (!this.InDesigner)
                        {
                            continue;
                        }
                        CodeBlockBuilder builder = (CodeBlockBuilder) current;
                        switch (builder.BlockType)
                        {
                            case CodeBlockType.Code:
                                str = "<%" + builder.Content + "%>";
                                break;

                            case CodeBlockType.Expression:
                                str = "<%=" + builder.Content + "%>";
                                break;

                            case CodeBlockType.DataBinding:
                                str = "<%#" + builder.Content + "%>";
                                break;

                            case CodeBlockType.EncodedExpression:
                                str = "<%:" + builder.Content + "%>";
                                break;

                            default:
                                str = null;
                                break;
                        }
                        obj2 = new LiteralControl(str);
                    }
                    else
                    {
                        ControlBuilder builder2 = (ControlBuilder) current;
                        builder2.SetServiceProvider(this.ServiceProvider);
                        try
                        {
                            obj2 = builder2.BuildObject(this.flags[0x8000]);
                            if (!this.InDesigner)
                            {
                                UserControl control = obj2 as UserControl;
                                if (control != null)
                                {
                                    Control control2 = parentObj as Control;
                                    control.InitializeAsUserControl(control2.Page);
                                }
                            }
                        }
                        finally
                        {
                            builder2.SetServiceProvider(null);
                        }
                    }
                    ((IParserAccessor) parentObj).AddParsedSubObject(obj2);
                }
            }
        }

        public virtual object BuildObject()
        {
            return this.BuildObjectInternal();
        }

        internal object BuildObject(bool shouldApplyTheme)
        {
            if (this.flags[0x8000] != shouldApplyTheme)
            {
                this.flags[0x8000] = shouldApplyTheme;
            }
            return this.BuildObject();
        }

        internal object BuildObjectInternal()
        {
            object themedObject;
            if (!this.flags[2])
            {
                ConstructorNeedsTagAttribute attribute = (ConstructorNeedsTagAttribute) TargetFrameworkUtil.GetAttributes(this.ControlType)[typeof(ConstructorNeedsTagAttribute)];
                if ((attribute != null) && attribute.NeedsTag)
                {
                    this.flags[4] = true;
                }
                this.flags[2] = true;
            }
            if (this.flags[4])
            {
                object[] args = new object[] { this.TagName };
                themedObject = HttpRuntime.CreatePublicInstance(this._controlType, args);
            }
            else
            {
                themedObject = HttpRuntime.FastCreatePublicInstance(this._controlType);
            }
            if (this.flags[0x8000])
            {
                themedObject = this.GetThemedObject(themedObject);
            }
            this.AttachTypeDescriptionProvider(themedObject);
            this.InitObject(themedObject);
            return themedObject;
        }

        public virtual void CloseControl()
        {
        }

        internal static ParsedAttributeCollection ConvertDictionaryToParsedAttributeCollection(IDictionary attribs)
        {
            if (attribs is ParsedAttributeCollection)
            {
                return (ParsedAttributeCollection) attribs;
            }
            ParsedAttributeCollection attributes = new ParsedAttributeCollection();
            foreach (DictionaryEntry entry in attribs)
            {
                string name = entry.Key.ToString();
                attributes.AddFilteredAttribute(string.Empty, name, entry.Value.ToString());
            }
            return attributes;
        }

        private static ControlBuilder CreateBuilderFromType(Type type)
        {
            if (s_controlBuilderFactoryCache == null)
            {
                s_controlBuilderFactoryGenerator = new FactoryGenerator();
                s_controlBuilderFactoryCache = Hashtable.Synchronized(new Hashtable());
                s_controlBuilderFactoryCache[typeof(Content)] = new ContentBuilderInternalFactory();
                s_controlBuilderFactoryCache[typeof(ContentPlaceHolder)] = new ContentPlaceHolderBuilderFactory();
            }
            IWebObjectFactory factory = (IWebObjectFactory) s_controlBuilderFactoryCache[type];
            if (factory == null)
            {
                ControlBuilderAttribute controlBuilderAttribute = GetControlBuilderAttribute(type);
                if (controlBuilderAttribute != null)
                {
                    System.Web.UI.Util.CheckAssignableType(typeof(ControlBuilder), controlBuilderAttribute.BuilderType);
                    if (controlBuilderAttribute.BuilderType.IsPublic)
                    {
                        factory = s_controlBuilderFactoryGenerator.CreateFactory(controlBuilderAttribute.BuilderType);
                    }
                    else
                    {
                        factory = new ReflectionBasedControlBuilderFactory(controlBuilderAttribute.BuilderType);
                    }
                }
                else
                {
                    factory = s_defaultControlBuilderFactory;
                }
                s_controlBuilderFactoryCache[type] = factory;
            }
            return (ControlBuilder) factory.CreateInstance();
        }

        public static ControlBuilder CreateBuilderFromType(TemplateParser parser, ControlBuilder parentBuilder, Type type, string tagName, string id, IDictionary attribs, int line, string sourceFileName)
        {
            ControlBuilder builder = CreateBuilderFromType(type);
            builder.Line = line;
            builder.VirtualPath = System.Web.VirtualPath.CreateAllowNull(sourceFileName);
            builder.Init(parser, parentBuilder, type, tagName, id, attribs);
            return builder;
        }

        internal ControlBuilder CreateChildBuilder(string filter, string tagName, IDictionary attribs, TemplateParser parser, ControlBuilder parentBuilder, string id, int line, System.Web.VirtualPath virtualPath, ref Type childType, bool defaultProperty)
        {
            ControlBuilder builder;
            if (this.FChildrenAsProperties)
            {
                if (this.DefaultPropertyBuilder != null)
                {
                    if (TargetFrameworkUtil.GetProperty(this._controlType, tagName, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase) != null)
                    {
                        builder = this.GetChildPropertyBuilder(tagName, attribs, ref childType, parser, false);
                        if (this.DefaultPropertyBuilder.SubBuilders.Count > 0)
                        {
                            ParseChildrenAttribute attribute = null;
                            attribute = (ParseChildrenAttribute) TargetFrameworkUtil.GetCustomAttributes(this.ControlType, typeof(ParseChildrenAttribute), true)[0];
                            throw new HttpException(System.Web.SR.GetString("Cant_use_default_items_and_filtered_collection", new object[] { this._controlType.FullName, attribute.DefaultProperty }));
                        }
                        this.ParseTimeData.DefaultPropertyBuilder = null;
                    }
                    else
                    {
                        builder = this.DefaultPropertyBuilder.CreateChildBuilder(filter, tagName, attribs, parser, parentBuilder, id, line, virtualPath, ref childType, false);
                    }
                }
                else
                {
                    builder = this.GetChildPropertyBuilder(tagName, attribs, ref childType, parser, defaultProperty);
                }
            }
            else
            {
                string str = System.Web.UI.Util.CreateFilteredName(filter, tagName);
                childType = this.GetChildControlType(str, attribs);
                if (childType == null)
                {
                    return null;
                }
                builder = CreateBuilderFromType(parser, parentBuilder, childType, str, id, attribs, line, this.PageVirtualPath);
            }
            if (builder == null)
            {
                return null;
            }
            builder.Filter = filter;
            builder.SetParentBuilder((parentBuilder != null) ? parentBuilder : this);
            return builder;
        }

        private void DataBindingMethod(object sender, EventArgs e)
        {
            ICollection boundPropertyEntries;
            bool flag = this is BindableTemplateBuilder;
            bool flag2 = this is TemplateBuilder;
            bool flag3 = true;
            Control control = null;
            if (!this.flags[0x200])
            {
                boundPropertyEntries = this.BoundPropertyEntries;
            }
            else
            {
                ServiceContainer serviceProvider = new ServiceContainer();
                serviceProvider.AddService(typeof(IFilterResolutionService), this.TemplateControl);
                try
                {
                    this.SetServiceProvider(serviceProvider);
                    boundPropertyEntries = this.GetFilteredPropertyEntrySet(this.BoundPropertyEntries);
                }
                finally
                {
                    this.SetServiceProvider(null);
                }
            }
            foreach (BoundPropertyEntry entry in boundPropertyEntries)
            {
                if (((!entry.TwoWayBound || (!flag && !entry.ReadOnlyProperty)) && (entry.TwoWayBound || !flag2)) && entry.IsDataBindingEntry)
                {
                    string str;
                    if (flag3)
                    {
                        flag3 = false;
                        if (this._bindingContainerDescriptor == null)
                        {
                            this._bindingContainerDescriptor = TargetFrameworkUtil.GetProperties(typeof(Control))["BindingContainer"];
                        }
                        control = this._bindingContainerDescriptor.GetValue(sender) as Control;
                        if (control.Page.GetDataItem() == null)
                        {
                            break;
                        }
                    }
                    object obj2 = control.TemplateControl.Eval(entry.FieldName, entry.FormatString);
                    MemberInfo propertyInfo = PropertyMapper.GetMemberInfo(entry.ControlType, entry.Name, out str);
                    if (!entry.Type.IsValueType || (obj2 != null))
                    {
                        object obj4 = obj2;
                        if (entry.Type == typeof(string))
                        {
                            obj4 = Convert.ToString(obj2, CultureInfo.CurrentCulture);
                        }
                        else if ((obj2 != null) && !entry.Type.IsAssignableFrom(obj2.GetType()))
                        {
                            obj4 = PropertyConverter.ObjectFromString(entry.Type, propertyInfo, Convert.ToString(obj2, CultureInfo.CurrentCulture));
                        }
                        PropertyMapper.SetMappedPropertyValue(sender, str, obj4, this.InDesigner);
                    }
                }
            }
        }

        private void DoInitObjectOptimizations(object obj)
        {
            this.flags[0x10] = typeof(ICollection).IsAssignableFrom(this.ControlType);
            this.flags[0x20] = typeof(IParserAccessor).IsAssignableFrom(obj.GetType());
            if (this._simplePropertyEntries != null)
            {
                this.flags[0x40] = this.HasFilteredEntries(this._simplePropertyEntries);
            }
            if (this._complexPropertyEntries != null)
            {
                this.flags[0x80] = this.HasFilteredEntries(this._complexPropertyEntries);
            }
            if (this._templatePropertyEntries != null)
            {
                this.flags[0x100] = this.HasFilteredEntries(this._templatePropertyEntries);
            }
            if (this._boundPropertyEntries != null)
            {
                this.flags[0x200] = this.HasFilteredEntries(this._boundPropertyEntries);
            }
        }

        internal void EnsureEntriesSorted()
        {
            if (!this.flags[0x4000])
            {
                this.flags[0x4000] = true;
                this.SortEntries();
            }
        }

        private void FillUpBoundPropertyEntry(BoundPropertyEntry entry, string name)
        {
            string str;
            MemberInfo info = PropertyMapper.GetMemberInfo(this._controlType, name, out str);
            entry.Name = str;
            if (info != null)
            {
                if (info is PropertyInfo)
                {
                    PropertyInfo info2 = (PropertyInfo) info;
                    if (info2.GetSetMethod() == null)
                    {
                        if (!this.SupportsAttributes)
                        {
                            throw new HttpException(System.Web.SR.GetString("Property_readonly", new object[] { name }));
                        }
                        if (entry.TwoWayBound)
                        {
                            entry.ReadOnlyProperty = true;
                        }
                        else
                        {
                            entry.UseSetAttribute = true;
                        }
                    }
                    else
                    {
                        entry.PropertyInfo = info2;
                        entry.Type = info2.PropertyType;
                    }
                }
                else
                {
                    entry.Type = ((FieldInfo) info).FieldType;
                }
            }
            else
            {
                if (!this.SupportsAttributes)
                {
                    throw new HttpException(System.Web.SR.GetString("Type_doesnt_have_property", new object[] { this._controlType.FullName, name }));
                }
                if (entry.TwoWayBound)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("ControlBuilder_TwoWayBindingNonProperty", new object[] { name, this.ControlType.Name }));
                }
                entry.Name = name;
                entry.UseSetAttribute = true;
            }
            if (entry.ParsedExpressionData == null)
            {
                entry.ParseExpression(new ExpressionBuilderContext(this.VirtualPath));
            }
            if ((!this.Parser.IgnoreParseErrors && (entry.ParsedExpressionData == null)) && System.Web.UI.Util.IsWhiteSpaceString(entry.Expression))
            {
                throw new HttpException(System.Web.SR.GetString("Empty_expression"));
            }
        }

        public virtual Type GetChildControlType(string tagName, IDictionary attribs)
        {
            return null;
        }

        private ControlBuilder GetChildPropertyBuilder(string tagName, IDictionary attribs, ref Type childType, TemplateParser templateParser, bool defaultProperty)
        {
            PropertyInfo element = TargetFrameworkUtil.GetProperty(this._controlType, tagName, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (element == null)
            {
                throw new HttpException(System.Web.SR.GetString("Type_doesnt_have_property", new object[] { this._controlType.FullName, tagName }));
            }
            childType = element.PropertyType;
            ControlBuilder builder = null;
            if (typeof(ICollection).IsAssignableFrom(childType))
            {
                IgnoreUnknownContentAttribute attribute = (IgnoreUnknownContentAttribute) Attribute.GetCustomAttribute(element, typeof(IgnoreUnknownContentAttribute), true);
                builder = new CollectionBuilder(attribute != null);
            }
            else if (typeof(ITemplate).IsAssignableFrom(childType))
            {
                bool flag = false;
                bool flag2 = true;
                object[] customAttributes = element.GetCustomAttributes(typeof(TemplateContainerAttribute), false);
                if ((customAttributes != null) && (customAttributes.Length > 0))
                {
                    flag = ((TemplateContainerAttribute) customAttributes[0]).BindingDirection == BindingDirection.TwoWay;
                }
                flag2 = System.Web.UI.Util.IsMultiInstanceTemplateProperty(element);
                if (flag)
                {
                    builder = new BindableTemplateBuilder();
                }
                else
                {
                    builder = new TemplateBuilder();
                }
                if (builder is TemplateBuilder)
                {
                    ((TemplateBuilder) builder).AllowMultipleInstances = flag2;
                    if (this.InDesigner)
                    {
                        ((TemplateBuilder) builder).SetDesignerHost(templateParser.DesignerHost);
                    }
                }
            }
            else if (childType == typeof(string))
            {
                PersistenceModeAttribute attribute2 = (PersistenceModeAttribute) Attribute.GetCustomAttribute(element, typeof(PersistenceModeAttribute), true);
                if (((attribute2 == null) || (attribute2.Mode == PersistenceMode.Attribute)) && !defaultProperty)
                {
                    throw new HttpException(System.Web.SR.GetString("ControlBuilder_CannotHaveComplexString", new object[] { this._controlType.FullName, tagName }));
                }
                builder = new StringPropertyBuilder();
            }
            if (builder != null)
            {
                builder.Line = this.Line;
                builder.VirtualPath = this.VirtualPath;
                builder.Init(this.Parser, this, null, tagName, null, attribs);
                return builder;
            }
            return CreateBuilderFromType(this.Parser, this, childType, tagName, null, attribs, this.Line, this.PageVirtualPath);
        }

        private static ControlBuilderAttribute GetControlBuilderAttribute(Type controlType)
        {
            ControlBuilderAttribute attribute = null;
            object[] objArray = TargetFrameworkUtil.GetCustomAttributes(controlType, typeof(ControlBuilderAttribute), true);
            if ((objArray != null) && (objArray.Length > 0))
            {
                attribute = (ControlBuilderAttribute) objArray[0];
            }
            return attribute;
        }

        internal ICollection GetFilteredPropertyEntrySet(ICollection entries)
        {
            IDictionary dictionary = new HybridDictionary(true);
            IFilterResolutionService currentFilterResolutionService = this.CurrentFilterResolutionService;
            if (currentFilterResolutionService != null)
            {
                foreach (PropertyEntry entry in entries)
                {
                    if (!dictionary.Contains(entry.Name))
                    {
                        string filter = entry.Filter;
                        if (string.IsNullOrEmpty(filter) || currentFilterResolutionService.EvaluateFilter(filter))
                        {
                            dictionary[entry.Name] = entry;
                        }
                    }
                }
            }
            else
            {
                foreach (PropertyEntry entry2 in entries)
                {
                    if (string.IsNullOrEmpty(entry2.Filter))
                    {
                        dictionary[entry2.Name] = entry2;
                    }
                }
            }
            return dictionary.Values;
        }

        internal object GetLastBuilder()
        {
            if (this.SubBuilders.Count == 0)
            {
                return null;
            }
            return this.SubBuilders[this.SubBuilders.Count - 1];
        }

        public ObjectPersistData GetObjectPersistData()
        {
            return new ObjectPersistData(this, this.Parser.RootBuilder.BuiltObjects);
        }

        private static ParseChildrenAttribute GetParseChildrenAttribute(Type controlType)
        {
            ParseChildrenAttribute attribute = (ParseChildrenAttribute) s_parseChildrenAttributeCache[controlType];
            if (attribute == null)
            {
                object[] objArray = TargetFrameworkUtil.GetCustomAttributes(controlType, typeof(ParseChildrenAttribute), true);
                if ((objArray != null) && (objArray.Length > 0))
                {
                    attribute = (ParseChildrenAttribute) objArray[0];
                }
                if (attribute == null)
                {
                    attribute = s_markerParseChildrenAttribute;
                }
                lock (s_parseChildrenAttributeCache.SyncRoot)
                {
                    s_parseChildrenAttributeCache[controlType] = attribute;
                }
            }
            if (attribute == s_markerParseChildrenAttribute)
            {
                return null;
            }
            return attribute;
        }

        public string GetResourceKey()
        {
            return this.ParseTimeData.ResourceKeyPrefix;
        }

        internal virtual object GetThemedObject(object obj)
        {
            Control control = obj as Control;
            if (control == null)
            {
                return obj;
            }
            IThemeResolutionService themeResolutionService = this.ThemeResolutionService;
            if (themeResolutionService != null)
            {
                if (!string.IsNullOrEmpty(this.SkinID))
                {
                    control.SkinID = this.SkinID;
                }
                ThemeProvider stylesheetThemeProvider = themeResolutionService.GetStylesheetThemeProvider();
                SkinBuilder skinBuilder = null;
                if (stylesheetThemeProvider == null)
                {
                    return control;
                }
                skinBuilder = stylesheetThemeProvider.GetSkinBuilder(control);
                if (skinBuilder == null)
                {
                    return control;
                }
                try
                {
                    skinBuilder.SetServiceProvider(this.ServiceProvider);
                    return skinBuilder.ApplyTheme();
                }
                finally
                {
                    skinBuilder.SetServiceProvider(null);
                }
            }
            return control;
        }

        public virtual bool HasBody()
        {
            return true;
        }

        private bool HasFilteredEntries(ICollection entries)
        {
            foreach (PropertyEntry entry in entries)
            {
                if (entry.Filter.Length > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public virtual bool HtmlDecodeLiterals()
        {
            return false;
        }

        public virtual void Init(TemplateParser parser, ControlBuilder parentBuilder, Type type, string tagName, string id, IDictionary attribs)
        {
            this.ParseTimeData.Parser = parser;
            this.ParseTimeData.ParentBuilder = parentBuilder;
            if (parser != null)
            {
                this.ParseTimeData.IgnoreControlProperties = parser.IgnoreControlProperties;
            }
            this._tagName = tagName;
            if (type != null)
            {
                this._controlType = type;
                this.flags[0x2000] = typeof(Control).IsAssignableFrom(this._controlType);
                this.ID = id;
                ParseChildrenAttribute parseChildrenAttribute = GetParseChildrenAttribute(type);
                if (!typeof(IParserAccessor).IsAssignableFrom(type))
                {
                    this.ParseTimeData.IsNonParserAccessor = true;
                    this.ParseTimeData.ChildrenAsProperties = true;
                }
                else if (parseChildrenAttribute != null)
                {
                    this.ParseTimeData.ChildrenAsProperties = parseChildrenAttribute.ChildrenAsProperties;
                }
                if ((this.FChildrenAsProperties && (parseChildrenAttribute != null)) && (parseChildrenAttribute.DefaultProperty.Length != 0))
                {
                    Type childType = null;
                    this.ParseTimeData.DefaultPropertyBuilder = this.CreateChildBuilder(string.Empty, parseChildrenAttribute.DefaultProperty, null, parser, null, null, this.Line, this.VirtualPath, ref childType, true);
                }
                this.ParseTimeData.IsHtmlControl = typeof(HtmlControl).IsAssignableFrom(this._controlType);
                this.ParseTimeData.SupportsAttributes = typeof(IAttributeAccessor).IsAssignableFrom(this._controlType);
            }
            else
            {
                this.flags[0x2000] = false;
            }
            if (attribs != null)
            {
                this.PreprocessAttributes(ConvertDictionaryToParsedAttributeCollection(attribs));
            }
            if (this.InPageTheme)
            {
                ControlBuilder currentSkinBuilder = ((PageThemeParser) parser).CurrentSkinBuilder;
                if (((currentSkinBuilder != null) && (currentSkinBuilder.ControlType == this.ControlType)) && string.Equals(currentSkinBuilder.SkinID, this.SkinID, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("Cannot_set_recursive_skin", new object[] { currentSkinBuilder.ControlType.Name }));
                }
            }
        }

        private void InitBoundProperties(object obj)
        {
            if (this._boundPropertyEntries != null)
            {
                ICollection filteredPropertyEntrySet;
                DataBindingCollection dataBindings = null;
                IAttributeAccessor attributeAccessor = null;
                if (this.flags[0x200])
                {
                    filteredPropertyEntrySet = this.GetFilteredPropertyEntrySet(this.BoundPropertyEntries);
                }
                else
                {
                    filteredPropertyEntrySet = this.BoundPropertyEntries;
                }
                foreach (BoundPropertyEntry entry in filteredPropertyEntrySet)
                {
                    if ((!entry.TwoWayBound || !(this is BindableTemplateBuilder)) || !this.InDesigner)
                    {
                        this.InitBoundProperty(obj, entry, ref dataBindings, ref attributeAccessor);
                    }
                }
            }
        }

        private void InitBoundProperty(object obj, BoundPropertyEntry entry, ref DataBindingCollection dataBindings, ref IAttributeAccessor attributeAccessor)
        {
            string str = (entry.ExpressionPrefix == null) ? string.Empty : entry.ExpressionPrefix.Trim();
            if (this.InDesigner)
            {
                if (string.IsNullOrEmpty(str))
                {
                    if ((dataBindings == null) && (obj is IDataBindingsAccessor))
                    {
                        dataBindings = ((IDataBindingsAccessor) obj).DataBindings;
                    }
                    dataBindings.Add(new DataBinding(entry.Name, entry.Type, entry.Expression.Trim()));
                }
                else if (obj is IExpressionsAccessor)
                {
                    string expression = (entry.Expression == null) ? string.Empty : entry.Expression.Trim();
                    ((IExpressionsAccessor) obj).Expressions.Add(new ExpressionBinding(entry.Name, entry.Type, str, expression, entry.Generated, entry.ParsedExpressionData));
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(str))
                {
                    ExpressionBuilderContext context;
                    ExpressionBuilder expressionBuilder = entry.ExpressionBuilder;
                    if (!expressionBuilder.SupportsEvaluate)
                    {
                        return;
                    }
                    string name = entry.Name;
                    if (this.TemplateControl != null)
                    {
                        context = new ExpressionBuilderContext(this.TemplateControl);
                    }
                    else
                    {
                        context = new ExpressionBuilderContext(this.VirtualPath);
                    }
                    object obj2 = expressionBuilder.EvaluateExpression(obj, entry, entry.ParsedExpressionData, context);
                    if (entry.UseSetAttribute)
                    {
                        if (attributeAccessor == null)
                        {
                            attributeAccessor = (IAttributeAccessor) obj;
                        }
                        attributeAccessor.SetAttribute(name, obj2.ToString());
                        return;
                    }
                    try
                    {
                        PropertyMapper.SetMappedPropertyValue(obj, name, obj2, this.InDesigner);
                        return;
                    }
                    catch (Exception exception)
                    {
                        throw new HttpException(System.Web.SR.GetString("Cannot_set_property", new object[] { entry.ExpressionPrefix + ":" + entry.Expression, name }), exception);
                    }
                }
                ((Control) obj).DataBinding += new EventHandler(this.DataBindingMethod);
            }
        }

        private void InitCollectionsComplexProperties(object obj)
        {
            if (this._complexPropertyEntries != null)
            {
                foreach (ComplexPropertyEntry entry in this.ComplexPropertyEntries)
                {
                    try
                    {
                        object obj2;
                        ControlBuilder builder = entry.Builder;
                        builder.SetServiceProvider(this.ServiceProvider);
                        try
                        {
                            obj2 = builder.BuildObject(this.flags[0x8000]);
                        }
                        finally
                        {
                            builder.SetServiceProvider(null);
                        }
                        object[] parameters = new object[] { obj2 };
                        MethodInfo methodInfo = this.ControlType.GetMethod("Add", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { obj2.GetType() }, null);
                        if (methodInfo == null)
                        {
                            throw new InvalidOperationException(System.Web.SR.GetString("ControlBuilder_CollectionHasNoAddMethod", new object[] { this.TagName }));
                        }
                        System.Web.UI.Util.InvokeMethod(methodInfo, obj, parameters);
                    }
                    catch (Exception exception)
                    {
                        throw new HttpException(System.Web.SR.GetString("Cannot_add_value_not_collection", new object[] { this.TagName, exception.Message }), exception);
                    }
                }
            }
        }

        private void InitComplexProperties(object obj)
        {
            if (this._complexPropertyEntries != null)
            {
                ICollection filteredPropertyEntrySet;
                if (this.flags[0x80])
                {
                    filteredPropertyEntrySet = this.GetFilteredPropertyEntrySet(this.ComplexPropertyEntries);
                }
                else
                {
                    filteredPropertyEntrySet = this.ComplexPropertyEntries;
                }
                foreach (ComplexPropertyEntry entry in filteredPropertyEntrySet)
                {
                    if (entry.ReadOnly)
                    {
                        try
                        {
                            object obj2 = FastPropertyAccessor.GetProperty(obj, entry.Name, this.InDesigner);
                            entry.Builder.SetServiceProvider(this.ServiceProvider);
                            try
                            {
                                if (entry.Builder.flags[0x8000] != this.flags[0x8000])
                                {
                                    entry.Builder.flags[0x8000] = this.flags[0x8000];
                                }
                                entry.Builder.InitObject(obj2);
                            }
                            finally
                            {
                                entry.Builder.SetServiceProvider(null);
                            }
                            continue;
                        }
                        catch (Exception exception)
                        {
                            throw new HttpException(System.Web.SR.GetString("Cannot_init", new object[] { entry.Name, exception.Message }), exception);
                        }
                    }
                    try
                    {
                        ControlBuilder builder = entry.Builder;
                        object val = null;
                        builder.SetServiceProvider(this.ServiceProvider);
                        try
                        {
                            val = builder.BuildObject(this.flags[0x8000]);
                        }
                        finally
                        {
                            builder.SetServiceProvider(null);
                        }
                        FastPropertyAccessor.SetProperty(obj, entry.Name, val, this.InDesigner);
                    }
                    catch (Exception exception2)
                    {
                        throw new HttpException(System.Web.SR.GetString("Cannot_set_property", new object[] { this.TagName, entry.Name }), exception2);
                    }
                }
            }
        }

        internal virtual void InitObject(object obj)
        {
            this.EnsureEntriesSorted();
            if (!this.flags[8])
            {
                this.DoInitObjectOptimizations(obj);
                this.flags[8] = true;
            }
            Control control = obj as Control;
            if (control != null)
            {
                if (this.InDesigner)
                {
                    control.SetDesignMode();
                }
                if (this.SkinID != null)
                {
                    control.SkinID = this.SkinID;
                }
                if (!this.InDesigner && (this.TemplateControl != null))
                {
                    control.ApplyStyleSheetSkin(this.TemplateControl.Page);
                }
            }
            this.InitSimpleProperties(obj);
            if (this.flags[0x10])
            {
                this.InitCollectionsComplexProperties(obj);
            }
            else
            {
                this.InitComplexProperties(obj);
            }
            if (this.InDesigner)
            {
                if (control != null)
                {
                    if (this.Parser.DesignTimeDataBindHandler != null)
                    {
                        control.DataBinding += this.Parser.DesignTimeDataBindHandler;
                    }
                    control.SetControlBuilder(this);
                }
                this.Parser.RootBuilder.BuiltObjects[obj] = this;
            }
            this.InitBoundProperties(obj);
            if (this.flags[0x20])
            {
                this.BuildChildren(obj);
            }
            this.InitTemplateProperties(obj);
            if (control != null)
            {
                this.BindFieldToControl(control);
            }
        }

        private void InitSimpleProperties(object obj)
        {
            if (this._simplePropertyEntries != null)
            {
                ICollection filteredPropertyEntrySet;
                if (this.flags[0x40])
                {
                    filteredPropertyEntrySet = this.GetFilteredPropertyEntrySet(this.SimplePropertyEntries);
                }
                else
                {
                    filteredPropertyEntrySet = this.SimplePropertyEntries;
                }
                foreach (SimplePropertyEntry entry in filteredPropertyEntrySet)
                {
                    this.SetSimpleProperty(entry, obj);
                }
            }
        }

        private void InitTemplateProperties(object obj)
        {
            if (this._templatePropertyEntries != null)
            {
                ICollection filteredPropertyEntrySet;
                object[] parameters = new object[1];
                if (this.flags[0x100])
                {
                    filteredPropertyEntrySet = this.GetFilteredPropertyEntrySet(this.TemplatePropertyEntries);
                }
                else
                {
                    filteredPropertyEntrySet = this.TemplatePropertyEntries;
                }
                foreach (TemplatePropertyEntry entry in filteredPropertyEntrySet)
                {
                    try
                    {
                        ControlBuilder builder = entry.Builder;
                        builder.SetServiceProvider(this.ServiceProvider);
                        try
                        {
                            parameters[0] = builder.BuildObject(this.flags[0x8000]);
                        }
                        finally
                        {
                            builder.SetServiceProvider(null);
                        }
                        System.Web.UI.Util.InvokeMethod(entry.PropertyInfo.GetSetMethod(), obj, parameters);
                    }
                    catch (Exception exception)
                    {
                        throw new HttpException(System.Web.SR.GetString("Cannot_set_property", new object[] { this.TagName, entry.Name }), exception);
                    }
                }
            }
        }

        private bool IsValidForImplicitLocalization()
        {
            if (this.flags[0x2000])
            {
                return true;
            }
            if (this.ParentBuilder == null)
            {
                return false;
            }
            if (this.ParentBuilder.DefaultPropertyBuilder != null)
            {
                return typeof(ICollection).IsAssignableFrom(this.ParentBuilder.DefaultPropertyBuilder.ControlType);
            }
            return typeof(ICollection).IsAssignableFrom(this.ParentBuilder.ControlType);
        }

        public virtual bool NeedsTagInnerText()
        {
            return false;
        }

        public virtual void OnAppendToParentBuilder(ControlBuilder parentBuilder)
        {
            if (this.DefaultPropertyBuilder != null)
            {
                ControlBuilder defaultPropertyBuilder = this.DefaultPropertyBuilder;
                this.ParseTimeData.DefaultPropertyBuilder = null;
                this.AppendSubBuilder(defaultPropertyBuilder);
            }
            if (!(this is BindableTemplateBuilder))
            {
                ControlBuilder builder2 = this;
                while ((builder2 != null) && !(builder2 is BindableTemplateBuilder))
                {
                    builder2 = builder2.ParentBuilder;
                }
                if ((builder2 != null) && (builder2 is BindableTemplateBuilder))
                {
                    foreach (BoundPropertyEntry entry in this.BoundPropertyEntries)
                    {
                        if (entry.TwoWayBound)
                        {
                            ((BindableTemplateBuilder) builder2).AddBoundProperty(entry);
                        }
                    }
                }
            }
        }

        internal virtual void PrepareNoCompilePageSupport()
        {
            this.flags[1] = true;
            this._parseTimeData = null;
            if ((this._eventEntries != null) && (this._eventEntries.Count == 0))
            {
                this._eventEntries = null;
            }
            if ((this._simplePropertyEntries != null) && (this._simplePropertyEntries.Count == 0))
            {
                this._simplePropertyEntries = null;
            }
            if (this._complexPropertyEntries != null)
            {
                if (this._complexPropertyEntries.Count == 0)
                {
                    this._complexPropertyEntries = null;
                }
                else
                {
                    foreach (BuilderPropertyEntry entry in this._complexPropertyEntries)
                    {
                        if (entry.Builder != null)
                        {
                            entry.Builder.PrepareNoCompilePageSupport();
                        }
                    }
                }
            }
            if (this._templatePropertyEntries != null)
            {
                if (this._templatePropertyEntries.Count == 0)
                {
                    this._templatePropertyEntries = null;
                }
                else
                {
                    foreach (BuilderPropertyEntry entry2 in this._templatePropertyEntries)
                    {
                        if (entry2.Builder != null)
                        {
                            entry2.Builder.PrepareNoCompilePageSupport();
                        }
                    }
                }
            }
            if ((this._boundPropertyEntries != null) && (this._boundPropertyEntries.Count == 0))
            {
                this._boundPropertyEntries = null;
            }
            if (this._subBuilders != null)
            {
                if (this._subBuilders.Count > 0)
                {
                    foreach (object obj2 in this._subBuilders)
                    {
                        ControlBuilder builder = obj2 as ControlBuilder;
                        if (builder != null)
                        {
                            builder.PrepareNoCompilePageSupport();
                        }
                    }
                }
                else
                {
                    this._subBuilders = null;
                }
            }
            this.EnsureEntriesSorted();
        }

        internal void PreprocessAttribute(string filter, string attribname, string attribvalue, bool mainDirectiveMode)
        {
            Match match;
            if (attribvalue == null)
            {
                attribvalue = string.Empty;
            }
            if ((match = databindRegex.Match(attribvalue, 0)).Success)
            {
                if (!BuildManager.PrecompilingForUpdatableDeployment)
                {
                    string input = match.Groups["code"].Value;
                    bool flag = false;
                    bool twoWayBound = false;
                    if (!this.InDesigner)
                    {
                        if ((match = bindExpressionRegex.Match(input, 0)).Success)
                        {
                            flag = true;
                            twoWayBound = true;
                        }
                        else if (((this.CompilationMode == System.Web.UI.CompilationMode.Never) || this.InPageTheme) && (match = evalExpressionRegex.Match(input, 0)).Success)
                        {
                            flag = true;
                        }
                    }
                    if (flag)
                    {
                        string str2 = match.Groups["params"].Value;
                        if (!(match = bindParametersRegex.Match(str2, 0)).Success)
                        {
                            throw new HttpException(System.Web.SR.GetString("BadlyFormattedBind"));
                        }
                        string fieldName = match.Groups["fieldName"].Value;
                        string str4 = string.Empty;
                        Group group = match.Groups["formatString"];
                        if (group != null)
                        {
                            str4 = group.Value;
                        }
                        if ((str4.Length > 0) && !(match = formatStringRegex.Match(str4, 0)).Success)
                        {
                            throw new HttpException(System.Web.SR.GetString("BadlyFormattedBind"));
                        }
                        if (this.InPageTheme && !twoWayBound)
                        {
                            this.AddBoundProperty(filter, attribname, string.Empty, input, null, null, string.Empty, string.Empty, false);
                        }
                        else
                        {
                            this.AddBoundProperty(filter, attribname, string.Empty, input, null, null, fieldName, str4, twoWayBound);
                        }
                    }
                    else if (!this.Parser.PageParserFilterProcessedDataBindingAttribute(this.ID, attribname, input))
                    {
                        this.Parser.EnsureCodeAllowed();
                        this.AddBoundProperty(filter, attribname, string.Empty, input, null, null, string.Empty, string.Empty, false);
                    }
                }
            }
            else if ((match = expressionBuilderRegex.Match(attribvalue, 0)).Success)
            {
                if (this.InPageTheme)
                {
                    throw new HttpParseException(System.Web.SR.GetString("ControlBuilder_ExpressionsNotAllowedInThemes"));
                }
                if (!BuildManager.PrecompilingForUpdatableDeployment)
                {
                    string str5 = match.Groups["code"].Value.Trim();
                    int index = str5.IndexOf(':');
                    if (index == -1)
                    {
                        throw new HttpParseException(System.Web.SR.GetString("InvalidExpressionSyntax", new object[] { attribvalue }));
                    }
                    string expressionPrefix = str5.Substring(0, index).Trim();
                    string expression = str5.Substring(index + 1).Trim();
                    if (expressionPrefix.Length == 0)
                    {
                        throw new HttpParseException(System.Web.SR.GetString("MissingExpressionPrefix", new object[] { attribvalue }));
                    }
                    if (expression.Length == 0)
                    {
                        throw new HttpParseException(System.Web.SR.GetString("MissingExpressionValue", new object[] { attribvalue }));
                    }
                    ExpressionBuilder expressionBuilder = null;
                    if (this.CompilationMode == System.Web.UI.CompilationMode.Never)
                    {
                        expressionBuilder = ExpressionBuilder.GetExpressionBuilder(expressionPrefix, this.Parser.CurrentVirtualPath);
                        if ((expressionBuilder != null) && !expressionBuilder.SupportsEvaluate)
                        {
                            throw new InvalidOperationException(System.Web.SR.GetString("Cannot_evaluate_expression", new object[] { expressionPrefix + ":" + expression }));
                        }
                    }
                    this.AddBoundProperty(filter, attribname, expressionPrefix, expression, expressionBuilder, null, string.Empty, string.Empty, false);
                }
            }
            else
            {
                this.AddProperty(filter, attribname, attribvalue, mainDirectiveMode);
            }
        }

        private void PreprocessAttributes(ParsedAttributeCollection attribs)
        {
            this.ProcessImplicitResources(attribs);
            foreach (FilteredAttributeDictionary dictionary in attribs.GetFilteredAttributeDictionaries())
            {
                string filter = dictionary.Filter;
                foreach (DictionaryEntry entry in (IEnumerable) dictionary)
                {
                    string attribname = entry.Key.ToString();
                    string attribvalue = entry.Value.ToString();
                    this.PreprocessAttribute(filter, attribname, attribvalue, false);
                }
            }
        }

        internal void ProcessAndSortPropertyEntries(ArrayList propertyEntries, ref FilteredPropertyEntryComparer comparer)
        {
            if ((propertyEntries != null) && (propertyEntries.Count > 1))
            {
                HybridDictionary dictionary = new HybridDictionary(propertyEntries.Count, true);
                int num = 0;
                foreach (PropertyEntry entry in propertyEntries)
                {
                    object obj2 = dictionary[entry.Name];
                    if (obj2 != null)
                    {
                        entry.Order = (int) obj2;
                    }
                    else
                    {
                        entry.Order = num;
                        dictionary.Add(entry.Name, num++);
                    }
                }
                if (comparer == null)
                {
                    comparer = new FilteredPropertyEntryComparer(this.CurrentFilterResolutionService);
                }
                propertyEntries.Sort(comparer);
            }
        }

        public virtual void ProcessGeneratedCode(CodeCompileUnit codeCompileUnit, CodeTypeDeclaration baseType, CodeTypeDeclaration derivedType, CodeMemberMethod buildMethod, CodeMemberMethod dataBindingMethod)
        {
        }

        internal void ProcessImplicitResources(ParsedAttributeCollection attribs)
        {
            string str = (string) ((IDictionary) attribs)["meta:localize"];
            if (str != null)
            {
                bool flag;
                if (!this.IsValidForImplicitLocalization())
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("meta_localize_notallowed", new object[] { this.TagName }));
                }
                if (!bool.TryParse(str, out flag))
                {
                    throw new HttpException(System.Web.SR.GetString("ControlBuilder_InvalidLocalizeValue", new object[] { str }));
                }
                this.ParseTimeData.Localize = flag;
            }
            else
            {
                this.ParseTimeData.Localize = true;
            }
            string str2 = (string) ((IDictionary) attribs)["meta:resourcekey"];
            attribs.ClearFilter("meta");
            if (str2 != null)
            {
                IImplicitResourceProvider service;
                if (!this.IsValidForImplicitLocalization())
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("meta_reskey_notallowed", new object[] { this.TagName }));
                }
                if (!CodeGenerator.IsValidLanguageIndependentIdentifier(str2))
                {
                    throw new HttpException(System.Web.SR.GetString("Invalid_resourcekey", new object[] { str2 }));
                }
                if (!this.ParseTimeData.Localize)
                {
                    throw new HttpException(System.Web.SR.GetString("meta_localize_error"));
                }
                this.ParseTimeData.ResourceKeyPrefix = str2;
                if (this.Parser.FInDesigner && (this.Parser.DesignerHost != null))
                {
                    service = (IImplicitResourceProvider) this.Parser.DesignerHost.GetService(typeof(IImplicitResourceProvider));
                }
                else
                {
                    service = this.Parser.GetImplicitResourceProvider();
                }
                ICollection implicitResourceKeys = null;
                if (service != null)
                {
                    implicitResourceKeys = service.GetImplicitResourceKeys(str2);
                }
                if (implicitResourceKeys != null)
                {
                    IDesignerHost designerHost = this.DesignerHost;
                    ExpressionBuilder expressionBuilder = ExpressionBuilder.GetExpressionBuilder("resources", this.Parser.CurrentVirtualPath, designerHost);
                    bool flag2 = typeof(ResourceExpressionBuilder) == expressionBuilder.GetType();
                    foreach (ImplicitResourceKey key in implicitResourceKeys)
                    {
                        string str5;
                        string expression = str2 + "." + key.Property;
                        if (key.Filter.Length > 0)
                        {
                            expression = key.Filter + ':' + expression;
                        }
                        string name = key.Property.Replace('.', '-');
                        object parsedExpressionData = null;
                        if (flag2)
                        {
                            parsedExpressionData = ResourceExpressionBuilder.ParseExpression(expression);
                            str5 = string.Empty;
                        }
                        else
                        {
                            str5 = expression;
                        }
                        this.AddBoundProperty(key.Filter, name, "resources", str5, expressionBuilder, parsedExpressionData, true, string.Empty, string.Empty, false);
                    }
                }
            }
        }

        internal void SetControlType(Type controlType)
        {
            this._controlType = controlType;
            if (this._controlType != null)
            {
                this.flags[0x2000] = typeof(Control).IsAssignableFrom(this._controlType);
            }
            else
            {
                this.flags[0x2000] = false;
            }
        }

        internal virtual void SetParentBuilder(ControlBuilder parentBuilder)
        {
            this.ParseTimeData.ParentBuilder = parentBuilder;
            if ((this.ParseTimeData.FirstNonThemableProperty != null) && (parentBuilder is FileLevelPageThemeBuilder))
            {
                throw new InvalidOperationException(System.Web.SR.GetString("Property_theme_disabled", new object[] { this.ParseTimeData.FirstNonThemableProperty.Name, this.ControlType.FullName }));
            }
        }

        public void SetResourceKey(string resourceKey)
        {
            SimplePropertyEntry entry = new SimplePropertyEntry {
                Filter = "meta",
                Name = "resourcekey",
                Value = resourceKey,
                PersistedValue = resourceKey,
                UseSetAttribute = true,
                Type = typeof(string)
            };
            this.AddEntry(this.SimplePropertyEntriesInternal, entry);
        }

        public void SetServiceProvider(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        internal void SetSimpleProperty(SimplePropertyEntry entry, object obj)
        {
            if (entry.UseSetAttribute)
            {
                ((IAttributeAccessor) obj).SetAttribute(entry.Name, entry.Value.ToString());
            }
            else
            {
                try
                {
                    PropertyMapper.SetMappedPropertyValue(obj, entry.Name, entry.Value, this.InDesigner);
                }
                catch (Exception exception)
                {
                    throw new HttpException(System.Web.SR.GetString("Cannot_set_property", new object[] { entry.PersistedValue, entry.Name }), exception);
                }
            }
        }

        public virtual void SetTagInnerText(string text)
        {
        }

        internal virtual void SortEntries()
        {
            if (!(this is CollectionBuilder))
            {
                FilteredPropertyEntryComparer comparer = null;
                this.ProcessAndSortPropertyEntries(this._boundPropertyEntries, ref comparer);
                this.ProcessAndSortPropertyEntries(this._complexPropertyEntries, ref comparer);
                this.ProcessAndSortPropertyEntries(this._simplePropertyEntries, ref comparer);
                this.ProcessAndSortPropertyEntries(this._templatePropertyEntries, ref comparer);
            }
        }

        private void ValidatePersistable(PropertyInfo propInfo, bool usingSetAttribute, bool mainDirectiveMode, bool simplePropertyEntry, string filter)
        {
            PropertyDescriptorCollection propertyDescriptors;
            bool flag = propInfo.DeclaringType.IsAssignableFrom(this._controlType);
            if (flag)
            {
                propertyDescriptors = this.PropertyDescriptors;
            }
            else
            {
                propertyDescriptors = TargetFrameworkUtil.GetProperties(propInfo.DeclaringType);
            }
            PropertyDescriptor propertyDescriptor = propertyDescriptors[propInfo.Name];
            if (propertyDescriptor != null)
            {
                if (flag)
                {
                    if (this.IsHtmlControl)
                    {
                        if (propertyDescriptor.Attributes.Contains(HtmlControlPersistableAttribute.No))
                        {
                            throw new HttpException(System.Web.SR.GetString("Property_Not_Persistable", new object[] { propertyDescriptor.Name }));
                        }
                    }
                    else if ((!usingSetAttribute && !mainDirectiveMode) && propertyDescriptor.Attributes.Contains(DesignerSerializationVisibilityAttribute.Hidden))
                    {
                        throw new HttpException(System.Web.SR.GetString("Property_Not_Persistable", new object[] { propertyDescriptor.Name }));
                    }
                }
                if (!FilterableAttribute.IsPropertyFilterable(propertyDescriptor) && !string.IsNullOrEmpty(filter))
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("Illegal_Device", new object[] { propertyDescriptor.Name }));
                }
                if ((this.InPageTheme && (this.ParseTimeData.FirstNonThemableProperty == null)) && (!simplePropertyEntry || !usingSetAttribute))
                {
                    ThemeableAttribute attribute = (ThemeableAttribute) propertyDescriptor.Attributes[typeof(ThemeableAttribute)];
                    if ((attribute != null) && !attribute.Themeable)
                    {
                        if (this.ParentBuilder != null)
                        {
                            if (this.ParentBuilder is FileLevelPageThemeBuilder)
                            {
                                throw new InvalidOperationException(System.Web.SR.GetString("Property_theme_disabled", new object[] { propertyDescriptor.Name, this.ControlType.FullName }));
                            }
                        }
                        else
                        {
                            this.ParseTimeData.FirstNonThemableProperty = propertyDescriptor;
                        }
                    }
                }
            }
        }

        public virtual Type BindingContainerType
        {
            get
            {
                if (this.NamingContainerBuilder == null)
                {
                    return typeof(Control);
                }
                Type controlType = this.NamingContainerBuilder.ControlType;
                if (typeof(INonBindingContainer).IsAssignableFrom(controlType))
                {
                    return this.NamingContainerBuilder.BindingContainerType;
                }
                return this.NamingContainerBuilder.ControlType;
            }
        }

        internal ICollection BoundPropertyEntries
        {
            get
            {
                if (this._boundPropertyEntries == null)
                {
                    return EmptyCollection.Instance;
                }
                return this._boundPropertyEntries;
            }
        }

        private ArrayList BoundPropertyEntriesInternal
        {
            get
            {
                if (this._boundPropertyEntries == null)
                {
                    this._boundPropertyEntries = new ArrayList();
                }
                return this._boundPropertyEntries;
            }
        }

        internal System.Web.UI.CompilationMode CompilationMode
        {
            get
            {
                return this.Parser.CompilationMode;
            }
        }

        internal ICollection ComplexPropertyEntries
        {
            get
            {
                if (this._complexPropertyEntries == null)
                {
                    return EmptyCollection.Instance;
                }
                return this._complexPropertyEntries;
            }
        }

        private ArrayList ComplexPropertyEntriesInternal
        {
            get
            {
                if (this._complexPropertyEntries == null)
                {
                    this._complexPropertyEntries = new ArrayList();
                }
                return this._complexPropertyEntries;
            }
        }

        public Type ControlType
        {
            get
            {
                return this._controlType;
            }
        }

        public IFilterResolutionService CurrentFilterResolutionService
        {
            get
            {
                if (this.ServiceProvider != null)
                {
                    return (IFilterResolutionService) this.ServiceProvider.GetService(typeof(IFilterResolutionService));
                }
                return this.TemplateControl;
            }
        }

        public virtual Type DeclareType
        {
            get
            {
                return this._controlType;
            }
        }

        private ControlBuilder DefaultPropertyBuilder
        {
            get
            {
                return this.ParseTimeData.DefaultPropertyBuilder;
            }
        }

        private IDesignerHost DesignerHost
        {
            get
            {
                if (this.InDesigner && (this.ParseTimeData != null))
                {
                    TemplateParser parser = this.ParseTimeData.Parser;
                    if (parser != null)
                    {
                        return parser.DesignerHost;
                    }
                }
                return null;
            }
        }

        private EventDescriptorCollection EventDescriptors
        {
            get
            {
                if (this.ParseTimeData.EventDescriptors == null)
                {
                    this.ParseTimeData.EventDescriptors = TargetFrameworkUtil.GetEvents(this._controlType);
                }
                return this.ParseTimeData.EventDescriptors;
            }
        }

        internal ICollection EventEntries
        {
            get
            {
                if (this._eventEntries == null)
                {
                    return EmptyCollection.Instance;
                }
                return this._eventEntries;
            }
        }

        private ArrayList EventEntriesInternal
        {
            get
            {
                if (this._eventEntries == null)
                {
                    this._eventEntries = new ArrayList();
                }
                return this._eventEntries;
            }
        }

        protected bool FChildrenAsProperties
        {
            get
            {
                return this.ParseTimeData.ChildrenAsProperties;
            }
        }

        internal string Filter
        {
            get
            {
                return this.ParseTimeData.Filter;
            }
            set
            {
                this.ParseTimeData.Filter = value;
            }
        }

        protected bool FIsNonParserAccessor
        {
            get
            {
                return this.ParseTimeData.IsNonParserAccessor;
            }
        }

        public virtual bool HasAspCode
        {
            get
            {
                return this.ParseTimeData.HasAspCode;
            }
        }

        internal bool HasFilteredBoundEntries
        {
            get
            {
                return this.flags[0x200];
            }
        }

        internal bool HasTwoWayBoundProperties
        {
            get
            {
                return this.flags[0x400];
            }
        }

        public string ID
        {
            get
            {
                return this.ParseTimeData.ID;
            }
            set
            {
                this.ParseTimeData.ID = value;
            }
        }

        private bool IgnoreControlProperty
        {
            get
            {
                return this.ParseTimeData.IgnoreControlProperties;
            }
        }

        protected bool InDesigner
        {
            get
            {
                if (this.IsNoCompile)
                {
                    return false;
                }
                if (this.Parser == null)
                {
                    return false;
                }
                return this.Parser.FInDesigner;
            }
        }

        protected bool InPageTheme
        {
            get
            {
                return (this.Parser is PageThemeParser);
            }
        }

        internal bool IsControlSkin
        {
            get
            {
                return (this.ParentBuilder is FileLevelPageThemeBuilder);
            }
        }

        internal bool IsGeneratedID
        {
            get
            {
                return this.ParseTimeData.IsGeneratedID;
            }
            set
            {
                this.ParseTimeData.IsGeneratedID = value;
            }
        }

        private bool IsHtmlControl
        {
            get
            {
                return this.ParseTimeData.IsHtmlControl;
            }
        }

        internal bool IsNoCompile
        {
            get
            {
                return this.flags[1];
            }
        }

        internal int Line
        {
            get
            {
                return this.ParseTimeData.Line;
            }
            set
            {
                this.ParseTimeData.Line = value;
            }
        }

        public bool Localize
        {
            get
            {
                if (this.ParseTimeData != null)
                {
                    return this.ParseTimeData.Localize;
                }
                return true;
            }
        }

        private ControlBuilder NamingContainerBuilder
        {
            get
            {
                if (!this.ParseTimeData.NamingContainerSearched)
                {
                    if ((this.ParentBuilder == null) || (this.ParentBuilder.ControlType == null))
                    {
                        this.ParseTimeData.NamingContainerBuilder = null;
                    }
                    else if (typeof(INamingContainer).IsAssignableFrom(this.ParentBuilder.ControlType))
                    {
                        this.ParseTimeData.NamingContainerBuilder = this.ParentBuilder;
                    }
                    else
                    {
                        this.ParseTimeData.NamingContainerBuilder = this.ParentBuilder.NamingContainerBuilder;
                    }
                    this.ParseTimeData.NamingContainerSearched = true;
                }
                return this.ParseTimeData.NamingContainerBuilder;
            }
        }

        public Type NamingContainerType
        {
            get
            {
                if (this.NamingContainerBuilder == null)
                {
                    return typeof(Control);
                }
                return this.NamingContainerBuilder.ControlType;
            }
        }

        public string PageVirtualPath
        {
            get
            {
                return System.Web.VirtualPath.GetVirtualPathString(this.VirtualPath);
            }
        }

        internal ControlBuilder ParentBuilder
        {
            get
            {
                return this.ParseTimeData.ParentBuilder;
            }
        }

        protected internal TemplateParser Parser
        {
            get
            {
                return this.ParseTimeData.Parser;
            }
        }

        private ControlBuilderParseTimeData ParseTimeData
        {
            get
            {
                if (this._parseTimeData == null)
                {
                    if (this.IsNoCompile)
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("ControlBuilder_ParseTimeDataNotAvailable"));
                    }
                    this._parseTimeData = new ControlBuilderParseTimeData();
                }
                return this._parseTimeData;
            }
        }

        private PropertyDescriptorCollection PropertyDescriptors
        {
            get
            {
                if (this.ParseTimeData.PropertyDescriptors == null)
                {
                    this.ParseTimeData.PropertyDescriptors = TargetFrameworkUtil.GetProperties(this._controlType);
                }
                return this.ParseTimeData.PropertyDescriptors;
            }
        }

        private StringSet PropertyEntries
        {
            get
            {
                if (this.ParseTimeData.PropertyEntries == null)
                {
                    this.ParseTimeData.PropertyEntries = new CaseInsensitiveStringSet();
                }
                return this.ParseTimeData.PropertyEntries;
            }
        }

        public IServiceProvider ServiceProvider
        {
            get
            {
                return this._serviceProvider;
            }
        }

        internal ICollection SimplePropertyEntries
        {
            get
            {
                if (this._simplePropertyEntries == null)
                {
                    return EmptyCollection.Instance;
                }
                return this._simplePropertyEntries;
            }
        }

        internal ArrayList SimplePropertyEntriesInternal
        {
            get
            {
                if (this._simplePropertyEntries == null)
                {
                    this._simplePropertyEntries = new ArrayList();
                }
                return this._simplePropertyEntries;
            }
        }

        internal string SkinID
        {
            get
            {
                return this._skinID;
            }
            set
            {
                this._skinID = value;
            }
        }

        internal ArrayList SubBuilders
        {
            get
            {
                if (this._subBuilders == null)
                {
                    this._subBuilders = new ArrayList();
                }
                return this._subBuilders;
            }
        }

        private bool SupportsAttributes
        {
            get
            {
                return this.ParseTimeData.SupportsAttributes;
            }
        }

        public string TagName
        {
            get
            {
                return this._tagName;
            }
        }

        internal System.Web.UI.TemplateControl TemplateControl
        {
            get
            {
                HttpContext current = HttpContext.Current;
                if (current == null)
                {
                    return null;
                }
                return current.TemplateControl;
            }
        }

        internal ICollection TemplatePropertyEntries
        {
            get
            {
                if (this._templatePropertyEntries == null)
                {
                    return EmptyCollection.Instance;
                }
                return this._templatePropertyEntries;
            }
        }

        private ArrayList TemplatePropertyEntriesInternal
        {
            get
            {
                if (this._templatePropertyEntries == null)
                {
                    this._templatePropertyEntries = new ArrayList();
                }
                return this._templatePropertyEntries;
            }
        }

        public IThemeResolutionService ThemeResolutionService
        {
            get
            {
                if (this.ServiceProvider != null)
                {
                    return (IThemeResolutionService) this.ServiceProvider.GetService(typeof(IThemeResolutionService));
                }
                return (this.TemplateControl as IThemeResolutionService);
            }
        }

        internal System.Web.VirtualPath VirtualPath
        {
            get
            {
                return this.ParseTimeData.VirtualPath;
            }
            set
            {
                this.ParseTimeData.VirtualPath = value;
            }
        }

        private sealed class ControlBuilderParseTimeData
        {
            private const int childrenAsProperties = 1;
            internal ControlBuilder DefaultPropertyBuilder;
            internal EventDescriptorCollection EventDescriptors;
            internal string Filter;
            internal PropertyDescriptor FirstNonThemableProperty;
            private SimpleBitVector32 flags;
            private const int hasAspCode = 2;
            internal string ID;
            private const int ignoreControlProperties = 0x100;
            private const int isGeneratedID = 0x40;
            private const int isHtmlControl = 4;
            private const int isNonParserAccessor = 8;
            internal int Line;
            private const int localize = 0x80;
            internal ControlBuilder NamingContainerBuilder;
            private const int namingContainerSearched = 0x10;
            internal ControlBuilder ParentBuilder;
            internal TemplateParser Parser;
            internal PropertyDescriptorCollection PropertyDescriptors;
            internal StringSet PropertyEntries;
            internal string ResourceKeyPrefix;
            private const int supportsAttributes = 0x20;
            internal System.Web.VirtualPath VirtualPath;

            internal bool ChildrenAsProperties
            {
                get
                {
                    return this.flags[1];
                }
                set
                {
                    this.flags[1] = value;
                }
            }

            internal bool HasAspCode
            {
                get
                {
                    return this.flags[2];
                }
                set
                {
                    this.flags[2] = value;
                }
            }

            internal bool IgnoreControlProperties
            {
                get
                {
                    return this.flags[0x100];
                }
                set
                {
                    this.flags[0x100] = value;
                }
            }

            internal bool IsGeneratedID
            {
                get
                {
                    return this.flags[0x40];
                }
                set
                {
                    this.flags[0x40] = value;
                }
            }

            internal bool IsHtmlControl
            {
                get
                {
                    return this.flags[4];
                }
                set
                {
                    this.flags[4] = value;
                }
            }

            internal bool IsNonParserAccessor
            {
                get
                {
                    return this.flags[8];
                }
                set
                {
                    this.flags[8] = value;
                }
            }

            internal bool Localize
            {
                get
                {
                    return this.flags[0x80];
                }
                set
                {
                    this.flags[0x80] = value;
                }
            }

            internal bool NamingContainerSearched
            {
                get
                {
                    return this.flags[0x10];
                }
                set
                {
                    this.flags[0x10] = value;
                }
            }

            internal bool SupportsAttributes
            {
                get
                {
                    return this.flags[0x20];
                }
                set
                {
                    this.flags[0x20] = value;
                }
            }
        }

        private class DefaultControlBuilderFactory : IWebObjectFactory
        {
            object IWebObjectFactory.CreateInstance()
            {
                return new ControlBuilder();
            }
        }

        internal sealed class FilteredPropertyEntryComparer : IComparer
        {
            private IFilterResolutionService _filterResolutionService;

            public FilteredPropertyEntryComparer(IFilterResolutionService filterResolutionService)
            {
                this._filterResolutionService = filterResolutionService;
            }

            int IComparer.Compare(object o1, object o2)
            {
                if (o1 == o2)
                {
                    return 0;
                }
                if (o1 == null)
                {
                    return 1;
                }
                if (o2 == null)
                {
                    return -1;
                }
                PropertyEntry entry = (PropertyEntry) o1;
                PropertyEntry entry2 = (PropertyEntry) o2;
                int num = entry.Order - entry2.Order;
                if (num == 0)
                {
                    if (this._filterResolutionService == null)
                    {
                        if (string.IsNullOrEmpty(entry.Filter))
                        {
                            if ((entry2.Filter != null) && (entry2.Filter.Length > 0))
                            {
                                num = 1;
                            }
                            else
                            {
                                num = 0;
                            }
                        }
                        else if (string.IsNullOrEmpty(entry2.Filter))
                        {
                            num = -1;
                        }
                        else
                        {
                            num = 0;
                        }
                    }
                    else
                    {
                        string str = (entry.Filter.Length == 0) ? "Default" : entry.Filter;
                        string str2 = (entry2.Filter.Length == 0) ? "Default" : entry2.Filter;
                        num = this._filterResolutionService.CompareFilters(str, str2);
                    }
                    if (num == 0)
                    {
                        return (entry.Index - entry2.Index);
                    }
                }
                return num;
            }
        }

        private class ReflectionBasedControlBuilderFactory : IWebObjectFactory
        {
            private Type _builderType;

            internal ReflectionBasedControlBuilderFactory(Type builderType)
            {
                this._builderType = builderType;
            }

            object IWebObjectFactory.CreateInstance()
            {
                return (ControlBuilder) HttpRuntime.CreateNonPublicInstance(this._builderType);
            }
        }
    }
}

