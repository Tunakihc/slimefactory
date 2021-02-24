using System.Collections.Generic;
using Helpers;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ChildInfo
{
    public string ObjectName;
    public Vector3 LocalPosition;
    public Vector3 LocalRotation;
    public Vector3 LocalScale;
    public string Settings;
}

[System.Serializable]
public class PatternObjectInfo
{
    public List<ChildInfo> Childs;
    public Vector3 EndPoint;
    public Vector3 Dir;
}

public class ObjectSerializer
{
#if UNITY_EDITOR
    const string ObjectsConfigPath = "Assets/_Configs/ObjectsConfig.asset";

    public static string SerializeObject(Transform targetObject)
    {
        var patternSettings = targetObject.GetComponent<PatternSettings>();

        var childCount = targetObject.childCount;
        var childsInfo = new List<ChildInfo>();
        var objectBounds = targetObject.gameObject.GetMaxBounds();
        // var center = patternSettings.FixCenter ? patternSettings.Center : objectBounds.center;

        PoolObject child = null;
        // ObjectInfo childInfo = null;

        patternSettings._StartPoint.LookAt(patternSettings._EntPoint);
        
        for (var i = 0; i < childCount; i++)
        {
            child = targetObject.GetChild(i).GetComponent<PoolObject>();
            
            if(child == null) continue;
            
            var prefabGameObject = PrefabUtility.GetCorrespondingObjectFromSource(child);
            var childInfo = prefabGameObject.name;

            if (child == null)
                continue;

            var info = new ChildInfo()
            {
                ObjectName = childInfo,
                LocalPosition = patternSettings._StartPoint.InverseTransformPoint(child.transform.position),
                LocalRotation = patternSettings._StartPoint.localEulerAngles + child.transform.eulerAngles,
                LocalScale = child.transform.localScale,
                Settings = child.SerializeSettings()
            };

            childsInfo.Add(info);
        }
        
       

        var objectInfo = new PatternObjectInfo()
        {
            Childs = childsInfo,
            EndPoint = patternSettings._StartPoint.InverseTransformPoint(patternSettings._EntPoint.position),
            Dir = patternSettings._StartPoint.forward
        };

        var serealizedInfo = XMLHelper.Serialize<PatternObjectInfo>(objectInfo);

        return serealizedInfo;
    }
#endif
    public static PatternObjectInfo DeserializeObject(TextAsset info)
    {
        return XMLHelper.Deserialize<PatternObjectInfo>(info.text);
    }
       
}


