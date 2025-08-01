using FractalDataWorks.SmartGenerators.CodeBuilders;
using FractalDataWorks.SmartGenerators.TestUtilities;
using Xunit;

namespace FractalDataWorks.SmartGenerators.CodeBuilder.Tests.IntegrationTests;

/// <summary>
/// Integration tests for the ExpectationsFactory API to ensure all features work together correctly.
/// </summary>
public class ExpectationsApiIntegrationTests
{
    [Fact]
    public void ExpectationsApiCompleteClassValidationValidatesAllFeatures()
    {
        // Generate complex class with all features
        var code = GenerateComplexClass();

        ExpectationsFactory.ExpectCode(code)
            .HasNamespace("TestNamespace")
            .HasUsing("System")
            .HasUsing("System.Collections.Generic")
            .HasClass("ComplexClass", c => c
                .IsPublic()
                .IsPartial()
                .HasConstructor(ctor => ctor
                    .IsPublic()
                    .HasParameter("name", p => p.HasType("string")))
                .HasProperty("Name", p => p
                    .IsPublic()
                    .HasType("string")
                    .HasGetter()
                    .HasPrivateSetter())
                .HasProperty("Id", p => p
                    .IsProtected()
                    .HasType("int")
                    .IsReadOnly())
                .HasMethod("ProcessAsync", m => m
                    .IsPublic()
                    .Is()
                    .HasReturnType("Task<bool>")
                    .HasParameter("data", p => p.HasType("string"))))
            .HasInterface("IProcessor", i => i
                .IsPublic()
                .HasMethod("Process", m => m.HasReturnType("void")))
            .HasEnum("Status", e => e
                .IsPublic()
                .HasValue("Active", 1)
                .HasValue("Inactive", 0))
            .HasRecord("DataRecord", r => r
                .IsPublic()
                .HasParameter("Id", p => p.HasType("int"))
                .HasParameter("Name", p => p.HasType("string")))
            .Assert();
    }

    [Fact]
    public void ExpectationsApiConstructorValidationValidatesAllConstructorFeatures()
    {
        {
            // Arrange
            var builder = new NamespaceBuilder("TestNamespace")
                .AddUsing("System")
                .AddClass(c => c
                    .WithName("TestClass")
                    .MakePublic()
                    .AddConstructor(ctor => ctor
                        .MakePublic()
                        .AddParameter("int", "id")
                        .AddParameter("string", "name")
                        .AddParameter("bool", "isActive", "true")
                        .WithBody(b => b
                            .AppendLine("_id = id;")
                            .AppendLine("_name = name;")
                            .AppendLine("_isActive = isActive;")))
                    .AddConstructor(ctor => ctor
                        .MakeProtected()
                        .WithBody(b => b.AppendLine("// Protected constructor")))
                    .AddConstructor(ctor => ctor
                        .MakePrivate()
                        .AddParameter("string", "value")
                        .WithBody(b => b.AppendLine("_name = value;"))));

            // Act
            var code = builder.Build();

            // Assert - Use parameter types to identify specific constructors
            ExpectationsFactory.ExpectCode(code)
                .HasClass("TestClass", c => c
                    // Test the public constructor with 3 parameters
                    .HasConstructor("int, string, bool", ctor => ctor
                        .IsPublic()
                        .HasParameter("id", p => p.HasType("int"))
                        .HasParameter("name", p => p.HasType("string"))
                        .HasParameter("isActive", p => p
                            .HasType("bool")
                            .HasDefaultValue("true")))
                    // Test the protected parameterless constructor
                    .HasConstructor("", ctor => ctor.IsProtected())
                    // Test the private constructor with 1 parameter
                    .HasConstructor("string", ctor => ctor
                        .IsPrivate()
                        .HasParameter("value", p => p.HasType("string"))))
                .Assert();
        }
    }

