/*
 * Need the DecoratorEditor script from https://gist.github.com/liortal53/352fda2d01d339306e03 to work
 * 
 * Made by Wendy "Shadowsun" Broeckx 
 * @wenshadowsun
 * support@shadowsun.pro
 * January 2019
 * 
 */
using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
[CanEditMultipleObjects, CustomEditor( typeof( Transform ) )]
public class CustomTransformDrawer : DecoratorEditor
{
    // Have we loaded the prefs yet
    private static bool _prefsLoaded = false;

    private static bool _isEnabled = false;
    private static float _gizmoScale = .5f;

    private CustomTransformDatas _datas;
    private GUISkin _guiSkin;

    private bool _disable = false;

    // Enable the modification of Internal Classes
    public CustomTransformDrawer() : base( "TransformInspector" )
    {

    }

    private void OnEnable()
    {
        // Load Preferences
        if (!_prefsLoaded)
        {
            _isEnabled = EditorPrefs.GetBool( "EnableCustomTransformKey", false );
            _gizmoScale = EditorPrefs.GetFloat( "GizmoScaleKey", .5f );
            _prefsLoaded = true;
        }

        // we check if the tool package path is correct 
        string path = "Packages/com.onanagro.tools.internal";
        if (Directory.Exists( path ))
        {
            _datas = AssetDatabase.LoadAssetAtPath<CustomTransformDatas>( path + "/CustomTransform/Editor/CustomTransform_Data.asset" );
            _guiSkin = AssetDatabase.LoadAssetAtPath<GUISkin>( path + "/CustomTransform/Editor/GUI/CustomTransformGUISkin.guiskin" );
            _disable = false;
        }
        else
        {
            path = "Assets/Onanagro Internal";
            if (Directory.Exists( path ))
            {
                _datas = AssetDatabase.LoadAssetAtPath<CustomTransformDatas>( path + "/CustomTransform/Editor/CustomTransform_Data.asset" );
                _guiSkin = AssetDatabase.LoadAssetAtPath<GUISkin>( path + "/CustomTransform/Editor/GUI/CustomTransformGUISkin.guiskin" );
                _disable = false;
            }
            else
            {
                _disable = true;
            }
        }
    }

