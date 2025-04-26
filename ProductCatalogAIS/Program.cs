using ProductCatalogAIS.Views;
using System;
using System.Windows.Forms;

namespace ProductCatalogAIS
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Инициализация базы данных перед запуском приложения
            ProductCatalogAIS.Data.DatabaseHelper.InitializeDatabase();

            Application.Run(new LoginForm());
        }
    }
}