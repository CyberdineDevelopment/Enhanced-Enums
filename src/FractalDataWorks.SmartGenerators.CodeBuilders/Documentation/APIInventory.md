# API Inventory - FractalDataWorks.SmartGenerators.CodeBuilder

This document provides a comprehensive list of every public class, interface, enum, constructor, method, and overload in the **FractalDataWorks.SmartGenerators.CodeBuilder** project, including signatures and brief descriptions.

---

## AttributeBuilder
- **Constructor**
  - `public AttributeBuilder(string attributeName)`
- **Methods**
  - `public AttributeBuilder AddArgument(string argument)` — Adds an argument to the attribute.
  - `public AttributeBuilder AddAttribute(string attributeText)` — Adds a nested attribute (e.g. `[Obsolete]`).
  - `public override string Build()` — Builds the attribute declaration as a string.

---

## ClassBuilder

### ClassBuilder

- `ClassBuilder(string className)`
- `WithBaseType(string baseTypeName)`
- `WithBaseType<T>()`
- `ImplementsInterface(string interfaceName)`
- `ImplementsInterface<T>()`
- `WithNamespace(string namespaceName)`
- `AddMethod(string methodName, string returnTypeName)`
- `AddProperty(string propertyName, string propertyTypeName)`
- `AddField(string fieldType, string fieldName)`
- `AddField<T>(string fieldName)`
- `AddConstructor()`
- `AddCodeBlock(string blockContent)`
- `AddCodeBlock(Action<ICodeBuilder> blockBuilder)`

- **Methods**
  - `public FieldBuilder AddField(string fieldType, string fieldName)`
  - `public FieldBuilder AddField<T>(string fieldName)`
  - `public PropertyBuilder AddProperty(string propertyName, string propertyTypeName)`
  - `public MethodBuilder AddMethod(string methodName, string returnTypeName)`
  - `public MethodBuilder AddMethod<T>(string methodName)`
  - `public ConstructorBuilder AddConstructor()`
  - `public CodeBlockBuilder AddCodeBlock(string blockContent)`
  - `public CodeBlockBuilder AddCodeBlock(Action<ICodeBuilder> blockBuilder)`
  - `public override string Build()`

---

## CodeBlockBuilder

### CodeBlockBuilder

- `CodeBlockBuilder(string initialContent)`
- `CodeBlockBuilder(Action<ICodeBuilder> builderAction)`
- `CodeBlockBuilder()` — Initializes an empty code block builder.
- `AppendLine(string line)`
- `Indent()`
- `Outdent()`
- `AddStatement(string statement)` — Adds a statement to the block.
- `public override string Build()`

- **Methods**
  - `public CodeBlockBuilder AppendLine(string line)`
  - `public CodeBlockBuilder Indent()`
  - `public CodeBlockBuilder Outdent()`
  - `public CodeBlockBuilder AddStatement(string statement)` — Adds a statement to the block.
  - `public override string Build()`

---

## CodeBuilder

### CodeBuilder

- `CodeBuilder(int indentSize = 4)`
- `AppendLine(string line)`
- `Append(string text)`
- `Indent()`
- `Outdent()`
- `OpenBlock()`
- `CloseBlock()`
- `AppendGeneratedCodeHeader()`
- `AppendNamespace(string namespaceName)`
- `public override string ToString()`

- **Methods (implements ICodeBuilder)**
  - `public ICodeBuilder AppendLine(string line)`
  - `public ICodeBuilder Append(string text)`
  - `public ICodeBuilder Indent()`
  - `public ICodeBuilder Outdent()`
  - `public ICodeBuilder OpenBlock()`
  - `public ICodeBuilder CloseBlock()`
  - `public ICodeBuilder AppendGeneratedCodeHeader()`
  - `public ICodeBuilder AppendNamespace(string namespaceName)`
  - `public override string ToString()`

---

## CodeBuilderBase<TBuilder>

