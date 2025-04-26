using ProductCatalogAIS.Data;
using ProductCatalogAIS.Models;
using System;
using System.Data.SQLite;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ProductCatalogAIS.Views
{
    public partial class UserEditForm : Form
    {
        private User _user;
        private bool _isNewUser;

        public UserEditForm()
        {
            InitializeComponent();
            _isNewUser = true;
            _user = new User();
            SetupToolTips();
        }

        public UserEditForm(User user) : this()
        {
            _isNewUser = false;
            _user = user;
            LoadUserData();
        }

        private void SetupToolTips()
        {
            toolTip.SetToolTip(txtLogin, "Введите логин пользователя");
            toolTip.SetToolTip(txtPassword, "Введите пароль пользователя");
            toolTip.SetToolTip(txtConfirmPassword, "Подтвердите пароль пользователя");
            toolTip.SetToolTip(txtFullName, "Введите ФИО пользователя");
            toolTip.SetToolTip(cmbRole, "Выберите роль пользователя");
            toolTip.SetToolTip(btnSave, "Сохранить изменения");
            toolTip.SetToolTip(btnCancel, "Отменить изменения");
        }

        private void LoadUserData()
        {
            txtLogin.Text = _user.Login;
            txtFullName.Text = _user.FullName;
            cmbRole.SelectedItem = _user.Role;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();

                    if (_isNewUser)
                    {
                        InsertUser(connection);
                    }
                    else
                    {
                        UpdateUser(connection);
                    }

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении пользователя: {ex.Message}", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtLogin.Text))
            {
                MessageBox.Show("Пожалуйста, введите логин", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (_isNewUser && string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Пожалуйста, введите пароль", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!string.IsNullOrWhiteSpace(txtPassword.Text) && txtPassword.Text != txtConfirmPassword.Text)
            {
                MessageBox.Show("Пароли не совпадают", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Пожалуйста, введите ФИО", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (cmbRole.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите роль", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private void InsertUser(SQLiteConnection connection)
        {
            string query = "INSERT INTO Users (Login, Password, Role, FullName) VALUES (@Login, @Password, @Role, @FullName)";

            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Login", txtLogin.Text.Trim());
                command.Parameters.AddWithValue("@Password", txtPassword.Text);
                command.Parameters.AddWithValue("@Role", cmbRole.SelectedItem.ToString());
                command.Parameters.AddWithValue("@FullName", txtFullName.Text.Trim());
                command.ExecuteNonQuery();
            }
        }

        private void UpdateUser(SQLiteConnection connection)
        {
            string query;
            if (!string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                query = "UPDATE Users SET Login = @Login, Password = @Password, Role = @Role, FullName = @FullName WHERE Id = @Id";
            }
            else
            {
                query = "UPDATE Users SET Login = @Login, Role = @Role, FullName = @FullName WHERE Id = @Id";
            }

            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", _user.Id);
                command.Parameters.AddWithValue("@Login", txtLogin.Text.Trim());

                if (!string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    command.Parameters.AddWithValue("@Password", txtPassword.Text);
                }

                command.Parameters.AddWithValue("@Role", cmbRole.SelectedItem.ToString());
                command.Parameters.AddWithValue("@FullName", txtFullName.Text.Trim());
                command.ExecuteNonQuery();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void UserEditForm_Load(object sender, EventArgs e)
        {
            cmbRole.Items.Add("Admin");
            cmbRole.Items.Add("User");

            if (_isNewUser)
            {
                cmbRole.SelectedIndex = 1; // По умолчанию выбираем "User"
            }
        }
    }
}