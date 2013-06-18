namespace System.Web.Compilation
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Globalization;
    using System.Web.UI;

    internal class MasterPageCodeDomTreeGenerator : TemplateControlCodeDomTreeGenerator
    {
        protected MasterPageParser _masterPageParser;
        private const string _masterPropertyName = "Master";

        internal MasterPageCodeDomTreeGenerator(MasterPageParser parser) : base(parser)
        {
            this._masterPageParser = parser;
        }

        private void BuildAddContentPlaceHolderNames(CodeMemberMethod method, string placeHolderID)
        {
            CodePropertyReferenceExpression targetObject = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "ContentPlaceHolders");
            CodeExpressionStatement statement = new CodeExpressionStatement {
                Expression = new CodeMethodInvokeExpression(targetObject, "Add", new CodeExpression[] { new CodePrimitiveExpression(placeHolderID.ToLower(CultureInfo.InvariantCulture)) })
            };
            method.Statements.Add(statement);
        }

        protected override void BuildDefaultConstructor()
        {
            base.BuildDefaultConstructor();
            foreach (string str in (IEnumerable) this.Parser.PlaceHolderList)
            {
                this.BuildAddContentPlaceHolderNames(base._ctor, str);
            }
        }

        protected override void BuildMiscClassMembers()
        {
            base.BuildMiscClassMembers();
            if (this.Parser.MasterPageType != null)
            {
                base.BuildStronglyTypedProperty("Master", this.Parser.MasterPageType);
            }
        }

        private MasterPageParser Parser
        {
            get
            {
                return this._masterPageParser;
            }
        }
    }
}

