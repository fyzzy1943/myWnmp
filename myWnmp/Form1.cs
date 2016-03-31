using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace myWnmp
{
    public partial class Form1 : Form
    {
        // windows is show or not now
        protected bool isShow = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("start");
            if (Process.GetProcessesByName("nginx").Length > 0
                || Process.GetProcessesByName("php-cgi").Length > 0)
            {
                return;
            }

            ExecBatCommand(p =>
            {
                p(@"D:\web\nginx-1.9.12\RunHiddenConsole.exe D:\web\php-7.0.4-nts-Win32-VC14-x64\php-cgi.exe -b 127.0.0.1:9000-c D:\web\php-7.0.4-nts-Win32-VC14-x64\php.ini");
                p(@"D:\web\nginx-1.9.12\RunHiddenConsole.exe D:\web\nginx-1.9.12\nginx.exe -p D:\web\nginx-1.9.12");
                p("exit 0");
            });
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecBatCommand(p =>
            {
                p(@"taskkill /F /IM nginx.exe > nul");
                p(@"taskkill /F /IM php-cgi.exe > nul");
                p("exit 0");
            });
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            int nginx = 0, php = 0, mysql = 0;
            string status = "";

            while (true)
            {
                status = "";

                nginx = Process.GetProcessesByName("nginx").Length;
                status += "nginx is " + (nginx > 0 ? "running" : "stop")
                    + ", " + nginx.ToString() + "process.\n";

                php = Process.GetProcessesByName("php-cgi").Length;
                status += "php is " + (php > 0 ? "running" : "stop")
                    + ", " + php.ToString() + "process.\n";

                mysql = Process.GetProcessesByName("mysqld").Length;
                status += "mysql is " + (mysql > 0 ? "running" : "stop")
                    + ", " + mysql.ToString() + "process.\n";

                CheckStatus(status);
                System.Threading.Thread.Sleep(500);
            }
        }

        private delegate void CheckStatusDelegate(string content);

        private void CheckStatus(string content)
        {
            if (label1.InvokeRequired)
            {
                label1.Invoke(new CheckStatusDelegate(CheckStatus), content);
            }
            else
            {
                label1.Text = content;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync();
            this.Visible = false;
            this.ShowInTaskbar = false;
        }

        /// <summary>
        /// 打开控制台执行拼接完成的批处理命令字符串
        /// </summary>
        /// <param name="inputAction">需要执行的命令委托方法：每次调用 <paramref name="inputAction"/> 中的参数都会执行一次</param>
        private static void ExecBatCommand(Action<Action<string>> inputAction)
        {
            Process pro = null;
            StreamWriter sIn = null;
            StreamReader sOut = null;

            try
            {
                pro = new Process();
                pro.StartInfo.FileName = "cmd.exe";
                pro.StartInfo.UseShellExecute = false;
                pro.StartInfo.CreateNoWindow = true;
                pro.StartInfo.RedirectStandardInput = true;
                pro.StartInfo.RedirectStandardOutput = true;
                pro.StartInfo.RedirectStandardError = true;

                pro.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
                pro.ErrorDataReceived += (sender, e) => Console.WriteLine(e.Data);

                pro.Start();
                sIn = pro.StandardInput;
                sIn.AutoFlush = true;

                pro.BeginOutputReadLine();
                inputAction(value => sIn.WriteLine(value));

                pro.WaitForExit();
            }
            finally
            {
                if (pro != null && !pro.HasExited)
                    pro.Kill();
                if (sIn != null)
                    sIn.Close();
                if (sOut != null)
                    sOut.Close();
                if (pro != null)
                    pro.Close();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Visible = false;
            this.ShowInTaskbar = false;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false;
            System.Environment.Exit(0);
        }

        private void notifyIcon1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (isShow == true)
                {
                    this.ShowInTaskbar = false;
                    this.Visible = false;
                    isShow = false;
                }
                else
                {
                    this.WindowState = FormWindowState.Normal;
                    this.ShowInTaskbar = true;
                    this.Visible = true;
                    this.Activate();
                    isShow = true;
                }
            }
        }
    }
}
