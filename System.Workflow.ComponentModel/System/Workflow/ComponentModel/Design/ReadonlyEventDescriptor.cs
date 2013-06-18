namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;

    internal sealed class ReadonlyEventDescriptor : EventDescriptor
    {
        private EventDescriptor realEventDescriptor;

        internal ReadonlyEventDescriptor(EventDescriptor e) : base(e, null)
        {
            this.realEventDescriptor = e;
        }

        public override void AddEventHandler(object component, Delegate value)
        {
        }

        public override void RemoveEventHandler(object component, Delegate value)
        {
        }

        public override AttributeCollection Attributes
        {
            get
            {
                ArrayList list = new ArrayList();
                foreach (Attribute attribute in this.realEventDescriptor.Attributes)
                {
                    if (!(attribute is EditorAttribute) && !(attribute is ReadOnlyAttribute))
                    {
                        list.Add(attribute);
                    }
                }
                list.Add(new ReadOnlyAttribute(true));
                return new AttributeCollection((Attribute[]) list.ToArray(typeof(Attribute)));
            }
        }

        public override string Category
        {
            get
            {
                return this.realEventDescriptor.Category;
            }
        }

        public override Type ComponentType
        {
            get
            {
                return this.realEventDescriptor.ComponentType;
            }
        }

        public override string Description
        {
            get
            {
                return this.realEventDescriptor.Description;
            }
        }

        public override Type EventType
        {
            get
            {
                return this.realEventDescriptor.EventType;
            }
        }

        public override bool IsMulticast
        {
            get
            {
                return this.realEventDescriptor.IsMulticast;
            }
        }
    }
}

