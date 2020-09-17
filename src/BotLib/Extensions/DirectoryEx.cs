using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BotLib.Extensions
{
    public class DirectoryEx
    {
        public static void AddSecurityControll2Folder(string dirPath)
        {
            var directoryInfo = new DirectoryInfo(dirPath);
            var accessControl = directoryInfo.GetAccessControl(AccessControlSections.All);
            var inheritanceFlags = InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit;
            var rule = new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, inheritanceFlags, PropagationFlags.None, AccessControlType.Allow);
            var rule2 = new FileSystemAccessRule("Users", FileSystemRights.FullControl, inheritanceFlags, PropagationFlags.None, AccessControlType.Allow);
            var rule3 = new FileSystemAccessRule("Authenticated Users", FileSystemRights.FullControl, inheritanceFlags, PropagationFlags.None, AccessControlType.Allow);
            var modified = false;
            accessControl.ModifyAccessRule(AccessControlModification.Add, rule, out modified);
            if (!modified)
            {
                Log.Error("设置everyone访问权限失败,dir=" + dirPath);
            }
            accessControl.ModifyAccessRule(AccessControlModification.Add, rule2, out modified);
            if (!modified)
            {
                Log.Error("设置users访问权限失败,dir=" + dirPath);
            }
            accessControl.ModifyAccessRule(AccessControlModification.Add, rule3, out modified);
            if (!modified)
            {
                Log.Error("设置Authenticated Users访问权限失败,dir=" + dirPath);
            }
            directoryInfo.SetAccessControl(accessControl);
        }

        public static void DeleteC(string dir, bool recursive = true)
        {
            try
            {
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, recursive);
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        public static void DeleteIfExist(string dir, bool recursive = true)
        {
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, recursive);
            }
        }


        public static void CreateNew(string dir, bool enableEveryOneFullAccess = true)
        {
            DirectoryEx.DeleteC(dir, true);
            Directory.CreateDirectory(dir);
            if (enableEveryOneFullAccess)
            {
                DirectoryEx.AddSecurityControll2Folder(dir);
            }
        }

        public static void Copy(string sourceDirectory, string targetDirectory)
        {
            DirectoryInfo source = new DirectoryInfo(sourceDirectory);
            DirectoryInfo target = new DirectoryInfo(targetDirectory);
            DirectoryEx.Copy(source, target);
        }

        private static void Copy(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);
            foreach (FileInfo fileInfo in source.GetFiles())
            {
                fileInfo.CopyTo(Path.Combine(target.FullName, fileInfo.Name), true);
            }
            foreach (DirectoryInfo directoryInfo in source.GetDirectories())
            {
                DirectoryEx.Copy(directoryInfo, target.CreateSubdirectory(directoryInfo.Name));
            }
        }
    }
}
