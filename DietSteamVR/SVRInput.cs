using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CVRInput=Valve.VR.CVRInput;
using OpenVR=Valve.VR.OpenVR;
using InputDigitalActionData_t=Valve.VR.InputDigitalActionData_t;
using InputAnalogActionData_t = Valve.VR.InputAnalogActionData_t;
using VRActiveActionSet_t=Valve.VR.VRActiveActionSet_t;
namespace SteamVRInput{

    public class SVRInput : MonoBehaviour{

        public const string PathTemplate= "/actions/{0}/in/{1}";
        public const string ManifestPath=@"StreamingAssets\DietSteamVR\actions.json";
        /*
            Static methods
        */
        private static SVRInput _instance;
        public static bool CheckInstance(){
            if(_instance==null){
                var temp = new GameObject("SVRInput").AddComponent<SVRInput>();
                temp.Init();
            }
            return true;
        }




        public static bool Get(Button b,Device device){ CheckInstance();return _instance._Get(b,device).bState;}
        public static bool GetUp(Button b,Device device){
                CheckInstance();
                var data= _instance._Get(b,device);
                return (!data.bState)&&data.bChanged;
             }
        public static bool GetDown(Button b,Device device){ 
            CheckInstance();
            var data= _instance._Get(b,device);
            return (data.bState)&&data.bChanged;
        }

        public static float Get(Axis1 axis,Device device){
            CheckInstance();
            var data=_instance._GetAxis(_instance.axis1Handles[(int)axis],device);
            return data.x;
        }

        public static Vector2 Get(Axis2 axis,Device device){
            CheckInstance();
            var data=_instance._GetAxis(_instance.axis2Handles[(int)axis],device);
            return new Vector2(data.x,data.y);
        }

        public static Vector3 Get(Axis3 axis,Device device){
            CheckInstance();
            var data=_instance._GetAxis(_instance.axis3Handles[(int)axis],device);
            return new Vector3(data.x,data.y,data.z);
        }

        /* 
        Instance Methods
        */
        CVRInput CVRInput;
        bool initd=false;
        float lastFrameCount=-1;
        ulong[] buttonHandles,axis1Handles,axis2Handles,axis3Handles,deviceHandles;
        uint sizeOfActionSet,sizeOfActionData,sizeOfAnalogData;
        Valve.VR.VRActiveActionSet_t[] actionSets;
        void Awake(){
            Init();
        }
        
        
        void Init(){
            if(initd) return;
            if(_instance==null){
                _instance=this;
                DontDestroyOnLoad(this);
                //because i'm paranoid
                DontDestroyOnLoad(this.gameObject);

                CVRInput=OpenVR.Input;
                SVRHelper.InitializeTemporarySession();
                var fi = new System.IO.FileInfo(System.IO.Path.Combine(Application.dataPath,ManifestPath));
                CVRInput.SetActionManifestPath(fi.FullName).Log("SetActionManifestPath:");
                SetupHandles();
                initd=true;   
            }else{
                DestroyImmediate(this);
            }
        }

        void OnDestroy(){
            if(_instance==this){
                _instance=null;
                SVRHelper.ExitTemporarySession();
            }
        }

        void Update(){
            if(!initd) return;
            if(Time.frameCount<=lastFrameCount) return;
            lastFrameCount=Time.frameCount;
            CVRInput.UpdateActionState(actionSets,sizeOfActionSet).Log("UpdateActionState::");
        }

        public InputDigitalActionData_t _Get(Button b,Device device){
            if(!initd) return new InputDigitalActionData_t();
            //consider caching this if you have GC concerns
            var actionData=new InputDigitalActionData_t();
            CVRInput.GetDigitalActionData(buttonHandles[(int)device],ref actionData,sizeOfActionData,deviceHandles[(int)device]).Log("GetDigitalActionData::"+b.ToString());
            return actionData;
        }

        public InputAnalogActionData_t _GetAxis(ulong axis, Device device){
            if(!initd) return new InputAnalogActionData_t();
            var actionData= new InputAnalogActionData_t();
            CVRInput.GetAnalogActionData(axis,ref actionData,sizeOfAnalogData,deviceHandles[(int)device]).Log("GetAnalogActionData::");
            return actionData;
        }



