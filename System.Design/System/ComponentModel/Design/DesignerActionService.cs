namespace System.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;

    public class DesignerActionService : IDisposable
    {
        private Hashtable componentToVerbsEventHookedUp;
        private Hashtable designerActionLists;
        private bool reEntrantCode;
        private ISelectionService selSvc;
        private IServiceProvider serviceProvider;

        public event DesignerActionListsChangedEventHandler DesignerActionListsChanged;

        internal event DesignerActionUIStateChangeEventHandler DesignerActionUIStateChange
        {
            add
            {
                DesignerActionUIService service = (DesignerActionUIService) this.serviceProvider.GetService(typeof(DesignerActionUIService));
                if (service != null)
                {
                    service.DesignerActionUIStateChange += value;
                }
            }
            remove
            {
                DesignerActionUIService service = (DesignerActionUIService) this.serviceProvider.GetService(typeof(DesignerActionUIService));
                if (service != null)
                {
                    service.DesignerActionUIStateChange -= value;
                }
            }
        }

        public DesignerActionService(IServiceProvider serviceProvider)
        {
            if (serviceProvider != null)
            {
                this.serviceProvider = serviceProvider;
                ((IDesignerHost) serviceProvider.GetService(typeof(IDesignerHost))).AddService(typeof(DesignerActionService), this);
                IComponentChangeService service = (IComponentChangeService) serviceProvider.GetService(typeof(IComponentChangeService));
                if (service != null)
                {
                    service.ComponentRemoved += new ComponentEventHandler(this.OnComponentRemoved);
                }
                this.selSvc = (ISelectionService) serviceProvider.GetService(typeof(ISelectionService));
                ISelectionService selSvc = this.selSvc;
            }
            this.designerActionLists = new Hashtable();
            this.componentToVerbsEventHookedUp = new Hashtable();
        }

        public void Add(IComponent comp, DesignerActionList actionList)
        {
            this.Add(comp, new DesignerActionListCollection(actionList));
        }

        public void Add(IComponent comp, DesignerActionListCollection designerActionListCollection)
        {
            if (comp == null)
            {
                throw new ArgumentNullException("comp");
            }
            if (designerActionListCollection == null)
            {
                throw new ArgumentNullException("designerActionListCollection");
            }
            DesignerActionListCollection lists = (DesignerActionListCollection) this.designerActionLists[comp];
            if (lists != null)
            {
                lists.AddRange(designerActionListCollection);
            }
            else
            {
                this.designerActionLists.Add(comp, designerActionListCollection);
            }
            this.OnDesignerActionListsChanged(new DesignerActionListsChangedEventArgs(comp, DesignerActionListsChangedType.ActionListsAdded, this.GetComponentActions(comp)));
        }

        public void Clear()
        {
            if (this.designerActionLists.Count != 0)
            {
                ArrayList list = new ArrayList(this.designerActionLists.Count);
                foreach (DictionaryEntry entry in this.designerActionLists)
                {
                    list.Add(entry.Key);
                }
                this.designerActionLists.Clear();
                foreach (Component component in list)
                {
                    this.OnDesignerActionListsChanged(new DesignerActionListsChangedEventArgs(component, DesignerActionListsChangedType.ActionListsRemoved, this.GetComponentActions(component)));
                }
            }
        }

        public bool Contains(IComponent comp)
        {
            if (comp == null)
            {
                throw new ArgumentNullException("comp");
            }
            return this.designerActionLists.Contains(comp);
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && (this.serviceProvider != null))
            {
                IDesignerHost host = (IDesignerHost) this.serviceProvider.GetService(typeof(IDesignerHost));
                if (host != null)
                {
                    host.RemoveService(typeof(DesignerActionService));
                }
                IComponentChangeService service = (IComponentChangeService) this.serviceProvider.GetService(typeof(IComponentChangeService));
                if (service != null)
                {
                    service.ComponentRemoved -= new ComponentEventHandler(this.OnComponentRemoved);
                }
            }
        }

        public DesignerActionListCollection GetComponentActions(IComponent component)
        {
            return this.GetComponentActions(component, ComponentActionsType.All);
        }

        public virtual DesignerActionListCollection GetComponentActions(IComponent component, ComponentActionsType type)
        {
            if (component == null)
            {
                throw new ArgumentNullException("component");
            }
            DesignerActionListCollection actionLists = new DesignerActionListCollection();
            switch (type)
            {
                case ComponentActionsType.All:
                    this.GetComponentDesignerActions(component, actionLists);
                    this.GetComponentServiceActions(component, actionLists);
                    return actionLists;

                case ComponentActionsType.Component:
                    this.GetComponentDesignerActions(component, actionLists);
                    return actionLists;

                case ComponentActionsType.Service:
                    this.GetComponentServiceActions(component, actionLists);
                    return actionLists;
            }
            return actionLists;
        }

        protected virtual void GetComponentDesignerActions(IComponent component, DesignerActionListCollection actionLists)
        {
            if (component == null)
            {
                throw new ArgumentNullException("component");
            }
            if (actionLists == null)
            {
                throw new ArgumentNullException("actionLists");
            }
            IServiceContainer site = component.Site as IServiceContainer;
            if (site != null)
            {
                DesignerCommandSet service = (DesignerCommandSet) site.GetService(typeof(DesignerCommandSet));
                if (service != null)
                {
                    DesignerActionListCollection lists = service.ActionLists;
                    if (lists != null)
                    {
                        actionLists.AddRange(lists);
                    }
                    if (actionLists.Count == 0)
                    {
                        DesignerVerbCollection verbs = service.Verbs;
                        if ((verbs != null) && (verbs.Count != 0))
                        {
                            ArrayList list = new ArrayList();
                            bool flag = this.componentToVerbsEventHookedUp[component] == null;
                            if (flag)
                            {
                                this.componentToVerbsEventHookedUp[component] = true;
                            }
                            foreach (DesignerVerb verb in verbs)
                            {
                                if (flag)
                                {
                                    verb.CommandChanged += new EventHandler(this.OnVerbStatusChanged);
                                }
                                if (verb.Enabled && verb.Visible)
                                {
                                    list.Add(verb);
                                }
                            }
                            if (list.Count != 0)
                            {
                                DesignerActionVerbList list2 = new DesignerActionVerbList((DesignerVerb[]) list.ToArray(typeof(DesignerVerb)));
                                actionLists.Add(list2);
                            }
                        }
                    }
                    if (lists != null)
                    {
                        foreach (DesignerActionList list3 in lists)
                        {
                            DesignerActionItemCollection sortedActionItems = list3.GetSortedActionItems();
                            if ((sortedActionItems == null) || (sortedActionItems.Count == 0))
                            {
                                actionLists.Remove(list3);
                            }
                        }
                    }
                }
            }
        }

        protected virtual void GetComponentServiceActions(IComponent component, DesignerActionListCollection actionLists)
        {
            if (component == null)
            {
                throw new ArgumentNullException("component");
            }
            if (actionLists == null)
            {
                throw new ArgumentNullException("actionLists");
            }
            DesignerActionListCollection lists = (DesignerActionListCollection) this.designerActionLists[component];
            if (lists != null)
            {
                actionLists.AddRange(lists);
                foreach (DesignerActionList list in lists)
                {
                    DesignerActionItemCollection sortedActionItems = list.GetSortedActionItems();
                    if ((sortedActionItems == null) || (sortedActionItems.Count == 0))
                    {
                        actionLists.Remove(list);
                    }
                }
            }
        }

        private void OnComponentRemoved(object source, ComponentEventArgs ce)
        {
            this.Remove(ce.Component);
        }

        private void OnDesignerActionListsChanged(DesignerActionListsChangedEventArgs e)
        {
            if (this.designerActionListsChanged != null)
            {
                this.designerActionListsChanged(this, e);
            }
        }

        private void OnVerbStatusChanged(object sender, EventArgs args)
        {
            if (!this.reEntrantCode)
            {
                try
                {
                    this.reEntrantCode = true;
                    IComponent primarySelection = this.selSvc.PrimarySelection as IComponent;
                    if (primarySelection != null)
                    {
                        IServiceContainer site = primarySelection.Site as IServiceContainer;
                        if (site != null)
                        {
                            DesignerCommandSet set = (DesignerCommandSet) site.GetService(typeof(DesignerCommandSet));
                            foreach (DesignerVerb verb in set.Verbs)
                            {
                                if (verb == sender)
                                {
                                    DesignerActionUIService service = (DesignerActionUIService) site.GetService(typeof(DesignerActionUIService));
                                    if (service != null)
                                    {
                                        service.Refresh(primarySelection);
                                    }
                                }
                            }
                        }
                    }
                }
                finally
                {
                    this.reEntrantCode = false;
                }
            }
        }

        public void Remove(DesignerActionList actionList)
        {
            if (actionList == null)
            {
                throw new ArgumentNullException("actionList");
            }
            foreach (IComponent component in this.designerActionLists.Keys)
            {
                if (((DesignerActionListCollection) this.designerActionLists[component]).Contains(actionList))
                {
                    this.Remove(component, actionList);
                    break;
                }
            }
        }

        public void Remove(IComponent comp)
        {
            if (comp == null)
            {
                throw new ArgumentNullException("comp");
            }
            if (this.designerActionLists.Contains(comp))
            {
                this.designerActionLists.Remove(comp);
                this.OnDesignerActionListsChanged(new DesignerActionListsChangedEventArgs(comp, DesignerActionListsChangedType.ActionListsRemoved, this.GetComponentActions(comp)));
            }
        }

        public void Remove(IComponent comp, DesignerActionList actionList)
        {
            if (comp == null)
            {
                throw new ArgumentNullException("comp");
            }
            if (actionList == null)
            {
                throw new ArgumentNullException("actionList");
            }
            if (this.designerActionLists.Contains(comp))
            {
                DesignerActionListCollection lists = (DesignerActionListCollection) this.designerActionLists[comp];
                if (lists.Contains(actionList))
                {
                    if (lists.Count == 1)
                    {
                        this.Remove(comp);
                    }
                    else
                    {
                        ArrayList list = new ArrayList(1);
                        foreach (DesignerActionList list2 in lists)
                        {
                            if (actionList.Equals(list2))
                            {
                                list.Add(list2);
                            }
                        }
                        foreach (DesignerActionList list3 in list)
                        {
                            lists.Remove(list3);
                        }
                        this.OnDesignerActionListsChanged(new DesignerActionListsChangedEventArgs(comp, DesignerActionListsChangedType.ActionListsRemoved, this.GetComponentActions(comp)));
                    }
                }
            }
        }
    }
}

