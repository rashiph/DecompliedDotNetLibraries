namespace System.Web.UI.WebControls
{
    using System;
    using System.Web.UI;

    public interface IRepeatInfoUser
    {
        Style GetItemStyle(ListItemType itemType, int repeatIndex);
        void RenderItem(ListItemType itemType, int repeatIndex, RepeatInfo repeatInfo, HtmlTextWriter writer);

        bool HasFooter { get; }

        bool HasHeader { get; }

        bool HasSeparators { get; }

        int RepeatedItemCount { get; }
    }
}

