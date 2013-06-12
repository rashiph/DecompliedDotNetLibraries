namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;

    [ControlBuilder(typeof(MultiViewControlBuilder)), Designer("System.Web.UI.Design.WebControls.MultiViewDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ParseChildren(typeof(System.Web.UI.WebControls.View)), ToolboxData("<{0}:MultiView runat=\"server\"></{0}:MultiView>"), DefaultEvent("ActiveViewChanged")]
    public class MultiView : Control
    {
        private int _activeViewIndex = -1;
        private int _cachedActiveViewIndex = -1;
        private bool _controlStateApplied;
        private static readonly object _eventActiveViewChanged = new object();
        private bool _ignoreBubbleEvents;
        public static readonly string NextViewCommandName = "NextView";
        public static readonly string PreviousViewCommandName = "PrevView";
        public static readonly string SwitchViewByIDCommandName = "SwitchViewByID";
        public static readonly string SwitchViewByIndexCommandName = "SwitchViewByIndex";

        [WebCategory("Action"), WebSysDescription("MultiView_ActiveViewChanged")]
        public event EventHandler ActiveViewChanged
        {
            add
            {
                base.Events.AddHandler(_eventActiveViewChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(_eventActiveViewChanged, value);
            }
        }

        protected override void AddParsedSubObject(object obj)
        {
            if (obj is System.Web.UI.WebControls.View)
            {
                this.Controls.Add((Control) obj);
            }
            else if (!(obj is LiteralControl))
            {
                throw new HttpException(System.Web.SR.GetString("MultiView_cannot_have_children_of_type", new object[] { obj.GetType().Name }));
            }
        }

        protected override ControlCollection CreateControlCollection()
        {
            return new ViewCollection(this);
        }

        public System.Web.UI.WebControls.View GetActiveView()
        {
            int activeViewIndex = this.ActiveViewIndex;
            if (activeViewIndex >= this.Views.Count)
            {
                throw new Exception(System.Web.SR.GetString("MultiView_ActiveViewIndex_out_of_range"));
            }
            if (activeViewIndex < 0)
            {
                return null;
            }
            System.Web.UI.WebControls.View view = this.Views[activeViewIndex];
            if (!view.Active)
            {
                this.UpdateActiveView(activeViewIndex);
            }
            return view;
        }

        internal void IgnoreBubbleEvents()
        {
            this._ignoreBubbleEvents = true;
        }

        protected internal override void LoadControlState(object state)
        {
            Pair pair = state as Pair;
            if (pair != null)
            {
                base.LoadControlState(pair.First);
                this.ActiveViewIndex = (int) pair.Second;
            }
            this._controlStateApplied = true;
        }

        protected virtual void OnActiveViewChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[_eventActiveViewChanged];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override bool OnBubbleEvent(object source, EventArgs e)
        {
            if (!this._ignoreBubbleEvents && (e is CommandEventArgs))
            {
                CommandEventArgs args = (CommandEventArgs) e;
                string commandName = args.CommandName;
                if (commandName == NextViewCommandName)
                {
                    if (this.ActiveViewIndex < (this.Views.Count - 1))
                    {
                        this.ActiveViewIndex++;
                    }
                    else
                    {
                        this.ActiveViewIndex = -1;
                    }
                    return true;
                }
                if (commandName == PreviousViewCommandName)
                {
                    if (this.ActiveViewIndex > -1)
                    {
                        this.ActiveViewIndex--;
                    }
                    return true;
                }
                if (commandName == SwitchViewByIDCommandName)
                {
                    System.Web.UI.WebControls.View view = this.FindControl((string) args.CommandArgument) as System.Web.UI.WebControls.View;
                    if ((view == null) || (view.Parent != this))
                    {
                        throw new HttpException(System.Web.SR.GetString("MultiView_invalid_view_id", new object[] { this.ID, (string) args.CommandArgument, SwitchViewByIDCommandName }));
                    }
                    this.SetActiveView(view);
                    return true;
                }
                if (commandName == SwitchViewByIndexCommandName)
                {
                    int num;
                    try
                    {
                        num = int.Parse((string) args.CommandArgument, CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                        throw new FormatException(System.Web.SR.GetString("MultiView_invalid_view_index_format", new object[] { (string) args.CommandArgument, SwitchViewByIndexCommandName }));
                    }
                    this.ActiveViewIndex = num;
                    return true;
                }
            }
            return false;
        }

        protected internal override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.Page.RegisterRequiresControlState(this);
            if (this._cachedActiveViewIndex > -1)
            {
                this.ActiveViewIndex = this._cachedActiveViewIndex;
                this._cachedActiveViewIndex = -1;
                this.GetActiveView();
            }
        }

        protected internal override void RemovedControl(Control ctl)
        {
            if (((System.Web.UI.WebControls.View) ctl).Active && (this.ActiveViewIndex < this.Views.Count))
            {
                this.GetActiveView();
            }
            base.RemovedControl(ctl);
        }

        protected internal override void Render(HtmlTextWriter writer)
        {
            System.Web.UI.WebControls.View activeView = this.GetActiveView();
            if (activeView != null)
            {
                activeView.RenderControl(writer);
            }
        }

        protected internal override object SaveControlState()
        {
            int activeViewIndex = this.ActiveViewIndex;
            object x = base.SaveControlState();
            if ((x == null) && (activeViewIndex == -1))
            {
                return null;
            }
            return new Pair(x, activeViewIndex);
        }

        public void SetActiveView(System.Web.UI.WebControls.View view)
        {
            int index = this.Views.IndexOf(view);
            if (index < 0)
            {
                throw new HttpException(System.Web.SR.GetString("MultiView_view_not_found", new object[] { (view == null) ? "null" : view.ID, this.ID }));
            }
            this.ActiveViewIndex = index;
        }

        private void UpdateActiveView(int activeViewIndex)
        {
            for (int i = 0; i < this.Views.Count; i++)
            {
                System.Web.UI.WebControls.View view = this.Views[i];
                if (i == activeViewIndex)
                {
                    view.Active = true;
                    if (this.ShouldTriggerViewEvent)
                    {
                        view.OnActivate(EventArgs.Empty);
                    }
                }
                else if (view.Active)
                {
                    view.Active = false;
                    if (this.ShouldTriggerViewEvent)
                    {
                        view.OnDeactivate(EventArgs.Empty);
                    }
                }
            }
        }

        [DefaultValue(-1), WebSysDescription("MultiView_ActiveView"), WebCategory("Behavior")]
        public virtual int ActiveViewIndex
        {
            get
            {
                if (this._cachedActiveViewIndex > -1)
                {
                    return this._cachedActiveViewIndex;
                }
                return this._activeViewIndex;
            }
            set
            {
                if (value < -1)
                {
                    throw new ArgumentOutOfRangeException("value", System.Web.SR.GetString("MultiView_ActiveViewIndex_less_than_minus_one", new object[] { value }));
                }
                if ((this.Views.Count == 0) && (base.ControlState < ControlState.FrameworkInitialized))
                {
                    this._cachedActiveViewIndex = value;
                }
                else
                {
                    if (value >= this.Views.Count)
                    {
                        throw new ArgumentOutOfRangeException("value", System.Web.SR.GetString("MultiView_ActiveViewIndex_equal_or_greater_than_count", new object[] { value, this.Views.Count }));
                    }
                    int num = (this._cachedActiveViewIndex != -1) ? -1 : this._activeViewIndex;
                    this._activeViewIndex = value;
                    this._cachedActiveViewIndex = -1;
                    if (((num != value) && (num != -1)) && (num < this.Views.Count))
                    {
                        this.Views[num].Active = false;
                        if (this.ShouldTriggerViewEvent)
                        {
                            this.Views[num].OnDeactivate(EventArgs.Empty);
                        }
                    }
                    if (((num != value) && (this.Views.Count != 0)) && (value != -1))
                    {
                        this.Views[value].Active = true;
                        if (this.ShouldTriggerViewEvent)
                        {
                            this.Views[value].OnActivate(EventArgs.Empty);
                            this.OnActiveViewChanged(EventArgs.Empty);
                        }
                    }
                }
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

        private bool ShouldTriggerViewEvent
        {
            get
            {
                return (this._controlStateApplied || ((this.Page != null) && !this.Page.IsPostBack));
            }
        }

        [PersistenceMode(PersistenceMode.InnerDefaultProperty), Browsable(false), WebSysDescription("MultiView_Views")]
        public virtual ViewCollection Views
        {
            get
            {
                return (ViewCollection) this.Controls;
            }
        }
    }
}

