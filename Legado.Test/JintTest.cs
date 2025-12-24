using Jint;
using Jint.Native.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Legado.Test
{
    internal class JintTest
    {
        [Test]
        public void TestJS()
        {

            var jsStr = File.ReadAllText("D:\\testb\\js.txt");
            Engine e = new Engine();
            var result = e.Evaluate(jsStr);
        }
    }
}
