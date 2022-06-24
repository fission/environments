using System;
using System.Collections.Generic;

public readonly record struct MyStruct(string myField);

public sealed class MyClass
{
    private static readonly Lazy<MyClass> lazy =
        new Lazy<MyClass>(() => new MyClass());

    public static MyClass Instance { get { return lazy.Value; } }

    private double _myValue = 0;

    public double myValue
    {
        get
        {
            if (_myValue == 0)
            {
                _myValue = MathNet.Numerics.SpecialFunctions.Erf(0.5);
            }
            return _myValue;
        }
        set
        {
            myValue = value;
        }
    }

    static MyClass()
    {
    }

    private MyClass()
    {
    }
}
