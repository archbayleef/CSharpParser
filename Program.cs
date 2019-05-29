using System;
using System.IO;
using System.Collections.Generic;

namespace CSharpParser
{
	public class Solution : SolutionBase
	{
		protected override void Solve()
		{
		}
	}

	public static class Algorithm
	{
		private static readonly Random Rnd = new Random();
		public static void Swap<T>(ref T a, ref T b)
		{
			var temp = a;
			a = b;
			b = temp;
		}

		public static T Max<T>(params T[] a)
		{
			var ans = a[0];
			var comp = Comparer<T>.Default;
			for (var i = 1; i < a.Length; i++) ans = comp.Compare(ans, a[i]) >= 0 ? ans : a[i];
			return ans;
		}

		public static T Min<T>(params T[] a)
		{
			var ans = a[0];
			var comp = Comparer<T>.Default;
			for (var i = 1; i < a.Length; i++) ans = comp.Compare(ans, a[i]) <= 0 ? ans : a[i];
			return ans;
		}

		public static void RandomShuffle<T>(IList<T> a, int index, int length)
		{
			if (index < 0 || length < 0) throw new ArgumentOutOfRangeException();
			var last = index + length;
			if (last > a.Count) throw new ArgumentException();
			for (var i = index + 1; i < last; i++)
			{
				var j = Rnd.Next(index, i + 1);
				var t = a[i];
				a[i] = a[j];
				a[j] = t;
			}
		}

		public static void RandomShuffle<T>(IList<T> a)
		{
			RandomShuffle(a, 0, a.Count);
		}

		public static bool NextPermutation<T>(IList<T> a, int index, int length, Comparison<T> compare = null)
		{
			compare = compare ?? Comparer<T>.Default.Compare;
			if (index < 0 || length < 0) throw new ArgumentOutOfRangeException();
			var last = index + length;
			if (last > a.Count) throw new ArgumentException();
			for (var i = last - 1; i > index; i--)
				if (compare(a[i], a[i - 1]) > 0)
				{
					var j = i + 1;
					for (; j < last; j++) if (compare(a[j], a[i - 1]) <= 0) break;
					var t = a[i - 1];
					a[i - 1] = a[j - 1];
					a[j - 1] = t;
					for (; i < last - 1; i++, last--)
					{
						t = a[i];
						a[i] = a[last - 1];
						a[last - 1] = t;
					}
					return true;
				}
			for (var i = index; i < last - 1; i++, last--)
			{
				var t = a[i];
				a[i] = a[last - 1];
				a[last - 1] = t;
			}
			return false;
		}

		public static bool NextPermutation<T>(IList<T> a, Comparison<T> compare = null)
		{
			return NextPermutation(a, 0, a.Count, compare);
		}

		public static bool PrevPermutation<T>(IList<T> a, int index, int length, Comparison<T> compare = null)
		{
			compare = compare ?? Comparer<T>.Default.Compare;
			if (index < 0 || length < 0) throw new ArgumentOutOfRangeException();
			var last = index + length;
			if (last > a.Count) throw new ArgumentException();
			for (var i = last - 1; i > index; i--)
				if (compare(a[i], a[i - 1]) < 0)
				{
					var j = i + 1;
					for (; j < last; j++) if (compare(a[j], a[i - 1]) >= 0) break;
					var t = a[i - 1];
					a[i - 1] = a[j - 1];
					a[j - 1] = t;
					for (; i < last - 1; i++, last--)
					{
						t = a[i];
						a[i] = a[last - 1];
						a[last - 1] = t;
					}
					return true;
				}
			for (var i = index; i < last - 1; i++, last--)
			{
				var t = a[i];
				a[i] = a[last - 1];
				a[last - 1] = t;
			}
			return false;
		}

		public static bool PrevPermutation<T>(IList<T> a, Comparison<T> compare = null)
		{
			return PrevPermutation(a, 0, a.Count, compare);
		}

		public static int LowerBound<T>(IList<T> a, int index, int length, T value, Comparison<T> compare = null)
		{
			compare = compare ?? Comparer<T>.Default.Compare;
			if (index < 0 || length < 0) throw new ArgumentOutOfRangeException();
			if (index + length > a.Count) throw new ArgumentException();
			var ans = index;
			var last = index + length;
			var p2 = 1;
			while (p2 <= length) p2 *= 2;
			for (p2 /= 2; p2 > 0; p2 /= 2) if (ans + p2 <= last && compare(a[ans + p2 - 1], value) < 0) ans += p2;
			return ans;
		}

		public static int LowerBound<T>(IList<T> a, T value, Comparison<T> compare = null)
		{
			return LowerBound(a, 0, a.Count, value, compare);
		}

		public static int UpperBound<T>(IList<T> a, int index, int length, T value, Comparison<T> compare = null)
		{
			compare = compare ?? Comparer<T>.Default.Compare;
			if (index < 0 || length < 0) throw new ArgumentOutOfRangeException();
			if (index + length > a.Count) throw new ArgumentException();
			var ans = index;
			var last = index + length;
			var p2 = 1;
			while (p2 <= length) p2 *= 2;
			for (p2 /= 2; p2 > 0; p2 /= 2) if (ans + p2 <= last && compare(a[ans + p2 - 1], value) <= 0) ans += p2;
			return ans;
		}

