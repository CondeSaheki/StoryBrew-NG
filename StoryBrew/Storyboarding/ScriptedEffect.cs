using StoryBrew.Common.Scripting;
using StoryBrew.Scripting;
using System;

namespace StoryBrew.Storyboarding
{
    public class ScriptedEffect : Effect
    {
        private readonly ScriptContainer<StoryboardObjectGenerator> ScriptContainer;
        private string? configScriptIdentifier;
        private bool beatmapDependant = true;

        public new bool Multithreaded = false;

        public override bool BeatmapDependant => beatmapDependant;
        public override string BaseName => ScriptContainer?.Name ?? string.Empty;
        public override string Path => ScriptContainer?.MainSourcePath ?? string.Empty;

        public ScriptedEffect(Project project, ScriptContainer<StoryboardObjectGenerator> scriptContainer, bool multithreaded = false) : base(project)
        {
            ScriptContainer = scriptContainer;
            Refresh();
            Multithreaded = multithreaded;
        }

        public override void Update()
        {
            if (!ScriptContainer.HasScript) return;

            var context = new EditorGeneratorContext(this, Project.ProjectFolderPath, Project.ProjectAssetFolderPath, Project.MapsetPath, Project.MainBeatmap, Project.MapsetManager.Beatmaps);
            try
            {
                var script = ScriptContainer.CreateScript();

                beatmapDependant = true;
                if (script.Identifier != configScriptIdentifier)
                {
                    script.UpdateConfiguration(Config);
                    configScriptIdentifier = script.Identifier;
                }
                else script.ApplyConfiguration(Config);
                
                script.Generate(context);
                foreach (var layer in context.EditorLayers)
                    layer.PostProcess();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Script load failed for {BaseName}\n{ex}");
                return;
            }
            finally
            {
                context.DisposeResources();
            }

            Multithreaded = context.Multithreaded;
            beatmapDependant = context.BeatmapDependent;

            UpdateLayers(context.EditorLayers);
        }

        public new void Dispose()
        {
            base.Dispose();
        }
    }
}
