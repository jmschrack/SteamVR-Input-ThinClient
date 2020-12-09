using Valve.VR;
using System.IO;
using UnityEngine;
namespace SteamVRInput
{
    public class SVRHelper{
        private static void IdentifyEditorApplication(bool showLogs = true)
        {
        
            var manifestFI= new FileInfo("unityProject.vrmanifest");
            string manifestPath = manifestFI.FullName;

            EVRApplicationError addManifestErr = OpenVR.Applications.AddApplicationManifest(manifestPath, true);
            if (addManifestErr != EVRApplicationError.None)
                Debug.LogError("<b>[SteamVR]</b> Error adding vr manifest file: " + addManifestErr.ToString());
            else
            {
                if (showLogs)
                    Debug.Log("<b>[SteamVR]</b> Successfully added VR manifest to SteamVR");
            }

            int processId = System.Diagnostics.Process.GetCurrentProcess().Id;
            var appkey=string.Format("application.generated.unity.{0}.exe",GenerateCleanProductName());
            EVRApplicationError applicationIdentifyErr = OpenVR.Applications.IdentifyApplication((uint)processId, appkey);

            if (applicationIdentifyErr != EVRApplicationError.None)
                Debug.LogError("<b>[SteamVR]</b> Error identifying application: " + applicationIdentifyErr.ToString());
            else
            {
                if (showLogs)
                    Debug.Log(string.Format("<b>[SteamVR]</b> Successfully identified process as editor project to SteamVR ({0})", appkey));
            }
        }

        public static string GenerateCleanProductName()
        {
            string productName = Application.productName;
            if (string.IsNullOrEmpty(productName))
                productName = "unnamed_product";
            else
            {
                productName = System.Text.RegularExpressions.Regex.Replace(Application.productName, "[^\\w\\._]", "");
                productName = productName.ToLower();
            }

            return productName;
        }

        private static bool runningTemporarySession = false;
        public static bool InitializeTemporarySession(bool initInput = false)
        {
            if (Application.isEditor)
            {
                //bool needsInit = (!active && !usingNativeSupport && !runningTemporarySession);

                EVRInitError initError = EVRInitError.None;
                OpenVR.GetGenericInterface(OpenVR.IVRCompositor_Version, ref initError);
                bool needsInit = initError != EVRInitError.None;

                if (needsInit)
                {
                    EVRInitError error = EVRInitError.None;
                    OpenVR.Init(ref error, EVRApplicationType.VRApplication_Overlay);

                    if (error != EVRInitError.None)
                    {
                        Debug.LogError("<b>[SteamVR]</b> Error during OpenVR Init: " + error.ToString());
                        return false;
                    }

                    IdentifyEditorApplication(false);

                    //SteamVR_Input.IdentifyActionsFile(false);

                    runningTemporarySession = true;
                }

            

                return needsInit;
            }

            return false;
        }

        public static void ExitTemporarySession()
        {
            if (runningTemporarySession)
            {
                OpenVR.Shutdown();
                runningTemporarySession = false;
            }
        }
    }
}