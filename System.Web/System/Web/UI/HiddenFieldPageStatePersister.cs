namespace System.Web.UI
{
    using System;

    public class HiddenFieldPageStatePersister : PageStatePersister
    {
        public HiddenFieldPageStatePersister(Page page) : base(page)
        {
        }

        public override void Load()
        {
            if (base.Page.RequestValueCollection != null)
            {
                string requestViewStateString = null;
                try
                {
                    requestViewStateString = base.Page.RequestViewStateString;
                    if (!string.IsNullOrEmpty(requestViewStateString) || !string.IsNullOrEmpty(base.Page.ViewStateUserKey))
                    {
                        Pair pair = (Pair) Util.DeserializeWithAssert(base.StateFormatter, requestViewStateString);
                        base.ViewState = pair.First;
                        base.ControlState = pair.Second;
                    }
                }
                catch (Exception exception)
                {
                    if (exception.InnerException is ViewStateException)
                    {
                        throw;
                    }
                    ViewStateException.ThrowViewStateError(exception, requestViewStateString);
                }
            }
        }

        public override void Save()
        {
            if ((base.ViewState != null) || (base.ControlState != null))
            {
                base.Page.ClientState = Util.SerializeWithAssert(base.StateFormatter, new Pair(base.ViewState, base.ControlState));
            }
        }
    }
}

