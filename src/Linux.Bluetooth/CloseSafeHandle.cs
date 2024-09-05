using System;
using System.Runtime.InteropServices;

namespace Tmds.DBus;

/// <summary>
/// Generic file descriptor SafeHandle.
/// </summary>
public class CloseSafeHandle : SafeHandle
{
  [DllImport("libc", SetLastError = true)]
  internal static extern int close(int fd);

  /// <summary>
  /// Creates a new CloseSafeHandle.
  /// </summary>
  /// <param name="preexistingHandle">An IntPtr object that represents the pre-existing handle to use.</param>
  /// <param name="ownsHandle"><c>true</c> to reliably release the handle during the finalization phase; <c>false</c> to prevent reliable release.</param>
  public CloseSafeHandle(IntPtr preexistingHandle, bool ownsHandle)
      : base(new IntPtr(-1), ownsHandle)
  {
    SetHandle(preexistingHandle);
  }

  /// <summary>
  /// Gets a value that indicates whether the handle is invalid.
  /// </summary>
  public override bool IsInvalid
  {
    get { return handle == new IntPtr(-1); }
  }

  /// <summary>
  /// When overridden in a derived class, executes the code required to free the handle.
  /// </summary>
  protected override bool ReleaseHandle()
  {
    return close(handle.ToInt32()) == 0;
  }
}
