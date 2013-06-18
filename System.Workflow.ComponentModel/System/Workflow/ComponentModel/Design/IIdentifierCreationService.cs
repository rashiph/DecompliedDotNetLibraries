namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.Workflow.ComponentModel;

    public interface IIdentifierCreationService
    {
        void EnsureUniqueIdentifiers(CompositeActivity parentActivity, ICollection childActivities);
        void ValidateIdentifier(Activity activity, string identifier);
    }
}

