namespace XYZ.Domain.Constants;

public static class RoleNames
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string Coach = "Coach";
    public const string Finance = "Finance";
    public const string Student = "Student";

    public static readonly string[] All =
    [
        SuperAdmin,
        Admin,
        Coach,
        Finance,
        Student
    ];
}
