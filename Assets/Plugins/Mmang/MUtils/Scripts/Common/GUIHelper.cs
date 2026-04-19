using System.Collections.Generic;
using UnityEngine;

namespace Mmang.Util
{
    public class GUIHelper : SingletonMono<GUIHelper>
    {
        private List<System.Action> m_OnGUIActions = new();

        public static void AddOnGUIAction(System.Action action)
        {
            Instance.m_OnGUIActions.Add(action);
        }

        private void Update()
        {
            m_OnGUIActions.Clear();
        }

        private void OnGUI()
        {
            if (m_OnGUIActions.Count <= 0)
                return;
            foreach (var action in m_OnGUIActions)
                action?.Invoke();
        }

        public static void DrawRect(Vector2 centerInScreen, Vector2 sizeInScreen, Color color)
        {
            centerInScreen = new(centerInScreen.x, 1f - centerInScreen.y);
            centerInScreen -= sizeInScreen / 2;

            centerInScreen.Scale(new(Screen.width, Screen.height));
            sizeInScreen.Scale(new(Screen.width, Screen.height));
        
            Rect rect = new(centerInScreen.x, centerInScreen.y, sizeInScreen.x, sizeInScreen.y);

            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, color, 0, 0);
        }

        public static void DrawSquare(Vector2 centerInScreen, float squareSizeInScreen, Color color)
        {
            Vector2 sizeInScreen = new(squareSizeInScreen, squareSizeInScreen * Screen.width / Screen.height);
            DrawRect(centerInScreen, sizeInScreen, color);
        }

        public static void DrawWireRect(Vector2 centerInScreen, Vector2 sizeInScreen, float border, Color color)
        {
            centerInScreen = new(centerInScreen.x, 1f - centerInScreen.y);
            centerInScreen -= sizeInScreen / 2;

            centerInScreen.Scale(new(Screen.width, Screen.height));
            sizeInScreen.Scale(new(Screen.width, Screen.height));
            
            Rect rect = new(centerInScreen.x, centerInScreen.y, sizeInScreen.x, sizeInScreen.y);

            float borderWidth = border * Screen.width;
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, color, borderWidth, 0);
        }
    }
}