using System;

// Credit: https://stackoverflow.com/questions/839788/is-there-a-way-of-setting-a-property-once-only-in-c-sharp
public sealed class SetOnce<T>
{
    private T _value;
    private bool _valueSet = false;

    public override string ToString()
    {
        return _valueSet ? Convert.ToString(_value) : "";
    }

    public T Value
    {
        get
        {
            if (!_valueSet)
            {
                throw new InvalidOperationException("Value not set.");
            }
            return _value;
        }

        set
        {
            if (_valueSet)
            {
                throw new InvalidOperationException("Value already set.");
            }
            _value = value;
            _valueSet = true;
        }
    }

    public T ValueOrDefault { get { return _value; } }

    public static implicit operator T(SetOnce<T> value)
    {
        return value.Value;
    }
}