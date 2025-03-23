using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;


namespace AssetRefTool
{

    public class AssetRefUtil
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
        public static Dictionary<string, UnityFileData> m_kAllUnityFileDataDic = new Dictionary<string, UnityFileData>();
        public static HashSet<UnityFileData> m_kRetDependsUnityFileData = new HashSet<UnityFileData>();

        public static void FindFolderRefs(string[] folderPaths)
        {
            m_kAllUnityFileDataDic.Clear();
            m_kRetDependsUnityFileData.Clear();
            m_kSelectedUnityFileData.Clear();

            var startTime = System.DateTime.Now;


            HashSet<string> allContainsFilePaths = new HashSet<string>(1024);
            List<string> srcFolders = new List<string>();
            foreach (string path in folderPaths)
            {
                if (AssetDatabase.IsValidFolder(path))
                {
                    srcFolders.Add(path);
                }
                else
                {
                    allContainsFilePaths.Add(path);
                }
            }
            if(srcFolders.Count > 0)
            {
                string[] allFolderFileGuids = AssetDatabase.FindAssets("", srcFolders.ToArray());
                HashSet<string> allContainsFileGuids = new HashSet<string>(1024);
                foreach (var folderGuid in allFolderFileGuids)
                {
                    allContainsFileGuids.Add(folderGuid);
                }
                foreach (var guid in allContainsFileGuids)
                {
                    allContainsFilePaths.Add(AssetDatabase.GUIDToAssetPath(guid));
                }
            }

            Debug.Log($"S1 {(System.DateTime.Now - startTime).TotalMilliseconds}");
            startTime = System.DateTime.Now;


            //下面这个foreach 会包含所有相关的对象
            foreach (var refFilePath in AssetDatabase.GetDependencies(allContainsFilePaths.ToArray(), true))
            {
                UnityFileData fileData = new UnityFileData() { m_kPath = refFilePath };
                if (allContainsFilePaths.Contains(refFilePath))
                {
                    m_kSelectedUnityFileData.Add(fileData);
                }
                else
                {
                    m_kRetDependsUnityFileData.Add(fileData);
                }

                if (!m_kAllUnityFileDataDic.ContainsKey(refFilePath))
                {
                    m_kAllUnityFileDataDic.Add(refFilePath, fileData);
                }
                else
                {
                    m_kAllUnityFileDataDic[refFilePath].Clear();
                }
            }
            Debug.Log($"S2 {(System.DateTime.Now - startTime).TotalMilliseconds}");
            int progressCount = 0;
            startTime = System.DateTime.Now;
            //接下来只需要梳理关系即可
            foreach (var fileKv in m_kAllUnityFileDataDic)
            {
                progressCount++;
                if(progressCount % 100 == 0)
                {
                    if(EditorUtility.DisplayCancelableProgressBar("检索总依赖",$"{progressCount}/{m_kAllUnityFileDataDic.Count}", (float)((double)progressCount/ (double)m_kAllUnityFileDataDic.Count)))
                    {
                        return;
                    }
                }
                var refFiles = AssetDatabase.GetDependencies(fileKv.Key, true);
                UnityFileData curFileData = fileKv.Value;
                foreach (var refFile in refFiles)
                {
                    if (refFile == curFileData.m_kPath)
                        continue;

                    var refFileData = m_kAllUnityFileDataDic[refFile];
                    curFileData.m_kDepends.Add(refFileData);
                    refFileData.m_kRefeds.Add(curFileData);
                }
            }
            Debug.Log($"S3 {(System.DateTime.Now - startTime).TotalMilliseconds}");
            EditorUtility.ClearProgressBar();
        }
    }
}

