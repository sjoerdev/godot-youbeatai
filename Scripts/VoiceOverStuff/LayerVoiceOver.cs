using Godot;
using System;

public partial class LayerVoiceOver : Node
{
	// singleton
    public static LayerVoiceOver instance = null;

	// user interface
	[Export] public TextureProgressBar textureProgressBar;
	[Export] public Button recordLayerButton;

	// recording
	public AudioStream[] voiceOvers = new AudioStream[10];
	AudioEffectRecord audioEffectRecord;
	AudioStreamPlayer2D audioPlayer;
	bool shouldRecord = false;
	bool recording = false;
	float recordingTimer = 0;

	public bool finished = false;

	// other
	[Export] Button snellerButton;
	[Export] Button langzamerButton;

	int currentLayer => Manager.instance.currentLayerIndex;
	public void SetCurrentLayerVoiceOver(AudioStream voiceOver) => voiceOvers[currentLayer] = voiceOver;
	public AudioStream GetCurrentLayerVoiceOver() => voiceOvers[currentLayer];

	public override void _Ready()
    {
		// init singleton
        instance ??= this;

		// init record button
		recordLayerButton.Pressed += () => shouldRecord = !shouldRecord;

		// create audioplayer
		audioPlayer = new AudioStreamPlayer2D();
		AddChild(audioPlayer);

		// setup record effect
        audioEffectRecord = (AudioEffectRecord)AudioServer.GetBusEffect(AudioServer.GetBusIndex("Microphone"), 1);
    }

    public override void _Process(double delta)
	{
		// set color of record button
		recordLayerButton.Modulate = shouldRecord ? new(1, 0.5f, 0.5f) : new(1, 1, 1);

		// update recording timer
		if (recording) recordingTimer += (float)delta;
		else recordingTimer = 0;

		// set progress bar value
		float secondsPerBeat = (60f / Manager.instance.bpm) / 2;
		float secondsPerRotation = secondsPerBeat * 32;
		float bpmfactor = 32 / secondsPerRotation;
		if (recording) textureProgressBar.Value = recordingTimer * bpmfactor;
		else
		{
			if (GetCurrentLayerVoiceOver() != null) textureProgressBar.Value = 32f;
			else textureProgressBar.Value = 0;
		}
	}

	public void OnTop()
	{
		if (audioPlayer.Playing) audioPlayer.Stop();

		if (shouldRecord && !recording) StartRecording();
		else if (recording) StopRecording();

		if (!recording)
		{
			audioPlayer.Stream = GetCurrentLayerVoiceOver();
			audioPlayer.Play();
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
		recordLayerButton.Visible = false;
		SongVoiceOver.instance.recordSongButton.Visible = false;

		SetVolume(0.5f);
    }

    private void StopRecording()
    {
        audioEffectRecord.SetRecordingActive(false);
		GD.Print("recording stopped");
		recording = false;
		shouldRecord = false;
		SetCurrentLayerVoiceOver(audioEffectRecord.GetRecording());

		// buttons after recording
		snellerButton.Visible = true;
		langzamerButton.Visible = true;
		Manager.instance.SetLayerSwitchButtonsVisibility(true);
		Manager.instance.PlayPauseButton.Visible = true;
		Manager.instance.ResetPlayerButton.Visible = true;
		recordLayerButton.Visible = true;
		SongVoiceOver.instance.recordSongButton.Visible = true;

		SetVolume(1f);

		finished = true;
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