using System.Collections.Generic;
using System.Linq;
using Mmang.Game;
using Mmang.Generic;
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

    public class Fish : MonoBehaviour, ILevelSavable
    {
        [SerializeField] private GameplayTag m_FishTypeTag;

        [Header("精灵图设置")]
        [SerializeField] private EDirection m_EDirection;
        [SerializeField] private bool m_AutoFlip = true; // 应该仅对向左向右的朝向有用
        [SerializeField] private Transform m_FlipRoot;

        [Header("挤压死亡设置")]
        [SerializeField] private CircleCollider2D m_DieCollider;
        [SerializeField] private float m_DieCollisionTime = 0.1f;
        [SerializeField] private float m_DieCollisionMaxSpeed = 1f;
        [SerializeField] private ParticleComponent m_DieCollisionParticle;

        [Header("感染设置")]
        [SerializeField] private EInfectedLevel m_InfectedLevel = EInfectedLevel.None;
        [SerializeField] private SpriteRenderer m_SporeRenderer1;
        [SerializeField] private SpriteRenderer m_SporeRenderer2;
        [SerializeField] private SpriteRenderer m_BodyRenderer;
        [SerializeField] private InterfaceObject<IMLight> m_Light;
        [SerializeField] private Color m_DefaultBodyColor;
        [SerializeField] private Color m_InfectedBodyColor;
        [SerializeField] private bool m_SetLightColor = false;
        [SerializeField] private Color m_DefaultLightColor;
        [SerializeField] private Color m_InfectedLightColor;
        

        [Header("饱食度")]
        [SerializeField] private float m_InitSaturation = 100f;
        [SerializeField] private float m_MaxSaturation = 100f;

        [Header("粒子设置")]
        [SerializeField] private ParticleComponent m_EatenParticle;
        [SerializeField] private Color m_DefaultEatenParticleColor;
        [SerializeField] private Color m_InfectedEatenParticleColor;
        public ParticleComponent EatenParticle => m_EatenParticle;
        public Color DefaultEatenParticleColor => m_DefaultEatenParticleColor;
        public Color InfectedEatenParticleColor => m_InfectedEatenParticleColor;


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

        public GameplayTag FishTypeTag => m_FishTypeTag;

        private List<FishBehaviour> m_Behaviours;
        private Dictionary<System.Type, FishBehaviour> m_BehaviourMap = new();
        public IReadOnlyList<FishBehaviour> Behaviours => m_Behaviours;

        private bool m_FacingLeft = false; // 这里是相机角度的左右

        private float m_DieCollisionTimer;
        private Vector2 m_CollisionLastFramePosition;

        public float Saturation { get; private set; }
        public float MaxSaturation => m_MaxSaturation;

        public EInfectedLevel InfectedLevel => m_InfectedLevel;

        public EDirection EDirection => m_EDirection;
        public Vector2 ForwardDirection => transform.rotation * s_DirectionMap[m_EDirection];
        public Vector2 Position => transform.position;

        private bool m_Transfered = false;
        private Vector2 m_TransferPosition;

        public bool Eaten { get; set; }
        public bool DontSave { get; set; } = false;

        private bool m_Dead = false;
        public bool IsLiving => !m_Dead;

        private Vector2 m_TotalMotion;

        private void Start()
        {
            GameManager.Instance.TryLoadSavedData(this);

            if (gameObject.activeSelf)
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
                if (!behaviour.enabled)
                    continue;

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
            if (m_Dead)
                return;

            foreach (var behaviour in m_Behaviours)
            {
                if (behaviour.enabled)
                    behaviour.BeforeFishUpdate();
            }
        }

        private void FixedUpdate()
        {
            if (m_Dead)
                return;

            foreach (var behaviour in m_Behaviours)
            {
                if (behaviour.enabled)
                    behaviour.BeforeFishFixedUpdate();
            }
            if (m_Transfered)
            {
                m_Transfered = false;
                Vector2 newPos = m_TotalMotion + m_TransferPosition;
                m_Rigidbody.position = newPos;
            }
            else
            {
                m_Rigidbody.MovePosition(m_TotalMotion + (Vector2)transform.position);    
            }
            
            m_TotalMotion = Vector2.zero;

            CheckDieCollision(Time.fixedDeltaTime);
        }

        public void SetPosition(Vector2 position)
        {
            m_Transfered = true;
            m_TransferPosition = position;
            //m_Rigidbody.position = position;
        }

        #region Behaviour

        public T GetBehaviour<T>() where T : FishBehaviour
        {
            Init();
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

        #endregion


        #region RB

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

        public float GetRotationEulerAngle(Vector2 direction, float offset = 0f)
        {
            float offsetAngle = 0f;
            switch (EDirection)
            {
                case EDirection.Up:
                    offsetAngle = -90f;
                    break;
                case EDirection.Down:
                    offsetAngle = 90f;
                    break;
                case EDirection.Left:
                    offsetAngle = 180f;
                    break;
            }
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + offsetAngle;
            return targetAngle + offset;
        }

        public Quaternion GetRotation(Vector2 direction, float offset = 0f)
        {
            float offsetAngle = 0f;
            switch (EDirection)
            {
                case EDirection.Up:
                    offsetAngle = -90f;
                    break;
                case EDirection.Down:
                    offsetAngle = 90f;
                    break;
                case EDirection.Left:
                    offsetAngle = 180f;
                    break;
            }

            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + offsetAngle;
            Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle + offset);
            return targetRotation;
        }

        public void Move(Vector2 motion)
        {
            m_TotalMotion += motion;
        }

        #endregion

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
            if (m_Dead)
                return;

            Debug.Log("Die");

            if (FishController != null)
            {
                SetController(null);
            }

            m_Dead = true;
            Saturation = 0f;
            
            if (!DontSave)
            {
                GameManager.Instance.Save(this);
            }

            int dieEffect;
            if (dieType == EDieType.Eaten)
            {
                dieEffect = 0;
            }
            else if (dieType == EDieType.Hunger)
            {
                if (FishTypeTag.Equals(FishUtils.JellyGleamTag))
                {
                    dieEffect = 2;
                }
                else
                {
                    dieEffect = 1;
                }
            }
            else
            {
                dieEffect = 2;
            }

            if (dieEffect == 0)
            {
                //gameObject.SetActive(false);
                Destroy(gameObject);
            }
            else if (dieEffect == 1)
            {
                if (TryGetBehaviour<FB_GenericAnimator>(out var animatorBehaviour))
                {
                    animatorBehaviour.TriggerDieAnimation();
                }
                // temp
                Destroy(gameObject);
            }
            else
            {
                //gameObject.SetActive(false);
                if (m_DieCollisionParticle != null)
                {
                    var particle = ParticleUtils.Create(m_DieCollisionParticle, Position, Quaternion.Euler(-90f, 0f, 0f));
                    particle.SetOverrideColor(m_InfectedLevel >= EInfectedLevel.High ? m_InfectedBodyColor : m_DefaultBodyColor);
                    particle.StartPlay();
                }
                Destroy(gameObject);
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
            if (m_InfectedLevel == EInfectedLevel.None)
            {
                if (m_SporeRenderer1 != null) m_SporeRenderer1.gameObject.SetActive(false);
                if (m_SporeRenderer2 != null) m_SporeRenderer2.gameObject.SetActive(false);
                if (m_BodyRenderer != null) m_BodyRenderer.color = m_DefaultBodyColor;
                if (m_Light.Value != null && m_SetLightColor) m_Light.Value.LightColor = m_DefaultLightColor;
            }
            else if (m_InfectedLevel == EInfectedLevel.Mid)
            {
                if (m_SporeRenderer1 != null) m_SporeRenderer1.gameObject.SetActive(true);
                if (m_SporeRenderer2 != null) m_SporeRenderer2.gameObject.SetActive(false);
                if (m_BodyRenderer != null) m_BodyRenderer.color = m_DefaultBodyColor;
                if (m_Light.Value != null && m_SetLightColor) m_Light.Value.LightColor = m_DefaultLightColor;
            }
            else if (m_InfectedLevel == EInfectedLevel.High)
            {
                if (m_SporeRenderer1 != null) m_SporeRenderer1.gameObject.SetActive(false);
                if (m_SporeRenderer2 != null) m_SporeRenderer2.gameObject.SetActive(true);
                if (m_BodyRenderer != null) m_BodyRenderer.color = m_InfectedBodyColor;
                if (m_Light.Value != null && m_SetLightColor) m_Light.Value.LightColor = m_InfectedLightColor;
            }
        }

        #endregion


        #region 挤压死亡

        private void CheckDieCollision(float dt)
        {
            if (m_DieCollider == null)
                return;
            
            Vector2 center = Position + m_DieCollider.offset;
            float radius = m_DieCollider.radius;

            int count = FishUtils.OverlapCircleObstacle(center, radius, out var colliders);
            if (count > 0)
            {
                float speed = Vector2.Distance(Position, m_CollisionLastFramePosition) / dt;
                m_DieCollisionTimer += dt;
                if (m_DieCollisionTimer >= m_DieCollisionTime && speed <= m_DieCollisionMaxSpeed)
                {
                    Die(EDieType.Other);
                }
            }
            else
            {
                m_DieCollisionTimer = 0f;
            }

            m_CollisionLastFramePosition = Position;
        }

        #endregion

        #region 保存和加载

        [SerializeField, HideInInspector] private string m_GUID = System.Guid.NewGuid().ToString();
        public string GUID => m_GUID;

        public virtual string SaveJson()
        {
            var saveData = new FishSaveData()
            {
                Exist = !m_Dead && !IsPlayer && !Eaten
            };

            string json = JsonUtility.ToJson(saveData);
            return json;
        }

        public virtual void LoadJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                return;

            var saveData = JsonUtility.FromJson<FishSaveData>(json);

            if (!saveData.Exist)
            {
                gameObject.SetActive(false);
                m_Dead = true;
            }
        }

#if UNITY_EDITOR
        public void Editor_SetGUID(string newGUID) => m_GUID = newGUID;
#endif

        #endregion
    }
}
