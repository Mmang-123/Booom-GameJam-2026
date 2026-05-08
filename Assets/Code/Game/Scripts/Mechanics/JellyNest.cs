using System.Collections.Generic;
using Mmang.Util;
using UnityEngine;

namespace Game
{
    public class JellyNest : MonoBehaviour
    {
        [SerializeField] private FishAIComponent m_AIPrefab;
        [SerializeField] private Fish m_FishPrefab;
        
        [Header("基础设置")]
        [SerializeField] private bool m_InitGenerate = true;
        [SerializeField] private CircleCollider2D m_MoveRange;
        [SerializeField] private int m_MaxCount = 3;
        [SerializeField] private Vector2 m_GenerateIntervalTime = new(2.5f, 4.5f);

        [Header("生成时")]
        [SerializeField] private List<Transform> m_GeneratePoints = new();
        [SerializeField] private Vector2 m_InitDirection;
        [SerializeField] private Vector2 m_InitDirectionRandomAngleRange = new(-20f, 20f);
        [SerializeField] private float m_InitSpeed = 5f;


        // Runtime
        private float m_Timer;
        private float m_IntervalTime;
        private List<Fish> m_LivingList = new();

        private void Start()
        {
            m_IntervalTime = RandomUtil.GetRandomValueInRange(m_GenerateIntervalTime);

            if (m_InitGenerate)
            {
                for (int i = 0; i < m_MaxCount; i++)
                {
                    CreateNewJelly();
                }
            }
        }

        private void FixedUpdate()
        {
            UpdateLiving();
            
            if (m_LivingList.Count < m_MaxCount)
            {
                m_Timer += Time.fixedDeltaTime;
                if (m_Timer >= m_IntervalTime)
                {
                    m_IntervalTime = RandomUtil.GetRandomValueInRange(m_GenerateIntervalTime);
                    m_Timer = 0f;

                    CreateNewJelly();
                }
            }
            else
            {
                m_Timer = 0f;
            }
        }

        private void UpdateLiving()
        {
            for (int i = m_LivingList.Count - 1; i >= 0; i--)
            {
                if (m_LivingList[i] == null || !m_LivingList[i].IsLiving
                || m_LivingList[i].IsPlayer)
                {
                    m_LivingList.RemoveAt(i);
                }
            }
        }

        [ContextMenu("Create New Jelly")]
        private void CreateNewJelly()
        {
            Vector2 position = GetGeneratePoint();
            Quaternion rotation = m_FishPrefab.GetRotation(m_InitDirection.normalized, RandomUtil.GetRandomValueInRange(m_InitDirectionRandomAngleRange));

            var pair = FishUtils.Create(m_AIPrefab, m_FishPrefab, position, rotation, inLevelRoot: GameManager.Instance.LevelValid);
            m_LivingList.Add(pair.fish);

            // 设置范围
            if (pair.ai.TryGetAbility<FA_FreeMove>(out var freeMoveAbility))
            {
                freeMoveAbility.SetRange(m_MoveRange);
            }

            // 动画
            if (pair.fish.TryGetBehaviour<FB_GenericAnimator>(out var genericAnimator))
            {
                genericAnimator.TriggerCustomAnimation("Summon");
            }

            // 初始速度
            if (pair.fish.TryGetBehaviour<FB_Swim>(out var swimBehaviour))
            {
                var velocity = AdditionalVelocity.Create(pair.fish.ForwardDirection * m_InitSpeed, m_InitSpeed * 3f);
                swimBehaviour.AddAdditionalVelocity(velocity);
            }
        }

        private Vector2 GetGeneratePoint()
        {
            if (m_GeneratePoints.Count > 0)
            {
                int index = Random.Range(0, m_GeneratePoints.Count);
                return m_GeneratePoints[index].position;
            }

            return (Vector2)transform.position + 0.1f * Vector2.up;
        }
    }
}