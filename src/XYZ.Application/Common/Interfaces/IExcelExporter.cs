using System.Collections.Generic;

namespace XYZ.Application.Common.Interfaces
{
    public interface IExcelExporter
    {
        byte[] Export<T>(IList<T> rows, string sheetName);
    }
}
