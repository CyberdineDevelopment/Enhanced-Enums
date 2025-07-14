# SmartGenerators Testing API Quick Reference

This document provides a quick reference for the SmartGenerators testing utilities API.

## SourceGeneratorTestHelper

Primary utility for running generators and capturing output.

### Methods

```csharp
// Run generator and get output files
public static Dictionary<string, string> RunGenerator(
    IIncrementalGenerator generator,
    string[] sources,
    out ImmutableArray<Diagnostic> diagnostics,
    params MetadataReference[] additionalReferences)

// Run generator and compile the result
public static (Compilation outputCompilation, GeneratorRunResult runResult) 
    RunGeneratorAndCompile(
        IIncrementalGenerator generator,
        string[] sources,
        params MetadataReference[] additionalReferences)

// Get syntax tree from generated output
public static SyntaxTree GetSyntaxTree(
    Dictionary<string, string> generatedOutput,
    string hintName)

// Compile code without running a generator
public static Compilation CompileCode(
    string[] sources,
    out ImmutableArray<Diagnostic> diagnostics,
    params MetadataReference[] additionalReferences)
```

### Usage Example

```csharp
var generator = new MyGenerator();
var sources = new[] { "public class Test { }" };

// Run generator
var output = SourceGeneratorTestHelper.RunGenerator(
    generator, 
    sources, 
    out var diagnostics);

// Check output
output.ShouldContainKey("Test.g.cs");
diagnostics.ShouldBeEmpty();
```

## ExpectationsFactory

Fluent API for creating structural assertions on generated code.

### Entry Points

```csharp
// From code string
public static SyntaxTreeExpectations ExpectCode(string code)

// From generated output dictionary
public static SyntaxTreeExpectations ExpectFile(
    Dictionary<string, string> generatedSources, 
    string fileName)

// From syntax tree
public static SyntaxTreeExpectations Expect(SyntaxTree syntaxTree)

// From compilation unit
public static CompilationUnitExpectations Expect(CompilationUnitSyntax compilationUnit)
```

### Usage Example

```csharp
ExpectationsFactory.ExpectCode(generatedCode)
    .HasNamespace("MyNamespace", ns => ns
        .HasClass("MyClass", c => c
            .IsPublic()
            .HasMethod("MyMethod")))
    .Assert();
```

## SyntaxTreeExpectations

Root expectations for validating syntax trees.

### Methods

```csharp
// Validate namespace
HasNamespace(string namespaceName)
HasNamespace(string namespaceName, Action<NamespaceExpectations> expectations)

// Validate class
HasClass(string className)
HasClass(string className, Action<ClassExpectations> expectations)

// Validate using directives
HasUsing(string usingDirective)

// Execute assertions
Assert()
```

## NamespaceExpectations

Validate namespace contents.

### Methods

```csharp
// Validate using directives
HasUsing(string usingDirective)

// Validate types
HasClass(string className)
HasClass(string className, Action<ClassExpectations> expectations)
HasInterface(string interfaceName)
HasInterface(string interfaceName, Action<InterfaceExpectations> expectations)
HasEnum(string enumName)
HasEnum(string enumName, Action<EnumExpectations> expectations)
```

## ClassExpectations

Validate class structure and members.

### Access Modifiers

```csharp
IsPublic()
IsPrivate()
IsProtected()
IsInternal()
IsProtectedInternal()
IsPrivateProtected()
```

### Type Modifiers

```csharp
IsStatic()
IsAbstract()
IsSealed()
IsPartial()
IsReadOnly()
IsRef()
```

### Inheritance

```csharp
HasBaseType(string baseTypeName)
ImplementsInterface(string interfaceName)
```

### Members

```csharp
// Constructors
HasConstructor()
HasConstructor(Action<ConstructorExpectations> expectations)
HasStaticConstructor()

// Methods
HasMethod(string methodName)
HasMethod(string methodName, Action<MethodExpectations> expectations)

// Properties
HasProperty(string propertyName)
HasProperty(string propertyName, Action<PropertyExpectations> expectations)

// Fields
HasField(string fieldName)
HasField(string fieldName, Action<FieldExpectations> expectations)

// Nested types
HasNestedClass(string className)
HasNestedClass(string className, Action<ClassExpectations> expectations)
HasNestedInterface(string interfaceName)
HasNestedEnum(string enumName)
```

## MethodExpectations

Validate method signatures and structure.

### Access Modifiers

```csharp
IsPublic()
IsPrivate()
IsProtected()
IsInternal()
```

### Method Modifiers

```csharp
IsStatic()
IsAsync()
IsOverride()
IsVirtual()
IsAbstract()
IsSealed()
IsExtern()
IsPartial()
```

### Signature

```csharp
HasReturnType(string typeName)
HasParameter(string parameterName)
HasParameter(string parameterName, Action<ParameterExpectations> expectations)
HasNoParameters()
HasGenericParameter(string parameterName)
HasGenericParameter(string parameterName, Action<GenericParameterExpectations> expectations)
```

