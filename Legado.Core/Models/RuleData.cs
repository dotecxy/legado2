using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Legado.Core.Models
{
    public class RuleData: IRuleData
    { 
        protected ConcurrentDictionary<string, string> variableMap { get; private set; } = new ConcurrentDictionary<string, string>();


        public void putBigVariable(string key, string value)
        {
            variableMap.AddOrUpdate(key, value, (_, _) => value);
        }
        public string getBigVariable(string key)
        {
            if (variableMap.TryGetValue(key, out var value))
            {
                return value;
            }
            return null;
        }

        public string getVariable()
        {
            if (variableMap.IsEmpty)
            {
                return null;
            }

            return JsonConvert.SerializeObject(variableMap);
        }
    }


    public interface IRuleData
    {
        void putBigVariable(string key, string value);
        string getBigVariable(string key);
        string getVariable();
    }
}
