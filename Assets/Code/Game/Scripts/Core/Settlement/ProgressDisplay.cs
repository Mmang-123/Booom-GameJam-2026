
using UnityEngine;

namespace Game
{
    public class ProgressDisplay : MonoBehaviour
    {
        private void Awake()
        {
            int progress = GameManager.Instance.GetCurrentProgress();
            if (progress <= 0)
            {
                
            }
        }
    }
}