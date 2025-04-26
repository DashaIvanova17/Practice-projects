// Файл Models/ChangeHistory.cs
using System;

namespace ProductCatalogAIS.Models
{
    public class ChangeHistory
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public string ChangeType { get; set; } // "create", "update", "delete"
        public string ChangeDetails { get; set; }
        public DateTime ChangedAt { get; set; }

        // Навигационные свойства
        public Product Product { get; set; }
        public User User { get; set; }
    }
}