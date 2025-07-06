using System.ComponentModel.DataAnnotations;

namespace CathayBank.API.Models
{
    public class Currency
    {
        [Key]
        [StringLength(10)]
        public required string Code { get; set; }

        [StringLength(256)]
        public required string ChineseName { get; set; }
    }
}
