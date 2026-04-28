using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations; // Tilføjet for at bruge [Required] og [StringLength] attributterne
using Plejecenter.Shared.Enums;

namespace Plejecenter.Domain;

public class User
{
    public int Id { get; set; }

 [Required(ErrorMessage = "Fornavn er påkrævet.")] //[Requirede] gør at feltet er påkrævet, og hvis det ikke er udfyldt, vil det vise den angivne fejlmeddelelse.
 public string FirstName { get; set; } = string.Empty;

 [Required(ErrorMessage = "Efternavn er påkrævet.")]
 public string LastName { get; set; } = string.Empty;

 [Required(ErrorMessage = "Alias er påkrævet.")]
 public string Alias { get; set; } = string.Empty;

 [Required(ErrorMessage = "Kode er mangler.")] 
 [StringLength(6, MinimumLength = 6, ErrorMessage = "Koden skal bestå af præcis 6 tegn.")]
 public string Password { get; set; } = string.Empty; 

 public UserRole Role { get; set; }

 public bool ActiveDeactive { get; set; } = true;

 public List<Responsibility> Responsibilities { get; set; } = new();
}
