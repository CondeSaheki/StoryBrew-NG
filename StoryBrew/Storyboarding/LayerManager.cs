using StoryBrew.Common.Storyboarding;

namespace StoryBrew.Storyboarding
{
    public class LayerManager
    {
        private readonly List<EditorStoryboardLayer> layers = new List<EditorStoryboardLayer>();

        public int LayersCount => layers.Count;
        public IEnumerable<EditorStoryboardLayer> Layers => layers;
        public List<EditorStoryboardLayer> FindLayers(Predicate<EditorStoryboardLayer> predicate) => layers.FindAll(predicate);

        public void Add(EditorStoryboardLayer layer)
        {
            layers.Insert(findLayerIndex(layer), layer);
        }

        public void Replace(EditorStoryboardLayer oldLayer, EditorStoryboardLayer newLayer)
        {
            var index = layers.IndexOf(oldLayer);
            if (index != -1)
            {
                newLayer.CopySettings(oldLayer, copyGuid: true);
                layers[index] = newLayer;
            }
            else throw new InvalidOperationException($"Cannot replace layer '{oldLayer.Name}' with '{newLayer.Name}', old layer not found");
        }

        public void Replace(List<EditorStoryboardLayer> oldLayers, List<EditorStoryboardLayer> newLayers)
        {
            oldLayers = new List<EditorStoryboardLayer>(oldLayers);
            foreach (var newLayer in newLayers)
            {
                var oldLayer = oldLayers.Find(l => l.Identifier == newLayer.Identifier);
                if (oldLayer != null)
                {
                    var index = layers.IndexOf(oldLayer);
                    if (index != -1)
                    {
                        newLayer.CopySettings(layers[index], copyGuid: true);
                        layers[index] = newLayer;
                    }
                    oldLayers.Remove(oldLayer);
                }
                else layers.Insert(findLayerIndex(newLayer), newLayer);
            }
            foreach (var oldLayer in oldLayers)
            {
                layers.Remove(oldLayer);
            }
        }

        public void Replace(EditorStoryboardLayer oldLayer, List<EditorStoryboardLayer> newLayers)
        {
            var index = layers.IndexOf(oldLayer);
            if (index != -1)
            {
                foreach (var newLayer in newLayers)
                {
                    newLayer.CopySettings(oldLayer, copyGuid: false);
                }
                layers.InsertRange(index, newLayers);
                layers.Remove(oldLayer);
            }
            else throw new InvalidOperationException($"Cannot replace layer '{oldLayer.Name}' with multiple layers, old layer not found");
        }

        public void Remove(EditorStoryboardLayer layer)
        {
            layers.Remove(layer);
        }

        public bool MoveUp(EditorStoryboardLayer layer)
        {
            var index = layers.IndexOf(layer);
            if (index != -1)
            {
                if (index > 0 && layer.CompareTo(layers[index - 1]) == 0)
                {
                    var otherLayer = layers[index - 1];
                    layers[index - 1] = layer;
                    layers[index] = otherLayer;
                }
                else return false;
            }
            else throw new InvalidOperationException($"Cannot move layer '{layer.Name}'");
            return true;
        }

        public bool MoveDown(EditorStoryboardLayer layer)
        {
            var index = layers.IndexOf(layer);
            if (index != -1)
            {
                if (index < layers.Count - 1 && layer.CompareTo(layers[index + 1]) == 0)
                {
                    var otherLayer = layers[index + 1];
                    layers[index + 1] = layer;
                    layers[index] = otherLayer;
                }
                else return false;
            }
            else throw new InvalidOperationException($"Cannot move layer '{layer.Name}'");
            return true;
        }

        public bool MoveToTop(EditorStoryboardLayer layer)
        {
            var index = layers.IndexOf(layer);
            if (index != -1)
            {
                if (index == 0) return false;
                while (index > 0 && layer.CompareTo(layers[index - 1]) == 0)
                {
                    var otherLayer = layers[index - 1];
                    layers[index - 1] = layer;
                    layers[index] = otherLayer;
                    --index;
                }
            }
            else throw new InvalidOperationException($"Cannot move layer '{layer.Name}'");
            return true;
        }

        public bool MoveToBottom(EditorStoryboardLayer layer)
        {
            var index = layers.IndexOf(layer);
            if (index != -1)
            {
                if (index == layers.Count - 1) return false;
                while (index < layers.Count - 1 && layer.CompareTo(layers[index + 1]) == 0)
                {
                    var otherLayer = layers[index + 1];
                    layers[index + 1] = layer;
                    layers[index] = otherLayer;
                    ++index;
                }
            }
            else throw new InvalidOperationException($"Cannot move layer '{layer.Name}'");
            return true;
        }

        public void MoveToOsbLayer(EditorStoryboardLayer layer, OsbLayer osbLayer)
        {
            var firstLayer = layers.FirstOrDefault(l => l.OsbLayer == osbLayer);
            if (firstLayer != null)
                MoveToLayer(layer, firstLayer);
            else layer.OsbLayer = osbLayer;
        }

        public void MoveToLayer(EditorStoryboardLayer layerToMove, EditorStoryboardLayer toLayer)
        {
            layerToMove.OsbLayer = toLayer.OsbLayer;

            var fromIndex = layers.IndexOf(layerToMove);
            var toIndex = layers.IndexOf(toLayer);
            if (fromIndex != -1 && toIndex != -1)
            {
                Move(layers, fromIndex, toIndex);
                sortLayer(layerToMove);
            }
            else throw new InvalidOperationException($"Cannot move layer '{layerToMove.Name}' to the position of '{layerToMove.Name}'");
        }

        public void TriggerEvents(double startTime, double endTime)
        {
            foreach (var layer in Layers)
                layer.TriggerEvents(startTime, endTime);
        }

        // public void Draw(DrawContext drawContext, Camera camera, Box2 bounds, float opacity, FrameStats frameStats)
        // {
        //     foreach (var layer in Layers)
        //         layer.Draw(drawContext, camera, bounds, opacity, frameStats);
        // }

        private void sortLayer(EditorStoryboardLayer layer)
        {
            var initialIndex = layers.IndexOf(layer);
            if (initialIndex < 0) new InvalidOperationException($"Layer '{layer.Name}' cannot be found");

            var newIndex = initialIndex;
            while (newIndex > 0 && layer.CompareTo(layers[newIndex - 1]) < 0) newIndex--;
            while (newIndex < layers.Count - 1 && layer.CompareTo(layers[newIndex + 1]) > 0) newIndex++;

            Move(layers, initialIndex, newIndex);
        }

        private int findLayerIndex(EditorStoryboardLayer layer)
        {
            var index = layers.BinarySearch(layer);
            if (index >= 0)
            {
                while (index < layers.Count && layer.CompareTo(layers[index]) == 0)
                    index++;
                return index;
            }
            else return ~index;
        }

        public static void Move<T>(List<T> list, int oldIndex, int newIndex)
        {
            if (oldIndex < 0 || oldIndex >= list.Count || newIndex < 0 || newIndex >= list.Count)
                throw new ArgumentOutOfRangeException();

            var item = list[oldIndex];
            list.RemoveAt(oldIndex);
            list.Insert(newIndex, item);
        }
    }
}