using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class PlayerDataContainer
{
    public string name = "Player";
    public int currentLevel = 1;
    public int coinCount;
}
public class PlayerData : Singleton<PlayerData>
{
    private const string Filename = "playerData.dat";
    public static PlayerDataContainer container;
    public static UnityAction<PlayerDataContainer> onDataChanged;


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void BeforeAwake() => container = BinarySerializer.Load<PlayerDataContainer>(Filename);




    private void Awake() => onDataChanged?.Invoke(container);

    public void Save()
    {
        BinarySerializer.Save(container, Filename);
        onDataChanged?.Invoke(container);
    }

    public void Load()
    {
        PlayerDataContainer player = BinarySerializer.Load<PlayerDataContainer>(Filename);
        container = player;
        onDataChanged?.Invoke(container);
    }

}


[CustomEditor(typeof(PlayerData))]
public class PlayerDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        PlayerData customer = (PlayerData)target;
        if (GUILayout.Button("Save"))
            customer.Save();

        if (GUILayout.Button("Load"))
            customer.Load();
    }
}