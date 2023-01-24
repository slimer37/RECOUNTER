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

        float _motionFactor;

        bool _isPlaying;

        Vector3 _needleBaseRot;

        protected override void OnInteract(Employee e)
        {
            _isPlaying = !_isPlaying;

            _musicInstance.setPaused(!_isPlaying);
        }

        protected override bool CanInteract(Employee e) => _motionFactor == 0 || _motionFactor == 1;

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
            _musicInstance = RuntimeManager.CreateInstance(_music);
            RuntimeManager.AttachInstanceToGameObject(_musicInstance, _horn);
            _musicInstance.start();

            _needleBaseRot = _needleArm.localEulerAngles;
        }

        IEnumerator Start()
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
            _musicDsp.getMeteringInfo(out var _meteringInfo, new IntPtr());

            var playerOutput = _meteringInfo.peaklevel[0] + _meteringInfo.peaklevel[1];

            return playerOutput;
        }

        void Update()
        {
            _motionFactor += (_isPlaying ? 1 : -1) * Time.deltaTime / _motionFadeTime;

            _motionFactor = Mathf.Clamp01(_motionFactor);

            if (_motionFactor == 0) return;

            _record.Rotate(_motionFactor * _spinSpeed * Time.deltaTime * _recordAxis);

            _crank.Rotate(_motionFactor * _crankSpeed * Time.deltaTime * _crankAxis);

            _needleArm.localEulerAngles = _needleBaseRot + _motionFactor * Mathf.PerlinNoise1D(Time.time * _needleFreq) * _needleRotAmount * _needleRotAxis;

            var volume = PollVolume();

            _smoothVolume = Mathf.SmoothDamp(_smoothVolume, volume, ref _velocity, _smoothing);

            _horn.localScale = _basisScale + _scaleAxis * _smoothVolume;

            if (!_isPlaying) return;

            _musicInstance.getPlaybackState(out var playbackState);

            if (playbackState == PLAYBACK_STATE.STOPPED)
                _isPlaying = false;
        }
    }
}
