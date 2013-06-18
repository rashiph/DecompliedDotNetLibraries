namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Runtime;

    [ContentProperty("Items")]
    internal sealed class ArrayExtension : MarkupExtension
    {
        private ArrayList arrayElementList;
        private System.Type arrayType;

        public ArrayExtension()
        {
            this.arrayElementList = new ArrayList();
        }

        public ArrayExtension(Array elements)
        {
            this.arrayElementList = new ArrayList();
            if (elements == null)
            {
                throw new ArgumentNullException("elements");
            }
            this.arrayElementList.AddRange(elements);
            this.arrayType = elements.GetType().GetElementType();
        }

        public ArrayExtension(System.Type arrayType)
        {
            this.arrayElementList = new ArrayList();
            if (arrayType == null)
            {
                throw new ArgumentNullException("arrayType");
            }
            this.arrayType = arrayType;
        }

        public override object ProvideValue(IServiceProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            if (this.arrayType == null)
            {
                throw new InvalidOperationException("ArrayType needs to be set.");
            }
            object obj2 = null;
            try
            {
                obj2 = this.arrayElementList.ToArray(this.arrayType);
            }
            catch (InvalidCastException)
            {
                throw new InvalidOperationException();
            }
            return obj2;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public IList Items
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.arrayElementList;
            }
        }

        public System.Type Type
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.arrayType;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.arrayType = value;
            }
        }
    }
}

