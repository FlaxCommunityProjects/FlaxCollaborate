using System;
using System.Collections.Generic;
using FlaxEditor;
using FlaxEditor.Gizmo;
using FlaxEditor.Options;
using FlaxEditor.SceneGraph;
using FlaxEngine;
using FlaxEngine.Rendering;

namespace MultiUsersEditingPlugin
{
    public class SessionSelectionOutline : PostProcessEffect
    {
        private Material _outlineMaterial;
        private MaterialInstance _material;
        private Color _color0, _color1;
        private bool _enabled;

        protected List<Actor> _actors;

        public int UserId;
        
        public SessionSelectionOutline(int id)
        {
            UserId = id;
            
            _outlineMaterial = Content.LoadAsyncInternal<Material>("Editor/Gizmo/SelectionOutlineMaterial");
            if (_outlineMaterial)
            {
                _material = _outlineMaterial.CreateVirtualInstance();
            }
            else
            {
                Editor.LogWarning("Failed to load gizmo selection outline material");
            }

            _enabled = Editor.Instance.Options.Options.Visual.ShowSelectionOutline;
            _color0 = EditingSessionPlugin.Instance.Session.GetUserById(UserId).SelectionColor;
            _color1 = Editor.Instance.Options.Options.Visual.SelectionOutlineColor1;
        }

        public override bool CanRender => _enabled && _material && _outlineMaterial.IsLoaded && EditingSessionPlugin.Instance.Session.GetUserById(UserId).Selection.Length > 0;

        public override void Render(GPUContext context, SceneRenderTask task, RenderTarget input, RenderTarget output)
        {
            Profiler.BeginEventGPU("Session Selection Outline");

            // Pick a temporary depth buffer
            var customDepth = RenderTarget.GetTemporary(PixelFormat.R32_Typeless, input.Width, input.Height, TextureFlags.DepthStencil | TextureFlags.ShaderResource);
            context.ClearDepth(customDepth);

            // Draw objects to depth buffer
            if (_actors == null)
                _actors = new List<Actor>();
            else
                _actors.Clear();
            DrawSelectionDepth(context, task, customDepth);
            _actors.Clear();

            var near = task.View.Near;
            var far = task.View.Far;
            var projection = task.View.Projection;

            // Render outline
            _material.GetParam("OutlineColor0").Value = _color0;
            _material.GetParam("OutlineColor1").Value = _color1;
            _material.GetParam("CustomDepth").Value = customDepth;
            _material.GetParam("ViewInfo").Value = new Vector4(1.0f / projection.M11, 1.0f / projection.M22, far / (far - near), (-far * near) / (far - near) / far);
            context.DrawPostFxMaterial(_material, output, input, task);

            // Cleanup
            RenderTarget.ReleaseTemporary(customDepth);

            Profiler.EndEventGPU();
        }

        /// <summary>
        /// Draws the selected object to depth buffer.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="task">The task.</param>
        /// <param name="customDepth">The custom depth (output).</param>
        protected virtual void DrawSelectionDepth(GPUContext context, SceneRenderTask task, RenderTarget customDepth)
        {
            // Get selected actors
            var selection = EditingSessionPlugin.Instance.Session.GetUserById(UserId).Selection;
            Debug.Log("Draw depth selec lenght " + selection.Length);
            _actors.Capacity = Mathf.NextPowerOfTwo(Mathf.Max(_actors.Capacity, selection.Length));
            for (int i = 0; i < selection.Length; i++)
            {
                if (selection[i] is ActorNode actorNode)
                    _actors.Add(actorNode.Actor);
            }

            // Render selected objects depth
            context.DrawSceneDepth(task, customDepth, true, _actors, ActorsSources.CustomActors);
        }
    }
}