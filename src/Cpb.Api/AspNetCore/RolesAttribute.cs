using Cpb.Domain;
using Microsoft.AspNetCore.Authorization;

namespace Cpb.Api.AspNetCore;

public class RolesAttribute : AuthorizeAttribute
{
    public RolesAttribute(params Roles[] roles)
    {
        Roles = string.Join(",", roles);
    }
}