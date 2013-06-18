namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.Web;
    using System.Web.Util;

    public sealed class ObjectTagBuilder : ControlBuilder
    {
        private string _clsid;
        private bool _fLateBinding;
        private bool _lateBound;
        private string _progid;
        private ObjectTagScope _scope;
        private Type _type;

        public override void AppendLiteralString(string s)
        {
        }

        public override void AppendSubBuilder(ControlBuilder subBuilder)
        {
        }

        public override void Init(TemplateParser parser, ControlBuilder parentBuilder, Type type, string tagName, string id, IDictionary attribs)
        {
            if (id == null)
            {
                throw new HttpException(System.Web.SR.GetString("Object_tag_must_have_id"));
            }
            base.ID = id;
            string str = (string) attribs["scope"];
            if (str == null)
            {
                this._scope = ObjectTagScope.Default;
            }
            else if (StringUtil.EqualsIgnoreCase(str, "page"))
            {
                this._scope = ObjectTagScope.Page;
            }
            else if (StringUtil.EqualsIgnoreCase(str, "session"))
            {
                this._scope = ObjectTagScope.Session;
            }
            else if (StringUtil.EqualsIgnoreCase(str, "application"))
            {
                this._scope = ObjectTagScope.Application;
            }
            else
            {
                if (!StringUtil.EqualsIgnoreCase(str, "appinstance"))
                {
                    throw new HttpException(System.Web.SR.GetString("Invalid_scope", new object[] { str }));
                }
                this._scope = ObjectTagScope.AppInstance;
            }
            Util.GetAndRemoveBooleanAttribute(attribs, "latebinding", ref this._fLateBinding);
            string typeName = (string) attribs["class"];
            if (typeName != null)
            {
                this._type = parser.GetType(typeName);
            }
            if (this._type == null)
            {
                typeName = (string) attribs["classid"];
                if (typeName != null)
                {
                    Guid clsid = new Guid(typeName);
                    this._type = Type.GetTypeFromCLSID(clsid);
                    if (this._type == null)
                    {
                        throw new HttpException(System.Web.SR.GetString("Invalid_clsid", new object[] { typeName }));
                    }
                    if (this._fLateBinding || Util.IsLateBoundComClassicType(this._type))
                    {
                        this._lateBound = true;
                        this._clsid = typeName;
                    }
                    else
                    {
                        parser.AddTypeDependency(this._type);
                    }
                }
            }
            if (this._type == null)
            {
                typeName = (string) attribs["progid"];
                if (typeName != null)
                {
                    this._type = Type.GetTypeFromProgID(typeName);
                    if (this._type == null)
                    {
                        throw new HttpException(System.Web.SR.GetString("Invalid_progid", new object[] { typeName }));
                    }
                    if (this._fLateBinding || Util.IsLateBoundComClassicType(this._type))
                    {
                        this._lateBound = true;
                        this._progid = typeName;
                    }
                    else
                    {
                        parser.AddTypeDependency(this._type);
                    }
                }
            }
            if (this._type == null)
            {
                throw new HttpException(System.Web.SR.GetString("Object_tag_must_have_class_classid_or_progid"));
            }
        }

        internal string Clsid
        {
            get
            {
                return this._clsid;
            }
        }

        internal Type DeclaredType
        {
            get
            {
                if (!this._lateBound)
                {
                    return this.ObjectType;
                }
                return typeof(object);
            }
        }

        internal bool LateBound
        {
            get
            {
                return this._lateBound;
            }
        }

        internal Type ObjectType
        {
            get
            {
                return this._type;
            }
        }

        internal string Progid
        {
            get
            {
                return this._progid;
            }
        }

        internal ObjectTagScope Scope
        {
            get
            {
                return this._scope;
            }
        }
    }
}

