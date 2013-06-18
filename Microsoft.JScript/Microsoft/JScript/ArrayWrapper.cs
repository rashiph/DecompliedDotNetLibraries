namespace Microsoft.JScript
{
    using System;
    using System.Collections;
    using System.Diagnostics;

    public class ArrayWrapper : ArrayObject
    {
        private bool implicitWrapper;
        internal Array value;

        internal ArrayWrapper(ScriptObject prototype, Array value, bool implicitWrapper) : base(prototype, typeof(ArrayWrapper))
        {
            this.value = value;
            this.implicitWrapper = implicitWrapper;
            if (value != null)
            {
                if (value.Rank != 1)
                {
                    throw new JScriptException(JSError.TypeMismatch);
                }
                base.len = (uint) value.Length;
            }
            else
            {
                base.len = 0;
            }
        }

        internal override void Concat(ArrayObject source)
        {
            throw new JScriptException(JSError.ActionNotSupported);
        }

        internal override void Concat(object value)
        {
            throw new JScriptException(JSError.ActionNotSupported);
        }

        internal override void GetPropertyEnumerator(ArrayList enums, ArrayList objects)
        {
            enums.Add(new Microsoft.JScript.ArrayEnumerator(this, new RangeEnumerator(0, ((int) base.len) - 1)));
            objects.Add(this);
            if (base.parent != null)
            {
                base.parent.GetPropertyEnumerator(enums, objects);
            }
        }

        public Type GetType()
        {
            if (!this.implicitWrapper)
            {
                return typeof(ArrayObject);
            }
            return this.value.GetType();
        }

        internal override object GetValueAtIndex(uint index)
        {
            return this.value.GetValue((int) index);
        }

        [DebuggerHidden, DebuggerStepThrough]
        internal override void SetMemberValue(string name, object val)
        {
            if (name.Equals("length"))
            {
                throw new JScriptException(JSError.AssignmentToReadOnly);
            }
            long num = ArrayObject.Array_index_for(name);
            if (num < 0L)
            {
                base.SetMemberValue(name, val);
            }
            else
            {
                this.value.SetValue(val, (int) num);
            }
        }

        internal override void SetValueAtIndex(uint index, object val)
        {
            Type type = this.value.GetType();
            this.value.SetValue(Microsoft.JScript.Convert.CoerceT(val, type.GetElementType()), (int) index);
        }

        internal override object Shift()
        {
            throw new JScriptException(JSError.ActionNotSupported);
        }

        internal override void Sort(ScriptFunction compareFn)
        {
            SortComparer comparer = new SortComparer(compareFn);
            Array.Sort(this.value, comparer);
        }

        internal override void Splice(uint start, uint deleteCount, object[] args, ArrayObject outArray, uint oldLength, uint newLength)
        {
            if (oldLength != newLength)
            {
                throw new JScriptException(JSError.ActionNotSupported);
            }
            base.SpliceSlowly(start, deleteCount, args, outArray, oldLength, newLength);
        }

        internal override void SwapValues(uint pi, uint qi)
        {
            object valueAtIndex = this.GetValueAtIndex(pi);
            object obj3 = this.GetValueAtIndex(qi);
            this.SetValueAtIndex(pi, obj3);
            this.SetValueAtIndex(qi, valueAtIndex);
        }

        internal override object[] ToArray()
        {
            object[] objArray = new object[base.len];
            for (uint i = 0; i < base.len; i++)
            {
                objArray[i] = this.GetValueAtIndex(i);
            }
            return objArray;
        }

        internal override Array ToNativeArray(Type elementType)
        {
            return this.value;
        }

        internal override ArrayObject Unshift(object[] args)
        {
            throw new JScriptException(JSError.ActionNotSupported);
        }

        public override object length
        {
            get
            {
                return base.len;
            }
            set
            {
                throw new JScriptException(JSError.AssignmentToReadOnly);
            }
        }

        internal sealed class SortComparer : IComparer
        {
            internal ScriptFunction compareFn;

            internal SortComparer(ScriptFunction compareFn)
            {
                this.compareFn = compareFn;
            }

            public int Compare(object x, object y)
            {
                if ((x == null) || (x is Missing))
                {
                    if ((y != null) && !(y is Missing))
                    {
                        return 1;
                    }
                    return 0;
                }
                if ((y == null) || (y is Missing))
                {
                    return -1;
                }
                if (this.compareFn == null)
                {
                    return string.CompareOrdinal(Microsoft.JScript.Convert.ToString(x), Microsoft.JScript.Convert.ToString(y));
                }
                double val = Microsoft.JScript.Convert.ToNumber(this.compareFn.Call(new object[] { x, y }, null));
                if (val != val)
                {
                    throw new JScriptException(JSError.NumberExpected);
                }
                return (int) Runtime.DoubleToInt64(val);
            }
        }
    }
}

