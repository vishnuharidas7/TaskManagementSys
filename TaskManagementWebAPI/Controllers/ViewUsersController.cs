using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using TaskManagement_Project.DTOs;
using TaskManagement_Project.Repositories;
using TaskManagementWebAPI.Data;
using TaskManagementWebAPI.DTOs;
using TaskManagementWebAPI.Models;

namespace TaskManagement_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ViewUsersController : ControllerBase
    {
        private readonly UserRepository _user;

        public ViewUsersController(UserRepository user)
        {

            _user = user ?? throw new ArgumentNullException(nameof(user));

        }

        [HttpGet("viewusers")]
        public async Task<ActionResult> UserList()
        {
            var allUser = await _user.ViewUsers();
            return Ok(allUser);
        }


        //commented
        //public async Task<List<ViewUserDTO>> ViewUsers()
        //{

        //    var usersWithRoles = await _db.User
        //    .Include(u => u.Role)
        //    .Select(u => new ViewUserDTO
        //    {
        //        UserName = u.UserName,
        //        Email = u.Email,
        //        RoleName = u.Role.RoleName,
        //        Status = u.IsActive
        //    })
        //    .ToListAsync();

        //        return usersWithRoles;
        //}

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            await _user.DeleteUser(id);
            return Ok();

        }
    }
}
