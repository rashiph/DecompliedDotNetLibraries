namespace System.ServiceModel.Activities
{
    using System;
    using System.ServiceModel;
    using System.Windows.Markup;

    [ContentProperty("MessageQuerySet")]
    public sealed class QueryCorrelationInitializer : CorrelationInitializer
    {
        private System.ServiceModel.MessageQuerySet messageQuerySet;

        internal override CorrelationInitializer CloneCore()
        {
            return new QueryCorrelationInitializer { MessageQuerySet = this.MessageQuerySet };
        }

        public System.ServiceModel.MessageQuerySet MessageQuerySet
        {
            get
            {
                if (this.messageQuerySet == null)
                {
                    this.messageQuerySet = new System.ServiceModel.MessageQuerySet();
                }
                return this.messageQuerySet;
            }
            set
            {
                this.messageQuerySet = value;
            }
        }
    }
}

