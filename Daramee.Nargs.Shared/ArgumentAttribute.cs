using System;
using System.Collections.Generic;
using System.Text;

namespace Daramee.Nargs
{
	[AttributeUsage (AttributeTargets.Property | AttributeTargets.Field,
		AllowMultiple = false, Inherited = true )]
	public class ArgumentAttribute : Attribute
	{
		public string Name { get; set; } = null;
		public string ShortName { get; set; } = null;
	}
}
