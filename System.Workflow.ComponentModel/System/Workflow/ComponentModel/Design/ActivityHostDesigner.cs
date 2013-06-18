namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.IO;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    internal abstract class ActivityHostDesigner : SequentialActivityDesigner
    {
        private ContainedDesignSurface containedDesignSurface;
        private ContainedDesignerLoader containedLoader;
        private IWorkflowRootDesigner containedRootDesigner;
        private MemoryStream lastInvokedWorkflowState;

        public override bool CanInsertActivities(HitTestInfo insertLocation, ReadOnlyCollection<Activity> activitiesToInsert)
        {
            return false;
        }

        public override bool CanRemoveActivities(ReadOnlyCollection<Activity> activitiesToRemove)
        {
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    this.containedRootDesigner = null;
                    this.containedLoader = null;
                    if (this.containedDesignSurface != null)
                    {
                        this.containedDesignSurface.Dispose();
                        this.containedDesignSurface = null;
                    }
                    if (this.lastInvokedWorkflowState != null)
                    {
                        this.lastInvokedWorkflowState.Close();
                        this.lastInvokedWorkflowState = null;
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override void InsertActivities(HitTestInfo insertLocation, ReadOnlyCollection<Activity> activitiesToInsert)
        {
        }

        private IWorkflowRootDesigner LoadHostedWorkflow()
        {
            if (this.RootActivity == null)
            {
                return null;
            }
            this.containedLoader = new ContainedDesignerLoader(this.RootActivity);
            this.containedDesignSurface = new ContainedDesignSurface(base.Activity.Site, this);
            if (!this.containedDesignSurface.IsLoaded)
            {
                this.containedDesignSurface.BeginLoad(this.containedLoader);
            }
            return ActivityDesigner.GetSafeRootDesigner(this.containedDesignSurface.GetService(typeof(IDesignerHost)) as IServiceProvider);
        }

        protected override void LoadViewState(BinaryReader reader)
        {
            base.LoadViewState(reader);
            if (reader.ReadBoolean())
            {
                if (this.containedDesignSurface == null)
                {
                    this.containedRootDesigner = this.LoadHostedWorkflow();
                }
                if (this.containedDesignSurface != null)
                {
                    IDesignerHost service = this.containedDesignSurface.GetService(typeof(IDesignerHost)) as IDesignerHost;
                    if (service == null)
                    {
                        throw new Exception(SR.GetString("General_MissingService", new object[] { typeof(IDesignerHost).FullName }));
                    }
                    DesignerHelpers.DeserializeDesignerStates(service, reader);
                }
            }
        }

        protected void RefreshHostedActivity()
        {
            if (this.containedRootDesigner != null)
            {
                this.lastInvokedWorkflowState = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(this.lastInvokedWorkflowState);
                this.SaveViewState(writer);
            }
            this.containedRootDesigner = this.LoadHostedWorkflow();
            if (this.lastInvokedWorkflowState != null)
            {
                this.lastInvokedWorkflowState.Position = 0L;
                BinaryReader reader = new BinaryReader(this.lastInvokedWorkflowState);
                try
                {
                    this.LoadViewState(reader);
                }
                catch
                {
                }
            }
            base.PerformLayout();
        }

        public override void RemoveActivities(ReadOnlyCollection<Activity> activitiesToRemove)
        {
        }

        protected override void SaveViewState(BinaryWriter writer)
        {
            base.SaveViewState(writer);
            if (this.containedDesignSurface != null)
            {
                writer.Write(true);
                IDesignerHost service = this.containedDesignSurface.GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (service == null)
                {
                    throw new Exception(SR.GetString("General_MissingService", new object[] { typeof(IDesignerHost).FullName }));
                }
                DesignerHelpers.SerializeDesignerStates(service, writer);
            }
            else
            {
                writer.Write(false);
            }
        }

        public override ReadOnlyCollection<ActivityDesigner> ContainedDesigners
        {
            get
            {
                List<ActivityDesigner> list = new List<ActivityDesigner>();
                if (this.containedRootDesigner != null)
                {
                    list.Add((ActivityDesigner) this.containedRootDesigner);
                }
                return list.AsReadOnly();
            }
        }

        protected abstract Activity RootActivity { get; }

        private sealed class ContainedDesignerLoader : WorkflowDesignerLoader
        {
            private Activity rootActivity;

            internal ContainedDesignerLoader(Activity rootActivity)
            {
                this.rootActivity = rootActivity;
            }

            public override void Flush()
            {
            }

            public override void ForceReload()
            {
            }

            public override TextReader GetFileReader(string filePath)
            {
                return null;
            }

            public override TextWriter GetFileWriter(string filePath)
            {
                return null;
            }

            protected override void Initialize()
            {
                base.Initialize();
                ServiceCreatorCallback callback = new ServiceCreatorCallback(this.OnCreateService);
                base.LoaderHost.RemoveService(typeof(IReferenceService));
                base.LoaderHost.AddService(typeof(IReferenceService), callback);
            }

            private object OnCreateService(IServiceContainer container, Type serviceType)
            {
                object obj2 = null;
                if (serviceType == typeof(IReferenceService))
                {
                    obj2 = new System.Workflow.ComponentModel.Design.ReferenceService(base.LoaderHost);
                }
                return obj2;
            }

            protected override void PerformLoad(IDesignerSerializationManager serializationManager)
            {
                IDesignerHost service = (IDesignerHost) base.GetService(typeof(IDesignerHost));
                if ((this.rootActivity != null) && (this.rootActivity != null))
                {
                    base.AddActivityToDesigner(this.rootActivity);
                    base.SetBaseComponentClassName(this.rootActivity.GetType().FullName);
                }
            }

            public override string FileName
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return string.Empty;
                }
            }
        }

        private class ContainedDesignSurface : DesignSurface
        {
            private CompositeActivityDesigner parentDesigner;

            internal ContainedDesignSurface(IServiceProvider parentServiceProvider, CompositeActivityDesigner parentDesigner) : base(parentServiceProvider)
            {
                this.parentDesigner = parentDesigner;
                if (base.ServiceContainer != null)
                {
                    base.ServiceContainer.RemoveService(typeof(ISelectionService));
                }
            }

            protected override IDesigner CreateDesigner(IComponent component, bool rootDesigner)
            {
                IDesigner designer = base.CreateDesigner(component, rootDesigner);
                if (rootDesigner)
                {
                    IWorkflowRootDesigner designer2 = designer as IWorkflowRootDesigner;
                    if (designer2 != null)
                    {
                        designer2.InvokingDesigner = this.parentDesigner;
                    }
                }
                return designer;
            }
        }
    }
}

