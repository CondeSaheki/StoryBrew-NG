﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Game.Database;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play;
using osuTK;

namespace osu.Game.StoryboardsNG.Drawables
{
    public partial class DrawableStoryboard : Container<DrawableStoryboardLayer>
    {
        [Cached(typeof(Storyboard))]
        public Storyboard Storyboard { get; }

        /// <summary>
        /// Whether the storyboard is considered finished.
        /// </summary>
        public IBindable<bool> HasStoryboardEnded => hasStoryboardEnded;

        private readonly BindableBool hasStoryboardEnded = new BindableBool(true);

        protected override Container<DrawableStoryboardLayer> Content { get; }

        protected override Vector2 DrawScale => new Vector2(Parent!.DrawHeight / 480);

        public override bool RemoveCompletedTransforms => false;

        private double? lastEventEndTime;

        [Cached(typeof(IReadOnlyList<Mod>))]
        public IReadOnlyList<Mod> Mods { get; }

        [Resolved]
        private GameHost host { get; set; } = null!;

        // [Resolved]
        // private RealmAccess realm { get; set; } = null!;

        private DependencyContainer dependencies = null!;

        private BindableNumber<double> health = null!;
        private readonly BindableBool passing = new BindableBool(true);

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        public string MapsetPath { get; }

        public DrawableStoryboard(Storyboard storyboard, string mapsetPath, IReadOnlyList<Mod>? mods = null)
        {
            MapsetPath = mapsetPath;
            Storyboard = storyboard;
            Mods = mods ?? [];

            Size = new Vector2(640, 480);

            bool onlyHasVideoElements = Storyboard.Layers.SelectMany(l => l.Elements).All(e => e is StoryboardVideo);

            Width = Height * (storyboard.Beatmap.WidescreenStoryboard || onlyHasVideoElements ? 16 / 9f : 4 / 3f);

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            AddInternal(Content = new Container<DrawableStoryboardLayer>
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
        }

        [BackgroundDependencyLoader]
        private void load(IGameplayClock? clock, CancellationToken? cancellationToken, GameplayState? gameplayState)
        {
            if (clock != null)
                Clock = clock;

            dependencies.CacheAs(typeof(TextureStore),
                new TextureStore(host.Renderer, host.CreateTextureLoaderStore(
                    CreateResourceLookupStore()
                ), false, scaleAdjust: 1));

            foreach (var layer in Storyboard.Layers)
            {
                cancellationToken?.ThrowIfCancellationRequested();

                Add(layer.CreateDrawable());
            }

            lastEventEndTime = Storyboard.LatestEventTime;

            health = gameplayState?.HealthProcessor.Health.GetBoundCopy() ?? new BindableDouble(1);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            health.BindValueChanged(val => passing.Value = val.NewValue >= 0.5, true);
            passing.BindValueChanged(_ => updateLayerVisibility(), true);
        }

        protected virtual IResourceStore<byte[]> CreateResourceLookupStore() => new StorageBackedResourceStoreTest(MapsetPath);

        protected override void Update()
        {
            base.Update();
            hasStoryboardEnded.Value = lastEventEndTime == null || Time.Current >= lastEventEndTime;
        }

        public DrawableStoryboardLayer OverlayLayer => Children.Single(layer => layer.Name == "Overlay");

        private void updateLayerVisibility()
        {
            foreach (var layer in Children)
                layer.Enabled = passing.Value ? layer.Layer.VisibleWhenPassing : layer.Layer.VisibleWhenFailing;
        }

        private class StoryboardResourceLookupStoreRE : IResourceStore<byte[]>
        {
            private readonly IResourceStore<byte[]> store;

            public StoryboardResourceLookupStoreRE(GameHost host, string mapsetPath)
            {
                if (!Directory.Exists(mapsetPath)) throw new FileNotFoundException($"Mapset path '{mapsetPath}' not found", mapsetPath);
                var native = new NativeStorage(mapsetPath, host);
                store = new StorageBackedResourceStore(native);
            }

            public void Dispose()
                => store.Dispose();

            public byte[] Get(string name)
                => store.Get(name.Split('$').Last().Replace("\\", "/"));

            public Task<byte[]> GetAsync(string name, CancellationToken cancellationToken = default)
                => store.GetAsync(name.Split('$').Last().Replace("\\", "/"), cancellationToken);

            public IEnumerable<string> GetAvailableResources()
                => store.GetAvailableResources();

            public Stream GetStream(string name)
                => store.GetStream(name.Split('$').Last().Replace("\\", "/"));
        }

        private class StorageBackedResourceStoreTest : IResourceStore<byte[]>
        {
            private readonly string basePath;

            public StorageBackedResourceStoreTest(string basePath)
            {
                if (!Directory.Exists(basePath))
                    throw new DirectoryNotFoundException($"The folder {basePath} does not exist.");

                this.basePath = basePath;
            }

            public void Dispose() { } // store.Dispose()

            public byte[] Get(string name)
            {
                string filePath = Path.Combine(basePath, name);
                return File.Exists(filePath) ? File.ReadAllBytes(filePath) : null!;
            }

            public Stream? GetStream(string name)
            {
                string filePath = Path.Combine(basePath, name);
                return File.Exists(filePath) ? File.OpenRead(filePath) : null;
            }

            public Task<byte[]> GetAsync(string name, CancellationToken cancellationToken = default)
            {
                string filePath = Path.Combine(basePath, name);
                return File.Exists(filePath) ? Task.FromResult(File.ReadAllBytes(filePath)) : Task.FromResult<byte[]>(null!);
            }

            public IEnumerable<string> GetAvailableResources() =>
                Directory.EnumerateFiles(basePath).Select(path => Path.GetFileName(path) ?? string.Empty);

        }

        // private class StoryboardResourceLookupStore : IResourceStore<byte[]>
        // {
        //     private readonly IResourceStore<byte[]> realmFileStore;
        //     private readonly Storyboard storyboard;

        //     public StoryboardResourceLookupStore(Storyboard storyboard, RealmAccess realm, GameHost host)
        //     {
        //         realmFileStore = new RealmFileStore(realm, host.Storage).Store;
        //         this.storyboard = storyboard;
        //     }

        //     public void Dispose() =>
        //         realmFileStore.Dispose();

        //     public byte[] Get(string name)
        //     {
        //         string? storagePath = storyboard.GetStoragePathFromStoryboardPath(name);

        //         return string.IsNullOrEmpty(storagePath)
        //             ? null!
        //             : realmFileStore.Get(storagePath);
        //     }

        //     public Task<byte[]> GetAsync(string name, CancellationToken cancellationToken = new CancellationToken())
        //     {
        //         string? storagePath = storyboard.GetStoragePathFromStoryboardPath(name);

        //         return string.IsNullOrEmpty(storagePath)
        //             ? Task.FromResult<byte[]>(null!)
        //             : realmFileStore.GetAsync(storagePath, cancellationToken);
        //     }

        //     public Stream? GetStream(string name)
        //     {
        //         string? storagePath = storyboard.GetStoragePathFromStoryboardPath(name);

        //         return string.IsNullOrEmpty(storagePath)
        //             ? null
        //             : realmFileStore.GetStream(storagePath);
        //     }

        //     public IEnumerable<string> GetAvailableResources() =>
        //         realmFileStore.GetAvailableResources();
        // }
    }
}
