using Terminal.Gui;

namespace Smalledit
{
    /// <summary>
    /// Main text editor window that provides a complete terminal-based text editing interface
    /// </summary>
    public class TextEditorWindow : Window
    {
        private readonly TextView textView;
        private readonly Label lineNumbersLabel;
        private readonly MenuBar menuBar;
        private readonly SearchManager searchManager;
        private readonly FileManager fileManager;
        private readonly BottomStatusBar statusBar;
        private bool updatePending = false;
        private DateTime lastUpdateRequest = DateTime.MinValue;
        private bool statsValid = false;
        private (int characters, int lines, int words) cachedStats;


        /// <summary>
        /// Initializes a new text editor window with optional file to load
        /// </summary>
        /// <param name="initialFilePath">Optional path to file to open on startup</param>
        public TextEditorWindow(string? initialFilePath = null)
        {
            fileManager = new FileManager();
            fileManager.FileStateChanged += UpdateTitle;
            UpdateTitle();

            Utils.SetTerminalCursorStyle();

            statusBar = new BottomStatusBar()
            {
                Y = Pos.AnchorEnd(Utils.Layout.STATUS_BAR_HEIGHT)
            };

            lineNumbersLabel = new Label()
            {
                X = Utils.Layout.LINE_NUMBERS_X,
                Y = Utils.Layout.TEXT_VIEW_Y_START,
                Width = Utils.Layout.LINE_NUMBERS_WIDTH,
                Height = Dim.Fill() - Utils.Layout.STATUS_BAR_HEIGHT
            };

            LayoutComplete += (rect) =>
            {
                RequestUpdate();
            };

            textView = new TextView()
            {
                X = Utils.Layout.TEXT_VIEW_X_OFFSET,
                Y = Utils.Layout.TEXT_VIEW_Y_START,
                Width = Dim.Fill(),
                Height = Dim.Fill() - Utils.Layout.STATUS_BAR_HEIGHT
            };

            searchManager = new SearchManager(textView);

            menuBar = new MenuBar(new MenuBarItem[]
            {
                new("_File", new MenuItem[]
                {
                    new("_New", "Create a new file", () => NewFile()),
                    new("_Open", "Open a file", () => OpenFile()),
                    new("_Save", "Save current file", () => SaveFile()),
                    new("Save _As", "Save file with new name", () => SaveAsFile()),
                    null!,
                    new("_Quit", "Exit the application", () => Quit())
                }),
                new("_Edit", new MenuItem[]
                {
                    new("_Cut", "Cut selected text", () => CutText()),
                    new("_Copy", "Copy selected text", () => CopyText()),
                    new("_Paste", "Paste text", () => PasteText()),
                    null!,
                    new("Select _All", "Select all text", () => SelectAllText())
                }),
                new("_Search", new MenuItem[]
                {
                    new("_Find", "Find text", () => searchManager.ShowFindDialog()),
                    new("Find _Next", "Find next occurrence", () => searchManager.FindNext())
                }),
                new("_View", new MenuItem[]
                {
                    new("_Line Numbers", "Toggle line numbers", () => ToggleLineNumbers()),
                    new("_Word Wrap", "Toggle word wrapping", () => ToggleWordWrap())
                }),
                new("_Help", new MenuItem[]
                {
                    new("_About", "About this editor", () => ShowAbout())
                })
            });

            textView.ColorScheme = Utils.CreateTextViewColorScheme();

            textView.TextChanged += () =>
            {
                fileManager.MarkAsModified();
                statsValid = false;
                RequestUpdate();
            };

            textView.KeyDown += (keyEvent) =>
            {
                var key = keyEvent.KeyEvent.Key;

                if (key == (Key.H | Key.CtrlMask))
                {
                    searchManager.FindNext();
                    keyEvent.Handled = true;
                    return;
                }

                if (key == (Key.F | Key.CtrlMask))
                {
                    searchManager.ShowFindDialog();
                    keyEvent.Handled = true;
                    return;
                }

                if (key == (Key.Q | Key.CtrlMask))
                {
                    Quit();
                    keyEvent.Handled = true;
                    return;
                }

                if (Utils.IsTextChangingKey(keyEvent.KeyEvent.Key))
                {
                    fileManager.MarkAsModified();
                }
            };

            textView.DrawContent += (rect) =>
            {
                RequestUpdate();
            };


            Add(menuBar, lineNumbersLabel, textView, statusBar);

            UpdateStatusBar();

            if (!string.IsNullOrWhiteSpace(initialFilePath))
            {
                LoadInitialFile(initialFilePath);
            }
        }

