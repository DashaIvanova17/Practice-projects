using ProductCatalogAIS.Data;
using ProductCatalogAIS.Models;
using System;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ProductCatalogAIS.Views
{
    public partial class ProductEditForm : Form
    {
        private Product _product;
        private bool _isNewProduct;
        private byte[] _imageBytes;

        public ProductEditForm()
        {
            InitializeComponent();
            _isNewProduct = true;
            _product = new Product();
            LoadCategories();
            SetupToolTips();
        }

        public ProductEditForm(Product product) : this()
        {
            _isNewProduct = false;
            _product = product;
            LoadProductData();
        }

        private void SetupToolTips()
        {
            toolTip.SetToolTip(txtName, "Введите название продукта");
            toolTip.SetToolTip(txtDescription, "Введите описание продукта");
            toolTip.SetToolTip(txtPrice, "Введите цену продукта");
            toolTip.SetToolTip(txtStockQuantity, "Введите количество на складе");
            toolTip.SetToolTip(txtManufacturer, "Введите производителя");
            toolTip.SetToolTip(cmbCategory, "Выберите категорию");
            toolTip.SetToolTip(btnSelectImage, "Выберите изображение продукта");
            toolTip.SetToolTip(btnSave, "Сохранить изменения");
            toolTip.SetToolTip(btnCancel, "Отменить изменения");
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
                            cmbCategory.Items.Clear();

                            while (reader.Read())
                            {
                                cmbCategory.Items.Add(new Category
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    Name = reader["Name"].ToString()
                                });
                            }

                            if (cmbCategory.Items.Count > 0)
                                cmbCategory.SelectedIndex = 0;
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

        private void LoadProductData()
        {
            txtName.Text = _product.Name;
            txtDescription.Text = _product.Description;
            txtPrice.Text = _product.Price.ToString("0.00");
            txtStockQuantity.Text = _product.StockQuantity.ToString();
            txtManufacturer.Text = _product.Manufacturer;

            // Установка категории
            foreach (Category item in cmbCategory.Items)
            {
                if (item.Id == _product.CategoryId)
                {
                    cmbCategory.SelectedItem = item;
                    break;
                }
            }

            // Загрузка изображения
            if (_product.Image != null && _product.Image.Length > 0)
            {
                _imageBytes = _product.Image;
                using (var ms = new MemoryStream(_product.Image))
                {
                    pictureBoxProduct.Image = Image.FromStream(ms);
                }
            }
        }

        private void btnSelectImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
                openFileDialog.Title = "Выберите изображение продукта";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _imageBytes = File.ReadAllBytes(openFileDialog.FileName);
                        pictureBoxProduct.Image = Image.FromFile(openFileDialog.FileName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}", "Ошибка",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
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

                    if (_isNewProduct)
                    {
                        InsertProduct(connection);
                    }
                    else
                    {
                        UpdateProduct(connection);
                    }

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении продукта: {ex.Message}", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Пожалуйста, введите название продукта", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Пожалуйста, введите корректную цену", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!int.TryParse(txtStockQuantity.Text, out int quantity) || quantity < 0)
            {
                MessageBox.Show("Пожалуйста, введите корректное количество", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (cmbCategory.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите категорию", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private void InsertProduct(SQLiteConnection connection)
        {
            string query = @"
            INSERT INTO Products (Name, Description, Price, CategoryId, StockQuantity, Manufacturer, Image)
            VALUES (@Name, @Description, @Price, @CategoryId, @StockQuantity, @Manufacturer, @Image)";

            using (var command = new SQLiteCommand(query, connection))
            {
                SetCommandParameters(command);
                command.ExecuteNonQuery();
            }
        }

        private void UpdateProduct(SQLiteConnection connection)
        {
            string query = @"
            UPDATE Products 
            SET Name = @Name, 
                Description = @Description, 
                Price = @Price, 
                CategoryId = @CategoryId, 
                StockQuantity = @StockQuantity, 
                Manufacturer = @Manufacturer, 
                Image = @Image
            WHERE Id = @Id";

            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", _product.Id);
                SetCommandParameters(command);
                command.ExecuteNonQuery();
            }
        }

        private void SetCommandParameters(SQLiteCommand command)
        {
            command.Parameters.AddWithValue("@Name", txtName.Text.Trim());
            command.Parameters.AddWithValue("@Description", txtDescription.Text.Trim());
            command.Parameters.AddWithValue("@Price", decimal.Parse(txtPrice.Text));
            command.Parameters.AddWithValue("@CategoryId", ((Category)cmbCategory.SelectedItem).Id);
            command.Parameters.AddWithValue("@StockQuantity", int.Parse(txtStockQuantity.Text));
            command.Parameters.AddWithValue("@Manufacturer", txtManufacturer.Text.Trim());

            if (_imageBytes != null && _imageBytes.Length > 0)
                command.Parameters.AddWithValue("@Image", _imageBytes);
            else
                command.Parameters.AddWithValue("@Image", DBNull.Value);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}