# FractalDataWorks.SmartGenerators.CodeBuilders

Fluent API for generating well-formatted C# code with builders for all major language constructs.

## Installation

```bash
dotnet add package FractalDataWorks.SmartGenerators.CodeBuilders
```

## Features

- **Fluent Builder Pattern** - Chain methods for intuitive code generation
- **Complete C# Coverage** - Builders for classes, interfaces, methods, properties, and more
- **Automatic Formatting** - Proper indentation and spacing
- **XML Documentation** - Built-in support for generating documentation
- **Modifiers & Attributes** - Full support for access modifiers and attributes

## Usage

### Building a Class

```csharp
var classCode = new ClassBuilder("UserService")
    .MakePublic()
    .MakeSealed()
    .WithSummary("Service for managing users")
    .AddInterface("IUserService")
    .AddField("ILogger<UserService>", "_logger", field => field
        .MakePrivate()
        .MakeReadOnly())
    .AddConstructor(ctor => ctor
        .MakePublic()
        .AddParameter("ILogger<UserService>", "logger")
        .WithBody("_logger = logger ?? throw new ArgumentNullException(nameof(logger));"))
    .AddProperty("IsInitialized", "bool", prop => prop
        .MakePublic()
        .WithGetter("return _logger != null;"))
    .AddMethod("GetUserAsync", method => method
        .MakePublic()
        .MakeAsync()
        .WithReturnType("Task<User>")
        .AddParameter("int", "userId")
        .WithSummary("Gets a user by ID")
        .WithBody(@"
            _logger.LogInformation(""Getting user {UserId}"", userId);
            // Implementation here
            return await Task.FromResult(new User());"))
    .Build();
```

### Building an Interface

```csharp
var interfaceCode = new InterfaceBuilder("IRepository")
    .MakePublic()
    .AddGenericParameter("T", genericParam => genericParam
        .AddConstraint("class")
        .AddConstraint("new()"))
    .WithSummary("Generic repository interface")
    .AddMethod("GetByIdAsync", method => method
        .WithReturnType("Task<T>")
        .AddParameter("int", "id")
        .WithSummary("Gets an entity by ID"))
    .AddMethod("AddAsync", method => method
        .WithReturnType("Task")
        .AddParameter("T", "entity"))
    .Build();
```

### Building a Record

```csharp
var recordCode = new RecordBuilder("PersonDto")
    .MakePublic()
    .WithSummary("Data transfer object for Person")
    .AddParameter("string", "FirstName")
    .AddParameter("string", "LastName")
    .AddParameter("int", "Age", param => param
        .WithDefaultValue("0"))
    .AddProperty("FullName", "string", prop => prop
        .MakePublic()
        .WithGetter("return $\"{FirstName} {LastName}\";"))
    .Build();
```

### Building an Enum

```csharp
var enumCode = new EnumBuilder("UserRole")
    .MakePublic()
    .WithSummary("Defines user roles in the system")
    .AddValue("Guest", value => value
        .WithValue(0)
        .WithSummary("Guest user with limited access"))
    .AddValue("User", value => value
        .WithValue(1))
    .AddValue("Admin", value => value
        .WithValue(100)
        .WithSummary("Administrator with full access"))
    .Build();
```

### Building with Namespace

```csharp
var namespaceCode = new NamespaceBuilder("MyApp.Services")
    .AddUsing("System")
    .AddUsing("System.Threading.Tasks")
    .AddUsing("Microsoft.Extensions.Logging")
    .AddMember(new ClassBuilder("MyService")
        .MakePublic()
        .Build())
    .Build();
```

## Builder Types

### Type Builders
- `ClassBuilder` - Build classes with full member support
- `InterfaceBuilder` - Build interfaces with methods and properties
- `RecordBuilder` - Build C# 9+ records
- `EnumBuilder` - Build enumerations
- `StructBuilder` - Build value types

### Member Builders
- `MethodBuilder` - Build methods with parameters and body
- `PropertyBuilder` - Build properties with getters/setters
- `FieldBuilder` - Build fields with initializers
- `ConstructorBuilder` - Build constructors
- `EventBuilder` - Build events

### Code Organization
- `NamespaceBuilder` - Organize code in namespaces
- `DirectiveBuilder` - Add using directives and preprocessor directives
- `AttributeBuilder` - Build attributes with arguments
- `CodeBlockBuilder` - Build arbitrary code blocks

## Advanced Features

### Generic Support

```csharp
var genericClass = new ClassBuilder("Repository")
    .AddGenericParameter("T", param => param
        .AddConstraint("class")
        .AddConstraint("IEntity")
        .AddConstraint("new()"))
    .MakePublic()
    .Build();
```

### Attribute Support

```csharp
var method = new MethodBuilder("Configure")
    .AddAttribute("Obsolete", attr => attr
        .AddArgument("\"Use ConfigureAsync instead\"")
        .AddArgument("true"))
    .MakePublic()
    .Build();
```

### XML Documentation

```csharp
var property = new PropertyBuilder("Id", "int")
    .WithSummary("Gets or sets the unique identifier")
    .WithRemarks("This value is auto-generated by the database")
    .AddXmlDocLine("<value>The unique identifier</value>")
    .Build();
```

### Expression Bodies

```csharp
var method = new MethodBuilder("GetFullName")
    .WithReturnType("string")
    .WithExpressionBody("$\"{FirstName} {LastName}\"")
    .Build();
```

## Formatting

All builders automatically handle:
- Proper indentation (customizable)
- Consistent spacing
- Brace placement
- Line breaks
- Comment formatting

```csharp
// Customize indentation
var builder = new ClassBuilder("MyClass")
    .WithIndentSize(2)  // Use 2 spaces
    .WithIndentLevel(1); // Start at indent level 1
```

## Best Practices

1. **Use Fluent Chaining** - Take advantage of the fluent API for readable code
2. **Add Documentation** - Use WithSummary() and WithRemarks() for XML docs
3. **Validate Input** - Builders validate method/property names
4. **Reuse Builders** - Create builder factories for common patterns
5. **Test Generated Code** - Always compile and test generated output

## License

Apache License 2.0