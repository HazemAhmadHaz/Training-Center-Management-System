using System.ComponentModel.DataAnnotations;

/// <summary>
/// Represents the configuration settings required for JWT authentication.
///
/// This class binds values from the "Jwt" section in appsettings.json
/// and provides strongly typed access to JWT configuration values.
///
/// Benefits:
///
/// 1. Strongly Typed Configuration:
///    Avoids using string keys throughout the application and provides
///    compile-time checking when accessing JWT settings.
///
/// 2. Configuration Validation:
///    Uses Data Annotation attributes and ValidateOnStart() to verify that
///    required settings such as Key, Issuer, Audience, and expiration values
///    are correctly configured before the application starts.
///
/// 3. Fail Fast Approach:
///    Prevents the API from running with invalid security configuration.
///    For example, missing JWT keys are detected during startup instead of
///    causing authentication failures during user login.
///
/// 4. Improved Security:
///    Ensures that required authentication settings are always present before
///    generating and validating JWT tokens.
///
/// 5. Easier Maintenance:
///    Keeps authentication configuration centralized in one place, making
///    changes easier when moving between development, testing, and production
///    environments.
///
/// Example flow:
///
/// appsettings.json
///        ↓
/// JwtSettings class
///        ↓
/// Configuration validation
///        ↓
/// Application starts only if settings are valid
///
/// This follows the principle of failing early by detecting configuration
/// problems before they affect users.
/// </summary>

namespace TrainingCenterAPI.Configurations
{
    public class JwtSettings
    {
        [Required]
        public string Key { get; set; } = string.Empty;

        [Required]
        public string Issuer { get; set; } = string.Empty;

        [Required]
        public string Audience { get; set; } = string.Empty;

        [Required]
        public int ExpiryMinutes { get; set; }
    }
}