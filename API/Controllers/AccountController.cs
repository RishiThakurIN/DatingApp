using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context; //   declaring variable of type DataContext

        private readonly ITokenService _tokenService; //    declaring variable of type ITokenService

        private readonly IMapper _mapper;

        public AccountController(DataContext context, ITokenService tokenService, IMapper mapper)
        {
            _context = context;
            _tokenService = tokenService;
            _mapper = mapper;
        }


        //register new user
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await UserExists(registerDto.Username)) //checking for existing user
            {
                return BadRequest("Username is taken");
            }
            var user = _mapper.Map<AppUser>(registerDto);

            using var hmac = new HMACSHA512();  // Creating object of random key generator


            user.UserName = registerDto.Username.ToLower();
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
            user.PasswordSalt = hmac.Key;



            _context.Users.Add(user); //    adding newly created instance of user(above Ln. 39) to context with Added entity state
            await _context.SaveChangesAsync();//    will insert a new record in the database

            return new UserDto
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user),
                KnownAs=user.KnownAs

            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _context.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(x => x.UserName == loginDto.Username); // retriving matchted user's details along with photo 
            if (user == null)
            {
                return Unauthorized("Invalid username");
            }
            using var hmac = new HMACSHA512(user.PasswordSalt); //  passing salt to key generator
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password)); //  computing hash of given password
            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i])    //comparing saved hash and provided hash
                {
                    return Unauthorized("Invalid password");
                }
            }
            return new UserDto
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs=user.KnownAs
            };
        }

        private async Task<bool> UserExists(string username)    //  checking for existing user
        {
            return await _context.Users.AnyAsync(x => x.UserName == username.ToLower());
        }

    }
}
