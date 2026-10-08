using System.ComponentModel.DataAnnotations;
namespace EHUB.Models.Home
{
    public class Expense
    {
        public int Id { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
        [Required]
        [StringLength(50)]
        public string Category { get; set; }
        [Required]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }
        [StringLength(30)]
        public string PaymentMethod { get; set; }
        [StringLength(200)]
        public string? Notes { get; set; }
        [StringLength(50)]
        public string? ReceiptNo { get; set; }
    }
}
