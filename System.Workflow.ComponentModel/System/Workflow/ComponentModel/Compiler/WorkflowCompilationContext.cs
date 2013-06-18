namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.Runtime;
    using System.Runtime.Versioning;

    public sealed class WorkflowCompilationContext
    {
        private ReadOnlyCollection<AuthorizedType> authorizedTypes;
        [ThreadStatic]
        private static WorkflowCompilationContext current;
        private ContextScope scope;

        private WorkflowCompilationContext(ContextScope scope)
        {
            this.scope = scope;
        }

        public static IDisposable CreateScope(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }
            IWorkflowCompilerOptionsService optionsService = serviceProvider.GetService(typeof(IWorkflowCompilerOptionsService)) as IWorkflowCompilerOptionsService;
            if (optionsService != null)
            {
                return CreateScope(serviceProvider, optionsService);
            }
            return new DefaultContextScope(serviceProvider);
        }

        private static IDisposable CreateScope(IServiceProvider serviceProvider, IWorkflowCompilerOptionsService optionsService)
        {
            WorkflowCompilerOptionsService service = optionsService as WorkflowCompilerOptionsService;
            if (service != null)
            {
                return new StandardContextScope(serviceProvider, service);
            }
            return new InterfaceContextScope(serviceProvider, optionsService);
        }

        internal static IDisposable CreateScope(IServiceProvider serviceProvider, WorkflowCompilerParameters parameters)
        {
            return new ParametersContextScope(serviceProvider, parameters);
        }

        public IList<AuthorizedType> GetAuthorizedTypes()
        {
            if (this.authorizedTypes == null)
            {
                try
                {
                    IList<AuthorizedType> list;
                    IDictionary<string, IList<AuthorizedType>> section = ConfigurationManager.GetSection("System.Workflow.ComponentModel.WorkflowCompiler/authorizedTypes") as IDictionary<string, IList<AuthorizedType>>;
                    if (section.TryGetValue("v" + this.TargetFrameworkVersion.ToString(), out list))
                    {
                        this.authorizedTypes = new ReadOnlyCollection<AuthorizedType>(list);
                    }
                }
                catch
                {
                }
            }
            return this.authorizedTypes;
        }

        public bool CheckTypes
        {
            get
            {
                return this.scope.CheckTypes;
            }
        }

        public static WorkflowCompilationContext Current
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return current;
            }
            private set
            {
                current = value;
            }
        }

        public string Language
        {
            get
            {
                return this.scope.Language;
            }
        }

        public string RootNamespace
        {
            get
            {
                return this.scope.RootNamespace;
            }
        }

        internal IServiceProvider ServiceProvider
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.scope;
            }
        }

        internal FrameworkName TargetFramework
        {
            get
            {
                return this.scope.TargetFramework;
            }
        }

        internal Version TargetFrameworkVersion
        {
            get
            {
                FrameworkName targetFramework = this.scope.TargetFramework;
                if (targetFramework != null)
                {
                    return targetFramework.Version;
                }
                return MultiTargetingInfo.DefaultTargetFramework;
            }
        }

        private abstract class ContextScope : IDisposable, IServiceProvider
        {
            private WorkflowCompilationContext currentContext;
            private bool disposed;
            private IServiceProvider serviceProvider;

            protected ContextScope(IServiceProvider serviceProvider)
            {
                this.serviceProvider = serviceProvider;
                this.currentContext = WorkflowCompilationContext.Current;
                WorkflowCompilationContext.Current = new WorkflowCompilationContext(this);
            }

            public void Dispose()
            {
                this.DisposeImpl();
                GC.SuppressFinalize(this);
            }

            private void DisposeImpl()
            {
                if (!this.disposed)
                {
                    WorkflowCompilationContext.Current = this.currentContext;
                    this.disposed = true;
                }
            }

            ~ContextScope()
            {
                this.DisposeImpl();
            }

            public object GetService(Type serviceType)
            {
                return this.serviceProvider.GetService(serviceType);
            }

            public abstract bool CheckTypes { get; }

            public abstract string Language { get; }

            public abstract string RootNamespace { get; }

            public abstract FrameworkName TargetFramework { get; }
        }

        private class DefaultContextScope : WorkflowCompilationContext.ContextScope
        {
            public DefaultContextScope(IServiceProvider serviceProvider) : base(serviceProvider)
            {
            }

            public override bool CheckTypes
            {
                get
                {
                    return false;
                }
            }

            public override string Language
            {
                get
                {
                    return "CSharp";
                }
            }

            public override string RootNamespace
            {
                get
                {
                    return string.Empty;
                }
            }

            public override FrameworkName TargetFramework
            {
                get
                {
                    return null;
                }
            }
        }

        private class InterfaceContextScope : WorkflowCompilationContext.ContextScope
        {
            private IWorkflowCompilerOptionsService service;

            public InterfaceContextScope(IServiceProvider serviceProvider, IWorkflowCompilerOptionsService service) : base(serviceProvider)
            {
                this.service = service;
            }

            public override bool CheckTypes
            {
                get
                {
                    return this.service.CheckTypes;
                }
            }

            public override string Language
            {
                get
                {
                    return this.service.Language;
                }
            }

            public override string RootNamespace
            {
                get
                {
                    return this.service.RootNamespace;
                }
            }

            public override FrameworkName TargetFramework
            {
                get
                {
                    return null;
                }
            }
        }

        private class ParametersContextScope : WorkflowCompilationContext.ContextScope
        {
            private WorkflowCompilerParameters parameters;

            public ParametersContextScope(IServiceProvider serviceProvider, WorkflowCompilerParameters parameters) : base(serviceProvider)
            {
                this.parameters = parameters;
            }

            public override bool CheckTypes
            {
                get
                {
                    return this.parameters.CheckTypes;
                }
            }

            public override string Language
            {
                get
                {
                    return this.parameters.LanguageToUse;
                }
            }

            public override string RootNamespace
            {
                get
                {
                    return WorkflowCompilerParameters.ExtractRootNamespace(this.parameters);
                }
            }

            public override FrameworkName TargetFramework
            {
                get
                {
                    if (this.parameters.MultiTargetingInformation != null)
                    {
                        return this.parameters.MultiTargetingInformation.TargetFramework;
                    }
                    return null;
                }
            }
        }

        private class StandardContextScope : WorkflowCompilationContext.ContextScope
        {
            private FrameworkName fxName;
            private WorkflowCompilerOptionsService service;

            public StandardContextScope(IServiceProvider serviceProvider, WorkflowCompilerOptionsService service) : base(serviceProvider)
            {
                this.service = service;
            }

            public override bool CheckTypes
            {
                get
                {
                    return this.service.CheckTypes;
                }
            }

            public override string Language
            {
                get
                {
                    return this.service.Language;
                }
            }

            public override string RootNamespace
            {
                get
                {
                    return this.service.RootNamespace;
                }
            }

            public override FrameworkName TargetFramework
            {
                get
                {
                    if (this.fxName == null)
                    {
                        string targetFrameworkMoniker = this.service.TargetFrameworkMoniker;
                        if (!string.IsNullOrEmpty(targetFrameworkMoniker))
                        {
                            this.fxName = new FrameworkName(targetFrameworkMoniker);
                        }
                    }
                    return this.fxName;
                }
            }
        }
    }
}