		public static int UpperBound<T>(IList<T> a, T value, Comparison<T> compare = null)
		{
			return UpperBound(a, 0, a.Count, value, compare);
		}

		public static void Fill<T>(this IList<T> array, T value) where T : struct
		{
			for (var i = 0; i < array.Count; i++)
				array[i] = value;
		}

		public static void Fill<T>(this IList<T> array, Func<int, T> func)
		{
			for (var i = 0; i < array.Count; i++)
				array[i] = func(i);
		}
	}

	public class InStream : IDisposable
	{
		protected readonly TextReader InputStream;
		private string[] _tokens;
		private int _pointer;

		private InStream(TextReader inputStream)
		{
			InputStream = inputStream;
		}

		public static InStream FromString(string str)
		{
			return new InStream(new StringReader(str));
		}

		public static InStream FromFile(string str)
		{
			return new InStream(new StreamReader(str));
		}

		public static InStream FromConsole()
		{
			return new InStream(Console.In);
		}

		public string NextLine()
		{
			try
			{
				return InputStream.ReadLine();
			}
			catch (Exception)
			{
				return null;
			}
		}

		private string NextString()
		{
			try
			{
				while (_tokens == null || _pointer >= _tokens.Length)
				{
					_tokens = NextLine().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
					_pointer = 0;
				}
				return _tokens[_pointer++];
			}
			catch (Exception)
			{
				return null;
			}
		}

		public bool Next<T>(out T ans)
		{
			var str = NextString();
			if (str == null)
			{
				ans = default;
				return false;
			}
			ans = (T)Convert.ChangeType(str, typeof(T));
			return true;
		}

		public T[] NextArray<T>(int length)
		{
			var array = new T[length];
			for (var i = 0; i < length; i++)
				if (!Next(out array[i]))
					return null;
			return array;
		}

		public T[,] NextArray<T>(int length, int width)
		{
			var array = new T[length, width];
			for (var i = 0; i < length; i++)
				for (var j = 0; j < width; j++)
					if (!Next(out array[i, j]))
						return null;
			return array;
		}

		public void Dispose()
		{
			InputStream.Close();
		}
	}

	public class OutStream : IDisposable
	{
		protected readonly TextWriter OutputStream;

		private OutStream(TextWriter outputStream)
		{
			OutputStream = outputStream;
		}

		public static OutStream FromString(System.Text.StringBuilder strB)
		{
			return new OutStream(new StringWriter(strB));
		}

		public static OutStream FromFile(string str)
		{
			return new OutStream(new StreamWriter(str));
		}

		public static OutStream FromConsole()
		{
			return new OutStream(Console.Out);
		}

		public void Print(string format, params object[] args)
		{
			OutputStream.Write(format, args);
		}

		public void PrintLine(string format, params object[] args)
		{
			Print(format, args);
			OutputStream.WriteLine();
		}

		public void PrintLine()
		{
			OutputStream.WriteLine();
		}

		public void Print<T>(T o)
		{
			OutputStream.Write(o);
		}

		public void PrintLine<T>(T o)
		{
			OutputStream.WriteLine(o);
		}

		public void PrintArray<T>(IList<T> a, string between = " ", string after = "\n", bool printCount = false, string afterCount = "\n")
		{
			if (printCount)
				Print("{0}{1}", a.Count, afterCount);
			for (var i = 0; i < a.Count; i++)
				Print("{0}{1}", a[i], i == a.Count - 1 ? after : between);
		}

		public void Dispose()
		{
			OutputStream.Close();
		}
	}

	public abstract class SolutionBase : IDisposable
	{
		private InStream _in;
		private OutStream _out;

		protected SolutionBase()
		{
			//System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
			_in = InStream.FromConsole();
			_out = OutStream.FromConsole();
		}

		protected string NextLine()
		{
			return _in.NextLine();
		}

		protected bool Next<T>(out T ans)
		{
			return _in.Next(out ans);
		}

		protected T[] NextArray<T>(int length)
		{
			return _in.NextArray<T>(length);
		}

		protected T[,] NextArray<T>(int length, int width)
		{
			return _in.NextArray<T>(length, width);
		}

		protected void PrintArray<T>(IList<T> a, string between = " ", string after = "\n", bool printCount = false, string afterCount = "\n")
		{
			_out.PrintArray(a, between, after, printCount, afterCount);
		}

		public void Print(string format, params object[] args)
		{
			_out.Print(format, args);
		}

		public void PrintLine(string format, params object[] args)
		{
			_out.PrintLine(format, args);
		}

		public void PrintLine()
		{
			_out.PrintLine();
		}

		public void Print<T>(T o)
		{
			_out.Print(o);
		}

		public void PrintLine<T>(T o)
		{
			_out.PrintLine(o);
		}

		public void Dispose()
		{
			_in.Dispose();
			_out.Dispose();
		}

		public void Freopen(string path, FileAccess access)
		{
			switch (access)
			{
				case FileAccess.Read:
					_in.Dispose();
					_in = InStream.FromFile(path);
					break;
				case FileAccess.Write:
					_out.Dispose();
					_out = OutStream.FromFile(path);
					break;
			}
		}

		protected abstract void Solve();

		public static void Main()
		{
			using (var p = new Solution()) p.Solve();
		}
	}
}
