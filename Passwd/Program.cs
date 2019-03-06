using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Windows.Forms;
using DomainPasswd;
using Passwd.SystemDirectoryServicesImpl;

namespace Passwd
{
    class Program
    {
        internal static string Log { get; private set; } = "";

        static int Main(string[] args)
        {
            EmbeddedAssembly.Init();
            if (args.Length == 0)
            {
                Application.EnableVisualStyles();
                Application.Run(new frmMain());
                return 0;
            }
            else return ExecuteFromCommandLine(args);

        }

        internal static int ExecuteFromCommandLine(string[] args, bool consoleMode = true, SecureString oldPwd = null, SecureString newPwd = null)
        {
            var options = new ProgramOptions()
            {
                ConsoleMode = consoleMode,
                OldPassword = oldPwd,
                NewPassword = newPwd
            };

            if (!CommandLine.Parser.Default.ParseArguments(args, options))
            {
                return ReturnCodes.ArgumentsError;
            }

            try
            {
                var p = new PasswordChanger(
                    new CommandLinePasswordChangerSource(),
                    new PrincipalDomainContextFactory());
                p.ChangePassword(options);
                Log = "Password changed.";
                Console.Error.WriteLine(Log);
                return ReturnCodes.Ok;
            }
            catch (Exception e)
            {
                if (options.Verbose)
                {
                    Log = e.Message;
                    Console.Error.WriteLine(e);
                }
                else
                {
                    Log = e.Message;
                    Console.Error.WriteLine(e.Message);
                }

                return ReturnCodes.ExecutionError;
            }
        }

        internal static class ReturnCodes
        {
            public const int ArgumentsError = 1;
            public const int ExecutionError = 2;
            public const int Ok = 0;
        }

        /// <summary>
        /// Enables loading assemblies from embedded resources
        /// </summary>
        /// <remarks>
        /// Based on http://www.codeproject.com/Articles/528178/Load-DLL-From-Embedded-Resource
        /// </remarks>
        private static class EmbeddedAssembly
        {
            public static void Init()
            {
                var embeddedDlls = Assembly.GetExecutingAssembly().GetManifestResourceNames()
                    .Where(x => x.EndsWith(".dll"));

                foreach (var embeddedDll in embeddedDlls)
                {
                    Load(embeddedDll);
                }

                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            }

            static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
            {
                return Get(args.Name);
            }

            static Dictionary<string, Assembly> dic;

            public static void Load(string embeddedResource)
            {
                if (dic == null)
                    dic = new Dictionary<string, Assembly>();

                byte[] ba = null;
                Assembly asm = null;
                Assembly curAsm = Assembly.GetExecutingAssembly();

                using (Stream stm = curAsm.GetManifestResourceStream(embeddedResource))
                {
                    // Either the file is not existed or it is not mark as embedded resource
                    if (stm == null)
                        throw new Exception(embeddedResource + " is not found in Embedded Resources.");

                    // Get byte[] from the file from embedded resource
                    ba = new byte[(int)stm.Length];
                    stm.Read(ba, 0, (int)stm.Length);
                    try
                    {
                        asm = Assembly.Load(ba);

                        // Add the assembly/dll into dictionary
                        dic.Add(asm.FullName, asm);
                        return;
                    }
                    catch
                    {
                        // Purposely do nothing
                        // Unmanaged dll or assembly cannot be loaded directly from byte[]
                        // Let the process fall through for next part
                    }
                }


                var tempFile = Path.GetTempFileName();

                System.IO.File.WriteAllBytes(tempFile, ba);

                asm = Assembly.LoadFile(tempFile);

                dic.Add(asm.FullName, asm);
            }

            public static Assembly Get(string assemblyFullName)
            {
                if (dic == null || dic.Count == 0)
                    return null;

                if (dic.ContainsKey(assemblyFullName))
                    return dic[assemblyFullName];

                return null;
            }
        }
    }
}
