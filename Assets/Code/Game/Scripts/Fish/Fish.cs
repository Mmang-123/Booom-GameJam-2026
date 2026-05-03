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

    public enum EDieType
    {
        Hunger, Eaten, Other
    }

    public enum EInfectedLevel
    {
        None, Mid, High
    }

    public interface IFishController
    {
        public Fish Fish { get; }
        public void ControlFish(Fish fish);
        public void LoseControl(IFishController newController);
    }

    public class Fish : MonoBehaviour
    {
        [Header("精灵图设置")]
        [SerializeField] private EDirection m_EDirection;
        [SerializeField] private bool m_AutoFlip = true; // 应该仅对向左向右的朝向有用
        [SerializeField] private Transform m_FlipRoot;

        [Header("感染设置")]
        [SerializeField] private SpriteRenderer m_SporeRenderer1;
        [SerializeField] private SpriteRenderer m_SporeRenderer2;
        [SerializeField] private SpriteRenderer m_BodyRenderer;
        [SerializeField] private Color m_DefaultBodyColor;
        [SerializeField] private Color m_InfectedBodyColor;

        [Header("饱食度")]
        [SerializeField] private float m_InitSaturation = 100f;
        [SerializeField] private float m_MaxSaturation = 100f;

        [SerializeField] private EInfectedLevel m_InfectedLevel = EInfectedLevel.None;

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

        public IFishController FishController { get; private set; }
        public bool IsPlayer => FishController is PlayerController;

        private List<FishBehaviour> m_Behaviours;
        private Dictionary<System.Type, FishBehaviour> m_BehaviourMap = new();
        private bool m_FacingLeft = false; // 这里是相机角度的左右

        public float Saturation { get; private set; }
        public float MaxSaturation => m_MaxSaturation;

        public EDirection EDirection => m_EDirection;
        public Vector2 ForwardDirection => transform.rotation * s_DirectionMap[m_EDirection];
        public Vector2 Position => transform.position;

        public bool IsLiving => Saturation > 0f;

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

            // Infected
            UpdateInfectedView();

            // Saturation
            Saturation = Mathf.Clamp(m_InitSaturation, 0f, MaxSaturation);

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

        public void SetController(IFishController fishController)
        {
            if (FishController != null)
            {
                FishController.LoseControl(fishController);
            }
            FishController = fishController;
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

        public bool TryGetBehaviour<T>(out T outBehaviour) where T : FishBehaviour
        {
            outBehaviour = GetBehaviour<T>();
            return outBehaviour != null;
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

        #region 饥饿值

        public void RemoveSaturation(float value)
        {
            Saturation = Mathf.Max(0f, Saturation - value);
        }

        public void AddSaturation(float value)
        {
            Saturation = Mathf.Min(MaxSaturation, Saturation + value);
        }

        public void Die(EDieType dieType)
        {
            Saturation = 0f;
            if (dieType == EDieType.Eaten)
            {
                gameObject.SetActive(false);
                return;
            }
            else
            {
                // TODO: 感染扩散
                if (TryGetBehaviour<FB_GenericAnimator>(out var animatorBehaviour))
                {
                    animatorBehaviour.TriggerDieAnimation();
                }
            }
        }

        public bool Eat(Fish otherFish, float reduceSaturation)
        {
            if (otherFish.Saturation > reduceSaturation)
            {
                otherFish.RemoveSaturation(reduceSaturation);
                return false;
            }
            else
            {
                otherFish.Die(EDieType.Eaten);
                return true;
            }
        }

        #endregion
    
        #region 感染

        public void SetInfectedLevel(EInfectedLevel infectedLevel)
        {
            m_InfectedLevel = infectedLevel;
            UpdateInfectedView();
        }

        public void AddInfectedLevel()
        {
            if (m_InfectedLevel < EInfectedLevel.High)
                m_InfectedLevel++;
            UpdateInfectedView();
        }

        private void UpdateInfectedView()
        {
            if (m_SporeRenderer1 == null || m_SporeRenderer2 == null)
                return;

            if (m_InfectedLevel == EInfectedLevel.None)
            {
                m_SporeRenderer1.gameObject.SetActive(false);
                m_SporeRenderer2.gameObject.SetActive(false);
                m_BodyRenderer.color = m_DefaultBodyColor;
            }
            else if (m_InfectedLevel == EInfectedLevel.Mid)
            {
                m_SporeRenderer1.gameObject.SetActive(true);
                m_SporeRenderer2.gameObject.SetActive(false);
                m_BodyRenderer.color = m_DefaultBodyColor;
            }
            else if (m_InfectedLevel == EInfectedLevel.High)
            {
                m_SporeRenderer1.gameObject.SetActive(false);
                m_SporeRenderer2.gameObject.SetActive(true);
                m_BodyRenderer.color = m_InfectedBodyColor;
            }
        }

        #endregion
    
    }
}
