using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kilt.Extensions;

namespace Kilt.EasyRopes2D.Examples
{
    public class FragmentSpawner : MonoBehaviour
    {

        #region Static Properties

        static int m_maxAmountOfFragmentsInScene = 40;
        static int m_currentAmountOfFragmentsInScene = 0;

        public static int MaxAmountOfFragmentsInScene
        {
            get
            {
                return m_maxAmountOfFragmentsInScene;
            }
            set
            {
                if (m_maxAmountOfFragmentsInScene == value)
                    return;
                m_maxAmountOfFragmentsInScene = value;
            }
        }

        public static int CurrentAmountOfFragmentsInScene
        {
            get
            {
                return m_currentAmountOfFragmentsInScene;
            }
        }

        public static void RegisterFragment(Fragment p_frag)
        {
            if (p_frag != null)
            {
                m_currentAmountOfFragmentsInScene++;
                if (m_currentAmountOfFragmentsInScene > MaxAmountOfFragmentsInScene)
                    UnregisterFragment(p_frag, true);
            }
        }

        public static void UnregisterFragment(Fragment p_frag, bool p_forceDestroy = false)
        {
            if (p_frag != null)
            {
                m_currentAmountOfFragmentsInScene--;
                m_currentAmountOfFragmentsInScene = Mathf.Max(0, m_currentAmountOfFragmentsInScene);
                if (p_forceDestroy)
                    Kilt.DestroyUtils.DestroyImmediate(p_frag.gameObject);
            }
        }

        public static void ResetFragmentsInScene()
        {
            m_currentAmountOfFragmentsInScene = 0;
        }

        #endregion

        #region Private Variables

        [SerializeField]
        List<Sprite> m_spriteList = new List<Sprite>();
        [SerializeField]
        GameObject m_individualFragmentEffect = null;
        [SerializeField]
        int m_maxAmountOfFragments = 5;
        [SerializeField]
        float m_minImpulseForce = 0.003f;
        [SerializeField]
        float m_maxImpulseForce = 0.012f;
        [SerializeField]
        float m_minAngularVelocity = 400;
        [SerializeField]
        float m_maxAngularVelocity = 800;
        [SerializeField]
        float m_lifeTime = 1;
        [SerializeField]
        Vector2 m_minInitialOffset = new Vector2(-0.5f, -0.5f);
        [SerializeField]
        Vector2 m_maxInitialOffset = new Vector2(0.5f, 0.5f);
        [SerializeField]
        float m_fragmentScale = 1f;
        [SerializeField]
        float m_fragmentMass = 0.008f;
        [SerializeField]
        float m_fragmentGravityScale = 0.7f;
        [SerializeField]
        bool m_createFragmentCollider = true;
        [SerializeField]
        bool m_enableIndividualFragmentEffect = true;
        [SerializeField]
        bool m_useOldRigidBodyVelocity = true;
        [SerializeField]
        bool m_isLocalDistance = true; // Min Max offsets is local distance

        #endregion

        #region Public Properties

        public bool IsLocalDistance
        {
            get
            {
                return m_isLocalDistance;
            }
            set
            {
                m_isLocalDistance = value;
            }
        }

        public List<Sprite> SpriteList
        {
            get
            {
                return this.m_spriteList;
            }
            set
            {
                m_spriteList = value;
            }
        }

        public GameObject IndividualFragmentEffect
        {
            get
            {
                return this.m_individualFragmentEffect;
            }
            set
            {
                m_individualFragmentEffect = value;
            }
        }

        public int MaxAmountOfFragments
        {
            get
            {
                return this.m_maxAmountOfFragments;
            }
            set
            {
                m_maxAmountOfFragments = value;
            }
        }

        public float MinImpulseForce
        {
            get
            {
                return this.m_minImpulseForce;
            }
            set
            {
                m_minImpulseForce = value;
            }
        }

        public float MaxImpulseForce
        {
            get
            {
                return this.m_maxImpulseForce;
            }
            set
            {
                m_maxImpulseForce = value;
            }
        }

        public float MinAngularVelocity
        {
            get
            {
                return this.m_minAngularVelocity;
            }
            set
            {
                m_minAngularVelocity = value;
            }
        }

        public float MaxAngularVelocity
        {
            get
            {
                return this.m_maxAngularVelocity;
            }
            set
            {
                m_maxAngularVelocity = value;
            }
        }

        public float LifeTime
        {
            get
            {
                return this.m_lifeTime;
            }
            set
            {
                m_lifeTime = value;
            }
        }

        public Vector2 MinInitialOffset
        {
            get
            {
                return this.m_minInitialOffset;
            }
            set
            {
                m_minInitialOffset = value;
            }
        }

