namespace FractalDataWorks.SmartGenerators.CodeBuilders;

/// <summary>
/// Builder for record parameters.
/// </summary>
public class RecordParameterBuilder
{
    private readonly RecordBuilder.RecordParameterInfo _parameter;

    internal RecordParameterBuilder(string type, string name)
    {
        _parameter = new RecordBuilder.RecordParameterInfo
        {
            Type = type,
            Name = name,
        };
    }

    /// <summary>
    /// Sets the default value for the parameter.
    /// </summary>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The builder instance for chaining.</returns>
    public RecordParameterBuilder WithDefaultValue(string defaultValue)
    {
        _parameter.DefaultValue = defaultValue;
        return this;
    }

    internal RecordBuilder.RecordParameterInfo Build() => _parameter;
}
