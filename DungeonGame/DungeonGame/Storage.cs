using System;
using System.IO;
using Microsoft.Xna.Framework;
using System.Diagnostics;

#if WIN8
using System.Threading.Tasks;
using Windows.Storage;
#else
using System.IO.IsolatedStorage;
#endif

namespace LegendsOfDescent
{
    public class Storage
    {
        private static Stream OpenFile(string filePath, FileMode mode, FileAccess access)
        {
#if WINDOWS_PHONE || SILVERLIGHT
            IsolatedStorageFile isoFile = IsolatedStorageFile.GetUserStoreForApplication();
            return new IsolatedStorageFileStream(filePath, mode, access, isoFile);
#elif WIN8
            Stream stream = null;

            Task.Run(async () =>
            {
                // Read a file from AppData
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                StorageFile localFile = null;
                try
                {
                    localFile = await localFolder.CreateFileAsync(
                        filePath, 
                        mode == FileMode.Create ? CreationCollisionOption.ReplaceExisting : CreationCollisionOption.OpenIfExists);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }

                if (access == FileAccess.Read)
                {
                    stream = await localFile.OpenStreamForReadAsync();
                }
                else
                {
                    stream = await localFile.OpenStreamForWriteAsync();

                    if (mode == FileMode.Append && stream.Length > 0)
                    {
                        stream.Seek(stream.Length, SeekOrigin.Begin);
                    }
                }

            }).Wait();

            return stream;
#else
            return File.Open(filePath, mode, access);
#endif
        }


        public static Stream OpenRead(string filePath)
        {
            return OpenFile(filePath, FileMode.Open, FileAccess.Read);
        }

        public static Stream OpenCreate(string filePath)
        {
            return OpenFile(filePath, FileMode.Create, FileAccess.Write);
        }

        public static Stream OpenAppend(string filePath)
        {
            return OpenFile(filePath, FileMode.Append, FileAccess.Write);
        }

        public static bool FileExists(string filePath)
        {
#if WINDOWS_PHONE || SILVERLIGHT
            IsolatedStorageFile isoFile = IsolatedStorageFile.GetUserStoreForApplication();
            return isoFile.FileExists(filePath);
#else
#if WIN8
            bool result = false;
            Task.Run(async () =>
            {
                // Read a file from AppData
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;

                try
                {
                    await localFolder.GetFileAsync(filePath);
                    result = true;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }

                StorageFile localFile = await localFolder.GetFileAsync(filePath);

            }).Wait();
            return result;
#else
            return File.Exists(filePath);
#endif
#endif
            
        }

        public static void DeleteFile(string filePath)
        {
#if WINDOWS_PHONE || SILVERLIGHT
            IsolatedStorageFile isoFile = IsolatedStorageFile.GetUserStoreForApplication();
            isoFile.DeleteFile(filePath);
#else
#if WIN8
            Task.Run(async () =>
            {
                // Read a file from AppData
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;

                try
                {
                    StorageFile localFile = await localFolder.GetFileAsync(filePath);
                    await localFile.DeleteAsync();
                }
                catch
                {
                    // swallow the exception since it should just mean the file doesn't exist
                }

            }).Wait();
#else
            File.Delete(filePath);
#endif
#endif
        }

        public static Stream TitleStream(string filePath)
        {
            try
            {
                return TitleContainer.OpenStream(filePath);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }
    }
}
