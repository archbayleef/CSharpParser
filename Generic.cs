using System;
using System.Collections.Generic;

namespace CSharpParser
{
    public class PriorityQueue<T>
    {
        private readonly List<T> _data;
        private readonly Comparison<T> _compare;

        public PriorityQueue()
            : this(Comparer<T>.Default.Compare)
        {
        }

        public PriorityQueue(Comparison<T> compare)
        {
            _compare = compare;
            _data = new List<T> {default(T)};
        }

        public int Count
        {
            get { return _data.Count - 1; }
        }

        public bool Empty()
        {
            return Count == 0;
        }

        public T Top()
        {
            return _data[1];
        }

        public void Push(T item)
        {
            _data.Add(item);
            var curPlace = Count;
            while (curPlace > 1 && _compare(item, _data[curPlace/2]) > 0)
            {
                _data[curPlace] = _data[curPlace/2];
                _data[curPlace/2] = item;
                curPlace /= 2;
            }
        }

        public void Pop()
        {
            _data[1] = _data[Count];
            _data.RemoveAt(Count);
            var curPlace = 1;
            while (true)
            {
                var max = curPlace;
                if (Count >= curPlace*2 && _compare(_data[max], _data[2*curPlace]) < 0) max = 2*curPlace;
                if (Count >= curPlace*2 + 1 && _compare(_data[max], _data[2*curPlace + 1]) < 0) max = 2*curPlace + 1;
                if (max == curPlace) break;
                var item = _data[max];
                _data[max] = _data[curPlace];
                _data[curPlace] = item;
                curPlace = max;
            }
        }
    }

    public class SegmentTree<T>
    {
        public delegate T UniFunction(T t1, T t2);

        public delegate bool BoundFunction(T t);

        private readonly UniFunction _union;
        private readonly T[] _tree;
        private readonly T _zero;

        public SegmentTree(IList<T> a, UniFunction f, T zero)
        {
            var p = 1;
            while (p < a.Count) p *= 2;
            _tree = new T[2*p];
            for (var i = 0; i < a.Count; i++) _tree[i + p] = a[i];
            for (var i = a.Count; i < p; i++) _tree[i + p] = zero;
            _union = f;
            _zero = zero;
            for (var i = p - 1; i > 0; i--) _tree[i] = _union(_tree[2*i], _tree[2*i + 1]);
        }

        public SegmentTree(int n, UniFunction f, T zero)
        {
            var p = 1;
            while (p < n) p *= 2;
            _tree = new T[2*p];
            for (var i = 0; i < p; i++) _tree[i + p] = zero;
            _union = f;
            _zero = zero;
            for (var i = p - 1; i > 0; i--) _tree[i] = _union(_tree[2*i], _tree[2*i + 1]);
        }

        public void Set(int index, T value)
        {
            index += _tree.Length/2;
            for (_tree[index] = value, index /= 2; index > 0; index /= 2)
                _tree[index] = _union(_tree[2*index], _tree[2*index + 1]);
        }

        public T Get(int left, int right)
        {
            left += _tree.Length/2;
            right += _tree.Length/2;
            var ansLeft = _zero;
            var ansRight = _zero;
            while (left <= right)
            {
                if (left%2 != 0) ansLeft = _union(ansLeft, _tree[left++]);
                if (right%2 == 0) ansRight = _union(_tree[right--], ansRight);
                left /= 2;
                right /= 2;
            }
            return _union(ansLeft, ansRight);
        }

        public int Leftmost(int leftBound, BoundFunction function)
        {
            if (leftBound < 0 || leftBound >= _tree.Length/2) throw new ArgumentOutOfRangeException();
            leftBound += _tree.Length/2;
            var right = _tree.Length - 1;
            var ans = _zero;
            T cand;
            while (leftBound <= right)
            {
                if (leftBound%2 != 0)
                {
                    cand = _union(ans, _tree[leftBound]);
                    if (function(cand)) break;
                    ans = cand;
                    leftBound++;
                }
                leftBound /= 2;
                right /= 2;
            }
            if (leftBound > right) return _tree.Length/2;
            while (2*leftBound < _tree.Length)
            {
                cand = _union(ans, _tree[2*leftBound]);
                if (function(cand)) leftBound *= 2;
                else
                {
                    ans = cand;
                    leftBound = 2*leftBound + 1;
                }
            }
            return leftBound - _tree.Length/2;
        }

