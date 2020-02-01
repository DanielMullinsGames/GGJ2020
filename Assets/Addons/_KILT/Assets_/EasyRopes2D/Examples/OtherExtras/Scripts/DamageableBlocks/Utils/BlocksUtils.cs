using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kilt.Extensions;

namespace Kilt.EasyRopes2D.Examples
{
    public class BlockUtils
    {
        #region Effect Utils

        public static GameObject InstantiateEffect(GameObject p_effect)
        {
            return InstantiateEffectOverOwner(null, p_effect);
        }

        public static GameObject InstantiateEffect(GameObject p_effect, int p_depth)
        {
            GameObject v_effectObject = InstantiateEffect(p_effect);
            if (v_effectObject != null)
            {
                List<Renderer> v_renderers = new List<Renderer>(v_effectObject.GetComponents<Renderer>());
                //v_renderers.MergeList(new List<Renderer>(v_effectObject.GetComponentsInChildren<Renderer>()));
                foreach (Renderer v_renderer in v_renderers)
                    v_renderer.sortingOrder = p_depth;
            }
            return v_effectObject;
        }

        public static GameObject InstantiateEffectOverOwner(Transform p_owner, GameObject p_effect, bool p_forceLocalScale = true, bool p_instantiateInOwnerParent = true, bool p_keepOriginalRotation = false)
        {
            GameObject v_effectObject = null;
            if (p_effect != null)
            {
                if (p_owner != null)
                {
                    v_effectObject = GameObject.Instantiate(p_effect) as GameObject;
                    v_effectObject.transform.position = p_owner.position;
                    v_effectObject.transform.rotation = p_owner.rotation;
                    v_effectObject.transform.parent = p_instantiateInOwnerParent ? p_owner.parent : p_owner;
                    v_effectObject.transform.Rotate(p_effect.transform.localEulerAngles);
                    //Force Scale to original one
                    if (p_forceLocalScale)
                        v_effectObject.transform.localScale = p_effect.transform.localScale;
                    else
                        RopeInternalUtils.SetLossyScale(v_effectObject.transform, p_effect.transform.localScale);
                    if (p_keepOriginalRotation)
                        v_effectObject.transform.eulerAngles = p_effect.transform.localEulerAngles;
                }
                else
                {
                    v_effectObject = GameObject.Instantiate(p_effect) as GameObject;
                }
            }
            return v_effectObject;
        }

        public static GameObject InstantiateEffectOverOwner(Transform p_owner, GameObject p_effect, int p_depth, bool p_forceLocalScale = true, bool p_instantiateInOwnerParent = true)
        {
            GameObject v_effectObject = InstantiateEffectOverOwner(p_owner, p_effect, p_forceLocalScale, p_instantiateInOwnerParent);
            if (v_effectObject != null)
            {
                List<Renderer> v_renderers = new List<Renderer>(v_effectObject.GetComponents<Renderer>());
                v_renderers.MergeList(new List<Renderer>(v_effectObject.GetComponentsInChildren<Renderer>()));
                foreach (Renderer v_renderer in v_renderers)
                    v_renderer.sortingOrder = p_depth;
            }
            return v_effectObject;
        }

        #endregion

        #region Shape Utils

        public static bool ObjectIsFromThisShape(GameObject p_object, string p_shapeName)
        {
            if (p_object != null)
            {
                if (string.IsNullOrEmpty(p_shapeName))
                    return true;
                else
                {
                    string[] v_shapesName = p_shapeName.Split('_');
                    foreach (string v_shapeName in v_shapesName)
                    {
                        if (p_object.name.Contains(v_shapeName))
                            return true;
                    }
                }

            }
            return false;
        }

        public static string FindShapeName(GameObject p_object)
        {
            string v_shape = "";
            if (p_object != null)
            {
                string v_name = p_object.name;
                if (v_name.Contains("Square") || v_name.Contains("Box") || v_name.Contains("Bar") || v_name.Contains("Brick"))
                    v_shape = "Square_Box_Bar_Brick";
                else if (v_name.Contains("Circle") || v_name.Contains("Wheel") || v_name.Contains("Ellipse"))
                    v_shape = "Circle_Wheel_Ellipse";
                else if (v_name.Contains("TriangleRect"))
                    v_shape = "TriangleRect";
                else if (v_name.Contains("TriangleEqui"))
                    v_shape = "TriangleEqui";
                else if (v_name.Contains("SpecialShape"))
                    v_shape = "SpecialShape";
            }
            return v_shape;
        }

        #endregion
    }
}
