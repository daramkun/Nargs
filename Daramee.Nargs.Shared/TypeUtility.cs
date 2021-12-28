using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Daramee.Nargs;

internal class TypeUtility
{
    private readonly Dictionary<ArgumentAttribute, MemberInfo> _memberInfos = new();
    public MemberInfo? ArgumentStoreMember { get; } = null;

    public IEnumerable<ArgumentAttribute> ValidMembers => _memberInfos.Keys;

    public TypeUtility(Type type)
    {
        foreach (var memberInfo in type.GetMembers())
        {
            if (memberInfo is not PropertyInfo and not FieldInfo)
                continue;
            
            var argAttr = memberInfo.GetCustomAttribute<ArgumentAttribute>();
            var argStoreAttr = memberInfo.GetCustomAttribute<ArgumentStoreAttribute>();

            if (argStoreAttr != null && argAttr != null)
                throw new ArgumentException("Argument Attributes must have one type.", nameof(type));

            if (argStoreAttr != null && ArgumentStoreMember != null)
                throw new ArgumentException("Argument Store Attribute must have single member.", nameof(type));

            if (argStoreAttr != null)
            {
                switch (memberInfo)
                {
                    case PropertyInfo propertyInfo when propertyInfo.PropertyType == typeof(Dictionary<string, string>):
                    case FieldInfo fieldInfo when fieldInfo.FieldType == typeof(Dictionary<string, string>):
                        ArgumentStoreMember = memberInfo;
                        break;
                    default:
                        throw new ArgumentException("ArgumentStoreAttribute's type must be Dictionary<string, string>.",
                            nameof(type));
                }
            }
            else
            {
                argAttr ??= new ArgumentAttribute(memberInfo.Name);
                _memberInfos.Add(argAttr, memberInfo);
            }
        }
    }

    public MemberInfo? GetMember(in ArgumentStyle style, string? key, bool shortName = false)
    {
        if (string.IsNullOrEmpty(key))
            key = null;
        
        if (key == null && shortName)
            throw new ArgumentException();

        foreach (var (argAttr, memberInfo) in _memberInfos)
        {
            if (((!shortName && argAttr.Name == null) || (shortName && argAttr.ShortName == null)) && key == null)
                return memberInfo;
            return $"{(shortName ? style.ShortNamePrefix : style.NamePrefix)}{(shortName ? argAttr.ShortName : argAttr.Name)}"
                .Equals(key, style.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)
                ? memberInfo
                : null;
        }

        return null;
    }

    public static bool SetValue<T>(ref T? obj, MemberInfo memberInfo, object? value)
    {
        switch (memberInfo)
        {
            case PropertyInfo propertyInfo:
                if (propertyInfo.CanWrite)
                {
                    if (typeof(T).IsClass)
                        propertyInfo.SetValue(obj, value);
                    else
                    {
                        object? boxed = obj;
                        propertyInfo.SetValue(boxed, value, null);
                        obj = (T)boxed! ?? throw new InvalidCastException();
                    }
                }
                else
                    throw new ReadOnlyException();

                return true;
                
            case FieldInfo fieldInfo:
                if (obj != null && typeof(T).IsClass)
                    fieldInfo.SetValue(obj, value);
                else
                {
                    var tr = __makeref(obj);
                    fieldInfo.SetValueDirect(tr, value!);
                }

                return true;
            
            default: return false;
        }
    }

    public bool SetStoreValue<T>(ref T? obj, string key, string value)
    {
        if (ArgumentStoreMember == null)
            return false;
        
        var member = ArgumentStoreMember switch
        {
            PropertyInfo propertyInfo => propertyInfo.GetValue(obj) as Dictionary<string, string>,
            FieldInfo fieldInfo => fieldInfo.GetValue(obj) as Dictionary<string, string>,
            _ => throw new InvalidOperationException(),
        } ?? new Dictionary<string, string>();
        
        member.Add(key, value);
        SetValue(ref obj, ArgumentStoreMember, member);
        
        return true;
    }

    public bool DetectNotSettedRequired(IEnumerable<MemberInfo> members) =>
        _memberInfos.Any(kv => kv.Key.IsRequired && !members.Contains(kv.Value));
}