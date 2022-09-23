using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCord.Core.Algorithms.Shuffling
{
    // https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle
    public class FisherYatesListShuffle<T> : IShuffle<T>
    {
        private IList<T> _list;

        private readonly Random _random;

        public FisherYatesListShuffle(IList<T> list)
        { 
            _list = list;
            _random = new Random();
        }

        public IList<T> Shuffle()
        {
            int n = _list.Count;
            while (n > 1)
            {
                n--;
                int k = _random.Next(n + 1);
                T value = _list[k];
                _list[k] = _list[n];
                _list[n] = value;
            }

            return _list;
        }
    }
}
