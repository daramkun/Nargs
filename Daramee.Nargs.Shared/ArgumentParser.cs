using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Daramee.Nargs
{
    public static class ArgumentParser
    {
		private static char ConvertSeparator ( ArgumentKeyValueSeparator separator )
		{
			switch ( separator )
			{
				case ArgumentKeyValueSeparator.Space: return ' ';
				case ArgumentKeyValueSeparator.Equals: return '=';
				case ArgumentKeyValueSeparator.Colon: return ':';
				default: return '\0';
			}
		}

		private static bool ConvertBoolean ( string boolean )
		{
			boolean = boolean?.ToLower ();
			switch ( boolean )
			{
				case "t":
				case "true":
				case "1":
				case null:
					return true;

				case "f":
				case "false":
				case "0":
					return false;

				default: throw new ArgumentException ( "Boolean value is not valid." );
			}
		}

		private static Type GetMemberType ( MemberInfo memberInfo )
		{
			if ( memberInfo is PropertyInfo )
				return ( memberInfo as PropertyInfo ).PropertyType;
			else if ( memberInfo is FieldInfo )
				return ( memberInfo as FieldInfo ).FieldType;
			else return null;
		}

		public static T Parse<T> ( string [] args, ArgumentStyle style, bool skipNoMember = false )
		{
			T ret = Activator.CreateInstance<T> ();
			Type retType = typeof ( T );
			TypeUtility util = new TypeUtility ( retType );

			char separatorChar = ConvertSeparator ( style.Separator );

			Queue<string> argsQueue = new Queue<string> ( args );
			while ( argsQueue.Count > 0 )
			{
				string arg = argsQueue.Dequeue ();

				bool isShortName = false;
				string key = null, value = null;

				if ( ( ( style.NamePrestring != style.ShortNamePrestring ) && !string.IsNullOrEmpty ( style.ShortNamePrestring ) )
					&& ( arg.Substring ( 0, style.NamePrestring.Length ) != style.NamePrestring )
					&& ( arg.Substring ( 0, style.ShortNamePrestring.Length ) == style.ShortNamePrestring ) )
					isShortName = true;

				int separatorIndex = arg.IndexOf ( separatorChar );
				if ( separatorIndex >= 0 )
				{
					key = arg.Substring ( 2, separatorIndex - 2 );
					value = arg.Substring ( separatorIndex );
				}
				else
				{
					key = arg;
					if ( key.Substring ( 0, style.NamePrestring.Length) != style.NamePrestring )
						if ( key.Substring ( 0, style.ShortNamePrestring.Length) != style.ShortNamePrestring )
						{
							value = key;
							key = null;
						}
				}

				MemberInfo member = util.GetMember ( style, key, isShortName );
				if ( member == null && !skipNoMember )
					throw new ArgumentException ();
				else if ( member == null && skipNoMember )
					continue;

				Type memberType = GetMemberType ( member );
				if ( style.Separator == ArgumentKeyValueSeparator.Space && memberType != typeof ( bool ) && value == null && argsQueue.Count > 0 )
					if ( argsQueue.Peek ().Substring ( 0, style.NamePrestring.Length ) != style.NamePrestring &&
						argsQueue.Peek ().Substring ( 0, style.ShortNamePrestring.Length ) != style.ShortNamePrestring )
						value = argsQueue.Dequeue ();

				if ( memberType == typeof ( bool ) )
					util.SetValue<T, bool> ( ret, member, ConvertBoolean ( value ) );
				else if ( memberType == typeof ( string ) )
					util.SetValue<T, string> ( ret, member, value );
				else if ( memberType == typeof ( int ) )
					util.SetValue<T, int> ( ret, member, int.Parse ( value ) );
				else if ( memberType == typeof ( uint ) )
					util.SetValue<T, uint> ( ret, member, uint.Parse ( value ) );
				else if ( memberType == typeof ( short ) )
					util.SetValue<T, short> ( ret, member, short.Parse ( value ) );
				else if ( memberType == typeof ( ushort ) )
					util.SetValue<T, ushort> ( ret, member, ushort.Parse ( value ) );
				else if ( memberType == typeof ( long ) )
					util.SetValue<T, long> ( ret, member, long.Parse ( value ) );
				else if ( memberType == typeof ( ulong ) )
					util.SetValue<T, ulong> ( ret, member, ulong.Parse ( value ) );
				else if ( memberType == typeof ( byte ) )
					util.SetValue<T, byte> ( ret, member, byte.Parse ( value ) );
				else if ( memberType == typeof ( sbyte ) )
					util.SetValue<T, sbyte> ( ret, member, sbyte.Parse ( value ) );
				else if ( memberType == typeof ( float ) )
					util.SetValue<T, float> ( ret, member, float.Parse ( value ) );
				else if ( memberType == typeof ( double ) )
					util.SetValue<T, double> ( ret, member, double.Parse ( value ) );
				else if ( memberType == typeof ( DateTime ) )
					util.SetValue<T, DateTime> ( ret, member, DateTime.Parse ( value ) );
				else if ( memberType == typeof ( TimeSpan ) )
					util.SetValue<T, TimeSpan> ( ret, member, TimeSpan.Parse ( value ) );
				else if ( memberType == typeof ( Enum ) )
				{
					try
					{
						object parsed = Enum.Parse ( memberType, value, true );
						util.SetValue<T, object> ( ret, member, parsed );
					}
					catch
					{
						util.SetValue<T, int> ( ret, member, int.Parse ( value ) );
					}
				}
				else if ( memberType == typeof ( string [] ) )
				{
					util.SetValue<T, string []> ( ret, member, value.Split ( '|' ) );
				}
				else
				{
					try
					{
						util.SetValue<T, object> ( ret, member, Activator.CreateInstance ( memberType, value ) );
					}
					catch
					{
						foreach ( var methodInfo in memberType.GetMethods () )
						{
							if ( !methodInfo.IsStatic ) continue;
							if ( methodInfo.ReturnType == typeof ( void ) ) continue;
							var ps = methodInfo.GetParameters ();
							if ( ps.Length != 1 ) continue;
							if ( ps [ 0 ].ParameterType == typeof ( string ) )
							{
								util.SetValue<T, object> ( ret, member, methodInfo.Invoke ( memberType, new object [] { value } ) );
								break;
							}
						}
					}
				}
			}

			return ret;
		}
    }
}
