using Plejecenter.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Plejecenter.Shared.DTOs.ResidentAdminPage;

public class ResidentAdminPageDTO
{
    public record ResidentDto(
        int Id,
        string FirstName,
        string LastName,
        string Alias,
        int Apartment,
        string Status,
        RiskIndicator RiskLevel,
        string ShoppingDay,
        string ShoppingNotes,
        string PaymentNotes,
        string Message
    );

    public class CreateResidentRequest{ //NOTE TO SELF: SHOULD I MOVE THIS OUTSIDE OF RESIDENTADMINPAGEDTO CLASS?
        // string FirstName,
        // string LastName,
        // string Alias,
        // int SocialSecurityNumber,
        // int Apartment,
        // string Status
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Alias { get; set; } = "";
        public int SocialSecurityNumber { get; set; }
        public int Apartment { get; set; }
        public string Status { get; set; } = "Aktiv";
    };

    public class UpdateResidentRequest
    {
        // string FirstName,
        // string LastName,
        // string Alias,
        // int Apartment,
        // string Status,
        // string ShoppingDay,
        // string PaymentMethod,
        // string ShoppingNotes,
        // string PaymentNotes,
        // string Message,
        // RiskIndicator RiskLevel
        public int Id { get; set; }

        [Required(ErrorMessage = "Fornavn er påkrævet")]
        [StringLength(50, ErrorMessage = "Fornavnet er for langt")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Efternavn er påkrævet")]
        [StringLength(50, ErrorMessage = "Efternavnet er for langt")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Initialer er påkrævet")]
        [StringLength(10, ErrorMessage = "Initialer må højst være 10 tegn")]
        public string Alias { get; set; } = string.Empty;

        [Range(0, 1000, ErrorMessage = "Ugyldigt lejlighedsnummer")]
        public int Apartment { get; set; }

        public string Status { get; set; } = "Aktiv";
        public string ShoppingDay { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string ShoppingNotes { get; set; } = string.Empty;
        public string PaymentNotes { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public RiskIndicator RiskLevel { get; set; }
    }   
    public record SetResidentActiveRequest(string Status); 
}