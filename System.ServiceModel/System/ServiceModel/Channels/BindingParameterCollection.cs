namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;

    public class BindingParameterCollection : KeyedByTypeCollection<object>
    {
        public BindingParameterCollection()
        {
        }

        internal BindingParameterCollection(params object[] parameters)
        {
            if (parameters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameters");
            }
            for (int i = 0; i < parameters.Length; i++)
            {
                base.Add(parameters[i]);
            }
        }

        internal BindingParameterCollection(BindingParameterCollection parameters)
        {
            if (parameters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameters");
            }
            for (int i = 0; i < parameters.Count; i++)
            {
                base.Add(parameters[i]);
            }
        }
    }
}

