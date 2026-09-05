using UnityEngine;

// Ustawienia aplikacji

public static class GameSettings
{
    private const string KeyLowPoly = "Settings.StartLowPoly";
    private const string KeyOrbits  = "Settings.ShowOrbits";

    public static bool startLowPoly;
    public static bool showOrbits;

    public static void Load()
    {
        startLowPoly = PlayerPrefs.GetInt(KeyLowPoly, 0) == 1;
        showOrbits   = PlayerPrefs.GetInt(KeyOrbits, 1) == 1;
    }

    public static void Save()
    {
        PlayerPrefs.SetInt(KeyLowPoly, startLowPoly ? 1 : 0);
        PlayerPrefs.SetInt(KeyOrbits,  showOrbits   ? 1 : 0);
        PlayerPrefs.Save();
    }

}