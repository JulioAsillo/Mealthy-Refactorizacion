namespace Mealthy.Api.Security.Domain.Model;

/// <summary>
/// Roles del sistema. Se persiste como texto (ver UserConfiguration) para que
/// la BD sea legible y reordenar el enum no corrompa datos.
/// </summary>
public enum Role
{
    Customer,
    StoreOwner,
    Admin
}
