using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace INSCmapaFerias.Areas.Identity.Data
{
    public class Paternity
    {
        [Key]
        [Column("PaternityId")]
        [Display(Name = "PaternityId")]
        public string PaternityId { get; set; }

        [Column("Name")]
        [Display(Name = "Nome Funcionário")]
        public string? Name { get; set; }

        [Column("PaternityStart")]
        [Display(Name = "Inicio")]
        [DataType(DataType.Date)]
        public DateTime? PaternityStart { get; set; }

        [Column("PaternityEnd")]
        [Display(Name = "Fim")]
        [DataType(DataType.Date)]
        public DateTime? PaternityEnd { get; set; }

        [Column("Total")]
        [DataType(DataType.Date)]
        public string? Total { get; set; }

    }

}

