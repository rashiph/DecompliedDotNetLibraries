namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Web;
    using System.Web.UI;

    [Themeable(true), Bindable(false), ParseChildren(true), PersistChildren(false), Designer("System.Web.UI.Design.WebControls.LoginViewDesigner,System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultProperty("CurrentView"), DefaultEvent("ViewChanged")]
    public class LoginView : Control, INamingContainer
    {
        private ITemplate _anonymousTemplate;
        private ITemplate _loggedInTemplate;
        private RoleGroupCollection _roleGroups;
        private int _templateIndex;
        private const int anonymousTemplateIndex = 0;
        private static readonly object EventViewChanged = new object();
        private static readonly object EventViewChanging = new object();
        private const int loggedInTemplateIndex = 1;
        private const int roleGroupStartingIndex = 2;

        [WebSysDescription("LoginView_ViewChanged"), WebCategory("Action")]
        public event EventHandler ViewChanged
        {
            add
            {
                base.Events.AddHandler(EventViewChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventViewChanged, value);
            }
        }

        [WebSysDescription("LoginView_ViewChanging"), WebCategory("Action")]
        public event EventHandler ViewChanging
        {
            add
            {
                base.Events.AddHandler(EventViewChanging, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventViewChanging, value);
            }
        }

        protected internal override void CreateChildControls()
        {
            this.Controls.Clear();
            Page page = this.Page;
            if (((page != null) && !page.IsPostBack) && !base.DesignMode)
            {
                this._templateIndex = this.GetTemplateIndex();
            }
            int templateIndex = this.TemplateIndex;
            ITemplate anonymousTemplate = null;
            switch (templateIndex)
            {
                case 0:
                    anonymousTemplate = this.AnonymousTemplate;
                    break;

                case 1:
                    anonymousTemplate = this.LoggedInTemplate;
                    break;

                default:
                {
                    int num2 = templateIndex - 2;
                    RoleGroupCollection roleGroups = this.RoleGroups;
                    if ((0 <= num2) && (num2 < roleGroups.Count))
                    {
                        anonymousTemplate = roleGroups[num2].ContentTemplate;
                    }
                    break;
                }
            }
            if (anonymousTemplate != null)
            {
                Control container = new Control();
                anonymousTemplate.InstantiateIn(container);
                this.Controls.Add(container);
            }
        }

        public override void DataBind()
        {
            this.OnDataBinding(EventArgs.Empty);
            this.EnsureChildControls();
            this.DataBindChildren();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void Focus()
        {
            throw new NotSupportedException(System.Web.SR.GetString("NoFocusSupport", new object[] { base.GetType().Name }));
        }

        private int GetTemplateIndex()
        {
            if ((base.DesignMode || (this.Page == null)) || !this.Page.Request.IsAuthenticated)
            {
                return 0;
            }
            IPrincipal user = LoginUtil.GetUser(this);
            int matchingRoleGroupInternal = -1;
            if (user != null)
            {
                matchingRoleGroupInternal = this.RoleGroups.GetMatchingRoleGroupInternal(user);
            }
            if (matchingRoleGroupInternal >= 0)
            {
                return (matchingRoleGroupInternal + 2);
            }
            return 1;
        }

        protected internal override void LoadControlState(object savedState)
        {
            if (savedState != null)
            {
                Pair pair = (Pair) savedState;
                if (pair.First != null)
                {
                    base.LoadControlState(pair.First);
                }
                if (pair.Second != null)
                {
                    this._templateIndex = (int) pair.Second;
                }
            }
        }

        protected internal override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (this.Page != null)
            {
                this.Page.RegisterRequiresControlState(this);
            }
        }

        protected internal override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            this.TemplateIndex = this.GetTemplateIndex();
            this.EnsureChildControls();
        }

        protected virtual void OnViewChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventViewChanged];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnViewChanging(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventViewChanging];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected internal override void Render(HtmlTextWriter writer)
        {
            this.EnsureChildControls();
            base.Render(writer);
        }

        protected internal override object SaveControlState()
        {
            object x = base.SaveControlState();
            if ((x == null) && (this._templateIndex == 0))
            {
                return null;
            }
            object y = null;
            if (this._templateIndex != 0)
            {
                y = this._templateIndex;
            }
            return new Pair(x, y);
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        protected override void SetDesignModeState(IDictionary data)
        {
            if (data != null)
            {
                object obj2 = data["TemplateIndex"];
                if (obj2 != null)
                {
                    this.TemplateIndex = (int) obj2;
                    base.ChildControlsCreated = false;
                }
            }
        }

        [Browsable(false), DefaultValue((string) null), TemplateContainer(typeof(LoginView)), PersistenceMode(PersistenceMode.InnerProperty)]
        public virtual ITemplate AnonymousTemplate
        {
            get
            {
                return this._anonymousTemplate;
            }
            set
            {
                this._anonymousTemplate = value;
            }
        }

        public override ControlCollection Controls
        {
            get
            {
                this.EnsureChildControls();
                return base.Controls;
            }
        }

        [Browsable(true)]
        public override bool EnableTheming
        {
            get
            {
                return base.EnableTheming;
            }
            set
            {
                base.EnableTheming = value;
            }
        }

        [TemplateContainer(typeof(LoginView)), Browsable(false), DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty)]
        public virtual ITemplate LoggedInTemplate
        {
            get
            {
                return this._loggedInTemplate;
            }
            set
            {
                this._loggedInTemplate = value;
            }
        }

        [Themeable(false), WebSysDescription("LoginView_RoleGroups"), MergableProperty(false), WebCategory("Behavior"), Filterable(false), PersistenceMode(PersistenceMode.InnerProperty)]
        public virtual RoleGroupCollection RoleGroups
        {
            get
            {
                if (this._roleGroups == null)
                {
                    this._roleGroups = new RoleGroupCollection();
                }
                return this._roleGroups;
            }
        }

        [Browsable(true)]
        public override string SkinID
        {
            get
            {
                return base.SkinID;
            }
            set
            {
                base.SkinID = value;
            }
        }

        private int TemplateIndex
        {
            get
            {
                return this._templateIndex;
            }
            set
            {
                if (value != this.TemplateIndex)
                {
                    this.OnViewChanging(EventArgs.Empty);
                    this._templateIndex = value;
                    base.ChildControlsCreated = false;
                    this.OnViewChanged(EventArgs.Empty);
                }
            }
        }
    }
}

