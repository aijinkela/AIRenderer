using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using FernGraph;
using Newtonsoft.Json;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SDGraphCore.StableDiffusionGraph
{
    [Node(Path = "Standard")]
    [Tags("SD Node")]
    public class SDUpdateModel : SDFlowNode, ICanExecuteSDFlow
    {
        [Input("Model")] public string Model;

        public override object OnRequestValue(Port port) => null;

        public override IEnumerator Execute()
        {
            Model = GetInputValue("Model", this.Model);
            SDUtil.Log(Model);
            yield return SetModelAsync(Model);
        }

        public override ICanExecuteSDFlow GetNext()
        {
            var port = GetPort("SDFlowOut");
            return port.ConnectedPorts.FirstOrDefault()?.Node as ICanExecuteSDFlow;
        }

        // <summary>
        /// Set a model to use by Stable Diffusion.
        /// </summary>
        /// <param name="modelName">Model to set</param>
        /// <returns></returns>
        public IEnumerator SetModelAsync(string modelName)
        {
            // Stable diffusion API url for setting a model
            string url = SDDataHandle.Instance.GetServerURL() + SDDataHandle.Instance.OptionAPI;

            // Load the list of models if not filled already
            if (string.IsNullOrEmpty(Model))
            {
                SDUtil.Log("Model is null");
                yield return null;
            }

            try
            {
                // Tell Stable Diffusion to use the specified model using an HTTP POST request
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                // add auth-header to request
                if (SDDataHandle.Instance.GetUseAuth() && !string.IsNullOrEmpty(SDDataHandle.Instance.GetUserName()) && !string.IsNullOrEmpty(SDDataHandle.Instance.GetPassword()))
                {
                    httpWebRequest.PreAuthenticate = true;
                    byte[] bytesToEncode = Encoding.UTF8.GetBytes(SDDataHandle.Instance.GetUserName() + ":" + SDDataHandle.Instance.GetPassword());
                    string encodedCredentials = Convert.ToBase64String(bytesToEncode);
                    httpWebRequest.Headers.Add("Authorization", "Basic " + encodedCredentials);
                }

                // Write to the stream the JSON parameters to set a model
                {
                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {
                        // Model to use
                        SDOption sd = new SDOption();
                        sd.sd_model_checkpoint = modelName;

                        // Serialize into a JSON string
                        string json = JsonConvert.SerializeObject(sd);

                        // Send the POST request to the server
                        streamWriter.Write(json);
                    }

                    // Get the response of the server
                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        string result = streamReader.ReadToEnd();
                        // We actually don't care about the response, we are not expecting anything particular
                        // We do this only to make sure we don't return from this function until the server has given a response (processed the request)
                    }
                }
            }
            catch (WebException e)
            {
                SDUtil.Log("Error: " + e.Message);
            }
        }
    }
}