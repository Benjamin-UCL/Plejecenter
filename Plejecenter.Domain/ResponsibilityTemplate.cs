using System;

namespace Plejecenter.Domain;

public class ResponsibilityTemplate
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public bool IsActive { get; set; } = true;

    public List<Responsibility> Instances { get; set; } = new();
}
