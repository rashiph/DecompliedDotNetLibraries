namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.Design.Serialization;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    public sealed class ValidationManager : IServiceProvider
    {
        private ContextStack context;
        private Hashtable hashOfValidators;
        private IServiceProvider serviceProvider;
        private bool validateChildActivities;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ValidationManager(IServiceProvider serviceProvider) : this(serviceProvider, true)
        {
        }

        public ValidationManager(IServiceProvider serviceProvider, bool validateChildActivities)
        {
            this.hashOfValidators = new Hashtable();
            this.validateChildActivities = true;
            this.serviceProvider = serviceProvider;
            this.validateChildActivities = validateChildActivities;
        }

        public object GetService(Type serviceType)
        {
            return this.serviceProvider.GetService(serviceType);
        }

        public Validator[] GetValidators(Type type)
        {
            if (this.hashOfValidators.Contains(type))
            {
                return ((List<Validator>) this.hashOfValidators[type]).ToArray();
            }
            List<Validator> list = new List<Validator>();
            foreach (Validator validator in ComponentDispenser.CreateComponents(type, typeof(ActivityValidatorAttribute)))
            {
                list.Add(validator);
            }
            this.hashOfValidators[type] = list;
            return list.ToArray();
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

        public bool ValidateChildActivities
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.validateChildActivities;
            }
        }
    }
}

