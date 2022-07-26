using System;
using System.Reflection;

namespace Linux.Bluetooth.Tests
{
  /// <summary>Helpers</summary>
  /// <remarks>
  ///   References:
  ///     - https://stackoverflow.com/a/9274326/249492
  ///     - https://stackoverflow.com/questions/1196991/get-property-value-from-string-using-reflection/1954663#1954663
  /// </remarks>
  public static class Helpers
  {
    public static object GetPropValue(this object obj, string name)
    {
      foreach (string part in name.Split('.'))
      {
        if (obj == null)
          return null;

        Type type = obj.GetType();
        PropertyInfo info = type.GetProperty(part);

        if (info is null)
          return null;

        obj = info.GetValue(obj, null);
      }

      return obj;
    }

    public static T GetPropValue<T>(this object obj, string name)
    {
      object retval = GetPropValue(obj, name);

      if (retval is null)
        return default(T);

      // throws InvalidCastException if types are incompatible
      return (T)retval;
    }

    public static object? GetPropertyValue(object srcobj, string propertyName)
    {
      if (srcobj == null)
        return null;

      object obj = srcobj;

      // Split property name to parts (propertyName could be hierarchical, like obj.subobj.subobj.property
      string[] propertyNameParts = propertyName.Split('.');

      foreach (string propertyNamePart in propertyNameParts)
      {
        if (obj == null)
          return null;

        // propertyNamePart could contain reference to specific 
        // element (by index) inside a collection
        if (!propertyNamePart.Contains("["))
        {
          PropertyInfo pi = obj.GetType().GetProperty(propertyNamePart);
          if (pi == null)
            return null;

          obj = pi.GetValue(obj, null);
        }
        else
        {   // propertyNamePart is areference to specific element 
            // (by index) inside a collection
            // like AggregatedCollection[123]
            //   get collection name and element index
          int indexStart = propertyNamePart.IndexOf("[") + 1;
          string collectionPropertyName = propertyNamePart.Substring(0, indexStart - 1);
          int collectionElementIndex = Int32.Parse(propertyNamePart.Substring(indexStart, propertyNamePart.Length - indexStart - 1));

          //   get collection object
          PropertyInfo pi = obj.GetType().GetProperty(collectionPropertyName);

          if (pi == null)
            return null;

          object unknownCollection = pi.GetValue(obj, null);

          //   try to process the collection as array
          if (unknownCollection.GetType().IsArray)
          {
            object[] collectionAsArray = unknownCollection as object[];
            obj = collectionAsArray[collectionElementIndex];
          }
          else
          {
            //   try to process the collection as IList
            System.Collections.IList collectionAsList = unknownCollection as System.Collections.IList;
            if (collectionAsList != null)
            {
              obj = collectionAsList[collectionElementIndex];
            }
            else
            {
              // ??? Unsupported collection type
            }
          }
        }
      }

      return obj;
    }
  }
}
