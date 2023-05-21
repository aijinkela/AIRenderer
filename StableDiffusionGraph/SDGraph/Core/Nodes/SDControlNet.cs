using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using FernGraph;
using Newtonsoft.Json;
using Unity.EditorCoroutines.Editor;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

namespace SDGraphCore.StableDiffusionGraph
{
    [Node(Path = "SD Standard")]
    [Tags("SD Node")]
    public class SDControlNet : Node
    {
        [Input("Image")] public Texture2D InputImage;

        [Output] public ControlNetData ControlNet;

        public string[] modelNames;
        public string[] moduleNames;
        public int currentIndex = 0;

        public override void OnAddedToGraph()
        {
            base.OnAddedToGraph();
            EditorCoroutineUtility.StartCoroutine(ListModelsAsync(), this);
            EditorCoroutineUtility.StartCoroutine(ListModulesAsync(), this);
        }

        public IEnumerator ListModelsAsync()
        {
            // Stable diffusion API url for getting the models list
            string url = SDDataHandle.Instance.GetServerURL() + SDDataHandle.Instance.ControlNetModelList;

            UnityWebRequest request = new UnityWebRequest(url, "GET");
            request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            if (SDDataHandle.Instance.GetUseAuth() && !string.IsNullOrEmpty(SDDataHandle.Instance.GetUserName()) && !string.IsNullOrEmpty(SDDataHandle.Instance.GetPassword()))
            {
                SDUtil.Log("Using API key to authenticate");
                byte[] bytesToEncode = Encoding.UTF8.GetBytes(SDDataHandle.Instance.GetUserName() + ":" + SDDataHandle.Instance.GetPassword());
                string encodedCredentials = Convert.ToBase64String(bytesToEncode);
                request.SetRequestHeader("Authorization", "Basic " + encodedCredentials);
            }
        
            yield return request.SendWebRequest();

            try
            {
                SDUtil.Log(request.downloadHandler.text);
                // Deserialize the response to a class
                ControlNetModel ms = JsonConvert.DeserializeObject<ControlNetModel>(request.downloadHandler.text);

                // Keep only the names of the models
                List<string> modelsNames = new List<string>();

                foreach (string m in ms.model_list) 
                    modelsNames.Add(m);

                // Convert the list into an array and store it for futur use
                modelNames = modelsNames.ToArray();
            }
            catch (Exception)
            {
                SDUtil.Log("Server needs and API key authentication. Please check your settings!");
            }
        }

        public IEnumerator ListModulesAsync()
        {
            // Stable diffusion API url for getting the models list
            string url = SDDataHandle.Instance.GetServerURL() + SDDataHandle.Instance.ControlNetMoudleList;

            UnityWebRequest request = new UnityWebRequest(url, "GET");
            request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            if (SDDataHandle.Instance.GetUseAuth() && !string.IsNullOrEmpty(SDDataHandle.Instance.GetUserName()) && !string.IsNullOrEmpty(SDDataHandle.Instance.GetPassword()))
            {
                SDUtil.Log("Using API key to authenticate");
                byte[] bytesToEncode = Encoding.UTF8.GetBytes(SDDataHandle.Instance.GetUserName() + ":" + SDDataHandle.Instance.GetPassword());
                string encodedCredentials = Convert.ToBase64String(bytesToEncode);
                request.SetRequestHeader("Authorization", "Basic " + encodedCredentials);
            }
        
            yield return request.SendWebRequest();

            try
            {
                SDUtil.Log(request.downloadHandler.text);
                // Deserialize the response to a class
                ControlNetMoudle ms = JsonConvert.DeserializeObject<ControlNetMoudle>(request.downloadHandler.text);

                // Keep only the names of the models
                List<string> modulesNames = new List<string>();

                foreach (string m in ms.module_list) 
                    modulesNames.Add(m);

                // Convert the list into an array and store it for futur use
                moduleNames = modulesNames.ToArray();
            }
            catch (Exception)
            {
                SDUtil.Log("Server needs and API key authentication. Please check your settings!");
            }
        }


        public SDControlNet()
        {
            ControlNet = new ControlNetData();
        }

        // public override IEnumerator Execute()
        // {
        //     InputImage = GetInputValue("Image", this.InputImage);
        //     yield return null;
        // }

        public override object OnRequestValue(Port port)
        {
            if(ControlNet == null) ControlNet = new ControlNetData();
            var InputImage = GetInputValue("Image", this.InputImage);
            
            byte[] inputImgBytes = InputImage.EncodeToPNG();
            string inputImgString = Convert.ToBase64String(inputImgBytes);
            ControlNet.input_image = inputImgString;
            return ControlNet;
        }
    }
}