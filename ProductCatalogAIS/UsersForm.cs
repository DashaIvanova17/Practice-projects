using ProductCatalogAIS.Data;
using ProductCatalogAIS.Models;
using System;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ProductCatalogAIS.Views
{
    public partial class UsersForm : Form
    {
        private DataTable usersTable;
        private SQLiteDataAdapter dataAdapter;

        public UsersForm()
        {
            InitializeComponent();
            LoadUsers();
            SetupToolTips();
        }

        private void SetupToolTips()
        {
            toolTip.SetToolTip(txtSearch, "Введите текст для поиска пользователей");
            toolTip.SetToolTip(btnSearch, "Выполнить поиск");
            toolTip.SetToolTip(btnRefresh, "Обновить список пользователей");
            toolTip.SetToolTip(btnAdd, "Добавить нового пользователя");
            toolTip.SetToolTip(btnEdit, "Редактировать выбранного пользователя");
            toolTip.SetToolTip(btnDelete, "Удалить выбранного пользователя");
        }

        private void LoadUsers()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT Id, Login, Role, FullName FROM Users";

                    dataAdapter = new SQLiteDataAdapter(query, connection);
                    var commandBuilder = new SQLiteCommandBuilder(dataAdapter);

                    usersTable = new DataTable();
                    dataAdapter.Fill(usersTable);

                    dataGridViewUsers.DataSource = usersTable;

                    // Настройка столбцов
                    dataGridViewUsers.Columns["Id"].Visible = false;
                    dataGridViewUsers.Columns["Login"].HeaderText = "Логин";
                    dataGridViewUsers.Columns["Role"].HeaderText = "Роль";
                    dataGridViewUsers.Columns["FullName"].HeaderText = "ФИО";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке пользователей: {ex.Message}", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadUsers();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var editForm = new UserEditForm();
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                LoadUsers();
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dataGridViewUsers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите пользователя для редактирования", "Информация",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedRow = dataGridViewUsers.SelectedRows[0];
            var user = new User
            {
                Id = Convert.ToInt32(selectedRow.Cells["Id"].Value),
                Login = selectedRow.Cells["Login"].Value.ToString(),
                Role = selectedRow.Cells["Role"].Value.ToString(),
                FullName = selectedRow.Cells["FullName"].Value.ToString()
            };

            var editForm = new UserEditForm(user);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                LoadUsers();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewUsers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите пользователя для удаления", "Информация",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedRow = dataGridViewUsers.SelectedRows[0];
            string login = selectedRow.Cells["Login"].Value.ToString();

            if (login == "admin")
            {
                MessageBox.Show("Нельзя удалить администратора системы", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show("Вы уверены, что хотите удалить выбранного пользователя?", "Подтверждение",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    int userId = Convert.ToInt32(selectedRow.Cells["Id"].Value);

                    using (var connection = DatabaseHelper.GetConnection())
                    {
                        connection.Open();
                        string query = "DELETE FROM Users WHERE Id = @Id";
                        using (var command = new SQLiteCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Id", userId);
                            command.ExecuteNonQuery();
                        }
                    }

                    LoadUsers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении пользователя: {ex.Message}", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.Trim();

            if (!string.IsNullOrEmpty(searchText))
            {
                string filter = $"Login LIKE '%{searchText}%' OR FullName LIKE '%{searchText}%' OR Role LIKE '%{searchText}%'";
                usersTable.DefaultView.RowFilter = filter;
            }
            else
            {
                usersTable.DefaultView.RowFilter = "";
            }
        }

        private void dataGridViewUsers_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                btnEdit_Click(sender, e);
            }
        }
    }
}