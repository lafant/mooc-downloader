﻿using Gecko;
using MoocDownloader.App.Views;
using Serilog;
using System;
using System.IO;
using System.Windows.Forms;

namespace MoocDownloader.App
{
    internal static class Program
    {
        /// <summary>
        /// gecko kernel.
        /// </summary>
        public const string FIREFOX_PATH = "firefox";

        public const string FFMPEG_EXE = @".\ffmpeg\ffmpeg.exe";

        /// <summary>
        /// gecko kernel temp path.
        /// </summary>
        public const string FIREFOX_TEMP = "temp";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            var profilePath = Path.Combine(Application.StartupPath, FIREFOX_TEMP);

            if (!Directory.Exists(profilePath))
            {
                Directory.CreateDirectory(profilePath);
            }
            else
            {
#if DEBUG
                // do not delete temp files in DEBUG mode.
#else
                DeleteFiles(profilePath);
#endif
            }

#if DEBUG
            Log.Logger = new LoggerConfiguration() // Logger.
                        .MinimumLevel.Verbose()
                        .WriteTo.File(@".\log\mooc.log", rollingInterval: RollingInterval.Day)
                        .CreateLogger();
#else
            Log.Logger = new LoggerConfiguration() // Logger.
                        .MinimumLevel.Information()
                        .WriteTo.File(@".\log\mooc.log", rollingInterval: RollingInterval.Day)
                        .CreateLogger();
#endif

            Xpcom.ProfileDirectory = profilePath;

            try // Check if the Firefox component exists.
            {
                Xpcom.Initialize(FIREFOX_PATH);
            }
            catch (Exception)
            {
                MessageBox.Show(
                    @"程序组件 ""FIREFOX"" 缺失, 请重新下载.", @"中国大学 MOOC 下载器", MessageBoxButtons.OK, MessageBoxIcon.Error
                );
                return;
            }

            if (!File.Exists(FFMPEG_EXE))
            {
                MessageBox.Show(
                    @"程序组件 ""FFMPEG"" 缺失, 请重新下载.", @"中国大学 MOOC 下载器", MessageBoxButtons.OK, MessageBoxIcon.Error
                );
                return;
            }

            Log.Information("=========程序启动=========");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                Application.Run(new MainForm());
            }
            catch (Exception exception)
            {
                Log.Error(exception, $@"程序运行发生错误: {exception.Message}");
                MessageBox.Show($@"程序运行发生错误: {exception.Message}");
            }
        }

        /// <summary>
        /// Delete ProfilePath Files
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static void DeleteFiles(string path)
        {
            var dir = new DirectoryInfo(path);

            try
            {
                foreach (var item in dir.GetFiles())
                {
                    File.Delete(item.FullName);
                }

                if (dir.GetDirectories().Length != 0)
                {
                    foreach (var item in dir.GetDirectories())
                    {
                        DeleteFiles(item.FullName);
                    }
                }

                if (Path.Combine(Application.StartupPath, FIREFOX_TEMP) != path)
                {
                    Directory.Delete(path);
                }
            }
            catch
            {
                // ignored
            }
        }
    }
}