using System;
using System.Threading;
using System.Windows.Forms;

namespace tools
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]

        static void Main()
        {
            bool onlyInstance = false;
            Mutex mutex = new Mutex(true, Application.ProductName, out onlyInstance);
            if (!onlyInstance)
            {
                MessageBox.Show(null, "请不要同时运行多个本程序。\n\n这个程序即将退出。", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
            mutex.ReleaseMutex();
        }
    }
}
