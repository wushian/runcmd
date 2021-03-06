﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunCmd.Common
{
    public class Utility
    {
        internal static string getAsolutePathForRelativeFileName(string relDirectoryName, string fileName)
        {
            if (!Directory.Exists(getAbsolutePathForRelativeDir(relDirectoryName)))
            {
                Directory.CreateDirectory(getAbsolutePathForRelativeDir(relDirectoryName));
            }

            string strFileName = Path.Combine(getAbsolutePathForRelativeDir(relDirectoryName), fileName);

            return (strFileName);
        }


        internal static void WriteToFile(string inputText, string filePath)
        {
            FileInfo fInfo = new FileInfo(filePath);
            if (!DirExists(fInfo.Directory.Name)) {
                Directory.CreateDirectory(fInfo.Directory.Name);
            }
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.Write(inputText);
            }
        }

        public static string getAbsolutePathForRelativeDir(string folder)
        {
            //if (!System.IO.Directory.Exists(folder))
            string relDirectoryName=Path.Combine(getAppBasePath(), folder);
            //if (!Directory.Exists(getAbsolutePathForRelativeDir(relDirectoryName)))
            //{
            //    Directory.CreateDirectory(getAbsolutePathForRelativeDir(relDirectoryName));
            //}

            return (relDirectoryName);
        }

        public static string getAppBasePath()
        {
            string codeBase = System.Reflection.Assembly.GetExecutingAssembly().Location;
            return Path.GetDirectoryName(codeBase);
        }

        public static string ReadFileString(string path)
        {
            // Use StreamReader to consume the entire text file.
            using (StreamReader reader = new StreamReader(path))
            {
                return reader.ReadToEnd();
            }
        }

        internal static bool DirExists(string absPath)
        {
            return (Directory.Exists(absPath));
        }

        public static Task RunProcessAsync(string processPath, string strCmdArgs, string strWorkingDirectory)
        {
            var tcs = new TaskCompletionSource<object>();
            var process = new Process
            {
                EnableRaisingEvents = true,
                StartInfo = new ProcessStartInfo(processPath)
                {
                    Arguments = strCmdArgs,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true,
                    WorkingDirectory = strWorkingDirectory
                }
            };

            process.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            process.EnableRaisingEvents = true;

            process.Exited += (sender, args) =>
            {
                if (process.ExitCode != 0)
                {
                    var errorMessage = process.StandardError.ReadToEnd();
                    throw new InvalidOperationException("The process did not exit correctly. " +
                        "The corresponding error message was: " + errorMessage);
                }
                tcs.SetResult(null);
                process.Dispose();
            };
            process.Start();
            process.BeginOutputReadLine();
            //process.BeginErrorReadLine();
            return tcs.Task;
        }

        public static DataReceivedEventHandler OutputHandler { get; set; }

        public static string[] GetAllFileNames(string Location, string searchPattern)
        {
            if (!DirExists(Location))
            {
                return null;
            }
            try
            {
                return (Directory.GetFileSystemEntries(Location, searchPattern, SearchOption.TopDirectoryOnly));
            }
            catch { throw; }
        }

        public static void DeleteFile(string fullFileName)
        {
            File.Delete(fullFileName);
        }

        internal static List<TextFileViewModel> LoadAllBatFiles(string LocationToSearch, string FileNameFilter)
        {
            List<TextFileViewModel> lstRtrn = null;

            var fileNames=GetAllFileNames(LocationToSearch,FileNameFilter);

            if ((fileNames != null) && (fileNames.Length > 0))
            {
                lstRtrn = new List<TextFileViewModel>();
                foreach (string fileName in fileNames)
                {
                    TextFileViewModel vm = new TextFileViewModel();
                    vm.TextFileName = fileName;
                    vm.CmdText = ReadFileString(fileName);
                    lstRtrn.Add(vm);
                }
            }
            return (lstRtrn);
        }

        internal static bool Exists(string fileAbsolutePath)
        {
            return (File.Exists(fileAbsolutePath));
        }
    }
}