        public int Rightmost(int rightBound, BoundFunction function)
        {
            if (rightBound < 0 || rightBound >= _tree.Length/2) throw new ArgumentOutOfRangeException();
            rightBound += _tree.Length/2;
            var left = _tree.Length/2;
            var ans = _zero;
            T cand;
            while (left <= rightBound)
            {
                if (rightBound%2 == 0)
                {
                    cand = _union(_tree[rightBound], ans);
                    if (function(cand)) break;
                    ans = cand;
                    rightBound--;
                }
                left /= 2;
                rightBound /= 2;
            }
            if (left > rightBound) return -1;
            while (2*rightBound < _tree.Length)
            {
                cand = _union(_tree[2*rightBound + 1], ans);
                if (function(cand)) rightBound = 2*rightBound + 1;
                else
                {
                    ans = cand;
                    rightBound *= 2;
                }
            }
            return rightBound - _tree.Length/2;
        }
    }

    public class Set<T> : IEnumerable<T>
    {
        public interface IBothSidesEnumerator<out T1> : IEnumerator<T1>
        {
            bool IsEmpty { get; }
            bool MovePrev();
        }

        private Node _root;
        private readonly Comparison<T> _compare;

        public Set()
            : this(Comparer<T>.Default.Compare)
        {
        }

        public Set(Comparison<T> compare)
        {
            _compare = compare;
            _root = null;
        }

        public int Count
        {
            get { return _root == null ? 0 : _root.Count; }
        }

        public IBothSidesEnumerator<T> GetBothSidesEnumerator()
        {
            return new Enumerator(_root);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return GetBothSidesEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetBothSidesEnumerator();
        }

        public void Add(T toAdd)
        {
            Node parent = null;
            var node = _root;
            if (!Find(toAdd).IsEmpty)
            {
                while (_compare(node.Data, toAdd) != 0) node = _compare(node.Data, toAdd) > 0 ? node.Left : node.Right;
                node.Data = toAdd;
                return;
            }
            while (node != null)
            {
                parent = node;
                parent.Count++;
                node = _compare(node.Data, toAdd) > 0 ? node.Left : node.Right;
            }
            node = new Node();
            node.Left = node.Right = null;
            node.Parent = parent;
            node.Level = node.Count = 1;
            node.Data = toAdd;
            if (parent != null)
                if (_compare(node.Data, parent.Data) < 0) parent.Left = node;
                else parent.Right = node;
            while (node != null)
            {
                Skew(ref node);
                Split(ref node);
                if (parent != null) parent = parent.Parent;
                else _root = node;
                node = node.Parent;
            }
        }

        public void Clear()
        {
            _root = null;
        }

        public IBothSidesEnumerator<T> Find(T toFind)
        {
            var node = _root;
            while (node != null)
            {
                var compareResult = _compare(node.Data, toFind);
                if (compareResult == 0) return new Enumerator(_root, node);
                node = compareResult < 0 ? node.Right : node.Left;
            }
            return new Enumerator(_root);
        }

        public int LessThan(T toFind)
        {
            var node = _root;
            var ans = 0;
            while (node != null)
            {
                var compareResult = _compare(node.Data, toFind);
                if (compareResult == 0) return ans + (node.Left == null ? 0 : node.Left.Count);
                if (compareResult < 0)
                    ans += 1 + (node.Left == null ? 0 : node.Left.Count);
                node = compareResult < 0 ? node.Right : node.Left;
            }
            return ans;
        }

        public IBothSidesEnumerator<T> LowerBound(T toFind)
        {
            var node = _root;
            Node lastLeft = null;
            while (node != null)
            {
                var compareResult = _compare(node.Data, toFind);
                if (compareResult == 0) return new Enumerator(_root, node);
                if (compareResult < 0) node = node.Right;
                else
                {
                    lastLeft = node;
                    node = node.Left;
                }
            }
            return new Enumerator(_root, lastLeft);
        }