        public Vector2 MaxInitialOffset
        {
            get
            {
                return this.m_maxInitialOffset;
            }
            set
            {
                m_maxInitialOffset = value;
            }
        }

        public float FragmentScale
        {
            get
            {
                return this.m_fragmentScale;
            }
            set
            {
                m_fragmentScale = value;
            }
        }

        public float FragmentMass
        {
            get
            {
                return this.m_fragmentMass;
            }
            set
            {
                m_fragmentMass = value;
            }
        }

        public float FragmentGravityScale
        {
            get
            {
                return this.m_fragmentGravityScale;
            }
            set
            {
                m_fragmentGravityScale = value;
            }
        }

        public bool CreateFragmentCollider
        {
            get
            {
                return this.m_createFragmentCollider;
            }
            set
            {
                m_createFragmentCollider = value;
            }
        }

        public bool EnableIndividualFragmentEffect
        {
            get
            {
                return this.m_enableIndividualFragmentEffect;
            }
            set
            {
                m_enableIndividualFragmentEffect = value;
            }
        }

        public bool UseOldRigidBodyVelocity
        {
            get
            {
                return this.m_useOldRigidBodyVelocity;
            }
            set
            {
                m_useOldRigidBodyVelocity = value;
            }
        }

        #endregion

        #region Unity Functions

        protected virtual void OnDrawGizmos()
        {
            try
            {
                Vector2 v_initialMaxOffset = GetGlobalMaxInitialOffSet();
                Vector2 v_initialMinOffset = GetGlobalMinInitialOffSet();

                Vector2 v_size = new Vector2(v_initialMaxOffset.x - v_initialMinOffset.x, v_initialMaxOffset.y - v_initialMinOffset.y);
                Vector2 v_middlePoint = new Vector2((v_initialMaxOffset.x + v_initialMinOffset.x) / 2, (v_initialMaxOffset.y + v_initialMinOffset.y) / 2);
                Vector3 v_scaleVector = new Vector3(transform.localScale.x < 0 ? -1 : 1, transform.localScale.y < 0 ? -1 : 1, transform.localScale.z < 0 ? -1 : 1);
                Matrix4x4 v_rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, v_scaleVector);
                Gizmos.matrix = v_rotationMatrix;
                Gizmos.color = new Color(1, 0.5f, 0, 0.4f);
                Gizmos.DrawWireCube(v_middlePoint, v_size);
            }
            catch { }
        }

        #endregion

        #region Helper Methods

        public Vector2 GetGlobalMaxInitialOffSet()
        {
            Vector2 v_return = m_maxInitialOffset;
            if (m_isLocalDistance)
            {
                //TODO
                v_return = new Vector2(m_maxInitialOffset.x * transform.lossyScale.x, m_maxInitialOffset.y * transform.lossyScale.y);
                //v_return = new Vector2(
            }
            return v_return;
        }

        public Vector2 GetGlobalMinInitialOffSet()
        {
            Vector2 v_return = m_minInitialOffset;
            if (m_isLocalDistance)
            {
                //TODO
                v_return = new Vector2(m_minInitialOffset.x * transform.lossyScale.x, m_minInitialOffset.y * transform.lossyScale.y);
            }
            return v_return;
        }

        public virtual void SpawnFragments()
        {
            List<Sprite> v_suffledSpriteList = m_spriteList.CloneList();
            v_suffledSpriteList.Shuffle();


            while (v_suffledSpriteList.Count < m_maxAmountOfFragments && m_spriteList.Count > 0)
            {
                for (int i = 0; i < m_spriteList.Count; i++)
                {
                    if (v_suffledSpriteList.Count < m_maxAmountOfFragments)
                        v_suffledSpriteList.Add(m_spriteList[i]);
                }
            }
            int v_fragNumber = Mathf.Max(0, Mathf.Min(m_maxAmountOfFragments, v_suffledSpriteList.Count));
            int v_fragSlotsNumberInScene = Mathf.Max(0, MaxAmountOfFragmentsInScene - CurrentAmountOfFragmentsInScene);
            v_fragNumber = v_fragNumber > v_fragSlotsNumberInScene ? v_fragSlotsNumberInScene : v_fragNumber;
            GameObject v_newParent = CreateNewFragmentContainer();
            for (int i = 0; i < v_fragNumber; i++)
            {
                //GameObject v_frag = CreateFragment(v_newParent, v_suffledSpriteList[i], i);
                CreateFragment(v_newParent, v_suffledSpriteList[i], i);
            }
            v_newParent.transform.rotation = this.transform.rotation;
        }

