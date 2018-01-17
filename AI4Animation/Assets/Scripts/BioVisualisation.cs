﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(BioAnimation_APFNN))]
public class BioVisualisation : MonoBehaviour {

	public CameraController CameraController;

	public Button StraightForward, StraightBack, StraightLeft, StraightRight, TurnLeft, TurnRight, Idle, Walk, Sprint, Jump, Sit, Lie, Stand;
	public Color Active = Utility.Orange;
	public Color Inactive = Utility.DarkGrey;

	public Button Skeleton, Transforms, Velocities, Trajectory, CyclicWeights, InverseKinematics;
	public Color VisualisationEnabled = Utility.Cyan;
	public Color VisualisationDisabled = Utility.Grey;

	public Button SmoothFollow, ConstantView, Static;
	public Color CameraEnabled = Utility.Mustard;
	public Color CameraDisabled = Utility.LightGrey;

	public Slider Yaw, Pitch;

	private int Frames = 150;
	private Queue<float>[] CW;
	private bool DrawCW = true;

	private BioAnimation_APFNN Animation;

	void Awake() {
		Animation = GetComponent<BioAnimation_APFNN>();
		CW = new Queue<float>[4];
		for(int i=0; i<4; i++) {
			CW[i] = new Queue<float>();
			for(int j=0; j<Frames; j++) {
				CW[i].Enqueue(0f);
			}
		}
		Skeleton.onClick.AddListener(ToggleSkeleton); UpdateColor(Skeleton, Animation.Character.DrawSkeleton ? VisualisationEnabled : VisualisationDisabled);
		Transforms.onClick.AddListener(ToggleTransforms); UpdateColor(Transforms, Animation.Character.DrawTransforms ? VisualisationEnabled : VisualisationDisabled);
		Velocities.onClick.AddListener(ToggleVelocities); UpdateColor(Velocities, Animation.ShowVelocities ? VisualisationEnabled : VisualisationDisabled);
		Trajectory.onClick.AddListener(ToggleTrajectory); UpdateColor(Trajectory, Animation.ShowTrajectory ? VisualisationEnabled : VisualisationDisabled);
		CyclicWeights.onClick.AddListener(ToggleCyclicWeights); UpdateColor(CyclicWeights, DrawCW ? VisualisationEnabled : VisualisationDisabled);
		InverseKinematics.onClick.AddListener(ToggleInverseKinematics); UpdateColor(InverseKinematics, Animation.SolveIK ? VisualisationEnabled : VisualisationDisabled);

		SmoothFollow.onClick.AddListener(SetSmoothFollow); UpdateColor(SmoothFollow, CameraController.Mode == CameraController.MODE.SmoothFollow ? CameraEnabled : CameraDisabled);
		ConstantView.onClick.AddListener(SetConstantView); UpdateColor(ConstantView, CameraController.Mode == CameraController.MODE.ConstantView ? CameraEnabled : CameraDisabled);
		Static.onClick.AddListener(SetStatic); UpdateColor(Static, CameraController.Mode == CameraController.MODE.Static ? CameraEnabled : CameraDisabled);

		Yaw.onValueChanged.AddListener(SetYaw);
		Pitch.onValueChanged.AddListener(SetPitch);
	}

	private void UpdateData() {
		for(int i=0; i<CW.Length; i++) {
			CW[i].Dequeue();
			CW[i].Enqueue(Animation.APFNN.GetControlPoint(i));
		}
	}

	private void DrawControlPoint(float x, float y, float width, float height, Queue<float> CW, Color color) {
		int _index = 0;
		float _x = 0f;
		float _xPrev = 0f;
		float _y = 0f;
		float _yPrev = 0f;
		foreach(float value in CW) {
			_x = x + (float)(_index)/(float)(Frames-1) * width;
			_y = y - height + value*height;
			if(_index > 0) {
				UnityGL.DrawGUILine(
					_xPrev,
					_yPrev, 
					_x,
					_y,
					2.5f,
					color
				);
			}
			_xPrev = _x; 
			_yPrev = _y;
			_index += 1;
		}
	}

	public void ToggleSkeleton() {
		Animation.Character.DrawSkeleton = !Animation.Character.DrawSkeleton;
		UpdateColor(Skeleton, Animation.Character.DrawSkeleton ? VisualisationEnabled : VisualisationDisabled);
	}

