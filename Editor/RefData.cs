using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class RefData
{
    public class UnityFileData
    {
        public string m_kPath;
        public HashSet<UnityFileData> m_kDepends = new HashSet<UnityFileData>();
        public HashSet<UnityFileData> m_kRefeds = new HashSet<UnityFileData>();

        public void Clear()
        {
            m_kDepends.Clear();
            m_kRefeds.Clear();
        }
    }

    public static HashSet<UnityFileData> m_kSelectedUnityFileData = new HashSet<UnityFileData>();
    public static Dictionary<string,UnityFileData> m_kAllUnityFileDataDic = new Dictionary<string,UnityFileData>();
    public static HashSet<UnityFileData> m_kRetDependsUnityFileData = new HashSet<UnityFileData>();


    //[MenuItem("Assets/FindFolderRefs")]
    //public static void FindFolderRefs()
    //{
    //    List<string> selectFolderPaths = new List<string>();
    //    var curFolderGuids = Selection.assetGUIDs;
    //    foreach(var folder in curFolderGuids)
    //    {
    //        string path = AssetDatabase.GUIDToAssetPath(folder);
    //        if(AssetDatabase.IsValidFolder(path))
    //        {
    //            selectFolderPaths.Add(path);
    //        }
    //    }

    //    FindFolderRefs(selectFolderPaths.ToArray());

    //}
    public static void FindFolderRefs(string[] folderPaths)
    {
        m_kAllUnityFileDataDic.Clear();
        m_kRetDependsUnityFileData.Clear();
        m_kSelectedUnityFileData.Clear();

        string[] allFolderFileGuids =  AssetDatabase.FindAssets("", folderPaths);
        HashSet<string> allContainsFileGuids = new HashSet<string>(1024);
        foreach(var folderGuid in allFolderFileGuids)
        {
            allContainsFileGuids.Add(folderGuid);
        }

        HashSet<string> allContainsFilePaths = new HashSet<string>(1024);

        foreach (var guid in allContainsFileGuids)
        {
            allContainsFilePaths.Add(AssetDatabase.GUIDToAssetPath(guid));
        }
        
        //下面这个foreach 会包含所有相关的对象
        foreach (var refFilePath in AssetDatabase.GetDependencies(allContainsFilePaths.ToArray(),true))
        {
            UnityFileData fileData = new UnityFileData() { m_kPath = refFilePath };
            if(allContainsFilePaths.Contains(refFilePath))
            {
                m_kSelectedUnityFileData.Add(fileData);
            }
            else
            {
                m_kRetDependsUnityFileData.Add(fileData);
            }

            if(!m_kAllUnityFileDataDic.ContainsKey(refFilePath))
            {
                m_kAllUnityFileDataDic.Add(refFilePath, fileData);
            }
            else
            {
                m_kAllUnityFileDataDic[refFilePath].Clear();
            }
        }
        //接下来只需要梳理关系即可
        foreach (var fileKv in m_kAllUnityFileDataDic)
        {
            var refFiles = AssetDatabase.GetDependencies(fileKv.Key, true);
            UnityFileData curFileData = m_kAllUnityFileDataDic[fileKv.Key];
            foreach (var refFile in refFiles)
            {
                var refFileData =  m_kAllUnityFileDataDic[refFile];
                curFileData.m_kDepends.Add(refFileData);
                refFileData.m_kRefeds.Add(curFileData);
            }
        }

    }
}
