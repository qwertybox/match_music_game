using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using NAudio.Wave;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Music_game
{
    public partial class Form1 : Form
    {
        public string tempFolderPath = "";
        public string currentSound = "";

        public List<string> soundPaths = new List<string>();
        public List<string> soundsAllPaths = new List<string>();
        public List<Button> chooseButtons = new List<Button>();

        public bool isStarted = false;
        public System.Media.SoundPlayer sound = null;
        public Form1()
        {
            InitializeComponent();
            this.Text = "Match song";
            Init();
        }

        public void Init() //innitialize 
        {
            tempFolderPath = "";
            currentSound = "";
            soundPaths = new List<string>();
            soundsAllPaths = new List<string>();
            chooseButtons = new List<Button>();
            isStarted = false;
            sound = null;
            chooseButtons.Add(button4);
            chooseButtons.Add(button5);
            chooseButtons.Add(button6);
            ResetComp();
        }

        public void ResetComp() //reset
        {
            button4.Text = "Answer:";
            button5.Text = "Answer:";
            button6.Text = "Answer:";
            button3.Enabled = true;
            label1.Text = "";
        }

        public void TrimMp3To30Sec(string mp3path, string outputpath, int minute)
        {
            TrimMp3(mp3path, outputpath, TimeSpan.FromMinutes(minute), TimeSpan.FromMinutes(minute + 0.5f));
        }

        private void TrimMp3(string inputPath, string outputPath, TimeSpan? begin, TimeSpan? end)
        {
            if (begin.HasValue && end.HasValue && begin > end)
            {
                throw new ArgumentOutOfRangeException("end", "end should be greater than begin");
            }
                

            using (var reader = new Mp3FileReader(inputPath))
            {
                using (var writer = File.Create(outputPath))
                {
                    Mp3Frame frame;
                    while ((frame = reader.ReadNextFrame()) != null)
                    {
                        if (reader.CurrentTime >= begin || !begin.HasValue)
                        {
                            if (reader.CurrentTime <= end || !end.HasValue)
                            {
                                writer.Write(frame.RawData, 0, frame.RawData.Length);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }

        private static void ConvertMp3ToWav(string _inPath, string _outPath)
        {
            using (Mp3FileReader mp3 = new Mp3FileReader(_inPath))
            {
                using (WaveStream pcm = WaveFormatConversionStream.CreatePcmStream(mp3))
                {
                    WaveFileWriter.CreateWaveFile(_outPath, pcm);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (var fdb = new FolderBrowserDialog())
            {
                DialogResult result = fdb.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fdb.SelectedPath))
                {
                    string[] files = Directory.GetFiles(fdb.SelectedPath);
                    if (files.Length < 3)
                    {
                        MessageBox.Show("Choose a folder with more or equal to 3 music files!");
                        return;
                    }
                    label1.Text = fdb.SelectedPath;
                    var Out_path = Path.Combine(fdb.SelectedPath, "Temp");
                    tempFolderPath = Out_path;
                    Directory.CreateDirectory(Out_path);
                    foreach (String soundPath in files)
                    {
                        var filename = Path.Combine(Out_path, soundPath.Substring(soundPath.LastIndexOf('\\') + 1));
                        TrimMp3To30Sec(soundPath, filename, 2);
                        ConvertMp3ToWav(filename, Path.ChangeExtension(filename, ".wav"));
                        File.Delete(filename);
                    }
                }
            }
        }

        public void PlaySound()
        {
            if (isStarted)
            {
                sound = new System.Media.SoundPlayer(currentSound);
                sound.Load();
                sound.Play();
            }
            else
            {
                MessageBox.Show("You haven't started a game!");
            }
        }

        private void OnClosedForm(object sender, FormClosedEventArgs e)
        {
            DeleteTemp();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (tempFolderPath != "")
            {
                isStarted = true;
                Button start_Button = sender as Button;
                start_Button.Enabled = false;
                if (Directory.Exists(tempFolderPath))
                {
                    soundPaths = new List<string>(Directory.GetFiles(tempFolderPath));
                    soundsAllPaths = new List<string>(soundPaths);
                    Select_Rd_Sound();
                }
            }
            else
            {
                MessageBox.Show("Choose a folder!");
            }

            PlaySound();
        }

        public void Select_Rd_Sound()
        {
            Random r = new Random();
            int index = r.Next(0, soundPaths.Count);
            if (soundPaths.Count <= 0)
            {
                MessageBox.Show("End of the list of songs");
                DeleteTemp();
                Init();
                return;
            }
            currentSound = soundPaths[index];
            soundPaths.RemoveAt(index);
            SetChooseButtons();
        }

        public void DeleteTemp() //delete directory created for music files
        {
            if (tempFolderPath != "")
            {
                if (Directory.Exists(tempFolderPath))
                {
                    string[] files = Directory.GetFiles(tempFolderPath);

                    foreach (var file in files)
                        File.Delete(file);

                    Directory.Delete(tempFolderPath);
                }
            }
        }

        public void SetChooseButtons() //set names for buttons
        {
            Random r = new Random();
            int rnd = r.Next(0, 3);
            var currentSoundFilename = currentSound.Substring(currentSound.LastIndexOf('\\') + 1);
            var tempSoundsAllPaths = new List<string>(soundsAllPaths);
            for (int i = 0; i < chooseButtons.Count; i++)
            {
                if (rnd == i)
                {
                    chooseButtons[i].Text = currentSoundFilename;
                }
                else
                {
                    var rnds = r.Next(0, tempSoundsAllPaths.Count);
                    var randomSoundPath = tempSoundsAllPaths[rnds];
                    var randomSoundFilename = randomSoundPath.Substring(randomSoundPath.LastIndexOf('\\') + 1);
                    while (randomSoundFilename == currentSoundFilename)
                    {
                        rnds = r.Next(0, tempSoundsAllPaths.Count);
                        randomSoundPath = tempSoundsAllPaths[rnds];
                        randomSoundFilename = randomSoundPath.Substring(randomSoundPath.LastIndexOf('\\') + 1);
                    }
                    tempSoundsAllPaths.Remove(randomSoundPath);
                    chooseButtons[i].Text = randomSoundFilename;
                }
            }
        }

        private void Button_pressed(object sender, EventArgs e)
        {
            if (isStarted)
            {
                var currentSoundFilename = currentSound.Substring(currentSound.LastIndexOf('\\') + 1);
                Button pressedButton = sender as Button;
                if (currentSoundFilename == pressedButton.Text)
                {
                    if (sound != null)
                    {
                        sound.Stop();
                    }
                    Select_Rd_Sound();
                }
                else
                {
                    MessageBox.Show("Oops! Try again!");
                }
            }
            else
            {
                MessageBox.Show("Start a game plz!");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            sound.Stop();
            button3.Enabled = true;
            if (Directory.Exists(tempFolderPath))
            {
                soundPaths = new List<string>(Directory.GetFiles(tempFolderPath));
                soundsAllPaths = new List<string>(soundPaths);
                Select_Rd_Sound();
            }
        }
    }
}
