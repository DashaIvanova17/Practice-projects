using ProductCatalogAIS.Data;
using ProductCatalogAIS.Models;
using System;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ProductCatalogAIS.Views
{
    public partial class CategoriesForm : Form
    {
        private DataTable categoriesTable;
        private SQLiteDataAdapter dataAdapter;

        public CategoriesForm()
        {
            InitializeComponent();
            LoadCategories();
            ConfigureBasedOnUserRole();
            SetupToolTips();
        }

        private void ConfigureBasedOnUserRole()
        {
            var user = LoginForm.CurrentUser;
            if (user.Role == "User")
            {
                btnAdd.Enabled = false;
                btnEdit.Enabled = false;
                btnDelete.Enabled = false;
            }
        }

        private void SetupToolTips()
        {
            toolTip.SetToolTip(txtSearch, "Введите текст для поиска категорий");
            toolTip.SetToolTip(btnSearch, "Выполнить поиск");
            toolTip.SetToolTip(btnRefresh, "Обновить список категорий");
            toolTip.SetToolTip(btnAdd, "Добавить новую категорию");
            toolTip.SetToolTip(btnEdit, "Редактировать выбранную категорию");
            toolTip.SetToolTip(btnDelete, "Удалить выбранную категорию");
        }

        private void LoadCategories()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT Id, Name, Description FROM Categories";

                    dataAdapter = new SQLiteDataAdapter(query, connection);
                    var commandBuilder = new SQLiteCommandBuilder(dataAdapter);

                    categoriesTable = new DataTable();
                    dataAdapter.Fill(categoriesTable);

                    dataGridViewCategories.DataSource = categoriesTable;

                    // Настройка столбцов
                    dataGridViewCategories.Columns["Id"].Visible = false;
                    dataGridViewCategories.Columns["Name"].HeaderText = "Название";
                    dataGridViewCategories.Columns["Description"].HeaderText = "Описание";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке категорий: {ex.Message}", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadCategories();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var editForm = new CategoryEditForm();
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                LoadCategories();
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dataGridViewCategories.SelectedRows.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите категорию для редактирования", "Информация",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedRow = dataGridViewCategories.SelectedRows[0];
            var category = new Category
            {
                Id = Convert.ToInt32(selectedRow.Cells["Id"].Value),
                Name = selectedRow.Cells["Name"].Value.ToString(),
                Description = selectedRow.Cells["Description"].Value.ToString()
            };

            var editForm = new CategoryEditForm(category);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                LoadCategories();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewCategories.SelectedRows.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите категорию для удаления", "Информация",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("Вы уверены, что хотите удалить выбранную категорию?", "Подтверждение",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    var selectedRow = dataGridViewCategories.SelectedRows[0];
                    int categoryId = Convert.ToInt32(selectedRow.Cells["Id"].Value);

                    using (var connection = DatabaseHelper.GetConnection())
                    {
                        connection.Open();

                        // Проверка, есть ли продукты в этой категории
                        string checkQuery = "SELECT COUNT(*) FROM Products WHERE CategoryId = @CategoryId";
                        using (var checkCommand = new SQLiteCommand(checkQuery, connection))
                        {
                            checkCommand.Parameters.AddWithValue("@CategoryId", categoryId);
                            int productCount = Convert.ToInt32(checkCommand.ExecuteScalar());

                            if (productCount > 0)
                            {
                                MessageBox.Show("Невозможно удалить категорию, так как в ней есть продукты. " +
                                              "Сначала удалите или переместите все продукты из этой категории.",
                                              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }

                        // Удаление категории
                        string deleteQuery = "DELETE FROM Categories WHERE Id = @Id";
                        using (var command = new SQLiteCommand(deleteQuery, connection))
                        {
                            command.Parameters.AddWithValue("@Id", categoryId);
                            command.ExecuteNonQuery();
                        }
                    }

                    LoadCategories();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении категории: {ex.Message}", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.Trim();

            if (!string.IsNullOrEmpty(searchText))
            {
                string filter = $"Name LIKE '%{searchText}%' OR Description LIKE '%{searchText}%'";
                categoriesTable.DefaultView.RowFilter = filter;
            }
            else
            {
                categoriesTable.DefaultView.RowFilter = "";
            }
        }

        private void dataGridViewCategories_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                btnEdit_Click(sender, e);
            }
        }
    }
}