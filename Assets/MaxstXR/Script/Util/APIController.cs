﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using JsonFx.Json;
using System.Text;
using System.IO;

namespace maxstAR
{
    public class APIController : MonoBehaviour
    {

        static internal IEnumerator GET(string url, Dictionary<string, string> headers,  int timeOut, System.Action<string> completed)
        {
            UnityWebRequest www = UnityWebRequest.Get(url);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            foreach (KeyValuePair<string, string> header in headers)
            {
                string headerKey = Escape(header.Key);
                string headerValue = Escape(header.Value);
                www.SetRequestHeader(headerKey, headerValue);
            }

            if (timeOut > 0)
            {
                www.timeout = timeOut;
            }

            yield return www.SendWebRequest();

            if (www.error != null)
            {
                completed(www.error);
            }
            else
            {
                completed(www.downloadHandler.text);
            }
        }
        static internal IEnumerator POST(string url, Dictionary<string, string> headers, Dictionary<string, string> parameters, int timeOut, System.Action<string> completed)
        {
            UploadHandler uploadHandler = null;
            WWWForm form = new WWWForm();

            if (headers.ContainsKey("Content-Type"))
            {
                if (headers["Content-Type"] == "application/json")
                {
                    string jsonData = JsonWriter.Serialize(parameters);
                    byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                    uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
                }
                else
                {
                    form = new WWWForm();
                    foreach (KeyValuePair<string, string> parameter in parameters)
                    {
                        form.AddField(parameter.Key, parameter.Value);
                    }
                }
            }

            UnityWebRequest www = UnityWebRequest.Post(url, form);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            www.uploadHandler = uploadHandler;

            foreach (KeyValuePair<string, string> header in headers)
            {
                string headerKey = Escape(header.Key);
                string headerValue = Escape(header.Value);
                www.SetRequestHeader(headerKey, headerValue);
            }

#if UNITY_2017_3_OR_NEWER
            if (timeOut > 0)
            {
                www.timeout = timeOut;
            }

            yield return www.SendWebRequest();

            if (www.error != null && www.error != "")
            {
                completed(www.error);
            }
            else
            {
                completed(www.downloadHandler.text);
            }
#else
            yield return www.Send();

            if (www.isNetworkError)
            {
                completed(www.error);
            }
            else
            {
                completed(www.downloadHandler.text);
            }
#endif
        }

        static string Escape(string text)
        {
            text = text.Replace("(", "&#40");
            text = text.Replace(")", "&#41");
            return text;
        }
    }
}
