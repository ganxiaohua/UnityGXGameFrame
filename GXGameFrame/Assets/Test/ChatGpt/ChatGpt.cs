using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class ChatGpt : MonoBehaviour
{
    public string str;

    public class ChatGPTReq
    {
        public string model;
        public string prompt;
        public int max_tokens;
        public float temperature;
    }

    void Start()
    {
        SendRequest(str);
    }

    public async UniTask  SendRequest(string text)
    {
        string url = "https://api.openai.com/v1/completions";
        string apiKey = "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";
        string prompt = "User: " + text + " \n ChatGPT: ";
        string mode = "text-davinci-003";
        ChatGPTReq reqObj = new ChatGPTReq();
        reqObj.model = mode;
        reqObj.prompt = prompt;
        reqObj.max_tokens = 3072;
        reqObj.temperature = 0.6f;
        string requestBody = JsonUtility.ToJson(reqObj);
        UnityWebRequest request = UnityWebRequest.Post(url, requestBody);
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);
        request.SetRequestHeader("Content-Type", "application/json");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(requestBody);
        request.uploadHandler = (UploadHandler) new UploadHandlerRaw(jsonToSend);
        request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
        request.disposeDownloadHandlerOnDispose = true;
        request.disposeUploadHandlerOnDispose = true;
        
        await  request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log(request.downloadHandler.text);
            // 解析响应并处理 ChatGPT 生成的文本
        }
        else
        {
            Debug.Log(request.error);
        }
        // request.uploadHandler.Dispose();
        // request.downloadHandler.Dispose();
        request.Dispose();
    }
}