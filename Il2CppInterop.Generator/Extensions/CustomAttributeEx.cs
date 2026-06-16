using AsmResolver;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;

namespace Il2CppInterop.Generator.Extensions;

public static class CustomAttributeEx
{
    public static long ExtractOffset(this IHasCustomAttribute originalMethod)
    {
        return ExtractLong(originalMethod, "AddressAttribute", "Offset");
    }

    public static long ExtractRva(this IHasCustomAttribute originalMethod)
    {
        return ExtractLong(originalMethod, "AddressAttribute", "RVA");
    }

    public static long ExtractToken(this IHasCustomAttribute originalMethod)
    {
        return ExtractLong(originalMethod, "TokenAttribute", "Token");
    }

    public static int ExtractFieldOffset(this IHasCustomAttribute originalField)
    {
        return ExtractInt(originalField, "FieldOffsetAttribute", "Offset");
    }

    public static string? GetElementAsString(this CustomAttributeArgument argument)
    {
        return argument.Element as Utf8String ?? argument.Element as string;
    }

    private static string? Extract(this IHasCustomAttribute originalMethod, string attributeName,
        string parameterName)
    {
        // NOTE: the cast to IMemberDescriptor is load-bearing. This assembly is compiled against
        // AsmResolver beta.2 but is packaged alongside Cpp2IL which pins AsmResolver rc.1, so at
        // runtime NuGet unifies to rc.1's physical AsmResolver.DotNet.dll. A bare
        // `it.Constructor?.DeclaringType` binds (in beta.2 IL) to IMethodDefOrRef.get_DeclaringType,
        // which rc.1 removed -> MissingMethodException -> generation swallows it and every value-type
        // field offset becomes 0 -> corrupt Vector3/Quaternion/Color on EVERY game. IMemberDescriptor
        // declares DeclaringType in BOTH beta.2 and rc.1, so the cast produces a token that resolves
        // either way.
        var attribute = originalMethod.CustomAttributes.SingleOrDefault(it => ((IMemberDescriptor?)it.Constructor)?.DeclaringType?.Name == attributeName);
        var field = attribute?.Signature?.NamedArguments.SingleOrDefault(it => it.MemberName == parameterName);

        return field?.Argument.GetElementAsString();
    }

    private static long ExtractLong(this IHasCustomAttribute originalMethod, string attributeName, string parameterName)
    {
        return Convert.ToInt64(Extract(originalMethod, attributeName, parameterName), 16);
    }

    private static int ExtractInt(this IHasCustomAttribute originalMethod, string attributeName, string parameterName)
    {
        return Convert.ToInt32(Extract(originalMethod, attributeName, parameterName), 16);
    }
}
