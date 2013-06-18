namespace System.ServiceModel.Activities
{
    using System;
    using System.Activities;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;

    [ContentProperty("Body")]
    internal class NoPersistScope : NativeActivity
    {
        private Variable<NoPersistHandle> noPersistHandle = new Variable<NoPersistHandle>();

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            metadata.AddChild(this.Body);
            metadata.AddImplementationVariable(this.noPersistHandle);
        }

        protected override void Execute(NativeActivityContext context)
        {
            if (this.Body != null)
            {
                this.noPersistHandle.Get(context).Enter(context);
                context.ScheduleActivity(this.Body);
            }
        }

        [DefaultValue((string) null)]
        public Activity Body { get; set; }
    }
}

