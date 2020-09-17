using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSvr.Security
{
    public static class SecurityConfiguration
    {
        public static string SigningKey = "4b990cd882af4519878c8e0a94419b0f90b23cd097c8226192ce22d9a619733a";
        public static string TokenIssuer = "https://github.com/renchengxiaofeixia";
        public static string TokenAudience = "https://github.com/renchengxiaofeixia";

        public static SecurityKey SecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey));
        public static SigningCredentials SigningCredentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256);
    }
}
