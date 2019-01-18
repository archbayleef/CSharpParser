using System.Linq;

namespace CSharpParser
{
	public class Fft
	{
		private readonly int _lg;
		private readonly int[] _g, _og, _rev, _e;

		public int Modulo { get; }
		public int Length { get; }

		private static int ModPow(int x, int k, int m)
		{
			var ans = 1;
			while (k != 0)
			{
				if (k % 2 == 1) ans = (int)((long)ans * x % m);
				x = (int)((long)x * x % m);
				k /= 2;
			}
			return ans;
		}

		private static int Generator(int m)
		{
			var phi = m - 1;
			var pr = new System.Collections.Generic.List<int>();
			for (var i = 2; i * i <= phi; i++)
				if (phi % i == 0)
				{
					pr.Add(m / i);
					while (phi % i == 0)
						phi /= i;
				}
			if (phi != 1)
				pr.Add(m / phi);
			for (var g = 2; ; g++)
				if (pr.All(x => ModPow(g, x, m) != 1))
					return g;
		}

		public Fft(int lg, int m)
		{
			_lg = lg;
			Length = 1 << lg;
			Modulo = m;
			var g = ModPow(Generator(Modulo), Modulo >> lg, Modulo);
			_g = new int[Length];
			_og = new int[Length];
			_rev = new int[Length];
			_e = new int[Length];
			_g[0] = _og[0] = 1;
			for (var i = 1; i < Length; i++)
			{
				_g[i] = _og[Length - i] = (int)((long)_g[i - 1] * g % Modulo);
				_rev[i] = _rev[i / 2] / 2 + (i % 2 << _lg - 1);
			}
		}

		public void Modify(int[] a, bool forward = true)
		{
			for (var i = 0; i < Length; i++)
				_e[i] = a[_rev[i]];
			Inner(_e, a, forward);
			if (_lg % 2 == 0)
				for (var i = 0; i < Length; i++)
					a[i] = _e[i];
		}

		public int[] Apply(int[] a, bool forward = true)
		{
			var c = new int[Length];
			Apply(a, c, forward);
			return c;
		}

		public void Apply(int[] a, int[] c, bool forward = true)
		{
			var e = _e;			
			if (_lg % 2 == 1)
			{
				for (var i = 0; i < Length; i++)
					e[i] = a[_rev[i]];
				Algorithm.Swap(ref e, ref c);
			}
			else
				for (var i = 0; i < Length; i++)
					c[i] = a[_rev[i]];
			Inner(c, e, forward);
		}

		private void Inner(int[] c, int[] e, bool forward)
		{
			var n2 = Length / 2;
			var g = forward ? _g : _og;
			for (var i = _lg - 1; i >= 0; i--)
			{
				for (var j = 0; j < n2; j++)
				{
					e[j] = (int)((c[2 * j] + (long)c[2 * j + 1] * g[j >> i << i]) % Modulo);
					e[n2 + j] = c[2 * j] * 2 - e[j] - Modulo;
					e[n2 + j] += e[n2 + j] >> 31 & Modulo;
					e[n2 + j] += e[n2 + j] >> 31 & Modulo;
				}
				Algorithm.Swap(ref e, ref c);
			}
			if (forward) return;
			var m = Modulo - Modulo / Length;
			for (var i = 0; i < Length; i++)
				c[i] = (int)((long)c[i] * m % Modulo);
		}
	}
}
