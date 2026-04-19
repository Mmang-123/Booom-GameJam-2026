using UnityEngine;

namespace Mmang.Generations
{

    public abstract class GenerationPointBehaviour : ScriptableObject { }

    [CreateAssetMenu(fileName = nameof(GenerationPointSettingConfig), menuName = "Configs/Generation/Setting")]
    public class GenerationPointSettingConfig : GenerationPointBehaviour
    {
        public LayerMask GenerationBlockLayer;
        public bool FilterSelectionCollider;
        
        public float GenerationDensity;
        public int GenerationMaxCount;

        public Vector2 OffsetHeight;

        public bool EnableNormalLimit;
        public Vector3 TargetNormal;
        public float AngleLessThan;

        [SerializeReference] private GenerationPointAdditionalSetting m_AdditionalSetting;
        public GenerationPointAdditionalSetting AdditionalSetting
        {
            get => m_AdditionalSetting;
            set => m_AdditionalSetting = value;
        }

        #region Property Name
        public static string PN_GenerationBlockLayer => nameof(GenerationBlockLayer);
        public static string PN_FilterSelectionCollider => nameof(FilterSelectionCollider);

        public static string PN_GenerationDensity => nameof(GenerationDensity);
        public static string PN_GenerationMaxCount => nameof(GenerationMaxCount);

        public static string PN_OffsetHeight => nameof(OffsetHeight);

        public static string PN_EnableNormalLimit => nameof(EnableNormalLimit);
        public static string PN_TargetNormal => nameof(TargetNormal);
        public static string PN_AngleLessThan => nameof(AngleLessThan);
        
        public static string PN_AdditionalSetting => nameof(m_AdditionalSetting);
        #endregion
    }

}