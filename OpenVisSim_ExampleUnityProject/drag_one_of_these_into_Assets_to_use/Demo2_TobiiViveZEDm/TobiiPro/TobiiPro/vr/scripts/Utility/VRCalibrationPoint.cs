//-----------------------------------------------------------------------
// Copyright © 2017 Tobii AB. All rights reserved.
//-----------------------------------------------------------------------

using UnityEngine;

namespace Tobii.Research.Unity
{
    public class VRCalibrationPoint : MonoBehaviour
    {
        // Animation start/end time.
        private float _startTime;
        private float _length;
        private float _speed;

        private Vector3 _zoomIn;
        private Vector3 _zoomOut;

        private bool _animation;

        private void Start()
        {
            _animation = false;
            _length = 1.7f;
            _speed = 1.0f;

            // Flat sphere
            _zoomOut = new Vector3(0.1f, 0.1f, 0.01f);
            _zoomIn = new Vector3(0.005f, 0.005f, 0.0f);
        }

        private void Update()
        {
            if (_animation == true)
            {
                var covered = (Time.time - _startTime) * _speed;
                var unitCovered = covered / _length;
                transform.localScale = Vector3.Lerp(_zoomOut, _zoomIn, unitCovered);
            }
        }

        public void StartAnim()
        {
            transform.localScale = _zoomOut;
            _startTime = Time.time;
            _animation = true;
        }
    }
}
