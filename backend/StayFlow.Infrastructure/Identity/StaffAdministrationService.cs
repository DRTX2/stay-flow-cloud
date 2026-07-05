using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Authorization;
using StayFlow.Application.Common.Exceptions;
using StayFlow.Application.Features.Staff;
using StayFlow.Domain.Tenants;
using StayFlow.Persistence.Identity;
using ValidationException = StayFlow.Application.Common.Exceptions.ValidationException;

namespace StayFlow.Infrastructure.Identity;

public sealed class StaffAdministrationService(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    ITenantProvider tenantProvider,
    IFeatureService features) : IStaffAdministrationService
{
    public async Task<IReadOnlyList<StaffUserDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = RequireTenant();
        var users = await userManager.Users
            .Where(user => user.TenantId == tenantId)
            .OrderBy(user => user.FullName)
            .ThenBy(user => user.Email)
            .ToListAsync(cancellationToken);

        var result = new List<StaffUserDto>(users.Count);
        foreach (var user in users)
        {
            result.Add(new StaffUserDto(
                user.Id,
                user.FullName,
                user.Email ?? string.Empty,
                user.IsActive,
                [.. await userManager.GetRolesAsync(user)]));
        }

        return result;
    }

    public async Task<Guid> CreateAsync(
        string fullName,
        string email,
        string password,
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken = default)
    {
        var tenantId = RequireTenant();
        await ValidateRolesAsync(roles);
        await EnforceUserLimitAsync(tenantId, cancellationToken);

        var user = new ApplicationUser
        {
            UserName = email.Trim().ToLowerInvariant(),
            Email = email.Trim().ToLowerInvariant(),
            EmailConfirmed = true,
            FullName = fullName.Trim(),
            TenantId = tenantId,
            IsActive = true,
        };

        var result = await userManager.CreateAsync(user, password);
        EnsureSucceeded(result, nameof(email));

        result = await userManager.AddToRolesAsync(user, roles);
        EnsureSucceeded(result, nameof(roles));

        return user.Id;
    }

    public async Task UpdateRolesAsync(Guid userId, IReadOnlyCollection<string> roles, CancellationToken cancellationToken = default)
    {
        var tenantId = RequireTenant();
        await ValidateRolesAsync(roles);

        var user = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId, cancellationToken)
            ?? throw new NotFoundException(nameof(ApplicationUser), userId);

        var currentRoles = await userManager.GetRolesAsync(user);
        var managedCurrentRoles = currentRoles.Where(Roles.StaffAssignable.Contains).ToArray();
        if (managedCurrentRoles.Length > 0)
        {
            EnsureSucceeded(await userManager.RemoveFromRolesAsync(user, managedCurrentRoles), nameof(roles));
        }

        EnsureSucceeded(await userManager.AddToRolesAsync(user, roles), nameof(roles));
    }

    private Guid RequireTenant()
        => tenantProvider.TenantId is { } tenantId && tenantId != Guid.Empty
            ? tenantId
            : throw new ValidationException([new ValidationFailure("tenant", "A tenant context is required to manage staff.")]);

    private async Task ValidateRolesAsync(IReadOnlyCollection<string> roles)
    {
        var invalidRole = roles.FirstOrDefault(role => !Roles.StaffAssignable.Contains(role, StringComparer.Ordinal));
        if (invalidRole is not null)
        {
            throw new ValidationException([new ValidationFailure(nameof(roles), $"Role '{invalidRole}' cannot be assigned from the tenant UI.")]);
        }

        foreach (var role in roles.Distinct(StringComparer.Ordinal))
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                throw new ValidationException([new ValidationFailure(nameof(roles), $"Role '{role}' is not configured.")]);
            }
        }
    }

    private async Task EnforceUserLimitAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var limits = await features.GetLimitsAsync(cancellationToken);
        if (limits.MaxUsers == PlanLimits.Unlimited)
        {
            return;
        }

        var users = await userManager.Users.CountAsync(user => user.TenantId == tenantId, cancellationToken);
        if (users >= limits.MaxUsers)
        {
            throw new ValidationException(
            [
                new ValidationFailure(nameof(ApplicationUser), $"Your current plan allows up to {limits.MaxUsers} users. Upgrade to add more staff."),
            ]);
        }
    }

    private static void EnsureSucceeded(IdentityResult result, string field)
    {
        if (result.Succeeded)
        {
            return;
        }

        throw new ValidationException(result.Errors
            .Select(error => new ValidationFailure(field, error.Description))
            .ToArray());
    }
}
