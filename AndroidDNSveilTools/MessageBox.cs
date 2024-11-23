using AlertDialog = AndroidX.AppCompat.App.AlertDialog;
using DialogFragment = AndroidX.Fragment.App.DialogFragment;

namespace AndroidDNSveilTools;

public class MessageBox(string title, string message) : DialogFragment
{
    public override Dialog OnCreateDialog(Bundle? savedInstanceState)
    {
        AlertDialog? alert = new AlertDialog.Builder(Context).
            SetTitle(title)?.SetMessage(message)?.
            SetPositiveButton("OK", (sender, args) =>
            {
                
            }).
            SetNegativeButton("Cancel", (sender, args) =>
            {
                // Do Nothing
            }).Create();

        return alert ?? new AlertDialog.Builder(Context).Create();
    }

}