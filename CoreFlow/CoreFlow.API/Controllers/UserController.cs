using CoreFlow.Application.Commands;
using CoreFlow.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoreFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IEnumerable<User>> Get() => Enumerable.Empty<User>();

    [HttpGet("{id}")]
    public Task<ActionResult<User>> Get(Guid id) => Task.FromResult<ActionResult<User>>(NotFound());

    [HttpPost]
    public async Task<IActionResult> Post(CreateUserCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(Get), new { id }, null);
    }
}
