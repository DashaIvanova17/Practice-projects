using ProductCatalogAIS.Data;
using ProductCatalogAIS.Models;
using System;
using System.Data.SQLite;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ProductCatalogAIS.Views
{
    public partial class CategoryEditForm : Form
    {
        private Category _category;
        private bool _isNewCategory;

        public CategoryEditForm()
        {
            InitializeComponent();
            _isNewCategory = true;
            _category = new Category();
            SetupToolTips();
        }

        public CategoryEditForm(Category category) : this()
        {
            _isNewCategory = false;
            _category = category;
            LoadCategoryData();
        }

        private void SetupToolTips()
        {
            toolTip.SetToolTip(txtName, "Введите название категории");
            toolTip.SetToolTip(txtDescription, "Введите описание категории");
            toolTip.SetToolTip(btnSave, "Сохранить изменения");
            toolTip.SetToolTip(btnCancel, "Отменить изменения");
        }

        private void LoadCategoryData()
        {
            txtName.Text = _category.Name;
            txtDescription.Text = _category.Description;
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

                    if (_isNewCategory)
                    {
                        InsertCategory(connection);
                    }
                    else
                    {
                        UpdateCategory(connection);
                    }

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении категории: {ex.Message}", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Пожалуйста, введите название категории", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private void InsertCategory(SQLiteConnection connection)
        {
            string query = "INSERT INTO Categories (Name, Description) VALUES (@Name, @Description)";

            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Name", txtName.Text.Trim());
                command.Parameters.AddWithValue("@Description", txtDescription.Text.Trim());
                command.ExecuteNonQuery();
            }
        }

        private void UpdateCategory(SQLiteConnection connection)
        {
            string query = "UPDATE Categories SET Name = @Name, Description = @Description WHERE Id = @Id";

            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", _category.Id);
                command.Parameters.AddWithValue("@Name", txtName.Text.Trim());
                command.Parameters.AddWithValue("@Description", txtDescription.Text.Trim());
                command.ExecuteNonQuery();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}