namespace System.Security.Policy
{
    internal interface IUnionSemanticCodeGroup
    {
        PolicyStatement InternalResolve(Evidence evidence);
    }
}

