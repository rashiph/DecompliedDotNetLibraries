namespace System.Windows.Forms
{
    using System;
    using System.Globalization;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public sealed class HtmlHistory : IDisposable
    {
        private bool disposed;
        private System.Windows.Forms.UnsafeNativeMethods.IOmHistory htmlHistory;

        [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        internal HtmlHistory(System.Windows.Forms.UnsafeNativeMethods.IOmHistory history)
        {
            this.htmlHistory = history;
        }

        public void Back(int numberBack)
        {
            if (numberBack < 0)
            {
                object[] args = new object[] { "numberBack", numberBack.ToString(CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture) };
                throw new ArgumentOutOfRangeException("numberBack", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", args));
            }
            if (numberBack > 0)
            {
                object pvargdistance = -numberBack;
                this.NativeOmHistory.Go(ref pvargdistance);
            }
        }

        public void Dispose()
        {
            this.htmlHistory = null;
            this.disposed = true;
            GC.SuppressFinalize(this);
        }

        public void Forward(int numberForward)
        {
            if (numberForward < 0)
            {
                object[] args = new object[] { "numberForward", numberForward.ToString(CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture) };
                throw new ArgumentOutOfRangeException("numberForward", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", args));
            }
            if (numberForward > 0)
            {
                object pvargdistance = numberForward;
                this.NativeOmHistory.Go(ref pvargdistance);
            }
        }

        public void Go(int relativePosition)
        {
            object pvargdistance = relativePosition;
            this.NativeOmHistory.Go(ref pvargdistance);
        }

        public void Go(string urlString)
        {
            object pvargdistance = urlString;
            this.NativeOmHistory.Go(ref pvargdistance);
        }

        public void Go(Uri url)
        {
            this.Go(url.ToString());
        }

        public object DomHistory
        {
            get
            {
                return this.NativeOmHistory;
            }
        }

        public int Length
        {
            get
            {
                return this.NativeOmHistory.GetLength();
            }
        }

        private System.Windows.Forms.UnsafeNativeMethods.IOmHistory NativeOmHistory
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                return this.htmlHistory;
            }
        }
    }
}

