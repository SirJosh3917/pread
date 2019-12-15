﻿using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;
using System.IO;

namespace pread.Benchmarks
{
	/* (SSD)
|            Method |     Mean |     Error |    StdDev |
|------------------ |---------:|----------:|----------:|
| StreamReadAndSeek | 2.253 us | 0.0291 us | 0.0272 us |
|             Pread | 1.984 us | 0.0264 us | 0.0234 us |
	 */

	public class StreamVsPread
	{
		public const int FileSize = 1024 * 1000;
		public const int DataSize = 1024 * 10;
		public const int Offset = 1024 * 100;
		private readonly FileStream _fileStream;
		private readonly byte[] _buffer = new byte[DataSize];

		public StreamVsPread()
		{
			new Span<byte>(_buffer).Fill((byte)'A');

			_fileStream = File.Create($"__tmp{new Random().Next()}__.txt");
			_fileStream.SetLength(FileSize);

			_fileStream.Position = Offset;
			_fileStream.Write(new Span<byte>(_buffer));
			_fileStream.Position = 0;
		}

		[Benchmark]
		public void StreamReadAndSeek()
		{
			_fileStream.Seek(Offset, SeekOrigin.Begin);
			_fileStream.Read(new Span<byte>(_buffer));
			_fileStream.Seek(0, SeekOrigin.Begin);
		}

		[Benchmark]
		public void Pread()
		{
			pread.Windows.Pread(_fileStream, new Span<byte>(_buffer), Offset, DataSize);
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			BenchmarkRunner.Run<StreamVsPread>();
		}
	}
}