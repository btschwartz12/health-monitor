using ISHealthMonitor.Core.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ISHealthMonitor.Rest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private IConfiguration _config;
        private string _userName;
        private string _password;
        private string _grantType;
        private ILogger<TokenController> _logger;
        public TokenController(IConfiguration config, ILogger<TokenController> logger)
        {
            _config = config;
            _userName = config.GetSection("ApiAuthConfig").GetSection("userName").Value;
            _password = config.GetSection("ApiAuthConfig").GetSection("password").Value;
            _grantType = config.GetSection("ApiAuthConfig").GetSection("grantType").Value;
            _logger = logger;
        }

        [HttpPost]
        public string GetToken(string grant_type, string userName, string password)
        {
            _logger.LogInformation($"GetTokenLOG: grant_type: {grant_type} userName: {userName} password: {password}");
            if (grant_type == _grantType)
            {
                if (userName == _userName && password == _password)
                {
                    var tokenService = new TokenService(_config);
                    var token = tokenService.GenerateSecurityToken("apiUser");

                    var response = new
                    {
                        access_token = token,
                        expires_in_min = 1400
                    };
                    var serializerSettings = new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented
                    };
                    _logger.LogInformation($"GetTokenLOG: Token: {token}");
                    return (JsonConvert.SerializeObject(response, serializerSettings));
                }
                else
                {
                    _logger.LogError($"GetTokenLOG: invalid credentials");
                    return "invalid credentials";
                }
            }
            else
            {
                _logger.LogError($"GetTokenLOG: invalid grantType");
                return "invalid grantType";
            }
        }
    }
}
