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

		public void SetValue<T1, T2> ( T1 obj, MemberInfo memberInfo, T2 value )
		{
			if ( memberInfo is PropertyInfo )
				( memberInfo as PropertyInfo ).SetValue ( obj, value );
			else if ( memberInfo is FieldInfo )
				( memberInfo as FieldInfo ).SetValue ( obj, value );
		}
    }
}
