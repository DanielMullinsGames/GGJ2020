using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Kilt.EasyRopes2D
{
    //Simple Code. ps: The Rope MUST have "User Can Cut the Rope" = true 
    public class KnifeCutter : MonoBehaviour
    {
        #region Private Properties

        [SerializeField]
        LayerMask m_collisionMask = -1;

        //Used To Prevent Deformations in Trail
        Vector2 _oldPosition = Vector2.zero;

        #endregion

        #region Public Properties

        public LayerMask CollisionMask
        {
            get { return m_collisionMask; }
            set
            {
                if (m_collisionMask == value)
                    return;
                m_collisionMask = value;
            }
        }

        Camera _cameraThatDrawThisObject = null;
        public Camera CameraThatDrawThisObject
        {
            get
            {
                if (_cameraThatDrawThisObject == null)
                    _cameraThatDrawThisObject = RopeInternalUtils.GetCameraThatDrawLayer(this.gameObject.layer);
                return _cameraThatDrawThisObject;
            }
        }

        #endregion

        #region Unity Functions

        protected virtual void Update()
        {
            UpdatePosition();
        }

        protected virtual void LateUpdate()
        {
            TryRayCastAll();
        }

        protected virtual void OnCollisionEnter2D(Collision2D p_collision)
        {
            TryCut(p_collision.gameObject);
        }

        #endregion

        #region Tensioned Rope Cutter Functions

        //Used to Avoid Bugs when RayCast pass between node collisors when rope is tensioned
        protected void CheckCollisionWithTensionedRopes()
        {
            List<Rope2D> v_tensionedRopes = GetTensionedCuttableRopes();
            Vector2 v_trail_point1 = _oldPosition;
            Vector2 v_trail_point2 = this.transform.position;

            foreach (Rope2D v_rope in v_tensionedRopes)
            {
                if (v_rope.Nodes.Count >= 2)
                {
                    GameObject v_firstNode = v_rope.GetAttachedObjectA();
                    GameObject v_lastNode = v_rope.GetAttachedObjectB();
                    if (v_firstNode != null && v_lastNode != null)
                    {
                        Vector2 v_rope_point1 = v_firstNode.transform.position;
                        Vector2 v_rope_point2 = v_lastNode.transform.position;
                        Vector2 v_intersection = Vector2.zero;
                        if (RopeInternalUtils.FindLineIntersection(v_trail_point1, v_trail_point2, v_rope_point1, v_rope_point2, ref v_intersection))
                        {
                            GameObject v_node = GetNodeWithSmallestDistanteToPoint(v_rope, v_intersection);
                            TryCut(v_node);
                        }
                    }
                    else
                        continue;
                }
            }
        }

        protected List<Rope2D> GetTensionedCuttableRopes()
        {
            List<Rope2D> v_tensionedRopes = new List<Rope2D>();
            foreach (Rope2D v_rope in Rope2D.AllRopesInScene)
            {
                if (v_rope != null && v_rope.IsRopeTensioned() && !v_rope.IsRopeBroken() && v_rope.UserCanCutTheRope)
                    v_tensionedRopes.Add(v_rope);
            }
            return v_tensionedRopes;
        }

        protected GameObject GetNodeWithSmallestDistanteToPoint(Rope2D p_rope, Vector2 p_point)
        {
            GameObject v_node = null;
            float v_currentDistance = -1;
            if (p_rope != null)
            {
                foreach (GameObject p_node in p_rope.Nodes)
                {
                    if (p_node != null)
                    {
                        if (v_currentDistance < 0)
                        {
                            v_node = p_node;
                            v_currentDistance = Vector2.Distance(p_point, v_node.transform.position);
                        }
                        else
                        {
                            float v_thisNodeDistance = Vector2.Distance(p_point, p_node.transform.position);
                            if (v_thisNodeDistance < v_currentDistance)
                            {
                                v_node = p_node;
                                v_currentDistance = v_thisNodeDistance;
                                continue;
                            }
                        }
                    }
                }
            }
            return v_node;
        }

        #endregion

        #region Cutter Functions

        protected void TryRayCastAll()
        {
            int v_rayCastlayerMask = CollisionMask.value; //~(1 << gameObject.layer); // everything excent this layer
            RaycastHit2D[] v_hits = Physics2D.LinecastAll(_oldPosition, this.transform.position, v_rayCastlayerMask);

            foreach (RaycastHit2D v_hit in v_hits)
            {
                if (v_hit.collider != null)
                {
                    TryCut(v_hit.collider.gameObject);
                }
            }
            CheckCollisionWithTensionedRopes();
        }

        protected virtual bool TryCut(GameObject p_object)
        {
            if (p_object != null)
            {
                Node2D v_node = p_object.GetComponent<Node2D>();
                if (v_node != null)
                    return v_node.Cut();
            }
            return false;
        }

        #endregion

        #region Public Functions

        public void UpdatePosition()
        {
            _oldPosition = this.transform.position;
            Vector3 v_position = CameraThatDrawThisObject.ScreenToWorldPoint(Input.mousePosition); //get position, where mouse cursor is
            v_position.z = -1;
        }

        #endregion
    }
}
