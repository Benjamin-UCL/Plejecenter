using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plejecenter.Shared.Enums;

namespace Plejecenter.Domain;
public class Responsibility
{
    public int Id { get; set; }
    public int TemplateId { get; set; }
    public ResponsibilityTemplate? Template { get; set; } // 
    public string Title { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public DateTime TaskDate { get; set; }
    public ShiftType Shift { get; set; }
    public int? UserId { get; set; }
    public User? AssignedUser { get; set; }

    public bool IsCompleted { get; set; }

    /// <summary>
    /// Valgfrit: gamle/skema-knytning til Overlap-vagter. Ansvarsliste pr. dag+skift bruger ikke OverlapId.
    /// </summary>
    public int? OverlapId { get; set; }
    public Overlap? Overlap { get; set; }

}