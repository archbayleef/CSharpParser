using System.Linq;

namespace CSharpParser
{
    public class Fft
    {
        private readonly int _lg, _n, _m;
        private readonly int[] _g, _og, _rev;

        public int Modulo { get { return _m; } }

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
            _m = m * _n + 1;
            var g = ModPow(Generator(_m), m, _m);
            _g = new int[_n];
            _og = new int[_n];
            _rev = new int[_n];
            _g[0] = _og[0] = 1;
            for (var i = 1; i < _n; i++)
            {
                _g[i] = _og[_n - i] = (int)((long)_g[i - 1] * g % _m);
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
            var n2 = _n/2;
            var e = new int[n2];
            var o = new int[n2];
            var g = forward ? _g : _og;
            for (var i = 0; i < _n; i++)
                c[i] = _rev[i] < a.Length ? a[_rev[i]] : 0;
            for (var i = _lg - 1; i >= 0; i--)
            {
                for (var j = 0; j < n2; j++)
                {
                    e[j] = c[j << 1];
                    o[j] = c[(j << 1) | 1];
                }
                for (var j = 0; j < n2; j++)
                {
                    c[j] = (int) ((e[j] + (long) o[j]*g[j >> i << i])%_m);
                    c[n2 + j] = e[j]*2 - c[j];
                    if (c[n2 + j] >= _m) c[n2 + j] -= _m;
                    else if (c[n2 + j] < 0) c[n2 + j] += _m;
                }
            }
            if (forward) return;
            var m = _m - _m/_n;
            for (var i = 0; i < _n; i++)
                c[i] = (int) ((long) c[i]*m%_m);
        }
    }
}
