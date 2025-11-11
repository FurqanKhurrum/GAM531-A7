using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenTK_Sprite_Animation
{
    /// <summary>
    /// Manages audio playback for sound effects and background music
    /// </summary>
    public class AudioManager : IDisposable
    {
        private Dictionary<string, CachedSound> _soundEffects;
        private IWavePlayer _musicPlayer;
        private AudioFileReader _musicFile;
        private float _musicVolume = 0.5f;
        private float _sfxVolume = 0.7f;

        public AudioManager()
        {
            _soundEffects = new Dictionary<string, CachedSound>();
        }

        /// <summary>
        /// Load a sound effect into memory
        /// </summary>
        public void LoadSoundEffect(string name, string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Warning: Audio file not found: {filePath}");
                return;
            }

            try
            {
                _soundEffects[name] = new CachedSound(filePath);
                Console.WriteLine($"Loaded sound: {name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading sound {name}: {ex.Message}");
            }
        }

        /// <summary>
        /// Play a sound effect
        /// </summary>
        public void PlaySoundEffect(string name)
        {
            if (!_soundEffects.ContainsKey(name))
            {
                Console.WriteLine($"Warning: Sound effect '{name}' not loaded");
                return;
            }

            try
            {
                var outputDevice = new WaveOutEvent();
                var cachedSound = _soundEffects[name];
                var reader = new CachedSoundSampleProvider(cachedSound);

                // Apply volume
                var volumeProvider = new VolumeSampleProvider(reader) { Volume = _sfxVolume };

                outputDevice.Init(volumeProvider);
                outputDevice.Play();

                // Dispose after playing
                outputDevice.PlaybackStopped += (s, e) => outputDevice.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error playing sound {name}: {ex.Message}");
            }
        }

        /// <summary>
        /// Play background music (loops)
        /// </summary>
        public void PlayMusic(string filePath, bool loop = true)
        {
            StopMusic();

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Warning: Music file not found: {filePath}");
                return;
            }

            try
            {
                _musicPlayer = new WaveOutEvent();
                _musicFile = new AudioFileReader(filePath) { Volume = _musicVolume };

                if (loop)
                {
                    var loopStream = new LoopStream(_musicFile);
                    _musicPlayer.Init(loopStream);
                }
                else
                {
                    _musicPlayer.Init(_musicFile);
                }

                _musicPlayer.Play();
                Console.WriteLine("Music started");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error playing music: {ex.Message}");
            }
        }

        /// <summary>
        /// Stop background music
        /// </summary>
        public void StopMusic()
        {
            if (_musicPlayer != null)
            {
                _musicPlayer.Stop();
                _musicPlayer.Dispose();
                _musicPlayer = null;
            }

            if (_musicFile != null)
            {
                _musicFile.Dispose();
                _musicFile = null;
            }
        }

        /// <summary>
        /// Set music volume (0.0 to 1.0)
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            _musicVolume = Math.Clamp(volume, 0f, 1f);
            if (_musicFile != null)
            {
                _musicFile.Volume = _musicVolume;
            }
        }

        /// <summary>
        /// Set sound effects volume (0.0 to 1.0)
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Math.Clamp(volume, 0f, 1f);
        }

        public void Dispose()
        {
            StopMusic();
            _soundEffects.Clear();
        }

        // Helper class to cache sounds in memory
        private class CachedSound
        {
            public float[] AudioData { get; private set; }
            public WaveFormat WaveFormat { get; private set; }

            public CachedSound(string audioFileName)
            {
                using (var audioFileReader = new AudioFileReader(audioFileName))
                {
                    WaveFormat = audioFileReader.WaveFormat;
                    var wholeFile = new List<float>((int)(audioFileReader.Length / 4));
                    var readBuffer = new float[audioFileReader.WaveFormat.SampleRate * audioFileReader.WaveFormat.Channels];
                    int samplesRead;
                    while ((samplesRead = audioFileReader.Read(readBuffer, 0, readBuffer.Length)) > 0)
                    {
                        wholeFile.AddRange(readBuffer.Take(samplesRead));
                    }
                    AudioData = wholeFile.ToArray();
                }
            }
        }

        private class CachedSoundSampleProvider : ISampleProvider
        {
            private readonly CachedSound _cachedSound;
            private long _position;

            public CachedSoundSampleProvider(CachedSound cachedSound)
            {
                _cachedSound = cachedSound;
                _position = 0;
            }

            public int Read(float[] buffer, int offset, int count)
            {
                // Calculate how many samples are available from current position
                long availableSamples = _cachedSound.AudioData.Length - _position;

                // Determine how many samples to actually copy (minimum of requested and available)
                int samplesToCopy = (int)Math.Min(availableSamples, count);

                // If no samples are available, return 0
                if (samplesToCopy <= 0)
                {
                    return 0;
                }

                // Safely copy samples with explicit int casts
                int sourceIndex = (int)_position;

                // Copy the audio data to the buffer
                for (int i = 0; i < samplesToCopy; i++)
                {
                    buffer[offset + i] = _cachedSound.AudioData[sourceIndex + i];
                }

                // Update position
                _position += samplesToCopy;

                return samplesToCopy;
            }

            public WaveFormat WaveFormat => _cachedSound.WaveFormat;
        }

        // Helper class to loop audio
        private class LoopStream : WaveStream
        {
            private readonly WaveStream _sourceStream;

            public LoopStream(WaveStream sourceStream)
            {
                _sourceStream = sourceStream;
            }

            public override WaveFormat WaveFormat => _sourceStream.WaveFormat;
            public override long Length => long.MaxValue;
            public override long Position
            {
                get => _sourceStream.Position;
                set => _sourceStream.Position = value;
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                int totalBytesRead = 0;

                while (totalBytesRead < count)
                {
                    int bytesRead = _sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
                    if (bytesRead == 0)
                    {
                        // Loop back to start
                        _sourceStream.Position = 0;
                    }
                    totalBytesRead += bytesRead;
                }

                return totalBytesRead;
            }
        }
    }
}