### CodeBuilderBase<TBuilder>

- `protected AccessModifier _accessModifier` — default public.
- `protected Modifiers _modifiers`
- `protected string? _xmlDocSummary`
- `WithAccessModifier(AccessModifier accessModifier)`
- `MakePublic()`
- `MakePrivate()`
- `MakeProtected()`
- `MakeInternal()`
- `MakeProtectedInternal()`
- `MakePrivateProtected()`
- `WithXmlDocSummary(string summary)`

- **Methods**
  - `public TBuilder WithAccessModifier(AccessModifier accessModifier)`
  - `public TBuilder MakePublic()`
  - `public TBuilder MakePrivate()`
  - `public TBuilder MakeProtected()`
  - `public TBuilder MakeInternal()`
  - `public TBuilder MakeProtectedInternal()`
  - `public TBuilder MakePrivateProtected()`
  - `public TBuilder WithXmlDocSummary(string summary)`

---

## CodeBuilderFactory

### CodeBuilderFactory

- `CreateCodeBuilder(int indentSize = 4)`
- `CreateClass(string className)`
- `CreateMethod(string methodName, string returnTypeName)`
- `CreateProperty(string propertyName, string propertyTypeName)`
- `CreateInterface(string interfaceName)`
- `CreateNamespace(string namespaceName)`
- `CreateField(string fieldName, string fieldTypeName)`
- `CreateConstructor(string className)`
- `CreateAttribute(string attributeName)`
- `CreateEnum(string enumName)`

- **Methods**
  - `public static ICodeBuilder CreateCodeBuilder(int indentSize = 4)`
  - `public static ClassBuilder CreateClass(string className)`
  - `public static MethodBuilder CreateMethod(string methodName, string returnTypeName)`
  - `public static PropertyBuilder CreateProperty(string propertyName, string propertyTypeName)`
  - `public static InterfaceBuilder CreateInterface(string interfaceName)`
  - `public static NamespaceBuilder CreateNamespace(string namespaceName)`
  - `public static FieldBuilder CreateField(string fieldName, string fieldTypeName)`
  - `public static ConstructorBuilder CreateConstructor(string className)`
  - `public static AttributeBuilder CreateAttribute(string attributeName)`
  - `public static EnumBuilder CreateEnum(string enumName)`

---

## ConstructorBuilder

### ConstructorBuilder

- `ConstructorBuilder(string className)`
- `AddParameter(string parameterTypeName, string parameterName, string? defaultValue = null)`
- `AddParameter<T>(string parameterName, string? defaultValue = null)`
- `WithParameter(string parameterName, string? defaultValue = null)`
- `AddBody(Action<ICodeBuilder> bodyBuilder)` — Sets constructor body via code builder.
- `AddBody(Action<CodeBlockBuilder> blockBuilder)` — Sets constructor body via CodeBlockBuilder enabling AddStatement.
- `WithCodeBlock(string blockContent)`
- `WithBaseCall(params string[] args)`
- `WithThisCall(params string[] args)`
- `public override string Build()`

- **Methods**
  - `public ConstructorBuilder AddParameter(string parameterTypeName, string parameterName, string? defaultValue = null)` — Adds a parameter with optional default.
  - `public ConstructorBuilder AddParameter<T>(string parameterName, string? defaultValue = null)` — Adds a parameter using a generic type.
  - `public ConstructorBuilder WithParameter(string parameterName, string? defaultValue = null)` — Configures an existing parameter.
  - `public ConstructorBuilder AddBody(Action<ICodeBuilder> bodyBuilder)` — Sets constructor body via code builder.
  - `public ConstructorBuilder AddBody(Action<CodeBlockBuilder> blockBuilder)` — Sets constructor body via CodeBlockBuilder enabling AddStatement.
  - `public ConstructorBuilder WithCodeBlock(string blockContent)` — Sets constructor body via string block.
  - `public ConstructorBuilder WithBaseCall(params string[] args)` — Adds base constructor call arguments.
  - `public ConstructorBuilder WithThisCall(params string[] args)` — Adds this constructor call arguments.
  - `public override string Build()` — Builds constructor declaration and body.

