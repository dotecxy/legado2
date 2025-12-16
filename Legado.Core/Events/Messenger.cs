using CommunityToolkit.Mvvm.Messaging;
using Legado.Core.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;


namespace Legado.Core.Events
{
    public class Messenger : IMessenger
    {
        private static Lazy<Messenger> g_messenger = new Lazy<Messenger>(() => new Messenger());
        public static Messenger Default => g_messenger.Value;

        readonly ConcurrentDictionary<object, (INormalRecipient, NormalToken)> _recipientMaps = new ConcurrentDictionary<object, (INormalRecipient, NormalToken)>();

        private interface INormalRecipient
        {
        }

        private class NormalRecipient<T> : INormalRecipient, IRecipient<T> where T : class
        {
            private readonly WeakAction<T> _action;
            public NormalRecipient(WeakAction<T> weakAction) { _action = weakAction; }

            public void Receive(T message)
            {
                _action?.Execute(message);
            }
        }

        private readonly struct NormalToken : IEquatable<NormalToken>
        {
            public readonly object _token;

            public NormalToken(object token)
            {
                _token = token;
            }

            public bool Equals(NormalToken other)
            {
                return this.ToString() == other.ToString();
            }

            public override bool Equals(object obj)
            {
                return ToString().Equals(ToString());
            }

            public override int GetHashCode()
            {
                return ToString().GetHashCode();
            }

            public override string ToString()
            {
                return _token?.ToString() ?? "";
            }
        }

        public void Register<TMessage>(object recipient, object token, Action<TMessage> action) where TMessage : class
        {
            var recipient2 = new Messenger.NormalRecipient<TMessage>(new WeakAction<TMessage>(recipient, action));
            var token2 = new NormalToken(token);
            var vt = (recipient2, token2);
            WeakReferenceMessenger.Default.Register<TMessage, NormalToken>(recipient2, token2);
            _recipientMaps.AddOrUpdate(recipient, vt, (_, _) => vt);
        }


        public void Send<TMessage>(TMessage message, object token) where TMessage : class
        {
            WeakReferenceMessenger.Default.Send<TMessage, NormalToken>(message, new NormalToken(token));
        }

        public void Unregister(object recipient)
        {
            if (_recipientMaps.TryGetValue(recipient, out var vt))
            {
                WeakReferenceMessenger.Default.UnregisterAll(vt.Item1, vt.Item2);
            }
        }

    }
}
