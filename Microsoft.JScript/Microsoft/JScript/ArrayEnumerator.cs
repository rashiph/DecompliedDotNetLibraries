namespace Microsoft.JScript
{
    using System;
    using System.Collections;
    using System.Globalization;

    internal class ArrayEnumerator : IEnumerator
    {
        private ArrayObject arrayOb;
        private int curr = -1;
        private IEnumerator denseEnum;
        private bool didDenseEnum = false;
        private bool doDenseEnum = false;

        internal ArrayEnumerator(ArrayObject arrayOb, IEnumerator denseEnum)
        {
            this.arrayOb = arrayOb;
            this.denseEnum = denseEnum;
        }

        public virtual bool MoveNext()
        {
            if (this.doDenseEnum)
            {
                if (this.denseEnum.MoveNext())
                {
                    return true;
                }
                this.doDenseEnum = false;
                this.didDenseEnum = true;
            }
            int num = this.curr + 1;
            if ((num >= this.arrayOb.len) || (num >= this.arrayOb.denseArrayLength))
            {
                this.doDenseEnum = !this.didDenseEnum;
                return this.denseEnum.MoveNext();
            }
            this.curr = num;
            if (this.arrayOb.GetValueAtIndex((uint) num) is Missing)
            {
                return this.MoveNext();
            }
            return true;
        }

        public virtual void Reset()
        {
            this.curr = -1;
            this.doDenseEnum = false;
            this.didDenseEnum = false;
            this.denseEnum.Reset();
        }

        public virtual object Current
        {
            get
            {
                if (!this.doDenseEnum && ((this.curr < this.arrayOb.len) && (this.curr < this.arrayOb.denseArrayLength)))
                {
                    return this.curr.ToString(CultureInfo.InvariantCulture);
                }
                return this.denseEnum.Current;
            }
        }
    }
}

