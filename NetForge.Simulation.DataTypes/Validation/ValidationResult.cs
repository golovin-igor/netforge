namespace NetForge.Simulation.DataTypes.Validation;

/// <summary>
/// Result of a validation operation containing errors and warnings
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Gets whether the validation was successful (no errors)
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Gets the list of validation errors
    /// </summary>
    public List<string> Errors { get; init; } = [];

    /// <summary>
    /// Gets the list of validation warnings
    /// </summary>
    public List<string> Warnings { get; init; } = [];

    /// <summary>
    /// Creates a successful validation result
    /// </summary>
    public static ValidationResult Success() => new() { IsValid = true };

    /// <summary>
    /// Creates a validation result with a single error
    /// </summary>
    /// <param name="message">The error message</param>
    public static ValidationResult Error(string message) => new()
    {
        IsValid = false,
        Errors = [message]
    };

    /// <summary>
    /// Creates a validation result with multiple errors
    /// </summary>
    /// <param name="errors">The error messages</param>
    public static ValidationResult WithErrors(params string[] errors) => new()
    {
        IsValid = false,
        Errors = [.. errors]
    };

    /// <summary>
    /// Creates a validation result with errors and warnings
    /// </summary>
    /// <param name="errors">The error messages</param>
    /// <param name="warnings">The warning messages</param>
    public static ValidationResult WithErrorsAndWarnings(string[] errors, string[] warnings) => new()
    {
        IsValid = errors.Length == 0,
        Errors = [.. errors],
        Warnings = [.. warnings]
    };

    /// <summary>
    /// Creates a validation result with only warnings (still valid)
    /// </summary>
    /// <param name="warnings">The warning messages</param>
    public static ValidationResult WithWarnings(params string[] warnings) => new()
    {
        IsValid = true,
        Warnings = [.. warnings]
    };

    /// <summary>
    /// Combines this validation result with another
    /// </summary>
    /// <param name="other">The other validation result to combine</param>
    /// <returns>A combined validation result</returns>
    public ValidationResult Combine(ValidationResult other)
    {
        var allErrors = Errors.Concat(other.Errors).ToList();
        var allWarnings = Warnings.Concat(other.Warnings).ToList();

        return new ValidationResult
        {
            IsValid = allErrors.Count == 0,
            Errors = allErrors,
            Warnings = allWarnings
        };
    }

    /// <summary>
    /// Gets a formatted string representation of all errors and warnings
    /// </summary>
    public override string ToString()
    {
        var messages = new List<string>();

        if (Errors.Count > 0)
        {
            messages.Add($"Errors: {string.Join(", ", Errors)}");
        }

        if (Warnings.Count > 0)
        {
            messages.Add($"Warnings: {string.Join(", ", Warnings)}");
        }

        return messages.Count > 0 ? string.Join("; ", messages) : "Valid";
    }
}