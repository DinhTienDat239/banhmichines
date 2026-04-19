using UnityEditor;
using UnityEngine;

public class SDKWelcomeWindow : EditorWindow
{
    private static readonly string PrefKey = "MySDK_WelcomeShown";
    private static Texture2D logoTexture;

    private GUIStyle titleStyle;
    private GUIStyle introStyle;

    [InitializeOnLoadMethod]
    static void InitOnLoad()
    {
        //EditorPrefs.DeleteKey("MySDK_WelcomeShown"); // Xoá dòng này nếu không muốn hiện lại mỗi lần

        if (!EditorPrefs.GetBool(PrefKey, false))
        {
            EditorApplication.update += ShowWindowOnce;
        }
    }

    static void ShowWindowOnce()
    {
        EditorApplication.update -= ShowWindowOnce;
        ShowWindow();
    }

    public static void ShowWindow()
    {
        SDKWelcomeWindow window = GetWindow<SDKWelcomeWindow>(true, "DAT SDK", true);
        window.minSize = new Vector2(400, 300);
        window.Show();
    }

    [MenuItem("DAT SDK/Welcome Panel")]
    public static void ShowWelcomePanelFromMenu()
    {
        ShowWindow();
    }

    private void OnEnable()
    {
        logoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(
            "Assets/DAT SDK/Editor/logo_joybit.png"
        );
    }

    private void InitStyles()
    {
        if (titleStyle == null)
        {
            titleStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.55f, 0f) } // Màu cam
            };
        }

        if (introStyle == null)
        {
            introStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 13,
                wordWrap = true
            };
        }
    }

    private void OnGUI()
    {
        InitStyles();
        GUILayout.Space(20);

        // Logo
        if (logoTexture != null)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(logoTexture, GUILayout.Width(128), GUILayout.Height(128));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(15);

        // Tiêu đề
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("DAT SDK", titleStyle, GUILayout.ExpandWidth(true));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        // Giới thiệu
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(
            "SDK này được phát triển bởi lập trình viên Đinh Tiến Đạt.\n\nVui lòng hiểu rằng đây là SDK vận hành nội bộ và đang được phát triển lâu dài.",
            introStyle,
            GUILayout.Width(360)
        );
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();

        // Nút "Đồng ý"
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Đồng ý sử dụng", GUILayout.Width(120), GUILayout.Height(30)))
        {
            EditorPrefs.SetBool(PrefKey, true);
            Close();
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(20);
    }
}
