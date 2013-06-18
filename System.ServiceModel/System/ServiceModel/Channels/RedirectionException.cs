namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.ServiceModel;
    using System.Text;

    [Serializable]
    public class RedirectionException : CommunicationException
    {
        private RedirectionException()
        {
        }

        protected RedirectionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.Type = (RedirectionType) info.GetValue("Type", typeof(RedirectionType));
            this.Duration = (RedirectionDuration) info.GetValue("Duration", typeof(RedirectionDuration));
            this.Scope = (RedirectionScope) info.GetValue("Scope", typeof(RedirectionScope));
            RedirectionLocation[] list = (RedirectionLocation[]) info.GetValue("Locations", typeof(RedirectionLocation[]));
            this.Locations = new ReadOnlyCollection<RedirectionLocation>(list);
        }

        public RedirectionException(RedirectionType type, RedirectionDuration duration, RedirectionScope scope, params RedirectionLocation[] locations) : this(GetDefaultMessage(type, locations), type, duration, scope, null, locations)
        {
        }

        public RedirectionException(RedirectionType type, RedirectionDuration duration, RedirectionScope scope, Exception innerException, params RedirectionLocation[] locations) : this(GetDefaultMessage(type, locations), type, duration, scope, innerException, locations)
        {
        }

        public RedirectionException(string message, RedirectionType type, RedirectionDuration duration, RedirectionScope scope, params RedirectionLocation[] locations) : this(message, type, duration, scope, null, locations)
        {
        }

        public RedirectionException(string message, RedirectionType type, RedirectionDuration duration, RedirectionScope scope, Exception innerException, params RedirectionLocation[] locations) : base(message, innerException)
        {
            if (type == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("type");
            }
            if (message == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            if (message.Length == 0)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("message", System.ServiceModel.SR.GetString("ParameterCannotBeEmpty"));
            }
            if ((type.InternalType == RedirectionType.InternalRedirectionType.UseIntermediary) || (type.InternalType == RedirectionType.InternalRedirectionType.Resource))
            {
                if (locations == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("locations", System.ServiceModel.SR.GetString("RedirectMustProvideLocation"));
                }
                if (locations.Length == 0)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("locations", System.ServiceModel.SR.GetString("RedirectMustProvideLocation"));
                }
            }
            if (((type.InternalType == RedirectionType.InternalRedirectionType.Cache) && (locations != null)) && (locations.Length > 0))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("RedirectCacheNoLocationAllowed"));
            }
            if (locations == null)
            {
                locations = EmptyArray<RedirectionLocation>.Instance;
            }
            this.Locations = new ReadOnlyCollection<RedirectionLocation>(locations);
            this.Type = type;
            this.Scope = scope;
            this.Duration = duration;
        }

        private static string FormatLocations(RedirectionLocation[] locations)
        {
            string str = string.Empty;
            if ((locations == null) || (locations.Length <= 0))
            {
                return str;
            }
            StringBuilder builder = new StringBuilder();
            int num = 0;
            for (int i = 0; i < locations.Length; i++)
            {
                if (locations[i] != null)
                {
                    num++;
                    if (num > 1)
                    {
                        builder.AppendLine();
                    }
                    builder.AppendFormat("    {0}", locations[i].Address.AbsoluteUri);
                }
            }
            return builder.ToString();
        }

        private static string GetDefaultMessage(RedirectionType type, RedirectionLocation[] locations)
        {
            string str = string.Empty;
            if (type == null)
            {
                return str;
            }
            if (type.InternalType == RedirectionType.InternalRedirectionType.Cache)
            {
                return System.ServiceModel.SR.GetString("RedirectCache");
            }
            if (type.InternalType == RedirectionType.InternalRedirectionType.Resource)
            {
                return System.ServiceModel.SR.GetString("RedirectResource", new object[] { FormatLocations(locations) });
            }
            if (type.InternalType == RedirectionType.InternalRedirectionType.UseIntermediary)
            {
                return System.ServiceModel.SR.GetString("RedirectUseIntermediary", new object[] { FormatLocations(locations) });
            }
            return System.ServiceModel.SR.GetString("RedirectGenericMessage");
        }

        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Type", this.Type, typeof(RedirectionType));
            info.AddValue("Duration", this.Duration, typeof(RedirectionDuration));
            info.AddValue("Scope", this.Scope, typeof(RedirectionScope));
            info.AddValue("Locations", this.Locations.ToArray<RedirectionLocation>(), typeof(RedirectionLocation[]));
        }

        public RedirectionDuration Duration { get; private set; }

        public IEnumerable<RedirectionLocation> Locations { get; private set; }

        public RedirectionScope Scope { get; private set; }

        public RedirectionType Type { get; private set; }
    }
}

