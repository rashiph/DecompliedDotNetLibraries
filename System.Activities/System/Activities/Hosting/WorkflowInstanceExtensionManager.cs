namespace System.Activities.Hosting
{
    using System;
    using System.Activities;
    using System.Activities.Tracking;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime;
    using System.Runtime.CompilerServices;

    public class WorkflowInstanceExtensionManager
    {
        private List<object> additionalSingletonExtensions;
        private List<object> allSingletonExtensions;
        internal static List<KeyValuePair<Type, WorkflowInstanceExtensionProvider>> EmptyExtensionProviders = new List<KeyValuePair<Type, WorkflowInstanceExtensionProvider>>(0);
        internal static List<object> EmptySingletonExtensions = new List<object>(0);
        private bool hasSingletonPersistenceModule;
        private bool hasSingletonTrackingParticipant;
        private bool isReadonly;

        public virtual void Add(object singletonExtension)
        {
            if (singletonExtension == null)
            {
                throw FxTrace.Exception.ArgumentNull("singletonExtension");
            }
            this.ThrowIfReadOnly();
            if (singletonExtension is System.Activities.Hosting.SymbolResolver)
            {
                if (this.SymbolResolver != null)
                {
                    throw FxTrace.Exception.Argument("singletonExtension", System.Activities.SR.SymbolResolverAlreadyExists);
                }
                this.SymbolResolver = (System.Activities.Hosting.SymbolResolver) singletonExtension;
            }
            else
            {
                if (singletonExtension is IWorkflowInstanceExtension)
                {
                    this.HasSingletonIWorkflowInstanceExtensions = true;
                }
                if (!this.HasSingletonTrackingParticipant && (singletonExtension is TrackingParticipant))
                {
                    this.hasSingletonTrackingParticipant = true;
                }
                if (!this.HasSingletonPersistenceModule && (singletonExtension is IPersistencePipelineModule))
                {
                    this.hasSingletonPersistenceModule = true;
                }
            }
            if (this.SingletonExtensions == null)
            {
                this.SingletonExtensions = new List<object>();
            }
            this.SingletonExtensions.Add(singletonExtension);
        }

        public virtual void Add<T>(Func<T> extensionCreationFunction) where T: class
        {
            if (extensionCreationFunction == null)
            {
                throw FxTrace.Exception.ArgumentNull("extensionCreationFunction");
            }
            this.ThrowIfReadOnly();
            if (this.ExtensionProviders == null)
            {
                this.ExtensionProviders = new List<KeyValuePair<Type, WorkflowInstanceExtensionProvider>>();
            }
            this.ExtensionProviders.Add(new KeyValuePair<Type, WorkflowInstanceExtensionProvider>(typeof(T), new WorkflowInstanceExtensionProvider<T>(extensionCreationFunction)));
        }

        internal void AddAllExtensionTypes(HashSet<Type> extensionTypes)
        {
            for (int i = 0; i < this.SingletonExtensions.Count; i++)
            {
                extensionTypes.Add(this.SingletonExtensions[i].GetType());
            }
            for (int j = 0; j < this.ExtensionProviders.Count; j++)
            {
                KeyValuePair<Type, WorkflowInstanceExtensionProvider> pair = this.ExtensionProviders[j];
                extensionTypes.Add(pair.Key);
            }
        }

        internal static void AddExtensionClosure(object newExtension, ref List<object> targetCollection, ref bool addedTrackingParticipant, ref bool addedPersistenceModule)
        {
            IWorkflowInstanceExtension extension = newExtension as IWorkflowInstanceExtension;
            if (extension != null)
            {
                Queue<IWorkflowInstanceExtension> queue = null;
                if (targetCollection == null)
                {
                    targetCollection = new List<object>();
                }
                while (extension != null)
                {
                    IEnumerable<object> additionalExtensions = extension.GetAdditionalExtensions();
                    if (additionalExtensions != null)
                    {
                        foreach (object obj2 in additionalExtensions)
                        {
                            targetCollection.Add(obj2);
                            if (obj2 is IWorkflowInstanceExtension)
                            {
                                if (queue == null)
                                {
                                    queue = new Queue<IWorkflowInstanceExtension>();
                                }
                                queue.Enqueue((IWorkflowInstanceExtension) obj2);
                            }
                            if (!addedTrackingParticipant && (obj2 is TrackingParticipant))
                            {
                                addedTrackingParticipant = true;
                            }
                            if (!addedPersistenceModule && (obj2 is IPersistencePipelineModule))
                            {
                                addedPersistenceModule = true;
                            }
                        }
                    }
                    if ((queue != null) && (queue.Count > 0))
                    {
                        extension = queue.Dequeue();
                    }
                    else
                    {
                        extension = null;
                    }
                }
            }
        }

        internal static WorkflowInstanceExtensionCollection CreateInstanceExtensions(Activity workflowDefinition, WorkflowInstanceExtensionManager extensionManager)
        {
            if (extensionManager != null)
            {
                extensionManager.MakeReadOnly();
                return new WorkflowInstanceExtensionCollection(workflowDefinition, extensionManager);
            }
            if ((workflowDefinition.DefaultExtensionsCount <= 0) && (workflowDefinition.RequiredExtensionTypesCount <= 0))
            {
                return null;
            }
            return new WorkflowInstanceExtensionCollection(workflowDefinition, null);
        }

        internal List<object> GetAllSingletonExtensions()
        {
            return this.allSingletonExtensions;
        }

        public void MakeReadOnly()
        {
            if (!this.isReadonly)
            {
                if (this.SingletonExtensions == null)
                {
                    this.SingletonExtensions = EmptySingletonExtensions;
                    this.allSingletonExtensions = EmptySingletonExtensions;
                }
                else
                {
                    if (this.HasSingletonIWorkflowInstanceExtensions)
                    {
                        foreach (IWorkflowInstanceExtension extension in this.SingletonExtensions.OfType<IWorkflowInstanceExtension>())
                        {
                            AddExtensionClosure(extension, ref this.additionalSingletonExtensions, ref this.hasSingletonTrackingParticipant, ref this.hasSingletonPersistenceModule);
                        }
                        if (this.AdditionalSingletonExtensions != null)
                        {
                            for (int i = 0; i < this.AdditionalSingletonExtensions.Count; i++)
                            {
                                object obj2 = this.AdditionalSingletonExtensions[i];
                                if (obj2 is IWorkflowInstanceExtension)
                                {
                                    this.HasAdditionalSingletonIWorkflowInstanceExtensions = true;
                                    break;
                                }
                            }
                        }
                    }
                    this.allSingletonExtensions = this.SingletonExtensions;
                    if ((this.AdditionalSingletonExtensions != null) && (this.AdditionalSingletonExtensions.Count > 0))
                    {
                        this.allSingletonExtensions = new List<object>(this.SingletonExtensions);
                        this.allSingletonExtensions.AddRange(this.AdditionalSingletonExtensions);
                    }
                }
                if (this.ExtensionProviders == null)
                {
                    this.ExtensionProviders = EmptyExtensionProviders;
                }
                this.isReadonly = true;
            }
        }

        private void ThrowIfReadOnly()
        {
            if (this.isReadonly)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.ExtensionsCannotBeModified));
            }
        }

        internal List<object> AdditionalSingletonExtensions
        {
            get
            {
                return this.additionalSingletonExtensions;
            }
        }

        internal List<KeyValuePair<Type, WorkflowInstanceExtensionProvider>> ExtensionProviders { get; private set; }

        internal bool HasAdditionalSingletonIWorkflowInstanceExtensions { get; private set; }

        internal bool HasSingletonIWorkflowInstanceExtensions { get; private set; }

        internal bool HasSingletonPersistenceModule
        {
            get
            {
                return this.hasSingletonPersistenceModule;
            }
        }

        internal bool HasSingletonTrackingParticipant
        {
            get
            {
                return this.hasSingletonTrackingParticipant;
            }
        }

        internal List<object> SingletonExtensions { get; private set; }

        internal System.Activities.Hosting.SymbolResolver SymbolResolver { get; private set; }
    }
}