    public override void OnInspectorGUI()
    {
        // if we don't find Tool package path, we ignore the rest of the script
        if (_disable)
        {
            base.OnInspectorGUI();
            return;
        }
        // if we disabled the tool in Editor Preferences Windo, we ignore the rest of the script
        if (!_isEnabled)
        {
            base.OnInspectorGUI();
            return;
        }

        // ref of the component
        Transform transform = (Transform)target;

        // local temporary variables
        // -------------------------
        Vector3 startPosition = transform.localPosition;
        Vector3 startRotation = transform.localEulerAngles;
        Vector3 startScale = transform.localScale;

        Vector3 position;
        Vector3 rotation;
        Vector3 scale;

        bool lockScale;
        bool is2D;
        // -------------------------

        // Switch between 2D or 3D Transform
        EditorGUILayout.BeginHorizontal();
        {
            is2D = EditorGUILayout.Toggle( "Enable 2D Transform", _datas.m_is2D );
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Reset - Copy - Paste Buttons
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button( "Reset", EditorStyles.miniButtonLeft ))
            {
                foreach (GameObject go in Selection.gameObjects)
                {
                    Undo.RecordObject( go.transform, "Transform Reset" );

                    go.transform.localPosition = Vector3.zero;
                    go.transform.localEulerAngles = Vector3.zero;
                    go.transform.localScale = Vector3.one;
                }
                return;
            }
            if (GUILayout.Button( "Copy", EditorStyles.miniButtonMid ))
            {
                Undo.RecordObject( transform, "Datas Changed" );

                _datas.m_copyPosition = transform.localPosition;
                _datas.m_copyRotation = transform.localEulerAngles;
                _datas.m_copyScale = transform.localScale;

                EditorUtility.SetDirty( _datas );
                return;
            }
            if (GUILayout.Button( "Paste", EditorStyles.miniButtonRight ))
            {
                foreach (GameObject go in Selection.gameObjects)
                {
                    Undo.RecordObject( go.transform, "Transform Changed" );

                    go.transform.localPosition = _datas.m_copyPosition;
                    go.transform.localEulerAngles = _datas.m_copyRotation;
                    go.transform.localScale = _datas.m_copyScale;
                }
                return;
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // 2D Transform Inspector GUI
        if (_datas.m_is2D)
        {
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space( 20 );
                position = EditorGUILayout.Vector2Field( "Position", transform.localPosition );
                position.z = transform.localPosition.z;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space( 20 );
                rotation = new Vector3( transform.localEulerAngles.x, transform.localEulerAngles.y, EditorGUILayout.Slider( "Rotation", transform.localEulerAngles.z, 0, 359 ) );
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                lockScale = EditorGUILayout.Toggle( _datas.m_lockScale, _guiSkin.customStyles[0], GUILayout.Width( 15 ), GUILayout.Height( 15 ) );

                if (_datas.m_lockScale)
                {
                    Vector2 scaleTemp = EditorGUILayout.Vector2Field( "Scale", transform.localScale );

                    if (scaleTemp.x != transform.localScale.x) scale = new Vector3( scaleTemp.x, scaleTemp.x, transform.localScale.z );
                    else scale = new Vector3( scaleTemp.y, scaleTemp.y, transform.localScale.z );
                }
                else
                {
                    scale = EditorGUILayout.Vector2Field( "Scale", transform.localScale );
                    scale.z = transform.localScale.z;
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        // 3D Transform Inspector GUI
        else
        {
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space( 20 );
                position = EditorGUILayout.Vector3Field( "Position", transform.localPosition );
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space( 20 );
                rotation = EditorGUILayout.Vector3Field( "Rotation", transform.localEulerAngles );
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                lockScale = EditorGUILayout.Toggle( _datas.m_lockScale, _guiSkin.customStyles[0], GUILayout.Width( 15 ), GUILayout.Height( 15 ) );

                if (_datas.m_lockScale)
                {
                    Vector3 scaleTemp = EditorGUILayout.Vector3Field( "Scale", transform.localScale );

                    if (scaleTemp.x != transform.localScale.x) scale = new Vector3( scaleTemp.x, scaleTemp.x, scaleTemp.x );
                    else if (scaleTemp.y != transform.localScale.y) scale = new Vector3( scaleTemp.y, scaleTemp.y, scaleTemp.y );
                    else scale = new Vector3( scaleTemp.z, scaleTemp.z, scaleTemp.z );
                }
                else
                {
                    scale = EditorGUILayout.Vector3Field( "Scale", transform.localScale );
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();

        // Add & Remove Gizmos Buttons
        EditorGUILayout.BeginHorizontal();
        {
            // Add a 3 colors & directions arrow gizmo to show constantly orientation of GameObject
            if (GUILayout.Button( "Add full Gizmo", EditorStyles.miniButtonLeft ))
            {
                foreach (GameObject go in Selection.gameObjects)
                {
                    if (go.GetComponent<DrawAllDirections>() == null)
                    {
                        Undo.RecordObject( go.transform, "Gizmo Added" );
                        if (go.GetComponent<ShowDirection>() != null) EditorApplication.delayCall += () => DestroyImmediate( go.GetComponent<ShowDirection>() );
                        go.AddComponent<DrawAllDirections>().m_arrowSize = _gizmoScale;
                    }
                }
                return;
            }
            // Add a Forward arrow gizmo to show constantly orientation of GameObject
            if (GUILayout.Button( "Add Forward Gizmo", EditorStyles.miniButtonMid ))
            {
                foreach (GameObject go in Selection.gameObjects)
                {
                    if (go.GetComponent<ShowDirection>() == null)
                    {
                        Undo.RecordObject( go.transform, "Gizmo Added" );
                        if (go.GetComponent<DrawAllDirections>() != null) EditorApplication.delayCall += () => DestroyImmediate( go.GetComponent<DrawAllDirections>() );
                        ShowDirection dir = go.AddComponent<ShowDirection>();
                        dir.m_arrowSize = _gizmoScale;
                        dir.m_gizmoColor = Color.blue;
                    }
                }
                return;
            }
            // Remove existing arrow gizmo on GameObject
            if (GUILayout.Button( "Remove Gizmos", EditorStyles.miniButtonRight ))
            {
                foreach (GameObject go in Selection.gameObjects)
                {
                    if (go.GetComponent<ShowDirection>() != null)
                    {
                        Undo.RecordObject( go.transform, "Gizmo Removed" );
                        EditorApplication.delayCall += () => DestroyImmediate( go.GetComponent<ShowDirection>() );
                    }
                    else if (go.GetComponent<DrawAllDirections>() != null)
                    {
                        Undo.RecordObject( go.transform, "Gizmo Removed" );
                        EditorApplication.delayCall += () => DestroyImmediate( go.GetComponent<DrawAllDirections>() );
                    }
                }
                return;
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // check for changes in Inspector
        if (GUI.changed)
        {
            // Apply Transform changes to all selected GameObject and record change for cancel
            foreach (GameObject go in Selection.gameObjects)
            {
                Undo.RecordObject( go.transform, "Transform Changed" );

                Vector3 pos = position - startPosition;
                Vector3 rot = rotation - startRotation;
                Vector3 scl = scale - startScale;

                go.transform.localPosition += pos;
                go.transform.localEulerAngles += rot;
                go.transform.localScale += scl;
            }

            // Apply and Record changes in tool datas
            Undo.RecordObject( _datas, "Datas Changed" );
            _datas.m_is2D = is2D;
            _datas.m_lockScale = lockScale;

            // notify the editor taht it should check changes and repaint
            EditorUtility.SetDirty( _datas );
        }
    }

    [PreferenceItem( "Onanagro Tools" )]
    public static void PreferenceGUI()
    {
        // Preferences GUI
        {
            EditorGUILayout.LabelField( "Custom Transform Tool", EditorStyles.boldLabel );
            EditorGUILayout.Space();

            // Add toggle in Editor Preferences Window to enable or disable it
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space( 10 );
                EditorGUILayout.LabelField( "Enable Custom Transform Tool", GUILayout.Width( 250 ) );
                _isEnabled = EditorGUILayout.Toggle( _isEnabled );
            }
            EditorGUILayout.EndHorizontal();

            // Add slider in Editor Preferences Window to select default gizmo arrow size
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space( 10 );
                EditorGUILayout.LabelField( "Default Gizmo Arrow Size", GUILayout.Width( 250 ) );
                _gizmoScale = EditorGUILayout.Slider( _gizmoScale, .1f, 10f, GUILayout.Width( 250 ) );
            }
            EditorGUILayout.EndHorizontal();
        }

        // check for changes in Editor Preference Window
        if (GUI.changed)
        {
            // Save the Preferences
            EditorPrefs.SetBool( "EnableCustomTransformKey", _isEnabled );
            EditorPrefs.SetFloat( "GizmoScaleKey", _gizmoScale );
        }
    }

    [MenuItem( "Tools/Onanagro/Preferences/Enable Custom Transform", false, 1001 )]
    private static void EnableCustomTransformMenuItem()
    {
        _isEnabled = !_isEnabled;
        EditorPrefs.SetBool( "EnableCustomTransformKey", _isEnabled );
    }

    // Convert Quaternion To Vector4 to use them in GUILayout
    private static Vector4 QuaternionToVector4(Quaternion rot)
    {
        return new Vector4( rot.x, rot.y, rot.z, rot.w );
    }

}
