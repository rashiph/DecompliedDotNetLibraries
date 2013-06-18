namespace System.Runtime.Serialization
{
    using System;
    using System.Runtime;

    internal class IntRef
    {
        private int value;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public IntRef(int value)
        {
            this.value = value;
        }

        public int Value
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.value;
            }
        }
    }
}

