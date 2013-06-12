namespace System.Web.UI.WebControls
{
    public interface IDataBoundItemControl : IDataBoundControl
    {
        System.Web.UI.WebControls.DataKey DataKey { get; }

        DataBoundControlMode Mode { get; }
    }
}

