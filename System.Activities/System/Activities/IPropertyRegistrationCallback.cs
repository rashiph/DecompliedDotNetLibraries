namespace System.Activities
{
    using System;

    public interface IPropertyRegistrationCallback
    {
        void Register(RegistrationContext context);
        void Unregister(RegistrationContext context);
    }
}

