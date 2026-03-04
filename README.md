# DODLocator
Data-Oriented Design Locator - A low-level memory organizer that transforms C# structs into efficient Structure of Arrays (SoA) layout for high-performance scenarios.

## Quick start
```csharp
// 1. Define your struct
public struct Vector2
{
    float x, y;
}

// 2. Create configuration
SoAConfig cfg = new SoAConfig(128);

// 3. Create SoA array
StructArray<Vector2> soa = new StructArray<Vector2>(cfg);

// That's it!
```

## Use as ECS Foundation
```csharp
public class ComponentsManager<T> where T : unmanaged
{
    private StructArray<T> _array;
    
    public ComponentsManager(SoAConfig config)
    {
        _array = new StructArray<T>(config);
    }
    
    // Add ECS systems here
}
```

## Features
1.Automatic SoA transformation for any unmanaged struct\
2.Cache-efficient contiguous field arrays\
3.Customizable memory allocation and growth strategies\
4.O(1) instance creation and destruction\
5.Perfect for ECS, particle systems, and data-oriented designs
