namespace ProductCatalogAIS.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } // "Admin" или "User"
        public string FullName { get; set; }
    }
}