    [Fact]
    public void ExpectationsApiPropertyAccessModifiersValidatesAllVariations()
    {
        // Arrange
        var code = GenerateClassWithVariousProperties();

        // Assert
        ExpectationsFactory.ExpectCode(code)
            .HasNamespace("TestNamespace")
            .HasClass("PropertyTest", c => c
                .HasProperty("PublicProperty", p => p
                    .IsPublic()
                    .HasType("string")
                    .HasGetter()
                    .HasSetter())
                .HasProperty("PrivateProperty", p => p
                    .IsPrivate()
                    .HasType("int")
                    .HasGetter()
                    .HasSetter())
                .HasProperty("ProtectedProperty", p => p
                    .IsProtected()
                    .HasType("bool")
                    .HasGetter()
                    .HasSetter())
                .HasProperty("InternalProperty", p => p
                    .IsInternal()
                    .HasType("decimal")
                    .HasGetter()
                    .HasSetter())
                .HasProperty("ProtectedInternalProperty", p => p
                    .IsProtectedInternal()
                    .HasType("double")
                    .HasGetter()
                    .HasSetter())
                .HasProperty("PrivateProtectedProperty", p => p
                    .IsPrivateProtected()
                    .HasType("float")
                    .HasGetter()
                    .HasSetter())
                .HasProperty("ReadOnlyProperty", p => p
                    .IsPublic()
                    .HasType("DateTime")
                    .IsReadOnly())
                .HasProperty("InitOnlyProperty", p => p
                    .IsPublic()
                    .HasType("Guid")
                    .HasGetter()
                    .HasInitSetter())
                .HasProperty("StaticProperty", p => p
                    .IsPublic()
                    .IsStatic()
                    .HasType("string"))
                .HasProperty("OverrideProperty", p => p
                    .IsPublic()
                    .IsOverride()
                    .HasType("object")))
            .Assert();
    }

    [Fact]
    public void ExpectationsApiAsyncMethodValidationValidatesAsyncMethods()
    {
        // Arrange
        var code = GenerateClassWithAsyncMethods();

        // Assert
        ExpectationsFactory.ExpectCode(code)
            .HasNamespace("TestNamespace")
            .HasClass("AsyncProcessor", c => c
                .HasMethod("ProcessAsync", m => m
                    .IsPublic()
                    .Is()
                    .HasReturnType("Task")
                    .HasNoParameters())
                .HasMethod("ProcessWithResultAsync", m => m
                    .IsPublic()
                    .Is()
                    .HasReturnType("Task<int>")
                    .HasParameter("input", p => p.HasType("string")))
                .HasMethod("ProcessWithCancellationAsync", m => m
                    .IsPublic()
                    .Is()
                    .HasReturnType("Task<bool>")
                    .HasParameter("data", p => p.HasType("byte[]"))
                    .HasParameter("cancellationToken", p => p
                        .HasType("CancellationToken")
                        .HasDefaultValue("default")))
                .HasMethod("ProcessValueTaskAsync", m => m
                    .IsProtected()
                    .Is()
                    .HasReturnType("ValueTask<string>")
                    .HasParameter("id", p => p.HasType("int"))))
            .Assert();
    }

    [Fact]
    public void ExpectationsApiInterfaceGenerationValidatesInterfaces()
    {
        // Arrange
        var code = GenerateInterfaceHierarchy();

        // Assert
        ExpectationsFactory.ExpectCode(code)
            .HasNamespace("TestNamespace")
            .HasInterface("IBase", i => i
                .IsPublic()
                .HasMethod("GetId", m => m.HasReturnType("int"))
                .HasProperty("Name", p => p.HasType("string")))
            .HasInterface("IDerived", i => i
                .IsPublic()
                .HasBaseInterface("IBase")
                .HasMethod("Process", m => m
                    .HasReturnType("void")
                    .HasParameter("data", p => p.HasType("string")))
                .HasProperty("IsActive", p => p
                    .HasType("bool")
                    .HasGetter()
                    .HasSetter()))
            .HasInterface("IGeneric", i => i
                .IsPublic()
                .HasTypeParameter("T")
                .HasTypeParameter("TResult")
                .HasMethod("Transform", m => m
                    .HasReturnType("TResult")
                    .HasParameter("input", p => p.HasType("T"))))
            .Assert();
    }

