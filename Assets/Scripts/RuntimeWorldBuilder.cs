using UnityEngine;

public static class RuntimeWorldBuilder
{
    private static Material sand;
    private static Material dune;
    private static Material stone;
    private static Material dryPlant;
    private static Material metal;
    private static Material water;
    private static Material beacon;
    private static Material creature;

    public static void Build(DesertGame game)
    {
        Random.InitState(218);
        ConfigureAtmosphere();
        CreateMaterials();
        CreateLandscape();
        CreateOasis();
        CreateLandmarks();
        CreateResources();
        CreatePlayer(game);
        CreateScavenger(game.Player);
    }

    private static void ConfigureAtmosphere()
    {
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.71f, 0.46f, 0.29f);
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.0045f;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.49f, 0.39f, 0.31f);

        GameObject sunObject = new GameObject("Sundown | key light");
        Light sun = sunObject.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.color = new Color(1f, 0.73f, 0.48f);
        sun.intensity = 1.2f;
        sun.shadows = LightShadows.Soft;
        sunObject.transform.rotation = Quaternion.Euler(28f, -32f, 0f);
        RenderSettings.sun = sun;
    }

    private static void CreateMaterials()
    {
        sand = MaterialFor(new Color(0.48f, 0.31f, 0.2f));
        dune = MaterialFor(new Color(0.58f, 0.37f, 0.23f));
        stone = MaterialFor(new Color(0.23f, 0.19f, 0.17f));
        dryPlant = MaterialFor(new Color(0.28f, 0.32f, 0.19f));
        metal = MaterialFor(new Color(0.15f, 0.58f, 0.67f), new Color(0.04f, 0.32f, 0.4f));
        water = MaterialFor(new Color(0.11f, 0.53f, 0.66f), new Color(0.04f, 0.2f, 0.3f));
        beacon = MaterialFor(new Color(0.74f, 0.34f, 0.94f), new Color(0.38f, 0.08f, 0.58f));
        creature = MaterialFor(new Color(0.27f, 0.16f, 0.32f), new Color(0.3f, 0.05f, 0.38f));
    }

    private static void CreateLandscape()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Salt basin";
        ground.transform.position = new Vector3(0f, -0.12f, 0f);
        ground.transform.localScale = new Vector3(14f, 1f, 14f);
        ground.GetComponent<Renderer>().sharedMaterial = sand;

        CreateDune(new Vector3(-35f, -1.7f, -27f), new Vector3(43f, 5f, 23f), -12f);
        CreateDune(new Vector3(37f, -1.9f, 16f), new Vector3(39f, 6f, 25f), 18f);
        CreateDune(new Vector3(2f, -2f, 65f), new Vector3(70f, 7f, 24f), -4f);
        CreateDune(new Vector3(-5f, -2.4f, -66f), new Vector3(72f, 8f, 30f), 8f);

        for (int i = 0; i < 43; i++)
        {
            Vector2 point = Random.insideUnitCircle * Random.Range(11f, 59f);
            if (point.magnitude < 13f || (point - new Vector2(0f, 3f)).magnitude < 8f)
                continue;

            float size = Random.Range(0.35f, 1.45f);
            GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rock.name = "Wind-cut basalt";
            rock.transform.position = new Vector3(point.x, size * 0.3f, point.y);
            rock.transform.localScale = new Vector3(size * 1.3f, size * 0.62f, size);
            rock.transform.rotation = Quaternion.Euler(Random.Range(-18f, 18f), Random.Range(0f, 180f), Random.Range(-20f, 20f));
            rock.GetComponent<Renderer>().sharedMaterial = stone;

            if (i % 4 == 0)
                CreateCactus(new Vector3(point.x + 1.8f, 0f, point.y + 1f), Random.Range(0.75f, 1.25f));
        }
    }

    private static void CreateDune(Vector3 position, Vector3 scale, float yaw)
    {
        GameObject shape = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        shape.name = "Dune ridge";
        shape.transform.position = position;
        shape.transform.localScale = scale;
        shape.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        shape.GetComponent<Renderer>().sharedMaterial = dune;
        Object.Destroy(shape.GetComponent<Collider>());
    }

    private static void CreateCactus(Vector3 position, float size)
    {
        GameObject cactus = new GameObject("Ghost cactus");
        cactus.transform.position = position;
        GameObject trunk = PrimitiveChild(PrimitiveType.Cylinder, cactus.transform, new Vector3(0f, size, 0f),
            new Vector3(0.19f * size, size, 0.19f * size), dryPlant);
        trunk.name = "Trunk";
        PrimitiveChild(PrimitiveType.Cylinder, cactus.transform, new Vector3(-0.31f * size, 0.83f * size, 0f),
            new Vector3(0.1f * size, 0.38f * size, 0.1f * size), dryPlant).transform.localRotation = Quaternion.Euler(0f, 0f, 52f);
        PrimitiveChild(PrimitiveType.Cylinder, cactus.transform, new Vector3(0.28f * size, 1.14f * size, 0f),
            new Vector3(0.1f * size, 0.34f * size, 0.1f * size), dryPlant).transform.localRotation = Quaternion.Euler(0f, 0f, -48f);
    }

    private static void CreateOasis()
    {
        GameObject pool = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pool.name = "Last water | oasis";
        pool.transform.position = new Vector3(0f, 0.04f, 3f);
        pool.transform.localScale = new Vector3(4.2f, 0.08f, 4.2f);
        pool.GetComponent<Renderer>().sharedMaterial = water;
        pool.AddComponent<OasisSource>();

        for (int i = 0; i < 7; i++)
        {
            float angle = i * Mathf.PI * 2f / 7f;
            Vector3 p = new Vector3(Mathf.Cos(angle) * 3.5f, 0f, 3f + Mathf.Sin(angle) * 3.5f);
            CreateCactus(p, Random.Range(0.6f, 0.95f));
        }
    }

    private static void CreateLandmarks()
    {
        CreateWreck(new Vector3(-19f, 0.2f, 18f), 28f);
        CreateWreck(new Vector3(22f, 0.2f, -13f), -19f);
        CreateSignalMast(new Vector3(0f, 0f, 48f));
        CreateMarker("OLD TRADE ROAD", new Vector3(12f, 0f, -8f), 15f);
    }

    private static void CreateWreck(Vector3 position, float yaw)
    {
        GameObject root = new GameObject("Buried courier wreck");
        root.transform.position = position;
        root.transform.rotation = Quaternion.Euler(0f, yaw, 12f);
        PrimitiveChild(PrimitiveType.Cube, root.transform, new Vector3(0f, 1.1f, 0f), new Vector3(5.2f, 1.4f, 2.8f), stone);
        PrimitiveChild(PrimitiveType.Cube, root.transform, new Vector3(1.7f, 1.9f, 0f), new Vector3(1.25f, 0.55f, 2.5f), metal);
        PrimitiveChild(PrimitiveType.Cylinder, root.transform, new Vector3(-1.4f, 0.46f, -1.35f), new Vector3(0.84f, 0.16f, 0.84f), stone);
        PrimitiveChild(PrimitiveType.Cylinder, root.transform, new Vector3(-1.4f, 0.46f, 1.35f), new Vector3(0.84f, 0.16f, 0.84f), stone);
        PrimitiveChild(PrimitiveType.Cylinder, root.transform, new Vector3(2.0f, 0.46f, -1.35f), new Vector3(0.84f, 0.16f, 0.84f), stone);
        PrimitiveChild(PrimitiveType.Cylinder, root.transform, new Vector3(2.0f, 0.46f, 1.35f), new Vector3(0.84f, 0.16f, 0.84f), stone);
    }

    private static void CreateSignalMast(Vector3 position)
    {
        GameObject root = new GameObject("Rescue signal | extraction point");
        root.transform.position = position;
        WorldPickup objective = root.AddComponent<WorldPickup>();
        objective.Kind = WorldPickup.PickupKind.Beacon;
        GameObject pole = PrimitiveChild(PrimitiveType.Cylinder, root.transform, new Vector3(0f, 3f, 0f),
            new Vector3(0.12f, 3f, 0.12f), stone);
        GameObject lamp = PrimitiveChild(PrimitiveType.Sphere, root.transform, new Vector3(0f, 6.1f, 0f),
            Vector3.one * 0.75f, beacon);
        lamp.name = "Signal lamp";
        GameObject ring = PrimitiveChild(PrimitiveType.Cylinder, root.transform, new Vector3(0f, 6.1f, 0f),
            new Vector3(1.4f, 0.035f, 1.4f), beacon);
        ring.name = "Signal halo";
        Light glow = lamp.AddComponent<Light>();
        glow.color = new Color(0.74f, 0.3f, 1f);
        glow.range = 15f;
        glow.intensity = 3f;
        CreateMarker("SIGNAL / 700 KHZ", position + new Vector3(0f, 0.1f, -2f), 0f);
    }

    private static void CreateMarker(string label, Vector3 position, float yaw)
    {
        GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cube);
        post.name = label;
        post.transform.position = position + Vector3.up * 0.8f;
        post.transform.localScale = new Vector3(0.14f, 1.6f, 0.14f);
        post.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        post.GetComponent<Renderer>().sharedMaterial = metal;
    }

    private static void CreateResources()
    {
        Vector3[] salvageSites = {
            new Vector3(-18f, 0f, 18f), new Vector3(-21f, 0f, 15f), new Vector3(-16f, 0f, 20f),
            new Vector3(20f, 0f, -14f), new Vector3(24f, 0f, -12f), new Vector3(19f, 0f, -10f),
            new Vector3(-30f, 0f, -4f), new Vector3(33f, 0f, 31f), new Vector3(-9f, 0f, 37f)
        };
        for (int i = 0; i < salvageSites.Length; i++)
            CreatePickup("Salvage cache", salvageSites[i] + Vector3.up * 0.9f, WorldPickup.PickupKind.Salvage, i % 3 == 0 ? 2 : 1, metal);

        Vector3[] waterSites = {
            new Vector3(-16f, 0f, 17f), new Vector3(23f, 0f, -11f), new Vector3(-28f, 0f, -5f),
            new Vector3(30f, 0f, 28f), new Vector3(7f, 0f, 36f)
        };
        for (int i = 0; i < waterSites.Length; i++)
            CreatePickup("Water flask", waterSites[i] + Vector3.up * 0.75f, WorldPickup.PickupKind.Water, 1, water);
    }

    private static void CreatePickup(string name, Vector3 position, WorldPickup.PickupKind kind, int amount, Material material)
    {
        GameObject root = new GameObject(name);
        root.transform.position = position;
        WorldPickup pickup = root.AddComponent<WorldPickup>();
        pickup.Kind = kind;
        pickup.Amount = amount;
        GameObject core = PrimitiveChild(kind == WorldPickup.PickupKind.Water ? PrimitiveType.Capsule : PrimitiveType.Cube,
            root.transform, Vector3.zero, kind == WorldPickup.PickupKind.Water
                ? new Vector3(0.48f, 0.58f, 0.48f)
                : new Vector3(0.55f, 0.4f, 0.42f), material);
        core.name = name + " marker";
        GameObject glint = PrimitiveChild(PrimitiveType.Sphere, root.transform, new Vector3(0f, 0.43f, 0f),
            Vector3.one * 0.16f, material);
        glint.name = "Signal glint";
    }

    private static void CreatePlayer(DesertGame game)
    {
        GameObject player = new GameObject("Survivor");
        player.transform.position = new Vector3(0f, 0.15f, -1f);
        CharacterController controller = player.AddComponent<CharacterController>();
        controller.height = 1.8f;
        controller.radius = 0.36f;
        controller.center = new Vector3(0f, 0.9f, 0f);
        controller.stepOffset = 0.3f;
        player.AddComponent<PlayerController>();

        GameObject cameraObject = new GameObject("First person view");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.SetParent(player.transform, false);
        cameraObject.transform.localPosition = new Vector3(0f, 1.58f, 0f);
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 78f;
        camera.nearClipPlane = 0.05f;
        camera.farClipPlane = 180f;
        camera.backgroundColor = new Color(0.68f, 0.42f, 0.27f);
        cameraObject.AddComponent<AudioListener>();
        game.SetPlayer(player.transform);
    }

    private static void CreateScavenger(Transform player)
    {
        GameObject hunter = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        hunter.name = "Sandstalker";
        hunter.transform.position = new Vector3(0f, 1.05f, 24f);
        hunter.transform.localScale = new Vector3(0.85f, 0.9f, 1.45f);
        hunter.GetComponent<Renderer>().sharedMaterial = creature;
        hunter.AddComponent<Scavenger>();

        GameObject eyes = PrimitiveChild(PrimitiveType.Sphere, hunter.transform,
            new Vector3(0f, 0.57f, 0.34f), new Vector3(0.38f, 0.15f, 0.1f), beacon);
        eyes.name = "Bioluminescent eyes";
        Object.Destroy(eyes.GetComponent<Collider>());
    }

    private static GameObject PrimitiveChild(PrimitiveType type, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
    {
        GameObject child = GameObject.CreatePrimitive(type);
        child.transform.SetParent(parent, false);
        child.transform.localPosition = localPosition;
        child.transform.localScale = localScale;
        child.GetComponent<Renderer>().sharedMaterial = material;
        return child;
    }

    private static Material MaterialFor(Color color, Color? emission = null)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");
        Material material = new Material(shader);
        material.color = color;
        if (emission.HasValue && material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emission.Value);
        }
        return material;
    }
}
