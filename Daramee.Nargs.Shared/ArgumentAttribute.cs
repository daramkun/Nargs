using System;

namespace Daramee.Nargs;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field,
    AllowMultiple = false, Inherited = true)]
public class ArgumentAttribute : Attribute
{
    public string? Name { get; set; }
    public string? ShortName { get; set; }
    public bool IsRequired { get; set; }

    public ArgumentAttribute(string? name = null, string? shortName = null, bool isRequired = false)
    {
        Name = name;
        ShortName = shortName;
        IsRequired = isRequired;
    }
}