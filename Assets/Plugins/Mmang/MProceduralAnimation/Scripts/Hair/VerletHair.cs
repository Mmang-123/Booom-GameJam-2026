using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace Mmang.ProceduralAnimation
{
    public class VerletHair : MonoBehaviour
    {
        [Header("设置")]
        public List<Transform> HairBones = new();
        public bool AutoTick = true;

        [Header("物理参数")]
        [Tooltip("重力影响")]
        public Vector3 Gravity = new(0, -9.81f, 0);

        [Tooltip("阻力 (0-1)，越小头发越飘，越大越像在水里")]
        [Range(0, 1)]
        public float Drag = 0.9f;

        [Header("形状保持")]
        [Range(0, 1)]
        public float Stiffness = 0.2f;

        [Tooltip("迭代次数")]
        public int ConstraintIterations = 3;

        [Header("骨骼轴向设置")]
        public Vector3 BoneAxis = new Vector3(0, 1, 0);

        [Header("跑步律动 (Bouncing)")]
        public float BobPhaseOffset = 0.0f;

        [Tooltip("律动频率：跑得越快通常也要设得越高")]
        public float BobFrequency = 10.0f;

        [Tooltip("律动强度：上下颠簸的力度")]
        public float BobIntensity = 2.0f;

        [Header("风扰动")]
        public float WindPhaseOffset = 0.0f;
        public float WindSpeed = 1.0f;
        public float WindStrength = 1.0f;

        [Header("碰撞 (可选)")]
        public Transform CollisionSphere;
        [Tooltip("碰撞球体的半径")]
        public float CollisionRadius = 0.3f;

    
        // 内部类：存储每个节点的物理状态
        class Node
        {
            public Vector3 position;      // 当前位置
            public Vector3 prevPosition;  // 上一帧位置
            public float lengthToParent;  // 与父节点的距离
            public Transform transform;   // 对应的 Unity Transform
            public Vector3 initialLocalPos;
            public Quaternion initialLocalRot;
        }

        // 内部变量用于计算速度
        private Vector3 m_LastPos;
        private float m_CurrentSpeed;

        private List<Node> nodes = new List<Node>();
        private bool isInitialized = false;

        void Start()
        {
            InitHair();
        }

#if UNITY_EDITOR
        [ContextMenu("自动查找子骨骼")]
        void AutoFindBones()
        {
            HairBones.Clear();
            // 简单的递归查找所有子节点作为一条链
            Transform current = transform;
            while (current.childCount > 0)
            {
                HairBones.Add(current);
                current = current.GetChild(0); // 默认取第一个子节点
            }
            HairBones.Add(current); // 添加最后一个末端
            Debug.Log($"已自动找到 {HairBones.Count} 个骨骼节点");
            EditorUtility.SetDirty(this);
        }
#endif

        void InitHair()
        {
            if (HairBones.Count < 2) return;

            nodes.Clear();

            // 初始化节点数据
            for (int i = 0; i < HairBones.Count; i++)
            {
                Node node = new Node();
                node.transform = HairBones[i];
                node.position = HairBones[i].position;
                node.prevPosition = HairBones[i].position;
                node.initialLocalPos = transform.InverseTransformPoint(HairBones[i].position);
                node.initialLocalRot = HairBones[i].localRotation;

                // 计算与上一个节点的原始距离
                if (i > 0)
                {
                    node.lengthToParent = Vector3.Distance(HairBones[i].position, HairBones[i - 1].position);
                }

                nodes.Add(node);
            }
            isInitialized = true;
        }

        void FixedUpdate()
        {
            if (AutoTick)
            {
                Tick(Time.fixedDeltaTime);
            }
        }

        public void Tick(float dt)
        {
            if (!isInitialized) return;

            SimulatePhysics(dt);
            ApplyConstraints();
            SolveCollision();
        }

        void LateUpdate()
        {
            if (!isInitialized) return;
            ApplyToTransform();
        }

        void SimulatePhysics(float dt)
        {
            // 根节点处理保持不变
            nodes[0].position = HairBones[0].position;


            // 计算角色水平移动速度
            Vector3 curPos = HairBones[0].position;
            Vector3 horizontalDelta = curPos - m_LastPos;
            horizontalDelta.y = 0; // 只看水平位移
            m_CurrentSpeed = horizontalDelta.magnitude / dt; // 米/秒
            m_LastPos = curPos;

            // 只有当角色移动速度大于 0.1 时才施加律动
            Vector3 bobForce = Vector3.zero;
            if (m_CurrentSpeed > 0.1f)
            {
                float wave = (Mathf.Sin(Time.time * BobFrequency + BobPhaseOffset) - 1f) * 0.5f;
                float speedFactor = Mathf.Clamp(m_CurrentSpeed, 0, 5f);

                bobForce = new Vector3(0, wave * BobIntensity * speedFactor, 0);
            }


            for (int i = 1; i < nodes.Count; i++)
            {
                Node node = nodes[i];

                float noise = Mathf.PerlinNoise(Time.time * WindSpeed, i * 0.5f + WindPhaseOffset); 
                Vector3 windForce = new Vector3(1, 0, 0) * (noise - 0.5f) * WindStrength * (1f + Mathf.Clamp(m_CurrentSpeed, 0f, 0.1f) * 10f);

                // 物理计算
                Vector3 velocity = (node.position - node.prevPosition) * Drag;
                Vector3 tempPos = node.position;
                Vector3 physicsPos = node.position + velocity + (Gravity * dt * dt) + ((bobForce + windForce) * dt * dt);

                // 如果不受力的世界坐标
                Vector3 targetShapePos = this.transform.TransformPoint(node.initialLocalPos);

                // 混合
                Vector3 finalPos = Vector3.Lerp(physicsPos, targetShapePos, Stiffness);

                // 更新数据
                node.prevPosition = tempPos;
                node.position = finalPos;
            }
        }

        // 2. 约束求解 (保证头发不被拉长)
        void ApplyConstraints()
        {
            // 迭代多次可以增加刚度 (Stiffness)
            for (int k = 0; k < ConstraintIterations; k++)
            {
                // 第 0 个节点强制锁定在发根位置
                nodes[0].position = HairBones[0].position;

                for (int i = 1; i < nodes.Count; i++)
                {
                    Node parent = nodes[i - 1];
                    Node child = nodes[i];

                    Vector3 direction = child.position - parent.position;
                    float currentDist = direction.magnitude;
                    float targetDist = child.lengthToParent;

                    // 如果距离不对，拉回来
                    if (currentDist > 0.0001f) // 防止除以0
                    {
                        float difference = (currentDist - targetDist) / currentDist;
                        Vector3 correction = direction * difference;

                        if (i == 1)
                        {
                            child.position -= correction;
                        }
                        else
                        {
                            parent.position += correction * 0.5f;
                            child.position -= correction * 0.5f;
                        }
                    }
                }
            }
        }

        // 简单的球体碰撞 (防止穿过头部/身体)
        void SolveCollision()
        {
            if (CollisionSphere == null) return;

            Vector3 spherePos = CollisionSphere.position;

            for (int i = 1; i < nodes.Count; i++)
            {
                Vector3 dir = nodes[i].position - spherePos;
                float dist = dir.magnitude;

                if (dist < CollisionRadius)
                {
                    // 如果在球内部，推挤到球表面
                    Vector3 pushDir = dir.normalized;
                    nodes[i].position = spherePos + pushDir * CollisionRadius;

                    // 也可以把 prevPosition 重置，防止碰撞后乱飞
                    // nodes[i].prevPosition = nodes[i].position; 
                }
            }
        }

        void ApplyToTransform()
        {
            for (int i = 0; i < nodes.Count - 1; i++)
            {
                Node currentNode = nodes[i];
                Node nextNode = nodes[i + 1];

                // 1. 计算【原始造型方向】(Rest Direction)
                // 即：如果不受物理影响，骨骼应该指向哪里？
                Transform parent = (i == 0) ? transform.parent : nodes[i - 1].transform;
                //Quaternion parentRot = (i == 0) ? this.transform.rotation : nodes[i-1].transform.rotation;
                Quaternion restRot = parent == null ? currentNode.initialLocalRot : (parent.rotation * currentNode.initialLocalRot);
                Vector3 restDir = restRot * BoneAxis;

                // 2. 计算【物理目标方向】(Physics Direction)
                Vector3 targetDir = nextNode.position - currentNode.transform.position;

                // 安全检查：防止向量长度为0导致报错
                if (targetDir.sqrMagnitude < 0.0001f) targetDir = restDir; // 如果出错就用原始方向兜底

                // 混合向量
                Vector3 finalDir = Vector3.Slerp(targetDir.normalized, restDir.normalized, Stiffness);

                // 应用旋转
                Quaternion swing = Quaternion.FromToRotation(restDir, finalDir);
                currentNode.transform.rotation = swing * restRot;

                // 物理粒子位置
                Vector3 boneEndPos = currentNode.transform.position + (currentNode.transform.rotation * BoneAxis * nextNode.lengthToParent);
                nextNode.position = boneEndPos;
            }
        }
    }
}