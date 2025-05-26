using System.Runtime.InteropServices;
using Silk.NET.SDL;

namespace TheAdventure;

public unsafe class AudioManager : IDisposable
{
    private readonly Sdl _sdl;
    private readonly Dictionary<string, (IntPtr Buffer, uint Length)> _loadedSounds = new();
    private const int SAMPLE_RATE = 44100;
    private const int CHANNELS = 2;
    private uint _deviceId;

    public AudioManager(Sdl sdl)
    {
        _sdl = sdl;

        // Initialize audio with 44.1KHz, signed 16-bit little-endian format, 2 channels (stereo)
        var spec = new AudioSpec
        {
            Freq = SAMPLE_RATE,
            Format = 0x8010, // AUDIO_S16LSB
            Channels = 2,
            Samples = 2048,
        };

        AudioSpec* obtained = (AudioSpec*)Marshal.AllocHGlobal(sizeof(AudioSpec));
        try
        {
            _deviceId = (uint)_sdl.OpenAudioDevice((byte*)null, 0, &spec, obtained, 0);
            if (_deviceId == 0)
            {
                throw new Exception("Failed to open audio device.");
            }

            // Resume audio playback since it starts in paused state
            _sdl.PauseAudioDevice(_deviceId, 0);

            // Generate jump sound
            GenerateJumpSound();
        }
        finally
        {
            Marshal.FreeHGlobal((IntPtr)obtained);
        }
    }

    private void GenerateJumpSound()
    {
        // Create a simple "boop" sound for jumping
        const float duration = 0.15f; // seconds
        const int numSamples = (int)(SAMPLE_RATE * duration);
        const float baseFreq = 400f; // Hz
        const float freqRange = 200f;
        
        // Allocate buffer for sound data (16-bit stereo)
        var bufferSize = numSamples * CHANNELS * sizeof(short);
        var buffer = (byte*)Marshal.AllocHGlobal(bufferSize);

        try
        {
            var shortBuffer = (short*)buffer;
            for (int i = 0; i < numSamples; i++)
            {
                float t = i / (float)SAMPLE_RATE;
                float envelope = 1.0f - (t / duration); // Linear fade out
                float freq = baseFreq + (freqRange * (1.0f - t / duration));
                short value = (short)(envelope * 32767 * MathF.Sin(2 * MathF.PI * freq * t));

                // Fill both channels with the same value
                shortBuffer[i * 2] = value;
                shortBuffer[i * 2 + 1] = value;
            }

            _loadedSounds["jump"] = ((IntPtr)buffer, (uint)bufferSize);
        }
        catch
        {
            Marshal.FreeHGlobal((IntPtr)buffer);
            throw;
        }
    }

    public void PlaySound(string name)
    {
        if (_loadedSounds.TryGetValue(name, out var sound))
        {
            if (_sdl.GetQueuedAudioSize(_deviceId) == 0) // Only queue if nothing is playing
            {
                _sdl.QueueAudio(_deviceId, (void*)sound.Buffer, sound.Length);
            }
        }
    }

    private void ReleaseUnmanagedResources()
    {
        foreach (var sound in _loadedSounds.Values)
        {
            Marshal.FreeHGlobal(sound.Buffer);
        }
        _loadedSounds.Clear();
        
        if (_deviceId != 0)
        {
            _sdl.CloseAudioDevice(_deviceId);
            _deviceId = 0;
        }
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~AudioManager()
    {
        ReleaseUnmanagedResources();
    }
}