using Terminal.Gui;

namespace Smalledit
{
    /// <summary>
    /// Modal dialog for entering search terms
    /// </summary>
    public class FindDialog : Dialog
    {
        /// <summary>
        /// Result of the find dialog interaction
        /// </summary>
        public enum FindResult
        {
            Find,
            Cancel
        }

        /// <summary>
        /// Gets the dialog result after closing
        /// </summary>
        public FindResult Result { get; private set; } = FindResult.Cancel;
        /// <summary>
        /// Gets the search term entered by the user
        /// </summary>
        public string SearchTerm { get; private set; } = "";

        /// <summary>
        /// Initializes a new find dialog with optional initial search term
        /// </summary>
        /// <param name="initialSearchTerm">Initial text to populate in search field</param>
        public FindDialog(string initialSearchTerm = "") : base("Find", 60, 8)
        {
            var searchLabel = new Label()
            {
                Text = "Search for:",
                X = 1,
                Y = 1
            };

            var searchField = new TextField()
            {
                Text = initialSearchTerm,
                X = 1,
                Y = 2,
                Width = Dim.Fill() - 2
            };

            var findButton = new Button()
            {
                Text = "Find",
                X = 1,
                Y = 4,
                IsDefault = true
            };

            var cancelButton = new Button()
            {
                Text = "Cancel",
                X = Pos.Right(findButton) + 2,
                Y = 4
            };

            findButton.Clicked += () =>
            {
                SearchTerm = searchField.Text?.ToString() ?? "";
                Result = FindResult.Find;
                Application.RequestStop();
            };

            cancelButton.Clicked += () =>
            {
                Result = FindResult.Cancel;
                Application.RequestStop();
            };

            Add(searchLabel, searchField, findButton, cancelButton);
        }
    }
}