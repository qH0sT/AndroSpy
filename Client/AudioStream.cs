using Android.Media;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Task2
{
    internal class AudioStream : IAudioStream
	{
	    readonly int bufferSize;
		readonly ChannelIn channels = ChannelIn.Mono;
		readonly Encoding audioFormat = Encoding.Pcm16bit;

	/// <summary>
	/// The audio source.
	/// </summary>
	AudioRecord audioSource;

		/// <summary>
		/// Occurs when new audio has been streamed.
		/// </summary>
		public event EventHandler<byte[]> OnBroadcast;

		/// <summary>
		/// Occurs when the audio stream active status changes.
		/// </summary>
		public event EventHandler<bool> OnActiveChanged;

		/// <summary>
		/// Occurs when there's an error while capturing audio.
		/// </summary>
		public event EventHandler<Exception> OnException;

		/// <summary>
		/// The default device.
		/// </summary>
		public AudioSource DefaultDevice = AudioSource.Default;

		/// <summary>
		/// Gets the sample rate.
		/// </summary>
		/// <value>
		/// The sample rate.
		/// </value>
		public int SampleRate { get; private set; } = 44100;

		/// <summary>
		/// Gets bits per sample.
		/// </summary>
		public int BitsPerSample => (audioSource.AudioFormat == Encoding.Pcm16bit) ? 16 : 8;

		/// <summary>
		/// Gets the channel count.
		/// </summary>
		/// <value>
		/// The channel count.
		/// </value>        
		public int ChannelCount => audioSource.ChannelCount;

		/// <summary>
		/// Gets the average data transfer rate
		/// </summary>
		/// <value>The average data transfer rate in bytes per second.</value>
		public int AverageBytesPerSecond => SampleRate * BitsPerSample / 8 * ChannelCount;

		/// <summary>
		/// Gets a value indicating if the audio stream is active.
		/// </summary>
		public bool Active => audioSource?.RecordingState == RecordState.Recording;

		void Init()
		{
			Stop(); // just in case

			audioSource = new AudioRecord(
				DefaultDevice,
				SampleRate,
				channels,
				audioFormat,
				bufferSize);

			if (audioSource.State == State.Uninitialized)
			{
				throw new Exception("Unable to successfully initialize AudioStream; reporting State.Uninitialized.  If using an emulator, make sure it has access to the system microphone.");
			}
		}

		/// <summary>
		/// Starts the audio stream.
		/// </summary>
		public Task Start()
		{
			try
			{
				if (!Active)
				{
					// not sure this does anything or if should be here... inherited via copied code ¯\_(ツ)_/¯
					Android.OS.Process.SetThreadPriority(Android.OS.ThreadPriority.UrgentAudio);

					Init();

					audioSource.StartRecording();

					OnActiveChanged?.Invoke(this, true);

					Task.Run(() => Record());
				}

				return Task.FromResult(true);
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Error in AudioStream.Start(): {0}", ex.Message);

				Stop();
				throw;
			}
		}

		/// <summary>
		/// Stops the audio stream.
		/// </summary>
		public Task Stop()
		{
			if (Active)
			{
				audioSource.Stop();
				audioSource.Release();

				OnActiveChanged?.Invoke(this, false);
			}
			else // just in case
			{
				audioSource?.Release();
			}

			return Task.FromResult(true);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AudioStream"/> class.
		/// </summary>
		/// <param name="sampleRate">Sample rate.</param>
		/// <param name="channels">The <see cref="ChannelIn"/> value representing the number of channels to record.</param>
		/// <param name="audioFormat">The format of the recorded audio.</param>
		public AudioStream(int sampleRate = 44100, AudioSource audioSource = AudioSource.Default,ChannelIn channels = ChannelIn.Mono, Encoding audioFormat = Encoding.Pcm16bit)
		{
			bufferSize = AudioRecord.GetMinBufferSize(sampleRate, channels, audioFormat);

			if (bufferSize < 0)
			{
				throw new Exception("Invalid buffer size calculated; audio settings used may not be supported on this device");
			}
			DefaultDevice = audioSource;
			SampleRate = sampleRate;
			this.channels = channels;
			this.audioFormat = audioFormat;
		}

		/// <summary>
		/// Record from the microphone and broadcast the buffer.
		/// </summary>
		async Task Record()
		{
			byte[] data = new byte[bufferSize];
			int readFailureCount = 0;
			int readResult = 0;

			Debug.WriteLine("AudioStream.Record(): Starting background loop to read audio stream");

			while (Active)
			{
				try
				{
					// not sure if this is even a good idea, but we'll try to allow a single bad read, and past that shut it down
					if (readFailureCount > 1)
					{
						Debug.WriteLine("AudioStream.Record(): Multiple read failures detected, stopping stream");
						await Stop();
						break;
					}

					readResult = audioSource.Read(data, 0, bufferSize); // this can block if there are no bytes to read

					// readResult should == the # bytes read, except a few special cases
					if (readResult > 0)
					{
						readFailureCount = 0;
						OnBroadcast?.Invoke(this, data);
					}
					else
					{
						switch (readResult)
						{
							case (int)TrackStatus.ErrorInvalidOperation:
							case (int)TrackStatus.ErrorBadValue:
							case (int)TrackStatus.ErrorDeadObject:
								Debug.WriteLine("AudioStream.Record(): readResult returned error code: {0}", readResult);
								await Stop();
								break;
							//case (int)TrackStatus.Error:
							default:
								readFailureCount++;
								Debug.WriteLine("AudioStream.Record(): readResult returned error code: {0}", readResult);
								break;
						}
					}
				}
				catch (Exception ex)
				{
					readFailureCount++;

					Debug.WriteLine("Error in Android AudioStream.Record(): {0}", ex.Message);

					OnException?.Invoke(this, ex);
				}
			}
		}

		/// <summary>
		/// Flushes any audio bytes in memory but not yet broadcast out to any listeners.
		/// </summary>
		public void Flush()
		{

			audioSource.Dispose();
			// not needed for this implementation
		}
	}
}