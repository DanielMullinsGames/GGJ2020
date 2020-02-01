using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kilt.Extensions;

public enum AutomaticDestroyEnum {DontDestroy, DestroyWhenOutOfBounds, DestroyWhenOutOfCamera, DestroyWhenNotColliding, DestroyWhenOutOfCameraOrNotColliding}

namespace Kilt.EasyRopes2D.Examples
{
    [SelectionBase]
    public abstract class DamageableBlock : MonoBehaviour
    {

        #region Static Properties

        static int m_globalPressCount = 0;

        //This mode is useful when you must call only some methods in controllers (Every object must Implement your own controller to use this variable)
        public static bool GlobalPressOnly
        {
            get
            {
                return m_globalPressCount > 0 ? true : false;
            }
        }

        public static void AddInGlobalPressCount()
        {
            m_globalPressCount += 1;
        }

        public static void SubInGlobalPressCount()
        {
            m_globalPressCount -= 1;
            m_globalPressCount = Mathf.Max(0, m_globalPressCount);
        }

        public static void ResetGlobalPressCount()
        {
            m_globalPressCount = 0;
        }

        #endregion

        #region Events

        public event System.Action<DamageEventArgs> OnDamaged;
        public event System.Action OnDie;
        public event System.Action OnDestroyedByUser;

        #endregion

        #region Private Variables

        [SerializeField]
        List<Sprite> m_damageSprites = new List<Sprite>();
        [SerializeField]
        GameObject m_collisionEffectObject = null;
        [SerializeField]
        GameObject m_destroyEffectObject = null;
        [SerializeField]
        float m_minDamageToShowCollisionEffect = 0.3f;
        [SerializeField]
        float m_maxLife = 10f;
        [SerializeField]
        float _currentLife = -1f;
        float m_currentLife = -1f;
        [SerializeField]
        float m_attack = 1; // Multiply damage by this value when generating damage;
        [SerializeField]
        float m_defense = 1; // Multiply damage by this value when receiving damage (Except in TRUE DAMAGE);
        [SerializeField]
        AutomaticDestroyEnum m_automaticDestroyPolitic = AutomaticDestroyEnum.DestroyWhenOutOfCamera;
        [SerializeField]
        int m_maxDistanceToOutOfBounds = 5; // In ExtraCameras. Ex. 0 = CameraSize, 1 = CameraSize*2, 2 = CameraSize*3, etc...
        [SerializeField]
        bool m_useFragments = true;
        [SerializeField]
        bool m_acceptUserClick = true;
        [SerializeField]
        bool m_safeRemove = false; //This Parameter Prevent Call Win/Lose

        float m_timeToDestroy = 1f;
        float m_currentTimeToDestroy = 1f;
        float _currentTimeToEnableApplyDamage = 0.8f;
        float _maxCurrentTimeToEnableApplyDamage = 0.8f;
        FragmentSpawner m_fragmentSpawner = null;
        float _timeToCheckIfInsideCameraView = 1f;
        float _currentTimeCheckToIfInsideCameraView = 1f;
        bool _isInsideCamera = true;
        Vector2 _oldVelocity = Vector2.zero; // Use This To Get The Correct Relactive Velocity of two objects
        List<GameObject> _recentCollidedObjects = new List<GameObject>();

        #endregion

        #region Public Properties

        public List<GameObject> RecentCollidedObjects
        {
            get
            {
                return _recentCollidedObjects;
            }
            protected set
            {
                _recentCollidedObjects = value;
            }
        }

        public Vector2 OldVelocity
        {
            get
            {
                return _oldVelocity;
            }
            protected set
            {
                _oldVelocity = value;
            }
        }

        public int MaxDistanceToOutOfBounds
        {
            get
            {
                return m_maxDistanceToOutOfBounds;
            }
            set
            {
                m_maxDistanceToOutOfBounds = value;
            }
        }

        public bool AcceptUserClick
        {
            get
            {
                return m_acceptUserClick;
            }
            set
            {
                m_acceptUserClick = value;
            }
        }