        public IBothSidesEnumerator<T> NthElement(int n)
        {
            var node = _root;
            while (node != null)
            {
                if (node.Left != null)
                    if (node.Left.Count > n)
                    {
                        node = node.Left;
                        continue;
                    }
                    else n -= node.Left.Count;
                if (n == 0) return new Enumerator(_root, node);
                n--;
                node = node.Right;
            }
            return new Enumerator(_root);
        }

        public IBothSidesEnumerator<T> UpperBound(T toFind)
        {
            var node = _root;
            Node lastLeft = null;
            while (node != null)
            {
                var compareResult = _compare(node.Data, toFind);
                if (compareResult == 0)
                {
                    var ans = new Enumerator(_root, node);
                    ans.MoveNext();
                    return ans;
                }
                if (compareResult < 0) node = node.Right;
                else
                {
                    lastLeft = node;
                    node = node.Left;
                }
            }
            return new Enumerator(_root, lastLeft);
        }

        public bool Remove(T toDelete)
        {
            var node = _root;
            if (Find(toDelete).IsEmpty) return false;
            Node last = null;
            while (node != null)
            {
                node.Count--;
                var compareResult = _compare(toDelete, node.Data);
                if (compareResult < 0) node = node.Left;
                else if (compareResult > 0) node = node.Right;
                else
                {
                    if (node.Left == null && node.Right == null)
                    {
                        last = node;
                        break;
                    }
                    IBothSidesEnumerator<T> it = new Enumerator(node, node);
                    if (it.MoveNext())
                    {
                        node.Data = it.Current;
                        toDelete = it.Current;
                        node = node.Right;
                    }
                    else
                    {
                        it = new Enumerator(node, node);
                        it.MovePrev();
                        node.Data = it.Current;
                        toDelete = it.Current;
                        node = node.Left;
                    }
                }
            }
            if (last == null) return false;
            node = last.Parent;
            if (node == null)
            {
                _root = null;
                return true;
            }
            last.Parent = null;
            if (node.Left == last) node.Left = null;
            else node.Right = null;
            while (node != null)
            {
                if (node.Left != null && node.Left.Level < node.Level - 1 ||
                    node.Right != null && node.Right.Level < node.Level - 1)
                {
                    node.Level--;
                    if (node.Right != null && node.Right.Level > node.Level) node.Right.Level = node.Level;
                    Skew(ref node);
                    if (node.Right != null)
                    {
                        Skew(ref node.Right);
                        if (node.Right.Right != null) Skew(ref node.Right.Right);
                    }
                    Split(ref node);
                    if (node.Right != null) Split(ref node.Right);
                }
                _root = node;
                node = node.Parent;
            }
            return true;
        }

        private static void UpdateCount(Node node)
        {
            if (node == null) return;
            node.Count = 1;
            if (node.Left != null) node.Count += node.Left.Count;
            if (node.Right != null) node.Count += node.Right.Count;
        }

        private static void Skew(ref Node node)
        {
            var left = node.Left;
            if (left == null || left.Level != node.Level) return;
            Algorithm.Swap(ref node, ref left);
            if (node.Right != null) node.Right.Parent = left;
            left.Left = node.Right;
            node.Right = left;
            if (left.Parent != null)
                if (left.Parent.Left == left) left.Parent.Left = node;
                else left.Parent.Right = node;
            node.Parent = left.Parent;
            left.Parent = node;
            UpdateCount(left);
            UpdateCount(node);
        }

        private static void Split(ref Node node)
        {
            var right = node.Right;
            if (right == null || right.Right == null || right.Right.Level != node.Level) return;
            Algorithm.Swap(ref node, ref right);
            if (node.Left != null) node.Left.Parent = right;
            right.Right = node.Left;
            node.Left = right;
            if (right.Parent != null)
                if (right.Parent.Left == right) right.Parent.Left = node;
                else right.Parent.Right = node;
            node.Parent = right.Parent;
            right.Parent = node;
            node.Level++;
            UpdateCount(right);
            UpdateCount(node);
        }

