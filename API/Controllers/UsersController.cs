using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _userRepository;   //  declaring IUserRepository
        private readonly IMapper _mapper;   //  declaring Mapper
        private readonly IPhotoService _photoService;   //declaring photo service

        public UsersController(IUserRepository userRepository,
            IMapper mapper,
            IPhotoService photoService)
        {
            _userRepository = userRepository;   //assigning of type userRepository
            _mapper = mapper;
            _photoService = photoService;
        }

        /// <summary>
        /// Returns all users
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
        {
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());
            userParams.CurrentUsername = user.UserName;

            if (string.IsNullOrEmpty(userParams.Gender))
                userParams.Gender = user.Gender == "male" ? "female":"male";



            var users = await _userRepository.GetMembersAsync(userParams);
            Response.AddPaginationHeader(users.CurrentPage,
                users.PageSize, users.TotalCount, users.TotalPages);
            return Ok(users);
        }

        /// <summary>
        /// Returns user detail by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns></returns>
        [HttpGet("{username}",Name ="GetUser")]        
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
           return await _userRepository.GetMemberAsync(username);            
        }


        /// <summary>
        /// update's user details
        /// </summary>
        /// <param name="memberUpdateDto"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            //var username = User.GetUsername();//it will return username from the token being used to authenticate this user
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

            _mapper.Map(memberUpdateDto, user); //mapping Dto to User entity

            _userRepository.Update(user);
            if (await _userRepository.SaveAllAsync()) return NoContent();
            return BadRequest("Failed to update user");
        }


        /// <summary>
        /// add user's photo
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());//Getting user
            var result = await _photoService.AddPhotoAsync(file);//getting result back from photo service
            if (result.Error != null) return BadRequest(result.Error.Message);//checking for error

            var photo = new Photo //[Table] creating a new photo with properties
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            if (user.Photos.Count == 0) //checking user already has any photo
            {
                photo.IsMain = true; //setting photo as main
            }
            user.Photos.Add(photo); //adding photo

            if (await _userRepository.SaveAllAsync())//saving photo
            {
                //return CreatedAtRoute("GetUser", _mapper.Map<PhotoDto>(photo));
                return CreatedAtRoute("GetUser", new { username = user.UserName }, _mapper.Map<PhotoDto>(photo));
            }
                


            return BadRequest("Problem adding photo");
        }


        /// <summary>
        /// update photo to main photo by id
        /// </summary>
        /// <param name="photoId"></param>
        /// <returns></returns>
        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhot(int photoId)
        {
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId); //getting photo from table

            if (photo.IsMain) return BadRequest("This is already your main photo");

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
            if (currentMain != null) currentMain.IsMain = false; //setting all photos isMain property to false
            photo.IsMain = true; //setting isMain to true which matches the photo id in Photo table

            if (await _userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to set main photo");
        }

        /// <summary>
        /// delete photo by id
        /// </summary>
        /// <param name="photoId"></param>
        /// <returns></returns>
        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
            if (photo == null) return NotFound();
            if (photo.IsMain) return BadRequest("You cannot delete your main photo");
            if (photo.PublicId != null)
            {
               var result= await _photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error.Message);

            }
            user.Photos.Remove(photo);
            if (await _userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Failed to delete the photo");
        }
    }
}