        public bool SafeRemove
        {
            get
            {
                return m_safeRemove;
            }
            set
            {
                m_safeRemove = value;
            }
        }

        public float Attack
        {
            get
            {
                return m_attack;
            }
            set
            {
                m_attack = value;
            }
        }

        public float Defense
        {
            get
            {
                return m_defense;
            }
            set
            {
                m_defense = value;
            }
        }

        SpriteRenderer _spriteRendererComponent = null;
        public SpriteRenderer SpriteRendererComponent
        {
            get
            {
                if (_spriteRendererComponent == null)
                    _spriteRendererComponent = GetComponent<SpriteRenderer>();
                return _spriteRendererComponent;
            }
        }

        public bool IsInsideCamera
        {
            get
            {
                return _isInsideCamera;
            }
            protected set
            {
                _isInsideCamera = value;
            }
        }

        public AutomaticDestroyEnum AutomaticDestroyPolitic
        {
            get
            {
                return m_automaticDestroyPolitic;
            }
            set
            {
                m_automaticDestroyPolitic = value;
            }
        }

        public bool UseFragments
        {
            get
            {
                return m_useFragments;
            }
            set
            {
                m_useFragments = value;
            }
        }

        public float MinDamageToShowCollisionEffect
        {
            get
            {
                return m_minDamageToShowCollisionEffect;
            }
            set
            {
                m_minDamageToShowCollisionEffect = value;
            }
        }

        public List<Sprite> DamageSprites
        {
            get
            {
                return m_damageSprites;
            }
            set
            {
                m_damageSprites = value;
            }
        }


        public GameObject CollisionEffectObject
        {
            get
            {
                return m_collisionEffectObject;
            }
            set
            {
                m_collisionEffectObject = value;
            }
        }

        public GameObject DestroyEffectObject
        {
            get
            {
                return m_destroyEffectObject;
            }
            set
            {
                m_destroyEffectObject = value;
            }
        }

        public FragmentSpawner FragmentSpawner
        {
            get
            {
                if (m_fragmentSpawner == null)
                {
                    foreach (FragmentSpawner v_spawner in gameObject.GetComponents<FragmentSpawner>())
                    {
                        if (v_spawner.GetType() == typeof(FragmentSpawner))
                        {
                            m_fragmentSpawner = v_spawner;
                            break;
                        }
                    }
                }
                return m_fragmentSpawner;
            }
        }

        public float MaxLife { get { return m_maxLife; } set { m_maxLife = value; } }

        public float CurrentLife
        {
            get { return _currentLife; }
            set
            {
                _currentLife = value;
                m_currentLife = value;
                if (DamageSprites != null && DamageSprites.Count > 1)
                {
                    //float v_amount = m_currentLife /MaxLife;
                    float v_currentToChangeSprite = m_currentLife <= 0 && !Application.isPlaying ? m_maxLife : m_currentLife;
                    int v_index = Mathf.Clamp(Mathf.FloorToInt((DamageSprites.Count) - ((DamageSprites.Count) * Mathf.Clamp(v_currentToChangeSprite / Mathf.Max(0.001f, MaxLife), 0, 1))), 0, DamageSprites.Count - 1);
                    if (SpriteRendererComponent != null && DamageSprites[v_index] != null)
                        SpriteRendererComponent.sprite = DamageSprites[v_index];
                }
            }
        }

        #endregion

        #region Event Implementations

        //protected bool _stageFinished = false;

        protected virtual void OnStageBeginEnd()
        {
            //_stageFinished = true;
        }

        protected virtual void OnLose()
        {
        }

        protected virtual void OnWin()
        {
        }

        #endregion

        #region Unity Functions

        protected virtual void Awake()
        {
            CorrectCurrentDamageInEditor(true);
            if (SpriteRendererComponent != null && SpriteRendererComponent.sprite != null &&
               DamageSprites != null && !DamageSprites.Contains(SpriteRendererComponent.sprite))
                DamageSprites.Insert(0, SpriteRendererComponent.sprite);
            CurrentLife = CurrentLife <= 0 ? MaxLife : Mathf.Clamp(CurrentLife, 0, MaxLife);

            m_currentTimeToDestroy = m_timeToDestroy;
            AddInBlockType(1);
        }

