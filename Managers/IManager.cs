using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityCollectorPlugin.Managers
{
    public interface IManager
    {
        bool IsInitialized { get; }
        void Run();
    }
}
