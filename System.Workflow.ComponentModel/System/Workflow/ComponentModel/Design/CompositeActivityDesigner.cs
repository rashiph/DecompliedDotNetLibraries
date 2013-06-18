namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Windows.Forms;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Serialization;

    [DesignerSerializer(typeof(CompositeActivityDesignerLayoutSerializer), typeof(WorkflowMarkupSerializer)), SRCategory("CompositeActivityDesigners", "System.Workflow.ComponentModel.Design.DesignerResources"), ActivityDesignerTheme(typeof(CompositeDesignerTheme))]
    public abstract class CompositeActivityDesigner : ActivityDesigner
    {
        private CompositeDesignerAccessibleObject accessibilityObject;
        private Size actualTextSize = Size.Empty;
        private const string CF_DESIGNER = "CF_WINOEDESIGNERCOMPONENTS";
        private const string CF_DESIGNERSTATE = "CF_WINOEDESIGNERCOMPONENTSSTATE";
        private List<ActivityDesigner> containedActivityDesigners;
        private bool expanded = true;
        private const int MaximumCharsPerLine = 8;
        private const int MaximumTextLines = 1;

        protected CompositeActivityDesigner()
        {
        }

        public virtual bool CanInsertActivities(System.Workflow.ComponentModel.Design.HitTestInfo insertLocation, ReadOnlyCollection<Activity> activitiesToInsert)
        {
            if (insertLocation == null)
            {
                throw new ArgumentNullException("insertLocation");
            }
            if (activitiesToInsert == null)
            {
                throw new ArgumentNullException("activitiesToInsert");
            }
            if (!(base.Activity is CompositeActivity))
            {
                return false;
            }
            if (!this.IsEditable)
            {
                return false;
            }
            IExtendedUIService2 service = base.GetService(typeof(IExtendedUIService2)) as IExtendedUIService2;
            foreach (Activity activity2 in activitiesToInsert)
            {
                if (activity2 == null)
                {
                    throw new ArgumentException("activitiesToInsert", SR.GetString("Error_CollectionHasNullEntry"));
                }
                if ((service != null) && !service.IsSupportedType(activity2.GetType()))
                {
                    return false;
                }
                if ((activity2 is CompositeActivity) && Helpers.IsAlternateFlowActivity(activity2))
                {
                    return false;
                }
                ActivityDesigner designer = null;
                if (activity2.Site != null)
                {
                    IDesignerHost host = activity2.Site.GetService(typeof(IDesignerHost)) as IDesignerHost;
                    designer = (host != null) ? (host.GetDesigner(activity2) as ActivityDesigner) : null;
                }
                else if (activity2.UserData.Contains(typeof(ActivityDesigner)))
                {
                    designer = activity2.UserData[typeof(ActivityDesigner)] as ActivityDesigner;
                }
                else
                {
                    designer = ActivityDesigner.CreateDesigner(base.Activity.Site, activity2);
                    activity2.UserData[typeof(ActivityDesigner)] = designer;
                }
                if (designer == null)
                {
                    return false;
                }
                if (!designer.CanBeParentedTo(this))
                {
                    return false;
                }
            }
            return true;
        }

        public virtual bool CanMoveActivities(System.Workflow.ComponentModel.Design.HitTestInfo moveLocation, ReadOnlyCollection<Activity> activitiesToMove)
        {
            if (moveLocation == null)
            {
                throw new ArgumentNullException("moveLocation");
            }
            if (activitiesToMove == null)
            {
                throw new ArgumentNullException("activitiesToMove");
            }
            if (!this.IsEditable)
            {
                return false;
            }
            foreach (Activity activity in activitiesToMove)
            {
                ActivityDesigner designer = ActivityDesigner.GetDesigner(activity);
                if ((designer == null) || designer.IsLocked)
                {
                    return false;
                }
            }
            return true;
        }

        public virtual bool CanRemoveActivities(ReadOnlyCollection<Activity> activitiesToRemove)
        {
            if (activitiesToRemove == null)
            {
                throw new ArgumentNullException("activitiesToRemove");
            }
            if (!this.IsEditable)
            {
                return false;
            }
            foreach (Activity activity in activitiesToRemove)
            {
                ActivityDesigner designer = ActivityDesigner.GetDesigner(activity);
                if ((designer == null) || designer.IsLocked)
                {
                    return false;
                }
            }
            return true;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static Activity[] DeserializeActivitiesFromDataObject(IServiceProvider serviceProvider, IDataObject dataObj)
        {
            return DeserializeActivitiesFromDataObject(serviceProvider, dataObj, false);
        }

        internal static Activity[] DeserializeActivitiesFromDataObject(IServiceProvider serviceProvider, IDataObject dataObj, bool addAssemblyReference)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }
            if (dataObj != null)
            {
                IDesignerHost host = (IDesignerHost) serviceProvider.GetService(typeof(IDesignerHost));
                if (host == null)
                {
                    throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(IDesignerHost).Name }));
                }
                object data = dataObj.GetData("CF_WINOEDESIGNERCOMPONENTS");
                ICollection activities = null;
                if (data is Stream)
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    ((Stream) data).Seek(0L, SeekOrigin.Begin);
                    object obj3 = formatter.Deserialize((Stream) data);
                    if (obj3 is SerializationStore)
                    {
                        ComponentSerializationService service = serviceProvider.GetService(typeof(ComponentSerializationService)) as ComponentSerializationService;
                        if (service == null)
                        {
                            throw new Exception(SR.GetString("General_MissingService", new object[] { typeof(ComponentSerializationService).Name }));
                        }
                        activities = service.Deserialize((SerializationStore) obj3);
                    }
                }
                else
                {
                    IToolboxService service2 = (IToolboxService) serviceProvider.GetService(typeof(IToolboxService));
                    if ((service2 != null) && service2.IsSupported(dataObj, host))
                    {
                        ToolboxItem toolBoxItem = service2.DeserializeToolboxItem(dataObj, host);
                        if (toolBoxItem != null)
                        {
                            activities = GetActivitiesFromToolboxItem(serviceProvider, addAssemblyReference, host, activities, toolBoxItem);
                        }
                    }
                }
                if ((activities != null) && Helpers.AreAllActivities(activities))
                {
                    return (Activity[]) new ArrayList(activities).ToArray(typeof(Activity));
                }
            }
            return new Activity[0];
        }

        internal static Activity[] DeserializeActivitiesFromToolboxItem(IServiceProvider serviceProvider, ToolboxItem toolboxItem, bool addAssemblyReference)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }
            IDesignerHost service = (IDesignerHost) serviceProvider.GetService(typeof(IDesignerHost));
            if (service == null)
            {
                throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(IDesignerHost).Name }));
            }
            ICollection activities = null;
            if (toolboxItem != null)
            {
                activities = GetActivitiesFromToolboxItem(serviceProvider, addAssemblyReference, service, activities, toolboxItem);
            }
            if ((activities != null) && Helpers.AreAllActivities(activities))
            {
                return (Activity[]) new ArrayList(activities).ToArray(typeof(Activity));
            }
            return new Activity[0];
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CompositeActivity activity = base.Activity as CompositeActivity;
                if (activity != null)
                {
                    activity.Activities.ListChanging -= new EventHandler<ActivityCollectionChangeEventArgs>(this.OnActivityListChanging);
                    activity.Activities.ListChanged -= new EventHandler<ActivityCollectionChangeEventArgs>(this.OnActivityListChanged);
                }
                if (base.IsRootDesigner)
                {
                    IComponentChangeService service = base.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                    if (service != null)
                    {
                        service.ComponentAdded -= new ComponentEventHandler(this.OnComponentAdded);
                        service.ComponentChanged -= new ComponentChangedEventHandler(this.OnComponentChanged);
                    }
                }
            }
            base.Dispose(disposing);
        }

        public virtual void EnsureVisibleContainedDesigner(ActivityDesigner containedDesigner)
        {
            if (containedDesigner == null)
            {
                throw new ArgumentNullException("containedDesigner");
            }
            this.Expanded = true;
        }

        private static ICollection GetActivitiesFromToolboxItem(IServiceProvider serviceProvider, bool addAssemblyReference, IDesignerHost designerHost, ICollection activities, ToolboxItem toolBoxItem)
        {
            if (addAssemblyReference && (toolBoxItem.AssemblyName != null))
            {
                ITypeResolutionService service = serviceProvider.GetService(typeof(ITypeResolutionService)) as ITypeResolutionService;
                if (service != null)
                {
                    service.ReferenceAssembly(toolBoxItem.AssemblyName);
                }
            }
            ActivityToolboxItem item = toolBoxItem as ActivityToolboxItem;
            if (addAssemblyReference && (item != null))
            {
                activities = item.CreateComponentsWithUI(designerHost);
                return activities;
            }
            activities = toolBoxItem.CreateComponents(designerHost);
            return activities;
        }

        public static ActivityDesigner[] GetIntersectingDesigners(ActivityDesigner topLevelDesigner, Rectangle rectangle)
        {
            if (topLevelDesigner == null)
            {
                throw new ArgumentNullException("topLevelDesigner");
            }
            List<ActivityDesigner> list = new List<ActivityDesigner>();
            if (rectangle.IntersectsWith(topLevelDesigner.Bounds))
            {
                if (!topLevelDesigner.Bounds.Contains(rectangle))
                {
                    list.Add(topLevelDesigner);
                }
                if (topLevelDesigner is CompositeActivityDesigner)
                {
                    Queue queue = new Queue();
                    queue.Enqueue(topLevelDesigner);
                    while (queue.Count > 0)
                    {
                        CompositeActivityDesigner designer = queue.Dequeue() as CompositeActivityDesigner;
                        if (designer != null)
                        {
                            bool flag = false;
                            foreach (ActivityDesigner designer2 in designer.ContainedDesigners)
                            {
                                if (designer2.IsVisible && rectangle.IntersectsWith(designer2.Bounds))
                                {
                                    flag = true;
                                    if (!designer2.Bounds.Contains(rectangle))
                                    {
                                        list.Add(designer2);
                                    }
                                    if (designer2 is CompositeActivityDesigner)
                                    {
                                        queue.Enqueue(designer2);
                                    }
                                }
                                else if (!(designer is FreeformActivityDesigner) && flag)
                                {
                                    continue;
                                }
                            }
                        }
                    }
                }
            }
            return list.ToArray();
        }

        public virtual object GetNextSelectableObject(object current, DesignerNavigationDirection direction)
        {
            return null;
        }

        public override System.Workflow.ComponentModel.Design.HitTestInfo HitTest(Point point)
        {
            System.Workflow.ComponentModel.Design.HitTestInfo nowhere = System.Workflow.ComponentModel.Design.HitTestInfo.Nowhere;
            if (this.ExpandButtonRectangle.Contains(point))
            {
                nowhere = new System.Workflow.ComponentModel.Design.HitTestInfo(this, HitTestLocations.ActionArea | HitTestLocations.Designer);
            }
            else if (this.Expanded && base.Bounds.Contains(point))
            {
                ReadOnlyCollection<ActivityDesigner> containedDesigners = this.ContainedDesigners;
                for (int i = containedDesigners.Count - 1; i >= 0; i--)
                {
                    ActivityDesigner designer = containedDesigners[i];
                    if ((designer != null) && designer.IsVisible)
                    {
                        nowhere = designer.HitTest(point);
                        if (nowhere.HitLocation != HitTestLocations.None)
                        {
                            break;
                        }
                    }
                }
            }
            if (nowhere == System.Workflow.ComponentModel.Design.HitTestInfo.Nowhere)
            {
                nowhere = base.HitTest(point);
            }
            if ((nowhere.AssociatedDesigner != null) && (nowhere.AssociatedDesigner.DrawingState != ActivityDesigner.DrawingStates.Valid))
            {
                nowhere = new System.Workflow.ComponentModel.Design.HitTestInfo(nowhere.AssociatedDesigner, HitTestLocations.ActionArea | HitTestLocations.Designer);
            }
            return nowhere;
        }

        protected override void Initialize(Activity activity)
        {
            base.Initialize(activity);
            CompositeActivity activity2 = activity as CompositeActivity;
            if (activity2 != null)
            {
                activity2.Activities.ListChanging += new EventHandler<ActivityCollectionChangeEventArgs>(this.OnActivityListChanging);
                activity2.Activities.ListChanged += new EventHandler<ActivityCollectionChangeEventArgs>(this.OnActivityListChanged);
            }
            if (base.IsRootDesigner)
            {
                IComponentChangeService service = base.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                if (service != null)
                {
                    service.ComponentAdded += new ComponentEventHandler(this.OnComponentAdded);
                    service.ComponentChanged += new ComponentChangedEventHandler(this.OnComponentChanged);
                }
            }
        }

        public virtual void InsertActivities(System.Workflow.ComponentModel.Design.HitTestInfo insertLocation, ReadOnlyCollection<Activity> activitiesToInsert)
        {
            WalkerEventHandler handler = null;
            if (insertLocation == null)
            {
                throw new ArgumentNullException("insertLocation");
            }
            if (activitiesToInsert == null)
            {
                throw new ArgumentNullException("activitiesToInsert");
            }
            CompositeActivity parentActivity = base.Activity as CompositeActivity;
            if (parentActivity == null)
            {
                throw new Exception(SR.GetString("Error_DragDropInvalid"));
            }
            int num = insertLocation.MapToIndex();
            IIdentifierCreationService service = base.GetService(typeof(IIdentifierCreationService)) as IIdentifierCreationService;
            if (service == null)
            {
                throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(IIdentifierCreationService).FullName }));
            }
            service.EnsureUniqueIdentifiers(parentActivity, activitiesToInsert);
            foreach (Activity activity2 in activitiesToInsert)
            {
                if (activity2 == null)
                {
                    throw new ArgumentException("activitiesToInsert", SR.GetString("Error_CollectionHasNullEntry"));
                }
                if (activity2.Parent == null)
                {
                    parentActivity.Activities.Insert(num++, activity2);
                    WorkflowDesignerLoader.AddActivityToDesigner(base.Activity.Site, activity2);
                }
            }
            foreach (Activity activity3 in activitiesToInsert)
            {
                Walker walker = new Walker();
                if (handler == null)
                {
                    handler = (w, walkerEventArgs) => ExtenderHelpers.FilterDependencyProperties(base.Activity.Site, walkerEventArgs.CurrentActivity);
                }
                walker.FoundActivity += handler;
                walker.Walk(activity3);
            }
        }

        public static void InsertActivities(CompositeActivityDesigner compositeActivityDesigner, System.Workflow.ComponentModel.Design.HitTestInfo insertLocation, ReadOnlyCollection<Activity> activitiesToInsert, string undoTransactionDescription)
        {
            if (compositeActivityDesigner == null)
            {
                throw new ArgumentNullException("compositeActivityDesigner");
            }
            if (((compositeActivityDesigner.Activity == null) || (compositeActivityDesigner.Activity.Site == null)) || !(compositeActivityDesigner.Activity is CompositeActivity))
            {
                throw new ArgumentException("compositeActivityDesigner");
            }
            if (insertLocation == null)
            {
                throw new ArgumentNullException("insertLocation");
            }
            if (activitiesToInsert == null)
            {
                throw new ArgumentNullException("activitiesToInsert");
            }
            IDesignerHost service = compositeActivityDesigner.Activity.Site.GetService(typeof(IDesignerHost)) as IDesignerHost;
            DesignerTransaction transaction = null;
            if ((service != null) && !string.IsNullOrEmpty(undoTransactionDescription))
            {
                transaction = service.CreateTransaction(undoTransactionDescription);
            }
            bool flag = false;
            try
            {
                foreach (Activity activity in activitiesToInsert)
                {
                    if (activity == null)
                    {
                        throw new ArgumentException("activitiesToInsert", SR.GetString("Error_CollectionHasNullEntry"));
                    }
                    flag = activity.Site != null;
                    break;
                }
                if (flag)
                {
                    compositeActivityDesigner.MoveActivities(insertLocation, activitiesToInsert);
                }
                else
                {
                    compositeActivityDesigner.InsertActivities(insertLocation, activitiesToInsert);
                }
                if (transaction != null)
                {
                    transaction.Commit();
                }
            }
            catch (Exception exception)
            {
                if (transaction != null)
                {
                    transaction.Cancel();
                }
                throw exception;
            }
            if (!flag)
            {
                ArrayList list = new ArrayList();
                foreach (Activity activity2 in activitiesToInsert)
                {
                    list.Add(activity2);
                    if (activity2 is CompositeActivity)
                    {
                        list.AddRange(Helpers.GetNestedActivities((CompositeActivity) activity2));
                    }
                }
            }
        }

        public virtual bool IsContainedDesignerVisible(ActivityDesigner containedDesigner)
        {
            if (containedDesigner == null)
            {
                throw new ArgumentNullException("containedDesigner");
            }
            return true;
        }

        protected override void LoadViewState(BinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            this.Expanded = reader.ReadBoolean();
            base.LoadViewState(reader);
        }

        public virtual void MoveActivities(System.Workflow.ComponentModel.Design.HitTestInfo moveLocation, ReadOnlyCollection<Activity> activitiesToMove)
        {
            WalkerEventHandler handler = null;
            if (moveLocation == null)
            {
                throw new ArgumentNullException("moveLocation");
            }
            if (activitiesToMove == null)
            {
                throw new ArgumentNullException("activitiesToMove");
            }
            CompositeActivity parentActivity = base.Activity as CompositeActivity;
            if (parentActivity == null)
            {
                throw new Exception(SR.GetString("Error_DragDropInvalid"));
            }
            IIdentifierCreationService service = base.GetService(typeof(IIdentifierCreationService)) as IIdentifierCreationService;
            if (service == null)
            {
                throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(IIdentifierCreationService).FullName }));
            }
            if (!(base.GetService(typeof(IDesignerHost)) is IDesignerHost))
            {
                throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(IDesignerHost).FullName }));
            }
            int num = moveLocation.MapToIndex();
            foreach (Activity activity2 in activitiesToMove)
            {
                ActivityDesigner designer = ActivityDesigner.GetDesigner(activity2);
                if ((designer != null) && (designer.ParentDesigner == this))
                {
                    int num2 = parentActivity.Activities.IndexOf(activity2);
                    if (num > num2)
                    {
                        num--;
                    }
                }
                CompositeActivity parent = activity2.Parent;
                int index = parent.Activities.IndexOf(activity2);
                activity2.Parent.Activities.Remove(activity2);
                service.EnsureUniqueIdentifiers(parentActivity, new Activity[] { activity2 });
                DesignerHelpers.UpdateSiteName(activity2, "_activityonthemove_");
                CompositeActivity compositeActivity = activity2 as CompositeActivity;
                if (compositeActivity != null)
                {
                    int num4 = 1;
                    foreach (Activity activity5 in Helpers.GetNestedActivities(compositeActivity))
                    {
                        DesignerHelpers.UpdateSiteName(activity5, "_activityonthemove_" + num4.ToString(CultureInfo.InvariantCulture));
                        num4++;
                    }
                }
                try
                {
                    parentActivity.Activities.Insert(num++, activity2);
                }
                catch (Exception exception)
                {
                    parent.Activities.Insert(index, activity2);
                    throw exception;
                }
                DesignerHelpers.UpdateSiteName(activity2, activity2.Name);
                if (compositeActivity != null)
                {
                    foreach (Activity activity6 in Helpers.GetNestedActivities(compositeActivity))
                    {
                        DesignerHelpers.UpdateSiteName(activity6, activity6.Name);
                    }
                }
            }
            foreach (Activity activity7 in activitiesToMove)
            {
                Walker walker = new Walker();
                if (handler == null)
                {
                    handler = delegate (Walker w, WalkerEventArgs walkerEventArgs) {
                        ExtenderHelpers.FilterDependencyProperties(base.Activity.Site, walkerEventArgs.CurrentActivity);
                        TypeDescriptor.Refresh(walkerEventArgs.CurrentActivity);
                    };
                }
                walker.FoundActivity += handler;
                walker.Walk(activity7);
            }
        }

        public static void MoveDesigners(ActivityDesigner activityDesigner, bool moveBack)
        {
            if (activityDesigner == null)
            {
                throw new ArgumentNullException("activityDesigner");
            }
            Activity item = activityDesigner.Activity;
            if ((item != null) && (item.Parent != null))
            {
                CompositeActivity parent = item.Parent;
                if ((parent != null) && parent.Activities.Contains(item))
                {
                    int index = parent.Activities.IndexOf(item) + (moveBack ? -1 : 1);
                    if ((index >= 0) && (index < parent.Activities.Count))
                    {
                        IDesignerHost host = parent.Site.GetService(typeof(IDesignerHost)) as IDesignerHost;
                        if (host != null)
                        {
                            DesignerTransaction transaction = host.CreateTransaction(SR.GetString("MovingActivities"));
                            try
                            {
                                parent.Activities.Remove(item);
                                parent.Activities.Insert(index, item);
                                ISelectionService service = parent.Site.GetService(typeof(ISelectionService)) as ISelectionService;
                                if (service != null)
                                {
                                    service.SetSelectedComponents(new object[] { item });
                                }
                                if (transaction != null)
                                {
                                    transaction.Commit();
                                }
                            }
                            catch
                            {
                                if (transaction != null)
                                {
                                    transaction.Cancel();
                                }
                                throw;
                            }
                            CompositeActivityDesigner designer = ActivityDesigner.GetDesigner(parent) as CompositeActivityDesigner;
                            if (designer != null)
                            {
                                designer.PerformLayout();
                            }
                        }
                    }
                }
            }
        }

        private void OnActivityListChanged(object sender, ActivityCollectionChangeEventArgs e)
        {
            this.OnContainedActivitiesChanged(e);
        }

        private void OnActivityListChanging(object sender, ActivityCollectionChangeEventArgs e)
        {
            this.OnContainedActivitiesChanging(e);
        }

        private void OnComponentAdded(object sender, ComponentEventArgs e)
        {
            ActivityDesigner designer = ActivityDesigner.GetDesigner(e.Component as Activity);
            if (((base.Activity != e.Component) && (designer != null)) && designer.IsLocked)
            {
                DesignerHelpers.MakePropertiesReadOnly(e.Component.Site, designer.Activity);
            }
        }

        private void OnComponentChanged(object sender, ComponentChangedEventArgs e)
        {
            IReferenceService service = base.GetService(typeof(IReferenceService)) as IReferenceService;
            Activity activity = (service != null) ? (service.GetComponent(e.Component) as Activity) : (e.Component as Activity);
            if (activity != null)
            {
                ActivityDesigner designer = ActivityDesigner.GetDesigner(activity);
                if (designer != null)
                {
                    CompositeActivityDesigner parentDesigner = designer.ParentDesigner;
                    if (parentDesigner != null)
                    {
                        parentDesigner.OnContainedActivityChanged(new ActivityChangedEventArgs(activity, e.Member, e.OldValue, e.NewValue));
                    }
                }
            }
        }

        protected virtual void OnContainedActivitiesChanged(ActivityCollectionChangeEventArgs listChangeArgs)
        {
            if (listChangeArgs == null)
            {
                throw new ArgumentNullException("listChangeArgs");
            }
            foreach (ActivityDesigner designer in this.ContainedDesigners)
            {
                foreach (DesignerVerb verb in ((IDesigner) designer).Verbs)
                {
                    int oleStatus = verb.OleStatus;
                }
            }
            base.RefreshDesignerVerbs();
            this.containedActivityDesigners = null;
            base.PerformLayout();
        }

        protected virtual void OnContainedActivitiesChanging(ActivityCollectionChangeEventArgs listChangeArgs)
        {
            if (listChangeArgs == null)
            {
                throw new ArgumentNullException("listChangeArgs");
            }
        }

        protected virtual void OnContainedActivityChanged(ActivityChangedEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            ISelectionService service = base.GetService(typeof(ISelectionService)) as ISelectionService;
            object activity = (service != null) ? service.PrimarySelection : null;
            if (activity == null)
            {
                return;
            }
            object selectableObject = null;
            if ((e.KeyCode == Keys.Down) || ((e.KeyCode == Keys.Tab) && !e.Shift))
            {
                CompositeActivityDesigner designer = ActivityDesigner.GetDesigner(activity as Activity) as CompositeActivityDesigner;
                if (designer != null)
                {
                    selectableObject = designer.FirstSelectableObject;
                }
                if (selectableObject != null)
                {
                    goto Label_0188;
                }
                while (true)
                {
                    CompositeActivityDesigner parentDesigner = ActivityDesigner.GetParentDesigner(activity);
                    if (parentDesigner == null)
                    {
                        selectableObject = activity;
                        goto Label_0188;
                    }
                    selectableObject = parentDesigner.GetNextSelectableObject(activity, DesignerNavigationDirection.Down);
                    if (selectableObject != null)
                    {
                        goto Label_0188;
                    }
                    activity = parentDesigner.Activity;
                }
            }
            if ((e.KeyCode == Keys.Up) || ((e.KeyCode == Keys.Tab) && e.Shift))
            {
                CompositeActivityDesigner designer3 = ActivityDesigner.GetParentDesigner(activity);
                if (designer3 == null)
                {
                    CompositeActivityDesigner designer4 = ActivityDesigner.GetDesigner(activity as Activity) as CompositeActivityDesigner;
                    if (designer4 != null)
                    {
                        selectableObject = designer4.LastSelectableObject;
                    }
                }
                else
                {
                    selectableObject = designer3.GetNextSelectableObject(activity, DesignerNavigationDirection.Up);
                    if (selectableObject != null)
                    {
                        CompositeActivityDesigner designer5 = ActivityDesigner.GetDesigner(selectableObject as Activity) as CompositeActivityDesigner;
                        if (designer5 != null)
                        {
                            object lastSelectableObject = designer5.LastSelectableObject;
                            if (lastSelectableObject != null)
                            {
                                selectableObject = lastSelectableObject;
                            }
                        }
                    }
                    else
                    {
                        selectableObject = designer3.Activity;
                    }
                }
            }
            else
            {
                if (e.KeyCode == Keys.Left)
                {
                    while (true)
                    {
                        CompositeActivityDesigner designer6 = ActivityDesigner.GetParentDesigner(activity);
                        if (designer6 == null)
                        {
                            goto Label_0188;
                        }
                        selectableObject = designer6.GetNextSelectableObject(activity, DesignerNavigationDirection.Left);
                        if (selectableObject != null)
                        {
                            goto Label_0188;
                        }
                        activity = designer6.Activity;
                    }
                }
                if (e.KeyCode == Keys.Right)
                {
                    while (true)
                    {
                        CompositeActivityDesigner designer7 = ActivityDesigner.GetParentDesigner(activity);
                        if (designer7 == null)
                        {
                            break;
                        }
                        selectableObject = designer7.GetNextSelectableObject(activity, DesignerNavigationDirection.Right);
                        if (selectableObject != null)
                        {
                            break;
                        }
                        activity = designer7.Activity;
                    }
                }
            }
        Label_0188:
            if (selectableObject != null)
            {
                service.SetSelectedComponents(new object[] { selectableObject }, SelectionTypes.Replace);
                base.ParentView.EnsureVisible(selectableObject);
            }
        }

        protected override void OnLayoutPosition(ActivityDesignerLayoutEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            base.OnLayoutPosition(e);
            foreach (ActivityDesigner designer in this.ContainedDesigners)
            {
                try
                {
                    ((IWorkflowDesignerMessageSink) designer).OnLayoutPosition(e.Graphics);
                    designer.DrawingState &= ~ActivityDesigner.DrawingStates.InvalidPosition;
                }
                catch
                {
                    designer.DrawingState |= ActivityDesigner.DrawingStates.InvalidPosition;
                }
            }
        }

        protected override Size OnLayoutSize(ActivityDesignerLayoutEventArgs e)
        {
            Size size = base.OnLayoutSize(e);
            foreach (ActivityDesigner designer in this.ContainedDesigners)
            {
                try
                {
                    ((IWorkflowDesignerMessageSink) designer).OnLayoutSize(e.Graphics);
                    designer.DrawingState &= ~ActivityDesigner.DrawingStates.InvalidSize;
                }
                catch
                {
                    designer.Size = designer.DesignerTheme.Size;
                    designer.DrawingState |= ActivityDesigner.DrawingStates.InvalidSize;
                }
            }
            if (!string.IsNullOrEmpty(this.Text))
            {
                this.actualTextSize = ActivityDesignerPaint.MeasureString(e.Graphics, e.DesignerTheme.BoldFont, this.Text, StringAlignment.Center, Size.Empty);
            }
            else
            {
                this.actualTextSize = Size.Empty;
            }
            if (this.Expanded)
            {
                size.Height = this.TitleHeight;
                return size;
            }
            size.Height = this.TitleHeight + WorkflowTheme.CurrentTheme.AmbientTheme.Margin.Height;
            return size;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (this.ExpandButtonRectangle.Contains(new Point(e.X, e.Y)))
            {
                IMenuCommandService service = base.GetService(typeof(IMenuCommandService)) as IMenuCommandService;
                if (service != null)
                {
                    service.GlobalInvoke(this.Expanded ? WorkflowMenuCommands.Collapse : WorkflowMenuCommands.Expand);
                }
                else
                {
                    this.Expanded = !this.Expanded;
                }
            }
        }

        protected override void OnPaint(ActivityDesignerPaintEventArgs e)
        {
            base.OnPaint(e);
            CompositeDesignerTheme designerTheme = e.DesignerTheme as CompositeDesignerTheme;
            if (designerTheme != null)
            {
                if (this.CanExpandCollapse)
                {
                    Rectangle expandButtonRectangle = this.ExpandButtonRectangle;
                    if (!expandButtonRectangle.Size.IsEmpty)
                    {
                        ActivityDesignerPaint.DrawExpandButton(e.Graphics, expandButtonRectangle, !this.Expanded, designerTheme);
                    }
                }
                if (this.Expanded)
                {
                    this.PaintContainedDesigners(e);
                }
            }
        }

        internal virtual void OnPaintContainedDesigners(ActivityDesignerPaintEventArgs e)
        {
            foreach (ActivityDesigner designer in this.ContainedDesigners)
            {
                using (PaintEventArgs args = new PaintEventArgs(e.Graphics, e.ViewPort))
                {
                    ((IWorkflowDesignerMessageSink) designer).OnPaint(args, e.ViewPort);
                }
            }
        }

        protected override void OnThemeChange(ActivityDesignerTheme designerTheme)
        {
            base.OnThemeChange(designerTheme);
            CompositeActivity activity = base.Activity as CompositeActivity;
            if (activity != null)
            {
                foreach (Activity activity2 in activity.Activities)
                {
                    IWorkflowDesignerMessageSink designer = ActivityDesigner.GetDesigner(activity2);
                    if (designer != null)
                    {
                        designer.OnThemeChange();
                    }
                }
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected void PaintContainedDesigners(ActivityDesignerPaintEventArgs e)
        {
            this.OnPaintContainedDesigners(e);
        }

        public virtual void RemoveActivities(ReadOnlyCollection<Activity> activitiesToRemove)
        {
            if (activitiesToRemove == null)
            {
                throw new ArgumentNullException("activitiesToRemove");
            }
            CompositeActivity activity = base.Activity as CompositeActivity;
            if (activity == null)
            {
                throw new Exception(SR.GetString("Error_DragDropInvalid"));
            }
            foreach (Activity activity2 in activitiesToRemove)
            {
                activity.Activities.Remove(activity2);
                activity2.SetParent(null);
                if (activity2 is CompositeActivity)
                {
                    foreach (Activity activity3 in Helpers.GetNestedActivities(activity2 as CompositeActivity))
                    {
                        activity3.SetParent(null);
                    }
                }
                WorkflowDesignerLoader.RemoveActivityFromDesigner(base.Activity.Site, activity2);
            }
        }

        public static void RemoveActivities(IServiceProvider serviceProvider, ReadOnlyCollection<Activity> activitiesToRemove, string transactionDescription)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException();
            }
            if (activitiesToRemove == null)
            {
                throw new ArgumentNullException("activitiesToRemove");
            }
            Activity nextSelectableActivity = null;
            IDesignerHost host = serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
            DesignerTransaction transaction = null;
            if ((host != null) && !string.IsNullOrEmpty(transactionDescription))
            {
                transaction = host.CreateTransaction(transactionDescription);
            }
            try
            {
                foreach (Activity activity2 in activitiesToRemove)
                {
                    ActivityDesigner designer = ActivityDesigner.GetDesigner(activity2);
                    if (designer != null)
                    {
                        CompositeActivityDesigner parentDesigner = designer.ParentDesigner;
                        if (parentDesigner != null)
                        {
                            nextSelectableActivity = DesignerHelpers.GetNextSelectableActivity(activity2);
                            parentDesigner.RemoveActivities(new List<Activity>(new Activity[] { activity2 }).AsReadOnly());
                        }
                    }
                }
                if (transaction != null)
                {
                    transaction.Commit();
                }
            }
            catch
            {
                if (transaction != null)
                {
                    transaction.Cancel();
                }
                throw;
            }
            if ((nextSelectableActivity != null) && (nextSelectableActivity.Site != null))
            {
                ISelectionService service = nextSelectableActivity.Site.GetService(typeof(ISelectionService)) as ISelectionService;
                if (service != null)
                {
                    service.SetSelectedComponents(new Activity[] { nextSelectableActivity }, SelectionTypes.Replace);
                }
            }
        }

        protected override void SaveViewState(BinaryWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            writer.Write(this.Expanded);
            base.SaveViewState(writer);
        }

        public static IDataObject SerializeActivitiesToDataObject(IServiceProvider serviceProvider, Activity[] activities)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }
            if (activities == null)
            {
                throw new ArgumentNullException("activities");
            }
            ComponentSerializationService service = (ComponentSerializationService) serviceProvider.GetService(typeof(ComponentSerializationService));
            if (service == null)
            {
                throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(ComponentSerializationService).Name }));
            }
            SerializationStore store = service.CreateStore();
            using (store)
            {
                foreach (Activity activity in activities)
                {
                    service.Serialize(store, activity);
                }
            }
            Stream serializationStream = new MemoryStream();
            new BinaryFormatter().Serialize(serializationStream, store);
            serializationStream.Seek(0L, SeekOrigin.Begin);
            DataObject obj2 = new DataObject("CF_WINOEDESIGNERCOMPONENTS", serializationStream);
            obj2.SetData("CF_WINOEDESIGNERCOMPONENTSSTATE", Helpers.SerializeDesignersToStream(activities));
            return obj2;
        }

        public override AccessibleObject AccessibilityObject
        {
            get
            {
                if (this.accessibilityObject == null)
                {
                    this.accessibilityObject = new CompositeDesignerAccessibleObject(this);
                }
                return this.accessibilityObject;
            }
        }

        public virtual bool CanExpandCollapse
        {
            get
            {
                return !base.IsRootDesigner;
            }
        }

        public virtual ReadOnlyCollection<ActivityDesigner> ContainedDesigners
        {
            get
            {
                List<ActivityDesigner> containedActivityDesigners = new List<ActivityDesigner>();
                CompositeActivity activity = base.Activity as CompositeActivity;
                if (activity != null)
                {
                    if (this.containedActivityDesigners == null)
                    {
                        bool flag = true;
                        foreach (Activity activity2 in activity.Activities)
                        {
                            ActivityDesigner item = ActivityDesigner.GetDesigner(activity2);
                            if (item != null)
                            {
                                containedActivityDesigners.Add(item);
                            }
                            else
                            {
                                flag = false;
                            }
                        }
                        if (flag)
                        {
                            this.containedActivityDesigners = containedActivityDesigners;
                        }
                    }
                    else
                    {
                        containedActivityDesigners = this.containedActivityDesigners;
                    }
                }
                return containedActivityDesigners.AsReadOnly();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), EditorBrowsable(EditorBrowsableState.Never)]
        internal List<ActivityDesigner> Designers
        {
            get
            {
                List<ActivityDesigner> list = new List<ActivityDesigner>();
                CompositeActivity activity = base.Activity as CompositeActivity;
                IDesignerHost service = base.GetService(typeof(IDesignerHost)) as IDesignerHost;
                if ((service != null) && (activity != null))
                {
                    foreach (Activity activity2 in activity.Activities)
                    {
                        ActivityDesigner item = service.GetDesigner(activity2) as ActivityDesigner;
                        if (item != null)
                        {
                            list.Add(item);
                        }
                    }
                }
                return list;
            }
        }

        protected virtual Rectangle ExpandButtonRectangle
        {
            get
            {
                Rectangle rectangle2;
                if (!this.CanExpandCollapse)
                {
                    return Rectangle.Empty;
                }
                CompositeDesignerTheme designerTheme = base.DesignerTheme as CompositeDesignerTheme;
                if (designerTheme == null)
                {
                    return Rectangle.Empty;
                }
                Size size = this.TextRectangle.Size;
                Size size2 = (this.Image != null) ? designerTheme.ImageSize : Size.Empty;
                Rectangle bounds = base.Bounds;
                Size size3 = !size.IsEmpty ? size : size2;
                rectangle2 = new Rectangle(bounds.Location, designerTheme.ExpandButtonSize) {
                    X = rectangle2.X + ((bounds.Width - (((3 * designerTheme.ExpandButtonSize.Width) / 2) + size3.Width)) / 2),
                    Y = rectangle2.Y + (2 * WorkflowTheme.CurrentTheme.AmbientTheme.Margin.Height)
                };
                if (size3.Height > rectangle2.Height)
                {
                    rectangle2.Y += (size3.Height - rectangle2.Height) / 2;
                }
                return rectangle2;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual bool Expanded
        {
            get
            {
                if (!this.CanExpandCollapse && !this.expanded)
                {
                    this.Expanded = true;
                }
                return this.expanded;
            }
            set
            {
                if ((this.expanded != value) && (this.CanExpandCollapse || value))
                {
                    this.expanded = value;
                    base.PerformLayout();
                }
            }
        }

        public virtual object FirstSelectableObject
        {
            get
            {
                return null;
            }
        }

        protected internal override ActivityDesignerGlyphCollection Glyphs
        {
            get
            {
                ActivityDesignerGlyphCollection glyphs = new ActivityDesignerGlyphCollection();
                glyphs.AddRange(base.Glyphs);
                CompositeDesignerTheme designerTheme = base.DesignerTheme as CompositeDesignerTheme;
                if ((designerTheme != null) && designerTheme.ShowDropShadow)
                {
                    glyphs.Add(ShadowGlyph.Default);
                }
                return glyphs;
            }
        }

        protected override Rectangle ImageRectangle
        {
            get
            {
                if (this.Image == null)
                {
                    return Rectangle.Empty;
                }
                CompositeDesignerTheme designerTheme = base.DesignerTheme as CompositeDesignerTheme;
                if (designerTheme == null)
                {
                    return Rectangle.Empty;
                }
                Rectangle bounds = base.Bounds;
                Size size = this.ExpandButtonRectangle.Size;
                Size imageSize = designerTheme.ImageSize;
                Size size3 = this.TextRectangle.Size;
                Size margin = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;
                Rectangle rectangle2 = new Rectangle(bounds.Location, imageSize);
                if (size3.Width > 0)
                {
                    rectangle2.X += (bounds.Width - imageSize.Width) / 2;
                }
                else
                {
                    rectangle2.X += (bounds.Width - (imageSize.Width + ((3 * size.Width) / 2))) / 2;
                    rectangle2.X += (3 * size.Width) / 2;
                }
                rectangle2.Y += 2 * margin.Height;
                if (size3.Height > 0)
                {
                    rectangle2.Y += size3.Height + margin.Height;
                }
                return rectangle2;
            }
        }

        public bool IsEditable
        {
            get
            {
                if (!(base.Activity is CompositeActivity))
                {
                    return false;
                }
                if (base.IsLocked)
                {
                    return false;
                }
                if (Helpers.IsCustomActivity(base.Activity as CompositeActivity))
                {
                    return false;
                }
                return true;
            }
        }

        public virtual object LastSelectableObject
        {
            get
            {
                return null;
            }
        }

        public override Point Location
        {
            get
            {
                return base.Location;
            }
            set
            {
                if (base.Location != value)
                {
                    Size size = new Size(value.X - base.Location.X, value.Y - base.Location.Y);
                    foreach (ActivityDesigner designer in this.ContainedDesigners)
                    {
                        designer.Location = new Point(designer.Location.X + size.Width, designer.Location.Y + size.Height);
                    }
                    base.Location = value;
                }
            }
        }

        protected override Rectangle TextRectangle
        {
            get
            {
                Rectangle rectangle2;
                if (string.IsNullOrEmpty(this.Text))
                {
                    return Rectangle.Empty;
                }
                CompositeDesignerTheme designerTheme = base.DesignerTheme as CompositeDesignerTheme;
                if (designerTheme == null)
                {
                    return Rectangle.Empty;
                }
                Rectangle bounds = base.Bounds;
                Size margin = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;
                Size size2 = this.CanExpandCollapse ? designerTheme.ExpandButtonSize : Size.Empty;
                int num = bounds.Width - ((2 * margin.Width) + ((3 * size2.Width) / 2));
                Size actualTextSize = this.actualTextSize;
                actualTextSize.Width /= this.Text.Length;
                actualTextSize.Width += ((actualTextSize.Width % this.Text.Length) > 0) ? 1 : 0;
                actualTextSize.Width *= Math.Min(this.Text.Length, 7);
                Size empty = Size.Empty;
                empty.Width = Math.Min(num, this.actualTextSize.Width);
                empty.Width = Math.Max(1, Math.Max(empty.Width, actualTextSize.Width));
                empty.Height = actualTextSize.Height;
                int num2 = this.actualTextSize.Width / empty.Width;
                num2 += ((this.actualTextSize.Width % empty.Width) > 0) ? 1 : 0;
                num2 = Math.Min(num2, 1);
                empty.Height *= num2;
                rectangle2 = new Rectangle(bounds.Location, empty) {
                    X = rectangle2.X + ((bounds.Width - (((3 * size2.Width) / 2) + empty.Width)) / 2),
                    X = rectangle2.X + ((3 * size2.Width) / 2),
                    Y = rectangle2.Y + (2 * margin.Height)
                };
                if (size2.Height > empty.Height)
                {
                    rectangle2.Y += (size2.Height - empty.Height) / 2;
                }
                rectangle2.Size = empty;
                return rectangle2;
            }
        }

        protected virtual int TitleHeight
        {
            get
            {
                Size margin = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;
                Rectangle expandButtonRectangle = this.ExpandButtonRectangle;
                Rectangle textRectangle = this.TextRectangle;
                Rectangle imageRectangle = this.ImageRectangle;
                int num = 0;
                if (!textRectangle.Size.IsEmpty)
                {
                    num = Math.Max(expandButtonRectangle.Height, textRectangle.Height) + imageRectangle.Height;
                }
                else
                {
                    num = Math.Max(expandButtonRectangle.Height, imageRectangle.Height);
                }
                if ((!expandButtonRectangle.Size.IsEmpty || !textRectangle.Size.IsEmpty) || !imageRectangle.Size.IsEmpty)
                {
                    num += (this.Expanded ? 2 : 3) * margin.Height;
                }
                if (!imageRectangle.Size.IsEmpty && !textRectangle.Size.IsEmpty)
                {
                    num += margin.Height;
                }
                return num;
            }
        }
    }
}

