using MagicVilla_Utility;
using MagicVilla_Web.Models;
using MagicVilla_Web.Models.Dto;
using MagicVilla_Web.Services.IServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MagicVilla_Web.Controllers
{
	public class AuthController : Controller
	{
		private readonly IAuthService _authservice;
		public AuthController(IAuthService authservice)
		{
			_authservice = authservice;
		}

		[HttpGet]
		public IActionResult Login()
		{
			LoginRequestDTO Obj = new();
			return View(Obj);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task< IActionResult> Login(LoginRequestDTO Obj)
		{
			APIResponse response = await _authservice.loginAsync<APIResponse>(Obj);
			if(response!= null && response.IsSuccess)
			{
				LoginResponseDTO model = JsonConvert.DeserializeObject<LoginResponseDTO>(Convert.ToString(response.Result));
				var handler = new JwtSecurityTokenHandler();
				var JWT = handler.ReadJwtToken(model.Token);
				var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
				identity.AddClaim(new Claim(ClaimTypes.Name, JWT.Claims.FirstOrDefault(u=>u.Type== "unique_name").Value));
                identity.AddClaim(new Claim(ClaimTypes.Role, JWT.Claims.FirstOrDefault(u =>u.Type=="role").Value));
				var principal = new ClaimsPrincipal(identity);
				await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                HttpContext.Session.SetString(SD.sessionToken, model.Token);
				return RedirectToAction("Index", "Home");

			}
			else
			{
				ModelState.AddModelError("Custom Error",response.ErrorMessages.FirstOrDefault());
				return View(Obj);
			}
			
		}

		[HttpGet]
		public IActionResult Register()
		{
			
			return View();
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Register(RegistrationRequestDTO Obj)
		{
			APIResponse result = await _authservice.registerAsync<APIResponse>(Obj);
			if(result!=null && result.IsSuccess)
			{
				return RedirectToAction("Login");
			}

			return View();
		}
		
		public async Task<IActionResult> Logout()
		{
			await HttpContext.SignOutAsync();
			HttpContext.Session.SetString(SD.sessionToken, "");
			return RedirectToAction("Index", "Home");
			
			
		}

		
		public IActionResult AccessDenied()
		{
			
			return View();
		}

	}
}
