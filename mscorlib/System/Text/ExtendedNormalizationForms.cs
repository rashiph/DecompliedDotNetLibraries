namespace System.Text
{
    using System;

    internal enum ExtendedNormalizationForms
    {
        FormC = 1,
        FormCDisallowUnassigned = 0x101,
        FormD = 2,
        FormDDisallowUnassigned = 0x102,
        FormIdna = 13,
        FormIdnaDisallowUnassigned = 0x10d,
        FormKC = 5,
        FormKCDisallowUnassigned = 0x105,
        FormKD = 6,
        FormKDDisallowUnassigned = 0x106
    }
}

