using System.ComponentModel.DataAnnotations;

namespace ToDoApp.Shared.Enums;

public enum Priority
{
    [Display(Name = "Very low")]
    VeryLow = 1,

    [Display(Name = "Low")]
    Low = 2,

    [Display(Name = "Medium")]
    Medium = 3,

    [Display(Name = "High")]
    High = 4,

    [Display(Name = "Critical")]
    Critical = 5
}
