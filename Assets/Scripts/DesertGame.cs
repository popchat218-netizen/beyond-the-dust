using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class DesertGame : MonoBehaviour
{
    public static DesertGame Instance { get; private set; }

    public int Salvage { get; private set; }
    public int Water { get; private set; }
    public float Hydration { get; private set; } = 100f;
    public float Health { get; private set; } = 100f;
    public bool HasPurifier { get; private set; }
    public bool IsComplete { get; private set; }
    public bool IsDead { get; private set; }
    public bool IsPaused { get; private set; }
    public Transform Player { get; private set; }

    private string statusMessage;
    private float statusMessageTimer;
    private GUIStyle titleStyle;
    private GUIStyle bodyStyle;
    private GUIStyle smallStyle;
    private GUIStyle panelStyle;
    private GUIStyle promptStyle;
    private Texture2D whiteTexture;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        Time.timeScale = 1f;
        RuntimeWorldBuilder.Build(this);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (!IsDead && !IsComplete && Input.GetKeyDown(KeyCode.P))
            SetPaused(!IsPaused);

        if (IsPaused)
            return;

        statusMessageTimer = Mathf.Max(0f, statusMessageTimer - Time.unscaledDeltaTime);

        if (IsDead || IsComplete)
        {
            if (Input.GetKeyDown(KeyCode.R))
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        float drain = HasPurifier ? 0.22f : 0.48f;
        if (PlayerController.IsSprinting)
            drain *= 1.7f;

        Hydration = Mathf.Max(0f, Hydration - drain * Time.deltaTime);
        if (Hydration <= 0f)
            Health = Mathf.Max(0f, Health - 3.4f * Time.deltaTime);
        if (Health <= 0f)
            IsDead = true;

        if (Input.GetKeyDown(KeyCode.C))
            CraftPurifier();
    }

    public void SetPlayer(Transform player)
    {
        Player = player;
    }

    public void Collect(WorldPickup pickup)
    {
        if (pickup == null || pickup.IsCollected)
            return;

        switch (pickup.Kind)
        {
            case WorldPickup.PickupKind.Salvage:
                Salvage += pickup.Amount;
                break;
            case WorldPickup.PickupKind.Water:
                Water += pickup.Amount;
                break;
            case WorldPickup.PickupKind.Beacon:
                if (!HasPurifier)
                {
                    Notify("SIGNAL JAMMED  /  BUILD THE PURIFIER FIRST");
                    return;
                }
                IsComplete = true;
                break;
        }

        pickup.Collect();
        if (pickup.Kind == WorldPickup.PickupKind.Salvage)
            Notify("SALVAGE RECOVERED  /  +" + pickup.Amount);
        else if (pickup.Kind == WorldPickup.PickupKind.Water)
            Notify("WATER RECOVERED  /  +" + pickup.Amount);
        else
            Notify("RELAY LOCKED  /  SIGNAL FOUND");
    }

    public void Drink()
    {
        if (Water <= 0 || Hydration >= 100f)
        {
            Notify(Water <= 0 ? "NO WATER IN PACK" : "HYDRATION ALREADY FULL");
            return;
        }
        Water--;
        Hydration = Mathf.Min(100f, Hydration + 34f);
        Notify("DRANK WATER  /  HYDRATION RESTORED");
    }

    public void TakeDamage(float amount)
    {
        if (IsDead || IsComplete)
            return;
        Health = Mathf.Max(0f, Health - amount);
        if (Health <= 0f)
            IsDead = true;
    }

    private void CraftPurifier()
    {
        if (HasPurifier)
        {
            Notify("PURIFIER ALREADY ONLINE");
            return;
        }
        if (Salvage < 5)
        {
            Notify("NEED 5 SALVAGE TO BUILD THE PURIFIER");
            return;
        }
        Salvage -= 5;
        HasPurifier = true;
        Hydration = Mathf.Min(100f, Hydration + 18f);
        Notify("PURIFIER ONLINE  /  FIND THE VIOLET SIGNAL");
    }

    private void Notify(string message)
    {
        statusMessage = message;
        statusMessageTimer = 2.5f;
    }

    private void SetPaused(bool paused)
    {
        IsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;
        Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = paused;
    }

    private void RestartRun()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnGUI()
    {
        EnsureStyles();
        float width = Screen.width;
        float height = Screen.height;

        GUI.color = new Color(0.04f, 0.055f, 0.075f, 0.78f);
        GUI.Box(new Rect(24f, 24f, 315f, 148f), GUIContent.none, panelStyle);
        GUI.color = Color.white;
        GUI.Label(new Rect(42f, 34f, 270f, 28f), "FIELD STATUS   /   DAY 01", smallStyle);
        DrawMeter(42f, 71f, 224f, 13f, Health / 100f, new Color(0.91f, 0.38f, 0.29f), "VITALS");
        DrawMeter(42f, 115f, 224f, 13f, Hydration / 100f, new Color(0.25f, 0.75f, 0.88f), "HYDRATION");
        GUI.Label(new Rect(278f, 65f, 50f, 24f), Mathf.CeilToInt(Health) + "%", smallStyle);
        GUI.Label(new Rect(278f, 109f, 50f, 24f), Mathf.CeilToInt(Hydration) + "%", smallStyle);

        GUI.color = new Color(0.04f, 0.055f, 0.075f, 0.78f);
        GUI.Box(new Rect(24f, 186f, 315f, 72f), GUIContent.none, panelStyle);
        GUI.color = Color.white;
        GUI.Label(new Rect(42f, 194f, 280f, 20f), CurrentObjectiveTitle(), bodyStyle);
        GUI.Label(new Rect(42f, 220f, 280f, 25f), CurrentObjectiveDetail(), smallStyle);

        GUI.color = new Color(0.04f, 0.055f, 0.075f, 0.78f);
        GUI.Box(new Rect(width - 255f, 24f, 231f, 105f), GUIContent.none, panelStyle);
        GUI.color = Color.white;
        GUI.Label(new Rect(width - 235f, 40f, 190f, 22f), "PACK   /   SALVAGE  " + Salvage.ToString("00"), bodyStyle);
        GUI.Label(new Rect(width - 235f, 75f, 190f, 22f), "WATER  /   " + Water.ToString("00"), bodyStyle);

        Rect center = new Rect(width * 0.5f - 2f, height * 0.5f - 2f, 4f, 4f);
        GUI.color = new Color(1f, 0.83f, 0.55f, 0.9f);
        GUI.DrawTexture(center, whiteTexture);
        GUI.color = Color.white;

        if (!HasPurifier)
            GUI.Label(new Rect(width * 0.5f - 175f, height - 68f, 350f, 28f),
                "PURIFIER   " + Salvage + " / 5 SALVAGE     ·     PRESS C TO CRAFT", promptStyle);
        else
            GUI.Label(new Rect(width * 0.5f - 175f, height - 68f, 350f, 28f),
                "PURIFIER ONLINE   ·   HYDRATION LOSS REDUCED", promptStyle);

        GUI.Label(new Rect(28f, height - 36f, 500f, 24f), "WASD MOVE  SPACE JUMP  SHIFT SPRINT  E INTERACT  P PAUSE", smallStyle);
        DrawInteractionPrompt(width, height);
        if (statusMessageTimer > 0f && !IsPaused)
            GUI.Label(new Rect(width * 0.5f - 260f, 150f, 520f, 26f), statusMessage, promptStyle);
        DrawEndCard(width, height);
    }

    private string CurrentObjectiveTitle()
    {
        if (Salvage < 5)
            return "01   RECOVER SALVAGE";
        if (!HasPurifier)
            return "02   BUILD THE PURIFIER";
        return "03   REACH THE SIGNAL";
    }

    private string CurrentObjectiveDetail()
    {
        if (Salvage < 5)
            return "Search the wrecks  /  " + Salvage + " of 5 recovered";
        if (!HasPurifier)
            return "Press C to craft  /  purifier stabilizes the relay";
        return "Follow the violet mast north  /  press E to transmit";
    }

    private void DrawMeter(float x, float y, float meterWidth, float meterHeight, float value, Color fill, string label)
    {
        GUI.color = new Color(0.8f, 0.86f, 0.9f, 0.16f);
        GUI.DrawTexture(new Rect(x, y, meterWidth, meterHeight), whiteTexture);
        GUI.color = fill;
        GUI.DrawTexture(new Rect(x, y, meterWidth * Mathf.Clamp01(value), meterHeight), whiteTexture);
        GUI.color = Color.white;
        GUI.Label(new Rect(x, y - 18f, meterWidth, 17f), label, smallStyle);
    }

    private void DrawInteractionPrompt(float width, float height)
    {
        if (Player == null || Camera.main == null || IsComplete || IsDead)
            return;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, 3.4f))
        {
            WorldPickup pickup = hit.collider.GetComponentInParent<WorldPickup>();
            if (pickup != null && !pickup.IsCollected)
            {
                string action = pickup.Kind == WorldPickup.PickupKind.Beacon
                    ? HasPurifier ? "ACTIVATE SIGNAL" : "SIGNAL JAMMED  /  PURIFIER REQUIRED" :
                    pickup.Kind == WorldPickup.PickupKind.Water ? "SCAVENGE WATER" : "SCAVENGE SALVAGE";
                GUI.Label(new Rect(width * 0.5f - 190f, height * 0.5f + 26f, 380f, 28f), "[ E ]   " + action, promptStyle);
            }
            else if (pickup == null && hit.collider.GetComponentInParent<OasisSource>() != null)
            {
                GUI.Label(new Rect(width * 0.5f - 150f, height * 0.5f + 26f, 300f, 28f),
                    Water > 0 ? "[ E ]   DRINK STORED WATER" : "NO WATER IN PACK", promptStyle);
            }
        }
    }

    private void DrawEndCard(float width, float height)
    {
        if (IsPaused)
        {
            GUI.color = new Color(0.015f, 0.025f, 0.035f, 0.82f);
            GUI.DrawTexture(new Rect(0f, 0f, width, height), whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(width * 0.5f - 200f, height * 0.5f - 90f, 400f, 50f), "FIELD PAUSED", titleStyle);
            GUI.Label(new Rect(width * 0.5f - 190f, height * 0.5f - 30f, 380f, 28f), "Take a breath. The basin can wait.", bodyStyle);
            if (GUI.Button(new Rect(width * 0.5f - 110f, height * 0.5f + 20f, 220f, 38f), "RESUME  /  P"))
                SetPaused(false);
            if (GUI.Button(new Rect(width * 0.5f - 110f, height * 0.5f + 68f, 220f, 34f), "RESTART RUN"))
                RestartRun();
            return;
        }
        if (!IsComplete && !IsDead)
            return;

        GUI.color = new Color(0.015f, 0.025f, 0.035f, 0.82f);
        GUI.DrawTexture(new Rect(0f, 0f, width, height), whiteTexture);
        GUI.color = Color.white;
        string heading = IsComplete ? "SIGNAL FOUND" : "LOST TO THE DUST";
        string detail = IsComplete
            ? "A reply cuts through the static. Someone heard you."
            : "The basin falls quiet. The beacon is still out there.";
        GUI.Label(new Rect(width * 0.5f - 280f, height * 0.5f - 72f, 560f, 52f), heading, titleStyle);
        GUI.Label(new Rect(width * 0.5f - 300f, height * 0.5f - 8f, 600f, 30f), detail, bodyStyle);
        GUI.Label(new Rect(width * 0.5f - 180f, height * 0.5f + 46f, 360f, 28f), "PRESS R TO RUN IT BACK", promptStyle);
    }

    private void EnsureStyles()
    {
        if (whiteTexture == null)
        {
            whiteTexture = new Texture2D(1, 1);
            whiteTexture.SetPixel(0, 0, Color.white);
            whiteTexture.Apply();
        }
        if (titleStyle != null)
            return;

        titleStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 34, fontStyle = FontStyle.Bold };
        titleStyle.normal.textColor = new Color(1f, 0.82f, 0.58f);
        bodyStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold };
        bodyStyle.normal.textColor = new Color(0.88f, 0.91f, 0.94f);
        smallStyle = new GUIStyle(GUI.skin.label) { fontSize = 10, fontStyle = FontStyle.Bold };
        smallStyle.normal.textColor = new Color(0.63f, 0.73f, 0.8f);
        panelStyle = new GUIStyle(GUI.skin.box);
        promptStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 12, fontStyle = FontStyle.Bold };
        promptStyle.normal.textColor = new Color(1f, 0.84f, 0.61f);
    }
}
