using System;
using System.Collections.Generic;
using System.Text;

namespace Daramee.Nargs
{
	public enum ArgumentKeyValueSeparator
	{
		Space,
		Equals,
		Colon,
	}
	
	public struct ArgumentStyle
	{
		public static ArgumentStyle DOSStyle => new ArgumentStyle
		{
			NamePrestring = "/",
			ShortNamePrestring = "/",
			Separator = ArgumentKeyValueSeparator.Space,
		};
		public static ArgumentStyle UNIXStyle => new ArgumentStyle
		{
			NamePrestring = "--",
			ShortNamePrestring = "-",
			Separator = ArgumentKeyValueSeparator.Space,
		};

		public string NamePrestring;
		public string ShortNamePrestring;
		public ArgumentKeyValueSeparator Separator;
	}
}
