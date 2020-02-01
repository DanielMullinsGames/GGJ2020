using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Kilt.Reflection;
using Kilt.Extensions;

namespace Kilt.EasyRopes2D
{
    public static class RopeInternalUtils
    {
        #region Layer Utils

        public static Camera CameraThatDrawObject(GameObject p_object)
        {
            Camera v_returnCamera = GetCameraThatDrawLayer(p_object.layer);
            return v_returnCamera;
        }

        public static Camera GetCameraThatDrawLayer(int p_layer)
        {
            Camera v_returnCamera = null;
            foreach (Camera v_camera in Camera.allCameras)
            {
                if (v_camera != null)
                {
                    //Camera Draw Specific Layer
                    if ((v_camera.cullingMask & (1 << p_layer)) == (1 << p_layer))
                    {
                        v_returnCamera = v_camera;
                        break;
                    }
                }
            }
            return v_returnCamera;
        }

        #endregion

        #region RigidBody Utils

        public static void TryClearRigidBody2D(Rigidbody2D p_body)
        {
            if (p_body != null)
            {
                bool v_oldKinematic = p_body.isKinematic;
                p_body.velocity = Vector2.zero;
                p_body.angularVelocity = 0;
                p_body.isKinematic = true;
                p_body.isKinematic = v_oldKinematic;
            }
        }

        public static float GetObjectMass(GameObject p_object, bool p_includeChildrens = true, bool p_multiplyByGravityScale = true)
        {
            float v_objectMass = 0;
            if (p_object != null)
            {
                List<Rigidbody2D> v_objectRigidBodysList = p_includeChildrens ? new List<Rigidbody2D>(p_object.GetNonMarkedComponentsInChildren<Rigidbody2D>(false, false)) : new List<Rigidbody2D>();
                v_objectRigidBodysList.AddChecking(p_object.GetNonMarkedComponent<Rigidbody2D>());
                foreach (Rigidbody2D v_body in v_objectRigidBodysList)
                {
                    if (v_body != null)
                        v_objectMass += p_multiplyByGravityScale ? (v_body.mass * v_body.gravityScale) : v_body.mass;
                }
            }
            return v_objectMass;
        }

        #endregion

        #region Scale Utils

        public static void SetLossyScale(Transform p_prefabTransform, Vector3 p_prefabNewGlobalScale)
        {
            if (p_prefabTransform != null)
            {
                Vector3 v_newLocalScale = GetLocalScaleFromWorldScale(p_prefabTransform, p_prefabNewGlobalScale);
                p_prefabTransform.localScale = v_newLocalScale;
            }
        }

        public static Vector3 GetLocalScaleFromWorldScale(Transform p_prefabTransform, Vector3 p_prefabNewGlobalScale)
        {
            return GetLocalScaleFromWorldScale(p_prefabTransform.parent != null ? p_prefabTransform.parent.lossyScale : new Vector3(1, 1, 1), p_prefabNewGlobalScale);
        }

        public static Vector3 GetLocalScaleFromWorldScale(Vector3 p_parentLossyScale, Vector3 p_prefabGlobalScale)
        {
            float v_x = p_parentLossyScale.x == 0 ? 0 : p_prefabGlobalScale.x / p_parentLossyScale.x;
            float v_y = p_parentLossyScale.y == 0 ? 0 : p_prefabGlobalScale.y / p_parentLossyScale.y;
            float v_z = p_parentLossyScale.z == 0 ? 0 : p_prefabGlobalScale.z / p_parentLossyScale.z;
            return new Vector3(v_x, v_y, v_z);
        }

        #endregion

        #region Vector Utils

