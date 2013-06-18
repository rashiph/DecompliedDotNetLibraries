namespace System.Windows.Markup
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Xaml;

    [TypeForwardedFrom("PresentationFramework, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"), MarkupExtensionReturnType(typeof(Array)), ContentProperty("Items")]
    public class ArrayExtension : MarkupExtension
    {
        private ArrayList _arrayList;
        private System.Type _arrayType;

        public ArrayExtension()
        {
            this._arrayList = new ArrayList();
        }

        public ArrayExtension(Array elements)
        {
            this._arrayList = new ArrayList();
            if (elements == null)
            {
                throw new ArgumentNullException("elements");
            }
            this._arrayList.AddRange(elements);
            this._arrayType = elements.GetType().GetElementType();
        }

        public ArrayExtension(System.Type arrayType)
        {
            this._arrayList = new ArrayList();
            if (arrayType == null)
            {
                throw new ArgumentNullException("arrayType");
            }
            this._arrayType = arrayType;
        }

        public void AddChild(object value)
        {
            this._arrayList.Add(value);
        }

        public void AddText(string text)
        {
            this.AddChild(text);
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (this._arrayType == null)
            {
                throw new InvalidOperationException(System.Xaml.SR.Get("MarkupExtensionArrayType"));
            }
            object obj2 = null;
            try
            {
                obj2 = this._arrayList.ToArray(this._arrayType);
            }
            catch (InvalidCastException)
            {
                throw new InvalidOperationException(System.Xaml.SR.Get("MarkupExtensionArrayBadType", new object[] { this._arrayType.Name }));
            }
            return obj2;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public IList Items
        {
            get
            {
                return this._arrayList;
            }
        }

        [ConstructorArgument("type")]
        public System.Type Type
        {
            get
            {
                return this._arrayType;
            }
            set
            {
                this._arrayType = value;
            }
        }
    }
}

