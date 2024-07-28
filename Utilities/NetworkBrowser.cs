using FileManagementApp.Models;
using FileManagementApp.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace FileManagementApp.Utilities
{
    public class NetworkBrowser
    {
        [DllImport("Netapi32", CharSet = CharSet.Auto)]
        public static extern int NetServerEnum(
            string ServerName,
            int dwLevel,
            out IntPtr pBuf,
            int dwPrefMaxLen,
            out int dwEntriesRead,
            out int dwTotalEntries,
            int dwServerType,
            string domain,
            int dwResumeHandle
        );

        public static void EnumerateServers()
        {
            IntPtr pBuffer;
            int entriesRead, totalEntries;

            NetServerEnum(null, 100, out pBuffer, -1, out entriesRead, out totalEntries, 0, null, 0);

            // Обработка полученных данных
        }
    }

    public class NetApi32
    {
        public const uint MAX_PREFERRED_LENGTH = 0xFFFFFFFF;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct FILE_INFO_3
        {
            public int fi3_id;
            public int fi3_permission;
            public int fi3_num_locks;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string fi3_pathname;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string fi3_username;
        }

        [DllImport("Netapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern uint NetFileEnum(
            string servername,
            string basepath,
            string username,
            int level,
            out IntPtr bufptr,
            uint prefmaxlen,
            out int entriesread,
            out int totalentries,
            ref int resume_handle
        );

        [DllImport("Netapi32.dll")]
        public static extern uint NetFileClose(
            string servername,
            int id
        );

        [DllImport("Netapi32.dll")]
        public static extern uint NetApiBufferFree(IntPtr buffer);

        // Измененный метод, возвращающий ObservableCollection<FileModel>
        public static ObservableCollection<FileModel> EnumerateOpenFiles(string serverName, string searchTerm)
        {
            ObservableCollection<FileModel> files = new ObservableCollection<FileModel>();
            int resumeHandle = 0;
            IntPtr buffer;
            int entriesRead, totalEntries;

            uint result = NetFileEnum(serverName, null, null, 3, out buffer, MAX_PREFERRED_LENGTH, out entriesRead, out totalEntries, ref resumeHandle);

            if (result == 0)
            {
                IntPtr current = buffer;

                for (int i = 0; i < entriesRead; i++)
                {
                    FILE_INFO_3 fileInfo = (FILE_INFO_3)Marshal.PtrToStructure(current, typeof(FILE_INFO_3));

                    if (fileInfo.fi3_pathname.ToLower().Contains(searchTerm.ToLower()) ||
                        System.IO.Path.GetExtension(fileInfo.fi3_pathname).ToLower().Contains(searchTerm.ToLower()))
                    {
                        files.Add(new FileModel
                        {
                            Id = fileInfo.fi3_id,
                            PathName = fileInfo.fi3_pathname,
                            UserName = fileInfo.fi3_username
                        });
                    }
                    current = (IntPtr)((long)current + Marshal.SizeOf(typeof(FILE_INFO_3)));
                }

                NetApiBufferFree(buffer);
            }
            else
            {
                int errorCode = Marshal.GetLastWin32Error();
                // Обработка ошибки
                //Console.WriteLine("Ошибка при запросе открытых файлов. Код ошибки: " + errorCode);
            }

            return files; // Возвращаем коллекцию файлов
        }



        public static bool CloseFileOnServer(int fileId, string serverName = "" )
        {
            
            uint result = NetFileClose("", fileId);
            return result == 0;
        }
                
    }

    public struct FILE_INFO_3
    {
        public int fi3_id;
        public int fi3_permission;
        public int fi3_num_locks;
        public string fi3_pathname;
        public string fi3_username;
    }


}
