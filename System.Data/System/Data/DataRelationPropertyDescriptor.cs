namespace System.Data
{
    using System;
    using System.ComponentModel;

    internal sealed class DataRelationPropertyDescriptor : PropertyDescriptor
    {
        private DataRelation relation;

        internal DataRelationPropertyDescriptor(DataRelation dataRelation) : base(dataRelation.RelationName, null)
        {
            this.relation = dataRelation;
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override bool Equals(object other)
        {
            if (other is DataRelationPropertyDescriptor)
            {
                DataRelationPropertyDescriptor descriptor = (DataRelationPropertyDescriptor) other;
                return (descriptor.Relation == this.Relation);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.Relation.GetHashCode();
        }

        public override object GetValue(object component)
        {
            DataRowView view = (DataRowView) component;
            return view.CreateChildView(this.relation);
        }

        public override void ResetValue(object component)
        {
        }

        public override void SetValue(object component, object value)
        {
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }

        public override Type ComponentType
        {
            get
            {
                return typeof(DataRowView);
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public override Type PropertyType
        {
            get
            {
                return typeof(IBindingList);
            }
        }

        internal DataRelation Relation
        {
            get
            {
                return this.relation;
            }
        }
    }
}

