using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks;
using Shouldly;
using Xunit;

namespace FractalDataWorks.Tests;

public class FractalTests
{
    [Fact]
    public void FractalValueReturnsSameInstance()
    {
        // Act
        var fractal1 = Fractal.Value;
        var fractal2 = Fractal.Value;

        // Assert
        fractal1.ShouldBe(fractal2);
    }

    [Fact]
    public void FractalEqualsAlwaysReturnsTrue()
    {
        // Arrange
        var fractal1 = new Fractal();
        var fractal2 = new Fractal();

        // Act & Assert
        fractal1.Equals(fractal2).ShouldBeTrue();
        fractal1.Equals(Fractal.Value).ShouldBeTrue();
    }

    [Fact]
    public void FractalEqualsObjectReturnsTrueForFractal()
    {
        // Arrange
        var fractal = new Fractal();
        object boxedFractal = new Fractal();

        // Act & Assert
        fractal.Equals(boxedFractal).ShouldBeTrue();
        fractal.Equals(null).ShouldBeFalse();
        fractal.Equals("not a fractal").ShouldBeFalse();
        fractal.Equals(42).ShouldBeFalse();
    }

    [Fact]
    public void FractalGetHashCodeAlwaysReturnsZero()
    {
        // Arrange
        var fractal1 = new Fractal();
        var fractal2 = Fractal.Value;

        // Act & Assert
        fractal1.GetHashCode().ShouldBe(0);
        fractal2.GetHashCode().ShouldBe(0);
    }

    [Fact]
    public void FractalToStringReturnsParentheses()
    {
        // Arrange
        var fractal = new Fractal();

        // Act
        var result = fractal.ToString();

        // Assert
        result.ShouldBe("()");
    }

    [Fact]
    public void FractalEqualityOperatorAlwaysReturnsTrue()
    {
        // Arrange
        var fractal1 = new Fractal();
        var fractal2 = Fractal.Value;

        // Act & Assert
        (fractal1 == fractal2).ShouldBeTrue();
        (Fractal.Value == new Fractal()).ShouldBeTrue();
    }

    [Fact]
    public void FractalInequalityOperatorAlwaysReturnsFalse()
    {
        // Arrange
        var fractal1 = new Fractal();
        var fractal2 = Fractal.Value;

        // Act & Assert
        (fractal1 != fractal2).ShouldBeFalse();
        (Fractal.Value != new Fractal()).ShouldBeFalse();
    }


    [Fact]
    public void FractalWorksInCollections()
    {
        // Arrange
        var set = new HashSet<Fractal> { new Fractal(), Fractal.Value, new Fractal() };
        var list = new List<Fractal> { new Fractal(), Fractal.Value, new Fractal() };

        // Assert
        set.Count.ShouldBe(1); // All instances are equal, so HashSet contains only one
        list.Count.ShouldBe(3); // List preserves all instances
        list.Distinct().Count().ShouldBe(1); // But they're all equal
    }
}