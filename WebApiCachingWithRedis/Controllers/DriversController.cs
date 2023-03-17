using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiCachingWithRedis.Data;
using WebApiCachingWithRedis.Models;
using WebApiCachingWithRedis.Services;

namespace WebApiCachingWithRedis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriversController : ControllerBase
    {
        private readonly ILogger<DriversController> _logger;
        private readonly ICacheService _cacheService;
        private readonly AppDbContext _context;

        public DriversController(ILogger<DriversController> logger, ICacheService cacheService, AppDbContext context)
        {
            _logger = logger;
            _cacheService = cacheService;
            _context = context;
        }

        [HttpGet("drivers")]
        public async Task<IActionResult> Get()
        {
            var cacheData = _cacheService.GetData<IEnumerable<Driver>>("drivers");
            if (cacheData != null && cacheData.Count() > 0)
            {
                return Ok(cacheData);
            }
            cacheData = await _context.Drivers.ToListAsync();

            var expiryTime = DateTimeOffset.Now.AddSeconds(30);
            _cacheService.SetData<IEnumerable<Driver>>("drivers", cacheData, expiryTime);

            return Ok(cacheData);
        }

        [HttpPost("AddDriver")]
        public async Task<IActionResult> Post(Driver value)
        {
            var addedObj = await _context.Drivers.AddAsync(value);
            var expiryTime = DateTimeOffset.Now.AddSeconds(30);

            _cacheService.SetData<Driver>($"driver{value.Id}", addedObj.Entity, expiryTime);

            await _context.SaveChangesAsync();

            return Ok(addedObj.Entity);
        }

        [HttpDelete("DeleteDriver")]
        public async Task<IActionResult> Delete(int id)
        {
            var exist = await _context.Drivers.FirstOrDefaultAsync(x => x.Id == id);

            if (exist != null)
            {
                _context.Remove(exist);
                _cacheService.RemoveData($"driver{id}");
                await _context.SaveChangesAsync();

                return NoContent();
            }

            return NotFound();
        }
    }
}
