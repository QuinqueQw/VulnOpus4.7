using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using OfficeOpenXml;
using Vullnerability.db;

namespace Vullnerability
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // EPPlus 8 требует выставить лицензию ДО создания любого ExcelPackage,
            // поэтому ставим её здесь, в самом начале, а не в Form1
            ExcelPackage.License.SetNonCommercialPersonal("VulnOpus");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // создаём БД при первом запуске, чтобы EF потом не упал
            SqliteBootstrap.EnsureDatabase();
            Application.Run(new Form1());
        }
    }
}
