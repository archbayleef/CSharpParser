using System;
using System.Numerics;

namespace CSharpParser
{
	internal struct Fraction : IComparable<Fraction>
	{
		public bool Equals(Fraction other)
		{
			return Numerator.Equals(other.Numerator) && Denominator.Equals(other.Denominator);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Fraction && Equals((Fraction)obj);
		}

		public Fraction(int nom, int denom = 1) : this((BigInteger)nom, denom) { }
		public Fraction(long nom, long denom = 1) : this((BigInteger)nom, denom) { }
		public Fraction(BigInteger nom) : this(nom, BigInteger.One) { }

		public Fraction(BigInteger nom, BigInteger denom)
		{
			Numerator = nom;
			Denominator = denom;
			Simplify();
		}

		public BigInteger Numerator { get; private set; }
		public BigInteger Denominator { get; private set; }

		public static implicit operator Fraction(int value)
		{
			return new Fraction(value);
		}

		public static implicit operator Fraction(long value)
		{
			return new Fraction(value);
		}

		public static implicit operator Fraction(BigInteger value)
		{
			return new Fraction(value);
		}

		private void Simplify()
		{
			var g = BigInteger.GreatestCommonDivisor(Numerator, Denominator);
			Numerator /= g;
			Denominator /= g;
			if (Denominator >= 0) return;
			Numerator = -Numerator;
			Denominator = -Denominator;
		}

		public int CompareTo(Fraction other)
		{
			var f = this - other;
			return f.Numerator > 0 ? 1 : f.Numerator < 0 ? -1 : 0;
		}

		public override string ToString()
		{
			return Numerator + "/" + Denominator;
		}

		public static Fraction operator +(Fraction f1, Fraction f2)
		{
			return new Fraction(f1.Numerator * f2.Denominator + f1.Denominator * f2.Numerator, f1.Denominator * f2.Denominator);
		}

		public static Fraction operator -(Fraction f1, Fraction f2)
		{
			return new Fraction(f1.Numerator * f2.Denominator - f1.Denominator * f2.Numerator, f1.Denominator * f2.Denominator);
		}

		public static Fraction operator *(Fraction f1, Fraction f2)
		{
			return new Fraction(f1.Numerator * f2.Numerator, f1.Denominator * f2.Denominator);
		}

		public static Fraction operator /(Fraction f1, Fraction f2)
		{
			if (f2.Denominator == 0)
				throw new DivideByZeroException();
			return new Fraction(f1.Numerator * f2.Denominator, f1.Denominator * f2.Numerator);
		}

		public static Fraction operator -(Fraction f1)
		{
			return new Fraction(-f1.Numerator, f1.Denominator);
		}

		public static bool operator ==(Fraction f1, Fraction f2)
		{
			return f1.Numerator == f2.Numerator && f1.Denominator == f2.Denominator;
		}

		public static bool operator !=(Fraction f1, Fraction f2)
		{
			return f1.Numerator != f2.Numerator || f1.Denominator != f2.Denominator;
		}

		public static bool operator >(Fraction f1, Fraction f2)
		{
			return f1.Numerator * f2.Denominator > f1.Denominator * f2.Numerator;
		}

		public static bool operator <(Fraction f1, Fraction f2)
		{
			return f1.Numerator * f2.Denominator < f1.Denominator * f2.Numerator;
		}

		public static bool operator >=(Fraction f1, Fraction f2)
		{
			return f1.Numerator * f2.Denominator >= f1.Denominator * f2.Numerator;
		}

		public static bool operator <=(Fraction f1, Fraction f2)
		{
			return f1.Numerator * f2.Denominator <= f1.Denominator * f2.Numerator;
		}

		public static explicit operator double(Fraction f)
		{
			return (double)f.Numerator / (double)f.Denominator;
		}

		public static explicit operator int(Fraction f)
		{
			return (int)(f.Numerator / f.Denominator);
		}

		public static explicit operator long(Fraction f)
		{
			return (long)(f.Numerator / f.Denominator);
		}

		public static explicit operator BigInteger(Fraction f)
		{
			return f.Numerator / f.Denominator;
		}

		public static Fraction Pow(Fraction f, int k)
		{
			return new Fraction(BigInteger.Pow(f.Numerator, k), BigInteger.Pow(f.Denominator, k));
		}

		public static Fraction Zero { get; } = new Fraction(0);
		public static Fraction One { get; } = new Fraction(1);
	}
}
