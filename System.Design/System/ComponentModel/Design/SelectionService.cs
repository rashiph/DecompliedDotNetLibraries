namespace System.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    internal sealed class SelectionService : ISelectionService, IDisposable
    {
        private string[] _contextAttributes;
        private short _contextKeyword;
        private EventHandlerList _events;
        private IServiceProvider _provider;
        private ArrayList _selection;
        private BitVector32 _state;
        private StatusCommandUI _statusCommandUI;
        private static readonly object EventSelectionChanged = new object();
        private static readonly object EventSelectionChanging = new object();
        private static readonly string[] SelectionKeywords = new string[] { "None", "Single", "Multiple" };
        private static readonly int StateTransaction = BitVector32.CreateMask();
        private static readonly int StateTransactionChange = BitVector32.CreateMask(StateTransaction);

        event EventHandler ISelectionService.SelectionChanged
        {
            add
            {
                this._events.AddHandler(EventSelectionChanged, value);
            }
            remove
            {
                this._events.RemoveHandler(EventSelectionChanged, value);
            }
        }

        event EventHandler ISelectionService.SelectionChanging
        {
            add
            {
                this._events.AddHandler(EventSelectionChanging, value);
            }
            remove
            {
                this._events.RemoveHandler(EventSelectionChanging, value);
            }
        }

        internal SelectionService(IServiceProvider provider)
        {
            this._provider = provider;
            this._state = new BitVector32();
            this._events = new EventHandlerList();
            this._statusCommandUI = new StatusCommandUI(provider);
        }

        internal void AddSelection(object sel)
        {
            if (this._selection == null)
            {
                this._selection = new ArrayList();
                IComponentChangeService service = this.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                if (service != null)
                {
                    service.ComponentRemoved += new ComponentEventHandler(this.OnComponentRemove);
                }
                IDesignerHost sender = this.GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (sender != null)
                {
                    sender.TransactionOpened += new EventHandler(this.OnTransactionOpened);
                    sender.TransactionClosed += new DesignerTransactionCloseEventHandler(this.OnTransactionClosed);
                    if (sender.InTransaction)
                    {
                        this.OnTransactionOpened(sender, EventArgs.Empty);
                    }
                }
            }
            if (!this._selection.Contains(sel))
            {
                this._selection.Add(sel);
            }
        }

        private void ApplicationIdle(object source, EventArgs args)
        {
            this.UpdateHelpKeyword(false);
            Application.Idle -= new EventHandler(this.ApplicationIdle);
        }

        private void FlushSelectionChanges()
        {
            if (!this._state[StateTransaction] && this._state[StateTransactionChange])
            {
                this._state[StateTransactionChange] = false;
                this.OnSelectionChanged();
            }
        }

        private object GetService(System.Type serviceType)
        {
            if (this._provider != null)
            {
                return this._provider.GetService(serviceType);
            }
            return null;
        }

        private void OnComponentRemove(object sender, ComponentEventArgs ce)
        {
            if ((this._selection != null) && this._selection.Contains(ce.Component))
            {
                this.RemoveSelection(ce.Component);
                this.OnSelectionChanged();
            }
        }

        private void OnSelectionChanged()
        {
            if (this._state[StateTransaction])
            {
                this._state[StateTransactionChange] = true;
            }
            else
            {
                EventHandler handler = this._events[EventSelectionChanging] as EventHandler;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
                this.UpdateHelpKeyword(true);
                handler = this._events[EventSelectionChanged] as EventHandler;
                if (handler != null)
                {
                    try
                    {
                        handler(this, EventArgs.Empty);
                    }
                    catch
                    {
                    }
                }
            }
        }

        private void OnTransactionClosed(object sender, DesignerTransactionCloseEventArgs e)
        {
            if (e.LastTransaction)
            {
                this._state[StateTransaction] = false;
                this.FlushSelectionChanges();
            }
        }

        private void OnTransactionOpened(object sender, EventArgs e)
        {
            this._state[StateTransaction] = true;
        }

        internal void RemoveSelection(object sel)
        {
            if (this._selection != null)
            {
                this._selection.Remove(sel);
            }
        }

        bool ISelectionService.GetComponentSelected(object component)
        {
            if (component == null)
            {
                throw new ArgumentNullException("component");
            }
            return ((this._selection != null) && this._selection.Contains(component));
        }

        ICollection ISelectionService.GetSelectedComponents()
        {
            if (this._selection != null)
            {
                object[] array = new object[this._selection.Count];
                this._selection.CopyTo(array, 0);
                return array;
            }
            return new object[0];
        }

        void ISelectionService.SetSelectedComponents(ICollection components)
        {
            ((ISelectionService) this).SetSelectedComponents(components, SelectionTypes.Auto);
        }

        void ISelectionService.SetSelectedComponents(ICollection components, SelectionTypes selectionType)
        {
            int num;
            bool flag = (selectionType & SelectionTypes.Toggle) == SelectionTypes.Toggle;
            bool flag2 = (selectionType & SelectionTypes.Click) == SelectionTypes.Click;
            bool flag3 = (selectionType & SelectionTypes.Add) == SelectionTypes.Add;
            bool flag4 = (selectionType & SelectionTypes.Remove) == SelectionTypes.Remove;
            bool flag5 = (selectionType & SelectionTypes.Replace) == SelectionTypes.Replace;
            bool flag6 = !(((flag | flag3) | flag4) | flag5);
            if (components == null)
            {
                components = new object[0];
            }
            if (flag6)
            {
                flag = (Control.ModifierKeys & (Keys.Control | Keys.Shift)) > Keys.None;
                flag3 |= Control.ModifierKeys == Keys.Shift;
                if (flag || flag3)
                {
                    flag2 = false;
                }
            }
            bool flag7 = false;
            object obj2 = null;
            if (flag2 && (1 == components.Count))
            {
                foreach (object obj3 in components)
                {
                    obj2 = obj3;
                    if (obj3 == null)
                    {
                        throw new ArgumentNullException("components");
                    }
                    break;
                }
            }
            if (((obj2 != null) && (this._selection != null)) && ((num = this._selection.IndexOf(obj2)) != -1))
            {
                if (num != 0)
                {
                    object obj4 = this._selection[0];
                    this._selection[0] = this._selection[num];
                    this._selection[num] = obj4;
                    flag7 = true;
                }
            }
            else
            {
                if ((!flag && !flag3) && (!flag4 && (this._selection != null)))
                {
                    object[] array = new object[this._selection.Count];
                    this._selection.CopyTo(array, 0);
                    foreach (object obj5 in array)
                    {
                        bool flag8 = true;
                        foreach (object obj6 in components)
                        {
                            if (obj6 == null)
                            {
                                throw new ArgumentNullException("components");
                            }
                            if (object.ReferenceEquals(obj6, obj5))
                            {
                                flag8 = false;
                                break;
                            }
                        }
                        if (flag8)
                        {
                            this.RemoveSelection(obj5);
                            flag7 = true;
                        }
                    }
                }
                foreach (object obj7 in components)
                {
                    if (obj7 == null)
                    {
                        throw new ArgumentNullException("components");
                    }
                    if ((this._selection != null) && this._selection.Contains(obj7))
                    {
                        if (flag || flag4)
                        {
                            this.RemoveSelection(obj7);
                            flag7 = true;
                        }
                    }
                    else if (!flag4)
                    {
                        this.AddSelection(obj7);
                        flag7 = true;
                    }
                }
            }
            if (flag7)
            {
                if (this._selection.Count > 0)
                {
                    this._statusCommandUI.SetStatusInformation(this._selection[0] as Component);
                }
                else
                {
                    this._statusCommandUI.SetStatusInformation(Rectangle.Empty);
                }
                this.OnSelectionChanged();
            }
        }

        void IDisposable.Dispose()
        {
            if (this._selection != null)
            {
                IDesignerHost sender = this.GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (sender != null)
                {
                    sender.TransactionOpened -= new EventHandler(this.OnTransactionOpened);
                    sender.TransactionClosed -= new DesignerTransactionCloseEventHandler(this.OnTransactionClosed);
                    if (sender.InTransaction)
                    {
                        this.OnTransactionClosed(sender, new DesignerTransactionCloseEventArgs(true, true));
                    }
                }
                IComponentChangeService service = this.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                if (service != null)
                {
                    service.ComponentRemoved -= new ComponentEventHandler(this.OnComponentRemove);
                }
                this._selection.Clear();
            }
            this._statusCommandUI = null;
            this._provider = null;
        }

        private void UpdateHelpKeyword(bool tryLater)
        {
            IHelpService service = this.GetService(typeof(IHelpService)) as IHelpService;
            if (service == null)
            {
                if (tryLater)
                {
                    Application.Idle += new EventHandler(this.ApplicationIdle);
                }
            }
            else
            {
                if (this._contextAttributes != null)
                {
                    foreach (string str in this._contextAttributes)
                    {
                        service.RemoveContextAttribute("Keyword", str);
                    }
                    this._contextAttributes = null;
                }
                service.RemoveContextAttribute("Selection", SelectionKeywords[this._contextKeyword]);
                bool flag = false;
                if (this._selection.Count == 0)
                {
                    flag = true;
                }
                else if (this._selection.Count == 1)
                {
                    IDesignerHost host = this.GetService(typeof(IDesignerHost)) as IDesignerHost;
                    if ((host != null) && this._selection.Contains(host.RootComponent))
                    {
                        flag = true;
                    }
                }
                this._contextAttributes = new string[this._selection.Count];
                for (int i = 0; i < this._selection.Count; i++)
                {
                    object component = this._selection[i];
                    string className = TypeDescriptor.GetClassName(component);
                    HelpKeywordAttribute attribute = (HelpKeywordAttribute) TypeDescriptor.GetAttributes(component)[typeof(HelpKeywordAttribute)];
                    if ((attribute != null) && !attribute.IsDefaultAttribute())
                    {
                        className = attribute.HelpKeyword;
                    }
                    this._contextAttributes[i] = className;
                }
                HelpKeywordType keywordType = flag ? HelpKeywordType.GeneralKeyword : HelpKeywordType.F1Keyword;
                foreach (string str3 in this._contextAttributes)
                {
                    service.AddContextAttribute("Keyword", str3, keywordType);
                }
                int count = this._selection.Count;
                if ((count == 1) && flag)
                {
                    count--;
                }
                this._contextKeyword = (short) Math.Min(count, SelectionKeywords.Length - 1);
                service.AddContextAttribute("Selection", SelectionKeywords[this._contextKeyword], HelpKeywordType.FilterKeyword);
            }
        }

        object ISelectionService.PrimarySelection
        {
            get
            {
                if ((this._selection != null) && (this._selection.Count > 0))
                {
                    return this._selection[0];
                }
                return null;
            }
        }

        int ISelectionService.SelectionCount
        {
            get
            {
                if (this._selection != null)
                {
                    return this._selection.Count;
                }
                return 0;
            }
        }
    }
}