        internal class Node
        {
            public Node Left, Right, Parent;
            public int Level, Count;
            public T Data;
        }

        public struct Enumerator : IBothSidesEnumerator<T>
        {
            private Node _curr;
            private readonly Node _root;

            internal Enumerator(Node root, Node initCurr = null)
            {
                _curr = initCurr;
                _root = root;
            }

            object System.Collections.IEnumerator.Current
            {
                get { return _curr.Data; }
            }

            T IEnumerator<T>.Current
            {
                get { return _curr.Data; }
            }

            public bool MoveNext()
            {
                if (_root == null) return false;
                if (_curr == null)
                {
                    _curr = _root;
                    while (_curr.Left != null) _curr = _curr.Left;
                    return true;
                }
                if (_curr.Right != null)
                {
                    _curr = _curr.Right;
                    while (_curr.Left != null) _curr = _curr.Left;
                    return true;
                }
                while (_curr != _root && _curr.Parent.Left != _curr) _curr = _curr.Parent;
                if (_curr == _root)
                {
                    _curr = null;
                    return false;
                }
                _curr = _curr.Parent;
                return true;
            }

            public bool MovePrev()
            {
                if (_root == null) return false;
                if (_curr == null)
                {
                    _curr = _root;
                    while (_curr.Right != null) _curr = _curr.Right;
                    return true;
                }
                if (_curr.Left != null)
                {
                    _curr = _curr.Left;
                    while (_curr.Right != null) _curr = _curr.Right;
                    return true;
                }
                while (_curr != _root && _curr.Parent.Right != _curr) _curr = _curr.Parent;
                if (_curr == _root)
                {
                    _curr = null;
                    return false;
                }
                _curr = _curr.Parent;
                return true;
            }

            public void Reset()
            {
                _curr = null;
            }

            public void Dispose()
            {
            }

            public bool IsEmpty
            {
                get { return _curr == null; }
            }
        }
    }

    public class Map<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly Set<KeyValuePair<TKey, TValue>> _set;

        public Map()
            : this(Comparer<TKey>.Default.Compare)
        {
        }

        public Map(Comparison<TKey> compare)
        {
            _set = new Set<KeyValuePair<TKey, TValue>>((x, y) => compare(x.Key, y.Key));
        }

