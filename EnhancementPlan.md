# Enhanced Enums Enhancement Plan

## Overview
Transform Enhanced Enums to use constructor-based initialization with auto properties, avoiding abstract properties entirely. Use SmartGenerators CodeBuilders properly for clean code generation.

## Detailed Implementation Plan

### Phase 1: Core Infrastructure

#### 1.1 Create EnhancedEnumBase<T> Base Class
- [x] Location: `src/FractalDataWorks.EnhancedEnums/EnhancedEnumBase.cs`
- [x] Protected constructor taking `(int id, string name)`
- [x] Auto properties for Id and Name (no abstract properties!)
- [ ] Implement IEnhancedEnumOption interface properly
- [ ] Add XML documentation explaining the pattern
- [ ] Consider adding equality members (Equals, GetHashCode, operators)

#### 1.2 Update EnumOption Attribute
- [ ] Location: `src/FractalDataWorks.EnhancedEnums/Attributes/EnumOptionAttribute.cs`
- [ ] Add properties:
  ```csharp
  public string? ReturnType { get; set; }
  public string? ReturnTypeNamespace { get; set; }
  ```
- [ ] Keep existing properties (Name, Order, CollectionName)
- [ ] Update XML documentation

#### 1.3 Create Constructor Tracking Model
- [ ] Create new file: `src/FractalDataWorks.EnhancedEnums/Models/ConstructorInfo.cs`
  ```csharp
  public sealed class ConstructorInfo : IEquatable<ConstructorInfo>
  {
      public List<ParameterInfo> Parameters { get; set; } = new();
      public Accessibility Accessibility { get; set; }
      public bool IsPrimary { get; set; } // For C# 12 primary constructors
      
      // Implement IEquatable, GetHashCode, etc.
  }
  
  public sealed class ParameterInfo : IEquatable<ParameterInfo>
  {
      public string TypeName { get; set; } = string.Empty;
      public string Name { get; set; } = string.Empty;
      public string? DefaultValue { get; set; }
      public bool HasDefaultValue { get; set; }
      public string? Namespace { get; set; } // For type imports
  }
  ```

- [ ] Update `EnumTypeInfo` model:
  - Add `List<ConstructorInfo> Constructors` property
  - Update `WriteToHash` to include constructor data
  - Update `Equals` implementation

### Phase 2: Generator Core Updates

#### 2.1 Constructor Detection Implementation
- [ ] Location: `src/FractalDataWorks.EnhancedEnums/Generators/EnhancedEnumGenerator.cs`
- [ ] Add method `ExtractConstructors(INamedTypeSymbol typeSymbol)`:
  ```csharp
  private static List<ConstructorInfo> ExtractConstructors(INamedTypeSymbol typeSymbol)
  {
      var constructors = new List<ConstructorInfo>();
      
      // Get all public constructors
      foreach (var ctor in typeSymbol.Constructors)
      {
          if (ctor.DeclaredAccessibility != Accessibility.Public)
              continue;
              
          var ctorInfo = new ConstructorInfo
          {
              Accessibility = ctor.DeclaredAccessibility,
              IsPrimary = IsPrimaryConstructor(ctor)
          };
          
          foreach (var param in ctor.Parameters)
          {
              ctorInfo.Parameters.Add(new ParameterInfo
              {
                  TypeName = param.Type.ToDisplayString(),
                  Name = param.Name,
                  HasDefaultValue = param.HasExplicitDefaultValue,
                  DefaultValue = param.HasExplicitDefaultValue 
                      ? GetDefaultValueString(param) : null,
                  Namespace = param.Type.ContainingNamespace?.ToDisplayString()
              });
          }
          
          constructors.Add(ctorInfo);
      }
      
      return constructors;
  }
  ```

