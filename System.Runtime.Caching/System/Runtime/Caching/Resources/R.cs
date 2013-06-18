namespace System.Runtime.Caching.Resources
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Resources;
    using System.Runtime.CompilerServices;

    [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0"), DebuggerNonUserCode, CompilerGenerated]
    internal class R
    {
        private static CultureInfo resourceCulture;
        private static System.Resources.ResourceManager resourceMan;

        internal R()
        {
        }

        internal static string Argument_out_of_range
        {
            get
            {
                return ResourceManager.GetString("Argument_out_of_range", resourceCulture);
            }
        }

        internal static string Collection_contains_null_element
        {
            get
            {
                return ResourceManager.GetString("Collection_contains_null_element", resourceCulture);
            }
        }

        internal static string Collection_contains_null_or_empty_string
        {
            get
            {
                return ResourceManager.GetString("Collection_contains_null_or_empty_string", resourceCulture);
            }
        }

        internal static string Config_unable_to_get_section
        {
            get
            {
                return ResourceManager.GetString("Config_unable_to_get_section", resourceCulture);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static CultureInfo Culture
        {
            get
            {
                return resourceCulture;
            }
            set
            {
                resourceCulture = value;
            }
        }

        internal static string Default_is_reserved
        {
            get
            {
                return ResourceManager.GetString("Default_is_reserved", resourceCulture);
            }
        }

        internal static string Empty_collection
        {
            get
            {
                return ResourceManager.GetString("Empty_collection", resourceCulture);
            }
        }

        internal static string Empty_string_invalid
        {
            get
            {
                return ResourceManager.GetString("Empty_string_invalid", resourceCulture);
            }
        }

        internal static string Init_not_complete
        {
            get
            {
                return ResourceManager.GetString("Init_not_complete", resourceCulture);
            }
        }

        internal static string Invalid_argument_combination
        {
            get
            {
                return ResourceManager.GetString("Invalid_argument_combination", resourceCulture);
            }
        }

        internal static string Invalid_callback_combination
        {
            get
            {
                return ResourceManager.GetString("Invalid_callback_combination", resourceCulture);
            }
        }

        internal static string Invalid_expiration_combination
        {
            get
            {
                return ResourceManager.GetString("Invalid_expiration_combination", resourceCulture);
            }
        }

        internal static string Invalid_state
        {
            get
            {
                return ResourceManager.GetString("Invalid_state", resourceCulture);
            }
        }

        internal static string Method_already_invoked
        {
            get
            {
                return ResourceManager.GetString("Method_already_invoked", resourceCulture);
            }
        }

        internal static string Property_already_set
        {
            get
            {
                return ResourceManager.GetString("Property_already_set", resourceCulture);
            }
        }

        internal static string RegionName_not_supported
        {
            get
            {
                return ResourceManager.GetString("RegionName_not_supported", resourceCulture);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceMan, null))
                {
                    System.Resources.ResourceManager manager = new System.Resources.ResourceManager("System.Runtime.Caching.Resources.R", typeof(R).Assembly);
                    resourceMan = manager;
                }
                return resourceMan;
            }
        }

        internal static string TimeSpan_invalid_format
        {
            get
            {
                return ResourceManager.GetString("TimeSpan_invalid_format", resourceCulture);
            }
        }

        internal static string Update_callback_must_be_null
        {
            get
            {
                return ResourceManager.GetString("Update_callback_must_be_null", resourceCulture);
            }
        }

        internal static string Value_must_be_non_negative_integer
        {
            get
            {
                return ResourceManager.GetString("Value_must_be_non_negative_integer", resourceCulture);
            }
        }

        internal static string Value_must_be_positive_integer
        {
            get
            {
                return ResourceManager.GetString("Value_must_be_positive_integer", resourceCulture);
            }
        }

        internal static string Value_too_big
        {
            get
            {
                return ResourceManager.GetString("Value_too_big", resourceCulture);
            }
        }
    }
}

