using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vullnerability.db;

namespace Vullnerability
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // создаём БД при первом запуске, чтобы EF потом не упал
            SqliteBootstrap.EnsureDatabase();
            Application.Run(new Form1());
        }
    }
}
