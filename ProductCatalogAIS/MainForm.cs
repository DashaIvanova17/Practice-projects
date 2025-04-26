using ProductCatalogAIS.Models;
using ProductCatalogAIS.Views;
using System;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ProductCatalogAIS
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            ConfigureBasedOnUserRole();
            SetupToolTips();
        }

        private void ConfigureBasedOnUserRole()
        {
            var user = LoginForm.CurrentUser;
            this.Text = $"Каталог продукции - {user.FullName} ({user.Role})";

            // Настройка доступных функций в зависимости от роли
            if (user.Role == "User")
            {
                btnManageUsers.Enabled = false;
                btnManageUsers.Visible = false;
                // Сдвигаем остальные кнопки вверх
                btnHelp.Location = btnManageUsers.Location;
                btnChangeHistory.Location = btnHelp.Location;
                btnExit.Location = btnChangeHistory.Location;
            }
        }

        private void SetupToolTips()
        {
            toolTip.SetToolTip(btnProducts, "Просмотр и управление продуктами");
            toolTip.SetToolTip(btnCategories, "Просмотр и управление категориями");
            toolTip.SetToolTip(btnManageUsers, "Управление пользователями (только для администраторов)");
            toolTip.SetToolTip(btnHelp, "Открыть справочное руководство");
            toolTip.SetToolTip(btnChangeHistory, "Просмотр истории изменений в системе");
            toolTip.SetToolTip(btnExit, "Выйти из системы");
        }

        private void btnProducts_Click(object sender, EventArgs e)
        {
            var productsForm = new ProductsForm();
            productsForm.ShowDialog();
        }

        private void btnCategories_Click(object sender, EventArgs e)
        {
            var categoriesForm = new CategoriesForm();
            categoriesForm.ShowDialog();
        }

        private void btnManageUsers_Click(object sender, EventArgs e)
        {
            var usersForm = new UsersForm();
            usersForm.ShowDialog();
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            var helpForm = new HelpForm();
            helpForm.ShowDialog();
        }

        private void btnChangeHistory_Click(object sender, EventArgs e)
        {
            var historyForm = new ChangeHistoryForm();
            historyForm.ShowDialog();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }
}