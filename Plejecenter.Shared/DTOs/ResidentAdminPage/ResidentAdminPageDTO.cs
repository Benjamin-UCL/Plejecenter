using Plejecenter.Shared.Enums;

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
        RiskIndicator RiskLevel
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

    public record UpdateResidentRequest(
        string FirstName,
        string LastName,
        string Alias,
        int Apartment,
        string Status,
        string ShoppingDay,
        string PaymentMethod,
        string ShoppingNotes,
        string PaymentNotes,
        string Message,
        RiskIndicator RiskLevel
    );
    public record SetResidentActiveRequest(string Status); 
}