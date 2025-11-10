//using Mapster;
//using XYZ.Application.Common.Interfaces;
//using XYZ.Application.Features.Students.DTOs;
//using XYZ.Domain.Entities;

//public class GlobalMappingConfig : IMapsterConfig
//{
//    public void Configure(TypeAdapterConfig config)
//    {
//        config.NewConfig<Student, StudentListDto>()
//            .Map(dest => dest.FullName, src => src.User.FullName)
//            .Map(dest => dest.PhoneNumber, src => src.User.PhoneNumber)
//            .Map(dest => dest.IsActive, src => src.IsActive)
//            .Map(dest => dest.ClassName, src => src.Class != null ? src.Class.Name : null);

//        config.NewConfig<Student, StudentDetailDto>()
//            .Map(dest => dest.FirstName, src => src.User.FirstName)
//            .Map(dest => dest.LastName, src => src.User.LastName)
//            .Map(dest => dest.FullName, src => src.User.FullName)
//            .Map(dest => dest.Email, src => src.User.Email)
//            .Map(dest => dest.PhoneNumber, src => src.User.PhoneNumber)
//            .Map(dest => dest.BirthDate, src => src.User.BirthDate)
//            .Map(dest => dest.Age, src => DateTime.Now.Year - src.User.BirthDate.Year)
//            .Map(dest => dest.BloodType, src => src.User.BloodType)
//            .Map(dest => dest.IdentityNumber, src => src.IdentityNumber)
//            .Map(dest => dest.Branch, src => src.User.Branch)

//            .Map(dest => dest.Parent1FirstName, src => src.Parent1FirstName)
//            .Map(dest => dest.Parent1LastName, src => src.Parent1LastName)
//            .Map(dest => dest.Parent1PhoneNumber, src => src.Parent1PhoneNumber)
//            .Map(dest => dest.Parent1Email, src => src.Parent1Email)
//            .Map(dest => dest.Parent2FirstName, src => src.Parent2FirstName)
//            .Map(dest => dest.Parent2LastName, src => src.Parent2LastName)
//            .Map(dest => dest.Parent2PhoneNumber, src => src.Parent2PhoneNumber)
//            .Map(dest => dest.Parent2Email, src => src.Parent2Email)
//            .Map(dest => dest.Address, src => src.Address)
//            .Map(dest => dest.IsActive, src => src.IsActive)

//            .Map(dest => dest.ClassName, src => src.Class != null ? src.Class.Name : null)
//            .Map(dest => dest.ClassId, src => src.Class != null ? src.Class.Id : (int?)null)

//            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
//            .Map(dest => dest.UpdatedAt, src => src.UpdatedAt);

//        config.NewConfig<CreateStudentRequest, Student>()
//            .Map(dest => dest.Parent1FirstName, src => src.Parent1FirstName)
//            .Map(dest => dest.Parent1LastName, src => src.Parent1LastName)
//            .Map(dest => dest.Parent1PhoneNumber, src => src.Parent1PhoneNumber)
//            .Map(dest => dest.Parent1Email, src => src.Parent1Email)
//            .Map(dest => dest.Parent2FirstName, src => src.Parent2FirstName)
//            .Map(dest => dest.Parent2LastName, src => src.Parent2LastName)
//            .Map(dest => dest.Parent2PhoneNumber, src => src.Parent2PhoneNumber)
//            .Map(dest => dest.Parent2Email, src => src.Parent2Email)
//            .Map(dest => dest.Address, src => src.Address)
//            .Map(dest => dest.User.Branch, src => src.Branch)

//            .Ignore(dest => dest.Id)
//            .Ignore(dest => dest.UserId)
//            .Ignore(dest => dest.User)
//            .Ignore(dest => dest.TenantId)
//            .Ignore(dest => dest.ClassId)
//            .Ignore(dest => dest.Class)
//            .Ignore(dest => dest.CreatedAt)
//            .Ignore(dest => dest.UpdatedAt)
//            .Ignore(dest => dest.IsActive);

//        config.NewConfig<UpdateStudentRequest, Student>()
//            .Map(dest => dest.Id, src => src.Id)
//            .Map(dest => dest.Parent1FirstName, src => src.Parent1FirstName)
//            .Map(dest => dest.Parent1LastName, src => src.Parent1LastName)
//            .Map(dest => dest.Parent1PhoneNumber, src => src.Parent1PhoneNumber)
//            .Map(dest => dest.Parent1Email, src => src.Parent1Email)
//            .Map(dest => dest.Parent2FirstName, src => src.Parent2FirstName)
//            .Map(dest => dest.Parent2LastName, src => src.Parent2LastName)
//            .Map(dest => dest.Parent2PhoneNumber, src => src.Parent2PhoneNumber)
//            .Map(dest => dest.Parent2Email, src => src.Parent2Email)
//            .Map(dest => dest.Address, src => src.Address)
//            .Map(dest => dest.User.Branch, src => src.Branch)

//            .Ignore(dest => dest.UserId)
//            .Ignore(dest => dest.User)
//            .Ignore(dest => dest.TenantId)
//            .Ignore(dest => dest.ClassId)
//            .Ignore(dest => dest.Class)
//            .Ignore(dest => dest.CreatedAt)
//            .Ignore(dest => dest.UpdatedAt)
//            .Ignore(dest => dest.IsActive);
//    }
//}