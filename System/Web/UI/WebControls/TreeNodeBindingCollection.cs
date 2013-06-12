namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Web.UI;

    public sealed class TreeNodeBindingCollection : StateManagedCollection
    {
        private TreeNodeBinding _defaultBinding;
        private static readonly Type[] knownTypes = new Type[] { typeof(TreeNodeBinding) };

        internal TreeNodeBindingCollection()
        {
        }

        public int Add(TreeNodeBinding binding)
        {
            return ((IList) this).Add(binding);
        }

        public bool Contains(TreeNodeBinding binding)
        {
            return ((IList) this).Contains(binding);
        }

        public void CopyTo(TreeNodeBinding[] bindingArray, int index)
        {
            base.CopyTo(bindingArray, index);
        }

        protected override object CreateKnownType(int index)
        {
            return new TreeNodeBinding();
        }

        private void FindDefaultBinding()
        {
            this._defaultBinding = null;
            foreach (TreeNodeBinding binding in this)
            {
                if ((binding.Depth == -1) && (binding.DataMember.Length == 0))
                {
                    this._defaultBinding = binding;
                    break;
                }
            }
        }

        internal TreeNodeBinding GetBinding(string dataMember, int depth)
        {
            TreeNodeBinding binding = null;
            int num = 0;
            if ((dataMember != null) && (dataMember.Length == 0))
            {
                dataMember = null;
            }
            foreach (TreeNodeBinding binding2 in this)
            {
                if (binding2.Depth == depth)
                {
                    if (string.Equals(binding2.DataMember, dataMember, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return binding2;
                    }
                    if ((num < 1) && (binding2.DataMember.Length == 0))
                    {
                        binding = binding2;
                        num = 1;
                    }
                }
                else if ((string.Equals(binding2.DataMember, dataMember, StringComparison.CurrentCultureIgnoreCase) && (num < 2)) && (binding2.Depth == -1))
                {
                    binding = binding2;
                    num = 2;
                }
            }
            if ((binding != null) || (this._defaultBinding == null))
            {
                return binding;
            }
            if ((this._defaultBinding.Depth != -1) || (this._defaultBinding.DataMember.Length != 0))
            {
                this.FindDefaultBinding();
            }
            return this._defaultBinding;
        }

        protected override Type[] GetKnownTypes()
        {
            return knownTypes;
        }

        public int IndexOf(TreeNodeBinding binding)
        {
            return ((IList) this).IndexOf(binding);
        }

        public void Insert(int index, TreeNodeBinding binding)
        {
            ((IList) this).Insert(index, binding);
        }

        protected override void OnClear()
        {
            base.OnClear();
            this._defaultBinding = null;
        }

        protected override void OnRemoveComplete(int index, object value)
        {
            if (value == this._defaultBinding)
            {
                this.FindDefaultBinding();
            }
        }

        protected override void OnValidate(object value)
        {
            base.OnValidate(value);
            TreeNodeBinding binding = value as TreeNodeBinding;
            if (((binding != null) && (binding.DataMember.Length == 0)) && (binding.Depth == -1))
            {
                this._defaultBinding = binding;
            }
        }

        public void Remove(TreeNodeBinding binding)
        {
            ((IList) this).Remove(binding);
        }

        public void RemoveAt(int index)
        {
            ((IList) this).RemoveAt(index);
        }

        protected override void SetDirtyObject(object o)
        {
            if (o is TreeNodeBinding)
            {
                ((TreeNodeBinding) o).SetDirty();
            }
        }

        public TreeNodeBinding this[int i]
        {
            get
            {
                return (TreeNodeBinding) this[i];
            }
            set
            {
                this[i] = value;
            }
        }
    }
}

