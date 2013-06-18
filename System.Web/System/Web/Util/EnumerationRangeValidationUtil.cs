namespace System.Web.Util
{
    using System;
    using System.Web.UI.WebControls;

    internal static class EnumerationRangeValidationUtil
    {
        public static void ValidateRepeatLayout(RepeatLayout value)
        {
            if ((value < RepeatLayout.Table) || (value > RepeatLayout.OrderedList))
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }
    }
}

