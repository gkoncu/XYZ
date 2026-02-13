namespace XYZ.Domain.Constants;

public static class RoleNames
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string Coach = "Coach";
    public const string Finance = "Finance";
    public const string Student = "Student";

    public const string AdminOrSuperAdmin = Admin + "," + SuperAdmin;
    public const string AdminCoachOrSuperAdmin = Admin + "," + Coach + "," + SuperAdmin;
    public const string AdminCoachStudentOrSuperAdmin = Admin + "," + Coach + "," + Student + "," + SuperAdmin;

    public static readonly string[] All =
    [
        SuperAdmin,
        Admin,
        Coach,
        Finance,
        Student
    ];
}
