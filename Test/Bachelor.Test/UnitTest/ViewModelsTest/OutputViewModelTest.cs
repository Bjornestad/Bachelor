using System.Linq;
using Bachelor.Tools;
using Bachelor.ViewModels;
using Xunit;

namespace Bachelor.Test.UnitTest.ViewModelsTest
{
    public class OutputViewModelTest
    {
        [Fact]
        public void Log_AddsEntryToLogText()
        {
            var viewModel = new OutputViewModel(new ImmediateDispatcher());

            viewModel.Log("Pressing key: Space | Movement: MouthOpen");
            viewModel.Log("Releasing key: Space | Movement: MouthOpen");

            Assert.Contains("Pressing key: Space | Movement: MouthOpen", viewModel.LogText);
            Assert.Contains("Releasing key: Space | Movement: MouthOpen", viewModel.LogText);
            Assert.Equal(2, viewModel.LogEntries.Count);
        }

        [Fact]
        public void LogFormat_FormatsMessageCorrectly()
        {
            var viewModel = new OutputViewModel(new ImmediateDispatcher());

            viewModel.LogFormat("Value: {0}, Status: {1}", 42, "OK");

            Assert.Contains("Value: 42, Status: OK", viewModel.LogText);
            Assert.Single(viewModel.LogEntries);
        }

        [Fact]
        public void LogKeyEvent_FormatsKeyEventCorrectly()
        {
            var viewModel = new OutputViewModel(new ImmediateDispatcher());

            viewModel.LogKeyEvent("KeyDown", "Space", "Jump");

            Assert.Contains("KeyDown: Space | Movement: Jump", viewModel.LogText);
            Assert.Single(viewModel.LogEntries);
        }

        [Fact]
        public void Clear_RemovesAllEntries()
        {
            var viewModel = new OutputViewModel(new ImmediateDispatcher());
            viewModel.Log("First entry");
            viewModel.Log("Second entry");

            viewModel.Clear();

            Assert.Equal(string.Empty, viewModel.LogText);
            Assert.Empty(viewModel.LogEntries);
        }

        [Fact]
        public void LogEntries_LimitsNumberOfEntries()
        {
            var viewModel = new OutputViewModel(new ImmediateDispatcher());
            const int maxEntries = 400; // Same as in OutputViewModel
            const int extraEntries = 10;

            for (int i = 0; i < maxEntries + extraEntries; i++)
            {
                viewModel.Log($"Entry {i}");
            }

            Assert.Equal(maxEntries, viewModel.LogEntries.Count);
            Assert.DoesNotContain("Entry 0", viewModel.LogEntries.FirstOrDefault() ?? "");
            Assert.Contains($"Entry {maxEntries + extraEntries - 1}", viewModel.LogEntries.LastOrDefault() ?? "");
        }
    }
}