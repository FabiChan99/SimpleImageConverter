using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ImageMagick;

public class DragDropForm : Form
{
    private Label dropLabel;
    private ComboBox formatComboBox;
    private Label outputFormatLabel;
    private int conversionCount;

    private ContextMenuStrip contextMenuStrip;
    private ToolStripMenuItem setOutputPathMenuItem;
    private ToolStripMenuItem aboutMenuItem;

    private string customOutputPath = "";

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
            Text = "Drop image files here to convert\n\nRight-click here to set custom output path",
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

        contextMenuStrip = new ContextMenuStrip();
        setOutputPathMenuItem = new ToolStripMenuItem("Set Custom Output Path");
        setOutputPathMenuItem.Image = SystemIcons.Application.ToBitmap();
        aboutMenuItem = new ToolStripMenuItem("About the Program");
        aboutMenuItem.Image = SystemIcons.Information.ToBitmap();

        contextMenuStrip.Items.Add(setOutputPathMenuItem);
        contextMenuStrip.Items.Add(aboutMenuItem);
        this.ContextMenuStrip = contextMenuStrip;
        setOutputPathMenuItem.Click += SetOutputPathMenuItem_Click;
        aboutMenuItem.Click += AboutMenuItem_Click;

        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Text = "Simple Image Converter";
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
                string outputPath = customOutputPath;
                if (string.IsNullOrEmpty(outputPath))
                {
                    outputPath = Path.ChangeExtension(file, formatComboBox.SelectedItem.ToString().ToLower());
                }
                else
                {
                    outputPath = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(file) + "." + formatComboBox.SelectedItem.ToString().ToLower());
                }

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

    private void SetOutputPathMenuItem_Click(object sender, EventArgs e)
    {
        using (var folderDialog = new FolderBrowserDialog())
        {
            DialogResult result = folderDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                customOutputPath = folderDialog.SelectedPath;
                MessageBox.Show("Custom output path set to: " + customOutputPath);
            }
        }
    }

    private void AboutMenuItem_Click(object sender, EventArgs e)
    {
        using (var aboutForm = new AboutForm())
        {
            aboutForm.ShowDialog();
        }
    }

    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new DragDropForm());
    }
}

public class AboutForm : Form
{
    private Label nameLabel;
    private Label versionLabel;
    private Label createdByLabel;

    public AboutForm()
    {
        this.Text = "About";
        this.Size = new Size(300, 150);
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.FormBorderStyle = FormBorderStyle.FixedSingle;

        nameLabel = new Label()
        {
            Text = "Simple Image Converter",
            Font = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(10, 10)
        };
        this.Controls.Add(nameLabel);

        versionLabel = new Label()
        {
            Text = "Version 1.1.1",
            AutoSize = true,
            Location = new Point(10, 40)
        };
        this.Controls.Add(versionLabel);

        createdByLabel = new Label()
        {
            Text = "Created by Fabi-Chan",
            AutoSize = true,
            Location = new Point(10, 70)
        };
        this.Controls.Add(createdByLabel);
    }
}
