namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.UI;

    [Designer("System.Web.UI.Design.WebControls.WizardDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), Bindable(false), DefaultEvent("FinishButtonClick"), ToolboxData("<{0}:Wizard runat=\"server\"> <WizardSteps> <asp:WizardStep title=\"Step 1\" runat=\"server\"></asp:WizardStep> <asp:WizardStep title=\"Step 2\" runat=\"server\"></asp:WizardStep> </WizardSteps> </{0}:Wizard>")]
    public class Wizard : CompositeControl
    {
        private bool _activeStepIndexSet;
        private Style _cancelButtonStyle;
        private IButtonControl _commandSender;
        private const string _customNavigationContainerIdPrefix = "__CustomNav";
        private Dictionary<WizardStepBase, BaseNavigationTemplateContainer> _customNavigationContainers;
        internal const string _customNavigationControls = "CustomNavigationControls";
        private IDictionary _designModeState;
        private bool _displaySideBar;
        private bool _displaySideBarDefault;
        private const bool _displaySideBarDefaultValue = true;
        private static readonly object _eventActiveStepChanged = new object();
        private static readonly object _eventCancelButtonClick = new object();
        private static readonly object _eventFinishButtonClick = new object();
        private static readonly object _eventNextButtonClick = new object();
        private static readonly object _eventPreviousButtonClick = new object();
        private static readonly object _eventSideBarButtonClick = new object();
        private Style _finishCompleteButtonStyle;
        private ITemplate _finishNavigationTemplate;
        private Style _finishPreviousButtonStyle;
        private TableItemStyle _headerStyle;
        private ITemplate _headerTemplate;
        private Stack<int> _historyStack;
        private bool? _isMacIE;
        private ITemplate _layoutTemplate;
        private System.Web.UI.WebControls.MultiView _multiView;
        private const string _multiViewID = "WizardMultiView";
        private Style _navigationButtonStyle;
        private TableItemStyle _navigationStyle;
        private WizardRenderingBase _rendering;
        private bool _renderSideBarDataList;
        private Style _sideBarButtonStyle;
        private IWizardSideBarListControl _sideBarList;
        private TableItemStyle _sideBarStyle;
        private TableCell _sideBarTableCell;
        private ITemplate _sideBarTemplate;
        private ITemplate _startNavigationTemplate;
        private Style _startNextButtonStyle;
        private ITemplate _stepNavigationTemplate;
        private Style _stepNextButtonStyle;
        private Style _stepPreviousButtonStyle;
        private TableItemStyle _stepStyle;
        private List<TemplatedWizardStep> _templatedSteps;
        private const string _templatedStepsID = "TemplatedWizardSteps";
        private const int _viewStateArrayLength = 15;
        private const string _wizardContentMark = "_SkipLink";
        private WizardStepCollection _wizardStepCollection;
        protected static readonly string CancelButtonID = "CancelButton";
        public static readonly string CancelCommandName = "Cancel";
        protected static readonly string CustomFinishButtonID = "CustomFinishButton";
        protected static readonly string CustomNextButtonID = "CustomNextButton";
        protected static readonly string CustomPreviousButtonID = "CustomPreviousButton";
        protected static readonly string DataListID = "SideBarList";
        protected static readonly string FinishButtonID = "FinishButton";
        protected static readonly string FinishPreviousButtonID = "FinishPreviousButton";
        public static readonly string HeaderPlaceholderId = "headerPlaceholder";
        public static readonly string MoveCompleteCommandName = "MoveComplete";
        public static readonly string MoveNextCommandName = "MoveNext";
        public static readonly string MovePreviousCommandName = "MovePrevious";
        public static readonly string MoveToCommandName = "Move";
        public static readonly string NavigationPlaceholderId = "navigationPlaceholder";
        protected static readonly string SideBarButtonID = "SideBarButton";
        public static readonly string SideBarPlaceholderId = "sideBarPlaceholder";
        protected static readonly string StartNextButtonID = "StartNextButton";
        protected static readonly string StepNextButtonID = "StepNextButton";
        protected static readonly string StepPreviousButtonID = "StepPreviousButton";
        public static readonly string WizardStepPlaceholderId = "wizardStepPlaceholder";

        [WebCategory("Action"), WebSysDescription("Wizard_ActiveStepChanged")]
        public event EventHandler ActiveStepChanged
        {
            add
            {
                base.Events.AddHandler(_eventActiveStepChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(_eventActiveStepChanged, value);
            }
        }

        [WebSysDescription("Wizard_CancelButtonClick"), WebCategory("Action")]
        public event EventHandler CancelButtonClick
        {
            add
            {
                base.Events.AddHandler(_eventCancelButtonClick, value);
            }
            remove
            {
                base.Events.RemoveHandler(_eventCancelButtonClick, value);
            }
        }

        [WebSysDescription("Wizard_FinishButtonClick"), WebCategory("Action")]
        public event WizardNavigationEventHandler FinishButtonClick
        {
            add
            {
                base.Events.AddHandler(_eventFinishButtonClick, value);
            }
            remove
            {
                base.Events.RemoveHandler(_eventFinishButtonClick, value);
            }
        }

        [WebCategory("Action"), WebSysDescription("Wizard_NextButtonClick")]
        public event WizardNavigationEventHandler NextButtonClick
        {
            add
            {
                base.Events.AddHandler(_eventNextButtonClick, value);
            }
            remove
            {
                base.Events.RemoveHandler(_eventNextButtonClick, value);
            }
        }

        [WebSysDescription("Wizard_PreviousButtonClick"), WebCategory("Action")]
        public event WizardNavigationEventHandler PreviousButtonClick
        {
            add
            {
                base.Events.AddHandler(_eventPreviousButtonClick, value);
            }
            remove
            {
                base.Events.RemoveHandler(_eventPreviousButtonClick, value);
            }
        }

        [WebSysDescription("Wizard_SideBarButtonClick"), WebCategory("Action")]
        public event WizardNavigationEventHandler SideBarButtonClick
        {
            add
            {
                base.Events.AddHandler(_eventSideBarButtonClick, value);
            }
            remove
            {
                base.Events.RemoveHandler(_eventSideBarButtonClick, value);
            }
        }

        public Wizard() : this(true)
        {
        }

        internal Wizard(bool displaySideBarDefault)
        {
            this._displaySideBarDefault = displaySideBarDefault;
            this._displaySideBar = displaySideBarDefault;
        }

        protected virtual bool AllowNavigationToStep(int index)
        {
            if ((this._historyStack != null) && this._historyStack.Contains(index))
            {
                return this.WizardSteps[index].AllowReturn;
            }
            return true;
        }

        private void ApplyControlProperties()
        {
            this._rendering.ApplyControlProperties();
        }

        internal BaseNavigationTemplateContainer CreateBaseNavigationTemplateContainer(string id)
        {
            return new BaseNavigationTemplateContainer(this) { ID = id };
        }

        protected internal override void CreateChildControls()
        {
            using (new WizardControlCollectionModifier(this))
            {
                this.Controls.Clear();
                this._customNavigationContainers = null;
            }
            if (this.LayoutTemplate == null)
            {
                this._rendering = this.CreateTableRendering();
            }
            else
            {
                this._rendering = this.CreateLayoutTemplateRendering();
            }
            this.CreateControlHierarchy();
            base.ClearChildViewState();
        }

        protected override ControlCollection CreateControlCollection()
        {
            return new WizardControlCollection(this);
        }

        protected virtual void CreateControlHierarchy()
        {
            this._rendering.CreateControlHierarchy();
        }

        protected override Style CreateControlStyle()
        {
            return new TableStyle { CellSpacing = 0, CellPadding = 0 };
        }

        internal virtual void CreateCustomNavigationTemplates()
        {
            for (int i = 0; i < this.WizardSteps.Count; i++)
            {
                TemplatedWizardStep step = this.WizardSteps[i] as TemplatedWizardStep;
                if (step != null)
                {
                    this.RegisterCustomNavigationContainers(step);
                }
            }
        }

        internal virtual ITemplate CreateDefaultDataListItemTemplate()
        {
            return new DataListItemTemplate(this);
        }

        internal virtual ITemplate CreateDefaultSideBarTemplate()
        {
            return new DefaultSideBarTemplate(this);
        }

        internal virtual LayoutTemplateWizardRendering CreateLayoutTemplateRendering()
        {
            return new LayoutTemplateWizardRendering(this);
        }

        internal virtual TableWizardRendering CreateTableRendering()
        {
            return new TableWizardRendering(this);
        }

        private void DataListItemCommand(object sender, CommandEventArgs e)
        {
            if (MoveToCommandName.Equals(e.CommandName, StringComparison.OrdinalIgnoreCase))
            {
                int activeStepIndex = this.ActiveStepIndex;
                int nextStepIndex = int.Parse((string) e.CommandArgument, CultureInfo.InvariantCulture);
                WizardNavigationEventArgs args = new WizardNavigationEventArgs(activeStepIndex, nextStepIndex);
                if (((this._commandSender != null) && !base.DesignMode) && ((this.Page != null) && !this.Page.IsValid))
                {
                    args.Cancel = true;
                }
                this._activeStepIndexSet = false;
                this.OnSideBarButtonClick(args);
                if (!args.Cancel)
                {
                    if (!this._activeStepIndexSet && this.AllowNavigationToStep(nextStepIndex))
                    {
                        this.ActiveStepIndex = nextStepIndex;
                    }
                }
                else
                {
                    this.ActiveStepIndex = activeStepIndex;
                }
            }
        }

        internal virtual void DataListItemDataBound(object sender, WizardSideBarListControlItemEventArgs e)
        {
            WizardSideBarListControlItem item = e.Item;
            if (((item.ItemType == ListItemType.Item) || (item.ItemType == ListItemType.AlternatingItem)) || ((item.ItemType == ListItemType.SelectedItem) || (item.ItemType == ListItemType.EditItem)))
            {
                IButtonControl control = item.FindControl(SideBarButtonID) as IButtonControl;
                if (control == null)
                {
                    if (!base.DesignMode)
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("Wizard_SideBar_Button_Not_Found", new object[] { DataListID, SideBarButtonID }));
                    }
                }
                else
                {
                    Button button = control as Button;
                    if (button != null)
                    {
                        button.UseSubmitBehavior = false;
                    }
                    WebControl control2 = control as WebControl;
                    if (control2 != null)
                    {
                        control2.TabIndex = this.TabIndex;
                    }
                    int index = 0;
                    WizardStepBase dataItem = item.DataItem as WizardStepBase;
                    if (dataItem != null)
                    {
                        if ((this.GetStepType(dataItem) == WizardStepType.Complete) && (control2 != null))
                        {
                            control2.Enabled = false;
                        }
                        this.RegisterSideBarDataListForRender();
                        if (dataItem.Title.Length > 0)
                        {
                            control.Text = dataItem.Title;
                        }
                        else
                        {
                            control.Text = dataItem.ID;
                        }
                        index = this.WizardSteps.IndexOf(dataItem);
                        control.CommandName = MoveToCommandName;
                        control.CommandArgument = index.ToString(NumberFormatInfo.InvariantInfo);
                        this.RegisterCommandEvents(control);
                    }
                }
            }
        }

        internal static string GetCustomContainerID(int index)
        {
            return ("__CustomNav" + index);
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        protected override IDictionary GetDesignModeState()
        {
            IDictionary designModeState = base.GetDesignModeState();
            this._designModeState = designModeState;
            int activeStepIndex = this.ActiveStepIndex;
            try
            {
                if ((activeStepIndex == -1) && (this.WizardSteps.Count > 0))
                {
                    this.ActiveStepIndex = 0;
                }
                this.RequiresControlsRecreation();
                this.EnsureChildControls();
                this.ApplyControlProperties();
                this._rendering.SetDesignModeState(designModeState);
                if (this.ShowCustomNavigationTemplate)
                {
                    BaseNavigationTemplateContainer container = this.CustomNavigationContainers[this.ActiveStep];
                    designModeState[CustomNextButtonID] = container.NextButton;
                    designModeState[CustomPreviousButtonID] = container.PreviousButton;
                    designModeState[CustomFinishButtonID] = container.FinishButton;
                    designModeState[CancelButtonID] = container.CancelButton;
                    designModeState["CustomNavigationControls"] = container.Controls;
                }
                if ((this.SideBarTemplate == null) && (this._sideBarList != null))
                {
                    this._sideBarList.ItemTemplate = this.CreateDefaultDataListItemTemplate();
                }
                designModeState[DataListID] = this._sideBarList;
                designModeState["TemplatedWizardSteps"] = this.TemplatedSteps;
            }
            finally
            {
                this.ActiveStepIndex = activeStepIndex;
            }
            return designModeState;
        }

        public ICollection GetHistory()
        {
            ArrayList list = new ArrayList();
            foreach (int num in this.History)
            {
                list.Add(this.WizardSteps[num]);
            }
            return list;
        }

        internal int GetPreviousStepIndex(bool popStack)
        {
            int num = -1;
            int activeStepIndex = this.ActiveStepIndex;
            if ((this._historyStack != null) && (this._historyStack.Count != 0))
            {
                if (popStack)
                {
                    num = this._historyStack.Pop();
                    if ((num == activeStepIndex) && (this._historyStack.Count > 0))
                    {
                        num = this._historyStack.Pop();
                    }
                }
                else
                {
                    num = this._historyStack.Peek();
                    if ((num == activeStepIndex) && (this._historyStack.Count > 1))
                    {
                        int item = this._historyStack.Pop();
                        num = this._historyStack.Peek();
                        this._historyStack.Push(item);
                    }
                }
                if (num == activeStepIndex)
                {
                    return -1;
                }
            }
            return num;
        }

        private WizardStepType GetStepType(int index)
        {
            WizardStepBase wizardStep = this.WizardSteps[index];
            return this.GetStepType(wizardStep, index);
        }

        private WizardStepType GetStepType(WizardStepBase step)
        {
            int index = this.WizardSteps.IndexOf(step);
            return this.GetStepType(step, index);
        }

        public WizardStepType GetStepType(WizardStepBase wizardStep, int index)
        {
            if (wizardStep.StepType != WizardStepType.Auto)
            {
                return wizardStep.StepType;
            }
            if ((this.WizardSteps.Count == 1) || ((index < (this.WizardSteps.Count - 1)) && (this.WizardSteps[index + 1].StepType == WizardStepType.Complete)))
            {
                return WizardStepType.Finish;
            }
            if (index == 0)
            {
                return WizardStepType.Start;
            }
            if (index == (this.WizardSteps.Count - 1))
            {
                return WizardStepType.Finish;
            }
            return WizardStepType.Step;
        }

        internal void InstantiateStepContentTemplate(TemplatedWizardStep step)
        {
            step.Controls.Clear();
            BaseContentTemplateContainer child = new BaseContentTemplateContainer(this, true);
            ITemplate contentTemplate = step.ContentTemplate;
            if (contentTemplate != null)
            {
                child.SetEnableTheming();
                contentTemplate.InstantiateIn(child.InnerCell);
            }
            step.ContentTemplateContainer = child;
            step.Controls.Add(child);
        }

        internal virtual void InstantiateStepContentTemplates()
        {
            this.TemplatedSteps.ForEach(step => this.InstantiateStepContentTemplate(step));
        }

        protected internal override void LoadControlState(object state)
        {
            Triplet triplet = state as Triplet;
            if (triplet != null)
            {
                base.LoadControlState(triplet.First);
                Array second = triplet.Second as Array;
                if (second != null)
                {
                    Array.Reverse(second);
                    this._historyStack = new Stack<int>(second.Cast<int>());
                }
                this.ActiveStepIndex = (int) triplet.Third;
            }
        }

        protected override void LoadViewState(object savedState)
        {
            if (savedState == null)
            {
                base.LoadViewState(null);
            }
            else
            {
                object[] objArray = (object[]) savedState;
                if (objArray.Length != 15)
                {
                    throw new ArgumentException(System.Web.SR.GetString("ViewState_InvalidViewState"));
                }
                base.LoadViewState(objArray[0]);
                if (objArray[1] != null)
                {
                    ((IStateManager) this.NavigationButtonStyle).LoadViewState(objArray[1]);
                }
                if (objArray[2] != null)
                {
                    ((IStateManager) this.SideBarButtonStyle).LoadViewState(objArray[2]);
                }
                if (objArray[3] != null)
                {
                    ((IStateManager) this.HeaderStyle).LoadViewState(objArray[3]);
                }
                if (objArray[4] != null)
                {
                    ((IStateManager) this.NavigationStyle).LoadViewState(objArray[4]);
                }
                if (objArray[5] != null)
                {
                    ((IStateManager) this.SideBarStyle).LoadViewState(objArray[5]);
                }
                if (objArray[6] != null)
                {
                    ((IStateManager) this.StepStyle).LoadViewState(objArray[6]);
                }
                if (objArray[7] != null)
                {
                    ((IStateManager) this.StartNextButtonStyle).LoadViewState(objArray[7]);
                }
                if (objArray[8] != null)
                {
                    ((IStateManager) this.StepNextButtonStyle).LoadViewState(objArray[8]);
                }
                if (objArray[9] != null)
                {
                    ((IStateManager) this.StepPreviousButtonStyle).LoadViewState(objArray[9]);
                }
                if (objArray[10] != null)
                {
                    ((IStateManager) this.FinishPreviousButtonStyle).LoadViewState(objArray[10]);
                }
                if (objArray[11] != null)
                {
                    ((IStateManager) this.FinishCompleteButtonStyle).LoadViewState(objArray[11]);
                }
                if (objArray[12] != null)
                {
                    ((IStateManager) this.CancelButtonStyle).LoadViewState(objArray[12]);
                }
                if (objArray[13] != null)
                {
                    ((IStateManager) base.ControlStyle).LoadViewState(objArray[13]);
                }
                if (objArray[14] != null)
                {
                    this.DisplaySideBar = (bool) objArray[14];
                }
            }
        }

        public void MoveTo(WizardStepBase wizardStep)
        {
            if (wizardStep == null)
            {
                throw new ArgumentNullException("wizardStep");
            }
            int index = this.WizardSteps.IndexOf(wizardStep);
            if (index == -1)
            {
                throw new ArgumentException(System.Web.SR.GetString("Wizard_Step_Not_In_Wizard"));
            }
            this.ActiveStepIndex = index;
        }

        private void MultiViewActiveViewChanged(object source, EventArgs e)
        {
            this.OnActiveStepChanged(this, EventArgs.Empty);
        }

        protected virtual void OnActiveStepChanged(object source, EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[_eventActiveStepChanged];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override bool OnBubbleEvent(object source, EventArgs e)
        {
            bool flag = false;
            CommandEventArgs args = e as CommandEventArgs;
            if (args != null)
            {
                if (string.Equals(CancelCommandName, args.CommandName, StringComparison.OrdinalIgnoreCase))
                {
                    this.OnCancelButtonClick(EventArgs.Empty);
                    return true;
                }
                int activeStepIndex = this.ActiveStepIndex;
                int nextStepIndex = activeStepIndex;
                bool flag2 = true;
                WizardStepType auto = WizardStepType.Auto;
                WizardStepBase step = this.WizardSteps[activeStepIndex];
                if (step is TemplatedWizardStep)
                {
                    flag2 = false;
                }
                else
                {
                    auto = this.GetStepType(step);
                }
                WizardNavigationEventArgs args2 = new WizardNavigationEventArgs(activeStepIndex, nextStepIndex);
                if (((this._commandSender != null) && (this.Page != null)) && !this.Page.IsValid)
                {
                    args2.Cancel = true;
                }
                bool flag3 = false;
                this._activeStepIndexSet = false;
                if (string.Equals(MoveNextCommandName, args.CommandName, StringComparison.OrdinalIgnoreCase))
                {
                    if ((flag2 && (auto != WizardStepType.Start)) && (auto != WizardStepType.Step))
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("Wizard_InvalidBubbleEvent", new object[] { MoveNextCommandName }));
                    }
                    if (activeStepIndex < (this.WizardSteps.Count - 1))
                    {
                        args2.SetNextStepIndex(activeStepIndex + 1);
                    }
                    this.OnNextButtonClick(args2);
                    flag = true;
                }
                else if (string.Equals(MovePreviousCommandName, args.CommandName, StringComparison.OrdinalIgnoreCase))
                {
                    if ((flag2 && (auto != WizardStepType.Step)) && (auto != WizardStepType.Finish))
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("Wizard_InvalidBubbleEvent", new object[] { MovePreviousCommandName }));
                    }
                    flag3 = true;
                    int previousStepIndex = this.GetPreviousStepIndex(false);
                    if (previousStepIndex != -1)
                    {
                        args2.SetNextStepIndex(previousStepIndex);
                    }
                    this.OnPreviousButtonClick(args2);
                    flag = true;
                }
                else if (string.Equals(MoveCompleteCommandName, args.CommandName, StringComparison.OrdinalIgnoreCase))
                {
                    if (flag2 && (auto != WizardStepType.Finish))
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("Wizard_InvalidBubbleEvent", new object[] { MoveCompleteCommandName }));
                    }
                    if (activeStepIndex < (this.WizardSteps.Count - 1))
                    {
                        args2.SetNextStepIndex(activeStepIndex + 1);
                    }
                    this.OnFinishButtonClick(args2);
                    flag = true;
                }
                else if (string.Equals(MoveToCommandName, args.CommandName, StringComparison.OrdinalIgnoreCase))
                {
                    nextStepIndex = int.Parse((string) args.CommandArgument, CultureInfo.InvariantCulture);
                    args2.SetNextStepIndex(nextStepIndex);
                    flag = true;
                }
                if (flag)
                {
                    if (!args2.Cancel)
                    {
                        if (!this._activeStepIndexSet && this.AllowNavigationToStep(args2.NextStepIndex))
                        {
                            if (flag3)
                            {
                                this.GetPreviousStepIndex(true);
                            }
                            this.ActiveStepIndex = args2.NextStepIndex;
                        }
                        return flag;
                    }
                    this.ActiveStepIndex = activeStepIndex;
                }
            }
            return flag;
        }

        protected virtual void OnCancelButtonClick(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[_eventCancelButtonClick];
            if (handler != null)
            {
                handler(this, e);
            }
            string cancelDestinationPageUrl = this.CancelDestinationPageUrl;
            if (!string.IsNullOrEmpty(cancelDestinationPageUrl))
            {
                this.Page.Response.Redirect(base.ResolveClientUrl(cancelDestinationPageUrl), false);
            }
        }

        private void OnCommand(object sender, CommandEventArgs e)
        {
            this._commandSender = sender as IButtonControl;
        }

        protected virtual void OnFinishButtonClick(WizardNavigationEventArgs e)
        {
            WizardNavigationEventHandler handler = (WizardNavigationEventHandler) base.Events[_eventFinishButtonClick];
            if (handler != null)
            {
                handler(this, e);
            }
            string finishDestinationPageUrl = this.FinishDestinationPageUrl;
            if (!string.IsNullOrEmpty(finishDestinationPageUrl))
            {
                this.Page.Response.Redirect(base.ResolveClientUrl(finishDestinationPageUrl), false);
            }
        }

        protected internal override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (((this.ActiveStepIndex == -1) && (this.WizardSteps.Count > 0)) && !base.DesignMode)
            {
                this.ActiveStepIndex = 0;
            }
            this.EnsureChildControls();
            if (this.Page != null)
            {
                this.Page.RegisterRequiresControlState(this);
            }
        }

        protected virtual void OnNextButtonClick(WizardNavigationEventArgs e)
        {
            WizardNavigationEventHandler handler = (WizardNavigationEventHandler) base.Events[_eventNextButtonClick];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnPreviousButtonClick(WizardNavigationEventArgs e)
        {
            WizardNavigationEventHandler handler = (WizardNavigationEventHandler) base.Events[_eventPreviousButtonClick];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnSideBarButtonClick(WizardNavigationEventArgs e)
        {
            WizardNavigationEventHandler handler = (WizardNavigationEventHandler) base.Events[_eventSideBarButtonClick];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        internal void OnWizardStepsChanged()
        {
            this.SetStepsAndDataBindSideBarList(this._sideBarList);
        }

        protected internal void RegisterCommandEvents(IButtonControl button)
        {
            if ((button != null) && button.CausesValidation)
            {
                button.Command += new CommandEventHandler(this.OnCommand);
            }
        }

        internal void RegisterCustomNavigationContainers(TemplatedWizardStep step)
        {
            this.InstantiateStepContentTemplate(step);
            if (!this.CustomNavigationContainers.ContainsKey(step))
            {
                BaseNavigationTemplateContainer container = null;
                string customContainerID = GetCustomContainerID(this.WizardSteps.IndexOf(step));
                if (step.CustomNavigationTemplate != null)
                {
                    container = this.CreateBaseNavigationTemplateContainer(customContainerID);
                    step.CustomNavigationTemplate.InstantiateIn(container);
                    step.CustomNavigationTemplateContainer = container;
                    container.RegisterButtonCommandEvents();
                }
                else
                {
                    container = this.CreateBaseNavigationTemplateContainer(customContainerID);
                    container.RegisterButtonCommandEvents();
                }
                this.CustomNavigationContainers[step] = container;
            }
        }

        internal void RegisterSideBarDataListForRender()
        {
            this._renderSideBarDataList = true;
        }

        protected internal override void Render(HtmlTextWriter writer)
        {
            if (this.Page != null)
            {
                this.Page.VerifyRenderingInServerForm(this);
            }
            this.EnsureChildControls();
            this.ApplyControlProperties();
            if ((this.ActiveStepIndex != -1) && (this.WizardSteps.Count != 0))
            {
                this.RenderContents(writer);
            }
        }

        internal void RequiresControlsRecreation()
        {
            if (base.ChildControlsCreated)
            {
                using (new WizardControlCollectionModifier(this))
                {
                    base.ChildControlsCreated = false;
                }
                this._rendering = null;
            }
        }

        protected internal override object SaveControlState()
        {
            int activeStepIndex = this.ActiveStepIndex;
            if (((this._historyStack == null) || (this._historyStack.Count == 0)) || (this._historyStack.Peek() != activeStepIndex))
            {
                this.History.Push(this.ActiveStepIndex);
            }
            object x = base.SaveControlState();
            bool flag = (this._historyStack != null) && (this._historyStack.Count > 0);
            if (((x != null) || flag) || (activeStepIndex != -1))
            {
                return new Triplet(x, flag ? this._historyStack.ToArray() : null, activeStepIndex);
            }
            return null;
        }

        protected override object SaveViewState()
        {
            object[] objArray = new object[15];
            objArray[0] = base.SaveViewState();
            objArray[1] = (this._navigationButtonStyle != null) ? ((IStateManager) this._navigationButtonStyle).SaveViewState() : null;
            objArray[2] = (this._sideBarButtonStyle != null) ? ((IStateManager) this._sideBarButtonStyle).SaveViewState() : null;
            objArray[3] = (this._headerStyle != null) ? ((IStateManager) this._headerStyle).SaveViewState() : null;
            objArray[4] = (this._navigationStyle != null) ? ((IStateManager) this._navigationStyle).SaveViewState() : null;
            objArray[5] = (this._sideBarStyle != null) ? ((IStateManager) this._sideBarStyle).SaveViewState() : null;
            objArray[6] = (this._stepStyle != null) ? ((IStateManager) this._stepStyle).SaveViewState() : null;
            objArray[7] = (this._startNextButtonStyle != null) ? ((IStateManager) this._startNextButtonStyle).SaveViewState() : null;
            objArray[8] = (this._stepNextButtonStyle != null) ? ((IStateManager) this._stepNextButtonStyle).SaveViewState() : null;
            objArray[9] = (this._stepPreviousButtonStyle != null) ? ((IStateManager) this._stepPreviousButtonStyle).SaveViewState() : null;
            objArray[10] = (this._finishPreviousButtonStyle != null) ? ((IStateManager) this._finishPreviousButtonStyle).SaveViewState() : null;
            objArray[11] = (this._finishCompleteButtonStyle != null) ? ((IStateManager) this._finishCompleteButtonStyle).SaveViewState() : null;
            objArray[12] = (this._cancelButtonStyle != null) ? ((IStateManager) this._cancelButtonStyle).SaveViewState() : null;
            objArray[13] = base.ControlStyleCreated ? ((IStateManager) base.ControlStyle).SaveViewState() : null;
            if (this.DisplaySideBar != this._displaySideBarDefault)
            {
                objArray[14] = this.DisplaySideBar;
            }
            for (int i = 0; i < 15; i++)
            {
                if (objArray[i] != null)
                {
                    return objArray;
                }
            }
            return null;
        }

        private void SetCancelButtonVisibility(BaseNavigationTemplateContainer container)
        {
            Control cancelButton = container.CancelButton as Control;
            if (cancelButton != null)
            {
                Control parent = cancelButton.Parent;
                if (parent != null)
                {
                    parent.Visible = this.DisplayCancelButton;
                }
                cancelButton.Visible = this.DisplayCancelButton;
            }
        }

        private void SetStepsAndDataBindSideBarList(IWizardSideBarListControl sideBarList)
        {
            if (sideBarList != null)
            {
                sideBarList.DataSource = this.WizardSteps;
                sideBarList.SelectedIndex = this.ActiveStepIndex;
                sideBarList.DataBind();
            }
        }

        protected override void TrackViewState()
        {
            base.TrackViewState();
            if (this._navigationButtonStyle != null)
            {
                ((IStateManager) this._navigationButtonStyle).TrackViewState();
            }
            if (this._sideBarButtonStyle != null)
            {
                ((IStateManager) this._sideBarButtonStyle).TrackViewState();
            }
            if (this._headerStyle != null)
            {
                ((IStateManager) this._headerStyle).TrackViewState();
            }
            if (this._navigationStyle != null)
            {
                ((IStateManager) this._navigationStyle).TrackViewState();
            }
            if (this._sideBarStyle != null)
            {
                ((IStateManager) this._sideBarStyle).TrackViewState();
            }
            if (this._stepStyle != null)
            {
                ((IStateManager) this._stepStyle).TrackViewState();
            }
            if (this._startNextButtonStyle != null)
            {
                ((IStateManager) this._startNextButtonStyle).TrackViewState();
            }
            if (this._stepPreviousButtonStyle != null)
            {
                ((IStateManager) this._stepPreviousButtonStyle).TrackViewState();
            }
            if (this._stepNextButtonStyle != null)
            {
                ((IStateManager) this._stepNextButtonStyle).TrackViewState();
            }
            if (this._finishPreviousButtonStyle != null)
            {
                ((IStateManager) this._finishPreviousButtonStyle).TrackViewState();
            }
            if (this._finishCompleteButtonStyle != null)
            {
                ((IStateManager) this._finishCompleteButtonStyle).TrackViewState();
            }
            if (this._cancelButtonStyle != null)
            {
                ((IStateManager) this._cancelButtonStyle).TrackViewState();
            }
            if (base.ControlStyleCreated)
            {
                ((IStateManager) base.ControlStyle).TrackViewState();
            }
        }

        private static void ValidateButtonType(ButtonType value)
        {
            if ((value < ButtonType.Button) || (value > ButtonType.Link))
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebSysDescription("Wizard_ActiveStep")]
        public WizardStepBase ActiveStep
        {
            get
            {
                if ((this.ActiveStepIndex < -1) || (this.ActiveStepIndex >= this.WizardSteps.Count))
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("Wizard_ActiveStepIndex_out_of_range"));
                }
                return (this.MultiView.GetActiveView() as WizardStepBase);
            }
        }

        [Themeable(false), WebSysDescription("Wizard_ActiveStepIndex"), DefaultValue(-1), WebCategory("Behavior")]
        public virtual int ActiveStepIndex
        {
            get
            {
                return this.MultiView.ActiveViewIndex;
            }
            set
            {
                if ((value < -1) || ((value >= this.WizardSteps.Count) && (base.ControlState >= ControlState.FrameworkInitialized)))
                {
                    throw new ArgumentOutOfRangeException("value", System.Web.SR.GetString("Wizard_ActiveStepIndex_out_of_range"));
                }
                if (this.MultiView.ActiveViewIndex != value)
                {
                    this.MultiView.ActiveViewIndex = value;
                    this._activeStepIndexSet = true;
                    if ((this._sideBarList != null) && (this.SideBarTemplate != null))
                    {
                        this._sideBarList.SelectedIndex = this.ActiveStepIndex;
                        this._sideBarList.DataBind();
                    }
                }
            }
        }

        [WebCategory("Appearance"), UrlProperty, WebSysDescription("Wizard_CancelButtonImageUrl"), DefaultValue(""), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public virtual string CancelButtonImageUrl
        {
            get
            {
                object obj2 = this.ViewState["CancelButtonImageUrl"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["CancelButtonImageUrl"] = value;
            }
        }

        [WebCategory("Styles"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), WebSysDescription("Wizard_CancelButtonStyle"), DefaultValue((string) null), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
        public Style CancelButtonStyle
        {
            get
            {
                if (this._cancelButtonStyle == null)
                {
                    this._cancelButtonStyle = new Style();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._cancelButtonStyle).TrackViewState();
                    }
                }
                return this._cancelButtonStyle;
            }
        }

        [Localizable(true), WebSysDefaultValue("Wizard_Default_CancelButtonText"), WebCategory("Appearance"), WebSysDescription("Wizard_CancelButtonText")]
        public virtual string CancelButtonText
        {
            get
            {
                string str = this.ViewState["CancelButtonText"] as string;
                if (str != null)
                {
                    return str;
                }
                return System.Web.SR.GetString("Wizard_Default_CancelButtonText");
            }
            set
            {
                if (value != this.CancelButtonText)
                {
                    this.ViewState["CancelButtonText"] = value;
                }
            }
        }

        [WebSysDescription("Wizard_CancelButtonType"), DefaultValue(0), WebCategory("Appearance")]
        public virtual ButtonType CancelButtonType
        {
            get
            {
                object obj2 = this.ViewState["CancelButtonType"];
                if (obj2 != null)
                {
                    return (ButtonType) obj2;
                }
                return ButtonType.Button;
            }
            set
            {
                ValidateButtonType(value);
                this.ViewState["CancelButtonType"] = value;
            }
        }

        [Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), DefaultValue(""), Themeable(false), WebCategory("Behavior"), WebSysDescription("Wizard_CancelDestinationPageUrl"), UrlProperty]
        public virtual string CancelDestinationPageUrl
        {
            get
            {
                string str = this.ViewState["CancelDestinationPageUrl"] as string;
                if (str != null)
                {
                    return str;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["CancelDestinationPageUrl"] = value;
            }
        }

        [DefaultValue(0), WebCategory("Layout"), WebSysDescription("Wizard_CellPadding")]
        public virtual int CellPadding
        {
            get
            {
                if (!base.ControlStyleCreated)
                {
                    return 0;
                }
                return ((TableStyle) base.ControlStyle).CellPadding;
            }
            set
            {
                ((TableStyle) base.ControlStyle).CellPadding = value;
            }
        }

        [DefaultValue(0), WebCategory("Layout"), WebSysDescription("Wizard_CellSpacing")]
        public virtual int CellSpacing
        {
            get
            {
                if (!base.ControlStyleCreated)
                {
                    return 0;
                }
                return ((TableStyle) base.ControlStyle).CellSpacing;
            }
            set
            {
                ((TableStyle) base.ControlStyle).CellSpacing = value;
            }
        }

        internal Dictionary<WizardStepBase, BaseNavigationTemplateContainer> CustomNavigationContainers
        {
            get
            {
                if (this._customNavigationContainers == null)
                {
                    this._customNavigationContainers = new Dictionary<WizardStepBase, BaseNavigationTemplateContainer>();
                }
                return this._customNavigationContainers;
            }
        }

        private ITemplate CustomNavigationTemplate
        {
            get
            {
                TemplatedWizardStep activeStep = this.ActiveStep as TemplatedWizardStep;
                if (activeStep != null)
                {
                    return activeStep.CustomNavigationTemplate;
                }
                return null;
            }
        }

        [WebSysDescription("Wizard_DisplayCancelButton"), DefaultValue(false), Themeable(false), WebCategory("Behavior")]
        public virtual bool DisplayCancelButton
        {
            get
            {
                object obj2 = this.ViewState["DisplayCancelButton"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                this.ViewState["DisplayCancelButton"] = value;
            }
        }

        [WebSysDescription("Wizard_DisplaySideBar"), WebCategory("Behavior"), DefaultValue(true), Themeable(false)]
        public virtual bool DisplaySideBar
        {
            get
            {
                return this._displaySideBar;
            }
            set
            {
                if (value != this._displaySideBar)
                {
                    this._displaySideBar = value;
                    this._sideBarTableCell = null;
                    this.RequiresControlsRecreation();
                }
            }
        }

        [WebCategory("Appearance"), DefaultValue(""), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), WebSysDescription("Wizard_FinishCompleteButtonImageUrl"), UrlProperty]
        public virtual string FinishCompleteButtonImageUrl
        {
            get
            {
                object obj2 = this.ViewState["FinishCompleteButtonImageUrl"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["FinishCompleteButtonImageUrl"] = value;
            }
        }

        [WebCategory("Styles"), WebSysDescription("Wizard_FinishCompleteButtonStyle"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
        public Style FinishCompleteButtonStyle
        {
            get
            {
                if (this._finishCompleteButtonStyle == null)
                {
                    this._finishCompleteButtonStyle = new Style();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._finishCompleteButtonStyle).TrackViewState();
                    }
                }
                return this._finishCompleteButtonStyle;
            }
        }

        [WebSysDefaultValue("Wizard_Default_FinishButtonText"), WebSysDescription("Wizard_FinishCompleteButtonText"), Localizable(true), WebCategory("Appearance")]
        public virtual string FinishCompleteButtonText
        {
            get
            {
                string str = this.ViewState["FinishCompleteButtonText"] as string;
                if (str != null)
                {
                    return str;
                }
                return System.Web.SR.GetString("Wizard_Default_FinishButtonText");
            }
            set
            {
                this.ViewState["FinishCompleteButtonText"] = value;
            }
        }

        [DefaultValue(0), WebCategory("Appearance"), WebSysDescription("Wizard_FinishCompleteButtonType")]
        public virtual ButtonType FinishCompleteButtonType
        {
            get
            {
                object obj2 = this.ViewState["FinishCompleteButtonType"];
                if (obj2 != null)
                {
                    return (ButtonType) obj2;
                }
                return ButtonType.Button;
            }
            set
            {
                ValidateButtonType(value);
                this.ViewState["FinishCompleteButtonType"] = value;
            }
        }

        [DefaultValue(""), UrlProperty, Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), Themeable(false), WebCategory("Behavior"), WebSysDescription("Wizard_FinishDestinationPageUrl")]
        public virtual string FinishDestinationPageUrl
        {
            get
            {
                object obj2 = this.ViewState["FinishDestinationPageUrl"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["FinishDestinationPageUrl"] = value;
            }
        }

        [WebSysDescription("Wizard_FinishNavigationTemplate"), TemplateContainer(typeof(Wizard)), Browsable(false), DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty)]
        public virtual ITemplate FinishNavigationTemplate
        {
            get
            {
                return this._finishNavigationTemplate;
            }
            set
            {
                this._finishNavigationTemplate = value;
                this.RequiresControlsRecreation();
            }
        }

        [Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty, DefaultValue(""), WebCategory("Appearance"), WebSysDescription("Wizard_FinishPreviousButtonImageUrl")]
        public virtual string FinishPreviousButtonImageUrl
        {
            get
            {
                object obj2 = this.ViewState["FinishPreviousButtonImageUrl"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["FinishPreviousButtonImageUrl"] = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), WebSysDescription("Wizard_FinishPreviousButtonStyle"), PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Styles"), DefaultValue((string) null), NotifyParentProperty(true)]
        public Style FinishPreviousButtonStyle
        {
            get
            {
                if (this._finishPreviousButtonStyle == null)
                {
                    this._finishPreviousButtonStyle = new Style();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._finishPreviousButtonStyle).TrackViewState();
                    }
                }
                return this._finishPreviousButtonStyle;
            }
        }

        [WebCategory("Appearance"), WebSysDescription("Wizard_FinishPreviousButtonText"), Localizable(true), WebSysDefaultValue("Wizard_Default_StepPreviousButtonText")]
        public virtual string FinishPreviousButtonText
        {
            get
            {
                string str = this.ViewState["FinishPreviousButtonText"] as string;
                if (str != null)
                {
                    return str;
                }
                return System.Web.SR.GetString("Wizard_Default_StepPreviousButtonText");
            }
            set
            {
                this.ViewState["FinishPreviousButtonText"] = value;
            }
        }

        [WebCategory("Appearance"), WebSysDescription("Wizard_FinishPreviousButtonType"), DefaultValue(0)]
        public virtual ButtonType FinishPreviousButtonType
        {
            get
            {
                object obj2 = this.ViewState["FinishPreviousButtonType"];
                if (obj2 != null)
                {
                    return (ButtonType) obj2;
                }
                return ButtonType.Button;
            }
            set
            {
                ValidateButtonType(value);
                this.ViewState["FinishPreviousButtonType"] = value;
            }
        }

        [PersistenceMode(PersistenceMode.InnerProperty), NotifyParentProperty(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), WebSysDescription("WebControl_HeaderStyle"), WebCategory("Styles"), DefaultValue((string) null)]
        public TableItemStyle HeaderStyle
        {
            get
            {
                if (this._headerStyle == null)
                {
                    this._headerStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._headerStyle).TrackViewState();
                    }
                }
                return this._headerStyle;
            }
        }

        [TemplateContainer(typeof(Wizard)), WebSysDescription("WebControl_HeaderTemplate"), PersistenceMode(PersistenceMode.InnerProperty), Browsable(false), DefaultValue((string) null)]
        public virtual ITemplate HeaderTemplate
        {
            get
            {
                return this._headerTemplate;
            }
            set
            {
                this._headerTemplate = value;
                this.RequiresControlsRecreation();
            }
        }

        [Localizable(true), DefaultValue(""), WebCategory("Appearance"), WebSysDescription("Wizard_HeaderText")]
        public virtual string HeaderText
        {
            get
            {
                string str = this.ViewState["HeaderText"] as string;
                if (str != null)
                {
                    return str;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["HeaderText"] = value;
            }
        }

        private Stack<int> History
        {
            get
            {
                if (this._historyStack == null)
                {
                    this._historyStack = new Stack<int>();
                }
                return this._historyStack;
            }
        }

        private bool IsMacIE5
        {
            get
            {
                if (!this._isMacIE.HasValue && !base.DesignMode)
                {
                    HttpBrowserCapabilities browser = null;
                    if (this.Page != null)
                    {
                        browser = this.Page.Request.Browser;
                    }
                    else
                    {
                        HttpContext current = HttpContext.Current;
                        if (current != null)
                        {
                            browser = current.Request.Browser;
                        }
                    }
                    this._isMacIE = new bool?(((browser != null) && (browser.Type == "IE5")) && (browser.Platform == "MacPPC"));
                }
                return this._isMacIE.Value;
            }
        }

        [WebSysDescription("Wizard_LayoutTemplate"), TemplateContainer(typeof(Wizard)), PersistenceMode(PersistenceMode.InnerProperty), Browsable(false), DefaultValue((string) null)]
        public virtual ITemplate LayoutTemplate
        {
            get
            {
                return this._layoutTemplate;
            }
            set
            {
                this._layoutTemplate = value;
                this.RequiresControlsRecreation();
            }
        }

        internal System.Web.UI.WebControls.MultiView MultiView
        {
            get
            {
                if (this._multiView == null)
                {
                    this._multiView = new System.Web.UI.WebControls.MultiView();
                    this._multiView.EnableTheming = true;
                    this._multiView.ID = "WizardMultiView";
                    this._multiView.ActiveViewChanged += new EventHandler(this.MultiViewActiveViewChanged);
                    this._multiView.IgnoreBubbleEvents();
                }
                return this._multiView;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), WebCategory("Styles"), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("Wizard_NavigationButtonStyle"), DefaultValue((string) null)]
        public Style NavigationButtonStyle
        {
            get
            {
                if (this._navigationButtonStyle == null)
                {
                    this._navigationButtonStyle = new Style();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._navigationButtonStyle).TrackViewState();
                    }
                }
                return this._navigationButtonStyle;
            }
        }

        [DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("Wizard_NavigationStyle"), WebCategory("Styles")]
        public TableItemStyle NavigationStyle
        {
            get
            {
                if (this._navigationStyle == null)
                {
                    this._navigationStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._navigationStyle).TrackViewState();
                    }
                }
                return this._navigationStyle;
            }
        }

        internal bool ShouldRenderChildControl
        {
            get
            {
                if (base.DesignMode)
                {
                    if (this._designModeState == null)
                    {
                        return true;
                    }
                    object obj2 = this._designModeState["ShouldRenderWizardSteps"];
                    if (obj2 != null)
                    {
                        return (bool) obj2;
                    }
                }
                return true;
            }
        }

        internal virtual bool ShowCustomNavigationTemplate
        {
            get
            {
                return (this.CustomNavigationTemplate != null);
            }
        }

        [DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), WebSysDescription("Wizard_SideBarButtonStyle"), WebCategory("Styles"), PersistenceMode(PersistenceMode.InnerProperty)]
        public Style SideBarButtonStyle
        {
            get
            {
                if (this._sideBarButtonStyle == null)
                {
                    this._sideBarButtonStyle = new Style();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._sideBarButtonStyle).TrackViewState();
                    }
                }
                return this._sideBarButtonStyle;
            }
        }

        private bool SideBarEnabled
        {
            get
            {
                return ((this._sideBarList != null) && this.DisplaySideBar);
            }
        }

        private IWizardSideBarListControl SideBarList
        {
            get
            {
                return this._sideBarList;
            }
        }

        [WebSysDescription("Wizard_SideBarStyle"), WebCategory("Styles"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
        public TableItemStyle SideBarStyle
        {
            get
            {
                if (this._sideBarStyle == null)
                {
                    this._sideBarStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._sideBarStyle).TrackViewState();
                    }
                }
                return this._sideBarStyle;
            }
        }

        [TemplateContainer(typeof(Wizard)), WebSysDescription("Wizard_SideBarTemplate"), Browsable(false), DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty)]
        public virtual ITemplate SideBarTemplate
        {
            get
            {
                return this._sideBarTemplate;
            }
            set
            {
                this._sideBarTemplate = value;
                this._sideBarTableCell = null;
                this.RequiresControlsRecreation();
            }
        }

        [WebSysDescription("WebControl_SkipLinkText"), WebSysDefaultValue("Wizard_Default_SkipToContentText"), Localizable(true), WebCategory("Appearance")]
        public virtual string SkipLinkText
        {
            get
            {
                string skipLinkTextInternal = this.SkipLinkTextInternal;
                if (skipLinkTextInternal != null)
                {
                    return skipLinkTextInternal;
                }
                return System.Web.SR.GetString("Wizard_Default_SkipToContentText");
            }
            set
            {
                this.ViewState["SkipLinkText"] = value;
            }
        }

        internal string SkipLinkTextInternal
        {
            get
            {
                return (this.ViewState["SkipLinkText"] as string);
            }
        }

        [PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("Wizard_StartNavigationTemplate"), TemplateContainer(typeof(Wizard)), Browsable(false), DefaultValue((string) null)]
        public virtual ITemplate StartNavigationTemplate
        {
            get
            {
                return this._startNavigationTemplate;
            }
            set
            {
                this._startNavigationTemplate = value;
                this.RequiresControlsRecreation();
            }
        }

        [DefaultValue(""), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), WebSysDescription("Wizard_StartNextButtonImageUrl"), UrlProperty, WebCategory("Appearance")]
        public virtual string StartNextButtonImageUrl
        {
            get
            {
                object obj2 = this.ViewState["StartNextButtonImageUrl"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["StartNextButtonImageUrl"] = value;
            }
        }

        [WebCategory("Styles"), DefaultValue((string) null), WebSysDescription("Wizard_StartNextButtonStyle"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
        public Style StartNextButtonStyle
        {
            get
            {
                if (this._startNextButtonStyle == null)
                {
                    this._startNextButtonStyle = new Style();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._startNextButtonStyle).TrackViewState();
                    }
                }
                return this._startNextButtonStyle;
            }
        }

        [WebSysDescription("Wizard_StartNextButtonText"), WebSysDefaultValue("Wizard_Default_StepNextButtonText"), Localizable(true), WebCategory("Appearance")]
        public virtual string StartNextButtonText
        {
            get
            {
                string str = this.ViewState["StartNextButtonText"] as string;
                if (str != null)
                {
                    return str;
                }
                return System.Web.SR.GetString("Wizard_Default_StepNextButtonText");
            }
            set
            {
                this.ViewState["StartNextButtonText"] = value;
            }
        }

        [DefaultValue(0), WebCategory("Appearance"), WebSysDescription("Wizard_StartNextButtonType")]
        public virtual ButtonType StartNextButtonType
        {
            get
            {
                object obj2 = this.ViewState["StartNextButtonType"];
                if (obj2 != null)
                {
                    return (ButtonType) obj2;
                }
                return ButtonType.Button;
            }
            set
            {
                ValidateButtonType(value);
                this.ViewState["StartNextButtonType"] = value;
            }
        }

        [Browsable(false), WebSysDescription("Wizard_StepNavigationTemplate"), DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(Wizard))]
        public virtual ITemplate StepNavigationTemplate
        {
            get
            {
                return this._stepNavigationTemplate;
            }
            set
            {
                this._stepNavigationTemplate = value;
                this.RequiresControlsRecreation();
            }
        }

        [DefaultValue(""), UrlProperty, Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), WebCategory("Appearance"), WebSysDescription("Wizard_StepNextButtonImageUrl")]
        public virtual string StepNextButtonImageUrl
        {
            get
            {
                object obj2 = this.ViewState["StepNextButtonImageUrl"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["StepNextButtonImageUrl"] = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), WebSysDescription("Wizard_StepNextButtonStyle"), WebCategory("Styles"), DefaultValue((string) null), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
        public Style StepNextButtonStyle
        {
            get
            {
                if (this._stepNextButtonStyle == null)
                {
                    this._stepNextButtonStyle = new Style();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._stepNextButtonStyle).TrackViewState();
                    }
                }
                return this._stepNextButtonStyle;
            }
        }

        [WebSysDefaultValue("Wizard_Default_StepNextButtonText"), WebSysDescription("Wizard_StepNextButtonText"), Localizable(true), WebCategory("Appearance")]
        public virtual string StepNextButtonText
        {
            get
            {
                string str = this.ViewState["StepNextButtonText"] as string;
                if (str != null)
                {
                    return str;
                }
                return System.Web.SR.GetString("Wizard_Default_StepNextButtonText");
            }
            set
            {
                this.ViewState["StepNextButtonText"] = value;
            }
        }

        [DefaultValue(0), WebSysDescription("Wizard_StepNextButtonType"), WebCategory("Appearance")]
        public virtual ButtonType StepNextButtonType
        {
            get
            {
                object obj2 = this.ViewState["StepNextButtonType"];
                if (obj2 != null)
                {
                    return (ButtonType) obj2;
                }
                return ButtonType.Button;
            }
            set
            {
                ValidateButtonType(value);
                this.ViewState["StepNextButtonType"] = value;
            }
        }

        [DefaultValue(""), WebCategory("Appearance"), UrlProperty, Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), WebSysDescription("Wizard_StepPreviousButtonImageUrl")]
        public virtual string StepPreviousButtonImageUrl
        {
            get
            {
                object obj2 = this.ViewState["StepPreviousButtonImageUrl"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["StepPreviousButtonImageUrl"] = value;
            }
        }

        [WebSysDescription("Wizard_StepPreviousButtonStyle"), WebCategory("Styles"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
        public Style StepPreviousButtonStyle
        {
            get
            {
                if (this._stepPreviousButtonStyle == null)
                {
                    this._stepPreviousButtonStyle = new Style();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._stepPreviousButtonStyle).TrackViewState();
                    }
                }
                return this._stepPreviousButtonStyle;
            }
        }

        [WebCategory("Appearance"), WebSysDescription("Wizard_StepPreviousButtonText"), Localizable(true), WebSysDefaultValue("Wizard_Default_StepPreviousButtonText")]
        public virtual string StepPreviousButtonText
        {
            get
            {
                string str = this.ViewState["StepPreviousButtonText"] as string;
                if (str != null)
                {
                    return str;
                }
                return System.Web.SR.GetString("Wizard_Default_StepPreviousButtonText");
            }
            set
            {
                this.ViewState["StepPreviousButtonText"] = value;
            }
        }

        [WebSysDescription("Wizard_StepPreviousButtonType"), DefaultValue(0), WebCategory("Appearance")]
        public virtual ButtonType StepPreviousButtonType
        {
            get
            {
                object obj2 = this.ViewState["StepPreviousButtonType"];
                if (obj2 != null)
                {
                    return (ButtonType) obj2;
                }
                return ButtonType.Button;
            }
            set
            {
                ValidateButtonType(value);
                this.ViewState["StepPreviousButtonType"] = value;
            }
        }

        [NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Styles"), WebSysDescription("Wizard_StepStyle"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public TableItemStyle StepStyle
        {
            get
            {
                if (this._stepStyle == null)
                {
                    this._stepStyle = new TableItemStyle();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._stepStyle).TrackViewState();
                    }
                }
                return this._stepStyle;
            }
        }

        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Table;
            }
        }

        internal List<TemplatedWizardStep> TemplatedSteps
        {
            get
            {
                if (this._templatedSteps == null)
                {
                    this._templatedSteps = new List<TemplatedWizardStep>();
                }
                return this._templatedSteps;
            }
        }

        [Themeable(false), WebSysDescription("Wizard_WizardSteps"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty), Editor("System.Web.UI.Design.WebControls.WizardStepCollectionEditor,System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public virtual WizardStepCollection WizardSteps
        {
            get
            {
                if (this._wizardStepCollection == null)
                {
                    this._wizardStepCollection = new WizardStepCollection(this);
                }
                return this._wizardStepCollection;
            }
        }

        private class AccessibleTableCell : Wizard.InternalTableCell
        {
            internal AccessibleTableCell(Wizard owner) : base(owner)
            {
            }

            protected internal override void RenderChildren(HtmlTextWriter writer)
            {
                bool flag = !string.IsNullOrEmpty(base._owner.SkipLinkText) && !base._owner.DesignMode;
                string str = base._owner.ClientID + "_SkipLink";
                if (flag)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Href, "#" + str);
                    writer.RenderBeginTag(HtmlTextWriterTag.A);
                    writer.AddAttribute(HtmlTextWriterAttribute.Alt, base._owner.SkipLinkText);
                    writer.AddAttribute(HtmlTextWriterAttribute.Height, "0");
                    writer.AddAttribute(HtmlTextWriterAttribute.Width, "0");
                    writer.AddStyleAttribute(HtmlTextWriterStyle.BorderWidth, "0px");
                    writer.AddAttribute(HtmlTextWriterAttribute.Src, base.SpacerImageUrl);
                    writer.RenderBeginTag(HtmlTextWriterTag.Img);
                    writer.RenderEndTag();
                    writer.RenderEndTag();
                }
                base.RenderChildren(writer);
                if (flag)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Id, str);
                    writer.RenderBeginTag(HtmlTextWriterTag.A);
                    writer.RenderEndTag();
                }
            }
        }

        internal class BaseContentTemplateContainer : Wizard.BlockControl
        {
            private bool _useInnerTable;

            internal BaseContentTemplateContainer(Wizard owner, bool useInnerTable) : base(owner)
            {
                this._useInnerTable = useInnerTable;
                if (useInnerTable)
                {
                    base.Table.Width = Unit.Percentage(100.0);
                    base.Table.Height = Unit.Percentage(100.0);
                }
                else
                {
                    this.Controls.Clear();
                }
            }

            internal void AddChildControl(Control c)
            {
                this.Container.Controls.Add(c);
            }

            internal Control Container
            {
                get
                {
                    if (!this._useInnerTable)
                    {
                        return this;
                    }
                    return base.InnerCell;
                }
            }
        }

        internal class BaseNavigationTemplateContainer : WebControl, INonBindingContainer, INamingContainer
        {
            private IButtonControl _cancelButton;
            private IButtonControl _finishButton;
            private IButtonControl _nextButton;
            private Wizard _owner;
            private IButtonControl _previousButton;

            internal BaseNavigationTemplateContainer(Wizard owner)
            {
                this._owner = owner;
            }

            internal void ApplyButtonStyle(Style finishStyle, Style prevStyle, Style nextStyle, Style cancelStyle)
            {
                if (this.FinishButton != null)
                {
                    this.ApplyButtonStyleInternal(this.FinishButton, finishStyle);
                }
                if (this.PreviousButton != null)
                {
                    this.ApplyButtonStyleInternal(this.PreviousButton, prevStyle);
                }
                if (this.NextButton != null)
                {
                    this.ApplyButtonStyleInternal(this.NextButton, nextStyle);
                }
                if (this.CancelButton != null)
                {
                    this.ApplyButtonStyleInternal(this.CancelButton, cancelStyle);
                }
            }

            protected void ApplyButtonStyleInternal(IButtonControl control, Style buttonStyle)
            {
                WebControl control2 = control as WebControl;
                if (control2 != null)
                {
                    control2.ApplyStyle(buttonStyle);
                    control2.ControlStyle.MergeWith(this.Owner.NavigationButtonStyle);
                }
            }

            public override void Focus()
            {
                throw new NotSupportedException(System.Web.SR.GetString("NoFocusSupport", new object[] { base.GetType().Name }));
            }

            internal void RegisterButtonCommandEvents()
            {
                this.Owner.RegisterCommandEvents(this.NextButton);
                this.Owner.RegisterCommandEvents(this.FinishButton);
                this.Owner.RegisterCommandEvents(this.PreviousButton);
                this.Owner.RegisterCommandEvents(this.CancelButton);
            }

            protected internal override void Render(HtmlTextWriter writer)
            {
                this.RenderContents(writer);
            }

            internal void SetEnableTheming()
            {
                this.EnableTheming = this._owner.EnableTheming;
            }

            internal IButtonControl CancelButton
            {
                get
                {
                    if (this._cancelButton == null)
                    {
                        this._cancelButton = this.FindControl(Wizard.CancelButtonID) as IButtonControl;
                    }
                    return this._cancelButton;
                }
                set
                {
                    this._cancelButton = value;
                }
            }

            internal IButtonControl FinishButton
            {
                get
                {
                    if (this._finishButton == null)
                    {
                        this._finishButton = this.FindControl(Wizard.FinishButtonID) as IButtonControl;
                    }
                    return this._finishButton;
                }
                set
                {
                    this._finishButton = value;
                }
            }

            internal virtual IButtonControl NextButton
            {
                get
                {
                    if (this._nextButton == null)
                    {
                        this._nextButton = this.FindControl(Wizard.StepNextButtonID) as IButtonControl;
                    }
                    return this._nextButton;
                }
                set
                {
                    this._nextButton = value;
                }
            }

            internal Wizard Owner
            {
                get
                {
                    return this._owner;
                }
            }

            internal virtual IButtonControl PreviousButton
            {
                get
                {
                    if (this._previousButton == null)
                    {
                        this._previousButton = this.FindControl(Wizard.StepPreviousButtonID) as IButtonControl;
                    }
                    return this._previousButton;
                }
                set
                {
                    this._previousButton = value;
                }
            }
        }

        internal abstract class BlockControl : WebControl, INonBindingContainer, INamingContainer
        {
            internal TableCell _cell;
            internal Wizard _owner;
            private System.Web.UI.WebControls.Table _table;

            internal BlockControl(Wizard owner)
            {
                this._owner = owner;
                this._table = new WizardDefaultInnerTable();
                this._table.EnableTheming = false;
                this.Controls.Add(this._table);
                TableRow child = new TableRow();
                this._table.Controls.Add(child);
                this._cell = new TableCell();
                this._cell.Height = Unit.Percentage(100.0);
                this._cell.Width = Unit.Percentage(100.0);
                child.Controls.Add(this._cell);
                this.HandleMacIECellHeight();
                base.PreventAutoID();
            }

            protected override Style CreateControlStyle()
            {
                return new TableItemStyle(this.ViewState);
            }

            public override void Focus()
            {
                throw new NotSupportedException(System.Web.SR.GetString("NoFocusSupport", new object[] { base.GetType().Name }));
            }

            internal void HandleMacIECellHeight()
            {
                if (!this._owner.DesignMode && this._owner.IsMacIE5)
                {
                    this._cell.Height = Unit.Pixel(1);
                }
            }

            protected internal override void Render(HtmlTextWriter writer)
            {
                this.RenderContents(writer);
            }

            internal void SetEnableTheming()
            {
                this._cell.EnableTheming = this._owner.EnableTheming;
            }

            internal TableCell InnerCell
            {
                get
                {
                    return this._cell;
                }
            }

            protected System.Web.UI.WebControls.Table Table
            {
                get
                {
                    return this._table;
                }
            }
        }

        private class DataListItemTemplate : ITemplate
        {
            private Wizard _owner;

            internal DataListItemTemplate(Wizard owner)
            {
                this._owner = owner;
            }

            public void InstantiateIn(Control container)
            {
                LinkButton child = new LinkButton();
                container.Controls.Add(child);
                child.ID = Wizard.SideBarButtonID;
                if (this._owner.DesignMode)
                {
                    child.MergeStyle(this._owner.SideBarButtonStyle);
                }
            }
        }

        private class DefaultSideBarTemplate : ITemplate
        {
            private Wizard _owner;

            internal DefaultSideBarTemplate(Wizard owner)
            {
                this._owner = owner;
            }

            public void InstantiateIn(Control container)
            {
                Control child = null;
                if (this._owner.SideBarList == null)
                {
                    DataList list = new DataList {
                        ID = Wizard.DataListID
                    };
                    list.SelectedItemStyle.Font.Bold = true;
                    list.ItemTemplate = this._owner.CreateDefaultDataListItemTemplate();
                    child = list;
                }
                else
                {
                    child = (Control) this._owner.SideBarList;
                }
                container.Controls.Add(child);
            }
        }

        private class FinishNavigationTemplateContainer : Wizard.BaseNavigationTemplateContainer
        {
            private IButtonControl _previousButton;

            internal FinishNavigationTemplateContainer(Wizard owner) : base(owner)
            {
            }

            internal override IButtonControl PreviousButton
            {
                get
                {
                    if (this._previousButton == null)
                    {
                        this._previousButton = this.FindControl(Wizard.FinishPreviousButtonID) as IButtonControl;
                    }
                    return this._previousButton;
                }
                set
                {
                    this._previousButton = value;
                }
            }
        }

        private class InternalTableCell : TableCell, INonBindingContainer, INamingContainer
        {
            protected Wizard _owner;

            internal InternalTableCell(Wizard owner)
            {
                this._owner = owner;
            }

            protected override void AddAttributesToRender(HtmlTextWriter writer)
            {
                if (base.ControlStyleCreated && !base.ControlStyle.IsEmpty)
                {
                    base.ControlStyle.AddAttributesToRender(writer, this);
                }
            }
        }

        internal class LayoutTemplateWizardRendering : Wizard.WizardRenderingBase
        {
            private Literal _headerLiteral;
            private WizardContainer _layoutContainer;

            public LayoutTemplateWizardRendering(Wizard wizard) : base(wizard)
            {
            }

            public override void ApplyControlProperties()
            {
                this.ApplyControlProperties_Header();
                base.ApplyControlProperties_Sidebar();
                this.ApplyControlProperties_Navigation();
            }

            private void ApplyControlProperties_Header()
            {
                if (base.Owner.HeaderTemplate == null)
                {
                    if (this._headerLiteral != null)
                    {
                        this._headerLiteral.Text = base.Owner.HeaderText;
                    }
                    else if (!string.IsNullOrEmpty(base.Owner.HeaderText))
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("Wizard_Header_Placeholder_Must_Be_Specified_For_HeaderText", new object[] { base.Owner.ID, Wizard.HeaderPlaceholderId }));
                    }
                }
            }

            private void ApplyControlProperties_Navigation()
            {
                base.ApplyNavigationTemplateProperties();
                base.ApplyCustomNavigationTemplateProperties();
            }

            public override void CreateControlHierarchy()
            {
                this._layoutContainer = new WizardContainer();
                base.Owner.LayoutTemplate.InstantiateIn(this._layoutContainer);
                using (new Wizard.WizardControlCollectionModifier(base.Owner))
                {
                    base.Owner.Controls.Add(this._layoutContainer);
                }
                this.CreateControlHierarchy_Header(this._layoutContainer);
                this.CreateControlHierarchy_SideBar(this._layoutContainer);
                this.CreateControlHierarchy_WizardStep(this._layoutContainer);
                this.CreateControlHierarchy_Navigation(this._layoutContainer);
            }

            private void CreateControlHierarchy_Header(Control layoutContainer)
            {
                Control placeholder = layoutContainer.FindControl(Wizard.HeaderPlaceholderId);
                if (base.Owner.HeaderTemplate != null)
                {
                    if (placeholder == null)
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("Wizard_Header_Placeholder_Must_Be_Specified_For_HeaderTemplate", new object[] { base.Owner.ID, Wizard.HeaderPlaceholderId }));
                    }
                    ReplacePlaceholderWithTemplateInstance(layoutContainer, placeholder, base.Owner.HeaderTemplate);
                }
                else if (placeholder != null)
                {
                    this._headerLiteral = new Literal();
                    ReplacePlaceholderWithControl(layoutContainer, placeholder, this._headerLiteral);
                }
            }

            private void CreateControlHierarchy_Navigation(Control layoutContainer)
            {
                Control placeholder = layoutContainer.FindControl(Wizard.NavigationPlaceholderId);
                if (placeholder == null)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("Wizard_Navigation_Placeholder_Must_Be_Specified", new object[] { base.Owner.ID, Wizard.NavigationPlaceholderId }));
                }
                Control replacement = new Control();
                ReplacePlaceholderWithControl(layoutContainer, placeholder, replacement);
                base.CreateNavigationControlHierarchy(replacement);
            }

            private void CreateControlHierarchy_SideBar(Control layoutContainer)
            {
                if (base.Owner.DisplaySideBar)
                {
                    Control placeholder = layoutContainer.FindControl(Wizard.SideBarPlaceholderId);
                    if (placeholder == null)
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("Wizard_Sidebar_Placeholder_Must_Be_Specified", new object[] { base.Owner.ID, Wizard.SideBarPlaceholderId }));
                    }
                    ITemplate template = base.Owner.SideBarTemplate ?? base.Owner.CreateDefaultSideBarTemplate();
                    ReplacePlaceholderWithTemplateInstance(layoutContainer, placeholder, template);
                    base.CreateControlHierarchy_CleanUpOldSideBarList(base.Owner.SideBarList);
                    base.Owner._sideBarList = base.CreateControlHierarchy_SetUpSideBarList(layoutContainer);
                }
            }

            private void CreateControlHierarchy_WizardStep(Control layoutContainer)
            {
                Control placeholder = layoutContainer.FindControl(Wizard.WizardStepPlaceholderId);
                if (placeholder == null)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("Wizard_Step_Placeholder_Must_Be_Specified", new object[] { base.Owner.ID, Wizard.WizardStepPlaceholderId }));
                }
                ReplacePlaceholderWithControl(layoutContainer, placeholder, base.Owner.MultiView);
            }

            public override void OnlyShowCompleteStep()
            {
                this._layoutContainer.ControlToRender = base.Owner.MultiView;
            }

            private static void ReplacePlaceholderWithControl(Control targetContainer, Control placeholder, Control replacement)
            {
                int index = targetContainer.Controls.IndexOf(placeholder);
                targetContainer.Controls.RemoveAt(index);
                targetContainer.Controls.AddAt(index, replacement);
            }

            private static void ReplacePlaceholderWithTemplateInstance(Control targetContainer, Control placeholder, ITemplate template)
            {
                Control container = new Control();
                template.InstantiateIn(container);
                ReplacePlaceholderWithControl(targetContainer, placeholder, container);
            }

            internal class WizardContainer : WebControl
            {
                protected internal override void Render(HtmlTextWriter writer)
                {
                    if (this.ControlToRender == null)
                    {
                        this.RenderChildren(writer);
                    }
                    else
                    {
                        this.ControlToRender.Render(writer);
                    }
                }

                internal Control ControlToRender { get; set; }
            }
        }

        private sealed class NavigationTemplate : ITemplate
        {
            private bool _button1CausesValidation;
            private string _button1ID;
            private string _button2ID;
            private string _button3ID;
            private IButtonControl[][] _buttons;
            private const string _cancelButtonID = "Cancel";
            private const string _finishButtonID = "Finish";
            private const string _finishPreviousButtonID = "FinishPrevious";
            private TableRow _row;
            private const string _startNextButtonID = "StartNext";
            private const string _stepNextButtonID = "StepNext";
            private const string _stepPreviousButtonID = "StepPrevious";
            private Wizard.WizardTemplateType _templateType;
            private Wizard _wizard;

            private NavigationTemplate(Wizard wizard, Wizard.WizardTemplateType templateType, bool button1CausesValidation, string label1ID, string label2ID, string label3ID)
            {
                this._wizard = wizard;
                this._button1ID = label1ID;
                this._button2ID = label2ID;
                this._button3ID = label3ID;
                this._templateType = templateType;
                this._buttons = new IButtonControl[][] { new IButtonControl[3], new IButtonControl[3], new IButtonControl[3] };
                this._button1CausesValidation = button1CausesValidation;
            }

            private void CreateButtonControl(IButtonControl[] buttons, string id, bool causesValidation, string commandName)
            {
                LinkButton button = new LinkButton {
                    CausesValidation = causesValidation,
                    ID = id + "LinkButton",
                    Visible = false,
                    CommandName = commandName,
                    TabIndex = this._wizard.TabIndex
                };
                this._wizard.RegisterCommandEvents(button);
                buttons[0] = button;
                ImageButton button2 = new ImageButton {
                    CausesValidation = causesValidation,
                    ID = id + "ImageButton",
                    Visible = true,
                    CommandName = commandName,
                    TabIndex = this._wizard.TabIndex
                };
                this._wizard.RegisterCommandEvents(button2);
                button2.PreRender += new EventHandler(this.OnPreRender);
                buttons[1] = button2;
                Button button3 = new Button {
                    CausesValidation = causesValidation,
                    ID = id + "Button",
                    Visible = false,
                    CommandName = commandName,
                    TabIndex = this._wizard.TabIndex
                };
                this._wizard.RegisterCommandEvents(button3);
                buttons[2] = button3;
                TableCell cell = new TableCell {
                    HorizontalAlign = HorizontalAlign.Right
                };
                this._row.Cells.Add(cell);
                cell.Controls.Add(button);
                cell.Controls.Add(button2);
                cell.Controls.Add(button3);
            }

            private IButtonControl GetButtonBasedOnType(int pos, ButtonType type)
            {
                switch (type)
                {
                    case ButtonType.Button:
                        return this._buttons[pos][2];

                    case ButtonType.Image:
                        return this._buttons[pos][1];

                    case ButtonType.Link:
                        return this._buttons[pos][0];
                }
                return null;
            }

            internal static Wizard.NavigationTemplate GetDefaultFinishNavigationTemplate(Wizard wizard)
            {
                return new Wizard.NavigationTemplate(wizard, Wizard.WizardTemplateType.FinishNavigationTemplate, false, "FinishPrevious", "Finish", "Cancel");
            }

            internal static Wizard.NavigationTemplate GetDefaultStartNavigationTemplate(Wizard wizard)
            {
                return new Wizard.NavigationTemplate(wizard, Wizard.WizardTemplateType.StartNavigationTemplate, true, null, "StartNext", "Cancel");
            }

            internal static Wizard.NavigationTemplate GetDefaultStepNavigationTemplate(Wizard wizard)
            {
                return new Wizard.NavigationTemplate(wizard, Wizard.WizardTemplateType.StepNavigationTemplate, false, "StepPrevious", "StepNext", "Cancel");
            }

            private void OnPreRender(object source, EventArgs e)
            {
                ((ImageButton) source).Visible = false;
            }

            internal void ResetButtonsVisibility()
            {
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        Control control = this._buttons[i][j] as Control;
                        if (control != null)
                        {
                            control.Visible = false;
                        }
                    }
                }
            }

            void ITemplate.InstantiateIn(Control container)
            {
                Table child = new WizardDefaultInnerTable {
                    CellSpacing = 5,
                    CellPadding = 5
                };
                container.Controls.Add(child);
                this._row = new TableRow();
                child.Rows.Add(this._row);
                if (this._button1ID != null)
                {
                    this.CreateButtonControl(this._buttons[0], this._button1ID, this._button1CausesValidation, Wizard.MovePreviousCommandName);
                }
                if (this._button2ID != null)
                {
                    this.CreateButtonControl(this._buttons[1], this._button2ID, true, (this._templateType == Wizard.WizardTemplateType.FinishNavigationTemplate) ? Wizard.MoveCompleteCommandName : Wizard.MoveNextCommandName);
                }
                this.CreateButtonControl(this._buttons[2], this._button3ID, false, Wizard.CancelCommandName);
            }

            internal IButtonControl CancelButton
            {
                get
                {
                    ButtonType cancelButtonType = this._wizard.CancelButtonType;
                    return this.GetButtonBasedOnType(2, cancelButtonType);
                }
            }

            internal IButtonControl FirstButton
            {
                get
                {
                    ButtonType button = ButtonType.Button;
                    switch (this._templateType)
                    {
                        case Wizard.WizardTemplateType.StartNavigationTemplate:
                            break;

                        case Wizard.WizardTemplateType.StepNavigationTemplate:
                            button = this._wizard.StepPreviousButtonType;
                            break;

                        default:
                            button = this._wizard.FinishPreviousButtonType;
                            break;
                    }
                    return this.GetButtonBasedOnType(0, button);
                }
            }

            internal IButtonControl SecondButton
            {
                get
                {
                    ButtonType button = ButtonType.Button;
                    switch (this._templateType)
                    {
                        case Wizard.WizardTemplateType.StartNavigationTemplate:
                            button = this._wizard.StartNextButtonType;
                            break;

                        case Wizard.WizardTemplateType.StepNavigationTemplate:
                            button = this._wizard.StepNextButtonType;
                            break;

                        default:
                            button = this._wizard.FinishCompleteButtonType;
                            break;
                    }
                    return this.GetButtonBasedOnType(1, button);
                }
            }
        }

        private class StartNavigationTemplateContainer : Wizard.BaseNavigationTemplateContainer
        {
            private IButtonControl _nextButton;

            internal StartNavigationTemplateContainer(Wizard owner) : base(owner)
            {
            }

            internal override IButtonControl NextButton
            {
                get
                {
                    if (this._nextButton == null)
                    {
                        this._nextButton = this.FindControl(Wizard.StartNextButtonID) as IButtonControl;
                    }
                    return this._nextButton;
                }
                set
                {
                    this._nextButton = value;
                }
            }
        }

        private class StepNavigationTemplateContainer : Wizard.BaseNavigationTemplateContainer
        {
            internal StepNavigationTemplateContainer(Wizard owner) : base(owner)
            {
            }
        }

        internal class TableWizardRendering : Wizard.WizardRenderingBase
        {
            private const string _headerCellID = "HeaderContainer";
            private TableCell _headerTableCell;
            private TableRow _headerTableRow;
            private TableRow _navigationRow;
            private TableCell _navigationTableCell;
            private Table _renderTable;
            private const string _sideBarCellID = "SideBarContainer";
            private TableCell _stepTableCell;
            private const string _stepTableCellID = "StepTableCell";
            private LiteralControl _titleLiteral;

            public TableWizardRendering(Wizard wizard) : base(wizard)
            {
            }

            public override void ApplyControlProperties()
            {
                if (base.Owner.DesignMode || (((base.Owner.ActiveStepIndex >= 0) && (base.Owner.ActiveStepIndex < base.Owner.WizardSteps.Count)) && (base.Owner.WizardSteps.Count != 0)))
                {
                    if (base.Owner.SideBarEnabled && (base.Owner._sideBarStyle != null))
                    {
                        base.Owner._sideBarTableCell.ApplyStyle(base.Owner._sideBarStyle);
                    }
                    this.ApplyControlProperties_Header();
                    this.ApplyControlProperties_WizardSteps();
                    this.ApplyControlProperties_Navigation();
                    base.ApplyControlProperties_Sidebar();
                    if (this._renderTable != null)
                    {
                        Util.CopyBaseAttributesToInnerControl(base.Owner, this._renderTable);
                        if (base.Owner.ControlStyleCreated)
                        {
                            this._renderTable.ApplyStyle(base.Owner.ControlStyle);
                        }
                        else
                        {
                            this._renderTable.CellSpacing = 0;
                            this._renderTable.CellPadding = 0;
                        }
                        if ((!base.Owner.DesignMode && base.Owner.IsMacIE5) && (!base.Owner.ControlStyleCreated || (base.Owner.ControlStyle.Height == Unit.Empty)))
                        {
                            this._renderTable.ControlStyle.Height = Unit.Pixel(1);
                        }
                    }
                    if ((!base.Owner.DesignMode && (this._navigationTableCell != null)) && base.Owner.IsMacIE5)
                    {
                        this._navigationTableCell.ControlStyle.Height = Unit.Pixel(1);
                    }
                }
            }

            private void ApplyControlProperties_Header()
            {
                if (this._headerTableRow != null)
                {
                    if ((base.Owner.HeaderTemplate == null) && string.IsNullOrEmpty(base.Owner.HeaderText))
                    {
                        this._headerTableRow.Visible = false;
                    }
                    else
                    {
                        this._headerTableCell.ApplyStyle(base.Owner._headerStyle);
                        if (base.Owner.HeaderTemplate != null)
                        {
                            if (this._titleLiteral != null)
                            {
                                this._titleLiteral.Visible = false;
                            }
                        }
                        else if (this._titleLiteral != null)
                        {
                            this._titleLiteral.Text = base.Owner.HeaderText;
                        }
                    }
                }
            }

            private void ApplyControlProperties_Navigation()
            {
                base.ApplyNavigationTemplateProperties();
                base.ApplyCustomNavigationTemplateProperties();
                if (this._navigationTableCell != null)
                {
                    this.NavigationTableCell.HorizontalAlign = HorizontalAlign.Right;
                    if (base.Owner._navigationStyle != null)
                    {
                        if ((!base.Owner.DesignMode && base.Owner.IsMacIE5) && (base.Owner._navigationStyle.Height == Unit.Empty))
                        {
                            base.Owner._navigationStyle.Height = Unit.Pixel(1);
                        }
                        this._navigationTableCell.ApplyStyle(base.Owner._navigationStyle);
                    }
                }
                if (base.Owner.ShowCustomNavigationTemplate)
                {
                    this._navigationRow.Visible = true;
                }
            }

            private void ApplyControlProperties_WizardSteps()
            {
                if ((this._stepTableCell != null) && (base.Owner._stepStyle != null))
                {
                    if ((!base.Owner.DesignMode && base.Owner.IsMacIE5) && (base.Owner._stepStyle.Height == Unit.Empty))
                    {
                        base.Owner._stepStyle.Height = Unit.Pixel(1);
                    }
                    this._stepTableCell.ApplyStyle(base.Owner._stepStyle);
                }
            }

            public override void CreateControlHierarchy()
            {
                Table mainContentTable = null;
                if (base.Owner.DisplaySideBar)
                {
                    mainContentTable = this.CreateControlHierarchy_CreateLayoutWithSideBar();
                }
                else
                {
                    mainContentTable = this.CreateControlHierarchy_CreateLayoutWithoutSideBar();
                }
                this.CreateControlHierarchy_CreateHeaderArea(mainContentTable);
                this.CreateControlHierarchy_CreateStepArea(mainContentTable);
                this.CreateControlHierarchy_CreateNavigationArea(mainContentTable);
            }

            private void CreateControlHierarchy_CreateHeaderArea(Table mainContentTable)
            {
                this._headerTableRow = new TableRow();
                mainContentTable.Controls.Add(this._headerTableRow);
                Wizard.InternalTableCell cell = new Wizard.InternalTableCell(base.Owner) {
                    ID = "HeaderContainer"
                };
                this._headerTableCell = cell;
                if (base.Owner.HeaderTemplate != null)
                {
                    this._headerTableCell.EnableTheming = base.Owner.EnableTheming;
                    base.Owner.HeaderTemplate.InstantiateIn(this._headerTableCell);
                }
                else
                {
                    this._titleLiteral = new LiteralControl();
                    this._headerTableCell.Controls.Add(this._titleLiteral);
                }
                this._headerTableRow.Controls.Add(this._headerTableCell);
            }

            private Table CreateControlHierarchy_CreateLayoutWithoutSideBar()
            {
                Wizard.WizardChildTable child = new Wizard.WizardChildTable(base.Owner) {
                    EnableTheming = false
                };
                using (new Wizard.WizardControlCollectionModifier(base.Owner))
                {
                    base.Owner.Controls.Add(child);
                }
                this._renderTable = child;
                return child;
            }

            private Table CreateControlHierarchy_CreateLayoutWithSideBar()
            {
                Wizard.WizardChildTable table3 = new Wizard.WizardChildTable(base.Owner) {
                    EnableTheming = false
                };
                Table child = table3;
                TableRow row = new TableRow();
                child.Controls.Add(row);
                TableCell cell = base.Owner._sideBarTableCell ?? this.CreateControlHierarchy_CreateSideBarTableCell();
                row.Controls.Add(cell);
                base.Owner._sideBarTableCell = cell;
                base.Owner._renderSideBarDataList = false;
                TableCell cell2 = new TableCell {
                    Height = Unit.Percentage(100.0)
                };
                row.Controls.Add(cell2);
                WizardDefaultInnerTable table2 = new WizardDefaultInnerTable {
                    CellSpacing = 0,
                    Height = Unit.Percentage(100.0),
                    Width = Unit.Percentage(100.0)
                };
                cell2.Controls.Add(table2);
                if (!base.Owner.DesignMode && base.Owner.IsMacIE5)
                {
                    cell2.Height = Unit.Pixel(1);
                }
                using (new Wizard.WizardControlCollectionModifier(base.Owner))
                {
                    base.Owner.Controls.Add(child);
                }
                base.CreateControlHierarchy_CleanUpOldSideBarList(base.Owner.SideBarList);
                base.Owner._sideBarList = base.CreateControlHierarchy_SetUpSideBarList(base.Owner._sideBarTableCell);
                this._renderTable = child;
                return table2;
            }

            private void CreateControlHierarchy_CreateNavigationArea(Table mainContentTable)
            {
                this._navigationRow = new TableRow();
                mainContentTable.Controls.Add(this._navigationRow);
                this._navigationRow.Controls.Add(this.NavigationTableCell);
                base.CreateNavigationControlHierarchy(this.NavigationTableCell);
            }

            private TableCell CreateControlHierarchy_CreateSideBarTableCell()
            {
                Wizard.AccessibleTableCell cell2 = new Wizard.AccessibleTableCell(base.Owner) {
                    ID = "SideBarContainer",
                    Height = Unit.Percentage(100.0)
                };
                TableCell container = cell2;
                ITemplate sideBarTemplate = base.Owner.SideBarTemplate;
                if (sideBarTemplate == null)
                {
                    container.EnableViewState = false;
                    sideBarTemplate = base.Owner.CreateDefaultSideBarTemplate();
                }
                else
                {
                    container.EnableTheming = base.Owner.EnableTheming;
                }
                sideBarTemplate.InstantiateIn(container);
                return container;
            }

            private void CreateControlHierarchy_CreateStepArea(Table mainContentTable)
            {
                TableRow child = new TableRow {
                    Height = Unit.Percentage(100.0)
                };
                mainContentTable.Controls.Add(child);
                this._stepTableCell = new TableCell();
                child.Controls.Add(this._stepTableCell);
                this._stepTableCell.Controls.Add(base.Owner.MultiView);
                base.Owner.InstantiateStepContentTemplates();
            }

            public override void OnlyShowCompleteStep()
            {
                if (this._headerTableRow != null)
                {
                    this._headerTableRow.Visible = false;
                }
                if (base.Owner._sideBarTableCell != null)
                {
                    base.Owner._sideBarTableCell.Visible = false;
                }
                this._navigationRow.Visible = false;
            }

            protected override WizardStepType SetActiveTemplates()
            {
                WizardStepType type = base.SetActiveTemplates();
                if ((type != WizardStepType.Complete) && (base.Owner._sideBarTableCell != null))
                {
                    base.Owner._sideBarTableCell.Visible = base.Owner.SideBarEnabled && base.Owner._renderSideBarDataList;
                }
                return type;
            }

            public override void SetDesignModeState(IDictionary dictionary)
            {
                base.SetDesignModeState(dictionary);
                dictionary["StepTableCell"] = this._stepTableCell;
            }

            private TableCell NavigationTableCell
            {
                get
                {
                    if (this._navigationTableCell == null)
                    {
                        this._navigationTableCell = new TableCell();
                    }
                    return this._navigationTableCell;
                }
            }
        }

        [SupportsEventValidation]
        private class WizardChildTable : ChildTable
        {
            private Wizard _owner;

            internal WizardChildTable(Wizard owner)
            {
                this._owner = owner;
            }

            protected override bool OnBubbleEvent(object source, EventArgs args)
            {
                return this._owner.OnBubbleEvent(source, args);
            }
        }

        private class WizardControlCollection : ControlCollection
        {
            public WizardControlCollection(Wizard wizard) : base(wizard)
            {
                if (!wizard.DesignMode)
                {
                    base.SetCollectionReadOnly("Wizard_Cannot_Modify_ControlCollection");
                }
            }
        }

        private class WizardControlCollectionModifier : IDisposable
        {
            private ControlCollection _controls;
            private string _originalError;
            private Wizard _wizard;

            public WizardControlCollectionModifier(Wizard wizard)
            {
                this._wizard = wizard;
                if (!this._wizard.DesignMode)
                {
                    this._controls = this._wizard.Controls;
                    this._originalError = this._controls.SetCollectionReadOnly(null);
                }
            }

            void IDisposable.Dispose()
            {
                if (!this._wizard.DesignMode)
                {
                    this._controls.SetCollectionReadOnly(this._originalError);
                }
            }
        }

        internal abstract class WizardRenderingBase
        {
            private Wizard.NavigationTemplate _defaultFinishNavigationTemplate;
            private Wizard.NavigationTemplate _defaultStartNavigationTemplate;
            private Wizard.NavigationTemplate _defaultStepNavigationTemplate;
            protected Wizard.BaseNavigationTemplateContainer _finishNavigationTemplateContainer;
            private const string _finishNavigationTemplateContainerID = "FinishNavigationTemplateContainerID";
            protected Wizard.BaseNavigationTemplateContainer _startNavigationTemplateContainer;
            private const string _startNavigationTemplateContainerID = "StartNavigationTemplateContainerID";
            protected Wizard.BaseNavigationTemplateContainer _stepNavigationTemplateContainer;
            private const string _stepNavigationTemplateContainerID = "StepNavigationTemplateContainerID";

            protected WizardRenderingBase(Wizard wizard)
            {
                this.Owner = wizard;
            }

            private static void ApplyButtonProperties(IButtonControl button, string text, string imageUrl)
            {
                ApplyButtonProperties(button, text, imageUrl, true);
            }

            private static void ApplyButtonProperties(IButtonControl button, string text, string imageUrl, bool imageButtonVisible)
            {
                if (button != null)
                {
                    ImageButton button2 = button as ImageButton;
                    if (button2 != null)
                    {
                        button2.ImageUrl = imageUrl;
                        button2.AlternateText = text;
                        button2.Visible = imageButtonVisible;
                    }
                    else
                    {
                        button.Text = text;
                    }
                }
            }

            public abstract void ApplyControlProperties();
            protected void ApplyControlProperties_Sidebar()
            {
                if (this.Owner.SideBarEnabled)
                {
                    this.Owner.SetStepsAndDataBindSideBarList(this.Owner._sideBarList);
                    if (this.Owner.SideBarTemplate == null)
                    {
                        foreach (Control control in this.Owner._sideBarList.Items)
                        {
                            WebControl control2 = control.FindControl(Wizard.SideBarButtonID) as WebControl;
                            if (control2 != null)
                            {
                                control2.MergeStyle(this.Owner._sideBarButtonStyle);
                            }
                        }
                    }
                }
            }

            protected void ApplyCustomNavigationTemplateProperties()
            {
                foreach (Wizard.BaseNavigationTemplateContainer container in this.Owner.CustomNavigationContainers.Values)
                {
                    container.Visible = false;
                }
                if (this.Owner.ShowCustomNavigationTemplate)
                {
                    Wizard.BaseNavigationTemplateContainer container2 = this.Owner._customNavigationContainers[this.Owner.ActiveStep];
                    container2.Visible = true;
                    this._startNavigationTemplateContainer.Visible = false;
                    this._stepNavigationTemplateContainer.Visible = false;
                    this._finishNavigationTemplateContainer.Visible = false;
                }
            }

            private void ApplyDefaultFinishNavigationTemplateProperties(bool previousImageButtonVisible)
            {
                if (this.Owner.FinishNavigationTemplate == null)
                {
                    Wizard.BaseNavigationTemplateContainer container = this._finishNavigationTemplateContainer;
                    Wizard.NavigationTemplate template = this._defaultFinishNavigationTemplate;
                    if (this.Owner.DesignMode)
                    {
                        template.ResetButtonsVisibility();
                    }
                    container.PreviousButton = template.FirstButton;
                    ((Control) container.PreviousButton).Visible = true;
                    container.FinishButton = template.SecondButton;
                    ((Control) container.FinishButton).Visible = true;
                    container.CancelButton = template.CancelButton;
                    container.FinishButton.CommandName = Wizard.MoveCompleteCommandName;
                    ApplyButtonProperties(container.FinishButton, this.Owner.FinishCompleteButtonText, this.Owner.FinishCompleteButtonImageUrl);
                    ApplyButtonProperties(container.PreviousButton, this.Owner.FinishPreviousButtonText, this.Owner.FinishPreviousButtonImageUrl, previousImageButtonVisible);
                    ApplyButtonProperties(container.CancelButton, this.Owner.CancelButtonText, this.Owner.CancelButtonImageUrl);
                    int previousStepIndex = this.Owner.GetPreviousStepIndex(false);
                    if ((previousStepIndex != -1) && !this.Owner.WizardSteps[previousStepIndex].AllowReturn)
                    {
                        ((Control) container.PreviousButton).Visible = false;
                    }
                    this.Owner.SetCancelButtonVisibility(container);
                    container.ApplyButtonStyle(this.Owner.FinishCompleteButtonStyle, this.Owner.FinishPreviousButtonStyle, this.Owner.StepNextButtonStyle, this.Owner.CancelButtonStyle);
                }
            }

            private void ApplyDefaultStartNavigationTemplateProperties()
            {
                if (this.Owner.StartNavigationTemplate == null)
                {
                    Wizard.BaseNavigationTemplateContainer container = this._startNavigationTemplateContainer;
                    Wizard.NavigationTemplate template = this._defaultStartNavigationTemplate;
                    if (this.Owner.DesignMode)
                    {
                        template.ResetButtonsVisibility();
                    }
                    container.NextButton = template.SecondButton;
                    ((Control) container.NextButton).Visible = true;
                    container.CancelButton = template.CancelButton;
                    ApplyButtonProperties(container.NextButton, this.Owner.StartNextButtonText, this.Owner.StartNextButtonImageUrl);
                    ApplyButtonProperties(container.CancelButton, this.Owner.CancelButtonText, this.Owner.CancelButtonImageUrl);
                    this.Owner.SetCancelButtonVisibility(container);
                    container.ApplyButtonStyle(this.Owner.FinishCompleteButtonStyle, this.Owner.StepPreviousButtonStyle, this.Owner.StartNextButtonStyle, this.Owner.CancelButtonStyle);
                }
            }

            private void ApplyDefaultStepNavigationTemplateProperties(bool previousImageButtonVisible)
            {
                if (this.Owner.StepNavigationTemplate == null)
                {
                    Wizard.BaseNavigationTemplateContainer container = this._stepNavigationTemplateContainer;
                    Wizard.NavigationTemplate template = this._defaultStepNavigationTemplate;
                    if (this.Owner.DesignMode)
                    {
                        template.ResetButtonsVisibility();
                    }
                    container.PreviousButton = template.FirstButton;
                    ((Control) container.PreviousButton).Visible = true;
                    container.NextButton = template.SecondButton;
                    ((Control) container.NextButton).Visible = true;
                    container.CancelButton = template.CancelButton;
                    ApplyButtonProperties(container.NextButton, this.Owner.StepNextButtonText, this.Owner.StepNextButtonImageUrl);
                    ApplyButtonProperties(container.PreviousButton, this.Owner.StepPreviousButtonText, this.Owner.StepPreviousButtonImageUrl, previousImageButtonVisible);
                    ApplyButtonProperties(container.CancelButton, this.Owner.CancelButtonText, this.Owner.CancelButtonImageUrl);
                    int previousStepIndex = this.Owner.GetPreviousStepIndex(false);
                    if ((previousStepIndex != -1) && !this.Owner.WizardSteps[previousStepIndex].AllowReturn)
                    {
                        ((Control) container.PreviousButton).Visible = false;
                    }
                    this.Owner.SetCancelButtonVisibility(container);
                    container.ApplyButtonStyle(this.Owner.FinishCompleteButtonStyle, this.Owner.StepPreviousButtonStyle, this.Owner.StepNextButtonStyle, this.Owner.CancelButtonStyle);
                }
            }

            protected void ApplyNavigationTemplateProperties()
            {
                if (((this._finishNavigationTemplateContainer != null) && (this._startNavigationTemplateContainer != null)) && (this._stepNavigationTemplateContainer != null))
                {
                    if ((this.Owner.ActiveStepIndex < this.Owner.WizardSteps.Count) && (this.Owner.ActiveStepIndex >= 0))
                    {
                        bool flag = ((this.SetActiveTemplates() != WizardStepType.Finish) || (this.Owner.ActiveStepIndex != 0)) || (this.Owner.ActiveStep.StepType != WizardStepType.Auto);
                        this.ApplyDefaultStartNavigationTemplateProperties();
                        bool previousImageButtonVisible = true;
                        int previousStepIndex = this.Owner.GetPreviousStepIndex(false);
                        if (previousStepIndex >= 0)
                        {
                            previousImageButtonVisible = this.Owner.WizardSteps[previousStepIndex].AllowReturn;
                        }
                        this.ApplyDefaultFinishNavigationTemplateProperties(previousImageButtonVisible);
                        this.ApplyDefaultStepNavigationTemplateProperties(previousImageButtonVisible);
                        if (!flag)
                        {
                            Control previousButton = this._finishNavigationTemplateContainer.PreviousButton as Control;
                            if (previousButton != null)
                            {
                                if (this.Owner.FinishNavigationTemplate == null)
                                {
                                    previousButton.Parent.Visible = false;
                                }
                                else
                                {
                                    previousButton.Visible = false;
                                }
                            }
                        }
                    }
                }
            }

            public abstract void CreateControlHierarchy();
            protected void CreateControlHierarchy_CleanUpOldSideBarList(IWizardSideBarListControl sideBarList)
            {
                if (sideBarList != null)
                {
                    sideBarList.ItemCommand -= new CommandEventHandler(this.Owner.DataListItemCommand);
                    Wizard owner = this.Owner;
                    sideBarList.ItemDataBound -= new EventHandler<WizardSideBarListControlItemEventArgs>(owner.DataListItemDataBound);
                }
            }

            protected IWizardSideBarListControl CreateControlHierarchy_SetUpSideBarList(Control sideBarContainer)
            {
                IWizardSideBarListControl sideBarList = sideBarContainer.FindControl(Wizard.DataListID) as IWizardSideBarListControl;
                if (sideBarList != null)
                {
                    sideBarList.ItemCommand += new CommandEventHandler(this.Owner.DataListItemCommand);
                    Wizard owner = this.Owner;
                    sideBarList.ItemDataBound += new EventHandler<WizardSideBarListControlItemEventArgs>(owner.DataListItemDataBound);
                    if (this.Owner.DesignMode)
                    {
                        ((IControlDesignerAccessor) sideBarList).GetDesignModeState()["EnableDesignTimeDataBinding"] = true;
                    }
                    this.Owner.SetStepsAndDataBindSideBarList(sideBarList);
                    return sideBarList;
                }
                if (!this.Owner.DesignMode)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("Wizard_DataList_Not_Found", new object[] { Wizard.DataListID }));
                }
                return sideBarList;
            }

            private void CreateFinishNavigationTemplate(Control container)
            {
                ITemplate finishNavigationTemplate = this.Owner.FinishNavigationTemplate;
                this._finishNavigationTemplateContainer = new Wizard.FinishNavigationTemplateContainer(this.Owner);
                this._finishNavigationTemplateContainer.ID = "FinishNavigationTemplateContainerID";
                if (finishNavigationTemplate == null)
                {
                    this._finishNavigationTemplateContainer.EnableViewState = false;
                    this._defaultFinishNavigationTemplate = Wizard.NavigationTemplate.GetDefaultFinishNavigationTemplate(this.Owner);
                    finishNavigationTemplate = this._defaultFinishNavigationTemplate;
                }
                else
                {
                    this._finishNavigationTemplateContainer.SetEnableTheming();
                }
                finishNavigationTemplate.InstantiateIn(this._finishNavigationTemplateContainer);
                container.Controls.Add(this._finishNavigationTemplateContainer);
            }

            protected void CreateNavigationControlHierarchy(Control container)
            {
                container.Controls.Clear();
                this.Owner.CustomNavigationContainers.Clear();
                this.Owner.CreateCustomNavigationTemplates();
                foreach (Wizard.BaseNavigationTemplateContainer container2 in this.Owner.CustomNavigationContainers.Values)
                {
                    container.Controls.Add(container2);
                }
                this.CreateStartNavigationTemplate(container);
                this.CreateFinishNavigationTemplate(container);
                this.CreateStepNavigationTemplate(container);
            }

            private void CreateStartNavigationTemplate(Control container)
            {
                ITemplate startNavigationTemplate = this.Owner.StartNavigationTemplate;
                this._startNavigationTemplateContainer = new Wizard.StartNavigationTemplateContainer(this.Owner);
                this._startNavigationTemplateContainer.ID = "StartNavigationTemplateContainerID";
                if (startNavigationTemplate == null)
                {
                    this._startNavigationTemplateContainer.EnableViewState = false;
                    this._defaultStartNavigationTemplate = Wizard.NavigationTemplate.GetDefaultStartNavigationTemplate(this.Owner);
                    startNavigationTemplate = this._defaultStartNavigationTemplate;
                }
                else
                {
                    this._startNavigationTemplateContainer.SetEnableTheming();
                }
                startNavigationTemplate.InstantiateIn(this._startNavigationTemplateContainer);
                container.Controls.Add(this._startNavigationTemplateContainer);
            }

            private void CreateStepNavigationTemplate(Control container)
            {
                ITemplate stepNavigationTemplate = this.Owner.StepNavigationTemplate;
                this._stepNavigationTemplateContainer = new Wizard.StepNavigationTemplateContainer(this.Owner);
                this._stepNavigationTemplateContainer.ID = "StepNavigationTemplateContainerID";
                if (stepNavigationTemplate == null)
                {
                    this._stepNavigationTemplateContainer.EnableViewState = false;
                    this._defaultStepNavigationTemplate = Wizard.NavigationTemplate.GetDefaultStepNavigationTemplate(this.Owner);
                    stepNavigationTemplate = this._defaultStepNavigationTemplate;
                }
                else
                {
                    this._stepNavigationTemplateContainer.SetEnableTheming();
                }
                stepNavigationTemplate.InstantiateIn(this._stepNavigationTemplateContainer);
                container.Controls.Add(this._stepNavigationTemplateContainer);
            }

            public abstract void OnlyShowCompleteStep();
            protected virtual WizardStepType SetActiveTemplates()
            {
                WizardStepType stepType = this.Owner.GetStepType(this.Owner.ActiveStepIndex);
                this._startNavigationTemplateContainer.Visible = stepType == WizardStepType.Start;
                this._stepNavigationTemplateContainer.Visible = stepType == WizardStepType.Step;
                this._finishNavigationTemplateContainer.Visible = stepType == WizardStepType.Finish;
                if (stepType == WizardStepType.Complete)
                {
                    this.OnlyShowCompleteStep();
                }
                return stepType;
            }

            public virtual void SetDesignModeState(IDictionary dictionary)
            {
                if (this._startNavigationTemplateContainer != null)
                {
                    dictionary[Wizard.StartNextButtonID] = this._startNavigationTemplateContainer.NextButton;
                    dictionary[Wizard.CancelButtonID] = this._startNavigationTemplateContainer.CancelButton;
                }
                if (this._stepNavigationTemplateContainer != null)
                {
                    dictionary[Wizard.StepNextButtonID] = this._stepNavigationTemplateContainer.NextButton;
                    dictionary[Wizard.StepPreviousButtonID] = this._stepNavigationTemplateContainer.PreviousButton;
                    dictionary[Wizard.CancelButtonID] = this._stepNavigationTemplateContainer.CancelButton;
                }
                if (this._finishNavigationTemplateContainer != null)
                {
                    dictionary[Wizard.FinishPreviousButtonID] = this._finishNavigationTemplateContainer.PreviousButton;
                    dictionary[Wizard.FinishButtonID] = this._finishNavigationTemplateContainer.FinishButton;
                    dictionary[Wizard.CancelButtonID] = this._finishNavigationTemplateContainer.CancelButton;
                }
            }

            protected Wizard Owner { get; private set; }
        }

        private enum WizardTemplateType
        {
            StartNavigationTemplate,
            StepNavigationTemplate,
            FinishNavigationTemplate
        }
    }
}

