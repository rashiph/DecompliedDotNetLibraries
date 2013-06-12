namespace System.Net
{
    using System;

    internal enum IgnoreCertProblem
    {
        all_not_time_valid = 7,
        all_rev_unknown = 0xf00,
        allow_unknown_ca = 0x10,
        ca_rev_unknown = 0x400,
        ctl_not_time_valid = 2,
        ctl_signer_rev_unknown = 0x200,
        end_rev_unknown = 0x100,
        invalid_basic_constraints = 8,
        invalid_name = 0x40,
        invalid_policy = 0x80,
        none = 0xfff,
        not_time_nested = 4,
        not_time_valid = 1,
        root_rev_unknown = 0x800,
        wrong_usage = 0x20
    }
}

