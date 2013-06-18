namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Collections;

    public class VBArrayObject : JSObject
    {
        private Array array;

        public VBArrayObject(VBArrayPrototype parent, Array array) : base(parent)
        {
            this.array = array;
            base.noExpando = false;
        }

        internal virtual int dimensions()
        {
            return this.array.Rank;
        }

        internal virtual object getItem(object[] args)
        {
            if ((args == null) || (args.Length == 0))
            {
                throw new JScriptException(JSError.TooFewParameters);
            }
            if (args.Length == 1)
            {
                return this.array.GetValue(Microsoft.JScript.Convert.ToInt32(args[0]));
            }
            if (args.Length == 2)
            {
                return this.array.GetValue(Microsoft.JScript.Convert.ToInt32(args[0]), Microsoft.JScript.Convert.ToInt32(args[1]));
            }
            if (args.Length == 3)
            {
                return this.array.GetValue(Microsoft.JScript.Convert.ToInt32(args[0]), Microsoft.JScript.Convert.ToInt32(args[1]), Microsoft.JScript.Convert.ToInt32(args[2]));
            }
            int length = args.Length;
            int[] indices = new int[length];
            for (int i = 0; i < length; i++)
            {
                indices[i] = Microsoft.JScript.Convert.ToInt32(args[i]);
            }
            return this.array.GetValue(indices);
        }

        internal virtual int lbound(object dimension)
        {
            int num = Microsoft.JScript.Convert.ToInt32(dimension);
            return this.array.GetLowerBound(num);
        }

        internal virtual ArrayObject toArray(VsaEngine engine)
        {
            IList array = this.array;
            ArrayObject obj2 = engine.GetOriginalArrayConstructor().Construct();
            uint num = 0;
            int count = array.Count;
            IEnumerator enumerator = array.GetEnumerator();
            obj2.length = count;
            while (enumerator.MoveNext())
            {
                obj2.SetValueAtIndex(num++, enumerator.Current);
            }
            return obj2;
        }

        internal virtual int ubound(object dimension)
        {
            int num = Microsoft.JScript.Convert.ToInt32(dimension);
            return this.array.GetUpperBound(num);
        }
    }
}