    [Fact]
    public void ExpectationsApiEnumGenerationValidatesEnumsWithValuesAndBaseTypes()
    {
        // Arrange
        var code = GenerateEnumsWithVariousFeatures();

        // Assert
        ExpectationsFactory.ExpectCode(code)
            .HasNamespace("TestNamespace")
            .HasEnum("SimpleEnum", e => e
                .IsPublic()
                .HasValue("None", 0)
                .HasValue("Active", 1)
                .HasValue("Inactive", 2))
            .HasEnum("FlagsEnum", e => e
                .IsPublic()
                .HasAttribute("Flags")
                .HasValue("None", 0)
                .HasValue("Read", 1)
                .HasValue("Write", 2)
                .HasValue("Execute", 4)
                .HasValue("All", 7))
            .HasEnum("ByteEnum", e => e
                .IsInternal()
                .HasBaseType("byte")
                .HasValue("Min", 0)
                .HasValue("Max", 255))
            .HasEnum("LongEnum", e => e
                .IsPrivate()
                .HasBaseType("long")
                .HasValue("Negative", -1)
                .HasValue("Zero", 0)
                .HasValue("Positive", 1))
            .Assert();
    }

    [Fact]
    public void ExpectationsApiRecordGenerationValidatesRecordsWithPrimaryConstructor()
    {
        // Arrange
        var code = GenerateRecordsWithVariousFeatures();

        // Assert
        ExpectationsFactory.ExpectCode(code)
            .HasNamespace("TestNamespace")
            .HasRecord("SimpleRecord", r => r
                .IsPublic()
                .HasParameter("Id", p => p.HasType("int"))
                .HasParameter("Name", p => p.HasType("string")))
            .HasRecord("ComplexRecord", r => r
                .IsPublic()
                .HasParameter("Id", p => p.HasType("Guid"))
                .HasParameter("CreatedAt", p => p.HasType("DateTime"))
                .HasParameter("IsActive", p => p
                    .HasType("bool")
                    .HasDefaultValue("true"))
                .HasMethod("Validate", m => m
                    .IsPublic()
                    .HasReturnType("bool"))
                .HasProperty("UpdatedAt", p => p
                    .HasType("DateTime?")
                    .HasGetter()
                    .HasInitSetter()))
            .HasRecord("DerivedRecord", r => r
                .IsPublic()
                .HasBaseType("ComplexRecord")
                .HasParameter("AdditionalData", p => p.HasType("string"))
                .HasMethod("ToString", m => m
                    .IsPublic()
                    .IsOverride()
                    .HasReturnType("string")))
            .Assert();
    }

    [Fact]
    public void ExpectationsApiComplexNestedScenariosValidatesDeepHierarchies()
    {
        // Arrange
        var code = GenerateComplexNestedStructure();

        // Assert
        ExpectationsFactory.ExpectCode(code)
            .HasNamespace("TestNamespace")
            .HasClass("OuterClass", c => c
                .IsPublic()
                .IsPartial()
                .HasNestedClass("InnerClass", nc => nc
                    .IsPrivate()
                    .HasProperty("Value", p => p
                        .HasType("string")
                        .IsPublic())
                    .HasNestedEnum("InnerEnum", e => e
                        .IsPublic()
                        .HasValue("Option1", 1)
                        .HasValue("Option2", 2)))
                .HasNestedInterface("IInnerInterface", i => i
                    .IsProtected()
                    .HasMethod("Execute", m => m
                        .HasReturnType("void")))
                .HasNestedRecord("InnerRecord", r => r
                    .IsPublic()
                    .HasParameter("Data", p => p.HasType("string")))
                .HasMethod("CreateInner", m => m
                    .IsPublic()
                    .HasReturnType("InnerClass")))
            .Assert();
    }

    // Helper methods to generate test code