        protected virtual void OnDestroy()
        {
            AddInBlockType(-1);
        }

        // Update is called once per frame
        protected virtual void Update()
        {
            CorrectCurrentDamageInEditor();
            UpdateTimeToApplyDamage();
            UpdateDestroyTimer();
            CheckIfBlockDied();
        }

        //int _counter = 0;
        protected virtual void LateUpdate()
        {
            OldVelocity = GetComponent<Rigidbody2D>() != null ? GetComponent<Rigidbody2D>().velocity : Vector2.zero;
        }

        #endregion

        #region Events Receivers

        protected void OnPress(bool p_isDown)
        {
            if (Input.GetMouseButton(0))
            {
                SimulateOnPress(p_isDown);
            }
        }

        //Dont Check MouseButton in this Method
        public void SimulateOnPress(bool p_isDown)
        {
            if (CanBeClicked() && p_isDown)
            {
                OnUserClick();
            }
        }

        public virtual void OnUserClick()
        {
        }

        #endregion

        #region Helper Functions

        #region Damage

        private void UpdateTimeToApplyDamage()
        {
            _currentTimeToEnableApplyDamage = Mathf.Max(0, _currentTimeToEnableApplyDamage - Time.deltaTime);
            if (_currentTimeToEnableApplyDamage <= 0 && RecentCollidedObjects.Count > 0)
                RecentCollidedObjects.Clear();
        }

        bool _markToDie = false;
        public void MarkToDie()
        {
            _markToDie = true;
        }

        public void CheckIfBlockDied(bool p_forceDie = false)
        {
            if (_markToDie || p_forceDie)
            {
                _markToDie = false;
                //gameObject.SetActive(false);
                if (OnDie != null)
                    OnDie();
                StartDestroyEffects(UseFragments);
                //KiltUtils.DestroyInEditor(this.gameObject);
                Destroy(this.gameObject);
            }
        }

        public float ApplyTrueDamage(float p_damage)
        {
            return ApplyDamage(p_damage, true, false, true);
        }

        public float ApplySimpleDamage(float p_damage)
        {
            return ApplyDamage(p_damage, true, false, false);
        }

        public float ApplyDamage(float p_damage, bool p_forceApplyDamage = false, bool p_forceDestroyImmediate = false, bool p_isTrueDamage = false)
        {
            float v_damageApplyed = 0;
            {
                p_damage = p_isTrueDamage ? p_damage : p_damage * Defense;
                if (p_damage >= MinDamageToShowCollisionEffect || p_forceApplyDamage)
                {
                    DamageEventArgs v_args = new DamageEventArgs(this, p_damage);
                    //CallEvent OnDamage
                    if (p_damage > 0)
                    {
                        if (OnDamaged != null)
                            OnDamaged(v_args);

                        CurrentLife -= v_args.Damage;
                        if (CurrentLife <= 0)
                        {
                            if (p_forceDestroyImmediate)
                                Destroy();
                            else
                                MarkToDie();
                        }
                        v_damageApplyed = p_damage;
                    }
                }
            }
            return v_damageApplyed;
        }

        #endregion

        #region Others

        public virtual bool CanBeClicked()
        {
            bool v_canBeClicked = DamageableBlock.GlobalPressOnly ? false : AcceptUserClick;
            return v_canBeClicked;
        }

        protected virtual void CorrectCurrentDamageInEditor(bool p_force = false)
        {
            if (p_force || _currentLife != m_currentLife)
                CurrentLife = _currentLife;
        }