---

## EnumBuilder

### EnumBuilder

- `EnumBuilder(string enumName)`
- `AddMember(string memberName)`
- `public override string Build()`

- **Methods**
  - `public EnumBuilder AddMember(string memberName)` — Adds a member to the enum.
  - `public override string Build()` — Builds enum declaration.

---

## FieldBuilder

### FieldBuilder

- `FieldBuilder(string fieldName, string typeName)`
- `WithInitializer(CodeBlockBuilder initializerBuilder)`
- `MakeReadOnly()`
- `MakeConst(string value)`
- `WithInitializer(string initializer)`
- `AddAttribute(AttributeBuilder attributeBuilder)`
- `public override string Build()`

- **Methods**
  - `public FieldBuilder WithInitializer(CodeBlockBuilder initializerBuilder)` — Sets initializer via code builder.
  - `public FieldBuilder MakeReadOnly()` — Marks field as readonly.
  - `public FieldBuilder MakeConst(string value)` — Marks field as const with a value.
  - `public FieldBuilder WithInitializer(string initializer)` — Sets initializer via expression.
  - `public FieldBuilder AddAttribute(AttributeBuilder attributeBuilder)` — Adds an attribute to the field.
  - `public override string Build()` — Builds field declaration.

---

## InterfaceBuilder

### InterfaceBuilder

- `InterfaceBuilder(string interfaceName)`
- `WithBaseInterface(string baseInterfaceName)`
- `WithBaseInterface<T>()`
- `AddMethod(string methodName, string returnTypeName)`
- `AddProperty(string propertyName, string propertyTypeName)`
- `public override string Build()`

- **Methods**
  - `public InterfaceBuilder WithBaseInterface(string baseInterfaceName)` — Adds a base interface.
  - `public InterfaceBuilder WithBaseInterface<T>()` — Adds a base interface using a generic type.
  - `public MethodBuilder AddMethod(string methodName, string returnTypeName)` — Adds a method to the interface.
  - `public PropertyBuilder AddProperty(string propertyName, string propertyTypeName)` — Adds a property to the interface.
  - `public override string Build()` — Builds interface declaration.

---

## MemberBuilderBase<TBuilder>

### MemberBuilderBase<TBuilder>

- `public string Name { get; }` — Member name.
- `public string TypeName { get; }` — Member type name.
- `MakeVirtual()`
- `WithModifier(Modifiers modifier)`
- `protected string GetXmlDocumentation(string indent = "")` — Builds XML documentation comments.
- `protected string GetAttributes(string indent = "")` — Builds attributes as strings.
- `protected string GetDeclaration(string indent = "")` — Builds declaration prefix.

- **Methods**
  - `public TBuilder MakeVirtual()` — Marks member as virtual.
  - `public TBuilder WithModifier(Modifiers modifier)` — Adds a modifier.
  - `protected string GetXmlDocumentation(string indent = "")` — Builds XML documentation comments.
  - `protected string GetAttributes(string indent = "")` — Builds attributes as strings.
  - `protected string GetDeclaration(string indent = "")` — Builds declaration prefix.

---

## MethodBuilder

### MethodBuilder

- `MethodBuilder(string methodName, string returnTypeName)`
- `MethodBuilder(string methodName)` — Initializes with default void return type.
- `AddParameter(string parameterTypeName, string parameterName, string? defaultValue = null)`
- `AddParameter<T>(string parameterName, string? defaultValue = null)`
- `WithParameter(string parameterName, string? defaultValue = null)`
- `WithXmlDocParam(string parameterName, string description)`
- `WithXmlDocReturns(string description)`
- `WithXmlDocException(string exceptionType, string description)`
- `WithBody(Action<ICodeBuilder> bodyBuilder)`
- `WithCodeBlock(string blockContent)`
- `WithBody(string bodyContent)`
- `MakeAbstract()`
- `MakeAsync()`
- `WithExpressionBody(string expression)` — Sets an expression-bodied method.
- `public override string Build()`

