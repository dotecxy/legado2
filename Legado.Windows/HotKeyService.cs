using Legado.Core;
using Legado.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using Serilog.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanara.PInvoke;

namespace Legado.Windows
{
    [SingletonDependency]
    [ExposeServices(typeof(IHotKeyService))]
    public class HotKeyService : IHotKeyService, IDisposable
    {

        private readonly Form1 _form;
        private readonly INativeAudioService _audio;
        private readonly ILogger _logger;
        private AsyncLock _mutex = new AsyncLock();
        private bool _initialization;

        private int _id = 1;

        public int ID
        {
            get
            {
                return Interlocked.Increment(ref _id);
            }
        }


        private readonly ConcurrentDictionary<int, (nint, Action)> _registerHotKetRecords = new ConcurrentDictionary<int, (nint, Action)>();

        public HotKeyService(ILogger<HotKeyService> logger, Form1 form, INativeAudioService audioService)
        {
            _form = form;
            _audio = audioService;
            _logger = logger;
        }

        public async Task ApplyAsync()
        {
            using var _ = await _mutex.LockAsync();

            if (_initialization)
            {
                return;
            }
            _initialization = true;
            await Task.Delay(10);
            var id = ID;
            var hWnd = _form.Handle;
            if (User32.RegisterHotKey(hWnd, id, User32.HotKeyModifiers.MOD_ALT | User32.HotKeyModifiers.MOD_CONTROL, (uint)User32.VK.VK_SPACE))
            {
                var action = new Action(async () =>
                {


                    if (_audio.IsPlaying)
                    {
                        await _audio.PauseAsync();
                    }
                    else
                    {
                        await _audio.PlayAsync(_audio.CurrentPositionMillisecond);
                    }
                });
                _registerHotKetRecords.AddOrUpdate(id, (hWnd, action), (_, _) => (hWnd, action));
            }
            else
            {
                _logger.LogError($"注册{User32.VK.VK_SPACE.ToString()}热键失败!");
            }

            if (_form is IMessageProc recv)
            {
                recv.OnMessage += Recv_OnMessage;
            }
        }

        public void Dispose()
        {
            if (_form is IMessageProc recv)
            {
                recv.OnMessage -= Recv_OnMessage;
            }
            if (_registerHotKetRecords.Any())
            {
                var keys = _registerHotKetRecords.Keys;
                foreach (var item in keys)
                {
                    if (_registerHotKetRecords.TryGetValue(item, out var item2))
                    {
                        User32.UnregisterHotKey(item2.Item1, item);
                    }
                }
            }
        }

        private void Recv_OnMessage(object? sender, Message e)
        {

            if (e.Msg == 0x0312)
            {
                int hotkeyId = e.WParam.ToInt32();
                if (_registerHotKetRecords.TryGetValue(hotkeyId, out var item))
                {
                    try
                    {
                        item.Item2.Invoke();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[Recv_OnMessage]");
                    }
                }
            }
        }
    }

    public interface IMessageProc
    {
        event EventHandler<Message> OnMessage;
    }
}
