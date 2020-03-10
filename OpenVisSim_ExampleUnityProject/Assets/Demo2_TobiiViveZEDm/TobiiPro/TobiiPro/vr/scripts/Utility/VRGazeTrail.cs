//-----------------------------------------------------------------------
// Copyright © 2017 Tobii AB. All rights reserved.
//-----------------------------------------------------------------------

using UnityEngine;

namespace Tobii.Research.Unity
{
    public class VRGazeTrail : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The color of the particle")]
        private Color _color = Color.green;

        [SerializeField]
        [Range(1, 1000)]
        [Tooltip("The number of particles to allocate.")]
        private int _particleCount = 100;

        [SerializeField]
        [Range(0.005f, 0.2f)]
        [Tooltip("The size of the particle.")]
        private float _particleSize = 0.05f;

        [SerializeField]
        [Tooltip("Turn gaze trail on or off.")]
        public bool _on = true;


        /// <summary>
        /// Turn gaze trail on or off.
        /// </summary>
        public bool On
        {
            get
            {
                return _on;
            }

            set
            {
                _on = value;
                OnSwitch();
            }
        }

        /// <summary>
        /// Set particle count between 1 and 1000.
        /// </summary>
        public int ParticleCount
        {
            get
            {
                return _particleCount;
            }

            set
            {
                if (value < 1 || value > 1000)
                {
                    return;
                }

                _particleCount = value;
                CheckCount();
            }
        }

        private bool _lastOn;
        private int _lastParticleCount;
        private int _particleIndex;
        private ParticleSystem.Particle[] _particles;
        private ParticleSystem _particleSystem;
        private bool _particlesDirty;
        private VREyeTracker _eyeTracker;
        private VRCalibration _calibrationObject;

        private void Start()
        {
            _lastParticleCount = _particleCount;
            _particles = new ParticleSystem.Particle[_particleCount];
            _eyeTracker = VREyeTracker.Instance;
            _particleSystem = GetComponent<ParticleSystem>();
            _calibrationObject = VRCalibration.Instance;
        }

        private void Update()
        {
            if (_particlesDirty)
            {
                _particleSystem.SetParticles(_particles, _particles.Length);
                _particlesDirty = false;
            }

            CheckCount();

            OnSwitch();

            if (_calibrationObject != null && _calibrationObject.CalibrationInProgress)
            {
                // Don't do anything if we are calibrating.
                return;
            }

            if (_eyeTracker != null && _on)
            {
                var latestData = _eyeTracker.LatestGazeData;
                if (latestData.CombinedGazeRayWorldValid)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(latestData.CombinedGazeRayWorld, out hit))
                    {
                        PlaceParticle(hit.point, _color, _particleSize);
                    }
                }
            }
        }

        private void CheckCount()
        {
            if (_lastParticleCount != _particleCount)
            {
                RemoveParticles();
                _particleIndex = 0;
                _particles = new ParticleSystem.Particle[_particleCount];
                _lastParticleCount = _particleCount;
            }
        }

        private void OnSwitch()
        {
            if (_lastOn && !_on)
            {
                // Switch off.
                RemoveParticles();
                _lastOn = false;
            }
            else if (!_lastOn && _on)
            {
                // Switch on.
                _lastOn = true;
            }
        }

        private void RemoveParticles()
        {
            for (int i = 0; i < _particles.Length; i++)
            {
                PlaceParticle(Vector3.zero, Color.white, 0);
            }
        }

        private void PlaceParticle(Vector3 pos, Color color, float size)
        {
            var particle = _particles[_particleIndex];
            particle.position = pos;
            particle.startColor = color;
            particle.startSize = size;
            _particles[_particleIndex] = particle;
            _particleIndex = (_particleIndex + 1) % _particles.Length;
            _particlesDirty = true;
        }
    }
}