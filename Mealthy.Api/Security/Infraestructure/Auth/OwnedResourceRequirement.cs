using Mealthy.Api.Shared.Application.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Mealthy.Api.Security.Infraestructure.Auth;

public sealed record OwnedResourceRequirement(ResourceAction Action) : IAuthorizationRequirement;
