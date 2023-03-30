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
        [SerializeField] float gain = -20f;
        [SerializeField] float frequency = 220f;

        Channel channel;
        DSP fader;
        DSP oscillator;

        bool play;
        bool paused;
        float t;

        const int GAIN = (int)DSP_FADER.GAIN;

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
            RuntimeManager.CoreSystem.createDSPByType(DSP_TYPE.OSCILLATOR, out oscillator);
            RuntimeManager.CoreSystem.getMasterChannelGroup(out var master);
            RuntimeManager.CoreSystem.playDSP(oscillator, master, false, out channel);

            oscillator.setParameterInt((int)DSP_OSCILLATOR.TYPE, (int)waveform);
            oscillator.setParameterFloat((int)DSP_OSCILLATOR.RATE, frequency);

            channel.getDSP(CHANNELCONTROL_DSP_INDEX.FADER, out fader);

            paused = true;

            channel.setPaused(true);

            fader.setParameterFloat(GAIN, -80f);
        }

        void OnDestroy()
        {
            channel.stop();
            oscillator.release();
        }

        public void Play() => play = true;
        public void Stop() => play = false;

        void Update()
        {
            if (play)
            {
                t += Time.deltaTime / attack;

                if (paused)
                {
                    channel.setPaused(false);
                    paused = false;
                }
            }
            else
            {
                t -= Time.deltaTime / decay;

                if (!paused && t < 0)
                {
                    channel.setPaused(true);
                    paused = true;
                }
            }

            t = Mathf.Clamp01(t);

            fader.setParameterFloat(GAIN, Mathf.Lerp(-80f, gain, t));
        }
    }
}
