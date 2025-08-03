using Terminal.Gui;

namespace Smalledit
{
    /// <summary>
    /// Manages text search functionality with highlighting and navigation
    /// </summary>
    public class SearchManager
    {
        private string lastSearchTerm = "";
        private int lastSearchIndex = -1;
        private TextView textView;

        /// <summary>
        /// Initializes search manager for the specified text view
        /// </summary>
        /// <param name="textView">Text view to perform searches on</param>
        public SearchManager(TextView textView)
        {
            this.textView = textView;
        }

        /// <summary>
        /// Shows the find dialog and initiates search if term is entered
        /// </summary>
        public void ShowFindDialog()
        {
            var findDialog = new FindDialog(lastSearchTerm);
            Application.Run(findDialog);

            if (findDialog.Result == FindDialog.FindResult.Find)
            {
                var searchTerm = findDialog.SearchTerm;
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    lastSearchTerm = searchTerm;
                    lastSearchIndex = -1;
                    FindNext();
                }
            }
        }

        /// <summary>
        /// Finds the next occurrence of the last search term
        /// </summary>
        public void FindNext()
        {
            if (string.IsNullOrEmpty(lastSearchTerm))
            {
                ShowFindDialog();
                return;
            }

            var text = textView.Text?.ToString() ?? "";
            if (string.IsNullOrEmpty(text))
            {
                MessageBox.Query("Find", "No text to search", "OK");
                return;
            }

            int startIndex = lastSearchIndex + 1;
            int foundIndex = text.IndexOf(lastSearchTerm, startIndex, StringComparison.OrdinalIgnoreCase);

            if (foundIndex == -1)
            {
                foundIndex = text.IndexOf(lastSearchTerm, 0, StringComparison.OrdinalIgnoreCase);
                if (foundIndex == -1)
                {
                    MessageBox.Query("Find", $"'{lastSearchTerm}' not found", "OK");
                    return;
                }
            }

            lastSearchIndex = foundIndex;

            MoveCursorToPosition(foundIndex);
        }

        /// <summary>
        /// Moves cursor to specified character position in text
        /// </summary>
        /// <param name="position">Character position to move cursor to</param>
        private void MoveCursorToPosition(int position)
        {
            try
            {
                var text = textView.Text?.ToString() ?? "";

                int line = 0;
                int column = 0;

                for (int i = 0; i < position && i < text.Length; i++)
                {
                    if (text[i] == '\n')
                    {
                        line++;
                        column = 0;
                    }
                    else
                    {
                        column++;
                    }
                }

                try
                {
                    var cursorPositionProperty = typeof(TextView).GetProperty("CursorPosition");
                    if (cursorPositionProperty != null)
                    {
                        var point = new Point(column, line);
                        cursorPositionProperty.SetValue(textView, point);
                    }
                }
                catch
                {
                    var topRowProperty = typeof(TextView).GetProperty("TopRow");
                    topRowProperty?.SetValue(textView, Math.Max(0, line - 5));
                }

                textView.SetNeedsDisplay();
            }
            catch
            {
                MessageBox.Query("Found", $"Found '{lastSearchTerm}' at position {position}", "OK");
            }
        }
    }
}