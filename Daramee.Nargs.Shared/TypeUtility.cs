using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Daramee.Nargs
{
	public class TypeUtility
	{
		Type type;

		List<MemberInfo> memberInfoList = new List<MemberInfo> ();

		public TypeUtility ( Type type )
		{
			this.type = type;

			foreach ( var memberInfo in type.GetMembers () )
			{
				if ( memberInfo.GetCustomAttribute<ArgumentAttribute> () == null )
					continue;
				memberInfoList.Add ( memberInfo );
			}
		}

		public MemberInfo GetMember ( ArgumentStyle style, string key, bool shortName = false )
		{
			if ( key == "" ) key = null;
			if ( key == null && shortName == true )
				throw new ArgumentException ();
			if ( !shortName )
				key = key?.ToLower ();

			foreach ( var memberInfo in memberInfoList )
			{
				ArgumentAttribute attr = memberInfo.GetCustomAttribute<ArgumentAttribute> ();
				if ( !shortName )
				{
					attr.Name = attr.Name?.ToLower ();
					if ( ( attr.Name == null && key == null ) || $"{style.NamePrestring}{attr.Name}" == key )
					{
						memberInfoList.Remove ( memberInfo );
						return memberInfo;
					}
				}
				else
				{
					if ( ( attr.ShortName == null && key == null ) || $"{style.ShortNamePrestring}{attr.ShortName}" == key )
					{
						memberInfoList.Remove ( memberInfo );
						return memberInfo;
					}
				}
			}

			return null;
		}

		public bool SetValue<T> ( ref T obj, MemberInfo memberInfo, object value )
		{
			if ( memberInfo is PropertyInfo )
			{
				if ( ( memberInfo as PropertyInfo ).CanWrite )
				{
					if ( obj.GetType ().IsClass )
						( memberInfo as PropertyInfo ).SetValue ( obj, value );
					else
					{
						object boxed = obj;
						( memberInfo as PropertyInfo ).SetValue ( boxed, value, null );
						obj = ( T ) boxed;
					}
				}
				else
					return ( memberInfo as PropertyInfo ).GetValue ( obj ).Equals ( value );
				return true;
			}
			else if ( memberInfo is FieldInfo )
			{
				if ( obj.GetType ().IsClass )
					( memberInfo as FieldInfo ).SetValue ( obj, value );
				else
				{
					TypedReference tr = __makeref(obj);
					( memberInfo as FieldInfo ).SetValueDirect ( tr, value );
				}
				return true;
			}
			return false;
		}
    }
}
