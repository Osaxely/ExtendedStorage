﻿using ExtendedStorage.Enums;
using System;
using System.IO;
using static ExtendedStorage.Win32;
using FileAttributes = System.IO.FileAttributes;

namespace ExtendedStorage
{
    public class EStorageItem
    {
        public FileAttributes Attributes { get; internal set; }
        public DateTimeOffset DateCreated { get; internal set; }
        public string Name { get; internal set; }
        public string Path { get; internal set; }

        public bool IsOfType(EStorageItemTypes type)
        {
            if (type == EStorageItemTypes.File)
            {
                if (this is EStorageFile) return true;
            }
            else if (type == EStorageItemTypes.Folder)
            {
                if (this is EStorageFolder) return true;
            }
            else if (type == EStorageItemTypes.None)
            {
                if (this is null) return true;
            }
            return false;
        }

        /// <summary>
        /// Deletes an item.
        /// </summary>
        /// <param name="item">The item to delete.</param>
        /// <returns>Returns true if the item was deleted, else false.</returns>
        public bool Delete()
        {
            return DeleteFile(Path);
        }

        /// <summary>
        /// Deletes an extended file from its path.
        /// </summary>
        /// <param name="itemPath">The item path to delete.</param>
        /// <returns>Returns true if the item was deleted, else false.</returns>
        private bool DeleteFile(string itemPath)
        {
            bool isDeleted = false;
            bool isFailed = false;

            if (IsFolder(itemPath))
            {
                foreach (EStorageFile file in (this as EStorageFolder).GetFiles())
                {
                    if (!DeleteFileFromApp(file.Path))
                    {
                        isFailed = true;
                        break;
                    }
                }

                foreach (EStorageFolder folder in (this as EStorageFolder).GetFolders())
                {

                    if (!DeleteFile(folder.Path))
                    {
                        isFailed = true;
                        break;
                    }
                }

                if (!isFailed)
                {
                    isDeleted = RemoveDirectoryFromApp(itemPath);
                }
            }
            else
            {
                DeleteFileFromApp(itemPath);
            }
            return isDeleted;
        }

        /// <summary>
        /// Renames an extended storage item.
        /// </summary>
        /// <param name="item">The item to rename.</param>
        /// <param name="newName">The new name.</param>
        public EStorageItem Rename(string newName)
        {
            if (MoveFileFromApp(Path, Directory.GetParent(Path) + @"\" + newName))
            {
                Path = Directory.GetParent(Path) + @"\" + newName;
                Name = newName;

                if (this is EStorageFile)
                {
                    (this as EStorageFile).DisplayName = System.IO.Path.GetFileNameWithoutExtension(newName);
                    return this;
                }
                else if (this is EStorageFolder)
                {
                    (this as EStorageFolder).Name = System.IO.Path.GetFileName(newName);
                    return this;
                }
            }
            return null;
        }

        /// <summary>
        /// Get the last write time of an extended storage item.
        /// </summary>
        /// <param name="source"></param>
        /// <returns>Returns the last write time of an extended storage item as DateTime.</returns>
        public DateTime GetLastWriteTime()
        {
            if (GetFileAttributesExFromApp(Path, GET_FILEEX_INFO_LEVELS.GetFileExInfoStandard, out WIN32_FILE_ATTRIBUTE_DATA fileData))
            {
                return DateTime.FromFileTimeUtc((((long)fileData.ftLastWriteTime.dwHighDateTime) << 32) | ((uint)fileData.ftLastWriteTime.dwLowDateTime));
            }
            else
            {
                return new DateTime();
            }
        }

        /// <summary>
        /// Get the last access time of an extended storage item.
        /// </summary>
        /// <param name="source"></param>
        /// <returns>Returns the last access time as DateTime.</returns>
        public DateTime GetLastAccessTime()
        {
            if (GetFileAttributesExFromApp(Path, GET_FILEEX_INFO_LEVELS.GetFileExInfoStandard, out WIN32_FILE_ATTRIBUTE_DATA fileData))
            {
                return DateTime.FromFileTimeUtc((((long)fileData.ftLastAccessTime.dwHighDateTime) << 32) | ((uint)fileData.ftLastAccessTime.dwLowDateTime));
            }
            else
            {
                return new DateTime();
            }
        }

        /// <summary>
        /// Get parent of the actual item.
        /// </summary>
        /// <returns>Returns the parent folder as ExtendedStorageFolder.</returns>
        public EStorageFolder GetParent()
        {
            DirectoryInfo path = Directory.GetParent(Path);
            if (path != null)
            {
                return EStorageFolder.GetFromPath(path.FullName);
            }
            else
            {
                return null;
            }
        }
    }
}