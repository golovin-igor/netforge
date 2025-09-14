using DataAnnotationValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;
using System.Net;

namespace NetForge.Simulation.DataTypes.Validation;

/// <summary>
/// Base class for configuration objects with common validation methods
/// </summary>
public abstract class ConfigurationBase : IValidatable
{
    /// <summary>
    /// Validates this configuration object
    /// </summary>
    public abstract ValidationResult Validate();

    /// <summary>
    /// Validates that a string value is not null or empty
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="fieldName">The name of the field being validated</param>
    protected ValidationResult ValidateRequired(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ValidationResult.Error($"{fieldName} is required");
        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates that an integer value is within a specified range
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="min">The minimum allowed value (inclusive)</param>
    /// <param name="max">The maximum allowed value (inclusive)</param>
    /// <param name="fieldName">The name of the field being validated</param>
    protected ValidationResult ValidateRange(int value, int min, int max, string fieldName)
    {
        if (value < min || value > max)
            return ValidationResult.Error($"{fieldName} must be between {min} and {max} (got {value})");
        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates that a double value is within a specified range
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="min">The minimum allowed value (inclusive)</param>
    /// <param name="max">The maximum allowed value (inclusive)</param>
    /// <param name="fieldName">The name of the field being validated</param>
    protected ValidationResult ValidateRange(double value, double min, double max, string fieldName)
    {
        if (value < min || value > max)
            return ValidationResult.Error($"{fieldName} must be between {min} and {max} (got {value})");
        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates that an integer value is positive
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="fieldName">The name of the field being validated</param>
    protected ValidationResult ValidatePositive(int value, string fieldName)
    {
        if (value <= 0)
            return ValidationResult.Error($"{fieldName} must be positive (got {value})");
        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates that a double value is positive
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="fieldName">The name of the field being validated</param>
    protected ValidationResult ValidatePositive(double value, string fieldName)
    {
        if (value <= 0)
            return ValidationResult.Error($"{fieldName} must be positive (got {value})");
        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates that a string represents a valid IP address
    /// </summary>
    /// <param name="value">The IP address string to validate</param>
    /// <param name="fieldName">The name of the field being validated</param>
    protected ValidationResult ValidateIpAddress(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ValidationResult.Error($"{fieldName} is required");

        if (!IPAddress.TryParse(value, out _))
            return ValidationResult.Error($"{fieldName} must be a valid IP address (got '{value}')");

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates that a string represents a valid MAC address
    /// </summary>
    /// <param name="value">The MAC address string to validate</param>
    /// <param name="fieldName">The name of the field being validated</param>
    protected ValidationResult ValidateMacAddress(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ValidationResult.Error($"{fieldName} is required");

        // Accept formats: XX:XX:XX:XX:XX:XX, XX-XX-XX-XX-XX-XX, XXXXXXXXXXXX
        var macPatterns = new[]
        {
            @"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$", // XX:XX:XX:XX:XX:XX or XX-XX-XX-XX-XX-XX
            @"^[0-9A-Fa-f]{12}$" // XXXXXXXXXXXX
        };

        bool isValid = macPatterns.Any(pattern => System.Text.RegularExpressions.Regex.IsMatch(value, pattern));

        if (!isValid)
            return ValidationResult.Error($"{fieldName} must be a valid MAC address (got '{value}')");

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates that a port number is within the valid range
    /// </summary>
    /// <param name="port">The port number to validate</param>
    /// <param name="fieldName">The name of the field being validated</param>
    protected ValidationResult ValidatePort(int port, string fieldName)
    {
        return ValidateRange(port, 1, 65535, fieldName);
    }

    /// <summary>
    /// Validates that a VLAN ID is within the valid range
    /// </summary>
    /// <param name="vlanId">The VLAN ID to validate</param>
    /// <param name="fieldName">The name of the field being validated</param>
    protected ValidationResult ValidateVlanId(int vlanId, string fieldName)
    {
        return ValidateRange(vlanId, 1, 4094, fieldName);
    }

    /// <summary>
    /// Validates multiple validation results and combines them
    /// </summary>
    /// <param name="validationResults">The validation results to combine</param>
    protected ValidationResult CombineResults(params ValidationResult[] validationResults)
    {
        return validationResults.Aggregate(ValidationResult.Success(), (result, next) => result.Combine(next));
    }
}