        void SetupHandles(){
            List<string> actionSetNames= new List<string>();
            /*
                Get All the Action Handles.
                Keep track of what actionSets we will need
            */
            
            var buttons=(Button[])System.Enum.GetValues(typeof(SteamVRInput.Button));
            buttonHandles= new ulong[buttons.Length];
            foreach(Button b in buttons){
                var actionSet=b.GetActionSet();
                if(!actionSetNames.Contains(actionSet)) actionSetNames.Add(actionSet);
                CVRInput.GetActionHandle(string.Format(PathTemplate,actionSet,b.ToString().ToLower()),ref buttonHandles[(int)b]).Log("GetActionHandle::"+b.ToString());
            }

            var axes=(Axis1[])System.Enum.GetValues(typeof(SteamVRInput.Axis1));
            axis1Handles= new ulong[axes.Length];
            foreach(Axis1 a in axes){
                var actionSet=a.GetActionSet();
                if(!actionSetNames.Contains(actionSet)) actionSetNames.Add(actionSet);
                CVRInput.GetActionHandle(string.Format(PathTemplate,actionSet,a.ToString().ToLower()),ref axis1Handles[(int)a]).Log("GetActionHandle::"+a.ToString());
            }

            var axes2=(Axis2[])System.Enum.GetValues(typeof(SteamVRInput.Axis2));
            axis2Handles= new ulong[axes2.Length];
            foreach(Axis2 a in axes){
                var actionSet=a.GetActionSet();
                if(!actionSetNames.Contains(actionSet)) actionSetNames.Add(actionSet);
                CVRInput.GetActionHandle(string.Format(PathTemplate,actionSet,a.ToString().ToLower()),ref axis2Handles[(int)a]).Log("GetActionHandle::"+a.ToString());
            }

            var axes3=(Axis3[])System.Enum.GetValues(typeof(SteamVRInput.Axis3));
            axis3Handles= new ulong[axes3.Length];
            foreach(Axis3 a in axes){
                var actionSet=a.GetActionSet();
                if(!actionSetNames.Contains(actionSet)) actionSetNames.Add(actionSet);
                CVRInput.GetActionHandle(string.Format(PathTemplate,actionSet,a.ToString().ToLower()),ref axis3Handles[(int)a]).Log("GetActionHandle::"+a.ToString());
            }

            /*
                Get Device Handles
            */

            var devices = (Device[])System.Enum.GetValues(typeof(SteamVRInput.Device));
            deviceHandles=new ulong[devices.Length];
            foreach(Device d in deviceHandles){
                CVRInput.GetInputSourceHandle(d.GetPath(),ref deviceHandles[(int)d]).Log("GetInputSourceHandle::"+d.ToString());
                //this is apparently broken according to the SteamVR Input Unity Integration
                //so we get the handle and zero it out.
                if(d==Device.Any)
                    deviceHandles[(int)d]=0;
            }

            /*
                Get ActionSetHandles
                Assume we want left,right,any for every action set.
            */
            this.actionSets= new Valve.VR.VRActiveActionSet_t[actionSetNames.Count*3];
            int index=0;
            foreach(string s in actionSetNames){
                
                actionSets[index].ulRestrictedToDevice=deviceHandles[(int)Device.Any];
                actionSets[index].nPriority=0;
                CVRInput.GetActionSetHandle(string.Format("/actions/{0}",s),ref actionSets[index].ulActionSet).Log("GetActionSetHandle:");
                index++;

                actionSets[index].ulRestrictedToDevice=deviceHandles[(int)Device.LeftHand];
                actionSets[index].nPriority=0;
                CVRInput.GetActionSetHandle(string.Format("/actions/{0}",s),ref actionSets[index].ulActionSet).Log("GetActionSetHandle:");
                index++;

                actionSets[index].ulRestrictedToDevice=deviceHandles[(int)Device.RightHand];
                actionSets[index].nPriority=0;
                CVRInput.GetActionSetHandle(string.Format("/actions/{0}",s),ref actionSets[index].ulActionSet).Log("GetActionSetHandle:");
                index++;
            }


            /* 
            Get Marshalling sizes
            */

            sizeOfActionSet=(uint)(Marshal.SizeOf(typeof(VRActiveActionSet_t)));
            sizeOfActionData=(uint)Marshal.SizeOf<InputDigitalActionData_t>();
            sizeOfAnalogData=(uint)Marshal.SizeOf<InputAnalogActionData_t>();
        }
        
    }
}