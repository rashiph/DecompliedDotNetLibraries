namespace System.Reflection
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential), ComVisible(true)]
    public struct ParameterModifier
    {
        private bool[] _byRef;
        public ParameterModifier(int parameterCount)
        {
            if (parameterCount <= 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_ParmArraySize"));
            }
            this._byRef = new bool[parameterCount];
        }

        internal bool[] IsByRefArray
        {
            get
            {
                return this._byRef;
            }
        }
        public bool this[int index]
        {
            get
            {
                return this._byRef[index];
            }
            set
            {
                this._byRef[index] = value;
            }
        }
    }
}

