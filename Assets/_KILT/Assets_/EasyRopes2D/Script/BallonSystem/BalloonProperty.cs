using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kilt;
using Kilt.Extensions;

namespace Kilt.EasyRopes2D
{
    public enum AutomaticForceCalculatorCallerEnum { Never, OnAwake, OnStart, OnEnabled, OnFirstFixedUpdate }

    public class BalloonProperty : MonoBehaviour
    {
        #region Private Variables

        [SerializeField]
        List<BallonObjectToCloneByMass> m_spriteChangerModel = new List<BallonObjectToCloneByMass>(); // Used to Change Sprite of Ballon (Visual Effect)
        [SerializeField]
        float m_extraThrust = 0;// Offset Force (not affected by BallonForceScale)
        [SerializeField]
        float m_ballonForceScale = 1f; //Used to ponderate the strength of other object force vector
        [SerializeField]
        float m_ropeForceScale = 1f; //Used to ponderate the strength of direct rope component in force vector
        [SerializeField]
        float m_indirectRopeForceScale = 1f; //Used to ponderate the strength of indirect rope component in force vector
        [SerializeField]
        AutomaticForceCalculatorCallerEnum m_automaticForceCalculatorOption = AutomaticForceCalculatorCallerEnum.OnFirstFixedUpdate; // Used To Make Ballon Calc exact force to make pluggedObject in same location in Air
        [SerializeField]
        bool m_includeIndirectRopesInMassCalc = true;

        //Aux Variables
        bool _needRecalcForce = false;
        bool _needRecalcAndClear = false;
        float _selfMass = 0; // Include Childrens
        float _sumOfPluggedRopesMass = 0;
        float _sumOfIndirectRopesMass = 0;
        float _sumOfOtherObjectsMass = 0; // Sum of objects in Other side of the rope
                                          //Total Sum of the mass = _sumOfPluggedComponentsMass + _sumOfOtherObjectsMass

        //Lerped Values To prevent Insane forces in Recalc
        float _selfMassLerp = 0;
        float _sumOfPluggedRopesMassLerp = 0;
        float _sumOfIndirectRopesMassLerp = 0;
        float _sumOfOtherObjectsMassLerp = 0; // Sum of objects in Other side of the rope

        #endregion

        #region Public Properties

        public bool IncludeIndirectRopesInMassCalc
        {
            get
            {
                return m_includeIndirectRopesInMassCalc;
            }
            set
            {
                if (m_includeIndirectRopesInMassCalc == value)
                    return;
                m_includeIndirectRopesInMassCalc = value;
                MarkToRecalcForce();
            }
        }

        RopesAttached _ropesAttachedComponent = null;
        public RopesAttached RopesAttachedComponent
        {
            get
            {
                if (_ropesAttachedComponent == null)
                {
                    _ropesAttachedComponent = GetComponent<RopesAttached>();
                    if (_ropesAttachedComponent == null)
                        _ropesAttachedComponent = gameObject.AddComponent<RopesAttached>();
                }
                return _ropesAttachedComponent;
            }
        }

        public AutomaticForceCalculatorCallerEnum AutomaticForceCalculatorOption
        {
            get
            {
                return m_automaticForceCalculatorOption;
            }
            set
            {
                if (m_automaticForceCalculatorOption == value)
                    return;
                m_automaticForceCalculatorOption = value;
            }
        }

        public float BallonForceScale
        {
            get
            {
                return m_ballonForceScale;
            }
            set
            {
                if (m_ballonForceScale == value)
                    return;
                m_ballonForceScale = value;
                CloneModelForceBallonComponents();
            }
        }

        public float IndirectRopeForceScale
        {
            get
            {
                return m_indirectRopeForceScale;
            }
            set
            {
                if (m_indirectRopeForceScale == value)
                    return;
                m_indirectRopeForceScale = value;
            }
        }

        public float RopeForceScale
        {
            get
            {
                return m_ropeForceScale;
            }
            set
            {
                if (m_ropeForceScale == value)
                    return;
                m_ropeForceScale = value;
            }
        }

        public float ExtraThrust
        {
            get
            {
                return m_extraThrust;
            }
            set
            {
                if (m_extraThrust == value)
                    return;
                m_extraThrust = value;
                CloneModelForceBallonComponents();
            }
        }

