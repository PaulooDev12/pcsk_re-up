using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pcsk1.src.Events
{
    public interface IEventable
    {
        void Initialize();
        void Dispose();
    }
}
