using UnityEngine;

namespace Game
{

    // 这里本意是写玩家避障的，但是变成了某种墙壁碰撞效果
    public class FB_Avoidance : FishBehaviour
    {
        [SerializeField] private float m_BounceForce = 5f;

        private float m_CD = 0f;

        private void Update()
        {
            if (m_CD > 0f)
                m_CD -= Time.deltaTime;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // TODO: 后面有需要可以区分下障碍物
            if (m_CD > 0f)
                return;

            Vector2 normal = Vector2.zero;
            for (int i = collision.contactCount - 1; i >= 0; i--)
            {
                normal += collision.contacts[i].normal;
            }
            normal.Normalize();
            float d = Mathf.Abs(Vector2.Dot(Fish.ForwardDirection, normal));

            var swimBehaviour = Fish.GetBehaviour<FB_Swim>();
            swimBehaviour.CurrentSpeed *= (1 - d);

            float strength = Mathf.Lerp(m_BounceForce / 3f, m_BounceForce, Mathf.Clamp01(swimBehaviour.CurrentSpeed * d / swimBehaviour.MaxSpeed));
            var velocity = AdditionalVelocity.Create(normal * strength);

            swimBehaviour.AddAdditionalVelocity(velocity);

            m_CD = 0.1f;
        }
    }
}