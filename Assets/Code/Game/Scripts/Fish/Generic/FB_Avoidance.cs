using UnityEngine;

namespace Game
{
    public class FB_Avoidance : FishBehaviour
    {
        [SerializeField] private float m_BounceForce = 5f;

        private void OnCollisionEnter2D(Collision2D collision)
        {
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
        }
    }
}