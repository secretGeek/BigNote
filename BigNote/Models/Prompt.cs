using System.Windows.Forms;

public static class Prompt
{
    public static string ShowDialog(string text, string caption, string defaultValue = "", string OKButtonText = "OK", int OKButtonWidth = 100)
    {
        var formWidth = 480;
        var leftRightIndent = 30;
        Form prompt = new Form()
        {
            Width = formWidth,
            Height = 155,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            Text = caption,
            StartPosition = FormStartPosition.CenterScreen,
            BackColor = System.Drawing.Color.Black,
            ForeColor = System.Drawing.Color.White
        };

        Label textLabel = new Label() { Left = leftRightIndent, Top = 14, AutoSize = true, Text = text };
        TextBox textBox = new TextBox() { Left = leftRightIndent, Top = 50, Width = formWidth - (leftRightIndent * 2), Text = defaultValue };
        Button confirmation = new Button() { Text = OKButtonText, Left = formWidth - OKButtonWidth - leftRightIndent, Width = OKButtonWidth, Top = 80, DialogResult = DialogResult.OK, FlatStyle = FlatStyle.Flat };
        confirmation.Click += (sender, e) => { prompt.Close(); };
        prompt.Controls.Add(textBox);
        prompt.Controls.Add(confirmation);
        prompt.Controls.Add(textLabel);
        prompt.AcceptButton = confirmation;

        return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
    }
}
