
using Mmang.Game;
using UnityEngine;

namespace Game
{
    public class LabScreenPoint : MonoBehaviour
    {
        [SerializeField] private string m_FishTag;
        [SerializeField] private bool m_FollowPlayer;
        [SerializeField] private Transform m_ScreenLeftBottom;
        [SerializeField] private Transform m_ScreenRightTop;
        [SerializeField] private Transform m_WorldLeftBottom;
        [SerializeField] private Transform m_WorldRightTop;

        private void FixedUpdate()
        {
            if (GameManager.Instance.InfectionSourceTags.Contains(m_FishTag))
            {
                gameObject.SetActive(false);
            }

            if (m_FollowPlayer)
            {
                FollowPlayer();
            }
        }

        private void FollowPlayer()
        {
            var fish = PlayerController.Instance.Fish;
            if (fish == null)
                return;
            
            Vector2 worldSize = m_WorldRightTop.position - m_WorldLeftBottom.position;
            Vector2 screenSize = m_ScreenRightTop.position - m_ScreenLeftBottom.position;

            Vector2 playerPos = fish.Position;
            playerPos -= (Vector2)m_WorldLeftBottom.position;

            Vector2 newPos = new Vector2(playerPos.x / worldSize.x * screenSize.x, playerPos.y / worldSize.y * screenSize.y) + (Vector2)m_ScreenLeftBottom.position;
            transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
        }
    }
}