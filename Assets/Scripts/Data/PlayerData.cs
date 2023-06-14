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
    public static PlayerDataContainer playerData;
    public static UnityAction<PlayerDataContainer> onDataChanged;


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void BeforeAwake() => playerData = BinarySerializer.Load<PlayerDataContainer>(Filename);




    private void Awake() => onDataChanged?.Invoke(playerData);

    public void Save()
    {
        BinarySerializer.Save(playerData, Filename);
        onDataChanged?.Invoke(playerData);
    }

    public void Load()
    {
        PlayerDataContainer player = BinarySerializer.Load<PlayerDataContainer>(Filename);
        playerData = player;
        onDataChanged?.Invoke(playerData);
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