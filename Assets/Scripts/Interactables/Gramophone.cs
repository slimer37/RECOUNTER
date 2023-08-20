using FMOD.Studio;
using FMOD;
using FMODUnity;
using System;
using UnityEngine;
using System.Collections;

namespace Recounter
{
    public class Gramophone : Interactable
    {
        [Header("Elements")]
        [SerializeField] Transform _horn;
        [SerializeField] Transform _record;
        [SerializeField] float _spinSpeed;
        [SerializeField] Vector3 _recordAxis = Vector3.up;
        [Space]
        [SerializeField] Transform _needleArm;
        [SerializeField] Vector3 _needleRotAxis = Vector3.up;
        [SerializeField] float _needleRotAmount;
        [SerializeField] float _needleFreq;
        [Space]
        [SerializeField] Transform _crank;
        [SerializeField] Vector3 _crankAxis;
        [SerializeField] float _crankSpeed;
        [SerializeField, Min(0.01f)] float _motionFadeTime = 1;

        [Header("FX")]
        [SerializeField] EventReference _music;
        [SerializeField] Vector3 _basisScale = Vector3.one;
        [SerializeField] Vector3 _scaleAxis = Vector3.up;
        [SerializeField] float _smoothing;

        EventInstance _musicInstance;

        DSP _musicDsp;

        float _velocity;
        float _smoothVolume;

        float _transitionTime;

        bool _isPlaying;

        Vector3 _needleBaseRot;

        bool _initialized;

        protected override void OnInteract(Employee e)
        {
            if (!_initialized)
            {
                BeginFirst();
                _transitionTime = Time.deltaTime / _motionFadeTime;
                return;
            }

            _isPlaying = !_isPlaying;
        }

        protected override bool CanInteract(Employee e) => _transitionTime == 0 || _transitionTime == 1;

        protected override HudInfo FormHud(Employee e) => new()
        {
            icon = Icon.Hand,
            text = _isPlaying ? "Stop" : "Start"
        };

        protected override HudInfo FormNonInteractHud(Employee e) => new()
        {
            icon = Icon.Invalid
        };

        void Awake()
        {
            _needleBaseRot = _needleArm.localEulerAngles;
            _horn.localScale = _basisScale;
        }

        void BeginFirst()
        {
            _initialized = true;

            _musicInstance = RuntimeManager.CreateInstance(_music);
            RuntimeManager.AttachInstanceToGameObject(_musicInstance, _horn);
            _musicInstance.start();

            StartCoroutine(WaitToPlay());
        }

        IEnumerator WaitToPlay()
        {
            PLAYBACK_STATE playbackState;

            do
            {
                _musicInstance.getPlaybackState(out playbackState);
                yield return null;
            }
            while (playbackState != PLAYBACK_STATE.PLAYING);

            GetChannel();

            _isPlaying = true;
        }

        void OnDestroy()
        {
            _musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            _musicInstance.release();
        }

        void GetChannel()
        {
            _musicInstance.getChannelGroup(out var channelGroup);

            channelGroup.getDSP(CHANNELCONTROL_DSP_INDEX.FADER, out _musicDsp);

            _musicDsp.setMeteringEnabled(true, true);
        }

        float PollVolume()
        {
            _musicDsp.getMeteringInfo(out var meteringInfo, IntPtr.Zero);

            var playerOutput = meteringInfo.peaklevel[0] + meteringInfo.peaklevel[1];

            return playerOutput;
        }

        void Update()
        {
            _transitionTime += (_isPlaying ? 1 : -1) * Time.deltaTime / _motionFadeTime;

            _transitionTime = Mathf.Clamp01(_transitionTime);

            if (_transitionTime == 0)
            {
                _musicInstance.setPaused(true);
                return;
            }
            else
            {
                _musicInstance.setPaused(false);
            }

            _musicInstance.setVolume(_transitionTime);

            _record.Rotate(_transitionTime * _spinSpeed * Time.deltaTime * _recordAxis);

            _crank.Rotate(_transitionTime * _crankSpeed * Time.deltaTime * _crankAxis);

            _needleArm.localEulerAngles = _needleBaseRot + _transitionTime * Mathf.PerlinNoise1D(Time.time * _needleFreq) * _needleRotAmount * _needleRotAxis;

            if (_transitionTime == 0) return;

            var volume = PollVolume();

            _smoothVolume = Mathf.SmoothDamp(_smoothVolume, volume, ref _velocity, _smoothing);

            _horn.localScale = _basisScale + _scaleAxis * _smoothVolume * _transitionTime;

            _musicInstance.getPlaybackState(out var playbackState);

            if (playbackState == PLAYBACK_STATE.STOPPED)
                _isPlaying = false;
        }
    }
}
