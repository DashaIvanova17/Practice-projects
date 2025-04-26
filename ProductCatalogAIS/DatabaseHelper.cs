using System;
using System.Data.SQLite;
using System.IO;

namespace ProductCatalogAIS.Data
{
    public static class DatabaseHelper
    {
        private static string databaseFileName = "ProductCatalog.db";
        private static string connectionString = $"Data Source={databaseFileName};Version=3;";

        public static void InitializeDatabase()
        {
            bool isNewDatabase = !File.Exists(databaseFileName);

            if (isNewDatabase)
            {
                SQLiteConnection.CreateFile(databaseFileName);
            }

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                if (isNewDatabase)
                {
                    CreateAllTables(connection);
                    AddInitialData(connection);
                }
                else
                {
                    CheckAndCreateTables(connection);
                }
            }
        }

        private static void CreateAllTables(SQLiteConnection connection)
        {
            // Таблица пользователей
            string createUsersTable = @"
            CREATE TABLE Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Login TEXT NOT NULL UNIQUE,
                Password TEXT NOT NULL,
                Role TEXT NOT NULL,
                FullName TEXT NOT NULL
            );";
            new SQLiteCommand(createUsersTable, connection).ExecuteNonQuery();

            // Таблица категорий
            string createCategoriesTable = @"
            CREATE TABLE Categories (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Description TEXT
            );";
            new SQLiteCommand(createCategoriesTable, connection).ExecuteNonQuery();

            // Таблица продуктов
            string createProductsTable = @"
            CREATE TABLE Products (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Description TEXT,
                Price DECIMAL NOT NULL,
                CategoryId INTEGER NOT NULL,
                StockQuantity INTEGER NOT NULL,
                Manufacturer TEXT,
                Image BLOB,
                FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
            );";
            new SQLiteCommand(createProductsTable, connection).ExecuteNonQuery();

            // Таблица истории изменений
            string createChangeHistoryTable = @"
            CREATE TABLE ChangeHistory (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ProductId INTEGER NOT NULL,
                UserId INTEGER NOT NULL,
                ChangeType TEXT NOT NULL CHECK(ChangeType IN ('create', 'update', 'delete')),
                ChangeDetails TEXT,
                ChangedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (ProductId) REFERENCES Products(Id),
                FOREIGN KEY (UserId) REFERENCES Users(Id)
            );";
            new SQLiteCommand(createChangeHistoryTable, connection).ExecuteNonQuery();
        }

        private static void CheckAndCreateTables(SQLiteConnection connection)
        {
            // Проверка и создание таблицы ChangeHistory, если её нет
            string checkTableExists = "SELECT name FROM sqlite_master WHERE type='table' AND name='ChangeHistory'";
            using (var command = new SQLiteCommand(checkTableExists, connection))
            {
                var result = command.ExecuteScalar();
                if (result == null)
                {
                    string createChangeHistoryTable = @"
                    CREATE TABLE ChangeHistory (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        ProductId INTEGER NOT NULL,
                        UserId INTEGER NOT NULL,
                        ChangeType TEXT NOT NULL CHECK(ChangeType IN ('create', 'update', 'delete')),
                        ChangeDetails TEXT,
                        ChangedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY (ProductId) REFERENCES Products(Id),
                        FOREIGN KEY (UserId) REFERENCES Users(Id)
                    );";
                    new SQLiteCommand(createChangeHistoryTable, connection).ExecuteNonQuery();
                }
            }
        }

        private static void AddInitialData(SQLiteConnection connection)
        {
            // Добавление администратора
            string addAdmin = @"
            INSERT INTO Users (Login, Password, Role, FullName)
            VALUES ('admin', 'admin123', 'Admin', 'Администратор системы');";
            new SQLiteCommand(addAdmin, connection).ExecuteNonQuery();

            // Добавление обычного пользователя
            string addUser = @"
            INSERT INTO Users (Login, Password, Role, FullName)
            VALUES ('user', 'user123', 'User', 'Обычный пользователь');";
            new SQLiteCommand(addUser, connection).ExecuteNonQuery();

            // Добавление тестовых категорий
            string addCategories = @"
            INSERT INTO Categories (Name, Description) VALUES ('Электроника', 'Электронные устройства и компоненты');
            INSERT INTO Categories (Name, Description) VALUES ('Одежда', 'Одежда и аксессуары');
            INSERT INTO Categories (Name, Description) VALUES ('Продукты питания', 'Продукты и напитки');";
            new SQLiteCommand(addCategories, connection).ExecuteNonQuery();

            // Добавление тестовых продуктов
            string addProducts = @"
            INSERT INTO Products (Name, Description, Price, CategoryId, StockQuantity, Manufacturer)
            VALUES ('Смартфон X', 'Новейший смартфон с отличной камерой', 599.99, 1, 50, 'ТехноКорп');
            
            INSERT INTO Products (Name, Description, Price, CategoryId, StockQuantity, Manufacturer)
            VALUES ('Джинсы', 'Классические синие джинсы', 49.99, 2, 100, 'МодныйДом');
            
            INSERT INTO Products (Name, Description, Price, CategoryId, StockQuantity, Manufacturer)
            VALUES ('Яблоки', 'Свежие яблоки, 1 кг', 2.99, 3, 200, 'ФруктовыйСад');";
            new SQLiteCommand(addProducts, connection).ExecuteNonQuery();

            // Добавление тестовых записей в историю изменений
            string addChangeHistory = @"
            INSERT INTO ChangeHistory (ProductId, UserId, ChangeType, ChangeDetails)
            VALUES (1, 1, 'create', 'Добавлен новый продукт: Смартфон X');
            
            INSERT INTO ChangeHistory (ProductId, UserId, ChangeType, ChangeDetails)
            VALUES (2, 1, 'create', 'Добавлен новый продукт: Джинсы');
            
            INSERT INTO ChangeHistory (ProductId, UserId, ChangeType, ChangeDetails)
            VALUES (3, 2, 'create', 'Добавлен новый продукт: Яблоки');";
            new SQLiteCommand(addChangeHistory, connection).ExecuteNonQuery();
        }

        public static SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(connectionString);
        }

        public static void LogChange(int productId, int userId, string changeType, string changeDetails)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    string query = @"
                    INSERT INTO ChangeHistory (ProductId, UserId, ChangeType, ChangeDetails)
                    VALUES (@ProductId, @UserId, @ChangeType, @ChangeDetails)";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ProductId", productId);
                        command.Parameters.AddWithValue("@UserId", userId);
                        command.Parameters.AddWithValue("@ChangeType", changeType);
                        command.Parameters.AddWithValue("@ChangeDetails", changeDetails);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при записи в историю изменений: {ex.Message}");
            }
        }
    }
}