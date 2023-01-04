using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;

namespace INSCmapaFerias.Areas.Identity.Data;

public class ApplicationUser : IdentityUser
{


    [Key]
    [Column("Id")]
    [Display(Name = "UtilizadorID")]
    public override string Id { get; set; }


    [Column("FirstName")]
    [Display(Name = "Primeiro Nome")]
    public string? FirstName { get; set; }

    [Column("LastName")]
    [Display(Name = "Último Nome")]
    public string? LastName { get; set; }
    
    [Column("Email")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Column("UserName")]
    [Display(Name ="Email")]
    public override string? UserName { get; set; }

    [Column("PhoneNumber")]
    [Display(Name = "Telefone")]
    public string? PhoneNumber { get; set; }

    [Column("Admission_Date")]
    [Display(Name = "Data Admissão")]
    [DataType(DataType.Date)]
    public DateTime? AdmissionDate { get; set; }

    [Column("professional_category")]
    [Display(Name = "Cargo")]
    public string? prof_category { get; set; }

    [Column("userImage")]
    [Display(Name = "Imagem")]
    public string? userImage { get; set; }

    [Column("Color")]
    [Display(Name = "Cor")]
    public string? Color { get; set; }


}



