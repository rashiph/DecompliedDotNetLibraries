namespace System.Web.UI
{
    using System.Collections;

    public interface IAutoFieldGenerator
    {
        ICollection GenerateFields(Control control);
    }
}

