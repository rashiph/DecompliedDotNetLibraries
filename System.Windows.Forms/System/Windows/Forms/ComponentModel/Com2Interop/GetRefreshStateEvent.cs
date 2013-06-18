namespace System.Windows.Forms.ComponentModel.Com2Interop
{
    using System;

    internal class GetRefreshStateEvent : GetBoolValueEvent
    {
        private Com2ShouldRefreshTypes item;

        public GetRefreshStateEvent(Com2ShouldRefreshTypes item, bool defValue) : base(defValue)
        {
            this.item = item;
        }
    }
}

