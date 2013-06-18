namespace Microsoft.JScript
{
    using System;

    internal sealed class QuickSort
    {
        internal ScriptFunction compareFn;
        internal object obj;

        internal QuickSort(object obj, ScriptFunction compareFn)
        {
            this.compareFn = compareFn;
            this.obj = obj;
        }

        private int Compare(object x, object y)
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

        internal void SortArray(int left, int right)
        {
            object obj4;
            ArrayObject obj2 = (ArrayObject) this.obj;
            if (right <= left)
            {
                return;
            }
            int index = left + ((int) ((right - left) * MathObject.random()));
            object x = obj2.denseArray[index];
            obj2.denseArray[index] = obj2.denseArray[right];
            obj2.denseArray[right] = x;
            int i = left - 1;
            int j = right;
        Label_004B:
            obj4 = obj2.denseArray[++i];
            if ((i < j) && (this.Compare(x, obj4) >= 0))
            {
                goto Label_004B;
            }
            do
            {
                obj4 = obj2.denseArray[--j];
            }
            while ((j > i) && (this.Compare(x, obj4) <= 0));
            if (i < j)
            {
                Swap(obj2.denseArray, i, j);
                goto Label_004B;
            }
            Swap(obj2.denseArray, i, right);
            this.SortArray(left, i - 1);
            this.SortArray(i + 1, right);
        }

        internal void SortObject(long left, long right)
        {
            object obj3;
            if (right <= left)
            {
                return;
            }
            long num = left + ((long) ((right - left) * MathObject.random()));
            LateBinding.SwapValues(this.obj, (uint) num, (uint) right);
            object valueAtIndex = LateBinding.GetValueAtIndex(this.obj, (ulong) right);
            long num2 = left - 1L;
            long num3 = right;
        Label_0039:
            obj3 = LateBinding.GetValueAtIndex(this.obj, (ulong) (num2 += 1L));
            if ((num2 < num3) && (this.Compare(valueAtIndex, obj3) >= 0))
            {
                goto Label_0039;
            }
            do
            {
                obj3 = LateBinding.GetValueAtIndex(this.obj, (ulong) (num3 -= 1L));
            }
            while ((num3 > num2) && (this.Compare(valueAtIndex, obj3) <= 0));
            if (num2 < num3)
            {
                LateBinding.SwapValues(this.obj, (uint) num2, (uint) num3);
                goto Label_0039;
            }
            LateBinding.SwapValues(this.obj, (uint) num2, (uint) right);
            this.SortObject(left, num2 - 1L);
            this.SortObject(num2 + 1L, right);
        }

        private static void Swap(object[] array, int i, int j)
        {
            object obj2 = array[i];
            array[i] = array[j];
            array[j] = obj2;
        }
    }
}

