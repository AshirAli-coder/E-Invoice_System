using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Invoice_system.Models
{
    [Table("sale_details")]
    public class Sale
    {
        [Key]
        [Column("id")]
        public int id { get; set; }

        [Column("customer_name")]
        public string? customer_name { get; set; }
        [Column("date")]
        public DateTime date { get; set; }
   
        [Column("prod_name/service")]
        public string? prod_name_service { get; set; }

        [Column("barcode")]
        public string? barcode { get; set; }

        [Column("qty/unit_type")]
        public string? qty_unit_type { get; set; }

        [Column("price")]
        public decimal price { get; set; }

        [Column("discount")]
        public decimal discount { get; set; }
        [Column("expiry_date")]
        public DateTime? expiry_date { get; set; }

        [Column("total_price")]
        public decimal total_price { get; set; }
        [Column("description")]
        public string? description { get; set; }

        [Column("payment_method")]
        public string? payment_method { get; set; }
        [Column("status")]
        public string? status { get; set; }





    }
}
