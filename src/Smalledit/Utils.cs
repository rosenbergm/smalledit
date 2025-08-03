using System;
using System.IO;
using System.Text;
using Terminal.Gui;

namespace Smalledit
{
    public class Utils
    {
        public static class Layout
        {
            // Menu and main window
            public const int MENU_BAR_HEIGHT = 1;
            public const int STATUS_BAR_HEIGHT = 1;
            public const int WINDOW_BORDER_AND_MENU_OFFSET = 3; // window border + menu + padding

            // Line numbers
            public const int LINE_NUMBERS_WIDTH = 5;
            public const int LINE_NUMBERS_X = 0;
            public const int TEXT_VIEW_X_OFFSET = LINE_NUMBERS_WIDTH + 1;

            // Text view
            public const int TEXT_VIEW_Y_START = 1; // below menu bar

            // Scrollbar and padding
            public const int SCROLLBAR_WIDTH = 1;

            // Fallback dimensions
            public const int DEFAULT_VISIBLE_HEIGHT = 20;
            public const int DEFAULT_TEXT_WIDTH = 80;

            // Dialog dimensions
            public const int QUIT_DIALOG_WIDTH = 60;
            public const int QUIT_DIALOG_HEIGHT = 8;

            // Button positioning in dialogs
            public const int DIALOG_CONTENT_X = 1;
            public const int DIALOG_CONTENT_Y = 1;
            public const int DIALOG_BUTTON_Y = 4;
            public const int BUTTON_SPACING = 2;

            // Status bar positioning and cursor adjustment
            public const int CURSOR_POSITION_OFFSET = 1;

            // UI loop
            public const int UI_UPDATE_TIMEOUT_MS = 10;
        }

        public static class TextEditor
        {
            // Default values for text statistics
            public const int DEFAULT_LINE_COUNT = 1;
            public const int DEFAULT_CHAR_COUNT = 0;
            public const int DEFAULT_WORD_COUNT = 0;
        }

        public static bool IsTextChangingKey(Key key)
        {
            return key == Key.Backspace ||
                   key == Key.Delete ||
                   key == Key.Enter ||
                   key == Key.Tab ||
                   (key >= Key.Space && key <= Key.CharMask) ||
                   char.IsLetterOrDigit((char)key) ||
                   char.IsPunctuation((char)key) ||
                   char.IsSymbol((char)key);
        }

        /// <summary>
        /// Calculates text statistics including character count, line count, and word count
        /// </summary>
        public static (int characters, int lines, int words) CalculateTextStats(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return (TextEditor.DEFAULT_CHAR_COUNT, TextEditor.DEFAULT_LINE_COUNT, TextEditor.DEFAULT_WORD_COUNT);
            }

            int characters = text.Length;
            int lines = Math.Max(TextEditor.DEFAULT_LINE_COUNT, text.Split('\n').Length);
            int words = text.Split(new char[] { ' ', '\t', '\n', '\r' },
                StringSplitOptions.RemoveEmptyEntries).Length;

            return (characters, lines, words);
        }

        /// <summary>
        /// Creates a standard color scheme for text views
        /// </summary>
        public static ColorScheme CreateTextViewColorScheme()
        {
            return new ColorScheme()
            {
                Normal = Terminal.Gui.Attribute.Make(Color.White, Color.Black),
                Focus = Terminal.Gui.Attribute.Make(Color.Black, Color.White),
                HotNormal = Terminal.Gui.Attribute.Make(Color.BrightYellow, Color.Black),
                HotFocus = Terminal.Gui.Attribute.Make(Color.Black, Color.BrightYellow),
                Disabled = Terminal.Gui.Attribute.Make(Color.Gray, Color.Black)
            };
        }

        /// <summary>
        /// Attempts to set terminal cursor style and color
        /// </summary>
        public static void SetTerminalCursorStyle()
        {
            try
            {
                Console.Write("\x1b]12;#404040\x1b\\"); // cursor = dark grey
                Console.Write("\x1b[2 q"); // cursor = block
                Console.Out.Flush();
            }
            catch
            {
                // ANSI not supported in this terminal
            }
        }

        /// <summary>
        /// Generates line numbers for a text view with support for word wrapping
        /// </summary>
        public static string GenerateLineNumbers(string text, bool wordWrap, int visibleHeight,
            int topRow, int textViewWidth)
        {
            var lineNumbers = new StringBuilder();

            if (string.IsNullOrEmpty(text))
            {
                // For empty text, just show line 1 and fill the rest with spaces
                lineNumbers.AppendLine("   1");
                for (int i = 2; i <= Math.Max(1, visibleHeight); i++)
                {
                    lineNumbers.AppendLine("    ");
                }
                return lineNumbers.ToString().TrimEnd();
            }

            // Count total lines without splitting the entire string
            int totalLines = CountLines(text);

            if (wordWrap)
            {
                return GenerateWrappedLineNumbers(text, visibleHeight, topRow, textViewWidth, totalLines);
            }
            else
            {
                return GenerateSimpleLineNumbers(totalLines, visibleHeight, topRow);
            }
        }

        /// <summary>
        /// Counts lines without splitting the entire string
        /// </summary>
        private static int CountLines(string text)
        {
            if (string.IsNullOrEmpty(text)) return 1;

            int count = 1;
            int index = 0;
            while ((index = text.IndexOf('\n', index)) != -1)
            {
                count++;
                index++;
            }
            return count;
        }

        /// <summary>
        /// Line number generation for word wrap 
        /// </summary>
        private static string GenerateWrappedLineNumbers(string text, int visibleHeight,
            int topRow, int textViewWidth, int totalLines)
        {
            var lineNumbers = new StringBuilder();

            // For word wrap, show simple line numbers based on visible area
            // This avoids the expensive word wrap calculations for large files
            int startLine = Math.Max(1, topRow + 1);

            for (int i = 0; i < visibleHeight; i++)
            {
                var lineNumber = startLine + i;
                if (lineNumber <= totalLines)
                {
                    lineNumbers.AppendLine($"{lineNumber,4}");
                }
                else
                {
                    lineNumbers.AppendLine("    ");
                }
            }

            return lineNumbers.ToString().TrimEnd();
        }

        private static string GenerateSimpleLineNumbers(int totalLines, int visibleHeight, int topRow)
        {
            var lineNumbers = new StringBuilder();

            for (int i = 0; i < visibleHeight; i++)
            {
                var lineNumber = topRow + i + 1;
                if (lineNumber <= totalLines)
                {
                    lineNumbers.AppendLine($"{lineNumber,4}");
                }
                else
                {
                    lineNumbers.AppendLine("    ");
                }
            }

            return lineNumbers.ToString().TrimEnd();
        }

        /// <summary>
        /// Gets a safe file name for display purposes
        /// </summary>
        public static string GetDisplayFileName(string filePath)
        {
            return string.IsNullOrEmpty(filePath) ? "Untitled" : Path.GetFileName(filePath);
        }
    }
}