using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace INSCmapaFerias.Areas.Identity.Data
{
    public class Overtime
    {
        [Key]
        [Column("OvertimeId")]
        [Display(Name = "OvertimeId")]
        public string OvertimeId { get; set; }

        [Column("Name")]
        [Display(Name = "Nome Funcionário")]
        public string? OvertimeName { get; set; }

        [Column("OvertimeStart")]
        [Display(Name = "Inicio")]
        [DataType(DataType.Date)]
        public DateTime? OvertimeStart { get; set; }

        [Column("OvertimeEnd")]
        [Display(Name = "Fim")]
        [DataType(DataType.Date)]
        public DateTime? OvertimeEnd { get; set; }

        [Column("Total")]
        [Display(Name = "Todal(dias)")]
        [DataType(DataType.Date)]
        public string? Total { get; set; }

        [Column("Hours")]
        [Display(Name = "Horas(por dia)")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh}")]
        public DateTime? Hours { get; set; }
    }
}
