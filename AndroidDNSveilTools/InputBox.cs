using Android.Views;
using AlertDialog = AndroidX.AppCompat.App.AlertDialog;

namespace AndroidDNSveilTools;

public class InputBox
{
    public class InputBoxEventArgs : EventArgs
    {
        public bool IsOK { get; set; } = false;
        public string Text { get; set; } = string.Empty;

        public InputBoxEventArgs() { }

        public InputBoxEventArgs(bool isOK, string text)
        {
            IsOK = isOK;
            Text = text;
        }
    }

    public event EventHandler<InputBoxEventArgs>? OnResultRecieved;

    public InputBox(Android.Content.Context? context, string title, string hint, string text, int minLines, int maxLines)
    {
        if (context == null) return;

        LayoutInflater? inflator = LayoutInflater.From(context);
        if (inflator == null) return;

        View? dialogView = inflator.Inflate(Resource.Layout.InputBox, null);
        if (dialogView == null) return;

        TextView? textViewTitle = dialogView.FindViewById<TextView>(Resource.Id.textViewTitle);
        if (textViewTitle == null) return;

        TextView? textViewHint = dialogView.FindViewById<TextView>(Resource.Id.textViewHint);
        if (textViewHint == null) return;

        EditText? editTextText = dialogView.FindViewById<EditText>(Resource.Id.editTextText);
        if (editTextText == null) return;

        if (string.IsNullOrEmpty(hint)) textViewHint.SetHeight(0);

        if (minLines < 1) minLines = 1;
        if (maxLines < 1) maxLines = 1;

        if (maxLines == 1)
        {
            bool isTextInt = int.TryParse(text, out _);
            if (isTextInt)
                editTextText.InputType = Android.Text.InputTypes.ClassNumber;
            else
                editTextText.InputType = Android.Text.InputTypes.TextFlagNoSuggestions;

            editTextText.SetSingleLine(true);
        }
        else
        {
            editTextText.InputType = Android.Text.InputTypes.TextFlagMultiLine;
            editTextText.SetSingleLine(false);
            editTextText.SetElegantTextHeight(true);
        }

        editTextText.SetMinLines(minLines);
        editTextText.SetMaxLines(maxLines);

        textViewTitle.Text = title;
        textViewHint.Text = hint;
        editTextText.Text = text;

        // Build The Dialog
        AlertDialog.Builder dialog = new(context);
        dialog.SetView(dialogView);
        dialog.SetPositiveButton("OK", (sender, e) =>
        {
            // User Input On OK Button
            text = editTextText.Text.Trim();
            OnResultRecieved?.Invoke(this, new InputBoxEventArgs(true, text));
        });
        dialog.SetNegativeButton("Cancel", (sender, e) =>
        {
            // User Input On Cancel Button
            OnResultRecieved?.Invoke(this, new InputBoxEventArgs());
        });

        AlertDialog alert = dialog.Create();
        alert.Show();

        alert.DismissEvent += (s, e) =>
        {
            try
            {
                inflator.Dispose();
                dialogView.Dispose();
                textViewTitle.Dispose();
                textViewHint.Dispose();
                editTextText.Dispose();
                dialog.Dispose();
                alert.Dispose();
            }
            catch (Exception) { }
        };
    }

}