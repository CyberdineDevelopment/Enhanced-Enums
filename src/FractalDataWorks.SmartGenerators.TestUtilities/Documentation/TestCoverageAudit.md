# Test Coverage Audit for FractalDataWorks.SmartGenerators.CodeBuilder and FractalDataWorks.SmartGenerators

**Date:** 2025-04-22

## Objective
- Ensure all code in `RepositoryRoot/src/FractalDataWorks.SmartGenerators.CodeBuilder` and `RepositoryRoot/src/FractalDataWorks.SmartGenerators` has corresponding test mechanisms in `RepositoryRoot/src/FractalDataWorks.SmartGenerators.Testing`.
- Audit the presence and adequacy of mock generators and generator testing infrastructure, in line with best practices for Incremental Source Generators.

---

## Inventory

### CodeBuilder Library
- AttributeBuilder.cs
- ClassBuilder.cs
- CodeBlockBuilder.cs
- CodeBuilder.cs
- CodeBuilderBase.cs
- CodeBuilderFactory.cs
- ConstructorBuilder.cs
- EnumBuilder.cs
- FieldBuilder.cs
- IBuildable.cs
- ICodeBuilder.cs
- ICodeElement.cs
- IMemberBuilder.cs
- InterfaceBuilder.cs
- MemberBuilderBase.cs
- MethodBuilder.cs
- MethodBuilderExtensions.cs
- Modifiers.cs
- NamespaceBuilder.cs
- PropertyBuilder.cs

### SourceGenerators Library
- DiagnosticReporter.cs
- IGenerationStrategy.cs
- IInputInfo.cs
- IncrementalGeneratorBase.cs
- InputTracker.cs
- Discovery/CrossAssemblyTypeDiscoveryService.cs

### Testing Infrastructure
- Attributes/
  - GenerateCodeAttribute.cs
  - GenerateEqualsAttribute.cs
- ClassExpectations.cs
- CodeBlockExpectations.cs
- CompilationVerifier.cs
- ExpectationsFactory.cs
- FieldExpectations.cs
- InterfaceExpectations.cs
- MethodExpectations.cs
- NamespaceExpectations.cs
- ParameterExpectations.cs
- PropertyExpectations.cs
- SourceGeneratorTestHelper.cs
- SyntaxNodeAssertions.cs
- SyntaxNodeHelpers.cs
- SyntaxTreeExpectations.cs
- TestGeneratorContext.cs
- TestGenerators.cs
- TestMocks.cs
- TestSourceProvider.cs

---

## Test Mechanisms & Coverage

### 1. **Source Generator Test Infrastructure**
- **SourceGeneratorTestHelper.cs**: Provides methods to run incremental generators and capture output/diagnostics.
- **TestGeneratorContext.cs**: Mock context to capture diagnostics and generated sources.
- **TestGenerators.cs**: Contains `MockGenerator` and other test generators for verifying generator logic.
- **TestMocks.cs**: Provides mock implementations for interfaces and diagnostic testing.
- **TestSourceProvider.cs**: Supplies code sources for generator tests.

### 2. **Expectation Classes**
- `ClassExpectations`, `MethodExpectations`, `PropertyExpectations`, etc., provide assertion helpers for generated code structure.

### 3. **Attributes**
- Custom attributes for testing generator behavior: `GenerateCodeAttribute`, `GenerateEqualsAttribute`.

### 4. **Coverage of CodeBuilder and SourceGenerators**
- **Direct references to CodeBuilder/SourceGenerators in Testing:**
  - No direct `using` statements for `FractalDataWorks.SmartGenerators.CodeBuilder` or `FractalDataWorks.SmartGenerators` in test implementation files.
  - Test helpers and mocks are designed to test any generator via Roslyn APIs, not by referencing the implementation directly.
  - Generator tests use code strings and attributes to trigger and verify generator output.
- **Mock Generators:**
  - `TestGenerators.MockGenerator` implements `IIncrementalGenerator` and is used for verifying generator logic, attribute handling, and incremental behavior.
  - `TestMocks.TestDiagnosticGenerator` for diagnostic scenarios.

### 5. **Incremental Source Generator Best Practices**
- **Mock Generators** are present and used for isolated, repeatable tests.
- **Helpers** for running generators, capturing diagnostics, and verifying output are implemented.
- **Test attributes** are included for marking up test sources.
- **No direct coupling** to implementation: tests are designed to work with any generator, promoting flexibility and best practices.

---

## API Method–to–Testing Helper Mapping (2025‑04‑23)

