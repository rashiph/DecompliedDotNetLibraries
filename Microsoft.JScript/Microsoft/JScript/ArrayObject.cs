namespace Microsoft.JScript
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;

    public class ArrayObject : JSObject
    {
        internal object[] denseArray;
        internal uint denseArrayLength;
        internal uint len;
        internal const int MaxIndex = 0x186a0;
        internal const int MinDenseSize = 0x80;

        internal ArrayObject(ScriptObject prototype) : base(prototype)
        {
            this.len = 0;
            this.denseArray = null;
            this.denseArrayLength = 0;
            base.noExpando = false;
        }

        internal ArrayObject(ScriptObject prototype, Type subType) : base(prototype, subType)
        {
            this.len = 0;
            this.denseArray = null;
            this.denseArrayLength = 0;
            base.noExpando = false;
        }

        internal static long Array_index_for(object index)
        {
            if (index is int)
            {
                return (long) ((int) index);
            }
            IConvertible iConvertible = Microsoft.JScript.Convert.GetIConvertible(index);
            switch (Microsoft.JScript.Convert.GetTypeCode(index, iConvertible))
            {
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                {
                    double num = iConvertible.ToDouble(null);
                    long num2 = (long) num;
                    if ((num2 < 0L) || (num2 != num))
                    {
                        break;
                    }
                    return num2;
                }
            }
            return -1L;
        }

        internal static long Array_index_for(string name)
        {
            int length = name.Length;
            if (length <= 0)
            {
                return -1L;
            }
            char ch = name[0];
            if ((ch < '1') || (ch > '9'))
            {
                if ((ch == '0') && (length == 1))
                {
                    return 0L;
                }
                return -1L;
            }
            long num2 = ch - '0';
            for (int i = 1; i < length; i++)
            {
                ch = name[i];
                if ((ch < '0') || (ch > '9'))
                {
                    return -1L;
                }
                num2 = (num2 * 10L) + (ch - '0');
                if (num2 > 0xffffffffL)
                {
                    return -1L;
                }
            }
            return num2;
        }

        internal virtual void Concat(ArrayObject source)
        {
            uint len = source.len;
            if (len != 0)
            {
                uint num2 = this.len;
                this.SetLength(num2 + len);
                uint denseArrayLength = len;
                if (!(source is ArrayWrapper) && (len > source.denseArrayLength))
                {
                    denseArrayLength = source.denseArrayLength;
                }
                uint num4 = num2;
                for (uint i = 0; i < denseArrayLength; i++)
                {
                    this.SetValueAtIndex(num4++, source.GetValueAtIndex(i));
                }
                if (denseArrayLength != len)
                {
                    IDictionaryEnumerator enumerator = source.NameTable.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        long num6 = Array_index_for(enumerator.Key.ToString());
                        if (num6 >= 0L)
                        {
                            this.SetValueAtIndex(num2 + ((uint) num6), ((JSField) enumerator.Value).GetValue(null));
                        }
                    }
                }
            }
        }

        internal virtual void Concat(object value)
        {
            Array array = value as Array;
            if ((array != null) && (array.Rank == 1))
            {
                this.Concat((ArrayObject) new ArrayWrapper(ArrayPrototype.ob, array, true));
            }
            else
            {
                uint len = this.len;
                this.SetLength((ulong) (1L + len));
                this.SetValueAtIndex(len, value);
            }
        }

        internal static void Copy(object[] source, object[] target, int n)
        {
            Copy(source, 0, target, 0, n);
        }

        internal static void Copy(object[] source, int i, object[] target, int j, int n)
        {
            if (i < j)
            {
                for (int k = n - 1; k >= 0; k--)
                {
                    target[j + k] = source[i + k];
                }
            }
            else
            {
                for (int m = 0; m < n; m++)
                {
                    target[j + m] = source[i + m];
                }
            }
        }

        internal DebugArrayFieldEnumerator DebugGetEnumerator()
        {
            return new DebugArrayFieldEnumerator(new ScriptObjectPropertyEnumerator(this), this);
        }

        internal object DebugGetValueAtIndex(int index)
        {
            return this.GetValueAtIndex((uint) index);
        }

        internal void DebugSetValueAtIndex(int index, object value)
        {
            this.SetValueAtIndex((uint) index, value);
        }

        internal override bool DeleteMember(string name)
        {
            long num = Array_index_for(name);
            if (num >= 0L)
            {
                return this.DeleteValueAtIndex((uint) num);
            }
            return base.DeleteMember(name);
        }

        private void DeleteRange(uint start, uint end)
        {
            uint denseArrayLength = this.denseArrayLength;
            if (denseArrayLength > end)
            {
                denseArrayLength = end;
            }
            while (start < denseArrayLength)
            {
                this.denseArray[start] = Missing.Value;
                start++;
            }
            if (denseArrayLength != end)
            {
                IDictionaryEnumerator enumerator = base.NameTable.GetEnumerator();
                ArrayList list = new ArrayList(base.name_table.count);
                while (enumerator.MoveNext())
                {
                    long num2 = Array_index_for(enumerator.Key.ToString());
                    if ((num2 >= start) && (num2 <= end))
                    {
                        list.Add(enumerator.Key);
                    }
                }
                IEnumerator enumerator2 = list.GetEnumerator();
                while (enumerator2.MoveNext())
                {
                    this.DeleteMember((string) enumerator2.Current);
                }
            }
        }

        internal virtual bool DeleteValueAtIndex(uint index)
        {
            if (index >= this.denseArrayLength)
            {
                return base.DeleteMember(index.ToString(CultureInfo.InvariantCulture));
            }
            if (this.denseArray[index] is Missing)
            {
                return false;
            }
            this.denseArray[index] = Missing.Value;
            return true;
        }

        internal override string GetClassName()
        {
            return "Array";
        }

        internal override object GetDefaultValue(PreferredType preferred_type)
        {
            if (base.GetParent() is LenientArrayPrototype)
            {
                return base.GetDefaultValue(preferred_type);
            }
            if (preferred_type == PreferredType.String)
            {
                if (!base.noExpando && (base.NameTable["toString"] != null))
                {
                    return base.GetDefaultValue(preferred_type);
                }
                return ArrayPrototype.toString(this);
            }
            if (preferred_type == PreferredType.LocaleString)
            {
                if (!base.noExpando && (base.NameTable["toLocaleString"] != null))
                {
                    return base.GetDefaultValue(preferred_type);
                }
                return ArrayPrototype.toLocaleString(this);
            }
            if (!base.noExpando)
            {
                object obj4 = base.NameTable["valueOf"];
                if ((obj4 == null) && (preferred_type == PreferredType.Either))
                {
                    obj4 = base.NameTable["toString"];
                }
                if (obj4 != null)
                {
                    return base.GetDefaultValue(preferred_type);
                }
            }
            return ArrayPrototype.toString(this);
        }

        [DebuggerHidden, DebuggerStepThrough]
        internal override object GetMemberValue(string name)
        {
            long num = Array_index_for(name);
            if (num < 0L)
            {
                return base.GetMemberValue(name);
            }
            return this.GetValueAtIndex((uint) num);
        }

        internal override void GetPropertyEnumerator(ArrayList enums, ArrayList objects)
        {
            if (base.field_table == null)
            {
                base.field_table = new ArrayList();
            }
            enums.Add(new Microsoft.JScript.ArrayEnumerator(this, new ListEnumerator(base.field_table)));
            objects.Add(this);
            if (base.parent != null)
            {
                base.parent.GetPropertyEnumerator(enums, objects);
            }
        }

        internal override object GetValueAtIndex(uint index)
        {
            if (index < this.denseArrayLength)
            {
                object obj2 = this.denseArray[index];
                if (obj2 != Missing.Value)
                {
                    return obj2;
                }
            }
            return base.GetValueAtIndex(index);
        }

        private void Realloc(uint newLength)
        {
            uint denseArrayLength = this.denseArrayLength;
            uint num2 = denseArrayLength * 2;
            if (num2 < newLength)
            {
                num2 = newLength;
            }
            object[] target = new object[num2];
            if (denseArrayLength > 0)
            {
                Copy(this.denseArray, target, (int) denseArrayLength);
            }
            for (int i = (int) denseArrayLength; i < num2; i++)
            {
                target[i] = Missing.Value;
            }
            this.denseArray = target;
            this.denseArrayLength = num2;
        }

        private void SetLength(ulong newLength)
        {
            uint len = this.len;
            if (newLength < len)
            {
                this.DeleteRange((uint) newLength, len);
            }
            else
            {
                if (newLength > 0xffffffffL)
                {
                    throw new JScriptException(JSError.ArrayLengthAssignIncorrect);
                }
                if ((((newLength > this.denseArrayLength) && (len <= this.denseArrayLength)) && (newLength <= 0x186a0L)) && ((newLength <= 0x80L) || (newLength <= (len * 2))))
                {
                    this.Realloc((uint) newLength);
                }
            }
            this.len = (uint) newLength;
        }

        [DebuggerHidden, DebuggerStepThrough]
        internal override void SetMemberValue(string name, object value)
        {
            if (name.Equals("length"))
            {
                this.length = value;
            }
            else
            {
                long num = Array_index_for(name);
                if (num < 0L)
                {
                    base.SetMemberValue(name, value);
                }
                else
                {
                    this.SetValueAtIndex((uint) num, value);
                }
            }
        }

        internal override void SetValueAtIndex(uint index, object value)
        {
            if ((index >= this.len) && (index < uint.MaxValue))
            {
                this.SetLength((ulong) (index + 1));
            }
            if (index < this.denseArrayLength)
            {
                this.denseArray[index] = value;
            }
            else
            {
                base.SetMemberValue(index.ToString(CultureInfo.InvariantCulture), value);
            }
        }

        internal virtual object Shift()
        {
            object valueAtIndex = null;
            uint len = this.len;
            if (len != 0)
            {
                uint num2 = (this.denseArrayLength >= len) ? len : this.denseArrayLength;
                if (num2 > 0)
                {
                    valueAtIndex = this.denseArray[0];
                    Copy(this.denseArray, 1, this.denseArray, 0, ((int) num2) - 1);
                }
                else
                {
                    valueAtIndex = base.GetValueAtIndex(0);
                }
                for (uint i = num2; i < len; i++)
                {
                    this.SetValueAtIndex(i - 1, this.GetValueAtIndex(i));
                }
                this.SetValueAtIndex(len - 1, Missing.Value);
                this.SetLength((ulong) (len - 1));
                if (valueAtIndex is Missing)
                {
                    return null;
                }
            }
            return valueAtIndex;
        }

        internal virtual void Sort(ScriptFunction compareFn)
        {
            QuickSort sort = new QuickSort(this, compareFn);
            uint len = this.len;
            if (len <= this.denseArrayLength)
            {
                sort.SortArray(0, ((int) len) - 1);
            }
            else
            {
                sort.SortObject(0L, (long) (len - 1));
            }
        }

        internal virtual void Splice(uint start, uint deleteCount, object[] args, ArrayObject outArray, uint oldLength, uint newLength)
        {
            if (oldLength > this.denseArrayLength)
            {
                this.SpliceSlowly(start, deleteCount, args, outArray, oldLength, newLength);
            }
            else
            {
                if (newLength > oldLength)
                {
                    this.SetLength((ulong) newLength);
                    if (newLength > this.denseArrayLength)
                    {
                        this.SpliceSlowly(start, deleteCount, args, outArray, oldLength, newLength);
                        return;
                    }
                }
                if (deleteCount > oldLength)
                {
                    deleteCount = oldLength;
                }
                if (deleteCount > 0)
                {
                    Copy(this.denseArray, (int) start, outArray.denseArray, 0, (int) deleteCount);
                }
                if (oldLength > 0)
                {
                    Copy(this.denseArray, (int) (start + deleteCount), this.denseArray, ((int) start) + args.Length, (int) ((oldLength - start) - deleteCount));
                }
                if (args != null)
                {
                    int length = args.Length;
                    if (length > 0)
                    {
                        Copy(args, 0, this.denseArray, (int) start, length);
                    }
                    if (length < deleteCount)
                    {
                        this.SetLength((ulong) newLength);
                    }
                }
                else if (deleteCount > 0)
                {
                    this.SetLength((ulong) newLength);
                }
            }
        }

        protected void SpliceSlowly(uint start, uint deleteCount, object[] args, ArrayObject outArray, uint oldLength, uint newLength)
        {
            for (uint i = 0; i < deleteCount; i++)
            {
                outArray.SetValueAtIndex(i, this.GetValueAtIndex(i + start));
            }
            uint num2 = (oldLength - start) - deleteCount;
            if (newLength < oldLength)
            {
                for (uint k = 0; k < num2; k++)
                {
                    this.SetValueAtIndex((k + start) + ((uint) args.Length), this.GetValueAtIndex((k + start) + deleteCount));
                }
                this.SetLength((ulong) newLength);
            }
            else
            {
                if (newLength > oldLength)
                {
                    this.SetLength((ulong) newLength);
                }
                for (uint m = num2; m > 0; m--)
                {
                    this.SetValueAtIndex((uint) (((m + start) + args.Length) - 1), this.GetValueAtIndex(((m + start) + deleteCount) - 1));
                }
            }
            int num5 = (args == null) ? 0 : args.Length;
            for (uint j = 0; j < num5; j++)
            {
                this.SetValueAtIndex(j + start, args[j]);
            }
        }

        internal override void SwapValues(uint pi, uint qi)
        {
            if (pi > qi)
            {
                this.SwapValues(qi, pi);
            }
            else if (pi >= this.denseArrayLength)
            {
                base.SwapValues(pi, qi);
            }
            else
            {
                object obj2 = this.denseArray[pi];
                this.denseArray[pi] = this.GetValueAtIndex(qi);
                if (obj2 == Missing.Value)
                {
                    this.DeleteValueAtIndex(qi);
                }
                else
                {
                    this.SetValueAtIndex(qi, obj2);
                }
            }
        }

        internal virtual object[] ToArray()
        {
            int len = (int) this.len;
            if (len == 0)
            {
                return new object[0];
            }
            if (len == this.denseArrayLength)
            {
                return this.denseArray;
            }
            if (len < this.denseArrayLength)
            {
                object[] objArray = new object[len];
                Copy(this.denseArray, 0, objArray, 0, len);
                return objArray;
            }
            object[] target = new object[len];
            Copy(this.denseArray, 0, target, 0, (int) this.denseArrayLength);
            for (uint i = this.denseArrayLength; i < len; i++)
            {
                target[i] = this.GetValueAtIndex(i);
            }
            return target;
        }

        internal virtual Array ToNativeArray(Type elementType)
        {
            uint len = this.len;
            if (len > 0x7fffffff)
            {
                throw new JScriptException(JSError.OutOfMemory);
            }
            if (elementType == null)
            {
                elementType = typeof(object);
            }
            uint denseArrayLength = this.denseArrayLength;
            if (denseArrayLength > len)
            {
                denseArrayLength = len;
            }
            Array array = Array.CreateInstance(elementType, (int) len);
            for (int i = 0; i < denseArrayLength; i++)
            {
                array.SetValue(Microsoft.JScript.Convert.CoerceT(this.denseArray[i], elementType), i);
            }
            for (int j = (int) denseArrayLength; j < len; j++)
            {
                array.SetValue(Microsoft.JScript.Convert.CoerceT(this.GetValueAtIndex((uint) j), elementType), j);
            }
            return array;
        }

        internal virtual ArrayObject Unshift(object[] args)
        {
            uint len = this.len;
            int length = args.Length;
            ulong newLength = (ulong) (len + length);
            this.SetLength(newLength);
            if (newLength <= this.denseArrayLength)
            {
                for (int i = ((int) len) - 1; i >= 0; i--)
                {
                    this.denseArray[i + length] = this.denseArray[i];
                }
                Copy(args, 0, this.denseArray, 0, args.Length);
            }
            else
            {
                for (long j = len - 1; j >= 0L; j -= 1L)
                {
                    this.SetValueAtIndex((uint) (j + length), this.GetValueAtIndex((uint) j));
                }
                for (uint k = 0; k < length; k++)
                {
                    this.SetValueAtIndex(k, args[k]);
                }
            }
            return this;
        }

        public virtual object length
        {
            get
            {
                if (this.len < 0x7fffffff)
                {
                    return (int) this.len;
                }
                return (double) this.len;
            }
            set
            {
                IConvertible iConvertible = Microsoft.JScript.Convert.GetIConvertible(value);
                uint num = Microsoft.JScript.Convert.ToUint32(value, iConvertible);
                if (num != Microsoft.JScript.Convert.ToNumber(value, iConvertible))
                {
                    throw new JScriptException(JSError.ArrayLengthAssignIncorrect);
                }
                this.SetLength((ulong) num);
            }
        }
    }
}

