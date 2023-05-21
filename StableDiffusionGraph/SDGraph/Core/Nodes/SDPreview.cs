using System;
using System.Collections;
using FernGraph;
using UnityEngine;

namespace FernNPRCore.StableDiffusionGraph
{
    [Node(Path = "SD Standard")]
    [Tags("SD Node")]
    public class SDPreview : Node
    {
        [Input("In Image")] public Texture2D Image;
        [Output("Out")] public Texture2D OutImage;
        public Action<Texture2D> OnUpdateAction;
        [Input("Seed", Editable = false)] public long seed;


        public override void OnValidate()
        {
            base.OnValidate();
            Image = GetInputValue("In Image", this.Image);
            OutImage = Image;
            seed = GetInputValue("Seed", this.seed);
            OnUpdateAction?.Invoke(Image);
        }
        
        public override object OnRequestValue(Port port)
        {
            Image = GetInputValue("In Image", this.Image);
            OutImage = Image;
            return OutImage;
        }
    }
}
