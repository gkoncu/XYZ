using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Features.Students.DTOs;
using XYZ.Domain.Entities;

namespace XYZ.Application.Common.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Student, StudentDto>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.Class != null ? src.Class.Name : null))
                .ForMember(dest => dest.CoachNames, opt => opt.MapFrom(src =>
                    src.Class != null ?
                    src.Class.Coaches.Select(c => $"{c.User.FirstName} {c.User.LastName}").ToList() :
                    new List<string>()));

            CreateMap<CreateStudentDto, Student>();

            CreateMap<UpdateStudentDto, Student>();
        }
    }
}
