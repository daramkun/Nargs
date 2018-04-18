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

				object settingValue = null;

				if ( memberType == typeof ( bool ) )
					settingValue = ConvertBoolean ( value );
				else if ( memberType == typeof ( string ) )
					settingValue = value;
				else if ( memberType == typeof ( int ) )
					settingValue = int.Parse ( value );
				else if ( memberType == typeof ( uint ) )
					settingValue = uint.Parse ( value );
				else if ( memberType == typeof ( short ) )
					settingValue = short.Parse ( value );
				else if ( memberType == typeof ( ushort ) )
					settingValue = ushort.Parse ( value );
				else if ( memberType == typeof ( long ) )
					settingValue = long.Parse ( value );
				else if ( memberType == typeof ( ulong ) )
					settingValue = ulong.Parse ( value );
				else if ( memberType == typeof ( byte ) )
					settingValue = byte.Parse ( value );
				else if ( memberType == typeof ( sbyte ) )
					settingValue = sbyte.Parse ( value );
				else if ( memberType == typeof ( float ) )
					settingValue = float.Parse ( value );
				else if ( memberType == typeof ( double ) )
					settingValue = double.Parse ( value );
				else if ( memberType == typeof ( DateTime ) )
					settingValue = DateTime.Parse ( value );
				else if ( memberType == typeof ( TimeSpan ) )
					settingValue = TimeSpan.Parse ( value );
				else if ( memberType == typeof ( Enum ) )
				{
					try
					{
						settingValue = Enum.Parse ( memberType, value, true );
					}
					catch
					{
						settingValue = int.Parse ( value );
					}
				}
				else if ( memberType == typeof ( string [] ) )
				{
					settingValue = value.Split ( '|' );
				}
				else
				{
					try
					{
						settingValue = Activator.CreateInstance ( memberType, value );
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
								settingValue = methodInfo.Invoke ( memberType, new object [] { value } );
								break;
							}
						}
					}
				}

				if ( !util.SetValue<T> ( ref ret, member, settingValue ) )
					throw new ArgumentException ();
			}

			return ret;
		}
    }
}