### Body

```csharp
HasBody()
HasBody(string expectedBody)
HasNoBody()
```

## PropertyExpectations

Validate property declarations.

### Access Modifiers

```csharp
IsPublic()
IsPrivate()
IsProtected()
IsInternal()
IsProtectedInternal()
IsPrivateProtected()
```

### Property Modifiers

```csharp
IsStatic()
IsOverride()
IsVirtual()
IsAbstract()
IsSealed()
IsReadOnly()
IsRequired()
```

### Type and Accessors

```csharp
HasType(string typeName)
HasGetter()
HasSetter()
HasInitSetter()
IsAutoProperty()
```

## FieldExpectations

Validate field declarations.

### Access Modifiers

```csharp
IsPrivate()
IsPublic()
IsProtected()
IsInternal()
```

### Field Modifiers

```csharp
IsStatic()
IsReadOnly()
IsConst()
IsVolatile()
IsRequired()
```

### Type and Initialization

```csharp
HasType(string typeName)
HasInitializer()
HasInitializer(string expectedValue)
HasNoInitializer()
```

## ParameterExpectations

Validate method parameters.

### Type

```csharp
HasType(string typeName)
```

### Modifiers

```csharp
IsRef()
IsOut()
IsIn()
IsParams()
IsThis()
```

### Default Value

```csharp
HasDefaultValue()
HasDefaultValue(string expectedValue)
HasNoDefaultValue()
```

## InterfaceExpectations

Validate interface declarations.

### Access Modifiers

```csharp
IsPublic()
IsPrivate()
IsProtected()
IsInternal()
```

### Members

```csharp
HasMethod(string methodName)
HasMethod(string methodName, Action<MethodExpectations> expectations)
HasProperty(string propertyName)
HasProperty(string propertyName, Action<PropertyExpectations> expectations)
```

## EnumExpectations

Validate enum declarations.

### Access Modifiers

```csharp
IsPublic()
IsPrivate()
IsProtected()
IsInternal()
```

### Structure

```csharp
HasBaseType(string baseTypeName)
HasValue(string valueName)
HasValue(string valueName, int expectedValue)
HasValue(string valueName, Action<EnumValueExpectations> expectations)
```

## ConstructorExpectations

Validate constructor declarations.

### Access Modifiers

```csharp
IsPublic()
IsPrivate()
IsProtected()
IsInternal()
```

### Parameters

```csharp
HasParameter(string parameterName)
HasParameter(string parameterName, Action<ParameterExpectations> expectations)
HasNoParameters()
```

### Constructor Initializer

```csharp
HasBaseInitializer()
HasThisInitializer()
HasNoInitializer()
```

## Common Patterns

### Basic Class Validation

```csharp
ExpectationsFactory.ExpectCode(code)
    .HasNamespace("MyNamespace", ns => ns
        .HasClass("MyClass", c => c
            .IsPublic()
            .IsPartial()
            .HasConstructor(ctor => ctor
                .IsPublic()
                .HasParameter("name", p => p.HasType("string")))
            .HasProperty("Name", p => p
                .IsPublic()
                .HasType("string")
                .HasGetter()
                .HasSetter())))
    .Assert();
```

### Interface Implementation

```csharp
ExpectationsFactory.ExpectCode(code)
    .HasClass("MyClass", c => c
        .ImplementsInterface("IMyInterface")
        .HasMethod("InterfaceMethod", m => m
            .IsPublic()
            .HasReturnType("void")))
    .Assert();
```

### Static Factory Method

```csharp
ExpectationsFactory.ExpectCode(code)
    .HasClass("Factory", c => c
        .IsPublic()
        .IsStatic()
        .HasMethod("Create", m => m
            .IsPublic()
            .IsStatic()
            .HasReturnType("Product")
            .HasParameter("type", p => p.HasType("string"))))
    .Assert();
```

### Enum with Values

```csharp
ExpectationsFactory.ExpectCode(code)
    .HasEnum("Status", e => e
        .IsPublic()
        .HasValue("Active", 1)
        .HasValue("Inactive", 2)
        .HasValue("Pending", 3))
    .Assert();
```

## Tips

1. **Chain Expectations**: Build complex validations by chaining method calls
2. **Use Actions**: Pass actions to configure nested expectations
3. **Be Specific**: Only test what matters; don't over-specify
4. **Assert Once**: Call `Assert()` at the end to execute all validations
5. **Readable Tests**: Structure expectations to match code structure

## Common Issues

### "No suitable method found"
Ensure you're using the correct expectation type for your validation.

### "Expected X but found Y"
Check spelling, namespaces, and access modifiers carefully.

### "Cannot find type/member"
Verify the generated code structure matches your expectations.

## See Also

- [EnhancedEnumOptions Testing Guide](README.md)
- [Testing Patterns](TESTING_PATTERNS.md)
- [SmartGenerators Documentation](/mnt/c/development/FractalDataWorks/SmartGenerators/docs/)