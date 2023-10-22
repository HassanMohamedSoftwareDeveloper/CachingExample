using CachingExample.Entities;
using CachingExample.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CachingExample.Controllers;
[ApiController]
[Route("[controller]")]
public class DriverController(IDriverRepository driverRepository) : ControllerBase
{

    #region Actions :
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return Ok(await driverRepository.GetDrivers());
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        return Ok(await driverRepository.GetDriverById(id));
    }
    [HttpPost]
    public async Task<IActionResult> Post(Driver value)
    {
        return Ok(await driverRepository.AddDriver(value));
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        return await driverRepository.DeleteDriver(id)
            ? NoContent()
            : NotFound();
    }
    #endregion
}
