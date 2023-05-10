using MagicVilla_Utility;
using MagicVilla_Web.Models;
using MagicVilla_Web.Models.Dto;
using MagicVilla_Web.Services.IServices;

namespace MagicVilla_Web.Services
{
	public class AuthService : BaseService, IAuthService
	{
		private readonly IHttpClientFactory _clientFactory;
		private string villaUrl;
		public AuthService(IHttpClientFactory clientFactory, IConfiguration configuration) : base(clientFactory)
		{
			_clientFactory = clientFactory;
			villaUrl = configuration.GetValue<string>("ServiceUrls:VillaAPI");
		}

		public Task<T> loginAsync<T>(LoginRequestDTO Obj)
		{
			return SendAsync<T>(new APIRequest()
			{
				ApiType = SD.ApiType.POST,
				Data = Obj,
				Url = villaUrl + "/api/v1/usersAuth/login"
			});
		}

		public Task<T> registerAsync<T>(RegistrationRequestDTO Obj)
		{
			return SendAsync<T>(new APIRequest()
			{
				ApiType = SD.ApiType.POST,
				Data = Obj,
				Url = villaUrl + "/api/v1/usersAuth/Register"
			});
		}
	}
}
