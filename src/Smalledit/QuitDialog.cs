using Terminal.Gui;

namespace Smalledit
{
    /// <summary>
    /// Modal dialog for handling quit with unsaved changes
    /// </summary>
    public class QuitDialog : Dialog
    {
        /// <summary>
        /// Result of the quit dialog interaction
        /// </summary>
        public enum QuitResult
        {
            Save,
            QuitWithoutSaving,
            Cancel
        }

        /// <summary>
        /// Gets the dialog result after closing
        /// </summary>
        public QuitResult Result { get; private set; } = QuitResult.Cancel;

        /// <summary>
        /// Initializes a new quit confirmation dialog
        /// </summary>
        public QuitDialog() : base("Do you really want to quit?", Utils.Layout.QUIT_DIALOG_WIDTH, Utils.Layout.QUIT_DIALOG_HEIGHT)
        {
            var messageLabel = new Label()
            {
                Text = "You are about to quit with unsaved changes. Do you want to save?",
                X = Utils.Layout.DIALOG_CONTENT_X,
                Y = Utils.Layout.DIALOG_CONTENT_Y
            };

            var saveButton = new Button()
            {
                Text = "Save",
                X = Utils.Layout.DIALOG_CONTENT_X,
                Y = Utils.Layout.DIALOG_BUTTON_Y,
                IsDefault = true
            };

            var quitWithoutSaveButton = new Button()
            {
                Text = "Quit Without Saving",
                X = Pos.Right(saveButton) + Utils.Layout.BUTTON_SPACING,
                Y = Utils.Layout.DIALOG_BUTTON_Y
            };

            var cancelButton = new Button()
            {
                Text = "Cancel",
                X = Pos.Right(quitWithoutSaveButton) + Utils.Layout.BUTTON_SPACING,
                Y = Utils.Layout.DIALOG_BUTTON_Y
            };

            saveButton.Clicked += () =>
            {
                Result = QuitResult.Save;
                Application.RequestStop();
            };

            quitWithoutSaveButton.Clicked += () =>
            {
                Result = QuitResult.QuitWithoutSaving;
                Application.RequestStop();
            };

            cancelButton.Clicked += () =>
            {
                Result = QuitResult.Cancel;
                Application.RequestStop();
            };

            Add(messageLabel, saveButton, quitWithoutSaveButton, cancelButton);
        }
    }
}