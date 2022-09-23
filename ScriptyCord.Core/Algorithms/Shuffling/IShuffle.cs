using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCord.Core.Algorithms.Shuffling
{
    public interface IShuffle<T>
    {
        IList<T> Shuffle();
    }
}
