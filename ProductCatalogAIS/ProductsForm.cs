using ProductCatalogAIS.Data;
using ProductCatalogAIS.Models;
using System;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;

namespace ProductCatalogAIS.Views
{
    public partial class ProductsForm : Form
    {
        private DataTable productsTable;
        private SQLiteDataAdapter dataAdapter;

        public ProductsForm()
        {
            InitializeComponent();
            LoadCategories();
            LoadProducts();
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
            toolTip.SetToolTip(txtSearch, "Введите текст для поиска продуктов");
            toolTip.SetToolTip(btnSearch, "Выполнить поиск");
            toolTip.SetToolTip(btnRefresh, "Обновить список продуктов");
            toolTip.SetToolTip(btnAdd, "Добавить новый продукт");
            toolTip.SetToolTip(btnEdit, "Редактировать выбранный продукт");
            toolTip.SetToolTip(btnDelete, "Удалить выбранный продукт");
            toolTip.SetToolTip(cmbCategoriesFilter, "Фильтр по категории");
        }

        private void LoadCategories()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT Id, Name FROM Categories ORDER BY Name";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            cmbCategoriesFilter.Items.Clear();
                            cmbCategoriesFilter.Items.Add(new { Id = 0, Name = "Все категории" });

                            while (reader.Read())
                            {
                                cmbCategoriesFilter.Items.Add(new
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    Name = reader["Name"].ToString()
                                });
                            }

                            cmbCategoriesFilter.DisplayMember = "Name";
                            cmbCategoriesFilter.ValueMember = "Id";
                            cmbCategoriesFilter.SelectedIndex = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке категорий: {ex.Message}", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadProducts()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = @"
                    SELECT p.Id, p.Name, p.Description, p.Price, p.CategoryId, 
                           p.StockQuantity, p.Manufacturer, c.Name as CategoryName
                    FROM Products p
                    JOIN Categories c ON p.CategoryId = c.Id";

                    dataAdapter = new SQLiteDataAdapter(query, connection);
                    var commandBuilder = new SQLiteCommandBuilder(dataAdapter);

                    productsTable = new DataTable();
                    dataAdapter.Fill(productsTable);

                    dataGridViewProducts.DataSource = productsTable;

                    // Настройка столбцов
                    dataGridViewProducts.Columns["Id"].Visible = false;
                    dataGridViewProducts.Columns["CategoryId"].Visible = false;
                    dataGridViewProducts.Columns["Name"].HeaderText = "Название";
                    dataGridViewProducts.Columns["Description"].HeaderText = "Описание";
                    dataGridViewProducts.Columns["Price"].HeaderText = "Цена";
                    dataGridViewProducts.Columns["StockQuantity"].HeaderText = "Количество";
                    dataGridViewProducts.Columns["Manufacturer"].HeaderText = "Производитель";
                    dataGridViewProducts.Columns["CategoryName"].HeaderText = "Категория";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке продуктов: {ex.Message}", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadProducts();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var editForm = new ProductEditForm();
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                int newProductId = GetLastInsertedProductId();
                if (newProductId > 0)
                {
                    LogProductChange(newProductId, "create", "Добавлен новый продукт");
                }
                LoadProducts();
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dataGridViewProducts.SelectedRows.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите продукт для редактирования", "Информация",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedRow = dataGridViewProducts.SelectedRows[0];
            var product = new Product
            {
                Id = Convert.ToInt32(selectedRow.Cells["Id"].Value),
                Name = selectedRow.Cells["Name"].Value.ToString(),
                Description = selectedRow.Cells["Description"].Value.ToString(),
                Price = Convert.ToDecimal(selectedRow.Cells["Price"].Value),
                CategoryId = Convert.ToInt32(selectedRow.Cells["CategoryId"].Value),
                StockQuantity = Convert.ToInt32(selectedRow.Cells["StockQuantity"].Value),
                Manufacturer = selectedRow.Cells["Manufacturer"].Value.ToString()
            };

            var editForm = new ProductEditForm(product);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                LogProductChange(product.Id, "update", "Обновлены данные продукта");
                LoadProducts();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewProducts.SelectedRows.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите продукт для удаления", "Информация",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedRow = dataGridViewProducts.SelectedRows[0];
            int productId = Convert.ToInt32(selectedRow.Cells["Id"].Value);
            string productName = selectedRow.Cells["Name"].Value.ToString();

            if (MessageBox.Show($"Вы уверены, что хотите удалить продукт '{productName}'?", "Подтверждение",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    LogProductChange(productId, "delete", $"Удален продукт: {productName}");

                    using (var connection = DatabaseHelper.GetConnection())
                    {
                        connection.Open();
                        string query = "DELETE FROM Products WHERE Id = @Id";
                        using (var command = new SQLiteCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Id", productId);
                            command.ExecuteNonQuery();
                        }
                    }

                    LoadProducts();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении продукта: {ex.Message}", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                string searchText = txtSearch.Text.Trim();
                int categoryId = 0;

                if (cmbCategoriesFilter.SelectedIndex > 0)
                {
                    dynamic selectedItem = cmbCategoriesFilter.SelectedItem;
                    categoryId = selectedItem.Id;
                }

                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = @"
                    SELECT p.Id, p.Name, p.Description, p.Price, p.CategoryId, 
                           p.StockQuantity, p.Manufacturer, c.Name as CategoryName
                    FROM Products p
                    JOIN Categories c ON p.CategoryId = c.Id
                    WHERE (p.Name LIKE @SearchText OR p.Description LIKE @SearchText OR p.Manufacturer LIKE @SearchText)";

                    if (categoryId > 0)
                    {
                        query += " AND p.CategoryId = @CategoryId";
                    }

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SearchText", $"%{searchText}%");
                        if (categoryId > 0)
                        {
                            command.Parameters.AddWithValue("@CategoryId", categoryId);
                        }

                        DataTable searchResults = new DataTable();
                        using (var adapter = new SQLiteDataAdapter(command))
                        {
                            adapter.Fill(searchResults);
                            dataGridViewProducts.DataSource = searchResults;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при поиске: {ex.Message}", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataGridViewProducts_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                btnEdit_Click(sender, e);
            }
        }

        private int GetLastInsertedProductId()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT last_insert_rowid()";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
            catch
            {
                return -1;
            }
        }

        private void LogProductChange(int productId, string changeType, string details)
        {
            try
            {
                DatabaseHelper.LogChange(
                    productId,
                    LoginForm.CurrentUser.Id,
                    changeType,
                    $"{DateTime.Now}: {details}\nПользователь: {LoginForm.CurrentUser.FullName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка записи в историю: {ex.Message}");
            }
        }
    }
}