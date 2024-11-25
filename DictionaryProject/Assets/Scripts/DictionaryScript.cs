using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;



public class DictionaryScript : MonoBehaviour
{
    private string synApiUrl = "https://api.datamuse.com/words?rel_syn=[input]";
    private string antApiUrl = "https://api.datamuse.com/words?rel_ant=[input]";
    private string callSynorAnt = "";
    public TMP_InputField inputField;
    public TMP_Text synResultText;
    public TMP_Text antResultText;


    // Start is called before the first frame update
    void Start()
    {

    }

    public void SynBtnTrigger()
    {
        callSynorAnt = "Synonym";
        StartCoroutine(CallDatamuseApi());
    }

    public void AntBtnTrigger()
    {
        callSynorAnt = "Antonym";
        StartCoroutine(CallDatamuseApi());
    }

    // Update is called once per frame
    IEnumerator CallDatamuseApi()
    {
        string inputText = inputField.text;

        UnityWebRequest request = null;
        if (callSynorAnt == "Synonym")
            request = UnityWebRequest.Get(synApiUrl.Replace("[input]", UnityWebRequest.EscapeURL(inputText)));
        else if (callSynorAnt == "Antonym")
            request = UnityWebRequest.Get(antApiUrl.Replace("[input]", UnityWebRequest.EscapeURL(inputText)));

        if (request == null)
        {
            Debug.LogError("Request was not initialized.");
            yield break;
        }

        // Wait for the response
        yield return request.SendWebRequest();

        // Check for errors
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + request.error);
        }
        else
        {
            // Get the response as a JSON string
            string jsonResponse = request.downloadHandler.text;

            // Log the raw JSON response
            Debug.Log("Response: " + jsonResponse);

            // Parse the JSON (Optional: if you want to work with the data programmatically)
            ParseResponse(jsonResponse);
        }
    }

    void ParseResponse(string jsonResponse)
    {
        if (callSynorAnt == "Synonym")
        {
            Synonym[] words = JsonHelper.FromJson<Synonym>(jsonResponse);

            // Build the result string
            string result = $"{callSynorAnt}s for '{inputField.text}':\n";
            foreach (Synonym word in words)
            {
                result += "- " + word.word + "\n";
            }

            // Update the TextMeshPro field with the result
            synResultText.text = result;
        }
        else if (callSynorAnt == "Antonym")
        {
            Antonym[] words = JsonHelper.FromJson<Antonym>(jsonResponse);

            // Build the result string
            string result = $"{callSynorAnt}s for '{inputField.text}':\n";
            foreach (Antonym word in words)
            {
                result += "- " + word.word + "\n";
            }

            // Update the TextMeshPro field with the result
            antResultText.text = result;
        }

    }


    // Define a class to map the JSON response
    [System.Serializable]
    public class Synonym
    {
        public string word;
    }
    [System.Serializable]
    public class Antonym
    {
        public string word;
    }

    // Helper class to handle JSON arrays (Unity's JsonUtility doesn't support them directly)
    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            string wrappedJson = "{\"Items\":" + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(wrappedJson);
            return wrapper.Items;
        }

        [System.Serializable]
        private class Wrapper<T>
        {
            public T[] Items;
        }
    }
}