        public static bool FindLineIntersection(Vector2 p_line1_p1, Vector2 p_line1_p2, Vector2 p_line2_p1, Vector2 p_line2_p2, ref Vector2 p_intersection)
        {

            Vector2 p1 = p_line1_p1;
            Vector2 p2 = p_line1_p2;
            Vector2 p3 = p_line2_p1;
            Vector2 p4 = p_line2_p2;
            float Ax, Bx, Cx, Ay, By, Cy, d, e, f, num/*,offset*/;
            float x1lo, x1hi, y1lo, y1hi;

            Ax = p2.x - p1.x;
            Bx = p3.x - p4.x;

            // X bound box test/

            if (Ax < 0)
            {
                x1lo = p2.x;
                x1hi = p1.x;
            }
            else
            {
                x1hi = p2.x;
                x1lo = p1.x;
            }

            if (Bx > 0)
            {
                if (x1hi < p4.x || p3.x < x1lo)
                    return false;

            }
            else
            {
                if (x1hi < p3.x || p4.x < x1lo)
                    return false;
            }

            Ay = p2.y - p1.y;
            By = p3.y - p4.y;

            // Y bound box test//
            if (Ay < 0)
            {
                y1lo = p2.y;
                y1hi = p1.y;
            }
            else
            {
                y1hi = p2.y;
                y1lo = p1.y;
            }

            if (By > 0)
            {
                if (y1hi < p4.y || p3.y < y1lo)
                    return false;
            }
            else
            {
                if (y1hi < p3.y || p4.y < y1lo)
                    return false;
            }

            Cx = p1.x - p3.x;
            Cy = p1.y - p3.y;
            d = By * Cx - Bx * Cy;  // alpha numerator//
            f = Ay * Bx - Ax * By;  // both denominator//

            // alpha tests//

            if (f > 0)
            {
                if (d < 0 || d > f)
                    return false;

            }
            else
            {
                if (d > 0 || d < f)
                    return false;
            }
            e = Ax * Cy - Ay * Cx;  // beta numerator//

            // beta tests //

            if (f > 0)
            {
                if (e < 0 || e > f)
                    return false;
            }
            else
            {
                if (e > 0 || e < f)
                    return false;
            }

            // check if they are parallel
            if (f == 0)
                return false;

            // compute intersection coordinates //
            num = d * Ax; // numerator //

            //    offset = same_sign(num,f) ? f*0.5f : -f*0.5f;   // round direction //
            //    intersection.x = p1.x + (num+offset) / f;
            p_intersection.x = p1.x + num / f;
            num = d * Ay;

            //    offset = same_sign(num,f) ? f*0.5f : -f*0.5f;
            //    intersection.y = p1.y + (num+offset) / f;
            p_intersection.y = p1.y + num / f;
            return true;
        }

        public static Vector3 GetVectorDirection(Vector3 from, Vector3 to)
        {
            return (to - from).normalized;
        }

        public static Vector3 RotateZ(Vector3 p, float angle)
        {
            Vector3 v = new Vector3(p.x, p.y, p.z);
            v = Quaternion.AngleAxis(angle, new Vector3(0, 0, 1)) * v;
            return v;
        }

        #endregion

        #region Other Utils

        public static bool IsPrefab(GameObject p_object)
        {
            if (p_object != null)
            {
                if (Application.isEditor)
                {
#if UNITY_EDITOR
                    var v_path = UnityEditor.AssetDatabase.GetAssetPath(p_object);
                    return !string.IsNullOrEmpty(v_path);
#endif
                }
                else
                {
#if UNITY_5_4_OR_NEWER
                    return !p_object.scene.IsValid() || string.IsNullOrEmpty(p_object.scene.name);
#else
                    var v_sucess = false;
                    GameObject v_tempObject = new GameObject();
                    //v_tempObject.hideFlags = HideFlags.HideAndDontSave;
                    v_tempObject.transform.SetAsFirstSibling();
                    var v_parent = p_object.transform.root;
                    if (v_parent == null)
                        v_parent = p_object.transform;
                    if (v_parent.GetSiblingIndex() == 0)
                        v_sucess = true;
                    DestroyUtils.Destroy(v_tempObject);
                    return v_sucess;
#endif
                }
            }
            return false;
        }

