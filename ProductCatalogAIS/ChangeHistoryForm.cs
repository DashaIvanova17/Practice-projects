using ProductCatalogAIS.Data;
using System;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;

namespace ProductCatalogAIS.Views
{
    public partial class ChangeHistoryForm : Form
    {
        public ChangeHistoryForm()
        {
            InitializeComponent();
            LoadChangeHistory();
            SetupToolTips();
        }

        private void SetupToolTips()
        {
            toolTip.SetToolTip(btnRefresh, "Обновить список изменений");
            toolTip.SetToolTip(btnClose, "Закрыть форму истории изменений");
        }

        private void LoadChangeHistory()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();

                    // Проверяем существование таблицы
                    string checkTable = "SELECT name FROM sqlite_master WHERE type='table' AND name='ChangeHistory'";
                    using (var checkCommand = new SQLiteCommand(checkTable, connection))
                    {
                        if (checkCommand.ExecuteScalar() == null)
                        {
                            MessageBox.Show("Таблица истории изменений не найдена в базе данных.", "Информация",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                    }

                    string query = @"
                    SELECT ch.Id, 
                           p.Name as ProductName, 
                           u.FullName as UserName, 
                           ch.ChangeType, 
                           ch.ChangeDetails, 
                           ch.ChangedAt
                    FROM ChangeHistory ch
                    JOIN Products p ON ch.ProductId = p.Id
                    JOIN Users u ON ch.UserId = u.Id
                    ORDER BY ch.ChangedAt DESC";

                    using (var adapter = new SQLiteDataAdapter(query, connection))
                    {
                        DataTable table = new DataTable();
                        adapter.Fill(table);

                        dataGridViewHistory.DataSource = table;

                        // Настройка столбцов
                        dataGridViewHistory.Columns["Id"].Visible = false;
                        dataGridViewHistory.Columns["ProductName"].HeaderText = "Продукт";
                        dataGridViewHistory.Columns["UserName"].HeaderText = "Пользователь";
                        dataGridViewHistory.Columns["ChangeType"].HeaderText = "Тип изменения";
                        dataGridViewHistory.Columns["ChangeDetails"].HeaderText = "Детали изменений";
                        dataGridViewHistory.Columns["ChangedAt"].HeaderText = "Дата изменения";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке истории изменений: {ex.Message}", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadChangeHistory();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}