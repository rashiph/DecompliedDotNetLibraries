namespace System.Web.UI.WebControls
{
    using System.Web.UI;

    public interface IPostBackContainer
    {
        PostBackOptions GetPostBackOptions(IButtonControl buttonControl);
    }
}

