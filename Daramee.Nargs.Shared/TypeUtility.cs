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
		MemberInfo argumentStore;

		public MemberInfo ArgumentStoreMember => argumentStore;

		public TypeUtility ( Type type )
		{
			this.type = type;

			foreach ( var memberInfo in type.GetMembers () )
			{
				if ( memberInfo.GetCustomAttribute<ArgumentAttribute> () == null )
				{
					if ( memberInfo.GetCustomAttribute<ArgumentStoreAttribute> () == null )
						continue;
					if ( memberInfo is PropertyInfo && ( memberInfo as PropertyInfo ).PropertyType != typeof ( Dictionary<string, string> ) )
						if ( memberInfo is FieldInfo && ( memberInfo as FieldInfo ).FieldType != typeof ( Dictionary<string, string> ) )
							throw new ArgumentException ( "ArgumentStoreAttribute's type must be Dictionary<string, string>." );

					if ( argumentStore != null )
						throw new ArgumentException ( "ArgumentStoreAttribute must have one or lesser." );
					argumentStore = memberInfo;
					continue;
				}
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

		public bool SetValue<T> ( ref T obj, string key, string value )
		{
			Dictionary<string, string> member;
			if ( ArgumentStoreMember is PropertyInfo )
				member = ( ArgumentStoreMember as PropertyInfo ).GetValue ( obj ) as Dictionary<string, string>;
			else if ( ArgumentStoreMember is FieldInfo )
				member = ( ArgumentStoreMember as FieldInfo ).GetValue ( obj ) as Dictionary<string, string>;
			else return false;
			if ( member == null ) member = new Dictionary<string, string> ();
			member.Add ( key, value ?? "true" );
			SetValue<T> ( ref obj, ArgumentStoreMember, member );
			return true;
		}
    }
}
