using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreX.Tools
{
    internal class SearchJava
    {
        public static List<string> search()
        {
            List<string> paths = new List<string>();

            string javaKey = "SOFTWARE\\JavaSoft\\Java Runtime Environment";
            using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(javaKey))
            {
                if (rk != null)
                {
                    string[] versionNames = rk.GetSubKeyNames();
                    foreach (string versionName in versionNames)
                    {
                        using (RegistryKey versionKey = rk.OpenSubKey(versionName))
                        {
                            if (versionKey != null)
                            {
                                object javaHome = versionKey.GetValue("JavaHome");
                                if (javaHome != null)
                                {
                                    paths.Add(javaHome.ToString());
                                }
                            }
                        }
                    }
                }
            }

            // 如果在注册表中未找到，尝试在常见路径中查找
            string[] possiblePaths = {
            @"C:\Program Files\Java\",
            @"C:\Program Files (x86)\Java\"
        };

            foreach (string path in possiblePaths)
            {
                if (Directory.Exists(path))
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(path);
                    DirectoryInfo[] subDirs = dirInfo.GetDirectories();
                    foreach (DirectoryInfo subDir in subDirs)
                    {
                        string potentialJavaPath = Path.Combine(subDir.FullName, "bin", "java.exe");
                        if (File.Exists(potentialJavaPath))
                        {
                            paths.Add(subDir.FullName);
                        }
                    }
                }
            }

            return paths;
        }
    }
}
