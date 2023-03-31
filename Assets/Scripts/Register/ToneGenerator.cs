using FMOD;
using FMODUnity;
using UnityEngine;

namespace Recounter
{
    public class ToneGenerator : MonoBehaviour
    {
        enum Waveform
        {
            Sine = 0,
            Square = 1,
            Sawup = 2,
            Sawdown = 3,
            Triangle = 4,
            Noise = 5
        }

        [SerializeField] Waveform waveform;
        [SerializeField] float attack = 0;
        [SerializeField] float decay = 0.2f;
        [SerializeField] float volume = 1.0f;
        [SerializeField] float frequency = 220f;
        [SerializeField] float defaultBeepLength = 0.5f;

        Channel channel;
        DSP oscillator;

        int rate;

        ulong lastFadePoint;

        void OnValidate()
        {
            if (Application.isPlaying)
            {
                oscillator.setParameterInt((int)DSP_OSCILLATOR.TYPE, (int)waveform);
                oscillator.setParameterFloat((int)DSP_OSCILLATOR.RATE, frequency);
            }
        }

        void Awake()
        {
            RuntimeManager.CoreSystem.getSoftwareFormat(out rate, out _, out _);

            RuntimeManager.CoreSystem.createDSPByType(DSP_TYPE.OSCILLATOR, out oscillator);
            RuntimeManager.CoreSystem.getMasterChannelGroup(out var master);
            RuntimeManager.CoreSystem.playDSP(oscillator, master, false, out channel);

            oscillator.setParameterInt((int)DSP_OSCILLATOR.TYPE, (int)waveform);
            oscillator.setParameterFloat((int)DSP_OSCILLATOR.RATE, frequency);

            channel.setPaused(true);
        }

        void OnDestroy()
        {
            channel.stop();
            oscillator.release();
        }

        public void Beep() => Beep(defaultBeepLength);

        public void Beep(float length)
        {
            channel.setPaused(false);

            channel.getDSPClock(out _, out var dspClock);

            if (dspClock < lastFadePoint)
            {
                channel.removeFadePoints(dspClock, lastFadePoint);

                float fadeVolume;

                var elapsedDelay = dspClock - (lastFadePoint - rate * decay);

                if (elapsedDelay < 0)
                {
                    fadeVolume = volume;
                }
                else
                {
                    fadeVolume = (1 - elapsedDelay / (rate * decay)) * volume;
                }

                channel.addFadePoint(dspClock, fadeVolume);
            }
            else
            {
                channel.addFadePoint(dspClock, 0.0f);
            }

            var start = dspClock + (ulong)(rate * attack);
            var startDecay = start + (ulong)(rate * length);
            lastFadePoint = startDecay + (ulong)(rate * decay);

            channel.addFadePoint(start, volume);
            channel.addFadePoint(startDecay, volume);

            channel.addFadePoint(lastFadePoint, 0.0f);
        }
    }
}
