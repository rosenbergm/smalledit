using Terminal.Gui;

namespace Smalledit
{
    /// <summary>
    /// Manages file operations and tracks file modification state
    /// </summary>
    public class FileManager
    {
        /// <summary>
        /// Gets the current file path being edited
        /// </summary>
        public string CurrentFilePath { get; private set; } = "";
        /// <summary>
        /// Gets whether the current file has unsaved changes
        /// </summary>
        public bool HasUnsavedChanges { get; private set; } = false;

        /// <summary>
        /// Event raised when file state changes (path or modification status)
        /// </summary>
        public event Action? FileStateChanged;

        /// <summary>
        /// Marks the current file as having unsaved changes
        /// </summary>
        public void MarkAsModified()
        {
            if (!HasUnsavedChanges)
            {
                HasUnsavedChanges = true;
                FileStateChanged?.Invoke();
            }
        }

        /// <summary>
        /// Marks the current file as saved (no unsaved changes)
        /// </summary>
        public void MarkAsSaved()
        {
            if (HasUnsavedChanges)
            {
                HasUnsavedChanges = false;
                FileStateChanged?.Invoke();
            }
        }

        /// <summary>
        /// Resets state for creating a new file
        /// </summary>
        public void NewFile()
        {
            CurrentFilePath = "";
            HasUnsavedChanges = false;
            FileStateChanged?.Invoke();
        }

        /// <summary>
        /// Opens a file using file dialog and returns its content
        /// </summary>
        /// <returns>File content if successful, null if cancelled or failed</returns>
        public string? OpenFile()
        {
            var dialog = new OpenDialog("Open File", "Select a file to open");
            Application.Run(dialog);

            if (!dialog.Canceled && dialog.FilePath != null)
            {
                try
                {
                    var filePath = dialog.FilePath.ToString();
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        var content = System.IO.File.ReadAllText(filePath);
                        CurrentFilePath = filePath;
                        HasUnsavedChanges = false;
                        FileStateChanged?.Invoke();
                        return content;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.ErrorQuery("Error", $"Could not open file: {ex.Message}", "OK");
                }
            }
            return null;
        }

        /// <summary>
        /// Saves content to the current file path
        /// </summary>
        /// <param name="content">Text content to save</param>
        /// <returns>True if save successful, false otherwise</returns>
        public bool SaveFile(string content)
        {
            if (!string.IsNullOrEmpty(CurrentFilePath))
            {
                try
                {
                    System.IO.File.WriteAllText(CurrentFilePath, content);
                    MarkAsSaved();
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.ErrorQuery("Error", $"Could not save file: {ex.Message}", "OK");
                    return false;
                }
            }
            else
            {
                return SaveAsFile(content);
            }
        }

        /// <summary>
        /// Saves content to a new file path using save dialog
        /// </summary>
        /// <param name="content">Text content to save</param>
        /// <returns>True if save successful, false otherwise</returns>
        public bool SaveAsFile(string content)
        {
            var dialog = new SaveDialog("Save As", "Save file as...");
            Application.Run(dialog);

            if (!dialog.Canceled && dialog.FilePath != null)
            {
                try
                {
                    var filePath = dialog.FilePath.ToString();
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        System.IO.File.WriteAllText(filePath, content);
                        CurrentFilePath = filePath;
                        MarkAsSaved();
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.ErrorQuery("Error", $"Could not save file: {ex.Message}", "OK");
                }
            }
            return false;
        }

        /// <summary>
        /// Gets a display-friendly filename for the current file
        /// </summary>
        /// <returns>Filename or "New File" if no current file</returns>
        public string GetDisplayFileName()
        {
            return string.IsNullOrEmpty(CurrentFilePath) ? "New File" : System.IO.Path.GetFileName(CurrentFilePath);
        }

        /// <summary>
        /// Sets the current file path without loading content
        /// </summary>
        /// <param name="filePath">Path to set as current file</param>
        public void SetFilePath(string filePath)
        {
            CurrentFilePath = filePath ?? "";
            FileStateChanged?.Invoke();
        }
    }
}