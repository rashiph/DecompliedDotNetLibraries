namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    internal sealed class ActivityCodeDomReferenceService : IReferenceService
    {
        private IReferenceService refService;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ActivityCodeDomReferenceService(IReferenceService referenceService)
        {
            this.refService = referenceService;
        }

        public IComponent GetComponent(object reference)
        {
            if (this.refService != null)
            {
                return this.refService.GetComponent(reference);
            }
            return null;
        }

        public string GetName(object reference)
        {
            Activity activity = reference as Activity;
            if (activity != null)
            {
                return activity.QualifiedName.Replace('.', '_');
            }
            if (this.refService != null)
            {
                return this.refService.GetName(reference);
            }
            return null;
        }

        public object GetReference(string name)
        {
            if (this.refService != null)
            {
                return this.refService.GetReference(name);
            }
            return null;
        }

        public object[] GetReferences()
        {
            if (this.refService != null)
            {
                return this.refService.GetReferences();
            }
            return null;
        }

        public object[] GetReferences(Type baseType)
        {
            if (this.refService != null)
            {
                return this.refService.GetReferences(baseType);
            }
            return null;
        }
    }
}

