using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

namespace Capybara67
{
    public sealed partial class CapyGame : MonoBehaviour
    {
        [SerializeField] RectTransform board, fruit, capy, leftPaw, rightPaw, particles;
        [SerializeField] Text scoreText, timerText, bestText, comboText, hintText, actionText, walletText, collectionText;
        [SerializeField] Text[] lifeTexts;
        [SerializeField] SoftShape[] rings;
        [SerializeField] Button actionButton, tapButton, soundButton, pauseButton, collectionButton;
        [SerializeField] Text soundLabel, pauseLabel;
        [SerializeField] GameObject hat;
        [SerializeField] int layoutVersion;
        readonly RoundModel round = new RoundModel();
        int best, pearls, outfit;
        bool started, settled, paused, sound = true;
        float feedbackUntil, bounce, partyUntil;
        AudioSource audioSource;
        AudioClip hitClip, failClip, partyClip;
        const string SaveKey = "capy67.save.v1";
        static readonly Color Ink = C("F7EBDD"), Muted = C("A8A0C7"), Mint = C("A4F0CC");

        [System.Serializable] class Save { public int best, pearls, outfit; public bool sound = true; }

        void Start()
        {
            if (layoutVersion < 2) {
                foreach (var cv in GetComponentsInChildren<Canvas>(true)) { cv.gameObject.SetActive(false); Destroy(cv.gameObject); }
                BuildSceneUI();
            }
            try { var s = JsonUtility.FromJson<Save>(PlayerPrefs.GetString(SaveKey, "{}"));
                if (s != null) { best = s.best; pearls = s.pearls; outfit = s.outfit; sound = s.sound; } }
            catch { Debug.LogWarning("Capy67: local save could not be read; using defaults."); }
            audioSource = gameObject.AddComponent<AudioSource>();
            hitClip = Tone(680, .09f); failClip = Tone(170, .16f); partyClip = Tone(1040, .25f);
            actionButton.onClick.AddListener(Action);
            tapButton.onClick.AddListener(Tap);
            soundButton.onClick.AddListener(() => { sound = !sound; Persist(); Refresh(); });
            pauseButton.onClick.AddListener(TogglePause);
            collectionButton.onClick.AddListener(ChangeOutfit);
            InitGarden();
            foreach (var label in GetComponentsInChildren<Text>(true)) {
                bool bold=label.fontStyle==FontStyle.Bold || (label.font && label.font.name.Contains("Bold"));
                label.font=GameFont(bold);label.fontStyle=FontStyle.Normal;
            }
            // Invisible hit areas must still participate in UI raycasts.
            foreach (var button in GetComponentsInChildren<Button>(true))
                if (button.targetGraphic) button.targetGraphic.canvasRenderer.cullTransparentMesh = false;
            Refresh();
        }

        void Action()
        {
            if (wardrobe.activeSelf || platformPaused) return;
            resultPanel.SetActive(false);
            if (paused) { TogglePause(); return; }
            if (round.Running) { Tap(); return; }
            round.Begin(); started = true; settled = false; bounce = 0;
            PlatformPlaying(true);
            hintText.text = "Дождись кольца и тапни!"; feedbackUntil = Time.unscaledTime + 2;
            Refresh();
        }

        void Update()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) Action();
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) TogglePause();
#else
            if (Input.GetKeyDown(KeyCode.Space)) Action();
            if (Input.GetKeyDown(KeyCode.Escape)) TogglePause();
