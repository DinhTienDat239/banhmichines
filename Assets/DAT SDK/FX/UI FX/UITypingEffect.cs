using System.Collections;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DAT.UI.FX
{
    [AddComponentMenu("DAT SDK/UI/UITypingEffect")]
    public class UITypingEffect : MonoBehaviour
    {
        [Header("Target Text (auto-detect)")]
        [Tooltip("Nếu để trống, component sẽ tự tìm UI.Text hoặc TextMeshPro trên object.")]
        public Text uiText;
        public TMP_Text tmpText;

        [Header("Typing Settings")]
        [Tooltip("Nội dung cần gõ")]
        [TextArea(2, 8)] public string fullText = string.Empty;
        [Tooltip("Số ký tự hiển thị mỗi giây")]
        public float charactersPerSecond = 30f;
        [Tooltip("Bắt đầu khi OnEnable")]
        public bool autoStart = true;
        [Tooltip("Thời gian chờ trước khi bắt đầu")]
        public float startDelay = 0f;
        [Tooltip("Sử dụng unscaled time")]
        public bool useUnscaledTime = true;
        [Tooltip("Giữ nguyên rich text tags (in đậm, màu, ...) khi gõ")]
        public bool preserveRichText = true;

        [Header("Caret (Con trỏ)")]
        [Tooltip("Hiển thị con trỏ khi đang gõ")]
        public bool showCaretWhileTyping = true;
        [Tooltip("Hiển thị con trỏ sau khi gõ xong")]
        public bool showCaretWhenCompleted = false;
        [Tooltip("Ký tự con trỏ")]
        public string caretChar = "|";
        [Tooltip("Tần suất nhấp nháy (lần/giây)")]
        public float caretBlinkRate = 2f;

        [Header("Controls (ReadOnly)")]
        [SerializeField] private bool isTyping = false;
        [SerializeField] private int visibleCharCount = 0;

        Coroutine typingRoutine;
        Coroutine caretRoutine;
        bool caretVisible;

        void Awake()
        {
            if (uiText == null)
            {
                uiText = GetComponent<Text>();
            }

            if (tmpText == null)
            {
                TryGetComponent(out tmpText);
            }
        }

        void OnEnable()
        {
            if (autoStart)
            {
                if (startDelay > 0f)
                    Invoke(nameof(Play), startDelay);
                else
                    Play();
            }
        }

        void OnDisable()
        {
            Stop();
        }

        public void SetText(string text, bool autoPlay = true)
        {
            fullText = text ?? string.Empty;
            if (autoPlay)
            {
                Play();
            }
            else
            {
                ApplyDisplay(0, false);
            }
        }

        public void Play()
        {
            Stop();
            visibleCharCount = 0;
            typingRoutine = StartCoroutine(TypeRoutine());
        }

        public void Stop()
        {
            if (typingRoutine != null)
            {
                StopCoroutine(typingRoutine);
                typingRoutine = null;
            }
            StopCaret();
        }

        public void SkipToEnd()
        {
            Stop();
            ApplyDisplay(int.MaxValue, false);
            if (showCaretWhenCompleted)
                StartCaret();
        }

        IEnumerator TypeRoutine()
        {
            isTyping = true;
            float secondsPerChar = charactersPerSecond > 0f ? 1f / charactersPerSecond : 0f;
            float accumulator = 0f;

            // reset display
            ApplyDisplay(0, false);

            if (showCaretWhileTyping)
            {
                StartCaret();
            }

            while (true)
            {
                float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                accumulator += dt;

                if (secondsPerChar <= 0f)
                {
                    // immediate
                    visibleCharCount = CountVisibleCharacters(fullText);
                }
                else
                {
                    while (accumulator >= secondsPerChar)
                    {
                        accumulator -= secondsPerChar;
                        visibleCharCount++;
                    }
                }

                int totalVisible = CountVisibleCharacters(fullText);
                bool completed = visibleCharCount >= totalVisible;
                ApplyDisplay(visibleCharCount, !completed && showCaretWhileTyping);

                if (completed)
                    break;

                yield return null;
            }

            isTyping = false;
            StopCaret();
            if (showCaretWhenCompleted)
            {
                StartCaret();
            }
        }

        void StartCaret()
        {
            StopCaret();
            caretRoutine = StartCoroutine(CaretRoutine());
        }

        void StopCaret()
        {
            if (caretRoutine != null)
            {
                StopCoroutine(caretRoutine);
                caretRoutine = null;
            }
            caretVisible = false;
            // refresh to remove caret
            ApplyDisplay(isTyping ? visibleCharCount : CountVisibleCharacters(fullText), false);
        }

        IEnumerator CaretRoutine()
        {
            float interval = caretBlinkRate > 0f ? 1f / caretBlinkRate : 0.5f;
            float timer = 0f;
            while (true)
            {
                float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                timer += dt;
                if (timer >= interval)
                {
                    timer -= interval;
                    caretVisible = !caretVisible;
                    int visible = isTyping ? visibleCharCount : CountVisibleCharacters(fullText);
                    ApplyDisplay(visible, caretVisible);
                }
                yield return null;
            }
        }

        void ApplyDisplay(int targetVisibleChars, bool withCaret)
        {
            string display = preserveRichText
                ? BuildRichTextProgress(fullText, targetVisibleChars)
                : BuildPlainProgress(fullText, targetVisibleChars);

            if (withCaret && !string.IsNullOrEmpty(caretChar))
            {
                display += caretChar;
            }

            if (uiText != null)
            {
                uiText.text = display;
            }

            if (tmpText != null)
            {
                tmpText.text = display;
            }
        }

        static int CountVisibleCharacters(string source)
        {
            if (string.IsNullOrEmpty(source)) return 0;
            int count = 0;
            bool inTag = false;
            for (int i = 0; i < source.Length; i++)
            {
                char c = source[i];
                if (c == '<') { inTag = true; continue; }
                if (inTag)
                {
                    if (c == '>') inTag = false;
                    continue;
                }
                count++;
            }
            return count;
        }

        static string BuildPlainProgress(string source, int visibleChars)
        {
            if (string.IsNullOrEmpty(source) || visibleChars <= 0) return string.Empty;
            if (visibleChars >= source.Length) return source;
            return source.Substring(0, Mathf.Clamp(visibleChars, 0, source.Length));
        }

        static string BuildRichTextProgress(string source, int visibleChars)
        {
            if (string.IsNullOrEmpty(source) || visibleChars <= 0) return string.Empty;

            StringBuilder sb = new StringBuilder(source.Length + 16);
            int shown = 0;
            Stack<string> openTags = new Stack<string>();

            for (int i = 0; i < source.Length; i++)
            {
                char c = source[i];
                if (c == '<')
                {
                    int tagEnd = source.IndexOf('>', i);
                    if (tagEnd == -1)
                    {
                        // malformed, break
                        break;
                    }

                    string tag = source.Substring(i, tagEnd - i + 1);
                    sb.Append(tag);

                    // track simple open/close tags to auto-close later
                    bool isClosing = tag.Length > 2 && tag[1] == '/';
                    if (!isClosing)
                    {
                        // extract tag name until space or '>'
                        int nameStart = 1;
                        int nameLen = 0;
                        for (int k = nameStart; k < tag.Length; k++)
                        {
                            char tk = tag[k];
                            if (tk == ' ' || tk == '>') break;
                            nameLen++;
                        }
                        string tagName = tag.Substring(nameStart, nameLen);
                        // ignore self-closing
                        if (tag[tag.Length - 2] != '/')
                            openTags.Push(tagName);
                    }
                    else
                    {
                        // pop if matches
                        if (openTags.Count > 0)
                        {
                            string top = openTags.Peek();
                            // </b>
                            int nameStart2 = 2;
                            int nameLen2 = 0;
                            for (int k = nameStart2; k < tag.Length; k++)
                            {
                                char tk = tag[k];
                                if (tk == ' ' || tk == '>') break;
                                nameLen2++;
                            }
                            string closingName = tag.Substring(nameStart2, nameLen2);
                            if (top == closingName) openTags.Pop();
                        }
                    }

                    i = tagEnd;
                    continue;
                }

                if (shown < visibleChars)
                {
                    sb.Append(c);
                    shown++;
                }
                else
                {
                    break;
                }
            }

            // auto-close remaining tags
            while (openTags.Count > 0)
            {
                string name = openTags.Pop();
                sb.Append("</").Append(name).Append('>');
            }
            return sb.ToString();
        }
    }
}
