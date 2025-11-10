using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Students.DTOs
{
    public class StudentListDto
    {
        public int Id { get; set; }
        public required string FullName { get; set; }
        public string? ClassName { get; set; }
        public bool IsActive { get; set; }
        public required string PhoneNumber { get; set; }
    }

    public class StudentDetailDto
    {
        public int Id { get; set; }

        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
        public DateTime BirthDate { get; set; }
        public int Age { get; set; }
        public string? BloodType { get; set; }
        public string? IdentityNumber { get; set; }
        public string? Branch { get; set; }

        public string? Parent1FirstName { get; set; }
        public string? Parent1LastName { get; set; }
        public string? Parent1PhoneNumber { get; set; }
        public string? Parent1Email { get; set; }
        public string? Parent2FirstName { get; set; }
        public string? Parent2LastName { get; set; }
        public string? Parent2PhoneNumber { get; set; }
        public string? Parent2Email { get; set; }
        public string? Address { get; set; }

        public string? ClassName { get; set; }
        public int? ClassId { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateStudentRequest
    {
        [Required]
        [StringLength(50)]
        public required string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public required string LastName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public required string Email { get; set; }

        [DataType(DataType.PhoneNumber)]
        public string? PhoneNumber { get; set; }

        [Required]
        public DateTime BirthDate { get; set; }

        [StringLength(5)]
        public string? BloodType { get; set; }

        [StringLength(20)]
        public string? IdentityNumber { get; set; }

        [StringLength(100)]
        public string? Parent1FirstName { get; set; }

        [StringLength(100)]
        public string? Parent1LastName { get; set; }

        [DataType(DataType.PhoneNumber)]
        [StringLength(20)]
        public string? Parent1PhoneNumber { get; set; }

        [DataType(DataType.EmailAddress)]
        [StringLength(100)]
        public string? Parent1Email { get; set; }

        [StringLength(100)]
        public string? Parent2FirstName { get; set; }

        [StringLength(100)]
        public string? Parent2LastName { get; set; }

        [DataType(DataType.PhoneNumber)]
        [StringLength(20)]
        public string? Parent2PhoneNumber { get; set; }

        [DataType(DataType.EmailAddress)]
        [StringLength(100)]
        public string? Parent2Email { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        public int? ClassId { get; set; }
        public string? Branch { get; set; }

    }

    public class UpdateStudentRequest
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public required string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public required string LastName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public required string Email { get; set; }

        [DataType(DataType.PhoneNumber)]
        public string? PhoneNumber { get; set; }

        [Required]
        public DateTime BirthDate { get; set; }

        [StringLength(5)]
        public string? BloodType { get; set; }

        [StringLength(20)]
        public string? IdentityNumber { get; set; }

        [StringLength(100)]
        public string? Parent1FirstName { get; set; }

        [StringLength(100)]
        public string? Parent1LastName { get; set; }

        [DataType(DataType.PhoneNumber)]
        [StringLength(20)]
        public string? Parent1PhoneNumber { get; set; }

        [DataType(DataType.EmailAddress)]
        [StringLength(100)]
        public string? Parent1Email { get; set; }

        [StringLength(100)]
        public string? Parent2FirstName { get; set; }

        [StringLength(100)]
        public string? Parent2LastName { get; set; }

        [DataType(DataType.PhoneNumber)]
        [StringLength(20)]
        public string? Parent2PhoneNumber { get; set; }

        [DataType(DataType.EmailAddress)]
        [StringLength(100)]
        public string? Parent2Email { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        public int? ClassId { get; set; }

        public string? Branch { get; set; }
    }

}