        protected virtual GameObject CreateNewFragmentContainer()
        {
            GameObject v_newParent = new GameObject("FragmentContainer");
            v_newParent.transform.parent = this.transform.parent;
            v_newParent.transform.localScale = new Vector3(1, 1, 1);
            v_newParent.transform.position = this.transform.position;
            FragmentContainer v_frag = v_newParent.AddComponent<FragmentContainer>();
            v_frag.LifeTime = m_lifeTime;
            return v_newParent;
        }

        protected virtual GameObject CreateFragment(GameObject p_parent, Sprite p_sprite, int p_index)
        {
            Vector2 v_minInitialOffset = GetGlobalMinInitialOffSet();
            Vector2 v_maxInitialOffset = GetGlobalMaxInitialOffSet();

            GameObject v_object = new GameObject("Frag");
            v_object.transform.parent = p_parent != null ? p_parent.transform : this.transform.parent;
            v_object.transform.localScale = new Vector3(m_fragmentScale * this.transform.localScale.x, m_fragmentScale * this.transform.localScale.y, m_fragmentScale * this.transform.localScale.z);
            v_object.transform.position = new Vector3(this.transform.position.x + Random.Range(v_minInitialOffset.x, v_maxInitialOffset.x), this.transform.position.y + Random.Range(v_minInitialOffset.y, v_maxInitialOffset.y), this.transform.position.z);
            v_object.transform.rotation = this.transform.rotation;
            v_object.AddComponent<Fragment>();
            Rigidbody2D v_body = v_object.AddComponent<Rigidbody2D>();
            if (m_createFragmentCollider)
            {
                CircleCollider2D v_collider = v_object.AddComponent<CircleCollider2D>();
                v_collider.radius = 0.01f;
                CircleCollider2D v_collider2 = v_object.AddComponent<CircleCollider2D>();
                v_collider2.radius = 0.01f;
                v_collider2.offset = new Vector2(0.005f, 0.005f);
            }
            if (this.GetComponent<Rigidbody2D>() != null)
            {
                v_body.mass = Mathf.Max(0.001f, m_fragmentMass);//(rigidbody2D.mass/Mathf.Max(1,m_maxAmountOfFragments))/25;
                v_body.velocity = UseOldRigidBodyVelocity ? GetComponent<Rigidbody2D>().velocity : Vector2.zero;
                v_body.angularVelocity = UseOldRigidBodyVelocity ? GetComponent<Rigidbody2D>().angularVelocity : 0;
                v_body.gravityScale = m_fragmentGravityScale;

            }
            SpriteRenderer v_spriteRenderer = v_object.AddComponent<SpriteRenderer>();
            v_spriteRenderer.sprite = p_sprite;
            SpriteRenderer v_currentSpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            if (v_currentSpriteRenderer != null)
            {
                v_spriteRenderer.color = v_currentSpriteRenderer.color; //renderer.material.GetColor("_Color");
                v_spriteRenderer.material = v_currentSpriteRenderer.material;
                v_spriteRenderer.sortingLayerID = v_currentSpriteRenderer.sortingLayerID;
                v_spriteRenderer.sortingLayerName = v_currentSpriteRenderer.sortingLayerName;
                v_spriteRenderer.sortingOrder = 15;
            }
            AddRandomImpulse(v_body);
            AddRandomRotation(v_body);
            if (m_enableIndividualFragmentEffect && m_individualFragmentEffect != null)
            {
                GameObject v_effect = GameObject.Instantiate(m_individualFragmentEffect) as GameObject;
                v_effect.transform.parent = v_object.transform;
                v_effect.transform.localPosition = Vector2.zero;
                v_effect.transform.rotation = Quaternion.identity;
                v_effect.transform.localScale = Vector3.one;
            }
            //PauseManager.Pause();
            return v_object;
        }

        public void AddRandomImpulse(Rigidbody2D p_body)
        {
            if (p_body != null)
            {
                float v_randomImpulse = Random.Range(m_minImpulseForce, m_maxImpulseForce);
                Vector2 v_finalVector = RopeInternalUtils.GetVectorDirection(p_body.gameObject.transform.parent.position, p_body.gameObject.transform.position);
                v_finalVector = new Vector2(v_finalVector.x * v_randomImpulse, v_finalVector.y * v_randomImpulse);
                p_body.AddForce(v_finalVector, ForceMode2D.Impulse);
                //p_body.AddForce(new Vector2(0,v_randomImpulse));
            }
        }

        public void AddRandomRotation(Rigidbody2D p_body)
        {
            if (p_body != null)
            {
                float v_randomRotation = Random.Range(m_minAngularVelocity, m_maxAngularVelocity);
                p_body.angularVelocity += v_randomRotation;
            }
        }

        #endregion
    }
}
