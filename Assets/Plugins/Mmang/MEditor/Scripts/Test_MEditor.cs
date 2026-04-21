using System.Collections.Generic;
using Mmang;
using UnityEngine;

#if UNITY_EDITOR
namespace Mmang.Test
{
    #region VariableType

    [System.Serializable]
    [VariableTypeDefine(CanBeNull = true)]
    public abstract class TestVariable
    {
        public float Base;
    }

    [System.Serializable]
    public class TestVariableA : TestVariable
    {
        public float A;
    }

    [System.Serializable]
    public class TestVariableB : TestVariable
    {
        public float B;
    }

    [System.Serializable]
    public class TestVariableCCCCC : TestVariableB
    {
        public float A;
    }

    #endregion


    #region MEnums
    public enum TestMEnums
    {
        [MEnum("Name A")] A, // 自定义名称
        B,
        [MEnum(hide = true)] End // 过滤
    }

    #endregion


    #region Dropdown

    [DropdownCollection]
    public static class TestDropdownCollection
    {
        public static readonly int A = 1;
        public static readonly int B = 2;
        public static readonly int C = 3;
        public static readonly uint D = 4; // 不匹配不会显示
    }

    #endregion


    [ExecuteAlways]
    public class Test_MEditor : MonoBehaviour
    {
        #region Variable Type
        [Header("Variable Type")]
        [SerializeReference, VariableType] private TestVariable m_Variable;
        [SerializeReference, VariableType] private TestVariable m_Variable2 = new TestVariableA();
        [SerializeReference, VariableType] private List<TestVariable> m_Variables = new();

        #endregion

        #region MEnums

        [Header("MEnums")]
        [SerializeField, MEnums] private TestMEnums m_MEnum;

        public static void MEnumsTestFunc()
        {
            var names = MEnums.GetNames<TestMEnums>();
            foreach (var name in names)
            {
                Debug.Log(name);
            }
        }

        #endregion


        #region Dropdown
        [Header("Dropdown")]
        [SerializeField, Dropdown(typeof(TestDropdownCollection))]
        private int m_Dropdown1;

        [SerializeField, Dropdown(nameof(GetMap))]
        private int m_Dropdown2;

        private Dictionary<int, string> GetMap()
        {
            return new()
            {
                [1] = "A",
                [2] = "B"
            };
        }

        #endregion

        #region SODetails
        [Header("SODetails")]
        [SODetails, SerializeField] private ScriptableObject m_SODetails;

        [SerializeField] private ScriptableObject m_NormalField;
        #endregion

        private void OnEnable()
        {
            //MEnumsTestFunc();
        }
    }


}
#endif