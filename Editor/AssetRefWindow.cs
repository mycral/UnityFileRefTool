using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

public class AssetRefWindow : EditorWindow
{
    [MenuItem("Assets/FindFolderRefs")]
    public static void CreateWindow()
    {
        GetWindow<AssetRefWindow>().Show();
        
    }

    static string[] m_kLastGUIDs;

    private List<string> m_kSelectFolderPaths = new List<string>();
    private List<string> m_kRefFilePaths = new List<string>();

    private Vector2 m_kScrollPos;
    private Vector2 m_kScrollPos1;
    private SearchField m_kSearchField;
    private string m_kCurSearchText;

    private AssetRefUtil.UnityFileData m_kCurSelectUnityFileData;

    private float m_fNumberWidth = 50;


    private static bool AreArraysEqual(string[] a, string[] b)
    {
        if (a == null || b == null || a.Length != b.Length) return false;
        for (int i = 0; i < a.Length; i++)
        {
            if (a[i] != b[i]) return false;
        }
        return true;
    }


    private void OnEnable()
    {
        if(m_kSearchField == null)
        {
            m_kSearchField = new SearchField();
        }


        EditorApplication.update -= Update;
        EditorApplication.update += Update;
    }

    private void OnDisable()
    {
        EditorApplication.update -= Update;
    }


    private void _onSelectionChange()
    {
        m_kCurSelectUnityFileData = null;
        m_kRefFilePaths.Clear();
        m_kSelectFolderPaths.Clear();

        var curFolderGuids = Selection.assetGUIDs;
        foreach (var folder in curFolderGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(folder);
            m_kSelectFolderPaths.Add(path);
        }

        AssetRefUtil.FindFolderRefs(m_kSelectFolderPaths.ToArray());

        Repaint();
    }


    private void Update()
    {

        string[] currentGUIDs = Selection.assetGUIDs;
        if (m_kLastGUIDs == null || !AreArraysEqual(m_kLastGUIDs, currentGUIDs))
        {
            m_kLastGUIDs = currentGUIDs;
            _onSelectionChange();
        }
    }

    private void OnGUI()
    {
        using (new GUILayout.HorizontalScope())
        {
            OnLeftPart();
            OnRightPart();
        }

    }

    private void OnRightPart()
    {
        using(new GUILayout.VerticalScope(GUILayout.ExpandWidth(true)))
        {
            if (m_kCurSelectUnityFileData == null)
                return;

            EditorGUILayout.LabelField($"Selected: {m_kCurSelectUnityFileData.m_kPath}");
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Reference From:");

            foreach(var refSrc in m_kCurSelectUnityFileData.m_kRefeds)
            {
                EditorGUILayout.LabelField(refSrc.m_kPath);
            }
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Depends:");

            foreach (var dps in m_kCurSelectUnityFileData.m_kDepends)
            {
                EditorGUILayout.LabelField(dps.m_kPath);
            }
        }                           
    }

    private void OnLeftPart()
    {

        Vector2 windowSize = this.position.size;

        GUILayout.BeginVertical(GUILayout.Width(windowSize.x/ 2));

        foreach(var selectPath in m_kSelectFolderPaths)
        {
            EditorGUILayout.LabelField(selectPath);
        }

        m_kScrollPos1 = GUILayout.BeginScrollView(m_kScrollPos1,GUILayout.ExpandHeight(false));
        int count1 = 0;
        foreach (var selectData in AssetRefUtil.m_kSelectedUnityFileData)
        {
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button($"{count1}", GUILayout.Width(m_fNumberWidth)))
                {
                    m_kCurSelectUnityFileData = selectData;
                    GUIUtility.systemCopyBuffer = selectData.m_kPath;
                }

                EditorGUILayout.LabelField(selectData.m_kPath);
                count1++;
            }

        }
        GUILayout.EndScrollView();

        m_kCurSearchText = m_kSearchField.OnGUI(m_kCurSearchText);

        m_kScrollPos = GUILayout.BeginScrollView(m_kScrollPos);
        int count = 0;
        foreach(var refData in AssetRefUtil.m_kRetDependsUnityFileData)
        {
            if (m_kCurSearchText != null && m_kCurSearchText.Length > 0)
            {
                if(refData.m_kPath.Contains(m_kCurSearchText))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        if( GUILayout.Button($"{count}",GUILayout.Width(m_fNumberWidth)))
                        {
                            m_kCurSelectUnityFileData = refData;
                            GUIUtility.systemCopyBuffer = refData.m_kPath;
                        }

                        EditorGUILayout.LabelField(refData.m_kPath);
                        count++;
                    }
                }
            }
            else
            {
                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button($"{count}", GUILayout.Width(m_fNumberWidth)))
                    {
                        m_kCurSelectUnityFileData = refData;
                        GUIUtility.systemCopyBuffer = refData.m_kPath;
                    }
                    EditorGUILayout.LabelField(refData.m_kPath);
                    count++;
                }
            }
        }

        GUILayout.EndScrollView();

        GUILayout.EndVertical();
    }




}
