namespace System.Web.UI.Design
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public abstract class TemplatedControlDesigner : ControlDesigner
    {
        private TemplatedControlDesignerTemplateGroup _currentTemplateGroup;
        private IDictionary _templateGroupTable;
        private bool enableTemplateEditing = true;

        [Obsolete("Use of this method is not recommended because template editing is handled in ControlDesigner. To support template editing expose template data in the TemplateGroups property and call SetViewFlags(ViewFlags.TemplateEditing, true). http://go.microsoft.com/fwlink/?linkid=14202")]
        protected abstract ITemplateEditingFrame CreateTemplateEditingFrame(TemplateEditingVerb verb);
        private void EnableTemplateEditing(bool enable)
        {
            this.enableTemplateEditing = enable;
        }

        [Obsolete("Use of this method is not recommended because template editing is handled in ControlDesigner. To support template editing expose template data in the TemplateGroups property and call SetViewFlags(ViewFlags.TemplateEditing, true). http://go.microsoft.com/fwlink/?linkid=14202")]
        public void EnterTemplateMode(ITemplateEditingFrame newTemplateEditingFrame)
        {
            if ((this.ActiveTemplateEditingFrame != newTemplateEditingFrame) && (this.BehaviorInternal != null))
            {
                IControlDesignerBehavior behaviorInternal = (IControlDesignerBehavior) this.BehaviorInternal;
                try
                {
                    bool fSwitchingTemplates = false;
                    if (this.InTemplateModeInternal)
                    {
                        fSwitchingTemplates = true;
                        this.ExitTemplateModeInternal(fSwitchingTemplates, false, true);
                    }
                    else if (behaviorInternal != null)
                    {
                        behaviorInternal.DesignTimeHtml = string.Empty;
                    }
                    this._currentTemplateGroup = (TemplatedControlDesignerTemplateGroup) this.TemplateGroupTable[newTemplateEditingFrame];
                    if (this._currentTemplateGroup == null)
                    {
                        this._currentTemplateGroup = new TemplatedControlDesignerTemplateGroup(null, newTemplateEditingFrame);
                    }
                    if (!fSwitchingTemplates)
                    {
                        this.RaiseTemplateModeChanged();
                    }
                    this.ActiveTemplateEditingFrame.Open();
                    base.IsDirtyInternal = true;
                    TypeDescriptor.Refresh(base.Component);
                }
                catch
                {
                }
                IWebFormsDocumentService service = (IWebFormsDocumentService) this.GetService(typeof(IWebFormsDocumentService));
                if (service != null)
                {
                    service.UpdateSelection();
                }
            }
        }

        private void EnterTemplateModeInternal(ITemplateEditingFrame newTemplateEditingFrame)
        {
            this.EnterTemplateMode(newTemplateEditingFrame);
        }

        private void ExitNestedTemplates(bool fSave)
        {
            try
            {
                IComponent viewControl = base.ViewControl;
                IDesignerHost service = (IDesignerHost) viewControl.Site.GetService(typeof(IDesignerHost));
                ControlCollection controls = ((Control) viewControl).Controls;
                for (int i = 0; i < controls.Count; i++)
                {
                    IDesigner designer = service.GetDesigner(controls[i]);
                    if (designer is TemplatedControlDesigner)
                    {
                        TemplatedControlDesigner designer2 = (TemplatedControlDesigner) designer;
                        if (designer2.InTemplateModeInternal)
                        {
                            designer2.ExitTemplateModeInternal(false, true, fSave);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        [Obsolete("Use of this method is not recommended because template editing is handled in ControlDesigner. To support template editing expose template data in the TemplateGroups property and call SetViewFlags(ViewFlags.TemplateEditing, true). http://go.microsoft.com/fwlink/?linkid=14202")]
        public void ExitTemplateMode(bool fSwitchingTemplates, bool fNested, bool fSave)
        {
            try
            {
                this.ExitNestedTemplates(fSave);
                this.ActiveTemplateEditingFrame.Close(fSave);
                if (!fSwitchingTemplates)
                {
                    this._currentTemplateGroup = null;
                    this.RaiseTemplateModeChanged();
                    if (!fNested)
                    {
                        this.UpdateDesignTimeHtml();
                        TypeDescriptor.Refresh(base.Component);
                        IWebFormsDocumentService service = (IWebFormsDocumentService) this.GetService(typeof(IWebFormsDocumentService));
                        if (service != null)
                        {
                            service.UpdateSelection();
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private void ExitTemplateModeInternal(bool fSwitchingTemplates, bool fNested, bool fSave)
        {
            this.ExitTemplateMode(fSwitchingTemplates, fNested, fSave);
        }

        [Obsolete("Use of this method is not recommended because template editing is handled in ControlDesigner. To support template editing expose template data in the TemplateGroups property and call SetViewFlags(ViewFlags.TemplateEditing, true). http://go.microsoft.com/fwlink/?linkid=14202")]
        protected abstract TemplateEditingVerb[] GetCachedTemplateEditingVerbs();
        internal override string GetPersistInnerHtmlInternal()
        {
            if (this.InTemplateModeInternal)
            {
                this.SaveActiveTemplateEditingFrame();
            }
            string persistInnerHtmlInternal = base.GetPersistInnerHtmlInternal();
            if (this.InTemplateModeInternal)
            {
                base.IsDirtyInternal = true;
            }
            return persistInnerHtmlInternal;
        }

        [Obsolete("Use of this method is not recommended because template editing is handled in ControlDesigner. To support template editing expose template data in the TemplateGroups property and call SetViewFlags(ViewFlags.TemplateEditing, true). http://go.microsoft.com/fwlink/?linkid=14202")]
        public virtual string GetTemplateContainerDataItemProperty(string templateName)
        {
            return string.Empty;
        }

        [Obsolete("Use of this method is not recommended because template editing is handled in ControlDesigner. To support template editing expose template data in the TemplateGroups property and call SetViewFlags(ViewFlags.TemplateEditing, true). http://go.microsoft.com/fwlink/?linkid=14202")]
        public virtual IEnumerable GetTemplateContainerDataSource(string templateName)
        {
            return null;
        }

        [Obsolete("Use of this method is not recommended because template editing is handled in ControlDesigner. To support template editing expose template data in the TemplateGroups property and call SetViewFlags(ViewFlags.TemplateEditing, true). http://go.microsoft.com/fwlink/?linkid=14202")]
        public abstract string GetTemplateContent(ITemplateEditingFrame editingFrame, string templateName, out bool allowEditing);
        [Obsolete("Use of this method is not recommended because template editing is handled in ControlDesigner. To support template editing expose template data in the TemplateGroups property and call SetViewFlags(ViewFlags.TemplateEditing, true). http://go.microsoft.com/fwlink/?linkid=14202")]
        public TemplateEditingVerb[] GetTemplateEditingVerbs()
        {
            if (((ITemplateEditingService) this.GetService(typeof(ITemplateEditingService))) == null)
            {
                return null;
            }
            TemplateEditingVerbCollection templateEditingVerbsInternal = this.GetTemplateEditingVerbsInternal();
            TemplateEditingVerb[] array = new TemplateEditingVerb[templateEditingVerbsInternal.Count];
            ((ICollection) templateEditingVerbsInternal).CopyTo(array, 0);
            return array;
        }

        private TemplateEditingVerbCollection GetTemplateEditingVerbsInternal()
        {
            TemplateEditingVerbCollection verbs = new TemplateEditingVerbCollection();
            TemplateEditingVerb[] cachedTemplateEditingVerbs = this.GetCachedTemplateEditingVerbs();
            if ((cachedTemplateEditingVerbs != null) && (cachedTemplateEditingVerbs.Length > 0))
            {
                for (int i = 0; i < cachedTemplateEditingVerbs.Length; i++)
                {
                    if ((this._currentTemplateGroup != null) && (this._currentTemplateGroup.Verb == cachedTemplateEditingVerbs[i]))
                    {
                        cachedTemplateEditingVerbs[i].Checked = true;
                    }
                    else
                    {
                        cachedTemplateEditingVerbs[i].Checked = false;
                    }
                    verbs.Add(cachedTemplateEditingVerbs[i]);
                }
            }
            return verbs;
        }

        protected ITemplate GetTemplateFromText(string text)
        {
            return this.GetTemplateFromText(text, null);
        }

        internal ITemplate GetTemplateFromText(string text, ITemplate currentTemplate)
        {
            if ((text == null) || (text.Length == 0))
            {
                throw new ArgumentNullException("text");
            }
            IDesignerHost service = (IDesignerHost) base.Component.Site.GetService(typeof(IDesignerHost));
            try
            {
                ITemplate template = ControlParser.ParseTemplate(service, text);
                if (template != null)
                {
                    return template;
                }
            }
            catch
            {
            }
            return currentTemplate;
        }

        [Obsolete("Use of this method is not recommended because template editing is handled in ControlDesigner. To support template editing expose template data in the TemplateGroups property and call SetViewFlags(ViewFlags.TemplateEditing, true). http://go.microsoft.com/fwlink/?linkid=14202")]
        public virtual Type GetTemplatePropertyParentType(string templateName)
        {
            return base.Component.GetType();
        }

        protected string GetTextFromTemplate(ITemplate template)
        {
            if (template == null)
            {
                throw new ArgumentNullException("template");
            }
            if (template is TemplateBuilder)
            {
                return ((TemplateBuilder) template).Text;
            }
            return string.Empty;
        }

        public override void Initialize(IComponent component)
        {
            base.Initialize(component);
            if (base.View != null)
            {
                base.View.ViewEvent += new ViewEventHandler(this.OnViewEvent);
                base.View.SetFlags(ViewFlags.TemplateEditing, true);
            }
        }

        [Obsolete("The recommended alternative is ControlDesigner.Tag. http://go.microsoft.com/fwlink/?linkid=14202")]
        protected override void OnBehaviorAttached()
        {
            if (this.InTemplateModeInternal)
            {
                this.ActiveTemplateEditingFrame.Close(false);
                this.ActiveTemplateEditingFrame.Dispose();
                this._currentTemplateGroup = null;
                TypeDescriptor.Refresh(base.Component);
            }
            base.OnBehaviorAttached();
        }

        public override void OnComponentChanged(object sender, ComponentChangedEventArgs ce)
        {
            base.OnComponentChanged(sender, ce);
            if ((this.InTemplateModeInternal && (ce.Member != null)) && ((ce.NewValue != null) && ce.Member.Name.Equals("ID")))
            {
                this.ActiveTemplateEditingFrame.UpdateControlName(ce.NewValue.ToString());
            }
        }

        public override void OnSetParent()
        {
            Control component = (Control) base.Component;
            bool enable = false;
            IDesignerHost host = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            ITemplateEditingService service = (ITemplateEditingService) host.GetService(typeof(ITemplateEditingService));
            if (service != null)
            {
                enable = true;
                Control parent = component.Parent;
                Control page = component.Page;
                while ((parent != null) && (parent != page))
                {
                    if (host.GetDesigner(parent) is TemplatedControlDesigner)
                    {
                        enable = service.SupportsNestedTemplateEditing;
                        break;
                    }
                    parent = parent.Parent;
                }
            }
            this.EnableTemplateEditing(enable);
        }

        private void OnTemplateEditingVerbInvoked(object sender, EventArgs e)
        {
            TemplateEditingVerb verb = (TemplateEditingVerb) sender;
            if (verb.EditingFrame == null)
            {
                verb.EditingFrame = this.CreateTemplateEditingFrame(verb);
            }
            if (verb.EditingFrame != null)
            {
                verb.EditingFrame.Verb = verb;
                this.EnterTemplateModeInternal(verb.EditingFrame);
            }
        }

        protected virtual void OnTemplateModeChanged()
        {
        }

        internal void OnTemplateModeChangedInternal(TemplateModeChangedEventArgs e)
        {
            TemplateGroup newTemplateGroup = e.NewTemplateGroup;
            if (newTemplateGroup != null)
            {
                if (this._currentTemplateGroup != newTemplateGroup)
                {
                    this.EnterTemplateModeInternal(((TemplatedControlDesignerTemplateGroup) newTemplateGroup).Frame);
                }
            }
            else
            {
                this.ExitTemplateModeInternal(false, false, true);
            }
        }

        private void OnViewEvent(object sender, ViewEventArgs e)
        {
            if (e.EventType == ViewEvent.TemplateModeChanged)
            {
                this.OnTemplateModeChangedInternal((TemplateModeChangedEventArgs) e.EventArgs);
            }
        }

        private void RaiseTemplateModeChanged()
        {
            if (this.BehaviorInternal != null)
            {
                ((IControlDesignerBehavior) this.BehaviorInternal).OnTemplateModeChanged();
            }
            this.OnTemplateModeChanged();
        }

        protected void SaveActiveTemplateEditingFrame()
        {
            this.ActiveTemplateEditingFrame.Save();
        }

        [Obsolete("Use of this method is not recommended because template editing is handled in ControlDesigner. To support template editing expose template data in the TemplateGroups property and call SetViewFlags(ViewFlags.TemplateEditing, true). http://go.microsoft.com/fwlink/?linkid=14202")]
        public abstract void SetTemplateContent(ITemplateEditingFrame editingFrame, string templateName, string templateContent);
        public override void UpdateDesignTimeHtml()
        {
            if (!this.InTemplateModeInternal)
            {
                base.UpdateDesignTimeHtml();
            }
        }

        [Obsolete("Use of this property is not recommended because template editing is handled in ControlDesigner. To support template editing expose template data in the TemplateGroups property and call SetViewFlags(ViewFlags.TemplateEditing, true). http://go.microsoft.com/fwlink/?linkid=14202")]
        public ITemplateEditingFrame ActiveTemplateEditingFrame
        {
            get
            {
                if (this._currentTemplateGroup != null)
                {
                    return this._currentTemplateGroup.Frame;
                }
                return null;
            }
        }

        public bool CanEnterTemplateMode
        {
            get
            {
                return this.enableTemplateEditing;
            }
        }

        protected override bool DataBindingsEnabled
        {
            get
            {
                if (this.InTemplateModeInternal && this.HidePropertiesInTemplateMode)
                {
                    return false;
                }
                return base.DataBindingsEnabled;
            }
        }

        [Obsolete("The recommended alternative is System.Web.UI.Design.ControlDesigner.InTemplateMode. http://go.microsoft.com/fwlink/?linkid=14202")]
        public bool InTemplateMode
        {
            get
            {
                return (this._currentTemplateGroup != null);
            }
        }

        internal bool InTemplateModeInternal
        {
            get
            {
                return this.InTemplateMode;
            }
        }

        internal EventHandler TemplateEditingVerbHandler
        {
            get
            {
                return new EventHandler(this.OnTemplateEditingVerbInvoked);
            }
        }

        public override TemplateGroupCollection TemplateGroups
        {
            get
            {
                TemplateGroupCollection templateGroups = base.TemplateGroups;
                this.TemplateGroupTable.Clear();
                foreach (TemplateEditingVerb verb in (IEnumerable) this.GetTemplateEditingVerbsInternal())
                {
                    if (verb.Enabled && verb.Visible)
                    {
                        ITemplateEditingFrame frame = this.CreateTemplateEditingFrame(verb);
                        frame.Verb = verb;
                        TemplateGroup group = new TemplatedControlDesignerTemplateGroup(verb, frame);
                        bool flag = frame.TemplateStyles != null;
                        for (int i = 0; i < frame.TemplateNames.Length; i++)
                        {
                            Style style = flag ? frame.TemplateStyles[i] : null;
                            TemplatedControlDesignerTemplateDefinition templateDefinition = new TemplatedControlDesignerTemplateDefinition(frame.TemplateNames[i], style, this, frame) {
                                SupportsDataBinding = true
                            };
                            group.AddTemplateDefinition(templateDefinition);
                        }
                        templateGroups.Add(group);
                        this.TemplateGroupTable[frame] = group;
                    }
                }
                return templateGroups;
            }
        }

        private IDictionary TemplateGroupTable
        {
            get
            {
                if (this._templateGroupTable == null)
                {
                    this._templateGroupTable = new HybridDictionary();
                }
                return this._templateGroupTable;
            }
        }

        private class TemplatedControlDesignerTemplateDefinition : TemplateDefinition
        {
            private ITemplateEditingFrame _frame;
            private TemplatedControlDesigner _parent;

            public TemplatedControlDesignerTemplateDefinition(string name, Style style, TemplatedControlDesigner parent, ITemplateEditingFrame frame) : base(parent, name, parent.Component, name, style)
            {
                this._parent = parent;
                this._frame = frame;
                base.Properties[typeof(Control)] = (Control) this._parent.Component;
            }

            public override bool AllowEditing
            {
                get
                {
                    bool flag;
                    this._parent.GetTemplateContent(this._frame, base.Name, out flag);
                    return flag;
                }
            }

            public override string Content
            {
                get
                {
                    bool flag;
                    return this._parent.GetTemplateContent(this._frame, base.Name, out flag);
                }
                set
                {
                    this._parent.SetTemplateContent(this._frame, base.Name, value);
                    this._parent.Tag.SetDirty(true);
                    this._parent.UpdateDesignTimeHtml();
                }
            }
        }

        private class TemplatedControlDesignerTemplateGroup : TemplateGroup
        {
            private ITemplateEditingFrame _frame;
            private TemplateEditingVerb _verb;

            public TemplatedControlDesignerTemplateGroup(TemplateEditingVerb verb, ITemplateEditingFrame frame) : base(verb.Text, frame.ControlStyle)
            {
                this._frame = frame;
                this._verb = verb;
            }

            public ITemplateEditingFrame Frame
            {
                get
                {
                    return this._frame;
                }
            }

            public TemplateEditingVerb Verb
            {
                get
                {
                    return this._verb;
                }
            }
        }

        private class TemplateEditingVerbCollection : IList, ICollection, IEnumerable
        {
            private ArrayList _list;

            public TemplateEditingVerbCollection()
            {
            }

            internal TemplateEditingVerbCollection(TemplateEditingVerb[] verbs)
            {
                for (int i = 0; i < verbs.Length; i++)
                {
                    this.Add(verbs[i]);
                }
            }

            public int Add(TemplateEditingVerb verb)
            {
                return this.InternalList.Add(verb);
            }

            public void Clear()
            {
                this.InternalList.Clear();
            }

            public bool Contains(TemplateEditingVerb verb)
            {
                return this.InternalList.Contains(verb);
            }

            public int IndexOf(TemplateEditingVerb verb)
            {
                return this.InternalList.IndexOf(verb);
            }

            public void Insert(int index, TemplateEditingVerb verb)
            {
                this.InternalList.Insert(index, verb);
            }

            public void Remove(TemplateEditingVerb verb)
            {
                this.InternalList.Remove(verb);
            }

            public void RemoveAt(int index)
            {
                this.InternalList.RemoveAt(index);
            }

            void ICollection.CopyTo(Array array, int index)
            {
                this.InternalList.CopyTo(array, index);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.InternalList.GetEnumerator();
            }

            int IList.Add(object o)
            {
                if (!(o is TemplateEditingVerb))
                {
                    throw new ArgumentException();
                }
                return this.Add((TemplateEditingVerb) o);
            }

            void IList.Clear()
            {
                this.Clear();
            }

            bool IList.Contains(object o)
            {
                if (!(o is TemplateEditingVerb))
                {
                    throw new ArgumentException();
                }
                return this.Contains((TemplateEditingVerb) o);
            }

            int IList.IndexOf(object o)
            {
                if (!(o is TemplateEditingVerb))
                {
                    throw new ArgumentException();
                }
                return this.IndexOf((TemplateEditingVerb) o);
            }

            void IList.Insert(int index, object o)
            {
                if (!(o is TemplateEditingVerb))
                {
                    throw new ArgumentException();
                }
                this.Insert(index, (TemplateEditingVerb) o);
            }

            void IList.Remove(object o)
            {
                if (!(o is TemplateEditingVerb))
                {
                    throw new ArgumentException();
                }
                this.Remove((TemplateEditingVerb) o);
            }

            void IList.RemoveAt(int index)
            {
                this.RemoveAt(index);
            }

            public int Count
            {
                get
                {
                    return this.InternalList.Count;
                }
            }

            private ArrayList InternalList
            {
                get
                {
                    if (this._list == null)
                    {
                        this._list = new ArrayList();
                    }
                    return this._list;
                }
            }

            public TemplateEditingVerb this[int index]
            {
                get
                {
                    return (TemplateEditingVerb) this.InternalList[index];
                }
                set
                {
                    this.InternalList[index] = value;
                }
            }

            int ICollection.Count
            {
                get
                {
                    return this.Count;
                }
            }

            bool ICollection.IsSynchronized
            {
                get
                {
                    return this.InternalList.IsSynchronized;
                }
            }

            object ICollection.SyncRoot
            {
                get
                {
                    return this.InternalList.SyncRoot;
                }
            }

            bool IList.IsFixedSize
            {
                get
                {
                    return this.InternalList.IsFixedSize;
                }
            }

            bool IList.IsReadOnly
            {
                get
                {
                    return this.InternalList.IsReadOnly;
                }
            }

            object IList.this[int index]
            {
                get
                {
                    return this[index];
                }
                set
                {
                    if (!(value is TemplateEditingVerb))
                    {
                        throw new ArgumentException();
                    }
                    this[index] = (TemplateEditingVerb) value;
                }
            }
        }
    }
}

