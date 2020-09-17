using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Filters;
using System.Web.Http.Results;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace BotSvr.Security
{
    public class AuthenticationFilter : Attribute, IAuthenticationFilter
    {
        #region Properties

        public bool AllowMultiple
        {
            get
            {
                return false;
            }
        }

        #endregion Properties

        #region Methods

        public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                if (SkipAuthorization(context.ActionContext))
                {
                    return;
                }
                var request = context.Request;
                var authorization = request.Headers.Authorization;

                if (authorization == null || authorization.Scheme != "Bearer")
                {
                    context.ErrorResult = new UnauthorizedResult(new AuthenticationHeaderValue[0], request);
                    return;
                }

                if (string.IsNullOrEmpty(authorization.Parameter))
                {
                    context.ErrorResult = new UnauthorizedResult(new AuthenticationHeaderValue[0], request);
                    return;
                }

                // Validate the JWT
                var token = authorization.Parameter;
                var validationParams = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = SecurityConfiguration.TokenIssuer,

                    ValidateAudience = true,
                    ValidAudience = SecurityConfiguration.TokenAudience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = SecurityConfiguration.SecurityKey,

                    RequireExpirationTime = true,
                    ValidateLifetime = true
                };

                try
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    tokenHandler.InboundClaimTypeMap["name"] = ClaimTypes.Name;

                    SecurityToken securityToken;
                    var principal = context.Principal = tokenHandler.ValidateToken(token, validationParams, out securityToken);
                }
                catch (Exception ex)
                {
                    context.ErrorResult = new UnauthorizedResult(new AuthenticationHeaderValue[0], request);
                }
            });

        }

        private static bool SkipAuthorization(HttpActionContext actionContext)
        {
            return actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any<AllowAnonymousAttribute>() || actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any<AllowAnonymousAttribute>();
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            // Challenge was added in at AuthenticateAsync

            return Task.FromResult(0);
        }

        #endregion Methods
    }
}
