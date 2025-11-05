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
            // Student mappings
            CreateMap<Student, StudentDto>()
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.Class != null ? src.Class.Name : null))
                .ForMember(dest => dest.CoachName, opt => opt.MapFrom(src =>
                    src.Class.Coach != null ? src.Class.Coach.User.FullName : string.Empty));

            CreateMap<CreateStudentDto, Student>();
            CreateMap<UpdateStudentDto, Student>();
        }
    }
}
