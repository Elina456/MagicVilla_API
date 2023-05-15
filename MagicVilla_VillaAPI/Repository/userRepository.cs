using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MagicVilla_VillaAPI.Repository
{
	public class userRepository : IuserRepository
	{
		private readonly ApplicationDbContext _db;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private string SecretKey;
		private readonly IMapper _mapper;
		public userRepository(ApplicationDbContext db, IConfiguration iconfiguration, UserManager<ApplicationUser> userManager,IMapper mapper, RoleManager<IdentityRole> roleManager)
		{
			_db = db;
			SecretKey = iconfiguration.GetValue<string>("ApiSettings:Secret");
			_userManager = userManager;
			_mapper = mapper;
			_roleManager = roleManager;
		}

		public bool IsUniqueUser(string username)
		{
			var user = _db.applicationUsers.FirstOrDefault(x => x.UserName == username);
			if(user == null)
			{
				return true;
			}
			return false;
		}

		public  async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO)
		{
			var user = _db.applicationUsers.FirstOrDefault(u => u.UserName.ToLower() == loginRequestDTO.userName.ToLower() );
			bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDTO.Password);

			if(user == null || isValid==false)
			{
				return new LoginResponseDTO()
				{
					Token = "",
					user = null
				};
			}
			//if user was found generate JWT token
			var tokenHandler = new JwtSecurityTokenHandler();
			//convert string to bytes
			var key = Encoding.ASCII.GetBytes(SecretKey);
			var roles = await _userManager.GetRolesAsync(user);

			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(new Claim[]
				{
					new Claim(ClaimTypes.Name, user.UserName.ToString()),
					new Claim(ClaimTypes.Role, roles.FirstOrDefault())
				}),
				Expires = DateTime.UtcNow.AddDays(7),
				SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
			};
			var token = tokenHandler.CreateToken(tokenDescriptor);
			LoginResponseDTO loginResponseDTO = new LoginResponseDTO()
			{
				Token = tokenHandler.WriteToken(token),
				user = _mapper.Map<UserDTO>(user),
				//Role = roles.FirstOrDefault()
			};
			return loginResponseDTO;

		}

		public  async Task<UserDTO> Register(RegistrationRequestDTO registrationRequestDTO)
		{
			ApplicationUser user = new()
			{
				UserName = registrationRequestDTO.userName,
				Email = registrationRequestDTO.userName,
				NormalizedEmail = registrationRequestDTO.userName.ToUpper(),
				Name = registrationRequestDTO.Name
				
			};
			try
			{
				var result = await _userManager.CreateAsync(user,registrationRequestDTO.Password);
				if (result.Succeeded)
				{
					if(!_roleManager.RoleExistsAsync("Admin").GetAwaiter().GetResult())
					{
						await _roleManager.CreateAsync(new IdentityRole("Admin"));
						await _roleManager.CreateAsync(new IdentityRole("Customer"));
					}
					await _userManager.AddToRoleAsync(user, "Admin");
					var userToReturn = _db.applicationUsers.FirstOrDefault(u => u.UserName == registrationRequestDTO.userName);
					return _mapper.Map<UserDTO>(userToReturn);
				}

			}
			catch (Exception e) 
			{ 
			}
			
			return new UserDTO();
		}

	}
}
