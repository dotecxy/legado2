using System;
using System.Collections.Generic;
using System.Text;

namespace Legado.Core.Events
{
    public interface IMessenger
    {
        void Register<TMessage>(object recipient, object token, Action<TMessage> action) where TMessage : class;
        void Send<TMessage>(TMessage message, object token) where TMessage : class;
        void Unregister(object recipient); 

        
    }
}
