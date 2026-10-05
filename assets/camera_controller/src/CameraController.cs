using Godot;
using Godot.Collections;
using System;


namespace SlackingOff;

public partial class CameraController : Node3D
{
	// === Signals === (PascalCaseEventHandler())

	// === Enums === (PascalCase, members CONSTANT_CASE)

	// === Constants === (CONSTANT_CASE)

	// === Exported Vars === (PascalCase)

	// === Public Vars === (PascalCase)
	public Camera3D Camera { get; private set; }
	public Tween MovementTween { get; private set;  }

	// === Private Vars === (_underscoredCamelCase)
	private Node _cameraPath;
	private Path3D _path;
	private PathFollow3D _pathFollow;
	private Node3D _offset;
	private RemoteTransform3D _remoteTransform;
	private System.Collections.Generic.Queue<(float, float, float, Tween.TransitionType, Tween.EaseType)> _tweenQueue;
	
	// === Godot Methods ===
	
	public CameraController()
	{
		//runs when the object is created in memory, Parent -> Child
	}
	
	public override void _Ready()
	{
		//runs when the object and its children are ready, Child -> Parent
		Camera = GetNode<Camera3D>("%CameraInstance");
		_tweenQueue = new();
	}
	
	// Prefer using PhysicsProcess over Process
	public override void _PhysicsProcess(double delta)
	{

	}
	
	// === Further Methods === (PascalCase, local variables camelCase)

	/// <summary>
	/// Overwrites the current path to the provided path. 
	/// If passed, progressRatioOverride sets the follower's progress ratio to the new ratio
	/// </summary>
	/// <remarks>
	/// Expected structure of path:
	///		<code lang="text">
	/// Path3D
	/// |- PathFollow3D
	///		|- OffsetNode
	///			|- RemoteTransform3D
	///		</code>
	/// </remarks>
	/// <param name="cameraPath"></param>
	/// <param name="progressRatioOverride"></param>
	public void SetNewPath(Node cameraPath, float progressRatioOverride = -1)
	{
		// asserts
		Godot.Collections.Array nodeRefs = (Godot.Collections.Array)cameraPath.Call("get_node_refs");
		Path3D path = (Path3D)nodeRefs[0];
        PathFollow3D follower = (PathFollow3D)nodeRefs[1];
		Node3D offset = (Node3D)nodeRefs[2];
        RemoteTransform3D remote = (RemoteTransform3D)nodeRefs[3];

		if(!IsInstanceValid(path))
		{
			throw new ArgumentException($"path reference of cameraPath {cameraPath.Name} is not valid");
		}
        if (!IsInstanceValid(follower))
        {
            throw new ArgumentException($"follower reference of cameraPath {cameraPath.Name} is not valid");
        }
        if (!IsInstanceValid(offset))
        {
            throw new ArgumentException($"offset reference of cameraPath {cameraPath.Name} is not valid");
        }
        if (!IsInstanceValid(remote))
        {
            throw new ArgumentException($"remote reference of cameraPath {cameraPath.Name} is not valid");
        }

        // check progress override
        if (progressRatioOverride >= 0)
		{
			follower.ProgressRatio = progressRatioOverride;
		}

		// now overwrite our locals
		_cameraPath = cameraPath;
		_path = path;
		_pathFollow = follower;
		_offset = offset;
		_remoteTransform = remote;

		// finally, update remote
		_remoteTransform.RemotePath = _remoteTransform.GetPathTo(Camera);
	}

	/// <summary>
	/// Sets the camera to the index of the (control) point on the path
	/// </summary>
	/// <param name="index"></param>
	public void SetCameraToPathIndex(int index)
	{
		if(!IsInstanceValid(_path))
		{
			throw new Exception("_path is not a valid instance");
		}
		if (index < 0 || index >= _path.Curve.PointCount)
		{
			throw new IndexOutOfRangeException(index.ToString());
		}

		// update the camera by updating the follower and thus the remote transform
		_pathFollow.Progress = GetProgressForPointIndex(index);
    }

	/// <summary>
	/// Runs the camera tween using a time and the progress directly
	/// </summary>
	/// <param name="fromProgress">The progress distance to go from</param>
	/// <param name="toProgress">the progress distance to tween to</param>
	/// <param name="time">the time to complete the tween in</param>
	/// <param name="overwriteOld">if true, kills the previous tween. If false, adds this tween at the end of the current tween</param>
    private void RunCameraTween(float fromProgress, float toProgress, float time, bool overwriteOld,
		Tween.TransitionType trans = Tween.TransitionType.Linear, Tween.EaseType ease = Tween.EaseType.InOut)
    {
        if (!IsInstanceValid(_pathFollow))
        {
            throw new Exception("_pathFollow is not a valid instance");
        }

		// if we are overwriting and there's something to kill
		if(overwriteOld && IsInstanceValid(MovementTween))
		{
            MovementTween.Kill();
			_tweenQueue.Clear();
        }
		else if (IsInstanceValid(MovementTween) && MovementTween.IsRunning())
		{
			// if there isn't we have to check that the tween is running.
			// If it isn't, continue to make tween as normal (this is the "base case")
			// otherwise, add some info to the queue and stop
			_tweenQueue.Enqueue(
				(fromProgress, toProgress, time,
				trans, ease)
				);
			return;
		}


		MovementTween = CreateTween();
		MovementTween.SetEase(ease);
		MovementTween.SetTrans(trans);
		MovementTween.SetParallel(true);
		MovementTween.TweenProperty(_pathFollow, "progress", toProgress, time).From(fromProgress);
		MovementTween.TweenMethod(Callable.From<float>((p) => SetOffsetVariables(p)), fromProgress, toProgress, time);
		// Add a check to run a "queued" tweend
		MovementTween.Finished += () => { RunDequeuedTween(); };
    }

