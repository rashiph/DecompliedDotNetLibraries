namespace System.Workflow.Activities
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Workflow.ComponentModel.Design;

    [Serializable]
    internal sealed class InvokeWebServiceToolboxItem : ActivityToolboxItem
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InvokeWebServiceToolboxItem(Type type) : base(type)
        {
        }

        private InvokeWebServiceToolboxItem(SerializationInfo info, StreamingContext context)
        {
            base.Deserialize(info, context);
        }

        public override IComponent[] CreateComponentsWithUI(IDesignerHost host)
        {
            Uri url = null;
            Type proxyClass = null;
            IExtendedUIService service = host.GetService(typeof(IExtendedUIService)) as IExtendedUIService;
            if (service != null)
            {
                service.AddWebReference(out url, out proxyClass);
            }
            IComponent[] componentArray = base.CreateComponentsWithUI(host);
            if (componentArray.GetLength(0) > 0)
            {
                InvokeWebServiceActivity activity = componentArray[0] as InvokeWebServiceActivity;
                if (activity != null)
                {
                    activity.ProxyClass = proxyClass;
                }
            }
            return componentArray;
        }
    }
}

