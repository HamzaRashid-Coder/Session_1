using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LabInventory.Data;
using LabInventory.Models.DTOs.Users;
using System.Security.Claims;
namespace LabInventory.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 🔐 protects ALL endpoints in this controller
    public class TestController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TestController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .Select(x => new UserDto
                {
                    UserId = x.UserId,
                    FullName = x.FullName,
                    Email = x.Email,
                    Username = x.Username,
                    IsActive = x.IsActive
                })
                .ToListAsync();

            return Ok(users);
        }
    }
}