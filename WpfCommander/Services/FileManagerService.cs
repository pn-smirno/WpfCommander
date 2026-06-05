using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using WpfCommander.Models;

namespace WpfCommander.Services
{
    public class FileManagerService
    {
        public List<string> GetDrives()
        {
            try
            {
                return DriveInfo.GetDrives().Select(d => d.Name).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения дисков: {ex.Message}");
                return new List<string>();
            }
        }

        public List<FileSystemItem> GetDirectoryContents(string path)
        {
            var items = new List<FileSystemItem>();

            try
            {
                if (!Directory.Exists(path))
                    return items;

                // Добавляем ".." для возврата на уровень выше
                var parentPath = Directory.GetParent(path)?.FullName;
                if (parentPath != null)
                {
                    var upItem = new FileSystemItem(parentPath);
                    var property = upItem.GetType().GetProperty("Name");
                    if (property != null && property.CanWrite)
                        property.SetValue(upItem, "..");
                    items.Add(upItem);
                }

                // Добавляем папки
                foreach (var dir in Directory.GetDirectories(path))
                {
                    try
                    {
                        var dirName = Path.GetFileName(dir);
                        if (!dirName.StartsWith("$") && dirName != "System Volume Information")
                            items.Add(new FileSystemItem(dir));
                    }
                    catch (UnauthorizedAccessException) { continue; }
                }

                // Добавляем файлы
                foreach (var file in Directory.GetFiles(path))
                {
                    try
                    {
                        items.Add(new FileSystemItem(file));
                    }
                    catch (UnauthorizedAccessException) { continue; }
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Отказано в доступе к папке", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return items;
        }

        public bool Copy(string sourcePath, string destinationPath, bool isDirectory)
        {
            try
            {
                if (isDirectory)
                    CopyDirectory(sourcePath, destinationPath);
                else
                    File.Copy(sourcePath, destinationPath, true);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Отказано в доступе", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка копирования: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void CopyDirectory(string sourceDir, string destDir)
        {
            if (!Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = Path.Combine(destDir, Path.GetFileName(subDir));
                CopyDirectory(subDir, destSubDir);
            }
        }

        public bool Move(string sourcePath, string destinationPath, bool isDirectory)
        {
            try
            {
                if (isDirectory)
                    Directory.Move(sourcePath, destinationPath);
                else
                    File.Move(sourcePath, destinationPath, true);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Отказано в доступе", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка перемещения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public bool CreateDirectory(string path, string newFolderName)
        {
            try
            {
                string fullPath = Path.Combine(path, newFolderName);

                if (Directory.Exists(fullPath))
                {
                    MessageBox.Show("Папка уже существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                Directory.CreateDirectory(fullPath);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания папки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public bool Delete(string path, bool isDirectory)
        {
            try
            {
                if (isDirectory)
                    Directory.Delete(path, true);
                else
                    File.Delete(path);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Отказано в доступе", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            catch (IOException)
            {
                MessageBox.Show("Файл используется другой программой", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public bool IsDirectory(string path) => Directory.Exists(path);

        public string GetItemName(string path) => Path.GetFileName(path);
    }
}
