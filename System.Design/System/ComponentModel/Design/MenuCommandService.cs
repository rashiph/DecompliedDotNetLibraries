namespace System.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Design;
    using System.Diagnostics;

    public class MenuCommandService : IMenuCommandService, IDisposable
    {
        private EventHandler _commandChangedHandler;
        private Hashtable _commandGroups;
        private DesignerVerbCollection _currentVerbs;
        private ArrayList _globalVerbs;
        private ISelectionService _selectionService;
        private IServiceProvider _serviceProvider;
        private Type _verbSourceType;
        internal static TraceSwitch MENUSERVICE = new TraceSwitch("MENUSERVICE", "MenuCommandService: Track menu command routing");

        public event MenuCommandsChangedEventHandler MenuCommandsChanged;

        public MenuCommandService(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
            this._commandGroups = new Hashtable();
            this._commandChangedHandler = new EventHandler(this.OnCommandChanged);
            TypeDescriptor.Refreshed += new RefreshEventHandler(this.OnTypeRefreshed);
        }

        public virtual void AddCommand(MenuCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            if (this.FindCommand(command.CommandID) != null)
            {
                throw new ArgumentException(System.Design.SR.GetString("MenuCommandService_DuplicateCommand", new object[] { command.CommandID.ToString() }));
            }
            ArrayList list = this._commandGroups[command.CommandID.Guid] as ArrayList;
            if (list == null)
            {
                list = new ArrayList();
                list.Add(command);
                this._commandGroups.Add(command.CommandID.Guid, list);
            }
            else
            {
                list.Add(command);
            }
            command.CommandChanged += this._commandChangedHandler;
            this.OnCommandsChanged(new MenuCommandsChangedEventArgs(MenuCommandsChangedType.CommandAdded, command));
        }

        public virtual void AddVerb(DesignerVerb verb)
        {
            if (verb == null)
            {
                throw new ArgumentNullException("verb");
            }
            if (this._globalVerbs == null)
            {
                this._globalVerbs = new ArrayList();
            }
            this._globalVerbs.Add(verb);
            this.OnCommandsChanged(new MenuCommandsChangedEventArgs(MenuCommandsChangedType.CommandAdded, verb));
            this.EnsureVerbs();
            if (!this.Verbs.Contains(verb))
            {
                this.Verbs.Add(verb);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this._selectionService != null)
                {
                    this._selectionService.SelectionChanging -= new EventHandler(this.OnSelectionChanging);
                    this._selectionService = null;
                }
                if (this._serviceProvider != null)
                {
                    this._serviceProvider = null;
                    TypeDescriptor.Refreshed -= new RefreshEventHandler(this.OnTypeRefreshed);
                }
                IDictionaryEnumerator enumerator = this._commandGroups.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    ArrayList list = (ArrayList) enumerator.Value;
                    foreach (MenuCommand command in list)
                    {
                        command.CommandChanged -= this._commandChangedHandler;
                    }
                    list.Clear();
                }
            }
        }

        protected void EnsureVerbs()
        {
            bool flag = false;
            if ((this._currentVerbs == null) && (this._serviceProvider != null))
            {
                Hashtable hashtable = null;
                if (this._selectionService == null)
                {
                    this._selectionService = this.GetService(typeof(ISelectionService)) as ISelectionService;
                    if (this._selectionService != null)
                    {
                        this._selectionService.SelectionChanging += new EventHandler(this.OnSelectionChanging);
                    }
                }
                int capacity = 0;
                DesignerVerbCollection verbs = null;
                DesignerVerbCollection verbs2 = new DesignerVerbCollection();
                IDesignerHost host = this.GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (((this._selectionService != null) && (host != null)) && (this._selectionService.SelectionCount == 1))
                {
                    object primarySelection = this._selectionService.PrimarySelection;
                    if ((primarySelection is IComponent) && !TypeDescriptor.GetAttributes(primarySelection).Contains(InheritanceAttribute.InheritedReadOnly))
                    {
                        flag = primarySelection == host.RootComponent;
                        IDesigner designer = host.GetDesigner((IComponent) primarySelection);
                        if (designer != null)
                        {
                            verbs = designer.Verbs;
                            if (verbs != null)
                            {
                                capacity += verbs.Count;
                                this._verbSourceType = primarySelection.GetType();
                            }
                            else
                            {
                                this._verbSourceType = null;
                            }
                        }
                        DesignerActionService service = this.GetService(typeof(DesignerActionService)) as DesignerActionService;
                        if (service != null)
                        {
                            DesignerActionListCollection componentActions = service.GetComponentActions(primarySelection as IComponent);
                            if (componentActions != null)
                            {
                                foreach (DesignerActionList list2 in componentActions)
                                {
                                    DesignerActionItemCollection sortedActionItems = list2.GetSortedActionItems();
                                    if (sortedActionItems != null)
                                    {
                                        for (int j = 0; j < sortedActionItems.Count; j++)
                                        {
                                            DesignerActionMethodItem item = sortedActionItems[j] as DesignerActionMethodItem;
                                            if ((item != null) && item.IncludeAsDesignerVerb)
                                            {
                                                EventHandler handler = new EventHandler(item.Invoke);
                                                DesignerVerb verb = new DesignerVerb(item.DisplayName, handler);
                                                verbs2.Add(verb);
                                                capacity++;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (flag && (this._globalVerbs == null))
                {
                    flag = false;
                }
                if (flag)
                {
                    capacity += this._globalVerbs.Count;
                }
                hashtable = new Hashtable(capacity, StringComparer.OrdinalIgnoreCase);
                ArrayList list = new ArrayList(capacity);
                if (flag)
                {
                    for (int k = 0; k < this._globalVerbs.Count; k++)
                    {
                        string text = ((DesignerVerb) this._globalVerbs[k]).Text;
                        hashtable[text] = list.Add(this._globalVerbs[k]);
                    }
                }
                if (verbs2.Count > 0)
                {
                    for (int m = 0; m < verbs2.Count; m++)
                    {
                        string str2 = verbs2[m].Text;
                        hashtable[str2] = list.Add(verbs2[m]);
                    }
                }
                if ((verbs != null) && (verbs.Count > 0))
                {
                    for (int n = 0; n < verbs.Count; n++)
                    {
                        string str3 = verbs[n].Text;
                        hashtable[str3] = list.Add(verbs[n]);
                    }
                }
                DesignerVerb[] verbArray = new DesignerVerb[hashtable.Count];
                int index = 0;
                for (int i = 0; i < list.Count; i++)
                {
                    DesignerVerb verb2 = (DesignerVerb) list[i];
                    string str4 = verb2.Text;
                    if (((int) hashtable[str4]) == i)
                    {
                        verbArray[index] = verb2;
                        index++;
                    }
                }
                this._currentVerbs = new DesignerVerbCollection(verbArray);
            }
        }

        public MenuCommand FindCommand(CommandID commandID)
        {
            return this.FindCommand(commandID.Guid, commandID.ID);
        }

        protected MenuCommand FindCommand(Guid guid, int id)
        {
            ArrayList list = this._commandGroups[guid] as ArrayList;
            if (list != null)
            {
                foreach (MenuCommand command in list)
                {
                    if (command.CommandID.ID == id)
                    {
                        return command;
                    }
                }
            }
            this.EnsureVerbs();
            if (this._currentVerbs != null)
            {
                int iD = StandardCommands.VerbFirst.ID;
                foreach (DesignerVerb verb in this._currentVerbs)
                {
                    CommandID commandID = verb.CommandID;
                    if ((commandID.ID == id) && commandID.Guid.Equals(guid))
                    {
                        return verb;
                    }
                    if ((iD == id) && commandID.Guid.Equals(guid))
                    {
                        return verb;
                    }
                    if (commandID.Equals(StandardCommands.VerbFirst))
                    {
                        iD++;
                    }
                }
            }
            return null;
        }

        protected ICollection GetCommandList(Guid guid)
        {
            return (this._commandGroups[guid] as ArrayList);
        }

        protected object GetService(Type serviceType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }
            if (this._serviceProvider != null)
            {
                return this._serviceProvider.GetService(serviceType);
            }
            return null;
        }

        public virtual bool GlobalInvoke(CommandID commandID)
        {
            MenuCommand command = this.FindCommand(commandID);
            if (command != null)
            {
                command.Invoke();
                return true;
            }
            return false;
        }

        public virtual bool GlobalInvoke(CommandID commandId, object arg)
        {
            MenuCommand command = this.FindCommand(commandId);
            if (command != null)
            {
                command.Invoke(arg);
                return true;
            }
            return false;
        }

        private void OnCommandChanged(object sender, EventArgs e)
        {
            this.OnCommandsChanged(new MenuCommandsChangedEventArgs(MenuCommandsChangedType.CommandChanged, (MenuCommand) sender));
        }

        protected virtual void OnCommandsChanged(MenuCommandsChangedEventArgs e)
        {
            if (this._commandsChangedHandler != null)
            {
                this._commandsChangedHandler(this, e);
            }
        }

        private void OnSelectionChanging(object sender, EventArgs e)
        {
            if (this._currentVerbs != null)
            {
                this._currentVerbs = null;
                this.OnCommandsChanged(new MenuCommandsChangedEventArgs(MenuCommandsChangedType.CommandChanged, null));
            }
        }

        private void OnTypeRefreshed(RefreshEventArgs e)
        {
            if ((this._verbSourceType != null) && this._verbSourceType.IsAssignableFrom(e.TypeChanged))
            {
                this._currentVerbs = null;
            }
        }

        public virtual void RemoveCommand(MenuCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            ArrayList list = this._commandGroups[command.CommandID.Guid] as ArrayList;
            if (list != null)
            {
                int index = list.IndexOf(command);
                if (-1 != index)
                {
                    list.RemoveAt(index);
                    if (list.Count == 0)
                    {
                        this._commandGroups.Remove(command.CommandID.Guid);
                    }
                    command.CommandChanged -= this._commandChangedHandler;
                    this.OnCommandsChanged(new MenuCommandsChangedEventArgs(MenuCommandsChangedType.CommandRemoved, command));
                }
            }
        }

        public virtual void RemoveVerb(DesignerVerb verb)
        {
            if (verb == null)
            {
                throw new ArgumentNullException("verb");
            }
            if (this._globalVerbs != null)
            {
                int index = this._globalVerbs.IndexOf(verb);
                if (index != -1)
                {
                    this._globalVerbs.RemoveAt(index);
                    this.EnsureVerbs();
                    if (this.Verbs.Contains(verb))
                    {
                        this.Verbs.Remove(verb);
                    }
                    this.OnCommandsChanged(new MenuCommandsChangedEventArgs(MenuCommandsChangedType.CommandRemoved, verb));
                }
            }
        }

        public virtual void ShowContextMenu(CommandID menuID, int x, int y)
        {
        }

        public virtual DesignerVerbCollection Verbs
        {
            get
            {
                this.EnsureVerbs();
                return this._currentVerbs;
            }
        }
    }
}

