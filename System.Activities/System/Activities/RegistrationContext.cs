namespace System.Activities
{
    using System;
    using System.Activities.Runtime;

    public sealed class RegistrationContext
    {
        private IdSpace currentIdSpace;
        private ExecutionPropertyManager properties;

        internal RegistrationContext(ExecutionPropertyManager properties, IdSpace currentIdSpace)
        {
            this.properties = properties;
            this.currentIdSpace = currentIdSpace;
        }

        public object FindProperty(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("name");
            }
            if (this.properties == null)
            {
                return null;
            }
            return this.properties.GetProperty(name, this.currentIdSpace);
        }
    }
}

