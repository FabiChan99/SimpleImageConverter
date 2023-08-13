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

    private Label qualityLabel;
    private TrackBar qualitySlider;

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
            AutoSize = true,
            Location = new Point(10, 10)
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

        qualityLabel = new Label()
        {
            Text = "Quality: 100%",
            AutoSize = true,
            Location = new Point(10, 43)
        };
        this.Controls.Add(qualityLabel);

        qualitySlider = new TrackBar()
        {
            Minimum = 0,
            Maximum = 100,
            TickFrequency = 10,
            Value = 100,
            Location = new Point(100, 40),
            Size = new Size(200, 20)
        };
        qualitySlider.Scroll += QualitySlider_Scroll;
        this.Controls.Add(qualitySlider);

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

        int paddingHorizontal = 20;
        int paddingVertical = 10;
        this.Size = new Size(this.Size.Width + paddingHorizontal, this.Size.Height + paddingVertical);
        dropLabel.Location = new Point(dropLabel.Location.X + paddingHorizontal / 2, dropLabel.Location.Y + paddingVertical / 2);
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

                    int quality = qualitySlider.Value;
                    image.Quality = quality;

                    image.Write(outputPath);
                }
                if (++conversionCount == 1)
                {
                    dropLabel.Text = "1 file converted successfully\n\nRight-click here to set custom output path";
                }
                else
                {
                    dropLabel.Text = $"{conversionCount} files converted successfully\n\nRight-click here to set custom output path";
                }
            }
            catch (MagickException ex)
            {
                MessageBox.Show("ImageMagick Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void SetOutputPathMenuItem_Click(object sender, EventArgs e)
    {
        using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
        {
            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
            {
                customOutputPath = folderBrowserDialog.SelectedPath;
            }
        }
    }

    private void AboutMenuItem_Click(object sender, EventArgs e)
    {
        MessageBox.Show("Simple Image Converter\nVersion 1.2\n\nCreated by Fabi-Chan", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void QualitySlider_Scroll(object sender, EventArgs e)
    {
        int quality = qualitySlider.Value;
        qualityLabel.Text = "Quality: " + quality.ToString() + "%";
    }

    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new DragDropForm());
    }
}