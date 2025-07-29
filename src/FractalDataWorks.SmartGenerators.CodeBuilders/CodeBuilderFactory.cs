namespace FractalDataWorks.SmartGenerators.CodeBuilders;

/// <summary>
/// Factory class for creating code generation builders.
/// </summary>
// DESIGN PATTERN: Factory Method Pattern
// This class implements the Factory Method pattern from Gang of Four.
// It centralizes object creation and hides the instantiation logic,
// providing a unified interface to create different types of builders.
public static class CodeBuilderFactory
{
    /// <summary>
    /// Creates a new code builder.
    /// </summary>
    /// <param name="indentSize">The indentation size in spaces.</param>
    /// <returns>A new code builder instance.</returns>
    public static ICodeBuilder CreateCodeBuilder(int indentSize = 4) => new CodeBuilder(indentSize);

    /// <summary>
    /// Creates a new class builder.
    /// </summary>
    /// <param name="className">The name of the class.</param>
    /// <returns>A new class builder instance.</returns>
    public static ClassBuilder CreateClass(string className) => new(className);

    /// <summary>
    /// Creates a new method builder.
    /// </summary>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="returnTypeName">The return type of the method.</param>
    /// <returns>A new method builder instance.</returns>
    public static MethodBuilder CreateMethod(string methodName, string returnTypeName) => new(methodName, returnTypeName);

    /// <summary>
    /// Creates a new property builder.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="propertyTypeName">The type of the property.</param>
    /// <returns>A new property builder instance.</returns>
    public static PropertyBuilder CreateProperty(string propertyName, string propertyTypeName) => new(propertyName, propertyTypeName);

    /// <summary>
    /// Creates a new interface builder.
    /// </summary>
    /// <param name="interfaceName">The name of the interface.</param>
    /// <returns>A new interface builder instance.</returns>
    public static InterfaceBuilder CreateInterface(string interfaceName) => new(interfaceName);

    /// <summary>
    /// Creates a new namespace builder.
    /// </summary>
    /// <param name="namespaceName">The name of the namespace.</param>
    /// <returns>A new namespace builder instance.</returns>
    public static NamespaceBuilder CreateNamespace(string namespaceName) => new(namespaceName);

    /// <summary>
    /// Creates a new field builder.
    /// </summary>
    /// <param name="fieldName">The name of the field.</param>
    /// <param name="fieldTypeName">The type of the field.</param>
    /// <returns>A new field builder instance.</returns>
    public static FieldBuilder CreateField(string fieldName, string fieldTypeName) => new(fieldName, fieldTypeName);

    /// <summary>
    /// Creates a new constructor builder.
    /// </summary>
    /// <param name="className">The name of the class that this constructor is for.</param>
    /// <returns>A new constructor builder instance.</returns>
    public static ConstructorBuilder CreateConstructor(string className) => new(className);

    /// <summary>
    /// Creates a new attribute builder.
    /// </summary>
    /// <param name="attributeName">The name of the attribute.</param>
    /// <returns>A new attribute builder instance.</returns>
    public static AttributeBuilder CreateAttribute(string attributeName) => new(attributeName);

    /// <summary>
    /// Creates a new enum builder.
    /// </summary>
    /// <param name="enumName">The name of the enum.</param>
    /// <returns>A new enum builder instance.</returns>
    public static EnumBuilder CreateEnum(string enumName) => new(enumName);
}
