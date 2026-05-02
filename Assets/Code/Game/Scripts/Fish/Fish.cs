using System.Collections.Generic;
using System.Linq;
using Mmang.Game;
using UnityEngine;
using UnityEngine.Pool;

namespace Game
{

    public enum EDirection
    {
        Up, Down, Left, Right
    }

    public class Fish : MonoBehaviour
    {
        [SerializeField] private EDirection m_EDirection;
        [SerializeField] private bool m_AutoFlip = true; // 应该仅对向左向右的朝向有用
        [SerializeField] private Transform m_FlipRoot;

        // Static
        private static Dictionary<EDirection, Vector2> s_DirectionMap = new()
        {
            [EDirection.Up] = Vector2.up,
            [EDirection.Down] = Vector2.down,
            [EDirection.Left] = Vector2.left,
            [EDirection.Right] = Vector2.right
        };


        // Runtime
        private bool m_Inited = false;
        private Rigidbody2D m_Rigidbody;

        private List<FishBehaviour> m_Behaviours;
        private Dictionary<System.Type, FishBehaviour> m_BehaviourMap = new();
        private bool m_FacingLeft = false; // 这里是相机角度的左右

        private List<Collider2D> m_Colliders = new();

        public EDirection EDirection => m_EDirection;
        public Vector2 ForwardDirection => transform.rotation * s_DirectionMap[m_EDirection];
        public Vector2 Position => transform.position;

        private Vector2 m_TotalMotion;

        private void Start()
        {
            Init();
        }

        public void Init()
        {
            if (m_Inited)
                return;
            m_Inited = true;
            m_Rigidbody = GetComponent<Rigidbody2D>();

            // Collider
            var colliders = GetComponentsInChildren<Collider2D>();
            HashSet<GameObject> goSet = HashSetPool<GameObject>.Get();
            foreach (var collider in colliders)
            {
                if (goSet.Contains(collider.gameObject))
                    continue;
                
                goSet.Add(collider.gameObject);
                var component = collider.gameObject.AddComponent<FishCollider>();
                component.Init(this);
            }

            HashSetPool<GameObject>.Release(goSet);

            // Behaviour
            m_Behaviours = GetComponents<FishBehaviour>().ToList();
            m_BehaviourMap.Clear();

            foreach (var behaviour in m_Behaviours)
            {
                m_BehaviourMap.Add(behaviour.GetType(), behaviour);
                behaviour.Init(this);
            }
        }

        private void Update()
        {
            foreach (var behaviour in m_Behaviours)
            {
                behaviour.BeforeFishUpdate();
            }
        }

        private void FixedUpdate()
        {
            foreach (var behaviour in m_Behaviours)
            {
                behaviour.BeforeFishFixedUpdate();
            }
            m_Rigidbody.MovePosition(m_TotalMotion + (Vector2)transform.position);
            m_TotalMotion = Vector2.zero;
        }

        public T GetBehaviour<T>() where T : FishBehaviour
        {
            if (m_BehaviourMap.TryGetValue(typeof(T), out var result))
            {
                return result as T;
            }

            // 这里是个危险的设计: 可能存在继承同一个基类的两个Behaviour
            foreach (var behaviour in m_Behaviours)
            {
                if (behaviour is T tBehaviour)
                {
                    m_BehaviourMap.Add(typeof(T), behaviour);
                    return tBehaviour;
                }
            }

            return null;
        }

        public void SetRotation(Quaternion rotation)
        {
            transform.rotation = rotation;
            if (m_AutoFlip)
            {
                var forward = ForwardDirection;
                bool facingLeft = forward.x < 0f;

                if (facingLeft != m_FacingLeft)
                {
                    m_FacingLeft = facingLeft;
                    if ((facingLeft && m_EDirection == EDirection.Left)
                    || (!facingLeft && m_EDirection == EDirection.Right))
                    {
                        m_FlipRoot.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                    }
                    else
                    {
                        m_FlipRoot.transform.localRotation = Quaternion.Euler(180f, 0f, 0f);
                    }
                }
            }
        }

        public void Move(Vector2 motion)
        {
            m_TotalMotion += motion;
        }

        public void SetVelocity(Vector2 velocity)
        {
            
        }
    }
}
