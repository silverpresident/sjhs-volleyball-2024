using System.ComponentModel.DataAnnotations;

namespace VolleyballRallyManager.Lib.Models;

public abstract class BaseEntity
{
    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        CreatedBy = "system";
        UpdatedBy = "system";
    }

    [Display(Name = "Id")]
    public Guid Id { get; set; }

    [Display(Name = "Created At")]
    public DateTime CreatedAt { get; set; }

    [Display(Name = "Created By")]
    public string CreatedBy { get; set; } = "system";

    [Display(Name = "Updated At")]
    public DateTime UpdatedAt { get; set; }

    [Display(Name = "Updated By")]
    public string UpdatedBy { get; set; } = "system";
}
