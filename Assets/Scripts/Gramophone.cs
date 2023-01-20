using FMOD.Studio;
using FMOD;
using FMODUnity;
using System;
using UnityEngine;
using System.Collections;

namespace Recounter
{
    public class Gramophone : MonoBehaviour
    {
        [Header("Elements")]
        [SerializeField] Transform _horn;
        [SerializeField] Transform _record;
        [SerializeField] float _spinSpeed;
        [SerializeField] Vector3 _recordAxis = Vector3.up;
        [SerializeField] Transform _needleArm;
        [SerializeField] Vector3 _needleRotAxis = Vector3.up;
        [SerializeField] float _needleRotAmount;
        [SerializeField] float _needleFreq;

        [Header("FX")]
        [SerializeField] EventReference _music;
        [SerializeField] Vector3 _basisScale = Vector3.one;
        [SerializeField] Vector3 _scaleAxis = Vector3.up;
        [SerializeField] float _smoothing;

        EventInstance _musicInstance;

        DSP _musicDsp;

        float _velocity;
        float _smoothVolume;

        bool _isPlaying;

        Vector3 _needleBaseRot;

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
            if (!_isPlaying) return;

            var volume = PollVolume();

            _smoothVolume = Mathf.SmoothDamp(_smoothVolume, volume, ref _velocity, _smoothing);

            _horn.localScale = _basisScale + _scaleAxis * _smoothVolume;

            _record.Rotate(_spinSpeed * Time.deltaTime * _recordAxis);

            _needleArm.localEulerAngles = _needleBaseRot + Mathf.PerlinNoise1D(Time.time * _needleFreq) * _needleRotAmount * _needleRotAxis;

            _musicInstance.getPlaybackState(out var playbackState);

            if (playbackState == PLAYBACK_STATE.STOPPED)
                _isPlaying = false;
        }
    }
}
