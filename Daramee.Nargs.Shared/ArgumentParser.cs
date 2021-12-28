using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Daramee.Nargs;

public static class ArgumentParser
{
    public static object Parse(Type retType, IEnumerable<string> args, in ArgumentStyle style, bool skipNoMember = false)
    {
        var ret = Activator.CreateInstance(retType) ??
                  throw new ArgumentException("Type must valid.", nameof(retType));
        
        var util = new TypeUtility(retType);

        var separatorChar = ConvertSeparator(style.Separator);

        var argsQueue = new Queue<string>(args);
        var proceedQueue = new Queue<MemberInfo>();
        while (argsQueue.Count > 0)
        {
            var arg = argsQueue.Dequeue();

            var isShortName = IsShortName(style, arg);
            GetKeyAndValue(style, arg, separatorChar, out var key, out var value);

            var member = util.GetMember(style, key, isShortName);
            if (member == null)
            {
                if (util.SetStoreValue(ref ret, key!, value!) || skipNoMember)
                    continue;
                
                throw new ArgumentException("Unknown argument(s) detected.");
            }

            var memberType = GetMemberType(member);
            if (style.Separator == ArgumentKeyValueSeparator.Space &&
                memberType != typeof(bool) &&
                value == null &&
                argsQueue.Count > 0)
                if (argsQueue.Peek()[..style.NamePrefix.Length] != style.NamePrefix &&
                    argsQueue.Peek()[..style.ShortNamePrefix.Length] != style.ShortNamePrefix)
                    value = argsQueue.Dequeue();

            object? settingValue = null;
            
            if (memberType == typeof(bool))
                settingValue = ConvertBoolean(value);
            else if (memberType == typeof(string))
                settingValue = value;
            else if (memberType == typeof(int))
                settingValue = int.Parse(value!);
            else if (memberType == typeof(uint))
                settingValue = uint.Parse(value!);
            else if (memberType == typeof(short))
                settingValue = short.Parse(value!);
            else if (memberType == typeof(ushort))
                settingValue = ushort.Parse(value!);
            else if (memberType == typeof(long))
                settingValue = long.Parse(value!);
            else if (memberType == typeof(ulong))
                settingValue = ulong.Parse(value!);
            else if (memberType == typeof(byte))
                settingValue = byte.Parse(value!);
            else if (memberType == typeof(sbyte))
                settingValue = sbyte.Parse(value!);
            else if (memberType == typeof(float))
                settingValue = float.Parse(value!);
            else if (memberType == typeof(double))
                settingValue = double.Parse(value!);
            else if (memberType == typeof(DateTime))
                settingValue = DateTime.Parse(value!);
            else if (memberType == typeof(TimeSpan))
                settingValue = TimeSpan.Parse(value!);
            else if (memberType == typeof(Enum))
            {
                if (!Enum.TryParse(memberType, value, true, out settingValue))
                {
                    if (int.TryParse(value!, out var integerSettingValue))
                        settingValue = Enum.ToObject(memberType, integerSettingValue);
                    else throw new InvalidDataException("Invalid enum value.");
                }
            }
            else if (memberType == typeof(string[]))
            {
                settingValue = value!.Split('|');
            }
            else
            {
                try
                {
                    settingValue = Activator.CreateInstance(memberType, value);
                }
                catch
                {
                    foreach (var methodInfo in memberType.GetMethods())
                    {
                        if (!methodInfo.IsStatic) continue;
                        if (methodInfo.ReturnType == typeof(void)) continue;
                        
                        var ps = methodInfo.GetParameters();
                        
                        if (ps.Length != 1) continue;
                        if (ps[0].ParameterType != typeof(string))
                            continue;
                        
                        settingValue = methodInfo.Invoke(memberType, new object[] {value});
                        break;
                    }
                }
            }

            if (!TypeUtility.SetValue(ref ret, member, settingValue))
                throw new ArgumentException();
            
            proceedQueue.Enqueue(member);
        }

        if (util.DetectNotSettedRequired(proceedQueue))
            throw new ArgumentException("Required parameter is no exist.");

        return ret!;
    }

    private static bool IsShortName(in ArgumentStyle style, string arg)
    {
        return (style.NamePrefix != style.ShortNamePrefix && !string.IsNullOrEmpty(style.ShortNamePrefix))
               && (arg[..style.NamePrefix.Length] != style.NamePrefix)
               && (arg[..style.ShortNamePrefix.Length] == style.ShortNamePrefix);
    }

    private static void GetKeyAndValue(in ArgumentStyle style, string arg, char separatorChar, out string? key, out string? value)
    {
        var separatorIndex = arg.IndexOf(separatorChar);
        if (separatorIndex >= 0)
        {
            key = arg.Substring(2, separatorIndex - 2);
            value = arg[separatorIndex..];
        }
        else
        {
            key = arg;
            value = TrueString;
            if (key[..style.NamePrefix.Length] == style.NamePrefix)
                return;
            if (key[..style.ShortNamePrefix.Length] == style.ShortNamePrefix)
                return;
            value = key;
            key = null;
        }
    }

    public static T Parse<T>(IEnumerable<string> args, in ArgumentStyle style, bool skipNoMember = false) =>
        (T) Parse(typeof(T), args, style, skipNoMember);

    private static char ConvertSeparator(ArgumentKeyValueSeparator separator) =>
        separator switch
        {
            ArgumentKeyValueSeparator.Space => ' ',
            ArgumentKeyValueSeparator.Equals => '=',
            ArgumentKeyValueSeparator.Colon => ':',
            _ => throw new ArgumentOutOfRangeException(nameof(separator)),
        };

    private static bool ConvertBoolean(string? boolean) =>
        boolean?.ToLower() switch
        {
            "t" => true,
            "true" => true,
            "1" => true,
            "" => true,
            null => true,
            
            "f" => false,
            "false" => false,
            "0" => false,
            
            _ => throw new ArgumentException("Boolean value is not valid.")
        };

    private static Type GetMemberType(MemberInfo memberInfo)
    {
        return memberInfo switch
        {
            PropertyInfo propertyInfo => propertyInfo.PropertyType,
            FieldInfo fieldInfo => fieldInfo.FieldType,
            _ => throw new ArgumentException("Member must Property or Field.", nameof(memberInfo)),
        };
    }

    private const string TrueString = "true";
}