using System;
using System.Collections.Generic;
using System.Text;

namespace Daramee.Nargs;

public enum ArgumentKeyValueSeparator
{
    Space,
    Equals,
    Colon,
}

public readonly struct ArgumentStyle
{
    public static readonly ArgumentStyle DosStyle = new("/", "/", ArgumentKeyValueSeparator.Space);
    public static readonly ArgumentStyle UnixStyle = new("--", "-", ArgumentKeyValueSeparator.Space);

    public readonly string NamePrefix;
    public readonly string ShortNamePrefix;
    public readonly ArgumentKeyValueSeparator Separator;
    public readonly bool IgnoreCase;

    public ArgumentStyle(string namePrefix, string shortNamePrefix, ArgumentKeyValueSeparator separator, bool ignoreCase = false)
    {
        NamePrefix = namePrefix;
        ShortNamePrefix = shortNamePrefix;
        Separator = separator;
        IgnoreCase = ignoreCase;
    }
}