﻿using pread.Implementations;

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace pread
{
	/// <summary>
	/// Main API entrypoint for atomic seek and read/write operations.
	/// </summary>
	public static class P
	{
		private static readonly bool _windows = Windows.MachineIsWindows;
		private static readonly bool _linux = Unix.MachineIsUnix;

		/// <summary>
		/// Returns true if atmoic seek and read/write operations are supported.
		/// </summary>
		public static bool IsSupported { get; } = _windows || _linux;

		/// <summary>
		/// Performs an atomic seek and read operation to read data from within a file.
		/// <remarks>
		/// DO NOT RELY ON THE POSITION OF THE FILESTREAM AFTER THIS CALL TO BE UNIFORM ON BOTH PLATFORMS.
		/// </remarks>
		/// </summary>
		/// <param name="fileStream">The <see cref="FileStream"/> to perform the operation with.</param>
		/// <param name="buffer">The buffer to read data in from.</param>
		/// <param name="fileOffset">The offset, starting at the beginning of the file, to read data from.</param>
		/// <returns>The number of bytes read.</returns>
		/// <exception cref="IOException">When a read operation fails.</exception>
		/// <exception cref="NotSupportedException">If the platform the code is running on does not support this operation.
		/// Check if this platform is supported with <see cref="P.IsSupported"/> to avoid this.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint Read(FileStream fileStream, Span<byte> buffer, ulong fileOffset)
		{
			if (_windows)
			{
				return ResultWindows(Windows.PRead(fileStream, buffer, fileOffset));
			}
			else if (_linux)
			{
				return ResultUnix(Unix.PRead(fileStream, buffer, fileOffset));
			}
			else
			{
				ThrowHelper();
				return 0;

				static void ThrowHelper() => throw new NotSupportedException("Cannot use atomic seek and read/write operations on this platform.");
			}
		}

		/// <summary>
		/// Performs an atomic seek and write operation to write data from within a file without changing
		/// where the <see cref="FileStream"/>'s <see cref="FileStream.Position"/> is.
		/// <remarks>
		/// DO NOT RELY ON THE POSITION OF THE FILESTREAM AFTER THIS CALL TO BE UNIFORM ON BOTH PLATFORMS.
		/// </remarks>
		/// </summary>
		/// <param name="fileStream">The <see cref="FileStream"/> to perform the operation with.</param>
		/// <param name="data">The data to write.</param>
		/// <param name="fileOffset">The offset, starting at the beginning of the file, to write data from.</param>
		/// <returns>The number of bytes written.</returns>
		/// <exception cref="IOException">When a read operation fails.</exception>
		/// <exception cref="NotSupportedException">If the platform the code is running on does not support this operation.
		/// Check if this platform is supported with <see cref="P.IsSupported"/> to avoid this.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint Write(FileStream fileStream, ReadOnlySpan<byte> data, ulong fileOffset)
		{
			if (_windows)
			{
				return ResultWindows(Windows.PWrite(fileStream, data, fileOffset));
			}
			else if (_linux)
			{
				return ResultUnix(Unix.PWrite(fileStream, data, fileOffset));
			}
			else
			{
				ThrowHelper();
				return 0;

				static void ThrowHelper() => throw new NotSupportedException("Cannot use atomic seek and read/write operations on this platform.");
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static uint ResultWindows(Windows.PResult result)
		{
			if (!result.DidSucceed)
			{
				ThrowHelper(result.WindowsErrorCode);
				static void ThrowHelper(int errorCode) => throw new IOException(Windows.StringError(errorCode));
			}

			return result.Bytes;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static uint ResultUnix(Unix.PResult result)
		{
			if (!result.DidSucceed)
			{
				ThrowHelper(result.Errno);
				static void ThrowHelper(int errorCode) => throw new IOException(Unix.StringError(errorCode));
			}

			// no way for dot net to write more than uint.MaxValue data so this is safe
			return (uint)result.Bytes;
		}
	}
}