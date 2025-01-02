using osu.Framework.Testing;

namespace StoryBrew.Game.Tests.Visual
{
    public abstract partial class StoryBrewTestScene : TestScene
    {
        protected override ITestSceneTestRunner CreateRunner() => new StoryBrewTestSceneTestRunner();

        private partial class StoryBrewTestSceneTestRunner : StoryBrewGameBase, ITestSceneTestRunner
        {
            private TestSceneTestRunner.TestRunner runner;

            protected override void LoadAsyncComplete()
            {
                base.LoadAsyncComplete();
                Add(runner = new TestSceneTestRunner.TestRunner());
            }

            public void RunTestBlocking(TestScene test) => runner.RunTestBlocking(test);
        }
    }
}
