using Daramee.Nargs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
	class PingArgument
	{
		[Argument]
		public string Host { get; set; }
		[Argument ( Name = "t" )]
		public bool UntilStop { get; set; }
		[ArgumentStore]
		public Dictionary<string, string> Stored { get; set; }

		public override string ToString ()
		{
			return $"Host = {Host}, UntilStop = {UntilStop}";
		}
	}

	class Program
	{
		static void Main ( string [] args )
		{
			args = new string [] { "192.168.0.1", "/t", "/asdf" };
			var arg = ArgumentParser.Parse<PingArgument> ( args, ArgumentStyle.DOSStyle );

			Console.WriteLine ( arg );
		}
	}
}
