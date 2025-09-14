namespace NetForge.Simulation.DataTypes.Validation;

/// <summary>
/// Interface for objects that can be validated
/// </summary>
public interface IValidatable
{
    /// <summary>
    /// Validates the object and returns validation result
    /// </summary>
    /// <returns>ValidationResult containing validation status and any errors/warnings</returns>
    ValidationResult Validate();
}