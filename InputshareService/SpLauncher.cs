using InputshareLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;

namespace InputshareService
{
    class SpLauncher
    {
        public bool SpRunning { get => CheckSpRunning(); }
        const bool CheckSecureLocation = false;
        private bool CheckSpRunning()
        {
            Process[] procs = Process.GetProcessesByName("inputsharesp");
            if (procs.Length == 0)
                return false;
            else
                return true;
        }

        public void KillSp()
        {
            Process[] procs = Process.GetProcessesByName("inputsharesp");

            if (procs.Length == 0)
            {
                throw new InvalidOperationException("Inputsharesp not running");
            }

            foreach (Process proc in procs)
            {
                proc.Kill();
                ISLogger.Write($"Killed process {proc.ProcessName}");
            }
        }

        public void LaunchSp(string serviceWritePipe, string serviceReadPipe)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;

            string pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string pf86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            //if settings.CheckSecureLocation is true, the service will not allow SP to be launcher outside of 
            //program files to prevent any application from replacing SP with any other executable that will be run as system
            if (CheckSecureLocation)
            {
                
                if (path.StartsWith(pf) | path.StartsWith(pf86))
                {
                    ISLogger.Write("Directory {0} is secure", AppDomain.CurrentDomain.BaseDirectory, serviceWritePipe, serviceReadPipe);
                    StartAsSystem(AppDomain.CurrentDomain.BaseDirectory + "\\inputsharesp.exe", AppDomain.CurrentDomain.BaseDirectory, serviceWritePipe, serviceReadPipe);
                    return;
                }
                else
                {
                    throw new InvalidOperationException("Cannot launch inputshareSP from " + path + "\nProcess must be in a secure directory");
                }
            }
            else
            {
                if(StartAsSystem(AppDomain.CurrentDomain.BaseDirectory + "\\inputsharesp.exe", AppDomain.CurrentDomain.BaseDirectory, serviceWritePipe, serviceReadPipe) == null)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
        }
        private Process StartAsSystem(string procName, string procDir, string serviceWritePipe, string serviceReadPipe)
        {
            try
            {
                Process winLogon = null;
                foreach (Process p in Process.GetProcesses())
                {
                    if (p.ProcessName.Contains("winlogon"))
                    {
                        winLogon = p;
                        break;
                    }
                }

                if(winLogon == null)
                {
                    ISLogger.Write("Failed to launch inputsharesp: Could not find process");
                    return null;
                }

                IntPtr userToken = IntPtr.Zero;
                if (!OpenProcessToken(winLogon.Handle, (uint)TokenAccessLevels.Query | (uint)TokenAccessLevels.Impersonate | (uint)TokenAccessLevels.Duplicate, out userToken))
                {
                    ISLogger.Write("ERROR: OpenProcessToken returned false - " + Marshal.GetLastWin32Error());
                    return null;
                }

                IntPtr newToken = IntPtr.Zero;
                SECURITY_ATTRIBUTES tokenAttribs = new SECURITY_ATTRIBUTES();
                tokenAttribs.nLength = Marshal.SizeOf(tokenAttribs);
                SECURITY_ATTRIBUTES threadAttribs = new SECURITY_ATTRIBUTES();
                threadAttribs.nLength = Marshal.SizeOf(threadAttribs);

                //DUPLICATE TOKEN 
                if (!DuplicateTokenEx(userToken, 0x10000000, ref tokenAttribs, SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation,
                TOKEN_TYPE.TokenImpersonation, out newToken))
                {
                    ISLogger.Write("ERROR: DuplicateTokenEx returned false - " + Marshal.GetLastWin32Error());
                    return null;
                }

                TOKEN_PRIVILEGES tokPrivs = new TOKEN_PRIVILEGES();
                tokPrivs.PrivilegeCount = 1;
                LUID seDebugNameValue = new LUID();
                if (!LookupPrivilegeValue(null, SE_DEBUG_NAME, ref seDebugNameValue))
                {
                    ISLogger.Write("ERROR: LookupPrivilegeValue returned false - " + Marshal.GetLastWin32Error());
                    return null;
                }

                tokPrivs.Privileges = new LUID_AND_ATTRIBUTES[1];
                tokPrivs.Privileges[0].Luid = seDebugNameValue;
                tokPrivs.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;

                if (!AdjustTokenPrivileges(newToken, false, ref tokPrivs, 0, IntPtr.Zero, IntPtr.Zero))
                {
                    ISLogger.Write("ERROR: AdjustTokenPrivileges returned false - " + Marshal.GetLastWin32Error());
                    return null;
                }

                PROCESS_INFORMATION pi = new PROCESS_INFORMATION();
                STARTUPINFO si = new STARTUPINFO();
                si.cb = Marshal.SizeOf(si);
                //si.lpDesktop = "Winsta0\\default";
                si.lpDesktop = "Winsta0\\Winlogon";
                if (!CreateProcessAsUser(newToken, procName," " + serviceReadPipe + " " + serviceWritePipe, ref tokenAttribs, ref threadAttribs,
                true, (uint)CreateProcessFlags.REALTIME_PRIORITY_CLASS | (uint)CreateProcessFlags.CREATE_NO_WINDOW, IntPtr.Zero, procDir, ref si, out pi))
                {
                    ISLogger.Write("ERROR: CreateProcessAsUser returned false - " + Marshal.GetLastWin32Error());
                }

                Process _p = Process.GetProcessById(pi.dwProcessId);
                if (_p != null)
                {
                    ISLogger.Write("InputshareSP started: Process " + _p.Id + " Name " + _p.ProcessName);
                    return _p;
                }
                else
                {
                    ISLogger.Write("Process not found");
                    return null;
                }
            }
            catch (Exception ex)
            {
                ISLogger.Write("Failed to launch process as system: " + ex.Message);
                return null;
            }
        }

