using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace RelationalGit
{
    public class FileUtility
    {

        private static string[] _buildExtensions=new [] {".ps1",".sh",".make",".cmake",".cmd"};
        private static string[] _configExtensions=new [] {".yml",".conf",".config",".json",".txt"};
        private static string[] _implementationExtensions=new [] {".cs",".php",".java",".c",".cpp",".h",".py",".js",".ts",".r",".vb",".il",};
        private static string[] _interfaceExtensions=new [] {".css",".html",".jsx"};
        
        internal static string GetExtension(string path)
        {
            var lastDotIndex=path.LastIndexOf('.');

            if(lastDotIndex==-1)
                return null;

            return path.Substring(lastDotIndex);
        }

        internal static string GetFileType(string path)
        {
            var extention=GetExtension(path);
            
            if(extention==null)
                return FileType.Unknown.ToString();

            if(_buildExtensions.Any(m=>m==extention))
                return FileType.Build.ToString();

            if(_configExtensions.Any(m=>m==extention))
                return FileType.Config.ToString();

            if(_implementationExtensions.Any(m=>m==extention))
                return FileType.Implementation.ToString();

            if(_interfaceExtensions.Any(m=>m==extention))
                return FileType.Interface.ToString();

            return FileType.Unknown.ToString();
        }

        internal static bool IsTestFile(string path)
        {
            return path.ToLower().Contains("test");
        }
    }
}
