using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GoogleLogin.Services;
using System.Text.Json;
using GoogleLogin.Models;

namespace GoogleLogin.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class OAuthController : ControllerBase
    {
        private readonly MyService _myService;
        private readonly GameContext _context;

        // 생성자
        public OAuthController(MyService myService, GameContext context)
        {
            _myService = myService;
            _context = context;
        }


        [HttpGet]
        public async Task<ActionResult> HandleOAuthRedirect([FromQuery] string code)
        {
            Console.WriteLine(code);
            //POST 요청
            var response = await _myService.SendPostRequestAsync(code);

            //응답을 JSON으로 변환
            var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(response);

            //return Ok(jsonResponse);

            if (jsonResponse!.TryGetValue("access_token", out var accessTokenElement))
            {
                string? access_token = accessTokenElement.GetString();

                // **Access Token 로그 출력**
                Console.WriteLine($"Access Token: {access_token}");

                var userResponse = await _myService.SendGetRequestAsync(access_token!);
                var userJsonResponse = JsonSerializer
                    .Deserialize<Dictionary<string, JsonElement>>(userResponse);

                if (userJsonResponse.TryGetValue("id", out var idElement))
                {
                    //long id = idElement.GetInt64();
                    string id = idElement.GetString() ?? "Unknown"; // 문자열로 처리

                    var user = _context.Users.FindAsync(id);

                    if (user == null)
                    {
                        Console.WriteLine("회원가입 대상");

                        _context.Users.Add(new Models.User { Id = id, Email = "" });
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        Console.WriteLine("로그인 대상");
                    }
                    var htmlContent = await System.IO.File.ReadAllTextAsync("./htmlPage.html");

                    return Content(htmlContent, "text/html");
                }
                else
                {
                    return BadRequest();
                }
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost("revoke")]
        public async Task<IActionResult> RevokeToken([FromBody] TokenRevokeRequest request)
        {
            if (string.IsNullOrEmpty(request.Token))
            {
                return BadRequest(new { message = "Token is required." });
            }

            try
            {
                // 토큰 취소 요청
                var result = await _myService.RevokeTokenAsync(request.Token);

                // 성공 응답 반환
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to revoke token.", error = ex.Message });
            }
        }

    }

    public class TokenRevokeRequest
    {
        public string Token { get; set; } // 액세스 토큰 또는 리프레시 토큰
    }
}
