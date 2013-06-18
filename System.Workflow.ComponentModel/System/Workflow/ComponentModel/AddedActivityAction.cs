namespace System.Workflow.ComponentModel
{
    using System;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Serialization;
    using System.Xml;

    public sealed class AddedActivityAction : ActivityChangeAction
    {
        private Activity addedActivity;
        private int index;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public AddedActivityAction()
        {
        }

        public AddedActivityAction(CompositeActivity compositeActivity, Activity activityAdded) : base(compositeActivity)
        {
            if (compositeActivity == null)
            {
                throw new ArgumentNullException("compositeActivity");
            }
            if (activityAdded == null)
            {
                throw new ArgumentNullException("activityAdded");
            }
            this.index = (compositeActivity.Activities != null) ? compositeActivity.Activities.IndexOf(activityAdded) : -1;
            this.addedActivity = activityAdded;
        }

        protected internal override bool ApplyTo(Activity rootActivity)
        {
            if (rootActivity == null)
            {
                throw new ArgumentNullException("rootActivity");
            }
            if (!(rootActivity is CompositeActivity))
            {
                throw new ArgumentException(SR.GetString("Error_RootActivityTypeInvalid"), "rootActivity");
            }
            CompositeActivity compositeActivity = rootActivity.TraverseDottedPathFromRoot(base.OwnerActivityDottedPath) as CompositeActivity;
            if (compositeActivity == null)
            {
                return false;
            }
            compositeActivity.DynamicUpdateMode = true;
            CompositeActivity parent = this.addedActivity.Parent;
            try
            {
                this.addedActivity.SetParent(compositeActivity);
                Activity addedActivity = this.addedActivity;
                if (!this.addedActivity.DesignMode)
                {
                    addedActivity = this.addedActivity.Clone();
                }
                else
                {
                    TypeProvider serviceInstance = WorkflowChanges.CreateTypeProvider(rootActivity);
                    ServiceContainer provider = new ServiceContainer();
                    provider.AddService(typeof(ITypeProvider), serviceInstance);
                    DesignerSerializationManager manager = new DesignerSerializationManager(provider);
                    WorkflowMarkupSerializer serializer = new WorkflowMarkupSerializer();
                    string s = string.Empty;
                    using (manager.CreateSession())
                    {
                        using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
                        {
                            using (XmlWriter writer2 = Helpers.CreateXmlWriter(writer))
                            {
                                WorkflowMarkupSerializationManager serializationManager = new WorkflowMarkupSerializationManager(manager);
                                serializer.Serialize(serializationManager, writer2, this.addedActivity);
                                s = writer.ToString();
                            }
                        }
                        using (StringReader reader = new StringReader(s))
                        {
                            using (XmlReader reader2 = XmlReader.Create(reader))
                            {
                                WorkflowMarkupSerializationManager manager3 = new WorkflowMarkupSerializationManager(manager);
                                addedActivity = serializer.Deserialize(manager3, reader2) as Activity;
                            }
                        }
                    }
                    if (addedActivity == null)
                    {
                        throw new InvalidOperationException(SR.GetString("Error_ApplyDynamicChangeFailed"));
                    }
                }
                if (compositeActivity.WorkflowCoreRuntime != null)
                {
                    ((IDependencyObjectAccessor) addedActivity).InitializeInstanceForRuntime(compositeActivity.WorkflowCoreRuntime);
                }
                addedActivity.SetParent(null);
                compositeActivity.Activities.Insert(this.index, addedActivity);
            }
            finally
            {
                this.addedActivity.SetParent(parent);
                compositeActivity.DynamicUpdateMode = false;
            }
            return true;
        }

        public Activity AddedActivity
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.addedActivity;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            internal set
            {
                this.addedActivity = value;
            }
        }

        public int Index
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.index;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            internal set
            {
                this.index = value;
            }
        }
    }
}

