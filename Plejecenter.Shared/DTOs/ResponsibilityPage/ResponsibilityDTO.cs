using System;
using Plejecenter.Shared.Enums;

namespace Plejecenter.Shared.DTOs.ResponsibilityPage;

public class ResponsibilityDTO
{
 public record ResponsibilityDto(
     int Id,
     int TemplateId,
     string Title,
     int SortOrder,
     DateTime TaskDate,
     ShiftType Shift,
     int? UserId,
     bool IsCompleted,
     ResponsibilityRecurrence Recurrence,
     ShiftMask ApplicableShifts
 );

 //Opretning af opgave (template). Instanser oprettes for StartDate og valgte vagter.
 public record CreateTemplateRequest(
     string Title,
     DateTime StartDate,
     ResponsibilityRecurrence Recurrence,
     ShiftMask ApplicableShifts
 );

 //Opdatering af ansvar
 public record UpdateResponsibilityRequest(
     string Title,        
     int? UserId
 );

 //Status på toggle
 public record SetCompletedRequest(bool IsCompleted);

 //Byt rundt på ansvar
 public record MoveRequest(MoveDirection Direction);

 //Retning for at flytte ansvar i listen
 public enum MoveDirection
 {
     Up = 0,
     Down = 1
 }
}
