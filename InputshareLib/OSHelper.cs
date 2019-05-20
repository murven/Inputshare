using System.Runtime.InteropServices;

namespace InputshareLib
{
    public static class OSHelper
    {
        public static Os CurrentOs { get; } = GetOsVersion();

        public static Os GetOsVersion()
        {
            Platform p = Platform.Windows;
            Architecture arc = Architecture.X86;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                p = Platform.Windows;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                p = Platform.Linux;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                p = Platform.MacOs;

            switch (RuntimeInformation.OSArchitecture)
            {
                case System.Runtime.InteropServices.Architecture.X64:
                    arc = Architecture.X64;
                    break;
                case System.Runtime.InteropServices.Architecture.X86:
                    arc = Architecture.X86;
                    break;
                case System.Runtime.InteropServices.Architecture.Arm:
                    arc = Architecture.ARM;
                    break;
                case System.Runtime.InteropServices.Architecture.Arm64:
                    arc = Architecture.ARM64;
                    break;
            }

            return new Os(p, arc, RuntimeInformation.OSDescription, RuntimeInformation.FrameworkDescription);
        }

        public class Os
        {
            public Os(Platform system, Architecture type, string description, string frameworkDescription)
            {
                System = system;
                Type = type;
                Description = description;
                FrameworkDescription = frameworkDescription;
            }

            public override string ToString()
            {
                return string.Format("{0} ({1}) using {2}", System, Type, FrameworkDescription);
            }

            public Platform System { get; }
            public Architecture Type { get; }
            public string Description { get; }
            public string FrameworkDescription { get; }
        }

        public enum Architecture
        {
            X86 = 0,
            X64 = 1,
            ARM = 2,
            ARM64 = 3
        }

        public enum Platform
        {
            Windows = 0,
            Linux = 1,
            MacOs = 2,
            FreeBSD = 3,
        }
    }
}
