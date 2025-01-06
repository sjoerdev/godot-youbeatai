using Godot;
using System;

public partial class SongVoiceOver : Node
{
	// singleton
    public static SongVoiceOver instance = null;

	// user interface
	[Export] public ProgressBar progressbar;
	[Export] public Button recordSongButton;

	// recording
	public AudioStream voiceOver = null;
    AudioEffectRecord audioEffectRecord;
	AudioStreamPlayer2D audioPlayer;
	bool shouldRecord = false;
	public bool recording = false;
	float recordingTimer = 0;

	// other
	[Export] Button snellerButton;
	[Export] Button langzamerButton;

	public override void _Ready()
    {
		// init singleton
        instance ??= this;

		// init record button
		recordSongButton.Pressed += () => shouldRecord = !shouldRecord;

		// create audioplayer
		audioPlayer = new AudioStreamPlayer2D();
		AddChild(audioPlayer);

		// setup record effect
        audioEffectRecord = (AudioEffectRecord)AudioServer.GetBusEffect(AudioServer.GetBusIndex("Microphone"), 1);

		Manager.instance.PlayPauseButton.Pressed += () =>
		{
			if (voiceOver != null)
			{
				if (audioPlayer.Playing) audioPlayer.Stop();
				else audioPlayer.Play();
			}
		};
    }

    public override void _Process(double delta)
	{
		// set color of record button
		recordSongButton.Modulate = shouldRecord ? new(1, 0.5f, 0.5f) : new(1, 1, 1);

		// update recording timer
		if (recording) recordingTimer += (float)delta;
		else recordingTimer = 0;

		// set progress bar value
		if (recording) progressbar.Value = (recordingTimer / (10f * (32f * (60f / (float)Manager.instance.bpm)))) * 2f;
	}

	public void OnBeginning()
	{
		if (recording)
		{
			StopRecording();
			if (voiceOver != null) audioPlayer.Play();
		}
		else
		{
			if (shouldRecord)
			{
				StartRecording();
			}
		}
	}

    private void StartRecording()
    {
		recording = true;
        audioEffectRecord.SetRecordingActive(true);
		GD.Print("recording started");

		// buttons during recording
		snellerButton.Visible = false;
		langzamerButton.Visible = false;
		Manager.instance.SetLayerSwitchButtonsVisibility(false);
		Manager.instance.PlayPauseButton.Visible = false;
		Manager.instance.ResetPlayerButton.Visible = false;
		recordSongButton.Visible = false;
		LayerVoiceOver.instance.recordLayerButton.Visible = false;

		SetVolume(0f);
    }

    private void StopRecording()
    {
        audioEffectRecord.SetRecordingActive(false);
		GD.Print("recording stopped");
		recording = false;
		shouldRecord = false;
		voiceOver = audioEffectRecord.GetRecording();
		audioPlayer.Stream = voiceOver;

		// buttons during recording
		snellerButton.Visible = true;
		langzamerButton.Visible = true;
		Manager.instance.SetLayerSwitchButtonsVisibility(true);
		Manager.instance.PlayPauseButton.Visible = true;
		Manager.instance.ResetPlayerButton.Visible = true;
		recordSongButton.Visible = true;
		LayerVoiceOver.instance.recordLayerButton.Visible = true;

		SetVolume(1f);
    }

	void SetVolume(float value)
    {
        float db = Mathf.LinearToDb(value);
        Manager.instance.firstAudioPlayer.VolumeDb = db;
        Manager.instance.secondAudioPlayer.VolumeDb = db;
        Manager.instance.thirdAudioPlayer.VolumeDb = db;
        Manager.instance.fourthAudioPlayer.VolumeDb = db;
    }
}
