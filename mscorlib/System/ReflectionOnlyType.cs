namespace System
{
    [Serializable]
    internal class ReflectionOnlyType : RuntimeType
    {
        private ReflectionOnlyType()
        {
        }

        public override RuntimeTypeHandle TypeHandle
        {
            get
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotAllowedInReflectionOnly"));
            }
        }
    }
}