- **Methods**
  - `public MethodBuilder AddParameter(string parameterTypeName, string parameterName, string? defaultValue = null)` — Adds method parameter.
  - `public MethodBuilder AddParameter<T>(string parameterName, string? defaultValue = null)` — Adds generic parameter.
  - `public MethodBuilder WithParameter(string parameterName, string? defaultValue = null)` — Configures existing parameter.
  - `public MethodBuilder WithXmlDocParam(string parameterName, string description)` — Adds XML `<param>` documentation.
  - `public MethodBuilder WithXmlDocReturns(string description)` — Adds XML `<returns>` documentation.
  - `public MethodBuilder WithXmlDocException(string exceptionType, string description)` — Adds XML `<exception>` documentation.
  - `public MethodBuilder WithBody(Action<ICodeBuilder> bodyBuilder)` — Sets method body via code builder.
  - `public MethodBuilder WithCodeBlock(string blockContent)` — Sets method body via string block.
  - `public MethodBuilder WithBody(string bodyContent)` — Sets method body via content.
  - `public MethodBuilder MakeAbstract()` — Marks method as abstract.
  - `public MethodBuilder MakeAsync()` — Marks method as async.
  - `public MethodBuilder WithExpressionBody(string expression)` — Adds expression-bodied syntax.
  - `public override string Build()` — Builds method declaration and body.

---

## NamespaceBuilder

### NamespaceBuilder

- `NamespaceBuilder(string namespaceName)`
- `AddUsing(string usingStatement)`
- `WithUsing(string usingStatement)`
- `AddClass(ClassBuilder classBuilder)`
- `WithClass(ClassBuilder classBuilder)`
- `AddInterface(InterfaceBuilder interfaceBuilder)`
- `AddEnum(EnumBuilder enumBuilder)`
- `WithEnum(EnumBuilder enumBuilder)`
- `public string Build()`
- **Implements** `ICodeBuilder` methods: `AppendLine`, `Append`, `Indent`, `Outdent`, `OpenBlock`, `CloseBlock`, `AppendGeneratedCodeHeader`, `AppendNamespace`.

- **Methods**
  - `public NamespaceBuilder AddUsing(string usingStatement)` — Adds a using directive.
  - `public NamespaceBuilder WithUsing(string usingStatement)` — Alternative naming for using directive.
  - `public NamespaceBuilder AddClass(ClassBuilder classBuilder)` — Adds a class to the namespace.
  - `public NamespaceBuilder WithClass(ClassBuilder classBuilder)` — Alternative class addition.
  - `public NamespaceBuilder AddInterface(InterfaceBuilder interfaceBuilder)` — Adds an interface to the namespace.
  - `public NamespaceBuilder AddEnum(EnumBuilder enumBuilder)` — Adds an enum to the namespace.
  - `public NamespaceBuilder WithEnum(EnumBuilder enumBuilder)` — Alternative enum addition.
  - `public string Build()` — Builds namespace code.
  - **Implements** `ICodeBuilder` methods: `AppendLine`, `Append`, `Indent`, `Outdent`, `OpenBlock`, `CloseBlock`, `AppendGeneratedCodeHeader`, `AppendNamespace`.

---

## PropertyBuilder

### PropertyBuilder

- `PropertyBuilder(string propertyName, string propertyTypeName)`
- `WithGetter(Action<ICodeBuilder> getterBuilder)`
- `WithGetter(string expression)`
- `WithSetter(Action<ICodeBuilder> setterBuilder)`
- `WithSetter(string expression)`
- `WithInitializer(string initializerExpression)`
- `WithInitializer(Action<ICodeBuilder> initializerBuilder)`
- `WithAccessors(PropertyAccessor accessors)`
- `WithXmlDoc(string xmlDoc)`
- `WithAttribute(AttributeBuilder attributeBuilder)`
- `WithModifier(Modifiers modifier)`
- `MakeReadOnly()` — Removes the setter, making this a read-only property.
- `public override string Build()`