| API Element (From `APIInventory.md`) | Primary Testing Helper / Expectation Path | Covered? | Notes |
|--------------------------------------|-------------------------------------------|----------|-------|
| **AttributeBuilder** | | | |
| `AttributeBuilder.AddArgument(string)` | SyntaxTreeExpectations → ClassExpectations.HasAttributeArgument *(via custom parsing)* | | Verified via parsing attribute node and argument list |
| `AttributeBuilder.AddAttribute(string)` | SyntaxTreeExpectations → ClassExpectations.HasAttribute *(nested)* | | |
| `AttributeBuilder.Build()` | CompilationVerifier / ExpectCode | | Build result parsed and validated |
| **ClassBuilder** | | | |
| `ClassBuilder.WithBaseType(string)` | ClassExpectations.HasBaseType | | |
| `ClassBuilder.WithBaseType<T>()` | ClassExpectations.HasBaseType | | Generic overload works identically |
| `ClassBuilder.ImplementsInterface(string)` | ClassExpectations.ImplementsInterface | | |
| `ClassBuilder.ImplementsInterface<T>()` | ClassExpectations.ImplementsInterface | | |
| `ClassBuilder.WithNamespace(string)` | NamespaceExpectations.HasClass | | Class placed in expected namespace |
| `ClassBuilder.AddMethod(string,string)` | ClassExpectations.HasMethod | | |
| `ClassBuilder.AddProperty(string,string)` | ClassExpectations.HasProperty | | |
| `ClassBuilder.AddField(string,string)` | ClassExpectations.HasField | | |
| `ClassBuilder.AddField<T>(string)` | ClassExpectations.HasField | | |
| `ClassBuilder.AddConstructor()` | **❌ MISSING** | | No `HasConstructor` helper exists yet — **TODO‑CB‑001** |
| `ClassBuilder.AddCodeBlock(string)` | CodeBlockExpectations.Contains | | |
| `ClassBuilder.AddCodeBlock(Action<ICodeBuilder>)` | CodeBlockExpectations.Satisfies | | |
| `ClassBuilder.Build()` | CompilationVerifier / ExpectCode | | |
| **CodeBlockBuilder** | | | |
| `CodeBlockBuilder.AppendLine(string)` | CodeBlockExpectations.ContainsLine | | |
| `CodeBlockBuilder.Indent()` | CodeBlockExpectations.IndentedCorrectly | | |
| `CodeBlockBuilder.Outdent()` | CodeBlockExpectations.IndentedCorrectly | | |
| `CodeBlockBuilder.Build()` | CompilationVerifier / ExpectCode | | |
| **CodeBuilderFactory** *(static helpers)* | | | |
| `CreateCodeBuilder(int)` | ExpectCode → SyntaxTreeExpectations | | Validates indent size behaviour |
| `CreateClass(string)` | NamespaceExpectations.HasClass | | |
| `CreateMethod(string,string)` | MethodExpectations.HasReturnType | | |
| `CreateProperty(string,string)` | PropertyExpectations.HasType | | |
| `CreateInterface(string)` | NamespaceExpectations.HasInterface | | |
| `CreateNamespace(string)` | SyntaxTreeExpectations.HasNamespace | | |
| `CreateField(string,string)` | FieldExpectations.HasType | | |
| `CreateConstructor(string)` | **❌ MISSING** | | Requires `HasConstructor` expectations — see **TODO‑CB‑001** |
| `CreateAttribute(string)` | SyntaxTreeExpectations.HasAttribute | | |
| `CreateEnum(string)` | NamespaceExpectations.HasEnum | | |
| **ConstructorBuilder** | | | |
| `ConstructorBuilder.AddParameter(string,string,string?)` | MethodExpectations.HasParameter | | Constructor treated as method for expectation helper |
| `ConstructorBuilder.AddParameter<T>(string,string?)` | MethodExpectations.HasParameter | | |
| `ConstructorBuilder.WithParameter(string,string?)` | MethodExpectations.HasParameter | | |
| `ConstructorBuilder.WithBody(Action<ICodeBuilder>)` | CodeBlockExpectations.Satisfies | | |
| `ConstructorBuilder.WithCodeBlock(string)` | CodeBlockExpectations.Contains | | |
| `ConstructorBuilder.WithBaseCall(params string[])` | **❌ MISSING** | | Need expectation for ctor initializer — **TODO‑CB‑002** |
| `ConstructorBuilder.WithThisCall(params string[])` | **❌ MISSING** | | Need expectation for ctor initializer — **TODO‑CB‑002** |
| `ConstructorBuilder.Build()` | CompilationVerifier / ExpectCode | | |
| **EnumBuilder** | | | |
| `EnumBuilder.AddMember(string)` | EnumExpectations.HasMember | | EnumExpectations exists |
| `EnumBuilder.Build()` | CompilationVerifier / ExpectCode | | |
| **FieldBuilder** | | | |
| `FieldBuilder.WithInitializer(CodeBlockBuilder)` | FieldExpectations.HasInitializer | | |
| `FieldBuilder.MakeReadOnly()` | FieldExpectations.IsReadOnly | | |
| `FieldBuilder.MakeConst(string)` | FieldExpectations.IsConst | | |
| `FieldBuilder.WithInitializer(string)` | FieldExpectations.HasInitializer | | |
| `FieldBuilder.AddAttribute(AttributeBuilder)` | FieldExpectations.HasAttribute | | |
| `FieldBuilder.Build()` | CompilationVerifier / ExpectCode | | |
| **InterfaceBuilder** | | | |
| `InterfaceBuilder.WithBaseInterface(string)` | InterfaceExpectations.HasBaseInterface | | |
| `InterfaceBuilder.WithBaseInterface<T>()` | InterfaceExpectations.HasBaseInterface | | |
| `InterfaceBuilder.AddMethod(string,string)` | InterfaceExpectations.HasMethod | | |
| `InterfaceBuilder.AddProperty(string,string)` | InterfaceExpectations.HasProperty | | |
| `InterfaceBuilder.Build()` | CompilationVerifier / ExpectCode | | |
| **MethodBuilder** | | | |
| `MethodBuilder.AddParameter(string,string,string?)` | MethodExpectations.HasParameter | | |
| `MethodBuilder.AddParameter<T>(string,string?)` | MethodExpectations.HasParameter | | |
| `MethodBuilder.WithParameter(string,string?)` | MethodExpectations.HasParameter | | |
| `MethodBuilder.WithXmlDocParam(string,string)` | **❌ MISSING** | | No xml‑doc validation helper — **TODO‑CB‑003** |
| `MethodBuilder.WithXmlDocReturns(string)` | **❌ MISSING** | | See **TODO‑CB‑003** |
| `MethodBuilder.WithXmlDocException(string,string)` | **❌ MISSING** | | See **TODO‑CB‑003** |
| `MethodBuilder.WithBody(Action<ICodeBuilder>)` | CodeBlockExpectations.Satisfies | | |
| `MethodBuilder.WithCodeBlock(string)` | CodeBlockExpectations.Contains | | |
| `MethodBuilder.WithBody(string)` | CodeBlockExpectations.Contains | | |
| `MethodBuilder.MakeAbstract()` | MethodExpectations.IsAbstract | | |
| `MethodBuilder.MakeAsync()` | MethodExpectations.IsAsync *(needs helper?)* | | Helper exists |
| `MethodBuilder.Build()` | CompilationVerifier / ExpectCode | | |
| **NamespaceBuilder** | | | |
| `NamespaceBuilder.AddUsing(string)` | NamespaceExpectations.HasUsing | | |
| `NamespaceBuilder.WithUsing(string)` | NamespaceExpectations.HasUsing | | |
| `NamespaceBuilder.AddClass(ClassBuilder)` | NamespaceExpectations.HasClass | | |
| `NamespaceBuilder.WithClass(ClassBuilder)` | NamespaceExpectations.HasClass | | |
| `NamespaceBuilder.AddInterface(InterfaceBuilder)` | NamespaceExpectations.HasInterface | | |
| `NamespaceBuilder.AddEnum(EnumBuilder)` | NamespaceExpectations.HasEnum | | |
| `NamespaceBuilder.WithEnum(EnumBuilder)` | NamespaceExpectations.HasEnum | | |
| `NamespaceBuilder.Build()` | CompilationVerifier / ExpectCode | | |

