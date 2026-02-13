using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Invoice_system.Models
{
    [Table("products_services")]
    public class ProductService
    {
        [Key]
        [Column("id")]
        public int id { get; set; }

        [Column("prod_name/service")]
        public string? prod_name_service { get; set; }

        [Column("barcode")]
        public string? barcode { get; set; }

        [Column("qty/unit_type")]
        public string? qty_unit_type { get; set; }

        [Column("price")]
        public decimal price { get; set; }

        [Column("description")]
        public string? description { get; set; }

        [Column("discount")]
        public decimal discount { get; set; }

        [Column("tax")]
        public decimal tax { get; set; }

        [Column("image")]
        public string? image { get; set; }

        [Column("status")]
        public string? status { get; set; }

        [Column("date")]
        public DateTime date { get; set; }

        [Column("expiry_date")]
        public DateTime? expiry_date { get; set; }
    }
}
