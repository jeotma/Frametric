using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Frametric.Domain.Entities;
using Frametric.Infrastructure.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Frametric.UnitTests;

public class SecurityTests
{
    [Fact]
    public void PasswordHasher_ShouldHashAndPasswordVerificationShouldSucceed()
    {
        // Arrange
        var passwordHasher = new PasswordHasher();
        var password = "SuperSecretPassword123!";

        // Act
        var hash = passwordHasher.Hash(password);
        var verifySuccess = passwordHasher.Verify(password, hash);
        var verifyFail = passwordHasher.Verify("WrongPassword", hash);

        // Assert
        Assert.NotNull(hash);
        Assert.Contains(".", hash);
        Assert.True(verifySuccess);
        Assert.False(verifyFail);
    }

    [Fact]
    public void JwtTokenGenerator_ShouldGenerateValidAccessToken()
    {
        // Arrange
        var settings = new JwtSettings
        {
            Secret = "SuperSecretSecurityKeyThatNeedsToBeLongEnoughForHMAC256Matches32BytesLength!",
            Issuer = "FrametricTest",
            Audience = "FrametricUsersTest",
            ExpiryMinutes = 60,
            RefreshTokenExpiryDays = 7
        };
        var options = Options.Create(settings);
        var generator = new JwtTokenGenerator(options);
        
        var user = new User(Guid.NewGuid(), "john_doe", "john@example.com", "some_hash");

        // Act
        var token = generator.GenerateAccessToken(user);
        var refreshToken = generator.GenerateRefreshToken();

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.NotNull(refreshToken);
        Assert.Equal(88, refreshToken.Length); // 64 bytes base64 encoded is 88 chars

        // Decode and verify Jwts
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = settings.Issuer,
            ValidAudience = settings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Secret))
        };

        var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
        Assert.NotNull(principal);
        Assert.NotNull(validatedToken);

        var nameIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var nameClaim = principal.FindFirst(ClaimTypes.Name)?.Value;
        var emailClaim = principal.FindFirst(ClaimTypes.Email)?.Value;

        Assert.Equal(user.Id.ToString(), nameIdClaim);
        Assert.Equal(user.Username, nameClaim);
        Assert.Equal(user.Email, emailClaim);
    }
}
