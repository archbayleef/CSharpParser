using System.Linq;

namespace CSharpParser
{
	public class Fft
	{
		private readonly int _lg, _n;
		private readonly int[] _g, _og, _rev, _e;

		public int Modulo { get; }

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
			_n = 1 << lg;
			Modulo = m;
			var g = ModPow(Generator(Modulo), Modulo >> lg, Modulo);
			_g = new int[_n];
			_og = new int[_n];
			_rev = new int[_n];
			_e = new int[_n];
			_g[0] = _og[0] = 1;
			for (var i = 1; i < _n; i++)
			{
				_g[i] = _og[_n - i] = (int)((long)_g[i - 1] * g % Modulo);
				_rev[i] = _rev[i / 2] / 2 + (i % 2 << _lg - 1);
			}
		}

		public int[] Apply(int[] a, bool forward = true)
		{
			var c = new int[_n];
			Apply(a, c, forward);
			return c;
		}

		public void Apply(int[] a, int[] c, bool forward = true)
		{
			var n2 = _n / 2;
			var e = _e;
			var g = forward ? _g : _og;
			if (_lg % 2 == 1)
			{
				for (var i = 0; i < _n; i++)
					e[i] = a[_rev[i]];
				Algorithm.Swap(ref e, ref c);
			}
			else
				for (var i = 0; i < _n; i++)
					c[i] = a[_rev[i]];
			for (var i = _lg - 1; i >= 0; i--)
			{
				for (var j = 0; j < n2; j++)
				{
					e[j] = (int)((c[2 * j] + (long)c[2 * j + 1] * g[j >> i << i]) % Modulo);
					e[n2 + j] = c[2 * j] * 2 - e[j] - Modulo;
					e[n2 + j] += e[n2 + j] >> 31 & Modulo;
				}
				Algorithm.Swap(ref e, ref c);
			}
			if (forward) return;
			var m = Modulo - Modulo / _n;
			for (var i = 0; i < _n; i++)
				c[i] = (int)((long)c[i] * m % Modulo);
		}
	}
}
