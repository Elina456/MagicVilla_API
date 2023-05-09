using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MagicVilla_VillaAPI.Repository
{
	public class userRepository : IuserRepository
	{
		private readonly ApplicationDbContext _db;
		private string SecretKey;
		public userRepository(ApplicationDbContext db, IConfiguration iconfiguration)
		{
			_db = db;
			SecretKey = iconfiguration.GetValue<string>("ApiSettings:Secret");
		}

		public bool IsUniqueUser(string username)
		{
			var user = _db.LocalUsers.FirstOrDefault(x => x.userName == username);
			if(user == null)
			{
				return true;
			}
			return false;
		}

		public  async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO)
		{
			var user = _db.LocalUsers.FirstOrDefault(u => u.userName.ToLower() == loginRequestDTO.userName.ToLower() && u.Password == loginRequestDTO.Password);
			if(user == null)
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

			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(new Claim[]
				{
					new Claim(ClaimTypes.Name, user.Id.ToString()),
					new Claim(ClaimTypes.Role, user.Role)
				}),
				Expires = DateTime.UtcNow.AddDays(7),
				SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
			};
			var token = tokenHandler.CreateToken(tokenDescriptor);
			LoginResponseDTO loginResponseDTO = new LoginResponseDTO()
			{
				Token = tokenHandler.WriteToken(token),
				user = user
			};
			return loginResponseDTO;

		}

		public  async Task<LocalUser> Register(RegistrationRequestDTO registrationRequestDTO)
		{
			LocalUser user = new()
			{
				userName = registrationRequestDTO.userName,
				Password = registrationRequestDTO.Password,
				Name = registrationRequestDTO.Name,
				Role = registrationRequestDTO.Role
			};
			_db.LocalUsers.Add(user);	
			 await _db.SaveChangesAsync();
			user.Password = "";
			return user;
		}

	}
}
