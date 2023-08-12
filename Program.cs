using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using ImageMagick;

public class DragDropForm : Form
{
    private Label dropLabel;
    private ComboBox formatComboBox;
    private Label outputFormatLabel;
    private int conversionCount;

    public DragDropForm()
    {
        this.AllowDrop = true;
        this.DragEnter += new DragEventHandler(Form_DragEnter);
        this.DragDrop += new DragEventHandler(Form_DragDrop);
        string iconFilePath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "icon.ico");
        FileInfo fileInfo = new FileInfo(iconFilePath);
        if (fileInfo.Exists)
        {
            Icon icon = new Icon(iconFilePath);
            this.Icon = icon;
        }
        else
        {
            this.Icon = SystemIcons.Application;
        }
        dropLabel = new Label()
        {
            Text = "Drop image files here to convert",
            AutoSize = true
        };
        this.Controls.Add(dropLabel);

        outputFormatLabel = new Label()
        {
            Text = "Output Format:",
            AutoSize = true,
            Location = new Point(10, 13)
        };
        this.Controls.Add(outputFormatLabel);

        formatComboBox = new ComboBox()
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(100, 10)
        };
        formatComboBox.Items.AddRange(new object[] { "PNG", "JPG", "BMP", "WEBP", "ICO", "SVG" });
        formatComboBox.SelectedIndex = 0;
        this.Controls.Add(formatComboBox);

        this.FormBorderStyle = FormBorderStyle.FixedSingle; // Set the form's border style to FixedSingle
        this.MaximizeBox = false; // Disable the maximize button
        this.MinimizeBox = false; // Disable the minimize button
        this.Text = "Simple Image Converter"; // Set the window title

        conversionCount = 0;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        dropLabel.Location = new Point((this.ClientSize.Width - dropLabel.Width) / 2, (this.ClientSize.Height - dropLabel.Height) / 2);
    }

    private void Form_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
            e.Effect = DragDropEffects.Copy;
    }

    private void Form_DragDrop(object sender, DragEventArgs e)
    {
        string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
        foreach (string file in files)
        {
            try
            {
                string outputPath = Path.ChangeExtension(file, formatComboBox.SelectedItem.ToString().ToLower());
                using (MagickImage image = new MagickImage(file))
                {
                    if (formatComboBox.SelectedItem.ToString().Equals("WEBP", StringComparison.OrdinalIgnoreCase))
                    {
                        image.Format = MagickFormat.WebP;
                    }
                    else if (formatComboBox.SelectedItem.ToString().Equals("ICO", StringComparison.OrdinalIgnoreCase))
                    {
                        image.Format = MagickFormat.Ico;
                    }
                    else if (formatComboBox.SelectedItem.ToString().Equals("SVG", StringComparison.OrdinalIgnoreCase))
                    {
                        image.Format = MagickFormat.Svg;
                    }
                    image.Write(outputPath);
                }
                if (files.Length > 1)
                {
                    conversionCount++;
                }
                else
                {
                    MessageBox.Show("File converted: " + outputPath);
                }
            }
            catch (MagickException ex)
            {
                MessageBox.Show("Failed to convert file: " + file + "\nError: " + ex.Message);
            }
        }

        if (conversionCount > 0)
        {
            MessageBox.Show(conversionCount + " file(s) converted.");
            conversionCount = 0;
        }
    }

    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DragDropForm));
        SuspendLayout();
        // 
        // DragDropForm
        // 
        ClientSize = new Size(284, 261);
        Name = "DragDropForm";
        ResumeLayout(false);
    }

    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new DragDropForm());
    }
}
