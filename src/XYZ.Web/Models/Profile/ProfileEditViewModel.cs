using System;
using System.ComponentModel.DataAnnotations;
using XYZ.Domain.Enums;

namespace XYZ.Web.Models.Profile;

public sealed class ProfileEditViewModel
{
    [Required, MinLength(2), MaxLength(50)]
    public string FirstName { get; set; } = "";

    [Required, MinLength(2), MaxLength(50)]
    public string LastName { get; set; } = "";

    [MaxLength(30)]
    public string? PhoneNumber { get; set; }

    public Gender Gender { get; set; }
    public BloodType BloodType { get; set; }

    [Required]
    public DateTime BirthDate { get; set; } = DateTime.Today.AddYears(-18);
}