    private static string GenerateComplexClass()
    {
        var builder = new NamespaceBuilder("TestNamespace")
            .AddUsing("System")
            .AddUsing("System.Collections.Generic")
            .AddUsing("System.Threading.Tasks")
            .AddInterface(i => i
                .WithName("IProcessor")
                .MakePublic()
                .AddMethod("Process", "void"))
            .AddEnum(e => e
                .WithName("Status")
                .MakePublic()
                .AddValue("Inactive", 0)
                .AddValue("Active", 1))
            .AddRecord(r => r
                .WithName("DataRecord")
                .MakePublic()
                .WithParameter("int", "Id")
                .WithParameter("string", "Name"))
            .AddClass(c => c
                .WithName("ComplexClass")
                .MakePublic()
                .MakePartial()
                .ImplementsInterface("IProcessor")
                .AddField("int", "_id", f => f.MakePrivate())
                .AddField("string", "_name", f => f.MakePrivate())
                .AddConstructor(ctor => ctor
                    .MakePublic()
                    .AddParameter("string", "name")
                    .WithBody(b => b.AppendLine("_name = name;")))
                .AddProperty("Name", "string", p => p
                    .MakePublic()
                    .WithGetter("_name")
                    .WithSetter("_name")
                    .MakeSetterPrivate())
                .AddProperty("Id", "int", p => p
                    .MakeProtected()
                    .WithGetter("_id")
                    .MakeReadOnly())
                .AddMethod("ProcessAsync", "Task<bool>", m => m
                    .MakePublic()
                    .MakeAsync()
                    .AddParameter("string", "data")
                    .WithBody(b => b
                        .AppendLine("await Task.Delay(100);")
                        .AppendLine("return !string.IsNullOrEmpty(data);")))
                .AddMethod("Process", "void", m => m
                    .MakePublic()
                    .WithBody(b => b.AppendLine("// Implementation"))));

        return builder.Build();
    }

    private static string GenerateClassWithVariousProperties()
    {
        var builder = new NamespaceBuilder("TestNamespace")
            .AddUsing("System")
            .AddClass(c => c
                .WithName("PropertyTest")
                .MakePublic()
                .AddProperty("PublicProperty", "string", p => p.MakePublic())
                .AddProperty("PrivateProperty", "int", p => p.MakePrivate())
                .AddProperty("ProtectedProperty", "bool", p => p.MakeProtected())
                .AddProperty("InternalProperty", "decimal", p => p.MakeInternal())
                .AddProperty("ProtectedInternalProperty", "double", p => p.MakeProtectedInternal())
                .AddProperty("PrivateProtectedProperty", "float", p => p.MakePrivateProtected())
                .AddProperty("ReadOnlyProperty", "DateTime", p => p.MakePublic().MakeReadOnly())
                .AddProperty("InitOnlyProperty", "Guid", p => p.MakePublic().WithInitSetter())
                .AddProperty("StaticProperty", "string", p => p.MakePublic().MakeStatic())
                .AddProperty("OverrideProperty", "object", p => p.MakePublic().MakeOverride()));

        return builder.Build();
    }

    private static string GenerateClassWithAsyncMethods()
    {
        var builder = new NamespaceBuilder("TestNamespace")
            .AddUsing("System")
            .AddUsing("System.Threading")
            .AddUsing("System.Threading.Tasks")
            .AddClass(c => c
                .WithName("AsyncProcessor")
                .MakePublic()
                .AddMethod("ProcessAsync", "Task", m => m
                    .MakePublic()
                    .MakeAsync()
                    .WithBody(b => b.AppendLine("await Task.Delay(100);")))
                .AddMethod("ProcessWithResultAsync", "Task<int>", m => m
                    .MakePublic()
                    .MakeAsync()
                    .AddParameter("string", "input")
                    .WithBody(b => b
                        .AppendLine("await Task.Delay(100);")
                        .AppendLine("return input?.Length ?? 0;")))
                .AddMethod("ProcessWithCancellationAsync", "Task<bool>", m => m
                    .MakePublic()
                    .MakeAsync()
                    .AddParameter("byte[]", "data")
                    .AddParameter("CancellationToken", "cancellationToken", "default")
                    .WithBody(b => b
                        .AppendLine("await Task.Delay(100, cancellationToken);")
                        .AppendLine("return data?.Length > 0;")))
                .AddMethod("ProcessValueTaskAsync", "ValueTask<string>", m => m
                    .MakeProtected()
                    .MakeAsync()
                    .AddParameter("int", "id")
                    .WithBody(b => b
                        .AppendLine("await Task.Delay(50);")
                        .AppendLine("return $\"Processed: {id}\";"))));

        return builder.Build();
    }