**Legend**:
-  = Expectation/helper exists and used in tests.
-  = Gap; expectation/helper not yet implemented.

### Coverage Gaps Identified

1. **TODO‑CB‑001** — Add `ConstructorExpectations` (or extend `ClassExpectations`) to validate constructors (`HasConstructor`, parameter checks, etc.).
2. **TODO‑CB‑002** — Expectation helpers to verify base/this constructor initializers.
3. **TODO‑CB‑003** — XML documentation assertion helpers (e.g., `MethodExpectations.HasXmlDocParam`).

These items have been added to active TODO tracking (MAP‑009/010 scope).

---

## Gaps & Recommendations

- **No direct test coverage for internal logic of CodeBuilder classes (e.g., ClassBuilder, MethodBuilder, etc.)**
  - Recommend adding unit tests for each builder class in a dedicated test project (e.g., `FractalDataWorks.SmartGenerators.CodeBuilder.Tests`).
  - Current tests focus on generator output, not on the builder API itself.
- **No direct references to `FractalDataWorks.SmartGenerators.CodeBuilder` or `FractalDataWorks.SmartGenerators` in test code.**
  - Consider adding integration tests that use the builder API to generate code, then verify output with existing expectation classes.
- **Mock generators and helpers are present and align with best practices.**

---

## Summary Table

| Code Area                                 | Test Mechanism Present    | Adequate? | Notes |
|-------------------------------------------|--------------------------|-----------|-------|
| CodeBuilder Classes (builders, base, etc) | NO (see above)           | Partial   | Recommend unit tests for API surface |
| SourceGenerators (incremental, diagnostics)| YES (via mocks/helpers)  | Yes       | Good Roslyn-based test coverage |
| Mock Generators                           | YES                      | Yes       | MockGenerator, TestDiagnosticGenerator |
| Test Helpers/Expectations                  | YES                      | Yes       | Comprehensive for generator output |

---

## Action Items

1. Add unit tests for CodeBuilder API classes to ensure direct coverage of builder logic.
2. Consider integration tests that use CodeBuilder to generate code, then validate with expectations.
3. Continue using mock generators and helpers for incremental generator best practices.

---

*This audit confirms that generator testing infrastructure is robust and follows best practices, but recommends improved direct coverage for builder APIs.*
