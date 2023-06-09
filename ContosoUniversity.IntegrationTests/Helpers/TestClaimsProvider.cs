﻿using System.Security.Claims;

namespace ContosoUniversity.IntegrationTests.Helpers;

/// <summary>
/// <see href="https://github.com/gpeipman/AspNetCoreTests/blob/master/AspNetCoreTests/AspNetCoreTests.IntegrationTests/Helpers/TestClaimsProvider.cs"/>
/// </summary>
public class TestClaimsProvider
{
    public IList<Claim> Claims { get; }

    public TestClaimsProvider(IList<Claim> claims)
    {
        Claims = claims;
    }

    public TestClaimsProvider()
    {
        Claims = new List<Claim>();
    }

    public static TestClaimsProvider WithAdminClaims()
    {
        var provider = new TestClaimsProvider();
        provider.Claims.Add(new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()));
        provider.Claims.Add(new Claim(ClaimTypes.Name, "Admin user"));
        provider.Claims.Add(new Claim(ClaimTypes.Role, "Admin"));

        return provider;
    }

    public static TestClaimsProvider WithUserClaims()
    {
        var provider = new TestClaimsProvider();
        provider.Claims.Add(new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()));
        provider.Claims.Add(new Claim(ClaimTypes.Name, "User"));

        return provider;
    }
}