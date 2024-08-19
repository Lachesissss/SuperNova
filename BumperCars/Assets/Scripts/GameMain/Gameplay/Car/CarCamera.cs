using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Lachesis.GamePlay
{
    [RequireComponent(typeof(Camera))]
    public class CarCamera : Entity
    {
        public enum CameraType
        {
            leftCamera,
            rightCamera,
        }

        [Tooltip("相机跟随的车")]
        public CarComponent car;
        public CameraType type;
        [Header("一些控制参数")]
        [Tooltip("目标相对于汽车的位置。")]
        public Vector3 thirdPersonOffsetStart = new Vector3(0, 0.5f, 0);
        [Tooltip("摄像机相对于汽车的位置。")]
        public Vector3 thirdPersonOffsetEnd = new Vector3(0, 1, -3);
        [Tooltip("摄影机相对于目标的旋转。")]
        public Vector3 thirdPersonAngle = new Vector3(10, 0, 0);
        [Tooltip("当障碍物挡住相机时应保持的最小距离。")]
        public float thirdPersonSkinWidth = 0.1f;
        [Tooltip("如果刚体的速度低于此值，则降低相机的旋转。设置为 0 以禁用。")]
        public float interpolationUpToSpeed = 50;
        [Tooltip("旋转到目标的position速度")]
        public float thirdPersonPositionSpeed = 50;
        [Tooltip("旋转到目标的rotation速度")]
        public float thirdPersonRotationSpeed = 50;

        private Rigidbody cameraBody;
        private Camera m_camera;
        public Material flipMaterial;
        public bool m_isFlip;
        public static bool isCompressing;

        public override void OnInit(object userData = null)
        {
            base.OnInit(userData);
            m_camera = GetComponent<Camera>();
        }

        public override void OnReCreateFromPool(Vector3 pos, Quaternion rot, object userData = null)
        {
            base.OnReCreateFromPool(pos, rot, userData);
            ResetCarCamera(userData);
        }

        public override void OnReCreateFromPool(object userData = null)
        {
            base.OnReCreateFromPool(userData);
            ResetCarCamera(userData);
        }

        public void StopToTracked()
        {
            car = null;
            cameraBody.velocity = Vector3.zero;
            cameraBody.angularVelocity = Vector3.zero;
        }
        
        public void ReSetTrackedTarget(CarComponent trackedCar)
        {
            car = trackedCar;
            cameraBody = GetComponentInChildren<Rigidbody>();
            if (cameraBody == null)
            {
                cameraBody = gameObject.AddComponent<Rigidbody>();
                cameraBody.hideFlags = HideFlags.NotEditable;
                cameraBody.velocity = Vector3.zero;
                cameraBody.angularVelocity = Vector3.zero;
            }

            
            cameraBody.isKinematic = false;
            cameraBody.useGravity = false;
            cameraBody.drag = 0;
            cameraBody.angularDrag = 0;
            thirdPersonRotationSpeed = 50f;
            thirdPersonPositionSpeed = 50f;
            ResetCamera();
        }
        
        public override void OnReturnToPool(bool isShutDown = false)
        {
            base.OnReturnToPool(isShutDown);
            if(cameraBody!=null)
            {
                Destroy(cameraBody);
            }
        }

        private void ResetCarCamera(object userData)
        {
            StopAllCoroutines();
            if(type == CameraType.leftCamera)
            {
                m_camera.rect = new Rect(0,0,0.5f,1);
            }
            else
            {
                m_camera.rect = new Rect(0.5f,0,0.5f,1);
            }
            m_isFlip = false;
            isCompressing = false;
            if (userData is CarComponent trackedCar)
            {
                ReSetTrackedTarget(trackedCar);
            }
            else
            {
                Debug.LogError("没有给相机设置追踪目标!!!");
            }
        }

        public override void OnFixedUpdate(float fixedElapseSeconds)
        {
            base.OnFixedUpdate(fixedElapseSeconds);
            if (car == null) return;
            if (cameraBody == null) return;
            cameraBody.interpolation = car.bodyRb.interpolation;

            Vector3 previousPosition = transform.position;
            Quaternion previousRotation = transform.rotation;

            GetTargetTransforms(out var targetPosition, out var targetRotation);

            float lerpPositionSpeed = 0;
            float lerpRotationSpeed = 0;

            lerpPositionSpeed = thirdPersonPositionSpeed;
            lerpRotationSpeed = thirdPersonRotationSpeed;

            lerpPositionSpeed = Mathf.Clamp(lerpPositionSpeed, 0, 1f / fixedElapseSeconds);
            lerpRotationSpeed = Mathf.Clamp(lerpRotationSpeed, 0, 1f / fixedElapseSeconds);
            Quaternion rotationDifference = getShortestRotation(targetRotation, previousRotation);

            float angleInDegrees;
            Vector3 rotationAxis;
            rotationDifference.ToAngleAxis(out angleInDegrees, out rotationAxis);

            Vector3 angularDisplacement = rotationAxis * (angleInDegrees * Mathf.Deg2Rad);

            cameraBody.velocity = (targetPosition - previousPosition) * lerpPositionSpeed;
            cameraBody.angularVelocity = angularDisplacement * lerpRotationSpeed;
        }

        private void GetTargetTransforms(out Vector3 targetPosition, out Quaternion targetRotation, bool ignoreSpeed = false)
        {
            Vector3 followPosition = car.bodyRb.transform.position;
            Quaternion followRotation = car.bodyRb.transform.rotation;

            targetPosition = transform.position;
            targetRotation = transform.rotation;

            float interpolationMultiplier = 1;

            Rigidbody body = car.bodyRb;
            if (body != null)
            {
                float forwardVelocity = Vector3.Dot(car.bodyRb.velocity, car.bodyRb.transform.forward);
                interpolationMultiplier = interpolationUpToSpeed > 0 && !ignoreSpeed ? Mathf.Clamp01(Mathf.Abs(forwardVelocity) / interpolationUpToSpeed) : 1f;
                Vector3 rotationDirection = Vector3.Lerp(body.transform.forward, body.velocity.normalized, Mathf.Clamp01(forwardVelocity));
                followRotation = Quaternion.LookRotation(rotationDirection, Vector3.back);
            }

            Vector3 rotationEuler = thirdPersonAngle + Vector3.up * followRotation.eulerAngles.y;

            Quaternion xzRotation = Quaternion.Euler(new Vector3(rotationEuler.x, targetRotation.eulerAngles.y, rotationEuler.z));
            Quaternion lerpedRotation = Quaternion.Euler(rotationEuler);
            targetRotation = Quaternion.Lerp(xzRotation, lerpedRotation, interpolationMultiplier);

            Vector3 forwardDirection = targetRotation * Vector3.forward;
            Vector3 rightDirection = targetRotation * Vector3.right;
            Vector3 directionVector = forwardDirection * thirdPersonOffsetEnd.z + Vector3.up * thirdPersonOffsetEnd.y + rightDirection * thirdPersonOffsetEnd.x;

            Vector3 directionVectorNormal = directionVector.normalized;
            float directionMagnitude = directionVector.magnitude;

            Vector3 cameraWorldDirection = directionVectorNormal;
            Vector3 startCast = followPosition + thirdPersonOffsetStart;
            RaycastHit[] hits = Physics.RaycastAll(startCast, cameraWorldDirection, directionMagnitude);
            float hitDistance = -1;
            foreach (RaycastHit hit in hits)
            {
                if (!isChildOf(hit.transform, car.transform)) hitDistance = hitDistance >= 0 ? Mathf.Min(hitDistance, hit.distance) : hit.distance;
            }
            if (hitDistance >= 0)
            {
                targetPosition = followPosition + thirdPersonOffsetStart + directionVectorNormal * Mathf.Max(thirdPersonSkinWidth, hitDistance - thirdPersonSkinWidth);
            }
            else
            {
                targetPosition = directionVector + thirdPersonOffsetStart + followPosition;
            }

        }

        private bool isChildOf(Transform source, Transform target)
        {
            Transform child = source;
            while (child != null)
            {
                if (child == target) return true;
                child = child.parent;
            }
            return false;
        }

        private static Quaternion getShortestRotation(Quaternion a, Quaternion b)
        {
            if (Quaternion.Dot(a, b) < 0)
            {
                return a * Quaternion.Inverse(multiplyQuaternion(b, -1));
            }
            else return a * Quaternion.Inverse(b);
        }

        private static Quaternion multiplyQuaternion(Quaternion input, float scalar)
        {
            return new Quaternion(input.x * scalar, input.y * scalar, input.z * scalar, input.w * scalar);
        }

        private void ResetCamera()
        {
            if (cameraBody == null) return;
            GetTargetTransforms(out var targetPosition, out var targetRotation, true);
            cameraBody.position = targetPosition;
            cameraBody.rotation = targetRotation;
        }
        
        public void SetFlipVertical(bool isFlip)
        {
            m_isFlip = isFlip;
        }
        
        void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (m_isFlip)
            {
                Graphics.Blit(src, dest, flipMaterial);
            }
            else
            {
                Graphics.Blit(src, dest);
            }
        }
        
        //这里后面看需要改成配置吧
        public static void SwapController(CarCamera camera1, CarCamera camera2)
        {
            camera1.thirdPersonRotationSpeed = 8f;
            camera1.thirdPersonPositionSpeed = 8f;
            camera2.thirdPersonRotationSpeed = 8f;
            camera2.thirdPersonPositionSpeed = 8f;
            (camera1.car, camera2.car) = (camera2.car, camera1.car);
            GameEntry.instance.StartCoroutine(DelayToRecoverCameraSpeed(camera1, camera2));
        }
        
        private static IEnumerator DelayToRecoverCameraSpeed(CarCamera camera1, CarCamera camera2)
        {
            
            yield return new WaitForSeconds(0.8f);
            for (var i = 0; i < 42; i++)
            {
                camera1.thirdPersonRotationSpeed++;
                camera1.thirdPersonPositionSpeed++;
                camera2.thirdPersonRotationSpeed++;
                camera2.thirdPersonPositionSpeed++;
                yield return new WaitForSeconds(0.025f);
            }
        }
        
        public static void Compress(CarCamera strongCamera, CarCamera weekCamera)
        {
            if(isCompressing) return;
            if(strongCamera.type==CameraType.leftCamera&&weekCamera.type==CameraType.rightCamera)
            {
                GameEntry.instance.StartCoroutine(DelayToCompressLeftStrong(strongCamera, weekCamera));
            }
            else if(strongCamera.type==CameraType.rightCamera&&weekCamera.type==CameraType.leftCamera)
            {
                GameEntry.instance.StartCoroutine(DelayToCompressRightStrong(weekCamera,strongCamera));
            }
            else
            {
                Debug.LogError("压缩相机必须一左一右");
            }
        }
        
        private static IEnumerator DelayToCompressRightStrong(CarCamera leftCamera, CarCamera rightCamera)
        {
            isCompressing = true;
            // float changeTime = 2f;
            // float curTime = changeTime;
            // float startTime = Time.time;
            // float endTime = Time.time;
            Rect left = new Rect(0,0,0.5f,1);
            leftCamera.m_camera.rect = left;
            Rect right = new Rect(0.5f,0,0.5f,1);
            rightCamera.m_camera.rect = right;
            // while (curTime>0)
            // {
            //     curTime-=(endTime - startTime);
            //     startTime = Time.time;
            //     yield return new WaitForEndOfFrame();
            //     endTime = Time.time;
            //     var dt = ((endTime - startTime)/changeTime)*0.25f;
            //     left.width-=dt;
            //     leftCamera.m_camera.rect = left;
            //     right.x-=dt;
            //     right.width+=dt;
            //     rightCamera.m_camera.rect = right;
            // }
            left.width=0.25f;
            leftCamera.m_camera.rect = left;
            right.x=0.25f;
            right.width = 0.75f;
            rightCamera.m_camera.rect = right;
            
            yield return new WaitForSeconds(12f);
            // curTime = changeTime;
            // startTime = Time.time;
            // endTime = Time.time;
            // while (curTime>0)
            // {
            //     curTime-=(endTime - startTime);
            //     startTime = Time.time;
            //     yield return new WaitForEndOfFrame();
            //     endTime = Time.time;
            //     var dt = ((endTime - startTime)/changeTime)*0.25f;
            //     left.width+=dt;
            //     leftCamera.m_camera.rect = left;
            //     right.x+=dt;
            //     right.width-=dt;
            //     rightCamera.m_camera.rect = right;
            // }
            left.width=0.5f;
            leftCamera.m_camera.rect = left;
            right.x=0.5f;
            left.width = 0.5f;
            rightCamera.m_camera.rect = right;
            isCompressing = false;
        }
        
        private static IEnumerator DelayToCompressLeftStrong(CarCamera leftCamera, CarCamera rightCamera)
        {
            isCompressing = true;
            // float changeTime = 2f;
            // float curTime = changeTime;
            // float startTime = Time.time;
            // float endTime = Time.time;
            Rect left = new Rect(0,0,0.5f,1);
            leftCamera.m_camera.rect = left;
            Rect right = new Rect(0.5f,0,0.5f,1);
            rightCamera.m_camera.rect = right;
            // while (curTime>0)
            // {
            //     curTime-=(endTime - startTime);
            //     startTime = Time.time;
            //     yield return new WaitForEndOfFrame();
            //     endTime = Time.time;
            //     var dt = ((endTime - startTime)/changeTime)*0.25f;
            //     left.width+=dt;
            //     leftCamera.m_camera.rect = left;
            //     right.x+=dt;
            //     right.width-=dt;
            //     rightCamera.m_camera.rect = right;
            // }
            left.width=0.75f;
            leftCamera.m_camera.rect = left;
            right.x=0.75f;
            right.width = 0.25f;
            rightCamera.m_camera.rect = right;
            
            yield return new WaitForSeconds(12f);
            // curTime = changeTime;
            // startTime = Time.time;
            // endTime = Time.time;
            // while (curTime>0)
            // {
            //     curTime-=(endTime - startTime);
            //     startTime = Time.time;
            //     yield return new WaitForEndOfFrame();
            //     endTime = Time.time;
            //     var dt =((endTime - startTime)/changeTime)*0.25f;
            //     left.width-=dt;
            //     leftCamera.m_camera.rect = left;
            //     right.x-=dt;
            //     right.width+=dt;
            //     rightCamera.m_camera.rect = right;
            // }
            left.width=0.5f;
            leftCamera.m_camera.rect = left;
            right.x=0.5f;
            right.width = 0.5f;
            rightCamera.m_camera.rect = right;
            isCompressing = false;
        }
    }
}

