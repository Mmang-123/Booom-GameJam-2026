using UnityEngine;

namespace Mmang.InputExtensions
{
    public enum EDirectionState { None, Positive, Negative }
    
    public struct DirectionStates
    {
        public EDirectionState HorizontalState;
        public EDirectionState VerticalState;
        public DirectionStates(EDirectionState horizontal, EDirectionState vertical)
        {
            HorizontalState = horizontal;
            VerticalState = vertical;
        }

        public static bool Equals(DirectionStates a, DirectionStates b)
        {
            return a.HorizontalState == b.HorizontalState && a.VerticalState == b.VerticalState;
        }

        public readonly bool IsNone()
        {
            return HorizontalState == EDirectionState.None && VerticalState == EDirectionState.None;
        }

        public readonly Vector2 GetVector()
        {
            Vector2 v = new(
                HorizontalState.GetDirectionValue(),
                VerticalState.GetDirectionValue()
            );

            if (v.sqrMagnitude > 1f)
                v.Normalize();

            return v;
        }
    }

    public static class DirectionStatesExtensions
    {
        public static EDirectionState GetDirectionState(bool positiveButton, bool negativeButton)
        {
            if (positiveButton && !negativeButton)
            {
                return EDirectionState.Positive;
            }
            if (!positiveButton && negativeButton)
            {
                return EDirectionState.Negative;
            }
            return EDirectionState.None;
        }

        public static float GetDirectionValue(this EDirectionState state)
        {
            return state switch
            {
                EDirectionState.Positive => 1f,
                EDirectionState.Negative => -1f,
                _ => 0,
            };
        }
    }
}