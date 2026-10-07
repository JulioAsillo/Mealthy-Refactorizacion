namespace Mealthy.Api.Security.Infraestructure.Auth;

public static class AuthPolicies
{
    public const string CustomerOnly = nameof(CustomerOnly);
    public const string StoreOwnerOnly = nameof(StoreOwnerOnly);
    public const string AdminOnly = nameof(AdminOnly);
}
