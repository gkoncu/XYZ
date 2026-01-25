using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Features.Profile.Queries.GetMyProfile;

namespace XYZ.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProfileController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("me")]
        public async Task<ActionResult<MyProfileDto>> GetMyProfile(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetMyProfileQuery(), cancellationToken);
            return Ok(result);
        }
    }
}
