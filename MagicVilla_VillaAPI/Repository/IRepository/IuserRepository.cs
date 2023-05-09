using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;

namespace MagicVilla_VillaAPI.Repository.IRepository
{
	public interface IuserRepository
	{
		bool IsUniqueUser (string username);
		Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO);
		Task<LocalUser> Register(RegistrationRequestDTO registrationRequestDTO);


	}
}
