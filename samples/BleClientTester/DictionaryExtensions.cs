using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BleClientTester;

/// <summary>Dictionary class extension.</summary>
public static class DictionaryExtension
{
  /// <summary>Add dictionary to dictionary.</summary>
  /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
  /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
  /// <param name="destination">Destination dictionary.</param>
  /// <param name="source">Source dictionary.</param>
  /// <example>
  ///   <code>
  ///     using Xeno.Pos.Client.Data.Extensions;
  ///     ActiveSessions.AddRange(_userService.ActiveUsers);
  ///   </code>
  /// </example>
  /// <returns>Dictionary to destination.</returns>
  public static Dictionary<TKey, TValue> AddRange<TKey, TValue>(this Dictionary<TKey, TValue> destination, Dictionary<TKey, TValue> source)
  {
    if (destination == null)
      destination = new Dictionary<TKey, TValue>();

    if (source == null)
      return destination;

    foreach (var e in source)
      destination.SafeAdd(e.Key, e.Value);

    return destination;
  }

  /// <summary>Add to collection. If key exists, it will overwrite it.</summary>
  /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
  /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
  /// <param name="dict">Dictionary.</param>
  /// <param name="key">The key of the value to get or set.</param>
  /// <param name="value">The value of an element.</param>
  /// <example>
  ///   <code>
  ///     private Users _users;
  ///     var User user = new User();
  ///     _users.SafeAdd(user.UserId, user);
  ///   </code>
  /// </example>
  public static void SafeAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
  {
    if (dict == null)
      dict = new Dictionary<TKey, TValue>();

    dict[key] = value;
  }
}
