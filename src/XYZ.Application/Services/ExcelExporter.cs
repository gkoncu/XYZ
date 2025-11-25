using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ClosedXML.Excel;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Services
{
    public class ExcelExporter : IExcelExporter
    {
        public byte[] Export<T>(IList<T> rows, string sheetName)
        {
            using var workbook = new XLWorkbook();

            if (string.IsNullOrWhiteSpace(sheetName))
                sheetName = "Report";

            var worksheet = workbook.Worksheets.Add(sheetName);

            var properties = typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead)
                .ToArray();

            if (properties.Length == 0)
                throw new InvalidOperationException(
                    $"Tip {typeof(T).Name} için okunabilir property bulunamadı.");

            for (var i = 0; i < properties.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = properties[i].Name;
                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
            }

            for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                var row = rows[rowIndex];

                for (var colIndex = 0; colIndex < properties.Length; colIndex++)
                {
                    var prop = properties[colIndex];
                    var rawValue = prop.GetValue(row);
                    var text = ConvertToExcelString(rawValue);

                    worksheet.Cell(rowIndex + 2, colIndex + 1).Value =
                        text ?? string.Empty;
                }
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private static string? ConvertToExcelString(object? value)
        {
            if (value is null)
                return null;

            return value switch
            {
                DateOnly d => d.ToString("yyyy-MM-dd"),
                TimeOnly t => t.ToString("HH\\:mm"),
                Enum e => e.ToString(),
                _ => value.ToString()
            };
        }
    }
}