#endif
            if (round.Running && !paused && !platformPaused)
            {
                if (round.Tick(Time.unscaledDeltaTime) == -1) Feedback(-1);
                if (!round.Running) Finish();
            }
            bounce = Mathf.Max(0, bounce - Time.unscaledDeltaTime * 4);
            float t = round.Running || started ? round.Progress : (Mathf.Sin(Time.unscaledTime * 1.4f) + 1) / 2;
            float from = round.Target == 1 ? 117 : 363, to = round.Target == 1 ? 363 : 117;
            float x = Mathf.Lerp(from, to, t);
            float y = 447 - Mathf.Sin(t * Mathf.PI) * 155;
            fruit.anchoredPosition = new Vector2(x, -y);
            fruit.localEulerAngles = new Vector3(0, 0, Mathf.Sin(t * Mathf.PI * 2) * 28);
            capy.localScale = new Vector3(1 + bounce * .025f, 1 - bounce * .025f, 1);
            AnimateGarden();
            leftPaw.localEulerAngles = new Vector3(0, 0, (round.Target == 0 ? bounce * 14 : 0));
            rightPaw.localEulerAngles = new Vector3(0, 0, (round.Target == 1 ? -bounce * 14 : 0));
            for (int i = 0; i < rings.Length; i++)
            {
                bool active = round.Running && round.Target == i;
                bool ready = active && round.FlightTime >= round.FlightDuration - RoundModel.Window;
                rings[i].color = ready ? Mint : active ? C("8B7BB8") : C("554675");
                rings[i].transform.localScale = Vector3.one * (ready ? 1.12f : 1);
            }
            particles.gameObject.SetActive(Time.unscaledTime < partyUntil);
            if (particles.gameObject.activeSelf) particles.localEulerAngles = new Vector3(0, 0, Mathf.Sin(Time.unscaledTime * 8) * 8);
            if (round.Running && !paused && !platformPaused && Time.unscaledTime > feedbackUntil)
                hintText.text = round.FlightTime >= round.FlightDuration - RoundModel.Window ? "СЕЙЧАС!" : "Лови момент…";
            timerText.text = (started ? Mathf.CeilToInt(round.Remaining) : 67) + "с";
        }

        void Tap()
        {
            if (!round.Running || paused || platformPaused || wardrobe.activeSelf) return;
            Feedback(round.Tap());
            if (!round.Running) Finish();
            Refresh();
        }

        void Feedback(int result)
        {
            bounce = 1; feedbackUntil = Time.unscaledTime + .65f;
            hintText.text = result == 7 ? "СИКС-СЕВЕН!  +67" : result == 2 ? "Идеально!  +20" : result == 1 ? "Поймал!  +10" : "Мимо! Попробуй чуть точнее";
            hintText.color = result < 0 ? C("FFA6AD") : Mint;
            if (result == 7) partyUntil = Time.unscaledTime + 1.1f;
            feedbackKind = result;
            if (sound) audioSource.PlayOneShot(result < 0 ? failClip : result == 7 ? partyClip : hitClip, .23f);
            Refresh();
        }

        void Finish()
        {
            if (settled) return;
            settled = true;
            bool record = round.Score > best;
            best = Mathf.Max(best, round.Score);
            int reward = Mathf.CeilToInt(round.Hits / 3f); pearls += reward;
            hintText.text = record ? "Новый рекорд!  +" + reward + " жемч." : "Ещё одна попытка?  +" + reward + " жемч.";
            hintText.color = Mint; Persist(); Refresh();
            PlatformPlaying(false);
            ShowResult(record, reward);
        }

        void TogglePause()
        {
            if (!round.Running || platformPaused) return;
            paused = !paused;
            hintText.text = paused ? "Пауза. Твой мандарин подождёт" : "Лови момент…";
            PlatformPlaying(!paused);
            Refresh();
        }
        void OnApplicationFocus(bool focus) { if (!focus && round.Running && !paused) TogglePause(); }
        void OnApplicationPause(bool value) { if (value && round.Running && !paused) TogglePause(); }

        void ChangeOutfit()
        {
            if (round.Running) return;
            resultPanel.SetActive(false);
            previewOutfit = outfit; wardrobe.SetActive(true); RefreshWardrobe();
        }
        void Persist() { PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(new Save { best = best, pearls = pearls, outfit = outfit, sound = sound })); PlayerPrefs.Save(); }
        void Refresh()
        {
            scoreText.text = round.Score.ToString("000");
            bestText.text = "РЕКОРД  " + best;
            comboText.text = round.Combo > 0 ? "СЕРИЯ  " + round.Combo : "ПОЙМАЙ СВОЙ РИТМ";
            for (int i = 0; i < lifeTexts.Length; i++) lifeTexts[i].color = !started || i < round.Lives ? C("FFB2BA") : C("66557B");
            actionText.text = paused ? "ПРОДОЛЖИТЬ" : round.Running ? "ЛОВИ!" : started ? "ЕЩЁ РАЗ   →" : "ИГРАТЬ   →";
            soundLabel.text = sound ? "♪" : "×"; pauseLabel.text = paused ? ">" : "II";
            walletText.text = pearls + "  ЖЕМЧ.";
            collectionText.text = "ГАРДЕРОБ   →";
            if (cosmetics != null) for (int i=0;i<cosmetics.Length;i++) if(cosmetics[i]) cosmetics[i].SetActive(outfit == i);
            collectionButton.interactable = !round.Running;
            if (comboDots != null) for(int i=0;i<comboDots.Length;i++) comboDots[i].color = i < round.Combo % 7 ? Mint : C("554965");
        }

        static AudioClip Tone(float frequency, float length)
        {
            const int rate = 22050; var data = new float[Mathf.CeilToInt(rate * length)];
            for (int i = 0; i < data.Length; i++) { float t = (float)i / rate; data[i] = Mathf.Sin(t * frequency * Mathf.PI * 2) * Mathf.Sin(Mathf.PI * i / data.Length) * .5f; }
            var clip = AudioClip.Create("Capy tone", data.Length, 1, rate, false); clip.SetData(data, 0); return clip;
        }
        void OnDestroy() { UnbindPlatform(); if (hitClip) Destroy(hitClip); if (failClip) Destroy(failClip); if (partyClip) Destroy(partyClip); }

        public void BuildSceneUI()
        {
            var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGO.transform.SetParent(transform, false);
            canvasGO.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(480, 850); scaler.matchWidthOrHeight = .5f;
            var bg = Shape(canvasGO.transform, "Full background", 0, 0, 480, 850, "191528", 0);
            bg.rectTransform.anchorMin = Vector2.zero; bg.rectTransform.anchorMax = Vector2.one;
            bg.rectTransform.offsetMin = bg.rectTransform.offsetMax = Vector2.zero;
            board = Rect(canvasGO.transform, "Portrait game board", 0, 0, 480, 850);
            board.anchorMin = board.anchorMax = new Vector2(.5f, .5f); board.pivot = new Vector2(.5f, .5f); board.anchoredPosition = Vector2.zero;
            board.gameObject.AddComponent<RectMask2D>();
            // Aspect fit: preserves portrait on desktop and on narrow phones.
            var fit = board.gameObject.AddComponent<BoardFit>();
            Shape(board, "Board", 0, 0, 480, 850, "241D38", 0);
            Shape(board, "Top ambient", -80, 80, 380, 360, "2A2241", 0, true);
            Shape(board, "Bottom ambient", 190, 460, 390, 420, "29233E", 0, true);
            Shape(board, "Brand pill", 28, 27, 180, 29, "38304A", 14);
            Label(board, "Brand", "CAPY CLUB   /   01", 40, 29, 160, 24, 12, "DACAAF", TextAnchor.MiddleCenter);
            soundButton = Button(board, "Sound", 352, 24, 42, 36, "38304A", out soundLabel, "♪", 22);
            pauseButton = Button(board, "Pause", 404, 24, 42, 36, "38304A", out pauseLabel, "II", 19);
            Label(board, "Title", "КАПИБАРА", 25, 77, 300, 53, 42, "F7EBDD", TextAnchor.MiddleLeft, true);
            Shape(board, "67 pill", 331, 83, 118, 48, "A4F0CC", 22);
            Label(board, "67", "6 — 7", 334, 81, 112, 48, 32, "241D38", TextAnchor.MiddleCenter, true);
            Label(board, "Subtitle", "ЛОВИ ВАЙБ", 28, 132, 400, 25, 15, "ADA0C5", TextAnchor.MiddleLeft);
            Shape(board, "Score panel", 28, 180, 424, 82, "342B48", 22);
            Label(board, "Score label", "ОЧКИ", 48, 190, 100, 20, 11, "A8A0C7");
            scoreText = Label(board, "Score", "000", 46, 208, 122, 41, 32, "F7EBDD", TextAnchor.MiddleLeft, true);
            Label(board, "Time label", "ОСТАЛОСЬ", 206, 190, 100, 20, 11, "A8A0C7");
            timerText = Label(board, "Timer", "67с", 204, 210, 96, 36, 27, "A4F0CC", TextAnchor.MiddleLeft, true);
            lifeTexts = new Text[3];
            for (int i = 0; i < 3; i++) lifeTexts[i] = Label(board, "Life " + i, "♥", 343 + i * 28, 208, 29, 35, 23, "FFB2BA");
            Label(board, "Lives label", "ПОПЫТКИ", 342, 190, 98, 20, 11, "A8A0C7");
            comboText = Label(board, "Combo", "ПОЙМАЙ СВОЙ РИТМ", 80, 278, 320, 25, 12, "A8A0C7", TextAnchor.MiddleCenter);
            for (int i = 0; i < 17; i++) {
                float t = i / 16f;
                Shape(board, "Flight guide " + i, Mathf.Lerp(117, 363, t) - 2, 447 - Mathf.Sin(t * Mathf.PI) * 155 - 2, 4, 4, "60516D", 2, true);
            }
            Shape(board, "Ground shadow", 91, 636, 300, 24, "171321", 0, true);
            Shape(board, "Island", 71, 612, 338, 32, "454657", 0, true);
            Shape(board, "Island top", 75, 605, 330, 27, "68786C", 0, true);
            capy = Rect(board, "Capybara artwork", 124, 411, 232, 220); capy.pivot = new Vector2(.5f,.5f); capy.anchoredPosition += new Vector2(116,-110);
            Shape(capy, "Body outline", 34, 86, 170, 130, "35273A", 54);
            Shape(capy, "Body", 39, 87, 160, 122, "B47C52", 50);
            Shape(capy, "Belly", 75, 130, 93, 73, "C9976B", 35);
            Shape(capy, "Foot left", 38, 191, 61, 24, "805438", 12);
            Shape(capy, "Foot right", 147, 191, 61, 24, "805438", 12);
            Shape(capy, "Ear left outline", 30, 3, 43, 55, "35273A", 18);
            Shape(capy, "Ear left", 35, 7, 33, 44, "C18B61", 15);
            Shape(capy, "Ear right outline", 172, 3, 43, 55, "35273A", 18);
            Shape(capy, "Ear right", 177, 7, 33, 44, "C18B61", 15);
            Shape(capy, "Ear inner left", 43, 17, 16, 25, "946044", 8);
            Shape(capy, "Ear inner right", 185, 17, 16, 25, "946044", 8);
            Shape(capy, "Head outline", 10, 25, 224, 142, "35273A", 55);
            Shape(capy, "Head", 15, 28, 214, 133, "C9976B", 51);
            Shape(capy, "Face light", 30, 40, 183, 87, "D9AE7D", 40);
            Shape(capy, "Muzzle", 66, 94, 116, 60, "E8C495", 24);
            Shape(capy, "Eye left", 55, 81, 20, 7, "49333B", 3);
            Shape(capy, "Eye right", 170, 81, 20, 7, "49333B", 3);
            Shape(capy, "Blush left", 36, 99, 27, 11, "D98F78", 5);
            Shape(capy, "Blush right", 184, 99, 27, 11, "D98F78", 5);
            Shape(capy, "Nose left", 100, 111, 8, 6, "644539", 3);
            Shape(capy, "Nose right", 138, 111, 8, 6, "644539", 3);
            Shape(capy, "Mouth", 116, 131, 19, 4, "896047", 2);
            hat = Rect(capy, "Mint bucket hat", 35, 7, 176, 50).gameObject;
            Shape(hat.transform, "Hat crown", 26, -13, 123, 42, "A4F0CC", 13);
            Shape(hat.transform, "Hat brim", 3, 17, 168, 17, "76CBAA", 8);
            Label(hat.transform, "Hat flower", "✦", 75, -10, 28, 29, 24, "FFF4D6"); hat.SetActive(false);
            leftPaw = Rect(board, "Left raised paw", 78, 467, 75, 83);
            rightPaw = Rect(board, "Right raised paw", 329, 467, 75, 83);
            foreach (var paw in new[] {leftPaw, rightPaw}) {
                Shape(paw, "Paw outline", 0, 0, 72, 80, "35273A", 30);
                Shape(paw, "Paw", 5, 4, 62, 71, "C9976B", 27);
                Shape(paw, "Palm", 12, 8, 48, 24, "E8C495", 12);
                for (int j = 0; j < 3; j++) Shape(paw, "Toe " + j, 22+j*12, 13, 3, 8, "A77752", 1);
            }
            rings = new SoftShape[2];
            for(int i=0;i<2;i++) {
                float x = i == 0 ? 117 : 363;
                rings[i] = Shape(board, "Landing ring " + i, x-33, 423, 66, 48, "554675", 0, true);
                Shape(board, "Ring hole " + i, x-27, 428, 54, 38, "241D38", 0, true);
                Label(board, "Palm number " + i, i==0 ? "6" : "7", x-15, 474, 30, 33, 23, "77533B", TextAnchor.MiddleCenter, true);
            }
            fruit = Rect(board, "Mandarin", 117, 447, 44, 44); fruit.pivot = new Vector2(.5f,.5f);
            Shape(fruit, "Fruit outline", 0, 0, 44, 42, "6E483E", 0, true);
            Shape(fruit, "Orange", 3, 1, 38, 37, "FFAB43", 0, true);
            Shape(fruit, "Shine", 9, 6, 11, 7, "FFDA8B", 0, true);
            var leaf = Shape(fruit, "Leaf", 23, -5, 19, 9, "A4F0CC", 0, true); leaf.transform.localEulerAngles = new Vector3(0,0,28);
            particles = Rect(board, "Seven celebration", 240, 390, 200, 100);
            particles.pivot = new Vector2(.5f,.5f);
            Label(particles, "Six seven", "6 — 7!", 0, 0, 200, 68, 48, "A4F0CC", TextAnchor.MiddleCenter, true);
            particles.gameObject.SetActive(false);
            tapButton = Button(board, "Tap play area", 30, 270, 420, 380, "FFFFFF00", out var empty, "", 1);
            hintText = Label(board, "Hint", "Тапни, когда мандарин в кольце", 20, 672, 440, 29, 17, "F7EBDD", TextAnchor.MiddleCenter);
            bestText = Label(board, "Best", "РЕКОРД  0", 20, 707, 440, 23, 12, "A8A0C7", TextAnchor.MiddleCenter);
            actionButton = Button(board, "Primary action", 28, 749, 424, 59, "A4F0CC", out actionText, "ИГРАТЬ   →", 20);
            actionText.color = C("241D38");
            collectionButton = Button(board, "Collection", 27, 819, 225, 27, "FFFFFF00", out collectionText, "Панама  ·  0/10", 12);
            collectionText.color = Muted;
            walletText = Label(board, "Pearls", "0  ЖЕМЧ.", 300, 819, 146, 25, 12, "DACAAF", TextAnchor.MiddleRight);
            if (FindAnyObjectByType<EventSystem>() == null) {
                var es = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
                es.AddComponent<InputSystemUIInputModule>();
#else
                es.AddComponent<StandaloneInputModule>();
#endif
            }
            AddGardenDetails();
            BuildGardenPanels();
            layoutVersion = 2;
        }

        public static Color C(string hex) { ColorUtility.TryParseHtmlString("#" + hex, out var c); return c; }
        static Font GameFont(bool bold) => Resources.Load<Font>("Fonts/LiberationSans-"+(bold?"Bold":"Regular")) ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        static RectTransform Rect(Transform parent, string name, float x, float y, float w, float h)
        {
            var go = new GameObject(name, typeof(RectTransform)); var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false); rt.anchorMin = rt.anchorMax = new Vector2(0,1); rt.pivot = new Vector2(0,1);
            rt.anchoredPosition = new Vector2(x,-y); rt.sizeDelta = new Vector2(w,h); return rt;
        }
        static SoftShape Shape(Transform p, string name, float x,float y,float w,float h,string color,float radius,bool ellipse=false)
        { var s = Rect(p,name,x,y,w,h).gameObject.AddComponent<SoftShape>(); s.color=C(color);s.radius=radius;s.ellipse=ellipse;s.raycastTarget=false;return s; }
        static Text Label(Transform p,string name,string text,float x,float y,float w,float h,int size,string color,TextAnchor align=TextAnchor.MiddleLeft,bool bold=false)
        {
            var l=Rect(p,name,x,y,w,h).gameObject.AddComponent<Text>();l.font=GameFont(bold);
            l.text=text;l.fontSize=size;l.color=C(color);l.alignment=align;l.fontStyle=FontStyle.Normal;l.raycastTarget=false;
            l.horizontalOverflow=HorizontalWrapMode.Overflow;l.verticalOverflow=VerticalWrapMode.Overflow;return l;
        }
        static Button Button(Transform p,string name,float x,float y,float w,float h,string color,out Text label,string text,int size)
        {
            var s=Shape(p,name,x,y,w,h,color,20);s.raycastTarget=true;s.canvasRenderer.cullTransparentMesh=false;var b=s.gameObject.AddComponent<Button>();b.targetGraphic=s;
            var cs=b.colors;cs.highlightedColor=new Color(.93f,1, .97f);cs.pressedColor=new Color(.75f,.87f,.8f);cs.selectedColor=Color.white;b.colors=cs;
            var nav=b.navigation;nav.mode=Navigation.Mode.None;b.navigation=nav;
            label=Label(s.transform,"Label",text,0,0,w,h,size,"F7EBDD",TextAnchor.MiddleCenter,true);return b;
        }
    }
}
