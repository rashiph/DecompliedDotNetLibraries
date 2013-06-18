namespace System.IdentityModel.Claims
{
    using System;

    public static class ClaimTypes
    {
        private const string anonymous = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/anonymous";
        private const string authentication = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/authentication";
        private const string authorizationdecision = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/authorizationdecision";
        private const string claimTypeNamespace = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims";
        private const string country = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/country";
        private const string dateofbirth = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/dateofbirth";
        private const string denyOnlySid = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/denyonlysid";
        private const string dns = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/dns";
        private const string email = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";
        private const string gender = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/gender";
        private const string givenname = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname";
        private const string hash = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/hash";
        private const string homephone = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/homephone";
        private const string locality = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/locality";
        private const string mobilephone = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/mobilephone";
        private const string name = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
        private const string nameidentifier = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
        private const string otherphone = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/otherphone";
        private const string postalcode = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/postalcode";
        private const string ppid = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/privatepersonalidentifier";
        private const string rsa = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/rsa";
        private const string sid = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/sid";
        private const string spn = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/spn";
        private const string stateorprovince = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/stateorprovince";
        private const string streetaddress = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/streetaddress";
        private const string surname = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname";
        private const string system = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/system";
        private const string thumbprint = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/thumbprint";
        private const string upn = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn";
        private const string uri = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/uri";
        private const string webpage = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/webpage";
        private const string x500DistinguishedName = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/x500distinguishedname";

        public static string Anonymous
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/anonymous";
            }
        }

        public static string Authentication
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/authentication";
            }
        }

        public static string AuthorizationDecision
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/authorizationdecision";
            }
        }

        public static string Country
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/country";
            }
        }

        public static string DateOfBirth
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/dateofbirth";
            }
        }

        public static string DenyOnlySid
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/denyonlysid";
            }
        }

        public static string Dns
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/dns";
            }
        }

        public static string Email
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";
            }
        }

        public static string Gender
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/gender";
            }
        }

        public static string GivenName
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname";
            }
        }

        public static string Hash
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/hash";
            }
        }

        public static string HomePhone
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/homephone";
            }
        }

        public static string Locality
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/locality";
            }
        }

        public static string MobilePhone
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/mobilephone";
            }
        }

        public static string Name
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
            }
        }

        public static string NameIdentifier
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
            }
        }

        public static string OtherPhone
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/otherphone";
            }
        }

        public static string PostalCode
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/postalcode";
            }
        }

        public static string PPID
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/privatepersonalidentifier";
            }
        }

        public static string Rsa
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/rsa";
            }
        }

        public static string Sid
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/sid";
            }
        }

        public static string Spn
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/spn";
            }
        }

        public static string StateOrProvince
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/stateorprovince";
            }
        }

        public static string StreetAddress
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/streetaddress";
            }
        }

        public static string Surname
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname";
            }
        }

        public static string System
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/system";
            }
        }

        public static string Thumbprint
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/thumbprint";
            }
        }

        public static string Upn
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn";
            }
        }

        public static string Uri
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/uri";
            }
        }

        public static string Webpage
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/webpage";
            }
        }

        public static string X500DistinguishedName
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/x500distinguishedname";
            }
        }
    }
}

