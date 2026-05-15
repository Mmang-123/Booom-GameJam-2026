using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Game.Editors
{
    public static class TransformMirrorTools
    {
        private const string MenuPathMirrorY = "Sloane/Game Tools/Mirror Children Along Y Axis";
        private const string MenuPathFlipX = "Sloane/Game Tools/Flip Children On X Axis";

        [MenuItem(MenuPathMirrorY, false, 49)]
        private static void MirrorChildrenAlongYAxis()
        {
            GameObject[] selectedRoots = Selection.gameObjects;
            if (selectedRoots == null || selectedRoots.Length == 0)
                return;

            foreach (var root in selectedRoots)
            {
                if (root == null)
                    continue;

                MirrorDescendantsLocalPositionX(root.transform);
            }
        }

        [MenuItem(MenuPathFlipX, false, 50)]
        private static void FlipChildrenOnXAxis()
        {
            GameObject[] selectedRoots = Selection.gameObjects;
            if (selectedRoots == null || selectedRoots.Length == 0)
                return;

            foreach (var root in selectedRoots)
            {
                if (root == null)
                    continue;

                MirrorDescendantsLocalPositionY(root.transform);
            }
        }

        [MenuItem(MenuPathMirrorY, true)]
        private static bool ValidateMirrorChildrenAlongYAxis()
        {
            return Selection.gameObjects != null && Selection.gameObjects.Length > 0;
        }

        [MenuItem(MenuPathFlipX, true)]
        private static bool ValidateFlipChildrenOnXAxis()
        {
            return Selection.gameObjects != null && Selection.gameObjects.Length > 0;
        }

        private static void MirrorDescendantsLocalPositionX(Transform root)
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            if (transforms.Length <= 1)
                return;

            var undoTargets = new List<Object>(transforms.Length - 1);
            for (int i = 1; i < transforms.Length; i++)
            {
                undoTargets.Add(transforms[i]);
            }

            Undo.RecordObjects(undoTargets.ToArray(), "Mirror Children Along Y Axis");

            for (int i = 1; i < transforms.Length; i++)
            {
                Transform child = transforms[i];
                Vector3 localPosition = child.localPosition;
                localPosition.x = -localPosition.x;
                child.localPosition = localPosition;
            }
        }

        private static void MirrorDescendantsLocalPositionY(Transform root)
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            if (transforms.Length <= 1)
                return;

            var undoTargets = new List<Object>(transforms.Length - 1);
            for (int i = 1; i < transforms.Length; i++)
            {
                undoTargets.Add(transforms[i]);
            }

            Undo.RecordObjects(undoTargets.ToArray(), "Flip Children On X Axis");

            for (int i = 1; i < transforms.Length; i++)
            {
                Transform child = transforms[i];
                Vector3 localPosition = child.localPosition;
                localPosition.y = -localPosition.y;
                child.localPosition = localPosition;
            }
        }
    }
}
