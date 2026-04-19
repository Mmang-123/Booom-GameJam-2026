using UnityEngine;

namespace Mmang.Util
{
    public static class TransformUtil
    {
        public static Transform TransformHelper { get; private set; }

        public static void InitTransformer()
        {
            if (TransformHelper == null)
            {
                var curGO = GameObject.Find("Transform Helper");
                if (curGO != null)
                {
                    TransformHelper = curGO.transform;
                    return;
                }
            }
            if (TransformHelper != null && TransformHelper.gameObject.activeInHierarchy)
                return;

            var go = new GameObject("Transform Helper");
            TransformHelper = go.transform;
            if (Application.isPlaying)
                GameObject.DontDestroyOnLoad(go);
        }

        public static Transform GetHelper()
        {
            InitTransformer();
            return TransformHelper;
        }

        public static void Setup(Vector3 position, Quaternion rotation)
        {
            InitTransformer();
            TransformHelper.position = position;
            TransformHelper.rotation = rotation;
        }

        public static Vector3 PositionWorldToLocal(Vector3 sourcePosition)
            => TransformHelper.InverseTransformPoint(sourcePosition);

        public static Vector3 PositionLocalToWorld(Vector3 sourcePosition)
            => TransformHelper.TransformPoint(sourcePosition);

        public static Vector3 DirectionWorldToLocal(Vector3 sourceDirection)
            => TransformHelper.InverseTransformDirection(sourceDirection);

        public static Vector3 DirectionLocalToWorld(Vector3 sourceDirection)
            => TransformHelper.TransformDirection(sourceDirection);

        public static Vector3 PositionWorldToLocal(Vector3 sourcePosition, Vector3 position, Quaternion rotation)
        {
            InitTransformer();
            TransformHelper.position = position;
            TransformHelper.rotation = rotation;
            return TransformHelper.InverseTransformPoint(sourcePosition);
        }

        public static Vector3 PositionLocalToWorld(Vector3 sourcePosition, Vector3 position, Quaternion rotation)
        {
            InitTransformer();
            TransformHelper.position = position;
            TransformHelper.rotation = rotation;
            return TransformHelper.TransformPoint(sourcePosition);
        }

        public static Vector3 DirectionLocalToWorld(Vector3 direction, Quaternion rotation)
        {
            InitTransformer();
            TransformHelper.rotation = rotation;
            return TransformHelper.TransformDirection(direction);
        }

        public static Vector3 DirectionWorldToLocal(Vector3 direction, Quaternion rotation)
        {
            InitTransformer();
            TransformHelper.rotation = rotation;
            return TransformHelper.InverseTransformDirection(direction);
        }

        public static Vector3 DirectionLocalToWorldOnPlane(Vector2 sourceDirection, Quaternion rotation)
        {
            InitTransformer();
            Vector3 directionOnPlane = rotation * Vector3.forward;
            directionOnPlane.y = 0f;
            directionOnPlane.Normalize();
            TransformHelper.rotation = Quaternion.LookRotation(directionOnPlane);
            return TransformHelper.TransformDirection(Direction2To3(sourceDirection));
        }

        public static Vector3 Direction2To3(Vector2 direction) => new(direction.x, 0f, direction.y);
        public static Vector3 Direction2To3Normalized(Vector2 direction) => new Vector3(direction.x, 0f, direction.y).normalized;
        public static Vector2 Direction3To2(Vector3 direction) => new(direction.x, direction.z);
        public static Vector2 Direction3To2Normalized(Vector3 direction) => new Vector2(direction.x, direction.z).normalized;
        public static Vector3 DirectionToPlane(Vector3 direction) => new Vector3(direction.x, 0f, direction.z).normalized;
    
        #region 
        public static void SetLocalScaleX(this Transform transform, float x)
        {
            var scale = transform.localScale;
            scale.x = x;
            transform.localScale = scale;
        }

        public static void SetLocalScaleY(this Transform transform, float y)
        {
            var scale = transform.localScale;
            scale.y = y;
            transform.localScale = scale;
        }

        public static void SetLocalScaleZ(this Transform transform, float z)
        {
            var scale = transform.localScale;
            scale.z = z;
            transform.localScale = scale;
        }
        #endregion

        #region Vector拓展

        public static Vector2 Divide(this Vector2 a, Vector2 b)
        {
            return new(a.x / b.x, a.y / b.y);
        }

        public static Vector3 Divide(this Vector3 a, Vector3 b)
        {
            return new(a.x / b.x, a.y / b.y, a.z / b.z);
        }

        public static Vector4 Divide(this Vector4 a, Vector4 b)
        {
            return new(a.x / b.x, a.y / b.y, a.z / b.z, a.w / b.w);
        }

        public static Vector2 Multi(this Vector2 a, Vector2 b)
        {
            a.Scale(b);
            return a;
        }

        public static Vector3 Multi(this Vector3 a, Vector3 b)
        {
            a.Scale(b);
            return a;
        }

        public static Vector4 Multi(this Vector4 a, Vector4 b)
        {
            a.Scale(b);
            return a;
        }

        public static Vector2 Divide(this Vector2 a, float bx, float by)
        {
            return new(a.x / bx, a.y / by);
        }

        public static Vector3 Divide(this Vector3 a, float bx, float by, float bz)
        {
            return new(a.x / bx, a.y / by, a.z / bz);
        }

        public static Vector4 Divide(this Vector4 a, float bx, float by, float bz, float bw)
        {
            return new(a.x / bx, a.y / by, a.z / bz, a.w / bw);
        }

        public static Vector2 Multi(this Vector2 a, float bx, float by)
        {
            return new(a.x * bx, a.y * by);
        }

        public static Vector3 Multi(this Vector3 a, float bx, float by, float bz)
        {
            return new(a.x * bx, a.y * by, a.z * bz);
        }

        public static Vector4 Multi(this Vector4 a, float bx, float by, float bz, float bw)
        {
            return new(a.x * bx, a.y * by, a.z * bz, a.w * bw);
        }

        public static Vector3 GetScaleInScene(this Transform transform)
        {
            if (transform.parent == null)
                return Vector3.zero;
            return transform.parent.GetScaleInScene().Multi(transform.localScale);
        }


        #endregion


        #region 四元数和旋转矩阵

        public static Matrix4x4 QuaternionsToRotationMatrix(Quaternion quaternion)
        {
            return Matrix4x4.TRS(Vector3.zero, quaternion, Vector3.one);
        }

        public static Quaternion RotationMatrixToQuaternions(Matrix4x4 matrix4X4)
        {
            Vector4 vy = matrix4X4.GetColumn(1);
            Vector4 vz = matrix4X4.GetColumn(2);
            return Quaternion.LookRotation(new(vz.x, vz.y, vz.z), new(vy.x, vy.y, vy.z));
        }

        #endregion
    }
}