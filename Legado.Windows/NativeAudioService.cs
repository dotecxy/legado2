using AutoMapper;
using Legado.Core;
using Legado.Shared;
using NAudio.Utils;
using NAudio.Wave;

namespace Legado.Windows;


[SingletonDependency]
[ExposeServices(typeof(INativeAudioService))]
public class NativeAudioService : INativeAudioService
{
    string _uri;
    MediaFoundationReader reader;
    WasapiOut wasapiOut;

    public bool IsPlaying => wasapiOut != null
        && wasapiOut.PlaybackState == PlaybackState.Playing;

    public double CurrentPositionMillisecond => wasapiOut?.GetPositionTimeSpan().TotalMicroseconds ?? 0;
    public double CurrentDurationMillisecond => reader?.TotalTime.TotalMicroseconds ?? 0;
    public event EventHandler<bool> IsPlayingChanged;
    public event EventHandler PlayFinished;
    public event EventHandler PlayFailed;
    public event EventHandler Played;
    public event EventHandler Paused;
    public event EventHandler Stopped;
    public event EventHandler SkipToNext;
    public event EventHandler SkipToPrevious;

    public NativeAudioService()
    {

    }

    private async Task CreateOutAsync()
    {
        await DisposeAsync();
        wasapiOut = new WasapiOut();
        wasapiOut.PlaybackStopped += (_, _) =>
        {
            if (reader.Length - reader.Position < 10000)
            {
                PlayFinished?.Invoke(this, EventArgs.Empty);
            }
            IsPlayingChanged?.Invoke(this, IsPlaying);
        };
        //mediaPlayer.MediaEnded += (_, _) => PlayFinished?.Invoke(this, EventArgs.Empty);
        //mediaPlayer.MediaFailed += (_, _) => PlayFailed?.Invoke(this, EventArgs.Empty);
    }

    public async Task InitializeAsync(string audioURI)
    {
        await CreateOutAsync();
        reader = new MediaFoundationReader(audioURI);
        wasapiOut.Init(reader);
        IsPlayingChanged?.Invoke(this, IsPlaying);
    }

    public Task PauseAsync()
    {
        wasapiOut?.Pause();
        IsPlayingChanged?.Invoke(this, this.IsPlaying);
        return Task.CompletedTask;
    }

    public Task PlayAsync(double positionMillisecond = 0)
    {
        if (wasapiOut != null && reader != null)
        {
            var oldState = this.IsPlaying;
            reader.CurrentTime = TimeSpan.FromMilliseconds(positionMillisecond);
            wasapiOut.Play();
            if (oldState != this.IsPlaying)
            {
                IsPlayingChanged?.Invoke(this, this.IsPlaying);
            }
        }

        return Task.CompletedTask;
    }

    public Task SetCurrentTime(double value)
    {
        if (wasapiOut != null && reader != null)
        {
            reader.CurrentTime = TimeSpan.FromSeconds(value);
            wasapiOut.Play();
        }

        return Task.CompletedTask;
    }

    public Task SetMuted(bool value)
    {
        if (wasapiOut != null)
        {
            wasapiOut.Volume = value ? 0 : 1;
        }

        return Task.CompletedTask;
    }

    public Task SetVolume(int value)
    {
        if (wasapiOut != null)
        {
            wasapiOut.Volume = value != 0
                ? value / 100f
                : 0;
        }

        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        wasapiOut?.Dispose();
        reader?.DisposeAsync();
        return ValueTask.CompletedTask;
    }
}

