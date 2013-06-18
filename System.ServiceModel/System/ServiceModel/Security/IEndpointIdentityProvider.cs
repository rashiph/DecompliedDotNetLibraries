namespace System.ServiceModel.Security
{
    using System.IdentityModel.Selectors;
    using System.ServiceModel;

    public interface IEndpointIdentityProvider
    {
        EndpointIdentity GetIdentityOfSelf(SecurityTokenRequirement tokenRequirement);
    }
}

