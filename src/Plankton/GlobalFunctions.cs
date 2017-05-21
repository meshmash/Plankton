using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace PlanktonGeoTools
{
    public class Global
    {
        private bool _enable = false;
        public bool Enable
        {
            get
            {
                return _enable;
            }
            set
            {
                _enable = value;
                initConsole();
            }

        }
        public Global()
        {
            this.Enable = true;
        }
        public Global(bool enable)
        {
            this.Enable = enable;
        }
        public virtual void initConsole()
        {
            if (!_enable) return;
            AllocConsole();
            Console.WindowWidth = 136;
            Console.Clear();
        }
        public void Print(string str)
        {
            if (_enable) Console.WriteLine(str);
        }
        public void Print(double str)
        {
            if (_enable) Console.WriteLine(str.ToString());
        }
        public void Print(int str)
        {
            if (_enable) Console.WriteLine(str.ToString());
        }
        public void Print(float str)
        {
            if (_enable) Console.WriteLine(str.ToString());
        }
        public void Print(IEnumerable<string> collection)
        {
            if (!_enable) return;
            foreach (string str in collection)
            {
                Console.WriteLine(str);
            }

        }
        public void Print(IEnumerable<double> collection)
        {
            if (!_enable) return;
            foreach (double str in collection)
            {
                Console.WriteLine(str.ToString());
            }

        }
        public void Print(IEnumerable<float> collection)
        {
            if (!_enable) return;
            foreach (float str in collection)
            {
                Console.WriteLine(str.ToString());
            }

        }
        public void Print(IEnumerable<int> collection)
        {
            if (!_enable) return;
            foreach (int str in collection)
            {
                Console.WriteLine(str.ToString());
            }
        }
        [DllImport("kernel32.dll")]
        public static extern bool AllocConsole();
        [DllImport("kernel32.dll")]
        public static extern bool FreeConsole();
        public static bool _deletefolder_withoutroot(string dir)
        {
            if (Directory.Exists(dir))
            {
                foreach (string d in Directory.GetFileSystemEntries(dir))
                {
                    if (File.Exists(d))
                        File.Delete(d);
                    else
                        _deletefolder(d);
                }
            }
            else { return false; }
            return true;
        }
        public static bool _deletefolder(string dir)
        {
            if (Directory.Exists(dir))
            {
                foreach (string d in Directory.GetFileSystemEntries(dir))
                {
                    if (File.Exists(d))
                        File.Delete(d);
                    else
                        _deletefolder(d);
                }
                Directory.Delete(dir);
                return true;
            }
            return false;
        }
        public static void _deletefiles_withWhitelist(string dir, ref List<string> _whitelist)
        {
            if (_whitelist.Contains(dir)) return;
            if (Directory.Exists(dir))
            {
                foreach (string d in Directory.GetFileSystemEntries(dir))
                {
                    if (!_whitelist.Contains(d))
                    {
                        if (File.Exists(d)) { File.Delete(d); }
                        else { _deletefiles_withWhitelist(d, ref _whitelist); }
                    }
                }
            }
        }
        public static bool _deletefiles(string dir)
        {
            if (Directory.Exists(dir))
            {
                foreach (string d in Directory.GetFileSystemEntries(dir))
                {
                    if (File.Exists(d))
                        File.Delete(d);
                    else
                        _deletefiles(d);
                }
                return true;
            }
            return false;
        }
        public static void _SearchFiles(string dir, ref List<string> type, ref List<string> output)
        {
            //通过文件名或者或者扩展名
            if (Directory.Exists(dir))
            {
                foreach (string d in Directory.GetFileSystemEntries(dir))
                {
                    if (File.Exists(d))
                    {
                        string name = Path.GetFileNameWithoutExtension(d);
                        string extension = Path.GetExtension(d);
                        for (int i = 0; i < type.Count; i++)
                        {
                            if (name == type[i]) { output.Add(d); break; }
                            if (extension == type[i]) { output.Add(d); break; }
                        }
                    }
                    else
                        _SearchFiles(d, ref type, ref output);
                }
            }
        }
        public static void _SearchFiles(string dir, string type, ref List<string> output)
        {
            //通过文件名或者或者扩展名
            if (Directory.Exists(dir))
            {
                foreach (string d in Directory.GetFileSystemEntries(dir))
                {
                    if (File.Exists(d))
                    {
                        string name = Path.GetFileNameWithoutExtension(d);
                        string extension = Path.GetExtension(d);
                        
                            if (name == type || extension == type) { output.Add(d);  }                                            
                    }
                    else
                        _SearchFiles(d, type, ref output);
                }
            }
        }
        public static void _SearchFolders(string dir, ref List<string> type, ref List<string> output)
        {//文件夹名称
            if (Directory.Exists(dir))
            {
                string basefolder = Path.GetFileName(dir);
                for (int i = 0; i < type.Count; i++)
                {
                    if (basefolder == type[i]) { output.Add(dir); break; }
                }
                foreach (string d in Directory.GetFileSystemEntries(dir))
                {
                    _SearchFolders(d, ref type, ref output);
                }
            }
        }
        public static void _SearchFolders(string dir, string type, ref List<string> output)
        {//文件夹名称
            if (Directory.Exists(dir))
            {
                string basefolder = Path.GetFileName(dir);              
                    if (basefolder == type) { output.Add(dir); }            
                foreach (string d in Directory.GetFileSystemEntries(dir))
                {
                    _SearchFolders(d, type, ref output);
                }
            }
        }     
        public static bool _OpenFolder(string dir)
        {
            if (Directory.Exists(dir))
            {
                System.Diagnostics.Process.Start("Explorer.exe", dir);
            }
            else { return false; }
            return true;
        }
    }
}
