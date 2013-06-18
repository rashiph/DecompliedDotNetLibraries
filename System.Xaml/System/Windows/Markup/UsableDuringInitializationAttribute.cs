namespace System.Windows.Markup
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=true)]
    public sealed class UsableDuringInitializationAttribute : Attribute
    {
        private bool _usable;

        public UsableDuringInitializationAttribute(bool usable)
        {
            this._usable = usable;
        }

        public bool Usable
        {
            get
            {
                return this._usable;
            }
        }
    }
}

