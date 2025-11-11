using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SocialPlatforms.Impl;

public enum EntryType { ScorePerFaction, Faction, Donation, OverallScore }

public enum FactionNames
{
    NotSelected, GreatNorthAmericanUnion, GreaterChina, GoodArabLeague,
    GrandAfricanUnion, EasternEuropeCollective, GreatSlavicEmpire,
    SouthPacificUnion, UnitedStatesOfSouthAmerica
}

public class DynamicGridUI : MonoBehaviour
{
#if UNITY_WEBGL
    const string apiKeyConst = "68862fc71d8067eda2a193f1";
#else
    const string apiKeyConst = "061b0dd6e8052f2d503b103d77531b8182fbf";
#endif

    [Header("Setup")]
    public EntryType entryType;
    public GameObject rowPrefab;
    public GameObject valuePrefab;
    public FactionNames factionName;

    bool created = false;

    private string apiKey = apiKeyConst;
    private string baseUrl = "https://newdatabase-8dc6.restdb.io/rest";

    [System.Serializable] public class FactionEntry { public string factionname; public int total; }
    [System.Serializable] public class EntryPerFaction { public string username; public int score; }
    [System.Serializable] public class EntryList<T> { public List<T> entries; }

    // 🔗 Mapping EntryTypes to endpoint suffixes
    private readonly Dictionary<EntryType, string> endpointMap = new Dictionary<EntryType, string>
    {
        { EntryType.Faction, "/factions" },
        { EntryType.ScorePerFaction, "/leaderboard" },
        { EntryType.Donation, "/donations" },
        { EntryType.OverallScore, "/overallscore" }
    };

    void Start()
    {
        if (endpointMap.TryGetValue(entryType, out string endpoint))
            baseUrl += endpoint;

        StartCoroutine(LoadDataFromRestDB());
    }

    public IEnumerator LoadDataFromRestDB()
    {
        UnityWebRequest request = UnityWebRequest.Get(baseUrl);
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("x-apikey", apiKey);

        yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
        if (request.result == UnityWebRequest.Result.Success)
#else
        if (!request.isNetworkError && !request.isHttpError)
#endif
        {
            string wrappedJson = "{\"entries\":" + request.downloadHandler.text + "}";

            // 🌐 Deserialize and dispatch based on entry type
            if (entryType == EntryType.Faction)
            {
                var data = JsonUtility.FromJson<EntryList<FactionEntry>>(wrappedJson);
                PopulateGrid(data.entries);
            }
            else if (entryType == EntryType.ScorePerFaction)
            {
                var data = JsonUtility.FromJson<EntryList<EntryPerFaction>>(wrappedJson);
                PopulateGrid(data.entries);
            }
            // You can handle Donation/OverallScore here later if needed
        }
        else
        {
            Debug.LogError("Failed to fetch data: " + request.responseCode);
        }
    }

    List<TextMeshProUGUI> itemsListFaction = new List<TextMeshProUGUI>();
    List<TextMeshProUGUI> itemsListScore = new List<TextMeshProUGUI>();
    // 🧩 Generic method to build the UI grid
    void PopulateGrid<T>(List<T> entries)
    {
        var counter = 0;
        foreach (var entry in entries)
        {
            if (created)
            {
                if (entry is FactionEntry fact)
                {
                     itemsListFaction[counter].text = fact.total.ToString();
                }
                else if (entry is EntryPerFaction scoreE)
                {
                    itemsListScore[counter].text = scoreE.score.ToString();
                }
                counter++;
                continue;
            }

            GameObject newRow = Instantiate(rowPrefab, transform);


            if (entry is FactionEntry faction)
            {
                AddValueToRow(newRow.transform, faction.factionname, false);
                AddValueToRow(newRow.transform, faction.total.ToString(), true);
            }
            else if (entry is EntryPerFaction scoreEntry)
            {
                AddValueToRow(newRow.transform, scoreEntry.username, false, false);
                AddValueToRow(newRow.transform, scoreEntry.score.ToString(), true,false);
            }
        }
        if (!created)
        {
            created = true;
        }        
    }

    void AddValueToRow(Transform row, string valueText, bool toAdd, bool faction = true)
    {
        GameObject valueItem = Instantiate(valuePrefab, row);

        if (valueItem.TryGetComponent(out TextMeshProUGUI tmp))
        {
            if (toAdd)
            {
                if (faction) itemsListFaction.Add(tmp);
                else itemsListScore.Add(tmp);
            }
            tmp.text = valueText;
        }
        if (valueItem.TryGetComponent(out Text uiText))
        {
            uiText.text = valueText;
        } 
    }
}
