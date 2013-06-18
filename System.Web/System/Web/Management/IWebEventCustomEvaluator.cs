namespace System.Web.Management
{
    using System;

    public interface IWebEventCustomEvaluator
    {
        bool CanFire(WebBaseEvent raisedEvent, RuleFiringRecord record);
    }
}

