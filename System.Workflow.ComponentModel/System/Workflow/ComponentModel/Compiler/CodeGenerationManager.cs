namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.Design.Serialization;
    using System.Workflow.ComponentModel;

    public sealed class CodeGenerationManager : IServiceProvider
    {
        private ContextStack context;
        private Hashtable hashOfGenerators = new Hashtable();
        private IServiceProvider serviceProvider;

        public CodeGenerationManager(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public ActivityCodeGenerator[] GetCodeGenerators(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (this.hashOfGenerators.Contains(type))
            {
                return ((List<ActivityCodeGenerator>) this.hashOfGenerators[type]).ToArray();
            }
            List<ActivityCodeGenerator> list = new List<ActivityCodeGenerator>();
            foreach (ActivityCodeGenerator generator in ComponentDispenser.CreateComponents(type, typeof(ActivityCodeGeneratorAttribute)))
            {
                list.Add(generator);
            }
            this.hashOfGenerators[type] = list;
            return list.ToArray();
        }

        public object GetService(Type serviceType)
        {
            if (this.serviceProvider == null)
            {
                return null;
            }
            return this.serviceProvider.GetService(serviceType);
        }

        public ContextStack Context
        {
            get
            {
                if (this.context == null)
                {
                    this.context = new ContextStack();
                }
                return this.context;
            }
        }
    }
}

