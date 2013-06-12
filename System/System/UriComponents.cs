namespace System
{
    [Flags]
    public enum UriComponents
    {
        AbsoluteUri = 0x7f,
        Fragment = 0x40,
        Host = 4,
        HostAndPort = 0x84,
        HttpRequestUrl = 0x3d,
        KeepDelimiter = 0x40000000,
        Path = 0x10,
        PathAndQuery = 0x30,
        Port = 8,
        Query = 0x20,
        Scheme = 1,
        SchemeAndServer = 13,
        SerializationInfoString = -2147483648,
        StrongAuthority = 0x86,
        StrongPort = 0x80,
        UserInfo = 2
    }
}

