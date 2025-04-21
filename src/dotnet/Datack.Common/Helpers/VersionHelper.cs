using System.Reflection;

namespace Datack.Common.Helpers;

public static class VersionHelper
{
    public static String GetVersion()
    {
        var assembly = Assembly.GetEntryAssembly();

        if (assembly == null)
        {
            throw new("Cannot find EntryAssembly");
        }

        var versionAttribute = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();

        if (versionAttribute == null)
        {
            throw new($"AssemblyFileVersionAttribute not found for {assembly.FullName}");
        }

        return versionAttribute.Version;
    }
}