using System;
using System.Windows;
using Ookii.Dialogs.Wpf;

namespace ClipChopper
{
    internal static class TaskDialogHelper
    {
        public static void ShowInfoTaskDialog(Window window, string message)
        {
            ShowTaskDialog(
                window,
                message,
                TaskDialogIcon.Information,
                new TaskDialogButton(ButtonType.Ok)
            );
        }

        public static void ShowErrorTaskDialog(Window window, Exception ex)
        {
            ShowTaskDialog(
                window,
                $"Error: {ex.Message}",
                TaskDialogIcon.Error,
                new TaskDialogButton(ButtonType.Ok)
            );
        }

        public static void ShowTaskDialog(Window window, string? message,
            TaskDialogIcon taskDialogIcon, params TaskDialogButton[] buttons)
        {
            if (window is null)
            {
                throw new ArgumentNullException(nameof(window));
            }
            if (buttons is null)
            {
                throw new ArgumentNullException(nameof(buttons));
            }

            if (!TaskDialog.OSSupportsTaskDialogs)
            {
                MessageBox.Show(message);
                return;
            }

            // TODO: make a config file with constants.
            var dialog = new TaskDialog
            {
                WindowTitle = "Clip Chopper",
                MainInstruction = message,
                MainIcon = taskDialogIcon
            };

            foreach (var button in buttons)
            {
                dialog.Buttons.Add(button);
            }

            dialog.ShowDialog(window);
        }
    }
}
