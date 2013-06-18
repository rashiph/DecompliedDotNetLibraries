namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.SessionState;

    public class SessionPageStatePersister : PageStatePersister
    {
        private const string _viewStateQueueKey = "__VIEWSTATEQUEUE";
        private const string _viewStateSessionKey = "__SESSIONVIEWSTATE";

        public SessionPageStatePersister(Page page) : base(page)
        {
            HttpSessionState session = null;
            try
            {
                session = page.Session;
            }
            catch
            {
            }
            if (session == null)
            {
                throw new ArgumentException(System.Web.SR.GetString("SessionPageStatePersister_SessionMustBeEnabled"));
            }
        }

        public override void Load()
        {
            if (base.Page.RequestValueCollection != null)
            {
                try
                {
                    string requestViewStateString = base.Page.RequestViewStateString;
                    string second = null;
                    bool flag = false;
                    if (!string.IsNullOrEmpty(requestViewStateString))
                    {
                        Pair pair = (Pair) Util.DeserializeWithAssert(base.StateFormatter, requestViewStateString);
                        if ((bool) pair.First)
                        {
                            second = (string) pair.Second;
                            flag = true;
                        }
                        else
                        {
                            Pair pair2 = (Pair) pair.Second;
                            second = (string) pair2.First;
                            base.ControlState = pair2.Second;
                        }
                    }
                    if (second != null)
                    {
                        object obj2 = base.Page.Session["__SESSIONVIEWSTATE" + second];
                        if (flag)
                        {
                            Pair pair3 = obj2 as Pair;
                            if (pair3 != null)
                            {
                                base.ViewState = pair3.First;
                                base.ControlState = pair3.Second;
                            }
                        }
                        else
                        {
                            base.ViewState = obj2;
                        }
                    }
                }
                catch (Exception exception)
                {
                    HttpException e = new HttpException(System.Web.SR.GetString("Invalid_ControlState"), exception);
                    e.SetFormatter(new UseLastUnhandledErrorFormatter(e));
                    throw e;
                }
            }
        }

        public override void Save()
        {
            bool x = false;
            object y = null;
            Triplet viewState = base.ViewState as Triplet;
            if ((base.ControlState != null) || ((((viewState == null) || (viewState.Second != null)) || (viewState.Third != null)) && (base.ViewState != null)))
            {
                HttpSessionState session = base.Page.Session;
                string str = Convert.ToString(DateTime.Now.Ticks, 0x10);
                object obj3 = null;
                x = base.Page.Request.Browser.RequiresControlStateInSession;
                if (x)
                {
                    obj3 = new Pair(base.ViewState, base.ControlState);
                    y = str;
                }
                else
                {
                    obj3 = base.ViewState;
                    y = new Pair(str, base.ControlState);
                }
                string str2 = "__SESSIONVIEWSTATE" + str;
                session[str2] = obj3;
                Queue queue = session["__VIEWSTATEQUEUE"] as Queue;
                if (queue == null)
                {
                    queue = new Queue();
                    session["__VIEWSTATEQUEUE"] = queue;
                }
                queue.Enqueue(str2);
                SessionPageStateSection sessionPageState = RuntimeConfig.GetConfig(base.Page.Request.Context).SessionPageState;
                int count = queue.Count;
                if (((sessionPageState != null) && (count > sessionPageState.HistorySize)) || ((sessionPageState == null) && (count > 9)))
                {
                    string name = (string) queue.Dequeue();
                    session.Remove(name);
                }
            }
            if (y != null)
            {
                base.Page.ClientState = Util.SerializeWithAssert(base.StateFormatter, new Pair(x, y));
            }
        }
    }
}

