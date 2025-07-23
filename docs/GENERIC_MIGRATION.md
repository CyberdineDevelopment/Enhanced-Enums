# Generic Enhanced Enums Migration Guide

## Overview

Enhanced Enums now supports generic base types without requiring a separate attribute.

## Key Features

1. **Type Parameter Support**: Single or multiple type parameters
2. **Constraint Preservation**: All type constraints are preserved
3. **Namespace Resolution**: Automatic extraction of required namespaces
4. **Default Return Types**: Specify collection return types for complex generics

## Migration Steps

If you previously avoided generics due to ENH001 error:

1. Remove any workarounds
2. Apply `[EnhancedEnumBase]` directly to your generic base type
3. Optionally specify `DefaultGenericReturnType` for complex scenarios

## Examples

### Simple Generic
```csharp
[EnhancedEnumBase]
public abstract class Handler<T>
{
    public abstract void Handle(T item);
}
```

### Multiple Type Parameters
```csharp
[EnhancedEnumBase]
public abstract class Converter<TInput, TOutput>
{
    public abstract TOutput Convert(TInput input);
}
```

### With Constraints
```csharp
[EnhancedEnumBase]
public abstract class Repository<T> where T : class, IEntity, new()
{
    public abstract string TableName { get; }
}
```