using Microsoft.AspNetCore.Mvc;

namespace EnergyPlanner.Tests;

[ApiController]
[Route("api/test-errors")]
public class ThrowingTestController : ControllerBase
{
    public const string SecretDetail = "секретна-деталь-помилки";

    [HttpGet("crash")]
    public IActionResult Crash()
    {
        throw new InvalidOperationException(SecretDetail);
    }

    [HttpGet("invalid-argument")]
    public IActionResult InvalidArgument()
    {
        throw new ArgumentException(SecretDetail);
    }

    [HttpGet("null-argument")]
    public IActionResult NullArgument()
    {
        throw new ArgumentNullException(SecretDetail);
    }
}