        public List<BallonObjectToCloneByMass> SpriteChangerModel
        {
            get
            {
                return m_spriteChangerModel;
            }
            set
            {
                m_spriteChangerModel = value;
            }
        }

        Rigidbody2D _rigidBody2DComponent = null;
        public Rigidbody2D RigidBody2DComponent
        {
            get
            {
                if (_rigidBody2DComponent == null)
                    _rigidBody2DComponent = this.GetNonMarkedComponentInChildren<Rigidbody2D>();
                return _rigidBody2DComponent;
            }
        }

        #endregion

        #region Unity Functions

        protected virtual void Awake()
        {
            InitialRecalcCorrector();
            if (AutomaticForceCalculatorOption == AutomaticForceCalculatorCallerEnum.OnAwake)
            {
                RecalcForces(true);
                CloneModelForceBallonComponents();
            }
        }

        protected virtual void Start()
        {
            RegisterInitialEventsReceivers();
            if (AutomaticForceCalculatorOption == AutomaticForceCalculatorCallerEnum.OnStart)
            {
                RecalcForces(true);
                CloneModelForceBallonComponents();
            }
            else if (AutomaticForceCalculatorOption == AutomaticForceCalculatorCallerEnum.Never)
            {
                RecalcForces(true, true); // Recalc Ropes Only
                CloneModelForceBallonComponents();
            }
        }

        protected virtual void OnEnable()
        {
            SetFixedAngleActive(true);
            if (AutomaticForceCalculatorOption == AutomaticForceCalculatorCallerEnum.OnEnabled)
            {
                RecalcForces(true);
                CloneModelForceBallonComponents();
            }
        }

        protected virtual void OnDisable()
        {
            SetFixedAngleActive(false);
        }

        bool _firstFixedUpdate = true;
        protected virtual void FixedUpdate()
        {
            if (_waitOneCycle)
            {
                _waitOneCycle = false;
                RecalcRopesMass();
            }
            if (_firstFixedUpdate)
            {
                _firstFixedUpdate = false;
                if (AutomaticForceCalculatorOption == AutomaticForceCalculatorCallerEnum.OnFirstFixedUpdate)
                {
                    RecalcForces(true);
                    CloneModelForceBallonComponents();
                }
            }
            if (RigidBody2DComponent != null)
            {
                RecalcForcesAndClearBody(false, true);
                RecalcForces(false, true);
                //RigidBody2DComponent.AddForce(GetForceVector());
                RigidBody2DComponent.AddForceAtPosition(GetForceVector(), GetForceAnchor());
            }

        }

        bool _firstLateUpdate = true;
        protected virtual void LateUpdate()
        {
            if (_firstLateUpdate)
            {
                _firstLateUpdate = false;
                InitialRecalcCorrector();
                if (AutomaticForceCalculatorOption == AutomaticForceCalculatorCallerEnum.OnFirstFixedUpdate)
                    RecalcForces(true);
                else
                {
                    RecalcSelfMass();
                    RecalcOthersMass();
                }
            }
            if (_currentTimeToLerp > 0)
                _currentTimeToLerp = Mathf.Max(0, _currentTimeToLerp - Time.deltaTime);
        }

        protected virtual void OnDestroy()
        {
            UnregisterInitialEventsReceivers();
            //Important to not create new instance from controller on Destroy, so, if no RopesByBalloonsController founded, dont create a new one
            RemoveSelfFromRopesByBalloonsController(false);
        }

        #endregion

        #region Event Receiver

        protected void HandleOnRopePlugged(Rope2D p_rope)
        {
            _needRecalcForce = true;
        }

        protected void HandleOnRopeUnplugged(Rope2D p_rope)
        {
            _needRecalcAndClear = true;
            RecalcOthersMass();
        }

        protected void HandleOnIndirectRopeCreated(Rope2D p_rope)
        {
            _needRecalcForce = true;
        }

        protected void HandleOnIndirectRopeDestroyed(Rope2D p_rope)
        {
            _needRecalcForce = true;
        }

