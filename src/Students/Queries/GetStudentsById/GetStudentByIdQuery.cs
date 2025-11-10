using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Features.Students.DTOs;

namespace XYZ.Application.Features.Students.Queries.GetStudentsById
{
    public class GetStudentByIdQuery : IRequest<StudentDetailDto>
    {
        public int Id { get; set; }
    }

    public class GetStudentByIdQueryValidator : AbstractValidator<GetStudentByIdQuery>
    {
        public GetStudentByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Student ID must be greater than 0");
        }
    }
}
