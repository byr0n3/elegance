using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;

namespace Elegance.AspNet
{
	[ApiController]
	[Produces(MediaTypeNames.Application.Json)]
	public abstract class ApiController : ControllerBase;
}
