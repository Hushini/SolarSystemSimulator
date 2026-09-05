using System;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

/*
    Eksportuje pozycje planet dla wybranych dat do pliku CSV.
    Jak używać: dodać skrypt do dowolnego obiektu, w inspektorze
    kliknąć menu komponentu (trzy kropki) a następnie "Eksportuj pozycje do CSV".
    Plik zapisuje sie w glownym katalogu projektu obok folderu Assets.
*/
public class ValidationExporter : MonoBehaviour
{
    [Tooltip("Daty UTC w formacie RRRR-MM-DD")]
    public string[] dates = new string[]
    {
        "1950-01-01",
        "2000-01-01",
        "2025-01-01",
        "2030-01-01",
        "2050-01-01"
    };

    [ContextMenu("Eksportuj pozycje do CSV")]
    public void Export()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("date_utc;planet;x_AU;y_AU;z_AU;r_AU");

        foreach (string dateStr in dates)
        {
            if (!DateTime.TryParse(dateStr, CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal, out DateTime date))
            {
                Debug.LogWarning($"Nie mozna sparsowac daty: {dateStr}");
                continue;
            }
            date = DateTime.SpecifyKind(date, DateTimeKind.Utc);

            foreach (Planet p in Enum.GetValues(typeof(Planet)))
            {
                Vector3 pos = OrbitalMechanics.GetHeliocentricPosition(p, date);
                double r = Math.Sqrt(pos.x * pos.x + pos.y * pos.y + pos.z * pos.z);
                sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                    "{0};{1};{2:F8};{3:F8};{4:F8};{5:F8}",
                    date.ToString("yyyy-MM-dd"), p, pos.x, pos.y, pos.z, r));
            }
        }

        string path = Path.Combine(Application.dataPath, "..", "validation_unity.csv");
        File.WriteAllText(path, sb.ToString());
        Debug.Log($"Zapisano walidacje do: {Path.GetFullPath(path)}");
    }
}