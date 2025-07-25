using System.Linq;
using FractalDataWorks.EnhancedEnums.Generators;
using FractalDataWorks.SmartGenerators.TestUtilities;
using Shouldly;
using Xunit;
using ExpectationsFactory = FractalDataWorks.SmartGenerators.TestUtilities.ExpectationsFactory;

namespace FractalDataWorks.EnhancedEnums.Tests;

public class InterfaceBasedDiscoveryTests : EnhancedEnumOptionTestBase
{
    [Fact]
    public void GeneratorAlt_WithInterfaceBasedDiscovery_GeneratesCollection()
    {
        // Arrange
        var source = @"
            using FractalDataWorks.EnhancedEnums;
            using FractalDataWorks.EnhancedEnums.Attributes;
            
            namespace TestNamespace
            {
                // Base class
                public abstract class ServiceType
                {
                    public abstract string Name { get; }
                    public abstract int Id { get; }
                }
                
                // Implementation 1
                [EnumOption(Name = ""Service1"")]
                public class Service1Type : ServiceType, IEnhancedEnumOptionAlt<ServiceType>
                {
                    public override string Name => ""Service1"";
                    public override int Id => 1;
                }
                
                // Implementation 2
                [EnumOption(Name = ""Service2"")]
                public class Service2Type : ServiceType, IEnhancedEnumOptionAlt<ServiceType>
                {
                    public override string Name => ""Service2"";
                    public override int Id => 2;
                }
            }";
        
        // Act
        var generator = new EnhancedEnumGeneratorAlt();
        var output = SourceGeneratorTestHelper.RunGenerator(generator, [source], out var diagnostics, GetDefaultReferences());
        
        // Assert
        diagnostics.ShouldBeEmpty();
        
        // Should generate the diagnostic file
        output.ContainsKey("EnhancedEnumGeneratorAlt.Diagnostic.g.cs").ShouldBeTrue();
        
        // Should generate the collection
        output.ContainsKey("ServiceTypeCollection.g.cs").ShouldBeTrue();
        
        var generatedCode = output["ServiceTypeCollection.g.cs"];
        
        // Debug output
        WriteGeneratedCodeToFile("ServiceTypeCollection.g.cs", generatedCode);
        
        // Use ExpectationsFactory to verify the generated code structure
        ExpectationsFactory.ExpectCode(generatedCode)
            .HasNamespace("TestNamespace", ns => ns
                .HasClass("ServiceTypeCollection", cls => cls
                    .IsPublic()
                    .IsStatic()
                    .HasProperty("All", prop => prop
                        .IsPublic()
                        .IsStatic()
                        .HasType("ImmutableArray<TestNamespace.ServiceType>"))
                    .HasMethod("GetByName", method => method
                        .IsPublic()
                        .IsStatic()
                        .HasReturnType("TestNamespace.ServiceType?")
                        .HasParameter("name", param => param.HasType("string")))
                    .HasProperty("Service1", prop => prop
                        .IsPublic()
                        .IsStatic()
                        .HasType("TestNamespace.ServiceType"))
                    .HasProperty("Service2", prop => prop
                        .IsPublic()
                        .IsStatic()
                        .HasType("TestNamespace.ServiceType"))))
            .Assert();
    }
    