        public const string SE_DEBUG_NAME = "SeDebugPrivilege";
        public const uint SE_PRIVILEGE_ENABLED = 0x00000002;

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern Boolean SetTokenInformation(IntPtr TokenHandle, TOKEN_INFORMATION_CLASS TokenInformationClass, ref UInt32 TokenInformation, UInt32 TokenInformationLeng);
        /// <summary>
        /// Use System.Security.Principal.TokenAccessLevels
        /// </summary>
        /// <param name="ProcessHandle"></param>
        /// <param name="DesiredAccess"></param>
        /// <param name="TokenHandle"></param>
        /// <returns></returns>
        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool OpenProcessToken(IntPtr ProcessHandle, UInt32 DesiredAccess, out IntPtr TokenHandle);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public extern static bool DuplicateTokenEx(
            IntPtr hExistingToken,
            uint dwDesiredAccess,
            ref SECURITY_ATTRIBUTES lpTokenAttributes,
            SECURITY_IMPERSONATION_LEVEL ImpersonationLevel,
            TOKEN_TYPE TokenType,
            out IntPtr phNewToken);


        // Use this signature if you do not want the previous state
        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AdjustTokenPrivileges(IntPtr TokenHandle,
           [MarshalAs(UnmanagedType.Bool)]bool DisableAllPrivileges,
           ref TOKEN_PRIVILEGES NewState,
           UInt32 Zero,
           IntPtr Null1,
           IntPtr Null2);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool CreateProcessAsUser(
            IntPtr hToken,
            string lpApplicationName,
            string lpCommandLine,
            ref SECURITY_ATTRIBUTES lpProcessAttributes,
            ref SECURITY_ATTRIBUTES lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        [Flags]
        public enum CreateProcessFlags : uint
        {
            DEBUG_PROCESS = 0x00000001,
            DEBUG_ONLY_THIS_PROCESS = 0x00000002,
            CREATE_SUSPENDED = 0x00000004,
            DETACHED_PROCESS = 0x00000008,
            CREATE_NEW_CONSOLE = 0x00000010,
            NORMAL_PRIORITY_CLASS = 0x00000020,
            IDLE_PRIORITY_CLASS = 0x00000040,
            HIGH_PRIORITY_CLASS = 0x00000080,
            REALTIME_PRIORITY_CLASS = 0x00000100,
            CREATE_NEW_PROCESS_GROUP = 0x00000200,
            CREATE_UNICODE_ENVIRONMENT = 0x00000400,
            CREATE_SEPARATE_WOW_VDM = 0x00000800,
            CREATE_SHARED_WOW_VDM = 0x00001000,
            CREATE_FORCEDOS = 0x00002000,
            BELOW_NORMAL_PRIORITY_CLASS = 0x00004000,
            ABOVE_NORMAL_PRIORITY_CLASS = 0x00008000,
            INHERIT_PARENT_AFFINITY = 0x00010000,
            INHERIT_CALLER_PRIORITY = 0x00020000,
            CREATE_PROTECTED_PROCESS = 0x00040000,
            EXTENDED_STARTUPINFO_PRESENT = 0x00080000,
            PROCESS_MODE_BACKGROUND_BEGIN = 0x00100000,
            PROCESS_MODE_BACKGROUND_END = 0x00200000,
            CREATE_BREAKAWAY_FROM_JOB = 0x01000000,
            CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 0x02000000,
            CREATE_DEFAULT_ERROR_MODE = 0x04000000,
            CREATE_NO_WINDOW = 0x08000000,
            PROFILE_USER = 0x10000000,
            PROFILE_KERNEL = 0x20000000,
            PROFILE_SERVER = 0x40000000,
            CREATE_IGNORE_SYSTEM_DEFAULT = 0x80000000,
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct STARTUPINFOEX
        {
            public STARTUPINFO StartupInfo;
            public IntPtr lpAttributeList;
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct STARTUPINFO
        {
            public Int32 cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwYSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public Int32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }
        [DllImport("advapi32.dll")]
        public static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, ref LUID lpLuid);

        public struct LUID
        {
            public UInt32 LowPart;
            public Int32 HighPart;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct LUID_AND_ATTRIBUTES
        {
            public LUID Luid;
            public UInt32 Attributes;
        }

        public struct TOKEN_PRIVILEGES
        {
            public int PrivilegeCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
            public LUID_AND_ATTRIBUTES[] Privileges;
        }

        public enum TOKEN_TYPE
        {
            TokenPrimary = 1,
            TokenImpersonation
        }

        public enum SECURITY_IMPERSONATION_LEVEL
        {
            SecurityAnonymous,
            SecurityIdentification,
            SecurityImpersonation,
            SecurityDelegation
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public int bInheritHandle;
        }
        public enum TOKEN_INFORMATION_CLASS
        {
            TokenUser = 1,
            TokenGroups,
            TokenPrivileges,
            TokenOwner,
            TokenPrimaryGroup,
            TokenDefaultDacl,
            TokenSource,
            TokenType,
            TokenImpersonationLevel,
            TokenStatistics,
            TokenRestrictedSids,
            TokenSessionId,
            TokenGroupsAndPrivileges,
            TokenSessionReference,
            TokenSandBoxInert,
            TokenAuditPolicy,
            TokenOrigin,
            TokenElevationType,
            TokenLinkedToken,
            TokenElevation,
            TokenHasRestrictions,
            TokenAccessInformation,
            TokenVirtualizationAllowed,
            TokenVirtualizationEnabled,
            TokenIntegrityLevel,
            TokenUIAccess,
            TokenMandatoryPolicy,
            TokenLogonSid,
            TokenIsAppContainer,
            TokenCapabilities,
            TokenAppContainerSid,
            TokenAppContainerNumber,
            TokenUserClaimAttributes,
            TokenDeviceClaimAttributes,
            TokenRestrictedUserClaimAttributes,
            TokenRestrictedDeviceClaimAttributes,
            TokenDeviceGroups,
            TokenRestrictedDeviceGroups,
            TokenSecurityAttributes,
            TokenIsRestricted,
            TokenProcessTrustLevel,
            TokenPrivateNameSpace,
            TokenSingletonAttributes,
            TokenBnoIsolation,
            TokenChildProcessFlags,
            MaxTokenInfoClass,
        }
    }
}
