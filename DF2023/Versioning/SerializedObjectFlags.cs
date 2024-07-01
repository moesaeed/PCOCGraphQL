using System;

namespace DF2023.Versioning
{
    [Flags]
    public enum SerializedObjectFlags : byte
    {
        None = 0,
        IsNull = 1,
        Invalid = 255
    }
}