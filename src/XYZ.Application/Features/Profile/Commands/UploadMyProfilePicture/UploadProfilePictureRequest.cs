using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Profile.Commands.UploadMyProfilePicture
{
    public sealed class UploadProfilePictureRequest
    {
        public IFormFile File { get; set; } = default!;
    }
}
