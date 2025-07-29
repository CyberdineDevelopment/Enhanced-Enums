using System;

namespace FractalDataWorks.SmartGenerators.CodeBuilders;

/// <summary>
/// Extension methods for <see cref="ClassBuilder"/> to allow fluent configuration overloads.
/// </summary>
public static class ClassBuilderExtensions
{
    /// <summary>
    /// Adds a field to the class and configures it.
    /// </summary>
    /// <param name="builder">The class builder to add the field to.</param>
    /// <param name="fieldType">The type of the field.</param>
    /// <param name="fieldName">The name of the field.</param>
    /// <param name="configure">The action to configure the created <see cref="FieldBuilder"/>.</param>
    /// <returns>The <see cref="ClassBuilder"/> for chaining.</returns>
    public static ClassBuilder AddField(this ClassBuilder builder, string fieldType, string fieldName, Action<FieldBuilder> configure)
    {
        if (builder is null) throw new ArgumentNullException(nameof(builder));
        if (configure is null) throw new ArgumentNullException(nameof(configure));

        var field = builder.AddField(fieldType, fieldName);
        configure(field);
        return builder;
    }

    /// <summary>
    /// Adds a constructor to the class and configures it.
    /// </summary>
    /// <param name="builder">The class builder to add the constructor to.</param>
    /// <param name="configure">The action to configure the created <see cref="ConstructorBuilder"/>.</param>
    /// <returns>The <see cref="ClassBuilder"/> for chaining.</returns>
    public static ClassBuilder AddConstructor(this ClassBuilder builder, Action<ConstructorBuilder> configure)
    {
        if (builder is null) throw new ArgumentNullException(nameof(builder));
        if (configure is null) throw new ArgumentNullException(nameof(configure));

        var ctor = builder.AddConstructor();
        configure(ctor);
        return builder;
    }

    /// <summary>
    /// Adds a method to the class and configures it.
    /// </summary>
    /// <param name="builder">The class builder to add the method to.</param>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="returnType">The return type of the method.</param>
    /// <param name="configure">The action to configure the created <see cref="MethodBuilder"/>.</param>
    /// <returns>The <see cref="ClassBuilder"/> for chaining.</returns>
    public static ClassBuilder AddMethod(this ClassBuilder builder, string methodName, string returnType, Action<MethodBuilder> configure)
    {
        if (builder is null) throw new ArgumentNullException(nameof(builder));
        if (configure is null) throw new ArgumentNullException(nameof(configure));

        var method = builder.AddMethod(methodName, returnType);
        configure(method);
        return builder;
    }
}
