using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class DesertMapBuilder
{
    const string WorldDir = "Assets/Prefabs/World";
    const string ObsDir = "Assets/Prefabs/Obstacles";
    const string TileDir = "Assets/Prefabs/Tiles";
    const string MatDir = "Assets/Materials";

    // tweak these, then run the menu item again
    const float FenceStep = 2f;
    const float FenceRotY = 90f;
    const float BuildingRotY = 90f;

    [MenuItem("Chicken Dash/Build Desert Map")]
    public static void Build()
    {
        EnsureFolder(MatDir); EnsureFolder(ObsDir); EnsureFolder(TileDir);

        var hay = Load(ObsDir + "/Haybale.prefab");
        var fence = Load(ObsDir + "/Fence.prefab");
        var egg = Load(ObsDir + "/Egg.prefab");
        var gold = Load(ObsDir + "/GoldEgg.prefab");
        if (hay == null || fence == null || egg == null || gold == null)
        {
            EditorUtility.DisplayDialog("Chicken Dash",
                "Missing prefab in Assets/Prefabs/Obstacles. Need: Haybale, Fence, Egg, GoldEgg.", "OK");
            return;
        }

        var m = new Mats
        {
            sand = GetMat("Sand", new Color(0.94f, 0.82f, 0.62f)),
            road = GetMat("DesertRoad", new Color(0.86f, 0.66f, 0.44f)),
            mesa = GetMat("Mesa", new Color(0.78f, 0.44f, 0.26f)),
            mesaDark = GetMat("MesaDark", new Color(0.66f, 0.36f, 0.22f)),
            cactus = GetMat("Cactus", new Color(0.30f, 0.55f, 0.28f)),
            rock = GetMat("Rock", new Color(0.72f, 0.66f, 0.58f)),
            wood = GetMat("BeamBrown", new Color(0.45f, 0.28f, 0.12f)),
        };

        var beam = EnsureBeam(m.wood);
        var bench = EnsureBench(m.wood);
        var rockObs = EnsureRockObstacle(m.rock);

        var tiles = new GroundTile[3];
        for (int i = 0; i < 3; i++)
            tiles[i] = BuildTile("GroundTile_" + "ABC"[i], i + 11, m);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        SetupScene(tiles, new[] { hay, fence, beam, bench, rockObs }, egg, gold);
        Debug.Log("Chicken Dash: desert map built. Press Play.");
    }

    class Mats { public Material sand, road, mesa, mesaDark, cactus, rock, wood; }

    // ---------- helpers ----------
    static GameObject Load(string path) { return AssetDatabase.LoadAssetAtPath<GameObject>(path); }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }

    static Material GetMat(string name, Color c)
    {
        string path = MatDir + "/" + name + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m != null) return m;
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        m = new Material(shader);
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        if (m.HasProperty("_Color")) m.SetColor("_Color", c);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0f);
        if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", 0f);
        AssetDatabase.CreateAsset(m, path);
        return m;
    }

    static float R(System.Random r, float a, float b) { return a + (float)r.NextDouble() * (b - a); }

    static GameObject Prim(PrimitiveType type, string name, Transform parent, Vector3 pos, Vector3 scale, Material mat, Vector3 euler = default(Vector3))
    {
        var g = GameObject.CreatePrimitive(type);
        g.name = name;
        g.transform.SetParent(parent, false);
        g.transform.localPosition = pos;
        g.transform.localScale = scale;
        g.transform.localRotation = Quaternion.Euler(euler);
        g.GetComponent<Renderer>().sharedMaterial = mat;
        Object.DestroyImmediate(g.GetComponent<Collider>());
        return g;
    }

    static void Deco(GameObject parent, string name, Vector3 pos, float rotY)
    {
        var src = Load(WorldDir + "/" + name + ".prefab");
        if (src == null) { Debug.LogWarning("Missing prefab: " + name); return; }
        var go = (GameObject)PrefabUtility.InstantiatePrefab(src, parent.transform);
        go.transform.localPosition = pos;
        go.transform.localRotation = Quaternion.Euler(0f, rotY, 0f);
        foreach (var c in go.GetComponentsInChildren<Collider>()) c.enabled = false;
    }

    // ---------- obstacle prefabs ----------
    static GameObject EnsureBeam(Material mat)
    {
        string path = ObsDir + "/Beam.prefab";
        var existing = Load(path);
        if (existing != null) return existing;
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Beam";
        go.transform.position = new Vector3(0f, 1.7f, 0f);
        go.transform.localScale = new Vector3(2f, 0.4f, 0.4f);
        go.GetComponent<Renderer>().sharedMaterial = mat;
        go.GetComponent<BoxCollider>().isTrigger = true;
        go.AddComponent<Obstacle>().type = Obstacle.Type.High;
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    static GameObject EnsureBench(Material wood)
    {
        string path = ObsDir + "/Bench.prefab";
        var existing = Load(path);
        if (existing != null) return existing;
        var root = new GameObject("Bench");
        var t = root.transform;
        Prim(PrimitiveType.Cube, "Seat", t, new Vector3(0f, 0.55f, 0f), new Vector3(1.8f, 0.12f, 0.55f), wood);
        Prim(PrimitiveType.Cube, "Back", t, new Vector3(0f, 0.95f, -0.25f), new Vector3(1.8f, 0.5f, 0.08f), wood);
        Prim(PrimitiveType.Cube, "LegL", t, new Vector3(-0.7f, 0.27f, 0f), new Vector3(0.12f, 0.54f, 0.45f), wood);
        Prim(PrimitiveType.Cube, "LegR", t, new Vector3(0.7f, 0.27f, 0f), new Vector3(0.12f, 0.54f, 0.45f), wood);
        var bc = root.AddComponent<BoxCollider>();
        bc.isTrigger = true; bc.center = new Vector3(0f, 0.5f, 0f); bc.size = new Vector3(1.8f, 1f, 0.6f);
        root.AddComponent<Obstacle>().type = Obstacle.Type.Low;
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    static GameObject EnsureRockObstacle(Material rock)
    {
        string path = ObsDir + "/RockObstacle.prefab";
        var existing = Load(path);
        if (existing != null) return existing;
        var root = new GameObject("RockObstacle");
        var t = root.transform;
        Prim(PrimitiveType.Sphere, "RockA", t, new Vector3(-0.2f, 0.35f, 0f), new Vector3(1.2f, 0.8f, 1f), rock, new Vector3(0f, 20f, 0f));
        Prim(PrimitiveType.Sphere, "RockB", t, new Vector3(0.4f, 0.25f, 0.2f), new Vector3(0.8f, 0.6f, 0.7f), rock, new Vector3(0f, -30f, 0f));
        var bc = root.AddComponent<BoxCollider>();
        bc.isTrigger = true; bc.center = new Vector3(0f, 0.4f, 0f); bc.size = new Vector3(1.6f, 0.9f, 1.1f);
        root.AddComponent<Obstacle>().type = Obstacle.Type.Low;
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    // ---------- scenery ----------
    static void Cactus(Transform parent, Vector3 pos, float scale, Material mat)
    {
        var root = new GameObject("Cactus");
        root.transform.SetParent(parent, false);
        root.transform.localPosition = pos;
        root.transform.localScale = Vector3.one * scale;
        var t = root.transform;
        Prim(PrimitiveType.Cylinder, "Body", t, new Vector3(0f, 1f, 0f), new Vector3(0.5f, 1f, 0.5f), mat);
        Prim(PrimitiveType.Cylinder, "ArmLink", t, new Vector3(-0.4f, 0.9f, 0f), new Vector3(0.2f, 0.3f, 0.2f), mat, new Vector3(0f, 0f, 90f));
        Prim(PrimitiveType.Cylinder, "ArmL", t, new Vector3(-0.7f, 1.3f, 0f), new Vector3(0.22f, 0.5f, 0.22f), mat);
        Prim(PrimitiveType.Cylinder, "ArmLinkR", t, new Vector3(0.4f, 1.2f, 0f), new Vector3(0.2f, 0.3f, 0.2f), mat, new Vector3(0f, 0f, 90f));
        Prim(PrimitiveType.Cylinder, "ArmR", t, new Vector3(0.7f, 1.55f, 0f), new Vector3(0.22f, 0.4f, 0.22f), mat);
    }

    static void Rock(Transform parent, System.Random r, Vector3 pos, Material mat)
    {
        float s = R(r, 0.6f, 1.6f);
        Prim(PrimitiveType.Sphere, "Rock", parent, pos + new Vector3(0f, s * 0.2f, 0f),
            new Vector3(s * 1.3f, s * 0.7f, s), mat, new Vector3(0f, R(r, 0f, 360f), 0f));
    }

    static void Mesa(Transform parent, System.Random r, Vector3 pos, Material a, Material b)
    {
        var root = new GameObject("Mesa");
        root.transform.SetParent(parent, false);
        root.transform.localPosition = pos;
        root.transform.localRotation = Quaternion.Euler(0f, R(r, 0f, 360f), 0f);
        float s = R(r, 0.8f, 1.4f);
        float h1 = 7f * s, h2 = 5f * s, h3 = 3.5f * s;
        Prim(PrimitiveType.Cube, "Base", root.transform, new Vector3(0f, h1 / 2f, 0f), new Vector3(15f * s, h1, 11f * s), a);
        Prim(PrimitiveType.Cube, "Mid", root.transform, new Vector3(0f, h1 + h2 / 2f, 0f), new Vector3(11f * s, h2, 8f * s), b);
        Prim(PrimitiveType.Cube, "Top", root.transform, new Vector3(0f, h1 + h2 + h3 / 2f, 0f), new Vector3(7f * s, h3, 5f * s), a);
    }

    static void WaterTower(Transform parent, Vector3 pos, Material wood, Material rock)
    {
        var root = new GameObject("WaterTower");
        root.transform.SetParent(parent, false);
        root.transform.localPosition = pos;
        var t = root.transform;
        foreach (var o in new[] { new Vector2(-1f, -1f), new Vector2(1f, -1f), new Vector2(-1f, 1f), new Vector2(1f, 1f) })
            Prim(PrimitiveType.Cylinder, "Leg", t, new Vector3(o.x * 0.9f, 2.5f, o.y * 0.9f), new Vector3(0.18f, 2.5f, 0.18f), wood);
        Prim(PrimitiveType.Cylinder, "Tank", t, new Vector3(0f, 6f, 0f), new Vector3(2.4f, 1.1f, 2.4f), wood);
        Prim(PrimitiveType.Cylinder, "Lid", t, new Vector3(0f, 7.2f, 0f), new Vector3(2.5f, 0.08f, 2.5f), rock);
        Prim(PrimitiveType.Sphere, "Roof", t, new Vector3(0f, 7.3f, 0f), new Vector3(2.4f, 0.9f, 2.4f), wood);
    }

    // ---------- tile ----------
    static GroundTile BuildTile(string name, int seed, Mats m)
    {
        var r = new System.Random(seed);
        var root = new GameObject(name);
        var tile = root.AddComponent<GroundTile>();
        var items = new GameObject("Items");
        items.transform.SetParent(root.transform, false);
        tile.items = items.transform;
        var rt = root.transform;

        Prim(PrimitiveType.Cube, "Road", rt, new Vector3(0f, -0.05f, 12f), new Vector3(7f, 0.1f, 24f), m.road);
        Prim(PrimitiveType.Cube, "SandLeft", rt, new Vector3(-63.5f, -0.05f, 12f), new Vector3(120f, 0.1f, 24f), m.sand);
        Prim(PrimitiveType.Cube, "SandRight", rt, new Vector3(63.5f, -0.05f, 12f), new Vector3(120f, 0.1f, 24f), m.sand);

        string[] props = { "Prop_Wheelbarrow", "Prop_Haystack_01" };
        string[] barns = { "Bld_Barn_01", "Bld_Barn_02" };

        bool towerDone = r.NextDouble() > 0.4;   // about 40% of tiles get a water tower
        foreach (int side in new[] { -1, 1 })
        {
            // wooden fence along the road edge
            for (float z = 1f; z < 24f; z += FenceStep)
                Deco(root, "Env_WoodFence_01", new Vector3(side * 4.3f, 0f, z), FenceRotY);

            // red barn (some tiles)
            if (r.NextDouble() < 0.35)
                Deco(root, barns[r.Next(barns.Length)],
                    new Vector3(side * R(r, 13f, 19f), 0f, R(r, 6f, 18f)),
                    side > 0 ? -BuildingRotY : BuildingRotY);

            // water tower (rare)
            if (!towerDone && side == (r.NextDouble() < 0.5 ? -1 : 1))
            {
                WaterTower(rt, new Vector3(side * R(r, 14f, 22f), 0f, R(r, 4f, 20f)), m.wood, m.rock);
                towerDone = true;
            }

            // cacti, rocks, small farm props
            for (int i = 0; i < 3; i++)
                Cactus(rt, new Vector3(side * R(r, 6f, 18f), 0f, R(r, 1f, 23f)), R(r, 0.8f, 1.4f), m.cactus);
            for (int i = 0; i < 4; i++)
                Rock(rt, r, new Vector3(side * R(r, 5.5f, 20f), 0f, R(r, 1f, 23f)), m.rock);
            Deco(root, props[r.Next(props.Length)],
                new Vector3(side * R(r, 6f, 9f), 0f, R(r, 2f, 22f)), R(r, 0f, 360f));

            // far mesas
            Mesa(rt, r, new Vector3(side * R(r, 34f, 52f), 0f, R(r, 2f, 22f)), m.mesa, m.mesaDark);
        }

        string p = TileDir + "/" + name + ".prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, p);
        Object.DestroyImmediate(root);
        return prefab.GetComponent<GroundTile>();
    }

    // ---------- scene ----------
    static void SetupScene(GroundTile[] tiles, GameObject[] obstacles, GameObject egg, GameObject gold)
    {
        var oldWorld = GameObject.Find("World");
        if (oldWorld != null) Object.DestroyImmediate(oldWorld);

        var player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            player = GameObject.CreatePrimitive(PrimitiveType.Cube);
            player.name = "Player";
            player.tag = "Player";
            player.transform.position = new Vector3(0f, 0.5f, 0f);
            player.GetComponent<BoxCollider>().isTrigger = true;
            var rb = player.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            player.AddComponent<TempRunner>();
        }

        var worldGo = new GameObject("World");
        var wm = worldGo.AddComponent<WorldManager>();
        wm.player = player.transform;
        wm.tilePrefabs = tiles;
        wm.obstaclePrefabs = obstacles;
        wm.eggPrefab = egg;
        wm.goldEggPrefab = gold;
        wm.tileCount = 8;

        // desert look: blue sky, warm sun, sandy haze
        var cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.62f, 0.78f, 0.93f);
        }
        foreach (var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
        {
            if (l.type != LightType.Directional) continue;
            l.color = new Color(1f, 0.95f, 0.85f);
            l.intensity = 1.2f;
            l.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            break;
        }
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.90f, 0.80f, 0.66f);
        RenderSettings.fogStartDistance = 70f;
        RenderSettings.fogEndDistance = 160f;

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }
}