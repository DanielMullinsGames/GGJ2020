using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Kilt.EasyRopes2D
{

    public class TimedTrailRenderer : MonoBehaviour
    {
        #region Helper Classes

        public class Point
        {
            public float timeCreated = 0.00f;
            public Vector3 position;
            public bool lineBreak = false;
        }

        #endregion

        #region Public Variables

        public bool emit = true;
        public float emitTime = 0.00f;
        public Material material;

        public float lifeTime = 1.00f;

        public Color[] colors;
        public float[] sizes;

        public float uvLengthScale = 0.01f;
        public bool higherQualityUVs = true;

        public int movePixelsForRebuild = 6;
        public float maxRebuildTime = 0.1f;

        public float minVertexDistance = 0.10f;

        public float maxVertexDistance = 10.00f;
        public float maxAngle = 3.00f;

        public bool ignoreTimeScale = false;

        public bool autoDestruct = false;

        #endregion

        #region Private Variables

        private ArrayList points = new ArrayList();
        private GameObject o;
        private Vector3 lastPosition;
        private Vector3 lastCameraPosition1;
        private Vector3 lastCameraPosition2;
        private float lastRebuildTime = 0.00f;
        private bool lastFrameEmit = true;

        #endregion

        #region Unity Functions

        protected virtual void OnStart()
        {
            Init();
        }

        protected virtual void OnEnable()
        {
            Init();
        }

        protected virtual void OnDisable()
        {
            Destroy(o);
        }

        protected virtual void Update()
        {
            if (o != null)
            {
                o.transform.position = Vector3.zero;
                o.layer = this.gameObject.layer;
            }

            if (emit && emitTime != 0)
            {
                emitTime -= GetDeltaTime();
                if (emitTime == 0) emitTime = -1;
                if (emitTime < 0) emit = false;
            }

            if (!emit && points.Count == 0 && autoDestruct)
            {
                Destroy(o);
                Destroy(gameObject);
            }

            // early out if there is no camera
            Camera v_camera = RopeInternalUtils.GetCameraThatDrawLayer(this.gameObject.layer);
            if (v_camera == null) return;

            bool re = false;

            // if we have moved enough, create a new vertex and make sure we rebuild the mesh
            float theDistance = (lastPosition - transform.position).magnitude;
            if (emit)
            {
                if (theDistance > minVertexDistance)
                {
                    bool make = false;
                    if (points.Count < 3)
                    {
                        make = true;
                    }
                    else
                    {
                        Vector3 l1 = ((Point)points[points.Count - 2]).position - ((Point)points[points.Count - 3]).position;
                        Vector3 l2 = ((Point)points[points.Count - 1]).position - ((Point)points[points.Count - 2]).position;
                        if (Vector3.Angle(l1, l2) > maxAngle || theDistance > maxVertexDistance) make = true;
                    }

                    if (make)
                    {
                        Point p = new Point();
                        p.position = transform.position;
                        p.timeCreated = GetTime();
                        points.Add(p);
                        lastPosition = transform.position;
                    }
                    else
                    {
                        ((Point)points[points.Count - 1]).position = transform.position;
                        ((Point)points[points.Count - 1]).timeCreated = GetTime();
                    }
                }
                else if (points.Count > 0)
                {
                    ((Point)points[points.Count - 1]).position = transform.position;
                    ((Point)points[points.Count - 1]).timeCreated = GetTime();
                }
            }

            if (!emit && lastFrameEmit && points.Count > 0) ((Point)points[points.Count - 1]).lineBreak = true;
            lastFrameEmit = emit;

            // approximate if we should rebuild the mesh or not
            if (points.Count > 1)
            {
                Vector3 cur1 = v_camera.WorldToScreenPoint(((Point)points[0]).position);
                lastCameraPosition1.z = 0;
                Vector3 cur2 = v_camera.WorldToScreenPoint(((Point)points[points.Count - 1]).position);
                lastCameraPosition2.z = 0;

                float distance = (lastCameraPosition1 - cur1).magnitude;
                distance += (lastCameraPosition2 - cur2).magnitude;

                if (distance > movePixelsForRebuild || GetTime() - lastRebuildTime > maxRebuildTime)
                {
                    re = true;
                    lastCameraPosition1 = cur1;
                    lastCameraPosition2 = cur2;
                }
            }
            else
            {
                re = true;
            }

            if (re)
            {
                lastRebuildTime = GetTime();

                int i = 0;
                ArrayList v_newList = new ArrayList();
                foreach (Point p in points)
                {
                    // cull old points first
                    if (GetTime() - p.timeCreated <= lifeTime) v_newList.Add(p);
                    i++;
                }

                points.Clear();
                points = v_newList;

                if (points.Count > 1)
                {
                    Vector3[] newVertices = new Vector3[points.Count * 2];
                    Vector2[] newUV = new Vector2[points.Count * 2];
                    int[] newTriangles = new int[(points.Count - 1) * 6];
                    Color[] newColors = new Color[points.Count * 2];

                    i = 0;
                    float curDistance = 0.00f;

                    foreach (Point p in points)
                    {
                        float time = (GetTime() - p.timeCreated) / lifeTime;

                        Color color = Color.Lerp(Color.white, Color.clear, time);
                        if (colors != null && colors.Length > 0)
                        {
                            float colorTime = time * (colors.Length - 1);
                            float min = Mathf.Floor(colorTime);
                            float max = Mathf.Clamp(Mathf.Ceil(colorTime), 1, colors.Length - 1);
                            float lerp = Mathf.InverseLerp(min, max, colorTime);
                            if (min >= colors.Length) min = colors.Length - 1; if (min < 0) min = 0;
                            if (max >= colors.Length) max = colors.Length - 1; if (max < 0) max = 0;
                            if (colors.Length > min && min >= 0 && colors.Length > max && max >= 0)
                                color = Color.Lerp(colors[(int)min], colors[(int)max], lerp);
                        }

                        float size = 1f;
                        if (sizes != null && sizes.Length > 0)
                        {
                            float sizeTime = time * (sizes.Length - 1);
                            float min = Mathf.Floor(sizeTime);
                            float max = Mathf.Clamp(Mathf.Ceil(sizeTime), 1, sizes.Length - 1);
                            float lerp = Mathf.InverseLerp(min, max, sizeTime);
                            if (min >= sizes.Length) min = sizes.Length - 1; if (min < 0) min = 0;
                            if (max >= sizes.Length) max = sizes.Length - 1; if (max < 0) max = 0;
                            if (sizes.Length > min && min >= 0 && sizes.Length > max && max >= 0)
                                size = Mathf.Lerp(sizes[(int)min], sizes[(int)max], lerp);
                        }

                        Vector3 lineDirection = Vector3.zero;
                        if (i == 0) lineDirection = p.position - ((Point)points[i + 1]).position;
                        else lineDirection = ((Point)points[i - 1]).position - p.position;

                        Vector3 vectorToCamera = v_camera.transform.position - p.position;
                        Vector3 perpendicular = Vector3.Cross(lineDirection, vectorToCamera).normalized;

                        newVertices[i * 2] = p.position + (perpendicular * (size * 0.5f));
                        newVertices[(i * 2) + 1] = p.position + (-perpendicular * (size * 0.5f));

                        newColors[i * 2] = newColors[(i * 2) + 1] = color;

                        newUV[i * 2] = new Vector2(curDistance * uvLengthScale, 0);
                        newUV[(i * 2) + 1] = new Vector2(curDistance * uvLengthScale, 1);

                        if (i > 0 && !((Point)points[i - 1]).lineBreak)
                        {
                            if (higherQualityUVs) curDistance += (p.position - ((Point)points[i - 1]).position).magnitude;
                            else curDistance += (p.position - ((Point)points[i - 1]).position).sqrMagnitude;

                            newTriangles[(i - 1) * 6] = (i * 2) - 2;
                            newTriangles[((i - 1) * 6) + 1] = (i * 2) - 1;
                            newTriangles[((i - 1) * 6) + 2] = i * 2;

                            newTriangles[((i - 1) * 6) + 3] = (i * 2) + 1;
                            newTriangles[((i - 1) * 6) + 4] = i * 2;
                            newTriangles[((i - 1) * 6) + 5] = (i * 2) - 1;
                        }

                        i++;
                    }
                    if (o != null)
                    {
                        Mesh mesh = (o.GetComponent(typeof(MeshFilter)) as MeshFilter).mesh;
                        mesh.Clear();
                        mesh.vertices = newVertices;
                        mesh.colors = newColors;
                        mesh.uv = newUV;
                        mesh.triangles = newTriangles;
                    }
                }
                else
                {
                    if (o != null)
                    {
                        Mesh mesh = (o.GetComponent(typeof(MeshFilter)) as MeshFilter).mesh;
                        mesh.Clear();
                    }
                }
            }
        }

        protected virtual void LateUpdate()
        {
            if (o != null)
                o.transform.position = Vector3.zero;
        }

        #endregion

        #region Helper Functions

        public virtual void ClearPoints()
        {
            points.Clear();
        }

        public virtual void Init()
        {
            if (o == null)
            {
                lastPosition = transform.position;
                o = new GameObject("Trail");
                o.transform.parent = this.transform;
                o.transform.position = Vector3.zero;
                o.transform.rotation = Quaternion.identity;
                o.transform.localScale = Vector3.one;
                o.AddComponent(typeof(MeshFilter));
                o.AddComponent(typeof(MeshRenderer));
                o.GetComponent<Renderer>().material = material;
                o.layer = this.gameObject.layer;
            }
            else
                o.layer = this.gameObject.layer;
        }

        public float GetTime()
        {
            return ignoreTimeScale ? Time.unscaledTime : Time.time;
        }

        public float GetDeltaTime()
        {
            return ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
        }

        #endregion
    }
}