using UnityEngine;
using UnityEditor;
using SteamVRInput;
using System.Collections.Generic;
public class SVRInputManifestMaker : ScriptableWizard{
    [MenuItem("Tools/Create SteamVR Actions Manifest")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard<SVRInputManifestMaker>("Create SteamVR Actions Manifest","Create");
    }

    void OnWizardCreate(){
        Manifest m = new Manifest();
        List<InputAction> actions = new List<InputAction>();
        List<string> actionSets = new List<string>();
        var action_sets= new List<ActionSet>();

        var buttons=(Button[])System.Enum.GetValues(typeof(Button));
        foreach(Button b in buttons){
            var a = new InputAction();
            var actset=b.GetActionSet();
            if(!actionSets.Contains(actset)) actionSets.Add(actset);
            a.name=string.Format(SVRInput.PathTemplate,actset,b.ToString());
            a.type="boolean";
            a.requirement="mandatory";
            actions.Add(a);
        }

        var axes1=(Axis1[])System.Enum.GetValues(typeof(Axis1));
        foreach(Axis1 b in axes1){
            var a = new InputAction();
            var actset=b.GetActionSet();
            if(!actionSets.Contains(actset)) actionSets.Add(actset);
            a.name=string.Format(SVRInput.PathTemplate,actset,b.ToString());
            a.type="vector1";
            a.requirement="mandatory";
            actions.Add(a);
        }

        var axes2=(Axis2[])System.Enum.GetValues(typeof(Axis2));
        foreach(Axis2 b in axes2){
            var a = new InputAction();
            var actset=b.GetActionSet();
            if(!actionSets.Contains(actset)) actionSets.Add(actset);
            a.name=string.Format(SVRInput.PathTemplate,actset,b.ToString());
            a.type="vector2";
            a.requirement="mandatory";
            actions.Add(a);
        }

        var axes3=(Axis3[])System.Enum.GetValues(typeof(Axis3));
        foreach(Axis3 b in axes3){
            var a = new InputAction();
            var actset=b.GetActionSet();
            if(!actionSets.Contains(actset)) actionSets.Add(actset);
            a.name=string.Format(SVRInput.PathTemplate,actset,b.ToString());
            a.type="vector3";
            a.requirement="mandatory";
            actions.Add(a);
        }

        m.actions=actions.ToArray();
        foreach(string s in actionSets){
            var rawas= new ActionSet();
            rawas.name="/actions/"+s;
            rawas.usage="leftright";
            action_sets.Add(rawas);
        }
        m.action_sets=action_sets.ToArray();

        var fi = new System.IO.FileInfo(System.IO.Path.Combine(Application.dataPath,SVRInput.ManifestPath));
        System.IO.File.WriteAllText(fi.FullName,JsonUtility.ToJson(m,true));
    }

    [System.Serializable]
    private struct Manifest{
       public InputAction[] actions;
        public ActionSet[] action_sets;
    }
    [System.Serializable]
    private struct InputAction{
        public string name;
        public string type;
        public string requirement;
    }
    [System.Serializable]
    private struct ActionSet{
        public string name;
        public string usage;
    }
}