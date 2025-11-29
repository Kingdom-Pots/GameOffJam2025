using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Faction
{
    public string factionname;
    public int total;
    public string _id; // include ID to allow update
}

[System.Serializable]
public class FactionList
{
    public Faction[] factions;
}

public static class FactionService
{
    private static string apiUrl = "https://newdatabase-8dc6.restdb.io/rest/factions";
#if UNITY_WEBGL
    const string apiKeyConst = "68862fc71d8067eda2a193f1";
#else
    const string apiKeyConst = "061b0dd6e8052f2d503b103d77531b8182fbf";
#endif
    /// <summary>
    /// Fetches factions from RestDB and returns them as a list.
    /// </summary>
    public static IEnumerator GetFactions(System.Action<List<Faction>> callback)
    {
        UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("x-apikey", apiKeyConst);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            string wrappedJson = "{\"factions\":" + json + "}";

            FactionList factionList = JsonUtility.FromJson<FactionList>(wrappedJson);
            List<Faction> factions = new List<Faction>(factionList.factions);

            callback?.Invoke(factions);
        }
        else
        {
            Debug.LogError("FactionService Error: " + request.error);
            callback?.Invoke(new List<Faction>());
        }
    }

    /// <summary>
    /// Adds a value to the 'total' of a specific faction by name.
    /// </summary>
    public static IEnumerator AddToFactionTotal(string factionName, int valueToAdd, System.Action<bool> callback)
    {
        // First fetch factions to find the ID
        bool success = false;
        yield return GetFactions(factions =>
        {
            Faction target = factions.Find(f => f.factionname == factionName);
            if (target != null)
            {
                int newTotal = target.total + valueToAdd;

                string bodyJson = "{\"total\":" + newTotal + "}";

                UnityWebRequest patchRequest = UnityWebRequest.Put(apiUrl + "/" + target._id, bodyJson);
                patchRequest.method = "PATCH"; // override to PATCH
                patchRequest.SetRequestHeader("Content-Type", "application/json");
                patchRequest.SetRequestHeader("x-apikey", apiKeyConst);

                var operation = patchRequest.SendWebRequest();
                while (!operation.isDone) { }

                if (patchRequest.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"Updated {factionName} total to {newTotal}");
                    success = true;
                }
                else
                {
                    Debug.LogError("Update Error: " + patchRequest.error);
                }
            }
            else
            {
                Debug.LogError("Faction not found: " + factionName);
            }
        });

        callback?.Invoke(success);
    }
}
