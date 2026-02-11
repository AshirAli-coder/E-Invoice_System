using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Invoice_system.Models
{
    [Table("return_details")]
    public class ReturnDetail
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("original_sale_id")]
        public int OriginalSaleId { get; set; }

        [Column("customer_name")]
        public string? CustomerName { get; set; }

        [Column("product_name")]
        public string? ProductName { get; set; }

        [Column("return_qty")]
        public decimal ReturnQty { get; set; }

        [Column("refund_amount")]
        public decimal RefundAmount { get; set; }

        [Column("return_date")]
        public DateTime ReturnDate { get; set; }
    }
}
