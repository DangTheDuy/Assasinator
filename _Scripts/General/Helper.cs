using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public static class Helper
    {
        public static void ClearChilds(Transform root)
        {
            if (root)
            {
                int childs = root.childCount;

                if (childs > 0)
                {
                    for (int i = 0; i < childs; i++)
                    {
                        var child = root.GetChild(i);

                        if (child)
                            MonoBehaviour.Destroy(child.gameObject);
                    }
                }
            }
        }

        public static void AssignToRoot(Transform root, Transform obj, Vector3 pos, Vector3 scale, bool isChange = false)
        {
            obj.SetParent(root, isChange);

            obj.localPosition = pos;

            if (!isChange)
                obj.localScale = scale;

            if (!isChange)
                obj.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }
