using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Entities;

namespace XYZ.Application.Common.Mappings
{
    public class GlobalMappingConfig : IMapsterConfig
    {
        public void Configure(TypeAdapterConfig config)
        {
            config.NewConfig<Student, StudentDto>()
                .Map(dest => dest.FullName, src => $"{src.FirstName} {src.LastName}")
                .Map(dest => dest.Age, src => DateTime.Now.Year - src.BirthDate.Year);
        }
    }
}
