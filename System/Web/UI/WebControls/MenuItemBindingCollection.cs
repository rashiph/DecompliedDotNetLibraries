namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Web.UI;

    public sealed class MenuItemBindingCollection : StateManagedCollection
    {
        private MenuItemBinding _defaultBinding;
        private Menu _owner;
        private static readonly Type[] knownTypes = new Type[] { typeof(MenuItemBinding) };

        private MenuItemBindingCollection()
        {
        }

        internal MenuItemBindingCollection(Menu owner)
        {
            this._owner = owner;
        }

        public int Add(MenuItemBinding binding)
        {
            return ((IList) this).Add(binding);
        }

        public bool Contains(MenuItemBinding binding)
        {
            return ((IList) this).Contains(binding);
        }

        public void CopyTo(MenuItemBinding[] array, int index)
        {
            this.CopyTo(array, index);
        }

        protected override object CreateKnownType(int index)
        {
            return new MenuItemBinding();
        }

        private void FindDefaultBinding()
        {
            this._defaultBinding = null;
            foreach (MenuItemBinding binding in this)
            {
                if ((binding.Depth == -1) && (binding.DataMember.Length == 0))
                {
                    this._defaultBinding = binding;
                    break;
                }
            }
        }

        internal MenuItemBinding GetBinding(string dataMember, int depth)
        {
            MenuItemBinding binding = null;
            int num = 0;
            if ((dataMember != null) && (dataMember.Length == 0))
            {
                dataMember = null;
            }
            foreach (MenuItemBinding binding2 in this)
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

        public int IndexOf(MenuItemBinding value)
        {
            return ((IList) this).IndexOf(value);
        }

        public void Insert(int index, MenuItemBinding binding)
        {
            ((IList) this).Insert(index, binding);
        }

        protected override void OnClear()
        {
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
            MenuItemBinding binding = value as MenuItemBinding;
            if (((binding != null) && (binding.DataMember.Length == 0)) && (binding.Depth == -1))
            {
                this._defaultBinding = binding;
            }
        }

        public void Remove(MenuItemBinding binding)
        {
            ((IList) this).Remove(binding);
        }

        public void RemoveAt(int index)
        {
            ((IList) this).RemoveAt(index);
        }

        protected override void SetDirtyObject(object o)
        {
            if (o is MenuItemBinding)
            {
                ((MenuItemBinding) o).SetDirty();
            }
        }

        public MenuItemBinding this[int i]
        {
            get
            {
                return (MenuItemBinding) this[i];
            }
            set
            {
                this[i] = value;
            }
        }
    }
}

