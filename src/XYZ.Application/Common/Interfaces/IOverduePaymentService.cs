using System;
using System.Threading;
using System.Threading.Tasks;

namespace XYZ.Application.Common.Interfaces;

public interface IOverduePaymentService
{
    Task<int> MarkOverdueAsync(DateTime todayLocal, CancellationToken ct = default);

    Task<int> MarkOverdueAsync(CancellationToken ct = default);
}
