namespace Mediator.Abstractions;

/// <summary>
/// Marker attribute to trigger source generation for the Mediator.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public class MediatorOptionsAttribute : Attribute
{
    /// <summary>
    /// The namespace where the generated Mediator should reside.
    /// </summary>
    public string Namespace { get; set; } = "Mediator.Generated";
}
