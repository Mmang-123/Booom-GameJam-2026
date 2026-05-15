using UnityEngine;

namespace Game
{
    public class TitanFight : PlayerTrigger
    {
        [SerializeField] private AudioSource m_AudioSource;
        [SerializeField] private GameObject m_Group;

        //
        private bool m_Active;

        private void Update()
        {
            if (!m_Active)
                return;

            var fish = PlayerController.Instance.Fish;
            if (fish != null)
            {
                transform.position = fish.Position;
            }

            m_AudioSource.volume = 1f - Mathf.Clamp(GameManager.Instance.ScreenFadeT, 0f, 0.5f) * 2f;
        }

        protected override void Trigger(Fish fish)
        {
            base.Trigger(fish);
            m_StopCheck = true;

            m_Active = true;
            m_AudioSource.Play();
            m_Group.SetActive(true);
        }
    }
}