    private static string GenerateInterfaceHierarchy()
    {
        var builder = new NamespaceBuilder("TestNamespace")
            .AddInterface(i => i
                .WithName("IBase")
                .MakePublic()
                .AddMethodWithNoImplementation("GetId", "int")
                .AddProperty("Name", "string"))
            .AddInterface(i => i
                .WithName("IDerived")
                .MakePublic()
                .WithBaseInterface("IBase")
                .AddMethod("Process", "void", m => m
                    .AddParameter("string", "data"))
                .AddProperty("IsActive", "bool", p => p.WithSetter("")))
            .AddInterface(i => i
                .WithName("IGeneric")
                .MakePublic()
                .WithTypeParameter("T")
                .WithTypeParameter("TResult")
                .AddMethod("Transform", "TResult", m => m
                    .AddParameter("T", "input")));

        return builder.Build();
    }

    private static string GenerateEnumsWithVariousFeatures()
    {
        var builder = new NamespaceBuilder("TestNamespace")
            .AddUsing("System")
            .AddEnum(e => e
                .WithName("SimpleEnum")
                .MakePublic()
                .AddValue("None", 0)
                .AddValue("Active", 1)
                .AddValue("Inactive", 2))
            .AddEnum(e => e
                .WithName("FlagsEnum")
                .MakePublic()
                .AddAttribute("Flags")
                .AddValue("None", 0)
                .AddValue("Read", 1)
                .AddValue("Write", 2)
                .AddValue("Execute", 4)
                .AddValue("All", 7))
            .AddEnum(e => e
                .WithName("ByteEnum")
                .MakeInternal()
                .WithBaseType("byte")
                .AddValue("Min", 0)
                .AddValue("Max", 255))
            .AddEnum(e => e
                .WithName("LongEnum")
                .MakePrivate()
                .WithBaseType("long")
                .AddValue("Negative", -1)
                .AddValue("Zero", 0)
                .AddValue("Positive", 1));

        return builder.Build();
    }

    private static string GenerateRecordsWithVariousFeatures()
    {
        var builder = new NamespaceBuilder("TestNamespace")
            .AddUsing("System")
            .AddRecord(r => r
                .WithName("SimpleRecord")
                .MakePublic()
                .WithParameter("int", "Id")
                .WithParameter("string", "Name"))
            .AddRecord(r => r
                .WithName("ComplexRecord")
                .MakePublic()
                .WithParameter("Guid", "Id")
                .WithParameter("DateTime", "CreatedAt")
                .WithParameter("bool", "IsActive", p => p.WithDefaultValue("true"))
                .AddMethod("Validate", "bool", m => m
                    .MakePublic()
                    .WithBody(b => b.AppendLine("return Id != Guid.Empty;")))
                .AddProperty("UpdatedAt", "DateTime?", p => p
                    .WithInitSetter()))
            .AddRecord(r => r
                .WithName("DerivedRecord")
                .MakePublic()
                .WithBaseType("ComplexRecord")
                .WithParameter("string", "AdditionalData")
                .AddMethod("ToString", "string", m => m
                    .MakePublic()
                    .MakeOverride()
                    .WithBody(b => b.AppendLine("return $\"{base.ToString()} - {AdditionalData}\";"))));
        ;

        return builder.Build();
    }

    private static string GenerateComplexNestedStructure()
    {
        var builder = new NamespaceBuilder("TestNamespace")
            .AddClass(c => c
                .WithName("OuterClass")
                .MakePublic()
                .MakePartial()
                .AddNestedClass(nc => nc
                    .WithName("InnerClass")
                    .MakePrivate()
                    .AddProperty("Value", "string", p => p.MakePublic())
                    .AddNestedEnum(e => e
                        .WithName("InnerEnum")
                        .MakePublic()
                        .AddValue("Option1", 1)
                        .AddValue("Option2", 2)))
                .AddNestedInterface(i => i
                    .WithName("IInnerInterface")
                    .MakeProtected()
                    .AddMethod("Execute", "void"))
                .AddNestedRecord(r => r
                    .WithName("InnerRecord")
                    .MakePublic()
                    .WithParameter("string", "Data"))
                .AddMethod("CreateInner", "InnerClass", m => m
                    .MakePublic()
                    .WithBody(b => b.AppendLine("return new InnerClass();"))));

        return builder.Build();
    }
}
