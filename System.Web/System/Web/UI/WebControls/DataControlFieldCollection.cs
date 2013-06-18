namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Reflection;
    using System.Threading;
    using System.Web;
    using System.Web.UI;

    public sealed class DataControlFieldCollection : StateManagedCollection
    {
        private static readonly Type[] knownTypes = new Type[] { typeof(BoundField), typeof(ButtonField), typeof(CheckBoxField), typeof(CommandField), typeof(HyperLinkField), typeof(ImageField), typeof(TemplateField) };

        public event EventHandler FieldsChanged;

        public void Add(DataControlField field)
        {
            ((IList) this).Add(field);
        }

        public DataControlFieldCollection CloneFields()
        {
            DataControlFieldCollection fields = new DataControlFieldCollection();
            foreach (DataControlField field in this)
            {
                fields.Add(field.CloneField());
            }
            return fields;
        }

        public bool Contains(DataControlField field)
        {
            return ((IList) this).Contains(field);
        }

        public void CopyTo(DataControlField[] array, int index)
        {
            this.CopyTo(array, index);
        }

        protected override object CreateKnownType(int index)
        {
            switch (index)
            {
                case 0:
                    return new BoundField();

                case 1:
                    return new ButtonField();

                case 2:
                    return new CheckBoxField();

                case 3:
                    return new CommandField();

                case 4:
                    return new HyperLinkField();

                case 5:
                    return new ImageField();

                case 6:
                    return new TemplateField();
            }
            throw new ArgumentOutOfRangeException(System.Web.SR.GetString("DataControlFieldCollection_InvalidTypeIndex"));
        }

        protected override Type[] GetKnownTypes()
        {
            return knownTypes;
        }

        public int IndexOf(DataControlField field)
        {
            return ((IList) this).IndexOf(field);
        }

        public void Insert(int index, DataControlField field)
        {
            ((IList) this).Insert(index, field);
        }

        protected override void OnClearComplete()
        {
            this.OnFieldsChanged();
        }

        private void OnFieldChanged(object sender, EventArgs e)
        {
            this.OnFieldsChanged();
        }

        private void OnFieldsChanged()
        {
            if (this.FieldsChanged != null)
            {
                this.FieldsChanged(this, EventArgs.Empty);
            }
        }

        protected override void OnInsertComplete(int index, object value)
        {
            DataControlField field = value as DataControlField;
            if (field != null)
            {
                field.FieldChanged += new EventHandler(this.OnFieldChanged);
            }
            this.OnFieldsChanged();
        }

        protected override void OnRemoveComplete(int index, object value)
        {
            DataControlField field = value as DataControlField;
            if (field != null)
            {
                field.FieldChanged -= new EventHandler(this.OnFieldChanged);
            }
            this.OnFieldsChanged();
        }

        protected override void OnValidate(object o)
        {
            base.OnValidate(o);
            if (!(o is DataControlField))
            {
                throw new ArgumentException(System.Web.SR.GetString("DataControlFieldCollection_InvalidType"));
            }
        }

        public void Remove(DataControlField field)
        {
            ((IList) this).Remove(field);
        }

        public void RemoveAt(int index)
        {
            ((IList) this).RemoveAt(index);
        }

        protected override void SetDirtyObject(object o)
        {
            ((DataControlField) o).SetDirty();
        }

        [Browsable(false)]
        public DataControlField this[int index]
        {
            get
            {
                return (this[index] as DataControlField);
            }
        }
    }
}

