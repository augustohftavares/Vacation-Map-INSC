using System.ComponentModel.DataAnnotations;

namespace INSCmapaFerias.Areas.Identity.Data
{
    public class Teams
    {
        [Key]
        public int TeamId { get; set; }

        public string TeamName { get; set; }

    }
}
