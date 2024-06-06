using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Talabat.APIs.Dtos;
using Talabat.APIs.Errors;
using Talabat.APIs.Extensions;
using Talabat.Core.Entities.Identity;
using Talabat.Core.Service.Contract;

namespace Talabat.APIs.Controllers
{
    
    public class AccountController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IAuthService _authService;
        private readonly IMapper _mapper;

        public AccountController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IAuthService authService,
            IMapper mapper
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _authService = authService;
            _mapper = mapper;
        }

        [HttpPost("login")] // POST: /api/Account/login
        public async Task<ActionResult<UserDto>> Login(LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user is null)
                return Unauthorized(new ApiResponse(401));

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password,false);

            if(result.Succeeded is false)
                return Unauthorized(new ApiResponse(401));

            return Ok(new UserDto()
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                Token = await _authService.CreateTokenAsync(user,_userManager)
            });
        }

        [HttpPost("register")] // POST: /api/account/register

        public async Task<ActionResult<UserDto>> Register(RegisterDto model)
        {
            if (CheckEmailExists(model.Email).Result.Value)
                return BadRequest(new ApiValidationErrorResponse() { Errors = new string[] { "this email is already in use !!" } });

            var user = new AppUser()
            {
                DisplayName = model.DisplayName,
                Email = model.Email, // ahmed.nasr@linkdev.com
                UserName = model.Email.Split("@")[0], // ahmed.nasr 
                PhoneNumber = model.PhoneNumber,
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded is false)
                return BadRequest(new ApiResponse(400));

            return Ok(new UserDto()
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                Token = await _authService.CreateTokenAsync(user, _userManager)
            });
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            var user = await _userManager.FindByEmailAsync(email);

            return Ok(new UserDto()
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                Token = await _authService.CreateTokenAsync(user, _userManager)
            });
        }

        [Authorize]
        [HttpGet("address")] //GET:/api/Account/Address
        public async Task<ActionResult<AddressDto>> GetUserAddress()
        {
            var user = await _userManager.FindUserWithAddressAsync(User);

            var address = _mapper.Map<Address,AddressDto>(user.Address);

            return Ok(address);
        }

        [Authorize]
        [HttpPut("address")] // PUT : /api/account/address
        public async Task<ActionResult<AddressDto>> UpdateUserAddress(AddressDto updatedAddress)
        {
            var address = _mapper.Map<AddressDto, Address>(updatedAddress);

            var user = await _userManager.FindUserWithAddressAsync(User);

            address.Id = user.Address.Id;

            user.Address = address;

            //user.Address.FirstName = address.FirstName;
            //user.Address.LastName = address.LastName;
            //user.Address.Street = address.Street;
            //user.Address.City = address.City;
            //user.Address.Country = address.Country;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return BadRequest(new ApiResponse(400));

            return Ok(updatedAddress);
        }

        [HttpGet("emailexists")] // GET: /api/account/emailexists?email=ahmed,nasr@linkdev.com
        public async Task<ActionResult<bool>> CheckEmailExists(string email)
        {
            return await _userManager.FindByEmailAsync(email) is not null;
        }
    }
}