- **Methods**
  - `public PropertyBuilder WithGetterBody(CodeBlockBuilder getterBuilder)` — Sets getter body via builder.
  - `public PropertyBuilder WithSetterBody(CodeBlockBuilder setterBuilder)` — Sets setter body via builder.
  - `public PropertyBuilder WithGetter(string expression)` — Sets getter body from a string expression (wraps in CodeBlockBuilder).
  - `public PropertyBuilder WithSetter(string expression)` — Sets setter body from a string expression (wraps in CodeBlockBuilder).
  - `public PropertyBuilder WithSetterAccessModifier(AccessModifier accessModifier)` — Sets setter access modifier.
  - `public PropertyBuilder MakeSetterPrivate()` — Marks setter private.
  - `public PropertyBuilder MakeSetterProtected()` — Marks setter protected.
  - `public PropertyBuilder MakeSetterInternal()` — Marks setter internal.
  - `public PropertyBuilder MakeSetterProtectedInternal()` — Marks setter protected internal.
  - `public PropertyBuilder WithInitializer(string initializer)` — Sets initializer.
  - `public new PropertyBuilder AddAttribute(string attributeText)` — Adds attribute to property.
  - `public override string Build()` — Builds property declaration.

---

## Interfaces
### IBuildable
- `public interface IBuildable`
  - `string Build()` — Generates code.

### ICodeElement
- `public interface ICodeElement`
  - `void Generate(ICodeBuilder codeBuilder)` — Generates code element.

### ICodeBuilder
- `public interface ICodeBuilder`
  - `ICodeBuilder AppendLine(string line = "")`
  - `ICodeBuilder Append(string text)`
  - `ICodeBuilder Indent()`
  - `ICodeBuilder Outdent()`
  - `ICodeBuilder OpenBlock()`
  - `ICodeBuilder CloseBlock()`
  - `ICodeBuilder AppendGeneratedCodeHeader()`
  - `ICodeBuilder AppendNamespace(string namespaceName)`
  - `string Build()`
  - `string ToString()`

### IMemberBuilder<out TBuilder>
- `public interface IMemberBuilder<out TBuilder> : IBuildable`
  - `string Name { get; }`
  - `TBuilder WithAccessModifier(AccessModifier accessModifier)`
  - `TBuilder WithModifier(Modifiers modifier)`
  - `TBuilder WithXmlDocSummary(string summary)`

---

## Enums
### AccessModifier
- `None`
- `Public`
- `Private`
- `Protected`
- `Internal`
- `ProtectedInternal`
- `PrivateProtected`

### Modifiers
- `None`
- `Static`
- `ReadOnly`
- `Override`
- `Sealed`
- `Virtual`
- `Abstract`
- `Async`
- `Partial`
- `This`
- `Required`

---

## Extensions
### MethodBuilderExtensions
- `public static class MethodBuilderExtensions`
  - `public static MethodBuilder WithXmlDocParam(this MethodBuilder builder, string paramName, string description)`
  - `public static MethodBuilder WithXmlDocReturns(this MethodBuilder builder, string description)`
  - `public static MethodBuilder WithXmlDocException(this MethodBuilder builder, string exceptionType, string description)`
  - `public static IReadOnlyList<XmlDocParam> GetXmlDocParams(this MethodBuilder builder)`
  - `public static string GetXmlDocReturns(this MethodBuilder builder)`
  - `public static IReadOnlyList<XmlDocExceptionInformation> GetXmlDocExceptions(this MethodBuilder builder)`
- **Nested Types**
  - `XmlDocParam`
  - `XmlDocExceptionInformation`
