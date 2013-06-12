namespace System.Xml
{
    using System;
    using System.Collections;

    internal sealed class EmptyEnumerator : IEnumerator
    {
        bool IEnumerator.MoveNext()
        {
            return false;
        }

        void IEnumerator.Reset()
        {
        }

        object IEnumerator.Current
        {
            get
            {
                throw new InvalidOperationException(Res.GetString("Xml_InvalidOperation"));
            }
        }
    }
}

