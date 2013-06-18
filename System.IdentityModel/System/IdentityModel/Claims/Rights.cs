namespace System.IdentityModel.Claims
{
    using System;

    public static class Rights
    {
        private const string identity = "http://schemas.xmlsoap.org/ws/2005/05/identity/right/identity";
        private const string possessProperty = "http://schemas.xmlsoap.org/ws/2005/05/identity/right/possessproperty";
        private const string rightNamespace = "http://schemas.xmlsoap.org/ws/2005/05/identity/right";

        public static string Identity
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2005/05/identity/right/identity";
            }
        }

        public static string PossessProperty
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2005/05/identity/right/possessproperty";
            }
        }
    }
}

