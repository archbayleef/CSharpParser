using System;
using System.Numerics;

namespace CSharpParser
{
    internal struct Fraction : IComparable<Fraction>
    {
        public bool Equals(Fraction other)
        {
            return _numerator.Equals(other._numerator) && _denominator.Equals(other._denominator);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Fraction && Equals((Fraction) obj);
        }

        private BigInteger _numerator, _denominator;

        public Fraction(int nom, int denom = 1)
            : this((BigInteger) nom, denom)
        {
        }

        public Fraction(long nom, long denom = 1)
            : this((BigInteger) nom, denom)
        {
        }

        public Fraction(BigInteger nom, BigInteger denom)
        {
            _numerator = nom;
            _denominator = denom;
            Simplify();
        }

        public BigInteger Numerator
        {
            get { return _numerator; }
        }

        public BigInteger Denominator
        {
            get { return _denominator; }
        }

        public static implicit operator Fraction(int value)
        {
            return new Fraction(value);
        }

        public static implicit operator Fraction(long value)
        {
            return new Fraction(value);
        }

        private void Simplify()
        {
            var g = BigInteger.GreatestCommonDivisor(_numerator, _denominator);
            _numerator /= g;
            _denominator /= g;
            if (_denominator >= 0) return;
            _numerator = -_numerator;
            _denominator = -_denominator;
        }

        public int CompareTo(Fraction other)
        {
            var f = this - other;
            return f._numerator > 0 ? 1 : f._numerator < 0 ? -1 : 0;
        }

        public override string ToString()
        {
            return _numerator + "/" + _denominator;
        }

        public static Fraction operator +(Fraction f1, Fraction f2)
        {
            var f = new Fraction(f1._numerator*f2._denominator + f1._denominator*f2._numerator,
                f1._denominator*f2._denominator);
            f.Simplify();
            return f;
        }

        public static Fraction operator -(Fraction f1, Fraction f2)
        {
            var f = new Fraction(f1._numerator*f2._denominator - f1._denominator*f2._numerator,
                f1._denominator*f2._denominator);
            f.Simplify();
            return f;
        }

        public static Fraction operator *(Fraction f1, Fraction f2)
        {
            var f = new Fraction(f1._numerator*f2._numerator, f1._denominator*f2._denominator);
            f.Simplify();
            return f;
        }

        public static Fraction operator /(Fraction f1, Fraction f2)
        {
            if (f2._denominator == 0)
                throw new DivideByZeroException();
            var f = new Fraction(f1._numerator*f2._denominator, f1._denominator*f2._numerator);
            f.Simplify();
            return f;
        }

        public static Fraction operator -(Fraction f1)
        {
            return new Fraction(-f1._numerator, f1._denominator);
        }

        public static bool operator ==(Fraction f1, Fraction f2)
        {
            return f1._numerator == f2._numerator && f1._denominator == f2._denominator;
        }

        public static bool operator !=(Fraction f1, Fraction f2)
        {
            return f1._numerator != f2._numerator || f1._denominator != f2._denominator;
        }

        public static bool operator >(Fraction f1, Fraction f2)
        {
            return f1._numerator*f2._denominator > f1._denominator*f2._numerator;
        }

        public static bool operator <(Fraction f1, Fraction f2)
        {
            return f1._numerator*f2._denominator < f1._denominator*f2._numerator;
        }

        public static bool operator >=(Fraction f1, Fraction f2)
        {
            return f1._numerator*f2._denominator >= f1._denominator*f2._numerator;
        }

        public static bool operator <=(Fraction f1, Fraction f2)
        {
            return f1._numerator*f2._denominator <= f1._denominator*f2._numerator;
        }

        public static explicit operator double(Fraction f)
        {
            return (double) f._numerator/(double) f._denominator;
        }

        public static explicit operator int(Fraction f)
        {
            return (int) (f._numerator/f._denominator);
        }

        public static explicit operator long(Fraction f)
        {
            return (long) (f._numerator/f._denominator);
        }

        public static explicit operator BigInteger(Fraction f)
        {
            return f._numerator/f._denominator;
        }

        public static Fraction Pow(Fraction f, int k)
        {
            return new Fraction(BigInteger.Pow(f._numerator, k), BigInteger.Pow(f._denominator, k));
        }
    }
}