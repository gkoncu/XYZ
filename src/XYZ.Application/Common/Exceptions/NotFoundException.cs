using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Common.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException() { }

        public NotFoundException(string message) : base(message) { }

        public NotFoundException(string name, object key)
            : base($"{name} ({key}) bulunamadı.") { }

        public NotFoundException(string name, string field, object key)
            : base($"{name} [{field}={key}] bulunamadı.") { }
    }
}
