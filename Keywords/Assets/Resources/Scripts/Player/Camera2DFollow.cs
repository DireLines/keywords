using System;
using UnityEngine;
using System.Collections;

namespace UnityStandardAssets._2D {
    public class Camera2DFollow : MonoBehaviour {
        public Transform target;
        public float damping = 1;
        public float lookAheadFactor = 3;
        public float lookAheadReturnSpeed = 0.5f;
        public float lookAheadMoveThreshold = 0.1f;

        //zooming
        private bool isZooming = false;
        public float zoomRate;
        public float[] zoomTargets;
        private int zoomTargetIndex;
        const float epsilon = 0.01f;
        private Camera cam;

        private float m_OffsetZ;
        private Vector3 m_LastTargetPosition;
        private Vector3 m_CurrentVelocity;
        private Vector3 m_LookAheadPos;

        private bool shaking;
        public float shakeMagnitude = 1f;
        public float shakeDamping = 0f;

        // Use this for initialization
        private void Start() {
            transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
            m_LastTargetPosition = target.position;
            m_OffsetZ = (transform.position - target.position).z;
            transform.parent = null;
            ConstructCullingMask(target.gameObject.GetComponent<PlayerInfo>().playerNum);
            cam = GetComponent<Camera>();
            zoomTargets[0] = cam.orthographicSize;
            zoomTargetIndex = 0;
            shaking = false;
        }

        private void ConstructCullingMask(int playerNum) {
            Camera cam = GetComponent<Camera>();
            cam.cullingMask &= ~(1 << LayerMask.NameToLayer("P1"));
            cam.cullingMask &= ~(1 << LayerMask.NameToLayer("P2"));
            cam.cullingMask &= ~(1 << LayerMask.NameToLayer("P3"));
            cam.cullingMask &= ~(1 << LayerMask.NameToLayer("P4"));
            cam.cullingMask |= 1 << LayerMask.NameToLayer("P" + playerNum);
            if (playerNum == 1) {
                for (int i = 1; i < 16; i += 2) {
                    cam.cullingMask &= ~(1 << LayerMask.NameToLayer(Convert.ToString(i, 2).PadLeft(4, '0')));
                }
            } else if (playerNum == 2) {
                for (int i = 2; i < 16; i += 4) {
                    cam.cullingMask &= ~(1 << LayerMask.NameToLayer(Convert.ToString(i, 2).PadLeft(4, '0')));
                    cam.cullingMask &= ~(1 << LayerMask.NameToLayer(Convert.ToString(i + 1, 2).PadLeft(4, '0')));
                }
            } else if (playerNum == 3) {
                for (int i = 4; i < 16; i += 8) {
                    cam.cullingMask &= ~(1 << LayerMask.NameToLayer(Convert.ToString(i, 2).PadLeft(4, '0')));
                    cam.cullingMask &= ~(1 << LayerMask.NameToLayer(Convert.ToString(i + 1, 2).PadLeft(4, '0')));
                    cam.cullingMask &= ~(1 << LayerMask.NameToLayer(Convert.ToString(i + 2, 2).PadLeft(4, '0')));
                    cam.cullingMask &= ~(1 << LayerMask.NameToLayer(Convert.ToString(i + 3, 2).PadLeft(4, '0')));
                }
            } else if (playerNum == 4) {
                for (int i = 8; i < 16; i++) {
                    cam.cullingMask &= ~(1 << LayerMask.NameToLayer(Convert.ToString(i, 2).PadLeft(4, '0')));
                }
            } else {
                print("ConstructCullingMask called on weird value of playerNum");
            }
        }

        // Update is called once per frame
        private void Update() {
            // only update lookahead pos if accelerating or changed direction
            float xMoveDelta = (target.position - m_LastTargetPosition).x;

            bool updateLookAheadTarget = Mathf.Abs(xMoveDelta) > lookAheadMoveThreshold;

            if (updateLookAheadTarget) {
                m_LookAheadPos = lookAheadFactor * Vector3.right * Mathf.Sign(xMoveDelta);
            } else {
                m_LookAheadPos = Vector3.MoveTowards(m_LookAheadPos, Vector3.zero, Time.deltaTime * lookAheadReturnSpeed);
            }

            Vector3 aheadTargetPos = target.position + m_LookAheadPos + Vector3.forward * m_OffsetZ;
            Vector3 newPos = Vector3.SmoothDamp(transform.position, aheadTargetPos, ref m_CurrentVelocity, damping);

            transform.position = newPos;

            m_LastTargetPosition = target.position;

            if (isZooming) {
                float zoomTarget = zoomTargets[zoomTargetIndex];
                float zoomDiff = cam.orthographicSize - zoomTarget;
                cam.orthographicSize = zoomTarget + zoomDiff * zoomRate;
                if (Math.Abs(zoomDiff) <= epsilon) {
                    isZooming = false;
                    cam.orthographicSize = zoomTarget;
                }
            }
        }

        public void Shake(float duration) {
            if (shaking) return;
            StartCoroutine(ShakeCR(duration));
        }

        private Vector2 GetRandomGaussian() {
            float v1 = UnityEngine.Random.value;
            float v2 = UnityEngine.Random.value;
            float r = Mathf.Sqrt(-2 * Mathf.Log10(v1));
            float theta = 2 * Mathf.PI * v2;

            float x = r * Mathf.Cos(theta);
            float y = r * Mathf.Sin(theta);
            return new Vector2(x, y);
        }

        private IEnumerator ShakeCR(float duration) {
            shaking = true;
            float t = 0f;
            float initialDamping = damping;
            Transform initialTarget = target;
            string targetName = "Shake-" + initialTarget.name;
            GameObject shakePoint = new GameObject(targetName);
            target = shakePoint.transform;
            //print("Start Target: " + target.name);
            damping = shakeDamping;
            while (t < duration) {
                target.position = initialTarget.position + (Vector3)GetRandomGaussian() * shakeMagnitude;
                t += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            Destroy(target.gameObject);
            damping = initialDamping;
            target = initialTarget;
            //print("Target: " + target.name + "   InitialTarget: " + initialTarget.name);
            shaking = false;
        }

        public void ToggleZoom(bool zoomIn = false) {
            isZooming = true;
            if (zoomIn) {
                zoomTargetIndex = Game.mod(zoomTargetIndex - 1, zoomTargets.Length);
            } else {
                zoomTargetIndex = Game.mod(zoomTargetIndex + 1, zoomTargets.Length);
            }
        }
    }
}