        public virtual bool CheckIfIsInsideMaxLimits()
        {
            Camera v_camera = RopeInternalUtils.GetCameraThatDrawLayer(gameObject.layer);
            if (v_camera != null)
            {
                Vector2 v_viewPortPos = v_camera.WorldToViewportPoint(transform.position);
                if (v_viewPortPos.x >= -MaxDistanceToOutOfBounds && v_viewPortPos.x <= MaxDistanceToOutOfBounds + 1 && v_viewPortPos.y >= -MaxDistanceToOutOfBounds && v_viewPortPos.y <= MaxDistanceToOutOfBounds + 1)
                {
                    return true;
                }
            }
            return false;
        }

        public virtual bool CheckIfIsInsideCameraView()
        {
            return !RopeInternalUtils.IsOutOfScreen(gameObject, false, true);
        }

        protected virtual void UpdateIsInsideCameraTimer()
        {
            if (AutomaticDestroyPolitic == AutomaticDestroyEnum.DestroyWhenOutOfCamera || AutomaticDestroyPolitic == AutomaticDestroyEnum.DestroyWhenOutOfCameraOrNotColliding)
            {
                if (_currentTimeCheckToIfInsideCameraView > 0)
                {
                    _currentTimeCheckToIfInsideCameraView = Mathf.Max(0, _currentTimeCheckToIfInsideCameraView - Time.deltaTime);
                    if (_currentTimeCheckToIfInsideCameraView <= 0)
                    {
                        IsInsideCamera = CheckIfIsInsideCameraView();
                        _currentTimeCheckToIfInsideCameraView = _timeToCheckIfInsideCameraView;
                    }

                }
                else
                {
                    IsInsideCamera = CheckIfIsInsideCameraView();
                    _currentTimeCheckToIfInsideCameraView = _timeToCheckIfInsideCameraView;
                }
            }
        }

        protected virtual void UpdateDestroyTimer()
        {
            UpdateIsInsideCameraTimer();
            if (AutomaticDestroyPolitic != AutomaticDestroyEnum.DontDestroy)
            {
                if (m_currentTimeToDestroy > 0)
                {
                    m_currentTimeToDestroy = Mathf.Max(0, m_currentTimeToDestroy - Time.deltaTime);
                    if (CheckIfIsInsideMaxLimits())
                    {
                        if (AutomaticDestroyPolitic == AutomaticDestroyEnum.DestroyWhenOutOfCameraOrNotColliding || AutomaticDestroyPolitic == AutomaticDestroyEnum.DestroyWhenNotColliding)
                        {
                            if (GetComponent<Rigidbody2D>() != null)
                            {
                                if (Mathf.Abs(GetComponent<Rigidbody2D>().velocity.y) < 0.02f && Mathf.Abs(GetComponent<Rigidbody2D>().angularVelocity) < 2)
                                    m_currentTimeToDestroy = m_timeToDestroy;
                            }
                        }
                        if (AutomaticDestroyPolitic == AutomaticDestroyEnum.DestroyWhenOutOfCameraOrNotColliding || AutomaticDestroyPolitic == AutomaticDestroyEnum.DestroyWhenOutOfCamera)
                        {
                            if (m_currentTimeToDestroy <= 0)
                                IsInsideCamera = CheckIfIsInsideCameraView();
                            if (IsInsideCamera)
                                m_currentTimeToDestroy = m_timeToDestroy;
                        }
                        if (AutomaticDestroyPolitic == AutomaticDestroyEnum.DestroyWhenOutOfBounds)
                            m_currentTimeToDestroy = m_timeToDestroy;
                    }
                    if (m_currentTimeToDestroy <= 0)
                        DestroyImmediate(this.gameObject);
                }
                else
                    DestroyImmediate(this.gameObject);
            }
            else
            {
                m_currentTimeToDestroy = m_timeToDestroy;
            }
        }

