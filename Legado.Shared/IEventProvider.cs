using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace Legado.Shared
{
    public interface IEventProvider
    {

        void DragMove(int x, int y);

       ResourceManager GetResourceManager( );
    }
}
