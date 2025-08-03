using Terminal.Gui;

namespace Smalledit
{
    /// <summary>
    /// Status bar displaying file info, cursor position, and text statistics
    /// </summary>
    public class BottomStatusBar : View
    {
        private readonly Label statusLabel;
        private string fileName = "";
        private bool isModified = false;
        private int currentLine = Utils.TextEditor.DEFAULT_LINE_COUNT;
        private int currentColumn = Utils.TextEditor.DEFAULT_LINE_COUNT;
        private int totalCharacters = Utils.TextEditor.DEFAULT_CHAR_COUNT;
        private int totalLines = Utils.TextEditor.DEFAULT_LINE_COUNT;
        private int totalWords = Utils.TextEditor.DEFAULT_WORD_COUNT;

        /// <summary>
        /// Initializes a new status bar at the bottom of the window
        /// </summary>
        public BottomStatusBar()
        {
            Height = Utils.Layout.STATUS_BAR_HEIGHT;
            Width = Dim.Fill();

            statusLabel = new Label()
            {
                X = Utils.Layout.LINE_NUMBERS_X,
                Y = Utils.Layout.LINE_NUMBERS_X,
                Width = Dim.Fill(),
                Height = Utils.Layout.STATUS_BAR_HEIGHT,
                TextAlignment = TextAlignment.Left
            };

            Add(statusLabel);
            UpdateStatusText();
        }

        /// <summary>
        /// Updates the file information display
        /// </summary>
        /// <param name="fileName">Name of the current file</param>
        /// <param name="isModified">Whether the file has unsaved changes</param>
        public void UpdateFileInfo(string fileName, bool isModified)
        {
            this.fileName = fileName ?? "";
            this.isModified = isModified;
            UpdateStatusText();
        }

        /// <summary>
        /// Updates the cursor position display
        /// </summary>
        /// <param name="line">Current line number</param>
        /// <param name="column">Current column number</param>
        public void UpdatePosition(int line, int column)
        {
            currentLine = Math.Max(Utils.TextEditor.DEFAULT_LINE_COUNT, line);
            currentColumn = Math.Max(Utils.TextEditor.DEFAULT_LINE_COUNT, column);
            UpdateStatusText();
        }

        /// <summary>
        /// Updates text statistics (characters, words, lines)
        /// </summary>
        /// <param name="text">Text to analyze for statistics</param>
        public void UpdateTextStats(string text)
        {
            var (characters, lines, words) = Utils.CalculateTextStats(text);
            totalCharacters = characters;
            totalLines = lines;
            totalWords = words;
            UpdateStatusText();
        }

        /// <summary>
        /// Updates text statistics with pre-calculated values
        /// </summary>
        /// <param name="characters">Character count</param>
        /// <param name="lines">Line count</param>
        /// <param name="words">Word count</param>
        public void UpdateTextStats(int characters, int lines, int words)
        {
            totalCharacters = characters;
            totalLines = lines;
            totalWords = words;
            UpdateStatusText();
        }

        /// <summary>
        /// Updates the complete status bar text display
        /// </summary>
        private void UpdateStatusText()
        {
            var modifiedIndicator = isModified ? "*" : "";
            var fileNameDisplay = Utils.GetDisplayFileName(fileName);

            var statusText = $"{fileNameDisplay}{modifiedIndicator} | " +
                           $"Ln {currentLine}, Col {currentColumn} | " +
                           $"{totalCharacters} chars, {totalWords} words, {totalLines} lines";

            statusLabel.Text = statusText;
            SetNeedsDisplay();
        }

        /// <summary>
        /// Sets the color scheme for the status bar
        /// </summary>
        /// <param name="colorScheme">Color scheme to apply</param>
        public void SetColorScheme(ColorScheme colorScheme)
        {
            ColorScheme = colorScheme;
            statusLabel.ColorScheme = colorScheme;
        }
    }
}
