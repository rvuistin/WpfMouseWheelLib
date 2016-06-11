using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Lada.ComponentModel
{
  public abstract class StringableConverter<T> : TypeConverter
  {
    #region TypeConverter
    /// <summary>Determines whether an object of the specified type can be converted to an instance of T,using the specified context.</summary>
    /// <returns>true if this converter can perform the operation; otherwise, false.</returns>
    /// <param name="context">A format context that provides information about the environment from which this converter is being invoked.</param>
    /// <param name="sourceType">The type being evaluated for conversion.</param>
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
      return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
    }
    /// <summary>Determines whether an instance of T can be converted to the specified type, using the specified context.</summary>
    /// <returns>true if this converter can perform the operation; otherwise, false.</returns>
    /// <param name="context">A format context that provides information about the environment from which this converter is being invoked.</param>
    /// <param name="destinationType">The type being evaluated for conversion.</param>
    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
    {
      return ((destinationType == typeof(InstanceDescriptor)) || base.CanConvertTo(context, destinationType));
    }
    /// <summary>Attempts to convert the specified object to a T, using the specified context.</summary>
    /// <returns>The converted object.</returns>
    /// <param name="context">A format context that provides information about the environment from which this converter is being invoked.</param>
    /// <param name="culture">Culture specific information.</param>
    /// <param name="source">The object to convert.</param>
    /// <exception cref="T:System.FormatException">
    ///   <paramref name="source" /> does not map to a valid T.</exception>
    /// <exception cref="T:System.NotSupportedException">
    ///   <paramref name="source" /> cannot be converted.</exception>
    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object source)
    {
      if (source is string)
      {
        try
        {
          return Parse((string)source);
        }
        catch (FormatException exception)
        {
          throw new FormatException(string.Format("{0} is not a valid value for {1}", source, typeof(T)), exception);
        }
      }
      return base.ConvertFrom(context, culture, source);
    }
    /// <summary>Attempts to convert a source object to the specified type, using the specified context.</summary>
    /// <returns>The converted object.</returns>
    /// <param name="context">A format context that provides information about the environment from which this converter is being invoked.</param>
    /// <param name="culture">Culture specific information.</param>
    /// <param name="value">The object to convert.</param>
    /// <param name="destinationType">The type to convert the object to.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="destinationType" /> is null.</exception>
    /// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
    ///   <paramref name="value" /> does not map to a valid T.</exception>
    /// <exception cref="T:System.NotSupportedException">
    ///   <paramref name="value" /> cannot be converted.  </exception>
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
      if ((destinationType == typeof(string)) && (value is T))
        return ToString((T)value);
      return base.ConvertTo(context, culture, value, destinationType);
    }
    #endregion

    #region Overridables
    protected abstract T         Parse(string source);
    protected virtual  string ToString(T      value) { return value.ToString(); }
    #endregion
  }
}