        public static T CopyComponent<T>(GameObject p_destination, T p_original, bool p_pasteAsValue = true, bool p_deleteOldOne = false) where T : Component
        {
            if (p_destination != null && p_original != null)
            {
                System.Type v_type = p_original.GetType();
                Component v_copy = null;
                if (p_deleteOldOne)
                {
                    v_copy = p_destination.GetComponent(v_type);
                    if (v_copy != null)
                        Object.DestroyImmediate(v_copy);
                }
                if (p_pasteAsValue)
                    v_copy = p_destination.GetComponent(v_type);
                if (v_copy == null)
                    v_copy = p_destination.AddComponent(v_type);
                System.Reflection.FieldInfo[] v_fields = new System.Reflection.FieldInfo[0];
                while (v_type != null && v_type != typeof(Component) && v_type != typeof(Behaviour) && v_type != typeof(MonoBehaviour))
                {
                    //All Privates and Publics in this Specific Type (Avoid BaseType Component, Behaviour and MonoBehaviour to prevent bugs)
                    v_fields = v_type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Public);
                    foreach (System.Reflection.FieldInfo v_field in v_fields)
                    {
#if UNITY_WINRT && !UNITY_EDITOR
                        var v_customAtts = v_field.GetCustomAttributes(typeof(SerializeField), true) as object[];
                        if (v_customAtts != null && v_customAtts.Length > 0)
                        {
                            object v_newValue = v_field.GetValue(p_original);
					        v_field.SetValue(v_copy, v_newValue);
                        }
#else
                        if (!v_field.IsNotSerialized && System.Attribute.IsDefined(v_field, typeof(SerializeField)))
                        {
                            object v_newValue = v_field.GetValue(p_original);
                            v_field.SetValue(v_copy, v_newValue);
                        }
#endif
                    }
#if UNITY_WINRT && !UNITY_EDITOR && !UNITY_WP8
				    v_type = v_type.BaseType();
#else
                    v_type = v_type.BaseType;
#endif
                }

                if (v_copy is Behaviour && p_original is Behaviour)
                    (v_copy as Behaviour).enabled = (p_original as Behaviour).enabled;


                return v_copy as T;
            }
            return null;
        }

        public static bool IsOutOfScreen(GameObject p_object, bool p_quickAlgorithm = false, bool p_rendererCheck = true)
        {
            bool v_return = false;
            if (p_object != null)
            {
                Camera v_cameraThatDrawThisObject = GetCameraThatDrawLayer(p_object.layer);
                if (v_cameraThatDrawThisObject != null)
                {
                    Vector2 v_positionInViewPort = v_cameraThatDrawThisObject.WorldToViewportPoint(p_object.transform.position);
                    Rect v_screenRect = v_cameraThatDrawThisObject.rect;
                    if (!v_screenRect.Contains(v_positionInViewPort))
                    {
                        if (p_quickAlgorithm)
                            v_return = true;
                        //Need More Complex Maths
                        else
                        {
                            //Renderer Check
                            if (p_rendererCheck)
                            {
                                List<SpriteRenderer> v_renderers = new List<SpriteRenderer>(p_object.GetComponents<SpriteRenderer>());
                                v_renderers.MergeList(new List<SpriteRenderer>(p_object.GetComponentsInChildren<SpriteRenderer>()));
                                foreach (SpriteRenderer v_renderer in v_renderers)
                                {
                                    List<Vector2> v_boundsInPoints = new List<Vector2>();
                                    Vector2 v_topLeft = new Vector2(v_renderer.bounds.center.x - v_renderer.bounds.size.x / 2, v_renderer.bounds.center.y - v_renderer.bounds.size.y / 2);
                                    Vector2 v_topRight = new Vector2(v_topLeft.x + v_renderer.bounds.size.x, v_topLeft.y);
                                    Vector2 v_botLeft = new Vector2(v_topLeft.x, v_topLeft.y + v_renderer.bounds.size.y);
                                    Vector2 v_botRight = new Vector2(v_topLeft.x + v_renderer.bounds.size.x, v_topLeft.y + v_renderer.bounds.size.y);
                                    v_boundsInPoints.Add(v_topLeft);
                                    v_boundsInPoints.Add(v_topRight);
                                    v_boundsInPoints.Add(v_botLeft);
                                    v_boundsInPoints.Add(v_botRight);
                                    foreach (Vector2 v_point in v_boundsInPoints)
                                    {
                                        Vector2 v_viewPoint = v_cameraThatDrawThisObject.WorldToViewportPoint(v_point);
                                        //IsUnderView so return False
                                        if (v_screenRect.Contains(v_viewPoint))
                                        {
                                            v_return = false;
                                            return v_return;
                                        }
                                    }
                                }
                            }
                            v_return = true;
                        }
                    }
                }
                else
                    v_return = true; // Dont Have Camera so is out of Screen
            }
            return v_return;
        }

#endregion
    }
}