    [Fact]
    public void GeneratorAlt_WithInterfaceToInterface_GeneratesCollection()
    {
        // Arrange
        var source = @"
            using FractalDataWorks.EnhancedEnums;
            using FractalDataWorks.EnhancedEnums.Attributes;
            
            namespace TestNamespace
            {
                // Base interface
                public interface IService
                {
                    string Name { get; }
                }
                
                // Implementation 1
                [EnumOption(Name = ""EmailService"")]
                public class EmailService : IService, IEnhancedEnumOptionAlt<IService>
                {
                    public string Name => ""Email"";
                }
                
                // Implementation 2
                [EnumOption(Name = ""SmsService"")]
                public class SmsService : IService, IEnhancedEnumOptionAlt<IService>
                {
                    public string Name => ""SMS"";
                }
            }";
        
        // Act
        var generator = new EnhancedEnumGeneratorAlt();
        var output = SourceGeneratorTestHelper.RunGenerator(generator, [source], out var diagnostics, GetDefaultReferences());
        
        // Assert
        diagnostics.ShouldBeEmpty();
        output.ContainsKey("IServiceCollection.g.cs").ShouldBeTrue();
        
        var generatedCode = output["IServiceCollection.g.cs"];
        
        // Use ExpectationsFactory to verify the generated code structure
        ExpectationsFactory.ExpectCode(generatedCode)
            .HasNamespace("TestNamespace", ns => ns
                .HasClass("IServiceCollection", cls => cls
                    .IsPublic()
                    .IsStatic()
                    .HasProperty("All", prop => prop
                        .IsPublic()
                        .IsStatic()
                        .HasType("ImmutableArray<TestNamespace.IService>"))
                    .HasProperty("EmailService", prop => prop
                        .IsPublic()
                        .IsStatic()
                        .HasType("TestNamespace.IService"))
                    .HasProperty("SmsService", prop => prop
                        .IsPublic()
                        .IsStatic()
                        .HasType("TestNamespace.IService"))))
            .Assert();
    }
    
    [Fact]
    public void GeneratorAlt_WithCrossAssemblyDiscovery_FindsTypes()
    {
        // Arrange
        var interfaceSource = @"
            // No need to define IEnhancedEnumOptionAlt - it's already in the references
            namespace TestNamespace { }";
            
        var baseSource = @"
            using FractalDataWorks.EnhancedEnums.Attributes;
            
            namespace TestNamespace
            {
                [EnhancedEnumBase(""PaymentMethods"")]
                public abstract class PaymentMethod
                {
                    public abstract string Name { get; }
                    [EnumLookup]
                    public abstract string Code { get; }
                }
            }";
            
        var implementationSource = @"
            using FractalDataWorks.EnhancedEnums;
            using FractalDataWorks.EnhancedEnums.Attributes;
            using TestNamespace;
            
            namespace ImplementationNamespace
            {
                [EnumOption(Name = ""CreditCard"")]
                public class CreditCardPayment : PaymentMethod, IEnhancedEnumOptionAlt<PaymentMethod>
                {
                    public override string Name => ""Credit Card"";
                    public override string Code => ""CC"";
                }
                
                [EnumOption(Name = ""BankTransfer"")]
                public class BankTransferPayment : PaymentMethod, IEnhancedEnumOptionAlt<PaymentMethod>
                {
                    public override string Name => ""Bank Transfer"";
                    public override string Code => ""BT"";
                }
            }";
        
        // Act
        var generator = new EnhancedEnumGeneratorAlt();
        var output = SourceGeneratorTestHelper.RunGenerator(generator, [interfaceSource, baseSource, implementationSource], out var diagnostics, GetDefaultReferences());
        
        // Assert
        diagnostics.ShouldBeEmpty();
        output.ContainsKey("PaymentMethods.g.cs").ShouldBeTrue();
        
        var generatedCode = output["PaymentMethods.g.cs"];
        
        // Use ExpectationsFactory to verify the generated code structure
        ExpectationsFactory.ExpectCode(generatedCode)
            .HasNamespace("TestNamespace", ns => ns
                .HasClass("PaymentMethods", cls => cls
                    .IsPublic()
                    .IsStatic()
                    .HasMethod("GetByCode", method => method
                        .IsPublic()
                        .IsStatic()
                        .HasReturnType("TestNamespace.PaymentMethod?")
                        .HasParameter("code", param => param.HasType("string")))
                    .HasProperty("CreditCard", prop => prop
                        .IsPublic()
                        .IsStatic()
                        .HasType("TestNamespace.PaymentMethod"))
                    .HasProperty("BankTransfer", prop => prop
                        .IsPublic()
                        .IsStatic()
                        .HasType("TestNamespace.PaymentMethod"))))
            .Assert();
    }
}