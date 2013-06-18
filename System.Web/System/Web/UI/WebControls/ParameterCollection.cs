namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Reflection;
    using System.Web;
    using System.Web.UI;

    [Editor("System.Web.UI.Design.WebControls.ParameterCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
    public class ParameterCollection : StateManagedCollection
    {
        private static readonly Type[] knownTypes = new Type[] { typeof(ControlParameter), typeof(CookieParameter), typeof(FormParameter), typeof(Parameter), typeof(QueryStringParameter), typeof(SessionParameter), typeof(ProfileParameter) };

        public event EventHandler ParametersChanged;

        public int Add(Parameter parameter)
        {
            return ((IList) this).Add(parameter);
        }

        public int Add(string name, string value)
        {
            return ((IList) this).Add(new Parameter(name, TypeCode.Empty, value));
        }

        public int Add(string name, DbType dbType, string value)
        {
            return ((IList) this).Add(new Parameter(name, dbType, value));
        }

        public int Add(string name, TypeCode type, string value)
        {
            return ((IList) this).Add(new Parameter(name, type, value));
        }

        internal void CallOnParametersChanged()
        {
            this.OnParametersChanged(EventArgs.Empty);
        }

        public bool Contains(Parameter parameter)
        {
            return ((IList) this).Contains(parameter);
        }

        public void CopyTo(Parameter[] parameterArray, int index)
        {
            base.CopyTo(parameterArray, index);
        }

        protected override object CreateKnownType(int index)
        {
            switch (index)
            {
                case 0:
                    return new ControlParameter();

                case 1:
                    return new CookieParameter();

                case 2:
                    return new FormParameter();

                case 3:
                    return new Parameter();

                case 4:
                    return new QueryStringParameter();

                case 5:
                    return new SessionParameter();

                case 6:
                    return new ProfileParameter();
            }
            throw new ArgumentOutOfRangeException("index");
        }

        protected override Type[] GetKnownTypes()
        {
            return knownTypes;
        }

        private int GetParameterIndex(string name)
        {
            for (int i = 0; i < base.Count; i++)
            {
                if (string.Equals(this[i].Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
            return -1;
        }

        public IOrderedDictionary GetValues(HttpContext context, Control control)
        {
            this.UpdateValues(context, control);
            IOrderedDictionary dictionary = new OrderedDictionary();
            foreach (Parameter parameter in this)
            {
                string name = parameter.Name;
                for (int i = 1; dictionary.Contains(name); i++)
                {
                    name = parameter.Name + i.ToString(CultureInfo.InvariantCulture);
                }
                dictionary.Add(name, parameter.ParameterValue);
            }
            return dictionary;
        }

        public int IndexOf(Parameter parameter)
        {
            return ((IList) this).IndexOf(parameter);
        }

        public void Insert(int index, Parameter parameter)
        {
            ((IList) this).Insert(index, parameter);
        }

        protected override void OnClearComplete()
        {
            base.OnClearComplete();
            this.OnParametersChanged(EventArgs.Empty);
        }

        protected override void OnInsert(int index, object value)
        {
            base.OnInsert(index, value);
            ((Parameter) value).SetOwner(this);
        }

        protected override void OnInsertComplete(int index, object value)
        {
            base.OnInsertComplete(index, value);
            this.OnParametersChanged(EventArgs.Empty);
        }

        protected virtual void OnParametersChanged(EventArgs e)
        {
            if (this._parametersChangedHandler != null)
            {
                this._parametersChangedHandler(this, e);
            }
        }

        protected override void OnRemoveComplete(int index, object value)
        {
            base.OnRemoveComplete(index, value);
            ((Parameter) value).SetOwner(null);
            this.OnParametersChanged(EventArgs.Empty);
        }

        protected override void OnValidate(object o)
        {
            base.OnValidate(o);
            if (!(o is Parameter))
            {
                throw new ArgumentException(System.Web.SR.GetString("ParameterCollection_NotParameter"), "o");
            }
        }

        public void Remove(Parameter parameter)
        {
            ((IList) this).Remove(parameter);
        }

        public void RemoveAt(int index)
        {
            ((IList) this).RemoveAt(index);
        }

        protected override void SetDirtyObject(object o)
        {
            ((Parameter) o).SetDirty();
        }

        public void UpdateValues(HttpContext context, Control control)
        {
            foreach (Parameter parameter in this)
            {
                parameter.UpdateValue(context, control);
            }
        }

        public Parameter this[int index]
        {
            get
            {
                return (Parameter) this[index];
            }
            set
            {
                this[index] = value;
            }
        }

        public Parameter this[string name]
        {
            get
            {
                int parameterIndex = this.GetParameterIndex(name);
                if (parameterIndex == -1)
                {
                    return null;
                }
                return this[parameterIndex];
            }
            set
            {
                int parameterIndex = this.GetParameterIndex(name);
                if (parameterIndex == -1)
                {
                    this.Add(value);
                }
                else
                {
                    this[parameterIndex] = value;
                }
            }
        }
    }
}

