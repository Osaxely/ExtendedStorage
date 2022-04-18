﻿using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using static ExtendedStorage.Win32;

namespace ExtendedStorage.Utils
{
    public static class FileReader
    {
        /// <summary>
        /// Read the text of a EStorageFile
        /// </summary>
        /// <returns>Returns the content of the file as string if success, else it returns null</returns>
        public static string ReadText(this EStorageFile source, FileShare shareMode = FileShare.Read)
        {
            string textToReturn = null;
            IntPtr hFile = CreateFileFromApp(source.Path, (uint)FileAccess.Read, (uint)shareMode, IntPtr.Zero, (uint)FileMode.Open, (uint)FileAttributes.Normal, IntPtr.Zero);
            if (hFile.ToInt64() != -1)
            {
                GetFileSizeEx(hFile, out long lpFileSize);
                uint bytesRead = 0;
                byte[] buffer = new byte[lpFileSize];

                if (ReadFile(hFile, buffer, Convert.ToUInt32(lpFileSize), bytesRead, IntPtr.Zero) == true)
                {
                    textToReturn = Encoding.UTF8.GetString(buffer);
                }
            }
            CloseHandle(hFile);
            return textToReturn;
        }

        /// <summary>
        /// Try to get an ImageSource from EStorageFile
        /// </summary>
        /// <param name="source">The source EStorageFile</param>
        /// <returns>Returns an ImageSource if success, else null.</returns>
        public static async Task<ImageSource> TryGetImage(this EStorageFile source)
        {
            try
            {
                if (source == null)
                {
                    return null;
                }

                using (Stream stream = source.OpenAsStream(FileAccess.Read))
                {
                    BitmapImage result = new BitmapImage();
                    await result.SetSourceAsync(stream.AsRandomAccessStream());
                    return result;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
