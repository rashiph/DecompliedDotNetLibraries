namespace System.Web.Security
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Web;

    [Serializable, TypeForwardedFrom("System.Web, Version=2.0.0.0, Culture=Neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public class MembershipCreateUserException : Exception
    {
        private MembershipCreateStatus _StatusCode;

        public MembershipCreateUserException()
        {
            this._StatusCode = MembershipCreateStatus.ProviderError;
        }

        public MembershipCreateUserException(string message) : base(message)
        {
            this._StatusCode = MembershipCreateStatus.ProviderError;
        }

        public MembershipCreateUserException(MembershipCreateStatus statusCode) : base(GetMessageFromStatusCode(statusCode))
        {
            this._StatusCode = MembershipCreateStatus.ProviderError;
            this._StatusCode = statusCode;
        }

        protected MembershipCreateUserException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this._StatusCode = MembershipCreateStatus.ProviderError;
            this._StatusCode = (MembershipCreateStatus) info.GetInt32("_StatusCode");
        }

        public MembershipCreateUserException(string message, Exception innerException) : base(message, innerException)
        {
            this._StatusCode = MembershipCreateStatus.ProviderError;
        }

        internal static string GetMessageFromStatusCode(MembershipCreateStatus statusCode)
        {
            switch (statusCode)
            {
                case MembershipCreateStatus.Success:
                    return ApplicationServicesStrings.Membership_no_error;

                case MembershipCreateStatus.InvalidUserName:
                    return ApplicationServicesStrings.Membership_InvalidUserName;

                case MembershipCreateStatus.InvalidPassword:
                    return ApplicationServicesStrings.Membership_InvalidPassword;

                case MembershipCreateStatus.InvalidQuestion:
                    return ApplicationServicesStrings.Membership_InvalidQuestion;

                case MembershipCreateStatus.InvalidAnswer:
                    return ApplicationServicesStrings.Membership_InvalidAnswer;

                case MembershipCreateStatus.InvalidEmail:
                    return ApplicationServicesStrings.Membership_InvalidEmail;

                case MembershipCreateStatus.DuplicateUserName:
                    return ApplicationServicesStrings.Membership_DuplicateUserName;

                case MembershipCreateStatus.DuplicateEmail:
                    return ApplicationServicesStrings.Membership_DuplicateEmail;

                case MembershipCreateStatus.UserRejected:
                    return ApplicationServicesStrings.Membership_UserRejected;

                case MembershipCreateStatus.InvalidProviderUserKey:
                    return ApplicationServicesStrings.Membership_InvalidProviderUserKey;

                case MembershipCreateStatus.DuplicateProviderUserKey:
                    return ApplicationServicesStrings.Membership_DuplicateProviderUserKey;
            }
            return ApplicationServicesStrings.Provider_Error;
        }

        [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.SerializationFormatter, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_StatusCode", this._StatusCode);
        }

        public MembershipCreateStatus StatusCode
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._StatusCode;
            }
        }
    }
}

