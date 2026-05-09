
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    using EState = LoopZone.EState;

    public class LostArea : MonoBehaviour
    {
        public enum EDirection { Up = 0, Right = 1, Down = 2 }
        [SerializeField] private LoopZone m_LoopZone;
        [SerializeField] private float m_LeftOffset = 8f;
        [SerializeField] private List<int> m_Answer = new() { 1, 2, 3, 2 };
        [SerializeField] private List<Sprite> m_CodeSprites = new();

        [SerializeField] private SpriteRenderer m_UpMarker;
        [SerializeField] private SpriteRenderer m_RightMarker;
        [SerializeField] private SpriteRenderer m_DownMarker;

        // Runtime
        private bool m_Active = false;
        private int m_SuccessCount = 0;
        private EDirection m_TargetDirection;
        private bool m_Completed = false;

        private void Start()
        {
            if (m_LoopZone)
            {
                m_LoopZone.BeforeApplyFunc = BeforeApplyNewPosition;
            }
        }

        private void FixedUpdate()
        {
            var playerFish = PlayerController.Instance.Fish;
            if (playerFish != null)
            {
                if (!m_Active && !m_Completed)
                {
                    var states = m_LoopZone.CheckPosition(playerFish.Position);
                    if (states.horizontalState == EState.Include
                    && states.verticalState == EState.Include)
                    {
                        SetActive(true);
                    }
                }
            }
        }

        private Vector2 BeforeApplyNewPosition((EState horizontalState, EState verticalState) states, Vector2 newPosition, Vector2 rawPosition)
        {
            if (states.horizontalState == EState.Positive)
                newPosition += Vector2.right * m_LeftOffset;
            else if (states.horizontalState == EState.Negative)
            {
                SetActive(false);
                return rawPosition;
            }

            if (!(states.horizontalState == EState.Include || states.verticalState == EState.Include))
            {
                // 同时走了两个方向
                m_SuccessCount = 0;
                GenerateMarks();
                return newPosition;
            }

            if ((states.horizontalState == EState.Positive && m_TargetDirection == EDirection.Right)
            || (states.verticalState == EState.Positive && m_TargetDirection == EDirection.Up)
            || (states.verticalState == EState.Negative && m_TargetDirection == EDirection.Down))
            {
                Debug.Log("走对了");
                m_SuccessCount++;
                GenerateMarks();
                if (m_SuccessCount >= m_Answer.Count)
                {
                    Complete();
                }
                return newPosition;
            }

            Debug.Log("走错了");
            m_SuccessCount = 0;
            GenerateMarks();

            return newPosition;
        }

        private void SetActive(bool active)
        {
            if (m_Active == active)
                return;

            m_Active = active;
            m_LoopZone.SetActive(active);
            if (active)
            {
                m_SuccessCount = 0;
                GenerateMarks();
            }
        }

        private void Complete()
        {
            m_Completed = true;
            SetActive(false);
        }

        private static List<int[]> s_IndexMap = new()
        {
            new int[3] {0, 1, 2},
            new int[3] {0, 2, 1},
            new int[3] {1, 0, 2},
            new int[3] {1, 2, 0},
            new int[3] {2, 0, 1},
            new int[3] {2, 1, 0},
        };

        private void GenerateMarks()
        {
            if (m_SuccessCount >= m_Answer.Count)
                return;

            var index = s_IndexMap[Random.Range(0, 6)];
            m_UpMarker.sprite = m_CodeSprites[index[0]];
            m_RightMarker.sprite = m_CodeSprites[index[1]];
            m_DownMarker.sprite = m_CodeSprites[index[2]];

            int targetIndex = m_Answer[m_SuccessCount];
            for (int i = 0; i < 3; i++)
            {
                if (index[i] == targetIndex)
                {
                    m_TargetDirection = (EDirection)i;
                    break;
                }
            }
        }
    }
}