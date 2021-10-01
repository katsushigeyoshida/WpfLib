using System;
using System.Collections.Generic;

namespace WpfLib
{
    /// <summary>
    /// C#でのPriority Queue（優先キュー）実装
    /// yambe2002.hatenablog.com/entry/2015/11/01/114503
    /// 二分ヒープを使った実装で、ほとんど蟻本(プログラミングコンテストチャレンジブック)のソースコードそのまま。
    /// ただし、次の拡張を行っている。
    /// ・ジェネリック型に対応
    /// ・昇順、降順の指定可
    /// ・カスタムIComparerを指定可
    /// ・Peek()、Count()、Clear()、ToArray()を追加
    /// さらに下記追加
    /// ・ヒープ領域の自動拡張
    /// 
    /// ヒープは完全2分木でこれを1次元配列で表している
    /// 1次元配列では配列を1から開始し最上位の親とするがスタートアドレスを0とした場合、アドレス計算は+1しておこなう
    /// このアドレスは自分のアドレスの2倍と2倍+1となる
    /// 親のアドレスは自分の値手の半分の切捨てとなる
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PriorityQueue<T> where T : IComparable
    {
        private IComparer<T> _comparer = null;
        private int _type = 0;

        private T[] _heap;
        private int _sz = 0;
        private int _maxSize;
        private int _originalSize;

        private int _count = 0;

        /// <summary>
        /// コンストラクタ
        /// Priority Queue with custom comparer
        /// </summary>
        /// <param name="maxSize"></param>
        /// <param name="comparer"></param>
        public PriorityQueue(int maxSize, IComparer<T> comparer)
        {
            _maxSize = maxSize;
            _originalSize = maxSize;
            _heap = new T[maxSize];
            _comparer = comparer;
        }

        /// <summary>
        /// コンストラクタ
        /// Priority queue
        /// </summary>
        /// <param name="maxSize">max size</param>
        /// <param name="type">0:asc(昇順), 1:desc(降順)</param>
        public PriorityQueue(int maxSize, int type = 0)
        {
            _maxSize = maxSize;
            _originalSize = maxSize;
            _heap = new T[maxSize];
            _type = type;
        }

        /// <summary>
        /// 比較処理
        /// _comparerが設定されていればその関数使う
        /// 設定されていなければ対象オブジェクトの比較関数(CompareTo()を使う(IComparableインタフェース)
        /// </summary>
        /// <param name="x">オブジェクトx</param>
        /// <param name="y">オブジェクトy</param>
        /// <returns>比較結果</returns>
        private int Compare(T x, T y)
        {
            if (_comparer != null)
                return _comparer.Compare(x, y);
            else
                return _type == 0 ? x.CompareTo(y) : y.CompareTo(x);
        }

        /// <summary>
        /// リストへ追加
        /// 要素の挿入はヒープの最後に追加し、その親より大きい場合は順次入れ替えていく
        /// </summary>
        /// <param name="x">オブジェクト</param>
        public void Push(T x)
        {
            //  ヒープ領域(配列)の拡張
            if (_maxSize <= _count) {
                _maxSize += _originalSize;
                Array.Resize<T>(ref _heap, _maxSize);
            }

            _count++;
            //  node number
            var i = _sz++;

            while (i > 0) {
                //  parent node number
                var p = (i - 1) / 2;
                if (Compare(_heap[p], x) <= 0)  //  _heap[p] < x break;
                    break;
                _heap[i] = _heap[p];
                i = p;
            }
            _heap[i] = x;
        }

        /// <summary>
        /// リストからの取り出し(取出した後そのオブジェクトは削除される)
        /// 取り出しは設定された優先順位の高いものから取り出す
        /// 最上位が取り出し対象でそこに最下位を入れて子と比較し子が小さければ順次入れ替えていく
        /// ともに小さければ小さい方と入れ替えていく
        /// </summary>
        /// <returns>オブジェクト</returns>
        public T Pop()
        {
            _count--;

            T ret = _heap[0];
            T x = _heap[--_sz];

            int i = 0;
            while (i * 2 + 1 < _sz) {
                //  children
                int a = i * 2 + 1;
                int b = i * 2 + 2;

                if (b < _sz && Compare(_heap[b], _heap[a]) < 0)
                    a = b;
                if (Compare(_heap[a], x) >= 0)
                    break;

                _heap[i] = _heap[a];
                i = a;
            }
            _heap[i] = x;

            return ret;
        }

        /// <summary>
        /// リストに登録されているオブジェクトの数
        /// </summary>
        /// <returns></returns>
        public int Count()
        {
            return _count;
        }

        /// <summary>
        /// オブジェクトの参照(リストから削除されない)
        /// </summary>
        /// <returns>オブジェクト</returns>
        public T Peek()
        {
            return _heap[0];
        }

        /// <summary>
        /// リストにオブジェクトがあるかを確認する
        /// </summary>
        /// <param name="x">オブジェクト</param>
        /// <returns>オブジェクトの有無</returns>
        public bool Contains(T x)
        {
            for (int i = 0; i < _sz; i++)
                if (x.Equals(_heap[i]))
                    return true;
            return false;
        }

        /// <summary>
        /// リストからすべてのオブジェクトを削除する
        /// </summary>
        public void Clear()
        {
            while (this.Count() > 0)
                this.Pop();
        }

        /// <summary>
        /// foreach の in の反復処理に対応
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            var ret = new List<T>();

            while (this.Count() > 0) {
                ret.Add(this.Pop());
            }

            foreach (var r in ret) {
                this.Push(r);
                yield return r;
            }
        }

        /// <summary>
        /// 配列に変換する
        /// </summary>
        /// <returns></returns>
        public T[] ToArray()
        {
            T[] array = new T[_sz];
            int i = 0;

            foreach (var r in this) {
                array[i++] = r;
            }

            return array;
        }
    }
}
