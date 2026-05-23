/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PaintDotNet.SystemLayer
{
    public sealed class FileSystem
    {
        private const string sessionLockFileName = "session.lock";
        private static string tempDir;
        private static Stream sessionToken;
        private static Random random = new Random();

        /// <summary>
        /// Creates a file stream with the given filename that is deleted when either the
        /// stream is closed, or the application is terminated.
        /// </summary>
        /// <remarks>
        /// If the file already exists, it is overwritten without any error (CREATE_ALWAYS).
        /// </remarks>
        /// <param name="fileName">The full path to the file to create.</param>
        /// <returns>A Stream with read and write access.</returns>
        public static FileStream CreateTempFile(string fileName)
        {
            IntPtr hFile = SafeNativeMethods.CreateFileW(
                fileName,
                NativeConstants.GENERIC_READ | NativeConstants.GENERIC_WRITE,
                NativeConstants.FILE_SHARE_READ,
                IntPtr.Zero,
                NativeConstants.CREATE_ALWAYS,
                NativeConstants.FILE_ATTRIBUTE_TEMPORARY | NativeConstants.FILE_FLAG_DELETE_ON_CLOSE,
                IntPtr.Zero);

            if (hFile == NativeConstants.INVALID_HANDLE_VALUE)
            {
                NativeMethods.ThrowOnWin32Error();
            }

            FileStream stream;
            
            try
            {
                stream = new FileStream(hFile, FileAccess.ReadWrite, true);
            }

            catch
            {
                SafeNativeMethods.CloseHandle(hFile);
                hFile = IntPtr.Zero;
                throw;
            }

            return stream;
        }

        /// <summary>
        /// Opens an existing file for streaming reading. This stream should be read
        /// contiguously for best performance. Random I/O is still permissible, but
        /// may not perform well.
        /// </summary>
        /// <param name="fileName">The file to open.</param>
        /// <returns>A Stream object that may be used to read from the file.</returns>
        public static FileStream OpenStreamingFileRead(string fileName)
        {
            IntPtr hFile = SafeNativeMethods.CreateFileW(
                fileName,
                NativeConstants.GENERIC_READ,
                NativeConstants.FILE_SHARE_READ,
                IntPtr.Zero,
                NativeConstants.OPEN_EXISTING,
                NativeConstants.FILE_ATTRIBUTE_TEMPORARY | NativeConstants.FILE_FLAG_SEQUENTIAL_SCAN,
                IntPtr.Zero);

            if (hFile == NativeConstants.INVALID_HANDLE_VALUE)
            {
                NativeMethods.ThrowOnWin32Error();
            }

            FileStream stream;
            
            try
            {
                stream = new FileStream(hFile, FileAccess.Read, true, 512, false);
            }

            catch
            {
                SafeNativeMethods.CloseHandle(hFile);
                hFile = IntPtr.Zero;
                throw;
            }

            return stream;
        }

        /// <summary>
        /// Closes a file handle that was created with CreateStreamingFileHandleWrite.
        /// </summary>
        public static void CloseStreamingFileHandle(object fileHandle)
        {
            IntPtr hFile = (IntPtr)fileHandle;
            bool result = SafeNativeMethods.CloseHandle(hFile);

            if (!result)
            {
                NativeMethods.ThrowOnWin32Error();
            }
        }

        /// <summary>
        /// Creates a file with the give name that is only usable for writing with WriteToStreamingFileGather,
        /// and must be closed with ClostStreamingFileHandle.
        /// </summary>
        public static object CreateStreamingFileHandleWrite(string fileName)
        {
            IntPtr hFile = SafeNativeMethods.CreateFileW(
                fileName,
                NativeConstants.GENERIC_WRITE,
                NativeConstants.FILE_SHARE_READ,
                IntPtr.Zero,
                NativeConstants.CREATE_ALWAYS,
                NativeConstants.FILE_ATTRIBUTE_TEMPORARY | NativeConstants.FILE_FLAG_SEQUENTIAL_SCAN | 
                    NativeConstants.FILE_FLAG_OVERLAPPED,
                IntPtr.Zero);

            if (hFile == NativeConstants.INVALID_HANDLE_VALUE)
            {
                NativeMethods.ThrowOnWin32Error();
            }

            return (object)hFile;
        }

        /*
        /// <summary>
        /// Writes the given bytes to a stream.
        /// </summary>
        /// <param name="output">The stream to write data to.</param>
        /// <param name="pvBits">A pointer to the data to write.</param>
        /// <param name="length">The number of bytes to write.</param>
        /// <remarks>
        /// This method is provided for performance (memory-usage) purposes.
        /// </remarks>
        [CLSCompliant(false)]
        public unsafe static void WriteToStream(FileStream output, void *pvBuffer, uint length)
        {
            IntPtr hFile = output.Handle;

            if (hFile == NativeConstants.INVALID_HANDLE_VALUE)
            {
                throw new ArgumentException("output", "File is closed");
            }

            void *pvWrite = pvBuffer;
            
            while (length > 0)
            {
                uint written;
                bool result = SafeNativeMethods.WriteFile(hFile, pvWrite, length, out written, IntPtr.Zero);

                if (!result)
                {
                    NativeMethods.ThrowOnWin32Error("WriteFile() returned false");
                }

                pvWrite = (void *)((byte *)pvWrite + written);
                length -= written;
            }

            GC.KeepAlive(output);
        }
        */

        private unsafe static void WriteToStreamingFileGatherAsync(IntPtr hFile, void *[] ppvBuffers, uint[] lengths)
        {
            bool result = true;
            uint dwResult = NativeConstants.ERROR_SUCCESS;

            //IntPtr hFile = (IntPtr)outputHanoutput.Handle;

            long totalBytes = 0;

            // Compute total amount of bytes they want to write
            for (int i = 0; i < lengths.Length; ++i)
            {
                totalBytes += (long)lengths[i];
            }

            // Resize the file to match how much they're writing to it
            ulong newFilePointer;

            result = SafeNativeMethods.SetFilePointerEx(hFile, (ulong)totalBytes, out newFilePointer, NativeConstants.FILE_BEGIN);

            if (!result)
            {
                NativeMethods.ThrowOnWin32Error("SetFilePointerEx returned false (1)");
            }

            result = SafeNativeMethods.SetEndOfFile(hFile);

            if (!result)
            {
                NativeMethods.ThrowOnWin32Error("SetEndOfFile returned false");
            }

            result = SafeNativeMethods.SetFilePointerEx(hFile, 0, out newFilePointer, NativeConstants.FILE_BEGIN);

            if (!result)
            {
                NativeMethods.ThrowOnWin32Error("SetFilePointerEx returned false (2)");
            }

            // Method 2 -- buffered
            const uint bufferSize = 8192; // increasing this to 65536 or 131072 did not seem to affect performance for dumping out very large images (190mb), so we'll err on the side of using less memory
            IntPtr pBuffer1 = IntPtr.Zero;
            IntPtr pBuffer2 = IntPtr.Zero;
            IntPtr hEvent = IntPtr.Zero;
            ulong position = 0;

            try
            {
                NativeStructs.OVERLAPPED overlapped = new NativeStructs.OVERLAPPED();

                hEvent = SafeNativeMethods.CreateEventW(IntPtr.Zero, true, true, null);

                if (hEvent == IntPtr.Zero)
                {
                    NativeMethods.ThrowOnWin32Error("CreateEventW returned false");
                }

                overlapped.hEvent = hEvent;

                pBuffer1 = Memory.AllocateLarge((ulong)bufferSize);
                pBuffer2 = Memory.AllocateLarge((ulong)bufferSize);
                byte *pBufferBytes1 = (byte *)pBuffer1.ToPointer();
                byte *pBufferBytes2 = (byte *)pBuffer2.ToPointer();
                uint writeCursor = 0;
                
                for (int i = 0; i < ppvBuffers.Length; ++i)
                {
                    uint readCursor = 0;

                    while (readCursor < lengths[i])
                    {
                        uint bytesToCopy = Math.Min(lengths[i] - readCursor, bufferSize - writeCursor);

                        Memory.Copy((void *)(pBufferBytes1 + writeCursor), (void *)((byte *)ppvBuffers[i] + readCursor),
                            (ulong)bytesToCopy);

                        writeCursor += bytesToCopy;
                        readCursor += bytesToCopy;

                        // If we filled the write buffer, OR if this it the very last block to write
                        if (writeCursor == bufferSize || (i == ppvBuffers.Length - 1 && readCursor == lengths[i]))
                        {
                            // Wait for the previous I/O to finish
                            dwResult = SafeNativeMethods.WaitForSingleObject(hEvent, NativeConstants.INFINITE);

                            if (dwResult != NativeConstants.WAIT_OBJECT_0)
                            {
                                NativeMethods.ThrowOnWin32Error("WaitForSingleObject did not return WAIT_OBJECT_0");
                            }

                            // Set up the new I/O
                            overlapped.Offset = (uint)(position & 0xffffffff);
                            overlapped.OffsetHigh = (uint)(position >> 32);
                            uint dwBytesWritten;

                            result = SafeNativeMethods.WriteFile(
                                hFile,
                                pBufferBytes1,
                                writeCursor,
                                out dwBytesWritten,
                                ref overlapped);

                            if (!result)
                            {
                                int error = Marshal.GetLastWin32Error();

                                if (error != NativeConstants.ERROR_IO_PENDING)
                                {
                                    throw new Win32Exception(error, "WriteFile returned false and GetLastError() did not return ERROR_IO_PENDING");
                                }
                            }

                            // Adjust cursors and swap buffers
                            position += (ulong)bufferSize;

                            byte *temp = pBufferBytes1;
                            pBufferBytes1 = pBufferBytes2;
                            pBufferBytes2 = temp;
                            writeCursor = 0;
                        }
                    }
                }

                // Flush remaining data by waiting for the previous I/O to finish
                dwResult = SafeNativeMethods.WaitForSingleObject(hEvent, NativeConstants.INFINITE);

                if (dwResult != NativeConstants.WAIT_OBJECT_0)
                {
                    NativeMethods.ThrowOnWin32Error("WaitForSingleObject did not return WAIT_OBJECT_0");
                }
            }

            finally
            {
                if (pBuffer1 != IntPtr.Zero)
                {
                    Memory.FreeLarge(pBuffer1, (ulong)bufferSize);
                    pBuffer1 = IntPtr.Zero;
                }

                if (pBuffer2 != IntPtr.Zero)
                {
                    Memory.FreeLarge(pBuffer2, (ulong)bufferSize);
                    pBuffer2 = IntPtr.Zero;
                }

                if (hEvent != IntPtr.Zero)
                {
                    result = SafeNativeMethods.CloseHandle(hEvent);

                    if (!result)
                    {
                        NativeMethods.ThrowOnWin32Error("CloseHandle returned false on hEvent");
                    }
                }
            }
        }

        /// <summary>
        /// Writes data to the file. This data may be scattered throughout memory, but is written contiguously
        /// to the file such that ppvBuffers[n][m] is written to file location m + summation of lengths[0 through n - 1].
        /// If n is 0, then ppvBuffers[0][m] is written to file location m.
        /// Or, in pseudo code:
        ///     for (int n = 0; n &lt; lengths.Length; ++n)
        ///     {
        ///         for (int m = 0; m &lt; lengths[n]; ++m)
        ///         {
        ///             WriteByte(outputHandle, ppvBuffers[n][m]);
        ///         }
        ///     }
        /// </summary>
        /// <param name="outputHandle">A file handle created with CreateStreamingFileHandleWrite.</param>
        /// <param name="ppvBuffers">Pointers to buffers to write from.</param>
        /// <param name="lengths">The lengths of each buffer.</param>
        /// <remarks>
        /// ppvBuffers.Length must equal lengths.Length
        /// </remarks>
        public unsafe static void WriteToStreamingFileGather(object outputHandle, void *[] ppvBuffers, uint[] lengths)
        {
            if (ppvBuffers.Length != lengths.Length)
            {
                throw new ArgumentException("ppvBuffers.Length != lengths.Length");
            }

            WriteToStreamingFileGatherAsync((IntPtr)outputHandle, ppvBuffers, lengths);
        }

        /// <summary>
        /// Reads data fromt he file. This data is read contiguously from the file, but the buffers may
        /// be scattered throughout memory such that ppvBuffers[n][m] is read from file location 
        /// m + summation of lenghts[0 through n - 1]. If n is 0, then ppvBuffers[0][m] is read from
        /// file location m.
        /// Or, in pseudo code:
        ///     for (int n = 0; n &lt; lengths.Length; ++n)
        ///     {
        ///         for (int m = 0; m &lt; lengths[n]; ++m)
        ///         {
        ///             ppvBuffers[n][m] = ReadByte(input);
        ///         }
        ///     }
        /// </summary>
        /// <param name="input"></param>
        /// <param name="ppvBuffers"></param>
        /// <param name="lengths"></param>
        /// <remarks>This method is the counter to WriteToStreamingFileGather. ppvBuffers.Length must equal lengths.Length.</remarks>
        public unsafe static void ReadFromStreamScatter(FileStream input, void *[] ppvBuffers, uint[] lengths)
        {
            if (ppvBuffers.Length != lengths.Length)
            {
                throw new ArgumentException("ppvBuffers.Length != lengths.Length");
            }

            for (int i = 0; i < ppvBuffers.Length; ++i)
            {
                if (lengths[i] > 0)
                {
                    ReadFromStream(input, ppvBuffers[i], lengths[i]);
                }
            }
        }

        /// <summary>
        /// Reads bytes from a stream.
        /// </summary>
        /// <param name="output"></param>
        /// <param name="pvBits"></param>
        /// <param name="length"></param>
        /// <remarks>
        /// This method is provided for performance (memory-usage) purposes.
        /// </remarks>
        [CLSCompliant(false)]
        public unsafe static void ReadFromStream(FileStream input, void *pvBuffer, uint length)
        {
            IntPtr hFile = input.Handle;

            if (hFile == NativeConstants.INVALID_HANDLE_VALUE)
            {
                throw new ArgumentException("input", "File is closed");
            }

            void *pvRead = pvBuffer;
            
            while (length > 0)
            {
                uint read;
                bool result = SafeNativeMethods.ReadFile(hFile, pvRead, length, out read, IntPtr.Zero);

                if (!result)
                {
                    NativeMethods.ThrowOnWin32Error("ReadFile() returned false");
                }

                pvRead = (void *)((byte *)pvRead + read);
                length -= read;
            }

            GC.KeepAlive(input);
        }

        static FileSystem()
        {
            // Determine root path of where we store our persisted data
            string localSettingsDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string tempDirRoot = Path.Combine(localSettingsDir, "Paint.NET");

            DirectoryInfo tempDirRootInfo = new DirectoryInfo(tempDirRoot);

            if (!tempDirRootInfo.Exists)
            {
                tempDirRootInfo.Create();
            }

            // Clean up old session data
            string[] oldDirPaths = Directory.GetDirectories(tempDirRoot);

            foreach (string oldDirPath in oldDirPaths)
            {
                bool cleanUp = false;
                string lockPath = Path.Combine(oldDirPath, sessionLockFileName);

                // If the file doesn't exists, then clean up
                if (!cleanUp)
                {
                    FileInfo lockFileInfo = new FileInfo(lockPath);

                    if (!lockFileInfo.Exists)
                    {
                        cleanUp = true;
                    }
                }

                // If we can delete the lock file, then definitely clean up
                if (!cleanUp)
                {
                    try
                    {
                        File.Delete(lockPath);
                        cleanUp = true;
                    }

                    catch
                    {
                    }
                }
        
                if (cleanUp)
                {
                    string[] filesToCleanUp = Directory.GetFiles(oldDirPath, "*.");

                    foreach (string fileToCleanUp in filesToCleanUp)
                    {
                        try
                        {
                            // The only files we create have filenames that are parseable to integers
                            // So we only delete those files
                            int theInt = int.Parse(Path.GetFileName(fileToCleanUp), CultureInfo.InvariantCulture);
                            File.Delete(fileToCleanUp);
                        }

                        catch
                        {
                        }
                    }
                
                    Directory.Delete(oldDirPath, false);
                }
            }

            // Determine the directory where we will store this session's data
            while (true)
            {
                int subDirOrdinal = random.Next();
                string subDir = Path.Combine(tempDirRoot, subDirOrdinal.ToString(CultureInfo.InvariantCulture));
                DirectoryInfo dirInfo = new DirectoryInfo(subDir);
                
                if (!dirInfo.Exists)
                {
                    dirInfo.Create();
                    tempDir = subDir;
                    break;
                }
            }

            // Create our session lock cookie -- this file is locked for our process' lifetime
            // If our process is terminated, the file is deleted but our session files are not
            // However, if the system is abnormally shut down, neither the session lock file nor
            // the session temp files are deleted.
            string sessionTokenPath = Path.Combine(tempDir, sessionLockFileName);
            sessionToken = FileSystem.CreateTempFile(sessionTokenPath);

            // Cleanup when the app exits.
            Application.ApplicationExit += new EventHandler(Application_ApplicationExit);
        }

        private static void Application_ApplicationExit(object sender, EventArgs e)
        {
            if (sessionToken != null)
            {
                sessionToken.Close();
                sessionToken = null;
            }     
        }

        /// <summary>
        /// Generates a random filename for a file in the app's per-user temporary directory.
        /// </summary>
        /// <returns>
        /// The full path for a temporary filename. The file does not exist at the time this method returns.
        /// </returns>
        public static string GetTempFileName()
        {           
            string returnPath;

            while (true)
            {
                int ordinal = random.Next();
                string path = Path.Combine(tempDir, ordinal.ToString(CultureInfo.InvariantCulture));
                FileInfo fileInfo = new FileInfo(path);

                if (!fileInfo.Exists)
                {
                    returnPath = path;
                    break;
                }
            }

            return returnPath;
        }

        private FileSystem()
        {
        }
    }
}
