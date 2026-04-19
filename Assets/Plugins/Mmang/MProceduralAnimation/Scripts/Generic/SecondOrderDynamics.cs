using UnityEngine;

namespace Mmang.ProceduralAnimation
{

    [System.Serializable]
    public struct SecondOrderDynamicsSetting
    {
        [Range(0.1f, 10f)]
        public float F;
        [Range(0f, 2f)]
        public float Z;
        [Range(-5f, 5f)]
        public float R;
    }

    public abstract class SecondOrderDynamics<T>
    {
        protected T m_XP; // 上一帧记录
        protected T m_Y, m_YD;
        protected float m_K1, m_K2, m_K3;

        public SecondOrderDynamics(float f, float z, float r, T x0)
        {
            UpdateAttribute(f, z, r);

            m_XP = x0;
            m_Y = x0;
            m_YD = default;
        }

        public void UpdateAttribute(SecondOrderDynamicsSetting setting)
            => UpdateAttribute(setting.F, setting.Z, setting.R);
        public void UpdateAttribute(float f, float z, float r)
        {
            m_K1 = z / (Mathf.PI * f);
            m_K2 = 1 / (2 * Mathf.PI * f * (2 * Mathf.PI * f));
            m_K3 = r * z / (2 * Mathf.PI * f);
        }

        public abstract T Update(float dt, T x);
        public abstract T Update(float dt, T x, T xd);
    }


    #region 实现
    public sealed class FloatSecondOrderDynamics : SecondOrderDynamics<float>
    {
        public FloatSecondOrderDynamics(SecondOrderDynamicsSetting setting, float x0) : base(setting.F, setting.Z, setting.R, x0) { }
        public FloatSecondOrderDynamics(float f, float z, float r, float x0) : base(f, z, r, x0) { }

        public override float Update(float dt, float x)
        {
            float xd;
            xd = (x - m_XP) / dt;
            m_XP = x;
            return Update(dt, x, xd);
        }

        public override float Update(float dt, float x, float xd)
        {
            float k2_stable = Mathf.Max(m_K2, 1.1f * (dt * dt / 4 + dt * m_K1 / 2));
            m_Y += dt * m_YD;
            m_YD += dt * (x + m_K3 * xd - m_Y - m_K1 * m_YD) / k2_stable;
            return m_Y;
        }
    }

    public sealed class Vector2SecondOrderDynamics : SecondOrderDynamics<Vector2>
    {
        public Vector2SecondOrderDynamics(SecondOrderDynamicsSetting setting, Vector2 x0) : base(setting.F, setting.Z, setting.R, x0) { }
        public Vector2SecondOrderDynamics(float f, float z, float r, Vector2 x0) : base(f, z, r, x0) { }

        public override Vector2 Update(float dt, Vector2 x)
        {
            Vector2 xd;
            xd = (x - m_XP) / dt;
            m_XP = x;
            return Update(dt, x, xd);
        }

        public override Vector2 Update(float dt, Vector2 x, Vector2 xd)
        {
            float k2_stable = Mathf.Max(m_K2, 1.1f * (dt * dt / 4 + dt * m_K1 / 2));
            m_Y += dt * m_YD;
            m_YD += dt * (x + m_K3 * xd - m_Y - m_K1 * m_YD) / k2_stable;
            return m_Y;
        }
    }

    public sealed class Vector3SecondOrderDynamics : SecondOrderDynamics<Vector3>
    {
        public Vector3SecondOrderDynamics(SecondOrderDynamicsSetting setting, Vector3 x0) : base(setting.F, setting.Z, setting.R, x0) { }
        public Vector3SecondOrderDynamics(float f, float z, float r, Vector3 x0) : base(f, z, r, x0) { }

        public override Vector3 Update(float dt, Vector3 x)
        {
            Vector3 xd;
            xd = (x - m_XP) / dt;
            m_XP = x;
            return Update(dt, x, xd);
        }

        public override Vector3 Update(float dt, Vector3 x, Vector3 xd)
        {
            float k2_stable = Mathf.Max(m_K2, 1.1f * (dt * dt / 4 + dt * m_K1 / 2));
            m_Y += dt * m_YD;
            m_YD += dt * (x + m_K3 * xd - m_Y - m_K1 * m_YD) / k2_stable;
            return m_Y;
        }
    }

    public sealed class Vector4SecondOrderDynamics : SecondOrderDynamics<Vector4>
    {
        public Vector4SecondOrderDynamics(SecondOrderDynamicsSetting setting, Vector4 x0) : base(setting.F, setting.Z, setting.R, x0) { }
        public Vector4SecondOrderDynamics(float f, float z, float r, Vector4 x0) : base(f, z, r, x0) { }

        public override Vector4 Update(float dt, Vector4 x)
        {
            Vector4 xd;
            xd = (x - m_XP) / dt;
            m_XP = x;
            return Update(dt, x, xd);
        }

        public override Vector4 Update(float dt, Vector4 x, Vector4 xd)
        {
            float k2_stable = Mathf.Max(m_K2, 1.1f * (dt * dt / 4 + dt * m_K1 / 2));
            m_Y += dt * m_YD;
            m_YD += dt * (x + m_K3 * xd - m_Y - m_K1 * m_YD) / k2_stable;
            return m_Y;
        }
    }

    #endregion


    /* OLD
    public class SecondOrderDynamics : SecondOrderDynamicsBase
    {
        private Vector3 xp; //上一帧位置
        private Vector3 y, yd;

        public SecondOrderDynamics(SecondOrderDynamicsSetting setting, Vector3 x0)
            : this(setting.F, setting.Z, setting.R, x0) { }
        public SecondOrderDynamics(float f, float z, float r, Vector3 x0)
        {
            UpdateAttribute(f, z, r);

            xp = x0;
            y = x0;
            yd = Vector3.zero;
        }

        public Vector3 Update(float T, Vector3 x)
        {
            Vector3 xd;
            xd = (x - xp) / T;
            xp = x;
            return Update(T, x, xd);
        }
        public Vector3 Update(float T, Vector3 x, Vector3 xd)
        {
            //float k2_stable = Mathf.Max(m_K2,Mathf.Max(T*T/2+T*m_K1/2,T*m_K1));
            float k2_stable = Mathf.Max(m_K2, 1.1f * (T * T / 4 + T * m_K1 / 2));
            y += T * yd;
            yd += T * (x + m_K3 * xd - y - m_K1 * yd) / k2_stable;
            return y;
        }
    }


    public class SecondOrderDynamics_Float : SecondOrderDynamicsBase
    {
        private float xp; //上一帧位置
        private float y, yd;

        public SecondOrderDynamics_Float(SecondOrderDynamicsSetting setting, float x0)
            : this(setting.F, setting.Z, setting.R, x0) { }
        public SecondOrderDynamics_Float(float f, float z, float r, float x0)
        {
            UpdateAttribute(f, z, r);

            xp = x0;
            y = x0;
            yd = 0;
        }

        public float Update(float T, float x)
        {
            float xd;
            xd = (x - xp) / T;
            xp = x;
            return Update(T, x, xd);
        }
        public float Update(float T, float x, float xd)
        {
            float k2_stable = Mathf.Max(m_K2, 1.1f * (T * T / 4 + T * m_K1 / 2));
            y += T * yd;
            yd += T * (x + m_K3 * xd - y - m_K1 * yd) / k2_stable;
            return y;
        }
    }

    */
}