        protected void HandleOnIndirectRopeBreak(Rope2D p_rope)
        {
            _needRecalcForce = true;
        }

        protected void HandleOnRopeBreak(Rope2D p_rope, int p_brokenIndex)
        {
            _needRecalcForce = true;
        }

        #endregion

        #region Init Functions

        protected virtual void InitialRecalcCorrector()
        {
            //Prevent Bugs when other objects mass change after this late update cycle
            //When not paused
            if (DelayedFunctionUtils.InstanceExists())
            {
                DelayedFunctionUtils.CallFunction(new System.Action(RecalcSelfMass), 0.7f, false);
                DelayedFunctionUtils.CallFunction(new System.Action(RecalcOthersMass), 0.7f, false);
                DelayedFunctionUtils.CallFunction(new System.Action(RecalcRopesMass), 0.7f, false);
            }
            //and after Pause
            Invoke("RecalcSelfMass", 0.05f);
            Invoke("RecalcOthersMass", 0.05f);
        }

        protected virtual void SetFixedAngleActive(bool p_enabled)
        {
            if (RigidBody2DComponent != null)
            {
                RigidBody2DComponent.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
        }

        #endregion

        #region Direct/Indirect Rope Functions

        bool _waitOneCycle = false;
        //Used To Calculate amount of Direct/Indirect Ballons plugged in Ropes(Need this to know correct amount of force needed to void ropes mass
        public virtual void UpdateRopesByBalloonsController()
        {
            _waitOneCycle = true;
            RemoveSelfFromRopesByBalloonsController();
            List<RopesByBallons> v_structsThatContainBallon = RopesByBalloonsController.GetAllStructsWithBallon(this);
            foreach (RopesByBallons v_struct in v_structsThatContainBallon)
            {
                if (v_struct != null && v_struct.Rope != null)
                {
                    if (!v_struct.DirectBallons.RemoveChecking(this))
                        v_struct.IndirectBallons.RemoveChecking(this);
                }
            }
            if (m_includeIndirectRopesInMassCalc)
            {
                //Get Indirect Ropes and Add in Ballons indirect Plugged in this ropes
                foreach (Rope2D v_indirectRope in RopesAttachedComponent.IndirectRopes)
                {
                    if (v_indirectRope != null)
                    {
                        RopesByBallons v_struct = RopesByBalloonsController.GetRopeByBallonStructure(v_indirectRope, true);
                        if (v_struct != null)
                            v_struct.IndirectBallons.AddChecking(this);
                    }
                }
            }
            //Get direct Ropes and Add in Ballons Plugged in this ropes
            foreach (Rope2D v_directRope in RopesAttachedComponent.PluggedRopes)
            {
                if (v_directRope != null)
                {
                    RopesByBallons v_struct = RopesByBalloonsController.GetRopeByBallonStructure(v_directRope, true);
                    if (v_struct != null)
                        v_struct.DirectBallons.AddChecking(this);
                }
            }
        }

        public void RemoveSelfFromRopesByBalloonsController(bool p_canCreateNewInstanceFromController = true)
        {
            //Clear All Nodes To Recalc
            List<RopesByBallons> v_structsThatContainBallon = RopesByBalloonsController.GetAllStructsWithBallon(this, p_canCreateNewInstanceFromController);
            foreach (RopesByBallons v_struct in v_structsThatContainBallon)
            {
                if (v_struct != null && v_struct.Rope != null)
                {
                    List<BalloonProperty> v_newDirectBallonsList = new List<BalloonProperty>(); // Used to Remove Nulls and Self;
                    List<BalloonProperty> v_newIndirectBallonsList = new List<BalloonProperty>(); // Used to Remove Nulls and Self;
                    foreach (BalloonProperty v_ballon in v_struct.DirectBallons)
                    {
                        if (v_ballon != null && v_ballon != this)
                        {
                            v_newDirectBallonsList.Add(v_ballon);
                            v_ballon.MarkToRecalcForce(false);
                        }
                    }
                    foreach (BalloonProperty v_ballon in v_struct.IndirectBallons)
                    {
                        if (v_ballon != null && v_ballon != this)
                        {
                            v_newIndirectBallonsList.Add(v_ballon);
                            v_ballon.MarkToRecalcForce(false);
                        }
                    }
                    v_struct.DirectBallons = v_newDirectBallonsList;
                    v_struct.IndirectBallons = v_newIndirectBallonsList;
                }
            }
        }

        #endregion

        #region Helper Functions

        protected virtual Vector3 GetForceAnchor()
        {
            Vector3 v_anchor = this.transform.position;
            if (GetComponent<Renderer>() != null)
            {
                v_anchor = new Vector3(v_anchor.x, v_anchor.y + GetComponent<Renderer>().bounds.size.y / 2.0f, v_anchor.z);
            }
            return v_anchor;
        }

        public void MarkToRecalcForce(bool p_force = true)
        {
            //if(p_force)
            _needRecalcForce = true;
            //else
            //	GlobalScheduler.CallFunction(this.gameObject, new System.Action(MarkToRecalcForce), 0.4f, false);
        }

        public void MarkToRecalcAndClearForce(bool p_force = true)
        {
            //if(p_force)
            _needRecalcAndClear = true;
            //else
            //	GlobalScheduler.CallFunction(this.gameObject, new System.Action(MarkToRecalcForce), 0.4f, false);
        }

        protected virtual void UnregisterInitialEventsReceivers()
        {
            RopesAttachedComponent.OnRopePlugged -= HandleOnRopePlugged;
            RopesAttachedComponent.OnRopeUnplugged -= HandleOnRopeUnplugged;
            RopesAttachedComponent.OnIndirectRopeCreated -= HandleOnIndirectRopeCreated;
            RopesAttachedComponent.OnIndirectRopeDestroyed -= HandleOnIndirectRopeDestroyed;
            RopesAttachedComponent.OnIndirectRopeBreak -= HandleOnIndirectRopeBreak;
            RopesAttachedComponent.OnRopeBreak -= HandleOnRopeBreak;
        }

        protected virtual void RegisterInitialEventsReceivers()
        {
            UnregisterInitialEventsReceivers();
            RopesAttachedComponent.OnRopePlugged += HandleOnRopePlugged;
            RopesAttachedComponent.OnRopeUnplugged += HandleOnRopeUnplugged;
            RopesAttachedComponent.OnIndirectRopeCreated += HandleOnIndirectRopeCreated;
            RopesAttachedComponent.OnIndirectRopeDestroyed += HandleOnIndirectRopeDestroyed;
            RopesAttachedComponent.OnIndirectRopeBreak += HandleOnIndirectRopeBreak;
            RopesAttachedComponent.OnRopeBreak += HandleOnRopeBreak;
        }

        public virtual void RecalcSelfMass()
        {
            if (AutomaticForceCalculatorOption != AutomaticForceCalculatorCallerEnum.Never)
                _selfMass = RopesAttachedComponent.GetSelfMass();
        }

        public virtual void RecalcOthersMass()
        {
            if (AutomaticForceCalculatorOption != AutomaticForceCalculatorCallerEnum.Never)
            {
                _sumOfOtherObjectsMass = RopesAttachedComponent.GetSumOfOtherObjectsMass();
                //If Not Attached to Ropes Add force equal own Mass to not stay in Air
                if (_sumOfOtherObjectsMass == 0)
                    _sumOfOtherObjectsMass = RopesAttachedComponent.GetSelfMass();
            }
        }

        public virtual void RecalcRopesMass()
        {
            float v_indirectSum = 0;
            float v_directSum = 0;
            List<RopesByBallons> v_structsThatContainBallon = RopesByBalloonsController.GetAllStructsWithBallon(this);
            foreach (RopesByBallons v_struct in v_structsThatContainBallon)
            {
                if (v_struct != null && v_struct.Rope != null)
                {
                    if (v_struct.DirectBallons.Contains(this))
                    {
                        float v_sumOfDirectIntensitys = 0;
                        foreach (BalloonProperty v_ballon in v_struct.DirectBallons)
                        {
                            if (v_ballon != null)
                                v_sumOfDirectIntensitys += v_ballon.GetIndependentForceIntensity();
                        }
                        float v_percent = v_sumOfDirectIntensitys == 0 ? 1 / v_struct.DirectBallons.Count : GetIndependentForceIntensity() / v_sumOfDirectIntensitys;
                        v_directSum += v_struct.Rope.GetMassAttachedToObject(this.gameObject) * v_percent; // Mass Of The Rope will be divided to all Direct Ballon
                    }
                    else if (v_struct.DirectBallons.Count <= 0 && v_struct.IndirectBallons.Contains(this)) // Only if no direct ballons plugged in this rope
                    {
                        float v_sumOfIndirectIntensitys = 0;
                        foreach (BalloonProperty v_ballon in v_struct.IndirectBallons)
                        {
                            if (v_ballon != null)
                                v_sumOfIndirectIntensitys += v_ballon.GetIndependentForceIntensity();
                        }
                        float v_percent = v_sumOfIndirectIntensitys == 0 ? 1 / v_struct.IndirectBallons.Count : GetIndependentForceIntensity() / v_sumOfIndirectIntensitys;
                        v_indirectSum += v_struct.Rope.GetRopeMass(true) * v_percent;// Mass Of The Rope will be divided to all Indirect Ballon
                    }
                }
            }
            _sumOfPluggedRopesMass = v_directSum;
            if (m_includeIndirectRopesInMassCalc)
                _sumOfIndirectRopesMass = v_indirectSum;
            else
                _sumOfIndirectRopesMass = 0;
        }

        public virtual void RecalcForces(bool p_forceRecalc = false, bool p_ropesMassOnly = false)
        {
            if (_needRecalcForce || p_forceRecalc)
            {
                if (!p_forceRecalc)
                    ResetTimeToLerp();
                _needRecalcForce = false;
                UpdateRopesByBalloonsController();
                //RecalcRopesMass();

                if (!p_ropesMassOnly)
                {
                    RecalcSelfMass();
                    RecalcOthersMass();
                }
            }
        }

        public virtual void RecalcForcesAndClearBody(bool p_forceRecalc = false, bool p_ropesMassOnly = false)
        {
            if (_needRecalcAndClear || p_forceRecalc)
            {
                RecalcForces(true, p_ropesMassOnly);
                _needRecalcAndClear = false;
                RopeInternalUtils.TryClearRigidBody2D(RigidBody2DComponent);
                ResetTimeToLerp();
            }
        }

        public void CloneModelForceBallonComponents()
        {
            if (Application.isPlaying && !RopeInternalUtils.IsPrefab(this.gameObject))
            {
                BallonObjectToCloneByMass v_structToCheck = null;
                //Find Correct Struct
                foreach (BallonObjectToCloneByMass v_struct in SpriteChangerModel)
                {
                    if (v_struct != null && v_struct.ObjectContainer != null && (_sumOfOtherObjectsMass * BallonForceScale) + ExtraThrust >= v_struct.MassToAcceptClone)
                    {
                        if (v_structToCheck == null)
                        {
                            v_structToCheck = v_struct;
                        }
                        else
                        {
                            if (v_struct.MassToAcceptClone > v_structToCheck.MassToAcceptClone)
                                v_structToCheck = v_struct;
                        }
                    }
                }

                //Copy Components
                if (v_structToCheck != null && v_structToCheck.ObjectContainer != null)
                {
                    MonoBehaviour[] v_componentsArray = v_structToCheck.ObjectContainer.GetComponents<MonoBehaviour>();
                    foreach (MonoBehaviour v_component in v_componentsArray)
                    {
                        if (v_component != null)
                            RopeInternalUtils.CopyComponent<MonoBehaviour>(this.gameObject, v_component);
                    }
                    //Copy Sprite Renderer
                    SpriteRenderer v_otherSpriteRenderer = v_structToCheck.ObjectContainer.GetComponent<SpriteRenderer>();
                    SpriteRenderer v_myRenderer = this.gameObject.GetComponent<SpriteRenderer>();
                    if (v_myRenderer != null && v_otherSpriteRenderer != null)
                    {
                        v_myRenderer.sprite = v_otherSpriteRenderer.sprite;
                        //v_myRenderer.material = v_otherSpriteRenderer.material;
                    }
                }
            }
        }

        //ExtraThrust + (_sumOfOtherObjectsMass*BallonForceScale) only
        public virtual float GetIndependentForceIntensity()
        {
            float v_forceIntensity = (ExtraThrust) + ((_sumOfOtherObjectsMass * BallonForceScale));
            return v_forceIntensity;
        }

        //Ballon Force Scale only affect the other side of the rope
        public virtual Vector2 GetForceVector()
        {
            //Just Recalc but dont cancel WaitOneCycle, we must wait
            if (_waitOneCycle)
                RecalcRopesMass();
            LerpValues(GetLerpIntensity());
            float v_forceIntensity = Mathf.Abs((ExtraThrust + (RopeForceScale * _sumOfPluggedRopesMassLerp) + (IndirectRopeForceScale * _sumOfIndirectRopesMassLerp) + _selfMassLerp) + ((_sumOfOtherObjectsMassLerp * BallonForceScale)));
            Vector2 v_forceVector = new Vector2(v_forceIntensity * -Physics2D.gravity.x, v_forceIntensity * -Physics2D.gravity.y);
            return v_forceVector;
        }

        #endregion

        #region Lerp Functions

        float _selfMassLerpInitial = 0;
        float _sumOfPluggedRopesMassLerpInitial = 0;
        float _sumOfIndirectRopesMassLerpInitial = 0;
        float _sumOfOtherObjectsMassLerpInitial = 0;
        public void ResetTimeToLerp()
        {
            _selfMassLerpInitial = _selfMassLerp;
            _sumOfPluggedRopesMassLerpInitial = _sumOfPluggedRopesMassLerp;
            _sumOfIndirectRopesMassLerpInitial = _sumOfIndirectRopesMassLerp;
            _sumOfOtherObjectsMassLerpInitial = _sumOfOtherObjectsMassLerp;
            if (_maxTimeToLerp > 0)
                _currentTimeToLerp = _maxTimeToLerp;
        }

        float _maxTimeToLerp = 2.0f;
        float _currentTimeToLerp = 0;
        public float GetLerpIntensity()
        {
            if (_maxTimeToLerp > 0)
            {
                return (_maxTimeToLerp - _currentTimeToLerp) / _maxTimeToLerp;
            }
            return 1;
        }

        protected void LerpValues(float p_intensity)
        {
            p_intensity = Mathf.Clamp(p_intensity, 0, 1);
            if (_selfMass > _selfMassLerp)
                _selfMassLerp = Mathf.Lerp(_selfMassLerpInitial, _selfMass, p_intensity);
            else
                _selfMassLerp = _selfMass;

            if (_sumOfPluggedRopesMass > _sumOfPluggedRopesMassLerp)
                _sumOfPluggedRopesMassLerp = Mathf.Lerp(_sumOfPluggedRopesMassLerpInitial, _sumOfPluggedRopesMass, p_intensity);
            else
                _sumOfPluggedRopesMassLerp = _sumOfPluggedRopesMass;

            if (_sumOfIndirectRopesMass > _sumOfIndirectRopesMassLerp)
                _sumOfIndirectRopesMassLerp = Mathf.Lerp(_sumOfIndirectRopesMassLerpInitial, _sumOfIndirectRopesMass, p_intensity);
            else
                _sumOfIndirectRopesMassLerp = _sumOfIndirectRopesMass;
            if (_sumOfOtherObjectsMass > _sumOfOtherObjectsMassLerp)
                _sumOfOtherObjectsMassLerp = Mathf.Lerp(_sumOfOtherObjectsMassLerpInitial, _sumOfOtherObjectsMass, p_intensity);
            else
                _sumOfOtherObjectsMassLerp = _sumOfOtherObjectsMass;
        }

        #endregion
    }

    #region Helper Structures

    [System.Serializable]
    public class BallonObjectToCloneByMass
    {
        #region Private Variables

        [SerializeField]
        GameObject m_objectContainer = null;
        [SerializeField]
        float m_massToAcceptClone = 1f;

        #endregion

        #region Public Properties

        public GameObject ObjectContainer { get { return m_objectContainer; } set { m_objectContainer = value; } }
        public float MassToAcceptClone { get { return m_massToAcceptClone; } set { m_massToAcceptClone = value; } }

        #endregion

    }

    #endregion

}
