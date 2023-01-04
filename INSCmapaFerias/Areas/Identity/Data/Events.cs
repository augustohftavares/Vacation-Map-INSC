using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace INSCmapaFerias.Areas.Identity.Data
{
    public class Events
    {
        [Key]
        [Column("event_id")]
        [Display(Name = "FériasID")]
        public string event_id { get; set; }

        [Column("title")]
        [Display(Name = "Nome")]
        public string? Title { get; set; }

        [Column("event_start")]
        [Display(Name = "Inicio")]
        [DataType(DataType.Date)]
        public DateTime? event_start { get; set; }

        [Column("event_end")]
        [Display(Name = "Fim")]
        [DataType(DataType.Date)]
        public DateTime? event_end { get; set; }

        [Column("Total")]
        [DataType(DataType.Date)]
        public string? Total { get; set; }

        [Column("Status")]
        public string? Status { get; set; }

        [Column("Options")]
        [Display(Name = "Sugerimos -> De")]
        [DataType(DataType.Date)]
        public DateTime? Options { get; set; }

        [Column("Options_End")]
        [Display(Name = "Até")]
        [DataType(DataType.Date)]
        public DateTime? Options_End { get; set; }

        [Column("Email")]
        public string? Email { get; set; }

        [Column("Team")]
        [Display(Name ="Equipa")]
        public string? Team { get; set; }

        [Column("backgroundColor")]
        [Display(Name = "Cor")]
        public string? backgroundColor { get; set; }

        [Column("user_id")]
        [Display(Name = "user id")]
        public string? user_id { get; set; }

    } // end Events

}
