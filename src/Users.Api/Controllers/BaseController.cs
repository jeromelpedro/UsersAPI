using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Users.Api.Controllers
{
	[Authorize] // aplica para todos os controllers que herdarem
	[ApiController]
	public abstract class BaseController : ControllerBase
	{
	}
}