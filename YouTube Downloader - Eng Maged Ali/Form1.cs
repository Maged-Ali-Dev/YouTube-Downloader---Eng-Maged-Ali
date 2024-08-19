using System.Text.RegularExpressions;
using YoutubeExplode;
using YoutubeExplode.Playlists;
using System.Net.NetworkInformation;
using AngleSharp.Io;
namespace YouTube_Downloader___Eng_Maged_Ali
{
    public partial class Form1 : Form
    {

        private long _lastBytesReceived;
        private DateTime _lastCheckTime;

        public Form1()
        {
            InitializeComponent();

            label1.Visible = false;
            label2.Visible = false;
            textBox1.Visible = false;
            textBox2.Visible = false;
            button1.Visible = false;
            button2.Visible = false;
            progressBar1.Visible = false;
            button5.Visible = false;
            button6.Visible = false;
            button7.Visible = false;


            //Download Speed Test
            label3.Visible = false;
            _lastBytesReceived = NetworkInterface.GetAllNetworkInterfaces()
              .Where(n => n.OperationalStatus == OperationalStatus.Up)
              .Sum(n => n.GetIPv4Statistics().BytesReceived);
            _lastCheckTime = DateTime.Now;
            timer1.Interval = 1000; // Check every second
            timer1.Tick += Timer1_Tick;
            timer1.Start();

          

        }



        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private readonly YoutubeClient _youtube = new YoutubeClient();
        private async void button1_Click_1(object sender, EventArgs e)
        {

            

            try
            {
                // Get the video URL from the TextBox
                var videoUrl = textBox1.Text;

                // Validate URL
                if (string.IsNullOrEmpty(videoUrl))
                {
                    MessageBox.Show("Please enter a valid YouTube URL.");
                    return;
                }
                button1.Enabled = false;
                button7.Enabled = false;
                // Get video information
                var video = await _youtube.Videos.GetAsync(videoUrl);
                var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(video.Id);

                // Get the highest quality muxed (combined video and audio) stream
                var muxedStreamInfo = streamManifest
                    .GetMuxedStreams()
                    .OrderByDescending(s => s.VideoQuality.IsHighDefinition) // Order by quality
                    .FirstOrDefault();

                if (muxedStreamInfo == null)
                {
                    MessageBox.Show("No suitable muxed video stream found.");
                    return;
                }

                // Sanitize the video title to make it safe for use as a file name
                var sanitizedTitle = Regex.Replace(video.Title, @"[<>:""/\\|?*]", string.Empty);

                // Show the SaveFileDialog
                using (var saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "MP4 files (*.mp4)|*.mp4";
                    saveFileDialog.Title = "Save Video File";
                    saveFileDialog.FileName = sanitizedTitle; // Default file name

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        var filePath = saveFileDialog.FileName;

                        // Initialize the ProgressBar
                        progressBar1.Style = ProgressBarStyle.Blocks;
                        progressBar1.Value = 0;
                        progressBar1.Maximum = 100;

                        // Get total bytes from FileSize
                        var totalBytes = muxedStreamInfo.Size;
                        long totalBytesLong = totalBytes.Bytes;

                        // Use a larger buffer size to speed up I/O operations
                        var buffer = new byte[64 * 1024]; // 64 KB buffer
                        var bytesRead = 0L;

                        using (var fileStream = File.OpenWrite(filePath))
                        {
                            using (var stream = await _youtube.Videos.Streams.GetAsync(muxedStreamInfo))
                            {
                                int read;
                                while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                                {
                                    // Write data to file
                                    await fileStream.WriteAsync(buffer, 0, read);

                                    // Update progress bar periodically to reduce UI overhead
                                    bytesRead += read;
                                    var progressPercentage = (int)((bytesRead * 100) / totalBytesLong);
                                    if (progressPercentage > progressBar1.Value)
                                    {
                                        progressBar1.Value = progressPercentage;
                                    }
                                }
                            }
                        }

                       // MessageBox.Show("Download completed!",MessageBoxIcon.Information.ToString());
                       MessageBox.Show("Download completed !", "Successfully completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        textBox1.Text = "";

                        button1.Enabled = true;
                        button7.Enabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                button1.Enabled = true;
                button7.Enabled = true;
            }
            finally
            {
                // Reset progress bar
                progressBar1.Style = ProgressBarStyle.Continuous;
                progressBar1.Value = 0;
            }

        }

        private async void button2_Click_1(object sender, EventArgs e)
        {
            try
            {
                // Get the playlist URL from the TextBox
                var playlistUrl = textBox2.Text;

                // Validate URL
                if (string.IsNullOrEmpty(playlistUrl))
                {
                    MessageBox.Show("Please enter a valid YouTube playlist URL.");
                    return;
                }
                button2.Enabled = false;
                button7.Enabled = false;
                // Show FolderBrowserDialog to choose the directory
                using (var folderBrowserDialog = new FolderBrowserDialog())
                {
                    folderBrowserDialog.Description = "Select a folder to save videos";

                    if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
                    {
                        MessageBox.Show("No folder selected.");
                        return;
                    }

                    var saveDirectory = folderBrowserDialog.SelectedPath;

                    // Get playlist ID and details
                    var playlistId = PlaylistId.Parse(playlistUrl);
                    var playlist = await _youtube.Playlists.GetAsync(playlistId);

                    // Initialize progress tracking
                    progressBar1.Style = ProgressBarStyle.Blocks;
                    progressBar1.Value = 0;

                    // Get videos in the playlist
                    var videos = _youtube.Playlists.GetVideosAsync(playlistId);
                    int totalVideos = 0;
                    await foreach (var _ in videos)
                    {
                        totalVideos++;
                    }

                    // Reset the async enumerator to iterate again
                    videos = _youtube.Playlists.GetVideosAsync(playlistId);

                    progressBar1.Maximum = totalVideos; // Set max value to the number of videos

                    await foreach (var video in videos)
                    {
                        // Get video details
                        var videoInfo = await _youtube.Videos.GetAsync(video.Id);
                        var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(video.Id);

                        // Get the highest quality muxed (combined video and audio) stream
                        var muxedStreamInfo = streamManifest
                            .GetMuxedStreams()
                            .OrderByDescending(s => s.VideoQuality.MaxHeight) // Order by quality
                            .FirstOrDefault();

                        if (muxedStreamInfo == null)
                        {
                            MessageBox.Show($"No suitable muxed video stream found for video {videoInfo.Title}.");
                            continue;
                        }

                        // Sanitize the video title to make it safe for use as a file name
                        var sanitizedTitle = Regex.Replace(videoInfo.Title, @"[<>:""/\\|?*]", string.Empty);

                        // Set the file path
                        var filePath = Path.Combine(saveDirectory, $"{sanitizedTitle}.mp4");

                        // Get total bytes from FileSize
                        var totalBytes = muxedStreamInfo.Size;
                        long totalBytesLong = totalBytes.Bytes; // Access the numeric value of Size

                        var buffer = new byte[8192];
                        var bytesRead = 0L;

                        using (var fileStream = File.OpenWrite(filePath))
                        {
                            using (var stream = await _youtube.Videos.Streams.GetAsync(muxedStreamInfo)) // Use IStreamInfo here
                            {
                                int read;
                                while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                                {
                                    // Write data to file
                                    await fileStream.WriteAsync(buffer, 0, read);

                                    // Update progress bar
                                    bytesRead += read;
                                    progressBar1.Value = (int)((bytesRead * 100) / totalBytesLong);
                                }
                            }
                        }

                        // Update progress bar for each video
                        progressBar1.Value++;
                    }

                   
                    MessageBox.Show("Playlist download completed!", "Successfully completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    textBox2.Text = "";
                    button2.Enabled = true;
                    button7.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                button1.Enabled = true;
                button7.Enabled = true;
            }
            finally
            {
                // Reset progress bar
                progressBar1.Style = ProgressBarStyle.Continuous;
                progressBar1.Value = 0;
            }
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            button3.Visible = false;
            button4.Visible = false;
            button6.Visible = false;
            label2.Visible = false;
            textBox2.Visible = false;
            button2.Visible = false;

            label1.Visible = true;
            textBox1.Visible = true;
            button1.Visible = true;
            button7.Visible = true;
            progressBar1.Visible = true;
            button5.Visible = true;
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            button3.Visible = false;
            button4.Visible = false;
            button5.Visible = false;
            label1.Visible = false;
            textBox1.Visible = false;
            button1.Visible = false;

            button6.Visible = true;
            button7.Visible = true;
            label2.Visible = true;
            textBox2.Visible = true;
            button2.Visible = true;
            progressBar1.Visible = true;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            textBox2.Text = "";

            button3.Visible = true;
            button4.Visible = true;

            label1.Visible = false;
            textBox1.Visible = false;
            button1.Visible = false;

            label2.Visible = false;
            textBox2.Visible = false;
            button2.Visible = false;
            button5.Visible = false;
            progressBar1.Visible = false;
            button6.Visible = false;
            button7.Visible = false;
        }

        private void button5_Click_1(object sender, EventArgs e)
        {

            try
            {
                // Get text from clipboard
                var clipboardText = Clipboard.GetText();

                // Set the text to the TextBox
                textBox1.Text = clipboardText;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {

            try
            {
                // Get text from clipboard
                var clipboardText = Clipboard.GetText();

                // Set the text to the TextBox
                textBox2.Text = clipboardText;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void Timer1_Tick(object? sender, EventArgs e)
        {

            label3.Visible = true;
            var currentBytesReceived = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up)
                .Sum(n => n.GetIPv4Statistics().BytesReceived);
            var currentTime = DateTime.Now;

            var bytesReceived = currentBytesReceived - _lastBytesReceived;
            var timeSpan = currentTime - _lastCheckTime;

            var speed = (bytesReceived * 8) / timeSpan.TotalSeconds; // bits per second

            label3.Text = $"Download Speed: {speed / 1024 / 1024:F2} Mbps";

            _lastBytesReceived = currentBytesReceived;
            _lastCheckTime = currentTime;



        }

        
    }
}