        /// <summary>
        /// Loads a file on editor startup
        /// </summary>
        /// <param name="filePath">Path to the file to load</param>
        private void LoadInitialFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    fileManager.SetFilePath(filePath);
                    textView.Text = "";
                    fileManager.MarkAsModified();
                    UpdateStatusBar();
                    return;
                }

                var content = File.ReadAllText(filePath);
                textView.Text = content;
                fileManager.SetFilePath(filePath);
                fileManager.MarkAsSaved();
                UpdateStatusBar();
            }
            catch (Exception ex)
            {
                MessageBox.ErrorQuery("Error Loading File",
                    $"Could not load file '{filePath}':\n{ex.Message}", "OK");

                fileManager.NewFile();
                textView.Text = "";
                UpdateStatusBar();
            }
        }

        /// <summary>
        /// Handles application quit with unsaved changes protection
        /// </summary>
        private void Quit()
        {
            if (fileManager.HasUnsavedChanges)
            {
                var quitDialog = new QuitDialog();
                Application.Run(quitDialog);

                switch (quitDialog.Result)
                {
                    case QuitDialog.QuitResult.Save:
                        SaveFile();
                        Application.Shutdown();
                        break;
                    case QuitDialog.QuitResult.QuitWithoutSaving:
                        Application.Shutdown();
                        break;
                    case QuitDialog.QuitResult.Cancel:
                        break;
                }
            }
            else
            {
                Application.Shutdown();
            }
        }

        /// <summary>
        /// Schedules a throttled UI update to prevent excessive redraws
        /// </summary>
        private void RequestUpdate()
        {
            lastUpdateRequest = DateTime.Now;

            if (!updatePending)
            {
                updatePending = true;
                Application.MainLoop.AddTimeout(TimeSpan.FromMilliseconds(50), (_) =>
                {
                    // Only update if no newer request came in during the delay
                    if ((DateTime.Now - lastUpdateRequest).TotalMilliseconds >= 45)
                    {
                        UpdateLineNumbers();
                        UpdateStatusBar();
                        updatePending = false;
                    }
                    else
                    {
                        // Reschedule if there was a more recent request
                        return true;
                    }
                    return false;
                });
            }
        }

        /// <summary>
        /// Updates the line numbers display based on current text and scroll position
        /// </summary>
        private void UpdateLineNumbers()
        {
            try
            {
                var text = textView.Text?.ToString() ?? "";

                // Get the visible area height,...
                var visibleHeight = textView.Frame.Height;
                if (visibleHeight <= 0)
                {
                    // Fallback
                    visibleHeight = Frame.Height > 0 ? Frame.Height - Utils.Layout.WINDOW_BORDER_AND_MENU_OFFSET : Utils.Layout.DEFAULT_VISIBLE_HEIGHT;
                }

                // ...try to get scroll position...
                int topRow = 0;
                try
                {
                    var topRowProperty = typeof(TextView).GetProperty("TopRow");
                    if (topRowProperty != null)
                    {
                        topRow = (int)(topRowProperty.GetValue(textView) ?? 0);
                    }
                }
                catch
                {
                    topRow = 0;
                }

                int textViewWidth = Math.Max(1, textView.Frame.Width - Utils.Layout.SCROLLBAR_WIDTH);

                // ...and generate line numbers
                lineNumbersLabel.Text = Utils.GenerateLineNumbers(text, textView.WordWrap,
                    visibleHeight, topRow, textViewWidth);
            }
            catch (Exception)
            {
                lineNumbersLabel.Text = Utils.GenerateLineNumbers("", false, Utils.Layout.DEFAULT_VISIBLE_HEIGHT, 0, Utils.Layout.DEFAULT_TEXT_WIDTH);
            }
        }

        /// <summary>
        /// Updates the window title to reflect current file and modification status
        /// </summary>
        private void UpdateTitle()
        {
            string fileName = fileManager.GetDisplayFileName();
            string modifier = fileManager.HasUnsavedChanges ? "*" : "";
            Title = $"Text Editor - {fileName}{modifier}";
            UpdateStatusBar();
        }

        /// <summary>
        /// Updates the status bar with current file info, text stats, and cursor position
        /// </summary>
        private void UpdateStatusBar()
        {
            if (statusBar == null) return;

            string fileName = fileManager.GetDisplayFileName();
            bool isModified = fileManager.HasUnsavedChanges;
            statusBar.UpdateFileInfo(fileName, isModified);

            if (!statsValid)
            {
                string text = textView.Text?.ToString() ?? "";
                cachedStats = Utils.CalculateTextStats(text);
                statsValid = true;
            }

            statusBar.UpdateTextStats(cachedStats.characters, cachedStats.lines, cachedStats.words);

            var cursorPos = textView.CursorPosition;
            statusBar.UpdatePosition(cursorPos.Y + Utils.Layout.CURSOR_POSITION_OFFSET, cursorPos.X + Utils.Layout.CURSOR_POSITION_OFFSET);
        }

        /// <summary>
        /// Creates a new empty file
        /// </summary>
        private void NewFile()
        {
            if (fileManager.HasUnsavedChanges)
            {
                textView.Text = "";
                fileManager.NewFile();
                UpdateStatusBar();
            }
        }

        /// <summary>
        /// Opens an existing file using file dialog
        /// </summary>
        private void OpenFile()
        {
            var content = fileManager.OpenFile();
            if (content != null)
            {
                textView.Text = content;
                fileManager.MarkAsSaved();
                UpdateStatusBar();
            }
        }

        /// <summary>
        /// Saves the current file
        /// </summary>
        private void SaveFile()
        {
            fileManager.SaveFile(textView.Text?.ToString() ?? "");
            UpdateStatusBar();
        }

        /// <summary>
        /// Saves the current file with a new name using file dialog
        /// </summary>
        private void SaveAsFile()
        {
            fileManager.SaveAsFile(textView.Text?.ToString() ?? "");
            UpdateStatusBar();
        }

        /// <summary>
        /// Cuts selected text to clipboard
        /// </summary>
        private void CutText()
        {
            textView.Cut();
            fileManager.MarkAsModified();
        }

        /// <summary>
        /// Copies selected text to clipboard
        /// </summary>
        private void CopyText()
        {
            textView.Copy();
        }

        /// <summary>
        /// Pastes text from clipboard at cursor position
        /// </summary>
        private void PasteText()
        {
            textView.Paste();
        }

        /// <summary>
        /// Selects all text in the editor
        /// </summary>
        private void SelectAllText()
        {
            var text = textView.Text?.ToString() ?? "";
            textView.SelectAll();
        }

        /// <summary>
        /// Toggles line numbers visibility and adjusts text view layout
        /// </summary>
        private void ToggleLineNumbers()
        {
            lineNumbersLabel.Visible = !lineNumbersLabel.Visible;
            if (lineNumbersLabel.Visible)
            {
                textView.X = Utils.Layout.TEXT_VIEW_X_OFFSET;
                textView.Width = Dim.Fill();
            }
            else
            {
                textView.X = Utils.Layout.LINE_NUMBERS_X;
                textView.Width = Dim.Fill();
            }
        }

        /// <summary>
        /// Toggles word wrapping mode and updates display
        /// </summary>
        private void ToggleWordWrap()
        {
            textView.WordWrap = !textView.WordWrap;
            UpdateLineNumbers();
            textView.SetNeedsDisplay();
            lineNumbersLabel.SetNeedsDisplay();
        }

        /// <summary>
        /// Shows the about dialog with application information
        /// </summary>
        private static void ShowAbout()
        {
            MessageBox.Query("About", "Simple Text Editor\nBuilt with Terminal.Gui v1\nMartin Rosenberg, MFF CUNI\n\nPress Esc to quit", "OK");
        }
    }

    /// <summary>
    /// Main program entry point
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Application entry point that initializes the text editor
        /// </summary>
        /// <param name="args">Command line arguments, first argument is optional file path</param>
        static void Main(string[] args)
        {
            try
            {
                Application.Init();

                string? initialFilePath = null;

                if (args.Length > 0)
                {
                    initialFilePath = args[0];
                }

                var window = new TextEditorWindow(initialFilePath);
                Application.Run(window);
                Application.Shutdown();
            }
            catch (Exception) { }
        }
    }
}