        protected virtual void StartCollisionEffects(Vector2 p_collisionLocation)
        {
            Vector2 v_directionVector = RopeInternalUtils.GetVectorDirection(p_collisionLocation, this.transform.position);
            float v_angle = Vector2.Angle(Vector2.right, v_directionVector) + 180;
            if (CollisionEffectObject != null)
            {
                GameObject v_effectObject = GameObject.Instantiate(CollisionEffectObject) as GameObject;
                v_effectObject.transform.position = p_collisionLocation;
                v_effectObject.transform.rotation = Quaternion.Euler(Vector3.zero);
                v_effectObject.transform.parent = this.transform.parent;
                //Force Scale to original one
                //v_effectObject.transform.rotation = CollisionEffectObject.transform.rotation;
                //v_effectObject.transform.localScale = DestroyEffectObject.transform.localScale;
                if (DestroyEffectObject != null)
                    RopeInternalUtils.SetLossyScale(v_effectObject.transform, DestroyEffectObject.transform.localScale);
                v_effectObject.transform.Rotate(new Vector3(0, 0, v_angle));
            }

        }

        protected virtual void StartDestroyEffects(bool p_useFragments)
        {
            if (FragmentSpawner != null && p_useFragments)
                FragmentSpawner.SpawnFragments();
            BlockUtils.InstantiateEffectOverOwner(this.transform, DestroyEffectObject, false, true);
        }

        public virtual void Destroy()
        {
            //gameObject.SetActive(false);
            if (OnDestroyedByUser != null)
                OnDestroyedByUser();
            StartDestroyEffects(UseFragments);
            Kilt.DestroyUtils.Destroy(this.gameObject);
            //DestroyImmediate(this.gameObject);
        }

        protected virtual void AddInBlockType(int p_amount)
        {
        }

        public virtual bool IsFallingOrRotating()
        {
            if (GetComponent<Rigidbody2D>() != null)
            {
                //Debug.Log("Linear : "  + rigidbody2D.velocity + " Angular : " + rigidbody2D.angularVelocity);
                if (Mathf.Abs(GetComponent<Rigidbody2D>().velocity.y) > 0.7f || Mathf.Abs(GetComponent<Rigidbody2D>().angularVelocity) > 25)
                {
                    return true;
                }
            }
            return false;
        }

