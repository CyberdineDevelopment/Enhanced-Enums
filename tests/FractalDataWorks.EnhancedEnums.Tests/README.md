# FractalDataWorks.EnhancedEnums Tests

## Overview

This test suite validates the Enhanced Enums source generator functionality using the SmartGenerators testing framework.

## Test Categories

### Core Functionality
- **EnhancedEnumGeneratorTests.cs** - Basic generation scenarios
- **EnhancedEnumLookupTests.cs** - Lookup method generation
- **EnhancedEnumMultipleCollectionsTests.cs** - Multiple collection support
- **EnhancedEnumReturnTypeTests.cs** - Custom return type handling
- **GenericEnhancedEnumTests.cs** - Generic base type support

### Advanced Scenarios
- **EnhancedEnumIncrementalTests.cs** - Incremental generation optimization
- **EnhancedEnumErrorScenarioTests.cs** - Error handling and edge cases

### Services Tests
- **EnumAttributeParserTests.cs** - Attribute parsing logic
- **EnumValueCollectorTests.cs** - Enum value discovery
- **ReturnTypeResolverTests.cs** - Return type resolution
- **TypeDiscoveryServiceTests.cs** - Type discovery logic

## Running Tests

```bash
# Run all tests
dotnet test

# Run specific test category
dotnet test --filter "Category=Core"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Test Infrastructure

### Base Class
`EnhancedEnumTestBase` provides:
- Compilation helpers
- Generator execution utilities  
- Diagnostic validation
- Generated code verification

### Test Patterns

#### Basic Generator Test
```csharp
[Fact]
public void GeneratesCollectionClass()
{
    // Arrange
    var source = @"
        [EnumCollection]
        public abstract class Status : IEnhancedEnumOption
        {
            public abstract int Id { get; }
            public abstract string Name { get; }
        }
        
        [EnumOption]
        public class Active : Status
        {
            public override int Id => 1;
            public override string Name => ""Active"";
        }";
    
    // Act
    var result = GenerateCode(source);
    
    // Assert
    result.Diagnostics.ShouldBeEmpty();
    result.GeneratedCode.ShouldContain("public static class Statuses");
    result.GeneratedCode.ShouldContain("public static Status Active()");
}
```

## Known Issues

- Some tests may fail due to attribute namespace changes from FractalDataWorks package updates

## Contributing

When adding new tests:
1. Inherit from `EnhancedEnumTestBase`
2. Use descriptive test names
3. Test both success and failure cases
4. Include XML documentation for complex scenarios