        public void Add(TKey key, TValue value)
        {
            _set.Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        public bool ContainsKey(TKey key)
        {
            return !_set.Find(new KeyValuePair<TKey, TValue>(key, default(TValue))).IsEmpty;
        }

        public ICollection<TKey> Keys
        {
            get
            {
                var list = new List<TKey>();
                for (var iterator = GetEnumerator(); iterator.MoveNext();)
                    list.Add(iterator.Current.Key);
                return list;
            }
        }

        public bool Remove(TKey key)
        {
            return _set.Remove(new KeyValuePair<TKey, TValue>(key, default(TValue)));
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var iterator = _set.Find(new KeyValuePair<TKey, TValue>(key, default(TValue)));
            if (iterator.IsEmpty)
            {
                value = default(TValue);
                return false;
            }
            value = iterator.Current.Value;
            return true;
        }

        public ICollection<TValue> Values
        {
            get
            {
                var list = new List<TValue>();
                for (var iterator = GetEnumerator(); iterator.MoveNext();)
                    list.Add(iterator.Current.Value);
                return list;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                var iterator = _set.Find(new KeyValuePair<TKey, TValue>(key, default(TValue)));
                return iterator.IsEmpty ? default(TValue) : iterator.Current.Value;
            }
            set { _set.Add(new KeyValuePair<TKey, TValue>(key, value)); }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            _set.Add(item);
        }

        public void Clear()
        {
            _set.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            var iterator = _set.Find(item);
            return !iterator.IsEmpty && iterator.Current.Value.Equals(item.Value);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            var iterator = GetEnumerator();
            for (; iterator.MoveNext(); arrayIndex++)
                array[arrayIndex] = iterator.Current;
        }

        public int Count
        {
            get { return _set.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Contains(item) && _set.Remove(item);
        }

        public Set<KeyValuePair<TKey, TValue>>.IBothSidesEnumerator<KeyValuePair<TKey, TValue>> GetBothSidesEnumerator()
        {
            return _set.GetBothSidesEnumerator();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _set.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _set.GetEnumerator();
        }

        public Set<KeyValuePair<TKey, TValue>>.IBothSidesEnumerator<KeyValuePair<TKey, TValue>> LowerBound(TKey key)
        {
            return _set.LowerBound(new KeyValuePair<TKey, TValue>(key, default(TValue)));
        }

        public Set<KeyValuePair<TKey, TValue>>.IBothSidesEnumerator<KeyValuePair<TKey, TValue>> UpperBound(TKey key)
        {
            return _set.UpperBound(new KeyValuePair<TKey, TValue>(key, default(TValue)));
        }

        public Set<KeyValuePair<TKey, TValue>>.IBothSidesEnumerator<KeyValuePair<TKey, TValue>> NthElement(int n)
        {
            return _set.NthElement(n);
        }
    }

    public class Matrix
    {
        private readonly int[] _a;

        public Matrix(int size, int mod)
        {
            _a = new int[size*size];
            Mod = mod;
            Size = size;
        }

        public Matrix(int[,] a, int mod)
        {
            if (a.GetLength(0) != a.GetLength(1)) throw new ArgumentException();
            Size = a.GetLength(0);
            Mod = mod;
            _a = new int[Size*Size];
            for (var i = 0; i < Size; i++)
                for (var j = 0; j < Size; j++)
                    _a[i*Size + j] = a[i, j];
        }

        public static Matrix operator +(Matrix m1, Matrix m2)
        {
            if (m1.Size != m2.Size || m1.Mod != m2.Mod) throw new InvalidOperationException();
            var m = new Matrix(m1.Size, m1.Mod);
            for (var i = 0; i < m.Size*m.Size; i++)
                m._a[i] = (m1._a[i] + m2._a[i])%m.Mod;
            return m;
        }

        public static Matrix operator -(Matrix m1, Matrix m2)
        {
            if (m1.Size != m2.Size || m1.Mod != m2.Mod) throw new InvalidOperationException();
            var m = new Matrix(m1.Size, m1.Mod);
            for (var i = 0; i < m.Size*m.Size; i++)
                m._a[i] = (m1._a[i] - m2._a[i] + m.Mod)%m.Mod;
            return m;
        }

        public static Matrix operator *(Matrix m1, Matrix m2)
        {
            if (m1.Size != m2.Size || m1.Mod != m2.Mod) throw new InvalidOperationException();
            var m = new Matrix(m1.Size, m1.Mod);
            for (var i = 0; i < m.Size*m.Size; i++)
            {
                var temp = 0L;
                for (var k = 0; k < m.Size; k++)
                {
                    temp += (long) m1._a[i - i%m.Size + k]*m2._a[k*m.Size + i%m.Size];
                    if ((k & 15) == 15) temp %= m.Mod;
                }
                m._a[i] = (int) (temp%m.Mod);
            }
            return m;
        }

        public static Matrix Pow(Matrix m, long k)
        {
            var ans = One(m.Size, m.Mod);
            while (k != 0)
            {
                if (k%2 == 1) ans = ans*m;
                m = m*m;
                k /= 2;
            }
            return ans;
        }

        public static Matrix One(int size, int mod)
        {
            var m = new Matrix(size, mod);
            for (var i = 0; i < size; i++)
                m._a[i*(size + 1)] = 1;
            return m;
        }

        public static Matrix Zero(int size, int mod)
        {
            return new Matrix(size, mod);
        }

        public static int[] operator *(int[] vector, Matrix m)
        {
            if (vector.Length != m.Size) throw new ArgumentException();
            var result = new int[m.Size];
            for (var j = 0; j < m.Size; j++)
            {
                var temp = 0L;
                for (var k = 0; k < m.Size; k++)
                {
                    temp += (long) vector[k]*m._a[k*m.Size + j];
                    if ((k & 15) == 15)
                        temp %= m.Mod;
                }
                result[j] = (int) (temp%m.Mod);
            }
            return result;
        }

        public int Mod { get; private set; }

        public int Size { get; private set; }
    }
}
