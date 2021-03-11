using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Identity.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using API.DTOs;
using API.Errors;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using API.Extensions;
using AutoMapper;

namespace API.Controllers
{
    public class AccountController : BaseAPIController
    {
        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
            ITokenService tokenService, IMapper mapper)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            TokenService = tokenService;
            Mapper = mapper;
        }

        public UserManager<AppUser> UserManager { get; }
        public SignInManager<AppUser> SignInManager { get; }
        public IMapper Mapper { get; }

        private readonly ITokenService TokenService;

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await UserManager.FindByEmailAsync(loginDto.Email);
            if (user == null) return Unauthorized(new ApiResponse(401));

            var result = await SignInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if(!result.Succeeded) return Unauthorized(new ApiResponse(401));

            return new UserDto
            {
                Email = user.Email,
                Token = TokenService.CreateToken(user),
                DisplayName = user.DisplayName
            };
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (CheckEmailExistsAsync(registerDto.Email).Result.Value)
            {
                return new BadRequestObjectResult(new ValidationErrorResponse
                {
                    Errors= new[] {"Email address is in use"}
                });
            }
            var user = new AppUser
            {
                DisplayName = registerDto.DisplayName,
                Email = registerDto.Email,
                UserName = registerDto.Email
            };

            var result = await UserManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded) return BadRequest(new ApiResponse(400));

            return new UserDto
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                Token = TokenService.CreateToken(user)
            };
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var user = await UserManager.FindByEmailFromClaimsPrinciple(HttpContext.User);
            return new UserDto
            {
                DisplayName = user.DisplayName,
                Token = TokenService.CreateToken(user),
                Email = user.Email
            };

        }
        
        [HttpGet("emailexists")]
        public async Task<ActionResult<bool>> CheckEmailExistsAsync([FromQuery] string email)
        {
            return await UserManager.FindByEmailAsync(email) != null;
        }
        
        [HttpGet("address")]
        [Authorize]
        public async Task<ActionResult<AddressDTO>> GetUserAddress()
        {
            var user = await UserManager.FindByEmailWithAddressAsync(HttpContext.User);

            return Mapper.Map<Address, AddressDTO>(user.Address);

        }

        [HttpPut("address")]
        [Authorize]
        public async Task<ActionResult<AddressDTO>> UpdateUserAddres(AddressDTO address)
        {
            var user = await UserManager.FindByEmailWithAddressAsync(HttpContext.User);
            user.Address = Mapper.Map<AddressDTO, Address>(address);
            var result = await UserManager.UpdateAsync(user);
            if (result.Succeeded) return Ok(Mapper.Map<Address, AddressDTO>(user.Address));

            return BadRequest("Problem updating the user");

        }

    }
}