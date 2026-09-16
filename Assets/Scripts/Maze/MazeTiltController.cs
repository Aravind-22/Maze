using UnityEngine;
using UnityEngine.InputSystem;

namespace MazeGame.Generation
{
    public class MazeTiltController : MonoBehaviour
    {
        [SerializeField] Transform mazeRoot;
        [SerializeField] MazeConfig cfg;

        Vector2 tiltAngle;      // current X/Z degrees
        Vector2 tiltAngularVel; // current deg/sec, per axis
        private Vector2 _input;

        void Update()
        {
            Vector2 input = _input;

            for (int axis = 0; axis < 2; axis++)
            {
                float target = input[axis] != 0f ? cfg.maxTiltAngularSpeed * input[axis] : 0f;
                float rate = input[axis] != 0f ? cfg.tiltAngularAccel : cfg.tiltAngularDecel;
                tiltAngularVel[axis] = Mathf.MoveTowards(tiltAngularVel[axis], target, rate * Time.deltaTime);
            }

            tiltAngle += tiltAngularVel * Time.deltaTime;
            tiltAngle.x = Mathf.Clamp(tiltAngle.x, -cfg.maxTiltAngle, cfg.maxTiltAngle);
            tiltAngle.y = Mathf.Clamp(tiltAngle.y, -cfg.maxTiltAngle, cfg.maxTiltAngle);

            // Don't let angular velocity keep "pushing" once the clamp is hit
            if (Mathf.Abs(tiltAngle.x) >= cfg.maxTiltAngle) tiltAngularVel.x = 0f;
            if (Mathf.Abs(tiltAngle.y) >= cfg.maxTiltAngle) tiltAngularVel.y = 0f;

            mazeRoot.localRotation = Quaternion.Euler(tiltAngle.y, 0f, -tiltAngle.x);
        }

        // Placeholder polling input - swap for OnMove(InputValue) Send Messages callback
        // to match the PlayerController's existing Input System setup.

        private void OnMove(InputValue value)
        {
            Vector2 v = value.Get<Vector2>();
            _input = new Vector2(v.x, v.y);
        }

        public void SetActive(bool active)
        {
            enabled = active;
            if(!enabled)
            {
                tiltAngle = Vector2.zero;
                tiltAngularVel = Vector2.zero;
            }
        }
    }
}
