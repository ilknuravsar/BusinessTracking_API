using Application.Common;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IJwtService _jwtService;

        public AuthController(UserManager<AppUser> userManager, IJwtService jwtService)
        {
            _userManager = userManager;
            _jwtService = jwtService;
        }

        /// <summary>
        /// Kullanıcı girişi yaparak JWT Token üretir.
        /// </summary>
        /// <remarks>
        /// <b>Yetki:</b> Bu endpoint herkese açıktır. <br/>
        /// Başarılı girişten sonra dönen Token, diğer isteklere "Authorization: Bearer {token}" olarak eklenmelidir.
        /// </remarks>
        /// <param name="request">Kullanıcı adı ve şifre bilgileri.</param>
        /// <response code="200">Giriş başarılı, Token üretildi.</response>
        /// <response code="401">Geçersiz kullanıcı adı veya şifre.</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<string>>> Login([FromBody] LoginRequest request)
        {
       
            var user = await _userManager.FindByNameAsync(request.UserName);
            if (user == null)
                return Unauthorized(ApiResponse<string>.Fail("Geçersiz kullanıcı adı veya şifre."));

       
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
                return Unauthorized(ApiResponse<string>.Fail("Geçersiz kullanıcı adı veya şifre."));

        
            var token = _jwtService.GenerateToken(user);

          
            return Ok(ApiResponse<string>.Ok(token, "Giriş başarılı."));
        }
    }

    /// <summary>
    /// Login işlemi için gerekli veri transfer nesnesi.
    /// </summary>
    /// <param name="UserName">Kullanıcı adı (Seed datadaki örnekler kullanılabilir).</param>
    /// <param name="Password">Kullanıcı şifresi.</param>
    public record LoginRequest(string UserName, string Password);
}