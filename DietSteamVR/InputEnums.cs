using System.ComponentModel;
namespace SteamVRInput
{
    public enum Button{
        [Description("main")]
        Menu=0,
        [Description("ui")]
        MenuClick=1,
        [Description("main")]
        Teleport=3,
        [Description("main")]
        Kickflip=4

    }
    public enum Axis1{
        [Description("main")]
        Grip=0,
    }
    public enum Axis2{
        [Description("ui")]
        MenuNav=0
    }
    public enum Axis3{
        //I don't even know
    }

    //device paths are pulled from openvr_api.cs
    public enum Device{
        Any=0,
        LeftHand=1,
        RightHand=2,
        Head=3
    }
    
    public static class InputEnumsExt{
        public static string GetActionSet(this Button b)=>_GetActionSet(b);
        public static string GetActionSet(this Axis1 a)=>_GetActionSet(a);
        public static string GetActionSet(this Axis2 a)=>_GetActionSet(a);
        public static string GetActionSet(this Axis3 a)=>_GetActionSet(a);
        private static string _GetActionSet(System.Enum i)
        {

            var actionSet= ((System.ComponentModel.DescriptionAttribute)i.GetType().GetMember(i.ToString())[0].GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false)[0]).Description;
            //return string.Format("/actions/{0}/in/{1}",actionSet,i.ToString().ToLower());
            return actionSet;
        }

        //pulled from openvr_api.cs
        public static string GetPath(this Device device){
            switch(device){
                case Device.Any: return "/unrestricted";
                case Device.LeftHand: return "/user/hand/left";
                case Device.RightHand: return "/user/hand/right";
                case Device.Head: return "/user/head";
            }
            return "/unrestricted";
        }

        public static bool Log(this Valve.VR.EVRInputError error,string prefix=""){
            if(error!=0){
                UnityEngine.Debug.LogErrorFormat("{0}{1}",prefix,error.ToString());
                return false;
            }
            return true;
        }
    }
}