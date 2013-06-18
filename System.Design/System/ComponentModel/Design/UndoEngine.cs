namespace System.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Design;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public abstract class UndoEngine : IDisposable
    {
        private IComponentChangeService _componentChangeService;
        private bool _enabled;
        private UndoUnit _executingUnit;
        private IDesignerHost _host;
        private IServiceProvider _provider;
        private Dictionary<IComponent, List<ReferencingComponent>> _refToRemovedComponent;
        private ComponentSerializationService _serializationService;
        private Stack _unitStack;
        private static TraceSwitch traceUndo = new TraceSwitch("UndoEngine", "Trace UndoRedo");

        public event EventHandler Undoing;

        public event EventHandler Undone;

        protected UndoEngine(IServiceProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            this._provider = provider;
            this._unitStack = new Stack();
            this._enabled = true;
            this._host = this.GetRequiredService(typeof(IDesignerHost)) as IDesignerHost;
            this._componentChangeService = this.GetRequiredService(typeof(IComponentChangeService)) as IComponentChangeService;
            this._serializationService = this.GetRequiredService(typeof(ComponentSerializationService)) as ComponentSerializationService;
            this._host.TransactionOpening += new EventHandler(this.OnTransactionOpening);
            this._host.TransactionClosed += new DesignerTransactionCloseEventHandler(this.OnTransactionClosed);
            this._componentChangeService.ComponentAdding += new ComponentEventHandler(this.OnComponentAdding);
            this._componentChangeService.ComponentChanging += new ComponentChangingEventHandler(this.OnComponentChanging);
            this._componentChangeService.ComponentRemoving += new ComponentEventHandler(this.OnComponentRemoving);
            this._componentChangeService.ComponentAdded += new ComponentEventHandler(this.OnComponentAdded);
            this._componentChangeService.ComponentChanged += new ComponentChangedEventHandler(this.OnComponentChanged);
            this._componentChangeService.ComponentRemoved += new ComponentEventHandler(this.OnComponentRemoved);
            this._componentChangeService.ComponentRename += new ComponentRenameEventHandler(this.OnComponentRename);
        }

        protected abstract void AddUndoUnit(UndoUnit unit);
        private void CheckPopUnit(PopUnitReason reason)
        {
            if ((reason != PopUnitReason.Normal) || !this._host.InTransaction)
            {
                UndoUnit unit = (UndoUnit) this._unitStack.Pop();
                if (!unit.IsEmpty)
                {
                    unit.Close();
                    if (reason != PopUnitReason.TransactionCancel)
                    {
                        if (this._unitStack.Count == 0)
                        {
                            this.AddUndoUnit(unit);
                        }
                    }
                    else
                    {
                        unit.Undo();
                        if (this._unitStack.Count == 0)
                        {
                            this.DiscardUndoUnit(unit);
                        }
                    }
                }
                else if (this._unitStack.Count == 0)
                {
                    this.DiscardUndoUnit(unit);
                }
            }
        }

        protected virtual UndoUnit CreateUndoUnit(string name, bool primary)
        {
            return new UndoUnit(this, name);
        }

        protected virtual void DiscardUndoUnit(UndoUnit unit)
        {
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this._host != null)
                {
                    this._host.TransactionOpening -= new EventHandler(this.OnTransactionOpening);
                    this._host.TransactionClosed -= new DesignerTransactionCloseEventHandler(this.OnTransactionClosed);
                }
                if (this._componentChangeService != null)
                {
                    this._componentChangeService.ComponentAdding -= new ComponentEventHandler(this.OnComponentAdding);
                    this._componentChangeService.ComponentChanging -= new ComponentChangingEventHandler(this.OnComponentChanging);
                    this._componentChangeService.ComponentRemoving -= new ComponentEventHandler(this.OnComponentRemoving);
                    this._componentChangeService.ComponentAdded -= new ComponentEventHandler(this.OnComponentAdded);
                    this._componentChangeService.ComponentChanged -= new ComponentChangedEventHandler(this.OnComponentChanged);
                    this._componentChangeService.ComponentRemoved -= new ComponentEventHandler(this.OnComponentRemoved);
                    this._componentChangeService.ComponentRename -= new ComponentRenameEventHandler(this.OnComponentRename);
                }
                this._provider = null;
            }
        }

        internal string GetName(object obj, bool generateNew)
        {
            string name = null;
            if (obj != null)
            {
                IReferenceService service = this.GetService(typeof(IReferenceService)) as IReferenceService;
                if (service != null)
                {
                    name = service.GetName(obj);
                }
                else
                {
                    IComponent component = obj as IComponent;
                    if (component != null)
                    {
                        ISite site = component.Site;
                        if (site != null)
                        {
                            name = site.Name;
                        }
                    }
                }
            }
            if ((name != null) || !generateNew)
            {
                return name;
            }
            if (obj == null)
            {
                return "(null)";
            }
            return obj.GetType().Name;
        }

        protected object GetRequiredService(Type serviceType)
        {
            object service = this.GetService(serviceType);
            if (service == null)
            {
                Exception exception = new InvalidOperationException(System.Design.SR.GetString("UndoEngineMissingService", new object[] { serviceType.Name })) {
                    HelpLink = "UndoEngineMissingService"
                };
                throw exception;
            }
            return service;
        }

        protected object GetService(Type serviceType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }
            if (this._provider != null)
            {
                return this._provider.GetService(serviceType);
            }
            return null;
        }

        private void OnComponentAdded(object sender, ComponentEventArgs e)
        {
            foreach (UndoUnit unit in this._unitStack)
            {
                unit.ComponentAdded(e);
            }
            if (this.CurrentUnit != null)
            {
                this.CheckPopUnit(PopUnitReason.Normal);
            }
        }

        private void OnComponentAdding(object sender, ComponentEventArgs e)
        {
            if ((this._enabled && (this._executingUnit == null)) && (this._unitStack.Count == 0))
            {
                string str;
                if (e.Component != null)
                {
                    str = System.Design.SR.GetString("UndoEngineComponentAdd1", new object[] { this.GetName(e.Component, true) });
                }
                else
                {
                    str = System.Design.SR.GetString("UndoEngineComponentAdd0");
                }
                this._unitStack.Push(this.CreateUndoUnit(str, true));
            }
            foreach (UndoUnit unit in this._unitStack)
            {
                unit.ComponentAdding(e);
            }
        }

        private void OnComponentChanged(object sender, ComponentChangedEventArgs e)
        {
            foreach (UndoUnit unit in this._unitStack)
            {
                unit.ComponentChanged(e);
            }
            if (this.CurrentUnit != null)
            {
                this.CheckPopUnit(PopUnitReason.Normal);
            }
        }

        private void OnComponentChanging(object sender, ComponentChangingEventArgs e)
        {
            if ((this._enabled && (this._executingUnit == null)) && (this._unitStack.Count == 0))
            {
                string str;
                if ((e.Member != null) && (e.Component != null))
                {
                    str = System.Design.SR.GetString("UndoEngineComponentChange2", new object[] { this.GetName(e.Component, true), e.Member.Name });
                }
                else if (e.Component != null)
                {
                    str = System.Design.SR.GetString("UndoEngineComponentChange1", new object[] { this.GetName(e.Component, true) });
                }
                else
                {
                    str = System.Design.SR.GetString("UndoEngineComponentChange0");
                }
                this._unitStack.Push(this.CreateUndoUnit(str, true));
            }
            foreach (UndoUnit unit in this._unitStack)
            {
                unit.ComponentChanging(e);
            }
        }

        private void OnComponentRemoved(object sender, ComponentEventArgs e)
        {
            foreach (UndoUnit unit in this._unitStack)
            {
                unit.ComponentRemoved(e);
            }
            if (this.CurrentUnit != null)
            {
                this.CheckPopUnit(PopUnitReason.Normal);
            }
            List<ReferencingComponent> list = null;
            if (((this._refToRemovedComponent != null) && this._refToRemovedComponent.TryGetValue(e.Component, out list)) && ((list != null) && (this._componentChangeService != null)))
            {
                foreach (ReferencingComponent component in list)
                {
                    this._componentChangeService.OnComponentChanged(component.component, component.member, null, null);
                }
                this._refToRemovedComponent.Remove(e.Component);
            }
        }

        private void OnComponentRemoving(object sender, ComponentEventArgs e)
        {
            if ((this._enabled && (this._executingUnit == null)) && (this._unitStack.Count == 0))
            {
                string str;
                if (e.Component != null)
                {
                    str = System.Design.SR.GetString("UndoEngineComponentRemove1", new object[] { this.GetName(e.Component, true) });
                }
                else
                {
                    str = System.Design.SR.GetString("UndoEngineComponentRemove0");
                }
                this._unitStack.Push(this.CreateUndoUnit(str, true));
            }
            if ((this._enabled && (this._host != null)) && ((this._host.Container != null) && (this._componentChangeService != null)))
            {
                List<ReferencingComponent> list = null;
                foreach (IComponent component in this._host.Container.Components)
                {
                    if (component != e.Component)
                    {
                        foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(component))
                        {
                            if ((descriptor.PropertyType.IsAssignableFrom(e.Component.GetType()) && !descriptor.Attributes.Contains(DesignerSerializationVisibilityAttribute.Hidden)) && !descriptor.IsReadOnly)
                            {
                                object objA = null;
                                try
                                {
                                    objA = descriptor.GetValue(component);
                                }
                                catch (TargetInvocationException)
                                {
                                    continue;
                                }
                                if ((objA != null) && object.ReferenceEquals(objA, e.Component))
                                {
                                    if (list == null)
                                    {
                                        list = new List<ReferencingComponent>();
                                        if (this._refToRemovedComponent == null)
                                        {
                                            this._refToRemovedComponent = new Dictionary<IComponent, List<ReferencingComponent>>();
                                        }
                                        this._refToRemovedComponent[e.Component] = list;
                                    }
                                    this._componentChangeService.OnComponentChanging(component, descriptor);
                                    list.Add(new ReferencingComponent(component, descriptor));
                                }
                            }
                        }
                    }
                }
            }
            foreach (UndoUnit unit in this._unitStack)
            {
                unit.ComponentRemoving(e);
            }
        }

        private void OnComponentRename(object sender, ComponentRenameEventArgs e)
        {
            if ((this._enabled && (this._executingUnit == null)) && (this._unitStack.Count == 0))
            {
                string name = System.Design.SR.GetString("UndoEngineComponentRename", new object[] { e.OldName, e.NewName });
                this._unitStack.Push(this.CreateUndoUnit(name, true));
            }
            foreach (UndoUnit unit in this._unitStack)
            {
                unit.ComponentRename(e);
            }
        }

        private void OnTransactionClosed(object sender, DesignerTransactionCloseEventArgs e)
        {
            if ((this._executingUnit == null) && (this.CurrentUnit != null))
            {
                PopUnitReason reason = e.TransactionCommitted ? PopUnitReason.TransactionCommit : PopUnitReason.TransactionCancel;
                this.CheckPopUnit(reason);
            }
        }

        private void OnTransactionOpening(object sender, EventArgs e)
        {
            if (this._enabled && (this._executingUnit == null))
            {
                this._unitStack.Push(this.CreateUndoUnit(this._host.TransactionDescription, this._unitStack.Count == 0));
            }
        }

        protected virtual void OnUndoing(EventArgs e)
        {
            if (this._undoingEvent != null)
            {
                this._undoingEvent(this, e);
            }
        }

        protected virtual void OnUndone(EventArgs e)
        {
            if (this._undoneEvent != null)
            {
                this._undoneEvent(this, e);
            }
        }

        [Conditional("DEBUG")]
        private static void Trace(string text, params object[] values)
        {
        }

        internal IComponentChangeService ComponentChangeService
        {
            get
            {
                return this._componentChangeService;
            }
        }

        private UndoUnit CurrentUnit
        {
            get
            {
                if (this._unitStack.Count > 0)
                {
                    return (UndoUnit) this._unitStack.Peek();
                }
                return null;
            }
        }

        public bool Enabled
        {
            get
            {
                return this._enabled;
            }
            set
            {
                this._enabled = value;
            }
        }

        public bool UndoInProgress
        {
            get
            {
                return (this._executingUnit != null);
            }
        }

        private enum PopUnitReason
        {
            Normal,
            TransactionCommit,
            TransactionCancel
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ReferencingComponent
        {
            public IComponent component;
            public MemberDescriptor member;
            public ReferencingComponent(IComponent component, MemberDescriptor member)
            {
                this.component = component;
                this.member = member;
            }
        }

        protected class UndoUnit
        {
            private ArrayList _changeEvents;
            private System.ComponentModel.Design.UndoEngine _engine;
            private ArrayList _events;
            private ArrayList _ignoreAddedList;
            private ArrayList _ignoreAddingList;
            private Hashtable _lastSelection;
            private string _name;
            private ArrayList _removeEvents;
            private bool _reverse;

            public UndoUnit(System.ComponentModel.Design.UndoEngine engine, string name)
            {
                if (engine == null)
                {
                    throw new ArgumentNullException("engine");
                }
                if (name == null)
                {
                    name = string.Empty;
                }
                this._name = name;
                this._engine = engine;
                this._reverse = true;
                ISelectionService service = this._engine.GetService(typeof(ISelectionService)) as ISelectionService;
                if (service != null)
                {
                    ICollection selectedComponents = service.GetSelectedComponents();
                    Hashtable hashtable = new Hashtable();
                    foreach (object obj2 in selectedComponents)
                    {
                        IComponent component = obj2 as IComponent;
                        if ((component != null) && (component.Site != null))
                        {
                            hashtable[component.Site.Name] = component.Site.Container;
                        }
                    }
                    this._lastSelection = hashtable;
                }
            }

            private void AddEvent(UndoEvent e)
            {
                if (this._events == null)
                {
                    this._events = new ArrayList();
                }
                this._events.Add(e);
            }

            private bool CanRepositionEvent(int startIndex, ComponentChangedEventArgs e)
            {
                bool flag = false;
                bool flag2 = false;
                bool flag3 = false;
                for (int i = startIndex + 1; i < this._events.Count; i++)
                {
                    AddRemoveUndoEvent event2 = this._events[i] as AddRemoveUndoEvent;
                    RenameUndoEvent event3 = this._events[i] as RenameUndoEvent;
                    ChangeUndoEvent event4 = this._events[i] as ChangeUndoEvent;
                    if ((event2 != null) && !event2.NextUndoAdds)
                    {
                        flag = true;
                    }
                    else if ((event4 != null) && ChangeEventsSymmetric(event4.ComponentChangingEventArgs, e))
                    {
                        flag3 = true;
                    }
                    else if (event3 != null)
                    {
                        flag2 = true;
                    }
                }
                return ((flag && !flag2) && !flag3);
            }

            private static bool ChangeEventsSymmetric(ComponentChangingEventArgs changing, ComponentChangedEventArgs changed)
            {
                if ((changing == null) || (changed == null))
                {
                    return false;
                }
                return ((changing.Component == changed.Component) && (changing.Member == changed.Member));
            }

            public virtual void Close()
            {
                if (this._changeEvents != null)
                {
                    foreach (ChangeUndoEvent event2 in this._changeEvents)
                    {
                        event2.Commit(this._engine);
                    }
                }
                if (this._removeEvents != null)
                {
                    foreach (AddRemoveUndoEvent event3 in this._removeEvents)
                    {
                        event3.Commit(this._engine);
                    }
                }
                this._changeEvents = null;
                this._removeEvents = null;
                this._ignoreAddingList = null;
                this._ignoreAddedList = null;
            }

            public virtual void ComponentAdded(ComponentEventArgs e)
            {
                if ((e.Component.Site == null) || !(e.Component.Site.Container is INestedContainer))
                {
                    this.AddEvent(new AddRemoveUndoEvent(this._engine, e.Component, true));
                }
                if (this._ignoreAddingList != null)
                {
                    this._ignoreAddingList.Remove(e.Component);
                }
                if (this._ignoreAddedList == null)
                {
                    this._ignoreAddedList = new ArrayList();
                }
                this._ignoreAddedList.Add(e.Component);
            }

            public virtual void ComponentAdding(ComponentEventArgs e)
            {
                if (this._ignoreAddingList == null)
                {
                    this._ignoreAddingList = new ArrayList();
                }
                this._ignoreAddingList.Add(e.Component);
            }

            public virtual void ComponentChanged(ComponentChangedEventArgs e)
            {
                if ((this._events != null) && (e != null))
                {
                    for (int i = 0; i < this._events.Count; i++)
                    {
                        ChangeUndoEvent event2 = this._events[i] as ChangeUndoEvent;
                        if ((((event2 != null) && ChangeEventsSymmetric(event2.ComponentChangingEventArgs, e)) && ((i != (this._events.Count - 1)) && (e.Member != null))) && (e.Member.Attributes.Contains(DesignerSerializationVisibilityAttribute.Content) && this.CanRepositionEvent(i, e)))
                        {
                            this._events.RemoveAt(i);
                            this._events.Add(event2);
                        }
                    }
                }
            }

            public virtual void ComponentChanging(ComponentChangingEventArgs e)
            {
                if ((this._ignoreAddingList == null) || !this._ignoreAddingList.Contains(e.Component))
                {
                    if (this._changeEvents == null)
                    {
                        this._changeEvents = new ArrayList();
                    }
                    if ((this._engine != null) && (this._engine.GetName(e.Component, false) != null))
                    {
                        IComponent component = e.Component as IComponent;
                        bool flag = false;
                        for (int i = 0; i < this._changeEvents.Count; i++)
                        {
                            ChangeUndoEvent event2 = (ChangeUndoEvent) this._changeEvents[i];
                            if ((event2.OpenComponent == e.Component) && event2.ContainsChange(e.Member))
                            {
                                flag = true;
                                break;
                            }
                        }
                        if (!flag || (((e.Member != null) && (e.Member.Attributes != null)) && e.Member.Attributes.Contains(DesignerSerializationVisibilityAttribute.Content)))
                        {
                            ChangeUndoEvent event3 = null;
                            bool serializeBeforeState = true;
                            if ((this._ignoreAddedList != null) && this._ignoreAddedList.Contains(e.Component))
                            {
                                serializeBeforeState = false;
                            }
                            if ((component != null) && (component.Site != null))
                            {
                                event3 = new ChangeUndoEvent(this._engine, e, serializeBeforeState);
                            }
                            else if (e.Component != null)
                            {
                                IReferenceService service = this.GetService(typeof(IReferenceService)) as IReferenceService;
                                if (service != null)
                                {
                                    IComponent component2 = service.GetComponent(e.Component);
                                    if (component2 != null)
                                    {
                                        event3 = new ChangeUndoEvent(this._engine, new ComponentChangingEventArgs(component2, null), serializeBeforeState);
                                    }
                                }
                            }
                            if (event3 != null)
                            {
                                this.AddEvent(event3);
                                this._changeEvents.Add(event3);
                            }
                        }
                    }
                }
            }

            public virtual void ComponentRemoved(ComponentEventArgs e)
            {
                if ((this._events != null) && (e != null))
                {
                    ChangeUndoEvent event2 = null;
                    int index = -1;
                    for (int i = this._events.Count - 1; i >= 0; i--)
                    {
                        AddRemoveUndoEvent event3 = this._events[i] as AddRemoveUndoEvent;
                        if (event2 == null)
                        {
                            event2 = this._events[i] as ChangeUndoEvent;
                            index = i;
                        }
                        if ((event3 != null) && (event3.OpenComponent == e.Component))
                        {
                            event3.Commit(this._engine);
                            if ((i == (this._events.Count - 1)) || (event2 == null))
                            {
                                break;
                            }
                            bool flag = true;
                            for (int j = i + 1; j < index; j++)
                            {
                                if (!(this._events[j] is ChangeUndoEvent))
                                {
                                    flag = false;
                                    break;
                                }
                            }
                            if (!flag)
                            {
                                break;
                            }
                            this._events.RemoveAt(i);
                            this._events.Insert(index, event3);
                            return;
                        }
                    }
                }
            }

            public virtual void ComponentRemoving(ComponentEventArgs e)
            {
                if ((e.Component.Site == null) || !(e.Component.Site is INestedContainer))
                {
                    if (this._removeEvents == null)
                    {
                        this._removeEvents = new ArrayList();
                    }
                    try
                    {
                        AddRemoveUndoEvent event2 = new AddRemoveUndoEvent(this._engine, e.Component, false);
                        this.AddEvent(event2);
                        this._removeEvents.Add(event2);
                    }
                    catch (TargetInvocationException)
                    {
                    }
                }
            }

            public virtual void ComponentRename(ComponentRenameEventArgs e)
            {
                this.AddEvent(new RenameUndoEvent(e.OldName, e.NewName));
            }

            protected object GetService(Type serviceType)
            {
                return this._engine.GetService(serviceType);
            }

            public override string ToString()
            {
                return this.Name;
            }

            public void Undo()
            {
                System.ComponentModel.Design.UndoEngine.UndoUnit unit = this._engine._executingUnit;
                this._engine._executingUnit = this;
                DesignerTransaction transaction = null;
                try
                {
                    if (unit == null)
                    {
                        this._engine.OnUndoing(EventArgs.Empty);
                    }
                    transaction = this._engine._host.CreateTransaction();
                    this.UndoCore();
                }
                catch (CheckoutException)
                {
                    transaction.Cancel();
                    transaction = null;
                    throw;
                }
                finally
                {
                    if (transaction != null)
                    {
                        transaction.Commit();
                    }
                    this._engine._executingUnit = unit;
                    if (unit == null)
                    {
                        this._engine.OnUndone(EventArgs.Empty);
                    }
                }
            }

            protected virtual void UndoCore()
            {
                if (this._events != null)
                {
                    if (this._reverse)
                    {
                        for (int i = this._events.Count - 1; i >= 0; i--)
                        {
                            int num2 = i;
                            for (int j = i; j >= 0; j--)
                            {
                                if (!((UndoEvent) this._events[j]).CausesSideEffects)
                                {
                                    break;
                                }
                                num2 = j;
                            }
                            for (int k = i; k >= num2; k--)
                            {
                                ((UndoEvent) this._events[k]).BeforeUndo(this._engine);
                            }
                            for (int m = i; m >= num2; m--)
                            {
                                ((UndoEvent) this._events[m]).Undo(this._engine);
                            }
                            i = num2;
                        }
                        if (this._lastSelection != null)
                        {
                            ISelectionService service = this._engine.GetService(typeof(ISelectionService)) as ISelectionService;
                            if (service != null)
                            {
                                string[] array = new string[this._lastSelection.Keys.Count];
                                this._lastSelection.Keys.CopyTo(array, 0);
                                ArrayList components = new ArrayList(array.Length);
                                foreach (string str in array)
                                {
                                    if (str != null)
                                    {
                                        object obj2 = ((Container) this._lastSelection[str]).Components[str];
                                        if (obj2 != null)
                                        {
                                            components.Add(obj2);
                                        }
                                    }
                                }
                                service.SetSelectedComponents(components, SelectionTypes.Replace);
                            }
                        }
                    }
                    else
                    {
                        int count = this._events.Count;
                        for (int n = 0; n < count; n++)
                        {
                            int num8 = n;
                            for (int num9 = n; num9 < count; num9++)
                            {
                                if (!((UndoEvent) this._events[num9]).CausesSideEffects)
                                {
                                    break;
                                }
                                num8 = num9;
                            }
                            for (int num10 = n; num10 <= num8; num10++)
                            {
                                ((UndoEvent) this._events[num10]).BeforeUndo(this._engine);
                            }
                            for (int num11 = n; num11 <= num8; num11++)
                            {
                                ((UndoEvent) this._events[num11]).Undo(this._engine);
                            }
                            n = num8;
                        }
                    }
                }
                this._reverse = !this._reverse;
            }

            public virtual bool IsEmpty
            {
                get
                {
                    if (this._events != null)
                    {
                        return (this._events.Count == 0);
                    }
                    return true;
                }
            }

            public string Name
            {
                get
                {
                    return this._name;
                }
            }

            protected System.ComponentModel.Design.UndoEngine UndoEngine
            {
                get
                {
                    return this._engine;
                }
            }

            private sealed class AddRemoveUndoEvent : UndoEngine.UndoUnit.UndoEvent
            {
                private bool _committed;
                private string _componentName;
                private bool _nextUndoAdds;
                private IComponent _openComponent;
                private SerializationStore _serializedData;

                public AddRemoveUndoEvent(UndoEngine engine, IComponent component, bool add)
                {
                    this._componentName = component.Site.Name;
                    this._nextUndoAdds = !add;
                    this._openComponent = component;
                    using (this._serializedData = engine._serializationService.CreateStore())
                    {
                        engine._serializationService.Serialize(this._serializedData, component);
                    }
                    this._committed = add;
                }

                internal void Commit(UndoEngine engine)
                {
                    if (!this.Committed)
                    {
                        this._committed = true;
                    }
                }

                public override void Undo(UndoEngine engine)
                {
                    if (this._nextUndoAdds)
                    {
                        IDesignerHost requiredService = engine.GetRequiredService(typeof(IDesignerHost)) as IDesignerHost;
                        if (requiredService != null)
                        {
                            engine._serializationService.DeserializeTo(this._serializedData, requiredService.Container);
                        }
                    }
                    else
                    {
                        IDesignerHost host2 = engine.GetRequiredService(typeof(IDesignerHost)) as IDesignerHost;
                        IComponent component = host2.Container.Components[this._componentName];
                        if (component != null)
                        {
                            host2.DestroyComponent(component);
                        }
                    }
                    this._nextUndoAdds = !this._nextUndoAdds;
                }

                internal bool Committed
                {
                    get
                    {
                        return this._committed;
                    }
                }

                internal bool NextUndoAdds
                {
                    get
                    {
                        return this._nextUndoAdds;
                    }
                }

                internal IComponent OpenComponent
                {
                    get
                    {
                        return this._openComponent;
                    }
                }
            }

            private sealed class ChangeUndoEvent : UndoEngine.UndoUnit.UndoEvent
            {
                private SerializationStore _after;
                private SerializationStore _before;
                private string _componentName;
                private MemberDescriptor _member;
                private object _openComponent;
                private bool _savedAfterState;

                public ChangeUndoEvent(UndoEngine engine, System.ComponentModel.Design.ComponentChangingEventArgs e, bool serializeBeforeState)
                {
                    this._componentName = engine.GetName(e.Component, true);
                    this._openComponent = e.Component;
                    this._member = e.Member;
                    if (serializeBeforeState)
                    {
                        this._before = this.Serialize(engine, this._openComponent, this._member);
                    }
                }

                public override void BeforeUndo(UndoEngine engine)
                {
                    if (!this._savedAfterState)
                    {
                        this._savedAfterState = true;
                        this.SaveAfterState(engine);
                    }
                }

                public void Commit(UndoEngine engine)
                {
                    if (!this.Committed)
                    {
                        this._openComponent = null;
                    }
                }

                public bool ContainsChange(MemberDescriptor desc)
                {
                    if (this._member == null)
                    {
                        return true;
                    }
                    if (desc == null)
                    {
                        return false;
                    }
                    return desc.Equals(this._member);
                }

                private void SaveAfterState(UndoEngine engine)
                {
                    object component = null;
                    IReferenceService service = engine.GetService(typeof(IReferenceService)) as IReferenceService;
                    if (service != null)
                    {
                        component = service.GetReference(this._componentName);
                    }
                    else
                    {
                        IDesignerHost host = engine.GetService(typeof(IDesignerHost)) as IDesignerHost;
                        if (host != null)
                        {
                            component = host.Container.Components[this._componentName];
                        }
                    }
                    if (component != null)
                    {
                        this._after = this.Serialize(engine, component, this._member);
                    }
                }

                private SerializationStore Serialize(UndoEngine engine, object component, MemberDescriptor member)
                {
                    SerializationStore store;
                    using (store = engine._serializationService.CreateStore())
                    {
                        if ((member != null) && !member.Attributes.Contains(DesignerSerializationVisibilityAttribute.Hidden))
                        {
                            engine._serializationService.SerializeMemberAbsolute(store, component, member);
                            return store;
                        }
                        engine._serializationService.SerializeAbsolute(store, component);
                    }
                    return store;
                }

                public override void Undo(UndoEngine engine)
                {
                    if (this._before != null)
                    {
                        IDesignerHost service = engine.GetService(typeof(IDesignerHost)) as IDesignerHost;
                        if (service != null)
                        {
                            engine._serializationService.DeserializeTo(this._before, service.Container);
                        }
                    }
                    SerializationStore store = this._after;
                    this._after = this._before;
                    this._before = store;
                }

                public override bool CausesSideEffects
                {
                    get
                    {
                        return true;
                    }
                }

                public bool Committed
                {
                    get
                    {
                        return (this._openComponent == null);
                    }
                }

                public System.ComponentModel.Design.ComponentChangingEventArgs ComponentChangingEventArgs
                {
                    get
                    {
                        return new System.ComponentModel.Design.ComponentChangingEventArgs(this._openComponent, this._member);
                    }
                }

                public object OpenComponent
                {
                    get
                    {
                        return this._openComponent;
                    }
                }
            }

            private sealed class RenameUndoEvent : UndoEngine.UndoUnit.UndoEvent
            {
                private string _after;
                private string _before;

                public RenameUndoEvent(string before, string after)
                {
                    this._before = before;
                    this._after = after;
                }

                public override void Undo(UndoEngine engine)
                {
                    IComponent component = engine._host.Container.Components[this._after];
                    if (component != null)
                    {
                        engine.ComponentChangeService.OnComponentChanging(component, null);
                        component.Site.Name = this._before;
                        string str = this._after;
                        this._after = this._before;
                        this._before = str;
                    }
                }
            }

            private abstract class UndoEvent
            {
                protected UndoEvent()
                {
                }

                public virtual void BeforeUndo(UndoEngine engine)
                {
                }

                public abstract void Undo(UndoEngine engine);

                public virtual bool CausesSideEffects
                {
                    get
                    {
                        return false;
                    }
                }
            }
        }
    }
}

