using FlaxEditor;
using FlaxEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollaboratePlugin
{
    public static class UserDrawer
    {
        private static readonly Dictionary<int, MaterialInstance> _materials = new Dictionary<int, MaterialInstance>();
        private static readonly Dictionary<int, Matrix> _positions = new Dictionary<int, Matrix>();
        private static Model _cameraModel;
        private static Material _cameraMaterial;
        private static readonly object locker = new object();
        public static void Initialize()
        {
            Editor.Instance.Windows.EditWin.Viewport.RenderTask.Draw += DrawPlayerHeads;
            // TODO: Handle this case if packed as plugin (might check if still works) ... probably switch to GUID
            _cameraMaterial = Content.LoadAsync<Material>(StringUtils.CombinePaths(Globals.ContentFolder, "M_RemoteCamera.flax"));
            _cameraModel = Content.LoadAsyncInternal<Model>("Editor/Camera/O_Camera");
            if (_cameraMaterial == null)
            {
                Editor.LogWarning("Failed to load camera material");
            }

            if(_cameraModel == null)
            {
                Editor.LogWarning("Failed to load camera model.");
            }
        }

        public static void ProcessPacket(UserPositionPacket upp)
        {
            lock (locker)
            {
                _positions[upp.Author] = Matrix.Transformation(Vector3.One, upp.Orientation * Quaternion.RotationY(-90 * Mathf.Deg2Rad), upp.Position);
            }
        }

        private static void DrawPlayerHeads(FlaxEngine.Rendering.DrawCallsCollector collector)
        {
            if (_cameraModel == null || _cameraModel.LoadedLODs == 0)
                return;

            lock (locker)
            {
                foreach (var item in _positions)
                {
                    // NOTE: Can't do this in ProcessPacket since that once doesn't run on Main or Render thread
                    if (!_materials.ContainsKey(item.Key))
                    {
                        _materials[item.Key] = _cameraMaterial.CreateVirtualInstance();
                        _materials[item.Key].GetParam("Color").Value = EditingSessionPlugin.Instance.Session.GetUserById(item.Key).SelectionColor;
                    }

                    var transform = item.Value;

                    collector.AddDrawCall(_cameraModel.LODs[0].Meshes[0], _materials[item.Key], ref transform, StaticFlags.None, false);
                }
            }
        }

        public static void Deinitialize()
        {
            Editor.Instance.Windows.EditWin.Viewport.RenderTask.Draw -= DrawPlayerHeads;

            Object.Destroy(_cameraModel);
            Object.Destroy(_cameraMaterial);

            for (int i = 0; i < _materials.Count; i++)
                Object.Destroy(_materials[i]);
            _materials.Clear();
        }
    }
}
