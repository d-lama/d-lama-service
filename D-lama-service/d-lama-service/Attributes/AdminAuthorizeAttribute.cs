using d_lama_service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace d_lama_service.Attributes
{
    /// <summary>
    /// AdminAuthorizeAttribute adds middleware logic for autenticating Admins.
    /// </summary>
    public class AdminAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        /// <summary>
        /// Authorizes users who are as administrator registered.
        /// </summary>
        /// <param name="context"> The executing context. </param>
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            try
            {
                var isAuthenticated = context.HttpContext.User.Identity?.IsAuthenticated ?? false;
                var isAdmin = bool.Parse(context.HttpContext.User.FindFirst(Tokenizer.IsAdminClaim)!.Value);
                if (isAuthenticated && isAdmin)
                {
                    return;
                }
            }
            catch { }

            context.Result = new UnauthorizedResult();
            return;
        }

    }
        
}