	public void ToggleTransforms() {
		Animation.Character.DrawTransforms = !Animation.Character.DrawTransforms;
		UpdateColor(Transforms, Animation.Character.DrawTransforms ? VisualisationEnabled : VisualisationDisabled);
	}

	public void ToggleVelocities() {
		Animation.ShowVelocities = !Animation.ShowVelocities;
		UpdateColor(Velocities, Animation.ShowVelocities ? VisualisationEnabled : VisualisationDisabled);
	}

	public void ToggleTrajectory() {
		Animation.ShowTrajectory = !Animation.ShowTrajectory;
		UpdateColor(Trajectory, Animation.ShowTrajectory ? VisualisationEnabled : VisualisationDisabled);
	}

	public void ToggleCyclicWeights() {
		DrawCW = !DrawCW;
		UpdateColor(CyclicWeights, DrawCW ? VisualisationEnabled : VisualisationDisabled);
	}

	public void ToggleInverseKinematics() {
		Animation.SolveIK = !Animation.SolveIK;
		UpdateColor(InverseKinematics, Animation.SolveIK ? VisualisationEnabled : VisualisationDisabled);
	}

	public void SetSmoothFollow() {
		CameraController.SetMode(CameraController.MODE.SmoothFollow);
		UpdateColor(SmoothFollow, CameraEnabled); UpdateColor(ConstantView, CameraDisabled); UpdateColor(Static, CameraDisabled);
	}

	public void SetConstantView() {
		CameraController.SetMode(CameraController.MODE.ConstantView);
		UpdateColor(SmoothFollow, CameraDisabled); UpdateColor(ConstantView, CameraEnabled); UpdateColor(Static, CameraDisabled);
	}

	public void SetStatic() {
		CameraController.SetMode(CameraController.MODE.Static);
		UpdateColor(SmoothFollow, CameraDisabled); UpdateColor(ConstantView, CameraDisabled); UpdateColor(Static, CameraEnabled);
	}

	public void SetYaw(float value) {
		CameraController.Yaw = value;
	}

	public void SetPitch(float value) {
		CameraController.Pitch = value;
	}

	void OnGUI() {
		UpdateControl(StraightForward, Input.GetKey(Animation.Controller.MoveForward));
		UpdateControl(StraightBack, Input.GetKey(Animation.Controller.MoveBackward));
		UpdateControl(StraightLeft, Input.GetKey(Animation.Controller.MoveLeft));
		UpdateControl(StraightRight, Input.GetKey(Animation.Controller.MoveRight));
		UpdateControl(TurnLeft, Input.GetKey(Animation.Controller.TurnLeft));
		UpdateControl(TurnRight, Input.GetKey(Animation.Controller.TurnRight));

		UpdateStyle(Idle, 0);
		UpdateStyle(Walk, 1);
		UpdateStyle(Sprint, 2);
		UpdateStyle(Jump, 3);
		UpdateStyle(Sit, 4);
		UpdateStyle(Lie, 5);
		UpdateStyle(Stand, 6);
	}

	private void UpdateColor(Button button, Color color) {
		button.GetComponent<Image>().color = color;
	}

	private void UpdateControl(Button button, bool active) {
		UpdateColor(button, active ? Active : Inactive);
	}

	private void UpdateStyle(Button button, int index) {
		float activation = Animation.GetTrajectory().Points[60].Styles[index];
		button.GetComponent<Image>().color = Color.Lerp(Inactive, Active, activation);
		button.GetComponentInChildren<Text>().text = (100f*activation).ToString("F0")  + "%";
	}

	void OnRenderObject() {
		UpdateData();

		if(DrawCW) {
			UnityGL.Start();
			float x = 0.025f;
			float y = 0.15f;
			float width = 0.95f;
			float height = 0.1f;
			float border = 0.0025f;
			UnityGL.DrawGUIQuad(x-border/Screen.width*Screen.height, y-height-border, width+2f*border/Screen.width*Screen.height, height+2f*border, Utility.Black);
			UnityGL.DrawGUIQuad(x, y-height, width, height, Utility.White);
			DrawControlPoint(x, y, width, height, CW[0], Utility.Red);
			DrawControlPoint(x, y, width, height, CW[1], Utility.DarkGreen);
			DrawControlPoint(x, y, width, height, CW[2], Utility.Purple);
			DrawControlPoint(x, y, width, height, CW[3], Utility.Orange);
			UnityGL.Finish();
		}
	}

}