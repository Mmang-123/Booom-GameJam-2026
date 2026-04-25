using System.Collections.Generic;
using System.Linq;
using Mmang.Game;
using UnityEngine;

namespace Game
{
    public class Fish : MonoBehaviour
    {
        private List<FishBehaviour> m_Behaviours;
        private Dictionary<System.Type, FishBehaviour> m_BehaviourMap = new();

        public Vector2 ForwardDirection => transform.up;
        public Vector2 Position => transform.position;

        private void Start()
        {
            Init();
        }

        private void Init()
        {
            m_Behaviours = GetComponents<FishBehaviour>().ToList();

            foreach (var behaviour in m_Behaviours)
            {
                m_BehaviourMap.Add(behaviour.GetType(), behaviour);
                behaviour.Init(this);
            }
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
    }
}
