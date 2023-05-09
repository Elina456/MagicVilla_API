using MagicVilla_Web.Models.Dto;

namespace MagicVilla_Web.Services.IServices
{
	public interface IAuthService
	{
		Task<T> loginAsync<T>(LoginRequestDTO objToCreate);
		Task<T> registerAsync<T>(RegistrationRequestDTO objToCreate);
	}
}
