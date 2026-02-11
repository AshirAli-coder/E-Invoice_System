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

        [Column("sale_id")]
        public int SaleId { get; set; }

        [Column("customer_name")]
        public string? CustomerName { get; set; }

        [Column("date")]
        public DateTime Date { get; set; }

        [Column("prod_name/service")]
        public string? ProdNameService { get; set; }

        [Column("barcode")]
        public string? Barcode { get; set; }

        [Column("qty/unit_type")]
        public string? QtyUnitType { get; set; }

        [Column("amount")]
        public decimal Amount { get; set; }

        [Column("method")]
        public string? Method { get; set; }

        [Column("status")]
        public string? Status { get; set; }
    }
}
