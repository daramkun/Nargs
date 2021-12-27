using System;
using System.Collections.Generic;
using System.Text;

namespace Daramee.Nargs;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field,
    AllowMultiple = false, Inherited = true)]
public class ArgumentStoreAttribute : Attribute
{
}