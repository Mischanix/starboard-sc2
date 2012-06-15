using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text;

namespace Starboard.MemoryReader
{
    static class Process
    {
        [DllImportAttribute("kernel32.dll", EntryPoint="OpenProcess")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", EntryPoint = "ReadProcessMemory")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, uint lpBaseAddress, IntPtr lpBuffer, int nSize, out int lpBytesRead);

        private static int PROCESS_VM_READ = 0x0010;

        private static System.Diagnostics.Process SC2Process;
        private static IntPtr SC2Handle;

        public static bool IsSC2Open()
        {
            return (SC2Process != null && !SC2Process.HasExited) ||
                System.Diagnostics.Process.GetProcessesByName("SC2").Length > 0;
        }

        public static void Init()
        {
            var handleList = System.Diagnostics.Process.GetProcessesByName("SC2");
            if (handleList.Length == 0)
                throw new InvalidOperationException("Initialization failed:  Starcraft II is not open");
            SC2Process = handleList[0];
            var procId = handleList[0].Id;
            SC2Handle = OpenProcess(PROCESS_VM_READ, false, procId);
            if (SC2Handle == IntPtr.Zero)
                throw new InvalidOperationException("Initialization failed:  Insufficient access");
        }

        public static int Version
        {
            get
            {
                if (SC2Process != null && !SC2Process.HasExited)
                    return SC2Process.MainModule.FileVersionInfo.FilePrivatePart;
                else return -1;
            }
        }

        public static object ReadObject(uint offset, Type type)
        {
            int cb = Marshal.SizeOf(type), bytesRead;
            var resultPtr = Marshal.AllocHGlobal(cb);
            if (!ReadProcessMemory(SC2Handle, offset, resultPtr, cb, out bytesRead))
                throw new InvalidOperationException("ReadProcessMemory failed");
            object result = Marshal.PtrToStructure(resultPtr, type);
            Marshal.FreeHGlobal(resultPtr);
            return result;
        }

        public static UInt32 ReadUInt(uint offset)
        {
            int cb = 4, bytesRead;
            var resultPtr = Marshal.AllocHGlobal(cb);
            if (!ReadProcessMemory(SC2Handle, offset, resultPtr, cb, out bytesRead))
                throw new InvalidOperationException("ReadProcessMemory failed");
            var result = (uint)Marshal.ReadInt32(resultPtr);
            Marshal.FreeHGlobal(resultPtr);
            return result;
        }

        public static Int32 ReadInt(uint offset)
        {
            int cb = 4, bytesRead;
            var resultPtr = Marshal.AllocHGlobal(cb);
            if (!ReadProcessMemory(SC2Handle, offset, resultPtr, cb, out bytesRead))
                throw new InvalidOperationException("ReadProcessMemory failed");
            var result = Marshal.ReadInt32(resultPtr);
            Marshal.FreeHGlobal(resultPtr);
            return result;
        }

        public static byte ReadByte(uint offset)
        {
            int cb = 1, bytesRead;
            var resultPtr = Marshal.AllocHGlobal(cb);
            if (!ReadProcessMemory(SC2Handle, offset, resultPtr, cb, out bytesRead))
                throw new InvalidOperationException("ReadProcessMemory failed");
            var result = Marshal.ReadByte(resultPtr);
            Marshal.FreeHGlobal(resultPtr);
            return result;
        }

        public static string ReadASCIIString(uint offset, int length)
        {
            int cb = length, bytesRead;
            var resultPtr = Marshal.AllocHGlobal(cb);
            if (!ReadProcessMemory(SC2Handle, offset, resultPtr, cb, out bytesRead))
                throw new InvalidOperationException("ReadProcessMemory failed");
            var result = Marshal.PtrToStringAnsi(resultPtr, length);
            Marshal.FreeHGlobal(resultPtr);
            return result;
        }

        public static string ReadUTFString(uint offset, int length)
        {
            int cb = length, bytesRead;
            byte[] result = new byte[cb];
            var resultPtr = Marshal.UnsafeAddrOfPinnedArrayElement(result, 0);
            if (!ReadProcessMemory(SC2Handle, offset, resultPtr, cb, out bytesRead))
                throw new InvalidOperationException("ReadProcessMemory failed");
            return Encoding.UTF8.GetString(result, 0, cb);
        }
    }
}
