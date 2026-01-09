using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Common.Models;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Payments.Queries.GetPayments
{
    public class GetPaymentsQueryHandler
        : IRequestHandler<GetPaymentsQuery, PaginationResult<PaymentListItemDto>>
    {
        private readonly IDataScopeService _dataScope;

        public GetPaymentsQueryHandler(IDataScopeService dataScope)
        {
            _dataScope = dataScope;
        }

        public async Task<PaginationResult<PaymentListItemDto>> Handle(
            GetPaymentsQuery request,
            CancellationToken ct)
        {
            var query = _dataScope.Payments()
                .Include(p => p.Student)
                    .ThenInclude(s => s.User)
                .AsQueryable();

            if (request.StudentId.HasValue)
            {
                query = query.Where(p => p.StudentId == request.StudentId.Value);
            }

            if (request.Status.HasValue)
            {
                query = query.Where(p => p.Status == request.Status.Value);
            }

            if (request.FromDueDate.HasValue)
            {
                var from = request.FromDueDate.Value.ToDateTime(TimeOnly.MinValue);
                query = query.Where(p => p.DueDate >= from);
            }

            if (request.ToDueDate.HasValue)
            {
                var to = request.ToDueDate.Value.ToDateTime(TimeOnly.MaxValue);
                query = query.Where(p => p.DueDate <= to);
            }

            var totalCount = await query.CountAsync(ct);

            var page = request.PageNumber <= 0 ? 1 : request.PageNumber;
            var size = request.PageSize <= 0 ? 20 : request.PageSize;

            var items = await query
                .OrderByDescending(p =>
                    p.Status == PaymentStatus.Pending
                    || p.Status == PaymentStatus.Overdue)
                .ThenBy(p => p.DueDate)
                .ThenByDescending(p => p.Id)
                .Select(p => new PaymentListItemDto
                {
                    Id = p.Id,
                    StudentId = p.StudentId,
                    StudentFullName = p.Student.User.FullName,
                    Amount = p.Amount,
                    DiscountAmount = p.DiscountAmount,
                    Status = p.Status,

                    DueDate = p.DueDate,

                    CreatedAt = p.CreatedAt
                })
                .AsNoTracking()
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync(ct);

            return new PaginationResult<PaymentListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = size
            };
        }
    }
}