#### 2.2 Namespace Extraction Enhancement
- [ ] Create dedicated namespace extraction service:
  ```csharp
  private static HashSet<string> ExtractRequiredNamespaces(
      EnumTypeInfo enumType, 
      List<EnumValueInfo> values)
  {
      var namespaces = new HashSet<string>(StringComparer.Ordinal);
      
      // From return types
      ExtractNamespaceFromType(enumType.ReturnType, namespaces);
      
      // From constructor parameters
      foreach (var value in values)
      {
          foreach (var ctor in value.Constructors)
          {
              foreach (var param in ctor.Parameters)
              {
                  ExtractNamespaceFromType(param.TypeName, namespaces);
              }
          }
      }
      
      // From lookup properties
      foreach (var lookup in enumType.LookupProperties)
      {
          ExtractNamespaceFromType(lookup.PropertyType, namespaces);
      }
      
      // Remove system namespaces and current namespace
      namespaces.RemoveWhere(ns => 
          ns.StartsWith("System") || 
          ns == enumType.Namespace ||
          string.IsNullOrEmpty(ns));
          
      return namespaces;
  }
  ```

### Phase 3: Code Generation Refactoring

#### 3.1 Replace String Building with Builders
- [ ] Current location: `GenerateCollection` method
- [ ] Replace ALL StringBuilder usage with proper builders:

```csharp
protected virtual void GenerateCollection(
    SourceProductionContext context, 
    EnumTypeInfo def, 
    EquatableArray<EnumValueInfo> values, 
    Compilation compilation)
{
    // Step 1: Create namespace builder
    var namespaceBuilder = new NamespaceBuilder(def.Namespace)
        .AddUsing("System")
        .AddUsing("System.Collections.Generic")
        .AddUsing("System.Collections.Immutable")
        .AddUsing("System.Linq");
    
    // Add conditional usings
    namespaceBuilder.AddDirective("#if NET8_0_OR_GREATER")
        .AddUsing("System.Collections.Frozen")
        .AddDirective("#endif");
    
    // Add extracted namespaces
    foreach (var ns in ExtractRequiredNamespaces(def, values))
    {
        namespaceBuilder.AddUsing(ns);
    }
    
    // Step 2: Create class builder
    var classBuilder = new ClassBuilder(def.CollectionName)
        .MakePublic()
        .MakeStatic()
        .WithSummary($"Collection of all {def.ClassName} values.");
    
    // Step 3: Add fields using FieldBuilder
    AddCollectionFields(classBuilder, def, effectiveReturnType);
    
    // Step 4: Add static constructor using ConstructorBuilder
    AddStaticConstructor(classBuilder, def, values);
    
    // Step 5: Add properties using PropertyBuilder
    AddCollectionProperties(classBuilder, def, effectiveReturnType);
    
    // Step 6: Add factory methods using MethodBuilder
    AddFactoryMethods(classBuilder, def, values);
    
    // Step 7: Add lookup methods
    AddLookupMethods(classBuilder, def, effectiveReturnType);
    
    // Step 8: Add Empty value support
    AddEmptyValue(classBuilder, def, effectiveReturnType);
    
    // Build and emit
    namespaceBuilder.AddMember(classBuilder);
    var source = namespaceBuilder.Build();
    context.AddSource($"{def.CollectionName}.g.cs", source);
}
```

