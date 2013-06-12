namespace System.Web.Compilation
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    internal class PageThemeCodeDomTreeGenerator : BaseTemplateCodeDomTreeGenerator
    {
        private int _controlCount;
        private ArrayList _controlSkinBuilderEntryList;
        private CodeTypeReference _controlSkinDelegateType;
        private const string _controlSkinsPropertyName = "ControlSkins";
        private const string _controlSkinsVarName = "__controlSkins";
        private CodeTypeReference _controlSkinType;
        private Hashtable _controlSkinTypeNameCollection;
        private const string _linkedStyleSheetsPropertyName = "LinkedStyleSheets";
        private const string _linkedStyleSheetsVarName = "__linkedStyleSheets";
        private PageThemeParser _themeParser;

        internal PageThemeCodeDomTreeGenerator(PageThemeParser parser) : base(parser)
        {
            this._controlSkinTypeNameCollection = new Hashtable();
            this._controlSkinBuilderEntryList = new ArrayList();
            this._controlSkinDelegateType = new CodeTypeReference(typeof(ControlSkinDelegate));
            this._controlSkinType = new CodeTypeReference(typeof(ControlSkin));
            this._themeParser = parser;
        }

        private void AddMemberOverride(string name, Type type, CodeExpression expr)
        {
            CodeMemberProperty property = new CodeMemberProperty {
                Name = name,
                Attributes = MemberAttributes.Family | MemberAttributes.Override,
                Type = new CodeTypeReference(type.FullName)
            };
            CodeMethodReturnStatement statement = new CodeMethodReturnStatement(expr);
            property.GetStatements.Add(statement);
            base._sourceDataClass.Members.Add(property);
        }

        private CodeStatement BuildControlSkinAssignmentStatement(ControlBuilder builder, string skinID)
        {
            Type controlType = builder.ControlType;
            string name = base.GetMethodNameForBuilder(BaseTemplateCodeDomTreeGenerator.buildMethodPrefix, builder) + "_skinKey";
            CodeMemberField field = new CodeMemberField(typeof(object), name) {
                Attributes = MemberAttributes.Private | MemberAttributes.Static
            };
            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression {
                Method = new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(typeof(PageTheme)), "CreateSkinKey")
            };
            expression.Parameters.Add(new CodeTypeOfExpression(controlType));
            expression.Parameters.Add(new CodePrimitiveExpression(skinID));
            field.InitExpression = expression;
            base._sourceDataClass.Members.Add(field);
            CodeFieldReferenceExpression targetObject = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "__controlSkins");
            CodeIndexerExpression left = new CodeIndexerExpression(targetObject, new CodeExpression[] { new CodeVariableReferenceExpression(name) });
            CodeDelegateCreateExpression expression4 = new CodeDelegateCreateExpression(this._controlSkinDelegateType, new CodeThisReferenceExpression(), base.GetMethodNameForBuilder(BaseTemplateCodeDomTreeGenerator.buildMethodPrefix, builder));
            CodeObjectCreateExpression right = new CodeObjectCreateExpression(this._controlSkinType, new CodeExpression[0]);
            right.Parameters.Add(new CodeTypeOfExpression(controlType));
            right.Parameters.Add(expression4);
            return new CodeAssignStatement(left, right);
        }

        private void BuildControlSkinMember()
        {
            int count = this._controlSkinBuilderEntryList.Count;
            CodeMemberField field = new CodeMemberField(typeof(HybridDictionary).FullName, "__controlSkins");
            CodeObjectCreateExpression expression = new CodeObjectCreateExpression(typeof(HybridDictionary), new CodeExpression[0]);
            expression.Parameters.Add(new CodePrimitiveExpression(count));
            field.InitExpression = expression;
            base._sourceDataClass.Members.Add(field);
        }

        private void BuildControlSkinProperty()
        {
            CodeFieldReferenceExpression expr = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "__controlSkins");
            this.AddMemberOverride("ControlSkins", typeof(IDictionary), expr);
        }

        private void BuildControlSkins(CodeStatementCollection statements)
        {
            foreach (ControlSkinBuilderEntry entry in this._controlSkinBuilderEntryList)
            {
                string skinID = entry.SkinID;
                ControlBuilder builder = entry.Builder;
                statements.Add(this.BuildControlSkinAssignmentStatement(builder, skinID));
            }
        }

        protected override void BuildInitStatements(CodeStatementCollection trueStatements, CodeStatementCollection topLevelStatements)
        {
            base.BuildInitStatements(trueStatements, topLevelStatements);
            this.BuildControlSkins(topLevelStatements);
        }

        private void BuildLinkedStyleSheetMember()
        {
            CodeMemberField field = new CodeMemberField(typeof(string[]), "__linkedStyleSheets");
            if ((this._themeParser.CssFileList != null) && (this._themeParser.CssFileList.Count > 0))
            {
                CodeExpression[] initializers = new CodeExpression[this._themeParser.CssFileList.Count];
                int num = 0;
                foreach (string str in this._themeParser.CssFileList)
                {
                    initializers[num++] = new CodePrimitiveExpression(str);
                }
                CodeArrayCreateExpression expression = new CodeArrayCreateExpression(typeof(string), initializers);
                field.InitExpression = expression;
            }
            else
            {
                field.InitExpression = new CodePrimitiveExpression(null);
            }
            base._sourceDataClass.Members.Add(field);
        }

        private void BuildLinkedStyleSheetProperty()
        {
            CodeFieldReferenceExpression expr = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "__linkedStyleSheets");
            this.AddMemberOverride("LinkedStyleSheets", typeof(string[]), expr);
        }

        protected override void BuildMiscClassMembers()
        {
            base.BuildMiscClassMembers();
            this.AddMemberOverride(BaseTemplateCodeDomTreeGenerator.templateSourceDirectoryName, typeof(string), new CodePrimitiveExpression(this._themeParser.VirtualDirPath.VirtualPathString));
            this.BuildSourceDataTreeFromBuilder(this._themeParser.RootBuilder, false, false, null);
            this.BuildControlSkinMember();
            this.BuildControlSkinProperty();
            this.BuildLinkedStyleSheetMember();
            this.BuildLinkedStyleSheetProperty();
        }

        protected override void BuildSourceDataTreeFromBuilder(ControlBuilder builder, bool fInTemplate, bool topLevelControlInTemplate, PropertyEntry pse)
        {
            if (!(builder is CodeBlockBuilder))
            {
                bool fTemplate = builder is TemplateBuilder;
                bool flag2 = builder == this._themeParser.RootBuilder;
                bool fControlSkin = (!fInTemplate && !fTemplate) && topLevelControlInTemplate;
                this._controlCount++;
                builder.ID = "__control" + this._controlCount.ToString(NumberFormatInfo.InvariantInfo);
                builder.IsGeneratedID = true;
                if (fControlSkin && !(builder is DataBoundLiteralControlBuilder))
                {
                    Type controlType = builder.ControlType;
                    string skinID = builder.SkinID;
                    object key = PageTheme.CreateSkinKey(builder.ControlType, skinID);
                    if (this._controlSkinTypeNameCollection.Contains(key))
                    {
                        if (string.IsNullOrEmpty(skinID))
                        {
                            throw new HttpParseException(System.Web.SR.GetString("Page_theme_default_theme_already_defined", new object[] { builder.ControlType.FullName }), null, builder.VirtualPath, null, builder.Line);
                        }
                        throw new HttpParseException(System.Web.SR.GetString("Page_theme_skinID_already_defined", new object[] { skinID }), null, builder.VirtualPath, null, builder.Line);
                    }
                    this._controlSkinTypeNameCollection.Add(key, true);
                    this._controlSkinBuilderEntryList.Add(new ControlSkinBuilderEntry(builder, skinID));
                }
                if (builder.SubBuilders != null)
                {
                    foreach (object obj3 in builder.SubBuilders)
                    {
                        if (obj3 is ControlBuilder)
                        {
                            bool flag4 = fTemplate && typeof(Control).IsAssignableFrom(((ControlBuilder) obj3).ControlType);
                            this.BuildSourceDataTreeFromBuilder((ControlBuilder) obj3, fInTemplate, flag4, null);
                        }
                    }
                }
                foreach (TemplatePropertyEntry entry in builder.TemplatePropertyEntries)
                {
                    this.BuildSourceDataTreeFromBuilder(entry.Builder, true, false, entry);
                }
                foreach (ComplexPropertyEntry entry2 in builder.ComplexPropertyEntries)
                {
                    if (!(entry2.Builder is StringPropertyBuilder))
                    {
                        this.BuildSourceDataTreeFromBuilder(entry2.Builder, fInTemplate, false, entry2);
                    }
                }
                if (!flag2)
                {
                    base.BuildBuildMethod(builder, fTemplate, fInTemplate, topLevelControlInTemplate, pse, fControlSkin);
                }
                if (!fControlSkin && builder.HasAspCode)
                {
                    base.BuildRenderMethod(builder, fTemplate);
                }
                base.BuildExtractMethod(builder);
                base.BuildPropertyBindingMethod(builder, fControlSkin);
            }
        }

        internal override CodeExpression BuildStringPropertyExpression(PropertyEntry pse)
        {
            if ((pse.PropertyInfo != null) && (Attribute.GetCustomAttribute(pse.PropertyInfo, typeof(UrlPropertyAttribute)) is UrlPropertyAttribute))
            {
                if (pse is SimplePropertyEntry)
                {
                    SimplePropertyEntry entry = (SimplePropertyEntry) pse;
                    string virtualPath = (string) entry.Value;
                    if (UrlPath.IsRelativeUrl(virtualPath) && !UrlPath.IsAppRelativePath(virtualPath))
                    {
                        entry.Value = UrlPath.MakeVirtualPathAppRelative(UrlPath.Combine(this._themeParser.VirtualDirPath.VirtualPathString, virtualPath));
                    }
                }
                else
                {
                    ComplexPropertyEntry entry2 = (ComplexPropertyEntry) pse;
                    StringPropertyBuilder builder = (StringPropertyBuilder) entry2.Builder;
                    string str2 = (string) builder.BuildObject();
                    if (UrlPath.IsRelativeUrl(str2) && !UrlPath.IsAppRelativePath(str2))
                    {
                        entry2.Builder = new StringPropertyBuilder(UrlPath.MakeVirtualPathAppRelative(UrlPath.Combine(this._themeParser.VirtualDirPath.VirtualPathString, str2)));
                    }
                }
            }
            return base.BuildStringPropertyExpression(pse);
        }

        protected override CodeAssignStatement BuildTemplatePropertyStatement(CodeExpression ctrlRefExpr)
        {
            return new CodeAssignStatement { Left = new CodePropertyReferenceExpression(ctrlRefExpr, BaseTemplateCodeDomTreeGenerator.templateSourceDirectoryName), Right = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), BaseTemplateCodeDomTreeGenerator.templateSourceDirectoryName) };
        }

        protected override string GetGeneratedClassName()
        {
            return Util.MakeValidTypeNameFromString(this._themeParser.VirtualDirPath.FileName);
        }

        protected override bool UseResourceLiteralString(string s)
        {
            return false;
        }

        protected override bool NeedProfileProperty
        {
            get
            {
                return false;
            }
        }

        private class ControlSkinBuilderEntry
        {
            private ControlBuilder _builder;
            private string _id;

            public ControlSkinBuilderEntry(ControlBuilder builder, string skinID)
            {
                this._builder = builder;
                this._id = skinID;
            }

            public ControlBuilder Builder
            {
                get
                {
                    return this._builder;
                }
            }

            public string SkinID
            {
                get
                {
                    if (this._id != null)
                    {
                        return this._id;
                    }
                    return string.Empty;
                }
            }
        }
    }
}

