using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCord.Bot.Workers
{
    public interface IWorker
    {
        public Task Run();

        public void Stop();
    }
}
