using UnityEngine;
using System.Reflection;
//using Colorful;
using System;

namespace VisSim
{


    [HelpURL("http://http://www.ucl.ac.uk/~smgxprj")]
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    abstract public class LinkableBaseEffect : BaseEffect
    {
        // required Methods
        protected abstract void OnUpdate();
        protected abstract override void OnRenderImage(RenderTexture source, RenderTexture destination);

        // To link effect parameter-values across eyes (left/right)
        public bool LinkEyes = true;

        // Handles
        protected bool isLeftEye;
        private LinkableBaseEffect rightEyeEffectInstance;
        private LinkableBaseEffect leftEyeEffectInstance;
        

        /*
		public enum EyeType
		{
			LeftEye = 0,
			RightEye = 1,
			Neither = 2,
		}
		[TweakableMember(0,1, "mastereye", "myBrightnessContrastGamma")]
		public EyeType MasterEye = EyeType.LeftEye;
		*/


        public void OnEnable()
        {
            // ensure material is initialised
            Material.GetType();

            // check if this is the left eye effect
            isLeftEye = this.gameObject.tag == "LeftEye";

            // Check effect is present left eye
            GameObject leftEye = GameObject.FindWithTag("LeftEye");
            Component[] leftEyeEffectInstances = leftEye.GetComponentsInChildren(this.GetType());
            if (leftEyeEffectInstances.Length != 1)
            {
                Debug.LogError(this.GetType() + " disabled: 1, and only 1 instance of expected effect required on LEFT EYE.");
                this.enabled = false;
                return;
            }

            // Check effect is present on right eye
            GameObject rightEye = GameObject.FindWithTag("RightEye");
            Component[] rightEyeEffectInstances = rightEye.GetComponentsInChildren(this.GetType());
            if (rightEyeEffectInstances.Length != 1)
            {
                Debug.LogError(this.GetType() + " disabled: 1, and only 1 instance of expected effect required on RIGHT EYE.");
                this.enabled = false;
                return;
            }

            // store references
            leftEyeEffectInstance = leftEyeEffectInstances[0] as LinkableBaseEffect;
            rightEyeEffectInstance = rightEyeEffectInstances[0] as LinkableBaseEffect;

            // also enable right eye, if the two eyes are locked
            if (isLeftEye && this.LinkEyes)
            {
                rightEyeEffectInstance.enabled = true;
            }
        }

        protected override void OnDisable()
        {
            // also disable right eye, if the two eyes are locked
            if (isLeftEye && this.LinkEyes && (rightEyeEffectInstance!=null)) // (i.e., may be null if failed to enable in the first place)
            {
                rightEyeEffectInstance.enabled = false;
            }

            // call BaseEffect method
            base.OnDisable();
        }

        public void Update()
        {
            // enable if not done so already (e.g., if user forgot to include base.onEnable() in subclass!)
            if (leftEyeEffectInstance == null || rightEyeEffectInstance == null) // ||(isLeftEye && leftEyeEffectInstance.enabled == false)
            {
                this.OnEnable();
                if (!this.enabled) { return; }
            }

            //Debug.Log (this.gameObject.tag);
            if (isLeftEye)
            {
                // Sync lock value across eyes
                rightEyeEffectInstance.GetType().GetField("LinkEyes").SetValue(rightEyeEffectInstance, this.LinkEyes);

                // If LinkEyes, then set all LinkableAttribute fields to have the value of the left eye
                if (this.LinkEyes)
                {
                    rightEyeEffectInstance.enabled = leftEyeEffectInstance.enabled;

                    foreach (FieldInfo fi in this.GetType().GetFields())
                    {
                        if (fi.IsDefined(typeof(LinkableAttribute), false))
                        {
                            rightEyeEffectInstance.GetType().GetField(fi.Name).SetValue(rightEyeEffectInstance, fi.GetValue(this));
                        }
                    }
                }
            }
            else
            {
                this.LinkEyes = (bool)leftEyeEffectInstance.GetType().GetField("LinkEyes").GetValue(leftEyeEffectInstance);
                if (this.LinkEyes)
                {
                    rightEyeEffectInstance.enabled = leftEyeEffectInstance.enabled;
                }
            }
            
            // Call OnUpdate
            OnUpdate();
        }

        protected override string GetShaderName()
        {
            return "Hidden/VisSim/LinkableBaseEffect (this should be overriden)";
        }
    }
}


/// Attribute that can be used to mark fields or properties on MonoBehaivours as Linkable
public class LinkableAttribute : Attribute
{
    //String name;
    public LinkableAttribute()
    {
        //this.name = "default";
    }
    /*
    public LinkableAttribute(String name)
    {
        this.name = name;
    }
    */
}