using System;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using CoreTool.Archive;

namespace CoreTool
{
    class Program
    {
        private static Timer updateTimer;
        private static GitSync gitSync;

        static async Task Main(string[] args)
        {
            if (Config.Loader.Config.GitSync.Enabled)
            {
                gitSync = new GitSync();
                await gitSync.Load();
            }

            // Load data
            foreach (ArchiveMeta archive in Config.Loader.Config.ArchiveInstances)
            {
                await archive.Load();
            }

            // Do checks and download missing files
            foreach (ArchiveMeta archive in Config.Loader.Config.ArchiveInstances)
            {
                await archive.Check();
            }

            // Run GitSync if enabled
            if (Config.Loader.Config.GitSync.Enabled)
            {
                await gitSync.Check();
            }

            Utils.GenericLogger.Write("Done startup!");

            Utils.GenericLogger.Write("Starting update checker");

            // Check for updates every 5 mins
            updateTimer = new Timer(1 * 60 * 1000);
            updateTimer.Elapsed += OnUpdateEvent;
            updateTimer.AutoReset = true;
            updateTimer.Enabled = true;

            Utils.GenericLogger.Write("Will exit after 60 seconds.");

            //notify system
            {
                string str = "python notification.py";

                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
                p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
                p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
                p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
                p.StartInfo.CreateNoWindow = true;//不显示程序窗口
                p.Start();//启动程序

                //向cmd窗口发送输入信息
                p.StandardInput.WriteLine(str + "&exit");

                p.StandardInput.WriteLine("taskkill /f /im coretool.exe" + "&exit");

                p.StandardInput.AutoFlush = true;
                //p.StandardInput.WriteLine("exit");
                //向标准输入写入要执行的命令。这里使用&是批处理命令的符号，表示前面一个命令不管是否执行成功都执行后面(exit)命令，如果不执行exit命令，后面调用ReadToEnd()方法会假死
                //同类的符号还有&&和||前者表示必须前一个命令执行成功才会执行后面的命令，后者表示必须前一个命令执行失败才会执行后面的命令



                //获取cmd窗口的输出信息
                string output = p.StandardOutput.ReadToEnd();

                //StreamReader reader = p.StandardOutput;
                //string line=reader.ReadLine();
                //while (!reader.EndOfStream)
                //{
                //    str += line + "  ";
                //    line = reader.ReadLine();
                //}

                p.WaitForExit();//等待程序执行完退出进程
                p.Close();


                Console.WriteLine(output);
            }

            //Utils.GenericLogger.Write("Press enter to exit at any point");
            //Console.ReadLine();
            /*
            //delete AuthInfo
            if (File.Exists(Path.GetFullPath("msAuthInfo.json")))
                File.Delete(Path.GetFullPath("msAuthInfo.json"));
            */
        }

        private static async void OnUpdateEvent(object sender, ElapsedEventArgs e)
        {
            // Stop the timer before we check
            // This stops the timer ticking while we are still checking
            updateTimer.Enabled = false;

            Utils.GenericLogger.Write("Checking for updates...");

            foreach (ArchiveMeta archive in Config.Loader.Config.ArchiveInstances)
            {
                await archive.Load();
                await archive.Check();
            }

            // Run GitSync if enabled
            if (Config.Loader.Config.GitSync.Enabled)
            {
                await gitSync.Check();
            }

            // Resume the timer
            updateTimer.Enabled = true;
        }
    }
}