        public virtual bool IsUnderMove()
        {
            if (GetComponent<Rigidbody2D>() != null)
            {
                if (Mathf.Abs(GetComponent<Rigidbody2D>().velocity.x) > 0.01f || Mathf.Abs(GetComponent<Rigidbody2D>().velocity.y) > 0.01f || Mathf.Abs(GetComponent<Rigidbody2D>().angularVelocity) > 25)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #endregion

        #region Collision Controller

        public Vector2 GetRelativeVelocity(GameObject p_object1, GameObject p_object2)
        {
            Vector2 p_velocity1 = Vector2.zero;
            Vector2 p_velocity2 = Vector2.zero;
            if (p_object1 != null && p_object2 != null)
            {
                DamageableBlock v_block1 = p_object1.GetComponent<DamageableBlock>();
                DamageableBlock v_block2 = p_object2.GetComponent<DamageableBlock>();

                p_velocity1 = v_block1 != null ? v_block1.OldVelocity : (p_object1.GetComponent<Rigidbody2D>() != null ? p_object1.GetComponent<Rigidbody2D>().velocity : Vector2.zero);
                p_velocity2 = v_block2 != null ? v_block2.OldVelocity : (p_object2.GetComponent<Rigidbody2D>() != null ? p_object2.GetComponent<Rigidbody2D>().velocity : Vector2.zero);
            }
            float v_impactVelocityX = p_velocity1.x - p_velocity2.x;
            v_impactVelocityX *= Mathf.Sign(v_impactVelocityX);
            float v_impactVelocityY = p_velocity1.y - p_velocity2.y;
            v_impactVelocityY *= Mathf.Sign(v_impactVelocityY);

            return new Vector2(v_impactVelocityX, v_impactVelocityY);
        }

        public float GetImpactVelocityMagnitude(GameObject p_object1, GameObject p_object2, float p_scale = 1.5f)
        {
            return (GetRelativeVelocity(p_object1, p_object2).magnitude * p_scale);
        }

        public float GetImpactForce(GameObject p_otherObject)
        {
            float v_otherMass = 0;
            if (p_otherObject != null)
            {
                if (p_otherObject.GetComponent<Rigidbody2D>() != null)
                    v_otherMass = p_otherObject.GetComponent<Rigidbody2D>().mass;
                else if (this.GetComponent<Rigidbody2D>() != null)
                    v_otherMass = this.GetComponent<Rigidbody2D>().mass;
            }

            return GetImpactForce(v_otherMass, GetImpactVelocityMagnitude(this.gameObject, p_otherObject));
        }

        public float GetImpactForce(float p_mass, float p_impactMagnitude)
        {
            float v_impactForce = Mathf.Abs(p_impactMagnitude * p_mass);
            return v_impactForce;
        }

        //Used To Reduce Attack When in Low Velocities
        public float GetForceAttenuation()
        {
            //float v_attenuation = 1;
            float v_attenuation = Mathf.Clamp(OldVelocity.magnitude, 0.4f, 1);
            return v_attenuation;
        }

        protected virtual void OnCollisionEnter2D(Collision2D p_collision)
        {
            GameObject v_otherCollidedObject = p_collision.gameObject;
            if (v_otherCollidedObject != null && GetComponent<Rigidbody2D>() != null && !RecentCollidedObjects.Contains(v_otherCollidedObject))
            {
                DamageableBlock v_otherBlock = v_otherCollidedObject.GetComponent<DamageableBlock>();
                float v_impactForce = GetImpactForce(p_collision.gameObject);
                float v_attack = v_otherBlock != null ? v_otherBlock.Attack * v_otherBlock.GetForceAttenuation() : (v_otherCollidedObject.GetComponent<Rigidbody2D>() == null ? Attack * 1.5f * this.GetForceAttenuation() : 0);
                float v_attackImpact = v_impactForce * v_attack;
                float v_damageApplyed = ApplyDamage(v_attackImpact);
                if (v_damageApplyed > 0)
                    RecentCollidedObjects.AddChecking(v_otherCollidedObject.gameObject);
                if (p_collision.contacts.Length > 0 && v_damageApplyed > MinDamageToShowCollisionEffect)
                {
                    if (p_collision.contacts.Length > 0)
                        StartCollisionEffects(p_collision.contacts[0].point);
                }
                _currentTimeToEnableApplyDamage = _currentTimeToEnableApplyDamage <= 0 ? _maxCurrentTimeToEnableApplyDamage : _currentTimeToEnableApplyDamage;
                //OldVelocity = rigidbody2D != null? rigidbody2D.velocity : Vector2.zero;
            }
        }

        protected virtual void OnCollisionStay2D(Collision2D p_collision)
        {
            //Debug.Log("Collision Stay");
            //foreach (ContactPoint contact in p_collision.contacts)
            //	Debug.DrawRay(contact.point, contact.normal * 10, Color.white);
            if (AutomaticDestroyPolitic != AutomaticDestroyEnum.DontDestroy && CheckIfIsInsideMaxLimits())
            {
                if (AutomaticDestroyPolitic == AutomaticDestroyEnum.DestroyWhenNotColliding || AutomaticDestroyPolitic == AutomaticDestroyEnum.DestroyWhenOutOfCameraOrNotColliding)
                    m_currentTimeToDestroy = m_timeToDestroy;
            }

            //Debug.Log("Colidindo");
        }

        /*protected virtual void OnCollisionExit2D(Collision2D p_collision) {

        }*/

        #endregion
    }

    #region Helper Classes

    public class DamageEventArgs
    {
        #region Private Variables

        [SerializeField]
        float m_damage = 0;
        [SerializeField]
        DamageableBlock m_sender = null;

        #endregion

        #region Public Properties

        public float Damage { get { return m_damage; } set { m_damage = value; } }
        public DamageableBlock Sender { get { return m_sender; } set { m_sender = value; } }

        #endregion

        public DamageEventArgs(DamageableBlock p_sender, float p_damage)
        {
            m_damage = p_damage;
            m_sender = p_sender;
        }
    }

    #endregion
}
