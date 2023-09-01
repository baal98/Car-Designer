using System.Security.Claims;

namespace AdvertisingAgency.Service.Tests.Common
{
    public class ContextUser : ClaimsPrincipal
    {
        public ContextUser(params Claim[] claims) : base(new ClaimsIdentity(claims)) { }
    }
}