#### 3.2 Factory Method Generation
```csharp
private static void AddFactoryMethods(
    ClassBuilder classBuilder, 
    EnumTypeInfo def, 
    List<EnumValueInfo> values)
{
    foreach (var value in values)
    {
        var returnType = value.ReturnType ?? def.ReturnType ?? value.FullTypeName;
        
        if (value.Constructors.Count == 0)
        {
            // No public constructors - generate parameterless
            classBuilder.AddMethod(value.Name, returnType, method => method
                .MakePublic()
                .MakeStatic()
                .WithSummary($"Creates a new instance of {value.Name}.")
                .WithExpressionBody($"new {value.FullTypeName}()"));
        }
        else
        {
            // Generate method for each constructor
            foreach (var ctor in value.Constructors)
            {
                classBuilder.AddMethod(value.Name, returnType, method =>
                {
                    method.MakePublic().MakeStatic()
                        .WithSummary($"Creates a new instance of {value.Name}.");
                    
                    // Add parameters
                    foreach (var param in ctor.Parameters)
                    {
                        method.AddParameter(param.TypeName, param.Name, param.DefaultValue);
                        method.WithXmlDocParam(param.Name, $"The {param.Name} parameter.");
                    }
                    
                    // Build argument list
                    var args = string.Join(", ", ctor.Parameters.Select(p => p.Name));
                    method.WithExpressionBody($"new {value.FullTypeName}({args})");
                    
                    return method;
                });
            }
        }
    }
}
```

### Phase 4: Analyzer Development

#### 4.1 Create Enhanced Enum Analyzer
- [ ] Location: `src/FractalDataWorks.EnhancedEnums.Analyzers/EnhancedEnumAnalyzer.cs`
- [ ] Diagnostic IDs:
  - `ENH1001`: Abstract property in enhanced enum (Error)
  - `ENH1002`: Missing constructor parameter for property (Warning)
  - `ENH1003`: Property not initialized in constructor (Warning)
  - `ENH1004`: Using abstract instead of auto property (Info)

#### 4.2 Code Fix Provider
- [ ] Location: `src/FractalDataWorks.EnhancedEnums.CodeFixes/UseConstructorInitializationCodeFix.cs`
- [ ] Fixes:
  - Convert abstract property to auto property
  - Add constructor parameter
  - Initialize property in constructor

### Phase 5: Testing Strategy

#### 5.1 Unit Tests to Add/Update
- [ ] `ConstructorDetectionTests.cs`:
  - Test primary constructor detection
  - Test multiple constructor overloads
  - Test default parameter handling
  
- [ ] `FactoryMethodGenerationTests.cs`:
  - Test method generation for each constructor
  - Test return type overrides
  - Test parameter forwarding

- [ ] `NamespaceExtractionTests.cs`:
  - Test extraction from various sources
  - Test deduplication
  - Test system namespace filtering

#### 5.2 Integration Tests
- [ ] Full end-to-end test with complex enum hierarchy
- [ ] Cross-assembly test with constructor patterns
- [ ] Performance benchmarks for constructor-based approach

### Phase 6: Documentation Updates

#### 6.1 README.md Updates
- [ ] Update Quick Start section with constructor pattern
- [ ] Add "Constructor-Based Pattern" section
- [ ] Update all code examples
- [ ] Add migration guide section

#### 6.2 Create Migration Guide
- [ ] Location: `docs/MigrationGuide.md`
- [ ] Show before/after examples
- [ ] Explain benefits of new pattern
- [ ] Provide step-by-step migration

## Implementation Order

1. **Week 1**: Core Infrastructure
   - EnhancedEnumBase<T> ✓
   - EnumOption attribute updates
   - Constructor tracking model

2. **Week 2**: Generator Core
   - Constructor detection
   - Namespace extraction
   - Update EnumValueInfo processing

3. **Week 3**: Code Generation
   - Refactor to use builders
   - Implement factory methods
   - Clean up string concatenation

4. **Week 4**: Quality & Polish
   - Analyzer implementation
   - Comprehensive testing
   - Documentation updates

## Success Criteria

1. All generated code uses proper builders (NO AppendLine spam!)
2. Constructor-based initialization works seamlessly
3. All namespaces properly imported
4. Return type flexibility (attribute and per-option)
5. Clean, readable generated code
6. Backward compatibility maintained
7. Comprehensive test coverage
8. Clear migration path documented

## Notes

- Focus on developer experience
- Prioritize clean, idiomatic C# patterns
- Ensure generated code is debuggable
- Consider IntelliSense experience
- Performance should match or exceed current implementation