	private void RunDequeuedTween()
	{
		if(_tweenQueue.Count <= 0)
		{
			return;
		}
        (float, float, float, Tween.TransitionType, Tween.EaseType) data = _tweenQueue.Dequeue();
		RunCameraTween(data.Item1, data.Item2, data.Item3, false, data.Item4, data.Item5);
    }

	/// <summary>
	/// Tweens the camera to the given progressRatio in the given time
	/// </summary>
	/// <param name="progressRatio"></param>
	/// <param name="time"></param>
	public void TweenCameraToProgressRatioInTime(float progressRatio, float time, float? startRatio = null, bool overwriteOld = false)
	{
        if(!IsInstanceValid(_pathFollow)) 
		{
            throw new Exception("_pathFollow is not a valid instance");
        }
        if (!IsInstanceValid(_path))
        {
            throw new Exception("_path is not a valid instance");
        }

        RunCameraTween(startRatio * _path.Curve.GetBakedLength() ?? _pathFollow.Progress, progressRatio * _path.Curve.GetBakedLength(), time, overwriteOld);
	}
    public void TweenCameraToProgressRatioInTimeOverwrite(float progressRatio, float time, bool overwriteOld) => TweenCameraToProgressRatioInTime(progressRatio, time, null, overwriteOld);
    public void TweenCameraToProgressRatioInTimeWithStart(float progressRatio, float time, float start) => TweenCameraToProgressRatioInTime(progressRatio, time, start, false);
    public void TweenCameraToProgressRatioInTime(float progressRatio, float time) => TweenCameraToProgressRatioInTime(progressRatio, time, null, false);
    

    public void TweenCameraToProgressBySpeed(float progressRatio, float speed, float? startRatio = null, bool overwriteOld = false)
	{
        if (!IsInstanceValid(_pathFollow))
        {
            throw new Exception("_pathFollow is not a valid instance");
        }

        RunCameraTween(startRatio ?? _pathFollow.Progress, progressRatio, GetTimeFromSpeed(_pathFollow.Progress, progressRatio, speed), overwriteOld);
    }

	public void TweenCameraToProgressBySpeedOverwrite(float progressRatio, float speed, bool overwriteOld) => TweenCameraToProgressBySpeed(progressRatio, speed, null, overwriteOld);
    public void TweenCameraToProgressBySpeedWithStart(float progressRatio, float speed, float start) => TweenCameraToProgressBySpeed(progressRatio, speed, start, false);
    public void TweenCameraToProgressBySpeed(float progressRatio, float speed) => TweenCameraToProgressBySpeed(progressRatio, speed, null, false);

    public void TweenCameraToIndexInTime(int index, float time, int? startIndex = null, bool overwriteOld = false)
    {
        if (!IsInstanceValid(_pathFollow))
        {
            throw new Exception("_pathFollow is not a valid instance");
        }

		RunCameraTween(startIndex != null ? GetProgressForPointIndex((int)startIndex) : _pathFollow.Progress, 
			GetProgressForPointIndex(index), time, overwriteOld);
    }

	public void TweenCameraToIndexInTimeOverwrite(int index, int time, bool overwriteOld) => TweenCameraToIndexInTime(index, time, null, overwriteOld);
    public void TweenCameraToIndexInTimeWithStart(int index, int time, int start) => TweenCameraToIndexInTime(index, time, start, false);
    public void TweenCameraToIndexInTime(int index, int time) => TweenCameraToIndexInTime(index, time, null, false);

    public void TweenCameraToIndexBySpeed(int index, float speed, int? startIndex = null, bool overwriteOld = false)
	{
        if (!IsInstanceValid(_pathFollow))
        {
            throw new Exception("_pathFollow is not a valid instance");
        }

		float progress = GetProgressForPointIndex(index);
        RunCameraTween(startIndex != null ? GetProgressForPointIndex((int)startIndex) : _pathFollow.Progress, 
			progress, GetTimeFromSpeed(_pathFollow.Progress, progress, speed), overwriteOld);
    }

    public void TweenCameraToIndexBySpeedOverwrite(int index, float speed, bool overwriteOld) => TweenCameraToIndexBySpeed(index, speed, null, overwriteOld);
    public void TweenCameraToIndexBySpeedWithStart(int index, float speed, int start) => TweenCameraToIndexBySpeed(index, speed, start, false);
    public void TweenCameraToIndexBySpeed(int index, float speed) => TweenCameraToIndexBySpeed(index, speed, null, false);


    private float GetTimeFromSpeed(float progressRatioFrom, float progressRatioTo, float speed)
	{
        if (!IsInstanceValid(_path))
        {
            throw new Exception("_path is not a valid instance");
        }
		Curve3D c = _path.Curve;

		// calc distance - uses sample baked, so may be slightly inaccurate, but predicatbly so
		float dist = Mathf.Abs(
			c.GetClosestOffset(c.SampleBaked(progressRatioFrom * c.GetBakedLength()))
			- c.GetClosestOffset(c.SampleBaked(progressRatioTo * c.GetBakedLength()))
			);

		return dist / speed;
    }

	private float GetProgressForPointIndex(int index)
	{
        if (!IsInstanceValid(_path))
        {
            throw new Exception("_path is not a valid instance");
        }
		Curve3D c = _path.Curve;

		return c.GetClosestOffset(_path.Curve.GetPointPosition(index));
    }

	private void SetOffsetVariables(float progress)
	{
		_offset.Position = (Vector3)_cameraPath.Call("get_position_offset_at_progress", progress);
		_offset.RotationDegrees = (Vector3)_cameraPath.Call("get_rotation_offset_at_progress", progress);
    }


}
